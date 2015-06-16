using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO; // File: for BinaryWriter BinaryReader

namespace Spice
{
    public partial class Main : Form
    {
        int toolBarHeight = 50;
        int statusBarHeight = 22;
        int gridSize = 20;

        List<CircuitElm> elmList = new List<CircuitElm>();
        List<CircuitNode> nodeList;
        CircuitElm[] voltageSources;
        CircuitElm stopElm;
        bool circuitNonLinear, circuitNeedsMap, analyzeFlag;
        int voltageSourceCount;
        int[] circuitPermute;
        string stopMessage;
        public double t, timeStep = 5e-6;

        double[][] circuitMatrix, origMatrix;
        double[] circuitRightSide, origRightSide;
        int circuitMatrixSize, circuitMatrixFullSize;
        RowInfo[] circuitRowInfo;

        Point temppoint;
        bool _mousePressed;

        Scope vScope, iScope;

        char tool = 'w';

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
        }

        long lastTime = 0, lastFrameTime, lastIterTime, secTime = 0;
        int frames = 0;
        int steps = 0;
        int framerate = 0, steprate = 0;

        private void Main_Paint(object sender, PaintEventArgs e)
        {
            if (analyzeFlag)
            {
                analyze();
                analyzeFlag = false;
            }

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

            if (stopMessage != null)
            {
                screen.DrawString(stopMessage, new Font("Arial", 16), new SolidBrush(myPen.Color), new Point(100, 500));
            }

            runCircuit();

            textBox1.Text = "";

            foreach (CircuitElm elm in elmList)
            {
                textBox1.Text += elm.getCurrent().ToString() + " ";
            }

            if (iScope != null && vScope != null)
            {
                iScope.step(1);
                iScope.draw(screen);
                vScope.step(0);
                vScope.draw(screen);
            }
        }

