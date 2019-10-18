using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace s3iLib
{
    public static class Installer
    {
        public enum Action { NoAction, Install, Reinstall, Uninstall };
        public static string MsiExec { get; set;  } = "msiexec.exe";
        public static string InstallerFileExtension { get; set; } = ".msi";
        public static int RunInstall(string commandLineArgs, bool dryrun, TimeSpan timeout)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !dryrun)
            {
                var args = $"/c {MsiExec.Quote(null)} {commandLineArgs}";
                using (var process = Process.Start("cmd.exe", args))
                {
                    if (timeout.TotalMilliseconds <= 0) return 0;
                    if (false == process.WaitForExit((int)timeout.TotalMilliseconds) || 0 != process.ExitCode)
                    {
                        Console.WriteLine($"{MsiExec} failed with {process.ExitCode}");
                    }
                    return process.ExitCode;
                }
            }
            else
            {
                Console.WriteLine($"{MsiExec} {commandLineArgs}");
                return 0;
            }
        }
        public static string FormatCommand(string msiFilePath, ProductPropertiesDictionary props, string prefix, string suffix)
        {
            StringBuilder sb = new StringBuilder($"{prefix} {msiFilePath.Quote("\"")}");
            // begin with list of quoted if necessary props
            if(null != props) sb.Append(props.Aggregate("", (s, a) => { s = $"{s} {a.Key}={a.Value.Quote("\"")}"; return s; }));
            // now append extra args, so they may override props and set more msiexec options
            if (!string.IsNullOrEmpty(suffix)) sb.Append($" {suffix}");
            return sb.ToString();
        }
        public static int Install(string msiFilePath, ProductPropertiesDictionary props, string extraArgs, bool dryrun, TimeSpan timeout)
        {
            return RunInstall(FormatCommand(msiFilePath, props, " /i ", extraArgs), dryrun, timeout);
        }
        public static int Uninstall(string msiFilePath, string extraArgs, bool dryrun, TimeSpan timeout)
        {
            return RunInstall(FormatCommand(msiFilePath, null, " /x ", extraArgs), dryrun, timeout);
        }
    }
}

