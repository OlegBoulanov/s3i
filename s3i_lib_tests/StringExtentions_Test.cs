using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;
using System.Text;

using s3i_lib;

namespace s3i_lib_tests
{
    [TestClass]
    public class StringExtentions_Test
    {
        [TestMethod]
        public void RemoveDotSegments()
        {
            Assert.AreEqual("https://bucket.s3.amazonaws.com/directory1/die2/dir3/file.ext", "https://bucket.s3.amazonaws.com/directory1/die2/dir3/file.ext".RemoveDotSegments());
            Assert.AreEqual("https://bucket.s3.amazonaws.com/directory1/dir3/file.ext", "https://bucket.s3.amazonaws.com/directory1/die2/../dir3/file.ext".RemoveDotSegments());
        }
        [TestMethod]
        public void BuildRelativeUri()
        {
            Assert.AreEqual("https://bucket.s3.amazonaws.com:443/directory1/another.one", "https://bucket.s3.amazonaws.com/directory1/dir2/dir3/file.ext".BuildRelativeUri("..\\..\\another.one"));
        }
        [TestMethod]
        public void IsUri()
        {
            Assert.IsTrue("https://bucket.s3.amazonaws.com/directory1/die2/dir3/file.ext".IsUri());
            Assert.IsFalse("../../die2/dir3/file.ext".IsUri());
        }
        [TestMethod]
        public void RebaseUri()
        {
            Assert.AreEqual("https://bucket/dir1/dir2/dir3/file.ext", "https://bucket/dir1/dir2/dir3/file.ext".RebaseUri("https://bucket.s3.amazonaws.com/directory1/dir2/dir3/file.ext"));
            Assert.AreEqual("c:/dir3/file.ext", "c:/dir3/file.ext".RebaseUri("https://bucket/dir1/dir2/dir345/file99.ext88"));
            Assert.AreEqual("\\dir1\\dir2\\dir3/file.ext", Path.Combine(Path.GetDirectoryName("/dir1/dir2/dir345/file99.ext88"), "../dir3/file.ext").RemoveDotSegments());
            Assert.AreEqual("https://bucket:443/dir1/dir2/dir3/file.ext", "../dir3/file.ext".RebaseUri("https://bucket/dir1/dir2/dir345/file99.ext88"));
            Assert.AreEqual("https://bucket:443/dir1/dir2/dir3/", "../dir3/".RebaseUri("https://bucket/dir1/dir2/dir345/file99.ext88"));
        }
        [TestMethod]
        public void BuldLocalPath()
        {
            Assert.AreEqual("C:/Temp/subdir/bucket/dir1/dir2/dir345/file99.ext88", "https://bucket/dir1/dir2/dir345/file99.ext88".BuildLocalPath("C:\\Temp\\subdir"));
        }
    }
}
