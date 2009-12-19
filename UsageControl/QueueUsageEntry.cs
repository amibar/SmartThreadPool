using System.Drawing;

namespace UsageControl
{
    public partial class QueueUsageControl
    {
        public class QueueUsageEntry
        {
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
                Text = text;
                Color = color;
                IsExecuting = blink;
            }

            public Color Color { get; private set; }

            public string Text { get; private set; }

            public bool IsExecuting { get; set; }
        }
    }
}
