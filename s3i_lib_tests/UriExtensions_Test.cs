using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using s3i_lib;

namespace s3i_lib_tests
{
    [TestClass]
    public class UriExtensions_Test
    {
        [TestMethod]
        public void Compare()
        {
            Assert.AreEqual(0, new Uri("http://host/uri1").CompareTo(new Uri("https://host2/other")));
            Assert.AreEqual(-1, new Uri("http://host/uri1").CompareTo(new Uri("https://host2/1.2.3-develop")));
            Assert.AreEqual(+1, new Uri("http://host/3.4.5-master").CompareTo(new Uri("https://host2/other")));
            Assert.AreEqual(-1, new Uri("http://host/3.4.4").CompareTo(new Uri("https://host2/sub3/3.4.5")));
            Assert.AreEqual(+1, new Uri("http://host/3.4.5").CompareTo(new Uri("https://host2/other/3/4/4")));
        }
    }
}
