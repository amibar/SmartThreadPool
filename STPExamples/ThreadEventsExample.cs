using System;
using System.Collections.Generic;
using System.Text;
using Amib.Threading;

namespace STPExamples
{
    public class MyResource : IDisposable
    {
        // ...

        public void DoIt() 
        { 
            //... 
        }

        public void Dispose()
        {
            //... 
        }
    }

    public class ThreadEventsExample
    {
        public static void Main()
        {

            SmartThreadPool stp = new SmartThreadPool();
            stp.OnThreadInitialization += new ThreadInitializationHandler(OnInitialization);
            stp.OnThreadTermination += new ThreadTerminationHandler(OnTermination);

            stp.QueueWorkItem(DoSomeWork);
        }

        [ThreadStatic]
        private static MyResource _resource;

        public static void OnInitialization()
        {
            // Initialize the resource
            _resource = new MyResource();
        }

        private static object DoSomeWork(object state)
        {
            // Use the resouce
            _resource.DoIt();

            return null;
        }

        public static void OnTermination()
        {
            // Do resource cleanup
            _resource.Dispose();
        }
    }
}
