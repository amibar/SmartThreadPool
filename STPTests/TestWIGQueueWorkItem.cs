using NUnit.Framework;

using Amib.Threading;
using SmartThreadPoolTests;

namespace WorkItemsGroupTests
{
	/// <summary>
    /// Tests for QueueWorkItem.
	/// </summary>
	[TestFixture]
    [Category("TestQueueWorkItem")]
    public class TestQueueWorkItem
	{
        private SmartThreadPool _stp;
        private IWorkItemsGroup _wig;

        [SetUp]
        public void Init()
        {
            _stp = new SmartThreadPool();
            _wig = _stp.CreateWorkItemsGroup(SmartThreadPool.DefaultMaxWorkerThreads);
        }

        [TearDown]
        public void Fini()
        {
            _stp.Shutdown();
        }

        //IWorkItemResult QueueWorkItem(WorkItemCallback callback);
        [Test]
        public void TestQueueWorkItemCall()
        {
            QueueWorkItemHelper.TestQueueWorkItemCall(_wig);
        }

        //IWorkItemResult QueueWorkItem(WorkItemCallback callback, WorkItemPriority workItemPriority);
        [Test]
        public void TestQueueWorkItemCallPrio()
        {
            QueueWorkItemHelper.TestQueueWorkItemCallPrio(_wig);
        }

        //IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state);
        [Test]
        public void TestQueueWorkItemCallStat()
        {
            QueueWorkItemHelper.TestQueueWorkItemCallStat(_wig);
        }

        //IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, WorkItemPriority workItemPriority);
        [Test]
        public void TestQueueWorkItemCallStatPrio()
        {
            QueueWorkItemHelper.TestQueueWorkItemCallStatPrio(_wig);
        }

        //IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, PostExecuteWorkItemCallback postExecuteWorkItemCallback);
        [Test]
        public void TestQueueWorkItemCallStatPost()
        {
            QueueWorkItemHelper.TestQueueWorkItemCallStatPost(_wig);
        }

        //IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, PostExecuteWorkItemCallback postExecuteWorkItemCallback, WorkItemPriority workItemPriority);
        [Test]
        public void TestQueueWorkItemCallStatPostPrio()
        {
            QueueWorkItemHelper.TestQueueWorkItemCallStatPostPrio(_wig);
        }

        //IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, PostExecuteWorkItemCallback postExecuteWorkItemCallback, CallToPostExecute callToPostExecute);
        [Test]
        public void TestQueueWorkItemCallStatPostPflg()
        {
            QueueWorkItemHelper.TestQueueWorkItemCallStatPostPflg(_wig);
        }
        
        //IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, PostExecuteWorkItemCallback postExecuteWorkItemCallback, CallToPostExecute callToPostExecute, WorkItemPriority workItemPriority);
        [Test]
        public void TestQueueWorkItemCallStatPostPflgPrio()
        {
            QueueWorkItemHelper.TestQueueWorkItemCallStatPostPflgPrio(_wig);
        }

        //IWorkItemResult QueueWorkItem(WorkItemInfo workItemInfo, WorkItemCallback callback);
        [Test]
        public void TestQueueWorkItemInfoCall()
        {
            QueueWorkItemHelper.TestQueueWorkItemInfoCall(_wig);
        }

        //IWorkItemResult QueueWorkItem(WorkItemInfo workItemInfo, WorkItemCallback callback, object state);
        [Test]
        public void TestQueueWorkItemInfoCallStat()
        {
            QueueWorkItemHelper.TestQueueWorkItemInfoCallStat(_wig);
        }
	}
}
