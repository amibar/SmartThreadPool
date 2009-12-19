using System;
using System.Threading;

namespace Amib.Threading
{
	/// <summary>
	/// Summary description for STPStartInfo.
	/// </summary>
    public class STPStartInfo : WIGStartInfo
    {
        private int _idleTimeout;
        private int _minWorkerThreads;
        private int _maxWorkerThreads;
        private ThreadPriority _threadPriority;
        private string _performanceCounterInstanceName;

	    public STPStartInfo()
        {
            _performanceCounterInstanceName = SmartThreadPool.DefaultPerformanceCounterInstanceName;
            _threadPriority = SmartThreadPool.DefaultThreadPriority;
            _maxWorkerThreads = SmartThreadPool.DefaultMaxWorkerThreads;
            _idleTimeout = SmartThreadPool.DefaultIdleTimeout;
            _minWorkerThreads = SmartThreadPool.DefaultMinWorkerThreads;
        }

	    public STPStartInfo(STPStartInfo stpStartInfo)
            : base(stpStartInfo)
        {
            _idleTimeout = stpStartInfo.IdleTimeout;
            _minWorkerThreads = stpStartInfo.MinWorkerThreads;
            _maxWorkerThreads = stpStartInfo.MaxWorkerThreads;
            _threadPriority = stpStartInfo.ThreadPriority;
            _performanceCounterInstanceName = stpStartInfo.PerformanceCounterInstanceName;
        }

	  
	    /// <summary>
	    /// Get/Set the idle timeout in milliseconds.
	    /// If a thread is idle (starved) longer than IdleTimeout then it may quit.
	    /// </summary>
	    public virtual int IdleTimeout
	    {
	        get { return _idleTimeout; }
	        set 
            {
                ThrowIfReadOnly();
                _idleTimeout = value; 
            }
	    }


	    /// <summary>
	    /// Get/Set the lower limit of threads in the pool.
	    /// </summary>
	    public virtual int MinWorkerThreads
	    {
	        get { return _minWorkerThreads; }
	        set 
            {
                ThrowIfReadOnly();
                _minWorkerThreads = value; 
            }
	    }


	    /// <summary>
	    /// Get/Set the upper limit of threads in the pool.
	    /// </summary>
	    public virtual int MaxWorkerThreads
	    {
	        get { return _maxWorkerThreads; }
	        set 
            {
                ThrowIfReadOnly();
                _maxWorkerThreads = value; 
            }
	    }


	    /// <summary>
	    /// Get/Set the scheduling priority of the threads in the pool.
	    /// The Os handles the scheduling.
	    /// </summary>
	    public virtual ThreadPriority ThreadPriority
	    {
	        get { return _threadPriority; }
	        set 
            {
                ThrowIfReadOnly();
                _threadPriority = value; 
            }
	    }

	    /// <summary>
	    /// Get/Set the performance counter instance name of this SmartThreadPool
	    /// The default is null which indicate not to use performance counters at all.
	    /// </summary>
	    public virtual string PerformanceCounterInstanceName
	    {
	        get { return _performanceCounterInstanceName; }
	        set 
            {
                ThrowIfReadOnly();
                _performanceCounterInstanceName = value; 
            }
	    }

	    /// <summary>
        /// Get a readonly version of this STPStartInfo.
        /// </summary>
        /// <returns>Returns a readonly reference to this STPStartInfo</returns>
        public new STPStartInfo AsReadOnly()
        {
            return new STPStartInfo(this) { _readOnly = true };
        }
/*
        #region STPStartInfoRO class

        private class STPStartInfoRO : STPStartInfo
        {
            private readonly STPStartInfo _stpStartInfo;

            public STPStartInfoRO(STPStartInfo stpStartInfo)
            {
                _stpStartInfo = stpStartInfo;
            }

            /// <summary>
            /// Get/Set the idle timeout in milliseconds.
            /// If a thread is idle (starved) longer than IdleTimeout then it may quit.
            /// </summary>
            public override int IdleTimeout
            {
                get { return _stpStartInfo.IdleTimeout; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }

            /// <summary>
            /// Get the lower limit of threads in the pool.
            /// </summary>
            public override int MinWorkerThreads
            {
                get { return _stpStartInfo.MinWorkerThreads; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }

            /// <summary>
            /// Get the upper limit of threads in the pool.
            /// </summary>
            public override int MaxWorkerThreads
            {
                get { return _stpStartInfo.MaxWorkerThreads; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }

            /// <summary>
            /// Get the scheduling priority of the threads in the pool.
            /// The Os handles the scheduling.
            /// </summary>
            public override ThreadPriority ThreadPriority
            {
                get { return _stpStartInfo.ThreadPriority; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }

            /// <summary>
            /// Get the performance counter instance name of this SmartThreadPool
            /// The default is null which indicate not to use performance counters at all.
            /// </summary>
            public override string PerformanceCounterInstanceName
            {
                get { return _stpStartInfo.PerformanceCounterInstanceName; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }

            /// <summary>
            /// Get if to use the caller's security context
            /// </summary>
            public override bool UseCallerCallContext
            {
                get { return _stpStartInfo.UseCallerCallContext; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }

            /// <summary>
            /// Get if to use the caller's HTTP context
            /// </summary>
            public override bool UseCallerHttpContext
            {
                get { return _stpStartInfo.UseCallerHttpContext; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }

            /// <summary>
            /// Get if to dispose of the state object of a work item
            /// </summary>
            public override bool DisposeOfStateObjects
            {
                get { return _stpStartInfo.DisposeOfStateObjects; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }

            /// <summary>
            /// Get the run the post execute options
            /// </summary>
            public override CallToPostExecute CallToPostExecute
            {
                get { return _stpStartInfo.CallToPostExecute; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }

            /// <summary>
            /// Get the default post execute callback
            /// </summary>
            public override PostExecuteWorkItemCallback PostExecuteWorkItemCallback
            {
                get { return _stpStartInfo.PostExecuteWorkItemCallback; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }

            /// <summary>
            /// Get if the work items execution should be suspended until the Start()
            /// method is called.
            /// </summary>
            public override bool StartSuspended
            {
                get { return _stpStartInfo.StartSuspended; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }

            /// <summary>
            /// Get the default priority that a work item gets when it is enqueued
            /// </summary>
            public override WorkItemPriority WorkItemPriority
            {
                get { return _stpStartInfo.WorkItemPriority; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }

            /// <summary>
            /// Indicate if QueueWorkItem of Action<...>/Func<...> fill the
            /// arguments as an object array into the state of the work item.
            /// The arguments can be access later by IWorkItemResult.State.
            /// </summary>
            public override bool FillStateWithArgs
            {
                get { return _stpStartInfo.FillStateWithArgs; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }
        }

        #endregion
*/
    }
}
