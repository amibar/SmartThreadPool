using System;
using System.Threading;

using NUnit.Framework;

using Amib.Threading;

namespace WorkItemsGroupTests
{
	/// <summary>
	/// Summary description for GetResultExample.
	/// </summary>
	[TestFixture]
	[Category("Test WorkItemsGroup GetResult")]
	public class TestGetResult
	{
		/// <summary>
		/// Example of how to queue a work item and then wait infinitely for the result.
		/// </summary>
		[Test]
		public void BlockingCall() 
		{ 
			SmartThreadPool smartThreadPool = new SmartThreadPool();
			IWorkItemsGroup workItemsGroup = smartThreadPool.CreateWorkItemsGroup(int.MaxValue);

			bool success = false;

			IWorkItemResult wir = 
				workItemsGroup.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);

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
		public void Timeout() 
		{ 
			SmartThreadPool smartThreadPool = new SmartThreadPool();
			IWorkItemsGroup workItemsGroup = smartThreadPool.CreateWorkItemsGroup(int.MaxValue);

			bool success = false;

			IWorkItemResult wir = 
				workItemsGroup.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);

			try
			{
				wir.GetResult(500, true);
			}
			catch (WorkItemTimeoutException)
			{
				success = true;
			}

			smartThreadPool.Shutdown();

			Assert.IsTrue(success);
		} 

		/// <summary>
		/// Example of how to queue a work item and then cancel it while it is in the queue.
		/// </summary>
		[Test]
		public void WorkItemCanceling() 
		{ 
			// Create a SmartThreadPool with only one thread.
			// It just to show how to use the work item canceling feature
			SmartThreadPool smartThreadPool = new SmartThreadPool(10*1000, 1);
			IWorkItemsGroup workItemsGroup = smartThreadPool.CreateWorkItemsGroup(int.MaxValue);

			bool success = false;

			// Queue a work item that will occupy the thread in the pool
			IWorkItemResult wir1 = 
				workItemsGroup.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);

			// Queue another work item that will wait for the first to complete
			IWorkItemResult wir2 = 
				workItemsGroup.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);

			// Wait a while for the thread pool to start executing the first work item
			Thread.Sleep(100);

			// The first work item cannot be canceled since it is currently executing
			if (!wir1.Cancel())
			{
				// Cancel the second work item while it still in the queue
				if (wir2.Cancel())
				{
					try
					{
						// Retreiving result of a canceled work item throws an exception
						wir2.GetResult();
					}
					catch (WorkItemCancelException)
					{
						success = true;
					}
				}
			}

			smartThreadPool.Shutdown();

			Assert.IsTrue(success);
		} 

		/// <summary>
		/// Example of how to interrupt the waiting for a work item to complete.
		/// </summary>
		[Test]
		public void WorkItemWaitCanceling() 
		{ 
			SmartThreadPool smartThreadPool = new SmartThreadPool();
			IWorkItemsGroup workItemsGroup = smartThreadPool.CreateWorkItemsGroup(int.MaxValue);

			ManualResetEvent cancelWaitHandle = new ManualResetEvent(false);

			bool success = false;

			// Queue a work item that will occupy the thread in the pool
			IWorkItemResult wir1 = 
				workItemsGroup.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);

			// Queue another work item that will wait for the first to complete
			IWorkItemResult wir2 = 
				workItemsGroup.QueueWorkItem(new WorkItemCallback(this.SignalCancel), cancelWaitHandle);

			try
			{
				wir1.GetResult(System.Threading.Timeout.Infinite, true, cancelWaitHandle);
			}
			catch (WorkItemTimeoutException) 
			{
				success = true;
			}

			smartThreadPool.Shutdown();

			Assert.IsTrue(success);
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
