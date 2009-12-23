using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;


namespace UsageControl
{
    public partial class QueueUsageControl : UserControl
    {
        private List<QueueUsageEntry> _queuedItems;
        private int _maxItemsVisible = 0;
        private int _lastVisibleItemIndex = 0;
        private int _itemHeight = 0;
        private Dictionary<Color, Bitmap> _imagesCache = new Dictionary<Color, Bitmap>();
        private GraphicsPath _pathBorder;
        private RectangleF [] _slots;
        private bool _invalidate = true;

        public QueueUsageControl()
        {
            //Debug.WriteLine("QueueUsageControl");

            EnableDoubleBuffering();

            Reset();

            InitializeComponent();
            ControlRezised();
        }

        public static Bitmap GenerateItemImage(string text, Color color, int width, int height, Font font)
        {
            //Debug.WriteLine("GenerateItemImage");

            Rectangle rc = new Rectangle(0, 0, width, height);
            Bitmap itemImage = new Bitmap(rc.Width, rc.Height);

            /// Create button
            rc.Inflate(-3, -3);
            GraphicsPath path1 = GetPath(rc, 10);
            rc.Inflate(0, 6);
            LinearGradientBrush br1 = new LinearGradientBrush(rc, color, Color.White, LinearGradientMode.Vertical);
            rc.Inflate(0, -6);

            /// Create shadow
            Rectangle rc2 = rc;
            rc2.Offset(8, 8);
            GraphicsPath path2 = GetPath(rc2, 20);
            PathGradientBrush br2 = new PathGradientBrush(path2);
            br2.CenterColor = ControlPaint.DarkDark(Color.Silver);
            br2.SurroundColors = new Color[] { Color.White };

            /// Create bubble
            Rectangle rc3 = rc;
            rc3.Inflate(-15, -rc.Height / 3);
            rc3.Y = rc3.Y - 2;
            //rc3.Height = rc3.Height;
            GraphicsPath path3 = GetPath(rc3, rc3.Height);
            LinearGradientBrush br3 = new LinearGradientBrush(rc3, Color.FromArgb(255, Color.White), Color.FromArgb(0, Color.White), LinearGradientMode.Vertical);

            itemImage = new Bitmap(width - 2, height);
            Graphics g = Graphics.FromImage(itemImage);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillPath(br2, path2);
            g.FillPath(br1, path1);
            g.FillPath(br3, path3);

            //SizeF size = g.MeasureString(text, font);
            //int fontHeight = (int)size.Height + 5;

            //g.DrawString(
            //    text, 
            //    font, 
            //    Brushes.Black,
            //    new RectangleF((rc.Width - size.Width) / 2, 2, width, fontHeight));

            return itemImage;
        }

        private Bitmap GetItemsImage(Color color)
        {
            //Debug.WriteLine("GetItemsImage");

            Bitmap itemImage;

            if (!_imagesCache.ContainsKey(color))
            {
                Rectangle rc = new Rectangle(0, 0, Width, _itemHeight);
                itemImage = new Bitmap(rc.Width, rc.Height);

                /// Create button
                rc.Inflate(-3, -3);
                GraphicsPath path1 = GetPath(rc, 10);
                rc.Inflate(0, 6);
                LinearGradientBrush br1 = new LinearGradientBrush(rc, color, Color.White, LinearGradientMode.Vertical);
                rc.Inflate(0, -6);

                /// Create shadow
                Rectangle rc2 = rc;
                rc2.Offset(8, 8);
                GraphicsPath path2 = GetPath(rc2, 20);
                PathGradientBrush br2 = new PathGradientBrush(path2);
                br2.CenterColor = ControlPaint.DarkDark(Color.Silver);
                br2.SurroundColors = new Color[] { Color.White };

                /// Create bubble
                Rectangle rc3 = rc;
                rc3.Inflate(-15, -rc.Height / 3);
                rc3.Y = rc3.Y - 2;
                //rc3.Height = rc3.Height;
                GraphicsPath path3 = GetPath(rc3, rc3.Height);
                LinearGradientBrush br3 = new LinearGradientBrush(rc3, Color.FromArgb(255, Color.White), Color.FromArgb(0, Color.White), LinearGradientMode.Vertical);

                itemImage = new Bitmap(Width - 2, _itemHeight);
                Graphics g = Graphics.FromImage(itemImage);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillPath(br2, path2);
                g.FillPath(br1, path1);
                g.FillPath(br3, path3);

                _imagesCache[color] = itemImage;
            }

            itemImage = _imagesCache[color];

            return itemImage;
        }

        private void EnableDoubleBuffering()
        {
            //Debug.WriteLine("EnableDoubleBuffering");

            // Set the value of the double-buffering style bits to true.
            SetStyle(ControlStyles.DoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.SupportsTransparentBackColor,
                true);
            UpdateStyles();
        }

        public void Reset()
        {
            //Debug.WriteLine("Reset");
            _queuedItems = new List<QueueUsageEntry>();
        }

        //public void Queue(string text, Color color, bool blink)
        //{
        //    //Debug.WriteLine("Queue");
        //    _queuedItems.Add(new QueueUsageEntry(text, color, blink));
        //    //this.Invalidate();
        //}

