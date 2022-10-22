using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amib.Threading;
using NUnit.Framework;

namespace AsyncAwait
{
    [TestFixture]
    [Category("TestAsyncAwait")]
    public class TestWIGAsyncAwait : TestAsyncAwaitBase
    {
        protected SmartThreadPool _stp;
        private IWorkItemsGroup _wig;

        [OneTimeSetUp]
        public void Init()
        {
            Console.WriteLine("Creating SmartThreadPool");
            _stp = new SmartThreadPool();
            _wig = _stp.CreateWorkItemsGroup(10);
        }

        [OneTimeTearDown]
        public void Fini()
        {
            Console.WriteLine("Shutting down SmartThreadPool");
            _stp.Shutdown();
        }

        protected override IWorkItemsGroup GetWIG() => _wig;

        [Test]
        public async Task AwaitSerialSingle()
        {
            var wig = _stp.CreateWorkItemsGroup(1);

            async Task<int> DoProduce()
            {
                await Task.Delay(1000);

                return 17;
            }

            var wirProducer = wig.QueueWorkItem(DoProduce);

            async Task<bool> DoConsume()
            {
                var result = await wirProducer.GetResultAsync();

                return result == 17;
            }

            var wirConsumer = wig.QueueWorkItem(DoConsume);

            bool resultConsumer = await wirConsumer.GetResultAsync();

            Assert.IsTrue(resultConsumer);
            Assert.IsTrue(wirConsumer.Result);
        }

        [Test]
        public async Task AwaitSerialMultiple()
        {
            ConcurrentDictionary<int, bool> threads = new ConcurrentDictionary<int, bool>();

            SmartThreadPool smartThreadPool = new SmartThreadPool();
            var wig = smartThreadPool.CreateWorkItemsGroup(1);

            var wirAdders = new IWorkItemResult<int>[10];
            var wirCheckers = new IWorkItemResult<bool>[wirAdders.Length];

            async Task<int> DoAdd(int x, int y)
            {
                if (wig.InUseThreads > 1)
                    throw new Exception("Using too many threads");

                threads.TryAdd(Thread.CurrentThread.ManagedThreadId, true);

                await Task.Delay(100);

                if (wig.InUseThreads > 1)
                    throw new Exception("Using too many threads");

                threads.TryAdd(Thread.CurrentThread.ManagedThreadId, true);

                return x + y;
            }

            for (int i = 0; i < wirAdders.Length; i++)
            {
                wirAdders[i] = wig.QueueWorkItem(DoAdd, i, i + 1);
            }

            async Task<bool> DoCheck(int index, Task<int> task)
            {
                if (wig.InUseThreads > 1)
                    throw new Exception("Using too many threads");

                threads.TryAdd(Thread.CurrentThread.ManagedThreadId, true);

                var result = await task;

                if (wig.InUseThreads > 1)
                    throw new Exception("Using too many threads");

                threads.TryAdd(Thread.CurrentThread.ManagedThreadId, true);

                return result == (index + index + 1);
            }

            for (int i = 0; i < wirCheckers.Length; i++)
            {
                wirCheckers[i] = wig.QueueWorkItem(DoCheck, i, wirAdders[i].GetResultAsync());
            }

            await wig.WaitForIdleAsync(5_000);

            for (int i = 0; i < wirCheckers.Length; i++)
            {
                Assert.IsTrue(wirCheckers[i].IsCompleted);
                Assert.IsTrue(wirCheckers[i].Result);
            }

            smartThreadPool.Shutdown();
        }

        [Test]
        public async Task InterleavingTasks_Yield()
        {
            SmartThreadPool stp = new SmartThreadPool();
            var wig = stp.CreateWorkItemsGroup(1, new WIGStartInfo() { StartSuspended = true,});

            int count = 1000;
            var list = new System.Collections.Generic.List<int>(count);

            async Task AddEvens()
            {
                for (int i = 0; i < count; i += 2)
                {
                    list.Add(i);
                    await Task.Yield();
                }
            }

            async Task AddOdds()
            {
                for (int i = 1; i < count; i+=2)
                {
                    list.Add(i);
                    await Task.Yield();
                }
            }

            Assert.IsTrue(stp.IsIdle);
            Assert.IsTrue(wig.IsIdle);

            wig.QueueWorkItem(AddEvens);
            wig.QueueWorkItem(AddOdds);

            Assert.IsTrue(stp.IsIdle);
            Assert.IsFalse(wig.IsIdle);

            wig.Start();

            Assert.IsFalse(stp.IsIdle);
            Assert.IsFalse(wig.IsIdle);

            await wig.WaitForIdleAsync();

            Assert.That(list, Has.Count.EqualTo(count));
            Assert.That(list, Is.EquivalentTo(Enumerable.Range(0, count)));
        }
    }
}