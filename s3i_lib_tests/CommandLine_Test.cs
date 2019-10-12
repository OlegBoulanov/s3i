using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

using System.ComponentModel;
using System.Reflection;

using s3i_lib;

namespace s3i_lib_tests
{
    
    public class CommandLine_Test
    {

        class TestCommandLine : CommandLineBase
        {
            [CommandLine("user profile", "-p", "--profile <aws-profile-name>")]
            public string Profile { get; set; } = "default";
            [CommandLine("verbosity", "-v", "--verbose")]
            public bool Verbose { get; set; } = false;
            [CommandLine("timeout", "-t", "--timeout")]
            public TimeSpan Timeout { get; set; }
            public string ExtraField { get; set; }
        };

        [Test]
        public void ParseCommandLine()
        {
            var cmd = new TestCommandLine();
            Console.WriteLine($"profile0={cmd.Profile}");
            Assert.AreEqual("default", cmd.Profile);

            cmd.Parse("-p", "xp", "argument", "--verbose", "arg2", "-t", "00:01:23");
            Console.WriteLine($"profile2={cmd.Profile}, verbose={cmd.Verbose}, timeout={cmd.Timeout}");
            Assert.AreEqual("xp", cmd.Profile);
            Assert.AreEqual(2, cmd.Arguments.Count);
        }

        [Test]
        public void Help()
        {
            var cmd = new TestCommandLine();
            var help = cmd.Help();
            Console.WriteLine(help);
        }
    }
}
