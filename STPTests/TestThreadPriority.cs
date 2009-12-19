
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
			SmartThreadPool stp = new SmartThreadPool();

			IWorkItemResult wir = stp.QueueWorkItem(new WorkItemCallback(DoSomeWork));
			ThreadPriority currentThreadPriority = (ThreadPriority)wir.GetResult();

			Assert.AreEqual(currentThreadPriority, SmartThreadPool.DefaultThreadPriority);
		}

		[Test]
		public void TestPriorities()
		{
			ThreadPriority [] priorities = 
				{ 
					ThreadPriority.Lowest, 
					ThreadPriority.BelowNormal, 
					ThreadPriority.Normal, 
					ThreadPriority.AboveNormal, 
					ThreadPriority.Highest, 
				};

			foreach(ThreadPriority priority in priorities)
			{
				CheckSinglePriority(priority);
			}
		}

		private void CheckSinglePriority(ThreadPriority threadPriority)
		{
			STPStartInfo stpStartInfo = new STPStartInfo();
			stpStartInfo.ThreadPriority = threadPriority;

			SmartThreadPool stp = new SmartThreadPool(stpStartInfo);

			IWorkItemResult wir = stp.QueueWorkItem(new WorkItemCallback(DoSomeWork));
			ThreadPriority currentThreadPriority = (ThreadPriority)wir.GetResult();

			Assert.AreEqual(currentThreadPriority, threadPriority);
		}

		private object DoSomeWork(object state)
		{ 
			return Thread.CurrentThread.Priority;
		}
	}
}