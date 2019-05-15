using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;

using s3i_lib;

namespace s3i_lib
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

    public class Products : List<ProductInfo>
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
        public static async Task<Products> FromIni(Stream stream)
        {
            var products = new Products();
            await IniReader.Read(stream, async (sectionName, keyName, keyValue) =>
            {
                if (sectionProducts.Equals(sectionName, StringComparison.InvariantCultureIgnoreCase))
                {
                    products.Add(new ProductInfo { Name = keyName, Path = keyValue });
                }
                else if (0 < products.Count)
                {
                    products[products.Count - 1].Props.Add(keyName, keyValue);
                }
                await Task.CompletedTask;
            });
            return products;
        }
    }
}
