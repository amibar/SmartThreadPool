using System;

namespace Amib.Threading
{
    public class QueueRejectedException : Exception
    {
        public QueueRejectedException(string message) : base(message) {}
    }
}
