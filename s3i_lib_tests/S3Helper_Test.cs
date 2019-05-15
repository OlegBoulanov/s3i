using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Amazon.S3.Util;

using s3i_lib;

namespace s3i_lib_tests
{
    [TestClass]
    public class S3Helper_Test
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            var commandLine = CommandLine.Parse("-p", "elizacorp-shared.olegb", "https://install.elizacorp.com.s3.amazonaws.com/Test/Windows10/Config/Nashville/Group02/Products.ini");
            var s3 = new S3Helper(commandLine.Options[CommandLine.OptionType.ProfileName]);
            var uri = new AmazonS3Uri(commandLine.Args[0]);
            var lines = new ConcurrentQueue<string>();
            await s3.DownloadAsync(
                uri.Bucket, uri.Key, 
                async (type, stream) => {
                    using (var reader = new StreamReader(stream)) {
                        for (string s; null != (s = await reader.ReadLineAsync()); ) lines.Enqueue(s);
                    }
                });
            foreach (var s in lines) Console.WriteLine(s);
            Console.WriteLine($"Done: {lines.Count}");
            Assert.AreEqual(88, lines.Count);
        }
    }
}
