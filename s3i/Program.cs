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
        static async Task<int> Main(string[] args)
        {
            var commandLine = CommandLine.Parse(args);
            if (commandLine.Args.Count < 1)
            {
                Console.WriteLine($"s3i - download and install msi(s) from AWS S3");
                Console.WriteLine($"Usage:");
                Console.WriteLine($"  s3i [(-p|--profile) <profileName>] <S3-URI of .msi, .ini, or .json> ...");
                //Console.WriteLine($"{commandLine.Options.Aggregate($"Options:{Environment.NewLine}", (s, o) => { s += $"{o.Key} = {o.Value}{Environment.NewLine}"; return s; })}");
                return -1;
            }
            var s3 = new S3Helper(commandLine.Options[CommandLine.OptionType.ProfileName]);
            // read product descriptions in parallel
            string baseUri = null;
            var products = await Products.ReadProducts(s3, commandLine.Args.Select(
                (uri, index) =>
                {
                    // next product path can be ralative to previous base
                    return baseUri = (0 == index ? uri : uri.RebaseUri(baseUri));
                }), commandLine.Options[CommandLine.OptionType.TempFolder]);
            //System.Net.ServicePointManager.DefaultConnectionLimit = 50;
            //
            if (commandLine.Verbose)
            {
                foreach (var p in products)
                {
                    Console.WriteLine($"{p.Name}: {p.AbsoluteUri} => {p.LocalPath}");
                    foreach (var pp in p.Props)
                    {
                        Console.WriteLine($"  {pp.Key} = {pp.Value}");
                    }
                }
            }
            // downloading files also can be parallel
            await products.DownloadInstallers(s3, commandLine.Options[CommandLine.OptionType.TempFolder]);
            // but installation needs to be sequential
            var commands = products.Aggregate(new List<string>(),
                (list, p) => {
                    list.Add($"{p.Props.Aggregate($"{commandLine.Options[CommandLine.OptionType.MsiExecCmd]} \"{p.LocalPath}\"", (s, a) => { s = $"{s} {a.Key}=\"{a.Value}\""; return s; })}");
                    return list;
                });
            if (commandLine.Verbose || commandLine.DryRun)
            {
                foreach (var c in commands)
                {
                    var header = commandLine.DryRun ? "(DryRun) " : "(Exec) ";
                    Console.WriteLine();
                    Console.WriteLine($"{header}{c}");
                }
            }
            return 0;
        }

    }
}
