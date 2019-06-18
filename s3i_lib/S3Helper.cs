using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

namespace s3i_lib
{
    public class S3Helper
    {
        public AmazonS3Client S3 { get; protected set; }
        public S3Helper(string profileName)
        {
            var chain = new CredentialProfileStoreChain();
            AWSCredentials awsCredentials;
            if (chain.TryGetAWSCredentials(profileName, out awsCredentials))
            {
                // use awsCredentials
                S3 = new AmazonS3Client(awsCredentials);
            }
        }
        public async Task DownloadAsync(string bucket, string key, DateTime modifiedSinceDateUtc, Func<string, Stream, Task> processStream)
        {
            var request = new GetObjectRequest { BucketName = bucket, Key = key, ModifiedSinceDateUtc = modifiedSinceDateUtc };
            using (var response = await S3.GetObjectAsync(request))
            {
                using (var responseStream = response.ResponseStream)
                {
                    await processStream?.Invoke(response.Headers["Content-Type"], responseStream);
                }
            }
        }
    }
}
