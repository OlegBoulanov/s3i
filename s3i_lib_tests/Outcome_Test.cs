using System;
using System.Collections.Generic;
using NUnit.Framework;

using s3i_lib;

namespace s3i_lib_tests
{
    class Outcome_Test
    {
        [Test]
        public void Outcome()
        {
            var failed1 = Outcome<string, string>.Failure("no", "could", "not", "do");
            Assert.IsTrue(failed1.Failed);
            var failed2 = Outcome<string, string>.Failure("no").AddErrors(new List<string> { "could", "not", "do" }).AddErrors("one", "two").AddErrors();
            Assert.That(failed2.Errors.Count, Is.EqualTo(6));
            Assert.IsTrue(failed2.Failed);
            Assert.IsFalse(Outcome<string, string>.Failure("no", "whatever").Succeeded);
            Assert.IsFalse(Outcome<string, string>.Success("yes").Failed);
            Assert.IsTrue(Outcome<string, string>.Success("yes").Succeeded);
            failed2 = "success!!!";
            Assert.IsFalse(failed2.Failed);
        }
    }
}
