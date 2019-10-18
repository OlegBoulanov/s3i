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
            Assert.IsTrue(new Outcome<bool, string>(true).Succeeded);
            Assert.IsFalse(new Outcome<bool, string>(true).Failed);
            Assert.IsTrue(new Outcome<bool, string>(true).AddErrors("err").Failed);
            Assert.IsTrue(new Outcome<bool, string>(false).Succeeded);
            Assert.IsTrue(new Outcome<bool, string>(false).AddErrors("err").Failed);
            Assert.IsTrue(new Outcome<int, string>(1).Succeeded);
            Assert.IsTrue(new Outcome<int, string>(1).AddErrors("error!").Failed);
            var failed1 = new Outcome<string, string>("Ok").AddErrors("no", "could", "not", "do");
            Assert.IsTrue(failed1.Failed);
            var failed2 = new Outcome<string, string>("RESULT").AddErrors(new List<string> { "could", "not", "do" }).AddErrors("one", "two").AddErrors();
            Assert.That(failed2.Errors.Count, Is.EqualTo(5));
            Assert.IsTrue(failed2.Failed);
            failed2.ResetErrors();
            failed2.Result = "success!!!";
            Assert.IsFalse(failed2.Failed);
            failed2.AddErrors("again");
            Assert.IsFalse(failed2.Succeeded);
            Assert.IsTrue(failed2.Failed);
        }
    }
}
