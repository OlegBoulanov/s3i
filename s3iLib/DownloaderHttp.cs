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
        public override async Task<HttpStatusCode> DownloadAsync(Uri uri, DateTime modifiedSinceDateUtc, Func<Stream, Task> processStream)
        {
            Contract.Requires(null != uri);
            Contract.Requires(null != processStream);
            using (var client = new HttpClient())
            {
                using (var result = await client.GetAsync(uri).ConfigureAwait(false))
                {
                    if (result.IsSuccessStatusCode)
                    {
                        using (var responseStream = await result.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        {
                            await processStream.Invoke(responseStream).ConfigureAwait(false);
                        }
                    }
                    return result.StatusCode;
                }
            }
        }
    }
}
