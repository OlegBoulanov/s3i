using System;
using NUnit.Framework;

using System.Linq;
using System.Runtime.InteropServices;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public abstract class PlatformSpecificAttributeBase : CategoryAttribute { }

public sealed class WindowsOnlyAttribute : PlatformSpecificAttributeBase { }
public sealed class LinuxOnlyAttribute : PlatformSpecificAttributeBase { }
public sealed class OSXOnlyAttribute : PlatformSpecificAttributeBase { }
public sealed class FreeBSDAttribute : PlatformSpecificAttributeBase { }

public class PlatformSpecificTestBase
{
    // prepare instances for string comparison
    static readonly WindowsOnlyAttribute WindowsOnly = new WindowsOnlyAttribute();
    static readonly LinuxOnlyAttribute LinuxOnly = new LinuxOnlyAttribute();
    static readonly OSXOnlyAttribute OSXOnly = new OSXOnlyAttribute();
    static readonly FreeBSDAttribute FreeBSDOnly = new FreeBSDAttribute();
    [SetUp]
    public void Init()
    {
        var testCategories = TestContext.CurrentContext.Test.Properties["Category"];
        if (testCategories.Any(c => WindowsOnly.Name.Equals(c)) && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.Ignore($"Windows only");
        }
        if (testCategories.Any(c => LinuxOnly.Name.Equals(c)) && !RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Assert.Ignore($"Linux only");
        }
        if (testCategories.Any(c => OSXOnly.Name.Equals(c)) && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Assert.Ignore($"OSX only");
        }
        if (testCategories.Any(c => FreeBSDOnly.Name.Equals(c)) && !RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            Assert.Ignore($"FreeBSD only");
        }
    }
}