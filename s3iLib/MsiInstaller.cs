using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace s3iLib
{
    public class MsiInstaller : Installer
    {
        //
        // https://learn.microsoft.com/en-us/windows/win32/msi/error-codes
        //
        public static string MsiExec { get; set; } = "msiexec.exe";
        public static string SupportedFileExtension { get; set; } = ".msi";
        public static char [] CharsToQuote = new char [] { ' ', '\t', '<', '>' };
        public static IEnumerable<int> RetryOnErrors = new List<int> { 1618, };
        public static TimeSpan RetryDelay = TimeSpan.FromSeconds(8);
        public static int RunInstall(string commandLineArgs, bool dryrun, TimeSpan timeout)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!dryrun)
                {
                    var cmdexe = "cmd.exe";
                    var args = $"/c {MsiExec.Quote(null)} {commandLineArgs}";
                    for (var attempt = 0; ++attempt <= 4; )
                    {
                        using var process = Process.Start(cmdexe, args);
                        if (timeout.TotalMilliseconds <= 0) return 0;
                        if (false == process.WaitForExit((int)timeout.TotalMilliseconds) || 0 != process.ExitCode)
                        {
                            Console.WriteLine($"{cmdexe} {args}");
                            Console.WriteLine($"{MsiExec} failed with {process.ExitCode}");
                        }
                        if(RetryOnErrors.Contains(process.ExitCode)) 
                        {
                            var delay = attempt * RetryDelay;
                            Console.WriteLine($"... will retry after {delay} delay ...");
                            Task.Delay(delay).Wait();
                        }
                        else
                        {
                            return process.ExitCode;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"(DryRun) {MsiExec} {commandLineArgs}");
                }
            }
            else
            {
                Console.WriteLine($"(Windows) {MsiExec} {commandLineArgs}");
            }
            return 0;
        }
        public static string FormatCommand(string msiFilePath, ProductPropertiesDictionary props, string prefix, string suffix)
        {
            Contract.Requires(null != msiFilePath);
            return $"{prefix} {msiFilePath.Replace('/', Path.DirectorySeparatorChar).Quote("\"", CharsToQuote)}{props?.Aggregate("", (s, a) => { return $"{s} {a.Key}={a.Value.Quote("\"", CharsToQuote)}"; })} {suffix}";
        }
        public static bool CanInstall(Uri uri)
        {
            Contract.Requires(null != uri);
            return 0 == string.Compare(SupportedFileExtension, Path.GetExtension(uri.AbsolutePath), true, CultureInfo.InvariantCulture);
        }
        #region Installer methods implementation
        public override int Install(Uri uri, ProductPropertiesDictionary props, string extraArgs, bool dryrun, TimeSpan timeout)
        {
            Contract.Requires(null != uri);
            return RunInstall(FormatCommand(uri.AbsolutePath, props, " /i ", extraArgs), dryrun, timeout);
        }
        public override int Uninstall(Uri uri, string extraArgs, bool dryrun, TimeSpan timeout)
        {
            Contract.Requires(null != uri);
            return RunInstall(FormatCommand(uri.AbsolutePath, null, " /x ", extraArgs), dryrun, timeout);
        }
        #endregion
    }
}

