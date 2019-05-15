using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using s3i_lib;

namespace s3i_lib_tests
{
    [TestClass]
    public class CommandLine_Test
    {
        [TestMethod]
        public void TestMethod1()
        {
            var commandLine = CommandLine.Parse("");
            Assert.AreEqual(0, commandLine.Args.Count);
            Assert.AreEqual(2, commandLine.Options.Count);
            Assert.AreEqual("default", commandLine.Options[CommandLine.OptionType.ProfileName]);
            Assert.AreEqual(Environment.GetEnvironmentVariable("TEMP"), commandLine.Options[CommandLine.OptionType.TempFolder]);
            commandLine = CommandLine.Parse("-p", "profileName", "someArg", "-t", "xxx");
            Assert.AreEqual(1, commandLine.Args.Count);
            Assert.AreEqual("someArg", commandLine.Args[0]);
            Assert.AreEqual(2, commandLine.Options.Count);
            Assert.AreEqual("profileName", commandLine.Options[CommandLine.OptionType.ProfileName]);
            Assert.AreEqual("xxx", commandLine.Options[CommandLine.OptionType.TempFolder]);
        }
    }
}
