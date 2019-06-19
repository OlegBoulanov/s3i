using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Text.RegularExpressions;

using Amazon.S3.Util;

namespace s3i_lib
{
    public static class StringExtentions
    {
        public static readonly Regex rexDotSegments = new Regex(@"[^\\/]+[\\/]\.\.[\\/]", RegexOptions.Compiled);
        public static readonly Regex rexSlashes = new Regex(@"[\\/]+", RegexOptions.Compiled);
        public static string RemoveDotSegments(this string s)
        {
            return s.Replace(rexDotSegments, "");
        }
        public static string ReplaceSlashes(this string s, string slash = "/")
        {
            return s.Replace(rexSlashes, slash);
        }
        public static string Replace(this string s, Regex rex, string replacement)
        {
            string next = s;
            for (var prev = next; !prev.Equals(next = rex.Replace(prev, replacement, 1)); prev = next);
            return next;
        }
        public static string BuildRelativeUri(this string baseUri, string path)
        {
            var builder = new UriBuilder(baseUri);
            builder.Path = Path.Combine(Path.GetDirectoryName(builder.Path), path).RemoveDotSegments();
            return builder.ToString();
        }
        static Regex rexUri = new Regex(@"^[a-z]+\://", RegexOptions.Compiled);
        public static bool IsUri(this string path)
        {
            return rexUri.IsMatch(path);
        }
        public static string RebaseUri(this string uri, string baseUri)
        {
            return IsUri(uri) ? uri : Path.IsPathRooted(uri) ? uri : baseUri.BuildRelativeUri(uri);
        }
        public static string MapToLocalPath(this string uri, string localPath)
        {
            var builder = new UriBuilder(uri);
            var subPath = $"{builder.Host}/{builder.Path.Substring(1)}";
            return Path.Combine(localPath, subPath).ReplaceSlashes("/");
        }
    }
}
