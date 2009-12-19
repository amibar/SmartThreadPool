using System;
using System.Threading;
using System.Diagnostics;

using NUnit.Framework;

using Amib.Threading;

namespace SmartThreadPoolTests
{
	/// <summary>
	/// Summary description for TestWaitForIdle.
	/// </summary>
	[TestFixture]
	[Category("TestWaitForIdle")]
	public class TestWaitForIdle
	{
		public TestWaitForIdle()
		{
		}

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
	}
}
