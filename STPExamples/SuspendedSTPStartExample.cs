using System;
using Amib.Threading;

namespace Examples
{
	public class SuspendedSTPStartExample
	{
		public void DoWork(object [] states) 
		{ 
			STPStartInfo stpStartInfo = new STPStartInfo();
			stpStartInfo.StartSuspended = true;

			SmartThreadPool smartThreadPool = new SmartThreadPool(stpStartInfo);

			foreach(object state in states)
			{
				smartThreadPool.QueueWorkItem(new 
					WorkItemCallback(this.DoSomeWork), state);
			}

			// Start working on the work items in the queue
			smartThreadPool.Start();

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
