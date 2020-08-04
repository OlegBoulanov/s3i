using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using System.IO;
using System.Net;
//using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Amazon.S3.Util;

namespace s3iLib
{
    public static class Variables
    {
        static readonly Regex varableReference = new Regex(@"\$\{(?:(ssm:)(?:((\/[a-z0-9-_]+)+))|(env:)([A-Za-z0-9_]+))\}", RegexOptions.Compiled);
        static readonly Lazy<AmazonSimpleSystemsManagementClient> ssm = new Lazy<AmazonSimpleSystemsManagementClient>(() 
            => new AmazonSimpleSystemsManagementClient(AmazonAccount.Credentials.Value, Amazon.RegionEndpoint.GetBySystemName(AmazonAccount.RegionName)));
        public static string Expand(string s)
        {
            s = Environment.ExpandEnvironmentVariables(s);  // OS specific
            s = varableReference.Replace(s, m =>
            {
                var type = m.Groups[1].Value;
                var name = m.Groups[2].Value;
                switch (type)
                {
                    case "ssm:":
                        return ssm.Value.GetParameterAsync(new GetParameterRequest { Name = name, }).Result.Parameter.Value;
                    case "env:":
                        return Environment.GetEnvironmentVariable(name);    // OS agnostic
                }
                throw new ArgumentException($"Invalid variable type ({type}) in: '{m.Value}'");
            });
            return s;
        }
    }
}