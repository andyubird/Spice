using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Spice
{
    class Line
    {
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
    }
}
