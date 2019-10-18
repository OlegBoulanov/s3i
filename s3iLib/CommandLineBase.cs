using System;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace s3iLib
{
    public abstract class CommandLineBase
    {
        public List<string> Arguments { get; } = new List<string>();
        public string HelpHeader { get; set; }
        public string HelpTail { get; set; }
        public void Parse(IEnumerable<string> args)
        {
            Arguments.Clear();
            Parse(
                (s) => { Arguments.Add(s); },
                (p) => { SetProperty(p.Name, true); },
                (p, v) => {
                    if (typeof(string) == p.PropertyType) SetProperty(p.Name, v);
                    else SetProperty(p, v, null);
                }, 
                args);
        }
        public void Parse(Action<string> onArgument, Action<PropertyInfo> onFlag, Action<PropertyInfo, string> onOption, IEnumerable<string> args)
        {
            Contract.Requires(null != args);
            Arguments.Clear();
            PropertyInfo currentProp = null;
            foreach (var a in args)
            {
                if (null != currentProp)
                {
                    onOption?.Invoke(currentProp, a);
                    currentProp = null;
                    continue;
                }
                // check if arg is a key
                var propFound = false;
                foreach (var prop in GetType().GetProperties())
                {
                    foreach (CommandLineKeyAttribute attr in prop.GetCustomAttributes(true))
                    {
                        if (!attr.IsKey(a)) continue;
                        if (typeof(bool) == prop.PropertyType) onFlag?.Invoke(prop);
                        else currentProp = prop;
                        propFound = true;
                        break;
                    }
                    if (propFound) break;
                }
                if (propFound) continue;
                if (null == currentProp) onArgument?.Invoke(a);
            }
        }
        public static string FormatKeys(IEnumerable<string> keys)
        {
            return keys.Aggregate((a, k) => $"{(string.IsNullOrEmpty(a) ? " " : $"{a},")} {k}");
        }
        public string Help(int indent = 4)
        {
            var sb = new StringBuilder();
            sb.AppendLine(HelpHeader);
            int count = 0, keysLength = 0;
            foreach (var prop in GetType().GetProperties())
            {
                var attributes = prop.GetCustomAttributes(true).Where(a => a is CommandLineKeyAttribute);
                if (0 == attributes.Count()) continue;    // because Max may throw
                keysLength = Math.Max(keysLength, attributes.Max(a => FormatKeys(((CommandLineKeyAttribute)a).Keys).Length));
            }
            foreach (var prop in GetType().GetProperties())
            {
                var attributes = prop.GetCustomAttributes(true).Where(a => a is CommandLineKeyAttribute);
                foreach (CommandLineKeyAttribute attr in attributes)
                {
                    if (0 == count++) sb.AppendLine(" Options:");
                    sb.AppendLine($"  {FormatKeys(attr.Keys).PadRight(keysLength + indent)}  {attr.Help} [{prop.GetValue(this)}]");
                }
            }
            if (!string.IsNullOrEmpty(HelpTail)) sb.AppendLine(HelpTail);
            return sb.ToString();
        }
        void SetProperty(string name, object value)
        {
            _ = GetType().InvokeMember(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty, Type.DefaultBinder, this, new object[] { value }, CultureInfo.CurrentCulture);
        }
        void SetProperty(PropertyInfo prop, object value, object defaultValue)
        {
            var c = TypeDescriptor.GetConverter(prop.PropertyType);
            SetProperty(prop.Name, c.CanConvertFrom(typeof(string)) ? c.ConvertFrom(value) : defaultValue);
        }
    }
}
