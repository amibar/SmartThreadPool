using System;
using Amib.Threading;

namespace Examples
{
	public class SuspendedWIGStartExample
	{
		public void DoWork(object [] states) 
		{ 
			SmartThreadPool smartThreadPool = new SmartThreadPool();

			WIGStartInfo wigStartInfo = new WIGStartInfo();
			wigStartInfo.StartSuspended = true;

			IWorkItemsGroup wig = smartThreadPool.CreateWorkItemsGroup(1, wigStartInfo);

			foreach(object state in states)
			{
				wig.QueueWorkItem(new 
					WorkItemCallback(this.DoSomeWork), state);
			}

			// Start working on the work items in the work items group queue
			wig.Start();

			// Wait for the completion of all work items
			wig.WaitForIdle();

			smartThreadPool.Shutdown();
		} 

		private object DoSomeWork(object state)
		{ 
			// Do the work
			return null;
		}
	}

}
