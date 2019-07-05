using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s3i_lib
{
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple=false)]
    public class CommandLineAttribute : Attribute
    {
        public string Help { get; set; }
        public List<string> Keys { get; set; }
        public CommandLineAttribute(string help, params string[] keys)
        {
            Help = help;
            Keys = new List<string>(keys);
        }
    }
}
