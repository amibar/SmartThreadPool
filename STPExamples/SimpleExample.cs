using Amib.Threading;

namespace Examples
{
	public class SimpleExample
	{
		public void DoWork(object state) 
		{ 
			SmartThreadPool smartThreadPool = new SmartThreadPool();

			// Queue the work item
			IWorkItemResult wir = 
				smartThreadPool.QueueWorkItem(
				new WorkItemCallback(this.DoRealWork), 
				state); 

			// Do some other work here

			// Get the result of the operation
			object result = wir.Result;

			smartThreadPool.Shutdown();
		} 

		// Do the real work 
		private object DoRealWork(object state)
		{ 
			object result = null;

			// Do the real work here and put the result in 'result'

			return result;
		}
	}
}
