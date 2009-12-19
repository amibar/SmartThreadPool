using System;
using Amib.Threading;

namespace Examples
{
	public class GetExceptionExample
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

			Exception e;
			object obj = wir.GetResult(out e);
			// e contains the expetion that DoDiv threw
			if(null == e)
			{
				int result = (int)obj;
			}
			else
			{
				// Do something with the exception
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
