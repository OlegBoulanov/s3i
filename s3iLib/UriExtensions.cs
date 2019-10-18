using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using System.Net;

namespace s3iLib
{
    public static class UriExtensions
    {
        public static string MapToLocalPath(this Uri uri, string localPath)
        {
            var builder = new UriBuilder(uri);
            var subPath = $"{builder.Host}{Path.DirectorySeparatorChar}{builder.Path.Substring(1)}";
            return Path.Combine(localPath, subPath).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        }
        public static int CompareSemanticVersion(this Uri thisUri, Uri otherUri)
        {
            var thisVer = SemanticVersion.From(thisUri);
            var otherVer = SemanticVersion.From(otherUri);
            if (SemanticVersion.None == thisVer) return SemanticVersion.None == otherVer ? 0 : -1; else if (SemanticVersion.None == otherVer) return +1;
            return thisVer.CompareTo(otherVer);
        }
    }
}
