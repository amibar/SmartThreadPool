using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;

namespace UsageControl
{
	/// <summary>
	/// Summary description for UsageControl.
	/// </summary>
	public class UsageControl : System.Windows.Forms.UserControl
	{
		private System.ComponentModel.IContainer components = null;
		
		private const int ovalWidth = 18;
		private const int columns = 2;

		private int fixedWidth;
		private int rows = 1; 

		private int min = 0;	// Minimum value for progress range
		private int max = 100;	// Maximum value for progress range
		private int val1 = 0;	// Current progress
		private int val2 = 0;	// Current progress

		public UsageControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			EnableDoubleBuffering();
			fixedWidth = 2 + (ovalWidth+1)*columns + 1;
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

		private void EnableDoubleBuffering()
		{
			// Set the value of the double-buffering style bits to true.
			this.SetStyle(ControlStyles.DoubleBuffer | 
				ControlStyles.UserPaint | 
				ControlStyles.AllPaintingInWmPaint,
				true);
			this.UpdateStyles();
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// UsageControl
			// 
			this.BackColor = System.Drawing.Color.Black;
			this.Name = "UsageControl";
			this.Size = new System.Drawing.Size(192, 160);

		}
		#endregion

		protected override void OnResize(EventArgs e)
		{
			Width = fixedWidth;
			rows = (ClientRectangle.Height / 4) - 1;
			// Invalidate the control to get a repaint.
			this.Invalidate();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;

			int percent1 = ((val1 - min) * 100) / (max - min);
			int percent2 = ((val2 - min) * 100) / (max - min);

			int filledCount1 = (rows * percent1) / 100;
			int filledCount2 = (rows * percent2) / 100;
			int diff = Math.Abs(filledCount1 - filledCount2);

			int emptyCount = rows - Math.Max(filledCount1, filledCount2);

			int i = 0;

			for(i = 0; i < filledCount1; ++i)
			{
				for(int j = 0; j < columns; ++j)
				{
					DrawOval(
						g, 
						Pens.LawnGreen,
						2 + j*(1+ovalWidth), 
						ClientRectangle.Bottom - 5 - i*4);
				}
			}

			for(; i < filledCount2; ++i)
			{
				for(int j = 0; j < columns; ++j)
				{
					DrawOval(
						g, 
						Pens.Red,
						2 + j*(1+ovalWidth), 
						ClientRectangle.Bottom - 5 - i*4);
				}
			}

			for(; i < rows; ++i)
			{
				for(int j = 0; j < columns; ++j)
				{
					DrawOval(
						g, 
						Pens.Green,
						2 + j*(1+ovalWidth), 
						ClientRectangle.Bottom - 5 - i*4);
				}
			}

			// Draw a three-dimensional border around the control.
			Draw3DBorder(g);

			// Clean up.
			//g.Dispose();
			base.OnPaint (e);
		}

		private void DrawOval(Graphics g, Pen pen, int x, int y)
		{
			g.DrawLine(pen, x+1 , y, x+ovalWidth-2, y);
			g.DrawLine(pen, x, y+1, x+ovalWidth-1, y+1);
			g.DrawLine(pen, x+1, y+2, x+ovalWidth-2, y+2);
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

				// Make sure that value is still in range.
				if (val1 > max)
				{
					val1 = max;
				}

				// Invalidate the control to get a repaint.
				this.Invalidate();
			}
		}

		public int Value1
		{
			get
			{
				return val1;
			}

			set
			{
				int oldValue = val1;

				// Make sure that the value does not stray outside the valid range.
				if (value < min)
				{
					val1 = min;
				}
				else if (value > max)
				{
					val1 = max;
				}
				else
				{
					val1 = value;
				}
				Invalidate();
/*
				// Invalidate only the changed area.
				float percent;

				Rectangle newValueRect = this.ClientRectangle;
				Rectangle oldValueRect = this.ClientRectangle;

				// Use a new value to calculate the rectangle for progress.
				percent = (float)(val1 - min) / (float)(max - min);
				newValueRect.Width = (int)((float)newValueRect.Width * percent);

				// Use an old value to calculate the rectangle for progress.
				percent = (float)(oldValue - min) / (float)(max - min);
				oldValueRect.Width = (int)((float)oldValueRect.Width * percent);

				Rectangle updateRect = new Rectangle();

				// Find only the part of the screen that must be updated.
				if (newValueRect.Width > oldValueRect.Width)
				{
					updateRect.X = oldValueRect.Size.Width;
					updateRect.Width = newValueRect.Width - oldValueRect.Width;
				}
				else
				{
					updateRect.X = newValueRect.Size.Width;
					updateRect.Width = oldValueRect.Width - newValueRect.Width;
				}

				updateRect.Height = this.Height;

				// Invalidate the intersection region only.
				this.Invalidate(updateRect);
*/
			}
		}

		public int Value2
		{
			get
			{
				return val2;
			}

			set
			{
				int oldValue = val2;

				// Make sure that the value does not stray outside the valid range.
				if (value < min)
				{
					val2 = min;
				}
				else if (value > max)
				{
					val2 = max;
				}
				else
				{
					val2 = value;
				}
				Invalidate();

				/*
								// Invalidate only the changed area.
								float percent;

								Rectangle newValueRect = this.ClientRectangle;
								Rectangle oldValueRect = this.ClientRectangle;

								// Use a new value to calculate the rectangle for progress.
								percent = (float)(val1 - min) / (float)(max - min);
								newValueRect.Width = (int)((float)newValueRect.Width * percent);

								// Use an old value to calculate the rectangle for progress.
								percent = (float)(oldValue - min) / (float)(max - min);
								oldValueRect.Width = (int)((float)oldValueRect.Width * percent);

								Rectangle updateRect = new Rectangle();

								// Find only the part of the screen that must be updated.
								if (newValueRect.Width > oldValueRect.Width)
								{
									updateRect.X = oldValueRect.Size.Width;
									updateRect.Width = newValueRect.Width - oldValueRect.Width;
								}
								else
								{
									updateRect.X = newValueRect.Size.Width;
									updateRect.Width = oldValueRect.Width - newValueRect.Width;
								}

								updateRect.Height = this.Height;

								// Invalidate the intersection region only.
								this.Invalidate(updateRect);
				*/
			}
		}

		private void Draw3DBorder(Graphics g)
		{
			int PenWidth = (int)Pens.White.Width;

			g.DrawLine(Pens.DarkGray,
				new Point(this.ClientRectangle.Left, this.ClientRectangle.Top),
				new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Top));
			g.DrawLine(Pens.DarkGray,
				new Point(this.ClientRectangle.Left, this.ClientRectangle.Top),
				new Point(this.ClientRectangle.Left, this.ClientRectangle.Height - PenWidth));
			g.DrawLine(Pens.White,
				new Point(this.ClientRectangle.Left, this.ClientRectangle.Height - PenWidth),
				new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Height - PenWidth));
			g.DrawLine(Pens.White,
				new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Top),
				new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Height - PenWidth));
		}


	}
}
