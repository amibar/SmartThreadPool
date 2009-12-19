using System;
using Amib.Threading;

namespace Examples
{
	public class WaitForAllExample
	{
		public void DoWork() 
		{ 
			SmartThreadPool smartThreadPool = new SmartThreadPool();

			IWorkItemResult wir1 = 
				smartThreadPool.QueueWorkItem(new 
				WorkItemCallback(this.DoSomeWork1), null);

			IWorkItemResult wir2 = 
				smartThreadPool.QueueWorkItem(new 
				WorkItemCallback(this.DoSomeWork2), null);

			bool success = SmartThreadPool.WaitAll(new IWorkItemResult [] { wir1, wir2 });

			if (success)
			{
				int result1 = (int)wir1.Result;
				int result2 = (int)wir2.Result;
			}

			smartThreadPool.Shutdown();
		} 

		private object DoSomeWork1(object state)
		{ 
			return 1;
		}

		private object DoSomeWork2(object state)
		{ 
			return 2;
		}
	}
}
