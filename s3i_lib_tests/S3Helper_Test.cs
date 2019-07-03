using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

using System.Diagnostics;

using s3i_lib;

namespace s3i_lib_tests
{
    [TestClass]
    [TestCategory("AWS")]

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

        [TestMethod]
        public async Task ReadObjectDataAsync()
        {
            IAmazonS3 S3 = null;
            var chain = new CredentialProfileStoreChain();
            if (chain.TryGetAWSCredentials(testProfileName, out AWSCredentials credentials))
            {
                S3 = new AmazonS3Client(credentials, Amazon.RegionEndpoint.USEast1);
            }

            for (var i = 0; i < 10; i++)
            {
                Console.WriteLine($"(Sync):  {ReadObjectDataTest((request) => { return S3.GetObject(request); })}");
            }
            for (var i = 0; i < 10; i++)
            {
                Console.WriteLine($"(async): {ReadObjectDataTest((request) => { return S3.GetObjectAsync(request).Result; })}");
            }
            var tasks = new List<Task>();
            for (var i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    Console.WriteLine($"(tasks): {ReadObjectDataTestAsync(async (request) => { return await S3.GetObjectAsync(request); }).Result}");
                }));
            }
            await Task.WhenAll(tasks);

            await Task.CompletedTask;
        }

        public TimeSpan ReadObjectDataTest(Func<GetObjectRequest, GetObjectResponse> getObject)
        {
            var clock = Stopwatch.StartNew();
            // as in here: https://docs.aws.amazon.com/AmazonS3/latest/dev/RetrievingObjectUsingNetSDK.html
            var uri = new AmazonS3Uri(testObjectS3Uri);
            var request = new GetObjectRequest { BucketName = uri.Bucket, Key = uri.Key, };
            using (GetObjectResponse response = getObject(request))
            using (Stream responseStream = response.ResponseStream)
            using (StreamReader reader = new StreamReader(responseStream))
            {
                var responseBody = reader.ReadToEnd();
            }
            return clock.Elapsed;
        }

        public async Task<TimeSpan> ReadObjectDataTestAsync(Func<GetObjectRequest, Task<GetObjectResponse>> getObject)
        {
            var clock = Stopwatch.StartNew();
            // as in here: https://docs.aws.amazon.com/AmazonS3/latest/dev/RetrievingObjectUsingNetSDK.html
            var uri = new AmazonS3Uri(testObjectS3Uri);
            var request = new GetObjectRequest { BucketName = uri.Bucket, Key = uri.Key, };
            using (GetObjectResponse response = await getObject(request))
            using (Stream responseStream = response.ResponseStream)
            using (StreamReader reader = new StreamReader(responseStream))
            {
                var responseBody = await reader.ReadToEndAsync();
            }
            return clock.Elapsed;
        }
    }

}


