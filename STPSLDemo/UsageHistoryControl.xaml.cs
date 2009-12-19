using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace STPSLDemo
{
    public partial class UsageHistoryControl : UserControl
    {
        private const int _squareWidth = 12;
        private const int maxLastValuesCount = 2000;
        private const int _shiftStep = 3;

        private int _shift = 0;

        private int[] lastValues1 = new int[maxLastValuesCount];
        private int[] lastValues2 = new int[maxLastValuesCount];
        private int nextValueIndex;
        private int lastValuesCount;

        private int max = 100;
        private int min = 0;

        public UsageHistoryControl()
        {
            InitializeComponent();
        }

        public void Reset()
        {
            lastValues1.Initialize();
            lastValues2.Initialize();
            nextValueIndex = 0;
            lastValuesCount = 0;
            Redraw();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            Redraw();
        }

        private void Redraw()
        {
            _surface.Strokes.Clear();

            for (int i = 0; i <= Width + _squareWidth; i += _squareWidth)
            {
                int x = i - _shift;
                if (x>= 0 && x < Width)
                {
                    DrawLine(Colors.Green, x, 0, x, (int) Height);
                }
            }

            for (int i = 0; i < Height; i += _squareWidth)
            {
                DrawLine(Colors.Green, 0, i, (int)Width, i);
            }
            int startValueIndex = (nextValueIndex - 1 + maxLastValuesCount) % maxLastValuesCount;

            int prevVal1 = GetRelativeValue(lastValues1[startValueIndex]);
            int prevVal2 = GetRelativeValue(lastValues2[startValueIndex]);

            for (int i = 1; i < lastValuesCount; ++i)
            {
                int index = nextValueIndex - 1 - i;
                if (index < 0)
                {
                    index += maxLastValuesCount;
                }

                int val1 = GetRelativeValue(lastValues1[index]);
                int val2 = GetRelativeValue(lastValues2[index]);

                //Brush redBrush =
                //    new LinearGradientBrush(
                //        new Point(0, -(int)Height / 2),
                //        new Point(0, (int)Height),
                //        Color.Black,
                //        Color.Red);

                DrawLine(
                    Colors.Red,
                    (int)Width - (i - 1) * _shiftStep, (int)Height - prevVal2,
                    (int)Width - i * _shiftStep, (int)Height - val2);

                //g.FillPolygon(
                //    redBrush,
                //    new Point[] 
                //    {
                //        new Point((int)Width-(i-1)*_shiftStep, (int)Height-prevVal2), 
                //        new Point((int)Width-i*_shiftStep, (int)Height-val2), 
                //        new Point((int)Width-i*_shiftStep, (int)Height), 
                //        new Point((int)Width-(i-1)*_shiftStep, (int)Height), 
                //    });

                //Brush greenBrush =
                //    new LinearGradientBrush(
                //        new Point(0, -(int)Height / 2),
                //        new Point(0, (int)Height),
                //        Color.Black,
                //        Color.LawnGreen);

                DrawLine(
                    Color.FromArgb(255, 124, 252, 0), // Colors.LawnGreen
                    (int)Width - (i - 1) * _shiftStep, (int)Height - prevVal1,
                    (int)Width - i * _shiftStep, (int)Height - val1);

                //g.FillPolygon(
                //   greenBrush,
                //   new Point[] 
                //    {
                //        new Point((int)Width-(i-1)*_shiftStep, (int)Height-prevVal1), 
                //        new Point((int)Width-i*_shiftStep, (int)Height-val1), 
                //        new Point((int)Width-i*_shiftStep, (int)Height), 
                //        new Point((int)Width-(i-1)*_shiftStep, (int)Height), 
                //    });

                prevVal1 = val1;
                prevVal2 = val2;
            }

        }

        private int GetRelativeValue(int val)
        {
            int result = val * ((int)Height - 2) / max + 1;
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

            _shift += _shiftStep;
            _shift %= _squareWidth;
            Redraw();
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
                Redraw();
            }
        }
        
        private void DrawLine(Color color, int x1, int y1, int x2, int y2)
        {
            if (x1 < 0 || x2 < 0)
            {
                return;
            }
            DrawPolygon(color, new Point(x1, y1), new Point(x2, y2));
        }

        private void DrawPolygon(
            Color color,
            params Point[] points)
        {
            StylusPointCollection stylusPointCollection = new StylusPointCollection();
            foreach (Point point in points)
            {
                stylusPointCollection.Add(new StylusPoint(point.X, point.Y));
            }

            Stroke stroke = new Stroke(stylusPointCollection);

            stroke.DrawingAttributes.Color = color;
            stroke.DrawingAttributes.Width = 1;
            stroke.DrawingAttributes.Height = 1;

            _surface.Strokes.Add(stroke);
        }

    }
}
