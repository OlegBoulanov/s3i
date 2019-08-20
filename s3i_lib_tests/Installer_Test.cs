using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using s3i_lib;

namespace s3i_lib_tests
{
    [TestClass]
    public class Installer_Test
    {
        [TestMethod]
        public void FormatInstallerCommandFromProductInfo()
        {
            var product = new ProductInfo { Name = "TestProduct", LocalPath = "my temp\\product.msi" };
            product.Props.Add("Prop1", "Value1");
            product.Props.Add("Prop2", "Value 2");
            var installer = new Installer(product);
            Assert.AreEqual("msiKeys \"my temp\\product.msi\" Prop1=Value1 Prop2=\"Value 2\" msiArgs", installer.FormatCommand("msiKeys", "msiArgs"));
        }
    }
}
