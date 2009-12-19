using System;
using System.Drawing;
using System.Windows.Forms;
using STPCEDemo;

namespace UsageControl
{
	/// <summary>
	/// Summary description for UsageHistoryControl.
	/// </summary>
	public class UsageHistoryControl : UserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private const int squareWidth = 12;
		private const int maxLastValuesCount = 2000;
		private const int shiftStep = 3;

		private int shift = 0;

		private int [] lastValues1 = new int[maxLastValuesCount];
		private int [] lastValues2 = new int[maxLastValuesCount];
		private int nextValueIndex;
		private int lastValuesCount;

		private int max = 100;
		private int min = 0;

		public UsageHistoryControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			EnableDoubleBuffering();
			Reset();
		}

		public void Reset()
		{
			lastValues1.Initialize();
			lastValues2.Initialize();
			nextValueIndex = 0;
			lastValuesCount = 0;
			Invalidate();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// UsageHistoryControl
			// 
			this.BackColor = System.Drawing.Color.Black;
			this.Name = "UsageHistoryControl";

		}
		#endregion

		private void EnableDoubleBuffering()
		{
			// Set the value of the double-buffering style bits to true.
            //this.SetStyle(ControlStyles.DoubleBuffer | 
            //    ControlStyles.UserPaint | 
            //    ControlStyles.AllPaintingInWmPaint,
            //    true);
            //this.UpdateStyles();
		}

		protected override void OnResize(EventArgs e)
		{
			// Invalidate the control to get a repaint.
			Invalidate();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;

			for(int i = 0; i <= ClientRectangle.Width+squareWidth; i += squareWidth)
			{
				g.DrawLine(Pens.Green, i-shift, 0, i-shift, ClientRectangle.Height);
			}

			for(int i = 0; i < ClientRectangle.Height; i += squareWidth)
			{
				g.DrawLine(Pens.Green, 0, i, ClientRectangle.Width, i);
			}

			int startValueIndex = (nextValueIndex-1+maxLastValuesCount)%maxLastValuesCount;

			int prevVal1 = GetRelativeValue(lastValues1[startValueIndex]);
			int prevVal2 = GetRelativeValue(lastValues2[startValueIndex]);

			for(int i = 1; i < lastValuesCount; ++i)
			{
				int index = nextValueIndex - 1 - i;
				if (index < 0)
				{
					index += maxLastValuesCount;
				}

				int val1 = GetRelativeValue(lastValues1[index]);
				int val2 = GetRelativeValue(lastValues2[index]);

				g.DrawLine(
					Pens.Red,
					ClientRectangle.Width-(i-1)*shiftStep, ClientRectangle.Height-prevVal2,
					ClientRectangle.Width-i*shiftStep, ClientRectangle.Height-val2);

				g.DrawLine(
					Pens.LawnGreen,
					ClientRectangle.Width-(i-1)*shiftStep, ClientRectangle.Height-prevVal1,
					ClientRectangle.Width-i*shiftStep, ClientRectangle.Height-val1);

				prevVal1 = val1;
				prevVal2 = val2;
			}
		}

		private int GetRelativeValue(int val)
		{
			int result = val * (ClientRectangle.Height-2) / max + 1;
			return result;
		}

		public void AddValues(int val1, int val2)
		{
			lastValues1[nextValueIndex] = val1;
			lastValues2[nextValueIndex] = val2;
			
			nextValueIndex++;
			nextValueIndex %= maxLastValuesCount;
			lastValuesCount++;
			if (lastValuesCount > maxLastValuesCount)
			{
				lastValuesCount = maxLastValuesCount;
			}
			
			shift += shiftStep;
			shift %= squareWidth;
			Invalidate();
		}

		public int Maximum
		{
			get
			{
				return max;
			}

			set
			{
				// Make sure that the maximum value is never set lower than the minimum value.
				if (value < min)
				{
					min = value;
				}

				max = value;

				// Invalidate the control to get a repaint.
				Invalidate();
			}
		}

	}
}
