using System;
using System.Collections.Generic;
using System.Text;

namespace s3i_lib
{
    public static class ExceptionExtensions
    {
        public static string Format(this Exception x)
        {
            return x.Format(4);
        }
        public static string Format(this Exception x, int indent)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{x.GetType().Name}: {x.Message}");
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
