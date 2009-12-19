using System;
using System.Diagnostics;
using Amib.Threading;

namespace Examples
{
	public class PriorityExample
	{
		public void DoWork() 
		{ 
			STPStartInfo stpStartInfo = new STPStartInfo();
			stpStartInfo.StartSuspended = true;

			SmartThreadPool smartThreadPool = new SmartThreadPool();

			smartThreadPool.QueueWorkItem(
				new WorkItemCallback(this.DoSomeWork), 
				"Queued first",
				WorkItemPriority.BelowNormal);

			smartThreadPool.QueueWorkItem(
				new WorkItemCallback(this.DoSomeWork), 
				"Queued second",
				WorkItemPriority.AboveNormal);

			smartThreadPool.Start();

			smartThreadPool.WaitForIdle();

			smartThreadPool.Shutdown();
		} 

		private object DoSomeWork(object state)
		{ 
			Debug.WriteLine(state);
			return null;
		}

	}
}
