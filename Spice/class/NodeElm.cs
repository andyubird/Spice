using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Spice
{
    class NodeElm
    {
        public Point pt;

        public List<int> connectedCircuit = new List<int>();

        public NodeElm(Point x)
        {
            pt = x;
        }

        public void addtolist(int a)
        {
            connectedCircuit.Add(a);
        }
    }
}
