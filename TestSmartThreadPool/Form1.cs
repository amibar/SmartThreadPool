using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

using Amib.Threading;

namespace TestSmartThreadPool
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public partial class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label lblThreadInUse;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.NumericUpDown spinIdleTimeout;
		private System.Windows.Forms.NumericUpDown spinMaxThreads;
		private System.Windows.Forms.NumericUpDown spinMinThreads;
		private System.Windows.Forms.NumericUpDown spinInterval;
		private System.Windows.Forms.Timer timerPoll;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Label lblThreadsInPool;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown spinConsumingTime;

		private SmartThreadPool _smartThreadPool;
		private IWorkItemsGroup _workItemsGroup;
		private System.Windows.Forms.Label lblWaitingCallbacks;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label lblWorkItemsGenerated;
		private bool running;
		private long workItemsGenerated;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label lblWorkItemsCompleted;
		private UsageControl.UsageControl usageThreadsInPool;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox4;
		private UsageControl.UsageHistoryControl usageHistorySTP;
		private long workItemsCompleted;

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

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

            InitializeGUIPerformanceCounters();
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
	        this._pcActiveThreads.InstanceName = "Test SmartThreadPool";
	        // 
	        // pcInUseThreads
	        // 
	        this._pcInUseThreads.CategoryName = "SmartThreadPool";
	        this._pcInUseThreads.CounterName = "In use threads";
	        this._pcInUseThreads.InstanceName = "Test SmartThreadPool";
	        // 
	        // pcQueuedWorkItems
	        // 
	        this._pcQueuedWorkItems.CategoryName = "SmartThreadPool";
	        this._pcQueuedWorkItems.CounterName = "Work Items in queue";
	        this._pcQueuedWorkItems.InstanceName = "Test SmartThreadPool";
	        // 
	        // pcCompletedWorkItems
	        // 
	        this._pcCompletedWorkItems.CategoryName = "SmartThreadPool";
	        this._pcCompletedWorkItems.CounterName = "Work Items processed";
	        this._pcCompletedWorkItems.InstanceName = "Test SmartThreadPool";

	        _getActiveThreads = () => (long) _pcActiveThreads.NextValue();
	        _getInUseThreads = () => (long) _pcInUseThreads.NextValue();
	        _getQueuedWorkItems = () => (long) _pcQueuedWorkItems.NextValue();
	        _getCompletedWorkItems = () => (long) _pcCompletedWorkItems.NextValue();
        }
