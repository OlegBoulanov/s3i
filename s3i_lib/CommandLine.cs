using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s3i_lib
{
    public class CommandLine
    {
        public enum OptionType { None, ProfileName, TempFolder, MsiExecCmd, MsiExecKeys, MsiExtraArgs, Timeout};
        public enum FlagType { None, DryRun, Verbose };
        public class KeyInfo
        {
            public string BriefKey { get; set; }
            public string LongKey { get; set; }
        }
        public class OptionInfo : KeyInfo
        {
            public string Value { get; set; }
        }
        public class FlagInfo : KeyInfo
        {
            public bool Value { get; set; }
        }
        public Dictionary<OptionType, OptionInfo> Options { get; protected set; }
        public Dictionary<FlagType, FlagInfo> Flags { get; protected set; }
        public IList<string> Args { get; protected set; } = new List<string>();
        protected CommandLine()
        {
            Options = new Dictionary<OptionType, OptionInfo> {
                { OptionType.ProfileName, new OptionInfo { BriefKey = "-p", LongKey = "--profile", Value = "default" } },
                { OptionType.TempFolder, new OptionInfo { BriefKey = "-t", LongKey = "--temp", Value = Environment.GetEnvironmentVariable("TEMP") } },
                { OptionType.MsiExecCmd, new OptionInfo { BriefKey = "-e", LongKey = "--msiexec", Value = "msiexec.exe" } },
                { OptionType.MsiExecKeys, new OptionInfo { BriefKey = "-k", LongKey = "--msikeys", Value = "/i" } },
                { OptionType.MsiExtraArgs, new OptionInfo { BriefKey = "-a", LongKey = "--msiargs", Value = "" } },
                { OptionType.Timeout, new OptionInfo { BriefKey = "-t", LongKey = "--timeout", Value = "00:01:00" } },
            };
            Flags = new Dictionary<FlagType, FlagInfo> {
                { FlagType.DryRun, new FlagInfo { BriefKey = "-d", LongKey = "--dryrun", Value = false } },
                { FlagType.Verbose, new FlagInfo { BriefKey = "-v", LongKey = "--verbose", Value = false } },
            };
        }
        public string Help
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Options:");
                foreach (var o in Options.Keys)
                {
                    var oi = Options[o];
                    sb.AppendLine($"  {oi.BriefKey}, {oi.LongKey,-12} [{oi.Value}]");
                }
                sb.AppendLine($"Flags:");
                foreach (var f in Flags.Keys)
                {
                    var fi = Flags[f];
                    sb.AppendLine($"  {fi.BriefKey}, {fi.LongKey,-12}");
                }
                return sb.ToString();
            }
        }
        public static CommandLine Parse(params string [] args)
        {
            var commandLine = new CommandLine();
            var currentOption = OptionType.None;
            var currentFlag = FlagType.None;
            foreach (var arg in args)
            {
                if (string.IsNullOrWhiteSpace(arg)) continue;
                if (OptionType.None == currentOption)
                {
                    foreach (var ot in commandLine.Options.Keys)
                    {
                        var o = commandLine.Options[ot];
                        if (o.BriefKey.Equals(arg, StringComparison.CurrentCultureIgnoreCase) || o.LongKey.Equals(arg, StringComparison.CurrentCultureIgnoreCase))
                        {
                            currentOption = ot;
                            break;
                        }
                    }
                    if (OptionType.None != currentOption) continue;
                    foreach (var ft in commandLine.Flags.Keys)
                    {
                        var f = commandLine.Flags[ft];
                        if (f.BriefKey.Equals(arg, StringComparison.CurrentCultureIgnoreCase) || f.LongKey.Equals(arg, StringComparison.CurrentCultureIgnoreCase))
                        {
                            currentFlag = ft;
                            break;
                        }
                    }
                }
                if (OptionType.None != currentOption)
                {
                    commandLine.Options[currentOption].Value = arg;
                    currentOption = OptionType.None;
                }
                else if (FlagType.None != currentFlag)
                {
                    commandLine.Flags[currentFlag].Value = true;
                    currentFlag = FlagType.None;
                }
                else
                {
                    commandLine.Args.Add(arg);
                }
            }
            //if (OptionType.None != currentOption) commandLine.Options[currentOption] = "";
            return commandLine;
        }
    }
}
