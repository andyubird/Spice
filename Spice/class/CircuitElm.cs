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

        public char type = 'w';

        public float characteristic = 5;

        public int[] terminals = new int[2];

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
           
            
            Point midpoint = new Point((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2);
            Point [] turnpt = new Point [17];
            int i;
            if (type == 'r')
            {
                if ((pt1.X != pt2.X) && (pt1.Y != pt2.Y))
                    screen.DrawLine(myPen, pt1, pt2);

                if (pt1.X == pt2.X)
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
                    screen.DrawLine(myPen, pt1, turnpt[0]);
                    for (i = 0; i < 16; i++)
                        screen.DrawLine(myPen, turnpt[i], turnpt[i + 1]);
                    screen.DrawLine(myPen, turnpt[16], pt2);
                }//end if
                if (pt1.Y == pt2.Y)
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
                    screen.DrawLine(myPen, pt1, turnpt[0]);
                    for (i = 0; i < 16; i++)
                        screen.DrawLine(myPen, turnpt[i], turnpt[i + 1]);
                    screen.DrawLine(myPen, turnpt[16], pt2);
                }//end if
            }
            if(type == 'w')
                screen.DrawLine(myPen, pt1, pt2);
            if (type == 'v')
            {
                Point[] voltterminal = new Point[6];
                //when the line is vertical
                if (pt1.Y == pt2.Y)
                {
                    voltterminal[0].X = midpoint.X - 8;
                    voltterminal[0].Y = midpoint.Y;
                    voltterminal[1].X = midpoint.X + 8;
                    voltterminal[1].Y = midpoint.Y;
                    screen.DrawLine(myPen, voltterminal[1], pt2);
                    screen.DrawLine(myPen, pt1, voltterminal[0]);
                    //the negative pole(-)
                    voltterminal[2].X = midpoint.X - 8;
                    voltterminal[2].Y = midpoint.Y + 4;
                    voltterminal[3].X = midpoint.X - 8;
                    voltterminal[3].Y = midpoint.Y - 4;
                    screen.DrawLine(myPen, voltterminal[2], voltterminal[3]);
                    //the positive pole(+)
                    voltterminal[4].X = midpoint.X + 8;
                    voltterminal[4].Y = midpoint.Y + 8;
                    voltterminal[5].X = midpoint.X + 8;
                    voltterminal[5].Y = midpoint.Y - 8;
                    screen.DrawLine(myPen, voltterminal[4], voltterminal[5]);
                }
                //when the line is horizontal
                if (pt1.X == pt2.X)
                {
                    voltterminal[0].X = midpoint.X;
                    voltterminal[0].Y = midpoint.Y - 8;
                    voltterminal[1].X = midpoint.X;
                    voltterminal[1].Y = midpoint.Y + 8;
                    screen.DrawLine(myPen, voltterminal[1], pt2);
                    screen.DrawLine(myPen, pt1, voltterminal[0]);
                    //the negative pole(-)
                    voltterminal[2].X = midpoint.X + 4;
                    voltterminal[2].Y = midpoint.Y - 8;
                    voltterminal[3].X = midpoint.X - 4;
                    voltterminal[3].Y = midpoint.Y - 8;
                    screen.DrawLine(myPen, voltterminal[2], voltterminal[3]);
                    //the positive pole(+)
                    voltterminal[4].X = midpoint.X + 8;
                    voltterminal[4].Y = midpoint.Y + 8;
                    voltterminal[5].X = midpoint.X - 8;
                    voltterminal[5].Y = midpoint.Y + 8;
                    screen.DrawLine(myPen, voltterminal[4], voltterminal[5]);
                }
                if((pt1.X!=pt2.X) && (pt1.Y!=pt2.Y))
                    screen.DrawLine(myPen, pt1, pt2);
            }

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
               
            
            
    
                

            screen.DrawString(type.ToString(), new Font("Arial", 16), new SolidBrush(myPen.Color), midpoint);
        }

 /*      public void turningpoint(Point startpoint[])
        {
            startpoint[0].X - 2;
            startpoint[0].Y + 1;
            startpoint[0].X + 2;
            startpoint[0].Y + 3;
            startpoint[0].X;
            startpoint[0].Y + 4;
        }
 */
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
            if (type != 'w' && type != 'g')
            {
                return type.ToString() + " " + terminals[0].ToString() + " " + terminals[1].ToString() + " " + characteristic.ToString();
            }
            return type.ToString() + " " + terminals[0].ToString() + " " + terminals[1].ToString();
        }
    }
}
