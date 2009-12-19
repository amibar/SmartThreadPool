using System;
using Amib.Threading;

namespace Examples
{
	public class SimpleExample
	{
        public void DoWork(int[] numbers)
        {
            SmartThreadPool smartThreadPool = new SmartThreadPool();

            // Queue the work item
            IWorkItemResult<double> wir = smartThreadPool.QueueWorkItem(new Func<int[], double>(CalcAverage), numbers);

            // Do some other work here

            // Get the result of the operation
            double average = wir.Result;

            smartThreadPool.Shutdown();
        }

        // Do the real work 
        private double CalcAverage(int[] numbers)
        {
            double average = 0.0;

            // Do the real work here and put 
            // the result in 'result'

            return average;
        }
    }
}
