using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amib.Threading;
using NUnit.Framework;

namespace AsyncAwait
{
    public abstract class TestAsyncAwaitBase
    {
        protected abstract IWorkItemsGroup GetWIG();

        static async void AsyncVoid(bool[] success, int i)
        {
            var workItem1 = SmartThreadPool.CurrentThreadEntry.CurrentWorkItem;

            await Task.Delay(100);

            var workItem2 = SmartThreadPool.CurrentThreadEntry.CurrentWorkItem;

            // Ensure we are in the same context as in before the await
            success[i] = ReferenceEquals(workItem1, workItem2);

            if (success[i])
            {
                await Task.Delay(100);

                var workItem3 = SmartThreadPool.CurrentThreadEntry.CurrentWorkItem;

                success[i] = ReferenceEquals(workItem1, workItem3);
            }
        }

        static async Task AsyncTask(bool[] success, int i)
        {
            var workItem1 = SmartThreadPool.CurrentThreadEntry.CurrentWorkItem;

            await Task.Delay(100);

            var workItem2 = SmartThreadPool.CurrentThreadEntry.CurrentWorkItem;

            // Ensure we are in the same context as in before the await
            success[i] = ReferenceEquals(workItem1, workItem2);

            if (success[i])
            {
                await Task.Delay(100);

                var workItem3 = SmartThreadPool.CurrentThreadEntry.CurrentWorkItem;

                success[i] = ReferenceEquals(workItem1, workItem3);
            }
        }

        static async Task<T> AsyncTask<T>(bool[] success, int i, T result)
        {
            var workItem1 = SmartThreadPool.CurrentThreadEntry.CurrentWorkItem;

            await Task.Delay(100);

            var workItem2 = SmartThreadPool.CurrentThreadEntry.CurrentWorkItem;

            // Ensure we are in the same context as in before the await
            success[i] = ReferenceEquals(workItem1, workItem2);

            if (success[i])
            {
                await Task.Delay(100);

                var workItem3 = SmartThreadPool.CurrentThreadEntry.CurrentWorkItem;

                success[i] = ReferenceEquals(workItem1, workItem3);
            }

            return result;
        }

        [Test]
        public async Task SingleAsyncVoid()
        {
            bool[] success = {false};

            var wir = GetWIG().QueueWorkItem(AsyncVoid, success, 0);

            // Wait for the work item to complete
            await wir.GetResultAsync();

            Assert.IsTrue(success.All(s => s));
        }

        [Test]
        public async Task MultipleAsyncVoid()
        {
            bool[] success = new bool[10];

            for (int i = 0; i < success.Length; i++)
            {
                GetWIG().QueueWorkItem(AsyncVoid, success, i);
            }

            // Wait for all work items to complete
            await GetWIG().WaitForIdleAsync();

            Assert.IsTrue(success.All(s => s));
        }

        [Test]
        public async Task SingleAsyncTask()
        {
            bool[] success = {false};

            var wir = GetWIG().QueueWorkItem(AsyncTask, success, 0);

            // Wait for the work item to complete
            await wir.GetResultAsync();

            Assert.IsTrue(success.All(s => s));
        }

        [Test]
        public async Task MultipleAsyncTask()
        {
            bool[] success = new bool[10];

            for (int i = 0; i < success.Length; i++)
            {
                GetWIG().QueueWorkItem(AsyncTask, success, i);
            }

            // Wait for all work items to complete
            await GetWIG().WaitForIdleAsync();

            Assert.IsTrue(success.All(s => s));
        }

        [Test]
        public async Task SingleAsyncTaskT()
        {
            bool[] success = {false};

            var wir = GetWIG().QueueWorkItem(AsyncTask, success, 0, "abc");

            // Wait for the work item to complete
            var result = await wir.GetResultAsync();

            Assert.IsTrue(success.All(s => s));
            Assert.AreEqual(result, "abc");

        }

        [Test]
        public async Task MultipleAsyncTaskT()
        {
            bool[] success = new bool[10];
            IWorkItemResult<string>[] wirs = new IWorkItemResult<string>[success.Length];

            for (int i = 0; i < success.Length; i++)
            {
                wirs[i] = GetWIG().QueueWorkItem(AsyncTask, success, i, "abc" + i);
            }

            // Wait for all work items to complete
            await GetWIG().WaitForIdleAsync();

            Assert.IsTrue(success.All(s => s));

            for (int i = 0; i < success.Length; i++)
            {
                var task = wirs[i].GetResultAsync();
                Assert.IsTrue(task.IsCompleted);
                Assert.AreEqual(task.Result, "abc" + i);
            }
        }

