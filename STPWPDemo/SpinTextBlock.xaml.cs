using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace STPWPDemo
{
    public partial class SpinTextBlock : UserControl
    {
        public event EventHandler<EventArgs> Increment;
        public event EventHandler<EventArgs> Decrement;

        public SpinTextBlock()
        {
            InitializeComponent();
        }

        void Dec_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Decrement != null)
            {
                Decrement(this, EventArgs.Empty);
            }
        }

        void Inc_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Increment != null)
            {
                Increment(this, EventArgs.Empty);
            }
        }

        public string Text
        {
            get { return TextBlock.Text; }
            set { TextBlock.Text = value; }
        }
    }
}
