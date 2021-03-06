﻿using System;
using NUnit.Framework;

using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text;

using s3iLib;

namespace s3iLibTests
{
    
    public class IniReaderTest
    {
        [Test]
        public async Task ReadIniFromMemoryStream()
        {
            var sections = new Dictionary<string, Dictionary<string, string>>();
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes("\n" +
            "[$products$]\n" +
            "Something = at this path; comment\n" +
            "; comment this completely\n" +
            "[Something]\n" +
            "  prop1 =  value11 ; comment\n" +
            "prop2 = value22\n" +
            "prop3 = ;;; comment"));
            await IniReader.Read(stream,
                async (section, key, value) =>
                {
                    if (!sections.ContainsKey(section)) sections.Add(section, new Dictionary<string, string>());
                    sections[section][key] = value;
                    await Task.CompletedTask.ConfigureAwait(false);
                }).ConfigureAwait(false);
            Assert.AreEqual(2, sections.Count);
            Assert.AreEqual("value11", sections["Something"]["prop1"]);
            Assert.AreEqual("value22", sections["Something"]["prop2"]);
            Assert.AreEqual("", sections["Something"]["prop3"]);
        }
    }
}
