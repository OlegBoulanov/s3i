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
    class Program
    {
        static int Main(string[] args)
        {
            // to allow compilation on c# 7.0 (mono)
            return __Main(args).Result;
        }
        static async Task<int> __Main(string[] args)
        {
            var commandLine = CommandLine.Parse(args);
            if (commandLine.Args.Count < 1)
            {
                Console.WriteLine($"s3i - download and install msi(s) from AWS S3");
                Console.WriteLine($"Usage:");
                Console.WriteLine($"  s3i [<option> <value> ...] [<flag> ...] <S3-URI of .msi, .ini, or .json> ...");
                Console.WriteLine(commandLine.Help);
                return -1;
            }
            var verbose = commandLine.Flags[CommandLine.FlagType.Verbose].Value;
            var dryrun = commandLine.Flags[CommandLine.FlagType.DryRun].Value;
            TimeSpan msiExecTimeout = TimeSpan.FromMinutes(60);
            var s = commandLine.Options[CommandLine.OptionType.Timeout].Value;
            if(!TimeSpan.TryParse(s, out msiExecTimeout))
            {
                throw new FormatException($"Invalid timeout value: {s}");
            }
            var tempFolder = commandLine.Options[CommandLine.OptionType.TempFolder].Value;
            var clock = System.Diagnostics.Stopwatch.StartNew();
            var s3 = new S3Helper(commandLine.Options[CommandLine.OptionType.ProfileName].Value);
            if (verbose)
            {
                Console.WriteLine("Command line args:");
                Console.WriteLine(commandLine.Values);
            }
            // read product descriptions in parallel
            string baseUri = null;
            var products = await Products.ReadProducts(s3, commandLine.Args.Select(
                (uri, index) =>
                {
                    // next product path can be ralative to previous base
                    return baseUri = (0 == index ? uri : uri.RebaseUri(baseUri));
                }), tempFolder);
            //System.Net.ServicePointManager.DefaultConnectionLimit = 50;
            //
            if (verbose)
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
            await products.DownloadInstallers(s3, tempFolder);
            // but installation needs to be sequential
            var msiExecKeys = commandLine.Options[CommandLine.OptionType.MsiExecKeys].Value;
            var msiArgs = commandLine.Options[CommandLine.OptionType.MsiExtraArgs].Value;
            foreach(var product in products)
            {
                var installer = new Installer(product);
                var commandArgs = installer.FormatCommand(msiExecKeys, msiArgs);
                if (verbose || dryrun)
                {
                    var header = dryrun ? "(DryRun)" : "(Install)";
                    Console.WriteLine();
                    Console.WriteLine($"{header} [{msiExecTimeout}] {Installer.MsiExec} {commandArgs}");
                }
                if (!dryrun)
                {
                    installer.RunInstall(commandArgs, msiExecTimeout);
                }
            }
            if (verbose)
            {
                Console.WriteLine();
                Console.WriteLine($"Elapsed: {clock.Elapsed}");
            }
            return 0;
        }

    }
}
