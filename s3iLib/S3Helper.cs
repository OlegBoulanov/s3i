using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using System.IO;

using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

using System.Net;

namespace s3iLib
{
    public class S3Helper
    {
        public AWSCredentials Credentials { get; protected set; }
        public AmazonS3ClientMap Clients { get; protected set; }
        public S3Helper(string profileName)
            :this(profileName, null)
        {
        }
        public S3Helper(string profileName, AmazonS3Client client)
        {
            if (new CredentialProfileStoreChain().TryGetAWSCredentials(profileName, out AWSCredentials credentials))
            {
                Credentials = credentials;
                Clients = new AmazonS3ClientMap(credentials, client);
            }
            else
            {
                throw new ApplicationException($"Can't get [{profileName}] credentials");
            }
        }
        public async Task<HttpStatusCode> DownloadAsync(string bucket, string key, DateTime modifiedSinceDateUtc, Func<string, Stream, Task> processStream)
        {
            Contract.Requires(null != bucket);
            Contract.Requires(null != key);
            Contract.Requires(null != processStream);
            var request = new GetObjectRequest { BucketName = bucket, Key = key, ModifiedSinceDateUtc = modifiedSinceDateUtc };
            var regionClient = await Clients.GetClientAsync(bucket).ConfigureAwait(false);
            using (var response = await regionClient.GetObjectAsync(request).ConfigureAwait(false))
            {
                using (var responseStream = response.ResponseStream)
                {
                    await processStream.Invoke(response.Headers["Content-Type"], responseStream).ConfigureAwait(false);
                }
                return response.HttpStatusCode;
            }
        }
        public async Task<HttpStatusCode> DownloadAsync(string bucket, string key, string localFilePath)
        {
            Contract.Requires(null != bucket);
            Contract.Requires(null != key);
            Contract.Requires(null != localFilePath);
            var lastWriteTimeUtc = File.GetLastWriteTimeUtc(localFilePath);
            try
            {
                return await DownloadAsync(bucket, key, lastWriteTimeUtc,
                    async (contentType, stream) =>
                    {
                        using (var file = File.Create(localFilePath))
                        {
                            await stream.CopyToAsync(file).ConfigureAwait(false);
                        }
                    }).ConfigureAwait(false);
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
