﻿using System;
using System.IO;

using s3i_lib;

namespace s3i
{
    public class CommandLine : CommandLineBase
    {
        [CommandLine("Print this help info", "-h", "--help")]
        public bool PrintHelp { get; set; } = false;

        [CommandLine("AWS user profile name", "-p", "--profile <profile-name>")]
        public string ProfileName { get; set; } = "default";

        [CommandLine("Environment variable name (default command line)", "-e", "--envvar <var-name>")]
        public string EnvironmentVariableName { get; set; } = "s3i_args";

        [CommandLine("Path to staging folder", "-s", "--stage <path>")]
        public string StagingFolder { get; set; } = Environment.GetEnvironmentVariable("TEMP") ?? $"{Environment.GetEnvironmentVariable("HOME")}{Path.DirectorySeparatorChar}Temp";

        [CommandLine("Clear staging folder at startup", "-c", "--clean")]
        public bool ClearStagingFolder { get; set; } = false;

        [CommandLine("MsiExec command", "-m", "--msiexec <path>")]
        public string MsiExecCommand { get; set; } = "msiexec.exe";

        [CommandLine("MsiExec extra args", "-a", "--msiargs <args>")]
        public string MsiExecArgs { get; set; } = "/passive";

        [CommandLine("Installation timeout", "-t", "--timeout <timespan>")]
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(3);

        [CommandLine("Dry run", "-d", "--dryrun")]
        public bool DryRun { get; set; } = false;

        [CommandLine("Print full log info", "-v", "--verbose")]
        public bool Verbose { get; set; }
    }
}
