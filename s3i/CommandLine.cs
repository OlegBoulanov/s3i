using System;

using s3i_lib;

namespace s3i
{
    public class CommandLine : CommandLineBase
    {
        [CommandLine("Print help info", "-h", "--help")]
        public bool PrintHelp { get; set; } = false;

        [CommandLine("AWS user profile name", "-p", "--profile")]
        public string ProfileName { get; set; } = "default";

        [CommandLine("Environment variable name (default command line)", "-e", "--envvar")]
        public string EnvironmentVariableName { get; set; } = "s3i_args";

        [CommandLine("Path to temp folder", "-x", "--temp")]
        public string TempFolder { get; set; } = Environment.GetEnvironmentVariable("TEMP");

        [CommandLine("MsiExec command", "-m", "--msiexec")]
        public string MsiExecCommand { get; set; } = "msiexec.exe";

        [CommandLine("MsiExec extra args", "-a", "--msiargs")]
        public string MsiExecArgs { get; set; }

        [CommandLine("Installation timeout", "-t", "--timeout")]
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(3);

        [CommandLine("Dry run", "-d", "--dryrun")]
        public bool DryRun { get; set; } = false;

        [CommandLine("Print full log info", "-v", "--verbose")]
        public bool Verbose { get; set; }

    }
}
