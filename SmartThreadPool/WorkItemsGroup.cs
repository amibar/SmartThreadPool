using System;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Amib.Threading.Internal
{
    #region WorkItemsGroup class 

	/// <summary>
	/// Summary description for WorkItemsGroup.
	/// </summary>
	public class WorkItemsGroup : WorkItemsGroupBase
	{
		#region Private members

		private readonly object _lock = new object();

		/// <summary>
		/// A reference to the SmartThreadPool instance that created this 
		/// WorkItemsGroup.
		/// </summary>
		private readonly SmartThreadPool _stp;

		/// <summary>
		/// The OnIdle event
		/// </summary>
		private event WorkItemsGroupIdleHandler _onIdle;

        /// <summary>
        /// A flag to indicate if the Work Items Group is now suspended.
        /// </summary>
        private bool _isSuspended;

		/// <summary>
		/// Defines how many work items of this WorkItemsGroup can run at once.
		/// </summary>
		private int _concurrency;

		/// <summary>
		/// Priority queue to hold work items before they are passed 
		/// to the SmartThreadPool.
		/// </summary>
		private readonly PriorityQueue _workItemsQueue;

		/// <summary>
		/// Indicate how many work items are waiting in the SmartThreadPool
		/// queue.
		/// This value is used to apply the concurrency.
		/// </summary>
		private int _workItemsInStpQueue;

		/// <summary>
		/// Indicate how many work items are currently running in the SmartThreadPool.
		/// This value is used with the Cancel, to calculate if we can send new 
		/// work items to the STP.
		/// </summary>
		private int _workItemsExecutingInStp = 0;

        /// <summary>
        /// The number of work items this WorkItemsGroup is responsible on.
        /// A work item is attached to a WorkItemsGroup since it is enqueued until it is completed (or cancelled)
        /// We need this count in order to know if the WorkItemsGroup is idle. A WorkItemsGroup with all its
        /// work items awaiting is not idle
        /// </summary>
        private int _attachedWorkItemsCount = 0;

		/// <summary>
		/// WorkItemsGroup start information
		/// </summary>
		private readonly WIGStartInfo _workItemsGroupStartInfo;

		/// <summary>
		/// Signaled when all of the WorkItemsGroup's work item completed.
		/// </summary>
        //private readonly ManualResetEvent _isIdleWaitHandle = new ManualResetEvent(true);
        private readonly ManualResetEvent _isIdleWaitHandle = EventWaitHandleFactory.CreateManualResetEvent(true);

#if _ASYNC_SUPPORTED
        /// <summary>
        /// A task completion source to indicate that the WorkItemsGroup is idle, used in WaitForIdleAsync
        /// </summary>
        private TaskCompletionSource<bool> _isIdleTCS;
#endif
		/// <summary>
		/// A common object for all the work items that this work items group
		/// generate so we can mark them to cancel in O(1)
		/// </summary>
		private CanceledWorkItemsGroup _canceledWorkItemsGroup = new CanceledWorkItemsGroup();

		#endregion

		#region Construction

		public WorkItemsGroup(
			SmartThreadPool stp, 
			int concurrency, 
			WIGStartInfo wigStartInfo)
		{
			if (concurrency <= 0)
			{
				throw new ArgumentOutOfRangeException(
                    "concurrency",
                    concurrency,
                    "concurrency must be greater than zero");
			}
			_stp = stp;
			_concurrency = concurrency;
			_workItemsGroupStartInfo = new WIGStartInfo(wigStartInfo).AsReadOnly();
			_workItemsQueue = new PriorityQueue();
	        Name = "WorkItemsGroup";

			// The _workItemsInStpQueue gets the number of currently executing work items,
			// because once a work item is executing, it cannot be cancelled.
			_workItemsInStpQueue = _workItemsExecutingInStp;

            _isSuspended = _workItemsGroupStartInfo.StartSuspended;
		}

		#endregion 

        #region WorkItemsGroupBase Overrides

        public override int Concurrency
        {
            get { return _concurrency; }
            set
            {
                Debug.Assert(value > 0);

                int diff = value - _concurrency;
                _concurrency = value;
                if (diff > 0)
                {
                    EnqueueToSTPNextNWorkItem(diff);
                }
            }
        }

        public override int InUseThreads
        {
            get
            {
                return _workItemsExecutingInStp;
            }
        }

        public override int WaitingCallbacks
        {
            get { return _workItemsQueue.Count; }
        }

        public override object[] GetStates()
        {
            lock (_lock)
            {
                object[] states = new object[_workItemsQueue.Count];
                int i = 0;
                foreach (WorkItem workItem in _workItemsQueue)
                {
                    states[i] = workItem.GetWorkItemResult().State;
                    ++i;
                }
                return states;
            }
        }

	    /// <summary>
        /// WorkItemsGroup start information
        /// </summary>
        public override WIGStartInfo WIGStartInfo
        {
            get { return _workItemsGroupStartInfo; }
        }

	    /// <summary>
	    /// Start the Work Items Group if it was started suspended
	    /// </summary>
	    public override void Start()
	    {
	        // If the Work Items Group already started then quit
	        if (!_isSuspended)
	        {
	            return;
	        }
	        _isSuspended = false;
            
	        EnqueueToSTPNextNWorkItem(Math.Min(_workItemsQueue.Count, _concurrency));
	    }

	    public override void Cancel(bool abortExecution)
	    {
	        lock (_lock)
	        {
                _attachedWorkItemsCount -= _workItemsQueue.Count;
                _canceledWorkItemsGroup.IsCanceled = true;
	            _workItemsQueue.Clear();
	            _workItemsInStpQueue = 0;
	            _canceledWorkItemsGroup = new CanceledWorkItemsGroup();
            }

	        if (abortExecution)
	        {
	            _stp.CancelAbortWorkItemsGroup(this);
	        }
	    }

	    /// <summary>
        /// Wait for the thread pool to be idle
        /// </summary>
        public override bool WaitForIdle(int millisecondsTimeout)
        {
            SmartThreadPool.ValidateWorkItemsGroupWaitForIdle(this);
            return STPEventWaitHandle.WaitOne(_isIdleWaitHandle, millisecondsTimeout, false);
        }

#if _ASYNC_SUPPORTED
        public override Task WaitForIdleAsync(CancellationToken? cancellationToken = null)
        {
            SmartThreadPool.ValidateWorkItemsGroupWaitForIdle(this);

			// If the STP is already idle then return a completed task
			if (IsIdle)
            {
                return Task.CompletedTask;
            }

            if (cancellationToken?.IsCancellationRequested ?? false)
            {
                // Throw task cancel exception
                return Task.FromCanceled(cancellationToken.Value);
            }

            // Prepare a local tcs
            TaskCompletionSource<bool> isIdleTCS = null;

            lock (_lock)
            {
                // If the _isIdleTCS was not initialized or was already set then create a new one
                if (_isIdleTCS == null || _isIdleTCS.Task.IsCompleted)
                {
                    _isIdleTCS = new TaskCompletionSource<bool>();
                }

                // Store the local tcs
                isIdleTCS = _isIdleTCS;
            }

            // If in the meantime the STP become idle then set the tcs
            if (IsIdle)
            {
                isIdleTCS.TrySetResult(true);
            }

            if (!cancellationToken.HasValue)
            {
                return isIdleTCS.Task;
            }

            TaskCompletionSource<bool> cancelled = new TaskCompletionSource<bool>();
            cancellationToken.Value.Register(() => cancelled.TrySetCanceled());

            return Task.WhenAny(isIdleTCS.Task, cancelled.Task);
        }
#endif
        public override event WorkItemsGroupIdleHandler OnIdle
		{
			add { _onIdle += value; }
			remove { _onIdle -= value; }
		}

	    #endregion 

		#region Private methods

        private void OnWorkItemExecutionStatusChanged(WorkItem workItem, WorkItemExecutionStatus status)
        {
            if (status == WorkItemExecutionStatus.Started)
            {
                lock (_lock)
                {
                    ++_workItemsExecutingInStp;
                }
			}
			else if (status == WorkItemExecutionStatus.Completed)
            {
                workItem.OnWorkItemExecutionStatusChanged = null;
                EnqueueToSTPNextWorkItem(null, true, true);
            }
        }

        public void OnSTPIsStarting()
		{
            if (_isSuspended)
            {
                return;
            }
			
            EnqueueToSTPNextNWorkItem(_concurrency);
		}

	    public void EnqueueToSTPNextNWorkItem(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                EnqueueToSTPNextWorkItem(null, false, false);
            }
        }

		private object FireOnIdle(object state)
		{
			FireOnIdleImpl(_onIdle);
			return null;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void FireOnIdleImpl(WorkItemsGroupIdleHandler onIdle)
		{
			if(null == onIdle)
			{
				return;
			}

			Delegate[] delegates = onIdle.GetInvocationList();
			foreach(WorkItemsGroupIdleHandler eh in delegates)
			{
				try
				{
					eh(this);
				}
                catch { }  // Suppress exceptions
			}
		}

        internal override void Enqueue(WorkItem workItem)
        {
            EnqueueToSTPNextWorkItem(workItem, false, false);
        }

        internal override void Requeue(WorkItem workItem)
        {
            _stp.Enqueue(workItem);
        }

        private void EnqueueToSTPNextWorkItem(WorkItem workItem, bool decrementWorkItemsInStpQueue, bool workItemCompleted)
		{
			lock(_lock)
			{
                // Got here from OnWorkItemExecutionStatusChanged(status) when not status.HasFlag(WorkItemExecutionStatus.Executing)
                if (decrementWorkItemsInStpQueue)
                {
                    --_workItemsInStpQueue;

                    if (_workItemsInStpQueue < 0)
					{
						_workItemsInStpQueue = 0;
					}

					--_workItemsExecutingInStp;

					if(_workItemsExecutingInStp < 0)
					{
						_workItemsExecutingInStp = 0;
					}
				}

                if (workItemCompleted)
                {
                    --_attachedWorkItemsCount;
                }

                // If the work item is not null then enqueue it
                if (null != workItem)
				{
					workItem.CanceledWorkItemsGroup = _canceledWorkItemsGroup;
#if _ASYNC_SUPPORTED
					// Avoid duplicate event registration after awaiting
					if (!workItem.IsAsync)
#endif
					{
                        workItem.OnWorkItemExecutionStatusChanged = OnWorkItemExecutionStatusChanged;
						++_attachedWorkItemsCount;
                    }

                    _workItemsQueue.Enqueue(workItem);

					if ((1 == _workItemsQueue.Count) && 
						(0 == _workItemsInStpQueue))
					{
						_stp.RegisterWorkItemsGroup(this);
                        IsIdle = false;
                        _isIdleWaitHandle.Reset();
					}
				}

				// If the WorkItemsGroup has no more attached work items then notify idle
				if (0 == _attachedWorkItemsCount)
				{
					_stp.UnregisterWorkItemsGroup(this);
                    IsIdle = true;
                    _isIdleWaitHandle.Set();
#if _ASYNC_SUPPORTED
                    _isIdleTCS?.TrySetResult(true);
#endif

                    if (decrementWorkItemsInStpQueue && _onIdle != null && _onIdle.GetInvocationList().Length > 0)
                    {
                        _stp.QueueWorkItem(new WorkItemCallback(FireOnIdle));
                    }
				}

                if (!_isSuspended && _workItemsQueue.Count > 0)
				{
					if (_workItemsInStpQueue < _concurrency)
					{
						WorkItem nextWorkItem = _workItemsQueue.Dequeue() as WorkItem;
                        try
                        {
                            _stp.Enqueue(nextWorkItem);
                        }
                        catch (ObjectDisposedException e)
                        {
                            e.GetHashCode();
                            // The STP has been shutdown
                        }

						++_workItemsInStpQueue;
					}
				}
			}
		}

#endregion
    }

#endregion
}
