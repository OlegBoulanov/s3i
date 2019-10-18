using System;
using NUnit.Framework;

using s3iLib;

namespace s3iLibTests
{
    
    public class SemanticVersionTest
    {
        void CanParse(string s)
        {
            Assert.IsTrue(SemanticVersion.TryParse(s, out var v1));
        }
        void CantParse(string s)
        {
            Assert.IsFalse(SemanticVersion.TryParse(s, out var v1));
        }
        void Less(string one, string two)
        {
            Assert.IsTrue(SemanticVersion.TryParse(one, out var v1));
            Assert.IsTrue(SemanticVersion.TryParse(two, out var v2));
            Assert.IsTrue(v1.CompareTo(v2) < 0);
        }
        void Equal(string one, string two)
        {
            Assert.IsTrue(SemanticVersion.TryParse(one, out var v1));
            Assert.IsTrue(SemanticVersion.TryParse(two, out var v2));
            Assert.IsTrue(v1.CompareTo(v2) == 0);
        }
/*
0.0.4
1.2.3
10.20.30
1.1.2-prerelease+meta
1.1.2+meta
1.1.2+meta-valid
1.0.0-alpha
1.0.0-beta
1.0.0-alpha.beta
1.0.0-alpha.beta.1
1.0.0-alpha.1
1.0.0-alpha0.valid
1.0.0-alpha.0valid
1.0.0-alpha-a.b-c-somethinglong+build.1-aef.1-its-okay
1.0.0-rc.1+build.1
2.0.0-rc.1+build.123
1.2.3-beta
10.2.3-DEV-SNAPSHOT
1.2.3-SNAPSHOT-123
1.0.0
2.0.0
1.1.7
2.0.0+build.1848
2.0.1-alpha.1227
1.0.0-alpha+beta
1.2.3----RC-SNAPSHOT.12.9.1--.12+788
1.2.3----R-S.12.9.1--.12+meta
1.2.3----RC-SNAPSHOT.12.9.1--.12
1.0.0+0.build.1-rc.10000aaa-kk-0.1
99999999999999999999999.999999999999999999.99999999999999999
1.0.0-0A.is.legal
*/
        [Test]
        public void CanParse()
        {

            CanParse("1.2.3");
            CanParse("10.20.30");
            CanParse("1.1.2-prerelease+meta");
            CanParse("1.1.2+meta");
            CanParse("1.1.2+meta-valid");
            CanParse("1.0.0-alpha.0valid");
            CanParse("1.0.0-alpha.0valid");
            CanParse("1.0.0-alpha-a.b-c-somethinglong+build.1-aef.1-its-okay");
            CanParse("1.0.0-rc.1+build.1");
            CanParse("99999999999999999999999.999999999999999999.99999999999999999");
        }
/*
1
1.2
1.2.3-0123
1.2.3-0123.0123
1.1.2+.123
+invalid
-invalid
-invalid+invalid
-invalid.01
alpha
alpha.beta
alpha.beta.1
alpha.1
alpha+beta
alpha_beta
alpha.
alpha..
beta
1.0.0-alpha_beta
-alpha.
1.0.0-alpha..
1.0.0-alpha..1
1.0.0-alpha...1
1.0.0-alpha....1
1.0.0-alpha.....1
1.0.0-alpha......1
1.0.0-alpha.......1
01.1.1
1.01.1
1.1.01
1.2
1.2.3.DEV
1.2-SNAPSHOT
1.2.31.2.3----RC-SNAPSHOT.12.09.1--..12+788
1.2-RC-SNAPSHOT
-1.0.3-gamma+b7718
+justmeta
9.8.7+meta+meta
9.8.7-whatever+meta+meta
99999999999999999999999.999999999999999999.99999999999999999----RC-SNAPSHOT.12.09.1--------------------------------..12
*/
        [Test]
        public void CantParse()
        { 

            CantParse("");
            CantParse("1.2");
            CantParse("1.2.3-0123");
            CantParse("1.2.3-0123.0123");
            CantParse("1.1.2+.123");
            CantParse("1.2.31.2.3----RC-SNAPSHOT.12.09.1--..12+788");
        }
        [Test]
        public void Compare()
        {
            Assert.IsTrue(SemanticVersion.TryParse("2.3.4-5", out var v_2_3_4_5));
            Assert.IsTrue(SemanticVersion.TryParse("2.3.4-6", out var v_2_3_4_6) && v_2_3_4_5.CompareTo(v_2_3_4_6) < 0);
            Assert.IsTrue(SemanticVersion.TryParse("2.3.5-5", out var v_2_3_5_5) && v_2_3_4_5.CompareTo(v_2_3_5_5) < 0);
            Assert.IsTrue(SemanticVersion.TryParse("2.4.4-5", out var v_2_4_4_5) && v_2_3_4_5.CompareTo(v_2_4_4_5) < 0);
            Assert.IsTrue(SemanticVersion.TryParse("3.3.4-5", out var v_3_3_4_5) && v_2_3_4_5.CompareTo(v_3_3_4_5) < 0);
        }
        [Test]
        public void Compare2()
        {
            Assert.IsTrue(SemanticVersion.TryParse("2.3.4", out var v_2_3_4));
            Assert.IsTrue(SemanticVersion.TryParse("2.3.4-a", out var v_2_3_4_a) && v_2_3_4.CompareTo(v_2_3_4_a) > 0);
            Assert.IsTrue(SemanticVersion.TryParse("2.3.4-b", out var v_2_3_4_b) && v_2_3_4_a.CompareTo(v_2_3_4_b) < 0);
            Assert.IsTrue(SemanticVersion.TryParse("2.3.5-5", out var v_2_3_5_5) && v_2_3_4.CompareTo(v_2_3_5_5) < 0);
            Assert.IsTrue(SemanticVersion.TryParse("2.4.4", out var v_2_4_4) && v_2_3_4.CompareTo(v_2_4_4) < 0);
            Assert.IsTrue(SemanticVersion.TryParse("3.3.4-5", out var v_3_3_4_5) && v_2_3_4.CompareTo(v_3_3_4_5) < 0);
        }
        [Test]
        public void CompareNumerics()
        {
            Less("2.3.4", "2.3.5");
            Less("2.3.4", "2.4.4");
            Less("2.3.4", "3.3.4");
            Less("2.3.4", "2.3.15");
            Less("2.3.4", "2.13.4");
            Less("2.3.4", "12.3.4");
        }
        [Test]
        public void ComparePrereleases()
        {
            Less("2.3.4-pre", "2.3.4");
            Equal("2.3.4", "2.3.4");
            Equal("2.3.4+meta", "2.3.4");
            Equal("2.3.4", "2.3.4+meta");
            Less("2.3.4-v12", "2.3.4-v20");
            Less("2.3.4-v.12", "2.3.4-v.20");
            Less("2.3.4-v.10", "2.3.4-v.20");
        }
        [Test]
        public void FromProductVersion()
        {
            Assert.IsTrue(ProductVersion.TryParse("1.2.3.4", out var v1234));
            Assert.IsTrue(ProductVersion.TryParse("1.2.3.5", out var v1235));
            Assert.IsTrue(0 == SemanticVersion.From(v1234).CompareTo(SemanticVersion.From(v1235)));
            Assert.IsTrue(ProductVersion.TryParse("1.2.4.5", out var v1245));
            Assert.IsTrue(0 > SemanticVersion.From(v1234).CompareTo(SemanticVersion.From(v1245)));
        }
    }
}
