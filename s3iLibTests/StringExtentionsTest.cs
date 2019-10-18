using System;
using NUnit.Framework;

using System.IO;
using System.Text.RegularExpressions;

using s3iLib;

namespace s3iLibTests
{
    
    public class StringExtentionsTest
    {
        readonly char x = Path.DirectorySeparatorChar;

        [Test]
        public void RemoveDotSegments()
        {
            Assert.AreEqual(@"a/b\d/e/../g\x.y", StringExtentions.rexDotSegments.Replace(@"a/b\c\../d/e/f/..\../g\x.y", @""));
            Assert.AreEqual(@"a/b\d/g\x.y", StringExtentions.rexDotSegments.Replace(@"a/b\d/e/../g\x.y", @""));
            Assert.AreEqual(@"a/b\d/g\x.y", @"a/b\c\../d/e/f/..\../g\x.y".Replace(StringExtentions.rexDotSegments, @""));
            Assert.AreEqual(@"a/b/c/g/x.y", @"a/b/c/d/e/f/../../../g/x.y".Replace(StringExtentions.rexDotSegments, @""));
            Assert.AreEqual("https://bucket.s3.amazonaws.com/directory1/die2/dir3/file.ext", "https://bucket.s3.amazonaws.com/directory1/die2/dir3/file.ext".RemoveDotSegments());
            Assert.AreEqual("https://bucket.s3.amazonaws.com/directory1/dir3/file.ext", "https://bucket.s3.amazonaws.com/directory1/die2/../dir3/file.ext".RemoveDotSegments());
            Assert.AreEqual("https://another.bucket.com.s3.amazonaws.com/Test/Windows10/Distrib/Installer/9.4.188/Installer.msi", 
                "https://another.bucket.com.s3.amazonaws.com/Test/Windows10/Config/s3i/1/../../../Distrib/Installer/9.4.188/Installer.msi".RemoveDotSegments());
                Assert.AreEqual($"C:{Path.DirectorySeparatorChar}Temp{Path.DirectorySeparatorChar}file.ext", @"C:\Temp\\/\/\file.ext".UnifySlashes());
        }
        [Test]
        public void BuildRelativeUri()
        {
            Assert.AreEqual("https://bucket.s3.amazonaws.com/directory1/another.one", new Uri("https://bucket.s3.amazonaws.com/directory1/dir2/dir3/file.ext").BuildRelativeUri("..\\..\\another.one").ToString());
        }
        [Test]
        public void RebaseUri()
        {
            //Assert.AreEqual("https://bucket/dir1/dir2/dir3/file.ext", new Uri("https://bucket.s3.amazonaws.com/directory1/dir2/dir3/file.ext").BuildRelativeUri("https://bucket/dir1/dir2/dir3/file.ext").ToString());
            //Assert.AreEqual("c:/dir3/file.ext", new Uri("https://bucket/dir1/dir2/dir345/file99.ext88)").BuildRelativeUri("c:/dir3/file.ext").ToString());
            Assert.AreEqual($"{x}dir1{x}dir2{x}dir3/file.ext", Path.Combine(Path.GetDirectoryName("/dir1/dir2/dir345/file99.ext88"), "../dir3/file.ext").RemoveDotSegments());
            Assert.AreEqual("https://bucket/dir1/dir2/dir3/file.ext", new Uri("https://bucket/dir1/dir2/dir345/file99.ext88").BuildRelativeUri("../dir3/file.ext").ToString());
            Assert.AreEqual("https://bucket/dir1/dir2/dir3/", new Uri("https://bucket/dir1/dir2/dir345/file99.ext88").BuildRelativeUri("../dir3/").ToString());
        }

        [Test]
        public void RebaseUri2()
        {
            Assert.AreEqual("https://another.bucket.com.s3.amazonaws.com/Test/Windows10/Distrib/Installer/9.4.188/Installer.msi",
                new Uri("https://another.bucket.com.s3.amazonaws.com/Test/Windows10/Config/s3i/1/Products.ini").BuildRelativeUri("../../../Distrib/Installer/9.4.188/Installer.msi").ToString());
        }
        [Test]
        public void MapToLocalPath()
        {
            Assert.AreEqual($"C:{x}Temp{x}subdir{x}bucket{x}dir1{x}dir2{x}dir345{x}file99.ext88", new Uri("https://bucket/dir1/dir2/dir345/file99.ext88").MapToLocalPath("C:\\Temp\\subdir"));
            Assert.AreEqual($"C:{x}Temp{x}subdir{x}another.bucket.com.s3.amazonaws.com{x}Test{x}Windows10{x}Config{x}s3i{x}1{x}Products.ini", new Uri("https://another.bucket.com.s3.amazonaws.com/Test/Windows10/Config/s3i/1/Products.ini").MapToLocalPath("C:\\Temp\\subdir"));
            Assert.AreEqual($"C:{x}Temp{x}subdir{x}another.bucket.com.s3.amazonaws.com{x}Test{x}Windows10{x}Config{x}s3i{x}1{x}Products.ini", new Uri("https://another.bucket.com.s3.amazonaws.com/Test/Windows10/Config/s3i/1/Products.ini").MapToLocalPath("C:\\Temp\\subdir"));
        }
        [Test]
        public void TestQuoting()
        {
            Assert.AreEqual("\"\"", "".Quote(null));
            Assert.AreEqual("\"\"", "".Quote(""));
            Assert.AreEqual("abc", "abc".Quote(""));
            Assert.AreEqual("\"a b c\"", "a b c".Quote(null));
            Assert.AreEqual("\"a b c\"", "a b c".Quote(""));
            Assert.AreEqual("\"ab\tc\"", "ab\tc".Quote(""));
            Assert.AreEqual("***a b c***", "a b c".Quote("***"));
        }
    }
}
