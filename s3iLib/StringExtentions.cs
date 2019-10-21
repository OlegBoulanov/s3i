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
        public static readonly Regex rexDotSegments = new Regex(@"[^\\/]+[\\/]+\.\.[\\/]+", RegexOptions.Compiled);
        public static readonly Regex rexSlashes = new Regex(@"[\\/]+", RegexOptions.Compiled);
        public static string RemoveDotSegments(this string s)
        {
            Contract.Requires(null != s);
            return s.Replace(rexDotSegments, "");
        }
        public static string UnifySlashes(this string s, char separator = '\0')
        {
            Contract.Requires(null != s);
            return rexSlashes.Replace(s, $"{('\0' != separator ? separator : Path.DirectorySeparatorChar)}");
        }
        public static string Replace(this string s, Regex rex, string replacement)
        {
            Contract.Requires(null != s);
            Contract.Requires(null != rex);
            Contract.Requires(null != replacement);
            string next = s;
            // single regex replacement may produce next string for replacement, thus the loop
            for (var prev = next; null != prev && !prev.Equals(next = rex.Replace(prev, replacement, 1), StringComparison.CurrentCulture); prev = next) ;
            return next;
        }
        public static Uri BuildRelativeUri(this Uri baseUri, string path)
        {
            Contract.Requires(null != baseUri);
            try
            {
                return new Uri(path);
            }
#pragma warning disable CA1031  // ... catch more specific exception... can I even be more specific?
            catch(UriFormatException)
#pragma warning restore CA1031
            {
                // Not a URI: must be relative - do we need more checks?
                var builder = new UriBuilder(baseUri);
                builder.Path = Path.Combine(Path.GetDirectoryName(builder.Path), path).RemoveDotSegments();
                return builder.Uri;
            }
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
