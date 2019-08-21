using System;

using s3i_lib;

namespace s3i
{
    public class CommandLine : CommandLineBase
    {
        [CommandLine("AWS user profile name", "-p", "--profile")]
        public string ProfileName { get; set; } = "default";

        [CommandLine("Path to temp folder", "-e", "--temp")]
        public string TempFolder { get; set; } = Environment.GetEnvironmentVariable("TEMP");

        [CommandLine("MsiExec command", "-m", "--msiexec")]
        public string MsiExecCommand { get; set; } = "msiexec.exe";

        [CommandLine("MsiExec keys", "-k", "--msikeys")]
        public string MsiExecKeys { get; set; } = "/i";

        [CommandLine("MsiExec extra args", "-a", "--msiargs")]
        public string MsiExecArgs { get; set; }

        [CommandLine("Installation timeout", "-t", "--timeout")]
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(3);

        [CommandLine("Dry run", "-d", "--dryrun")]
        public bool DryRun { get; set; }

        [CommandLine("Print full log info", "-v", "--verbose")]
        public bool Verbose { get; set; }
    }
}
