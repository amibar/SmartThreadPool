using System;
using System.Threading;
using Amib.Threading.Internal;
using NUnit.Framework;

using Amib.Threading;
using System.Reflection;

#pragma warning disable 168

namespace SmartThreadPoolTests
{
    public static class QueueWorkItemHelper
    {
        //IWorkItemResult QueueWorkItem(WorkItemCallback callback);
        public static void TestQueueWorkItemCall(IWorkItemsGroup wig)
        {
            WorkItemInfo wii = new WorkItemInfo();
            WorkItemInfoComparer wiic = new WorkItemInfoComparer(wii);

            IWorkItemResult wir = wig.QueueWorkItem(wiic.CompareWorkItemInfo);

            bool success = (bool)wir.Result;

            Assert.IsTrue(success);
        }

        //IWorkItemResult QueueWorkItem(WorkItemCallback callback, WorkItemPriority workItemPriority);
        public static void TestQueueWorkItemCallPrio(IWorkItemsGroup wig)
        {
            WorkItemInfo wii = new WorkItemInfo();
            wii.WorkItemPriority = WorkItemPriority.AboveNormal;
            WorkItemInfoComparer wiic = new WorkItemInfoComparer(wii);

            IWorkItemResult wir = wig.QueueWorkItem((WorkItemCallback)wiic.CompareWorkItemInfo, WorkItemPriority.AboveNormal);

            bool success = (bool)wir.Result;

            Assert.IsTrue(success);
        }

        //IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state);
        public static void TestQueueWorkItemCallStat(IWorkItemsGroup wig)
        {
            object state = new object();
            WorkItemInfo wii = new WorkItemInfo();
            WorkItemInfoComparer wiic = new WorkItemInfoComparer(wii, state);

            IWorkItemResult wir = wig.QueueWorkItem((WorkItemCallback) wiic.CompareWorkItemInfo, state);

            bool success = (bool)wir.Result;

            Assert.IsTrue(success);
        }

        //IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, WorkItemPriority workItemPriority);
        public static void TestQueueWorkItemCallStatPrio(IWorkItemsGroup wig)
        {
            object state = new object();
            WorkItemInfo wii = new WorkItemInfo();
            wii.WorkItemPriority = WorkItemPriority.AboveNormal;
            WorkItemInfoComparer wiic = new WorkItemInfoComparer(wii, state);

            IWorkItemResult wir = wig.QueueWorkItem((WorkItemCallback) wiic.CompareWorkItemInfo, state, WorkItemPriority.AboveNormal);

            bool success = (bool)wir.Result;

            Assert.IsTrue(success);
        }

        //IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, PostExecuteWorkItemCallback postExecuteWorkItemCallback);
        public static void TestQueueWorkItemCallStatPost(IWorkItemsGroup wig)
        {
            bool postExecuteCalled = false;
            object state = new object();
            PostExecuteWorkItemCallback postExecuteWorkItemCallback = delegate(IWorkItemResult w) { postExecuteCalled = true; };
            WorkItemInfo wii = new WorkItemInfo();
            wii.PostExecuteWorkItemCallback = postExecuteWorkItemCallback;
            WorkItemInfoComparer wiic = new WorkItemInfoComparer(wii, state);

            IWorkItemResult wir = wig.QueueWorkItem(
                wiic.CompareWorkItemInfo,
                state,
                postExecuteWorkItemCallback);

            // We must wait for idle to let the post execute run
            wig.WaitForIdle();

            bool success = (bool)wir.Result;

            Assert.IsTrue(success);
            Assert.IsTrue(postExecuteCalled);
        }

