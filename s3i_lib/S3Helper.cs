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

using System.Net;

namespace s3i_lib
{
    public class S3Helper
    {
        public AWSCredentials Credentials { get; protected set; }
        public AmazonS3Client S3 { get; protected set; }
        public S3Helper(string profileName)
        {
            var chain = new CredentialProfileStoreChain();
            AWSCredentials credentials = null;
            if (chain.TryGetAWSCredentials(profileName, out credentials))
            {
                Credentials = credentials;
                S3 = new AmazonS3Client(credentials);
            }
        }
        public async Task<HttpStatusCode> DownloadAsync(string bucket, string key, DateTime modifiedSinceDateUtc, Func<string, Stream, Task> processStream)
        {
            var request = new GetObjectRequest { BucketName = bucket, Key = key, ModifiedSinceDateUtc = modifiedSinceDateUtc };
            //using (var response = await (S3 ?? new AmazonS3Client(Credentials)).GetObjectAsync(request))
            using (var response = (S3 ?? new AmazonS3Client(Credentials)).GetObject(request))
            {
                using (var responseStream = response.ResponseStream)
                {
                    await processStream?.Invoke(response.Headers["Content-Type"], responseStream);
                }
                return response.HttpStatusCode;
            }
        }
        public async Task<HttpStatusCode> DownloadAsync(string bucket, string key, string localFilePath)
        {
            var lastWriteTimeUtc = File.GetLastWriteTimeUtc(localFilePath);
            try
            {
                return await DownloadAsync(bucket, key, lastWriteTimeUtc,
                    async (contentType, stream) =>
                    {
                        using (var file = File.Create(localFilePath))
                        {
                            await stream.CopyToAsync(file);
                        }
                    });
            }
            catch (AmazonS3Exception x)
            {
                switch (x.StatusCode)
                {
                    case HttpStatusCode.NotModified:
                        return x.StatusCode;
                    case HttpStatusCode.NotFound:
                        //File.Delete(localFilePath);
                        throw;
                    default:
                        throw;
                }
            }
        }
    }
}
