using System.Diagnostics;
using System.Threading;

using NUnit.Framework;

using Amib.Threading;

namespace SmartThreadPoolTests
{
	/// <summary>
    /// Summary description for TestCancel.
	/// </summary>
	[TestFixture]
	[Category("TestCancel")]
	public partial class TestCancel
	{
	    /// <summary>
        /// 1. Create STP in suspended mode
        /// 2. Queue work item into the STP
        /// 3. Cancel the work item
        /// 4. Work item's GetResult should throw WorkItemCancelException
        /// </summary>        
        [Test]
        public void CancelInQueueWorkItem()
	    {
	        Assert.Throws<WorkItemCancelException>(() =>
	        {
	            STPStartInfo stpStartInfo = new STPStartInfo();
	            stpStartInfo.StartSuspended = true;

	            SmartThreadPool stp = new SmartThreadPool(stpStartInfo);
	            IWorkItemResult wir = stp.QueueWorkItem(arg => null);

	            wir.Cancel();

	            Assert.IsTrue(wir.IsCanceled);

	            try
	            {
	                wir.GetResult();
	            }
	            finally
	            {
	                stp.Shutdown();
	            }
	        });
	    }

        /// <summary>
        /// 1. Create STP
        /// 2. Queue work item that takes some time
        /// 3. Wait for it to start
        /// 4. Cancel the work item (soft)
        /// 5. Work item's GetResult should throw WorkItemCancelException
        /// </summary>        
        [Test]
        //[ExpectedException(typeof(WorkItemCancelException))]
        public void CancelInProgressWorkItemSoft()
        {
            Assert.Throws<WorkItemCancelException>(() =>
            {

                ManualResetEvent waitToStart = new ManualResetEvent(false);

                SmartThreadPool stp = new SmartThreadPool();
                IWorkItemResult wir = stp.QueueWorkItem(
                    state =>
                    {
                        waitToStart.Set();
                        Thread.Sleep(100);
                        return null;
                    }
                    );

                waitToStart.WaitOne();

                wir.Cancel(false);

                Assert.IsTrue(wir.IsCanceled);

                try
                {
                    wir.GetResult();
                }
                finally
                {
                    stp.Shutdown();
                }
            });
        }

        /// <summary>
        /// 1. Create STP
        /// 2. Queue work item that takes some time
        /// 3. Wait for it to start
        /// 4. Cancel the work item (soft)
        /// 5. Make sure, in the work item, that SmartThreadPool.IsWorkItemCanceled is true
        /// 5. Wait for the STP to get idle
        /// 6. Work item's GetResult should throw WorkItemCancelException
        /// </summary>        
        [Test]
        public void CancelInProgressWorkItemSoftWithSample()
        {
            bool cancelled = false;
            ManualResetEvent waitToStart = new ManualResetEvent(false);
            ManualResetEvent waitToComplete = new ManualResetEvent(false);

            SmartThreadPool stp = new SmartThreadPool();
            IWorkItemResult wir = stp.QueueWorkItem(
                state => {
                    waitToStart.Set();
                    waitToComplete.WaitOne();
                    cancelled = SmartThreadPool.IsWorkItemCanceled;
                    return null;
                }
                );

            waitToStart.WaitOne();

            wir.Cancel(false);

            waitToComplete.Set();

            stp.WaitForIdle();

            Assert.IsTrue(cancelled);

            stp.Shutdown();
        }      
        
        /// <summary>
        /// 1. Create STP
        /// 2. Queue work item that takes some time
        /// 3. Wait for it to start
        /// 4. Cancel the work item (soft)
        /// 5. Don't call to SmartThreadPool.IsWorkItemCanceled
        /// 6. Wait for the STP to get idle
        /// 7. Work item's GetResult should throw WorkItemCancelException
        /// </summary>        
        [Test]
        public void CancelInProgressWorkItemSoftWithIgnoreSample()
        {
            Assert.Throws<WorkItemCancelException>(() =>
            {
                ManualResetEvent waitToStart = new ManualResetEvent(false);
                ManualResetEvent waitToComplete = new ManualResetEvent(false);

                SmartThreadPool stp = new SmartThreadPool();
                IWorkItemResult wir = stp.QueueWorkItem(
                    state =>
                    {
                        waitToStart.Set();
                        Thread.Sleep(100);
                        waitToComplete.WaitOne();
                        return null;
                    }
                    );

                waitToStart.WaitOne();

                wir.Cancel(false);

                waitToComplete.Set();

                stp.WaitForIdle();

                // Throws WorkItemCancelException
                wir.GetResult();

                stp.Shutdown();
            });
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
        public void CancelInProgressWorkItemSoftWithAbortOnWorkItemCancel()
        {
            bool abortFailed = false;
            ManualResetEvent waitToStart = new ManualResetEvent(false);
            ManualResetEvent waitToCancel = new ManualResetEvent(false);

            SmartThreadPool stp = new SmartThreadPool();
            IWorkItemResult wir = stp.QueueWorkItem(
                state => {
                    waitToStart.Set();
                    waitToCancel.WaitOne();
                    SmartThreadPool.AbortOnWorkItemCancel();
                    abortFailed = true;
                    return null;
                });

            waitToStart.WaitOne();

            wir.Cancel(false);

            waitToCancel.Set();

            stp.WaitForIdle();

            Assert.IsTrue(wir.IsCanceled);
            Assert.IsFalse(abortFailed);

            stp.Shutdown();
        }

        /// <summary>
        /// 1. Create STP in suspended mode
        /// 2. Queue work item into the STP
        /// 3. Cancel the work item
        /// 4. Start the STP
        /// 5. Wait for the STP to get idle
        /// 6. Work item's GetResult should throw WorkItemCancelException
        /// 7. Cancel the work item again
        /// 8. Work item's GetResult should throw WorkItemCancelException
        /// </summary>        
        [Test]
        public void CancelCanceledWorkItem()
        {
            Assert.Throws<WorkItemCancelException>(() =>
            {
                STPStartInfo stpStartInfo = new STPStartInfo();
                stpStartInfo.StartSuspended = true;

                SmartThreadPool stp = new SmartThreadPool(stpStartInfo);
                IWorkItemResult wir = stp.QueueWorkItem(state => null);

                int counter = 0;

                wir.Cancel();

                try
                {
                    wir.GetResult();
                }
                catch (WorkItemCancelException ce)
                {
                    ce.GetHashCode();
                    ++counter;
                }

                Assert.AreEqual(counter, 1);

                wir.Cancel();

                try
                {
                    wir.GetResult();
                }
                finally
                {
                    stp.Shutdown();
                }
            });
        }

        /// <summary>
        /// 1. Create STP
        /// 2. Queue work item into the STP
        /// 3. Wait for the STP to get idle
        /// 4. Work item's GetResult should return value
        /// 4. Cancel the work item
        /// 5. Work item's GetResult should return value
        /// </summary>        
        [Test]
        public void CancelCompletedWorkItem()
        {
            SmartThreadPool stp = new SmartThreadPool();
            IWorkItemResult wir = stp.QueueWorkItem(
                state => 1
                );

            stp.WaitForIdle();

            Assert.AreEqual(wir.GetResult(), 1);

            wir.Cancel();

            Assert.AreEqual(wir.GetResult(), 1);

            stp.Shutdown();
        }
  	}  
}
