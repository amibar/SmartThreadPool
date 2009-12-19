using System;

using NUnit.Framework;

using Amib.Threading;

namespace WorkItemsGroupTests
{
	/// <summary>
	/// Summary description for TestExceptions.
	/// </summary>
	[TestFixture]
	[Category("Test WorkItemsGroup Exceptions")]
	public class TestExceptions
	{
		private class DivArgs
		{
			public int x;
			public int y;
		}

		[Test]
		public void ExceptionThrowing() 
		{ 
			SmartThreadPool smartThreadPool = new SmartThreadPool();
			IWorkItemsGroup workItemsGroup = smartThreadPool.CreateWorkItemsGroup(int.MaxValue);

			DivArgs divArgs = new DivArgs();
			divArgs.x = 10;
			divArgs.y = 0;

			IWorkItemResult wir = 
				workItemsGroup.QueueWorkItem(new WorkItemCallback(this.DoDiv), divArgs);

			try
			{
				wir.GetResult();
			}
			catch(WorkItemResultException wire)
			{
				Assert.IsTrue(wire.InnerException is DivideByZeroException);
				return;
			}
			catch(Exception e)
			{
                e.GetHashCode();
				Assert.Fail();
			}
			Assert.Fail();
		} 

		[Test]
		public void ExceptionReturning() 
		{ 
			bool success = true;

			SmartThreadPool smartThreadPool = new SmartThreadPool();
			IWorkItemsGroup workItemsGroup = smartThreadPool.CreateWorkItemsGroup(int.MaxValue);

			DivArgs divArgs = new DivArgs();
			divArgs.x = 10;
			divArgs.y = 0;

			IWorkItemResult wir = 
				workItemsGroup.QueueWorkItem(new WorkItemCallback(this.DoDiv), divArgs);

			Exception e = null;
			try
			{
				wir.GetResult(out e);
			}
			catch (Exception ex)
			{
                ex.GetHashCode();
				success = false;
			}

			Assert.IsTrue(success);
			Assert.IsTrue(e is DivideByZeroException);
		} 

		private object DoDiv(object state)
		{ 
			DivArgs divArgs = (DivArgs)state;
			return (divArgs.x / divArgs.y);
		}

	}
}
