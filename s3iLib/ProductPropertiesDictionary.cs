using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s3iLib
{
    public class ProductPropertiesDictionary : Dictionary<string, string>
    {
        public override string ToString()
        {
            return this.Aggregate(new StringBuilder(), (sb, kv) => { sb.AppendLine($"{kv.Key}={kv.Value}"); return sb; }, sb => sb.ToString());
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode(StringComparison.CurrentCulture);
        }
        public override bool Equals(object obj)
        {
            var pi = obj as ProductPropertiesDictionary;
            return Count == pi?.Count && this.All(kv => pi.ContainsKey(kv.Key) && pi[kv.Key].Equals(kv.Value, StringComparison.InvariantCulture));
        }
    }
}
