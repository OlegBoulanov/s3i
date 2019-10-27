using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

//using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace s3iLib
{
    public enum InstallAction { NoAction, Install, Reinstall, Upgrade, Downgrade, Uninstall };

    public class ProductInfo
    {
        public string Name { get; set; }
        public Uri Uri { get; set; }
        public string LocalPath { get; set; }
#pragma warning disable CA2227 // we need write access to deserialize
        public ProductPropertiesDictionary Props { get; set; } = new ProductPropertiesDictionary();
#pragma warning restore CA2227
        [JsonIgnore]
        public DateTimeOffset LastModified { get; set; }
        public string MapToLocalPath(string basePath)
        {
            return MapToLocalPath(basePath, Name, Path.GetFileName(Uri.AbsolutePath));
        }
        public static string MapToLocalPath(string basePath, string productName, string fileName)
        {
            return $"{Path.Combine(Path.Combine(basePath, productName), fileName)}";
        }
        public InstallAction CompareAndSelectAction(ProductInfo installedProduct, params string[] prefixes)
        {
            if (null == installedProduct) return InstallAction.Install;
            var versionIsNewer = Uri.CompareSemanticVersion(installedProduct.Uri, prefixes);
            // if new is greater, install
            if (0 < versionIsNewer) return InstallAction.Upgrade;
            // else (if less or props changed) reinstall
            if (versionIsNewer < 0) return InstallAction.Downgrade;
            // same version, changed props means reinstall
            if (!Props.Equals(installedProduct.Props)) return InstallAction.Reinstall;
            //// compare modification times then and reinstall, if upstream has been updated since last installation
            //if (DateTimeOffset.MinValue < installedProduct.LastModified && installedProduct.LastModified < LastModified) return InstallAction.Reinstall;
            // else (if same version, datetime, and no props changed) 
            return InstallAction.NoAction;
        }
        #region Json Serialization
        public string ToJson()
        {
            return JsonSerializer.Serialize<ProductInfo>(this, new JsonSerializerOptions { WriteIndented = true });
        }
        public async Task ToJson(Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(ToJson()).ConfigureAwait(false);
            }
        }
        public static async Task<ProductInfo> FromJson(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return JsonSerializer.Deserialize<ProductInfo>(await reader.ReadToEndAsync().ConfigureAwait(false));
            }
        }
        public static string LocalInfoFileExtension { get; } = ".json";
        public async Task SaveToLocal(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                await ToJson(fs).ConfigureAwait(false);
            }
            File.SetLastWriteTimeUtc(path, LastModified.UtcDateTime);
        }
        public static async Task<ProductInfo> FindInstalled(string path)
        {
            if (File.Exists(path))
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    return await FromJson(fs).ConfigureAwait(false);
                }
            }
            return null;
        }
        #endregion
        public override string ToString()
        {
            return $"{Name}: {Uri}";
        }
    }
}
