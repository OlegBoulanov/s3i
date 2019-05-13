using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;

using s3i.Readers;

namespace s3i
{
    class ProductProps : Dictionary<string, string>
    {
    }
    class ProductInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ProductProps Props { get; protected set; } = new ProductProps();
    }

    class Products : List<ProductInfo>
    {
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        public static Products FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Products>(json);
        }
        static readonly string sectionProducts = "$products$";
        public static Products FromIni(Stream stream)
        {
            var products = new Products();
            IniReader.Read(stream, (sectionName, keyName, keyValue) =>
            {
                if (sectionProducts.Equals(sectionName, StringComparison.InvariantCultureIgnoreCase))
                {
                    products.Add(new ProductInfo { Name = keyName, Path = keyValue });
                }
                else if (0 < products.Count)
                {
                    products[products.Count - 1].Props.Add(keyName, keyValue);
                }
            });
            return products;
        }
    }
}
