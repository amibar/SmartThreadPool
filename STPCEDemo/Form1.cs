using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Amib.Threading;
using System.Threading;

namespace STPCEDemo
{
    public partial class Form1 : Form
    {
        private bool running;
        private readonly System.Windows.Forms.Timer uiTimer;
        private SmartThreadPool _stp;
        private readonly System.Windows.Forms.Timer wiTimer;

        public Form1()
        {
            running = false;
            InitializeComponent();

            uiTimer = new System.Windows.Forms.Timer();
            uiTimer.Interval = 1000;
            uiTimer.Tick += uiTimer_Tick;

            wiTimer = new System.Windows.Forms.Timer();
            wiTimer.Interval = 1000;
            wiTimer.Tick += wiTimer_Tick;
            //Debug.WriteLine("Form Thread Priority is " + (int)GetCurrentThreadPriority());
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetRunningState(false);
        }

        void uiTimer_Tick(object sender, EventArgs e)
        {
            if (null == _stp)
            {
                return;
            }
            try
            {
                int inUse = (int)_stp.PerformanceCountersReader.InUseThreads;
                int inPool = (int)_stp.PerformanceCountersReader.ActiveThreads;

                usageHistoryControl1.AddValues(inUse, inPool);
                usageControl1.Value1 = inUse;
                usageControl1.Value2 = inPool;
                lblThreadsInUse.Text = inUse.ToString();
                lblThreadsInPool.Text = inPool.ToString();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
                return;
            }
        }

        void wiTimer_Tick(object sender, EventArgs e)
        {
            int count = Convert.ToInt32(spnWorkItemsPerSecond.Value);
            int sleepDuration = Convert.ToInt32(spnWorkItemDuration.Value);
            //Debug.WriteLine(string.Format("{0}: C = {1}, S = {2}", DateTime.Now.ToString("HH:mm:ss"), count, sleepDuration));
            for (int i = 0; i < count; i++)
            {
                _stp.QueueWorkItem(DoWork, sleepDuration);
            }
        }

        private void DoWork(int sleepDuration)
        {
            //Debug.WriteLine("DoWork Thread Priority is " + (int)GetCurrentThreadPriority());

            DateTime start = DateTime.Now;
            //int sleepDuration = Convert.ToInt32(state);
            Thread.Sleep(sleepDuration);
            TimeSpan duration = DateTime.Now - start;

            //Debug.WriteLine(string.Format("{0}: Duration = {1}", DateTime.Now.ToString("HH:mm:ss"), duration.TotalMilliseconds));

            //return null;
        }

        void btnStartStop_Click(object sender, EventArgs e)
        {
            SetRunningState(!running);
        }

        private void SetRunningState(bool newRunningState)
        {
            btnStartStop.Text = newRunningState ? "Stop" : "Start";
            if (newRunningState)
            {
                STPStartInfo stpStartInfo = new STPStartInfo()
                {
                    IdleTimeout = Convert.ToInt32(spnIdleTimeout.Value) * 1000,
                    MaxWorkerThreads = Convert.ToInt32(spnMaxThreads.Value),
                    MinWorkerThreads = Convert.ToInt32(spnMinThreads.Value),
                    EnableLocalPerformanceCounters = true,
                };
                _stp = new SmartThreadPool(stpStartInfo);
            }
            else
            {
                if (null != _stp)
                {
                    _stp.Shutdown();
                }
                _stp = null;
                usageHistoryControl1.Reset();
                usageControl1.Value1 = 0;
                usageControl1.Value2 = 0;
            }
            spnIdleTimeout.Enabled = !newRunningState;
            uiTimer.Enabled = newRunningState;
            wiTimer.Enabled = newRunningState;
            running = newRunningState;
        }

        private void spnMinThreads_ValueChanged(object sender, EventArgs e)
        {
            if (null != _stp)
            {
                _stp.MinThreads = Convert.ToInt32(spnMinThreads.Value);
            }
        }

        private void spnMaxThreads_ValueChanged(object sender, EventArgs e)
        {
            if (null != _stp)
            {
                _stp.MaxThreads = Convert.ToInt32(spnMaxThreads.Value);
            }
        }

        private static int GetCurrentThreadPriority()
        {
            IntPtr hThread = GetCurrentThread();
            int priority = CeGetThreadPriority(hThread);
            return priority;
        }

        [DllImport("coredll.dll", SetLastError = true)]
        public static extern int CeGetThreadPriority(IntPtr hThread); 

        [DllImport("coredll.dll")]
        public static extern IntPtr GetCurrentThread();
    }
}