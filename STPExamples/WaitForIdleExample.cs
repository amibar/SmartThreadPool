using System;
using Amib.Threading;

namespace Examples
{
	public class WaitForIdleExample
	{
		public void DoWork(object [] states) 
		{ 
			SmartThreadPool smartThreadPool = new SmartThreadPool();

			foreach(object state in states)
			{
				smartThreadPool.QueueWorkItem(new 
					WorkItemCallback(this.DoSomeWork), state);
			}

			// Wait for the completion of all work items
			smartThreadPool.WaitForIdle();

			smartThreadPool.Shutdown();
		} 

		private object DoSomeWork(object state)
		{ 
			// Do the work
			return null;
		}
	}
}