        //IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, PostExecuteWorkItemCallback postExecuteWorkItemCallback, WorkItemPriority workItemPriority);
        public static void TestQueueWorkItemCallStatPostPrio(IWorkItemsGroup wig)
        {
            bool postExecuteCalled = false;
            object state = new object();
            PostExecuteWorkItemCallback postExecuteWorkItemCallback = delegate(IWorkItemResult w) { postExecuteCalled = true; };
            WorkItemInfo wii = new WorkItemInfo();
            wii.WorkItemPriority = WorkItemPriority.BelowNormal;
            wii.PostExecuteWorkItemCallback = postExecuteWorkItemCallback;
            WorkItemInfoComparer wiic = new WorkItemInfoComparer(wii, state);

            IWorkItemResult wir = wig.QueueWorkItem(
                wiic.CompareWorkItemInfo,
                state,
                postExecuteWorkItemCallback, 
                WorkItemPriority.BelowNormal);

            // We must wait for idle to let the post execute run
            wig.WaitForIdle();

            bool success = (bool)wir.Result;

            Assert.IsTrue(success);
            Assert.IsTrue(postExecuteCalled);
        }

        //IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, PostExecuteWorkItemCallback postExecuteWorkItemCallback, CallToPostExecute callToPostExecute);
        public static void TestQueueWorkItemCallStatPostPflg(IWorkItemsGroup wig)
        {
            bool postExecuteCalled;
            CallToPostExecute callToPostExecute;
            object state = new object();
            PostExecuteWorkItemCallback postExecuteWorkItemCallback = delegate(IWorkItemResult w) { postExecuteCalled = true; };
            WorkItemInfo wii = new WorkItemInfo();
            wii.PostExecuteWorkItemCallback = postExecuteWorkItemCallback;
            WorkItemInfoComparer wiic = new WorkItemInfoComparer(wii, state);
            IWorkItemResult wir;
            bool success;

            /////////////////////////////////////////////////////////////////////////////

            callToPostExecute = CallToPostExecute.Always;

            // Check without cancel
            postExecuteCalled = false;
            wiic.SleepTime = 0;

            wii.CallToPostExecute = callToPostExecute;

            wir = wig.QueueWorkItem(
                wiic.CompareWorkItemInfo,
                state,
                postExecuteWorkItemCallback,
                callToPostExecute);

            // We must wait for idle to let the post execute run
            wig.WaitForIdle();

            success = (bool)wir.Result;

            Assert.IsTrue(success);
            Assert.IsTrue(postExecuteCalled);

            // Check with cancel
            success = false;
            postExecuteCalled = false;
            wiic.SleepTime = 100;

            wir = wig.QueueWorkItem(
                wiic.CompareWorkItemInfo,
                state,
                postExecuteWorkItemCallback,
                callToPostExecute);

            wir.Cancel();

            // We must wait for idle to let the post execute run
            wig.WaitForIdle();

            Assert.IsTrue(postExecuteCalled);

            try
            {
                wir.GetResult();
            }
            catch (WorkItemCancelException ce)
            {
                success = true;
            }

            Assert.IsTrue(success);

            /////////////////////////////////////////////////////////////////////////////

            callToPostExecute = CallToPostExecute.Never;

            postExecuteCalled = false;
            wiic.SleepTime = 0;

            wii.CallToPostExecute = callToPostExecute;

            wir = wig.QueueWorkItem(
                wiic.CompareWorkItemInfo,
                state,
                postExecuteWorkItemCallback,
                callToPostExecute);

            // We must wait for idle to let the post execute run
            wig.WaitForIdle();

            success = (bool)wir.Result;

            Assert.IsTrue(success);
            Assert.IsFalse(postExecuteCalled);

            // Check with cancel
            success = false;
            postExecuteCalled = false;
            wiic.SleepTime = 100;

            wir = wig.QueueWorkItem(
                wiic.CompareWorkItemInfo,
                state,
                postExecuteWorkItemCallback,
                callToPostExecute);

            wir.Cancel();

            // We must wait for idle to let the post execute run
            wig.WaitForIdle();

            Assert.IsFalse(postExecuteCalled);

            try
            {
                wir.GetResult();
            }
            catch (WorkItemCancelException ce)
            {
                success = true;
            }

            Assert.IsTrue(success);

            /////////////////////////////////////////////////////////////////////////////

            callToPostExecute = CallToPostExecute.WhenWorkItemNotCanceled;

            postExecuteCalled = false;
            wiic.SleepTime = 0;

            wii.CallToPostExecute = callToPostExecute;

            wir = wig.QueueWorkItem(
                wiic.CompareWorkItemInfo,
                state,
                postExecuteWorkItemCallback,
                callToPostExecute);

            // We must wait for idle to let the post execute run
            wig.WaitForIdle();

            success = (bool)wir.Result;

            Assert.IsTrue(success);
            Assert.IsTrue(postExecuteCalled);

            // Check with cancel
            success = false;
            postExecuteCalled = false;
            wiic.SleepTime = 100;

            wir = wig.QueueWorkItem(
                wiic.CompareWorkItemInfo,
                state,
                postExecuteWorkItemCallback,
                callToPostExecute);

            wir.Cancel();

            // We must wait for idle to let the post execute run
            wig.WaitForIdle();

            Assert.IsFalse(postExecuteCalled);

            try
            {
                wir.GetResult();
            }
            catch (WorkItemCancelException ce)
            {
                success = true;
            }

            Assert.IsTrue(success);

            /////////////////////////////////////////////////////////////////////////////

            callToPostExecute = CallToPostExecute.WhenWorkItemCanceled;

            postExecuteCalled = false;
            wiic.SleepTime = 0;

            wii.CallToPostExecute = callToPostExecute;

            wir = wig.QueueWorkItem(
                wiic.CompareWorkItemInfo,
                state,
                postExecuteWorkItemCallback,
                callToPostExecute);

            // We must wait for idle to let the post execute run
            wig.WaitForIdle();

            success = (bool)wir.Result;

            Assert.IsTrue(success);
            Assert.IsFalse(postExecuteCalled);

            // Check with cancel
            success = false;
            postExecuteCalled = false;
            wiic.SleepTime = 100;

            wir = wig.QueueWorkItem(
                wiic.CompareWorkItemInfo,
                state,
                postExecuteWorkItemCallback,
                callToPostExecute);

            wir.Cancel();

            // We must wait for idle to let the post execute run
            wig.WaitForIdle();

            Assert.IsTrue(postExecuteCalled);

            try
            {
                wir.GetResult();
            }
            catch (WorkItemCancelException ce)
            {
                success = true;
            }

            Assert.IsTrue(success);
        }