        [Test]
        public async Task WaitForIdle_AsyncVoid()
        {
            bool waited = false;

            async void DoWork()
            {
                await Task.Delay(1000);

                waited = true;
            }

            GetWIG().QueueWorkItem(DoWork);

            var isIdleTask = GetWIG().WaitForIdleAsync();
            var timeoutTask = Task.Delay(2000);

            var resultTask = await Task.WhenAny(isIdleTask, timeoutTask);

            Assert.AreSame(resultTask, isIdleTask);
            Assert.IsTrue(waited);
        }

        [Test]
        public async Task GetResultAsync_On_AsyncVoid()
        {
            int data = 0;

            async void DoProduce()
            {
                await Task.Delay(2000);
                data = 17;
            }

            var wirProducer = GetWIG().QueueWorkItem(DoProduce);

            async Task<bool> DoConsume()
            {
                await wirProducer.GetResultAsync();

                return data == 17;
            }

            var wirConsumer = GetWIG().QueueWorkItem(DoConsume);

            await Task.WhenAll(
                wirConsumer.GetResultAsync(),
                wirProducer.GetResultAsync());

            Assert.IsTrue(wirConsumer.Result);
        }

        [Test]
        public async Task GetResultAsync_On_AsyncTaskT()
        {
            async Task<int> DoProduce()
            {
                await Task.Delay(1000);

                return 17;
            }

            var wirProducer = GetWIG().QueueWorkItem(DoProduce);

            async Task<bool> DoConsume()
            {
                var result = await wirProducer.GetResultAsync();

                return result == 17;
            }

            var wirConsumer = GetWIG().QueueWorkItem(DoConsume);

            await Task.WhenAll(
                wirConsumer.GetResultAsync(),
                wirProducer.GetResultAsync());

            Assert.IsTrue(wirConsumer.Result);
        }

        [Test]
        public async Task QueueAnonymousAsync_Task()
        {
            var wig = GetWIG();
            var tcs = new TaskCompletionSource<bool>();

            async Task DoProduce()
            {
                bool correctWig =
                    ReferenceEquals(SmartThreadPool.CurrentThreadEntry.CurrentWorkItem.WorkItemsGroup, wig);

                if (!correctWig)
                {
                    tcs.SetResult(false);
                    return;
                }

                await Task.Delay(100);

                correctWig = ReferenceEquals(SmartThreadPool.CurrentThreadEntry.CurrentWorkItem.WorkItemsGroup, wig);

                if (!correctWig)
                {
                    tcs.SetResult(false);
                    return;
                }

                tcs.SetResult(true);
            }

            wig.QueueWorkItem(async () => await DoProduce());

            var result = await tcs.Task;

            Assert.IsTrue(result);
        }

        [Test]
        public async Task QueueAnonymousAsync_TaskT()
        {
            var wig = GetWIG();
            var tcs = new TaskCompletionSource<bool>();

            async Task<int> DoProduce()
            {
                bool correctWig =
                    ReferenceEquals(SmartThreadPool.CurrentThreadEntry.CurrentWorkItem.WorkItemsGroup, wig);

                if (!correctWig)
                {
                    tcs.SetResult(false);
                    return -1;
                }

                await Task.Delay(100);

                correctWig = ReferenceEquals(SmartThreadPool.CurrentThreadEntry.CurrentWorkItem.WorkItemsGroup, wig);

                if (!correctWig)
                {
                    tcs.SetResult(false);
                    return -1;
                }

                tcs.SetResult(true);
                return 17;
            }

            var wir = wig.QueueWorkItem(async () => await DoProduce());

            var result = await tcs.Task;

            Assert.IsTrue(result);
            Assert.AreEqual(17, wir.Result);
        }

