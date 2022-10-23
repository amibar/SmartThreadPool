using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Amib.Threading.Internal
{
    /// <summary>
    /// Holds a callback delegate and the state for that delegate.
    /// </summary>
    public partial class WorkItem : IHasWorkItemPriority
    {
        #region Static

        /// <summary>
        /// Cached functions for extracting result from a Task<>
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Func<Task, object>> _cachedTaskResultExtractors = new ConcurrentDictionary<Type, Func<Task, object>>();

        #endregion

        #region WorkItemState enum

        /// <summary>
        /// Indicates the state of the work item in the thread pool
        /// </summary>
        private enum WorkItemState
        {
            InQueue = 0,    // Nexts: InProgress, Canceled
            InProgress = 1,    // Nexts: Completed, Canceled, Awaiting
            Completed = 2,    // Stays Completed
            Canceled = 3,    // Stays Canceled

            Awaiting = 4,    // Nexts: InProgress
        }

        private static bool IsValidStatesTransition(WorkItemState currentState, WorkItemState nextState)
        {
            bool valid = false;

            switch (currentState)
            {
                case WorkItemState.InQueue:
                    valid = (WorkItemState.InProgress == nextState) || (WorkItemState.Canceled == nextState);
                    break;
                case WorkItemState.InProgress:
                    valid = (WorkItemState.Completed == nextState) || (WorkItemState.Canceled == nextState) || (WorkItemState.Awaiting == nextState);
                    break;
                case WorkItemState.Completed:
                case WorkItemState.Canceled:
                    // Cannot be changed
                    break;
                case WorkItemState.Awaiting:
                    valid = (WorkItemState.InProgress == nextState) || (WorkItemState.Completed == nextState) || (WorkItemState.Canceled == nextState);
                    break;
                default:
                    // Unknown state
                    throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
            }

            return valid;
        }

        #endregion

        #region Fields

        /// <summary>
        /// Callback delegate for the callback.
        /// </summary>
        /// <remarks>Changes when an await is called from an async method</remarks>
        private WorkItemCallback _callback;

#if _ASYNC_SUPPORTED
        /// <summary>
        /// When the original callback is an async method, this callback advances the
        /// async method to its next state
        /// </summary>
        private WorkItemCallback _nextCallback;
#endif
        /// <summary>
        /// State with which to call the callback delegate.
        /// </summary>
        private object _state;

#if (NETFRAMEWORK)
        /// <summary>
        /// Stores the caller's context
        /// </summary>
        private readonly CallerThreadContext _callerContext;
#endif
        /// <summary>
        /// Holds the result of the mehtod
        /// </summary>
        private object _result;

        /// <summary>
        /// Hold the exception if the method threw it
        /// </summary>
        private Exception _exception;

        /// <summary>
        /// Hold the previous state of the work item
        /// </summary>
        private WorkItemState _prevWorkItemState;

        /// <summary>
        /// Hold the state of the work item
        /// </summary>
        private WorkItemState _workItemState;

        /// <summary>
        /// A ManualResetEvent to indicate that the result is ready
        /// </summary>
        private ManualResetEvent _workItemCompleted;

        /// <summary>
        /// A reference count to the _workItemCompleted. 
        /// When it reaches to zero _workItemCompleted is Closed
        /// </summary>
        private int _workItemCompletedRefCount;

        /// <summary>
        /// Represents the result state of the work item
        /// </summary>
        private readonly WorkItemResult _workItemResult;

        /// <summary>
        /// Work item info
        /// </summary>
        private readonly WorkItemInfo _workItemInfo;

        /// <summary>
        /// A reference to an object that indicates whatever the 
        /// WorkItemsGroup has been canceled
        /// </summary>
        private CanceledWorkItemsGroup _canceledWorkItemsGroup = CanceledWorkItemsGroup.NotCanceledWorkItemsGroup;

        /// <summary>
        /// A reference to an object that indicates whatever the 
        /// SmartThreadPool has been canceled
        /// </summary>
        private CanceledWorkItemsGroup _canceledSmartThreadPool = CanceledWorkItemsGroup.NotCanceledWorkItemsGroup;

        /// <summary>
        /// The work item group this work item belong to.
        /// </summary>
        private readonly IWorkItemsGroup _workItemsGroup;

        /// <summary>
        /// The thread that executes this workitem.
        /// This field is available for the period when the work item is executed, before and after it is null.
        /// </summary>
        private Thread _executingThread;

        /// <summary>
        /// The absolute time when the work item will be timeout
        /// </summary>
        private long _expirationTime;

#if _ASYNC_SUPPORTED
        /// <summary>
        /// Store the SynchronizationContext of this work item so in case of async method it will continue
        /// in the same IWorkItemsGroup
        /// </summary>
        private STPSynchronizationContext _synchronizationContext;

        /// <summary>
        /// Stores the Task object so when the await returns the task can continue
        /// </summary>
        private Task _task;

        /// <summary>
        /// Stores the state of an async void method
        /// </summary>
        /// <remarks>
        /// async void methods (in contradiction to async Task DoWork() and async Task&lt;T&gt; DoWork()) don't
        /// return a Task object so in order to know if they are async we use the OperationStarted() and OperationCompleted() callbacks
        /// of the SynchronizationContext.
        /// This value is only relevant if _task is null.
        /// For some reason OperationStarted() and OperationCompleted() are only called by async void methods and not by async Task methods!
        /// </remarks>
        private bool? _taskCompleted;

        /// <summary>
        /// A TaskCompletionSource to support GetResultAsync()
        /// </summary>
        private TaskCompletionSource<object> _tcsResult;

        /// <summary>
        /// Indicates that this is an async work item.
        /// Initialized once the work item is awaiting.
        /// </summary>
        internal bool IsAsync { get; private set; }

        /// <summary>
        /// Indicates if the STPSynchronizationContext can queue the work item back.
        /// In some cases the await completes very fast and calls to the SynchronizationContext.Post
        /// before the SmartThreadPool thread completed handling the work item, which may cause
        /// several threads to work on the same work item at once.
        /// </summary>
        private bool _canRequeue = false;

        /// <summary>
        /// Let the SynchronizationContext set the _taskCompleted.
        /// true on OperationStarted() and false on OperationCompleted()
        /// </summary>
        internal void SetTaskState(bool completed) => _taskCompleted = completed;

        /// <summary>
        /// Requeue the work item so it can continue its awaitable method.
        /// This method is called from SynchronizationContext
        /// </summary>
        internal void HandleRequeue(SendOrPostCallback d, object state)
        {
            // Set the _nextCallback to what we got from the SynchronizationContext.
            // The new callback will continue where the await was called
            // We don't re-queue it yet, since the current work item may still be executing
            // on a SmartPoolThread thread
            _nextCallback = _ =>
            {
                 d(state);
                 return _task;
            };

            RequeueIfReady();
        }
        
        private void RequeueIfReady()
        {
            // Check if we need and can requeue the work item..

            // An async work item should be re-queued once its await is satisfied and
            // it is no longer running on a SmartPoolThread thread.
            // Since STPSynchronizationContext.Post is called from an arbitrary thread,
            // we need to make sure we can re-queue the work item.
            if (_canRequeue)
            {
                // Check if there is a next callback to call to
                var nextCallback = _nextCallback;
                if (nextCallback != null)
                {
                    // To overcome the race condition, use Interlocked.CompareExchange to
                    // get the next callback exclusively. 
                    var callback = Interlocked.CompareExchange(ref _nextCallback, null, nextCallback);
                    if (callback != null)
                    {
                        // We got the call back so re-queue the work item
                        _callback = callback;

                        // The Requeue queue the work item directly to the STP
                        ((WorkItemsGroupBase)_workItemsGroup).Requeue(this);
                    }
                }
            }
        }
#endif

        #region Performance Counter fields

        /// <summary>
        /// Stores how long the work item waited on the stp queue
        /// </summary>
        private Stopwatch _waitingOnQueueStopwatch;

        /// <summary>
        /// Stores how much time it took the work item to execute after it went out of the queue
        /// </summary>
        private Stopwatch _processingStopwatch;

#endregion

#endregion

#region Properties

        public TimeSpan WaitingTime
        {
            get
            {
                return _waitingOnQueueStopwatch.Elapsed;
            }
        }

        public TimeSpan ProcessTime
        {
            get
            {
                return _processingStopwatch.Elapsed;
            }
        }

        internal WorkItemInfo WorkItemInfo
        {
            get
            {
                return _workItemInfo;
            }
        }

        internal IWorkItemsGroup WorkItemsGroup
        {
            get
            {
                return _workItemsGroup;
            }
        }

        /// <summary>
        /// This callback is called each time a work item changes its execution status.
        /// It is used by the WorkItemsGroup to control its work items
        /// </summary>
        internal WorkItemExecutionStatusCallback OnWorkItemExecutionStatusChanged { get; set; }

        #endregion

        #region Construction

        /// <summary>
        /// Initialize the callback holding object.
        /// </summary>
        /// <param name="workItemsGroup">The workItemGroup of the workitem</param>
        /// <param name="workItemInfo">The WorkItemInfo of te workitem</param>
        /// <param name="callback">Callback delegate for the callback.</param>
        /// <param name="state">State with which to call the callback delegate.</param>
        /// 
        /// We assume that the WorkItem object is created within the thread
        /// that meant to run the callback
        public WorkItem(
            IWorkItemsGroup workItemsGroup,
            WorkItemInfo workItemInfo,
            WorkItemCallback callback,
            object state)
        {
            _workItemsGroup = workItemsGroup;
            _workItemInfo = workItemInfo;

#if (NETFRAMEWORK)
            if (_workItemInfo.UseCallerCallContext || _workItemInfo.UseCallerHttpContext)
            {
                _callerContext = CallerThreadContext.Capture(_workItemInfo.UseCallerCallContext, _workItemInfo.UseCallerHttpContext);
            }
#endif

            _callback = callback;
            _state = state;
            _workItemResult = new WorkItemResult(this);
            Initialize();
        }

        internal void Initialize()
        {
            // The _workItemState is changed directly instead of using the SetWorkItemState
            // method since we don't want to go through IsValidStateTransition.
            _workItemState = WorkItemState.InQueue;
            _prevWorkItemState = WorkItemState.InQueue;

            _workItemCompleted = null;
            _workItemCompletedRefCount = 0;
            _waitingOnQueueStopwatch = new Stopwatch();
            _processingStopwatch = new Stopwatch();
            _expirationTime =
                _workItemInfo.Timeout > 0 ?
                DateTime.UtcNow.Ticks + _workItemInfo.Timeout * TimeSpan.TicksPerMillisecond :
                long.MaxValue;
        }

        internal bool WasQueuedBy(IWorkItemsGroup workItemsGroup)
        {
            return (workItemsGroup == _workItemsGroup);
        }


#endregion

#region Methods

        internal CanceledWorkItemsGroup CanceledWorkItemsGroup
        {
            get { return _canceledWorkItemsGroup; }
            set { _canceledWorkItemsGroup = value; }
        }

        internal CanceledWorkItemsGroup CanceledSmartThreadPool
        {
            get { return _canceledSmartThreadPool; }
            set { _canceledSmartThreadPool = value; }
        }

        /// <summary>
        /// Change the state of the work item to in progress if it wasn't canceled.
        /// </summary>
        /// <returns>
        /// Return true on success or false in case the work item was canceled.
        /// If the work item needs to run a post execute then the method will return true.
        /// </returns>
        public bool StartingWorkItem()
        {
            _waitingOnQueueStopwatch.Stop();
            _processingStopwatch.Start();

            lock (this)
            {
                if (IsCanceled)
                {
                    bool result = false;
                    if ((_workItemInfo.PostExecuteWorkItemCallback != null) &&
                        ((_workItemInfo.CallToPostExecute & CallToPostExecute.WhenWorkItemCanceled) == CallToPostExecute.WhenWorkItemCanceled))
                    {
                        result = true;
                    }

                    return result;
                }

                var workItemState = GetWorkItemState();

                Debug.Assert(workItemState == WorkItemState.InQueue || workItemState == WorkItemState.Awaiting);

                // No need for a lock yet, only after the state has changed to InProgress
                _executingThread = Thread.CurrentThread;
                if (workItemState == WorkItemState.InQueue || workItemState == WorkItemState.Awaiting)
                {
                    SetWorkItemState(WorkItemState.InProgress);
                }
            }

            return true;
        }

        /// <summary>
        /// Execute the work item and the post execute
        /// </summary>
        public void Execute()
        {
            CallToPostExecute currentCallToPostExecute = 0;

            // Execute the work item if we are in the correct state
            switch (GetWorkItemState())
            {
                case WorkItemState.InProgress:
                    currentCallToPostExecute |= CallToPostExecute.WhenWorkItemNotCanceled;
                    ExecuteWorkItem();
                    break;
                case WorkItemState.Canceled:
                    currentCallToPostExecute |= CallToPostExecute.WhenWorkItemCanceled;
                    break;
                case WorkItemState.Awaiting:
                    // The WorkItemState.Awaiting case is not relevant since we changed the state
                    // to InProgress just before we get here.
                    break;
                default:
                    Debug.Assert(false);
                    throw new NotSupportedException();
            }

            // Run the post execute as needed
            if (IsCompleted && (currentCallToPostExecute & _workItemInfo.CallToPostExecute) != 0)
            {
                PostExecute();
            }

            _processingStopwatch.Stop();
        }

        internal void NotifyWorkItemExecutionStatusChanged(WorkItemExecutionStatus status)
        {
            OnWorkItemExecutionStatusChanged?.Invoke(this, status);

#if _ASYNC_SUPPORTED
            _canRequeue = !status.HasFlag(WorkItemExecutionStatus.Executing);
            RequeueIfReady();
#endif
        }

        /// <summary>
        /// Execute the work item
        /// </summary>
        private WorkItemState ExecuteWorkItem()
        {

#if (NETFRAMEWORK)
            CallerThreadContext ctc = null;
            if (null != _callerContext)
            {
                ctc = CallerThreadContext.Capture(_callerContext.CapturedCallContext, _callerContext.CapturedHttpContext);
                CallerThreadContext.Apply(_callerContext);
            }
#endif
#if _ASYNC_SUPPORTED
            if (_synchronizationContext == null)
            {
                _synchronizationContext = new STPSynchronizationContext(this);
            }

            SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
#endif

            Exception exception = null;
            object result = null;

            try
            {
                try
                {
#if _ASYNC_SUPPORTED
                    _nextCallback = null;
#endif
                    result = _callback(_state);

                    if (result is Task t && t.IsFaulted)
                    {
                        exception = t.Exception;
                    }
                }
                catch (Exception e)
                {
                    // Save the exception so we can rethrow it later
                    exception = e;
                }

                // Remove the value of the execution thread, so it will be impossible to cancel the work item,
                // since it is already completed.
                // Cancelling a work item that already completed may cause the abortion of the next work item!!!
                Thread executionThread = Interlocked.CompareExchange(ref _executingThread, null, _executingThread);

                if (null == executionThread)
                {
                    // Oops! we are going to be aborted..., Wait here so we can catch the ThreadAbortException
                    Thread.Sleep(60 * 1000);

                    // If after 1 minute this thread was not aborted then let it continue working.
                }
            }
            // We must treat the ThreadAbortException or else it will be stored in the exception variable
            catch (ThreadAbortException tae)
            {
                tae.GetHashCode();
                // Check if the work item was cancelled
                // If we got a ThreadAbortException and the STP is not shutting down, it means the 
                // work items was cancelled.
                if (!SmartThreadPool.CurrentThreadEntry.AssociatedSmartThreadPool.IsShuttingdown)
                {
                    Thread.ResetAbort();
                }
            }

#if (NETFRAMEWORK)
            if (null != _callerContext)
            {
                CallerThreadContext.Apply(ctc);
            }
#endif

            if (!SmartThreadPool.IsWorkItemCanceled)
            {
#if _ASYNC_SUPPORTED
                // If the result is a task, it means we are in async method and should "wait" for it to complete 
                if (result is Task task)
                {
                    if (!IsAsync)
                    {
                        IsAsync = true;
                    }

                    if (_task == null)
                    {
                        // Keep the _task so we can use it when the work item need to be re-queued
                        _task = task;
                    }

                    // If the task is completed then signal it
                    if (_task.IsCompleted)
                    {
                        var taskResult = !_task.IsFaulted ? ExtractTaskResult(_task) : null;
                        SetResult(taskResult, _task.Exception?.Flatten());
                    }
                    // Otherwise we are in await
                    else
                    {
                        SetWorkItemState(WorkItemState.Awaiting);
                    }
                }
                // Check if this is an async void method and it wasn't completed yet
                else if (_taskCompleted == false)
                {
                    if (!IsAsync)
                    {
                        IsAsync = true;
                    }
                    SetWorkItemState(WorkItemState.Awaiting);
                }
                else
#endif
                {
                    SetResult(result, exception);
                }
            }

            return _workItemState;
        }

#if _ASYNC_SUPPORTED
        private object ExtractTaskResult(Task task)
        {
            // If task is just a Task, it has no result so just return null.
            if (typeof(Task) == task.GetType())
            {
                return null;
            }

            var extractor = _cachedTaskResultExtractors.GetOrAdd(
                task.GetType(),
                type =>
                {
                    var getMethod = type.GetProperty("Result").GetGetMethod();

                    return t => getMethod.Invoke(t , null);

                });

            return extractor(task);
        }
#endif

        /// <summary>
        /// Runs the post execute callback
        /// </summary>
        private void PostExecute()
        {
            if (null != _workItemInfo.PostExecuteWorkItemCallback)
            {
                try
                {
                    _workItemInfo.PostExecuteWorkItemCallback(_workItemResult);
                }
                catch (Exception e)
                {
                    Debug.Assert(null != e);
                }
            }
        }

        /// <summary>
        /// Set the result of the work item to return
        /// </summary>
        /// <param name="result">The result of the work item</param>
        /// <param name="exception">The exception that was throw while the workitem executed, null
        /// if there was no exception.</param>
        internal void SetResult(object result, Exception exception)
        {
            _result = result;
            _exception = exception;
            SignalComplete(false);
        }

        /// <summary>
        /// Returns the work item result
        /// </summary>
        /// <returns>The work item result</returns>
        internal IWorkItemResult GetWorkItemResult()
        {
            return _workItemResult;
        }

        /// <summary>
        /// Wait for all work items to complete
        /// </summary>
        /// <param name="waitableResults">Array of work item result objects</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or Timeout.Infinite (-1) to wait indefinitely.</param>
        /// <param name="exitContext">
        /// true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it; otherwise, false. 
        /// </param>
        /// <param name="cancelWaitHandle">A cancel wait handle to interrupt the wait if needed</param>
        /// <returns>
        /// true when every work item in waitableResults has completed; otherwise false.
        /// </returns>
        internal static bool WaitAll(
            IWaitableResult[] waitableResults,
            int millisecondsTimeout,
            bool exitContext,
            WaitHandle cancelWaitHandle)
        {
            if (0 == waitableResults.Length)
            {
                return true;
            }

            bool success;
            WaitHandle[] waitHandles = new WaitHandle[waitableResults.Length];
            GetWaitHandles(waitableResults, waitHandles);

            if ((null == cancelWaitHandle) && (waitHandles.Length <= 64))
            {
                success = STPEventWaitHandle.WaitAll(waitHandles, millisecondsTimeout, exitContext);
            }
            else
            {
                success = true;
                int millisecondsLeft = millisecondsTimeout;
                Stopwatch stopwatch = Stopwatch.StartNew();

                WaitHandle[] whs;
                if (null != cancelWaitHandle)
                {
                    whs = new WaitHandle[] { null, cancelWaitHandle };
                }
                else
                {
                    whs = new WaitHandle[] { null };
                }

                bool waitInfinitely = (Timeout.Infinite == millisecondsTimeout);
                // Iterate over the wait handles and wait for each one to complete.
                // We cannot use WaitHandle.WaitAll directly, because the cancelWaitHandle
                // won't affect it.
                // Each iteration we update the time left for the timeout.
                for (int i = 0; i < waitableResults.Length; ++i)
                {
                    // WaitAny don't work with negative numbers
                    if (!waitInfinitely && (millisecondsLeft < 0))
                    {
                        success = false;
                        break;
                    }

                    whs[0] = waitHandles[i];
                    int result = STPEventWaitHandle.WaitAny(whs, millisecondsLeft, exitContext);
                    if ((result > 0) || (STPEventWaitHandle.WaitTimeout == result))
                    {
                        success = false;
                        break;
                    }

                    if (!waitInfinitely)
                    {
                        // Update the time left to wait
                        millisecondsLeft = millisecondsTimeout - (int)stopwatch.ElapsedMilliseconds;
                    }
                }
            }
            // Release the wait handles
            ReleaseWaitHandles(waitableResults);

            return success;
        }

        /// <summary>
        /// Waits for any of the work items in the specified array to complete, cancel, or timeout
        /// </summary>
        /// <param name="waitableResults">Array of work item result objects</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or Timeout.Infinite (-1) to wait indefinitely.</param>
        /// <param name="exitContext">
        /// true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it; otherwise, false. 
        /// </param>
        /// <param name="cancelWaitHandle">A cancel wait handle to interrupt the wait if needed</param>
        /// <returns>
        /// The array index of the work item result that satisfied the wait, or WaitTimeout if no work item result satisfied the wait and a time interval equivalent to millisecondsTimeout has passed or the work item has been canceled.
        /// </returns>
        internal static int WaitAny(
            IWaitableResult[] waitableResults,
            int millisecondsTimeout,
            bool exitContext,
            WaitHandle cancelWaitHandle)
        {
            WaitHandle[] waitHandles;

            if (null != cancelWaitHandle)
            {
                waitHandles = new WaitHandle[waitableResults.Length + 1];
                GetWaitHandles(waitableResults, waitHandles);
                waitHandles[waitableResults.Length] = cancelWaitHandle;
            }
            else
            {
                waitHandles = new WaitHandle[waitableResults.Length];
                GetWaitHandles(waitableResults, waitHandles);
            }

            int result = STPEventWaitHandle.WaitAny(waitHandles, millisecondsTimeout, exitContext);

            // Treat cancel as timeout
            if (null != cancelWaitHandle)
            {
                if (result == waitableResults.Length)
                {
                    result = STPEventWaitHandle.WaitTimeout;
                }
            }

            ReleaseWaitHandles(waitableResults);

            return result;
        }

        /// <summary>
        /// Fill an array of wait handles with the work items wait handles.
        /// </summary>
        /// <param name="waitableResults">An array of work item results</param>
        /// <param name="waitHandles">An array of wait handles to fill</param>
        private static void GetWaitHandles(
            IWaitableResult[] waitableResults,
            WaitHandle[] waitHandles)
        {
            for (int i = 0; i < waitableResults.Length; ++i)
            {
                WorkItemResult wir = waitableResults[i].GetWorkItemResult() as WorkItemResult;
                Debug.Assert(null != wir, "All waitableResults must be WorkItemResult objects");

                waitHandles[i] = wir.GetWorkItem().GetWaitHandle();
            }
        }

        /// <summary>
        /// Release the work items' wait handles
        /// </summary>
        /// <param name="waitableResults">An array of work item results</param>
        private static void ReleaseWaitHandles(IWaitableResult[] waitableResults)
        {
            for (int i = 0; i < waitableResults.Length; ++i)
            {
                WorkItemResult wir = (WorkItemResult)waitableResults[i].GetWorkItemResult();

                wir.GetWorkItem().ReleaseWaitHandle();
            }
        }

#endregion

#region Private Members

        private WorkItemState GetWorkItemState()
        {
            lock (this)
            {
                if (WorkItemState.Completed == _workItemState)
                {
                    return _workItemState;
                }

                long nowTicks = DateTime.UtcNow.Ticks;

                if (WorkItemState.Canceled != _workItemState && nowTicks > _expirationTime)
                {
                    _prevWorkItemState = _workItemState;
                    _workItemState = WorkItemState.Canceled;
                }

                if (WorkItemState.InProgress == _workItemState)
                {
                    return _workItemState;
                }

                if (CanceledSmartThreadPool.IsCanceled || CanceledWorkItemsGroup.IsCanceled)
                {
                    return WorkItemState.Canceled;
                }

                return _workItemState;
            }
        }

        /// <summary>
        /// Sets the work item's state
        /// </summary>
        /// <param name="workItemState">The state to set the work item to</param>
        private void SetWorkItemState(WorkItemState workItemState)
        {
            lock (this)
            {
                if (IsValidStatesTransition(_workItemState, workItemState))
                {
                    _prevWorkItemState = _workItemState;
                    _workItemState = workItemState;
                }
            }
        }

        /// <summary>
        /// Signals that work item has been completed or canceled
        /// </summary>
        /// <param name="canceled">Indicates that the work item has been canceled</param>
        private void SignalComplete(bool canceled)
        {
            SetWorkItemState(canceled ? WorkItemState.Canceled : WorkItemState.Completed);
            lock (this)
            {
                // If someone is waiting then signal.
                _workItemCompleted?.Set();
            }

#if _ASYNC_SUPPORTED
            if (_tcsResult != null)
            {
                if (canceled)
                {
                    _tcsResult.TrySetCanceled();
                }
                else if (_exception != null)
                {
                    _tcsResult.TrySetException(_exception);
                }
                else
                {
                    _tcsResult.TrySetResult(_result);
                }
            }
#endif
        }

        internal void WorkItemIsQueued()
        {
            _waitingOnQueueStopwatch.Start();
        }

#endregion

#region Members exposed by WorkItemResult

        /// <summary>
        /// Cancel the work item if it didn't start running yet.
        /// </summary>
        /// <returns>Returns true on success or false if the work item is in progress or already completed</returns>
        private bool Cancel(bool abortExecution)
        {
            bool success = false;
            bool signalComplete = false;

            lock (this)
            {
                switch (GetWorkItemState())
                {
                    case WorkItemState.Canceled:
                        //Debug.WriteLine("Work item already canceled");
                        if (abortExecution)
                        {
                            Thread executionThread = Interlocked.CompareExchange(ref _executingThread, null, _executingThread);
                            if (null != executionThread)
                            {
                                executionThread.Abort(); // "Cancel"
                                // No need to signalComplete, because we already cancelled this work item
                                // so it already signaled its completion.
                                //signalComplete = true;
                            }
                        } 
                        success = true;
                        break;
                    case WorkItemState.Completed:
                        //Debug.WriteLine("Work item cannot be canceled");
                        break;
                    case WorkItemState.InProgress:
                        if (abortExecution)
                        {
                            Thread executionThread = Interlocked.CompareExchange(ref _executingThread, null, _executingThread);
                            if (null != executionThread)
                            {
                                executionThread.Abort(); // "Cancel"
                                success = true;
                                signalComplete = true;
                            }
                        }
                        else
                        {
                            success = true;
                            signalComplete = true;
                        }
                        break;
                    case WorkItemState.InQueue:
                        // Signal to the wait for completion that the work
                        // item has been completed (canceled). There is no
                        // reason to wait for it to get out of the queue
                        signalComplete = true;
                        //Debug.WriteLine("Work item canceled");
                        success = true;
                        break;
                    case WorkItemState.Awaiting:
                        // Signal to the wait for completion that the work
                        // item has been completed (canceled). There is no
                        // reason to wait for it to get out of the queue
                        signalComplete = true;
                        //Debug.WriteLine("Work item canceled");
                        success = true;
                        break;
                }

                if (signalComplete)
                {
                    SignalComplete(true);
                }
            }
            return success;
        }

        /// <summary>
        /// Get the result of the work item.
        /// If the work item didn't run yet then the caller waits for the result, timeout, or cancel.
        /// In case of error the method throws and exception
        /// </summary>
        /// <returns>The result of the work item</returns>
        private object GetResult(
            int millisecondsTimeout,
            bool exitContext,
            WaitHandle cancelWaitHandle)
        {
            Exception e;
            object result = GetResult(millisecondsTimeout, exitContext, cancelWaitHandle, out e);
            if (null != e)
            {
                throw new WorkItemResultException("The work item caused an excpetion, see the inner exception for details", e);
            }
            return result;
        }

        /// <summary>
        /// Get the result of the work item.
        /// If the work item didn't run yet then the caller waits for the result, timeout, or cancel.
        /// In case of error the e argument is filled with the exception
        /// </summary>
        /// <returns>The result of the work item</returns>
        private object GetResult(
            int millisecondsTimeout,
            bool exitContext,
            WaitHandle cancelWaitHandle,
            out Exception e)
        {
            e = null;

            // Check for cancel
            if (WorkItemState.Canceled == GetWorkItemState())
            {
                throw new WorkItemCancelException("Work item canceled");
            }

            // Check for completion
            if (IsCompleted)
            {
                e = _exception;
                return _result;
            }

            // If no cancelWaitHandle is provided
            if (null == cancelWaitHandle)
            {
                WaitHandle wh = GetWaitHandle();

                bool timeout = !STPEventWaitHandle.WaitOne(wh, millisecondsTimeout, exitContext);

                ReleaseWaitHandle();

                if (timeout)
                {
                    throw new WorkItemTimeoutException("Work item timeout");
                }
            }
            else
            {
                WaitHandle wh = GetWaitHandle();
                int result = STPEventWaitHandle.WaitAny(new WaitHandle[] { wh, cancelWaitHandle });
                ReleaseWaitHandle();

                switch (result)
                {
                    case 0:
                        // The work item signaled
                        // Note that the signal could be also as a result of canceling the 
                        // work item (not the get result)
                        break;
                    case 1:
                    case STPEventWaitHandle.WaitTimeout:
                        throw new WorkItemTimeoutException("Work item timeout");
                    default:
                        Debug.Assert(false);
                        break;

                }
            }

            // Check for cancel
            if (WorkItemState.Canceled == GetWorkItemState())
            {
                throw new WorkItemCancelException("Work item canceled");
            }

            Debug.Assert(IsCompleted);

            e = _exception;

            // Return the result
            return _result;
        }

#if _ASYNC_SUPPORTED
        private Task<object> GetResultAsync()
        {
            if (_tcsResult == null)
            {
                Interlocked.CompareExchange(ref _tcsResult, new TaskCompletionSource<object>(), null);
            }

            if (IsCompleted && !_tcsResult.Task.IsCompleted)
            {
                if (IsCanceled)
                {
                    _tcsResult.TrySetCanceled();
                }
                else if (_exception != null)
                {
                    _tcsResult.TrySetException(_exception);
                }
                else
                {
                    _tcsResult.TrySetResult(_result);
                }
            }

            return _tcsResult.Task;
        }
#endif

        /// <summary>
        /// A wait handle to wait for completion, cancel, or timeout 
        /// </summary>
        private WaitHandle GetWaitHandle()
        {
            lock (this)
            {
                if (null == _workItemCompleted)
                {
                    _workItemCompleted = EventWaitHandleFactory.CreateManualResetEvent(IsCompleted);
                }
                ++_workItemCompletedRefCount;
            }
            return _workItemCompleted;
        }

        private void ReleaseWaitHandle()
        {
            lock (this)
            {
                if (null != _workItemCompleted)
                {
                    --_workItemCompletedRefCount;
                    if (0 == _workItemCompletedRefCount)
                    {
                        _workItemCompleted.Close();
                        _workItemCompleted = null;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true when the work item has completed or canceled
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                lock (this)
                {
                    WorkItemState workItemState = GetWorkItemState();
                    return ((workItemState == WorkItemState.Completed) ||
                            (workItemState == WorkItemState.Canceled));
                }
            }
        }

        /// <summary>
        /// Returns true when the work item has canceled
        /// </summary>
        public bool IsCanceled
        {
            get
            {
                lock (this)
                {
                    return (GetWorkItemState() == WorkItemState.Canceled);
                }
            }
        }

#endregion

#region IHasWorkItemPriority Members

        /// <summary>
        /// Returns the priority of the work item
        /// </summary>
        public WorkItemPriority WorkItemPriority
        {
            get
            {
                return _workItemInfo.WorkItemPriority;
            }
        }

#endregion

        public void DisposeOfState()
        {
            if (_workItemInfo.DisposeOfStateObjects)
            {
                if (_state is IDisposable disp)
                {
                    disp.Dispose();
                    _state = null;
                }
            }
        }
    }
}
