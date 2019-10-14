using System;
using NUnit.Framework;

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace s3i_lib__Study
{
    [TestFixture]
    [Category("Study")]
    public class __WhatTypeIsAsyncLambda__
    {

        class Processor
        {
            // These methods can even have the same name, I named them differently for illutrative purposes only
            public static async Task ProcessAction(Action action) { await Task.Run(() => { action(); }); }
            public static async Task ProcessFunc(Func<Task> func) { await Task.Run(async () => { await func(); }); }
        }

        const int millisecondsToWait = 1000;

        [Test]
        //[Category("Study")]
        public async Task Test_SyncAction()
        {
            var watch = Stopwatch.StartNew();
            await Processor.ProcessAction(() => { Thread.Sleep(millisecondsToWait); });
            Assert.IsTrue(millisecondsToWait / 2 < watch.Elapsed.TotalMilliseconds);
        }

        [Test]
        //[Category("Study")]
        public async Task Test_AsyncFunc()
        {
            var watch = Stopwatch.StartNew();
            await Processor.ProcessFunc(async () => { await Task.Run(() => { Thread.Sleep(millisecondsToWait); }); });
            Assert.IsTrue(millisecondsToWait / 2 < watch.Elapsed.TotalMilliseconds);
        }

        [Test]
        //[Category("Study")]
        public async Task Test_AsyncAction()
        {
            var watch = Stopwatch.StartNew();
            // ***** As it turns out, async lambda (which type should be Func<args, Task>)) can be implicitly cast to ... Action<args> *****
            await Processor.ProcessAction(async () => { await Task.Run(() => { Thread.Sleep(millisecondsToWait); }); });
            // ... so await always exits prematurely
            Assert.IsTrue(watch.Elapsed.TotalMilliseconds < millisecondsToWait / 2);
        }
        void AsyncLambdaTypeCasting()
        {
            // simplest sync lambda is just an action
            Action syncActionsSimple = () => { Task.Delay(100); };
            //Func<Task> asyncAction = () => { Task.Delay(100); };         // error CS1643: Not all code paths return a value in lambda expression of type 'Func<Task>' (needs async/await)
            // Syntactically Ok, but the same async lambda expression can be cast to different types!
            Func<Task> asyncFunc = async () => { await Task.Delay(100); }; // this is Ok, we can await on it
            Action asyncActionXX = async () => { await Task.Delay(100); }; // *** this is DANGEROUS, because the cast may happen implicitly, and await would return immediately ***
            // At the same time, implicit typing, or any type casting is not possible:
            //var asyncWhaaaaaat = async () => { await Task.Delay(100); }; // error CS0815: Cannot assign lambda expression to an implicitly-typed variable
            //Action asyncAction = asyncFunc;                              // error CS0029: Cannot implicitly convert type 'System.Func<System.Threading.Tasks.Task>' to 'System.Action'
            //Action asyncAction = (Action) asyncFunc;                     // error CS0030: Cannot convert type 'System.Func<System.Threading.Tasks.Task>' to 'System.Action'
            // See also: http://thebillwagner.com/Blog/Item/2016-05-18-DoasynclambdasreturnTasks
        }
    }
}
