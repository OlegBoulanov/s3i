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
            Assert.AreEqual(@"a/b\d/e/../g\x.y", StringExtentions.rexDotSegments.Replace(@"a/b\c\../d/e/f/..\../g\x.y", @""));
            Assert.AreEqual(@"a/b\d/g\x.y", StringExtentions.rexDotSegments.Replace(@"a/b\d/e/../g\x.y", @""));
            Assert.AreEqual(@"a/b\d/g\x.y", @"a/b\c\../d/e/f/..\../g\x.y".ReplaceAll(StringExtentions.rexDotSegments, @""));
            Assert.AreEqual(@"a/b/c/g/x.y", @"a/b/c/d/e/f/../../../g/x.y".ReplaceAll(StringExtentions.rexDotSegments, @""));
            Assert.AreEqual("https://bucket.s3.amazonaws.com/directory1/die2/dir3/file.ext", "https://bucket.s3.amazonaws.com/directory1/die2/dir3/file.ext".RemoveDotSegments());
            Assert.AreEqual("https://bucket.s3.amazonaws.com/directory1/dir3/file.ext", "https://bucket.s3.amazonaws.com/directory1/die2/../dir3/file.ext".RemoveDotSegments());
            Assert.AreEqual("https://install.elizacorp.com.s3.amazonaws.com/Test/Windows10/Distrib/Minnie/9.4.188/Minnie.msi", 
                "https://install.elizacorp.com.s3.amazonaws.com/Test/Windows10/Config/s3i/1/../../../Distrib/Minnie/9.4.188/Minnie.msi".RemoveDotSegments());
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
            Assert.AreEqual("https://bucket/dir1/dir2/dir3/file.ext",
                "https://bucket/dir1/dir2/dir3/file.ext".RebaseUri("https://bucket.s3.amazonaws.com/directory1/dir2/dir3/file.ext"));
            Assert.AreEqual("c:/dir3/file.ext", "c:/dir3/file.ext".RebaseUri("https://bucket/dir1/dir2/dir345/file99.ext88"));
            Assert.AreEqual("\\dir1\\dir2\\dir3/file.ext", Path.Combine(Path.GetDirectoryName("/dir1/dir2/dir345/file99.ext88"), "../dir3/file.ext").RemoveDotSegments());
            Assert.AreEqual("https://bucket:443/dir1/dir2/dir3/file.ext", "../dir3/file.ext".RebaseUri("https://bucket/dir1/dir2/dir345/file99.ext88"));
            Assert.AreEqual("https://bucket:443/dir1/dir2/dir3/", "../dir3/".RebaseUri("https://bucket/dir1/dir2/dir345/file99.ext88"));
        }

        [TestMethod]
        public void RebaseUri2()
        {
            Assert.AreEqual("https://install.elizacorp.com.s3.amazonaws.com:443/Test/Windows10/Distrib/Minnie/9.4.188/Minnie.msi",
                "https://install.elizacorp.com.s3.amazonaws.com/Test/Windows10/Config/s3i/1/Products.ini".BuildRelativeUri("../../../Distrib/Minnie/9.4.188/Minnie.msi"));
            Assert.AreEqual("https://install.elizacorp.com.s3.amazonaws.com:443/Test/Windows10/Distrib/Minnie/9.4.188/Minnie.msi", 
                "../../../Distrib/Minnie/9.4.188/Minnie.msi".RebaseUri("https://install.elizacorp.com.s3.amazonaws.com/Test/Windows10/Config/s3i/1/Products.ini"));
        }
        [TestMethod]
        public void MapToLocalPath()
        {
            Assert.AreEqual("C:/Temp/subdir/bucket/dir1/dir2/dir345/file99.ext88", "https://bucket/dir1/dir2/dir345/file99.ext88".MapToLocalPath("C:\\Temp\\subdir"));
            Assert.AreEqual("C:/Temp/subdir/install.elizacorp.com.s3.amazonaws.com/Test/Windows10/Config/s3i/1/Products.ini", "https://install.elizacorp.com.s3.amazonaws.com/Test/Windows10/Config/s3i/1/Products.ini".MapToLocalPath("C:\\Temp\\subdir"));
            Assert.AreEqual("C:/Temp/subdir/install.elizacorp.com.s3.amazonaws.com/Test/Windows10/Config/s3i/1/Products.ini", "https://install.elizacorp.com.s3.amazonaws.com/Test/Windows10/Config/s3i/1/Products.ini".MapToLocalPath("C:\\Temp\\subdir"));
        }
        [TestMethod]
        public void TestQuoting()
        {
            Assert.AreEqual("", "".Quote(""));
            Assert.AreEqual("abc", "abc".Quote(""));
            Assert.AreEqual("\"a b c\"", "a b c".Quote(null));
            Assert.AreEqual("\"a b c\"", "a b c".Quote(""));
            Assert.AreEqual("\"ab\tc\"", "ab\tc".Quote(""));
            Assert.AreEqual("***a b c***", "a b c".Quote("***"));
        }
    }
}
