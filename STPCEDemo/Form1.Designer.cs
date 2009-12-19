using System;
using System.Drawing;
using System.Windows.Forms;

namespace STPCEDemo
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu1;
        private UsageControl.UsageHistoryControl usageHistoryControl1;
        private Button btnStartStop;
        private Label label1;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lblThreadsInUse = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblThreadsInPool = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.spnMinThreads = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.spnMaxThreads = new System.Windows.Forms.NumericUpDown();
            this.spnIdleTimeout = new System.Windows.Forms.NumericUpDown();
            this.spnWorkItemsPerSecond = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.spnWorkItemDuration = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.usageControl1 = new UsageControl.UsageControl();
            this.usageHistoryControl1 = new UsageControl.UsageHistoryControl();
            this.SuspendLayout();
            // 
            // btnStartStop
            // 
            this.btnStartStop.Location = new System.Drawing.Point(148, 10);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(72, 20);
            this.btnStartStop.TabIndex = 0;
            this.btnStartStop.Text = "Start";
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(10, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(143, 20);
            this.label1.Text = "Smart Thread Pool";
            // 
            // lblThreadsInUse
            // 
            this.lblThreadsInUse.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular);
            this.lblThreadsInUse.Location = new System.Drawing.Point(148, 106);
            this.lblThreadsInUse.Name = "lblThreadsInUse";
            this.lblThreadsInUse.Size = new System.Drawing.Size(72, 24);
            this.lblThreadsInUse.Text = "0";
            this.lblThreadsInUse.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular);
            this.label7.Location = new System.Drawing.Point(10, 106);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(124, 24);
            this.label7.Text = "Used threads (Green)";
            // 
            // lblThreadsInPool
            // 
            this.lblThreadsInPool.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular);
            this.lblThreadsInPool.Location = new System.Drawing.Point(148, 88);
            this.lblThreadsInPool.Name = "lblThreadsInPool";
            this.lblThreadsInPool.Size = new System.Drawing.Size(72, 24);
            this.lblThreadsInPool.Text = "0";
            this.lblThreadsInPool.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular);
            this.label2.Location = new System.Drawing.Point(10, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(124, 24);
            this.label2.Text = "Threads in pool (Red)";
            // 
            // spnMinThreads
            // 
            this.spnMinThreads.Location = new System.Drawing.Point(148, 125);
            this.spnMinThreads.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.spnMinThreads.Name = "spnMinThreads";
            this.spnMinThreads.Size = new System.Drawing.Size(72, 24);
            this.spnMinThreads.TabIndex = 3;
            this.spnMinThreads.ValueChanged += new System.EventHandler(this.spnMinThreads_ValueChanged);
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular);
            this.label4.Location = new System.Drawing.Point(10, 173);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(120, 24);
            this.label4.Text = "Idle timeout (Seconds)";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular);
            this.label3.Location = new System.Drawing.Point(10, 149);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(104, 24);
            this.label3.Text = "Maximum Threads";
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular);
            this.label5.Location = new System.Drawing.Point(10, 125);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(104, 24);
            this.label5.Text = "Minimum Threads";
            // 
            // spnMaxThreads
            // 
            this.spnMaxThreads.Location = new System.Drawing.Point(148, 149);
            this.spnMaxThreads.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.spnMaxThreads.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.spnMaxThreads.Name = "spnMaxThreads";
            this.spnMaxThreads.Size = new System.Drawing.Size(72, 24);
            this.spnMaxThreads.TabIndex = 4;
            this.spnMaxThreads.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.spnMaxThreads.ValueChanged += new System.EventHandler(this.spnMaxThreads_ValueChanged);
            // 
            // spnIdleTimeout
            // 
            this.spnIdleTimeout.Location = new System.Drawing.Point(148, 173);
            this.spnIdleTimeout.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.spnIdleTimeout.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.spnIdleTimeout.Name = "spnIdleTimeout";
            this.spnIdleTimeout.Size = new System.Drawing.Size(72, 24);
            this.spnIdleTimeout.TabIndex = 5;
            this.spnIdleTimeout.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // spnWorkItemsPerSecond
            // 
            this.spnWorkItemsPerSecond.Location = new System.Drawing.Point(148, 203);
            this.spnWorkItemsPerSecond.Name = "spnWorkItemsPerSecond";
            this.spnWorkItemsPerSecond.Size = new System.Drawing.Size(72, 24);
            this.spnWorkItemsPerSecond.TabIndex = 6;
            this.spnWorkItemsPerSecond.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular);
            this.label6.Location = new System.Drawing.Point(10, 203);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(120, 24);
            this.label6.Text = "Work items/sec";
            // 
            // spnWorkItemDuration
            // 
            this.spnWorkItemDuration.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.spnWorkItemDuration.Location = new System.Drawing.Point(148, 227);
            this.spnWorkItemDuration.Maximum = new decimal(new int[] {
            6000,
            0,
            0,
            0});
            this.spnWorkItemDuration.Name = "spnWorkItemDuration";
            this.spnWorkItemDuration.Size = new System.Drawing.Size(72, 24);
            this.spnWorkItemDuration.TabIndex = 7;
            this.spnWorkItemDuration.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular);
            this.label8.Location = new System.Drawing.Point(10, 227);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(132, 24);
            this.label8.Text = "Work item duration (ms)";
            // 
            // usageControl1
            // 
            this.usageControl1.BackColor = System.Drawing.Color.Black;
            this.usageControl1.Location = new System.Drawing.Point(10, 33);
            this.usageControl1.Maximum = 10;
            this.usageControl1.Name = "usageControl1";
            this.usageControl1.Size = new System.Drawing.Size(41, 52);
            this.usageControl1.TabIndex = 1;
            this.usageControl1.Value1 = 0;
            this.usageControl1.Value2 = 0;
            // 
            // usageHistoryControl1
            // 
            this.usageHistoryControl1.BackColor = System.Drawing.Color.Black;
            this.usageHistoryControl1.Location = new System.Drawing.Point(66, 33);
            this.usageHistoryControl1.Maximum = 10;
            this.usageHistoryControl1.Name = "usageHistoryControl1";
            this.usageHistoryControl1.Size = new System.Drawing.Size(154, 52);
            this.usageHistoryControl1.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 258);
            this.Controls.Add(this.btnStartStop);
            this.Controls.Add(this.spnWorkItemDuration);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.spnWorkItemsPerSecond);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.spnIdleTimeout);
            this.Controls.Add(this.spnMaxThreads);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.spnMinThreads);
            this.Controls.Add(this.lblThreadsInUse);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.lblThreadsInPool);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.usageControl1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.usageHistoryControl1);
            this.Menu = this.mainMenu1;
            this.Name = "Form1";
            this.Text = "Smart Thread Pool";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private UsageControl.UsageControl usageControl1;
        private Label lblThreadsInUse;
        private Label label7;
        private Label lblThreadsInPool;
        private Label label2;
        private NumericUpDown spnMinThreads;
        private Label label4;
        private Label label3;
        private Label label5;
        private NumericUpDown spnMaxThreads;
        private NumericUpDown spnIdleTimeout;
        private NumericUpDown spnWorkItemsPerSecond;
        private Label label6;
        private NumericUpDown spnWorkItemDuration;
        private Label label8;


    }
}

