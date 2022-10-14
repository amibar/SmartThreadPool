using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

#if _ASYNC_SUPPORTED

namespace Amib.Threading.Internal
{
    class STPSynchronizationContext : SynchronizationContext
    {
        private WorkItem WorkItem { get; }

        public STPSynchronizationContext(WorkItem workItem)
        {
            WorkItem = workItem;
        }

        public override SynchronizationContext CreateCopy()
        {
            return new STPSynchronizationContext(WorkItem);
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            WorkItem.HandleRequeue(d, state);
        }

        public override void OperationStarted()
        {
            WorkItem.SetTaskState(false);
        }

        public override void OperationCompleted()
        {
            WorkItem.SetTaskState(true);
        }
    }
}

#endif