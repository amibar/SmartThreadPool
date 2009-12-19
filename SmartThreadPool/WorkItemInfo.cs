namespace Amib.Threading
{
	#region WorkItemInfo class

	/// <summary>
	/// Summary description for WorkItemInfo.
	/// </summary>
	public class WorkItemInfo
	{
	    public WorkItemInfo()
		{
			UseCallerCallContext = SmartThreadPool.DefaultUseCallerCallContext;
			UseCallerHttpContext = SmartThreadPool.DefaultUseCallerHttpContext;
			DisposeOfStateObjects = SmartThreadPool.DefaultDisposeOfStateObjects;
			CallToPostExecute = SmartThreadPool.DefaultCallToPostExecute;
			PostExecuteWorkItemCallback = SmartThreadPool.DefaultPostExecuteWorkItemCallback;
			WorkItemPriority = SmartThreadPool.DefaultWorkItemPriority;
		}

		public WorkItemInfo(WorkItemInfo workItemInfo)
		{
			UseCallerCallContext = workItemInfo.UseCallerCallContext;
			UseCallerHttpContext = workItemInfo.UseCallerHttpContext;
			DisposeOfStateObjects = workItemInfo.DisposeOfStateObjects;
			CallToPostExecute = workItemInfo.CallToPostExecute;
			PostExecuteWorkItemCallback = workItemInfo.PostExecuteWorkItemCallback;
			WorkItemPriority = workItemInfo.WorkItemPriority;
		}

	    /// <summary>
	    /// Get/Set if to use the caller's security context
	    /// </summary>
	    public bool UseCallerCallContext { get; set; }

	    /// <summary>
	    /// Get/Set if to use the caller's HTTP context
	    /// </summary>
	    public bool UseCallerHttpContext { get; set; }

	    /// <summary>
	    /// Get/Set if to dispose of the state object of a work item
	    /// </summary>
	    public bool DisposeOfStateObjects { get; set; }

	    /// <summary>
	    /// Get/Set the run the post execute options
	    /// </summary>
        public CallToPostExecute CallToPostExecute { get; set; }

	    /// <summary>
	    /// Get/Set the post execute callback
	    /// </summary>
        public PostExecuteWorkItemCallback PostExecuteWorkItemCallback { get; set; }

	    /// <summary>
	    /// Get/Set the work items priority
	    /// </summary>
	    public WorkItemPriority WorkItemPriority { get; set; }
	}

	#endregion
}