        public void SetQueue(List<QueueUsageEntry> list)
        {
            //Debug.WriteLine("SetQueue");
            bool invalidate = false;

            if (_queuedItems.Count != list.Count)
            {
                invalidate = true;
            }
            else
            {
                int counter = Math.Min(list.Count, _maxItemsVisible);
                for (int i = 0; i < counter; i++)
                {
                    if (_queuedItems[i] != list[i])
                    {
                        invalidate = true;
                        break;
                    }
                }
            }

            if (invalidate)
            {
                //Debug.WriteLine("_invalidate = true");
                _queuedItems = list;
                _invalidate = invalidate;
            }

            //_queuedItems = list;
            //_queuedItems.Clear();
            //foreach (QueueUsageEntry entry in list)
            //{
            //    _queuedItems.Add(entry);
            //}
            //_invalidate = true;
            //this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //Debug.WriteLine("OnPaint");

            Bitmap bitmap = new Bitmap(Width, Height);
            Graphics g = Graphics.FromImage(bitmap);

            if (!Enabled)
            {
                g.FillPath(Brushes.LightGray, _pathBorder);
                g.DrawPath(Pens.Black, _pathBorder);
                e.Graphics.DrawImage(bitmap, 0, 0);
                return;
            }

            g.FillPath(Brushes.White, _pathBorder);


            int counter = Math.Min(_maxItemsVisible, _queuedItems.Count);

            int i = 0;
            foreach (QueueUsageEntry entry in _queuedItems)
            {
                if (i > counter)
                {
                    break;
                }

                //int top = 0;
                //int bottom = 0;
                //if (topdown)
                //{
                //    bottom = 1;
                //    top = bottom;
                //    top += i * _itemHeight;
                //    bottom += (i + 1) * _itemHeight;
                //}
                //else
                //{
                //    bottom = this.Height - 1;
                //    top = bottom;
                //    bottom -= i * _itemHeight;
                //    top -= (i + 1) * _itemHeight;
                //}

                //g.DrawLine(Pens.Black, 0, top, this.Width, top);


                string text = (entry.IsExecuting ? ">" : "") + entry.Text;
                if (i == _lastVisibleItemIndex)
                {
                    text = "(" + (_queuedItems.Count - _maxItemsVisible) + ")...";
                }

                SizeF size = g.MeasureString(entry.Text, Font);
                //Debug.WriteLine(size.Width);

                Bitmap itemImage = GetItemsImage(entry.Color);

                RectangleF slot = _slots[i];

                //g.DrawImage(itemImage, -1, top);
                g.DrawImage(itemImage, -1, slot.Top-2);
                
                //g.DrawString(text, this.Font, Brushes.Black, new RectangleF((this.Width - size.Width) / 2, top + 2, this.Width, bottom));
                g.DrawString(text, Font, Brushes.Black, slot);
                ++i;
            }

            g.DrawPath(Pens.Black, _pathBorder);
            //g.DrawRectangle(Pens.Black, border);

            e.Graphics.DrawImage(bitmap, 0, 0);
        }

        private static GraphicsPath GetPath(Rectangle rc, int r)
        {
            //Debug.WriteLine("GetPath");
            int x = rc.X, y = rc.Y, w = rc.Width, h = rc.Height;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(x, y, r, r, 180, 90);				//Upper left corner
            path.AddArc(x + w - r, y, r, r, 270, 90);			//Upper right corner
            path.AddArc(x + w - r, y + h - r, r, r, 0, 90);		//Lower right corner
            path.AddArc(x, y + h - r, r, r, 90, 90);			//Lower left corner
            path.CloseFigure();
            return path;
        }

        private void ControlRezised()
        {
            //Debug.WriteLine("ControlRezised");
            Graphics g = Graphics.FromHwnd(Handle);
            SizeF size = g.MeasureString("X", Font);
            g.Dispose();
            _itemHeight = (int)size.Height + 5;
            _maxItemsVisible = Height / _itemHeight - 1;
            _lastVisibleItemIndex = _maxItemsVisible;
            _imagesCache = new Dictionary<Color, Bitmap>();

            Rectangle border = new Rectangle(0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
            _pathBorder = GetPath(border, 20);

            PrepareSlots();

        }

        private void PrepareSlots()
        {
            bool topdown = true;
            _slots = new RectangleF[_maxItemsVisible+5];
            for (int i = 0; i < _slots.Length; i++)
            {
                int top;
                int bottom;
                if (topdown)
                {
                    bottom = 1;
                    top = bottom;
                    top += i * _itemHeight;
                    bottom += (i + 1) * _itemHeight;
                }
                else
                {
                    bottom = Height - 1;
                    top = bottom;
                    bottom -= i * _itemHeight;
                    top -= (i + 1) * _itemHeight;
                }

                _slots[i] = new RectangleF((Width - 55) / 2, top + 2, Width, bottom);
            }
        }

        private void QueueUsageControl_Resize(object sender, EventArgs e)
        {
            //Debug.WriteLine("QueueUsageControl_Resize");
            ControlRezised();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_invalidate)
            {
                _invalidate = false;
            }
            Invalidate();
        }
    }
}
