using System;
using System.Diagnostics;
using Amib.Threading;

namespace Examples
{
	public class OnWIGIdleEventExample
	{
		public void DoWork(object [] states) 
		{ 
			SmartThreadPool smartThreadPool = new SmartThreadPool();

			IWorkItemsGroup wig = smartThreadPool.CreateWorkItemsGroup(1);

            wig.OnIdle += wig_OnIdle;

			foreach(object state in states)
			{
				wig.QueueWorkItem(new 
					WorkItemCallback(this.DoSomeWork), state);
			}

			smartThreadPool.WaitForIdle();
			smartThreadPool.Shutdown();
		} 

		private object DoSomeWork(object state)
		{ 
			// Do the work
			return null;
		}

		private void wig_OnIdle(IWorkItemsGroup workItemsGroup)
		{
			Debug.WriteLine("WIG is idle");
		}
	}

}
