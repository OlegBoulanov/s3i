using System;
using NUnit.Framework;

using s3i_lib;

namespace s3i_lib_tests
{
    [TestFixture]
    [Category("Study")]
    public class MsiInfo_Test
    {
        [Test]
        [Category("Study")]
        public void GetVersion()
        {
            var msi = new MsiInfo();
            var ret = msi.Open(@"C:\ProgramData\Eliza\Temp\SipExplorer\SipExplorer.msi");
            Assert.AreEqual(0, ret);
            uint PID_TEMPLATE = 7;
            var template = msi.GetProperty(PID_TEMPLATE);
            Assert.AreEqual("Intel;1033", template);
            var v1 = System.Diagnostics.FileVersionInfo.GetVersionInfo(@"C:\ProgramData\Eliza\Temp\SipExplorer\SipExplorer.msi");
            var v2 = System.Diagnostics.FileVersionInfo.GetVersionInfo(@"C:\Program Files (x86)\dotnet\host\fxr\3.0.0\hostfxr.dll");
            //Assert.AreEqual(0, v2.FileMajorPart);
        }
    }
}
