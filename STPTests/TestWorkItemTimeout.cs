using System.Threading;

using NUnit.Framework;

using Amib.Threading;

namespace SmartThreadPoolTests
{
    [TestFixture]
    [Category("TestWorkItemTimeout")]
    public class TestWorkItemTimeout
    {
        /// <summary>
        /// 1. Create STP in suspended mode
        /// 2. Queue work item into the STP
        /// 3. Wait for the work item to expire
        /// 4. Work item's GetResult should throw WorkItemCancelException
        /// </summary>        
        [Test]
        [ExpectedException(typeof(WorkItemCancelException))]
        public void CancelInQueueWorkItem()
        {
            STPStartInfo stpStartInfo = new STPStartInfo();
            stpStartInfo.StartSuspended = true;

            bool hasRun = false;

            SmartThreadPool stp = new SmartThreadPool(stpStartInfo);
            IWorkItemResult wir = stp.QueueWorkItem(
                new WorkItemInfo() { Timeout = 500 }, 
                state =>
                    {
                        hasRun = true;
                        return null;
                    });

            Assert.IsFalse(wir.IsCanceled);

            Thread.Sleep(2000);

            Assert.IsTrue(wir.IsCanceled);

            stp.Start();
            stp.WaitForIdle();

            Assert.IsFalse(hasRun);

            try
            {
                wir.GetResult();
            }
            finally
            {
                stp.Shutdown();
            }
        }

        /// <summary>
        /// 1. Create STP
        /// 2. Queue work item that takes some time
        /// 3. Wait for it to start
        /// 4. The work item timeout expires
        /// 5. Make sure, in the work item, that SmartThreadPool.IsWorkItemCanceled is true
        /// 5. Wait for the STP to get idle
        /// 6. Work item's GetResult should throw WorkItemCancelException
        /// </summary>        
        [Test]
        public void TimeoutInProgressWorkItemWithSample()
        {
            bool timedout = false;
            ManualResetEvent waitToStart = new ManualResetEvent(false);
            ManualResetEvent waitToComplete = new ManualResetEvent(false);

            SmartThreadPool stp = new SmartThreadPool();
            IWorkItemResult wir = stp.QueueWorkItem(
                new WorkItemInfo() { Timeout = 500 },
                state =>
                {
                    waitToStart.Set();
                    Thread.Sleep(1000);
                    timedout = SmartThreadPool.IsWorkItemCanceled;
                    waitToComplete.Set();
                    return null;
                });

            waitToStart.WaitOne();

            waitToComplete.WaitOne();

            Assert.IsTrue(timedout);

            stp.Shutdown();
        }

        /// <summary>
        /// 1. Create STP
        /// 2. Queue work item into the STP
        /// 3. Wait for the STP to get idle
        /// 4. Work item's GetResult should return value
        /// 4. The work item expires
        /// 5. Work item's GetResult should return value
        /// </summary>        
        [Test]
        public void TimeoutCompletedWorkItem()
        {
            SmartThreadPool stp = new SmartThreadPool();
            IWorkItemResult wir = 
                stp.QueueWorkItem(
                new WorkItemInfo() { Timeout = 500 },
                state => 1);

            stp.WaitForIdle();

            Assert.AreEqual(wir.GetResult(), 1);

            Thread.Sleep(1000);

            Assert.AreEqual(wir.GetResult(), 1);

            stp.Shutdown();
        }

        /// <summary>
        /// 1. Create STP
        /// 2. Queue work item that takes some time
        /// 3. Wait for it to start
        /// 4. Cancel the work item (soft)
        /// 5. Call to AbortOnWorkItemCancel
        /// 5. Wait for the STP to get idle
        /// 6. Make sure nothing ran in the work item after the AbortOnWorkItemCancel
        /// </summary>        
        [Test]
        public void TimeoutInProgressWorkItemSoftWithAbortOnWorkItemCancel()
        {
            bool abortFailed = false;
            ManualResetEvent waitToStart = new ManualResetEvent(false);
            ManualResetEvent waitToComplete = new ManualResetEvent(false);

            SmartThreadPool stp = new SmartThreadPool();
            IWorkItemResult wir = stp.QueueWorkItem(
                new WorkItemInfo() { Timeout = 500 },
                state =>
                {
                    waitToStart.Set();
                    Thread.Sleep(1000);
                    SmartThreadPool.AbortOnWorkItemCancel();
                    abortFailed = true;
                    return null;
                });

            waitToStart.WaitOne();

            stp.WaitForIdle();

            Assert.IsTrue(wir.IsCanceled);
            Assert.IsFalse(abortFailed);
            stp.Shutdown();
        }
    }
}
