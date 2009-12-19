using System;
using NUnit.Framework;

using Amib.Threading;

namespace SmartThreadPoolTests
{
	/// <summary>
	/// Summary description for TestChainedDelegates.
	/// </summary>
	[TestFixture]
	[Category("TestChainedDelegates")]
	public class TestChainedDelegates
	{
	    [Test]
		public void GoodCallback()
		{
			SmartThreadPool stp = new SmartThreadPool();

			stp.QueueWorkItem(new WorkItemCallback(DoWork));

			stp.WaitForIdle();

			stp.Shutdown();
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void ChainedDelegatesCallback()
		{
			SmartThreadPool stp = new SmartThreadPool();

			WorkItemCallback workItemCallback = new WorkItemCallback(DoWork);
			workItemCallback += new WorkItemCallback(DoWork);

			stp.QueueWorkItem(workItemCallback);

			stp.WaitForIdle();

			stp.Shutdown();
		}

		[Test]
		public void GoodPostExecute()
		{
			SmartThreadPool stp = new SmartThreadPool();

			stp.QueueWorkItem(
				new WorkItemCallback(DoWork),
				null,
				new PostExecuteWorkItemCallback(DoPostExecute));

			stp.WaitForIdle();

			stp.Shutdown();
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void ChainedDelegatesPostExecute()
		{
			SmartThreadPool stp = new SmartThreadPool();

            PostExecuteWorkItemCallback postExecuteWorkItemCallback = DoPostExecute;
			postExecuteWorkItemCallback += DoPostExecute;

			stp.QueueWorkItem(
				new WorkItemCallback(DoWork),
				null,
				postExecuteWorkItemCallback);

			stp.WaitForIdle();

			stp.Shutdown();
		}


		private object DoWork(object state)
		{
			return null;
		}

		private void DoPostExecute(IWorkItemResult wir)
		{
		}


	}
}
