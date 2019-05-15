using System;
using System.Collections.Generic;
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
            if(args.Length < 1)
            {
                Console.WriteLine($"s3i - download and install msi(s) from AWS S3");
                Console.WriteLine($"Usage:");
                Console.WriteLine($"  s3i [(-p|--profile) <profileName>] <S3-URI of .msi, .ini, or .json> ...");
                return -1;
            }
            var commandLine = CommandLine.Parse(args);
            var s3 = new S3Helper(commandLine.Options[CommandLine.OptionType.ProfileName]);
            // can read products and download files in parallel
            var products = new Products();
            var res = Parallel.ForEach(commandLine.Args,
                async (uri) =>
                {
                    var installer = new Installer(s3);
                    var prods = await installer.ReadProducts(uri);
                    lock (products)
                    {
                        products.AddRange(prods);
                    }
                });



            // but installation needs to be sequential
            return 0;
        }

    }
}
