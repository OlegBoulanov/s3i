using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using s3iLib;

namespace s3i
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                // to allow compilation on c# 7.0 (mono)
                return AsyncMain(args).Result;
            }
            catch (AggregateException x)
            {
                Console.WriteLine($"? {(x.InnerException??x).Format(4)}");
                return -1;
            }
            catch (ApplicationException x)
            {
                Console.WriteLine($"? {x.Format(4)}");
                return -1;
            }
        }
#pragma warning disable CA1303// warning CA1303: Method '***' passes a literal string as parameter 'value'
        static async Task<int> AsyncMain(string[] args)
        {
            int exitCode = 0;
            var assembly = Assembly.GetExecutingAssembly();
            var exeFileName = Path.GetFileNameWithoutExtension(assembly.Location);
            var version = FileVersionInfo.GetVersionInfo(assembly.Location);
            var commandLine = new CommandLine
            {
                HelpHeader = $"{exeFileName}: msi package batch installer v{version.ProductVersion}{Environment.NewLine}"
                           + $" Usage:{Environment.NewLine}"
                           + $"  {exeFileName} [<option> ...] <products> ..."
            };
            commandLine.Parse(args.Select(a => Environment.ExpandEnvironmentVariables(a)));
            commandLine.SetDefaults(exeFileName);
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
                if (validateResult.Failed)
                {
                    Console.WriteLine($"? Command line validation failed{(0 < validateResult.Errors.Count ? ":" : ".")}");
                    foreach (var e in validateResult.Errors) Console.WriteLine($"  {e}");
                    exitCode = -1;
                }
                else
                {
                    DownloaderS3.ProfileName = commandLine.ProfileName;
                    MsiInstaller.MsiExec = commandLine.MsiExecCommand;
                    var clock = System.Diagnostics.Stopwatch.StartNew();
                    exitCode = await ProcessAndExecute(commandLine).ConfigureAwait(false);
                    if (commandLine.Verbose)
                    {
                        //Console.WriteLine();
                        Console.WriteLine($"Elapsed: {clock.Elapsed}");
                    }
                }
            }
            return exitCode;
        }
#pragma warning restore CA1303
        static async Task<int> ProcessAndExecute(CommandLine commandLine)
        {
            var exitCode = 0;

            var remove = new List<string>();
            IEnumerable<ProductInfo> uninstall = new List<ProductInfo>(), install = null;
            var products = await ProductCollection.ReadProducts(commandLine.Arguments.Select((uri, index) => { return new Uri(uri); })).ConfigureAwait(false);
            products.MapToLocal(commandLine.StagingFolder);
            if (commandLine.Verbose)
            {
                Console.WriteLine($"Products [{products.Count}]:");
                foreach (var p in products)
                {
                    Console.WriteLine($"  {p.Name}: {p.Uri} => {p.LocalPath}");
                    foreach (var pp in p.Props) Console.WriteLine($"    {pp.Key} = {pp.Value}");
                }
            }
            // installed products (cached installer files) we don't need anymore
            foreach (var ext in Installer.GetSupportedExtensions())
            {
                foreach (var file in products.FindFilesToUninstall(Path.Combine(commandLine.StagingFolder, $"*{ext}")))
                {
                    remove.Add(file);
                }
            }
            if (commandLine.Verbose)
            {
                if (remove.Any())
                {
                    Console.WriteLine($"Remove [{remove.Count()}]:");
                    foreach (var f in remove) Console.WriteLine($"  {f}");
                }
            }
            // Prepare a list of files to uninstall for downgrade or props change, and another list of products to install/upgrade
            if (commandLine.ClearStagingFolder)
            {
                commandLine.DeleteStagingFolder();
                install = products;
            }
            else
            {
                (uninstall, install) = products.SeparateActions(localMsiFile =>
                {
                    var localInfoFile = Path.ChangeExtension(localMsiFile, ProductInfo.LocalInfoFileExtension);
                    var installedProduct = ProductInfo.FindInstalled(localInfoFile).Result;
                    // some backward compatibility in case if was not serialized
                    if (null != installedProduct)
                    {
                        if (string.IsNullOrEmpty(installedProduct.LocalPath)) installedProduct.LocalPath = localMsiFile;
                    }
                    return installedProduct;
                },
                commandLine.VersionPrefixes.Split(','));
            }
            if (commandLine.Verbose)
            {
                if (uninstall.Any())
                {
                    Console.WriteLine($"Uninstall [{uninstall.Count()}]:");
                    foreach (var f in uninstall) Console.WriteLine($"  {f.Uri}");
                }
                if (install.Any())
                {
                    Console.WriteLine($"Install [{install.Count()}]:");
                    foreach (var f in install) Console.WriteLine($"  {f.Uri}");
                }
            }
            // Ok, now we can proceed with changes:
            if (0 == exitCode)
            {
                // 1) Uninstall what's not needed anymore...
                foreach (var f in remove)
                {
                    var err = commandLine.Uninstall(f, !commandLine.DryRun);
                    if (0 == exitCode && 0 != err) { exitCode = err; break; }
                }
                // 2) Uninstall whose to be changed
                foreach (var f in uninstall)
                {
                    var err = commandLine.Uninstall(f.LocalPath, false);
                    if (0 == exitCode && 0 != err) { exitCode = err; break; }
                }
                // 3) Download/cache existing and new
                if (install.Any())
                {
                    var err = commandLine.DownloadProducts(install, commandLine.Timeout);
                    if (0 == err)
                    {
                        // 4) Install changed and new
                        foreach (var p in install)
                        {
                            err = commandLine.Install(p);
                            if (0 == exitCode && 0 != err) { exitCode = err; break; }
                        }
                    }
                }
            }
            return exitCode;
        }
    }
}
