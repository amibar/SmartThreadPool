using System;
using NUnit.Framework;

using Amib.Threading;

namespace WorkItemsGroupTests
{
	/// <summary>
	/// Summary description for TestChainedDelegates.
	/// </summary>
	[TestFixture]
	[Category("Test WorkItemsGroup ChainedDelegates")]
	public class TestChainedDelegates
	{
	    [Test]
		public void GoodCallback()
		{
			SmartThreadPool smartThreadPool = new SmartThreadPool();
			IWorkItemsGroup workItemsGroup = smartThreadPool.CreateWorkItemsGroup(int.MaxValue);

			workItemsGroup.QueueWorkItem(new WorkItemCallback(DoWork));

			workItemsGroup.WaitForIdle();

			smartThreadPool.Shutdown();
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void ChainedDelegatesCallback()
		{
			SmartThreadPool smartThreadPool = new SmartThreadPool();
			IWorkItemsGroup workItemsGroup = smartThreadPool.CreateWorkItemsGroup(int.MaxValue);

			WorkItemCallback workItemCallback = new WorkItemCallback(DoWork);
			workItemCallback += new WorkItemCallback(DoWork);

			workItemsGroup.QueueWorkItem(workItemCallback);

			workItemsGroup.WaitForIdle();

			smartThreadPool.Shutdown();
		}

		[Test]
		public void GoodPostExecute()
		{
			SmartThreadPool smartThreadPool = new SmartThreadPool();
			IWorkItemsGroup workItemsGroup = smartThreadPool.CreateWorkItemsGroup(int.MaxValue);

			workItemsGroup.QueueWorkItem(
				new WorkItemCallback(DoWork),
				null,
				new PostExecuteWorkItemCallback(DoPostExecute));

			workItemsGroup.WaitForIdle();

			smartThreadPool.Shutdown();
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void ChainedDelegatesPostExecute()
		{
			SmartThreadPool smartThreadPool = new SmartThreadPool();
			IWorkItemsGroup workItemsGroup = smartThreadPool.CreateWorkItemsGroup(int.MaxValue);

			PostExecuteWorkItemCallback postExecuteWorkItemCallback = 
				new PostExecuteWorkItemCallback(DoPostExecute);
			postExecuteWorkItemCallback += 
				new PostExecuteWorkItemCallback(DoPostExecute);

			workItemsGroup.QueueWorkItem(
				new WorkItemCallback(DoWork),
				null,
				postExecuteWorkItemCallback);

			workItemsGroup.WaitForIdle();

			smartThreadPool.Shutdown();
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
