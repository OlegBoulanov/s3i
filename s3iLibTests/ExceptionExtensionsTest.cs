using System;
using System.IO;
using System.Runtime.InteropServices;
using NUnit.Framework;

using s3iLib;

#pragma warning disable CA1303  //warning CA1303: Method 'void ExceptionExtemsionsTest.ErrorMessage()' passes a literal string as parameter 'message' of a call to
namespace s3iLibTests
{

    public class ExceptionExtemsionsTest
    {
        [Test]
        public void ErrorMessage()
        {
            var x = new FileNotFoundException("bla-blah");
            Console.WriteLine($"? {x.Format()}");
            Assert.That(x.Format(), Is.EqualTo($"FileNotFoundException: bla-blah{Environment.NewLine}"));
            var y = new IOException("with inner!", new FileNotFoundException("none", new Exception("more")));
            Console.WriteLine($"? {y.Format(4)}");
            Assert.That(y.Format(2), Is.EqualTo($"IOException: with inner!{Environment.NewLine}  FileNotFoundException: none{Environment.NewLine}    Exception: more{Environment.NewLine}"));
        }
    }
}
#pragma warning restore CA1303
