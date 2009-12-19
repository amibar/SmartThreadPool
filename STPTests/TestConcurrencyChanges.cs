using System;
using System.Threading;
using System.Diagnostics;

using NUnit.Framework;

using Amib.Threading;

namespace SmartThreadPoolTests
{
	/// <summary>
	/// Summary description for TestConcurrencyChanges.
	/// </summary>
	[TestFixture]
	[Category("TestConcurrencyChanges")]
	public class TestConcurrencyChanges
	{
	    /// <summary>
        /// Example of waiting for idle
        /// </summary>
        [Test]
        public void TestMaxThreadsChange()
        {
            SmartThreadPool smartThreadPool = new SmartThreadPool(1 * 1000, 1, 0);

            for (int i = 0; i < 100; ++i)
            {
                smartThreadPool.QueueWorkItem(
                    new WorkItemCallback(this.DoSomeWork),
                    null);
            }

            bool success = WaitForMaxThreadsValue(smartThreadPool, 1, 1 * 1000);
            Assert.IsTrue(success);

            smartThreadPool.MaxThreads = 5;
            success = WaitForMaxThreadsValue(smartThreadPool, 5, 2 * 1000);
            Assert.IsTrue(success);

            smartThreadPool.MaxThreads = 25;
            success = WaitForMaxThreadsValue(smartThreadPool, 25, 4 * 1000);
            Assert.IsTrue(success);

            smartThreadPool.MaxThreads = 10;
            success = WaitForMaxThreadsValue(smartThreadPool, 10, 10 * 1000);
            Assert.IsTrue(success);

            smartThreadPool.Shutdown();
        }

        [Test]
        public void TestMinThreadsChange()
        {
            SmartThreadPool smartThreadPool = new SmartThreadPool(1 * 1000, 25, 0);



            bool success = WaitForMinThreadsValue(smartThreadPool, 0, 1 * 1000);
            Assert.IsTrue(success);

            smartThreadPool.MinThreads = 5;
            success = WaitForMinThreadsValue(smartThreadPool, 5, 2 * 1000);
            Assert.IsTrue(success);

            smartThreadPool.MinThreads = 25;
            success = WaitForMinThreadsValue(smartThreadPool, 25, 4 * 1000);
            Assert.IsTrue(success);

            smartThreadPool.MinThreads = 10;
            success = WaitForMinThreadsValue(smartThreadPool, 10, 10 * 1000);
            Assert.IsTrue(success);

            smartThreadPool.Shutdown();
        }

        /// <summary>
        /// Example of waiting for idle
        /// </summary>
        [Test]
        public void TestConcurrencyChange()
        {
            SmartThreadPool smartThreadPool = new SmartThreadPool(10 * 1000, 1, 0);

            bool success = false;

            for (int i = 0; i < 100; ++i)
            {
                smartThreadPool.QueueWorkItem(
                    new WorkItemCallback(this.DoSomeWork),
                    null);
            }

            smartThreadPool.Concurrency = 1;
            success = WaitForMaxThreadsValue(smartThreadPool, 1, 1 * 1000);
            Assert.IsTrue(success);

            smartThreadPool.Concurrency = 5;
            success = WaitForMaxThreadsValue(smartThreadPool, 5, 2 * 1000);
            Assert.IsTrue(success);

            smartThreadPool.Concurrency = 25;
            success = WaitForMaxThreadsValue(smartThreadPool, 25, 4 * 1000);
            Assert.IsTrue(success);

            smartThreadPool.Concurrency = 10;
            success = WaitForMaxThreadsValue(smartThreadPool, 10, 10 * 1000);
            Assert.IsTrue(success);

            smartThreadPool.Shutdown();
        }


        private bool WaitForMaxThreadsValue(SmartThreadPool smartThreadPool, int maxThreadsCount, int timeout)
        {
            DateTime end = DateTime.Now + new TimeSpan(0, 0, 0, 0, timeout);

            bool success = false;
            while(DateTime.Now <= end && !success)
            {
                success = (smartThreadPool.InUseThreads == maxThreadsCount);
                Thread.Sleep(10);
            }

            return success;
        }

        private bool WaitForMinThreadsValue(SmartThreadPool smartThreadPool, int minThreadsCount, int timeout)
        {
            DateTime end = DateTime.Now + new TimeSpan(0, 0, 0, 0, timeout);

            bool success = false;
            while (DateTime.Now <= end && !success)
            {
                success = (smartThreadPool.ActiveThreads == minThreadsCount);
                Thread.Sleep(10);
            }

            return success;
        }


        private int x = 0;
        private object DoSomeWork(object state)
        {
            Debug.WriteLine(Interlocked.Increment(ref x));
            Thread.Sleep(1000);
            return 1;
        }
	}
}
