using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Amib.Threading;
using Microsoft.Phone.Controls;

namespace STPWPDemo
{
    public partial class MainPage : PhoneApplicationPage
    {
        private const int MaxThreads = 10;

        private readonly DispatcherTimer _timer;

        private bool _running = false;
        private SmartThreadPool _stp;
        private readonly Thread _producer;
        private int _workItemPerSec;
        private int _workItemDuration;
        private readonly AutoResetEvent _wakeupEvent = new AutoResetEvent(false);

        // Constructork
        public MainPage()
        {
            InitializeComponent();

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            _timer.Tick += _timer_Tick;
            _timer.Start();

            _producer = new Thread(WorkItemsProducer);
            _producer.IsBackground = true;
            _workItemDuration = Convert.ToInt32(txtWorkItemDuration.Text);
            _workItemPerSec = Convert.ToInt32(txtWorkItemPerSec.Text);

            usageControl1.Visibility = Visibility.Visible;
            usageControl1.Maximum = MaxThreads;
            usageHistoryControl1.Maximum = MaxThreads;

            usageControl1.Maximum = Convert.ToInt32(txtMaxThreads.Text);
            usageControl1.Value1 = 0;
            usageControl1.Value2 = 0;

            _producer.Start();
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
            
            txtUsedThreads.Text = threadsInUse.ToString();
            txtThreadsInPool.Text = threadsInPool.ToString();
            usageControl1.Value1 = threadsInUse;
            usageControl1.Value2 = threadsInPool;
            usageHistoryControl1.AddValues(threadsInUse, threadsInPool);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (_running)
            {
                _wakeupEvent.Set();
                _stp.Cancel();
                _stp.Shutdown(false, 5000);
                _stp = null;
                btnStart.Content = "Start";
                //txtIdleTimeout.Enabled = true;
                _running = false;
            }
            else
            {
                usageHistoryControl1.Reset();
                usageControl1.Value1 = 0;
                usageControl1.Value2 = 0;
                _wakeupEvent.Set();
                _running = true;
                btnStart.Content = "Stop";
                //txtIdleTimeout.Enabled = false;
                STPStartInfo stpStartInfo = new STPStartInfo()
                {
                    MinWorkerThreads = Convert.ToInt32(txtMinThreads.Text),
                    MaxWorkerThreads = Convert.ToInt32(txtMaxThreads.Text),
                    IdleTimeout = Convert.ToInt32(txtIdleTimeout.Text) * 1000,
                    EnableLocalPerformanceCounters = true,
                };

                _stp = new SmartThreadPool(stpStartInfo);
            }
        }

        private void WorkItemsProducer()
        {
            int timeout = _workItemPerSec == 0 ? Timeout.Infinite : (1000 / _workItemPerSec);
            while (true)
            {
                bool signal = _wakeupEvent.WaitOne(timeout);
                if (!signal)
                {
                    SmartThreadPool stp = _stp;
                    if (stp != null)
                    {
                        try
                        {
                            stp.QueueWorkItem(Sleep);
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    // UI changed _workItemPerSec
                    int workItemPerSec = _workItemPerSec;
                    timeout = workItemPerSec == 0 ? Timeout.Infinite : (1000 / workItemPerSec);
                }
            }
        }

        private void Sleep()
        {
            Thread.Sleep(_workItemDuration);
        }

        private void spinMinThreads_Decrement(object sender, EventArgs eventArgs)
        {
            int minThreads = Step(txtMinThreads, 0, MaxThreads, -1);
            if (_stp != null)
            {
                _stp.MinThreads = minThreads;
            }
        }

        private void spinMinThreads_Increment(object sender, EventArgs eventArgs)
        {
            int minThreads = Step(txtMinThreads, 0, Convert.ToInt32(txtMaxThreads.Text), +1);
            if (_stp != null)
            {
                _stp.MinThreads = minThreads;
            }
        }

        private void spinMaxThreads_Decrement(object sender, EventArgs e)
        {
            int maxThreads = Step(txtMaxThreads, Convert.ToInt32(txtMinThreads.Text), 10, -1);
            if (_stp != null)
            {
                _stp.MaxThreads = maxThreads;
            }
        }

        private void spinMaxThreads_Increment(object sender, EventArgs e)
        {
            int maxThreads = Step(txtMaxThreads, 0, MaxThreads, +1);
            usageControl1.Maximum = maxThreads;
            usageHistoryControl1.Maximum = maxThreads;
            if (_stp != null)
            {
                _stp.MaxThreads = maxThreads;
            }
        }

        private void spinIdleTimeout_Decrement(object sender, EventArgs e)
        {
            Step(txtIdleTimeout, 0, 10, -1);
        }

        private void spinIdleTimeout_Increment(object sender, EventArgs e)
        {
            Step(txtIdleTimeout, 0, 10, +1);
        }

        private void spinWorkItemPerSec_Decrement(object sender, EventArgs e)
        {
            int workItemPerSec = _workItemPerSec;
            _workItemPerSec = Step(txtWorkItemPerSec, 0, 10, -1);
            if (workItemPerSec != _workItemPerSec)
            {
                _wakeupEvent.Set();
            }
        }

        private void spinWorkItemPerSec_Increment(object sender, EventArgs e)
        {
            int workItemPerSec = _workItemPerSec;
            _workItemPerSec = Step(txtWorkItemPerSec, 0, 10, +1);
            if (workItemPerSec != _workItemPerSec)
            {
                _wakeupEvent.Set();
            }
        }

        private void spinWorkItemDuration_Decrement(object sender, EventArgs e)
        {
            _workItemDuration = Step(txtWorkItemDuration, 0, 1000, -100);
        }

        private void spinWorkItemDuration_Increment(object sender, EventArgs e)
        {
            _workItemDuration = Step(txtWorkItemDuration, 0, 1000, +100);
        }

        private static int Step(TextBlock txtField, int minValue, int maxValue, int change)
        {
            int value = Convert.ToInt32(txtField.Text);

            value += change;

            if (value < minValue)
            {
                value = minValue;
            }

            if (value > maxValue)
            {
                value = maxValue;
            }

            txtField.Text = value.ToString();

            return value;
        }

    }
}