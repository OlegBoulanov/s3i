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
        static readonly Regex varableReference = new Regex(@"\$\{(?:(?<type>ssm:)(?:(?<name>(\/[A-Za-z0-9-_]+)+))|(?<type>env:)(?<name>[A-Za-z0-9_]+))(?:\?(?<value>[^\}]*))?\}", RegexOptions.Compiled);
        static readonly Lazy<AmazonSimpleSystemsManagementClient> ssm = new Lazy<AmazonSimpleSystemsManagementClient>(() 
            => new AmazonSimpleSystemsManagementClient(AmazonAccount.Credentials.Value, Amazon.RegionEndpoint.GetBySystemName(AmazonAccount.RegionName)));
        public static string Expand(string s)
        {
            s = Environment.ExpandEnvironmentVariables(s);  // OS specific
            s = varableReference.Replace(s, m =>
            {
                var type = m.Groups["type"];
                var name = m.Groups["name"];
                if (type.Success && name.Success)
                {
                    var value = m.Groups["value"];
                    switch (type.Value)
                    {
                        case "ssm:":
                            try
                            {
                                return ssm.Value.GetParameterAsync(new GetParameterRequest { Name = name.Value, }).Result.Parameter.Value;
                            }
                            catch (AggregateException x)
                            {
                                if (x.InnerExceptions.Any(xx => xx is ParameterNotFoundException))
                                {
                                    if (value.Success) return value.Value;
                                    throw new ArgumentException($"SSM parameter not found: {name}");
                                }
                                throw;
                            }
                        case "env:":    // OS agnostic
                            var envar = Environment.GetEnvironmentVariable(name.Value);
                            if(null != envar) return envar;
                            if (value.Success) return value.Value;
                            throw new ArgumentException($"Environment Variable not found: {name}");
                    }
                }
                else
                {
                    // fall through
                }
                throw new ArgumentException($"Invalid variable reference in: '{m.Value}'");
            });
            return s;
        }
    }
}