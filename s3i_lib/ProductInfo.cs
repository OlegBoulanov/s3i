using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

namespace s3i_lib
{
    public class ProductInfo
    {
        public string Name { get; set; }
        public string RelativeUri { get; set; }
        public string AbsoluteUri { get; set; }
        public string LocalPath { get; set; }
        public HttpStatusCode DownloadResult { get; set; }
        public ProductProps Props { get; protected set; } = new ProductProps();
    }
}
