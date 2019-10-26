using System;
using System.IO;
using NUnit.Framework;

using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

using System.Runtime.InteropServices;

namespace s3iLibTests
{
    public class AwsProfileDependentTestBase
    {
        [SetUp]
        public void Init()
        { 
            var homePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") : Environment.GetEnvironmentVariable("HOME");
            var credentialsPath = $"{homePath}{Path.DirectorySeparatorChar}.aws{Path.DirectorySeparatorChar}credentials";
            if (!File.Exists(credentialsPath)) Assert.Ignore($"Missing {credentialsPath}");
        }
    }
}