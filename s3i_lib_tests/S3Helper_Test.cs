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
        const string testProfileName = "default";
        const string testObjectS3Uri = "https://bucket.s3.amazonaws.com/key";
        const int testObjectLineCount = 35;

        [TestMethod]
        public async Task S3Download()
        {
            var uri = new AmazonS3Uri(testObjectS3Uri);
            var lines = new ConcurrentQueue<string>();
            await new S3Helper(testProfileName).DownloadAsync(
                uri.Bucket, uri.Key, 
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
