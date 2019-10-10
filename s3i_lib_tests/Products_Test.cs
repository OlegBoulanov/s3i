﻿using System;
using NUnit.Framework;

using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using s3i_lib;
using System.Reflection;

namespace s3i_lib_tests
{
    
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
        [Test]
        public async Task TestTwoProductProps()
        {
            var products = await Products.FromIni(new MemoryStream(Encoding.ASCII.GetBytes(testConfig)), "https://xxx.s3.amazonaws.com/Test/Windows10", "C:\\Temp\\");
            Assert.AreEqual(2, products.Count);
            Assert.AreEqual("ProductOne", products[0].Name);
            Assert.AreEqual("SecondProduct", products[1].Name);
            Assert.AreEqual("https://xxx.s3.amazonaws.com/Test/Windows10/Distrib/ProductOne/12.6.16/ProductOne.msi", products[0].AbsoluteUri);
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
        [Test]
        public async Task TestDownloadPaths()
        {
            var products = await Products.FromIni(new MemoryStream(Encoding.ASCII.GetBytes(manyProducts)), "https://xxx.s3.amazonaws.com/Test/Windows10/Config/OneInstance/config.ini", "C:\\Temp\\");
            var files = from p in products select new { product = p, local = p.AbsoluteUri.MapToLocalPath("c:/Temp/")  };
            Assert.AreEqual(3, products.Count);
            Assert.AreEqual(3, files.Count());
            var x = Path.DirectorySeparatorChar;
            Assert.AreEqual($"c:{x}Temp{x}xxx.s3.amazonaws.com{x}Test{x}Windows10{x}Distrib{x}ProductOne{x}12.6.16{x}ProductOne.msi", files.First().local);
        }

        static string products1 = "[$products$]\nOne=https://x.amazonaws.com/one/config.ini\nTwo=https://x.amazonaws.com/one/config.ini\n[One]p11=1\np12=2\n[Two]\np21=11\np22=12\n";
        static string products2 = "[$products$]\nOne=https://x.amazonaws.com/one/config.ini\nTwo=https://x.amazonaws.com/one/config.ini\n[One]p11=1\np12=2\n[Two]\np21=11\np22=12\n";
        static string products3 = "[$products$]\nOne=https://x.amazonaws.com/one/config.ini\nTwo=https://x.amazonaws.com/one/config.ini\n[One]p11=1\np12=2\n[Two]\np21=11\np22=12\n";

        [Test]
        public async Task TestDiff()
        {
            var baseUri = "https://mecompany.s3.amazonaws.com/something/config.ini";
            var temp = "C:\\Temp\\";
            var p1 = await Products.FromIni(new MemoryStream(Encoding.ASCII.GetBytes(products1)), baseUri, temp);
            var p2 = await Products.FromIni(new MemoryStream(Encoding.ASCII.GetBytes(products2)), baseUri, temp);
            var p3 = await Products.FromIni(new MemoryStream(Encoding.ASCII.GetBytes(products3)), baseUri, temp);

        }

        [Test]
        public void ProductsToUninstall()
        {
            var tempFilePath = "X:\\temp\\";
            var products = new Products();
            products.AddRange(new List<string> {
                "https://download/from/here/p1.msi",
                "https://download/from/here/p2.msi",
                "https://download/from/there/p3.msi",
                "https://download/from/somewhere/p4.msi",
            }.Aggregate(new Products(), (ps, s) =>
            {
                ps.Add(new ProductInfo { Name = Path.GetFileNameWithoutExtension(s), AbsoluteUri = s, LocalPath = s.MapToLocalPath(tempFilePath) }); return ps;
            }));
            Assert.That(products.Count, Is.EqualTo(4));
            var existing = new List<string> {
                products[0].LocalPath,
                $"{tempFilePath}CrazyVirus.msi",
                products[3].LocalPath,
                $"{tempFilePath}SomethingElse.msi",
            };
            var uninstall = products.FilesToUninstall(existing).ToList();
            Assert.That(uninstall.Count(), Is.EqualTo(2));
            Assert.That(uninstall[0], Is.EqualTo(existing[1]));
            Assert.That(uninstall[1], Is.EqualTo(existing[3]));
        }

        [Test]
        [Category("Study")]
        public void TestEnumeration()
        {
            var tempFilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dlls = Directory.EnumerateFileSystemEntries(tempFilePath, "AWSSDK*.dll", SearchOption.AllDirectories).Select(s => Path.Combine(tempFilePath, s)).ToList();
            Assert.That(dlls.Count(), Is.EqualTo(2));
            Assert.That(dlls.Contains($"{Path.Combine(tempFilePath, "AWSSDK.Core.dll")}"));
            Assert.That(dlls.Contains($"{Path.Combine(tempFilePath, "AWSSDK.S3.dll")}"));
        }

        [Test]
        public void Separate()
        {
            var tempFilePath = "X:\\temp\\";
            //Assert.That(new SemanticVersion_Test())
            var installed = new Dictionary<string, string> {
                { $"{tempFilePath}\\Prod01\\p1.msi", "https://download/here/Prod01/9.4.7/p1.msi" },
                { $"{tempFilePath}\\Prod02\\p2.msi", "https://download/here/Prod02/3.3.5/p2.msi" },
                { $"{tempFilePath}\\Prod03\\p3.msi", "https://download/there/Prod03/12.5.8/p3.msi" },
                { $"{tempFilePath}\\ProdXX\\px.msi", "https://download/there/ProdXX/1.2.5.8+uninstall/px.msi" },
            };
            var products = new Products { 
                new ProductInfo { Name = "Prod01", AbsoluteUri = "https://download/here/Prod01/9.4.8+upgrade/p1.msi", LocalPath = $"{tempFilePath}\\Prod01\\p1.msi", },
                new ProductInfo { Name = "Prod02", AbsoluteUri = "https://download/here/Prod02/3.3.5+keep/p2.msi", LocalPath = $"{tempFilePath}\\Prod02\\p2.msi", },
                new ProductInfo { Name = "Prod03", AbsoluteUri = "https://download/there/Prod03/12.5.4+downgrade/p3.msi", LocalPath = $"{tempFilePath}\\Prod03\\p3.msi", },
                new ProductInfo { Name = "Prod04", AbsoluteUri = "https://download/from/Prod04/1.2.3+install/p4.msi", LocalPath = $"{tempFilePath}\\Prod04\\p4.msi", },
            };
            //var ppp = products[0].MapToLocalPath(tempFilePath);
            //Assert.That(products.Count, Is.EqualTo(4));
            var (uninstall, install) = products.Separate(installed.Values, localPath =>
            {
                var uri = installed.ContainsKey(localPath) ? installed[localPath] : null;
                return new ProductInfo { AbsoluteUri = uri, };
            });
            Console.WriteLine($"Uninstall:");
            foreach(var u in uninstall) Console.WriteLine($"  {u}");
            Console.WriteLine($"Install:");
            foreach(var i in install) Console.WriteLine($"  {i}");
            Assert.That(uninstall.Count(), Is.EqualTo(4));
            //Assert.That(uninstall.ElementAt(0), Is.EqualTo(installed[1]));
            //Assert.That(uninstall.ElementAt(1), Is.EqualTo(installed[3]));
        }
    }


}