        //IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, PostExecuteWorkItemCallback postExecuteWorkItemCallback, CallToPostExecute callToPostExecute, WorkItemPriority workItemPriority);
        public static void TestQueueWorkItemCallStatPostPflgPrio(IWorkItemsGroup wig)
        {
            bool postExecuteCalled;
            CallToPostExecute callToPostExecute;
            object state = new object();
            PostExecuteWorkItemCallback postExecuteWorkItemCallback = delegate(IWorkItemResult w) { postExecuteCalled = true; };
            WorkItemInfo wii = new WorkItemInfo();
            wii.PostExecuteWorkItemCallback = postExecuteWorkItemCallback;
            WorkItemInfoComparer wiic = new WorkItemInfoComparer(wii, state);
            WorkItemPriority workItemPriority;
            IWorkItemResult wir;
            bool success;

            /////////////////////////////////////////////////////////////////////////////

            callToPostExecute = CallToPostExecute.Always;
            workItemPriority = WorkItemPriority.Lowest;

            // Check without cancel
            postExecuteCalled = false;
            wiic.SleepTime = 0;

            wii.CallToPostExecute = callToPostExecute;
            wii.WorkItemPriority = workItemPriority;

            wir = wig.QueueWorkItem(
                wiic.CompareWorkItemInfo,
                state,
                postExecuteWorkItemCallback,
                callToPostExecute,
                workItemPriority);

            // We must wait for idle to let the post execute run
            wig.WaitForIdle();

            success = (bool)wir.Result;

            Assert.IsTrue(success);
            Assert.IsTrue(postExecuteCalled);

            // Check with cancel
            success = false;
            postExecuteCalled = false;
            wiic.SleepTime = 100;

            wir = wig.QueueWorkItem(
                wiic.CompareWorkItemInfo,
                state,
                postExecuteWorkItemCallback,
                callToPostExecute,
                workItemPriority);

            wir.Cancel();

            // We must wait for idle to let the post execute run
            wig.WaitForIdle();

            Assert.IsTrue(postExecuteCalled);

            try
            {
                wir.GetResult();
            }
            catch (WorkItemCancelException ce)
            {
                success = true;
            }

            Assert.IsTrue(success);

            /////////////////////////////////////////////////////////////////////////////

            callToPostExecute = CallToPostExecute.Never;
            workItemPriority = WorkItemPriority.Highest;

            postExecuteCalled = false;
            wiic.SleepTime = 0;

            wii.CallToPostExecute = callToPostExecute;
            wii.WorkItemPriority = workItemPriority;

            wir = wig.QueueWorkItem(
                wiic.CompareWorkItemInfo,
                state,
                postExecuteWorkItemCallback,
                callToPostExecute, 
                workItemPriority);

            // We must wait for idle to let the post execute run
            wig.WaitForIdle();

            success = (bool)wir.Result;

            Assert.IsTrue(success);
            Assert.IsFalse(postExecuteCalled);

            // Check with cancel
            success = false;
            postExecuteCalled = false;
            wiic.SleepTime = 100;

            wir = wig.QueueWorkItem(
                wiic.CompareWorkItemInfo,
                state,
                postExecuteWorkItemCallback,
                callToPostExecute,
                workItemPriority);

            wir.Cancel();

            // We must wait for idle to let the post execute run
            wig.WaitForIdle();

            Assert.IsFalse(postExecuteCalled);

            try
            {
                wir.GetResult();
            }
            catch (WorkItemCancelException ce)
            {
                success = true;
            }

            Assert.IsTrue(success);

            /////////////////////////////////////////////////////////////////////////////

            callToPostExecute = CallToPostExecute.WhenWorkItemNotCanceled;
            workItemPriority = WorkItemPriority.AboveNormal;

            postExecuteCalled = false;
            wiic.SleepTime = 0;

            wii.CallToPostExecute = callToPostExecute;
            wii.WorkItemPriority = workItemPriority;

            wir = wig.QueueWorkItem(
                wiic.CompareWorkItemInfo,
                state,
                postExecuteWorkItemCallback,
                callToPostExecute,
                workItemPriority);

            // We must wait for idle to let the post execute run
            wig.WaitForIdle();

            success = (bool)wir.Result;

            Assert.IsTrue(success);
            Assert.IsTrue(postExecuteCalled);

            // Check with cancel
            success = false;
            postExecuteCalled = false;
            wiic.SleepTime = 100;

            wir = wig.QueueWorkItem(
                wiic.CompareWorkItemInfo,
                state,
                postExecuteWorkItemCallback,
                callToPostExecute,
                workItemPriority);

            wir.Cancel();

            // We must wait for idle to let the post execute run
            wig.WaitForIdle();

            Assert.IsFalse(postExecuteCalled);

            try
            {
                wir.GetResult();
            }
            catch (WorkItemCancelException ce)
            {
                success = true;
            }

            Assert.IsTrue(success);


            /////////////////////////////////////////////////////////////////////////////

            callToPostExecute = CallToPostExecute.WhenWorkItemCanceled;
            workItemPriority = WorkItemPriority.BelowNormal;

            postExecuteCalled = false;
            wiic.SleepTime = 0;

            wii.CallToPostExecute = callToPostExecute;
            wii.WorkItemPriority = workItemPriority;

            wir = wig.QueueWorkItem(
                wiic.CompareWorkItemInfo,
                state,
                postExecuteWorkItemCallback,
                callToPostExecute,
                workItemPriority);

            // We must wait for idle to let the post execute run
            wig.WaitForIdle();

            success = (bool)wir.Result;

            Assert.IsTrue(success);
            Assert.IsFalse(postExecuteCalled);

            // Check with cancel
            success = false;
            postExecuteCalled = false;
            wiic.SleepTime = 100;

            wir = wig.QueueWorkItem(
                wiic.CompareWorkItemInfo,
                state,
                postExecuteWorkItemCallback,
                callToPostExecute,
                workItemPriority);

            wir.Cancel();

            // We must wait for idle to let the post execute run
            wig.WaitForIdle();

            Assert.IsTrue(postExecuteCalled);

            try
            {
                wir.GetResult();
            }
            catch (WorkItemCancelException ce)
            {
                success = true;
            }

            Assert.IsTrue(success);
        }