        [Test]
        public async Task RunTask_Action()
        {
            var wig = GetWIG();
            int result = 0;
            void DoSomething()
            {
                Assert.AreSame(SmartThreadPool.CurrentThreadEntry.CurrentWorkItem.WorkItemsGroup, wig);

                result = 17;
            }

            await wig.RunTask(DoSomething);

            Assert.AreEqual(17, result);
            Assert.IsTrue(wig.IsIdle);
        }

        [Test]
        public async Task RunTask_FuncT()
        {
            var wig = GetWIG();

            int ComputeSomething()
            {
                Assert.AreSame(SmartThreadPool.CurrentThreadEntry.CurrentWorkItem.WorkItemsGroup, wig);
                return 17;
            }

            int result = await wig.RunTask(ComputeSomething);

            Assert.AreEqual(17, result);
            Assert.IsTrue(wig.IsIdle);
        }

        [Test]
        public async Task RunTask_FuncTask()
        {
            var wig = GetWIG();
            int result = 0;
            Task DoSomething()
            {
                Assert.AreSame(SmartThreadPool.CurrentThreadEntry.CurrentWorkItem.WorkItemsGroup, wig);
                result = 17;
                return Task.CompletedTask;
            }

            await wig.RunTask(DoSomething);

            Assert.AreEqual(17, result);
            Assert.IsTrue(wig.IsIdle);
        }

        [Test]
        public async Task RunTask_FuncTaskT()
        {
            var wig = GetWIG();

            Task<int> ComputeSomething()
            {
                Assert.AreSame(SmartThreadPool.CurrentThreadEntry.CurrentWorkItem.WorkItemsGroup, wig);
                return Task.FromResult(17);
            }

            int result = await wig.RunTask(ComputeSomething);

            Assert.AreEqual(17, result);
            Assert.IsTrue(wig.IsIdle);
        }

        [Test]
        public async Task RunTask_CancellationToken_Awaiting()
        {
            var wig = GetWIG();
            var isReady = new TaskCompletionSource<bool>();
            var waitingForCancel = new TaskCompletionSource<bool>();

            bool runAfterWaiting = false;

            async Task<int> ComputeSomething()
            {
                Assert.AreSame(SmartThreadPool.CurrentThreadEntry.CurrentWorkItem.WorkItemsGroup, wig);

                isReady.SetResult(true);
                await waitingForCancel.Task;

                runAfterWaiting = true;

                return 0;
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            var t = wig.RunTask(ComputeSomething, cts.Token);

            await isReady.Task;

            cts.Cancel();
            waitingForCancel.SetResult(true);

            Exception ex = null;
            try
            {
                await t;
            }
            catch (Exception e)
            {
                ex = e;
            }

            await wig.WaitForIdleAsync();

            Assert.IsTrue(t.IsCompleted);
            Assert.IsFalse(runAfterWaiting);
            Assert.IsTrue(ex is TaskCanceledException);
            Assert.IsTrue(wig.IsIdle);
        }

        [Test]
        public async Task RunTask_CancellationToken_Waiting()
        {
            var wig = GetWIG();
            AutoResetEvent isReady = new AutoResetEvent(false);
            AutoResetEvent waitingForCancel = new AutoResetEvent(false);

            bool runAfterWaiting = false;
            bool isWorkItemCanceled = false;

            Task<int> ComputeSomething()
            {
                Assert.AreSame(SmartThreadPool.CurrentThreadEntry.CurrentWorkItem.WorkItemsGroup, wig);

                isReady.Set();
                waitingForCancel.WaitOne();

                runAfterWaiting = true;

                Assert.AreSame(SmartThreadPool.CurrentThreadEntry.CurrentWorkItem.WorkItemsGroup, wig);

                isWorkItemCanceled = SmartThreadPool.IsWorkItemCanceled;

                return Task.FromResult(0);
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            var t = wig.RunTask(ComputeSomething, cts.Token);

            isReady.WaitOne();

            cts.Cancel();
            waitingForCancel.Set();

            Exception ex = null;
            try
            {
                await t;
            }
            catch (Exception e)
            {
                ex = e;
            }

            await wig.WaitForIdleAsync();


            Assert.IsTrue(t.IsCompleted);
            Assert.IsTrue(runAfterWaiting);
            Assert.IsTrue(isWorkItemCanceled);
            Assert.IsTrue(ex is TaskCanceledException);
            Assert.IsTrue(wig.IsIdle);
        }
    }
}