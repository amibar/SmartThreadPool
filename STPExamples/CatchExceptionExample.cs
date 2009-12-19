using System;
using System.Diagnostics;
using Amib.Threading;

namespace Examples
{
	public class CatchExceptionExample
	{
        public void DoWork()
        {
            SmartThreadPool smartThreadPool = new SmartThreadPool();

            IWorkItemResult<double> wir = smartThreadPool.QueueWorkItem(new Func<double, double, double>(DoDiv), 10.0, 0.0);

            try
            {
                double result = wir.Result;
            }
            // Catch the exception that Result threw
            catch (WorkItemResultException e)
            {
                // Dump the inner exception which DoDiv threw
                Debug.WriteLine(e.InnerException);
            }

            smartThreadPool.Shutdown();
        }

        private double DoDiv(double x, double y)
        {
            return x / y;
        }
    }
}
