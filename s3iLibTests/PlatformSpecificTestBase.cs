using System;
using NUnit.Framework;

using System.Linq;
using System.Runtime.InteropServices;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public abstract class PlatformSpecificAttribute : CategoryAttribute { }

public sealed class WindowsOnlyAttribute : PlatformSpecificAttribute { }
public sealed class LinuxOnlyAttribute : PlatformSpecificAttribute { }
public sealed class OSXOnlyAttribute : PlatformSpecificAttribute { }
public sealed class FreeBSDAttribute : PlatformSpecificAttribute { }

public class PlatformDependentTestBase
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
            Assert.Ignore(WindowsOnly.Name);
        }
        if (testCategories.Any(c => LinuxOnly.Name.Equals(c)) && !RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Assert.Ignore(LinuxOnly.Name);
        }
        if (testCategories.Any(c => OSXOnly.Name.Equals(c)) && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Assert.Ignore(OSXOnly.Name);
        }
        if (testCategories.Any(c => FreeBSDOnly.Name.Equals(c)) && !RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            Assert.Ignore(FreeBSDOnly.Name);
        }
    }
}