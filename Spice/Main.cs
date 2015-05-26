using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO; // File: for BinaryWriter BinaryReader

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
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
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
                lines.Add(new CircuitElm(tool, temppoint, PointToClient(System.Windows.Forms.Cursor.Position)));
                lines[lines.Count - 1].round(gridSize);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].checkBound(PointToClient(System.Windows.Forms.Cursor.Position)))
                    {
                        PropertyEditor pe = new PropertyEditor(lines[i]);
                        pe.ShowDialog();
                        return;
                    }
                }
            }
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
            List<Terminal> terminals = new List<Terminal>();
            List<NodeElm> nodes = new List<NodeElm>();


            //Create terminal list
            foreach (CircuitElm item in lines)
            {
                if (!terminals.Exists(elm => elm.pt == item.pt1)) terminals.Add(new Terminal(item.pt1));
                if (!terminals.Exists(elm => elm.pt == item.pt2)) terminals.Add(new Terminal(item.pt2));
            }

            //Create connected circuit list inside each terminal
            for (int i = 0; i < lines.Count; i++)
            {
                foreach (Terminal terminal in terminals)
                {
                    if (terminal.pt == lines[i].pt1) terminal.addtolist(i);
                    if (terminal.pt == lines[i].pt2) terminal.addtolist(i);
                }
            }

            //Add terminal numbers to each CircuitElm in lines
            foreach (CircuitElm item in lines)
            {
                for (int i = 0; i < terminals.Count; i++)
                {
                    if (item.pt1 == terminals[i].pt) item.terminals[0] = i;
                    if (item.pt2 == terminals[i].pt) item.terminals[1] = i;
                }
            }

            //Create node list
            //找到相同電位的多個終點terminal並歸在同一個node
            for (int i = 0; i < terminals.Count; i++)
            {
                //Check if a terminal is already in a node
                if (nodes.Exists(node => node.connectedTerminals.Exists(t => t == i))) continue;

                //Create node and add all other connected teminals
                nodes.Add(new NodeElm());
                nodes.Last().addtolist(i);

                bool finished = true;
                do
                {
                    finished = true;
                    for (int j = 0; j < nodes.Last().connectedTerminals.Count; j++)
                    {
                        for (int k = 0; k < lines.Count; k++)
                        {
                            if (lines[k].type == 'w' && lines[k].terminals.Contains(nodes.Last().connectedTerminals[j]))
                            {
                                int index = System.Array.IndexOf(lines[k].terminals, nodes.Last().connectedTerminals[j]);
                                int otherTerminal = lines[k].terminals[(-1) * index + 1];
                                if (!nodes.Last().connectedTerminals.Exists(t => t == otherTerminal))
                                {
                                    nodes.Last().addtolist(otherTerminal);
                                    finished = false;
                                }
                            }
                        }
                    }
                } while (!finished);

            }
            //end node list
            

            textBox1.Text = "List of terminals:\r\n";

            foreach (Terminal terminal in terminals)
            {
                textBox1.Text += terminal.pt.ToString();
                foreach (int a in terminal.connectedCircuits)
                {
                    textBox1.Text += " " + a.ToString();
                }

                textBox1.Text += System.Environment.NewLine;
            }

            textBox1.Text += "\r\nList of elements:\r\n";

            foreach (CircuitElm item in lines)
            {
                textBox1.Text += item.getDump() + System.Environment.NewLine;
            }

            textBox1.Text += "\r\nList of nodes:\r\n";

            foreach (NodeElm node in nodes)
            {
                textBox1.Text += node.getDump() + System.Environment.NewLine;
            }

            //find ground terminal and obsolete ground terminal

            int gt, ogt, gn = new int();

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].type == 'g')
                {
                    ogt = terminals.FindIndex(t => t.connectedCircuits.Contains(i) && t.connectedCircuits.Count == 1);
                    gt = terminals.FindIndex(t => t.connectedCircuits.Contains(i) && t.connectedCircuits.Count != 1);
                    gn = nodes.FindIndex(n => n.connectedTerminals.Contains(gt));
                    textBox1.Text += "\r\nGround node:" + gn.ToString() + "\r\n";
                }
            }

            //build matrix

            float[,] matrix = new float[terminals.Count - 1, terminals.Count];

            List<CircuitElm> devices = lines.FindAll(line => line.type != 'w' && line.type != 'g');
            List<CircuitElm> links = lines.FindAll(line => line.type == 'w');

            for (int i = 1; i < devices.Count; i++)
            {
                for (int j = 0; j < links.Count; j++)
                {
                    if (devices[i].terminals[0] == links[j].terminals[0]) matrix[i - 1, j]++;
                    if (devices[i].terminals[0] == links[j].terminals[1]) matrix[i - 1, j]--;
                    if (devices[i].terminals[1] == links[j].terminals[0]) matrix[i - 1, j]++;
                    if (devices[i].terminals[1] == links[j].terminals[1]) matrix[i - 1, j]--;
                }
            }

            matrix[devices.Count - 1, links.Count + gn]++;

            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i].type == 'r')
                {
                    for (int j = 0; j < links.Count; j++)
                    {
                        if (devices[i].terminals[0] == links[j].terminals[0]) matrix[devices.Count + i, j] += devices[i].characteristic;
                        if (devices[i].terminals[0] == links[j].terminals[1]) matrix[devices.Count + i, j] -= devices[i].characteristic;
                    }

                    for (int j = 0; j < nodes.Count; j++)
                    {
                        if (nodes[j].connectedTerminals.Contains(devices[i].terminals[0])) matrix[devices.Count + i, links.Count + j]++;
                        if (nodes[j].connectedTerminals.Contains(devices[i].terminals[1])) matrix[devices.Count + i, links.Count + j]--;
                    }
                }

                if (devices[i].type == 'v')
                {
                    for (int j = 0; j < nodes.Count; j++)
                    {
                        if (nodes[j].connectedTerminals.Contains(devices[i].terminals[0])) matrix[devices.Count + i, links.Count + j]++;
                        if (nodes[j].connectedTerminals.Contains(devices[i].terminals[1])) matrix[devices.Count + i, links.Count + j]--;
                    }

                    matrix[devices.Count + i, terminals.Count - 1] += devices[i].characteristic;
                }
            }

            textBox1.Text += "\r\nMatrix:\r\n";

            for (int i = 0; i < terminals.Count - 1; i++)
            {
                for (int j = 0; j < terminals.Count; j++)
                {
                    textBox1.Text += matrix[i, j].ToString() + "\r\t";
                }
                textBox1.Text += System.Environment.NewLine;
            }

            //solve matrix
            int l = 1;
            float temp;
            float r;
            float[] sol = new float[terminals.Count - 1];
            for (int i = 0; i < terminals.Count - 1; i++)
                for (int j = 0; j < terminals.Count - 1; j++)
                {
                    l = 1;
                    if (i != j)
                    {
                        while (matrix[i, i] == 0)
                        {

                            if (matrix[l + i, i] != 0)
                                for (int k = i; k < terminals.Count; k++)
                                {
                                    temp = matrix[l + i, k];
                                    matrix[l + i, k] = matrix[i, k];
                                    matrix[i, k] = temp;
                                }
                            l++;
                        }


                        if (matrix[i, i] != 0)
                        {
                            r = matrix[j, i] / matrix[i, i];

                            for (int k = i; k < terminals.Count; k++)
                                matrix[j, k] += -r * matrix[i, k];
                        }

                    }
                }
            for (int i = 0; i < terminals.Count - 1; i++)
            {

                matrix[i, terminals.Count - 1] = matrix[i, terminals.Count - 1] / matrix[i, i];
                matrix[i, i] = 1;
                sol[i] = matrix[i, terminals.Count - 1];
            }

            //output sol
            for (int i = 0; i < links.Count; i++)
            {
                textBox1.Text += "Link " + i.ToString() + ": " + sol[i] + System.Environment.NewLine;
            }

            int groundnode = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (i != gn)
                {
                    textBox1.Text += "Node " + i.ToString() + ": " + sol[links.Count + i - groundnode] + System.Environment.NewLine;
                }
                else
                {
                    groundnode++;
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String myfile = "";

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            // From Open File Dialog to get the name of file
            saveFileDialog1.ShowDialog();
            myfile = saveFileDialog1.FileName;

            //Check there is somthing or not in "myfile"
            if (myfile == "")
            {
                return;
            }

            // setting
            FileStream outFile = new FileStream(myfile, FileMode.Create, FileAccess.Write);
            
            // opne File
            StreamWriter streamOut = new StreamWriter(outFile);
            // write File
            // for loop to save one by one
            for (int i = 0; i < lines.Count; i++)
            {
                streamOut.WriteLine(lines[i].saveDump());
            }
            streamOut.Close();
            
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filename = openFileDialog1.FileName;

                string[] filelines = File.ReadAllLines(filename);

                lines.Clear();

                for(int i = 0; i < filelines.Length; i++)
                {
                    // produce array
                    string[] splitLines = filelines[i].Split(' ');
                    // to check and construct constructor
                    // there're two constructor: one has 5 parameter and the other one has 6 parameter.
                    if (splitLines.Length == 5) { lines.Add(new CircuitElm(splitLines[0][0], Convert.ToInt32(splitLines[1]), Convert.ToInt32(splitLines[2]), Convert.ToInt32(splitLines[3]), Convert.ToInt32(splitLines[4]))); }
                    if (splitLines.Length == 6) { lines.Add(new CircuitElm(splitLines[0][0], Convert.ToInt32(splitLines[1]), Convert.ToInt32(splitLines[2]), Convert.ToInt32(splitLines[3]), Convert.ToInt32(splitLines[4]), (float)Convert.ToDouble(splitLines[5]))); }
                }

            }
        }

        
    }
}
