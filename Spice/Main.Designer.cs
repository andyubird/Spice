﻿namespace Spice
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.wireToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resistorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dCVoltageSourceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.capacitorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.inductorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.optionsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.stoppedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.Frametime = new System.Windows.Forms.Timer(this.components);
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.squareWaveGenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(984, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DoubleClickEnabled = true;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wireToolStripMenuItem,
            this.resistorToolStripMenuItem,
            this.dCVoltageSourceToolStripMenuItem,
            this.groundToolStripMenuItem,
            this.capacitorToolStripMenuItem,
            this.inductorToolStripMenuItem,
            this.squareWaveGenToolStripMenuItem});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(89, 22);
            this.toolStripDropDownButton1.Text = "Components";
            // 
            // wireToolStripMenuItem
            // 
            this.wireToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("wireToolStripMenuItem.Image")));
            this.wireToolStripMenuItem.Name = "wireToolStripMenuItem";
            this.wireToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.wireToolStripMenuItem.Text = "Wire";
            this.wireToolStripMenuItem.Click += new System.EventHandler(this.wireToolStripMenuItem_Click);
            // 
            // resistorToolStripMenuItem
            // 
            this.resistorToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("resistorToolStripMenuItem.Image")));
            this.resistorToolStripMenuItem.Name = "resistorToolStripMenuItem";
            this.resistorToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.resistorToolStripMenuItem.Text = "Resistor";
            this.resistorToolStripMenuItem.Click += new System.EventHandler(this.resistorToolStripMenuItem_Click);
            // 
            // dCVoltageSourceToolStripMenuItem
            // 
            this.dCVoltageSourceToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("dCVoltageSourceToolStripMenuItem.Image")));
            this.dCVoltageSourceToolStripMenuItem.Name = "dCVoltageSourceToolStripMenuItem";
            this.dCVoltageSourceToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.dCVoltageSourceToolStripMenuItem.Text = "DC Voltage Source";
            this.dCVoltageSourceToolStripMenuItem.Click += new System.EventHandler(this.dCVoltageSourceToolStripMenuItem_Click);
            // 
            // groundToolStripMenuItem
            // 
            this.groundToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("groundToolStripMenuItem.Image")));
            this.groundToolStripMenuItem.Name = "groundToolStripMenuItem";
            this.groundToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.groundToolStripMenuItem.Text = "Ground";
            this.groundToolStripMenuItem.Click += new System.EventHandler(this.groundToolStripMenuItem_Click);
            // 
            // capacitorToolStripMenuItem
            // 
            this.capacitorToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("capacitorToolStripMenuItem.Image")));
            this.capacitorToolStripMenuItem.Name = "capacitorToolStripMenuItem";
            this.capacitorToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.capacitorToolStripMenuItem.Text = "Capacitor";
            this.capacitorToolStripMenuItem.Click += new System.EventHandler(this.capacitorToolStripMenuItem_Click);
            // 
            // inductorToolStripMenuItem
            // 
            this.inductorToolStripMenuItem.Name = "inductorToolStripMenuItem";
            this.inductorToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.inductorToolStripMenuItem.Text = "Inductor";
            this.inductorToolStripMenuItem.Click += new System.EventHandler(this.inductorToolStripMenuItem_Click);
            // 
            // filesToolStripMenuItem
            // 
            this.filesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.filesToolStripMenuItem.Name = "filesToolStripMenuItem";
            this.filesToolStripMenuItem.Size = new System.Drawing.Size(42, 20);
            this.filesToolStripMenuItem.Text = "Files";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripMenuItem.Image")));
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.newToolStripMenuItem.Text = "New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripMenuItem.Image")));
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("exitToolStripMenuItem.Image")));
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.filesToolStripMenuItem,
            this.optionsToolStripMenuItem1,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(984, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // optionsToolStripMenuItem1
            // 
            this.optionsToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stoppedToolStripMenuItem,
            this.debugModeToolStripMenuItem,
            this.resetToolStripMenuItem});
            this.optionsToolStripMenuItem1.Name = "optionsToolStripMenuItem1";
            this.optionsToolStripMenuItem1.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem1.Text = "Options";
            // 
            // stoppedToolStripMenuItem
            // 
            this.stoppedToolStripMenuItem.Name = "stoppedToolStripMenuItem";
            this.stoppedToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.stoppedToolStripMenuItem.Text = "Stopped";
            this.stoppedToolStripMenuItem.Click += new System.EventHandler(this.stoppedToolStripMenuItem_Click);
            // 
            // debugModeToolStripMenuItem
            // 
            this.debugModeToolStripMenuItem.Checked = true;
            this.debugModeToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.debugModeToolStripMenuItem.Name = "debugModeToolStripMenuItem";
            this.debugModeToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.debugModeToolStripMenuItem.Text = "Debug mode";
            this.debugModeToolStripMenuItem.Click += new System.EventHandler(this.debugModeToolStripMenuItem_Click);
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.resetToolStripMenuItem.Text = "Reset";
            this.resetToolStripMenuItem.Click += new System.EventHandler(this.resetToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel3,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 540);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(984, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(851, 17);
            this.toolStripStatusLabel2.Spring = true;
            this.toolStripStatusLabel2.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(118, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // Frametime
            // 
            this.Frametime.Enabled = true;
            this.Frametime.Interval = 10;
            this.Frametime.Tick += new System.EventHandler(this.Frametime_Tick);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(820, 65);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(152, 461);
            this.textBox1.TabIndex = 3;
            // 
            // squareWaveGenToolStripMenuItem
            // 
            this.squareWaveGenToolStripMenuItem.Name = "squareWaveGenToolStripMenuItem";
            this.squareWaveGenToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.squareWaveGenToolStripMenuItem.Text = "Square Wave Gen.";
            this.squareWaveGenToolStripMenuItem.Click += new System.EventHandler(this.squareWaveGenToolStripMenuItem_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 562);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Main";
            this.Text = "Simple Circuit Simulator";
            this.Load += new System.EventHandler(this.Main_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Main_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Main_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Main_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Main_MouseUp);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripMenuItem filesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Timer Frametime;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem wireToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resistorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dCVoltageSourceToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripMenuItem groundToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ToolStripMenuItem capacitorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem stoppedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem inductorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem squareWaveGenToolStripMenuItem;

    }
}