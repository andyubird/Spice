using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Spice
{
    public partial class Main : Form
    {
        int toolBarHeight = 50;
        int statusBarHeight = 22;
        int gridSize = 20;

        List<CircuitElm> lines = new List<CircuitElm>();

        Point temppoint;
        bool _mousePressed;

        char tool = 'w';

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
        }

        private void Main_Paint(object sender, PaintEventArgs e)
        {
            //setup screen size and clear background
            Graphics screen = e.Graphics;
            Rectangle rect = this.ClientRectangle;
            rect.Height -= (toolBarHeight + statusBarHeight);
            rect.Y += toolBarHeight;

            Region reg = new Region(rect);
            screen.Clip = reg;
            //screen.TranslateTransform(0, toolBarHeight);

            screen.Clear(Color.Black);

            Pen myPen = new Pen(Color.DarkGray, 2);

            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].draw(screen);
            }
        }

        private void Frametime_Tick(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void Main_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].checkBound(PointToClient(System.Windows.Forms.Cursor.Position)))
                {
                    lines.RemoveAt(i);
                    return;
                }
            }
            temppoint = PointToClient(System.Windows.Forms.Cursor.Position);
            _mousePressed = true;
            lines.Add(new CircuitElm(tool,temppoint, PointToClient(System.Windows.Forms.Cursor.Position)));
            lines[lines.Count - 1].round(gridSize);
        }

        private void Main_MouseUp(object sender, MouseEventArgs e)
        {
            if (_mousePressed == true)
            {
                lines[lines.Count - 1].pt2 = PointToClient(System.Windows.Forms.Cursor.Position);
                lines[lines.Count - 1].round(gridSize);
                _mousePressed = false;
                toolStripStatusLabel1.Text = lines[lines.Count - 1].pt2.ToString();
            }

            lines.RemoveAll(item => item.pt1 == item.pt2);
        }

        private void Main_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mousePressed == true)
            {
                lines[lines.Count - 1].pt2 = PointToClient(System.Windows.Forms.Cursor.Position);
                lines[lines.Count - 1].round(gridSize);
                toolStripStatusLabel1.Text = lines[lines.Count - 1].pt2.ToString();
            }
            else
            {
                toolStripStatusLabel1.Text = PointToClient(System.Windows.Forms.Cursor.Position).ToString() + lines.Count.ToString();
            }

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].checkBound(PointToClient(System.Windows.Forms.Cursor.Position)))
                {
                    toolStripStatusLabel3.Text = lines[i].getDump();
                }
            }
        }

        private void wireToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tool = 'w';
            toolStripStatusLabel3.Text = "Wire";
        }

        private void resistorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tool = 'r';
            toolStripStatusLabel3.Text = "Resistor";
        }

        private void dCVoltageSourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tool = 'v';
            toolStripStatusLabel3.Text = "DC Voltage Source";
        }

        private void groundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tool = 'g';
            toolStripStatusLabel3.Text = "Ground";
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {

        }
    }
}
