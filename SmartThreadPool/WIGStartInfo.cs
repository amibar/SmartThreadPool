using System;

namespace Amib.Threading
{
	/// <summary>
	/// Summary description for WIGStartInfo.
	/// </summary>
	public class WIGStartInfo
	{
        private bool _useCallerCallContext;
        private bool _useCallerHttpContext;
        private bool _disposeOfStateObjects;
        private CallToPostExecute _callToPostExecute;
        private PostExecuteWorkItemCallback _postExecuteWorkItemCallback;
        private bool _startSuspended;
        private WorkItemPriority _workItemPriority;
        private bool _fillStateWithArgs;

        protected bool _readOnly;

	    public WIGStartInfo()
        {
	        _fillStateWithArgs = SmartThreadPool.DefaultFillStateWithArgs;
	        _workItemPriority = SmartThreadPool.DefaultWorkItemPriority;
            _startSuspended = SmartThreadPool.DefaultStartSuspended;
            _postExecuteWorkItemCallback = SmartThreadPool.DefaultPostExecuteWorkItemCallback;
            _callToPostExecute = SmartThreadPool.DefaultCallToPostExecute;
            _disposeOfStateObjects = SmartThreadPool.DefaultDisposeOfStateObjects;
            _useCallerHttpContext = SmartThreadPool.DefaultUseCallerHttpContext;
            _useCallerCallContext = SmartThreadPool.DefaultUseCallerCallContext;
        }

	    public WIGStartInfo(WIGStartInfo wigStartInfo)
        {
            _useCallerCallContext = wigStartInfo.UseCallerCallContext;
            _useCallerHttpContext = wigStartInfo.UseCallerHttpContext;
            _disposeOfStateObjects = wigStartInfo.DisposeOfStateObjects;
            _callToPostExecute = wigStartInfo.CallToPostExecute;
            _postExecuteWorkItemCallback = wigStartInfo.PostExecuteWorkItemCallback;
            _workItemPriority = wigStartInfo.WorkItemPriority;
            _startSuspended = wigStartInfo.StartSuspended;
            _fillStateWithArgs = wigStartInfo.FillStateWithArgs;
        }

        protected void ThrowIfReadOnly()
        {
            if (_readOnly)
            {
                throw new NotSupportedException("This is a readonly instance and set is not supported");
            }
        }

	    /// <summary>
	    /// Get/Set if to use the caller's security context
	    /// </summary>
	    public virtual bool UseCallerCallContext
	    {
	        get { return _useCallerCallContext; }
            set 
            { 
                ThrowIfReadOnly();  
                _useCallerCallContext = value; 
            }
	    }


	    /// <summary>
	    /// Get/Set if to use the caller's HTTP context
	    /// </summary>
	    public virtual bool UseCallerHttpContext
	    {
	        get { return _useCallerHttpContext; }
            set 
            { 
                ThrowIfReadOnly();  
                _useCallerHttpContext = value; 
            }
	    }


	    /// <summary>
	    /// Get/Set if to dispose of the state object of a work item
	    /// </summary>
	    public virtual bool DisposeOfStateObjects
	    {
	        get { return _disposeOfStateObjects; }
            set 
            { 
                ThrowIfReadOnly();  
                _disposeOfStateObjects = value; 
            }
	    }


	    /// <summary>
	    /// Get/Set the run the post execute options
	    /// </summary>
	    public virtual CallToPostExecute CallToPostExecute
	    {
	        get { return _callToPostExecute; }
            set 
            { 
                ThrowIfReadOnly();  
                _callToPostExecute = value; 
            }
	    }


	    /// <summary>
	    /// Get/Set the default post execute callback
	    /// </summary>
	    public virtual PostExecuteWorkItemCallback PostExecuteWorkItemCallback
	    {
	        get { return _postExecuteWorkItemCallback; }
            set 
            { 
                ThrowIfReadOnly();  
                _postExecuteWorkItemCallback = value; 
            }
	    }


	    /// <summary>
	    /// Get/Set if the work items execution should be suspended until the Start()
	    /// method is called.
	    /// </summary>
	    public virtual bool StartSuspended
	    {
	        get { return _startSuspended; }
            set 
            { 
                ThrowIfReadOnly();  
                _startSuspended = value; 
            }
	    }


	    /// <summary>
	    /// Get/Set the default priority that a work item gets when it is enqueued
	    /// </summary>
	    public virtual WorkItemPriority WorkItemPriority
	    {
	        get { return _workItemPriority; }
	        set { _workItemPriority = value; }
	    }

	    /// <summary>
	    /// Get/Set the if QueueWorkItem of Action<...>/Func<...> fill the
	    /// arguments as an object array into the state of the work item.
	    /// The arguments can be access later by IWorkItemResult.State.
	    /// </summary>
	    public virtual bool FillStateWithArgs
	    {
	        get { return _fillStateWithArgs; }
            set 
            { 
                ThrowIfReadOnly();  
                _fillStateWithArgs = value; 
            }
	    }

	    /// <summary>
        /// Get a readonly version of this WIGStartInfo
        /// </summary>
        /// <returns>Returns a readonly reference to this WIGStartInfoRO</returns>
        public WIGStartInfo AsReadOnly()
	    {
            return new WIGStartInfo(this) { _readOnly = true };
	    }
/*
	    #region WIGStartInfoRO class

        /// <summary>
        /// A readonly version of WIGStartInfo
        /// </summary>
        private class WIGStartInfoRO : WIGStartInfo
        {
            private readonly WIGStartInfo _wigStartInfoRO;

            public WIGStartInfoRO(WIGStartInfo wigStartInfoRO)
            {
                _wigStartInfoRO = wigStartInfoRO;
            }

            /// <summary>
            /// Get if to use the caller's security context
            /// </summary>
            public override bool UseCallerCallContext
            {
                get { return _wigStartInfoRO.UseCallerCallContext; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }

            /// <summary>
            /// Get if to use the caller's HTTP context
            /// </summary>
            public override bool UseCallerHttpContext
            {
                get { return _wigStartInfoRO.UseCallerHttpContext; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }

            /// <summary>
            /// Get if to dispose of the state object of a work item
            /// </summary>
            public override bool DisposeOfStateObjects
            {
                get { return _wigStartInfoRO.DisposeOfStateObjects; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }

            /// <summary>
            /// Get the run the post execute options
            /// </summary>
            public override CallToPostExecute CallToPostExecute
            {
                get { return _wigStartInfoRO.CallToPostExecute; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }

            /// <summary>
            /// Get the default post execute callback
            /// </summary>
            public override PostExecuteWorkItemCallback PostExecuteWorkItemCallback
            {
                get { return _wigStartInfoRO.PostExecuteWorkItemCallback; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }

            /// <summary>
            /// Get if the work items execution should be suspended until the Start()
            /// method is called.
            /// </summary>
            public override bool StartSuspended
            {
                get { return _wigStartInfoRO.StartSuspended; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }

            /// <summary>
            /// Get the default priority that a work item gets when it is enqueued
            /// </summary>
            public override WorkItemPriority WorkItemPriority
            {
                get { return _wigStartInfoRO.WorkItemPriority; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }

            /// <summary>
            /// Indicate if QueueWorkItem of Action<...>/Func<...> fill the
            /// arguments as an object array into the state of the work item.
            /// The arguments can be access later by IWorkItemResult.State.
            /// </summary>
            public override bool FillStateWithArgs
            {
                get { return _wigStartInfoRO.FillStateWithArgs; }
                set { throw new NotSupportedException("This is a readonly instance and set is not supported"); }
            }
        }
        #endregion
 */ 
    }
}
