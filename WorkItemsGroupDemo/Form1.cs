using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using UsageControl;
using Amib.Threading;
using System.Threading;
using System.Collections;

namespace WorkItemsGroupDemo
{
    public partial class Form1 : Form
    {
        private bool _running = false;
        private bool _paused = false;
        private bool _advancedMode = false;
        private int _workItemsGenerated;
        Hashtable _workingStates = Hashtable.Synchronized(new Hashtable());
        private SmartThreadPool _smartThreadPool;
        private IWorkItemsGroup _wig1;
        private IWorkItemsGroup _wig2;
        private IWorkItemsGroup _wig3;

        private int[] _lastIndex = new int[4];

        private static readonly Color _stpColor = Color.Gray;
        private static readonly Color _wig1Color = Color.Red;
        private static readonly Color _wig2Color = Color.Green;
        private static readonly Color _wig3Color = Color.Blue;

#if _WINDOWS

        private System.Diagnostics.PerformanceCounter _pcActiveThreads;
        private System.Diagnostics.PerformanceCounter _pcInUseThreads;
        private System.Diagnostics.PerformanceCounter _pcQueuedWorkItems;
        private System.Diagnostics.PerformanceCounter _pcCompletedWorkItems;
#endif


        private Func<long> _getActiveThreads;
        private Func<long> _getInUseThreads;
        private Func<long> _getQueuedWorkItems;
        private Func<long> _getCompletedWorkItems;

        private static bool _useWindowsPerformanceCounters;

        private class WigEntry
        {
            public IWorkItemsGroup _wig;
            public QueueUsageControl _queueUsageControl;
            public Label _isIdle;

            public WigEntry(
                IWorkItemsGroup wig,
                QueueUsageControl queueUsageControl,
                Label isIdle)
            {
                _wig = wig;
                _queueUsageControl = queueUsageControl;
                _isIdle = isIdle;
            }
        }

        private WigEntry[] _wigEntries = null;

        public Form1()
        {
            InitializeComponent();

            InitSTP();
            InitializeGUIPerformanceCounters();

            UpdateControls(false);
            UpdateModeControls();
        }

        private void InitSTP()
        {
            STPStartInfo stpStartInfo =
                new STPStartInfo
                {
                    StartSuspended = true,
                    MaxWorkerThreads = ((int)spinCon6.Value),
                    IdleTimeout = int.Parse(spinIdleTimeout.Text) * 1000,
                };

            if (_useWindowsPerformanceCounters)
            {
                stpStartInfo.PerformanceCounterInstanceName = "WIG Test SmartThreadPool";
            }
            else
            {
                stpStartInfo.EnableLocalPerformanceCounters = true;
            }

            _smartThreadPool = new SmartThreadPool(stpStartInfo);
            WIGStartInfo wigStartInfo = new WIGStartInfo()
            {
                FillStateWithArgs = true,
            };
            _wig1 = _smartThreadPool.CreateWorkItemsGroup((int)spinCon1.Value, wigStartInfo);
            _wig2 = _smartThreadPool.CreateWorkItemsGroup((int)spinCon2.Value, wigStartInfo);
            _wig3 = _smartThreadPool.CreateWorkItemsGroup((int)spinCon3.Value, wigStartInfo);

            spinCon1.Tag = _wig1;
            spinCon2.Tag = _wig2;
            spinCon3.Tag = _wig3;
            spinCon6.Tag = _smartThreadPool;

            comboWIPriority1.SelectedIndex = 1;
            comboWIPriority2.SelectedIndex = 1;
            comboWIPriority3.SelectedIndex = 1;
            comboWIPriority6.SelectedIndex = 1;

            _wigEntries = new WigEntry[]
            {
                new WigEntry(_wig1, queueUsageControl1, lblStatus1),
                new WigEntry(_wig2, queueUsageControl2, lblStatus2),
                new WigEntry(_wig3, queueUsageControl3, lblStatus3),
            };
            for (int i = 0; i < _lastIndex.Length; i++)
            {
                _lastIndex[i] = 1;
            }
        }

        public static void Init()
        {
#if _WINDOWS
            _useWindowsPerformanceCounters = InitializePerformanceCounters();
#endif
        }

