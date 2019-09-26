using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using System.IO;

using Amazon.S3.Util;
using System.Diagnostics;

namespace s3i_lib
{
    public class Installer
    {
        public enum Action { Install, Reinstall, Uninstall };
        public Dictionary<Action, string> ActionKeys { get; protected set; } = new Dictionary<Action, string> {
            { Action.Install, "/i" },
            { Action.Reinstall, "/fva" },
            { Action.Uninstall, "/x" },
        };
        public static string MsiExec { get; protected set; } = "msiexec.exe";
        public ProductInfo Product { get; protected set; }
        public Installer(ProductInfo product)
        {
            Product = product;
        }
        public int RunInstall(string commandLineArgs, TimeSpan timeout)
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
        public string FormatCommand(string msiExecKeys, string extraArgs)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Product.Props.Aggregate($"{msiExecKeys} {Product.LocalPath.Quote("\"")}", (s, a) => { s = $"{s} {a.Key}={a.Value.Quote("\"")}"; return s; }));
            // now apply extra args, so they may override ini props
            if (!string.IsNullOrEmpty(extraArgs)) sb.AppendFormat(" {0}", extraArgs);
            return sb.ToString();
        }
    }
}

