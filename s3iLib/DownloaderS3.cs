using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

namespace s3iLib
{
    public class DownloaderS3 : Downloader
    {
        public static bool CanDownload(Uri uri)
        {
#pragma warning disable CA1031  // Modify '***' to catch a more specific exception type, or rethrow the exception.
            try
            {
                return AmazonS3Uri.TryParseAmazonS3Uri(uri, out var s3url);
            }
            catch (Exception) { }
            return false;
#pragma warning restore CA1031
        }
        public override async Task<HttpStatusCode> DownloadAsync(Uri uri, DateTime modifiedSinceDateUtc, Func<Stream, Task> processStream)
        {
            Contract.Requires(null != uri);
            Contract.Requires(null != processStream);
            if(!AmazonS3Uri.TryParseAmazonS3Uri(uri, out var s3uri)) throw new UriFormatException($"Can't parse as AWS S3 URI: {uri}");
            var request = new GetObjectRequest { BucketName = s3uri.Bucket, Key = s3uri.Key, ModifiedSinceDateUtc = modifiedSinceDateUtc };
            var regionClient = await ClientMap.GetClientAsync(s3uri.Bucket).ConfigureAwait(false);
            using (var response = await regionClient.GetObjectAsync(request).ConfigureAwait(false))
            {
                using (var responseStream = response.ResponseStream)
                {
                    await processStream.Invoke(responseStream).ConfigureAwait(false);
                }
                return response.HttpStatusCode;
            }
        }
        public static bool SetProfile(string profileName)
        {
            if (new CredentialProfileStoreChain().TryGetAWSCredentials(profileName, out AWSCredentials credentials))
            {
                ClientMap = new AmazonS3ClientMap(credentials, null);
                return true;
            }
            return false;
        }
        protected static AmazonS3ClientMap ClientMap { get; set; }
    }
}
