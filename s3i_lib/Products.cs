using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;

using s3i_lib;

using Amazon.S3.Util;

namespace s3i_lib
{
    public class ProductProps : Dictionary<string, string>
    {
    }
    public class ProductInfo
    {
        public string Name { get; set; }
        public string RelativeUri { get; set; }
        public string AbsoluteUri { get; set; }
        public string LocalPath { get; set; }
        public ProductProps Props { get; protected set; } = new ProductProps();
    }

    public class Products : List<ProductInfo>
    {
        public Products()
        {
        }
        public Products(IEnumerable<ProductInfo> p)
        {
            AddRange(p);
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        public static async Task<Products> FromJson(Stream stream)
        {
            using(var reader = new StreamReader(stream))
            {
                return JsonConvert.DeserializeObject<Products>(await reader.ReadToEndAsync());
            }
        }
        static readonly string sectionProducts = "$products$";
        public static async Task<Products> FromIni(Stream stream, string baseUri, string tempFilePath)
        {
            var products = new Products();
            await IniReader.Read(stream, async (sectionName, keyName, keyValue) =>
            {
                if (sectionProducts.Equals(sectionName, StringComparison.InvariantCultureIgnoreCase))
                {
                    products.Add(new ProductInfo { Name = keyName, RelativeUri = keyValue });
                }
                else
                {
                    var product = products.FirstOrDefault((p) => { return sectionName.Equals(p.Name); });
                    if(default(ProductInfo) != product) product.Props.Add(keyName, keyValue);
                }
                await Task.CompletedTask;
            });
            products.ForEach((p) =>
            {
                p.AbsoluteUri = p.RelativeUri.RebaseUri(baseUri);
                p.LocalPath = p.AbsoluteUri.MapToLocalPath(tempFilePath);
            });
            return products;
        }
        public static async Task<Products> ReadProducts(S3Helper s3, string configFileUri, string tempFilePath)
        {
            var products = new Products();
            var uri = new AmazonS3Uri(configFileUri);
            await s3.DownloadAsync(uri.Bucket, uri.Key, DateTime.MinValue, async (contentType, stream) =>
            {
                switch (Path.GetExtension(configFileUri).ToLower())
                {
                    case ".msi":
                        products.Add(new ProductInfo { Name = configFileUri, RelativeUri = configFileUri, AbsoluteUri = configFileUri });
                        await Task.CompletedTask;
                        break;
                    case ".ini":
                        products.AddRange(await Products.FromIni(stream, configFileUri, tempFilePath));
                        break;
                    case ".json":
                        products.AddRange(await Products.FromJson(stream));
                        break;
                    default:
                        throw new FormatException($"Unsupported file extension in {configFileUri}");
                }
            });
            return products;
        }
        public static async Task<Products> ReadProducts(S3Helper s3, IEnumerable<string> uris, string tempFilePath)
        {
            var arrayOfProducts = await Task.WhenAll(
                uris.Aggregate(new ConcurrentQueue<Task<Products>>(),
                (tasks, uri) =>
                {
                    tasks.Enqueue(Task<Products>.Run(() =>
                    {
                        return ReadProducts(s3, uri, tempFilePath);
                    }));
                    return tasks;
                }));
            return new Products(arrayOfProducts.SelectMany(x => x));
            //return arrayOfProducts.Aggregate(new Products(), (p, pp) => { p.AddRange(pp); return p; });
        }

        public async Task DownloadInstallers(S3Helper s3, string localPathBase)
        {
            await Task.WhenAll(
                this.Aggregate(new ConcurrentQueue<Task>(),
                    (tasks, product) =>
                    {
                        var uri = new AmazonS3Uri(product.AbsoluteUri);
                        product.LocalPath = product.AbsoluteUri.MapToLocalPath(localPathBase);
                        Directory.CreateDirectory(Path.GetDirectoryName(product.LocalPath));
                        tasks.Enqueue(s3.DownloadAsync(uri.Bucket, uri.Key, product.LocalPath));
                        return tasks;
                    }
                )
            );
        }

    }
}
