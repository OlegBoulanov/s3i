using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace s3i_lib
{
    public class Installer
    {
        public enum Action { NoAction, Install, Reinstall, Uninstall };
        public static string MsiExec { get; } = "msiexec.exe";
        public static string InstallerFileExtension { get; } = ".msi";
        public static int RunInstall(string commandLineArgs, TimeSpan timeout)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using (var process = Process.Start(MsiExec, commandLineArgs))
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
                Console.WriteLine($"(Windows) {MsiExec} {commandLineArgs}");
                return 0;
            }
        }
        public static string FormatCommand(string msiFilePath, ProductProps props, string msiExecKeys, string extraArgs)
        {
            StringBuilder sb = new StringBuilder();
            // begin with list of quoted if necessary props
            if(null != props) sb.Append(props.Aggregate($"{msiExecKeys} {msiFilePath.Quote("\"")}", (s, a) => { s = $"{s} {a.Key}={a.Value.Quote("\"")}"; return s; }));
            // now append extra args, so they may override props and set more msiexec options
            if (!string.IsNullOrEmpty(extraArgs)) sb.AppendFormat(" {0}", extraArgs);
            return sb.ToString();
        }
        public static int Install(string msiFilePath, ProductProps props, string extraArgs, TimeSpan timeout)
        {
            return RunInstall(FormatCommand(msiFilePath, props, "/i", extraArgs), timeout);
        }
        public static int Uninstall(string msiFilePath, string extraArgs, TimeSpan timeout)
        {
            return RunInstall(FormatCommand(msiFilePath, null, "/x", "/qn " + extraArgs), timeout);
        }
    }
}

