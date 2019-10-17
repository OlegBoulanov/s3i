using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using Newtonsoft.Json;

using Amazon.S3.Util;

namespace s3iLib
{
    public class ProductCollection : List<ProductInfo>
    {

        public static AmazonS3Uri ParseS3Uri(string s)
        {
            try
            {
                var uri = new Uri(s);
                // this shitty method may actually throw, so does TryParseAmazonS3Uri(string): https://github.com/aws/aws-sdk-net/issues/1426
                if (!AmazonS3Uri.TryParseAmazonS3Uri(uri, out var s3uri)) throw new InvalidUriException(s, null);
                return s3uri;
            }
            catch(Exception x)
            {
                throw new InvalidUriException(s, x);
            }
        }
        public static async Task<ProductCollection> FromJson(Stream stream)
        {
            using(var reader = new StreamReader(stream))
            {
                return JsonConvert.DeserializeObject<ProductCollection>(await reader.ReadToEndAsync().ConfigureAwait(false));
            }
        }
        const string sectionProducts = "$products$";
        public static async Task<ProductCollection> FromIni(Stream stream, string baseUri, string tempFilePath)
        {
            return await FromIni(stream, new AmazonS3Uri(baseUri), tempFilePath).ConfigureAwait(false);
        }
        public static async Task<ProductCollection> FromIni(Stream stream, Uri baseUri, string tempFilePath)
        {
            return await FromIni(stream, new AmazonS3Uri(baseUri), tempFilePath).ConfigureAwait(false);
        }
        public static async Task<ProductCollection> FromIni(Stream stream, AmazonS3Uri baseUri, string tempFilePath)
        {
            Contract.Requires(null != baseUri);
            var products = new ProductCollection();
            await IniReader.Read(stream, (sectionName, keyName, keyValue) =>
            {
                if (sectionProducts.Equals(sectionName, StringComparison.InvariantCultureIgnoreCase))
                {
                    products.Add(new ProductInfo { Name = keyName, AbsoluteUri = keyValue });
                }
                else
                {
                    var product = products.FirstOrDefault((p) => { return sectionName.Equals(p.Name, StringComparison.InvariantCulture); });
                    if(default(ProductInfo) != product) product.Props.Add(keyName, keyValue);
                }
                //await Task.CompletedTask;
            }).ConfigureAwait(false);
            products.ForEach((p) =>
            {
                p.AbsoluteUri = p.AbsoluteUri.RebaseUri(baseUri.ToString());
                p.LocalPath = p.MapToLocalPath(tempFilePath);
            });
            return products;
        }
        public static async Task<ProductCollection> ReadProducts(S3Helper s3, AmazonS3Uri uri, string tempFilePath)
        {
            Contract.Requires(null != s3);
            Contract.Requires(null != uri);
            var products = new ProductCollection();
            await s3.DownloadAsync(uri.Bucket, uri.Key, DateTime.MinValue, async (contentType, stream) =>
            {
                  switch (Path.GetExtension(uri.Key).ToUpperInvariant())
                  {
                      case ".MSI":
                          products.Add(new ProductInfo { Name = uri.ToString(), AbsoluteUri = uri.ToString() });
                          await Task.CompletedTask.ConfigureAwait(false);
                          break;
                      case ".INI":
                          products.AddRange(await ProductCollection.FromIni(stream, uri, tempFilePath).ConfigureAwait(false));
                          break;
                      case ".JSON":
                          products.AddRange(await ProductCollection.FromJson(stream).ConfigureAwait(false));
                          break;
                      default:
                          throw new FormatException($"Unsupported file extension in {uri.ToString()}");
                  }
              }).ConfigureAwait(false);
            return products;
        }
        public static async Task<ProductCollection> ReadProducts(S3Helper s3, IEnumerable<string> uris, string tempFilePath)
        {
            var products = new ProductCollection();
            var arrayOfProducts = await Task.WhenAll(
                uris.Aggregate(new List<Task<ProductCollection>>(),
                (tasks, configFileUri) =>
                {
                    tasks.Add(Task<ProductCollection>.Run(() =>
                    {
                        var uri = ParseS3Uri(configFileUri);
                        return ReadProducts(s3, uri, tempFilePath);
                    }));
                    return tasks;
                })).ConfigureAwait(false);
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
            ).ConfigureAwait(false);
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
        public IEnumerable<string> FilesToUninstall(IEnumerable<string> files, Func<string, string, bool> compare = null)
        {
            if (null == compare) compare = (s1, s2) => { return 0 == string.Compare(s1, s2, StringComparison.CurrentCultureIgnoreCase); };
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
