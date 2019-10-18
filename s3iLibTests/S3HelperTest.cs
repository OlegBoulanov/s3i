using System;
using NUnit.Framework;

using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

using System.Net;
using System.Runtime.InteropServices;
using System.Diagnostics;

using s3iLib;

namespace s3iLibTests
{
    public class S3HelperTest : AwsProfileDependentTestBase
    {
        const string testProfileName = "test.s3i";
        const string testBucketName = "test.s3i";
        const string testBucketRegion = "us-east-2";
        const string testKey = "misc/AccessingBucket.txt";
        const string testObjectS3Uri = "https://" + testBucketName + ".s3." + testBucketRegion + ".amazonaws.com/" + testKey;
        const string testIniS3Uri = "https://s3.us-east-2.amazonaws.com/test.s3i/config/Site01/Group01/Product.ini";
        const int testObjectLineCount = 49;

        [Test]
        [Category("AWS")]
        public async Task S3Download()
        {
            var s3 = new S3Helper(testProfileName);//, new AmazonS3Client(RegionEndpoint.USEast2));
            Assert.IsNotNull(s3.Credentials);
            Assert.IsNotNull(s3.Clients);
            var uri = new AmazonS3Uri(testObjectS3Uri);
            var lines = new ConcurrentQueue<string>();
            await s3.DownloadAsync(
                uri.Bucket, uri.Key, DateTime.MinValue,
                async (type, stream) => {
                    using var reader = new StreamReader(stream);
                    for (string s; null != (s = await reader.ReadLineAsync().ConfigureAwait(false));) lines.Enqueue(s);
                }).ConfigureAwait(false);
            foreach (var s in lines.Select((s, i) => $"{(i + 1),3}: {s}")) Console.WriteLine(s);
            Console.WriteLine($"Line count: {lines.Count}");
            Assert.AreEqual(testObjectLineCount, lines.Count);
        }


        [Test]
        [Category("AWS")]
        public async Task ReadTwoIniFilesFromS3()
        {
            var s3 = new S3Helper(testProfileName);
            Assert.IsNotNull(s3.Credentials);
            Assert.IsNotNull(s3.Clients);
            var maxAttempts = 3;// 3000;
            for (var i = 0; i < maxAttempts; i++) {
                var clock = System.Diagnostics.Stopwatch.StartNew();
                var prods = await ProductCollection.ReadProducts(s3,
                    new List<Uri> {
                        new Uri(testIniS3Uri),
                        new Uri(testIniS3Uri),
                    }).ConfigureAwait(false);
                var ms = clock.ElapsedMilliseconds;
                if (100 < ms)
                {
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} [{i:0000}] {clock.Elapsed:mm\\:ss\\.fff} {new string('*', (int)(ms / 100))}");
                }
                Assert.AreEqual(2, prods.Count);
            }
        }

    }

}


