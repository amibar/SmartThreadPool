using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace UsageControl
{
    public partial class QueueUsageControl
    {
        public class QueueUsageEntry
        {
            private string _text;
            private Color _color;
            private bool _isExecuting;

            public QueueUsageEntry(
                string text,
                Color color) : this (text, color, false)
            {
            }

            public QueueUsageEntry(
                string text, 
                Color color,
                bool blink)
            {
                _text = text;
                _color = color;
                _isExecuting = blink;
            }

            public Color Color
            {
                get { return _color; }
            }

            public string Text
            {
                get { return _text; }
            }

            public bool IsExecuting
            {
                get { return _isExecuting; }
                set { _isExecuting = value; }
            }
        }
    }
}