#endif

        private void InitializeLocalPerformanceCounters()
	    {
	        _getActiveThreads = () => _smartThreadPool.PerformanceCountersReader.ActiveThreads;
	        _getInUseThreads = () => _smartThreadPool.PerformanceCountersReader.InUseThreads;
	        _getQueuedWorkItems = () => _smartThreadPool.PerformanceCountersReader.WorkItemsQueued;
	        _getCompletedWorkItems = () => _smartThreadPool.PerformanceCountersReader.WorkItemsProcessed;
	    }

	    /// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblThreadsInPool = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.spinIdleTimeout = new System.Windows.Forms.NumericUpDown();
            this.spinMaxThreads = new System.Windows.Forms.NumericUpDown();
            this.spinMinThreads = new System.Windows.Forms.NumericUpDown();
            this.spinInterval = new System.Windows.Forms.NumericUpDown();
            this.lblThreadInUse = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.timerPoll = new System.Windows.Forms.Timer(this.components);
            this.spinConsumingTime = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.lblWaitingCallbacks = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lblWorkItemsGenerated = new System.Windows.Forms.Label();
            this.lblWorkItemsCompleted = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.usageThreadsInPool = new UsageControl.UsageControl();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.usageHistorySTP = new UsageControl.UsageHistoryControl();
            ((System.ComponentModel.ISupportInitialize)(this.spinIdleTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinMaxThreads)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinMinThreads)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinConsumingTime)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStart.Location = new System.Drawing.Point(432, 352);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(72, 24);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStop.Location = new System.Drawing.Point(520, 352);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(72, 24);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "Stop";
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(104, 256);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 24);
            this.label1.TabIndex = 2;
            this.label1.Text = "Minimum Threads";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(104, 288);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(104, 24);
            this.label3.TabIndex = 4;
            this.label3.Text = "Maximum Threads";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(104, 320);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(120, 24);
            this.label4.TabIndex = 5;
            this.label4.Text = "Idle timeout (Seconds)";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(8, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 24);
            this.label2.TabIndex = 3;
            this.label2.Text = "In pool (Red)";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblThreadsInPool
            // 
            this.lblThreadsInPool.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblThreadsInPool.Location = new System.Drawing.Point(80, 16);
            this.lblThreadsInPool.Name = "lblThreadsInPool";
            this.lblThreadsInPool.Size = new System.Drawing.Size(80, 24);
            this.lblThreadsInPool.TabIndex = 11;
            this.lblThreadsInPool.Text = "XXXXXXXXX";
            this.lblThreadsInPool.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(336, 258);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(280, 24);
            this.label5.TabIndex = 12;
            this.label5.Text = "Interval between work item production (milliseconds)";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // spinIdleTimeout
            // 
            this.spinIdleTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.spinIdleTimeout.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spinIdleTimeout.Location = new System.Drawing.Point(8, 320);
            this.spinIdleTimeout.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.spinIdleTimeout.Name = "spinIdleTimeout";
            this.spinIdleTimeout.Size = new System.Drawing.Size(88, 29);
            this.spinIdleTimeout.TabIndex = 15;
            this.spinIdleTimeout.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.spinIdleTimeout.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // spinMaxThreads
            // 
            this.spinMaxThreads.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.spinMaxThreads.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spinMaxThreads.Location = new System.Drawing.Point(8, 288);
            this.spinMaxThreads.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.spinMaxThreads.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.spinMaxThreads.Name = "spinMaxThreads";
            this.spinMaxThreads.Size = new System.Drawing.Size(88, 29);
            this.spinMaxThreads.TabIndex = 14;
            this.spinMaxThreads.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.spinMaxThreads.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.spinMaxThreads.ValueChanged += new System.EventHandler(this.spinMaxThreads_ValueChanged);
            // 
            // spinMinThreads
            // 
            this.spinMinThreads.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.spinMinThreads.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spinMinThreads.Location = new System.Drawing.Point(8, 256);
            this.spinMinThreads.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.spinMinThreads.Name = "spinMinThreads";
            this.spinMinThreads.Size = new System.Drawing.Size(88, 29);
            this.spinMinThreads.TabIndex = 13;
            this.spinMinThreads.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.spinMinThreads.ValueChanged += new System.EventHandler(this.spinMinThreads_ValueChanged);
            // 
            // spinInterval
            // 
            this.spinInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.spinInterval.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spinInterval.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.spinInterval.Location = new System.Drawing.Point(240, 256);
            this.spinInterval.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.spinInterval.Name = "spinInterval";
            this.spinInterval.Size = new System.Drawing.Size(88, 29);
            this.spinInterval.TabIndex = 16;
            this.spinInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.spinInterval.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // lblThreadInUse
            // 
            this.lblThreadInUse.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblThreadInUse.Location = new System.Drawing.Point(80, 40);
            this.lblThreadInUse.Name = "lblThreadInUse";
            this.lblThreadInUse.Size = new System.Drawing.Size(80, 24);
            this.lblThreadInUse.TabIndex = 18;
            this.lblThreadInUse.Text = "XXXXXXXXX";
            this.lblThreadInUse.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(8, 40);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(80, 24);
            this.label7.TabIndex = 17;
            this.label7.Text = "Used (Green)";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // timerPoll
            // 
            this.timerPoll.Interval = 500;
            this.timerPoll.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // spinConsumingTime
            // 
            this.spinConsumingTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.spinConsumingTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spinConsumingTime.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.spinConsumingTime.Location = new System.Drawing.Point(240, 288);
            this.spinConsumingTime.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.spinConsumingTime.Name = "spinConsumingTime";
            this.spinConsumingTime.Size = new System.Drawing.Size(88, 29);
            this.spinConsumingTime.TabIndex = 20;
            this.spinConsumingTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.spinConsumingTime.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(336, 290);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(216, 24);
            this.label6.TabIndex = 19;
            this.label6.Text = "Work item consuming time (milliseconds)";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblWaitingCallbacks
            // 
            this.lblWaitingCallbacks.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWaitingCallbacks.Location = new System.Drawing.Point(64, 16);
            this.lblWaitingCallbacks.Name = "lblWaitingCallbacks";
            this.lblWaitingCallbacks.Size = new System.Drawing.Size(80, 24);
            this.lblWaitingCallbacks.TabIndex = 22;
            this.lblWaitingCallbacks.Text = "XXXXXXXXX";
            this.lblWaitingCallbacks.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label9
            // 
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(8, 16);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(48, 24);
            this.label9.TabIndex = 21;
            this.label9.Text = "Queued";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label8
            // 
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(8, 40);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(64, 24);
            this.label8.TabIndex = 25;
            this.label8.Text = "Generated";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label10
            // 
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(8, 64);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(64, 24);
            this.label10.TabIndex = 26;
            this.label10.Text = "Completed";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblWorkItemsGenerated
            // 
            this.lblWorkItemsGenerated.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWorkItemsGenerated.Location = new System.Drawing.Point(64, 40);
            this.lblWorkItemsGenerated.Name = "lblWorkItemsGenerated";
            this.lblWorkItemsGenerated.Size = new System.Drawing.Size(80, 24);
            this.lblWorkItemsGenerated.TabIndex = 27;
            this.lblWorkItemsGenerated.Text = "XXXXXXXXX";
            this.lblWorkItemsGenerated.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblWorkItemsCompleted
            // 
            this.lblWorkItemsCompleted.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWorkItemsCompleted.Location = new System.Drawing.Point(64, 64);
            this.lblWorkItemsCompleted.Name = "lblWorkItemsCompleted";
            this.lblWorkItemsCompleted.Size = new System.Drawing.Size(80, 24);
            this.lblWorkItemsCompleted.TabIndex = 28;
            this.lblWorkItemsCompleted.Text = "XXXXXXXXX";
            this.lblWorkItemsCompleted.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.lblWaitingCallbacks);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.lblWorkItemsGenerated);
            this.groupBox2.Controls.Add(this.lblWorkItemsCompleted);
            this.groupBox2.Location = new System.Drawing.Point(8, 144);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(152, 96);
            this.groupBox2.TabIndex = 33;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Work items";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox3.Controls.Add(this.lblThreadInUse);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.lblThreadsInPool);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Location = new System.Drawing.Point(176, 144);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(168, 72);
            this.groupBox3.TabIndex = 34;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Threads";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.usageThreadsInPool);
            this.groupBox1.Location = new System.Drawing.Point(8, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(80, 128);
            this.groupBox1.TabIndex = 35;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "STP Usage";
            // 
            // usageThreadsInPool
            // 
            this.usageThreadsInPool.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.usageThreadsInPool.BackColor = System.Drawing.Color.Black;
            this.usageThreadsInPool.Location = new System.Drawing.Point(20, 16);
            this.usageThreadsInPool.Maximum = 25;
            this.usageThreadsInPool.Name = "usageThreadsInPool";
            this.usageThreadsInPool.Size = new System.Drawing.Size(41, 104);
            this.usageThreadsInPool.TabIndex = 37;
            this.usageThreadsInPool.Value1 = 1;
            this.usageThreadsInPool.Value2 = 24;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.usageHistorySTP);
            this.groupBox4.Location = new System.Drawing.Point(104, 8);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(494, 128);
            this.groupBox4.TabIndex = 36;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "STP Usage History";
            // 
            // usageHistorySTP
            // 
            this.usageHistorySTP.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.usageHistorySTP.BackColor = System.Drawing.Color.Black;
            this.usageHistorySTP.Location = new System.Drawing.Point(8, 16);
            this.usageHistorySTP.Maximum = 25;
            this.usageHistorySTP.Name = "usageHistorySTP";
            this.usageHistorySTP.Size = new System.Drawing.Size(480, 104);
            this.usageHistorySTP.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(608, 382);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.spinConsumingTime);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.spinInterval);
            this.Controls.Add(this.spinIdleTimeout);
            this.Controls.Add(this.spinMaxThreads);
            this.Controls.Add(this.spinMinThreads);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(616, 416);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Test Smart Thread Pool";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Form1_Closing);
            ((System.ComponentModel.ISupportInitialize)(this.spinIdleTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinMaxThreads)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinMinThreads)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinConsumingTime)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
            //PerformanceCounterCategory.Delete("SmartThreadPool");
            //return;
