﻿using System;
using NUnit.Framework;

using s3iLib;

namespace s3iLibTests
{
    
    public class ProductPropertiesDictionaryTest
    {
        [Test]
        public void Compare()
        {
            Assert.AreEqual(new ProductPropertiesDictionary(), new ProductPropertiesDictionary { });
            Assert.AreEqual(new ProductPropertiesDictionary().GetHashCode(), new ProductPropertiesDictionary { }.GetHashCode());
            var pp10 = new ProductPropertiesDictionary { { "prop1", "value1" } };
            var pp11 = new ProductPropertiesDictionary { { "prop1", "value1" } };
            Assert.AreEqual(pp10.GetHashCode(), pp11.GetHashCode());
            Assert.AreEqual(pp10, pp11);
            var pp20 = new ProductPropertiesDictionary { { "prop1", "value1" }, { "prop2", "value2" } };
            var pp21 = new ProductPropertiesDictionary { { "prop1", "value1" }, { "prop2", "value2" } };
            Assert.AreEqual(pp20.GetHashCode(), pp21.GetHashCode());
            Assert.AreEqual(pp20, pp21);
            Assert.AreNotEqual(pp10, pp20);
            Assert.IsTrue(pp10.Equals(pp11));
            Assert.IsTrue(pp20.Equals(pp21));
            Assert.IsFalse(pp10.Equals(pp20));
        }
    }
}
