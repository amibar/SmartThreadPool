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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace STPSLDemo
{
    public partial class UsageControl : UserControl
    {
        private int _rows = 25;
        private int _columns = 2;

        private int _max = 100;	// Maximum value for progress range
        private int _value1 = 30;	// Current progress
        private int _value2 = 60;	// Current progress


        public int Value1
        {
            get { return _value1; }
            set
            {
                _value1 = value;
                UpdateDisplay();
            }
        }

        public int Value2
        {
            get { return _value2; }
            set
            {
                _value2 = value;
                UpdateDisplay();
            }
        }

        public int Maximum
        {
            get { return _max; }
            set
            {
                _max = value;
                UpdateDisplay();
            }
        }


        public UsageControl()
        {
            InitializeComponent();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutRoot.Height = _rows * 4;
            LayoutRoot.Width = _columns * 18;

            LayoutRoot.RowDefinitions.Clear();

            for (int i = 0; i < _rows; i++)
            {
                LayoutRoot.RowDefinitions.Add(new RowDefinition());
            }

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            LayoutRoot.Children.Clear();

            for (int j = 0; j < _columns; j++)
            {
                for (int i = 0; i < _rows; i++)
                {
                    Brush brush = EmptyCell;

                    int percent = i * _max / _rows;

                    if (percent <= _value2)
                    {
                        brush = RedCell;
                    }

                    if (percent <= _value1)
                    {
                        brush = GreenCell;
                    }

                    Border border = new Border { Background = brush };

                    Grid.SetRow(border, _rows - i - 1);
                    Grid.SetColumn(border, j);
                    LayoutRoot.Children.Add(border);
                }
            }
        }
    }
}
