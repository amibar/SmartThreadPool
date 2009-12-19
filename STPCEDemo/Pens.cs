using System.Drawing;

namespace STPCEDemo
{
    public static class Pens
    {
        private static Pen _red = new Pen(Color.Red);
        private static Pen _lawnGreen = new Pen(Color.LawnGreen);

        private static Pen _green = new Pen(Color.Green);
        private static Pen _darkGray = new Pen(Color.DarkGray);
        private static Pen _white = new Pen(Color.White);


        public static Pen Red
        {
            get { return _red; }
        }


        public static Pen LawnGreen
        {
            get { return _lawnGreen; }
        }

        public static Pen Green
        {
            get { return _green; }
        }

        public static Pen DarkGray
        {
            get { return _darkGray; }
        }

        public static Pen White
        {
            get { return _white; }
        }
    }
}
