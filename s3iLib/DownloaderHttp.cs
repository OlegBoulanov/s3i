using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace s3iLib
{
    public class DownloaderHttp : Downloader
    {
        public override async Task<HttpStatusCode> DownloadAsync(Uri uri, DateTime modifiedSinceDateUtc, Func<Stream, DateTimeOffset, Task> processStream)
        {
            Contract.Requires(null != uri);
            Contract.Requires(null != processStream);
            using (var handler = new HttpClientHandler { UseDefaultCredentials = true, })
            using (var client = new HttpClient(handler))
            using (var result = await client.GetAsync(uri).ConfigureAwait(false))
            {
                if (result.IsSuccessStatusCode)
                {
                    using (var responseStream = await result.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        await processStream.Invoke(responseStream, result.Content.Headers.LastModified ?? DateTimeOffset.UtcNow).ConfigureAwait(false);
                    }
                }
                return result.StatusCode;
            }
        }
    }
}
