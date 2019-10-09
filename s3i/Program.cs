using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Reflection;
using System.Threading.Tasks;

using s3i_lib;

namespace s3i
{
    class Program
    {
        static int Main(string[] args)
        {
            // to allow compilation on c# 7.0 (mono)
            return __Main(args).Result;
        }
        static async Task<int> __Main(string[] args)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var exeFileName = Path.GetFileName(assembly.Location);
            var version = assembly.GetName().Version;
            var commandLine = new CommandLine
            {
                HelpHeader = $"S3 download and install v{version}{Environment.NewLine}"
                           + $" Usage:{Environment.NewLine}"
                           + $"  {exeFileName} [<option> ...] <products> ..."
            };
            commandLine.Parse(args);
            if (commandLine.ResetDefaultCommandLine)
            {
                Properties.Settings.Default.CommandLineArgs = String.Empty;
                Properties.Settings.Default.Save();
                return 0;
            }
            if (commandLine.Arguments.Count < 1)
            {
                var defaultCommandLine = Properties.Settings.Default.CommandLineArgs;
                if(!string.IsNullOrEmpty(defaultCommandLine)) commandLine.HelpTail = $"Default command line: {defaultCommandLine}";
                // no args provided, try to use saved
                if (commandLine.PrintHelp)
                {
                    Console.WriteLine(commandLine.Help());
                    return -1;
                }
                var defaultArgs = defaultCommandLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                commandLine.Parse(defaultArgs);
                if (commandLine.Arguments.Count < 1)
                {
                    Console.WriteLine(commandLine.Help());
                    return -1;
                }
            }
            else
            {
                // user provided args, save those
                Properties.Settings.Default.CommandLineArgs = args.Aggregate("", (a, s) => { return $"{a} {s}"; });
                Properties.Settings.Default.Save();
            }
            var clock = System.Diagnostics.Stopwatch.StartNew();
            var s3 = new S3Helper(commandLine.ProfileName);
            //if (commandLine.Verbose)
            //{
            //    Console.WriteLine("Command line args:");
            //    Console.WriteLine(commandLine.Values);
            //}
            int exitCode = 0;
            try
            {            
                // read product descriptions in parallel
                var products = await Products.ReadProducts(s3, commandLine.Arguments.Select(
                (uri, index) =>
                {
                    return uri;
                }), commandLine.TempFolder);
                // what do we need no more?
                var uninstall = products.FindFilesToUninstall(commandLine.TempFolder).ToList();
                //
                if (commandLine.Verbose)
                {
                    Console.WriteLine($"Products [{products.Count}]:");
                    foreach (var p in products)
                    {
                        Console.WriteLine($"  {p.Name}: {p.AbsoluteUri} => {p.LocalPath}");
                        foreach (var pp in p.Props)
                        {
                            Console.WriteLine($"    {pp.Key} = {pp.Value}");
                        }
                    }
                    Console.WriteLine($"Uninstall [{uninstall.Count}]:");
                    foreach(var u in uninstall)
                    {
                        Console.WriteLine($"  {u}");
                    }
                }
                // first we need to uninstall, then download new versions, and install those
                // downloading files also can be parallel
                await products.DownloadInstallers(s3, commandLine.TempFolder);
                // but installation needs to be sequential due to msiexec nature
                foreach (var product in products)
                {
                    var code = await InstallProduct(product, commandLine);
                    if (0 == exitCode) exitCode = code;
                }
            }
            catch (Exception x)
            {
                Console.WriteLine($"? {x.GetType().Name}: {x.Message}");
                if (commandLine.Verbose)
                {
                    for (var xi = x.InnerException; null != xi; xi = xi.InnerException)
                    {
                        Console.WriteLine($"? {xi.GetType().Name}: {xi.Message}");
                    }
                }
                exitCode = x.HResult;
            }
            if (commandLine.Verbose)
            {
                Console.WriteLine();
                Console.WriteLine($"Elapsed: {clock.Elapsed}");
            }
            return exitCode;
        }

        static async Task<int> InstallProduct(ProductInfo product, CommandLine commandLine)
        {
            var actions = new List<Installer.Action> { };
            if (string.IsNullOrWhiteSpace(commandLine.MsiExecKeys))
            {
                try
                {
                    var installed = await ProductInfo.FindInstalled(product.LocalPath);
                    if (null != installed)
                    {
                        var action = product.CompareAndSelectAction(installed);
                        if (commandLine.Verbose || commandLine.DryRun)
                        {
                            Console.WriteLine($"Compared {product.AbsoluteUri} vs. {installed.AbsoluteUri} => {action}");
                        }
                        // translate to sequence
                        switch (action)
                        {
                            case Installer.Action.NoAction:
                                break;
                            case Installer.Action.Install:
                                actions.Add(Installer.Action.Install);
                                break;
                            case Installer.Action.Reinstall:
                                actions.Add(Installer.Action.Uninstall);
                                actions.Add(Installer.Action.Install);
                                break;
                            case Installer.Action.Uninstall:
                                actions.Add(Installer.Action.Uninstall);
                                break;
                        }
                    }
                    return await RunActions(new Installer(product), actions, commandLine);
                }
                catch (FileNotFoundException) { }
                catch (Exception x)
                {
                    Console.WriteLine($"? '{product.Name}' can't read saved configuration: {x.GetType().Name}: {x.Message}");
                }
            }
            return -1;
        }

        public static async Task<int> RunActions(Installer installer, IEnumerable<Installer.Action> actions, CommandLine commandLine)
        {
            int exitCode = 0;
            foreach (var action in actions)
            {
                var _exitCode = await RunAction(installer, action, commandLine);
                if (0 != _exitCode)
                {
                    if (0 == exitCode) exitCode = _exitCode;
                }
            }
            return exitCode;
        }

        public static async Task<int> RunAction(Installer installer, Installer.Action action, CommandLine commandLine)
        {
            var exitCode = 0;
            var commandArgs = installer.FormatCommand(Installer.ActionKeys[action], commandLine.MsiExecArgs);
            if (commandLine.Verbose || commandLine.DryRun)
            {
                var header = commandLine.DryRun ? "(DryRun)" : "(Install)";
                Console.WriteLine();
                Console.WriteLine($"{header} [{commandLine.Timeout}] {Installer.MsiExec} {commandArgs}");
            }
            if (!commandLine.DryRun)
            {
                exitCode = installer.RunInstall(commandArgs, commandLine.Timeout);
                if (0 == exitCode && (Installer.Action.Install == action || Installer.Action.Reinstall == action))
                {
                    // update saved configuration
                    try
                    {
                        await installer.Product.SaveToLocal();
                    }
                    catch (Exception x)
                    {
                        Console.WriteLine($"? '{installer.Product.Name}' saving configuration: {x.GetType().Name}: {x.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"? '{installer.Product.Name}' installation failed. Error 0x{exitCode:X8}({exitCode}): {Win32Helper.ErrorMessage(exitCode)}");
                }
            }
            return exitCode;
        }

    }
}
