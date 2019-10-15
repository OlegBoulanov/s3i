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
        public class InvalidUriException : ApplicationException 
        {
            public InvalidUriException(string s, Exception x) : base($"Invalid AWS S3 URI: {s}", x) { }
        }
        public static AmazonS3Uri ParseS3Uri(string s)
        {
            try
            {
                // this shitty method actually throws !!!
                if (!AmazonS3Uri.TryParseAmazonS3Uri(s, out var uri)) throw new InvalidUriException(s, null);
                return uri;
            }
            catch(Exception x)
            {
                throw new InvalidUriException(s, x);
            }
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
        public static async Task<Products> ReadProducts(S3Helper s3, AmazonS3Uri uri, string tempFilePath)
        {
            var products = new Products();
            await s3.DownloadAsync(uri.Bucket, uri.Key, DateTime.MinValue, async (contentType, stream) =>
            {
                switch (Path.GetExtension(uri.Key).ToLower())
                {
                    case ".msi":
                        products.Add(new ProductInfo { Name = uri.ToString(), AbsoluteUri = uri.ToString() });
                        await Task.CompletedTask;
                        break;
                    case ".ini":
                        products.AddRange(await Products.FromIni(stream, uri.ToString(), tempFilePath));
                        break;
                    case ".json":
                        products.AddRange(await Products.FromJson(stream));
                        break;
                    default:
                        throw new FormatException($"Unsupported file extension in {uri.ToString()}");
                }
            });
            return products;
        }
        public static async Task<Products> ReadProducts(S3Helper s3, IEnumerable<string> uris, string tempFilePath)
        {
            var products = new Products();
            var arrayOfProducts = await Task.WhenAll(
                uris.Aggregate(new List<Task<Products>>(),
                (tasks, configFileUri) =>
                {
                    tasks.Add(Task<Products>.Run(() =>
                    {
                        var uri = ParseS3Uri(configFileUri);
                        return ReadProducts(s3, uri, tempFilePath);
                    }));
                    return tasks;
                }));
            if(null != arrayOfProducts) products.AddRange(arrayOfProducts.SelectMany(x => x));
            return products;
            //return arrayOfProducts.Aggregate(new Products(), (p, pp) => { p.AddRange(pp); return p; });
        }

        public static async Task DownloadInstallers(IEnumerable<ProductInfo> products, S3Helper s3, string localPathBase)
        {
            await Task.WhenAll(
                products.Aggregate(new List<Task<HttpStatusCode>>(),
                    (tasks, product) =>
                    {
                        var uri = ParseS3Uri(product.AbsoluteUri);
                        product.LocalPath = product.MapToLocalPath(localPathBase);
                        Directory.CreateDirectory(Path.GetDirectoryName(product.LocalPath));
                        tasks.Add(s3.DownloadAsync(uri.Bucket, uri.Key, product.LocalPath));
                        return tasks;
                    }
                )
            );
        }
        /// <summary>
        /// Finds locally cached installer files which need to be uninstalled completely because they are no longer in the products
        /// </summary>
        /// <param name="rootFolderAndMask">Cache folder path and file mask, like C:\Cache\*.msi</param>
        /// <returns></returns>
        public IEnumerable<string> FindFilesToUninstall(string rootFolderAndMask)
        {
            var root = Path.GetDirectoryName(rootFolderAndMask);
            var mask = $"*{Path.GetExtension(rootFolderAndMask)}";
            var files = Directory.Exists(root) ? Directory.EnumerateFiles(root, mask, SearchOption.AllDirectories) : new List<string>();
            return FilesToUninstall(files.Select(s => Path.Combine(root, s)));
        }
        public static Func<string, string, bool> defaultPathCompare = (s1, s2) => { return 0 == string.Compare(s1, s2, true); };
        public IEnumerable<string> FilesToUninstall(IEnumerable<string> files, Func<string, string, bool> compare = null)
        {
            if (null == compare) compare = defaultPathCompare;
            return files.Where(e => !Exists(product => compare(product.LocalPath, e)));
        }
        public (IEnumerable<ProductInfo> filesToUninstall, IEnumerable<ProductInfo> productsToInstall) Separate(Func<string, ProductInfo> findInstalledProduct)
        {
            var uninstall = new List<ProductInfo>();
            var install = new List<ProductInfo>();
            foreach(var product in this) 
            {
                var installedProduct = findInstalledProduct(product.LocalPath);
                if (null == installedProduct)
                {
                    install.Add(product);
                    continue;
                }
                var installerAction = product.CompareAndSelectAction(installedProduct);
                switch (installerAction)
                {
                    case Installer.Action.Install:
                        install.Add(product);
                        break;
                    case Installer.Action.Reinstall:
                        uninstall.Add(installedProduct);
                        install.Add(product);
                        break;
                    case Installer.Action.Uninstall:
                        uninstall.Add(installedProduct);
                        break;
                }
            }
            return (uninstall, install);
        }
    }
}
