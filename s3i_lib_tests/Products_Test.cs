using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using s3i_lib;

namespace s3i_lib_tests
{
    [TestClass]
    public class Products_Test
    {
        [TestMethod]
        public async Task FromIni()
        {
            var products = await Products.FromIni(
                new MemoryStream(Encoding.ASCII.GetBytes("\n" +
                "[$Products$]\n" +
                "Something = at this path; comment\n" +
                "; comment this completely\n" +
                "[Something]\n" +
                "  prop1 =  value11 ; comment\n" +
                "prop2 = value22")));
            Assert.AreEqual(1, products.Count);
            Assert.AreEqual("Something", products[0].Name);
            Assert.AreEqual("at this path", products[0].Path);
            Assert.AreEqual(2, products[0].Props.Count);
            Assert.AreEqual("value11", products[0].Props["prop1"]);
        }
    }
}
