using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s3i_lib
{
    public class ProductProps : Dictionary<string, string>
    {
        public override string ToString()
        {
            return this.Aggregate(new StringBuilder(), (sb, kv) => { sb.AppendLine($"{kv.Key}={kv.Value}"); return sb; }, sb => sb.ToString());
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (!(obj is ProductProps)) return false;
            var pi = (ProductProps)obj;
            return Count == pi.Count && this.All(kv => pi.ContainsKey(kv.Key) && pi[kv.Key].Equals(kv.Value));
        }
    }
}
