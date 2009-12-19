using System;
using System.Threading;

using Amib.Threading;

namespace Examples
{
	public class WaitForAnyExample
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

			IWorkItemResult [] wirs = new IWorkItemResult [] { wir1, wir2 };

			int index = SmartThreadPool.WaitAny(wirs);

			if (index != WaitHandle.WaitTimeout)
			{
				int result = (int)wirs[index].Result;
			}

			smartThreadPool.Shutdown();
		} 

		private object DoSomeWork1(object state)
		{
			return 1;
		}

		private object DoSomeWork2(object state)
		{ 
			return 1;
		}
	}
}
