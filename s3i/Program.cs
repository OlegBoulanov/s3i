using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using s3i_lib;

namespace s3i
{
    class CommandLine : CommandLineBase
    {
        [CommandLine("AWS user profile name", "-p", "--profile")]
        public string ProfileName { get; set; } = "default";

        [CommandLine("Path to temp folder", "-t", "--temp")]
        public string TesmpFolder { get; set; } = Environment.GetEnvironmentVariable("TEMP");

        [CommandLine("MsiExec command", "-m", "--msiexec")]
        public string MsiExecCommand { get; set; } = "msiexec.exe";

        [CommandLine("MsiExec keys", "-k", "--msikeys")]
        public string MsiExecKeys { get; set; } = "/i";

        [CommandLine("MsiExec extra args", "-a", "--msiargs")]
        public string MsiExecArgs { get; set; }

        [CommandLine("Installation timeout", "-u", "--timeout")]
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(3);

        [CommandLine("Dry run", "-d", "--dryrun")]
        public bool DryRun { get; set; }

        [CommandLine("Print full log info", "-v", "--verbose")]
        public bool Verbose { get; set; }
    }

    class Program
    {
        static int Main(string[] args)
        {
            // to allow compilation on c# 7.0 (mono)
            return __Main(args).Result;
        }
        static async Task<int> __Main(string[] args)
        {
            var commandLine = new CommandLine { HelpHeader = "S3 download and install" };
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
            string baseUri = null;
            var products = await Products.ReadProducts(s3, commandLine.Arguments.Select(
                (uri, index) =>
                {
                    // next product path can be ralative to previous base
                    return baseUri = (0 == index ? uri : uri.RebaseUri(baseUri));
                }), commandLine.TesmpFolder);
            //System.Net.ServicePointManager.DefaultConnectionLimit = 50;
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
            await products.DownloadInstallers(s3, commandLine.TesmpFolder);
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
