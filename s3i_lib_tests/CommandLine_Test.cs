using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.ComponentModel;
using System.Reflection;

using s3i_lib;

namespace s3i_lib_tests
{
    [TestClass]
    public class CommandLine_Test
    {

        class TestCommandLine : CommandLineBase
        {
            [CommandLine("user profile", "-p", "--profile")]
            public string Profile { get; set; } = "default";
            [CommandLine("verbosity", "-v", "--verbose")]
            public bool Verbose { get; set; } = false;
            [CommandLine("timeout", "-t", "--timeout")]
            public TimeSpan Timeout { get; set; }
            public string ExtraField { get; set; }
        };

        [TestMethod]
        public void Test_CmdAttr()
        {
            var cmd = new TestCommandLine();
            Console.WriteLine($"profile0={cmd.Profile}");
            Assert.AreEqual("default", cmd.Profile);

            cmd.Parse("-p", "xp", "argument", "--verbose", "arg2", "-t", "00:01:23");
            Console.WriteLine($"profile2={cmd.Profile}, verbose={cmd.Verbose}, timeout={cmd.Timeout}");
            Assert.AreEqual("xp", cmd.Profile);
            Assert.AreEqual(2, cmd.Arguments.Count);
        }

    }
}
