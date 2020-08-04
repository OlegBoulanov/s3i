using System;
using NUnit.Framework;

using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using s3iLib;
using System.Reflection;

namespace s3iLibTests
{
    
    public class ProductCollectionTest
    {
        const string testConfig = @"[$Products$]

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
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(testConfig));
            var products = await ProductCollection.FromIni(stream, DateTimeOffset.UtcNow, null).ConfigureAwait(false); 
            products.MapToLocal("C:\\Temp\\");
            Assert.AreEqual(2, products.Count);
            Assert.AreEqual("ProductOne", products[0].Name);
            Assert.AreEqual("SecondProduct", products[1].Name);
            Assert.AreEqual("https://xxx.s3.amazonaws.com/Test/Windows10/Distrib/ProductOne/12.6.16/ProductOne.msi", products[0].Uri.ToString());
            Assert.AreEqual(2, products[0].Props.Count);
            Assert.AreEqual("https://s3.amazonaws.com/config.company.com/Engineering/ESR/", products[0].Props["CONFIG_ROOT"]);
            Assert.AreEqual("https://s3.amazonaws.com/logs.company.com/{0:yyyy}/{0:MM}/{0:dd}/{1}/{0:yyMMdd_HH}{3:00}.log", products[0].Props["ROLLING_LOG"]);
            Assert.AreEqual(4, products[1].Props.Count);
            Assert.AreEqual("https://s3.amazonaws.com/prompts.company.com/", products[1].Props["UNC_PROMPTS"]);
            Assert.AreEqual("https://s3.amazonaws.com/sessions.company.com/", products[1].Props["UNC_SESSIONS"]);
            Assert.AreEqual("https://s3.amazonaws.com/prompts.update.company.com/", products[1].Props["PROMPTS_UPDATE"]);
            Assert.AreEqual("00:15:00 1000 20000000 00:01:00", products[1].Props["SYNC"]);
        }

        const string ssmTestConfig = @"[$Products$]

ProductOne = https://xxx.s3.amazonaws.com/Test/Windows10/Distrib/ProductOne/12.6.16/ProductOne.msi
           
 [ProductOne]
 SYNC            = 00:15:00 1000 20000000 00:01:00
 BUCKET_SUFFIX = Local bucket suffix: ${ssm:/hms/tpm/dice/local/naming/s3/bucket_suffix} => ${ssm:/hms/tpm/dice/local/naming/env/name}

";
        //[Test]
        public async Task TestSSM()
        {
            AmazonAccount.ProfileName = "default";
            AmazonAccount.RegionName = "ap-southeast-2";
            
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(ssmTestConfig));
            var products = await ProductCollection.FromIni(stream, DateTimeOffset.UtcNow, null).ConfigureAwait(false); 
            products.MapToLocal("C:\\Temp\\");
            Assert.AreEqual(1, products.Count);
            Assert.AreEqual("00:15:00 1000 20000000 00:01:00", products[0].Props["SYNC"]);
            Assert.AreEqual("Local bucket suffix: ivr.dev.none.apse2.tpm.hms => dev", products[0].Props["BUCKET_SUFFIX"]);
        }

        const string manyProducts = @"[$Products$]
