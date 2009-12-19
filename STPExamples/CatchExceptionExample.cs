using System;
using System.Diagnostics;
using Amib.Threading;

namespace Examples
{
	public class CatchExceptionExample
	{
		private class DivArgs
		{
			public int x;
			public int y;
		}

		public void DoWork() 
		{ 
			SmartThreadPool smartThreadPool = new SmartThreadPool();

			DivArgs divArgs = new DivArgs();
			divArgs.x = 10;
			divArgs.y = 0;

			IWorkItemResult wir = 
				smartThreadPool.QueueWorkItem(new 
					WorkItemCallback(this.DoDiv), divArgs);

			try
			{
				int result = (int)wir.Result;
			}
			// Catch the exception that Result threw
			catch (WorkItemResultException e)
			{
				// Dump the inner exception which DoDiv threw
				Debug.WriteLine(e.InnerException);
			}

			smartThreadPool.Shutdown();
		} 

		private object DoDiv(object state)
		{ 
			DivArgs divArgs = (DivArgs)state;
			return (divArgs.x / divArgs.y);
		}
	}
}
