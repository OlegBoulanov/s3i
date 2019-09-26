using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

namespace s3i_lib
{
    public class ProductInfo
    {
        public string Name { get; set; }
        public string RelativeUri { get; set; }
        public string AbsoluteUri { get; set; }
        public string LocalPath { get; set; }
        public ProductProps Props { get; protected set; } = new ProductProps();
        public Installer.Action? CompareAndSelectAction(ProductInfo installedProduct)
        {
            // use absolute uri to compare versions
            var versionIsNewer = AbsoluteUri.CompareTo(installedProduct.AbsoluteUri);   // TODO: need more intelligence here...
            // if new is greater, install
            if (0 < versionIsNewer) return Installer.Action.Install;
            // else (if less or props changed) reinstall
            if (versionIsNewer < 0 || !Props.Equals(installedProduct.Props)) return Installer.Action.Reinstall;
            // else (if same and no props changed) do nothing
            return null;
        }
    }
}
