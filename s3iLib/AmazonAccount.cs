using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

namespace s3iLib
{
    public static class AmazonAccount
    {
        static ICredentialProfileSource credentialProfileSource => new SharedCredentialsFile();
        // these can be set from CommandLine
        public static string RegionName { get; set; } = "us-east-1";
        public static string ProfileName { get; set; } = "default";
        public static Lazy<AWSCredentials> Credentials => new Lazy<AWSCredentials>(() =>
        {
            if (credentialProfileSource.TryGetProfile(ProfileName, out var profile)
            && AWSCredentialsFactory.TryGetAWSCredentials(profile, credentialProfileSource, out var credentials))
                return credentials;
            if ("default" != ProfileName)
                throw new ApplicationException($"Can't locate profile [{ProfileName}]");
            return new InstanceProfileAWSCredentials();
        });
    }
}
