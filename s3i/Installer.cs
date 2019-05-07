using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Amazon.S3.Util;

namespace s3i
{
    class Installer
    {
        public AmazonS3Uri Uri { get; protected set; }
        public Installer(string s3uri)
        {
            Uri = new AmazonS3Uri(s3uri);
        }
    }
}
