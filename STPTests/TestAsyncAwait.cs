using Amib.Threading;
using NUnit.Framework;
using System;

namespace AsyncAwait
{
    [TestFixture]
    [Category("TestAsyncAwait")]
    public class TestAsyncAwait : TestAsyncAwaitBase
    {
        protected SmartThreadPool _stp;

        [SetUp]
        public void Init()
        {
            _stp = new SmartThreadPool();
        }

        [TearDown]
        public void Fini()
        {
            _stp.Shutdown();
        }

        protected override IWorkItemsGroup GetWIG() => _stp;
    }
}
