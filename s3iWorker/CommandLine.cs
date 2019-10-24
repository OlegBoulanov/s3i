using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Web;

using Microsoft.Extensions.Logging;

using s3iLib;

namespace s3iWorker
{
    internal class CommandLine : CommandLineBase
    {
        [CommandLineKey("Run in command line mode", "-c", "--console")]
        public bool RunConsole { get; set; }
        [CommandLineKey("Path to excutable to run", "-p", "--path <path-to-exe>")]
        public string ProcessFilePath { get; set; } = $"{Path.GetDirectoryName(HttpUtility.UrlDecode(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).AbsolutePath))}{Path.DirectorySeparatorChar}s3i.exe";
        [CommandLineKey("Process timeout", "-t", "--timeout <timespan>")]
        public TimeSpan ProcessTimeout { get; set; } = TimeSpan.FromMinutes(5);
        [CommandLineKey("Process start delay", "-d", "--delay <timespan>")]
        public TimeSpan StartProcessDelay { get; set; } = TimeSpan.FromSeconds(8);
        [CommandLineKey("Logging level", "-l", "--level <log-level>")]
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }
}
