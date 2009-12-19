using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Amib.Threading;
using System.Threading;

namespace STPSLDemo
{
    public partial class Page : UserControl
    {
        private System.Windows.Threading.DispatcherTimer _timer;

        private SmartThreadPool _stp;
        private bool running;

        private int workItemsCompleted;
        private int workItemsGenerated;
        private Thread workItemsProducerThread;

        private int _interval = 0;
        private int _consumingTime = 0;

        public Page()
        {
            InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            _timer.Tick += _timer_Tick;
            _timer.Start();

            _spinMinThreads.Value = 0;
            _spinMaxThreads.Value = 10;
            _spinIdleTimeout.Value = 5;
            _spinWorkItemTime.Value = 100;
            _spinInterval.Value = 100;
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            SmartThreadPool stp = _stp;
            if (null == stp)
            {
                return;
            }

            int threadsInUse = (int)_stp.PerformanceCountersReader.InUseThreads;
            int threadsInPool = (int)_stp.PerformanceCountersReader.ActiveThreads;

            _threadsUsed.Text = threadsInUse.ToString();
            _threadsInPool.Text = threadsInPool.ToString();
            _queuedWorkItems.Text = _stp.PerformanceCountersReader.WorkItemsQueued.ToString();
            _usageControl.Value1 = threadsInUse;
            _usageControl.Value2 = threadsInPool;
            _completedWorkItems.Text = _stp.PerformanceCountersReader.WorkItemsProcessed.ToString();
            _generatedWorkItems.Text = workItemsGenerated.ToString();
            _historyUsageControl.AddValues(threadsInUse, threadsInPool);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateControls(false);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;

            UpdateControls(true);
            workItemsCompleted = 0;
            workItemsGenerated = 0;

            STPStartInfo stpStartInfo = new STPStartInfo();
            stpStartInfo.IdleTimeout = _spinIdleTimeout.Value * 1000;
            stpStartInfo.MaxWorkerThreads = _spinMaxThreads.Value;
            stpStartInfo.MinWorkerThreads = _spinMinThreads.Value;
            stpStartInfo.EnableLocalPerformanceCounters = true;

            _stp = new SmartThreadPool(stpStartInfo);

            workItemsProducerThread = new Thread(WorkItemsProducer) {IsBackground = true};
            workItemsProducerThread.Start();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            running = false;
            workItemsProducerThread.Join();

            _stp.Shutdown(true, 1000);
            _stp.Dispose();
            _stp = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            UpdateControls(false);
        }

        private void UpdateControls(bool start)
        {
            running = start;
            _spinIdleTimeout.IsEnabled = !start;
            btnStart.IsEnabled = !start;

            btnStop.IsEnabled = start;
            if (start)
            {
                _timer.Start();
            }
            else
            {
                _timer.Stop();
            }

            _threadsUsed.Text = "0";
            _threadsInPool.Text = "0";
            _queuedWorkItems.Text = "0";
            _usageControl.Maximum = _spinMaxThreads.Maximum;
            _usageControl.Value1 = 0;
            _usageControl.Value2 = 0;
            _completedWorkItems.Text = "0";
            _generatedWorkItems.Text = "0";
            _historyUsageControl.Reset();
            _historyUsageControl.Maximum = _spinMaxThreads.Maximum;
        }


        private void WorkItemsProducer()
        {
            WorkItemCallback workItemCallback = DoWork;
            while (running)
            {
                IWorkItemsGroup workItemsGroup = _stp;
                if (null == workItemsGroup)
                {
                    return;
                }

                try
                {
                    workItemsGroup.QueueWorkItem(workItemCallback);
                }
                catch (ObjectDisposedException e)
                {
                    e.GetHashCode();
                    break;
                }
                workItemsGenerated++;
                Thread.Sleep(_interval);
            }
        }

        private object DoWork(object obj)
        {
            Thread.Sleep(_consumingTime);
            Interlocked.Increment(ref workItemsCompleted);
            return null;
        }

        private void _spinMinThreads_ValueChanged(int newValue)
        {
            if (newValue > _spinMaxThreads.Value)
            {
                _spinMaxThreads.Value = newValue;
                if (null != _stp)
                {
                    _stp.MaxThreads = newValue;
                }
            }

            if (null != _stp)
            {
                _stp.MinThreads = newValue;
            }

        }

        private void _spinMaxThreads_ValueChanged(int newValue)
        {
            if (newValue < _spinMinThreads.Value)
            {
                _spinMinThreads.Value = newValue;
                _usageControl.Maximum = newValue;
                _historyUsageControl.Maximum = newValue;
                if (null != _stp)
                {
                    _stp.MinThreads = newValue;
                }
            }
            if (null != _stp)
            {
                _stp.MaxThreads = newValue;
            }
        }

        private void _spinInterval_ValueChanged(int obj)
        {
            _interval = _spinInterval.Value;
        }

        private void _spinWorkItemTime_ValueChanged(int obj)
        {
            _consumingTime = _spinWorkItemTime.Value;
        }
    }
}
