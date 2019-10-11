using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            int exitCode = 0;
            var assembly = Assembly.GetExecutingAssembly();
            var exeFileName = Path.GetFileName(assembly.Location);
            var version = FileVersionInfo.GetVersionInfo(assembly.Location);
            var commandLine = new CommandLine
            {
                HelpHeader = $"{version.ProductName}: S3 download and install v{version.ProductVersion}{Environment.NewLine}"
                           + $" Usage:{Environment.NewLine}"
                           + $"  {exeFileName} [<option> ...] <products> ..."
            };
            commandLine.Parse(args);
            // decide if help needed
            if (commandLine.Arguments.Count < 1)
            {
                // no command args provided, try to obtain from env var
                var defaultCommandLine = Environment.GetEnvironmentVariable(commandLine.EnvironmentVariableName) ?? "";
                if (!string.IsNullOrEmpty(defaultCommandLine)) commandLine.HelpTail = $"Default command line (%{commandLine.EnvironmentVariableName}%): {defaultCommandLine}";
                // no args provided, try to use saved
                if (commandLine.PrintHelp)
                {
                    Console.WriteLine(commandLine.Help());
                }
                else
                {
                    var defaultArgs = defaultCommandLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    commandLine.Parse(defaultArgs);
                    if (commandLine.Arguments.Count < 1)
                    {
                        Console.WriteLine(commandLine.Help());
                    }
                }
            }
            // validate and execute
            if (0 < commandLine.Arguments.Count)
            {
                var validateResult = commandLine.Validate();
                if (!validateResult)
                {
                    Console.WriteLine($"? Command line validation failed{(0 < validateResult.Errors.Count ? ":" : ".")}");
                    foreach (var e in validateResult.Errors) Console.WriteLine($"  {e}");
                    exitCode = -1;
                }
                else
                {
                    var clock = System.Diagnostics.Stopwatch.StartNew();
                    exitCode = await ProcessAndExecute(commandLine);
                    if (commandLine.Verbose)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"Elapsed: {clock.Elapsed}");
                    }
                }
            }
            if (Debugger.IsAttached)
            {
                Console.Write("Press Enter..."); Console.ReadLine();
            }
            return exitCode;
        }

        static async Task<int> ProcessAndExecute(CommandLine commandLine)
        {
            var exitCode = 0;
            var s3 = new S3Helper(commandLine.ProfileName);

            Products products = null;
            IEnumerable<string> remove = null, uninstall = null;
            IEnumerable<ProductInfo> install = null;
            try
            {
                products = await Products.ReadProducts(s3, commandLine.Arguments.Select((uri, index) => { return uri; }), commandLine.StagingFolder);
                if (commandLine.Verbose)
                {
                    Console.WriteLine($"Products [{products.Count}]:");
                    foreach (var p in products)
                    {
                        Console.WriteLine($"  {p.Name}: {p.AbsoluteUri} => {p.LocalPath}");
                        foreach (var pp in p.Props) Console.WriteLine($"    {pp.Key} = {pp.Value}");
                    }
                }
                if (!Directory.Exists(commandLine.StagingFolder))
                {
                    if (commandLine.Verbose) Console.WriteLine($"Create {commandLine.StagingFolder}");
                    Directory.CreateDirectory(commandLine.StagingFolder);
                }
                // installed products (cached installer files) we don't need anymore
                remove = products.FindFilesToUninstall(Path.Combine(commandLine.StagingFolder, $"*{Installer.InstallerFileExtension}"));
                if (commandLine.Verbose)
                {
                    if (0 < remove.Count())
                    {
                        Console.WriteLine($"Remove [{remove.Count()}]:");
                        foreach (var f in remove) Console.WriteLine($"  {f}");
                    }
                }
                // Prepare a list of files to uninstall for downgrade or props change, and another list of products to install/upgrade
                (uninstall, install) = products.Separate(localMsiFile =>
                {
                    var localInfoFile = Path.ChangeExtension(localMsiFile, ProductInfo.LocalInfoFileExtension);
                    return ProductInfo.FindInstalled(localInfoFile).Result;
                });
                if (commandLine.Verbose)
                {
                    if (0 < uninstall.Count())
                    {
                        Console.WriteLine($"Uninstall [{uninstall.Count()}]:");
                        foreach (var f in uninstall) Console.WriteLine($"  {f}");
                    }
                    if (0 < install.Count())
                    {
                        Console.WriteLine($"Install [{install.Count()}]:");
                        foreach (var f in install) Console.WriteLine($"  {f.AbsoluteUri}");
                    }
                }
            }
            catch (Exception x)
            {
                Console.WriteLine($"? {x.Format(4)}");
                // no need to proceed if don't know what to do
                exitCode = -1;
            }
            // Ok, now we can proceed with changes:
            if (0 == exitCode)
            {
                // 1) uninstall old...
                foreach (var f in remove)
                {
                    var err = commandLine.Uninstall(f, true);
                    if (0 == exitCode && 0 != err) { exitCode = err; break; }
                }
                foreach (var f in uninstall)
                {
                    var err = commandLine.Uninstall(f, false);
                    if (0 == exitCode && 0 != err) { exitCode = err; break; }
                }
                // 2) ...download/cache new...
                await products.DownloadInstallers(s3, commandLine.StagingFolder);
                // 3) install them!
                foreach (var p in install)
                {
                    var err = commandLine.Install(p);
                    if (0 == exitCode && 0 != err) { exitCode = err; break; }
                }
            }
            return exitCode;
        }

    }
}
