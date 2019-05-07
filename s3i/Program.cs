using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

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
            foreach(var uri in commandLine.Args)
            {
                var installer = new Installer(uri);
                var s3h = new S3Helper(commandLine.Options[CommandLine.OptionType.ProfileName]);
                await s3h.Download(installer.Uri, (contentType, stream) => {
                    Console.WriteLine($"{contentType}");
                    var doc = Products.FromIni(stream);
                    Console.WriteLine(doc.ToJson());
                });
            }
            return 0;
        }

    }
}
