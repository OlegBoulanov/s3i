using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
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
        public static async Task<ProductCollection> FromJson(Stream stream, DateTimeOffset lastModified)
        {
            using var reader = new StreamReader(stream);
            var products = JsonSerializer.Deserialize<ProductCollection>(await reader.ReadToEndAsync().ConfigureAwait(false));
            products.ForEach(p => p.LastModifiedUtc = lastModified.UtcDateTime);
            return products;
        }
        public ProductCollection MapToLocal(string localBasePath)
        {
            ForEach(p =>
            {
                if (string.IsNullOrWhiteSpace(p.LocalPath))
                {
                    p.LocalPath = Path.Combine(localBasePath, $"{Uri.EscapeDataString(p.Name)}{Path.DirectorySeparatorChar}{Path.GetFileName(p.Uri.AbsolutePath)}").UnifySlashes();
                }
            });
            return this;
        }
        const string sectionDefine = "$define$";
        const string sectionProducts = "$products$";
        public static async Task<ProductCollection> FromIni(Stream stream, DateTimeOffset lastModified, Uri baseUri = null, Variables vars = null)
        {
            var defines = new Dictionary<string, string>();
            var products = new ProductCollection();
            await IniReader.Read(stream, (sectionName, keyName, keyValue) =>
            {
                keyValue = vars.Expand(keyValue.Expand(defines));
                if (sectionProducts.Equals(sectionName, StringComparison.InvariantCultureIgnoreCase))
                {
                    var nextUri = null != baseUri ? baseUri.BuildRelativeUri(keyValue) : new Uri(keyValue);
                    products.Add(new ProductInfo { Name = keyName, Uri = nextUri, LastModifiedUtc = lastModified });
                    baseUri = nextUri;
                }
                else if (sectionDefine.Equals(sectionName, StringComparison.InvariantCultureIgnoreCase))
                {
                    defines[keyName] = keyValue;
                }
                else
                {
                    var product = products.FirstOrDefault((p) => { return sectionName.Equals(p.Name, StringComparison.InvariantCulture); });
                    if (default(ProductInfo) != product) product.Props.Add(keyName, keyValue);
                }
                //await Task.CompletedTask;
            }).ConfigureAwait(false);
            return products;
        }
        public static async Task<ProductCollection> ReadProducts(Downloader downloader, Uri uri, Variables vars = null)
        {
            Contract.Requires(null != downloader);
            Contract.Requires(null != uri);
            var products = new ProductCollection();
            var statusCode = await downloader.DownloadAsync(uri, DateTime.MinValue, async (stream, lastModified) =>
            {
                switch (Path.GetExtension(uri.AbsolutePath).ToUpperInvariant())
                {
                    case ".MSI":
                        products.Add(new ProductInfo { Name = uri.ToString(), Uri = uri, LastModifiedUtc = lastModified });
                        await Task.CompletedTask.ConfigureAwait(false);
                        break;
                    case ".INI":
                        products.AddRange(await ProductCollection.FromIni(stream, lastModified, uri, vars).ConfigureAwait(false));
                        break;
                    case ".JSON":
                        products.AddRange(await ProductCollection.FromJson(stream, lastModified).ConfigureAwait(false));
                        break;
                    default:
                        throw new FormatException($"Unsupported file extension in {uri.ToString()}");
                }
            }).ConfigureAwait(false);
            switch (statusCode)
            {
                case HttpStatusCode.OK:
                case HttpStatusCode.NotModified:
                    break;
                default:
                    throw new ApplicationException($"Can't download {uri}, status: {statusCode}");
            }
            return products;
        }
        public static async Task<ProductCollection> ReadProducts(IEnumerable<Uri> uris, Variables vars = null)
        {
            var products = new ProductCollection();
            var arrayOfProducts = await Task.WhenAll(
                uris.Aggregate(new List<Task<ProductCollection>>(),
                (tasks, configFileUri) =>
                {
                    tasks.Add(Task<ProductCollection>.Run(() =>
                    {
                        return ReadProducts(Downloader.Select(configFileUri), configFileUri, vars);
                    }));
                    return tasks;
                })).ConfigureAwait(false);
            if (null != arrayOfProducts) products.AddRange(arrayOfProducts.SelectMany(x => x));
            return products;
            //return arrayOfProducts.Aggregate(new Products(), (p, pp) => { p.AddRange(pp); return p; });
        }

        public static async Task<ConcurrentDictionary<Uri, HttpStatusCode>> DownloadInstallers(IEnumerable<ProductInfo> products)
        {
            var results = new ConcurrentDictionary<Uri, HttpStatusCode>();
            await Task.WhenAll(
                products.Aggregate(new List<Task>(),
                    (tasks, product) =>
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(product.LocalPath));
                        tasks.Add(Downloader.Select(product.Uri)
                            .DownloadAsync(product.Uri, product.LocalPath)
                            .ContinueWith(t => { results.TryAdd(product.Uri, t.Result); }, TaskScheduler.Default)
                            );
                        return tasks;
                    }
                )
            ).ConfigureAwait(false);
            return results;
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
        public (IEnumerable<ProductInfo> filesToUninstall, IEnumerable<ProductInfo> productsToInstall) PrepareActions(Func<string, ProductInfo> findInstalledProduct, params string[] prefixes)
        {
            var uninstall = new List<ProductInfo>();
            var install = new List<ProductInfo>();
            foreach (var product in this)
            {
                var installedProduct = findInstalledProduct?.Invoke(product.LocalPath);
                var installAction = product.CompareAndSelectAction(installedProduct, prefixes);
                // map action to msiexec type action sequence
                switch (installAction)
                {
                    case InstallAction.NoAction:
                        // still install over - shouldn't do any harm
                        install.Add(product);
                        break;
                    case InstallAction.Install:
                    case InstallAction.Upgrade:
                        install.Add(product);
                        break;
                    case InstallAction.Reinstall:
                    case InstallAction.Downgrade:
                        uninstall.Add(installedProduct);
                        install.Add(product);
                        break;
                    case InstallAction.Uninstall:
                        uninstall.Add(installedProduct);
                        break;
                }
            }
            return (uninstall, install);
        }
        public (IEnumerable<string> filesToRemove, IEnumerable<ProductInfo> filesToUninstall, IEnumerable<ProductInfo> productsToInstall)
        Preprocess(Func<string, ProductInfo> findInstalledProduct, params string[] prefixes)
        {
            var filesToRemove = new List<string>();
            (var filesToUninstall, var productsToInstall) = PrepareActions(findInstalledProduct, prefixes);
            return (filesToRemove, filesToUninstall, productsToInstall);
        }
    }
}
