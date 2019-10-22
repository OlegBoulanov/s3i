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
        public Installer.Action CompareAndSelectAction(ProductInfo installedProduct)
        {
            if(null == installedProduct) return Installer.Action.Install;
            // use absolute uri to compare versions
            var thisVersion = SemanticVersion.From(Uri);
            var installedVersion = SemanticVersion.From(installedProduct.Uri);
            var versionIsNewer = thisVersion.CompareTo(installedVersion);
            // if new is greater, install
            if (0 < versionIsNewer) return Installer.Action.Install;
            // else (if less or props changed) reinstall
            if (versionIsNewer < 0 || !Props.Equals(installedProduct.Props)) return Installer.Action.Reinstall;
            // else (if same and no props changed) run anyway - won't do any harm
            return Installer.Action.Install;
        }
        #region Json Serialization
        public string ToJson()
        {
            return JsonSerializer.Serialize<ProductInfo>(this, new JsonSerializerOptions { WriteIndented = true } );
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
