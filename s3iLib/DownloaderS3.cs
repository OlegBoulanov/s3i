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
        public override async Task<HttpStatusCode> DownloadAsync(Uri uri, DateTime modifiedSinceDateUtc, Func<Stream, DateTimeOffset, Task> processStream)
        {
            Contract.Requires(null != uri);
            Contract.Requires(null != processStream);
            if (!TryParse(uri, out var s3uri)) throw new UriFormatException($"Can't parse as AWS S3 URI: {uri}");
            var regionClient = await ClientMap.Value.GetClientAsync(s3uri.Bucket).ConfigureAwait(false);
            var request = new GetObjectRequest { BucketName = s3uri.Bucket, Key = s3uri.Key, ModifiedSinceDateUtc = modifiedSinceDateUtc };
            try
            {
                using (var response = await regionClient.GetObjectAsync(request).ConfigureAwait(false))
                {
                    using (var responseStream = response.ResponseStream)
                    {
                        await processStream.Invoke(responseStream, response.LastModified).ConfigureAwait(false);
                    }
                    return response.HttpStatusCode;
                }
            }
            catch(AmazonS3Exception x)
            {
                if (HttpStatusCode.NotModified == x.StatusCode) return x.StatusCode;
                throw;
            }
        }
        #region Static data and methods
        public static AmazonS3Client DefaultClient { get; set; }
        static readonly Lazy<AmazonS3ClientMap> ClientMap = new Lazy<AmazonS3ClientMap>(() =>
        {
            var credentials = AmazonAccount.Credentials.Value;
            return new AmazonS3ClientMap(credentials, DefaultClient);
        });
        #endregion
    }
}
