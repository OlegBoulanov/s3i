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
   public class Variables : Dictionary<string, string>
    {
        // ${type:name[?default_value]}, use type/name to resolve, or return default_value
        // types supported are: ssm, env, and var (== no type)
        readonly Regex rexVar;
        public Dictionary<string, Func<string, string, string>> Resolvers { get; }
        Lazy<AmazonSimpleSystemsManagementClient> ssm;
        public Variables(Dictionary<string, Func<string, string, string>> resolvers = null, string prefix = @"\$\{", string suffix = @"\}")
        {
            rexVar = new Regex(prefix + @"((?<type>[a-z]+):)?(?<name>[\/A-Za-z0-9-_\.]+)(\?(?<value>[^\}]*))?" + suffix, RegexOptions.Compiled);
            if(!string.IsNullOrWhiteSpace(AmazonAccount.RegionName)) {
                ssm = new Lazy<AmazonSimpleSystemsManagementClient>(()
                    => new AmazonSimpleSystemsManagementClient(AmazonAccount.Credentials.Value, Amazon.RegionEndpoint.GetBySystemName(AmazonAccount.RegionName)));
            }
            Resolvers = resolvers;
        }
        public string Expand(string s, Func<string, string, string, string> resolver)
        {
            s = rexVar.Replace(s, m =>
            {
                var name = m.Groups["name"];
                var type = m.Groups["type"];
                var value = m.Groups["value"];
                if(!name.Success) throw new FormatException($"Invalid variable ref in '{m.Value}'");
                return resolver(name.Value, type.Success? type.Value : null, value.Success ? value.Value : null);
            });
            return s;
        }
        public string Expand(string s)
        {
            var s2 = Expand(s, (name, type, value) =>
            {
                if (!string.IsNullOrWhiteSpace(type))
                {
                    switch (type)
                    {
                        case "ssm":
                            try
                            {
                                return ssm.Value.GetParameterAsync(new GetParameterRequest { Name = name, }).Result.Parameter.Value;
                            }
                            catch (AggregateException x)
                            {
                                if (x.InnerExceptions.Any(xx => xx is ParameterNotFoundException))
                                {
                                    return value ?? throw new ArgumentException($"SSM parameter not found: '{name}'");
                                }
                                throw;
                            }
                        case "env":    // OS agnostic
                            var envar = System.Environment.GetEnvironmentVariable(name);
                            if (null != envar) return envar;
                            return value ?? throw new ArgumentException($"Environment variable not found: '{name}'");
                        case "var":
                            break;  // same as no type
                        default:
                            if(null != Resolvers && Resolvers.TryGetValue(type, out var res)) return res(name, value); 
                            throw new ArgumentException($"Unsupported variable type: '{type}:{name}'");
                    }
                }
                // ${name[?value]}
                if (base.TryGetValue(name, out var val)) return val;
                return val ?? throw new ArgumentException($"Undefined variable: '{name}'");
            });
            return s2 == s ? s2 : Expand(s2);
        }
        /// <summary>Read simple yaml-compatible map of 'name: value' lines</summart>
        public async Task<Variables> Read(Stream stream, Func<string, bool> redefine = null, Func<string, bool> mismatch = null)
        {
            var rexLine = new Regex(@"(?<name>[^:]+):\s*(?<value>.*)\s*");
            using var reader = new StreamReader(stream);
            for (string line; null != (line = await reader.ReadLineAsync().ConfigureAwait(false));)
            {
                var p = line.IndexOfAny(new char [] { '#' });
                if(0 <= p) line = line.Substring(0, p);
                line  = line.Trim();
                if(string.IsNullOrWhiteSpace(line)) continue;
                var m = rexLine.Match(line);
                if(m.Success)
                {
                    var name = m.Groups["name"].Value;
                    var value = Expand(m.Groups["value"].Value);
                    if(!TryAdd(name, value))
                    {
                        if(redefine?.Invoke(name) ?? false) {
                            this[name] = value;
                        }
                    }
                }
                else
                {
                    if(!mismatch?.Invoke(line) ?? false) break;
                }
            }
            return this;
        }
   }
}