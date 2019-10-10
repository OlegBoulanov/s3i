using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using s3i_lib;

namespace s3i
{
    public static class CommandLineExtensions
    {
        static string DryRunHeader { get; } = "(DryRun)";
        static string ExecuteHeader { get; } = "(Execute)";
        public static T LogAndExecute<T>(this CommandLine commandLine, string info, Func<T> func, Func<Exception, T> onException, Action<string> log)
        {
            if (commandLine.DryRun || commandLine.Verbose)
            {
                log($"{(commandLine.DryRun ? DryRunHeader : commandLine.Verbose ? ExecuteHeader : "")} {info}");
            }
            if (commandLine.DryRun) return default(T);
            try
            {
                return func();
            }
            catch(Exception x)
            {
                log($"? {x.Format(4)}");
                return onException(x);
            }
        }
        public static T LogToConsoleAndExecute<T>(this CommandLine commandLine, string info, Func<T> func, Func<Exception, T> onException)
        {
            return commandLine.LogAndExecute(info, func, onException, s => Console.WriteLine(s));
        }
        public static int Uninstall(this CommandLine commandLine, string msiFilePath, bool deleteAllFiles)
        {
            var retCode = commandLine.LogAndExecute($"Uninstall {msiFilePath}",
                () => Installer.Uninstall(msiFilePath, commandLine.MsiExecArgs, commandLine.Timeout),
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
                        () => { 
                            File.Delete(f); 
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

        public static int Install(this CommandLine commandLine, ProductInfo product)
        {
            var retCode = commandLine.LogAndExecute($"Install {product.LocalPath}",
                () => Installer.Install(product.LocalPath, product.Props, commandLine.MsiExecArgs, commandLine.Timeout),
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
                if(commandLine.DryRun && File.Exists(localInfoFile))
                {
                    Console.WriteLine($"{DryRunHeader} Save {localInfoFile}");
                }
                else
                {
                    Console.WriteLine($"Save {localInfoFile}");
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
    }
}
