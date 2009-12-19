using System;
using System.Threading;
using System.Diagnostics;

using NUnit.Framework;

using Amib.Threading;

namespace WorkItemsGroupTests
{
	/// <summary>
	/// Summary description for TestWaitForIdle.
	/// </summary>
	[TestFixture]
	[Category("WorkItemsGroup")]
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
			IWorkItemsGroup workItemsGroup = smartThreadPool.CreateWorkItemsGroup(int.MaxValue);

            bool success = false;

            for(int i = 0; i < 100; ++i)
            {
                workItemsGroup.QueueWorkItem(
                    new WorkItemCallback(this.DoSomeWork), 
                    null);
            }

            success = !workItemsGroup.WaitForIdle(3500);
            success = success && workItemsGroup.WaitForIdle(1000);

            smartThreadPool.Shutdown();

            Assert.IsTrue(success);
        } 

		[Test]
		public void WaitForIdleOnSTPThread() 
		{ 
			SmartThreadPool smartThreadPool = new SmartThreadPool(10*1000, 25, 0);
			IWorkItemsGroup workItemsGroup = smartThreadPool.CreateWorkItemsGroup(int.MaxValue);

			IWorkItemResult wir = workItemsGroup.QueueWorkItem(
				new WorkItemCallback(this.DoWaitForIdle), 
				workItemsGroup);

			Exception e;
			wir.GetResult(out e);

			smartThreadPool.Shutdown();

			Assert.IsNotNull(e);
		} 

		[Test]
		public void WaitForIdleOnSTPThreadForAnotherWorkItemsGroup()
		{ 
			SmartThreadPool smartThreadPool = new SmartThreadPool(10*1000, 25, 0);
			IWorkItemsGroup workItemsGroup1 = smartThreadPool.CreateWorkItemsGroup(int.MaxValue);
			IWorkItemsGroup workItemsGroup2 = smartThreadPool.CreateWorkItemsGroup(int.MaxValue);

			workItemsGroup1.QueueWorkItem(
				new WorkItemCallback(this.DoSomeWork), 
				null);

			workItemsGroup1.QueueWorkItem(
				new WorkItemCallback(this.DoSomeWork), 
				null);

			IWorkItemResult wir = workItemsGroup2.QueueWorkItem(
				new WorkItemCallback(this.DoWaitForIdle), 
				workItemsGroup1);

			Exception e;
			wir.GetResult(out e);

			smartThreadPool.Shutdown();

			Assert.IsNull(e);
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
			IWorkItemsGroup workItemsGroup = state as IWorkItemsGroup;
			workItemsGroup.WaitForIdle();
			return null;
		}
	}
}
