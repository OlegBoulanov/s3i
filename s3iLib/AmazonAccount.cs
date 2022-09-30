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
        public static string RegionName { get; set; } = "us-east-1";
        public static string ProfileName { get; set; } = "default";
        public static Lazy<AWSCredentials> Credentials { get; } = new Lazy<AWSCredentials>(() =>
        {
            if (!new CredentialProfileStoreChain().TryGetProfile(ProfileName, out var profile))
                throw new ApplicationException($"Can't find AWS profile [{ProfileName}]");
            // we need to avoid unnecessary AssumeRole() if running on EC2 and the role is available
            // see https://aws.amazon.com/blogs/security/announcing-an-update-to-iam-role-trust-policy-behavior/
            string instanceRoleName = null;
            try
            {
                var instanceRoles = InstanceProfileAWSCredentials.GetAvailableRoles();
                instanceRoleName = instanceRoles?.FirstOrDefault(role => profile.Options.RoleArn.EndsWith(role));
            }
            catch (Exception)
            {
                // not running on EC2
            }
            var parts = profile.Options.RoleArn.Split('/');     // arn:aws:iam::1234567890:role/name
            var roleName = 2 == parts.Length ? parts[1] : "";   // never null
            return string.IsNullOrWhiteSpace(instanceRoleName) || !roleName.Equals(instanceRoleName)
                ? profile.GetAWSCredentials(profile.CredentialProfileStore) // follow standard chain: https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/creds-assign.html
                : new InstanceProfileAWSCredentials(instanceRoleName);      // use instance refreshed credentials directly (not assuming any roles)
        });

    }
}
