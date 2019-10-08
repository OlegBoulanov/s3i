using System;
using System.IO;
using System.Linq;
using System.Configuration;
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
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var exeFileName = System.IO.Path.GetFileName(assembly.CodeBase);
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
            // read product descriptions in parallel
            var products = await Products.ReadProducts(s3, commandLine.Arguments.Select(
                (uri, index) =>
                {
                    return uri;
                }), commandLine.TempFolder);
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
            }
            // downloading files also can be parallel
            await products.DownloadInstallers(s3, commandLine.TempFolder);
            // but installation needs to be sequential due to msiexec nature
            int exitCode = 0;
            foreach(var product in products)
            {
                var code = await InstallProduct(product, commandLine);
                if (0 == exitCode) exitCode = code;
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
            int exitCode = 0;
            var msiExecKeys = commandLine.MsiExecKeys;
            if (string.IsNullOrWhiteSpace(msiExecKeys))
            {
                // if no keys provided, determine from previous and current installations
                try
                {
                    var installed = await ProductInfo.FromLocal(product.LocalPath);
                    var action = product.CompareAndSelectAction(installed);
                    if(commandLine.Verbose || commandLine.DryRun)
                    {
                        Console.WriteLine($"Compared {product.AbsoluteUri} vs. {installed.AbsoluteUri} => {action}");
                    }
                    if(Installer.Action.NoAction != action) msiExecKeys = Installer.ActionKeys[action];
                }
                catch (FileNotFoundException) { }
                catch (Exception x)
                {
                    Console.WriteLine($"? '{product.Name}' can't read saved configuration: {x.GetType().Name}: {x.Message}");
                }
            }
            if (!string.IsNullOrWhiteSpace(msiExecKeys))
            {
                // now install
                var installer = new Installer(product);
                var commandArgs = installer.FormatCommand(msiExecKeys, commandLine.MsiExecArgs);
                if (commandLine.Verbose || commandLine.DryRun)
                {
                    var header = commandLine.DryRun ? "(DryRun)" : "(Install)";
                    Console.WriteLine();
                    Console.WriteLine($"{header} [{commandLine.Timeout}] {Installer.MsiExec} {commandArgs}");
                }
                if (!commandLine.DryRun)
                {
                    exitCode = installer.RunInstall(commandArgs, commandLine.Timeout);
                    if (0 == exitCode)
                    {
                        // update saved configuration
                        try
                        {
                            await product.SaveToLocal();
                        }
                        catch (Exception x)
                        {
                            Console.WriteLine($"? '{product.Name}' saving configuration: {x.GetType().Name}: {x.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"? '{product.Name}' installation failed. Error 0x{exitCode:08x}({exitCode}): {Win32Helper.ErrorMessage(exitCode)}");
                    }
                }
            }
            return exitCode;
        }

    }
}
