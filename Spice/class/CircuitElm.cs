using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Spice
{
    class CircuitElm
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

        private char type = 'w';

        public Pen myPen = new Pen(Color.DarkGray, 2);

        public Point pt1;
        public Point pt2;

        public CircuitElm(char tool, int x1, int y1, int x2, int y2)
        {
            type = tool;
            pt1.X = x1;
            pt1.Y = y1;
            pt2.X = x2;
            pt2.Y = y2;
        }

        public CircuitElm(char tool, Point a, Point b)
        {
            type = tool;
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

        public void draw(Graphics screen)
        {
            screen.DrawLine(myPen, pt1, pt2);
        }

        public bool checkBound(Point mouse)
        {
            Rectangle r = new Rectangle((pt1.X + pt2.X) / 2 - 5, (pt1.Y + pt2.Y) / 2 - 5, 10, 10);
            if (r.Contains(mouse))
            {
                myPen.Color = Color.Yellow;
                return true;
            }
            else
            {
                myPen.Color = Color.DarkGray;
                return false;
            }
        }

        public string getDump()
        {
            return type.ToString();
        }
    }
}
