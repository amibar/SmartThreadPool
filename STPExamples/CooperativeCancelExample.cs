using System.Threading;
using Amib.Threading;

namespace Examples
{
	public class CooperativeCancelExample
	{
		public void DoWork(object state) 
		{ 
			SmartThreadPool smartThreadPool = new SmartThreadPool();

			// Queue the work item
			IWorkItemResult wir = smartThreadPool.QueueWorkItem(DoRealWork); 

            // Give the work item some time to complete.
            Thread.Sleep(1000);

            // If the work item hasn't completed yet then cancel it.
            if (!wir.IsCompleted)
            {
                wir.Cancel();
            }

			smartThreadPool.Shutdown();
		} 

		// Do some lengthy work
private void DoRealWork()
{
    // Do something here.

    // Sample SmartThreadPool.IsWorkItemCanceled
    if (SmartThreadPool.IsWorkItemCanceled)
    {
        return;
    }

    // Sample the SmartThreadPool.IsWorkItemCanceled in a loop
    while (!SmartThreadPool.IsWorkItemCanceled)
    {
        // Do some real work here
    }
}
	}
}
