using System;
using System.Collections.Generic;
using NUnit.Framework;

using s3iLib;

namespace s3iLibTests
{
    public class OutcomeTest
    {
        [Test]
        public void Outcome()
        {
            Assert.IsTrue(Outcome<bool, string>.Success(true).Succeeded);
            Assert.IsTrue(Outcome<bool, string>.Success(false).Succeeded);
            Assert.IsTrue(Outcome<bool, string>.Failure(true).Failed);
            Assert.IsTrue(Outcome<bool, string>.Failure(false).Failed);
            Assert.IsTrue(Outcome<int, string>.Success(1).Succeeded);
            Assert.IsTrue(Outcome<int, string>.Failure(1).Failed);
            var failed1 = Outcome<string, string>.Failure("no", "could", "not", "do");
            Assert.IsTrue(failed1.Failed);
            var failed2 = Outcome<string, string>.Failure("RESULT").AddErrors(new List<string> { "could", "not", "do" }).AddErrors("one", "two").AddErrors();
            Assert.That(failed2.Errors.Count, Is.EqualTo(5));
            Assert.IsTrue(failed2.Failed);
            Assert.IsFalse(Outcome<string, string>.Failure("no", "whatever").Succeeded);
            Assert.IsFalse(Outcome<string, string>.Success("yes").Failed);
            Assert.IsTrue(Outcome<string, string>.Success("yes").Succeeded);
            failed2 = "success!!!";
            Assert.IsFalse(failed2.Failed);
            failed2.AddErrors("again");
            Assert.IsFalse(failed2.Succeeded);
            Assert.IsTrue(failed2.Failed);
        }
    }
}
