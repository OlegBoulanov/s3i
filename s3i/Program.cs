using System;
using System.Linq;
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
            var exeFileName = System.IO.Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            var commandLine = new CommandLine {
                HelpHeader = $"S3 download and install{Environment.NewLine} Usage:{Environment.NewLine}  {exeFileName} [<option> ...] <products> ..."
            };
            commandLine.Parse(args);
            if (commandLine.Arguments.Count < 1)
            {
                Console.WriteLine(commandLine.Help());
                return -1;
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
            // but installation needs to be sequential
            foreach(var product in products)
            {
                var installer = new Installer(product);
                var commandArgs = installer.FormatCommand(commandLine.MsiExecKeys, commandLine.MsiExecArgs);
                if (commandLine.Verbose || commandLine.DryRun)
                {
                    var header = commandLine.DryRun ? "(DryRun)" : "(Install)";
                    Console.WriteLine();
                    Console.WriteLine($"{header} [{commandLine.Timeout}] {Installer.MsiExec} {commandArgs}");
                }
                if (!commandLine.DryRun)
                {
                    installer.RunInstall(commandArgs, commandLine.Timeout);
                }
            }
            if (commandLine.Verbose)
            {
                Console.WriteLine();
                Console.WriteLine($"Elapsed: {clock.Elapsed}");
            }
            return 0;
        }

    }
}