#if _WINDOWS
            _useWindowsPerformanceCounters = InitializePerformanceCounters();
#endif			
            Application.EnableVisualStyles();

			Application.Run(new Form1());
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
				stpStartInfo.PerformanceCounterInstanceName = "Test SmartThreadPool";

				SmartThreadPool stp = new SmartThreadPool(stpStartInfo);
				stp.Shutdown();

				if (!PerformanceCounterCategory.Exists("SmartThreadPool"))
				{
					MessageBox.Show("Failed to create Performance Counters category.\r\nIf you run on Vista or Windows 7, you need to run for the first time as Administrator to create the performance counters category.\r\n\r\nUsing internal performance counters instead.", "Test Smart Thread Pool", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}

				Process process = new Process();
				process.StartInfo.FileName = Application.ExecutablePath;

				try
				{
					process.Start();
				}
				catch(Exception e)
				{
					e.GetHashCode();
					MessageBox.Show("If this is the first time you get this message,\r\nplease try to run the demo again.", "Test Smart Thread Pool");
				}

				return false;
			}

			return true;
		}

		private Thread workItemsProducerThread;

		private void btnStart_Click(object sender, System.EventArgs e)
		{
			UpdateControls(true);
			workItemsCompleted = 0;
			workItemsGenerated = 0;

			STPStartInfo stpStartInfo = new STPStartInfo();
			stpStartInfo.IdleTimeout = Convert.ToInt32(spinIdleTimeout.Value)*1000;
			stpStartInfo.MaxWorkerThreads = Convert.ToInt32(spinMaxThreads.Value);
			stpStartInfo.MinWorkerThreads = Convert.ToInt32(spinMinThreads.Value);
            if (_useWindowsPerformanceCounters)
            {
                stpStartInfo.PerformanceCounterInstanceName = "Test SmartThreadPool";
            }
            else
            {
                stpStartInfo.EnableLocalPerformanceCounters = true;
            }

			_smartThreadPool = new SmartThreadPool(stpStartInfo);

			//_workItemsGroup = _smartThreadPool.CreateWorkItemsGroup(1);
			_workItemsGroup = _smartThreadPool;

			workItemsProducerThread = new Thread(new ThreadStart(this.WorkItemsProducer));
			workItemsProducerThread.IsBackground = true;
			workItemsProducerThread.Start();
		}

		private void btnStop_Click(object sender, System.EventArgs e)
		{
			running = false;
			workItemsProducerThread.Join();

			_smartThreadPool.Shutdown(true, 1000);
			_smartThreadPool.Dispose();
			_smartThreadPool = null;
			GC.Collect();
			GC.WaitForPendingFinalizers();
			UpdateControls(false);
		}

		private void Form1_Load(object sender, System.EventArgs e)
		{
			UpdateControls(false);
		}

		private void UpdateControls(bool start)
		{
			running = start;
			//spinMinThreads.Enabled = !start;
			//spinMaxThreads.Enabled = !start;
			spinIdleTimeout.Enabled = !start;
			btnStart.Enabled = !start;

			btnStop.Enabled = start;
			timerPoll.Enabled = start;

			lblThreadInUse.Text = "0";
			lblThreadsInPool.Text = "0";
			lblWaitingCallbacks.Text = "0";
			//usageThreadsInPool.Maximum = Convert.ToInt32(spinMaxThreads.Value);
			usageThreadsInPool.Value1 = 0;
			usageThreadsInPool.Value2 = 0;
			lblWorkItemsCompleted.Text = "0";
			lblWorkItemsGenerated.Text = "0";
			usageHistorySTP.Reset();
			//usageHistorySTP.Maximum = usageThreadsInPool.Maximum;
		}

		private void spinMinThreads_ValueChanged(object sender, System.EventArgs e)
		{
			if (spinMinThreads.Value > spinMaxThreads.Value)
			{
				spinMaxThreads.Value = spinMinThreads.Value;
			}

            if (null != _smartThreadPool)
            {
                _smartThreadPool.MinThreads = (int)spinMinThreads.Value;
            }
		}

		private void spinMaxThreads_ValueChanged(object sender, System.EventArgs e)
		{
			if (spinMaxThreads.Value < spinMinThreads.Value)
			{
				spinMinThreads.Value = spinMaxThreads.Value;
			}
			//usageThreadsInPool.Maximum = Convert.ToInt32(spinMaxThreads.Value);
            if (null != _smartThreadPool)
            {
                _smartThreadPool.MaxThreads = (int)spinMaxThreads.Value;
            }
        }

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			SmartThreadPool stp = _smartThreadPool;
			if (null == stp)
			{
				return;
			}

            //int threadsInUse = (int)_pcInUseThreads.NextValue();
            //int threadsInPool = (int)_pcActiveThreads.NextValue();
            int threadsInUse = (int)_getInUseThreads();
            int threadsInPool = (int)_getActiveThreads();

			lblThreadInUse.Text = threadsInUse.ToString();
			lblThreadsInPool.Text = threadsInPool.ToString();
			lblWaitingCallbacks.Text = _getQueuedWorkItems().ToString();  //stp.WaitingCallbacks.ToString();
			usageThreadsInPool.Value1 = threadsInUse;
			usageThreadsInPool.Value2 = threadsInPool;
			lblWorkItemsCompleted.Text = _getCompletedWorkItems().ToString();
			lblWorkItemsGenerated.Text = workItemsGenerated.ToString();
			usageHistorySTP.AddValues(threadsInUse, threadsInPool);
		}

		private void WorkItemsProducer()
		{
			WorkItemCallback workItemCallback = new WorkItemCallback(this.DoWork);
			while(running)
			{
				IWorkItemsGroup workItemsGroup = _workItemsGroup;
				if (null == workItemsGroup)
				{
					return;
				}

				try
				{
					workItemCallback = new WorkItemCallback(this.DoWork);
					workItemsGroup.QueueWorkItem(workItemCallback);
				}
				catch(ObjectDisposedException e)
				{
                    e.GetHashCode();
					break;
				}
				workItemsGenerated++;
				Thread.Sleep(Convert.ToInt32(spinInterval.Value));
			}
		}

		private object DoWork(object obj)
		{
			Thread.Sleep(Convert.ToInt32(spinConsumingTime.Value));
			Interlocked.Increment(ref workItemsCompleted);
			return null;
		}

		private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (null != _smartThreadPool)
			{
				_smartThreadPool.Shutdown(true, 1000);
				_smartThreadPool = null;
				_workItemsGroup = null;
			}
		}
	}
}
