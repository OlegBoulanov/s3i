using System;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.Text;

using Amazon.Runtime;
using Amazon.S3;

namespace s3iLib
{
    public static class ExceptionExtensions
    {
        public static string Format(this Exception x)
        {
            return x.Format(4);
        }
        public static string Format(this Exception x, int indent)
        {
            Contract.Requires(null != x);
            var sb = new StringBuilder();
            if (x is AmazonS3Exception ax) sb.AppendLine($"{ax.GetType().Name}: {ax.ErrorType}, {ax.ErrorCode}, {ax.Message}");
            else if (null != x) sb.AppendLine($"{x.GetType().Name}: {x.Message}");
            else sb.AppendLine($"(null) exception: (null)");
            if (0 < indent)
            {
                var spaceCount = indent;
                for (var inner = x.InnerException; null != inner; inner = inner.InnerException, spaceCount += indent)
                {
                    sb.AppendLine($"{"".PadLeft(spaceCount)}{inner.GetType().Name}: {inner.Message}");
                }
            }
            return sb.ToString();
        }
    }
}
