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
        // ${type:name[?default_value]}, use type/name to resolve, or return default_value
        // type supported are: ssm and env
        static readonly Regex varableReference = new Regex(@"\$\{(?:(?<type>ssm:)(?:(?<name>(\/[a-z0-9-_]+)+))|(?<type>env:)(?<name>[A-Za-z0-9_]+))(?:\?(?<value>[^\}]*))?\}", RegexOptions.Compiled);
        static readonly Lazy<AmazonSimpleSystemsManagementClient> ssm = new Lazy<AmazonSimpleSystemsManagementClient>(() 
            => new AmazonSimpleSystemsManagementClient(AmazonAccount.Credentials.Value, Amazon.RegionEndpoint.GetBySystemName(AmazonAccount.RegionName)));
        public static string Expand(string s)
        {
            s = Environment.ExpandEnvironmentVariables(s);  // OS specific
            s = varableReference.Replace(s, m =>
            {
                var type = m.Groups["type"]?.Value;
                var name = m.Groups["name"]?.Value;
                var value = m.Groups["value"]?.Value;
                switch (type)
                {
                    case "ssm:":
                        try {
                            return ssm.Value.GetParameterAsync(new GetParameterRequest { Name = name, }).Result.Parameter.Value;
                        } catch(AggregateException x) {
                            if(x.InnerExceptions.Any(xx => xx is ParameterNotFoundException)) {
                                return value;
                            }
                            throw;
                        }
                    case "env:":
                        return Environment.GetEnvironmentVariable(name) ?? value;    // OS agnostic
                }
                throw new ArgumentException($"Invalid variable type ({type}) in: '{m.Value}', only ssm: or env: supported");
            });
            return s;
        }
    }
}