using System;
using System.Collections.Generic;
using System.Text;

namespace s3iLib
{
    public class InvalidUriException : Exception
    {
        public InvalidUriException(string s, Exception x) : base($"Invalid AWS S3 URI: {s}", x) { }
        public InvalidUriException() { }
        public InvalidUriException(string message) : base(message) { }
    }
}
