using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace s3iLib
{
    public class MockInstaller : Installer
    {
        public static int ReturnResult { get; set; } = -1;
        public override int Install(Uri uri, ProductPropertiesDictionary props, string extraArgs, bool dryrun, TimeSpan timeout)
        {
            Console.WriteLine($"? ({nameof(MockInstaller)}) Install({uri}): not supported");
            return ReturnResult;
        }
        public override int Uninstall(Uri uri, string extraArgs, bool dryrun, TimeSpan timeout)
        {
            Console.WriteLine($"? ({nameof(MockInstaller)}) Unnstall({uri}): not supported");
            return ReturnResult;
        }
    }
}