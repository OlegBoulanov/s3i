using System;
using NUnit.Framework;

using s3i_lib;

using System.IO;

namespace s3i_lib_tests
{
    public class MsiInfo_Test : PlatformSpecificTestBase
    {
        [Test]
        [Category("Study")]
        [Category("WindowsOnly")]
        public void GetVersion()
        {
            var testDirectoryName = Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory);
            var configurationName = Path.GetFileName(testDirectoryName);
            var projectDirectory = Path.GetDirectoryName(Path.GetDirectoryName(testDirectoryName));
            var msiPath = $"{Path.GetDirectoryName(projectDirectory)}{Path.DirectorySeparatorChar}s3i_setup{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}{configurationName}{Path.DirectorySeparatorChar}s3i.msi";
            using var msi = new MsiInfo(msiPath);
            if (msi.IsOpen)
            {
                foreach(var p in (MsiInfo.StringPropertyType[]) Enum.GetValues(typeof(MsiInfo.StringPropertyType)))
                {
                    Console.WriteLine($"{p}: {msi.GetStringProperty(p, null)}");
                }
            }
            Assert.IsTrue(msi.IsOpen);
            Console.WriteLine(Win32Helper.ErrorMessage(msi.ErrorCode));
            Assert.AreEqual(0, msi.ErrorCode);
            Assert.AreEqual("s3i", msi.GetStringProperty(MsiInfo.StringPropertyType.Subject, null));
            var v1 = System.Diagnostics.FileVersionInfo.GetVersionInfo(msiPath);
            var v2 = System.Diagnostics.FileVersionInfo.GetVersionInfo(@"C:\Program Files (x86)\dotnet\host\fxr\3.0.0\hostfxr.dll");
            //Assert.AreEqual(0, v2.FileMajorPart);
        }
    }
}
