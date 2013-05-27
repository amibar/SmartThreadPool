using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

using Amib.Threading;

namespace WorkItemsGroupTests
{
	/// <summary>
    /// Summary description for TestWIGConcurrencyChanges.
	/// </summary>
	[TestFixture]
	[Category("TestWIGConcurrencyChanges")]
	public class TestWIGConcurrencyChanges
	{
        [Test]
        public void TestWIGConcurrencyChange1WIG()
        {
            SmartThreadPool smartThreadPool = new SmartThreadPool(10*1000, 1, 0);
            Assert.IsTrue(0 == smartThreadPool.WaitingCallbacks);

            PauseSTP(smartThreadPool);
            Thread.Sleep(100);
            Assert.IsTrue(0 == smartThreadPool.WaitingCallbacks);

            IWorkItemsGroup wig = smartThreadPool.CreateWorkItemsGroup(1);
            wig.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);
            Assert.IsTrue(1 == smartThreadPool.WaitingCallbacks);

            wig.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);
            Assert.IsTrue(1 == smartThreadPool.WaitingCallbacks);

            wig.Concurrency = 2;
            Thread.Sleep(100);
            Assert.IsTrue(2 == smartThreadPool.WaitingCallbacks);

            ResumeSTP(smartThreadPool);
            Thread.Sleep(100);
            Assert.IsTrue(0 == smartThreadPool.WaitingCallbacks);

            PauseSTP(smartThreadPool);
            wig.Concurrency = 1;

            wig.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);
            Assert.IsTrue(1 == smartThreadPool.WaitingCallbacks);

            wig.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);
            Assert.IsTrue(1 == smartThreadPool.WaitingCallbacks);

            ResumeSTP(smartThreadPool);
            Thread.Sleep(100);
            Assert.IsTrue(0 == smartThreadPool.WaitingCallbacks);

            smartThreadPool.Shutdown();
        }

        [Test]
        public void TestWIGConcurrencyChange2WIGs()
        {
            SmartThreadPool smartThreadPool = new SmartThreadPool(10 * 1000, 2, 0);
            Assert.IsTrue(0 == smartThreadPool.WaitingCallbacks);

            PauseSTP(smartThreadPool);
            PauseSTP(smartThreadPool);
            Thread.Sleep(100);
            Assert.IsTrue(0 == smartThreadPool.WaitingCallbacks);

            IWorkItemsGroup wig1 = smartThreadPool.CreateWorkItemsGroup(1);
            IWorkItemsGroup wig2 = smartThreadPool.CreateWorkItemsGroup(1);

            wig1.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);
            Assert.IsTrue(1 == smartThreadPool.WaitingCallbacks);

            wig2.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);
            Assert.IsTrue(2 == smartThreadPool.WaitingCallbacks);

            wig1.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);
            Assert.IsTrue(2 == smartThreadPool.WaitingCallbacks);

            wig2.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);
            Assert.IsTrue(2 == smartThreadPool.WaitingCallbacks);

            wig1.Concurrency = 2;
            Thread.Sleep(100);
            Assert.IsTrue(3 == smartThreadPool.WaitingCallbacks);

            wig2.Concurrency = 2;
            Thread.Sleep(100);
            Assert.IsTrue(4 == smartThreadPool.WaitingCallbacks);

            ResumeSTP(smartThreadPool);
            Thread.Sleep(100);
            Assert.IsTrue(0 == smartThreadPool.WaitingCallbacks);

            PauseSTP(smartThreadPool);
            PauseSTP(smartThreadPool);
            Thread.Sleep(100);
            wig1.Concurrency = 1;

            wig1.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);
            Assert.IsTrue(1 == smartThreadPool.WaitingCallbacks);

            wig2.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);
            Assert.IsTrue(2 == smartThreadPool.WaitingCallbacks);

            ResumeSTP(smartThreadPool);
            Thread.Sleep(100);
            Assert.IsTrue(0 == smartThreadPool.WaitingCallbacks);

            smartThreadPool.Shutdown();
        }

        private void PauseSTP(SmartThreadPool stp)
        {
            _pauseSTP.Reset();
            stp.QueueWorkItem(
                new WorkItemCallback(this.DoPauseSTP),
                null);
        }

        private void ResumeSTP(SmartThreadPool stp)
        {
            _pauseSTP.Set();
        }

        private ManualResetEvent _pauseSTP = new ManualResetEvent(false);
        private object DoPauseSTP(object state)
        {
            _pauseSTP.WaitOne();
            return 1;
        }

        private object DoSomeWork(object state)
        {
            return 1;
        }

        [Test]
        public void TestWIGConcurrencyChange()
        {
            SmartThreadPool smartThreadPool = new SmartThreadPool(10 * 1000, 25, 0);

            IWorkItemsGroup wig = smartThreadPool.CreateWorkItemsGroup(smartThreadPool.MaxThreads);
            bool success = false;

            for (int i = 0; i < 100; ++i)
            {
                wig.QueueWorkItem(new WorkItemCallback(this.DoSomeLongWork), null);
            }

            wig.Concurrency = 1;
            success = WaitForWIGThreadsInUse(wig, 1, 1 * 1000);
            Assert.IsTrue(success);

            wig.Concurrency = 5;
            success = WaitForWIGThreadsInUse(wig, 5, 2 * 1000);
            Assert.IsTrue(success);

            wig.Concurrency = 25;
            success = WaitForWIGThreadsInUse(wig, 25, 4 * 1000);
            Assert.IsTrue(success);

            wig.Concurrency = 10;
            success = WaitForWIGThreadsInUse(wig, 10, 10 * 1000);
            Assert.IsTrue(success);

            smartThreadPool.Shutdown();
        }


        private bool WaitForWIGThreadsInUse(IWorkItemsGroup wig, int maxThreadsCount, int timeout)
        {
            DateTime end = DateTime.Now + new TimeSpan(0, 0, 0, 0, timeout);

            bool success = false;
            while (DateTime.Now <= end && !success)
            {
                success = (wig.InUseThreads == maxThreadsCount);
                Thread.Sleep(10);
            }

            return success;
        }

        private object DoSomeLongWork(object state)
        {
            Thread.Sleep(250);
            return 1;
        }
    }
}
