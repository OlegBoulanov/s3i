using System;
using System.IO;
using System.Text;
using NUnit.Framework;

using s3iLib;

namespace s3iLibTests
{
    
    public class ProductInfoTest
    {
        [Test]
        public void Serialization()
        {
            var pi = new ProductInfo { Uri = new Uri("https://s3.something.com/install-me/me-me-me.msi"), Name = "TestProduct",  };
            pi.Props.Add("prop1", "value1");
            pi.Props.Add("prop2", "value2");
            var json = pi.ToJson();
            Assert.AreEqual(
                "{\r\n"
                + "  \"Name\": \"TestProduct\",\r\n"
                + "  \"Uri\": \"https://s3.something.com/install-me/me-me-me.msi\",\r\n"
                + "  \"LocalPath\": null,\r\n"
                + "  \"Props\": {\r\n"
                + "    \"prop1\": \"value1\",\r\n"
                + "    \"prop2\": \"value2\"\r\n"
                + "  }\r\n"
                + "}", 
                json);
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var p = ProductInfo.FromJson(ms).Result;
            Assert.AreEqual("TestProduct", p.Name);
            Assert.AreEqual("https://s3.something.com/install-me/me-me-me.msi", p.Uri.ToString());
            Assert.AreEqual(2, p.Props.Count);
            Assert.AreEqual("value1", p.Props["prop1"]);
            Assert.AreEqual("value2", p.Props["prop2"]);
        }

    }
}
