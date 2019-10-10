using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

using s3i_lib;

namespace s3i_lib_tests
{
    
    public class Win32Helper_Test
    {
        [Test]
        public void ErrorMessage()
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                Assert.AreEqual("No such file or directory", Win32Helper.ErrorMessage(2));
                Assert.AreEqual("Unknown error 12017", Win32Helper.ErrorMessage(12017));
                Assert.AreEqual("Unknown error 1603", Win32Helper.ErrorMessage(1603));
            }
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                Assert.AreEqual("The system cannot find the file specified", Win32Helper.ErrorMessage(2));
                Assert.AreEqual("Unknown error (0x2ef1)", Win32Helper.ErrorMessage(12017));
                Assert.AreEqual("Fatal error during installation", Win32Helper.ErrorMessage(1603));
            }
        }
    }
}
