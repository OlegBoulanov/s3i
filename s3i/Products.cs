using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;

namespace s3i
{
    class Products
    {
        public class ProductProps : Dictionary<string, string>
        {
        }
        public class ProductInfo
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public ProductProps Props { get; protected set; } = new ProductProps();
        }
        public List<ProductInfo> ProductsToInstall { get; protected set; } = new List<ProductInfo>();
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        public static Products FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Products>(json);
        }
        static readonly string sectionProducts = "$products$";
        static readonly Regex rexSectionName = new Regex(@"^\s*\[([^\]]+)\]\s*$", RegexOptions.Compiled);
        public static Products FromIni(Stream stream)
        {
            var productList = new List<string>();  // ordered
            var productInfos = new Dictionary<string, ProductInfo>();
            using (var reader = new StreamReader(stream))
            {
                for (string sectionName = null, line; null != (line = reader.ReadLine());)
                {
                    var m = rexSectionName.Match(line);
                    if (m.Success && 1 < m.Groups.Count)
                    {
                        sectionName = m.Groups[1].Value.Trim();
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(sectionName)) continue;
                    line = line.Split(';')[0];
                    string keyName = null, keyValue = null;
                    for (var pe = line.IndexOf('='); 0 <= pe;)
                    {
                        keyName = line.Substring(0, pe).Trim();
                        keyValue = line.Substring(pe + 1).Trim();
                        break;
                    }
                    if (string.IsNullOrWhiteSpace(keyName) || string.IsNullOrWhiteSpace(keyValue)) continue;
                    if (sectionProducts.Equals(sectionName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!productList.Contains(keyName)) productList.Add(keyName);
                        if (!productInfos.ContainsKey(keyName)) productInfos.Add(keyName, new ProductInfo { Path = keyValue  });
                    }
                    else
                    {
                        if (!productProps.ContainsKey(sectionName)) productProps.Add(sectionName, new ProductProps());
                        productProps[sectionName].Add(keyName, keyValue);
                    }
                }
            }
            // convert to ordered list
            var products = new Products();
            foreach (var productName in productList)
            {
                var props = productProps[productName]; 
                products.ProductsToInstall.Add(new ProductInfo { Path = props. });
            }
            return products;
        }
    }
}
