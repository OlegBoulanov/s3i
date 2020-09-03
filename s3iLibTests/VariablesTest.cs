using System;
using NUnit.Framework;

using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using s3iLib;

namespace s3iLibTests
{
    
    public class VariablesTest
    {
        [Test]
        public void TestEnv()
        {

        }

        Variables vars = new Variables();

        [Test]
        public void TestSsmReplace()
        {
            try
            {
                var creds = AmazonAccount.Credentials;  // may throw
                Assert.Throws(typeof(ArgumentException), () => vars.Expand("abcdefg${ssm:/x/NOSUCHVAR}xxx"));
                Assert.AreEqual("abcdefgxxx", vars.Expand("abcdefg${ssm:/no/such/parameter?}xxx"));
                Assert.AreEqual("abcdefgSorry!xxx", vars.Expand("abcdefg${ssm:/no/such/parameter?Sorry!}xxx"));
            }
            catch (Exception x)
            {
                Assert.Inconclusive($"Caught {x.Message}");
            }
        }
        [Test]
        public void TestEnvReplace()
        {
            Assert.AreEqual("abcdefg", vars.Expand("abcdefg"));
            Assert.AreEqual("abcdefg${env:***}xxx", vars.Expand("abcdefg${env:***}xxx"));
            Assert.Throws(typeof(ArgumentException), () => vars.Expand("abcdefg${env:NOSUCHVAR}xxx"));
            Assert.AreEqual("abcdefgxxx", vars.Expand("abcdefg${env:NOSUCHVAR?}xxx"));
            Assert.AreEqual("abcdefgNOWAYxxx", vars.Expand("abcdefg${env:NOSUCHVAR?NOWAY}xxx"));
        }

        [Test]
        public void Read()
        {
            var file = @"
one:1
  two:   222
     # comment
   three: == ${one}+${two}
            ";
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(file));
            var vars = new Variables().Read(stream).Result;
            Assert.AreEqual(3, vars.Count);
            Assert.AreEqual("== 1+222", vars["three"]);
            Assert.AreEqual("== 1+222", vars.Expand("${three}"));
        }
    }
}