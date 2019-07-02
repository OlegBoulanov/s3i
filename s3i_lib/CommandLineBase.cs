using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace s3i_lib
{
    public abstract class CommandLineBase
    {
        public List<string> Arguments { get; protected set; }
        public string HelpHeader { get; set; }
        public void Parse(params string[] args)
        {
            Arguments = new List<string>();
            Parse(
                (s) => { Arguments.Add(s); },
                (p) => { SetProperty(p.Name, true); },
                (p, v) => {
                    if (typeof(string) == p.PropertyType) SetProperty(p.Name, v);
                    else SetProperty(p, v, null);
                }, 
                args);
        }
        public void Parse(Action<string> onArgument, Action<PropertyInfo> onFlag, Action<PropertyInfo, string> onOption, string[] args)
        {
            Arguments = new List<string>();
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
                    foreach (var attr in prop.GetCustomAttributes(true))
                    {
                        if (!(attr is CommandLineAttribute cla)) continue;
                        if (!cla.Keys.Contains(a)) continue;
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
        public string Help()
        {
            var sb = new StringBuilder();
            sb.AppendLine(HelpHeader);
            var count = 0;
            foreach (var prop in GetType().GetProperties())
            {
                foreach (var attr in prop.GetCustomAttributes(true))
                {
                    if (!(attr is CommandLineAttribute cla)) continue;
                    if (0 == count++) sb.AppendLine("Options:");
                    sb.AppendLine($"  {cla.Keys.Aggregate((a, k) => { return $"{(string.IsNullOrEmpty(a) ? " " : $"{a},")} {k}"; })} - {cla.Help} [{prop.GetValue(this)}]");
                }
            }
            return sb.ToString();
        }
        void SetProperty(string name, object value)
        {
            GetType().InvokeMember(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty, Type.DefaultBinder, this, new Object[] { value });
        }
        void SetProperty(PropertyInfo prop, object value, object defaultValue)
        {
            var c = TypeDescriptor.GetConverter(prop.PropertyType);
            SetProperty(prop.Name, c.CanConvertFrom(typeof(string)) ? c.ConvertFrom(value) : defaultValue);
        }
    }
}
