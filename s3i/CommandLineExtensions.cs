using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using s3iLib;
using Amazon.S3.Util;

namespace s3i
{
    public static class CommandLineExtensions
    {
        static string DryRunHeader { get; } = "(DryRun)";
        static string ExecuteHeader { get; } = "(Execute)";
#pragma warning disable CA1031// warning CA1031: Modify '***' to catch a more specific exception type, or rethrow the exception.
        internal static T LogAndExecute<T>(this CommandLine commandLine, string info, Func<bool, T> func, Func<Exception, T> onException, Action<string> log)
        {
            if (commandLine.Verbose)
            {
                log($"{(commandLine.DryRun ? DryRunHeader : commandLine.Verbose ? ExecuteHeader : "")} {info}");
            }
            try
            {
                return func(!commandLine.DryRun);
            }
            catch (Exception x)
            {
                log($"? {x.Format(4)}");
                return onException(x);
            }
        }
#pragma warning restore CA1031
        internal static int DeleteStagingFolder(this CommandLine commandLine)
        {
            return commandLine.LogAndExecute($"Delete staging folder {commandLine.StagingFolder}",
                (run) =>
                {
                    if (run && Directory.Exists(commandLine.StagingFolder)) Directory.Delete(commandLine.StagingFolder, true);
                    return 0;
                },
                x => x.HResult,
                s => Console.WriteLine(s)
                );
        }
        internal static int DownloadProducts(this CommandLine commandLine, IEnumerable<ProductInfo> products, TimeSpan timeout)
        {
            return commandLine.LogAndExecute($"Download {products.Count()} product{products.Count().Plural()}:{products.Aggregate(new StringBuilder(), (sb, p) => { sb.AppendLine(); sb.Append($"  {p.Uri}"); return sb; }, sb => sb.ToString())}",
                (run) => { if (run) ProductCollection.DownloadInstallers(products, commandLine.StagingFolder).Wait(timeout); return 0; },
                x => x.HResult,
                s => Console.WriteLine(s)
                );
        }
        internal static int Uninstall(this CommandLine commandLine, string msiFilePath, bool deleteAllFiles)
        {
            var retCode = commandLine.LogAndExecute($"Uninstall {msiFilePath}",
                (run) => run ? Installer.Uninstall(msiFilePath, commandLine.MsiExecArgs, commandLine.DryRun, commandLine.Timeout) : 0,
                x => x.HResult,
                s => Console.WriteLine(s)
                );
            if (0 != retCode)
            {
                Console.WriteLine($"? Uninstall returned {retCode}: {Win32Helper.ErrorMessage(retCode)}");
            }
            else if (deleteAllFiles)
            {
                foreach (var f in Directory.EnumerateFiles(Path.GetDirectoryName(msiFilePath), $"{Path.GetFileNameWithoutExtension(msiFilePath)}.*", SearchOption.TopDirectoryOnly))
                {
                    var res = commandLine.LogAndExecute($"Delete {f}",
                        (run) =>
                        {
                            if (run) File.Delete(f);
                            return 0;
                        },
                        x => x.HResult,
                        s => Console.WriteLine(s)
                        );
                    if (0 == retCode && 0 != res) retCode = res;
                }
            }
            return retCode;
        }

        internal static int Install(this CommandLine commandLine, ProductInfo product)
        {
            var retCode = commandLine.LogAndExecute($"Install {product.LocalPath}",
                (run) => run ? Installer.Install(product.LocalPath, product.Props, commandLine.MsiExecArgs, commandLine.DryRun, commandLine.Timeout) : 0,
                x => x.HResult,
                s => Console.WriteLine(s)
                );
            if (0 != retCode)
            {
                Console.WriteLine($"? Install returned {retCode}: {Win32Helper.ErrorMessage(retCode)}");
            }
            else
            {
                var localInfoFile = Path.ChangeExtension(product.LocalPath, ProductInfo.LocalInfoFileExtension);
                // do not update if dry run
                if (commandLine.DryRun && File.Exists(localInfoFile))
                {
                    Console.WriteLine($"{DryRunHeader} Save {localInfoFile}");
                }
                else
                {
                    Console.WriteLine($"Save {localInfoFile}");
                    Directory.CreateDirectory(Path.GetDirectoryName(localInfoFile));
                    product.SaveToLocal(localInfoFile).Wait(2000);
                }
                //var res = commandLine.LogAndExecute($"Save info for {product.AbsoluteUri}",
                //   () => { 
                //       product.SaveToLocal().Wait(2000); 
                //       return 0; 
                //   },
                //   x => x.HResult,
                //   s => Console.WriteLine(s)
                //   );
            }
            return retCode;
        }
        internal static void SetDefaults(this CommandLine commandLine, string exeFileName)
        {
            Contract.Requires(null != commandLine);
            if (string.IsNullOrWhiteSpace(commandLine.StagingFolder))
            {
                var temp = Environment.GetEnvironmentVariable("TEMP") ?? $"{Environment.GetEnvironmentVariable("HOME")}{Path.DirectorySeparatorChar}Temp";
                commandLine.StagingFolder = $"{temp}{Path.DirectorySeparatorChar}{exeFileName}";
            }
        }
#pragma warning disable CA1031// warning CA1031: Modify '***' to catch a more specific exception type, or rethrow the exception.
#pragma warning disable CA1303// warning CA1303: Method '***' passes a literal string as parameter 'value'
        internal static Outcome<bool, string> Validate(this CommandLine commandLine)
        {
            Contract.Requires(null != commandLine);
            // we must have staging folder
            if (string.IsNullOrWhiteSpace(commandLine.StagingFolder)) return new Outcome<bool, string>(false).AddErrors("Staging folder is not specified, you may want to set TEMP or HOME environment variable");
            if (!commandLine.StagingFolder.EndsWith(Path.DirectorySeparatorChar)) commandLine.StagingFolder += Path.DirectorySeparatorChar;
            // validate
            var outcome = new Outcome<bool, string>(true);
            foreach (var a in commandLine.Arguments)
            {
                try
                {
                    // this one throws... thank you, Amazon
                    var uri = new Uri(a);
                    if (!AmazonS3Uri.TryParseAmazonS3Uri(uri, out var s3uri)) { outcome.AddErrors($"Invalid AWS S3 Uri: {a}"); }
                }
                catch (Exception x)
                {
                    outcome.AddErrors($"{x.GetType().Name} {x.Message}: {a}");
                }
            }
            return outcome;
        }
#pragma warning restore CA1303
#pragma warning restore CA1031
    }
}