        // This method is a work around for the Peformance Counter issue.
        // When the first SmartThreadPool is created with a Peformance 
        // Counter name on a machine, it creates the SmartThreadPool 
        // Peformance Counter category. In this demo I am using the Performance 
        // Counters to update the GUI. 
        // The issue is that if this demo runs for the first time on the 
        // machine, it creates the Peformance Counter category and then 
        // uses it. 
        // I don't know why, but every time the demo runs for the first
        // time on a machine, it fails to connect to the Peformance Counters,
        // because it can't find the Peformance Counter category. 
        // The work around is to check if the category exists, and if not 
        // create a SmartThreadPool instance that will create the category.
        // After that I spawn another process that runs the demo.
        // I tried the another work around and thats to check for the category
        // existance, run a second process that will create the category,
        // and then continue with the first process, but it doesn't work.
        // Thank you for reading the whole comment. If you have another way
        // to solve this issue please contact me: amibar@gmail.com.
        private static bool InitializePerformanceCounters()
        {
            if (!PerformanceCounterCategory.Exists("SmartThreadPool"))
            {
                STPStartInfo stpStartInfo = new STPStartInfo();
                stpStartInfo.PerformanceCounterInstanceName = "WIG Test SmartThreadPool";

                SmartThreadPool stp = new SmartThreadPool(stpStartInfo);
                stp.Shutdown();

                if (!PerformanceCounterCategory.Exists("SmartThreadPool"))
                {
                    MessageBox.Show("Failed to create Performance Counters category.\r\nIf you run on Vista, Windows 7, or later, you need to run for the first time as Administrator to create the performance counters category.\r\n\r\nUsing internal performance counters instead.", "Test Smart Thread Pool", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                Process process = new Process();
                process.StartInfo.FileName = Application.ExecutablePath;

                try
                {
                    process.Start();
                }
                catch (Exception e)
                {
                    e.GetHashCode();
                    MessageBox.Show("If this is the first time you get this message,\r\nplease try to run the demo again.", "Test Smart Thread Pool");
                }

                return false;
            }

            return true;
        }


        private void InitializeGUIPerformanceCounters()
        {
            if (_useWindowsPerformanceCounters)
            {
                InitializeWindowsPerformanceCounters();
            }
            else
            {
                InitializeLocalPerformanceCounters();
            }
        }

        partial void InitializeWindowsPerformanceCounters();

#if _WINDOWS
        partial void InitializeWindowsPerformanceCounters()
        {
            this._pcActiveThreads = new System.Diagnostics.PerformanceCounter();
            this._pcInUseThreads = new System.Diagnostics.PerformanceCounter();
            this._pcQueuedWorkItems = new System.Diagnostics.PerformanceCounter();
            this._pcCompletedWorkItems = new System.Diagnostics.PerformanceCounter();

            // 
            // pcActiveThreads
            // 
            this._pcActiveThreads.CategoryName = "SmartThreadPool";
            this._pcActiveThreads.CounterName = "Active threads";
            this._pcActiveThreads.InstanceName = "WIG Test SmartThreadPool";
            // 
            // pcInUseThreads
            // 
            this._pcInUseThreads.CategoryName = "SmartThreadPool";
            this._pcInUseThreads.CounterName = "In use threads";
            this._pcInUseThreads.InstanceName = "WIG Test SmartThreadPool";
            // 
            // pcQueuedWorkItems
            // 
            this._pcQueuedWorkItems.CategoryName = "SmartThreadPool";
            this._pcQueuedWorkItems.CounterName = "Work Items in queue";
            this._pcQueuedWorkItems.InstanceName = "WIG Test SmartThreadPool";
            // 
            // pcCompletedWorkItems
            // 
            this._pcCompletedWorkItems.CategoryName = "SmartThreadPool";
            this._pcCompletedWorkItems.CounterName = "Work Items processed";
            this._pcCompletedWorkItems.InstanceName = "WIG Test SmartThreadPool";

            _getActiveThreads = () => (long)_pcActiveThreads.NextValue();
            _getInUseThreads = () => (long)_pcInUseThreads.NextValue();
            _getQueuedWorkItems = () => (long)_pcQueuedWorkItems.NextValue();
            _getCompletedWorkItems = () => (long)_pcCompletedWorkItems.NextValue();
        }
#endif

        private void InitializeLocalPerformanceCounters()
        {
            _getActiveThreads = () => _smartThreadPool.PerformanceCountersReader.ActiveThreads;
            _getInUseThreads = () => _smartThreadPool.PerformanceCountersReader.InUseThreads;
            _getQueuedWorkItems = () => _smartThreadPool.PerformanceCountersReader.WorkItemsQueued;
            _getCompletedWorkItems = () => _smartThreadPool.PerformanceCountersReader.WorkItemsProcessed;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            _running = !_running;
            btnStart.Text = _running ? "Stop STP" : "Start STP";

            if (_running)
            {
                Start();
            }
            else
            {
                Shutdown();
            }
        }

        private void Start()
        {
            _workItemsGenerated = 0;
            UpdateControls(true);
            _smartThreadPool.Start();
            _wig1.Start();
            _wig2.Start();
            _wig3.Start();
        }

        private void Shutdown()
        {
            _smartThreadPool.Shutdown(false, 2000);
            InitSTP();
            UpdateControls(false);
        }

        private void UpdateControls(bool start)
        {
            timerPoll.Enabled = start;

            lblThreadInUse.Text = "0";
            lblThreadsInPool.Text = "0";
            lblWaitingCallbacks.Text = "0";
            usageThreadsInPool.Value1 = 0;
            usageThreadsInPool.Value2 = 0;
            lblWorkItemsCompleted.Text = "0";
            lblWorkItemsGenerated.Text = "0";
            usageHistorySTP.Reset();
            usageHistorySTP.Maximum = usageThreadsInPool.Maximum;

            spinIdleTimeout.Enabled = !start;
        }

        private void DoNothing(WorkItemState state)
        {
            WorkItemState workItemState = (WorkItemState)state;
            _workingStates.Add(workItemState.QueueUsageEntry, workItemState.QueueUsageEntry);
            do
            {
                if (SmartThreadPool.IsWorkItemCanceled)
                {
                    break;
                }
                Stopwatch stopwatch = Stopwatch.StartNew();
                //while (stopwatch.ElapsedMilliseconds < workItemState.SleepDuration);
                Thread.Sleep(workItemState.SleepDuration);
            } while (_paused);
            _workingStates.Remove(workItemState.QueueUsageEntry);
            
            //return null;
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach (WigEntry wigEntry in _wigEntries)
	        {
                UpdateQueueUsageControl(
                    wigEntry._wig,
                    wigEntry._queueUsageControl,
                    wigEntry._isIdle);
		 
	        }
            UpdateWorkingSet();
        }

        private void UpdateWorkingSet()
        {
            lblStatus6.Text = _smartThreadPool.IsIdle ? "Idle" : "Working";

            object [] statesWorking = null;
            lock (_workingStates.SyncRoot)
            {
                statesWorking = new object[_workingStates.Count];
                _workingStates.Keys.CopyTo(statesWorking, 0);
            }

            object[] statesSTP = _smartThreadPool.GetStates();
            List<QueueUsageControl.QueueUsageEntry> list = new List<QueueUsageControl.QueueUsageEntry>();

            foreach (QueueUsageControl.QueueUsageEntry entry in statesWorking)
            {
                if (null != entry)
                {
                    entry.IsExecuting = true;
                    list.Add(entry);
                }
            }

            foreach (object[] state in statesSTP)
            {
                if (null != state)
                {
                    list.Add(((WorkItemState)state[0]).QueueUsageEntry);
                }
            }

            queueUsageControl6.SetQueue(list);
        }

        private void UpdateQueueUsageControl(
            IWorkItemsGroup wig, 
            QueueUsageControl queueUsageControl,
            Label label)
        {
            label.Text = wig.IsIdle ? "Idle" : "Working";
            object[] states = wig.GetStates();

            List<QueueUsageControl.QueueUsageEntry> list = new List<QueueUsageControl.QueueUsageEntry>();

            foreach (object [] state in states)
            {
                if (null != state)
                {
                    list.Add(((WorkItemState)state[0]).QueueUsageEntry);
                }
            }

            queueUsageControl.SetQueue(list);
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            _paused = !_paused;
            btnPause.Text = _paused ? "Resume STP" : "Pause STP";
        }

        #region Test functions
        private void btnState1_Click(object sender, EventArgs e)
        {
            List<QueueUsageControl.QueueUsageEntry> list = new List<QueueUsageControl.QueueUsageEntry>();
            list.Add(new QueueUsageControl.QueueUsageEntry("#A", Color.Yellow));
            list.Add(new QueueUsageControl.QueueUsageEntry("#B", Color.Green));
            list.Add(new QueueUsageControl.QueueUsageEntry("#C", Color.Black));
            list.Add(new QueueUsageControl.QueueUsageEntry("#D", Color.HotPink));
            queueUsageControl1.SetQueue(list);

        }

        private void btnState2_Click(object sender, EventArgs e)
        {
            List<QueueUsageControl.QueueUsageEntry> list = new List<QueueUsageControl.QueueUsageEntry>();
            list.Add(new QueueUsageControl.QueueUsageEntry("#1", Color.Cyan));
            list.Add(new QueueUsageControl.QueueUsageEntry("#2", Color.Magenta));
            list.Add(new QueueUsageControl.QueueUsageEntry("#3", Color.Red));
            queueUsageControl1.SetQueue(list);
        }
        #endregion

        #region WorkItemState class

        private class WorkItemState
        {
            public readonly QueueUsageControl.QueueUsageEntry QueueUsageEntry;
            public readonly int SleepDuration;

            public WorkItemState(
                QueueUsageControl.QueueUsageEntry queueUsageEntry,
                int sleepDuration)
            {
                QueueUsageEntry = queueUsageEntry;
                SleepDuration = sleepDuration;
            }
        }

        #endregion

        private void EnqueueWorkItems(ref int startIndex, int count, string text, Color color, WorkItemPriority priority, IWorkItemsGroup wig, int sleepDuration)
        {
            for (int i = 0; i < count; ++i, ++startIndex)
            {
                wig.QueueWorkItem(
                    DoNothing,
                    new WorkItemState(new QueueUsageControl.QueueUsageEntry(string.Format("{0}{1} ({2})", text, startIndex, priority.ToString().Substring(0,2 )), color), sleepDuration),
                    priority);
            }
            _workItemsGenerated += count;
        }

        private void btnCancel1_Click(object sender, EventArgs e)
        {
            _wig1.Cancel();
        }

        private void btnCancel2_Click(object sender, EventArgs e)
        {
            _wig2.Cancel();
        }

        private void btnCancel3_Click(object sender, EventArgs e)
        {
            _wig3.Cancel();
        }

        private void btnCancel6_Click(object sender, EventArgs e)
        {
            _smartThreadPool.Cancel();
        }

        private void spinCon_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown spin = sender as NumericUpDown;
            IWorkItemsGroup wig = spin.Tag as IWorkItemsGroup;
            wig.Concurrency = (int)spin.Value;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            EnqueueWorkItems(ref _lastIndex[0], Convert.ToInt32(spinProduction1.Value), "#", _wig1Color, (WorkItemPriority)(2 * comboWIPriority1.SelectedIndex), _wig1, Convert.ToInt32(spinDuration1.Value));
            EnqueueWorkItems(ref _lastIndex[1], Convert.ToInt32(spinProduction2.Value), "#", _wig2Color, (WorkItemPriority)(2 * comboWIPriority2.SelectedIndex), _wig2, Convert.ToInt32(spinDuration2.Value));
            EnqueueWorkItems(ref _lastIndex[2], Convert.ToInt32(spinProduction3.Value), "#", _wig3Color, (WorkItemPriority)(2 * comboWIPriority3.SelectedIndex), _wig3, Convert.ToInt32(spinDuration3.Value));
            EnqueueWorkItems(ref _lastIndex[3], Convert.ToInt32(spinProduction6.Value), "#", _stpColor, (WorkItemPriority)(2 * comboWIPriority6.SelectedIndex), _smartThreadPool, Convert.ToInt32(spinDuration6.Value));
        }

        private void btnMode_Click(object sender, EventArgs e)
        {
            _advancedMode = !_advancedMode;
            UpdateModeControls();
        }

        private void UpdateModeControls()
        {
            btnMode.Text = _advancedMode ? "Basic <<" : "Advanced >>";
            panelWIGsCtrls.Enabled = _advancedMode;
            groupWIGQueues.Enabled = _advancedMode;
        }

        private void timerPoll_Tick(object sender, EventArgs e)
        {
            SmartThreadPool stp = _smartThreadPool;
            if (null == stp)
            {
                return;
            }

            int threadsInUse = (int)_getInUseThreads();
            int threadsInPool = (int)_getActiveThreads();

            lblThreadInUse.Text = threadsInUse.ToString();
            lblThreadsInPool.Text = threadsInPool.ToString();
            lblWaitingCallbacks.Text = _getQueuedWorkItems().ToString();  //stp.WaitingCallbacks.ToString();
            usageThreadsInPool.Value1 = threadsInUse;
            usageThreadsInPool.Value2 = threadsInPool;
            lblWorkItemsCompleted.Text = _getCompletedWorkItems().ToString();
            lblWorkItemsGenerated.Text = _workItemsGenerated.ToString();
            usageHistorySTP.AddValues(threadsInUse, threadsInPool);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label5.Image = QueueUsageControl.GenerateItemImage("", _wig1Color, 72, label5.Height, label5.Font); ;
            label4.Image = QueueUsageControl.GenerateItemImage("", _wig2Color, 72, label5.Height, label5.Font); ;
            label3.Image = QueueUsageControl.GenerateItemImage("", _wig3Color, 72, label5.Height, label5.Font); ;
            label8.Image = QueueUsageControl.GenerateItemImage("", _stpColor, 72, label5.Height, label5.Font); ;
        
        
        }
    }
}
