using System;
using NUnit.Framework;

using s3iLib;

namespace s3iLibTests
{
    
    public class ProductInfo_Test
    {
        [Test]
        public void Compare()
        {
        }
        [Test]
        public void Serialization()
        {
            var pi = new ProductInfo { AbsoluteUri = "https://s3.something.com/install-me/me-me-me.msi", Name = "TestProduct",  };
            pi.Props.Add("prop1", "value1");
            pi.Props.Add("prop2", "value2");
            var json = pi.ToJson().Replace("\r", "");
            Assert.AreEqual(
                "{\n"
                + "  \"Name\": \"TestProduct\",\n"
                + "  \"AbsoluteUri\": \"https://s3.something.com/install-me/me-me-me.msi\",\n"
                + "  \"LocalPath\": null,\n"
                + "  \"Props\": {\n"
                + "    \"prop1\": \"value1\",\n"
                + "    \"prop2\": \"value2\"\n"
                + "  }\n"
                + "}", 
                json);
        }
    }
}
