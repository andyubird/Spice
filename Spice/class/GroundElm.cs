using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Spice
{
    class GroundElm : CircuitElm
    {

        public GroundElm(int xx, int yy) : base(xx, yy) { }
	    int getDumpType() { return 'g'; }
	    int getPostCount() { return 1; }
	    void draw(Graphics screen) {
            Point[] gndterminal = new Point[6];
                screen.DrawLine(myPen, pt1, pt2);
                //the 1st line for ground
                gndterminal[0].Y = pt2.Y;
                gndterminal[0].X = pt2.X + 10;
                gndterminal[1].Y = pt2.Y;
                gndterminal[1].X = pt2.X - 10;
                screen.DrawLine(myPen, gndterminal[0], gndterminal[1]);
                //the 2nd line for ground
                gndterminal[2].Y = pt2.Y + 4;
                gndterminal[2].X = pt2.X + 6;
                gndterminal[3].Y = pt2.Y + 4;
                gndterminal[3].X = pt2.X - 6;
                screen.DrawLine(myPen, gndterminal[2], gndterminal[3]);
                //the 3rd line for ground
                gndterminal[4].Y = pt2.Y + 8;
                gndterminal[4].X = pt2.X + 2;
                gndterminal[5].Y = pt2.Y + 8;
                gndterminal[5].X = pt2.X - 2;
                screen.DrawLine(myPen, gndterminal[4], gndterminal[5]);
	    }
	    void setCurrent(int x, double c) { current = -c; }
	    void stamp() {
	        sim.stampVoltageSource(0, nodes[0], voltSource, 0);
	    }
	    double getVoltageDiff() { return 0; }
	    int getVoltageSourceCount() { return 1; }
	    void getInfo(String arr[]) {
	        arr[0] = "ground";
	        arr[1] = "I = " + getCurrentText(getCurrent());
	    }
	    boolean hasGroundConnection(int n1) { return true; }
	    int getShortcut() { return 'g'; }
    }
}