One = https://xxx.s3.amazonaws.com/Test/Windows10/Distrib/ProductOne/12.6.16/ProductOne.msi
Two    = ../../Distrib/SecondProduct/9.4.188/SecondProduct.msi
Three    = https://xxx.s3.amazonaws.com/Test/Windows10/Distrib//SecondProduct/9.4.188/SecondProduct.msi
";           
        [Test]
        public async Task TestDownloadPaths()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(manyProducts));
            var products = await ProductCollection.FromIni(stream, DateTimeOffset.UtcNow, null).ConfigureAwait(false);
            products.MapToLocal("C:\\Temp\\");
            var files = from p in products select new { product = p, local = p.LocalPath };
            Assert.AreEqual(3, products.Count);
            Assert.AreEqual(3, files.Count());
            var x = Path.DirectorySeparatorChar;
            Assert.AreEqual($"C:{x}Temp{x}One{x}ProductOne.msi", files.ToArray()[0].local);
            Assert.AreEqual($"C:{x}Temp{x}Two{x}SecondProduct.msi", files.ToArray()[1].local);
            Assert.AreEqual($"C:{x}Temp{x}Three{x}SecondProduct.msi", files.ToArray()[2].local);
        }

        const string products1 = "[$products$]\nOne=https://x.amazonaws.com/one/config.ini\nTwo=https://x.amazonaws.com/one/config.ini\n[One]p11=1\np12=2\n[Two]\np21=11\np22=12\n";
        const string products2 = "[$products$]\nOne=https://x.amazonaws.com/one/config.ini\nTwo=https://x.amazonaws.com/one/config.ini\n[One]p11=1\np12=2\n[Two]\np21=11\np22=12\n";
        const string products3 = "[$products$]\nOne=https://x.amazonaws.com/one/config.ini\nTwo=https://x.amazonaws.com/one/config.ini\n[One]p11=1\np12=2\n[Two]\np21=11\np22=12\n";

        [Test]
        public async Task TestDiff()
        {
            var baseUri = new Uri("https://mecompany.s3.amazonaws.com/something/config.ini");
            using var stream1 = new MemoryStream(Encoding.ASCII.GetBytes(products1));
            using var stream2 = new MemoryStream(Encoding.ASCII.GetBytes(products2));
            using var stream3 = new MemoryStream(Encoding.ASCII.GetBytes(products3));
            var p1 = await ProductCollection.FromIni(stream1, DateTimeOffset.UtcNow).ConfigureAwait(false);
            var p2 = await ProductCollection.FromIni(stream2, DateTimeOffset.UtcNow).ConfigureAwait(false);
            var p3 = await ProductCollection.FromIni(stream3, DateTimeOffset.UtcNow).ConfigureAwait(false);
        }

        [Test]
        public void ProductsToUninstall()
        {
            var tempFilePath = "X:\\temp\\";
            var products = new ProductCollection();
            products.AddRange(new List<string> {
                "https://download/from/here/p1.msi",
                "https://download/from/here/p2.msi",
                "https://download/from/there/p3.msi",
                "https://download/from/somewhere/p4.msi",
            }.Aggregate(new ProductCollection(), (ps, s) =>
            {
                ps.Add(new ProductInfo { Name = Path.GetFileNameWithoutExtension(s), Uri = new Uri(s), LocalPath = new Uri(s).MapToLocalPath(tempFilePath) }); return ps;
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
            Assert.That(dlls.Count(), Is.EqualTo(3));
            Assert.That(dlls.Contains($"{Path.Combine(tempFilePath, "AWSSDK.Core.dll")}"));
            Assert.That(dlls.Contains($"{Path.Combine(tempFilePath, "AWSSDK.S3.dll")}"));
            Assert.That(dlls.Contains($"{Path.Combine(tempFilePath, "AWSSDK.SecurityToken.dll")}"));
        }

        [Test]
        public void Separate()
        {
            Assert.That(SemanticVersion.From("https://download/here/Prod01/9.4.7/p1.msi").CompareTo(SemanticVersion.From("https://download/here/Prod01/9.4.8+upgrade/p1.msi")), Is.LessThan(0));
            Assert.That(SemanticVersion.From("https://download/here/Prod02/3.3.5/p2.msi").CompareTo(SemanticVersion.From("https://download/here/Prod02/3.3.5+keep/p2.msi")), Is.EqualTo(0));
            Assert.That(SemanticVersion.From("https://download/there/Prod03/12.5.8/p3.msi").CompareTo(SemanticVersion.From("https://download/there/Prod03/12.5.4+downgrade/p3.msi")), Is.GreaterThan(0));
            //Assert.That(SemanticVersion.From("https://download/here/Prod01/9.4.7/p1.msi").CompareTo(SemanticVersion.From("https://download/here/Prod01/9.4.8+upgrade/p1.msi")), Is.LessThan(0));
            var installed = new Dictionary<string, string> {
                { "Prod01\\p1.msi", "https://download/here/Prod01/9.4.7/p1.msi" },
                { "Prod02\\p2.msi", "https://download/here/Prod02/3.3.5/p2.msi" },
                { "Prod03\\p3.msi", "https://download/there/Prod03/12.5.8/p3.msi" },
                { "ProdXX\\px.msi", "https://download/there/ProdXX/1.2.5.8+uninstall/px.msi" },
            };
            var products = new ProductCollection { 
                new ProductInfo { Name = "Prod01", Uri = new Uri("https://download/here/Prod01/9.4.8+upgrade/p1.msi"), LocalPath = "Prod01\\p1.msi", },
                new ProductInfo { Name = "Prod02", Uri = new Uri("https://download/here/Prod02/3.3.5+keep/p2.msi"), LocalPath = "Prod02\\p2.msi", },
                new ProductInfo { Name = "Prod03", Uri = new Uri("https://download/there/Prod03/12.5.4+downgrade/p3.msi"), LocalPath = "Prod03\\p3.msi", },
                new ProductInfo { Name = "Prod04", Uri = new Uri("https://download/from/Prod04/1.2.3+install/p4.msi"), LocalPath = "Prod04\\p4.msi", },
            };
            var remove = products.FilesToUninstall(installed.Keys);
            var (uninstall, install) = products.PrepareActions(localPath =>
            {
                var uri = installed.ContainsKey(localPath) ? installed[localPath] : null;
                return null != uri ? new ProductInfo { Uri = new Uri(uri), } : null;
            });
            //Console.WriteLine($"Remove:");
            //foreach (var r in remove) Console.WriteLine($"  {r}"); 
            //Console.WriteLine($"Uninstall:");
            //foreach(var u in uninstall) Console.WriteLine($"  {u}");
            //Console.WriteLine($"Install:");
            //foreach(var i in install) Console.WriteLine($"  {i}");
            Assert.That(remove.Count(), Is.EqualTo(1));
            Assert.That(remove.ElementAt(0), Is.EqualTo("ProdXX\\px.msi"));
            Assert.That(uninstall.Count(), Is.EqualTo(1));
            Assert.That(uninstall.ElementAt(0).Uri.ToString(), Is.EqualTo("https://download/there/Prod03/12.5.8/p3.msi"));
            Assert.That(install.Count(), Is.EqualTo(4));
            Assert.That(install.ElementAt(0).Uri.ToString(), Is.EqualTo("https://download/here/Prod01/9.4.8+upgrade/p1.msi"));
            Assert.That(install.ElementAt(1).Uri.ToString(), Is.EqualTo("https://download/here/Prod02/3.3.5+keep/p2.msi"));
            Assert.That(install.ElementAt(2).Uri.ToString(), Is.EqualTo("https://download/there/Prod03/12.5.4+downgrade/p3.msi"));
            Assert.That(install.ElementAt(3).Uri.ToString(), Is.EqualTo("https://download/from/Prod04/1.2.3+install/p4.msi"));
        }


        [Test]
        public void RealisticSEparationTest()
        {
            var installed = new Dictionary<string, string> {
                { "Prod01\\p1.msi", "https://download/here/Prod01/9.4.7/p1.msi" },
                { "Prod02\\p2.msi", "https://download/here/Prod02/3.3.5/p2.msi" },
            };
            var products = new ProductCollection { 
                new ProductInfo { Name = "Prod01", Uri = new Uri("https://download/here/Prod01/9.4.8/p1.msi"), LocalPath = "Prod01\\p1.msi", },
                new ProductInfo { Name = "Prod04", Uri = new Uri("https://download/from/Prod04/1.2.3+install/p4.msi"), LocalPath = "Prod04\\p4.msi", },
            };
            var remove = products.FilesToUninstall(installed.Keys);
            var (uninstall, install) = products.PrepareActions(localPath =>
            {
                var uri = installed.ContainsKey(localPath) ? installed[localPath] : null;
                return null != uri ? new ProductInfo { Uri = new Uri(uri), } : null;
            });
            Assert.That(remove.Count(), Is.EqualTo(1));
            Assert.That(remove.ElementAt(0), Is.EqualTo("Prod02\\p2.msi"));
            Assert.That(uninstall.Count(), Is.EqualTo(0));
            Assert.That(install.Count(), Is.EqualTo(2));
            Assert.That(install.ElementAt(0).Uri.ToString(), Is.EqualTo("https://download/here/Prod01/9.4.8/p1.msi"));
            Assert.That(install.ElementAt(1).Uri.ToString(), Is.EqualTo("https://download/from/Prod04/1.2.3+install/p4.msi"));
        }
        static string ThisFilePath([System.Runtime.CompilerServices.CallerFilePath] string thisFilePath = "") { return thisFilePath; }

        [Test]
        public static void SelfTestLocal()
        {
            //Assert.AreEqual("", ThisFilePath());
            var uri = new Uri(Path.Combine(Path.GetDirectoryName(ThisFilePath()), "ProductCollectionTest.ini"));
            //Assert.AreEqual("", uri.GetAbsoluteFilePath());
            var products = ProductCollection.ReadProducts(new List<Uri> { uri }).Result;
            Assert.AreEqual(1, products.Count);
            var product1 = products.FirstOrDefault();
            Assert.AreEqual("UselessProduct", product1.Name);
            Assert.AreEqual(1, product1.Props.Count);
        }
        [Test]
        public static void SelfTestGitHub()
        {
            var products = ProductCollection.ReadProducts(new List<Uri> { new Uri("https://raw.githubusercontent.com/OlegBoulanov/s3i/develop/s3iLibTests/ProductCollectionTest.ini") }).Result;
            Assert.AreEqual(1, products.Count);
            var product1 = products.FirstOrDefault();
            Assert.AreEqual("UselessProduct", product1.Name);
            Assert.AreEqual(1, product1.Props.Count);
        }
    }


}

