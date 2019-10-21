using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
//using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Amazon.S3.Util;

namespace s3iLib
{
    public class ProductCollection : List<ProductInfo>
    {
        public static async Task<ProductCollection> FromJson(Stream stream)
        {
            using(var reader = new StreamReader(stream))
            {
                return JsonSerializer.Deserialize<ProductCollection>(await reader.ReadToEndAsync().ConfigureAwait(false));
            }
        }
        static string MapToLocal(Uri uri, string localBasePath)
        {
            return Path.Combine(localBasePath, $"{uri.Host}{Path.DirectorySeparatorChar}{uri.AbsolutePath.RemoveDotSegments()}").UnifySlashes();
        }
        public ProductCollection MapToLocal(string localBasePath)
        {
            ForEach(p =>
            {
                if(string.IsNullOrWhiteSpace(p.LocalPath)) {
                    var uri = p.Uri;
                    p.LocalPath = MapToLocal(uri, localBasePath);
                }
            });
            return this;
        }
        const string sectionProducts = "$products$";
        public static async Task<ProductCollection> FromIni(Stream stream, Uri baseUri = null)
        {
            var products = new ProductCollection();
            await IniReader.Read(stream, (sectionName, keyName, keyValue) =>
            {
                if (sectionProducts.Equals(sectionName, StringComparison.InvariantCultureIgnoreCase))
                {
                    var nextUri = null != baseUri ? baseUri.BuildRelativeUri(keyValue) : new Uri(keyValue);
                    products.Add(new ProductInfo { Name = keyName, Uri = nextUri });
                    baseUri = nextUri;
                }
                else
                {
                    var product = products.FirstOrDefault((p) => { return sectionName.Equals(p.Name, StringComparison.InvariantCulture); });
                    if(default(ProductInfo) != product) product.Props.Add(keyName, keyValue);
                }
                //await Task.CompletedTask;
            }).ConfigureAwait(false);
            return products;
        }
        public static async Task<ProductCollection> ReadProducts(Downloader downloader, Uri uri)
        {
            Contract.Requires(null != downloader);
            Contract.Requires(null != uri);
            var products = new ProductCollection();
            var statusCode = await downloader.DownloadAsync(uri, DateTime.MinValue, async (stream) =>
            {
                  switch (Path.GetExtension(uri.AbsolutePath).ToUpperInvariant())
                  {
                      case ".MSI":
                          products.Add(new ProductInfo { Name = uri.ToString(), Uri = uri });
                          await Task.CompletedTask.ConfigureAwait(false);
                          break;
                      case ".INI":
                          products.AddRange(await ProductCollection.FromIni(stream, uri).ConfigureAwait(false));
                          break;
                      case ".JSON":
                          products.AddRange(await ProductCollection.FromJson(stream).ConfigureAwait(false));
                          break;
                      default:
                          throw new FormatException($"Unsupported file extension in {uri.ToString()}");
                  }
              }).ConfigureAwait(false);
            switch(statusCode)
            {
                case HttpStatusCode.OK:
                case HttpStatusCode.NotModified:
                    break;
                default:
                    throw new ApplicationException($"Can't download {uri}, status: {statusCode}");
            }
            return products;
        }
        public static async Task<ProductCollection> ReadProducts(IEnumerable<Uri> uris)
        {
            var products = new ProductCollection();
            var arrayOfProducts = await Task.WhenAll(
                uris.Aggregate(new List<Task<ProductCollection>>(),
                (tasks, configFileUri) =>
                {
                    tasks.Add(Task<ProductCollection>.Run(() =>
                    {
                        return ReadProducts(Downloader.Select(configFileUri), configFileUri);
                    }));
                    return tasks;
                })).ConfigureAwait(false);
            if(null != arrayOfProducts) products.AddRange(arrayOfProducts.SelectMany(x => x));
            return products;
            //return arrayOfProducts.Aggregate(new Products(), (p, pp) => { p.AddRange(pp); return p; });
        }

        public static async Task DownloadInstallers(IEnumerable<ProductInfo> products, string localPathBase)
        {
            await Task.WhenAll(
                products.Aggregate(new List<Task<HttpStatusCode>>(),
                    (tasks, product) =>
                    {
                        product.LocalPath = product.MapToLocalPath(localPathBase);
                        Directory.CreateDirectory(Path.GetDirectoryName(product.LocalPath));
                        tasks.Add(Downloader.Select(product.Uri).DownloadAsync(product.Uri, product.LocalPath));
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
                var installedProduct = findInstalledProduct?.Invoke(product.LocalPath);
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
