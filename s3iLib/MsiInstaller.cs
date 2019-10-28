using System;
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
        public static string MsiExec { get; set; } = "msiexec.exe";
        public static string SupportedFileExtension { get; set; } = ".msi";
        public static int RunInstall(string commandLineArgs, bool dryrun, TimeSpan timeout)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!dryrun)
                {
                    var args = $"/c {MsiExec.Quote(null)} {commandLineArgs}";
                    using var process = Process.Start("cmd.exe", args);
                    if (timeout.TotalMilliseconds <= 0) return 0;
                    if (false == process.WaitForExit((int)timeout.TotalMilliseconds) || 0 != process.ExitCode)
                    {
                        Console.WriteLine($"{MsiExec} failed with {process.ExitCode}");
                    }
                    return process.ExitCode;
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
            StringBuilder sb = new StringBuilder($"{prefix} {msiFilePath.Replace('/', '\\').Quote("\"")}");
            // begin with list of quoted if necessary props
            if (null != props) sb.Append(props.Aggregate("", (s, a) => { s = $"{s} {a.Key}={a.Value.Quote("\"")}"; return s; }));
            // now append extra args, so they may override props and set more msiexec options
            if (!string.IsNullOrEmpty(suffix)) sb.Append($" {suffix}");
            return sb.ToString();
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

