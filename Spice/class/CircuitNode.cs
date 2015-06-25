using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spice
{
    class CircuitNode
    {
        public int x, y;
        public List<CircuitNodeLink> links;
        public bool isInternal;
        public CircuitNode() { links = new List<CircuitNodeLink>(); }
    }
}
