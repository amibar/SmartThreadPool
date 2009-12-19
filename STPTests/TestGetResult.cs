using System;
using System.Threading;

using NUnit.Framework;

using Amib.Threading;

namespace SmartThreadPoolTests
{
	/// <summary>
	/// Summary description for GetResultExample.
	/// </summary>
	[TestFixture]
	[Category("TestGetResult")]
	public class TestGetResult
	{
		/// <summary>
		/// Example of how to queue a work item and then wait infinitely for the result.
		/// </summary>
		[Test]
		public void BlockingCall() 
		{ 
			SmartThreadPool smartThreadPool = new SmartThreadPool();

			bool success = false;

			IWorkItemResult wir = 
				smartThreadPool.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);

			if (!wir.IsCompleted)
			{
				int result = (int)wir.GetResult();
				success = (1 == result);
			}

			smartThreadPool.Shutdown();

			Assert.IsTrue(success);
		} 

		/// <summary>
		/// Example of how to queue a work item and then wait on a timeout for the result.
		/// </summary>
		[Test]
        [ExpectedException(typeof(WorkItemTimeoutException))]
        public void Timeout() 
		{ 
			SmartThreadPool smartThreadPool = new SmartThreadPool();

			IWorkItemResult wir = 
				smartThreadPool.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);

            try
            {
                wir.GetResult(500, true);
            }
            finally
            {
                smartThreadPool.Shutdown();
            }
		}

        /// <summary>
        /// Example of how to interrupt the waiting for a work item to complete.
        /// </summary>
        [Test]
        [ExpectedException(typeof(WorkItemTimeoutException))]
        public void WorkItemWaitCanceling()
        {
            SmartThreadPool smartThreadPool = new SmartThreadPool();

            ManualResetEvent cancelWaitHandle = new ManualResetEvent(false);

            // Queue a work item that will occupy the thread in the pool
            IWorkItemResult wir1 =
                smartThreadPool.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);

            // Queue another work item that will wait for the first to complete
            IWorkItemResult wir2 =
                smartThreadPool.QueueWorkItem(new WorkItemCallback(this.SignalCancel), cancelWaitHandle);

            try
            {
                wir1.GetResult(System.Threading.Timeout.Infinite, true, cancelWaitHandle);
            }
            finally
            {
                smartThreadPool.Shutdown();
            }
        } 

		private object DoSomeWork(object state)
		{ 
			Thread.Sleep(1000);
			return 1;
		}

        private object SignalCancel(object state)
        {
            ManualResetEvent cancelWaitHandle = state as ManualResetEvent;
            Thread.Sleep(250);
            cancelWaitHandle.Set();
            return null;
        }

	}
}
