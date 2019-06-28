using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;
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
        static string testConfig = @"[$Products$]

ProductOne = https://xxx.s3.amazonaws.com/Test/Windows10/Distrib/ProductOne/12.6.16/ProductOne.msi
SecondProduct    = ../../../Distrib/SecondProduct/9.4.188/SecondProduct.msi
           
 [ProductOne]
 CONFIG_ROOT = https://s3.amazonaws.com/config.company.com/Engineering/ESR/
 ROLLING_LOG		= https://s3.amazonaws.com/logs.company.com/{0:yyyy}/{0:MM}/{0:dd}/{1}/{0:yyMMdd_HH}{3:00}.log
           
 [SecondProduct]
            
 ; required parameters
            
 UNC_PROMPTS			= https://s3.amazonaws.com/prompts.company.com/
 UNC_SESSIONS		= https://s3.amazonaws.com/sessions.company.com/
            
 ; optional config
            
 PROMPTS_UPDATE	= https://s3.amazonaws.com/prompts.update.company.com/
 SYNC            = 00:15:00 1000 20000000 00:01:00
";
        [TestMethod]
        public async Task TestTwoProductProps()
        {
            var products = await Products.FromIni(new MemoryStream(Encoding.ASCII.GetBytes(testConfig)), "https://xxx.s3.amazonaws.com/Test/Windows10", "C:\\Temp\\");
            Assert.AreEqual(2, products.Count);
            Assert.AreEqual("ProductOne", products[0].Name);
            Assert.AreEqual("SecondProduct", products[1].Name);
            Assert.AreEqual("https://xxx.s3.amazonaws.com/Test/Windows10/Distrib/ProductOne/12.6.16/ProductOne.msi", products[0].RelativeUri);
            Assert.AreEqual(2, products[0].Props.Count);
            Assert.AreEqual("https://s3.amazonaws.com/config.company.com/Engineering/ESR/", products[0].Props["CONFIG_ROOT"]);
            Assert.AreEqual("https://s3.amazonaws.com/logs.company.com/{0:yyyy}/{0:MM}/{0:dd}/{1}/{0:yyMMdd_HH}{3:00}.log", products[0].Props["ROLLING_LOG"]);
            Assert.AreEqual(4, products[1].Props.Count);
            Assert.AreEqual("https://s3.amazonaws.com/prompts.company.com/", products[1].Props["UNC_PROMPTS"]);
            Assert.AreEqual("https://s3.amazonaws.com/sessions.company.com/", products[1].Props["UNC_SESSIONS"]);
            Assert.AreEqual("https://s3.amazonaws.com/prompts.update.company.com/", products[1].Props["PROMPTS_UPDATE"]);
            Assert.AreEqual("00:15:00 1000 20000000 00:01:00", products[1].Props["SYNC"]);
        }

        static string manyProducts = @"[$Products$]
One = https://xxx.s3.amazonaws.com/Test/Windows10/Distrib/ProductOne/12.6.16/ProductOne.msi
Two    = ../../Distrib/SecondProduct/9.4.188/SecondProduct.msi
Three    = https://xxx.s3.amazonaws.com/Test/Windows10/Distrib//SecondProduct/9.4.188/SecondProduct.msi
";           
        [TestMethod]
        public async Task TestDownloadPaths()
        {
            var products = await Products.FromIni(new MemoryStream(Encoding.ASCII.GetBytes(manyProducts)), "https://xxx.s3.amazonaws.com/Test/Windows10/Config/OneInstance/config.ini", "C:\\Temp\\");
            var files = from p in products select new { product = p, local = p.AbsoluteUri.MapToLocalPath("c:/Temp/")  };
            Assert.AreEqual(3, products.Count);
            Assert.AreEqual(3, files.Count());
            Assert.AreEqual("c:/Temp/xxx.s3.amazonaws.com/Test/Windows10/Distrib/ProductOne/12.6.16/ProductOne.msi", files.First().local);
        }


        [TestMethod]
        public async Task ReadTwoFiles()
        {
            var s3 = new S3Helper("s3i");
            var maxAttempts = 1;// 3000;
            for (var i = 0; i < maxAttempts; i++) {
                var clock = System.Diagnostics.Stopwatch.StartNew();
                var prods = await Products.ReadProducts(s3,
                    new List<string> {
                    "https://install.elizacorp.com.s3.amazonaws.com/Test/Windows10/Config/s3i/1/Products.ini",
                    "https://install.elizacorp.com.s3.amazonaws.com/Test/Windows10/Config/s3i/2/Products.ini",
                    },
                    "D:/Temp/");
                var ms = clock.ElapsedMilliseconds;
                if (100 < ms)
                {
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [{i:0000}] {clock.Elapsed:mm\\:ss\\.fff} {new string('*', (int)(ms/100))}");
                }
                Assert.AreEqual(3, prods.Count);
            }
        }
    }
}
