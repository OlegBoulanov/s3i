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
                return -1;
            }
            var s3 = new S3Helper(commandLine.Options[CommandLine.OptionType.ProfileName]);
            // can read products and download files in parallel
            string baseUri = null;
            var products = await Products.ReadProducts(s3, commandLine.Args.Select(
                (uri, i) =>
                {
                    return baseUri = (0 == i ? uri : uri.RebaseUri(baseUri));
                }));

            // but installation needs to be sequential
            return 0;
        }

    }
}
