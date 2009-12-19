using System;

namespace Amib.Threading
{
	/// <summary>
	/// Summary description for WIGStartInfo.
	/// </summary>
	public class WIGStartInfo
	{
		private bool _useCallerCallContext = SmartThreadPool.DefaultUseCallerCallContext;
		private bool _useCallerHttpContext = SmartThreadPool.DefaultUseCallerHttpContext;
		private bool _disposeOfStateObjects = SmartThreadPool.DefaultDisposeOfStateObjects;
		private CallToPostExecute _callToPostExecute = SmartThreadPool.DefaultCallToPostExecute;
		private PostExecuteWorkItemCallback _postExecuteWorkItemCallback = SmartThreadPool.DefaultPostExecuteWorkItemCallback;
        private WorkItemPriority _workItemPriority = SmartThreadPool.DefaultWorkItemPriority;
		private bool _startSuspended = SmartThreadPool.DefaultStartSuspended;
	    private bool _fillStateWithArgs = SmartThreadPool.DefaultFillStateWithArgs;

        public WIGStartInfo()
        {
        }

        public WIGStartInfo(WIGStartInfo wigStartInfo)
        {
            _useCallerCallContext = wigStartInfo._useCallerCallContext;
            _useCallerHttpContext = wigStartInfo._useCallerHttpContext;
            _disposeOfStateObjects = wigStartInfo._disposeOfStateObjects;
            _callToPostExecute = wigStartInfo._callToPostExecute;
            _postExecuteWorkItemCallback = wigStartInfo._postExecuteWorkItemCallback;
            _workItemPriority = wigStartInfo._workItemPriority;
            _startSuspended = wigStartInfo._startSuspended;
            _fillStateWithArgs = wigStartInfo._fillStateWithArgs;
        }

        /// <summary>
        /// Get/Set if to use the caller's security context
        /// </summary>
        public virtual bool UseCallerCallContext
		{
			get { return _useCallerCallContext; }
			set { _useCallerCallContext = value; }
		}

        /// <summary>
        /// Get/Set if to use the caller's HTTP context
        /// </summary>
        public virtual bool UseCallerHttpContext
		{
			get { return _useCallerHttpContext; }
			set { _useCallerHttpContext = value; }
		}

        /// <summary>
        /// Get/Set if to dispose of the state object of a work item
        /// </summary>
        public virtual bool DisposeOfStateObjects
		{
			get { return _disposeOfStateObjects; }
			set { _disposeOfStateObjects = value; }
		}

        /// <summary>
        /// Get/Set the run the post execute options
        /// </summary>
        public virtual CallToPostExecute CallToPostExecute
		{
			get { return _callToPostExecute; }
			set { _callToPostExecute = value; }
		}

        /// <summary>
        /// Get/Set the default post execute callback
        /// </summary>
        public virtual PostExecuteWorkItemCallback PostExecuteWorkItemCallback
		{
			get { return _postExecuteWorkItemCallback; }
			set { _postExecuteWorkItemCallback = value; }
		}

        /// <summary>
        /// Get/Set if the work items execution should be suspended until the Start()
        /// method is called.
        /// </summary>
        public virtual bool StartSuspended
		{
			get { return _startSuspended; }
			set { _startSuspended = value; }
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
            set { _fillStateWithArgs = value; }
        }

        /// <summary>
        /// Get a readonly version of this WIGStartInfo
        /// </summary>
        /// <returns>Returns a readonly reference to this WIGStartInfoRO</returns>
        public WIGStartInfo AsReadOnly()
        {
            return new WIGStartInfoRO(this);
        }

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
    }
}
