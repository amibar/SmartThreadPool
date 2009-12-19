using System;
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
	    /// <summary>
        /// Example of waiting for idle
        /// </summary>
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

        /// <summary>
        /// Example of waiting for idle
        /// </summary>
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




/*
        /// <summary>
        /// Example of waiting for idle
        /// </summary>
        [Test]
        public void WaitForIdle() 
        { 
            SmartThreadPool smartThreadPool = new SmartThreadPool(10*1000, 25, 0);

            bool success = false;

            for(int i = 0; i < 100; ++i)
            {
                smartThreadPool.QueueWorkItem(
                    new WorkItemCallback(this.DoSomeWork), 
                    null);
            }

            success = !smartThreadPool.WaitForIdle(3500);
            success = success && smartThreadPool.WaitForIdle(1000);

            smartThreadPool.Shutdown();

            Assert.IsTrue(success);
        } 

		[Test]
		public void WaitForIdleOnWrongThread() 
		{ 
			SmartThreadPool smartThreadPool = new SmartThreadPool(10*1000, 25, 0);

			IWorkItemResult wir = smartThreadPool.QueueWorkItem(
				new WorkItemCallback(this.DoWaitForIdle), 
				smartThreadPool);

			Exception e;
			wir.GetResult(out e);

			smartThreadPool.Shutdown();

			Assert.IsTrue(e is NotSupportedException);
		} 


        private int x = 0;
        private object DoSomeWork(object state)
        { 
            Debug.WriteLine(Interlocked.Increment(ref x));
            Thread.Sleep(1000);
            return 1;
        }

		private object DoWaitForIdle(object state)
		{ 
			SmartThreadPool smartThreadPool = state as SmartThreadPool;
			smartThreadPool.WaitForIdle();
			return null;
		}
 */ 
	}
}
