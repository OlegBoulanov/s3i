using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

using Amazon.S3.Util;

namespace s3iLib
{
    public abstract class Downloader
    {
        public abstract Task<HttpStatusCode> DownloadAsync(Uri uri, DateTime modifiedSinceDateUtc, Func<Stream,DateTimeOffset, Task> processStream);
        public async Task<HttpStatusCode> DownloadAsync(Uri uri, string localFilePath)
        {
            Contract.Requires(null != uri);
            Contract.Requires(null != localFilePath);
            var lastWriteTimeUtc = File.GetLastWriteTimeUtc(localFilePath);
            return await DownloadAsync(uri, lastWriteTimeUtc,
                async (stream, lastModified) =>
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(localFilePath));
                    using (var file = File.Create(localFilePath))
                    {
                        await stream.CopyToAsync(file).ConfigureAwait(false);
                    }
                    File.SetLastWriteTimeUtc(localFilePath, lastModified.UtcDateTime);
                }).ConfigureAwait(false);
        }
        public static Downloader Select(Uri url)
        {
            Contract.Requires(null != url);
            if (DownloaderS3.CanDownload(url)) return new DownloaderS3();
            return new DownloaderHttp();
        }
    }
}