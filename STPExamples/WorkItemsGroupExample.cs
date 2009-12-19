using System;
using Amib.Threading;

namespace Examples
{
	public class WorkItemsGroupExample
	{
		public void DoWork(object [] states)
		{ 
			SmartThreadPool smartThreadPool = new SmartThreadPool();

			// Create a work items group that processes 
			// one work item at a time
			IWorkItemsGroup wig = smartThreadPool.CreateWorkItemsGroup(1);

			// Queue some work items 
			foreach(object state in states)
			{
				wig.QueueWorkItem(new WorkItemCallback(this.DoSomeWork), state);
			}

			// Wait for the completion of all work items in the work items group
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
