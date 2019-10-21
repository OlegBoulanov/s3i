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
            var regionClient = await ClientMap.Value.GetClientAsync(s3uri.Bucket).ConfigureAwait(false);
            var request = new GetObjectRequest { BucketName = s3uri.Bucket, Key = s3uri.Key, ModifiedSinceDateUtc = modifiedSinceDateUtc };
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
        public static string ProfileName { get; set; }
        public static AmazonS3Client DefaultClient { get; set; }
        static Lazy<AmazonS3ClientMap> ClientMap = new Lazy<AmazonS3ClientMap>(() =>
        {
            if (!new CredentialProfileStoreChain().TryGetAWSCredentials(ProfileName, out var credentials))
                throw new ApplicationException($"Can't find AWS profile [{ProfileName}]");
            return new AmazonS3ClientMap(credentials, DefaultClient);
        });
        #endregion
    }
}
