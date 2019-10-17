using System;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

using System.IO;
using System.Text.RegularExpressions;

using Amazon.S3.Util;

namespace s3iLib
{
    public static class PathExtensions
    {
        public static bool IsFullPath(this string path)
        {
            return !String.IsNullOrWhiteSpace(path)
                && path.IndexOfAny(System.IO.Path.GetInvalidPathChars().ToArray()) == -1
                && Path.IsPathRooted(path)
                && !Path.GetPathRoot(path).Equals(Path.DirectorySeparatorChar.ToString(CultureInfo.CurrentCulture), StringComparison.CurrentCulture);
        }
    }

    public static class StringExtentions
    {
        public static readonly Regex rexDotSegments = new Regex(@"[^\\/]+[\\/]\.\.[\\/]", RegexOptions.Compiled);
        public static readonly Regex rexSlashes = new Regex(@"[\\/]+", RegexOptions.Compiled);
        public static string RemoveDotSegments(this string s)
        {
            Contract.Requires(null != s);
            return s.ReplaceAll(rexDotSegments, "");
        }
        public static string ReplaceAll(this string s, Regex rex, string replacement)
        {
            Contract.Requires(null != s);
            Contract.Requires(null != rex);
            Contract.Requires(null != replacement);
            string next = s;
            for (var prev = next; !prev.Equals(next = rex.Replace(prev, replacement, 1), StringComparison.CurrentCulture); prev = next) ;
            return next;
        }
        public static Uri BuildRelativeUri(this Uri baseUri, string path)
        {
            Contract.Requires(null != baseUri);
            var builder = new UriBuilder(baseUri);
            builder.Path = Path.Combine(Path.GetDirectoryName(builder.Path), path).RemoveDotSegments();
            return builder.Uri;
        }
        public static string Quote(this string s, string quote, params char[] spaces)
        {
            Contract.Requires(null != s);
            Contract.Requires(null != spaces);
            if (string.IsNullOrEmpty(quote)) quote = "\"";
            if (0 == spaces.Length) spaces = new char[] { ' ', '\t' };
            if (string.IsNullOrEmpty(s) || 0 <= s.IndexOfAny(spaces))
            {
                return $"{quote}{s}{quote}";
            }
            return s;
        }
    }

    public static class IntegerExtensions
    { 
        public static string Plural(this int n, string suffix = "s") { return 1 == n ? "" : suffix; }
    }
}
