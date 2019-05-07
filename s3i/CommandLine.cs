using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s3i
{
    class CommandLine
    {
        public enum OptionType { None, ProfileName };
        public  Dictionary<OptionType, string> Options { get; protected set; }
        public IList<string> Args { get; protected set; } = new List<string>();
        public CommandLine()
        {
            Options = new Dictionary<OptionType, string>() {
                { OptionType.ProfileName, "default" }
            };
        }
        public static CommandLine Parse(string [] args)
        {
            var commandLine = new CommandLine();
            OptionType currentOption = OptionType.None;
            foreach(var arg in args)
            {
                switch (arg)
                {
                    case "-p":
                    case "--profile":
                        currentOption = OptionType.ProfileName;
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