        //IWorkItemResult QueueWorkItem(WorkItemInfo workItemInfo, WorkItemCallback callback);
        public static void TestQueueWorkItemInfoCall(IWorkItemsGroup wig)
        {
            WorkItemInfo wii = new WorkItemInfo();
            wii.CallToPostExecute = CallToPostExecute.Never;
            wii.DisposeOfStateObjects = true;
            wii.PostExecuteWorkItemCallback = delegate(IWorkItemResult w) { };
            wii.UseCallerCallContext = true;
            wii.UseCallerHttpContext = true;
            wii.WorkItemPriority = WorkItemPriority.Highest;

            WorkItemInfoComparer wiic = new WorkItemInfoComparer(wii);

            IWorkItemResult wir = wig.QueueWorkItem(wii, wiic.CompareWorkItemInfo);

            // We must wait for idle to let the post execute run
            wig.WaitForIdle();

            bool success = (bool)wir.Result;

            Assert.IsTrue(success);
        }

        //IWorkItemResult QueueWorkItem(WorkItemInfo workItemInfo, WorkItemCallback callback, object state);
        public static void TestQueueWorkItemInfoCallStat(IWorkItemsGroup wig)
        {
            object state = new object();
            WorkItemInfo wii = new WorkItemInfo();
            wii.CallToPostExecute = CallToPostExecute.Never;
            wii.DisposeOfStateObjects = true;
            wii.PostExecuteWorkItemCallback = delegate(IWorkItemResult w) { };
            wii.UseCallerCallContext = true;
            wii.UseCallerHttpContext = true;
            wii.WorkItemPriority = WorkItemPriority.Highest;

            WorkItemInfoComparer wiic = new WorkItemInfoComparer(wii, state);

            IWorkItemResult wir = wig.QueueWorkItem(wii, wiic.CompareWorkItemInfo, state);

            // We must wait for idle to let the post execute run
            wig.WaitForIdle();

            bool success = (bool)wir.Result;

            Assert.IsTrue(success);
        }

