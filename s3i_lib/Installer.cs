using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using System.IO;

using Amazon.S3.Util;
using System.Diagnostics;

namespace s3i_lib
{
    public class Installer
    {
        public ProductInfo Product { get; protected set; }
        public Installer(ProductInfo product)
        {
            Product = product;
        }

    }
}
