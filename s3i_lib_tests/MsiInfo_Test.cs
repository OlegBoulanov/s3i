using System;
using NUnit.Framework;

using s3i_lib;

namespace s3i_lib_tests
{
    public class MsiInfo_Test
    {
        [Test]
        [Category("Study")]
        public void GetVersion()
        {
            var msi = new MsiInfo(@"C:\ProgramData\Eliza\Temp\SipExplorer\SipExplorer.msi");
            if (msi)
            {
                foreach(var p in (MsiInfo.StringPropertyType[]) Enum.GetValues(typeof(MsiInfo.StringPropertyType)))
                {
                    Console.WriteLine($"{p}: {msi.GetStringProperty(p, null)}");
                }
            }
            Assert.IsTrue(msi);
            Console.WriteLine(Win32Helper.ErrorMessage(msi.ErrorCode));
            Assert.AreEqual(0, msi.ErrorCode);
            Assert.AreEqual("Eliza SipExplorer 12.6.64.0", msi.GetStringProperty(MsiInfo.StringPropertyType.Subject, null));
            var v1 = System.Diagnostics.FileVersionInfo.GetVersionInfo(@"C:\ProgramData\Eliza\Temp\SipExplorer\SipExplorer.msi");
            var v2 = System.Diagnostics.FileVersionInfo.GetVersionInfo(@"C:\Program Files (x86)\dotnet\host\fxr\3.0.0\hostfxr.dll");
            //Assert.AreEqual(0, v2.FileMajorPart);
        }
    }
}
