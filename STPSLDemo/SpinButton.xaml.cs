using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace STPSLDemo
{
    public partial class SpinButton : UserControl
    {
        public SpinButton()
        {
            InitializeComponent();

            Step = 1;
            Value = 5;
            Maximum = 10;
            Minimum = 0;
        }

        private void Increment_Click(object sender, RoutedEventArgs e)
        {
            int newValue = Math.Min(Value + Step, Maximum);
            if (Value != newValue)
            {
                Value = newValue;
            }
        }

        private void Decrement_Click(object sender, RoutedEventArgs e)
        {
            int newValue = Math.Max(Value - Step, Minimum);
            if (Value != newValue)
            {
                Value = newValue;
            }
        }

        private void UpdateLabel()
        {
            lblCurrentValue.Text = Value.ToString();
            ValueChanged(Value);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateLabel();
        }

        public event Action<int> ValueChanged = newValue => { };


        public static readonly DependencyProperty ValueProperty =
                   DependencyProperty.Register("Value", typeof(int), typeof(SpinButton), null);

        public static readonly DependencyProperty MinimumProperty =
                   DependencyProperty.Register("Minimum", typeof(int), typeof(SpinButton), null);

        public static readonly DependencyProperty MaximumProperty =
                   DependencyProperty.Register("Maximum", typeof(int), typeof(SpinButton), null);

        public static readonly DependencyProperty StepProperty =
                   DependencyProperty.Register("Step", typeof(int), typeof(SpinButton), null);

        public int Value
        {
            get { return (int)GetValue(ValueProperty); ; }
            set
            {
                SetValue(ValueProperty, value);
                UpdateLabel();
            }
        } 
        
        public int Step
        {
            get { return (int)GetValue(StepProperty); ; }
            set
            {
                SetValue(StepProperty, value);
            }
        }

        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set
            {
                SetValue(MinimumProperty, value);
            }
        } 
        
        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set
            {
                SetValue(MaximumProperty, value);
            }
        }
    }
}
