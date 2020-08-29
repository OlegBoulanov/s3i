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

        [Test]
        public void TestSsmReplace()
        {
            try
            {
                var creds = AmazonAccount.Credentials;  // may throw
                Assert.Throws(typeof(ArgumentException), () => Variables.Expand("abcdefg${ssm:/x/NOSUCHVAR}xxx"));
                Assert.AreEqual("abcdefgxxx", Variables.Expand("abcdefg${ssm:/no/such/parameter?}xxx"));
                Assert.AreEqual("abcdefgSorry!xxx", Variables.Expand("abcdefg${ssm:/no/such/parameter?Sorry!}xxx"));
            }
            catch (Exception x)
            {
                Assert.Inconclusive($"Caught {x.Message}");
            }
        }
        [Test]
        public void TestEnvReplace()
        {
            Assert.AreEqual("abcdefg", Variables.Expand("abcdefg"));
            Assert.AreEqual("abcdefg${env:***}xxx", Variables.Expand("abcdefg${env:***}xxx"));
            Assert.Throws(typeof(ArgumentException), () => Variables.Expand("abcdefg${env:NOSUCHVAR}xxx"));
            Assert.AreEqual("abcdefgxxx", Variables.Expand("abcdefg${env:NOSUCHVAR?}xxx"));
            Assert.AreEqual("abcdefgNOWAYxxx", Variables.Expand("abcdefg${env:NOSUCHVAR?NOWAY}xxx"));
        }
    }
}