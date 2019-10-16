using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s3iLib
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
    public sealed class CommandLineKeyAttribute : Attribute
    {
        public string Help { get; set; }
        public List<string> Keys { get; }
        public CommandLineKeyAttribute(string help, params string[] keys)
        {
            Help = help;
            Keys = new List<string>(keys);
        }
        public bool IsKey(string s)
        {
            return !string.IsNullOrWhiteSpace(Keys.Find(k => s == k.Split(' ', '\t').FirstOrDefault()));
        }
    }
}
