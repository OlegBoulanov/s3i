using System;
using System.Collections.Generic;
using System.Linq;
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
        public static string RegionName { get; set; } = "us-east-1";
        public static string ProfileName { get; set; } = "default";
        public static Lazy<AWSCredentials> Credentials { get; } = new Lazy<AWSCredentials>(() =>
        {
            if (!new CredentialProfileStoreChain().TryGetProfile(ProfileName, out var profile))
                throw new ApplicationException($"Can't find AWS profile [{ProfileName}]");
            // we need to avoid unnecessary AssumeRole() if running on EC2 and the role is available
            // see https://aws.amazon.com/blogs/security/announcing-an-update-to-iam-role-trust-policy-behavior/
            string profileRoleName = null;
            try
            {
                var instanceRoles = InstanceProfileAWSCredentials.GetAvailableRoles();
                profileRoleName = instanceRoles?.FirstOrDefault(role => profile.Options.RoleArn.EndsWith(role));
            }
            catch (Exception)
            {
                // not running on EC2
            }
            return string.IsNullOrWhiteSpace(profileRoleName)
                ? profile.GetAWSCredentials(profile.CredentialProfileStore)
                : new InstanceProfileAWSCredentials(profileRoleName);
        });

    }
}
