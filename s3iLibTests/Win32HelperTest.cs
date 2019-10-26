using System;
using NUnit.Framework;

using s3iLib;

namespace s3iLibTests
{
    public class Win32HelperTest : PlatformDependentTestBase
    {
        [Test]
        [LinuxOnly]
        public void ErrorMessageLinux()
        {
            Assert.AreEqual("No such file or directory", Win32Helper.ErrorMessage(2));
            Assert.AreEqual("Unknown error 12017", Win32Helper.ErrorMessage(12017));
            Assert.AreEqual("Unknown error 1603", Win32Helper.ErrorMessage(1603));
        }
        [Test]
        [WindowsOnly]
        public void ErrorMessageWindows()
        {
            Assert.AreEqual("The system cannot find the file specified.", Win32Helper.ErrorMessage(2));
            Assert.AreEqual("Unknown error (0x2ef1)", Win32Helper.ErrorMessage(12017));
            Assert.AreEqual("Fatal error during installation.", Win32Helper.ErrorMessage(1603));
        }
    }
}
