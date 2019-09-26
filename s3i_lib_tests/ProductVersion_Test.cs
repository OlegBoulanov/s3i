using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using s3i_lib;

namespace s3i_lib_tests
{
    [TestClass]
    public class ProductVersion_Test
    {
        void CanParse(string s)
        {
            Assert.IsTrue(ProductVersion.TryParse(s, out var v1));
        }
        void CantParse(string s)
        {
            Assert.IsFalse(ProductVersion.TryParse(s, out var v1));
        }
        void Less(string one, string two)
        {
            Assert.IsTrue(ProductVersion.TryParse(one, out var v1));
            Assert.IsTrue(ProductVersion.TryParse(two, out var v2));
            Assert.IsTrue(v1.CompareTo(v2) < 0);
        }
        void Equal(string one, string two)
        {
            Assert.IsTrue(ProductVersion.TryParse(one, out var v1));
            Assert.IsTrue(ProductVersion.TryParse(two, out var v2));
            Assert.IsTrue(v1.CompareTo(v2) == 0);
        }
        [TestMethod]
        public void CanParse()
        {
            CanParse("0.0.0.0");
            CanParse("1.2.3.4");
            CanParse("255.255.65535.65535");
        }
        [TestMethod]
        public void CantParse()
        {
            CantParse("");
            CantParse("1");
            CantParse("1.");
            CantParse("1.2");
            CantParse("1.2.");
            CantParse("1.2.3");
            CantParse("1.2.3.");
            CantParse("258.0.1.1");
            CantParse("1.290.2.3");
            CantParse("1.2.78000.4");
            CantParse("1.2.3.85000");
        }
        [TestMethod]
        public void Compare()
        {
            Assert.IsTrue(ProductVersion.TryParse("2.3.4.5", out var v_2_3_4_5));
            Assert.IsTrue(ProductVersion.TryParse("2.3.4.6", out var v_2_3_4_6) && v_2_3_4_5.CompareTo(v_2_3_4_6) == 0);
            Assert.IsTrue(ProductVersion.TryParse("2.3.5.5", out var v_2_3_5_5) && v_2_3_4_5.CompareTo(v_2_3_5_5) < 0);
            Assert.IsTrue(ProductVersion.TryParse("2.4.4.5", out var v_2_4_4_5) && v_2_3_4_5.CompareTo(v_2_4_4_5) < 0);
            Assert.IsTrue(ProductVersion.TryParse("3.3.4.5", out var v_3_3_4_5) && v_2_3_4_5.CompareTo(v_3_3_4_5) < 0);
        }
    }
}
