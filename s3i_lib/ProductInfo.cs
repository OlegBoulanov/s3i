using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace s3i_lib
{
    public class ProductInfo
    {
        public string Name { get; set; }
        public string AbsoluteUri { get; set; }
        [JsonIgnore]
        public string LocalPath { get; set; }
        public ProductProps Props { get; protected set; } = new ProductProps();
        public Installer.Action CompareAndSelectAction(ProductInfo installedProduct)
        {
            // use absolute uri to compare versions
            var versionIsNewer = AbsoluteUri.CompareTo(installedProduct.AbsoluteUri);
            // if new is greater, install
            if (0 < versionIsNewer) return Installer.Action.Install;
            // else (if less or props changed) reinstall
            if (versionIsNewer < 0 || !Props.Equals(installedProduct.Props)) return Installer.Action.Reinstall;
            // else (if same and no props changed) do nothing
            return Installer.Action.NoAction;
        }
        #region Json Serialization
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        public async Task ToJson(Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(ToJson());
            }
        }
        public static async Task<ProductInfo> FromJson(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return JsonConvert.DeserializeObject<ProductInfo>(await reader.ReadToEndAsync());
            }
        }
        public static string LocalInfoFileExtension { get; } = ".json";
        public async Task SaveToLocal()
        {
            var path = $"{Path.GetFileNameWithoutExtension(LocalPath)}{LocalInfoFileExtension}";
            using(var fs = new FileStream(path, FileMode.Create))
            {
                await ToJson(fs);
            }
        }
        public static async Task<ProductInfo> FromLocal(string localPath)
        {
            var path = $"{Path.GetFileNameWithoutExtension(localPath)}{LocalInfoFileExtension}";
            if (File.Exists(path))
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    return await FromJson(fs);
                }
            }
            return null;
        }
        #endregion
    }
}