        private class WorkItemInfoComparer
        {
            private WorkItemInfo _neededWorkItemInfo;
            private object _state;

            public int SleepTime { get; set; }

            public WorkItemInfoComparer(WorkItemInfo workItemInfo)
            {
                _neededWorkItemInfo = workItemInfo;
                _state = null;
            }

            public WorkItemInfoComparer(WorkItemInfo workItemInfo, object state)
            {
                _neededWorkItemInfo = workItemInfo;
                _state = state;
            }

            public object CompareWorkItemInfo(object state)
            {
                bool equals = object.Equals(_state, state);
                if (equals)
                {
                    WorkItemInfo currentWorkItemInfo = SmartThreadPool.CurrentThreadEntry.CurrentWorkItem.WorkItemInfo;
                    equals = CompareWorkItemInfo(currentWorkItemInfo, _neededWorkItemInfo);
                }
                if (SleepTime > 0)
                {
                    Thread.Sleep(SleepTime);
                }

                return equals;
            }

            private bool CompareWorkItemInfo(WorkItemInfo wii1, WorkItemInfo wii2)
            {
                bool equal = true;
                equal = equal && (wii1.CallToPostExecute == wii2.CallToPostExecute);
                equal = equal && (wii1.DisposeOfStateObjects == wii2.DisposeOfStateObjects);
                equal = equal && (wii1.PostExecuteWorkItemCallback == wii2.PostExecuteWorkItemCallback);
                equal = equal && (wii1.UseCallerCallContext == wii2.UseCallerCallContext);
                equal = equal && (wii1.UseCallerHttpContext == wii2.UseCallerHttpContext);
                equal = equal && (wii1.WorkItemPriority == wii2.WorkItemPriority);

                return equal;
            }
        }
    }
}
