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

        List<Line> lines = new List<Line>();

        Point temppoint;
        bool _mousePressed;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
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
                screen.DrawLine(myPen, lines[i].pt1, lines[i].pt2);
            }
        }

        private void Frametime_Tick(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void Main_MouseDown(object sender, MouseEventArgs e)
        {
            temppoint = PointToClient(System.Windows.Forms.Cursor.Position);
            _mousePressed = true;
            lines.Add(new Line(temppoint, PointToClient(System.Windows.Forms.Cursor.Position)));
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
                toolStripStatusLabel1.Text = PointToClient(System.Windows.Forms.Cursor.Position).ToString();
            }
        }
    }
}
