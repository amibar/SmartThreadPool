using System.Threading;

namespace Amib.Threading.Internal
{
    /// <summary>
    /// EventWaitHandleFactory class.
    /// This is a static class that creates AutoResetEvent and ManualResetEvent objects.
    /// In WindowCE the WaitForMultipleObjects API fails to use the Handle property 
    /// of XxxResetEvent. It can use only handles that were created by the CreateEvent API.
    /// Consequently this class creates the needed XxxResetEvent and replaces the handle if
    /// it's a WindowsCE OS.
    /// </summary>
    public static class EventWaitHandleFactory
    {
        /// <summary>
        /// Create a new AutoResetEvent object
        /// </summary>
        /// <returns>Return a new AutoResetEvent object</returns>
        public static AutoResetEvent CreateAutoResetEvent()
        {
            AutoResetEvent waitHandle = new AutoResetEvent(false);

            return waitHandle;
        }

        /// <summary>
        /// Create a new ManualResetEvent object
        /// </summary>
        /// <returns>Return a new ManualResetEvent object</returns>
        public static ManualResetEvent CreateManualResetEvent(bool initialState)
        {
            ManualResetEvent waitHandle = new ManualResetEvent(initialState);

            return waitHandle;
        }
    }
}
