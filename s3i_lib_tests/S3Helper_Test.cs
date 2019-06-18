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
        const string testProfileName = "s3i";
        const string testObjectS3Uri = "https://install.elizacorp.com.s3.amazonaws.com/Test/Windows10/Config/s3i/1/Products.ini";
        const int testObjectLineCount = 94;

        [TestMethod]
        public async Task S3Download()
        {
            var uri = new AmazonS3Uri(testObjectS3Uri);
            var lines = new ConcurrentQueue<string>();
            await new S3Helper(testProfileName).DownloadAsync(
                uri.Bucket, uri.Key, DateTime.MinValue,
                async (type, stream) => {
                    using (var reader = new StreamReader(stream)) {
                        for (string s; null != (s = await reader.ReadLineAsync()); ) lines.Enqueue(s);
                    }
                });
            foreach (var s in lines) Console.WriteLine(s);
            Console.WriteLine($"Done: {lines.Count}");
            Assert.AreEqual(testObjectLineCount, lines.Count);
        }
    }
}
