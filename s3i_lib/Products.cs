using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using Newtonsoft.Json;

using Amazon.S3.Util;

namespace s3i_lib
{
    public class Products : List<ProductInfo>
    {
        public Products() { }
        public Products(IEnumerable<ProductInfo> p) { AddRange(p); }
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
            await IniReader.Read(stream, (sectionName, keyName, keyValue) =>
            {
                if (sectionProducts.Equals(sectionName, StringComparison.InvariantCultureIgnoreCase))
                {
                    products.Add(new ProductInfo { Name = keyName, AbsoluteUri = keyValue });
                }
                else
                {
                    var product = products.FirstOrDefault((p) => { return sectionName.Equals(p.Name); });
                    if(default(ProductInfo) != product) product.Props.Add(keyName, keyValue);
                }
                //await Task.CompletedTask;
            });
            products.ForEach((p) =>
            {
                p.AbsoluteUri = p.AbsoluteUri.RebaseUri(baseUri);
                p.LocalPath = p.MapToLocalPath(tempFilePath);
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
                        products.Add(new ProductInfo { Name = configFileUri, AbsoluteUri = configFileUri });
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
                uris.Aggregate(new List<Task<Products>>(),
                (tasks, uri) =>
                {
                    tasks.Add(Task<Products>.Run(() =>
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
                this.Aggregate(new List<Task<HttpStatusCode>>(),
                    (tasks, product) =>
                    {
                        var uri = new AmazonS3Uri(product.AbsoluteUri);
                        product.LocalPath = product.MapToLocalPath(localPathBase);
                        Directory.CreateDirectory(Path.GetDirectoryName(product.LocalPath));
                        tasks.Add(s3.DownloadAsync(uri.Bucket, uri.Key, product.LocalPath));
                        return tasks;
                    }
                )
            );
        }
        public IEnumerable<string> FindFilesToUninstall(string tempFilePath)
        {
            return FilesToUninstall(Directory.EnumerateFileSystemEntries(tempFilePath, "*.msi", SearchOption.AllDirectories).Select(s => Path.Combine(tempFilePath, s)));
        }
        public static Func<string, string, bool> defaultPathCompare = (s1, s2) => { return 0 == string.Compare(s1, s2, true); };
        public IEnumerable<string> FilesToUninstall(IEnumerable<string> entries, Func<string, string, bool> compare = null)
        {
            if (null == compare) compare = defaultPathCompare;
            return entries.Where(e => !Exists(product => compare(product.LocalPath, e)));
        }
        public (IEnumerable<string> filesToUninstall, IEnumerable<ProductInfo> productsToInstall) Separate(string tempFilePath)
        {
            return Separate(FindFilesToUninstall(tempFilePath), p => { return ProductInfo.FindInstalled(tempFilePath).Result; });
        }
        public (IEnumerable<string> filesToUninstall, IEnumerable<ProductInfo> productsToInstall) Separate(IEnumerable<string> files, Func<string, ProductInfo> findInstalledProduct)
        {
            var uninstall = new List<string>(FilesToUninstall(files));
            var install = new List<ProductInfo>();
            foreach(var product in this) 
            {
                var installedProduct = findInstalledProduct(product.LocalPath);
                if (null == installedProduct)
                {
                    install.Add(product);
                    continue;
                }
                switch (product.CompareAndSelectAction(installedProduct))
                {
                    case Installer.Action.Install:
                        install.Add(product);
                        break;
                    case Installer.Action.Reinstall:
                        uninstall.Add(product.LocalPath);
                        install.Add(product);
                        break;
                    case Installer.Action.Uninstall:
                        uninstall.Add(product.LocalPath);
                        break;
                }
            }
            return (uninstall, install);
        }
    }
}
