using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s3i_lib
{
    public class CommandLine
    {
        public enum OptionType { None, ProfileName, TempFolder };
        public  Dictionary<OptionType, string> Options { get; protected set; }
        public IList<string> Args { get; protected set; } = new List<string>();
        protected CommandLine()
        {
            Options = new Dictionary<OptionType, string>() {
                { OptionType.ProfileName, "default" },
                { OptionType.TempFolder, Environment.GetEnvironmentVariable("TEMP") },
            };
        }
        public static CommandLine Parse(params string [] args)
        {
            var commandLine = new CommandLine();
            OptionType currentOption = OptionType.None;
            foreach(var arg in args)
            {
                if (string.IsNullOrWhiteSpace(arg)) continue;
                switch (arg)
                {
                    case "-p":
                    case "--profile":
                        currentOption = OptionType.ProfileName;
                        continue;
                    case "-t":
                    case "--temp":
                        currentOption = OptionType.TempFolder;
                        continue;
                }
                if (OptionType.None != currentOption)
                {
                    commandLine.Options[currentOption] = arg;
                    currentOption = OptionType.None;
                }
                else
                {
                    commandLine.Args.Add(arg);
                }
            }
            return commandLine;
        }
    }
}
