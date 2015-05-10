using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Spice
{
    class Terminal
    {
        public Point pt;

        public List<int> connectedCircuits = new List<int>();

        public Terminal(Point x)
        {
            pt = x;
        }

        public void addtolist(int a)
        {
            connectedCircuits.Add(a);
        }
    }
}
