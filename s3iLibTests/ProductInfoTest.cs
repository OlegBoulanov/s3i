using System;
using System.IO;
using System.Text;
using NUnit.Framework;

using s3iLib;

namespace s3iLibTests
{

    public class ProductInfoTest
    {
        readonly string productJson = "{" + Environment.NewLine
                + "  \"Name\": \"TestProduct\"," + Environment.NewLine
                + "  \"Uri\": \"https://s3.something.com/install-me/me-me-me.msi\"," + Environment.NewLine
                + "  \"LocalPath\": null," + Environment.NewLine
                + "  \"Props\": {" + Environment.NewLine
                + "    \"prop1\": \"value1\"," + Environment.NewLine
                + "    \"prop2\": \"value2\"" + Environment.NewLine
                + "  }" + Environment.NewLine
//                + "  \"LastModified\": \"2019-10-23T09:15:23.1234+04:00\"" + Environment.NewLine
                + "}";
        ProductInfo FromJson(string json = null)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json ?? productJson)))
            {
                return ProductInfo.FromJson(ms).Result;
            }
        }

        [Test]
        public void Serialization()
        {
            var pi = new ProductInfo
            {
                Uri = new Uri("https://s3.something.com/install-me/me-me-me.msi"),
                Name = "TestProduct",
#pragma warning disable CA1305
//                LastModified = DateTimeOffset.Parse("2019-10-23 09:15:23.1234+04:00"),
#pragma warning restore CA1305
            };
            pi.Props.Add("prop1", "value1");
            pi.Props.Add("prop2", "value2");
            var json = pi.ToJson();
            Assert.AreEqual(productJson, json);
            var p = FromJson(json);
            var json2 = p.ToJson();
            Assert.AreEqual(productJson, json2);
            Assert.AreEqual("TestProduct", p.Name);
            Assert.AreEqual("https://s3.something.com/install-me/me-me-me.msi", p.Uri.ToString());
            Assert.AreEqual(2, p.Props.Count);
            Assert.AreEqual("value1", p.Props["prop1"]);
            Assert.AreEqual("value2", p.Props["prop2"]);
        }

        [Test]
        public void SelectAction()
        {
            var downloaded = FromJson();
            var installed = FromJson();
            Assert.AreEqual(InstallAction.Install, downloaded.CompareAndSelectAction(null));
            Assert.AreEqual(InstallAction.NoAction, downloaded.CompareAndSelectAction(installed));
            //
            downloaded.Props.Add("prop3", "xxx");
            Assert.AreEqual(InstallAction.Reinstall, downloaded.CompareAndSelectAction(installed));
            //
            downloaded = FromJson();
            installed = FromJson();
            downloaded.Uri = new Uri("https://x.com/prod/1.2.3/p.msi");
            installed.Uri = new Uri("https://x.com/prod/1.2.3/p.msi");
            Assert.AreEqual(InstallAction.NoAction, downloaded.CompareAndSelectAction(installed));
            downloaded.Uri = new Uri("https://x.com/prod/1.2.4/p.msi");
            Assert.AreEqual(InstallAction.Upgrade, downloaded.CompareAndSelectAction(installed));
            downloaded.Uri = new Uri("https://x.com/prod/1.2.1/p.msi");
            Assert.AreEqual(InstallAction.Downgrade, downloaded.CompareAndSelectAction(installed));
            // 
            downloaded = FromJson();
            installed = FromJson();
            //installed.LastModified = downloaded.LastModified.AddMinutes(10);
            //Assert.AreEqual(InstallAction.NoAction, downloaded.CompareAndSelectAction(installed));
            //installed.LastModified = downloaded.LastModified.AddMinutes(-20);
            //Assert.AreEqual(InstallAction.Reinstall, downloaded.CompareAndSelectAction(installed));
        }
    }
}
