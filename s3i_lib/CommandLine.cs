using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s3i_lib
{

    public class CmdLine<OT, FT> //where OT:IEquatable<OT> where FT:IEquatable<FT>
    {
        public class ArgInfo<T>
        {
            public List<string> Keys { get; set; }
            public string Help { get; set; }
            public T Value { get; set; }
            public T Default { get; set; }
        }
        public string HelpHeader { get; set; }
        public IList<string> Args { get; set; } = new List<string>();
        public IDictionary<OT, ArgInfo<string>> Options { get; set; } = new Dictionary<OT, ArgInfo<string>>();
        public IDictionary<FT, ArgInfo<bool>> Flags { get; set; } = new Dictionary<FT, ArgInfo<bool>>();
        public void Parse(params string[] args)
        {
            var currentOption = default(OT);
            var currentFlag = default(FT);
            foreach (var arg in args)
            {
                //if (string.IsNullOrWhiteSpace(arg)) continue;
                if (default(OT).Equals(currentOption))
                {
                    currentOption = Options.FirstOrDefault(o => o.Value.Keys.Exists(k => k == arg)).Key;
                    if (!default(OT).Equals(currentOption)) continue;
                    currentFlag = Flags.FirstOrDefault(o => o.Value.Keys.Exists(k => k == arg)).Key;
                }
                if (!default(OT).Equals(currentOption))
                {
                    Options[currentOption].Value = arg;
                    currentOption = default(OT);
                }
                else if (!default(FT).Equals(currentFlag))
                {
                    Flags[currentFlag].Value = true;
                    currentFlag = default(FT);
                }
                else
                {
                    Args.Add(arg);
                }
            }
        }
        public string Help()
        {
            var sb = new StringBuilder();
            sb.AppendLine(HelpHeader);
            sb.AppendLine($"Options:");
            foreach (var ok in Options.Keys)
            {
                var o = Options[ok];
                sb.AppendLine($"  {o.Keys.Aggregate(" ", (a, k) => { return $"{a}, {k}"; })} [{o.Value}] - {o.Help}");
            }
            sb.AppendLine($"Flags:");
            foreach (var fk in Flags.Keys)
            {
                var f = Flags[fk];
                sb.AppendLine($"  {f.Keys.Aggregate(" ", (a, k) => { return $"{a}, {k}"; })} [{f.Value}] - {f.Help}");
            }
            return sb.ToString();
        }

    }


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
                { OptionType.Timeout, new OptionInfo { BriefKey = "-u", LongKey = "--timeout", Value = "00:01:00" } },
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
        public string Values
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Options:");
                foreach (var o in Options.Keys)
                {
                    var oi = Options[o];
                    sb.AppendLine($"  {oi.LongKey,-12} {oi.Value}");
                }
                sb.AppendLine($"Flags:");
                foreach (var f in Flags.Keys)
                {
                    var fi = Flags[f];
                    if(fi.Value) sb.AppendLine($"  {fi.LongKey,-12}");
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
