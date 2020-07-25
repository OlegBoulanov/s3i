using System;
using System.IO;

using s3iLib;

namespace s3i
{
    public class CommandLine : CommandLineBase
    {
        [CommandLineKey("Print this help info", "-h", "--help")]
        public bool PrintHelp { get; set; } = false;

        [CommandLineKey("AWS user profile name", "-p", "--profile <profile-name>")]
        public string ProfileName { get; set; } = "default";

        [CommandLineKey("AWS default region name", "-r", "--region <region-name>")]
        public string RegionName { get; set; } = "us-east-1";

        [CommandLineKey("List of comma separated allowed version prefixes", "-x", "--prefixes <list>")]
        public string VersionPrefixes { get; set; } = "v,V,ver,Ver";

        [CommandLineKey("Environment variable name (default command line)", "-e", "--envvar <var-name>")]
        public string EnvironmentVariableName { get; set; } = "s3i_args";

        [CommandLineKey("Path to staging folder", "-s", "--stage <path>")]
        public string StagingFolder { get; set; } = null;

        [CommandLineKey("Clear staging folder at startup", "-c", "--clean")]
        public bool ClearStagingFolder { get; set; } = false;

        [CommandLineKey("MsiExec command", "-m", "--msiexec <path>")]
        public string MsiExecCommand { get; set; } = "msiexec.exe";

        [CommandLineKey("MsiExec extra args", "-a", "--msiargs <args>")]
        public string MsiExecArgs { get; set; } = "/passive";

        [CommandLineKey("Installation timeout", "-t", "--timeout <timespan>")]
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(3);

        [CommandLineKey("Dry run", "-d", "--dryrun")]
        public bool DryRun { get; set; } = false;

        [CommandLineKey("Print full log info", "-v", "--verbose")]
        public bool Verbose { get; set; }
    }
}
