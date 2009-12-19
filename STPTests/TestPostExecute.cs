using System;
using System.Threading;

using NUnit.Framework;

using Amib.Threading;

namespace SmartThreadPoolTests
{
	/// <summary>
	/// Summary description for DoTestPostExecute.
	/// </summary>
	[TestFixture]
	[Category("DoTestPostExecute")]
	public class TestPostExecution
	{
		/// <summary>
		/// 
		/// </summary>
		[Test]
		public void DefaultPostExecute_AlwaysCall()
		{
			Assert.IsTrue(DoTestDefaultPostExecute(CallToPostExecute.Always, true));
		}

		[Test]
		public void DefaultPostExecute_NeverCall()
		{
			Assert.IsTrue(DoTestDefaultPostExecute(CallToPostExecute.Never, false));
		}

		[Test]
		public void DefaultPostExecute_CallWhenCanceled()
		{
			Assert.IsTrue(DoTestDefaultPostExecute(CallToPostExecute.WhenWorkItemCanceled, false));
		}

		[Test]
		public void DefaultPostExecute_CallWhenNotCanceled()
		{
			Assert.IsTrue(DoTestDefaultPostExecute(CallToPostExecute.WhenWorkItemNotCanceled, true));
		}

		/// <summary>
		/// 
		/// </summary>
		[Test]
		public void PostExecute_AlwaysCall()
		{
			Assert.IsTrue(DoTestPostExecute(CallToPostExecute.Always, true));
		}

		[Test]
		public void PostExecute_NeverCall()
		{
			Assert.IsTrue(DoTestPostExecute(CallToPostExecute.Never, false));
		}

		[Test]
		public void PostExecute_CallWhenCanceled()
		{
			Assert.IsTrue(DoTestPostExecute(CallToPostExecute.WhenWorkItemCanceled, false));
		}

		[Test]
		public void PostExecute_CallWhenNotCanceled()
		{
			Assert.IsTrue(DoTestPostExecute(CallToPostExecute.WhenWorkItemNotCanceled, true));
		}

		/// <summary>
		/// 
		/// </summary>
		[Test]
		public void PostExecuteWithCancel_AlwaysCall()
		{
			Assert.IsTrue(DoTestPostExecuteWithCancel(CallToPostExecute.Always, true));
		}

		[Test]
		public void PostExecuteWithCancel_NeverCall()
		{
			Assert.IsTrue(DoTestPostExecuteWithCancel(CallToPostExecute.Never, false));
		}

		[Test]
		public void PostExecuteWithCancel_CallWhenCanceled()
		{
			Assert.IsTrue(DoTestPostExecuteWithCancel(CallToPostExecute.WhenWorkItemCanceled, true));
		}

		[Test]
		public void PostExecuteWithCancel_CallWhenNotCanceled()
		{
			Assert.IsTrue(DoTestPostExecuteWithCancel(CallToPostExecute.WhenWorkItemNotCanceled, false));
		}


		private class PostExecuteResult
		{
			public ManualResetEvent wh = new ManualResetEvent(false);
		}

		/// <summary>
		/// Example of how to use the post execute callback
		/// </summary>
		private bool DoTestDefaultPostExecute(CallToPostExecute callToPostExecute, bool answer)
		{ 
			STPStartInfo stpStartInfo = new STPStartInfo();
			stpStartInfo.CallToPostExecute = callToPostExecute;
			stpStartInfo.PostExecuteWorkItemCallback = new PostExecuteWorkItemCallback(this.DoSomePostExecuteWork);

			SmartThreadPool smartThreadPool = new SmartThreadPool(stpStartInfo);

			bool success = false;

			PostExecuteResult postExecuteResult = new PostExecuteResult();

			IWorkItemResult wir = 
				smartThreadPool.QueueWorkItem(
				new WorkItemCallback(this.DoSomeWork), 
				postExecuteResult);

			if (!wir.IsCompleted)
			{
				int result = (int)wir.GetResult();
				success = (1 == result);
				success = success && (postExecuteResult.wh.WaitOne(1000, true) == answer);
			}

			smartThreadPool.Shutdown();

			return success;
		}


		/// <summary>
		/// Example of how to use the post execute callback
		/// </summary>
		private bool DoTestPostExecute(CallToPostExecute callToPostExecute, bool answer)
		{ 
			SmartThreadPool smartThreadPool = new SmartThreadPool();

			bool success = false;

			PostExecuteResult postExecuteResult = new PostExecuteResult();

			IWorkItemResult wir = 
				smartThreadPool.QueueWorkItem(
				new WorkItemCallback(this.DoSomeWork), 
				postExecuteResult,
				new PostExecuteWorkItemCallback(this.DoSomePostExecuteWork),
				callToPostExecute);

			if (!wir.IsCompleted)
			{
				int result = (int)wir.GetResult();
				success = (1 == result);
				success = success && (postExecuteResult.wh.WaitOne(1000, true) == answer);
			}

			smartThreadPool.Shutdown();

			return success;
		}

		/// <summary>
		/// Example of how to queue a work item and then cancel it while it is in the queue.
		/// </summary>
		private bool DoTestPostExecuteWithCancel(CallToPostExecute callToPostExecute, bool answer) 
		{ 
			// Create a SmartThreadPool with only one thread.
			// It just to show how to use the work item canceling feature
			SmartThreadPool smartThreadPool = new SmartThreadPool(10*1000, 1);

			bool success = false;
			PostExecuteResult postExecuteResult = new PostExecuteResult();

			// Queue a work item that will occupy the thread in the pool
			smartThreadPool.QueueWorkItem(
				new WorkItemCallback(this.DoSomeWork), 
				null);

			// Queue another work item that will wait for the first to complete
			IWorkItemResult wir = 
				smartThreadPool.QueueWorkItem(
					new WorkItemCallback(this.DoSomeWork), 
					postExecuteResult,
					new PostExecuteWorkItemCallback(this.DoSomePostExecuteWork),
					callToPostExecute);


			// Wait a while for the thread pool to start executing the first work item
			Thread.Sleep(100);

			// Cancel the second work item while it still in the queue
			if (wir.Cancel())
			{
				success = (postExecuteResult.wh.WaitOne(1000, true) == answer);
			}

			smartThreadPool.Shutdown();

			return success;
		} 

		private object DoSomeWork(object state)
		{ 
			Thread.Sleep(1000);
			return 1;
		}

		private void DoSomePostExecuteWork(IWorkItemResult wir)
		{ 
			PostExecuteResult postExecuteResult = wir.State as PostExecuteResult;
			postExecuteResult.wh.Set();
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
