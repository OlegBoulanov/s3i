using System;
using System.IO;
using System.Runtime.InteropServices;
using NUnit.Framework;

using s3i_lib;

namespace s3i_lib_tests
{

    public class ExceptionExtemsions_Test
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