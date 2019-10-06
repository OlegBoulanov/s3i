using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using System.Net;

namespace s3i_lib
{
    public static class UriExtensions
    {
        public static int CompareTo(this Uri thisUri, Uri otherUri)
        {
            var thisVer = SemanticVersion.From(thisUri);
            var otherVer = SemanticVersion.From(otherUri);
            if (null == thisVer) return null == otherVer ? 0 : -1; else if (null == otherVer) return +1;
            return thisVer.CompareTo(otherVer);
        }
    }
}
