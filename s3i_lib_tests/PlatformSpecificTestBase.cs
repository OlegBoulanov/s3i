using System;
using NUnit.Framework;

using System.Linq;
using System.Runtime.InteropServices;

public class PlatformSpecificTestBase
{
    [SetUp]
    public void Init()
    {
        var testCategories = TestContext.CurrentContext.Test.Properties["Category"];
        if (testCategories.Contains("WindowsOnly") && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.Inconclusive($"-- Can run this test on Windows only");
        }
        if (testCategories.Contains("LinuxOnly") && !RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Assert.Inconclusive($"-- Can run this test on Linux only");
        }
        if (testCategories.Contains("OSXOnly") && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Assert.Inconclusive($"-- Can run this test on OSX only");
        }
        if (testCategories.Contains("FreeBSDOnly") && !RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            Assert.Inconclusive($"-- Can run this test on FreeBSD only");
        }
    }
}