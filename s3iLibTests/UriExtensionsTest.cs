using System;
using NUnit.Framework;

using System.IO;
using System.Diagnostics.Contracts;

using s3iLib;

namespace s3iLibTests
{
    
    public class UriExtensionsTest
    {
        [Test]
        public void Compare()
        {
            Assert.AreEqual(0, new Uri("http://host/uri1").CompareSemanticVersion(new Uri("https://host2/other")));
            Assert.AreEqual(-1, new Uri("http://host/uri1").CompareSemanticVersion(new Uri("https://host2/1.2.3-develop")));
            Assert.AreEqual(+1, new Uri("http://host/3.4.5-master").CompareSemanticVersion(new Uri("https://host2/other")));
            Assert.AreEqual(-1, new Uri("http://host/3.4.4").CompareSemanticVersion(new Uri("https://host2/sub3/3.4.5")));
            Assert.AreEqual(+1, new Uri("http://host/3.4.5").CompareSemanticVersion(new Uri("https://host2/other/3/4/4")));
        }
        [Test]
        public void Varieties()
        {
            // URI
            var uri = new Uri("https://server.com/folder/file.ext");
            Assert.AreEqual("https", uri.Scheme);
            Assert.AreEqual("server.com", uri.Host);
            Assert.AreEqual(UriHostNameType.Dns, uri.HostNameType);
            Assert.AreEqual($"/folder/file.ext", uri.AbsolutePath);
            Assert.Throws(typeof(FormatException), () => uri.GetAbsoluteFilePath());
            // File URI
            var fileUri = new Uri("file:///c:\\folder\\file.ext");
            Assert.AreEqual("file", fileUri.Scheme);
            Assert.AreEqual(string.Empty, fileUri.Host);
            Assert.AreEqual(UriHostNameType.Basic, fileUri.HostNameType);
            Assert.AreEqual($"c:{Path.DirectorySeparatorChar}folder{Path.DirectorySeparatorChar}file.ext", fileUri.AbsolutePath);
            Assert.AreEqual($"c:{Path.DirectorySeparatorChar}folder{Path.DirectorySeparatorChar}file.ext", fileUri.GetAbsoluteFilePath());
            // Local file
            var rootedFile = new Uri("c:\\folder/file.ext");
            Assert.AreEqual("file", rootedFile.Scheme);
            Assert.AreEqual(string.Empty, rootedFile.Host);
            Assert.AreEqual(UriHostNameType.Basic, rootedFile.HostNameType);
            Assert.AreEqual($"c:{Path.DirectorySeparatorChar}folder{Path.DirectorySeparatorChar}file.ext", rootedFile.AbsolutePath);
            Assert.AreEqual($"c:{Path.DirectorySeparatorChar}folder{Path.DirectorySeparatorChar}file.ext", rootedFile.GetAbsoluteFilePath());
            // UNC
            var unc = new Uri("file:///\\\\server\\folder\\file.ext");
            Assert.AreEqual("file", unc.Scheme);
            Assert.AreEqual("server", unc.Host);
            Assert.AreEqual(UriHostNameType.Dns, unc.HostNameType);
            Assert.AreEqual($"{Path.DirectorySeparatorChar}folder{Path.DirectorySeparatorChar}file.ext", unc.AbsolutePath);        
            Assert.AreEqual($"\\\\{unc.Host}{Path.DirectorySeparatorChar}folder{Path.DirectorySeparatorChar}file.ext", unc.GetAbsoluteFilePath());        
           // UNC file
            var uncFile = new Uri("\\\\server\\folder\\file.ext");
            Assert.AreEqual("file", uncFile.Scheme);
            Assert.AreEqual("server", unc.Host);
            Assert.AreEqual(UriHostNameType.Dns, uncFile.HostNameType);
            Assert.AreEqual($"{Path.DirectorySeparatorChar}folder{Path.DirectorySeparatorChar}file.ext", uncFile.AbsolutePath);
            Assert.AreEqual($"\\\\{unc.Host}{Path.DirectorySeparatorChar}folder{Path.DirectorySeparatorChar}file.ext", uncFile.GetAbsoluteFilePath());
            // Relative file is not a URI
            Assert.Throws(typeof(System.UriFormatException), () =>
            {
                var relativeFile = new Uri("folder\\file.ext");
            });
            Assert.Throws(typeof(System.UriFormatException), () =>
            {
                var relativeFile = new Uri("\\folder\\file.ext");
            });
        }
    }
}
