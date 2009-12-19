using System;
using System.Threading;

using NUnit.Framework;

using Amib.Threading;

namespace SmartThreadPoolTests
{
    public class CallerState
    {
        public int Value { get; private set; }

        protected void IncValue()
        {
            ++Value;
        }
    }

    public class NonDisposableCallerState : CallerState
    {
        public NonDisposableCallerState()
        {
            IncValue();
        }
    }

    public class DisposableCallerState : CallerState, IDisposable
    {
        public DisposableCallerState()
        {
            IncValue();
        }

        #region IDisposable Members

        public void Dispose()
        {
            IncValue();
        }

        #endregion
    }


	/// <summary>
	/// Summary description for StateDisposeExample.
	/// </summary>
	[TestFixture]
	[Category("TestStateDispose")]
	public class TestStateDispose
	{
	    /// <summary>
        /// Example of non disposable caller state
        /// </summary>
        [Test]
        public void DisposeCallerState() 
        { 
            STPStartInfo stpStartInfo = new STPStartInfo();
            stpStartInfo.DisposeOfStateObjects = true;
     
            SmartThreadPool smartThreadPool = new SmartThreadPool(stpStartInfo);

            CallerState nonDisposableCallerState = new NonDisposableCallerState();
            CallerState disposableCallerState = new DisposableCallerState();

            IWorkItemResult wir1 = 
                smartThreadPool.QueueWorkItem(
                new WorkItemCallback(this.DoSomeWork), 
                nonDisposableCallerState);

            IWorkItemResult wir2 = 
                smartThreadPool.QueueWorkItem(
                new WorkItemCallback(this.DoSomeWork), 
                disposableCallerState);

            wir1.GetResult();
			Assert.AreEqual(1, nonDisposableCallerState.Value);

            wir2.GetResult();

			// Wait a little bit for the working thread to call dispose on the 
			// work item's state.
			smartThreadPool.WaitForIdle();

			Assert.AreEqual(2, disposableCallerState.Value);

            smartThreadPool.Shutdown();
        } 

        /// <summary>
        /// Example of non disposable caller state
        /// </summary>
        [Test]
        public void DontDisposeCallerState() 
        { 
            STPStartInfo stpStartInfo = new STPStartInfo();
            stpStartInfo.DisposeOfStateObjects = false;
     
            SmartThreadPool smartThreadPool = new SmartThreadPool(stpStartInfo);

            CallerState nonDisposableCallerState = new NonDisposableCallerState();
            CallerState disposableCallerState = new DisposableCallerState();

            IWorkItemResult wir1 = 
                smartThreadPool.QueueWorkItem(
                new WorkItemCallback(this.DoSomeWork), 
                nonDisposableCallerState);

            IWorkItemResult wir2 = 
                smartThreadPool.QueueWorkItem(
                new WorkItemCallback(this.DoSomeWork), 
                disposableCallerState);

            wir1.GetResult();
            bool success = (1 == nonDisposableCallerState.Value);

            wir2.GetResult();

            success = success && (1 == disposableCallerState.Value);

            smartThreadPool.Shutdown();

            Assert.IsTrue(success);
        } 

        private object DoSomeWork(object state)
        { 
            Thread.Sleep(1000);
            return 1;
        }
    }
}
