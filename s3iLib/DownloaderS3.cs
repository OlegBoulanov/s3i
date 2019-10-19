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
            return TryParse(uri, out _);
        }
        // See https://github.com/aws/aws-sdk-net/issues/1426
        public static bool TryParse(Uri uri, out AmazonS3Uri s3uri)
        {
            Contract.Requires(null != uri);
#pragma warning disable CA1031  // Modify '***' to catch a more specific exception type, or rethrow the exception.
            try { return AmazonS3Uri.TryParseAmazonS3Uri(uri, out s3uri); }
            catch (Exception) { s3uri = null; return false; }
#pragma warning restore CA1031
        }
        public override async Task<HttpStatusCode> DownloadAsync(Uri uri, DateTime modifiedSinceDateUtc, Func<Stream, Task> processStream)
        {
            Contract.Requires(null != uri);
            Contract.Requires(null != processStream);
            if (!TryParse(uri, out var s3uri)) throw new UriFormatException($"Can't parse as AWS S3 URI: {uri}");
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
        #region Static data and methods
        public static bool SetProfile(string profileName, AmazonS3Client defaultClient = null)
        {
            Contract.Requires(null != profileName);
            if (new CredentialProfileStoreChain().TryGetAWSCredentials(profileName, out var credentials))
            {
                ClientMap = new AmazonS3ClientMap(credentials, defaultClient);
                return true;
            }
            return false;
        }
        protected static AmazonS3ClientMap ClientMap { get; set; }
        #endregion
    }
}
