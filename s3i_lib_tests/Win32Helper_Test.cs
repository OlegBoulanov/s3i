﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using s3i_lib;

namespace s3i_lib_tests
{
    [TestClass]
    public class Win32Helper_Test
    {
        [TestMethod]
        public void ErrorMessage()
        {
            Assert.AreEqual("The system cannot find the file specified", Win32Helper.ErrorMessage(2));
            Assert.AreEqual("Fatal error during installation", Win32Helper.ErrorMessage(1603));
            Assert.AreEqual("Unknown error (0x2ef1)", Win32Helper.ErrorMessage(12017));
        }
    }
}