using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Text.RegularExpressions;


namespace s3i_lib
{
    public static class StringExtentions
    {
        static Regex rexDotSegments = new Regex(@"([^\\/]+[\\|/]\.\.[\\/])", RegexOptions.Compiled);
        public static string RemoveDotSegments(this string s)
        {
            string next, prev = s;
            for (; !prev.Equals(next = rexDotSegments.Replace(prev, "")); prev = next) ;
            return next;
        }
        public static string BuildRelativeUri(this string uri, string path)
        {
            var builder = new UriBuilder(uri);
            builder.Path = Path.Combine(builder.Path, path).RemoveDotSegments();
            return builder.ToString();
        }
    }
}
