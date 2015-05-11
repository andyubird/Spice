using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Spice
{
    class NodeElm
    {
        public List<int> connectedTerminals = new List<int>();

        public void addtolist(int a)
        {
            connectedTerminals.Add(a);
        }

        public string getDump()
        {
            string dump = "";
            foreach (int t in connectedTerminals)
            {
                dump += t.ToString() + " ";
            }

            return dump;
        }
    }
}
