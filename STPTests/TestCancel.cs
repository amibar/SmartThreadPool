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
        [ExpectedException(typeof(WorkItemCancelException))]
        public void CancelInQueueWorkItem()
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
        }

        /// <summary>
        /// 1. Create STP
        /// 2. Queue work item that takes some time
        /// 3. Wait for it to start
        /// 4. Cancel the work item (soft)
        /// 5. Work item's GetResult should throw WorkItemCancelException
        /// </summary>        
        [Test]
        [ExpectedException(typeof(WorkItemCancelException))]
        public void CancelInProgressWorkItemSoft()
        {
            ManualResetEvent waitToStart = new ManualResetEvent(false);

            SmartThreadPool stp = new SmartThreadPool();
            IWorkItemResult wir = stp.QueueWorkItem(
                 state => { waitToStart.Set();  Thread.Sleep(100); return null; }
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
        [ExpectedException(typeof(WorkItemCancelException))]
        public void CancelInProgressWorkItemAbort()
        {
            ManualResetEvent waitToStart = new ManualResetEvent(false);
            int counter = 0;

            SmartThreadPool stp = new SmartThreadPool();
            IWorkItemResult wir = stp.QueueWorkItem(
                state => { waitToStart.Set() ; Thread.Sleep(100); ++counter; return null; }
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
        [ExpectedException(typeof(WorkItemCancelException))]
        public void CancelInProgressWorkItemSoftWithIgnoreSample()
        {
            ManualResetEvent waitToStart = new ManualResetEvent(false);
            ManualResetEvent waitToComplete = new ManualResetEvent(false);

            SmartThreadPool stp = new SmartThreadPool();
            IWorkItemResult wir = stp.QueueWorkItem(
                state => {
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

            wir.GetResult();

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
        [ExpectedException(typeof(WorkItemCancelException))]
        public void CancelCanceledWorkItem()
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

        //////////////////////////////////////////////////////////////////////////////////////////////////
/*
        private int _counter;

        /// <summary>
		/// Example of how to queue a work item and then cancel it while it is in the queue.
		/// </summary>
		[Test]
        [ExpectedException(typeof(WorkItemCancelException))]
		public void WorkItemCanceling() 
		{ 
			// Create a SmartThreadPool with only one thread.
			// It just to show how to use the work item canceling feature
			SmartThreadPool smartThreadPool = new SmartThreadPool(10*1000, 1);

		    // Queue a work item that will occupy the thread in the pool
			IWorkItemResult wir1 = 
				smartThreadPool.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);

			// Queue another work item that will wait for the first to complete
			IWorkItemResult wir2 = 
				smartThreadPool.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);

			// Wait a while for the thread pool to start executing the first work item
			Thread.Sleep(100);

            try
            {
                // The first work item cannot be canceled since it is currently executing
                if (!wir1.Cancel())
                {
                    // Cancel the second work item while it still in the queue
                    if (wir2.Cancel())
                    {
                        // Retreiving result of a canceled work item throws WorkItemCancelException exception
                        wir2.GetResult();
                    }
                }
            }
            finally
            {
                smartThreadPool.Shutdown();
            }
		} 

		/// <summary>
		/// </summary>
		[Test]
		public void WorkItemCancelingAndInUseWorkerThreads() 
		{ 
			SmartThreadPool smartThreadPool = new SmartThreadPool(10*1000, 10);

			IWorkItemResult [] wirs = new IWorkItemResult[100];
			for(int i = 0; i < 100; ++i)
			{
				wirs[i] = smartThreadPool.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), null);
			}

			// Wait a while for the thread pool to start executing the first work item
			Thread.Sleep(100);

			for(int i = 0; i < 100; ++i)
			{
				wirs[i].Cancel();
			}

			smartThreadPool.WaitForIdle(2000);

			int inUseThreads = smartThreadPool.InUseThreads;

			smartThreadPool.Shutdown();

			Assert.AreEqual(0, inUseThreads);
		}

		private object DoSomeWork(object state)
		{
            Thread.Sleep(1000);
            return 1;
		}

        /// <summary>
        /// Check within the work item if it was cancelled
        /// </summary>
        [Test]
        public void SampleIfWorkItemCancelled()
        {
            _counter = 0;
            STPStartInfo stpStartInfo = new STPStartInfo();
            stpStartInfo.StartSuspended = true;

            // Create a SmartThreadPool with only one thread.
            SmartThreadPool smartThreadPool = new SmartThreadPool(stpStartInfo);

            IWorkItemResult[] wirs = new IWorkItemResult[100];
            for (int i = 0; i < 100; ++i)
            {
                wirs[i] = smartThreadPool.QueueWorkItem(new WorkItemCallback(this.DoCheckForCancelledWorkItem), null);
            }

            for (int i = 0; i < 50; ++i)
            {
                wirs[i].Cancel();
            }

            smartThreadPool.Start();

            smartThreadPool.WaitForIdle();

            smartThreadPool.Shutdown();

            Assert.AreEqual(50, _counter);
        }


        /// <summary>
        /// 1. Create STP
        /// 2. Queue work item into the STP
        /// 4. Cancel the work item
        /// 5. Work item doesn't check for cancel 
        /// 6. Work item quits
        /// 7. Make sure the work item result is ok, and not an exception
        /// </summary>
        [Test]
        public void TestWorkItemCancelledWorkItemOK()
        {
            // Create a SmartThreadPool with only one thread.
            SmartThreadPool smartThreadPool = new SmartThreadPool();

            AutoResetEvent start = new AutoResetEvent(false);

            // Queue the work item
            IWorkItemResult wir = smartThreadPool.QueueWorkItem(new WorkItemCallback(this.DoWaitForCancel), start);

            // Wait for it to start executing
            bool success = start.WaitOne(3000, false);

            // Make sure it was started
            Assert.IsTrue(success);

            // Cancel the work item
            wir.Cancel();

            // Let it complete
            start.Set();

            // Wait for the work item to complete
            smartThreadPool.WaitForIdle();

            // Check the work item's result
            // The work item started running after it was start its execution.
            // Since the work item didn't sample the cancel, it was not aware that it 
            // was canceled. Therefore the result should be what the work item returned
            // and not the cancel exeception
            int result = (int)wir.GetResult(0, false);

            smartThreadPool.Shutdown();

            Assert.AreEqual(1, result);
        }


        /// <summary>
        /// 1. Create STP
        /// 2. Create WIG
        /// 3. Queue work item into the WIG
        /// 4. Cancel the work items in the WIG
        /// 5. Work item doesn't check for cancel 
        /// 6. Work item quits
        /// 7. Make sure the work item result is ok, and not an exception
        /// </summary>
        [Test]
        public void TestWIGCancelledWorkItemOK()
        {
            // Create a SmartThreadPool with only one thread.
            SmartThreadPool smartThreadPool = new SmartThreadPool();

            IWorkItemsGroup wig = smartThreadPool.CreateWorkItemsGroup(1);

            AutoResetEvent start = new AutoResetEvent(false);

            // Queue the work item
            IWorkItemResult wir = wig.QueueWorkItem(new WorkItemCallback(this.DoWaitForCancel), start);

            // Wait for it to start executing
            bool success = start.WaitOne(3000, false);

            // Make sure it was started
            Assert.IsTrue(success);

            // Cancel the work item
            wig.Cancel();

            // Let it complete
            start.Set();

            // Wait for the work item to complete
            smartThreadPool.WaitForIdle();

            // Check the work item's result
            // The work item started running after it was start its execution.
            // Since the work item didn't sample the cancel, it was not aware that it 
            // was canceled. Therefore the result should be what the work item returned
            // and not the cancel exeception
            int result = (int)wir.GetResult(0, false);

            smartThreadPool.Shutdown();

            Assert.AreEqual(1, result);
        }


        /// <summary>
        /// 1. Create STP
        /// 2. Queue work item into the STP
        /// 3. Cancel the work items in the STP
        /// 4. Work item doesn't check for cancel 
        /// 5. Work item quits
        /// 6. Make sure the work item result is ok, and not an exception
        /// </summary>        
        [Test]
        public void TestSTPCancelledWorkItemOK()
        {
            // Create a SmartThreadPool with only one thread.
            SmartThreadPool smartThreadPool = new SmartThreadPool();

            AutoResetEvent start = new AutoResetEvent(false);

            // Queue the work item
            IWorkItemResult wir = smartThreadPool.QueueWorkItem(new WorkItemCallback(this.DoWaitForCancel), start);

            // Wait for it to start executing
            bool success = start.WaitOne(3000, false);

            // Make sure it was started
            Assert.IsTrue(success);

            // Cancel the work item
            smartThreadPool.Cancel();

            // Let it complete
            start.Set();

            // Wait for the work item to complete
            smartThreadPool.WaitForIdle();

            // Check the work item's result
            // The work item started running after it was start its execution.
            // Since the work item didn't sample the cancel, it was not aware that it 
            // was canceled. Therefore the result should be what the work item returned
            // and not the cancel exeception
            int result = (int)wir.GetResult(0, false);

            smartThreadPool.Shutdown();

            Assert.AreEqual(1, result);
        }


        /// <summary>
        /// 1. Create STP
        /// 2. Queue work item
        /// 3. Cancel the work item
        /// 4. Work item checks the cancel and quits
        /// 5. Make sure the work item result throws exception
        /// </summary>
        [Test]
        [ExpectedException(typeof(WorkItemCancelException))]
        public void TestWorkItemCanceledWorkItemCancelException()
        {
            // Create a SmartThreadPool with only one thread.
            SmartThreadPool smartThreadPool = new SmartThreadPool();

            AutoResetEvent start = new AutoResetEvent(false);

            // Queue the work item
            IWorkItemResult wir = smartThreadPool.QueueWorkItem(new WorkItemCallback(this.DoCheckForCancel), start);

            // Wait for it to start executing
            bool success = start.WaitOne(3000, false);

            // Make sure it was started
            Assert.IsTrue(success);

            // Cancel the work item
            wir.Cancel();

            // Let it complete
            start.Set();

            // Wait for the work item to complete
            smartThreadPool.WaitForIdle();

            try
            {
                // Check the work item's result
                // The work item started running after it was start its execution.
                // The work item samples the cancel, therefore it is aware that it 
                // was canceled. Using the GetResult should throw the cancel exeception
                wir.GetResult(0, false);
            }
            finally
            {
                smartThreadPool.Shutdown();
            }
        }

        /// <summary>
        /// 1. Create STP
        /// 2. Create WIG
        /// 3. Queue work item into the WIG
        /// 4. Cancel the work items in the WIG
        /// 5. Work item checks the cancel and quits
        /// 6. Make sure the work item result throws exception
        /// </summary>
        [Test]
        [ExpectedException(typeof(WorkItemCancelException))]
        public void TestWIGCancelledWorkItemCancelException()
        {
            // Create a SmartThreadPool with only one thread.
            SmartThreadPool smartThreadPool = new SmartThreadPool();

            IWorkItemsGroup wig = smartThreadPool.CreateWorkItemsGroup(1);

            AutoResetEvent start = new AutoResetEvent(false);

            // Queue the work item
            IWorkItemResult wir = wig.QueueWorkItem(new WorkItemCallback(this.DoCheckForCancel), start);

            // Wait for it to start executing
            bool success = start.WaitOne(3000, false);

            // Make sure it was started
            Assert.IsTrue(success);

            // Cancel the work item
            wig.Cancel();

            // Let it complete
            start.Set();

            // Wait for the work item to complete
            smartThreadPool.WaitForIdle();

            try
            {
                // Check the work item's result
                // The work item started running after it was start its execution.
                // The work item samples the cancel, therefore it is aware that it 
                // was canceled. Using the GetResult should throw the cancel exeception
                wir.GetResult(0, false);
            }
            finally
            {
                smartThreadPool.Shutdown();
            }
        }

        /// <summary>
        /// 1. Create STP
        /// 3. Queue work item into the STP
        /// 4. Cancel the work items in the STP
        /// 5. Work item checks the cancel and quits
        /// 6. Make sure the work item result throws exception
        /// </summary>
        [Test]
        [ExpectedException(typeof(WorkItemCancelException))]
        public void TestSTPCancelledWorkItemCancelException()
        {
            // Create a SmartThreadPool with only one thread.
            SmartThreadPool smartThreadPool = new SmartThreadPool();

            AutoResetEvent start = new AutoResetEvent(false);

            // Queue the work item
            IWorkItemResult wir = smartThreadPool.QueueWorkItem(new WorkItemCallback(this.DoCheckForCancel), start);

            // Wait for it to start executing
            bool success = start.WaitOne(3000, false);

            // Make sure it was started
            Assert.IsTrue(success);

            // Cancel the work item
            smartThreadPool.Cancel();

            // Let it complete
            start.Set();

            // Wait for the work item to complete
            smartThreadPool.WaitForIdle();

            try
            {
                // Check the work item's result
                // The work item started running after it was start its execution.
                // The work item samples the cancel, therefore it is aware that it 
                // was canceled. Using the GetResult should throw the cancel exeception
                wir.GetResult(0, false);

            }
            finally
            {
                smartThreadPool.Shutdown();
            }
        }

        private object DoCheckForCancelledWorkItem(object state)
        {
            if (!SmartThreadPool.IsWorkItemCanceled)
            {
                Interlocked.Increment(ref _counter);
            }

            return null;
        }

        private object DoWaitForCancel(object state)
        {
            AutoResetEvent start = state as AutoResetEvent;

            // Signal the test that the work item started
            start.Set();

            // Let the test run (or else the next line may reset the signaled event)
            Thread.Sleep(10);

            // Wait for the test to cancel the work item
            start.WaitOne();

            return 1;
        }

        private object DoCheckForCancel(object state)
        {
            AutoResetEvent start = state as AutoResetEvent;

            // Signal the test that the work item started
            start.Set();

            // Let the test run (or else the next line may reset the signaled event)
            Thread.Sleep(10);

            // Wait for the test to cancel the work item
            start.WaitOne();

            // Sample if the work item was cancelled
            bool cancelled = SmartThreadPool.IsWorkItemCanceled;

            return 1;
        }
 */
  	}
  
}
