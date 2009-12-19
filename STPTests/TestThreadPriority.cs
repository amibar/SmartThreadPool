
using System;
using System.Threading;

using NUnit.Framework;

using Amib.Threading;

namespace SmartThreadPoolTests
{
	/// <summary>
	/// Summary description for TestThreadPriority.
	/// </summary>
	[TestFixture]
	[Category("TestThreadPriority")]
	public class TestThreadPriority
	{
		[Test]
		public void TestDefaultPriority()
		{
            CheckSinglePriority(SmartThreadPool.DefaultThreadPriority);
		}

		[Test]
		public void TestLowestPriority()
		{
            CheckSinglePriority(ThreadPriority.Lowest);
		}

        [Test]
        public void TestBelowNormalPriority()
		{
            CheckSinglePriority(ThreadPriority.BelowNormal);
		}

        [Test]
        public void TestNormalPriority()
		{
            CheckSinglePriority(ThreadPriority.BelowNormal);
		}

        [Test]
        public void TestAboveNormalPriority()
		{
            CheckSinglePriority(ThreadPriority.AboveNormal);
		} 
        
        [Test]
        public void TestHighestPriority()
		{
            CheckSinglePriority(ThreadPriority.Highest);
		}

		private void CheckSinglePriority(ThreadPriority threadPriority)
		{
			STPStartInfo stpStartInfo = new STPStartInfo();
			stpStartInfo.ThreadPriority = threadPriority;

			SmartThreadPool stp = new SmartThreadPool(stpStartInfo);

			IWorkItemResult wir = stp.QueueWorkItem(new WorkItemCallback(GetThreadPriority));
			ThreadPriority currentThreadPriority = (ThreadPriority)wir.GetResult();

            Assert.AreEqual(threadPriority, currentThreadPriority);
		}

		private object GetThreadPriority(object state)
		{ 
			return Thread.CurrentThread.Priority;
		}
	}
}