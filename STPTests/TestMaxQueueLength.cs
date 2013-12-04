using System;
using System.Threading;
using Amib.Threading;
using NUnit.Framework;

namespace STPTests
{
    [TestFixture]
    [Category("TestMaxQueueLength")]
    public class TestMaxQueueLength
    {
        [Test]
        public void QueueWorkItem_WhenMaxIsNull_Queues()
        {
            var info = new STPStartInfo
            {
                MaxQueueLength = null,
            };
            var pool = new SmartThreadPool(info);
            pool.Start();
            var workItem = pool.QueueWorkItem<object>(ReturnNull);

            // If rejected, an exception would have been thrown instead.
            Assert.IsTrue(workItem.GetResult() == null);
        }

        [Test]
        [ExpectedException(typeof(QueueRejectedException))]
        public void QueueWorkItem_WhenMaxIsSet_ThrowsExceptionWhenHit()
        {
            var info = new STPStartInfo
            {
                MaxQueueLength = 1,
                MinWorkerThreads = 1,
                MaxWorkerThreads = 1,
            };
            var pool = new SmartThreadPool(info);
            pool.Start();

            try
            {
                pool.QueueWorkItem(SleepForOneSecond); // Taken by waiter immediately. Not queued.

                Thread.Sleep(100); // A pause to get around any locks and make sure the waiter picked up the task.

                pool.QueueWorkItem(SleepForOneSecond); // No waiters available, pool at max threads. Queued.
            }
            catch (QueueRejectedException e)
            {
                throw new Exception("Caught QueueRejectedException too early: ", e);
            }

            // No waiters available, queue is at max (1). Throws.
            pool.QueueWorkItem(SleepForOneSecond);
        }

        [Test, RequiresThread]
        [ExpectedException(typeof (QueueRejectedException))]
        public void QueueWorkItem_WhenBiggerMaxIsSet_ThrowsExceptionWhenHit()
        {
            var info = new STPStartInfo
            {
                MaxQueueLength = 5,
                MinWorkerThreads = 5,
                MaxWorkerThreads = 10,
            };
            var pool = new SmartThreadPool(info);
            pool.Start();

            try
            {
                // Pool starts with 5 available waiters.

                pool.QueueWorkItem(SleepForOneSecond); // Taken by waiter immediately. Not queued.
                pool.QueueWorkItem(SleepForOneSecond); // Taken by waiter immediately. Not queued.
                pool.QueueWorkItem(SleepForOneSecond); // Taken by waiter immediately. Not queued.
                pool.QueueWorkItem(SleepForOneSecond); // Taken by waiter immediately. Not queued.
                pool.QueueWorkItem(SleepForOneSecond); // Taken by waiter immediately. Not queued.

                pool.QueueWorkItem(SleepForOneSecond); // New thread created, takes work item. Not queued.
                pool.QueueWorkItem(SleepForOneSecond); // New thread created, takes work item. Not queued.
                pool.QueueWorkItem(SleepForOneSecond); // New thread created, takes work item. Not queued.
                pool.QueueWorkItem(SleepForOneSecond); // New thread created, takes work item. Not queued.
                pool.QueueWorkItem(SleepForOneSecond); // New thread created, takes work item. Not queued.

                Thread.Sleep(100); // A pause to get around any locks and make sure the waiters picked everything up.

                pool.QueueWorkItem(SleepForOneSecond); // No waiters available. Queued.
                pool.QueueWorkItem(SleepForOneSecond); // No waiters available. Queued.
                pool.QueueWorkItem(SleepForOneSecond); // No waiters available. Queued.
                pool.QueueWorkItem(SleepForOneSecond); // No waiters available. Queued.
                pool.QueueWorkItem(SleepForOneSecond); // No waiters available. Queued.
            }
            catch (QueueRejectedException e)
            {
                throw new Exception("Caught QueueRejectedException too early: ", e);
            }

            // All threads are busy, and queue is at its max. Throws.
            pool.QueueWorkItem(SleepForOneSecond);
        }

        private object ReturnNull()
        {
            return null;
        }

        private void SleepForOneSecond()
        {
            Thread.Sleep(1000);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void StpStartInfo_WithNegativeMaxQueueLength_FailsValidation()
        {
            var info = new STPStartInfo
            {
                MaxQueueLength = -1,
            };
            new SmartThreadPool(info);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void StpStartInfo_WithZeroMaxQueueLength_FailsValidation()
        {
            var info = new STPStartInfo
            {
                MaxQueueLength = 0,
            };
            new SmartThreadPool(info);
        }
    }
}
