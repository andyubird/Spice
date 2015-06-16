using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Spice
{
    
    public class CircuitElm
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

        private void allocNodes()
        {
            nodes = new int[getPostCount() + getInternalNodeCount()];
            volts = new double[getPostCount() + getInternalNodeCount()];
        }

        public int[] nodes;
        public double[] volts;
        public double current, curcount;
        public int voltSource;
        public double voltdiff, compResistance;

        public char type = 'w';

        public float characteristic = 5;

        public int[] terminals = new int[2];

        public Pen myPen = new Pen(Color.DarkGray, 2);

       
        public Point pt1;
        public Point pt2;

        public CircuitElm() { }

        public int getVoltageSourceCount() { if (type == 'v' || type == 'g' || type == 'w') return 1; else return 0; }

        public int getInternalNodeCount() { return 0; }

        public Point getPost(int i) { if (i == 0) return pt1; else return pt2; }

        public void setNode(int p, int n) { nodes[p] = n; }

        public int getNode(int n) { return nodes[n]; }

        public bool hasGroundConnection(int n1) { if (type == 'g') return true; else return false; }

        public bool getConnection(int n1, int n2) { return true; }

        public bool isWire() { if (type == 'w') return true; else return false; }

        public double getCurrent() { return current; }

        public void reset()
        {
            int i;
            for (i = 0; i != getPostCount() + getInternalNodeCount(); i++)
                volts[i] = 0;
            curcount = 0;
        }

        public void setNodeVoltage(int n, double c)
        {
            volts[n] = c;
            calculateCurrent();
            if (type == 'C') voltdiff = volts[0] - volts[1];
        }

        public void stamp(Main sim)
        {
            if (type == 'v') sim.stampVoltageSource(nodes[0], nodes[1], voltSource, characteristic);
            if (type == 'w') sim.stampVoltageSource(nodes[0], nodes[1], voltSource, 0);
            if (type == 'g') sim.stampVoltageSource(0, nodes[0], voltSource, 0);
            if (type == 'r') sim.stampResistor(nodes[0], nodes[1], characteristic);
            if (type == 'C')
            {
                compResistance = sim.timeStep / (2 * characteristic);
                sim.stampResistor(nodes[0], nodes[1], compResistance);
                sim.stampRightSide(nodes[0]);
                sim.stampRightSide(nodes[1]);
            }
        }

        public int getPostCount() { if (type == 'g') return 1; else return 2; }

        public bool nonLinear() { return false; }

        public void setVoltageSource(int n, int v) { voltSource = v; }

        void calculateCurrent()
        {
            if (type == 'r') current = (volts[0] - volts[1]) / characteristic;
            if (type == 'C')
            {
                double voltdiff = volts[0] - volts[1];
                // we check compResistance because this might get called
                // before stamp(), which sets compResistance, causing
                // infinite current
                if (compResistance > 0)
                    current = voltdiff / compResistance + curSourceValue;
            }
        }

        public CircuitElm(char tool, int x1, int y1, int x2, int y2)
        {
            type = tool;
            pt1.X = x1;
            pt1.Y = y1;
            pt2.X = x2;
            pt2.Y = y2;
            allocNodes();
        }

        public CircuitElm(char tool, int x1, int y1, int x2, int y2, float chara)
        {
            type = tool;
            pt1.X = x1;
            pt1.Y = y1;
            pt2.X = x2;
            pt2.Y = y2;
            characteristic = chara;
            allocNodes();
        }

        public CircuitElm(char tool, Point a, Point b)
        {
            type = tool;
            pt1 = a;
            pt2 = b;
            allocNodes();
        }

        public void round(int a)
        {
            pt1.X = roundto(a, pt1.X);
            pt1.Y = roundto(a, pt1.Y);
            pt2.X = roundto(a, pt2.X);
            pt2.Y = roundto(a, pt2.Y);
        }
        static int voltrange = 5;
        
        public void draw(Graphics screen)
        {
            Pen p1pen;
            Pen pmidpen;
            Pen p2pen;

                if (bound == true)
                {
                    p1pen = new Pen(Color.Yellow, 2);
                    pmidpen = new Pen(Color.Yellow, 2);
                    p2pen = new Pen(Color.Yellow, 2);
                }
                else
                {
                    //p2pen = new Pen(Color.FromArgb(127 + (int)volts[0] * 128 / voltrange, 127 - (int)volts[0] * 127 / voltrange, 127 + (int)volts[0] * 128 / voltrange), 2);
                   // pmidpen = new Pen(Color.FromArgb(127 + (int)(volts[0] + volts[1]) * 128 / (2 * voltrange), 127 - (int)(volts[0] + volts[1]) * 127 / (2 * voltrange), 127 + (int)(volts[0] + volts[1]) * 128 / (2 * voltrange)), 2);
                    //p1pen = new Pen(Color.FromArgb(127 + (int)volts[1] * 128 / voltrange, 127 - (int)volts[1] * 127 / voltrange, 127 + (int)volts[1] * 128 / voltrange), 2);

                   
                    p2pen = new Pen(Color.FromArgb(127 - (int)volts[0] * 127 / voltrange, 127 +(int)volts[0] * 128 / voltrange, 127 - (int)volts[0] * 127 / voltrange), 2);
                    pmidpen = new Pen(Color.FromArgb(127 - (int)(volts[0] + volts[1]) * 127 / (2 * voltrange), 127 + (int)(volts[0] + volts[1]) * 128 / (2 * voltrange), 127 - (int)(volts[0] + volts[1]) * 127 / (2 * voltrange)), 2);
                    p1pen = new Pen(Color.FromArgb(127 - (int)volts[1] * 127 / voltrange, 127 + (int)volts[1] * 128 / voltrange, 127 - (int)volts[1] * 127 / voltrange), 2);
                }
            Point midpoint = new Point((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2);
            Point[] turnpt = new Point[17];
            int i;
            double angle = 0;
            double del_X, del_Y;
            if ((pt1.X != pt2.X) && (pt1.Y != pt2.Y))
            {
                del_X = pt2.X - pt1.X;
                del_Y = pt2.Y - pt1.Y;
                angle = Math.Atan2(del_Y, del_X);//unit: arc
            }
 
            //draw resistor
            if (type == 'r')
            {
                if ((pt1.X == pt2.X) && (pt1.Y == pt2.Y))
                    screen.DrawLine(pmidpen, pt1, pt2);
                if ((pt1.X != pt2.X) && (pt1.Y != pt2.Y))
                {
                    //設定第0個轉折點
                    turnpt[0].X = midpoint.X - Convert.ToInt32(16*Math.Cos(angle));
                    turnpt[0].Y = midpoint.Y - Convert.ToInt32(16*Math.Sin(angle));
                    turnpt[16].X = midpoint.X + Convert.ToInt32(16 * Math.Cos(angle));
                    turnpt[16].Y = midpoint.Y + Convert.ToInt32(16 * Math.Sin(angle));
                    /*
                    set turnning point*16(1~16)
                      /\  /\
                        \/  \/
                    turnpt[i].X => turnpt[0].X +,- 2*
                    turnpt[i].Y => turnpt[0].Y + i
                    */
                    for (i = 1; i < 16; i++)
                    {
                        switch (i % 4)
                        {
                            case 0:
                                turnpt[i].Y = turnpt[0].Y + Convert.ToInt32(2 * i * Math.Sin(angle));
                                turnpt[i].X = turnpt[0].X + Convert.ToInt32(2 * i * Math.Cos(angle));
                                break;
                            case 1:
                                turnpt[i].X = turnpt[i - 1].X + Convert.ToInt32(Math.Cos(angle) - 6 * Math.Sin(angle));
                                turnpt[i].Y = turnpt[i - 1].Y + Convert.ToInt32(Math.Sin(angle) + 6 * Math.Cos(angle));
                                break;
                            case 2:
                                turnpt[i].Y = turnpt[0].Y + Convert.ToInt32(2 * i * Math.Sin(angle));
                                turnpt[i].X = turnpt[0].X + Convert.ToInt32(2 * i * Math.Cos(angle));
                                break;
                            case 3:
                                turnpt[i].X = turnpt[i - 1].X + Convert.ToInt32(Math.Cos(angle) + 6 * Math.Sin(angle));
                                turnpt[i].Y = turnpt[i - 1].Y + Convert.ToInt32(Math.Sin(angle) - 6 * Math.Cos(angle));
                                break;
                            default:
                                break;
                        }//end switch

                    }//end for
                    screen.DrawLine(p1pen, pt1, turnpt[0]);
                    for (i = 0; i < 16; i++)
                        screen.DrawLine(pmidpen, turnpt[i], turnpt[i + 1]);
                    screen.DrawLine(p2pen, turnpt[16], pt2);
                }


                if ((pt1.X == pt2.X) && (pt1.Y != pt2.Y))
                {
                    //設定第0個轉折點
                    turnpt[0].X = midpoint.X;
                    turnpt[0].Y = midpoint.Y - 16;
                    /*
                    set turnning point*16(1~16)
                      /\  /\
                        \/  \/
                    turnpt[i].X => turnpt[0].X +,- 2*
                    turnpt[i].Y => turnpt[0].Y + i
                    */
                    for (i = 1; i < 17; i++)
                    {
                        //y 方向
                        turnpt[i].Y = turnpt[0].Y + 2 * i;
                        //x 方向
                        switch (i % 4)
                        {
                            case 0:
                                turnpt[i].X = turnpt[0].X;
                                break;
                            case 1:
                                turnpt[i].X = turnpt[0].X + 6;
                                break;
                            case 2:
                                turnpt[i].X = turnpt[0].X;
                                break;
                            case 3:
                                turnpt[i].X = turnpt[0].X - 6;
                                break;
                            default:
                                break;
                        }//end switch
                        
                    }//end for
                    
                    for (i = 0; i < 16; i++)
                        screen.DrawLine(pmidpen, turnpt[i], turnpt[i + 1]);
                    if (pt1.Y > pt2.Y)
                    {
                        screen.DrawLine(p1pen, pt1, turnpt[16]);
                        screen.DrawLine(p2pen, turnpt[0], pt2);
                    }
                    if (pt1.Y < pt2.Y)
                    {
                        screen.DrawLine(p1pen, pt1, turnpt[0]);
                        screen.DrawLine(p2pen, turnpt[16], pt2);
                    }


                }//end if
                if ((pt1.X != pt2.X) && (pt1.Y == pt2.Y))
                {
                    //設定第0個轉折點
                    turnpt[0].X = midpoint.X - 16;
                    turnpt[0].Y = midpoint.Y;
                    /*
                    set turnning point*16(1~16)
                      /\  /\
                        \/  \/
                    turnpt[i].X => turnpt[0].X +,- 2*
                    turnpt[i].Y => turnpt[0].Y + i
                    */
                    for (i = 1; i < 17; i++)
                    {
                        //x 方向
                        turnpt[i].X = turnpt[0].X + 2 * i;
                        //y 方向
                        switch (i % 4)
                        {
                            case 0:
                                turnpt[i].Y = turnpt[0].Y;
                                break;
                            case 1:
                                turnpt[i].Y = turnpt[0].Y + 6;
                                break;
                            case 2:
                                turnpt[i].Y = turnpt[0].Y;
                                break;
                            case 3:
                                turnpt[i].Y = turnpt[0].Y - 6;
                                break;
                            default:
                                break;
                        }//end switch

                    }//end for
 
                    for (i = 0; i < 16; i++)
                        screen.DrawLine(pmidpen, turnpt[i], turnpt[i + 1]);
                    if (pt1.X > pt2.X)
                    {
                        screen.DrawLine(p1pen, pt1, turnpt[16]);
                        screen.DrawLine(p2pen, turnpt[0], pt2);
                    }
                    if (pt1.X < pt2.X)
                    {
                        screen.DrawLine(p1pen, pt1, turnpt[0]);
                        screen.DrawLine(p2pen, turnpt[16], pt2);
                    }

                }//end if
            }
            //end resistor

            //draw wire
            if (type == 'w')
                screen.DrawLine(p1pen, pt1, pt2);


            //end wire

            //draw capacitor
            if (type == 'C')
            {                

                 Point[] capterminal = new Point[8];
                // draw two side of the cap

                //draw the middle of the cap


                //when the line is horizontal
                if ((pt1.Y == pt2.Y) && (pt1.X != pt2.X))
                {
                    capterminal[0].X = midpoint.X - 8;
                    capterminal[0].Y = midpoint.Y;
                    capterminal[1].X = midpoint.X + 8;
                    capterminal[1].Y = midpoint.Y;
                    if (pt1.X > pt2.X)
                    {
                        capterminal[6].X = pt1.X;
                        capterminal[6].Y = pt1.Y;
                        capterminal[7].X = pt2.X;
                        capterminal[7].Y = pt2.Y;
                        screen.DrawLine(p1pen, capterminal[1], capterminal[6]);
                        screen.DrawLine(p2pen, capterminal[7], capterminal[0]);
                    }
                    if (pt1.X < pt2.X)
                    {
                        capterminal[6].X = pt2.X;
                        capterminal[6].Y = pt2.Y;
                        capterminal[7].X = pt1.X;
                        capterminal[7].Y = pt1.Y;
                        screen.DrawLine(p2pen, capterminal[1], capterminal[6]);
                        screen.DrawLine(p1pen, capterminal[7], capterminal[0]);
                    }
                    capterminal[0].X = midpoint.X - 8;
                    capterminal[0].Y = midpoint.Y;
                    capterminal[1].X = midpoint.X + 8;
                    capterminal[1].Y = midpoint.Y;
                    screen.DrawLine(myPen, capterminal[1], capterminal[6]);
                    screen.DrawLine(myPen, capterminal[7], capterminal[0]);
                    //the negative pole(-)
                    capterminal[2].X = midpoint.X - 8;
                    capterminal[2].Y = midpoint.Y + 8;
                    capterminal[3].X = midpoint.X - 8;
                    capterminal[3].Y = midpoint.Y - 8;

                        
                    //the positive pole(+)(in the right side)
                    capterminal[4].X = midpoint.X + 8;
                    capterminal[4].Y = midpoint.Y + 8;
                    capterminal[5].X = midpoint.X + 8;
                    capterminal[5].Y = midpoint.Y - 8;
                    if (pt1.X > pt2.X)
                    {
                        screen.DrawLine(p2pen, capterminal[2], capterminal[3]);//(L)
                        screen.DrawLine(p1pen, capterminal[4], capterminal[5]);//(R)
                    }
                    if (pt1.X < pt2.X)
                    {
                        screen.DrawLine(p1pen, capterminal[2], capterminal[3]);//(L)
                        screen.DrawLine(p2pen, capterminal[4], capterminal[5]);//(R)
                    }
                        
                    
                }
                //when the line is vertical
                if ((pt1.X == pt2.X) && (pt1.Y != pt2.Y))
                {
                    capterminal[0].X = midpoint.X;
                    capterminal[0].Y = midpoint.Y + 8;
                    capterminal[1].X = midpoint.X;
                    capterminal[1].Y = midpoint.Y - 8;
                    if (pt1.Y > pt2.Y)
                    {
                        capterminal[6].X = pt1.X;
                        capterminal[6].Y = pt1.Y;
                        capterminal[7].X = pt2.X;
                        capterminal[7].Y = pt2.Y;
                        screen.DrawLine(p1pen, capterminal[1], capterminal[7]);
                        screen.DrawLine(p2pen, capterminal[6], capterminal[0]);
                    }
                    if (pt1.Y < pt2.Y)
                    {
                        capterminal[6].X = pt2.X;
                        capterminal[6].Y = pt2.Y;
                        capterminal[7].X = pt1.X;
                        capterminal[7].Y = pt1.Y;
                        screen.DrawLine(p2pen, capterminal[1], capterminal[7]);
                        screen.DrawLine(p1pen, capterminal[6], capterminal[0]);
                    }


                    //the negative pole(-)(in lower side)
                    capterminal[2].X = midpoint.X + 8;
                    capterminal[2].Y = midpoint.Y - 8;
                    capterminal[3].X = midpoint.X - 8;
                    capterminal[3].Y = midpoint.Y - 8;

                    //the positive pole(+)(in the upper side)
                    capterminal[4].X = midpoint.X + 8;
                    capterminal[4].Y = midpoint.Y + 8;
                    capterminal[5].X = midpoint.X - 8;
                    capterminal[5].Y = midpoint.Y + 8;
                    if (pt1.Y < pt2.Y)
                    {
                        screen.DrawLine(p1pen, capterminal[2], capterminal[3]);//(L)
                        screen.DrawLine(p2pen, capterminal[4], capterminal[5]);//(U)
                    }
                    if (pt1.Y > pt2.Y)
                    {
                        screen.DrawLine(p2pen, capterminal[2], capterminal[3]);
                        screen.DrawLine(p1pen, capterminal[4], capterminal[5]);
                    }
                }
               //when the line has a slope
                if ((pt1.X != pt2.X) && (pt1.Y != pt2.Y))
                {
                    capterminal[6].X = pt2.X;
                    capterminal[6].Y = pt2.Y;
                    capterminal[7].X = pt1.X;
                    capterminal[7].Y = pt1.Y;

                    capterminal[0].X = midpoint.X - Convert.ToInt32(8 * Math.Cos(angle));
                    capterminal[0].Y = midpoint.Y - Convert.ToInt32(8 * Math.Sin(angle));
                    capterminal[1].X = midpoint.X + Convert.ToInt32(8 * Math.Cos(angle));
                    capterminal[1].Y = midpoint.Y + Convert.ToInt32(8 * Math.Sin(angle));
                    screen.DrawLine(p2pen, capterminal[1], capterminal[6]);
                    screen.DrawLine(p1pen, capterminal[7], capterminal[0]);
                    //the negative pole(-)
                    capterminal[2].X = capterminal[0].X - Convert.ToInt32(8 * Math.Sin(angle));
                    capterminal[2].Y = capterminal[0].Y + Convert.ToInt32(8 * Math.Cos(angle));
                    capterminal[3].X = capterminal[0].X + Convert.ToInt32(8 * Math.Sin(angle));
                    capterminal[3].Y = capterminal[0].Y - Convert.ToInt32(8 * Math.Cos(angle));
                    screen.DrawLine(p1pen, capterminal[2], capterminal[3]);
                    //the positive pole(+)
                    capterminal[4].X = capterminal[1].X - Convert.ToInt32(8 * Math.Sin(angle));
                    capterminal[4].Y = capterminal[1].Y + Convert.ToInt32(8 * Math.Cos(angle));
                    capterminal[5].X = capterminal[1].X + Convert.ToInt32(8 * Math.Sin(angle));
                    capterminal[5].Y = capterminal[1].Y - Convert.ToInt32(8 * Math.Cos(angle));
                    screen.DrawLine(p2pen, capterminal[4], capterminal[5]);
                }
                if ((pt1.X == pt2.X) && (pt1.Y == pt2.Y))
                    screen.DrawLine(myPen, pt1, pt2);
            }
            //end capacitor
            
            //draw voltage
            if (type == 'v')
            {
                Point[] voltterminal = new Point[6];
                //when the line is horizontal
                if (pt1.Y == pt2.Y)
                {
                    if (pt1.X > pt2.X)
                    {
                        voltterminal[0].X = midpoint.X - 8;
                        voltterminal[0].Y = midpoint.Y;
                        voltterminal[1].X = midpoint.X + 8;
                        voltterminal[1].Y = midpoint.Y;
                        screen.DrawLine(p1pen, voltterminal[1], pt1);
                        screen.DrawLine(p2pen, pt2, voltterminal[0]);
                        //the negative pole(-)
                        voltterminal[2].X = midpoint.X - 8;
                        voltterminal[2].Y = midpoint.Y + 4;
                        voltterminal[3].X = midpoint.X - 8;
                        voltterminal[3].Y = midpoint.Y - 4;
                        screen.DrawLine(p2pen, voltterminal[2], voltterminal[3]);
                        //the positive pole(+)
                        voltterminal[4].X = midpoint.X + 8;
                        voltterminal[4].Y = midpoint.Y + 8;
                        voltterminal[5].X = midpoint.X + 8;
                        voltterminal[5].Y = midpoint.Y - 8;
                        screen.DrawLine(p1pen, voltterminal[4], voltterminal[5]);

                    }
                    if (pt1.X < pt2.X)
                    {
                        voltterminal[0].X = midpoint.X - 8;
                        voltterminal[0].Y = midpoint.Y;
                        voltterminal[1].X = midpoint.X + 8;
                        voltterminal[1].Y = midpoint.Y;
                        screen.DrawLine(p2pen, voltterminal[1], pt2);
                        screen.DrawLine(p1pen, pt1, voltterminal[0]);
                        //the negative pole(-)
                        voltterminal[2].X = midpoint.X + 8;
                        voltterminal[2].Y = midpoint.Y + 4;
                        voltterminal[3].X = midpoint.X + 8;
                        voltterminal[3].Y = midpoint.Y - 4;
                        screen.DrawLine(p2pen, voltterminal[2], voltterminal[3]);
                        //the positive pole(+)
                        voltterminal[4].X = midpoint.X - 8;
                        voltterminal[4].Y = midpoint.Y + 8;
                        voltterminal[5].X = midpoint.X - 8;
                        voltterminal[5].Y = midpoint.Y - 8;
                        screen.DrawLine(p1pen, voltterminal[4], voltterminal[5]);
                    }
                }
                //when the line is vertical
                if (pt1.X == pt2.X)
                {
                    if (pt1.Y < pt2.Y)
                    {
                        voltterminal[0].X = midpoint.X;
                        voltterminal[0].Y = midpoint.Y - 8;
                        voltterminal[1].X = midpoint.X;
                        voltterminal[1].Y = midpoint.Y + 8;
                        screen.DrawLine(p2pen, voltterminal[1], pt2);
                        screen.DrawLine(p1pen, pt1, voltterminal[0]);
                        //the negative pole(-)
                        voltterminal[2].X = midpoint.X + 4;
                        voltterminal[2].Y = midpoint.Y + 8;
                        voltterminal[3].X = midpoint.X - 4;
                        voltterminal[3].Y = midpoint.Y + 8;
                        screen.DrawLine(p2pen, voltterminal[2], voltterminal[3]);
                        //the positive pole(+)
                        voltterminal[4].X = midpoint.X + 8;
                        voltterminal[4].Y = midpoint.Y - 8;
                        voltterminal[5].X = midpoint.X - 8;
                        voltterminal[5].Y = midpoint.Y - 8;
                        screen.DrawLine(p1pen, voltterminal[4], voltterminal[5]);
                    }
                    if (pt1.Y > pt2.Y)
                    {
                        voltterminal[0].X = midpoint.X;
                        voltterminal[0].Y = midpoint.Y - 8;
                        voltterminal[1].X = midpoint.X;
                        voltterminal[1].Y = midpoint.Y + 8;
                        screen.DrawLine(p1pen, voltterminal[1], pt1);
                        screen.DrawLine(p2pen, pt2, voltterminal[0]);
                        //the negative pole(-)
                        voltterminal[2].X = midpoint.X + 4;
                        voltterminal[2].Y = midpoint.Y - 8;
                        voltterminal[3].X = midpoint.X - 4;
                        voltterminal[3].Y = midpoint.Y - 8;
                        screen.DrawLine(p2pen, voltterminal[2], voltterminal[3]);
                        //the positive pole(+)
                        voltterminal[4].X = midpoint.X + 8;
                        voltterminal[4].Y = midpoint.Y + 8;
                        voltterminal[5].X = midpoint.X - 8;
                        voltterminal[5].Y = midpoint.Y + 8;
                        screen.DrawLine(p1pen, voltterminal[4], voltterminal[5]);
                    }
                    
                }
                if ((pt1.X != pt2.X) && (pt1.Y != pt2.Y))
                {
   
                    voltterminal[0].X = midpoint.X - Convert.ToInt32(8 * Math.Cos(angle));
                    voltterminal[0].Y = midpoint.Y - Convert.ToInt32(8 * Math.Sin(angle));
                    voltterminal[1].X = midpoint.X + Convert.ToInt32(8 * Math.Cos(angle));
                    voltterminal[1].Y = midpoint.Y + Convert.ToInt32(8 * Math.Sin(angle));
                    screen.DrawLine(p2pen, voltterminal[1], pt2);
                    screen.DrawLine(p1pen, pt1, voltterminal[0]);
                    //the positive pole(+)
                    voltterminal[2].X = voltterminal[0].X - Convert.ToInt32(8 * Math.Sin(angle));
                    voltterminal[2].Y = voltterminal[0].Y + Convert.ToInt32(8 * Math.Cos(angle));
                    voltterminal[3].X = voltterminal[0].X + Convert.ToInt32(8 * Math.Sin(angle));
                    voltterminal[3].Y = voltterminal[0].Y - Convert.ToInt32(8 * Math.Cos(angle));
                    screen.DrawLine(p1pen, voltterminal[2], voltterminal[3]);
                    //the negative pole(-)
                    voltterminal[4].X = voltterminal[1].X - Convert.ToInt32(4 * Math.Sin(angle));
                    voltterminal[4].Y = voltterminal[1].Y + Convert.ToInt32(4 * Math.Cos(angle));
                    voltterminal[5].X = voltterminal[1].X + Convert.ToInt32(4 * Math.Sin(angle));
                    voltterminal[5].Y = voltterminal[1].Y - Convert.ToInt32(4 * Math.Cos(angle));
                    screen.DrawLine(p2pen, voltterminal[4], voltterminal[5]);
                }
                if ((pt1.X == pt2.X) && (pt1.Y == pt2.Y))
                    screen.DrawLine(p1pen, pt1, pt2);
            }
            //end voltage


            //draw ground 
            if (type == 'g')
            {
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
            //end ground

            if (type != 'w' && type != 'g') screen.DrawString(type.ToString() + characteristic.ToString(), new Font("Arial", 16), new SolidBrush(myPen.Color), midpoint + new Size(10, 0));
        }

 
        public void setCurrent(int x, double c)
        {
            if (type != 'g') current = c;
            else current = -c;
        }

        double curSourceValue;

        public void startIteration()
        {
            if (type == 'C') curSourceValue = -voltdiff / compResistance - current;
        }

        public void doStep(Main sim)
        {
            if (type == 'C')
            {
                sim.stampCurrentSource(nodes[0], nodes[1], curSourceValue);
            }
        }
        public bool bound;
        public bool checkBound(Point mouse)
        {
            
            Rectangle r = new Rectangle((pt1.X + pt2.X) / 2 - 5, (pt1.Y + pt2.Y) / 2 - 5, 10, 10);
            if (r.Contains(mouse))
            {
                myPen.Color = Color.Yellow;
                bound = true;
                return true;
            }
            else
            {
                myPen.Color = Color.DarkGray;
                bound = false;
                return false;
            }
        }

        public string getDump()
        {
            if (type != 'w' && type != 'g')
            {
                return type.ToString() + " " + terminals[0].ToString() + " " + terminals[1].ToString() + " " + characteristic.ToString();
            }
            return type.ToString() + " " + terminals[0].ToString() + " " + terminals[1].ToString();
        }

        public string saveDump()
        {
            if (type != 'w' && type != 'g')
            {
                return type.ToString() + " " + pt1.X.ToString() + " " + pt1.Y.ToString() + " " + pt2.X.ToString() + " " + pt2.Y.ToString() + " " + characteristic.ToString();
            }
            return type.ToString() + " " + pt1.X.ToString() + " " + pt1.Y.ToString() + " " + pt2.X.ToString() + " " + pt2.Y.ToString();
        }
    }
}
