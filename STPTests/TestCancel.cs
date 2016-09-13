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
	public class TestCancel
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
        /// 5. Work item's GetResult should throw WorkItemCancelException
        /// </summary>        
        [Test]
        //[ExpectedException(typeof(WorkItemCancelException))]
        public void CancelCancelledWorkItemAbort()
        {
            Assert.Throws<WorkItemCancelException>(() =>
            {

                ManualResetEvent waitToStart = new ManualResetEvent(false);

                SmartThreadPool stp = new SmartThreadPool();
                IWorkItemResult wir = stp.QueueWorkItem(
                    state =>
                    {
                        waitToStart.Set();
                        while (true)
                        {
                            Thread.Sleep(1000);
                        }
                        return null;
                    }
                    );

                waitToStart.WaitOne();

                wir.Cancel(false);

                Assert.IsTrue(wir.IsCanceled);

                bool completed = stp.WaitForIdle(1000);

                Assert.IsFalse(completed);

                wir.Cancel(true);

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
        /// 2. Queue work item that:
        ///     a. Sleep for 0.1 seconds
        ///     b. Increment the counter
        /// 3. Wait for the work item to start
        /// 4. Cancel the work item (abort)
        /// 5. Make sure the work item result indicates the work item has been cancelled.
        /// 6. Make sure the counter incrementation didn't happen
        /// 7. Work item's GetResult should throw WorkItemCancelException
        /// </summary>        
        [Test]
        public void CancelInProgressWorkItemAbort()
        {
            Assert.Throws<WorkItemCancelException>(() =>
            {

                ManualResetEvent waitToStart = new ManualResetEvent(false);
                int counter = 0;

                SmartThreadPool stp = new SmartThreadPool();
                IWorkItemResult wir = stp.QueueWorkItem(
                    state =>
                    {
                        waitToStart.Set();
                        Thread.Sleep(100);
                        ++counter;
                        return null;
                    }
                    );

                waitToStart.WaitOne();

                wir.Cancel(true);

                Assert.IsTrue(wir.IsCanceled);

                Assert.AreEqual(counter, 0);

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

        /// <summary>
        /// 1. Zero counter
        /// 2. Create STP
        /// 3. Queue 10 work items, that sleep and then increment the counter, into the STP
        /// 4. Cancel the STP
        /// 5. Make sure the counter is still zero
        /// </summary>        
        [Test]
        public void CancelSTPWorkItems()
        {
            // I don't use lock on the counter, since any number above 0 is a failure.
            // In the worst case counter will be equal to 1 which is still not 0.
            int counter = 0;

            SmartThreadPool stp = new SmartThreadPool();

            for (int i = 0; i < 10; i++)
            {
                stp.QueueWorkItem(
                    state =>  { Thread.Sleep(500); ++counter; return null; }
                    );
            }

            Thread.Sleep(100);

            stp.Cancel(true);

            Assert.AreEqual(counter, 0);

            stp.Shutdown();
        }

        /// <summary>
        /// 1. Zero counter
        /// 2. Create STP
        /// 3. Create a WIG
        /// 4. Queue 10 work items, that sleep and then increment the counter, into the WIG
        /// 5. Cancel the WIG
        /// 6. Wait for the WIG to become idle
        /// 7. Make sure the counter is still zero
        /// </summary>        
        [Test]
        public void CancelWIGWorkItems()
        {
            // I don't use lock on the counter, since any number above 0 is a failure.
            // In the worst case counter will be equal to 1 which is still not 0.
            int counter = 0;

            SmartThreadPool stp = new SmartThreadPool();
            IWorkItemsGroup wig = stp.CreateWorkItemsGroup(10);

            for (int i = 0; i < 10; i++)
            {
                wig.QueueWorkItem(
                    state => { Thread.Sleep(500); ++counter; return null; }
                    );
            }

            Thread.Sleep(100);

            wig.Cancel(true);

            Assert.AreEqual(counter, 0);

            stp.Shutdown();
        }

        /// <summary>
        /// 1. Zero global counter
        /// 2. Create STP
        /// 3. Create a WIG1 in suspended mode
        /// 4. Create a WIG2 in suspended mode
        /// 5. Queue 5 work items, that increment the global counter, into the WIG1
        /// 6. Queue 7 work items, that increment the global counter, into the WIG2
        /// 7. Cancel the WIG1
        /// 8. Start the WIG1
        /// 9. Start the WIG2
        /// 10. Wait for the STP to get idle
        /// 11. Make sure the global counter is 7
        /// </summary>                
        [Test]
        public void Cancel1WIGof2WorkItems()
        {
            int counter1 = 0;
            int counter2 = 0;

            SmartThreadPool stp = new SmartThreadPool();
            IWorkItemsGroup wig1 = stp.CreateWorkItemsGroup(3);
            IWorkItemsGroup wig2 = stp.CreateWorkItemsGroup(3);

            for (int i = 0; i < 3; i++)
            {
                wig1.QueueWorkItem(
                    state => { Interlocked.Increment(ref counter1); Thread.Sleep(500); Interlocked.Increment(ref counter1); return null; }
                    );
            }

            for (int i = 0; i < 3; i++)
            {
                wig2.QueueWorkItem(
                    state => { Thread.Sleep(500); Interlocked.Increment(ref counter2); return null; }
                    );
            }

            while (counter1 < 3)
            {
                Thread.Sleep(1);
            }
            wig1.Cancel(true);

            stp.WaitForIdle();

            Assert.AreEqual(3, counter1, "Cancelled WIG1");
            Assert.AreEqual(3, counter2, "Normal WIG2");

            stp.Shutdown();
        }
  	}  
}
