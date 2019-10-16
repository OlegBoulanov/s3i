using System;
using NUnit.Framework;

using s3iLib;

namespace s3iLibTests
{
    
    public class Installer_Test
    {
        [Test]
        public void FormatInstallerCommandFromProductInfo()
        {
            var product = new ProductInfo { Name = "TestProduct", LocalPath = "my temp\\product.msi" };
            product.Props.Add("Prop1", "Value1");
            product.Props.Add("Prop2", "Value 2");
            Assert.AreEqual("msiKeys \"my temp\\product.msi\" Prop1=Value1 Prop2=\"Value 2\" msiArgs", Installer.FormatCommand(product.LocalPath, product.Props, "msiKeys", "msiArgs"));
        }
    }
}
