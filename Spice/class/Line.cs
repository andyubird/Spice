using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Spice
{
    class Line
    {
        private int roundto(int a, int b)
        {
            int forward = b;
            int back = b;
            while (true)
            {
                if (back % a == 0) return back;
                if (forward % a == 0) return forward;
                back--;
                forward++;
            }
        }

        public Point pt1;
        public Point pt2;
        public Line(int x1, int y1, int x2, int y2)
        {
            pt1.X = x1;
            pt1.Y = y1;
            pt2.X = x2;
            pt2.Y = y2;
        }

        public Line(Point a, Point b)
        {
            pt1 = a;
            pt2 = b;
        }

        public void round(int a)
        {
            pt1.X = roundto(a, pt1.X);
            pt1.Y = roundto(a, pt1.Y);
            pt2.X = roundto(a, pt2.X);
            pt2.Y = roundto(a, pt2.Y);
        }
    }
}
