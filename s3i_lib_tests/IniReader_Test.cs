using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text;

using s3i_lib;

namespace s3i_lib_tests
{
    [TestClass]
    public class IniReader_Test
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            var sections = new Dictionary<string, Dictionary<string, string>>();
            await IniReader.Read(
                new MemoryStream(Encoding.ASCII.GetBytes("\n" +
                "[$products$]\n" +
                "Something = at this path; comment\n" +
                "; comment this completely\n" +
                "[Something]\n" +
                "  prop1 =  value11 ; comment\n" +
                "prop2 = value22")),
                async (section, key, value) =>
                {
                    if (!sections.ContainsKey(section)) sections.Add(section, new Dictionary<string, string>());
                    sections[section][key] = value;
                    await Task.CompletedTask;
                });
            Assert.AreEqual(2, sections.Count);
            Assert.AreEqual("value11", sections["Something"]["prop1"]);
        }
    }
}
