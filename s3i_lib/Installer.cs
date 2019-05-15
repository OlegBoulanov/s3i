using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using System.IO;

using Amazon.S3.Util;

namespace s3i_lib
{
    public class Installer
    {
        public S3Helper S3 { get; protected set; }
        public Installer(S3Helper s3)
        {
            S3 = s3;
        }
        public async Task<Products> ReadProducts(string configFileUri)
        {
            Products products = null;
            var uri = new AmazonS3Uri(configFileUri);
            await S3.DownloadAsync(uri.Bucket, uri.Key, async (contentType, stream) =>
            {
                products = await Products.FromIni(stream);
            });
            return products;
        }

    }
}
