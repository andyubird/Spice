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

        bool circuitNeedsMap;

        List<CircuitElm> elmList = new List<CircuitElm>();

        Point temppoint;
        bool _mousePressed;

        char tool = 'w';

        string stopMessage;
        CircuitElm dragElm, menuElm, mouseElm, stopElm;

        List<CircuitNode> nodeList;
        CircuitElm[] voltageSources;

        double[][] circuitMatrix; // contains circuit state
		double[] circuitRightSide;
		double[][] origMatrix;
		double[] origRightSide;
		RowInfo[] circuitRowInfo;
		int[] circuitPermute;

        public CircuitElm getElm(int n) {
			return (n < elmList.Count) ? elmList[n] : null;
		}

        #region //// Stamp ////
		
		public void stampCurrentSource(int n1, int n2, double i) {
			stampRightSide(n1, -i);
			stampRightSide(n2, i);
		}

		// stamp independent voltage source #vs, from n1 to n2, amount v
		public void stampVoltageSource(int n1, int n2, int vs, double v) {
			int vn = nodeList.Count + vs;
			stampMatrix(vn, n1, -1);
			stampMatrix(vn, n2, 1);
			stampRightSide(vn, v);
			stampMatrix(n1, vn, 1);
			stampMatrix(n2, vn, -1);
		}

		// use this if the amount of voltage is going to be updated in doStep()
		public void stampVoltageSource(int n1, int n2, int vs) {
			int vn = nodeList.Count + vs;
			stampMatrix(vn, n1, -1);
			stampMatrix(vn, n2, 1);
			stampRightSide(vn);
			stampMatrix(n1, vn, 1);
			stampMatrix(n2, vn, -1);
		}

		public void stampResistor(int n1, int n2, double r) {
			double r0 = 1 / r;
			stampMatrix(n1, n1, r0);
			stampMatrix(n2, n2, r0);
			stampMatrix(n1, n2, -r0);
			stampMatrix(n2, n1, -r0);
		}

		public void stampConductance(int n1, int n2, double r0) {
			stampMatrix(n1, n1, r0);
			stampMatrix(n2, n2, r0);
			stampMatrix(n1, n2, -r0);
			stampMatrix(n2, n1, -r0);
		}

		/// <summary>
		/// Voltage-controlled voltage source.
		/// Control voltage source vs with voltage from n1 to n2 
		/// (must also call stampVoltageSource())
		/// </summary>
		public void stampVCVS(int n1, int n2, double coef, int vs) {
			int vn = nodeList.Count + vs;
			stampMatrix(vn, n1, coef);
			stampMatrix(vn, n2, -coef);
		}

		/// <summary>
		/// Voltage-controlled current source.
		/// Current from cn1 to cn2 is equal to voltage from vn1 to vn2, divided by g 
		/// </summary>
		public void stampVCCS(int cn1, int cn2, int vn1, int vn2, double g) {
			stampMatrix(cn1, vn1, g);
			stampMatrix(cn2, vn2, g);
			stampMatrix(cn1, vn2, -g);
			stampMatrix(cn2, vn1, -g);
		}

		// Current-controlled voltage source (CCVS)?

		/// <summary>
		/// Current-controlled current source.
		/// Stamp a current source from n1 to n2 depending on current through vs 
		/// </summary>
		public void stampCCCS(int n1, int n2, int vs, double gain) {
			int vn = nodeList.Count + vs;
			stampMatrix(n1, vn, gain);
			stampMatrix(n2, vn, -gain);
		}

		// stamp value x in row i, column j, meaning that a voltage change
		// of dv in node j will increase the current into node i by x dv
		// (Unless i or j is a voltage source node.)
		public void stampMatrix(int i, int j, double x) {
			if(i > 0 && j > 0) {
				if(circuitNeedsMap) {
					i = circuitRowInfo[i - 1].mapRow;
					RowInfo ri = circuitRowInfo[j - 1];
					if(ri.type == RowInfo.ROW_CONST) {
						circuitRightSide[i] -= x * ri.value;
						return;
					}
					j = ri.mapCol;
				} else {
					i--;
					j--;
				}
				circuitMatrix[i][j] += x;
			}
		}

		// stamp value x on the right side of row i, representing an
		// independent current source flowing into node i
		public void stampRightSide(int i, double x) {
			if(i > 0) {
				i = (circuitNeedsMap) ? circuitRowInfo[i - 1].mapRow : i - 1;
				circuitRightSide[i] += x;
			}
		}

		// indicate that the value on the right side of row i changes in doStep()
		public void stampRightSide(int i) {
			if(i > 0) circuitRowInfo[i - 1].rsChanges = true;
		}

		// indicate that the values on the left side of row i change in doStep()
		public void stampNonLinear(int i) {
			if(i > 0) circuitRowInfo[i - 1].lsChanges = true;
		}
		#endregion

        #region Click Functions

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

            for (int i = 0; i < elmList.Count; i++)
            {
                elmList[i].draw(screen);
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
                for (int i = 0; i < elmList.Count; i++)
                {
                    if (elmList[i].checkBound(PointToClient(System.Windows.Forms.Cursor.Position)))
                    {
                        elmList.RemoveAt(i);
                        return;
                    }
                }
                temppoint = PointToClient(System.Windows.Forms.Cursor.Position);
                _mousePressed = true;
                elmList.Add(new CircuitElm(tool, temppoint, PointToClient(System.Windows.Forms.Cursor.Position)));
                elmList[elmList.Count - 1].round(gridSize);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                for (int i = 0; i < elmList.Count; i++)
                {
                    if (elmList[i].checkBound(PointToClient(System.Windows.Forms.Cursor.Position)))
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
                elmList[elmList.Count - 1].pt2 = PointToClient(System.Windows.Forms.Cursor.Position);
                elmList[elmList.Count - 1].round(gridSize);
                _mousePressed = false;
                toolStripStatusLabel1.Text = elmList[elmList.Count - 1].pt2.ToString();
            }

            lines.RemoveAll(item => item.pt1 == item.pt2);
        }

        private void Main_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mousePressed == true)
            {
                elmList[elmList.Count - 1].pt2 = PointToClient(System.Windows.Forms.Cursor.Position);
                elmList[elmList.Count - 1].round(gridSize);
                toolStripStatusLabel1.Text = elmList[elmList.Count - 1].pt2.ToString();
            }
            else
            {
                toolStripStatusLabel1.Text = PointToClient(System.Windows.Forms.Cursor.Position).ToString() + elmList.Count.ToString();
            }

            for (int i = 0; i < elmList.Count; i++)
            {
                if (elmList[i].checkBound(PointToClient(System.Windows.Forms.Cursor.Position)))
                {
                    toolStripStatusLabel3.Text = elmList[i].getDump();
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
            #region OLD COLD
            //List<Terminal> terminals = new List<Terminal>();
            //List<NodeElm> nodes = new List<NodeElm>();

            //if (lines.Count == 0) return;

            ////Create terminal list
            //foreach (CircuitElm item in lines)
            //{
            //    if (!terminals.Exists(elm => elm.pt == item.pt1)) terminals.Add(new Terminal(item.pt1));
            //    if (!terminals.Exists(elm => elm.pt == item.pt2)) terminals.Add(new Terminal(item.pt2));
            //}

            ////Create connected circuit list inside each terminal
            //for (int i = 0; i < lines.Count; i++)
            //{
            //    foreach (Terminal terminal in terminals)
            //    {
            //        if (terminal.pt == lines[i].pt1) terminal.addtolist(i);
            //        if (terminal.pt == lines[i].pt2) terminal.addtolist(i);
            //    }
            //}

            ////Add terminal numbers to each CircuitElm in lines
            //foreach (CircuitElm item in lines)
            //{
            //    for (int i = 0; i < terminals.Count; i++)
            //    {
            //        if (item.pt1 == terminals[i].pt) item.terminals[0] = i;
            //        if (item.pt2 == terminals[i].pt) item.terminals[1] = i;
            //    }
            //}

            ////Create node list
            ////找到相同電位的多個終點terminal並歸在同一個node
            //for (int i = 0; i < terminals.Count; i++)
            //{
            //    //Check if a terminal is already in a node
            //    if (nodes.Exists(node => node.connectedTerminals.Exists(t => t == i))) continue;

            //    //Create node and add all other connected teminals
            //    nodes.Add(new NodeElm());
            //    nodes.Last().addtolist(i);

            //    bool finished = true;
            //    do
            //    {
            //        finished = true;
            //        for (int j = 0; j < nodes.Last().connectedTerminals.Count; j++)
            //        {
            //            for (int k = 0; k < lines.Count; k++)
            //            {
            //                if (lines[k].type == 'w' && lines[k].terminals.Contains(nodes.Last().connectedTerminals[j]))
            //                {
            //                    int index = System.Array.IndexOf(lines[k].terminals, nodes.Last().connectedTerminals[j]);
            //                    int otherTerminal = lines[k].terminals[(-1) * index + 1];
            //                    if (!nodes.Last().connectedTerminals.Exists(t => t == otherTerminal))
            //                    {
            //                        nodes.Last().addtolist(otherTerminal);
            //                        finished = false;
            //                    }
            //                }
            //            }
            //        }
            //    } while (!finished);

            //}
            ////end node list
            

            //textBox1.Text = "List of terminals:\r\n";

            //foreach (Terminal terminal in terminals)
            //{
            //    textBox1.Text += terminal.pt.ToString();
            //    foreach (int a in terminal.connectedCircuits)
            //    {
            //        textBox1.Text += " " + a.ToString();
            //    }

            //    textBox1.Text += System.Environment.NewLine;
            //}

            //textBox1.Text += "\r\nList of elements:\r\n";

            //foreach (CircuitElm item in lines)
            //{
            //    textBox1.Text += item.getDump() + System.Environment.NewLine;
            //}

            //textBox1.Text += "\r\nList of nodes:\r\n";

            //foreach (NodeElm node in nodes)
            //{
            //    textBox1.Text += node.getDump() + System.Environment.NewLine;
            //}

            ////find ground terminal and obsolete ground terminal

            //int gt, ogt, gn = new int();

            //for (int i = 0; i < lines.Count; i++)
            //{
            //    if (lines[i].type == 'g')
            //    {
            //        ogt = terminals.FindIndex(t => t.connectedCircuits.Contains(i) && t.connectedCircuits.Count == 1);
            //        gt = terminals.FindIndex(t => t.connectedCircuits.Contains(i) && t.connectedCircuits.Count != 1);
            //        gn = nodes.FindIndex(n => n.connectedTerminals.Contains(gt));
            //        textBox1.Text += "\r\nGround node:" + gn.ToString() + "\r\n";
            //    }
            //}

            ////build matrix

            //float[,] matrix = new float[terminals.Count - 1, terminals.Count];

            //List<CircuitElm> devices = lines.FindAll(line => line.type != 'w' && line.type != 'g');
            //List<CircuitElm> links = lines.FindAll(line => line.type == 'w');

            //for (int i = 1; i < devices.Count; i++)
            //{
            //    for (int j = 0; j < links.Count; j++)
            //    {
            //        if (devices[i].terminals[0] == links[j].terminals[0]) matrix[i - 1, j]++;
            //        if (devices[i].terminals[0] == links[j].terminals[1]) matrix[i - 1, j]--;
            //        if (devices[i].terminals[1] == links[j].terminals[0]) matrix[i - 1, j]++;
            //        if (devices[i].terminals[1] == links[j].terminals[1]) matrix[i - 1, j]--;
            //    }
            //}

            //matrix[devices.Count - 1, links.Count + gn]++;

            //for (int i = 0; i < devices.Count; i++)
            //{
            //    if (devices[i].type == 'r')
            //    {
            //        for (int j = 0; j < links.Count; j++)
            //        {
            //            if (devices[i].terminals[0] == links[j].terminals[0]) matrix[devices.Count + i, j] += devices[i].characteristic;
            //            if (devices[i].terminals[0] == links[j].terminals[1]) matrix[devices.Count + i, j] -= devices[i].characteristic;
            //        }

            //        for (int j = 0; j < nodes.Count; j++)
            //        {
            //            if (nodes[j].connectedTerminals.Contains(devices[i].terminals[0])) matrix[devices.Count + i, links.Count + j]++;
            //            if (nodes[j].connectedTerminals.Contains(devices[i].terminals[1])) matrix[devices.Count + i, links.Count + j]--;
            //        }
            //    }

            //    if (devices[i].type == 'v')
            //    {
            //        for (int j = 0; j < nodes.Count; j++)
            //        {
            //            if (nodes[j].connectedTerminals.Contains(devices[i].terminals[0])) matrix[devices.Count + i, links.Count + j]++;
            //            if (nodes[j].connectedTerminals.Contains(devices[i].terminals[1])) matrix[devices.Count + i, links.Count + j]--;
            //        }

            //        matrix[devices.Count + i, terminals.Count - 1] += devices[i].characteristic;
            //    }
            //}

            //textBox1.Text += "\r\nMatrix:\r\n";

            //for (int i = 0; i < terminals.Count - 1; i++)
            //{
            //    for (int j = 0; j < terminals.Count; j++)
            //    {
            //        textBox1.Text += matrix[i, j].ToString() + "\r\t";
            //    }
            //    textBox1.Text += System.Environment.NewLine;
            //}

            ////solve matrix
            //int l = 1;
            //float temp;
            //float r;
            //float[] sol = new float[terminals.Count - 1];
            //for (int i = 0; i < terminals.Count - 1; i++)
            //    for (int j = 0; j < terminals.Count - 1; j++)
            //    {
            //        l = 1;
            //        if (i != j)
            //        {
            //            while (matrix[i, i] == 0)
            //            {

            //                if (matrix[l + i, i] != 0)
            //                    for (int k = i; k < terminals.Count; k++)
            //                    {
            //                        temp = matrix[l + i, k];
            //                        matrix[l + i, k] = matrix[i, k];
            //                        matrix[i, k] = temp;
            //                    }
            //                l++;
            //            }


            //            if (matrix[i, i] != 0)
            //            {
            //                r = matrix[j, i] / matrix[i, i];

            //                for (int k = i; k < terminals.Count; k++)
            //                    matrix[j, k] += -r * matrix[i, k];
            //            }

            //        }
            //    }
            //for (int i = 0; i < terminals.Count - 1; i++)
            //{

            //    matrix[i, terminals.Count - 1] = matrix[i, terminals.Count - 1] / matrix[i, i];
            //    matrix[i, i] = 1;
            //    sol[i] = matrix[i, terminals.Count - 1];
            //}

            ////output sol
            //for (int i = 0; i < links.Count; i++)
            //{
            //    textBox1.Text += "Link " + i.ToString() + ": " + sol[i] + System.Environment.NewLine;
            //}

            //int groundnode = 0;
            //for (int i = 0; i < nodes.Count; i++)
            //{
            //    if (i != gn)
            //    {
            //        textBox1.Text += "Node " + i.ToString() + ": " + sol[links.Count + i - groundnode] + System.Environment.NewLine;
            //    }
            //    else
            //    {
            //        groundnode++;
            //    }
            //}

            #endregion

            analyze();
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

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lines.Clear();
        }

        #endregion

        private void analyzeCircuit()
        {
            	
		//calcCircuitBottom();
		if (elmList.Count() == 0)
		return;
		stopMessage = null;
		stopElm = null;
		int i, j;
		int vscount = 0;	
		nodeList = new List<CircuitNode>();
		bool gotGround = false;
		bool gotRail = false;
		CircuitElm volt = null;

		//System.out.println("ac1");
		// look for voltage or ground element
		for (i = 0; i != elmList.Count(); i++) {
			CircuitElm ce = getElm(i);
			if (ce is GroundElm) {
				gotGround = true;
				break;
			}
            //if (ce is RailElm)
            //    gotRail = true;
			if (volt == null && ce is VoltageElm)
			    volt = ce;
		}

		// if no ground, and no rails, then the voltage elm's first terminal
		// is ground
		if (!gotGround && volt != null && !gotRail) {
			CircuitNode cn = new CircuitNode();
			Point pt = volt.getPost(0);
			cn.x = pt.x;
			cn.y = pt.y;
			nodeList.addElement(cn);
		} else {
			// otherwise allocate extra node for ground
			CircuitNode cn = new CircuitNode();
			cn.x = cn.y = -1;
			nodeList.addElement(cn);
		}
		//System.out.println("ac2");

		// allocate nodes and voltage sources
		for (i = 0; i != elmList.size(); i++) {
			CircuitElm ce = getElm(i);
			int inodes = ce.getInternalNodeCount();
			int ivs = ce.getVoltageSourceCount();
			int posts = ce.getPostCount();
			
			// allocate a node for each post and match posts to nodes
			for (j = 0; j != posts; j++) {
				Point pt = ce.getPost(j);
				int k;
				for (k = 0; k != nodeList.size(); k++) {
					CircuitNode cn = getCircuitNode(k);
					if (pt.x == cn.x && pt.y == cn.y)
					break;
				}
				if (k == nodeList.size()) {
					CircuitNode cn = new CircuitNode();
					cn.x = pt.x;
					cn.y = pt.y;
					CircuitNodeLink cnl = new CircuitNodeLink();
					cnl.num = j;
					cnl.elm = ce;
					cn.links.addElement(cnl);
					ce.setNode(j, nodeList.size());
					nodeList.addElement(cn);
				} else {
					CircuitNodeLink cnl = new CircuitNodeLink();
					cnl.num = j;
					cnl.elm = ce;
					getCircuitNode(k).links.addElement(cnl);
					ce.setNode(j, k);
					// if it's the ground node, make sure the node voltage is 0,
					// cause it may not get set later
					if (k == 0)
					ce.setNodeVoltage(j, 0);
				}
			}
			for (j = 0; j != inodes; j++) {
				CircuitNode cn = new CircuitNode();
				cn.x = cn.y = -1;
				cn.internal = true;
				CircuitNodeLink cnl = new CircuitNodeLink();
				cnl.num = j+posts;
				cnl.elm = ce;
				cn.links.addElement(cnl);
				ce.setNode(cnl.num, nodeList.size());
				nodeList.addElement(cn);
			}
			vscount += ivs;
		}
		voltageSources = new CircuitElm[vscount];
		vscount = 0;
		circuitNonLinear = false;
		//System.out.println("ac3");

		// determine if circuit is nonlinear
		for (i = 0; i != elmList.size(); i++) {
			CircuitElm ce = getElm(i);
			if (ce.nonLinear())
			circuitNonLinear = true;
			int ivs = ce.getVoltageSourceCount();
			for (j = 0; j != ivs; j++) {
				voltageSources[vscount] = ce;
				ce.setVoltageSource(j, vscount++);
			}
		}
		voltageSourceCount = vscount;

		int matrixSize = nodeList.size()-1 + vscount;
		circuitMatrix = new double[matrixSize][matrixSize];
		circuitRightSide = new double[matrixSize];
		origMatrix = new double[matrixSize][matrixSize];
		origRightSide = new double[matrixSize];
		circuitMatrixSize = circuitMatrixFullSize = matrixSize;
		circuitRowInfo = new RowInfo[matrixSize];
		circuitPermute = new int[matrixSize];
		int vs = 0;
		for (i = 0; i != matrixSize; i++)
		circuitRowInfo[i] = new RowInfo();
		circuitNeedsMap = false;
		
		// stamp linear circuit elements
		for (i = 0; i != elmList.size(); i++) {
			CircuitElm ce = getElm(i);
			ce.stamp();
		}
		//System.out.println("ac4");

		// determine nodes that are unconnected
		boolean closure[] = new boolean[nodeList.size()];
		boolean tempclosure[] = new boolean[nodeList.size()];
		boolean changed = true;
		closure[0] = true;
		while (changed) {
			changed = false;
			for (i = 0; i != elmList.size(); i++) {
				CircuitElm ce = getElm(i);
				// loop through all ce's nodes to see if they are connected
				// to other nodes not in closure
				for (j = 0; j < ce.getPostCount(); j++) {
					if (!closure[ce.getNode(j)]) {
						if (ce.hasGroundConnection(j))
						closure[ce.getNode(j)] = changed = true;
						continue;
					}
					int k;
					for (k = 0; k != ce.getPostCount(); k++) {
						if (j == k)
						continue;
						int kn = ce.getNode(k);
						if (ce.getConnection(j, k) && !closure[kn]) {
							closure[kn] = true;
							changed = true;
						}
					}
				}
			}
			if (changed)
			continue;

			// connect unconnected nodes
			for (i = 0; i != nodeList.size(); i++)
			if (!closure[i] && !getCircuitNode(i).internal) {
				System.out.println("node " + i + " unconnected");
				stampResistor(0, i, 1e8);
				closure[i] = true;
				changed = true;
				break;
			}
		}
		//System.out.println("ac5");

		for (i = 0; i != elmList.size(); i++) {
			CircuitElm ce = getElm(i);
			// look for inductors with no current path
			if (ce instanceof InductorElm) {
				FindPathInfo fpi = new FindPathInfo(FindPathInfo.INDUCT, ce,
				ce.getNode(1));
				// first try findPath with maximum depth of 5, to avoid slowdowns
				if (!fpi.findPath(ce.getNode(0), 5) &&
						!fpi.findPath(ce.getNode(0))) {
					System.out.println(ce + " no path");
					ce.reset();
				}
			}
			// look for current sources with no current path
			if (ce instanceof CurrentElm) {
				FindPathInfo fpi = new FindPathInfo(FindPathInfo.INDUCT, ce,
				ce.getNode(1));
				if (!fpi.findPath(ce.getNode(0))) {
					stop("No path for current source!", ce);
					return;
				}
			}
			// look for voltage source loops
			if ((ce instanceof VoltageElm && ce.getPostCount() == 2) ||
					ce instanceof WireElm) {
				FindPathInfo fpi = new FindPathInfo(FindPathInfo.VOLTAGE, ce,
				ce.getNode(1));
				if (fpi.findPath(ce.getNode(0))) {
					stop("Voltage source/wire loop with no resistance!", ce);
					return;
				}
			}
			// look for shorted caps, or caps w/ voltage but no R
			if (ce instanceof CapacitorElm) {
				FindPathInfo fpi = new FindPathInfo(FindPathInfo.SHORT, ce,
				ce.getNode(1));
				if (fpi.findPath(ce.getNode(0))) {
					System.out.println(ce + " shorted");
					ce.reset();
				} else {
					fpi = new FindPathInfo(FindPathInfo.CAP_V, ce, ce.getNode(1));
					if (fpi.findPath(ce.getNode(0))) {
						stop("Capacitor loop with no resistance!", ce);
						return;
					}
				}
			}
		}
		//System.out.println("ac6");

		// simplify the matrix; this speeds things up quite a bit
		for (i = 0; i != matrixSize; i++) {
			int qm = -1, qp = -1;
			double qv = 0;
			RowInfo re = circuitRowInfo[i];
			/*System.out.println("row " + i + " " + re.lsChanges + " " + re.rsChanges + " " +
				re.dropRow);*/
			if (re.lsChanges || re.dropRow || re.rsChanges)
			continue;
			double rsadd = 0;

			// look for rows that can be removed
			for (j = 0; j != matrixSize; j++) {
				double q = circuitMatrix[i][j];
				if (circuitRowInfo[j].type == RowInfo.ROW_CONST) {
					// keep a running total of const values that have been
					// removed already
					rsadd -= circuitRowInfo[j].value*q;
					continue;
				}
				if (q == 0)
				continue;
				if (qp == -1) {
					qp = j;
					qv = q;
					continue;
				}
				if (qm == -1 && q == -qv) {
					qm = j;
					continue;
				}
				break;
			}
			//System.out.println("line " + i + " " + qp + " " + qm + " " + j);
			/*if (qp != -1 && circuitRowInfo[qp].lsChanges) {
		System.out.println("lschanges");
		continue;
		}
		if (qm != -1 && circuitRowInfo[qm].lsChanges) {
		System.out.println("lschanges");
		continue;
		}*/
			if (j == matrixSize) {
				if (qp == -1) {
					stop("Matrix error", null);
					return;
				}
				RowInfo elt = circuitRowInfo[qp];
				if (qm == -1) {
					// we found a row with only one nonzero entry; that value
					// is a constant
					int k;
					for (k = 0; elt.type == RowInfo.ROW_EQUAL && k < 100; k++) {
						// follow the chain
						/*System.out.println("following equal chain from " +
					i + " " + qp + " to " + elt.nodeEq);*/
						qp = elt.nodeEq;
						elt = circuitRowInfo[qp];
					}
					if (elt.type == RowInfo.ROW_EQUAL) {
						// break equal chains
						//System.out.println("Break equal chain");
						elt.type = RowInfo.ROW_NORMAL;
						continue;
					}
					if (elt.type != RowInfo.ROW_NORMAL) {
						System.out.println("type already " + elt.type + " for " + qp + "!");
						continue;
					}
					elt.type = RowInfo.ROW_CONST;
					elt.value = (circuitRightSide[i]+rsadd)/qv;
					circuitRowInfo[i].dropRow = true;
					//System.out.println(qp + " * " + qv + " = const " + elt.value);
					i = -1; // start over from scratch
				} else if (circuitRightSide[i]+rsadd == 0) {
					// we found a row with only two nonzero entries, and one
					// is the negative of the other; the values are equal
					if (elt.type != RowInfo.ROW_NORMAL) {
						//System.out.println("swapping");
						int qq = qm;
						qm = qp; qp = qq;
						elt = circuitRowInfo[qp];
						if (elt.type != RowInfo.ROW_NORMAL) {
							// we should follow the chain here, but this
							// hardly ever happens so it's not worth worrying
							// about
							System.out.println("swap failed");
							continue;
						}
					}
					elt.type = RowInfo.ROW_EQUAL;
					elt.nodeEq = qm;
					circuitRowInfo[i].dropRow = true;
					//System.out.println(qp + " = " + qm);
				}
			}
		}
		//System.out.println("ac7");

		// find size of new matrix
		int nn = 0;
		for (i = 0; i != matrixSize; i++) {
			RowInfo elt = circuitRowInfo[i];
			if (elt.type == RowInfo.ROW_NORMAL) {
				elt.mapCol = nn++;
				//System.out.println("col " + i + " maps to " + elt.mapCol);
				continue;
			}
			if (elt.type == RowInfo.ROW_EQUAL) {
				RowInfo e2 = null;
				// resolve chains of equality; 100 max steps to avoid loops
				for (j = 0; j != 100; j++) {
					e2 = circuitRowInfo[elt.nodeEq];
					if (e2.type != RowInfo.ROW_EQUAL)
					break;
					if (i == e2.nodeEq)
					break;
					elt.nodeEq = e2.nodeEq;
				}
			}
			if (elt.type == RowInfo.ROW_CONST)
			elt.mapCol = -1;
		}
		for (i = 0; i != matrixSize; i++) {
			RowInfo elt = circuitRowInfo[i];
			if (elt.type == RowInfo.ROW_EQUAL) {
				RowInfo e2 = circuitRowInfo[elt.nodeEq];
				if (e2.type == RowInfo.ROW_CONST) {
					// if something is equal to a const, it's a const
					elt.type = e2.type;
					elt.value = e2.value;
					elt.mapCol = -1;
					//System.out.println(i + " = [late]const " + elt.value);
				} else {
					elt.mapCol = e2.mapCol;
					//System.out.println(i + " maps to: " + e2.mapCol);
				}
			}
		}
		//System.out.println("ac8");

		/*System.out.println("matrixSize = " + matrixSize);
	
	for (j = 0; j != circuitMatrixSize; j++) {
		System.out.println(j + ": ");
		for (i = 0; i != circuitMatrixSize; i++)
		System.out.print(circuitMatrix[j][i] + " ");
		System.out.print("  " + circuitRightSide[j] + "\n");
	}
	System.out.print("\n");*/
		

		// make the new, simplified matrix
		int newsize = nn;
		double newmatx[][] = new double[newsize][newsize];
		double newrs  []   = new double[newsize];
		int ii = 0;
		for (i = 0; i != matrixSize; i++) {
			RowInfo rri = circuitRowInfo[i];
			if (rri.dropRow) {
				rri.mapRow = -1;
				continue;
			}
			newrs[ii] = circuitRightSide[i];
			rri.mapRow = ii;
			//System.out.println("Row " + i + " maps to " + ii);
			for (j = 0; j != matrixSize; j++) {
				RowInfo ri = circuitRowInfo[j];
				if (ri.type == RowInfo.ROW_CONST)
				newrs[ii] -= ri.value*circuitMatrix[i][j];
				else
				newmatx[ii][ri.mapCol] += circuitMatrix[i][j];
			}
			ii++;
		}

		circuitMatrix = newmatx;
		circuitRightSide = newrs;
		matrixSize = circuitMatrixSize = newsize;
		for (i = 0; i != matrixSize; i++)
		origRightSide[i] = circuitRightSide[i];
		for (i = 0; i != matrixSize; i++)
		for (j = 0; j != matrixSize; j++)
		origMatrix[i][j] = circuitMatrix[i][j];
		circuitNeedsMap = true;

		/*
	System.out.println("matrixSize = " + matrixSize + " " + circuitNonLinear);
	for (j = 0; j != circuitMatrixSize; j++) {
		for (i = 0; i != circuitMatrixSize; i++)
		System.out.print(circuitMatrix[j][i] + " ");
		System.out.print("  " + circuitRightSide[j] + "\n");
	}
	System.out.print("\n");*/

		// if a matrix is linear, we can do the lu_factor here instead of
		// needing to do it every frame
		if (!circuitNonLinear) {
			if (!lu_factor(circuitMatrix, circuitMatrixSize, circuitPermute)) {
				stop("Singular matrix!", null);
				return;
			}
		}
	}
        }
    }
}
