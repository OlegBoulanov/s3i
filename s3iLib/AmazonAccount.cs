using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

namespace s3iLib
{
    public static class AmazonAccount
    {
        public static string RegionName { get; set; }
        public static string ProfileName { get; set; } = "default";
        public static Lazy<AWSCredentials> Credentials { get; } = new Lazy<AWSCredentials>(() =>
        {
            if (!new CredentialProfileStoreChain().TryGetAWSCredentials(ProfileName, out var credentials))
                throw new ApplicationException($"Can't find AWS profile [{ProfileName}]");
            return credentials;
        });

    }
}
