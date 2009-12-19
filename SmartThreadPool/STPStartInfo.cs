using System;
using System.Threading;

namespace Amib.Threading
{
	/// <summary>
	/// Summary description for STPStartInfo.
	/// </summary>
    public class STPStartInfo : WIGStartInfo
    {
        private int _idleTimeout = SmartThreadPool.DefaultIdleTimeout;
        private int _minWorkerThreads = SmartThreadPool.DefaultMinWorkerThreads;
        private int _maxWorkerThreads = SmartThreadPool.DefaultMaxWorkerThreads;
        private ThreadPriority _threadPriority = SmartThreadPool.DefaultThreadPriority;
        private string _performanceCounterInstanceName = SmartThreadPool.DefaultPerformanceCounterInstanceName;
        private bool _enableLocalPerformanceCounters;

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
            _enableLocalPerformanceCounters = stpStartInfo._enableLocalPerformanceCounters;
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
        /// Enable/Disable the local performance counter.
        /// This enables the user to get some performance information about the SmartThreadPool 
        /// without using Windows performance counters. (Useful on WindowsCE, Silverlight, etc.)
        /// The default is false.
        /// </summary>
        public virtual bool EnableLocalPerformanceCounters
	    {
	        get { return _enableLocalPerformanceCounters; }
	        set 
			{ 
				ThrowIfReadOnly(); 
				_enableLocalPerformanceCounters = value; 
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
    }
}
