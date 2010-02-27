﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Autofac.Tests
{
    [TestFixture]
    public class ConcurrentTests
    {
        [Test]
        public void WhenTwoThreadsInitialiseASharedInstanceSimultaneouslyViaChildLifetime_OnlyOneInstanceIsActivated()
        {
            int activationCount = 0;
            var results = new ConcurrentBag<object>();
            var exceptions = new ConcurrentBag<Exception>();

            var builder = new ContainerBuilder();
            builder.Register(c =>
                {
                    Interlocked.Increment(ref activationCount);
                    Thread.Sleep(500);
                    return new object();
                })
                .SingleInstance();

            var container = builder.Build();

            ThreadStart work = () => {
                 try
                 {
                     var o = container.BeginLifetimeScope().Resolve<object>();
                     results.Add(o);
                 }
                 catch (Exception ex)
                 {
                     exceptions.Add(ex);
                 }
            };

            var t1 = new Thread(work);
            var t2 = new Thread(work);
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            Assert.AreEqual(1, activationCount);
            Assert.IsEmpty(exceptions);
            Assert.AreEqual(1, results.Distinct().Count());
        }
    }
}