
using System;

namespace Amib.Threading.Internal
{
    [Flags]
    internal enum WorkItemExecutionStatus
    {
        Started      = 0b_11,
        Executing    = 0b_01,
        Awaiting     = 0b_00,
        Completed    = 0b_10,
    }

    /// <summary>
    /// An internal delegate to call when a WorkItem starts or completes
    /// </summary>
    internal delegate void WorkItemStateCallback(WorkItem workItem);


    /// <summary>
    /// An internal delegate to call when a WorkItem starts or stops executing.
    /// </summary>
    /// <remarks>The WorkItemsGroup needs to know how many work items are currently executing, hence using a thread.
    /// An awaiting async method doesn't use a thread so another work item may execute.
    /// </remarks>
    internal delegate void WorkItemExecutionStatusCallback(WorkItem workItem, WorkItemExecutionStatus status);

    internal interface IInternalWorkItemResult
    {
        event WorkItemExecutionStatusCallback OnWorkItemExecutionStatusChanged;
    }

    internal interface IInternalWaitableResult
    {
        /// <summary>
        /// This method is intent for internal use.
        /// </summary>   
        IWorkItemResult GetWorkItemResult();
    }

    public interface IHasWorkItemPriority
    {
        WorkItemPriority WorkItemPriority { get; }
    }
}
