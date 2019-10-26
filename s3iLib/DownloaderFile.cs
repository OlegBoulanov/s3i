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
    public class DownloaderFile : Downloader
    {
        public static bool CanDownload(Uri uri)
        {
            Contract.Requires(null != uri);
            return "file" == uri.Scheme;
        }
        public override async Task<HttpStatusCode> DownloadAsync(Uri uri, DateTime modifiedSinceDateUtc, Func<Stream, DateTimeOffset, Task> processStream)
        {
            Contract.Requires(null != uri);
            Contract.Requires(null != processStream);
            var filePath = uri.GetAbsoluteFilePath();
            if(!File.Exists(filePath)) return HttpStatusCode.NotFound;
            var lastModified = File.GetLastWriteTimeUtc(filePath);
            using(var stream = new FileStream(filePath, FileMode.Open))
            {
                await processStream.Invoke(stream, lastModified).ConfigureAwait(false);
            }
            return HttpStatusCode.OK;
        }
    }
}