        private void Frametime_Tick(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private double getIterCount() { return .1 * Math.Exp((100 - 61) / 24); }

        private void analyze()
        {
            if (elmList.Count == 0) return;

            stopMessage = null;
            stopElm = null;
            int i, j;
            int vscount = 0;
            nodeList = new List<CircuitNode>();
            bool gotGround = false;
            //bool gotRail = false;
            CircuitElm volt = null;

            for (i = 0; i != elmList.Count; i++)
            {
                if (elmList[i].type == 'g')
                {
                    gotGround = true;
                    break;
                }

                if (volt == null && elmList[i].type == 'v') volt = elmList[i];
            }

            if (!gotGround && volt != null)
            {
                CircuitNode cn = new CircuitNode();
                Point pt = volt.pt1;
                cn.x = pt.X;
                cn.y = pt.Y;
                nodeList.Add(cn);
            }
            else
            {
                // otherwise allocate extra node for ground
                CircuitNode cn = new CircuitNode();
                cn.x = cn.y = -1;
                nodeList.Add(cn);
            }

            for (i = 0; i != elmList.Count; i++)
            {
                CircuitElm ce = elmList[i];
                int inodes = ce.getInternalNodeCount();
                int ivs = ce.getVoltageSourceCount();
                int posts = ce.getPostCount();

                // allocate a node for each post and match posts to nodes
                for (j = 0; j != posts; j++)
                {
                    Point pt = ce.getPost(j);
                    int k;
                    for (k = 0; k != nodeList.Count; k++)
                    {
                        CircuitNode cn = nodeList[k];
                        if (pt.X == cn.x && pt.Y == cn.y)
                            break;
                    }
                    if (k == nodeList.Count)
                    {
                        CircuitNode cn = new CircuitNode();
                        cn.x = pt.X;
                        cn.y = pt.Y;
                        CircuitNodeLink cnl = new CircuitNodeLink();
                        cnl.num = j;
                        cnl.elm = ce;
                        cn.links.Add(cnl);
                        ce.setNode(j, nodeList.Count);
                        nodeList.Add(cn);
                    }
                    else
                    {
                        CircuitNodeLink cnl = new CircuitNodeLink();
                        cnl.num = j;
                        cnl.elm = ce;
                        nodeList[k].links.Add(cnl);
                        ce.setNode(j, k);
                        // if it's the ground node, make sure the node voltage is 0,
                        // cause it may not get set later
                        if (k == 0)
                            ce.setNodeVoltage(j, 0);
                    }
                }
                for (j = 0; j != inodes; j++)
                {
                    CircuitNode cn = new CircuitNode();
                    cn.x = cn.y = -1;
                    cn.isInternal = true;
                    CircuitNodeLink cnl = new CircuitNodeLink();
                    cnl.num = j + posts;
                    cnl.elm = ce;
                    cn.links.Add(cnl);
                    ce.setNode(cnl.num, nodeList.Count);
                    nodeList.Add(cn);
                }
                vscount += ivs;
            }

            voltageSources = new CircuitElm[vscount];
            vscount = 0;
            circuitNonLinear = false;

            for (i = 0; i != elmList.Count; i++)
            {
                CircuitElm ce = elmList[i];
                if (ce.nonLinear()) circuitNonLinear = true;
                int ivs = ce.getVoltageSourceCount();
                for (j = 0; j != ivs; j++)
                {
                    voltageSources[vscount] = ce;
                    ce.setVoltageSource(j, vscount++);
                }
            }
            voltageSourceCount = vscount;

            int matrixSize = nodeList.Count - 1 + vscount;
            circuitMatrix = new double[matrixSize][];
            for (int z = 0; z < matrixSize; z++)
                circuitMatrix[z] = new double[matrixSize];

            circuitRightSide = new double[matrixSize];
            origMatrix = new double[matrixSize][];
            for (int z = 0; z < matrixSize; z++)
                origMatrix[z] = new double[matrixSize];

            origRightSide = new double[matrixSize];
            circuitMatrixSize = circuitMatrixFullSize = matrixSize;
            circuitRowInfo = new RowInfo[matrixSize];
            circuitPermute = new int[matrixSize];
            int vs = 0;
            for (i = 0; i != matrixSize; i++)
                circuitRowInfo[i] = new RowInfo();
            circuitNeedsMap = false;

            for (i = 0; i != elmList.Count; i++)
            {
                CircuitElm ce = elmList[i];
                ce.stamp(this);
            }

            bool[] closure = new bool[nodeList.Count];
            bool[] tempclosure = new bool[nodeList.Count];
            bool changed = true;
            closure[0] = true;
            while (changed)
            {
                changed = false;
                for (i = 0; i != elmList.Count; i++)
                {
                    CircuitElm ce = elmList[i];
                    // loop through all ce's nodes to see if they are connected
                    // to other nodes not in closure
                    for (j = 0; j < ce.getPostCount(); j++)
                    {
                        if (!closure[ce.getNode(j)])
                        {
                            if (ce.hasGroundConnection(j))
                                closure[ce.getNode(j)] = changed = true;
                            continue;
                        }
                        int k;
                        for (k = 0; k != ce.getPostCount(); k++)
                        {
                            if (j == k)
                                continue;
                            int kn = ce.getNode(k);
                            if (ce.getConnection(j, k) && !closure[kn])
                            {
                                closure[kn] = true;
                                changed = true;
                            }
                        }
                    }
                }
                if (changed)
                    continue;

                // connect unconnected nodes
                for (i = 0; i != nodeList.Count; i++)
                {
                    if (!closure[i] && !nodeList[i].isInternal)
                    {
                        Debug.WriteLine("node " + i + " unconnected");
                        stampResistor(0, i, 1e8);
                        closure[i] = true;
                        changed = true;
                        break;
                    }
                }
            }

            for (i = 0; i != elmList.Count; i++)
            {
                CircuitElm ce = elmList[i];
                // look for inductors with no current path
                if (ce.type == 'i')
                {
                    FindPathInfo fpi = new FindPathInfo(this, FindPathInfo.INDUCT, ce, ce.getNode(1));
                    // first try findPath with maximum depth of 5, to avoid slowdowns
                    if (!fpi.findPath(ce.getNode(0), 5) &&
                            !fpi.findPath(ce.getNode(0)))
                    {
                        Debug.WriteLine(ce + " no path");
                        ce.reset();
                    }
                }
                // look for current sources with no current path
                if (ce.type == 'c')
                {
                    FindPathInfo fpi = new FindPathInfo(this, FindPathInfo.INDUCT, ce, ce.getNode(1));
                    if (!fpi.findPath(ce.getNode(0)))
                    {
                        stop("No path for current source!", ce);
                        return;
                    }
                }
                // look for voltage source loops
                if ((ce.type == 'v' && ce.getPostCount() == 2) || ce.type == 'w')
                {
                    FindPathInfo fpi = new FindPathInfo(this, FindPathInfo.VOLTAGE, ce, ce.getNode(1));
                    if (fpi.findPath(ce.getNode(0)))
                    {
                        stop("Voltage source/wire loop with no resistance!", ce);
                        return;
                    }
                }
                // look for shorted caps, or caps w/ voltage but no R
                if (ce.type == 'C')
                {
                    FindPathInfo fpi = new FindPathInfo(this, FindPathInfo.SHORT, ce, ce.getNode(1));
                    if (fpi.findPath(ce.getNode(0)))
                    {
                        Debug.WriteLine(ce + " shorted");
                        ce.reset();
                    }
                    else
                    {
                        fpi = new FindPathInfo(this, FindPathInfo.CAP_V, ce, ce.getNode(1));
                        if (fpi.findPath(ce.getNode(0)))
                        {
                            stop("Capacitor loop with no resistance!", ce);
                            return;
                        }
                    }
                }
            }
            // simplify the matrix; this speeds things up quite a bit
            for (i = 0; i != matrixSize; i++)
            {
                int qm = -1, qp = -1;
                double qv = 0;
                RowInfo re = circuitRowInfo[i];
                /*System.out.println("row " + i + " " + re.lsChanges + " " + re.rsChanges + " " +
                    re.dropRow);*/
                if (re.lsChanges || re.dropRow || re.rsChanges)
                    continue;
                double rsadd = 0;

                // look for rows that can be removed
                for (j = 0; j != matrixSize; j++)
                {
                    double q = circuitMatrix[i][j];
                    if (circuitRowInfo[j].type == RowInfo.ROW_CONST)
                    {
                        // keep a running total of const values that have been
                        // removed already
                        rsadd -= circuitRowInfo[j].value * q;
                        continue;
                    }
                    if (q == 0)
                        continue;
                    if (qp == -1)
                    {
                        qp = j;
                        qv = q;
                        continue;
                    }
                    if (qm == -1 && q == -qv)
                    {
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
                if (j == matrixSize)
                {
                    if (qp == -1)
                    {
                        stop("Matrix error", null);
                        return;
                    }
                    RowInfo elt = circuitRowInfo[qp];
                    if (qm == -1)
                    {
                        // we found a row with only one nonzero entry; that value
                        // is a constant
                        int k;
                        for (k = 0; elt.type == RowInfo.ROW_EQUAL && k < 100; k++)
                        {
                            // follow the chain
                            /*System.out.println("following equal chain from " +
                        i + " " + qp + " to " + elt.nodeEq);*/
                            qp = elt.nodeEq;
                            elt = circuitRowInfo[qp];
                        }
                        if (elt.type == RowInfo.ROW_EQUAL)
                        {
                            // break equal chains
                            //System.out.println("Break equal chain");
                            elt.type = RowInfo.ROW_NORMAL;
                            continue;
                        }
                        if (elt.type != RowInfo.ROW_NORMAL)
                        {
                            Debug.WriteLine("type already " + elt.type + " for " + qp + "!");
                            continue;
                        }
                        elt.type = RowInfo.ROW_CONST;
                        elt.value = (circuitRightSide[i] + rsadd) / qv;
                        circuitRowInfo[i].dropRow = true;
                        //System.out.println(qp + " * " + qv + " = const " + elt.value);
                        i = -1; // start over from scratch
                    }
                    else if (circuitRightSide[i] + rsadd == 0)
                    {
                        // we found a row with only two nonzero entries, and one
                        // is the negative of the other; the values are equal
                        if (elt.type != RowInfo.ROW_NORMAL)
                        {
                            //System.out.println("swapping");
                            int qq = qm;
                            qm = qp; qp = qq;
                            elt = circuitRowInfo[qp];
                            if (elt.type != RowInfo.ROW_NORMAL)
                            {
                                // we should follow the chain here, but this
                                // hardly ever happens so it's not worth worrying
                                // about
                                Debug.WriteLine("swap failed");
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


            // find size of new matrix
            int nn = 0;
            for (i = 0; i != matrixSize; i++)
            {
                RowInfo elt = circuitRowInfo[i];
                if (elt.type == RowInfo.ROW_NORMAL)
                {
                    elt.mapCol = nn++;
                    //System.out.println("col " + i + " maps to " + elt.mapCol);
                    continue;
                }
                if (elt.type == RowInfo.ROW_EQUAL)
                {
                    RowInfo e2 = null;
                    // resolve chains of equality; 100 max steps to avoid loops
                    for (j = 0; j != 100; j++)
                    {
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
            for (i = 0; i != matrixSize; i++)
            {
                RowInfo elt = circuitRowInfo[i];
                if (elt.type == RowInfo.ROW_EQUAL)
                {
                    RowInfo e2 = circuitRowInfo[elt.nodeEq];
                    if (e2.type == RowInfo.ROW_CONST)
                    {
                        // if something is equal to a const, it's a const
                        elt.type = e2.type;
                        elt.value = e2.value;
                        elt.mapCol = -1;
                        //System.out.println(i + " = [late]const " + elt.value);
                    }
                    else
                    {
                        elt.mapCol = e2.mapCol;
                        //System.out.println(i + " maps to: " + e2.mapCol);
                    }
                }
            }

            // make the new, simplified matrix
            int newsize = nn;
            double[][] newmatx = new double[newsize][];
            for (int z = 0; z < newsize; z++)
                newmatx[z] = new double[newsize];

            double[] newrs = new double[newsize];
            int ii = 0;
            for (i = 0; i != matrixSize; i++)
            {
                RowInfo rri = circuitRowInfo[i];
                if (rri.dropRow)
                {
                    rri.mapRow = -1;
                    continue;
                }
                newrs[ii] = circuitRightSide[i];
                rri.mapRow = ii;
                //System.out.println("Row " + i + " maps to " + ii);
                for (j = 0; j != matrixSize; j++)
                {
                    RowInfo ri = circuitRowInfo[j];
                    if (ri.type == RowInfo.ROW_CONST)
                        newrs[ii] -= ri.value * circuitMatrix[i][j];
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
            if (!circuitNonLinear)
            {
                if (!lu_factor(circuitMatrix, circuitMatrixSize, circuitPermute))
                {
                    stop("Singular matrix!", null);
                    return;
                }
            }
        }

        bool converged;
        int subIterations;
        void runCircuit()
        {
            if (circuitMatrix == null || elmList.Count == 0)
            {
                circuitMatrix = null;
                return;
            }
            int iter;
            //int maxIter = getIterCount();
            //bool debugprint = dumpMatrix;
            //dumpMatrix = false;
            long steprate = (long)(160 * getIterCount());
            long tm = Environment.TickCount;
            long lit = lastIterTime;
            if (1000 >= steprate * (tm - lastIterTime))
                return;
            for (iter = 1; ; iter++)
            {
                int i, j, k, subiter;
                for (i = 0; i != elmList.Count; i++)
                {
                    CircuitElm ce = elmList[i];
                    ce.startIteration();
                }
                steps++;
                int subiterCount = 5000;
                for (subiter = 0; subiter != subiterCount; subiter++)
                {
                    converged = true;
                    subIterations = subiter;
                    for (i = 0; i != circuitMatrixSize; i++)
                        circuitRightSide[i] = origRightSide[i];
                    if (circuitNonLinear)
                    {
                        for (i = 0; i != circuitMatrixSize; i++)
                            for (j = 0; j != circuitMatrixSize; j++)
                                circuitMatrix[i][j] = origMatrix[i][j];
                    }
                    for (i = 0; i != elmList.Count; i++)
                    {
                        CircuitElm ce = elmList[i];
                        ce.doStep(this);
                    }
                    if (stopMessage != null)
                        return;
                    //boolean printit = debugprint;
                    //debugprint = false;
                    for (j = 0; j != circuitMatrixSize; j++)
                    {
                        for (i = 0; i != circuitMatrixSize; i++)
                        {
                            double x = circuitMatrix[i][j];
                            if (Double.IsNaN(x) || Double.IsInfinity(x))
                            {
                                stop("nan/infinite matrix!", null);
                                return;
                            }
                        }
                    }
                    //if (printit) {
                    //    for (j = 0; j != circuitMatrixSize; j++) {
                    //    for (i = 0; i != circuitMatrixSize; i++)
                    //        System.out.print(circuitMatrix[j][i] + ",");
                    //    System.out.print("  " + circuitRightSide[j] + "\n");
                    //    }
                    //    System.out.print("\n");
                    //}
                    if (circuitNonLinear)
                    {
                        if (converged && subiter > 0)
                            break;
                        if (!lu_factor(circuitMatrix, circuitMatrixSize,
                              circuitPermute))
                        {
                            stop("Singular matrix!", null);
                            return;
                        }
                    }
                    lu_solve(circuitMatrix, circuitMatrixSize, circuitPermute,
                         circuitRightSide);

                    for (j = 0; j != circuitMatrixFullSize; j++)
                    {
                        RowInfo ri = circuitRowInfo[j];
                        double res = 0;
                        if (ri.type == RowInfo.ROW_CONST)
                            res = ri.value;
                        else
                            res = circuitRightSide[ri.mapCol];
                        /*System.out.println(j + " " + res + " " +
                          ri.type + " " + ri.mapCol);*/
                        if (Double.IsNaN(res))
                        {
                            converged = false;
                            //debugprint = true;
                            break;
                        }
                        if (j < nodeList.Count - 1)
                        {
                            CircuitNode cn = nodeList[j + 1];
                            for (k = 0; k != cn.links.Count; k++)
                            {
                                CircuitNodeLink cnl = (CircuitNodeLink)
                                cn.links.ElementAt(k);
                                cnl.elm.setNodeVoltage(cnl.num, res);
                            }
                        }
                        else
                        {
                            int ji = j - (nodeList.Count - 1);
                            //System.out.println("setting vsrc " + ji + " to " + res);
                            voltageSources[ji].setCurrent(ji, res);
                        }
                    }
                    if (!circuitNonLinear)
                        break;
                }
                if (subiter > 5)
                    Debug.Write("converged after " + subiter + " iterations\n");
                if (subiter == subiterCount)
                {
                    stop("Convergence failed!", null);
                    break;
                }
                t += timeStep;
                //for (i = 0; i != scopeCount; i++)
                //scopes[i].timeStep();
                tm = Environment.TickCount;
                lit = tm;
                if (iter * 1000 >= steprate * (tm - lastIterTime) ||
                (tm - lastFrameTime > 500))
                    break;
            }
            lastIterTime = lit;
            //System.out.println((System.currentTimeMillis()-lastFrameTime)/(double) iter);
        }

        #region //// Stamping ////

        // control voltage source vs with voltage from n1 to n2 (must
        // also call stampVoltageSource())
        public void stampVCVS(int n1, int n2, double coef, int vs)
        {
            int vn = nodeList.Count + vs;
            stampMatrix(vn, n1, coef);
            stampMatrix(vn, n2, -coef);
        }

        // stamp independent voltage source #vs, from n1 to n2, amount v
        public void stampVoltageSource(int n1, int n2, int vs, double v)
        {
            int vn = nodeList.Count + vs;
            stampMatrix(vn, n1, -1);
            stampMatrix(vn, n2, 1);
            stampRightSide(vn, v);
            stampMatrix(n1, vn, 1);
            stampMatrix(n2, vn, -1);
        }

        // use this if the amount of voltage is going to be updated in doStep()
        public void stampVoltageSource(int n1, int n2, int vs)
        {
            int vn = nodeList.Count + vs;
            stampMatrix(vn, n1, -1);
            stampMatrix(vn, n2, 1);
            stampRightSide(vn);
            stampMatrix(n1, vn, 1);
            stampMatrix(n2, vn, -1);
        }

        public void updateVoltageSource(int n1, int n2, int vs, double v)
        {
            int vn = nodeList.Count + vs;
            stampRightSide(vn, v);
        }

        public void stampResistor(int n1, int n2, double r)
        {
            double r0 = 1 / r;
            if (Double.IsNaN(r0) || Double.IsInfinity(r0))
            {
                Debug.WriteLine("bad resistance " + r + " " + r0 + "\n");
                int a = 0;
                a /= a;
            }
            stampMatrix(n1, n1, r0);
            stampMatrix(n2, n2, r0);
            stampMatrix(n1, n2, -r0);
            stampMatrix(n2, n1, -r0);
        }

        public void stampConductance(int n1, int n2, double r0)
        {
            stampMatrix(n1, n1, r0);
            stampMatrix(n2, n2, r0);
            stampMatrix(n1, n2, -r0);
            stampMatrix(n2, n1, -r0);
        }

        // current from cn1 to cn2 is equal to voltage from vn1 to 2, divided by g
        public void stampVCCurrentSource(int cn1, int cn2, int vn1, int vn2, double g)
        {
            stampMatrix(cn1, vn1, g);
            stampMatrix(cn2, vn2, g);
            stampMatrix(cn1, vn2, -g);
            stampMatrix(cn2, vn1, -g);
        }

        public void stampCurrentSource(int n1, int n2, double i)
        {
            stampRightSide(n1, -i);
            stampRightSide(n2, i);
        }

        // stamp a current source from n1 to n2 depending on current through vs
        public void stampCCCS(int n1, int n2, int vs, double gain)
        {
            int vn = nodeList.Count + vs;
            stampMatrix(n1, vn, gain);
            stampMatrix(n2, vn, -gain);
        }

        // stamp value x in row i, column j, meaning that a voltage change
        // of dv in node j will increase the current into node i by x dv.
        // (Unless i or j is a voltage source node.)
        public void stampMatrix(int i, int j, double x)
        {
            if (i > 0 && j > 0)
            {
                if (circuitNeedsMap)
                {
                    i = circuitRowInfo[i - 1].mapRow;
                    RowInfo ri = circuitRowInfo[j - 1];
                    if (ri.type == RowInfo.ROW_CONST)
                    {
                        //System.out.println("Stamping constant " + i + " " + j + " " + x);
                        circuitRightSide[i] -= x * ri.value;
                        return;
                    }
                    j = ri.mapCol;
                    //System.out.println("stamping " + i + " " + j + " " + x);
                }
                else
                {
                    i--;
                    j--;
                }
                circuitMatrix[i][j] += x;
            }
        }

        // stamp value x on the right side of row i, representing an
        // independent current source flowing into node i
        public void stampRightSide(int i, double x)
        {
            if (i > 0)
            {
                if (circuitNeedsMap)
                {
                    i = circuitRowInfo[i - 1].mapRow;
                    //System.out.println("stamping " + i + " " + x);
                }
                else
                    i--;
                circuitRightSide[i] += x;
            }
        }

        // indicate that the value on the right side of row i changes in doStep()
        public void stampRightSide(int i)
        {
            //System.out.println("rschanges true " + (i-1));
            if (i > 0)
                circuitRowInfo[i - 1].rsChanges = true;
        }

        // indicate that the values on the left side of row i change in doStep()
        public void stampNonLinear(int i)
        {
            if (i > 0)
                circuitRowInfo[i - 1].lsChanges = true;
        }

        #endregion

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            #region //// Old Code ////

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

        private void needAnalyze() { analyzeFlag = true; }

        #region //// Mouse operations ////

        private void Main_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                for (int i = 0; i < elmList.Count; i++)
                {
                    if (elmList[i].checkBound(PointToClient(System.Windows.Forms.Cursor.Position)))
                    {
                        if ((ModifierKeys & Keys.Control) == Keys.Control)
                        {
                            vScope = new Scope(elmList[i], new Rectangle(40, 400, 500, 50));
                            iScope = new Scope(elmList[i], new Rectangle(40, 470, 500, 50));
                        }
                        else
                        {
                            elmList.RemoveAt(i);
                            needAnalyze();
                            return;
                        }
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
                        PropertyEditor pe = new PropertyEditor(elmList[i]);
                        pe.ShowDialog();
                        needAnalyze();
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

            elmList.RemoveAll(item => item.pt1 == item.pt2);
            needAnalyze();
        }

        private void Main_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mousePressed == true)
            {
                elmList[elmList.Count - 1].pt2 = PointToClient(System.Windows.Forms.Cursor.Position);
                elmList[elmList.Count - 1].round(gridSize);
                toolStripStatusLabel1.Text = elmList[elmList.Count - 1].pt2.ToString();
                needAnalyze();
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

        #endregion

        #region //// Tool Selection ////

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

        private void capacitorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tool = 'C';
            toolStripStatusLabel3.Text = "Capacitor";
        }

        #endregion

        #region //// File operations ////

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

            // open File
            StreamWriter streamOut = new StreamWriter(outFile);
            // write File
            // for loop to save one by one
            for (int i = 0; i < elmList.Count; i++)
            {
                streamOut.WriteLine(elmList[i].saveDump());
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

                elmList.Clear();

                for (int i = 0; i < filelines.Length; i++)
                {
                    // produce array
                    string[] splitLines = filelines[i].Split(' ');
                    // to check and construct constructor
                    // there're two constructor: one has 5 parameter and the other one has 6 parameter.
                    if (splitLines.Length == 5) { elmList.Add(new CircuitElm(splitLines[0][0], Convert.ToInt32(splitLines[1]), Convert.ToInt32(splitLines[2]), Convert.ToInt32(splitLines[3]), Convert.ToInt32(splitLines[4]))); }
                    if (splitLines.Length == 6) { elmList.Add(new CircuitElm(splitLines[0][0], Convert.ToInt32(splitLines[1]), Convert.ToInt32(splitLines[2]), Convert.ToInt32(splitLines[3]), Convert.ToInt32(splitLines[4]), (float)Convert.ToDouble(splitLines[5]))); }
                }

                needAnalyze();
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            elmList.Clear();
        }

        #endregion

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stopToolStripMenuItem.Checked = !stopToolStripMenuItem.Checked;
        }

        bool lu_factor(double[][] a, int n, int[] ipvt)
        {
            double[] scaleFactors;
            int i, j, k;

            scaleFactors = new double[n];

            // divide each row by its largest element, keeping track of the
            // scaling factors
            for (i = 0; i != n; i++)
            {
                double largest = 0;
                for (j = 0; j != n; j++)
                {
                    double x = Math.Abs(a[i][j]);
                    if (x > largest)
                        largest = x;
                }
                // if all zeros, it's a singular matrix
                if (largest == 0)
                    return false;
                scaleFactors[i] = 1.0 / largest;
            }

            // use Crout's method; loop through the columns
            for (j = 0; j != n; j++)
            {

                // calculate upper triangular elements for this column
                for (i = 0; i != j; i++)
                {
                    double q = a[i][j];
                    for (k = 0; k != i; k++)
                        q -= a[i][k] * a[k][j];
                    a[i][j] = q;
                }

                // calculate lower triangular elements for this column
                double largest = 0;
                int largestRow = -1;
                for (i = j; i != n; i++)
                {
                    double q = a[i][j];
                    for (k = 0; k != j; k++)
                        q -= a[i][k] * a[k][j];
                    a[i][j] = q;
                    double x = Math.Abs(q);
                    if (x >= largest)
                    {
                        largest = x;
                        largestRow = i;
                    }
                }

                // pivoting
                if (j != largestRow)
                {
                    double x;
                    for (k = 0; k != n; k++)
                    {
                        x = a[largestRow][k];
                        a[largestRow][k] = a[j][k];
                        a[j][k] = x;
                    }
                    scaleFactors[largestRow] = scaleFactors[j];
                }

                // keep track of row interchanges
                ipvt[j] = largestRow;

                // avoid zeros
                if (a[j][j] == 0.0)
                {
                    Debug.WriteLine("avoided zero");
                    a[j][j] = 1e-18;
                }

                if (j != n - 1)
                {
                    double mult = 1.0 / a[j][j];
                    for (i = j + 1; i != n; i++)
                        a[i][j] *= mult;
                }
            }
            return true;
        }

        void lu_solve(double[][] a, int n, int[] ipvt, double[] b)
        {
            int i;

            // find first nonzero b element
            for (i = 0; i != n; i++)
            {
                int row = ipvt[i];

                double swap = b[row];
                b[row] = b[i];
                b[i] = swap;
                if (swap != 0)
                    break;
            }

            int bi = i++;
            for (; i < n; i++)
            {
                int row = ipvt[i];
                int j;
                double tot = b[row];

                b[row] = b[i];
                // forward substitution using the lower triangular matrix
                for (j = bi; j < i; j++)
                    tot -= a[i][j] * b[j];
                b[i] = tot;
            }
            for (i = n - 1; i >= 0; i--)
            {
                double tot = b[i];

                // back-substitution using the upper triangular matrix
                int j;
                for (j = i + 1; j != n; j++)
                    tot -= a[i][j] * b[j];
                b[i] = tot / a[i][i];
            }
        }

        void stop(String s, CircuitElm ce)
        {
            stopMessage = s;
            circuitMatrix = null;
            stopElm = ce;
            Debug.WriteLine(s);
            stopToolStripMenuItem.Checked = true;
            analyzeFlag = false;
            //cv.repaint();
        }

        class FindPathInfo
        {
            public static readonly int INDUCT = 1;
            public static readonly int VOLTAGE = 2;
            public static readonly int SHORT = 3;
            public static readonly int CAP_V = 4;
            public bool[] used;
            public int dest;
            public CircuitElm firstElm;
            public Main sim;

            public int type;
            public FindPathInfo(Main r, int t, CircuitElm e, int d)
            {
                sim = r;
                dest = d;
                type = t;
                firstElm = e;
                used = new bool[sim.nodeList.Count];
            }
            public bool findPath(int n1) { return findPath(n1, -1); }
            public bool findPath(int n1, int depth)
            {
                if (n1 == dest)
                    return true;
                if (depth-- == 0)
                    return false;
                if (used[n1])
                {
                    //System.out.println("used " + n1);
                    return false;
                }

                used[n1] = true;
                int i;
                for (i = 0; i != sim.elmList.Count; i++)
                {
                    CircuitElm ce = sim.elmList[i];
                    if (ce == firstElm)
                        continue;
                    if (type == INDUCT)
                    {
                        if (ce.type == 'c')
                            continue;
                    }
                    if (type == VOLTAGE)
                    {
                        if (!(ce.isWire() || ce.type == 'v'))
                            continue;
                    }
                    if (type == SHORT && !ce.isWire())
                        continue;
                    if (type == CAP_V)
                    {
                        if (!(ce.isWire() || ce.type == 'C' || ce.type == 'v'))
                            continue;
                    }
                    if (n1 == 0)
                    {
                        // look for posts which have a ground connection;
                        // our path can go through ground
                        int z;
                        for (z = 0; z != ce.getPostCount(); z++)
                            if (ce.hasGroundConnection(z) && findPath(ce.getNode(z), depth))
                            {
                                used[n1] = false;
                                return true;
                            }
                    }
                    int j;
                    for (j = 0; j != ce.getPostCount(); j++)
                    {
                        //System.out.println(ce + " " + ce.getNode(j));
                        if (ce.getNode(j) == n1)
                            break;
                    }
                    if (j == ce.getPostCount())
                        continue;
                    if (ce.hasGroundConnection(j) && findPath(0, depth))
                    {
                        //System.out.println(ce + " has ground");
                        used[n1] = false;
                        return true;
                    }
                    if (type == INDUCT && ce.type == 'i')
                    {
                        double c = ce.getCurrent();
                        if (j == 0)
                            c = -c;
                        //System.out.println("matching " + c + " to " + firstElm.getCurrent());
                        //System.out.println(ce + " " + firstElm);
                        if (Math.Abs(c - firstElm.getCurrent()) > 1e-10)
                            continue;
                    }
                    int k;
                    for (k = 0; k != ce.getPostCount(); k++)
                    {
                        if (j == k)
                            continue;
                        //System.out.println(ce + " " + ce.getNode(j) + "-" + ce.getNode(k));
                        if (ce.getConnection(j, k) && findPath(ce.getNode(k), depth))
                        {
                            //System.out.println("got findpath " + n1);
                            used[n1] = false;
                            return true;
                        }
                        //System.out.println("back on findpath " + n1);
                    }
                }
                used[n1] = false;
                //System.out.println(n1 + " failed");
                return false;
            }
        }
    }
}
