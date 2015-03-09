////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;

namespace CLRProfiler
{
    /// <summary>
    /// Summary description for TimeLineViewForm.
    /// </summary>
    public class TimeLineViewForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.RadioButton twoKradioButton;
        private System.Windows.Forms.RadioButton oneKradioButton;
        private System.Windows.Forms.RadioButton oneMSradioButton;
        private System.Windows.Forms.RadioButton twoMSradioButton;
        private System.Windows.Forms.RadioButton fiveMSradioButton;
        private System.Windows.Forms.RadioButton tenMSradioButton;
        private System.Windows.Forms.RadioButton twentyMSradioButton;
        private System.Windows.Forms.RadioButton fiftyMSradioButton;
        private System.Windows.Forms.RadioButton oneHundredMSradioButton;
        private System.Windows.Forms.RadioButton twoHundredMSradioButton;
        private System.Windows.Forms.RadioButton fiveHundredMSradioButton;
        private System.Windows.Forms.RadioButton oneThousandMSradioButton;
        private System.Windows.Forms.RadioButton oneHundredKradioButton;
        private System.Windows.Forms.RadioButton fiftyKradioButton;
        private System.Windows.Forms.RadioButton tenKradioButton;
        private System.Windows.Forms.RadioButton fiveKradioButton;
        private System.Windows.Forms.RadioButton twoHundredKradioButton;
        private System.Windows.Forms.RadioButton fiveHundredKradioButton;
        private System.Windows.Forms.Panel graphOuterPanel;
        private System.Windows.Forms.Panel legendOuterPanel;
        private System.Windows.Forms.Panel graphPanel;
        private System.Windows.Forms.RadioButton twentyKradioButton;
        private System.Windows.Forms.GroupBox horizontalScaleGroupBox;
        private System.Windows.Forms.GroupBox verticalScaleGroupBox;
        private System.Windows.Forms.Panel typeLegendPanel;
        private System.Timers.Timer versionTimer;
        private System.Windows.Forms.MenuItem whoAllocatedMenuItem;
        private System.Windows.Forms.MenuItem showHistogramMenuItem;
        private System.Windows.Forms.MenuItem showObjectsMenuItem;
        private System.Windows.Forms.ContextMenu contextMenu;
        private System.Windows.Forms.MenuItem showRelocatedMenuItem;
        private System.Windows.Forms.MenuItem showAgeHistogramMenuItem;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.MenuItem setSelectionMenuItem;
        private int firstAllocTickIndex, lastAllocTickIndex;
        private System.Windows.Forms.MenuItem showTimeLineForSelectionMenuItem;
        private System.Windows.Forms.MenuItem showHeapGraphMenuItem;
        private RadioButton radioButton2;
        private RadioButton radioButton1;
        private RadioButton radioButton3;
        private RadioButton radioButton4;

        Font font;

        public TimeLineViewForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            toolTip = new ToolTip();
            toolTip.Active = false;
            toolTip.ShowAlways = true;
            toolTip.AutomaticDelay = 70;
            toolTip.ReshowDelay = 1;

            font = MainForm.instance.font;

            sampleObjectTable = MainForm.instance.lastLogResult.sampleObjectTable;
            lastLog = sampleObjectTable.readNewLog;
            typeName = lastLog.typeName;
            lastTickIndex = sampleObjectTable.lastTickIndex;
            firstAllocTickIndex = 0;
            lastAllocTickIndex = int.MaxValue;
        }

        private string Title()
        {
            if (firstAllocTickIndex != 0 || lastAllocTickIndex != int.MaxValue)
                return string.Format("Time Line for Objects allocated between {0:f3} and {1:f3} seconds",
                    lastLog.TickIndexToTime(firstAllocTickIndex),
                    lastLog.TickIndexToTime(lastAllocTickIndex));
            else
                return "Time Line";
        }

        public TimeLineViewForm(int firstAllocTickIndex, int lastAllocTickIndex) : this()
        {
            this.firstAllocTickIndex = firstAllocTickIndex;
            this.lastAllocTickIndex = lastAllocTickIndex;
            this.Text = Title();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            versionTimer.Stop();
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.horizontalScaleGroupBox = new System.Windows.Forms.GroupBox();
            this.oneThousandMSradioButton = new System.Windows.Forms.RadioButton();
            this.fiveHundredMSradioButton = new System.Windows.Forms.RadioButton();
            this.twoHundredMSradioButton = new System.Windows.Forms.RadioButton();
            this.oneHundredMSradioButton = new System.Windows.Forms.RadioButton();
            this.fiftyMSradioButton = new System.Windows.Forms.RadioButton();
            this.twentyMSradioButton = new System.Windows.Forms.RadioButton();
            this.tenMSradioButton = new System.Windows.Forms.RadioButton();
            this.fiveMSradioButton = new System.Windows.Forms.RadioButton();
            this.twoMSradioButton = new System.Windows.Forms.RadioButton();
            this.oneMSradioButton = new System.Windows.Forms.RadioButton();
            this.verticalScaleGroupBox = new System.Windows.Forms.GroupBox();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.fiveHundredKradioButton = new System.Windows.Forms.RadioButton();
            this.twoHundredKradioButton = new System.Windows.Forms.RadioButton();
            this.oneHundredKradioButton = new System.Windows.Forms.RadioButton();
            this.fiftyKradioButton = new System.Windows.Forms.RadioButton();
            this.twentyKradioButton = new System.Windows.Forms.RadioButton();
            this.tenKradioButton = new System.Windows.Forms.RadioButton();
            this.fiveKradioButton = new System.Windows.Forms.RadioButton();
            this.twoKradioButton = new System.Windows.Forms.RadioButton();
            this.oneKradioButton = new System.Windows.Forms.RadioButton();
            this.graphOuterPanel = new System.Windows.Forms.Panel();
            this.graphPanel = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.legendOuterPanel = new System.Windows.Forms.Panel();
            this.typeLegendPanel = new System.Windows.Forms.Panel();
            this.versionTimer = new System.Timers.Timer();
            this.contextMenu = new System.Windows.Forms.ContextMenu();
            this.whoAllocatedMenuItem = new System.Windows.Forms.MenuItem();
            this.showObjectsMenuItem = new System.Windows.Forms.MenuItem();
            this.showHistogramMenuItem = new System.Windows.Forms.MenuItem();
            this.showRelocatedMenuItem = new System.Windows.Forms.MenuItem();
            this.showAgeHistogramMenuItem = new System.Windows.Forms.MenuItem();
            this.setSelectionMenuItem = new System.Windows.Forms.MenuItem();
            this.showTimeLineForSelectionMenuItem = new System.Windows.Forms.MenuItem();
            this.showHeapGraphMenuItem = new System.Windows.Forms.MenuItem();
            this.panel1.SuspendLayout();
            this.horizontalScaleGroupBox.SuspendLayout();
            this.verticalScaleGroupBox.SuspendLayout();
            this.graphOuterPanel.SuspendLayout();
            this.legendOuterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.versionTimer)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.horizontalScaleGroupBox);
            this.panel1.Controls.Add(this.verticalScaleGroupBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1072, 88);
            this.panel1.TabIndex = 0;
            // 
            // horizontalScaleGroupBox
            // 
            this.horizontalScaleGroupBox.Controls.Add(this.oneThousandMSradioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.fiveHundredMSradioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.twoHundredMSradioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.oneHundredMSradioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.fiftyMSradioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.twentyMSradioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.tenMSradioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.fiveMSradioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.twoMSradioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.oneMSradioButton);
            this.horizontalScaleGroupBox.Location = new System.Drawing.Point(631, 8);
            this.horizontalScaleGroupBox.Name = "horizontalScaleGroupBox";
            this.horizontalScaleGroupBox.Size = new System.Drawing.Size(433, 64);
            this.horizontalScaleGroupBox.TabIndex = 1;
            this.horizontalScaleGroupBox.TabStop = false;
            this.horizontalScaleGroupBox.Text = "Horizontal Scale: Milliseconds/Pixel";
            // 
            // oneThousandMSradioButton
            // 
            this.oneThousandMSradioButton.Location = new System.Drawing.Point(370, 24);
            this.oneThousandMSradioButton.Name = "oneThousandMSradioButton";
            this.oneThousandMSradioButton.Size = new System.Drawing.Size(54, 24);
            this.oneThousandMSradioButton.TabIndex = 9;
            this.oneThousandMSradioButton.Text = "1000";
            this.oneThousandMSradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // fiveHundredMSradioButton
            // 
            this.fiveHundredMSradioButton.Location = new System.Drawing.Point(320, 24);
            this.fiveHundredMSradioButton.Name = "fiveHundredMSradioButton";
            this.fiveHundredMSradioButton.Size = new System.Drawing.Size(48, 24);
            this.fiveHundredMSradioButton.TabIndex = 8;
            this.fiveHundredMSradioButton.Text = "500";
            this.fiveHundredMSradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // twoHundredMSradioButton
            // 
            this.twoHundredMSradioButton.Location = new System.Drawing.Point(272, 24);
            this.twoHundredMSradioButton.Name = "twoHundredMSradioButton";
            this.twoHundredMSradioButton.Size = new System.Drawing.Size(48, 24);
            this.twoHundredMSradioButton.TabIndex = 7;
            this.twoHundredMSradioButton.Text = "200";
            this.twoHundredMSradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // oneHundredMSradioButton
            // 
            this.oneHundredMSradioButton.Location = new System.Drawing.Point(224, 24);
            this.oneHundredMSradioButton.Name = "oneHundredMSradioButton";
            this.oneHundredMSradioButton.Size = new System.Drawing.Size(48, 24);
            this.oneHundredMSradioButton.TabIndex = 6;
            this.oneHundredMSradioButton.Text = "100";
            this.oneHundredMSradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // fiftyMSradioButton
            // 
            this.fiftyMSradioButton.Location = new System.Drawing.Point(184, 24);
            this.fiftyMSradioButton.Name = "fiftyMSradioButton";
            this.fiftyMSradioButton.Size = new System.Drawing.Size(40, 24);
            this.fiftyMSradioButton.TabIndex = 5;
            this.fiftyMSradioButton.Text = "50";
            this.fiftyMSradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // twentyMSradioButton
            // 
            this.twentyMSradioButton.Location = new System.Drawing.Point(144, 24);
            this.twentyMSradioButton.Name = "twentyMSradioButton";
            this.twentyMSradioButton.Size = new System.Drawing.Size(40, 24);
            this.twentyMSradioButton.TabIndex = 4;
            this.twentyMSradioButton.Text = "20";
            this.twentyMSradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // tenMSradioButton
            // 
            this.tenMSradioButton.Location = new System.Drawing.Point(104, 24);
            this.tenMSradioButton.Name = "tenMSradioButton";
            this.tenMSradioButton.Size = new System.Drawing.Size(40, 24);
            this.tenMSradioButton.TabIndex = 3;
            this.tenMSradioButton.Text = "10";
            this.tenMSradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // fiveMSradioButton
            // 
            this.fiveMSradioButton.Location = new System.Drawing.Point(72, 24);
            this.fiveMSradioButton.Name = "fiveMSradioButton";
            this.fiveMSradioButton.Size = new System.Drawing.Size(32, 24);
            this.fiveMSradioButton.TabIndex = 2;
            this.fiveMSradioButton.Text = "5";
            this.fiveMSradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // twoMSradioButton
            // 
            this.twoMSradioButton.Location = new System.Drawing.Point(40, 24);
            this.twoMSradioButton.Name = "twoMSradioButton";
            this.twoMSradioButton.Size = new System.Drawing.Size(32, 24);
            this.twoMSradioButton.TabIndex = 1;
            this.twoMSradioButton.Text = "2";
            this.twoMSradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // oneMSradioButton
            // 
            this.oneMSradioButton.Location = new System.Drawing.Point(8, 24);
            this.oneMSradioButton.Name = "oneMSradioButton";
            this.oneMSradioButton.Size = new System.Drawing.Size(24, 24);
            this.oneMSradioButton.TabIndex = 0;
            this.oneMSradioButton.Text = "1";
            this.oneMSradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // verticalScaleGroupBox
            // 
            this.verticalScaleGroupBox.Controls.Add(this.radioButton4);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton3);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton2);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton1);
            this.verticalScaleGroupBox.Controls.Add(this.fiveHundredKradioButton);
            this.verticalScaleGroupBox.Controls.Add(this.twoHundredKradioButton);
            this.verticalScaleGroupBox.Controls.Add(this.oneHundredKradioButton);
            this.verticalScaleGroupBox.Controls.Add(this.fiftyKradioButton);
            this.verticalScaleGroupBox.Controls.Add(this.twentyKradioButton);
            this.verticalScaleGroupBox.Controls.Add(this.tenKradioButton);
            this.verticalScaleGroupBox.Controls.Add(this.fiveKradioButton);
            this.verticalScaleGroupBox.Controls.Add(this.twoKradioButton);
            this.verticalScaleGroupBox.Controls.Add(this.oneKradioButton);
            this.verticalScaleGroupBox.Location = new System.Drawing.Point(16, 8);
            this.verticalScaleGroupBox.Name = "verticalScaleGroupBox";
            this.verticalScaleGroupBox.Size = new System.Drawing.Size(609, 64);
            this.verticalScaleGroupBox.TabIndex = 0;
            this.verticalScaleGroupBox.TabStop = false;
            this.verticalScaleGroupBox.Text = "Vertical Scale: Kilobytes/Pixel";
            // 
            // radioButton4
            // 
            this.radioButton4.Location = new System.Drawing.Point(540, 24);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(63, 24);
            this.radioButton4.TabIndex = 12;
            this.radioButton4.Text = "10000";
            this.radioButton4.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton3
            // 
            this.radioButton3.Location = new System.Drawing.Point(486, 24);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(50, 24);
            this.radioButton3.TabIndex = 11;
            this.radioButton3.Text = "5000";
            this.radioButton3.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton2
            // 
            this.radioButton2.Location = new System.Drawing.Point(432, 24);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(56, 24);
            this.radioButton2.TabIndex = 10;
            this.radioButton2.Text = "2000";
            this.radioButton2.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton1
            // 
            this.radioButton1.Location = new System.Drawing.Point(374, 24);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(50, 24);
            this.radioButton1.TabIndex = 9;
            this.radioButton1.Text = "1000";
            this.radioButton1.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // fiveHundredKradioButton
            // 
            this.fiveHundredKradioButton.Location = new System.Drawing.Point(320, 24);
            this.fiveHundredKradioButton.Name = "fiveHundredKradioButton";
            this.fiveHundredKradioButton.Size = new System.Drawing.Size(48, 24);
            this.fiveHundredKradioButton.TabIndex = 8;
            this.fiveHundredKradioButton.Text = "500";
            this.fiveHundredKradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // twoHundredKradioButton
            // 
            this.twoHundredKradioButton.Location = new System.Drawing.Point(272, 24);
            this.twoHundredKradioButton.Name = "twoHundredKradioButton";
            this.twoHundredKradioButton.Size = new System.Drawing.Size(48, 24);
            this.twoHundredKradioButton.TabIndex = 7;
            this.twoHundredKradioButton.Text = "200";
            this.twoHundredKradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // oneHundredKradioButton
            // 
            this.oneHundredKradioButton.Location = new System.Drawing.Point(224, 24);
            this.oneHundredKradioButton.Name = "oneHundredKradioButton";
            this.oneHundredKradioButton.Size = new System.Drawing.Size(48, 24);
            this.oneHundredKradioButton.TabIndex = 6;
            this.oneHundredKradioButton.Text = "100";
            this.oneHundredKradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // fiftyKradioButton
            // 
            this.fiftyKradioButton.Location = new System.Drawing.Point(184, 24);
            this.fiftyKradioButton.Name = "fiftyKradioButton";
            this.fiftyKradioButton.Size = new System.Drawing.Size(40, 24);
            this.fiftyKradioButton.TabIndex = 5;
            this.fiftyKradioButton.Text = "50";
            this.fiftyKradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // twentyKradioButton
            // 
            this.twentyKradioButton.Location = new System.Drawing.Point(144, 24);
            this.twentyKradioButton.Name = "twentyKradioButton";
            this.twentyKradioButton.Size = new System.Drawing.Size(40, 24);
            this.twentyKradioButton.TabIndex = 4;
            this.twentyKradioButton.Text = "20";
            this.twentyKradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // tenKradioButton
            // 
            this.tenKradioButton.Location = new System.Drawing.Point(104, 24);
            this.tenKradioButton.Name = "tenKradioButton";
            this.tenKradioButton.Size = new System.Drawing.Size(40, 24);
            this.tenKradioButton.TabIndex = 3;
            this.tenKradioButton.Text = "10";
            this.tenKradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // fiveKradioButton
            // 
            this.fiveKradioButton.Location = new System.Drawing.Point(72, 24);
            this.fiveKradioButton.Name = "fiveKradioButton";
            this.fiveKradioButton.Size = new System.Drawing.Size(32, 24);
            this.fiveKradioButton.TabIndex = 2;
            this.fiveKradioButton.Text = "5";
            this.fiveKradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // twoKradioButton
            // 
            this.twoKradioButton.Location = new System.Drawing.Point(40, 24);
            this.twoKradioButton.Name = "twoKradioButton";
            this.twoKradioButton.Size = new System.Drawing.Size(32, 24);
            this.twoKradioButton.TabIndex = 1;
            this.twoKradioButton.Text = "2";
            this.twoKradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // oneKradioButton
            // 
            this.oneKradioButton.Location = new System.Drawing.Point(8, 24);
            this.oneKradioButton.Name = "oneKradioButton";
            this.oneKradioButton.Size = new System.Drawing.Size(32, 24);
            this.oneKradioButton.TabIndex = 0;
            this.oneKradioButton.Text = "1";
            this.oneKradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // graphOuterPanel
            // 
            this.graphOuterPanel.AutoScroll = true;
            this.graphOuterPanel.BackColor = System.Drawing.SystemColors.Control;
            this.graphOuterPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.graphOuterPanel.Controls.Add(this.graphPanel);
            this.graphOuterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphOuterPanel.Location = new System.Drawing.Point(0, 88);
            this.graphOuterPanel.Name = "graphOuterPanel";
            this.graphOuterPanel.Size = new System.Drawing.Size(761, 577);
            this.graphOuterPanel.TabIndex = 1;
            this.graphOuterPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.graphOuterPanel_MouseDown);
            // 
            // graphPanel
            // 
            this.graphPanel.BackColor = System.Drawing.SystemColors.Control;
            this.graphPanel.Location = new System.Drawing.Point(0, 0);
            this.graphPanel.Name = "graphPanel";
            this.graphPanel.Size = new System.Drawing.Size(576, 480);
            this.graphPanel.TabIndex = 0;
            this.graphPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.graphPanel_MouseDown);
            this.graphPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.graphPanel_MouseMove);
            this.graphPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.graphPanel_Paint);
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter1.Location = new System.Drawing.Point(761, 88);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 577);
            this.splitter1.TabIndex = 2;
            this.splitter1.TabStop = false;
            // 
            // legendOuterPanel
            // 
            this.legendOuterPanel.AutoScroll = true;
            this.legendOuterPanel.BackColor = System.Drawing.SystemColors.Control;
            this.legendOuterPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.legendOuterPanel.Controls.Add(this.typeLegendPanel);
            this.legendOuterPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.legendOuterPanel.Location = new System.Drawing.Point(764, 88);
            this.legendOuterPanel.Name = "legendOuterPanel";
            this.legendOuterPanel.Size = new System.Drawing.Size(308, 577);
            this.legendOuterPanel.TabIndex = 3;
            // 
            // typeLegendPanel
            // 
            this.typeLegendPanel.BackColor = System.Drawing.SystemColors.Control;
            this.typeLegendPanel.Location = new System.Drawing.Point(0, 0);
            this.typeLegendPanel.Name = "typeLegendPanel";
            this.typeLegendPanel.Size = new System.Drawing.Size(296, 480);
            this.typeLegendPanel.TabIndex = 0;
            this.typeLegendPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.typeLegendPanel_MouseDown);
            this.typeLegendPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.typeLegendPanel_Paint);
            // 
            // versionTimer
            // 
            this.versionTimer.Enabled = true;
            this.versionTimer.SynchronizingObject = this;
            this.versionTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.versionTimer_Elapsed);
            // 
            // contextMenu
            // 
            this.contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.whoAllocatedMenuItem,
            this.showObjectsMenuItem,
            this.showHistogramMenuItem,
            this.showRelocatedMenuItem,
            this.showAgeHistogramMenuItem,
            this.setSelectionMenuItem,
            this.showTimeLineForSelectionMenuItem,
            this.showHeapGraphMenuItem});
            // 
            // whoAllocatedMenuItem
            // 
            this.whoAllocatedMenuItem.Index = 0;
            this.whoAllocatedMenuItem.Text = "Show Who Allocated";
            this.whoAllocatedMenuItem.Click += new System.EventHandler(this.whoAllocatedMenuItem_Click);
            // 
            // showObjectsMenuItem
            // 
            this.showObjectsMenuItem.Index = 1;
            this.showObjectsMenuItem.Text = "Show Objects by Address";
            this.showObjectsMenuItem.Click += new System.EventHandler(this.showObjectsMenuItem_Click);
            // 
            // showHistogramMenuItem
            // 
            this.showHistogramMenuItem.Index = 2;
            this.showHistogramMenuItem.Text = "Show Histogram Allocated Types";
            this.showHistogramMenuItem.Click += new System.EventHandler(this.showHistogramMenuItem_Click);
            // 
            // showRelocatedMenuItem
            // 
            this.showRelocatedMenuItem.Index = 3;
            this.showRelocatedMenuItem.Text = "Show Histogram Relocated Types";
            this.showRelocatedMenuItem.Click += new System.EventHandler(this.showRelocatedMenuItem_Click);
            // 
            // showAgeHistogramMenuItem
            // 
            this.showAgeHistogramMenuItem.Index = 4;
            this.showAgeHistogramMenuItem.Text = "Show Histogram By Age";
            this.showAgeHistogramMenuItem.Click += new System.EventHandler(this.showAgeHistogramMenuItem_Click);
            // 
            // setSelectionMenuItem
            // 
            this.setSelectionMenuItem.Index = 5;
            this.setSelectionMenuItem.Text = "Set Selection to Marker...";
            this.setSelectionMenuItem.Click += new System.EventHandler(this.setSelectionMenuItem_Click);
            // 
            // showTimeLineForSelectionMenuItem
            // 
            this.showTimeLineForSelectionMenuItem.Index = 6;
            this.showTimeLineForSelectionMenuItem.Text = "Show Time Line for Selection";
            this.showTimeLineForSelectionMenuItem.Click += new System.EventHandler(this.showTimeLineForSelectionMenuItem_Click);
            // 
            // showHeapGraphMenuItem
            // 
            this.showHeapGraphMenuItem.Index = 7;
            this.showHeapGraphMenuItem.Text = "Show Heap Graph";
            this.showHeapGraphMenuItem.Click += new System.EventHandler(this.showHeapGraphMenuItem_Click);
            // 
            // TimeLineViewForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(1072, 665);
            this.Controls.Add(this.graphOuterPanel);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.legendOuterPanel);
            this.Controls.Add(this.panel1);
            this.Name = "TimeLineViewForm";
            this.Text = "Time Line";
            this.panel1.ResumeLayout(false);
            this.horizontalScaleGroupBox.ResumeLayout(false);
            this.verticalScaleGroupBox.ResumeLayout(false);
            this.graphOuterPanel.ResumeLayout(false);
            this.legendOuterPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.versionTimer)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        class AddressRange
        {
            internal ulong loAddr;
            internal ulong hiAddr;
            internal AddressRange next;
            internal int index;

            internal AddressRange(ulong loAddr, ulong hiAddr, AddressRange next, int index)
            {
                this.loAddr = loAddr;
                this.hiAddr = hiAddr;
                this.next = next;
                this.index = index;
            }
        }

        const int allowableGap = 64*1024-1;

        AddressRange rangeList = null;
        int rangeCount = 0;

        string[] typeName;

        private void AddAddress(ulong addr)
        {
            if (rangeList != null && addr - rangeList.hiAddr <= allowableGap)
            {
                Debug.Assert(addr >= rangeList.hiAddr);
                rangeList.hiAddr = addr;
            }
            else
            {
                rangeList = new AddressRange(addr, addr, rangeList, rangeCount++);
            }

        }

        class TypeDesc : IComparable
        {
            internal string typeName;
            internal long totalSize;
            internal Color color;
            internal Brush brush;
            internal Pen pen;
            internal bool selected;
            internal Rectangle rect;

            internal TypeDesc(string typeName)
            {
                this.typeName = typeName;
            }

            public int CompareTo(Object o)
            {
                TypeDesc t = (TypeDesc)o;
                if (t.totalSize < this.totalSize)
                    return -1;
                else if (t.totalSize > this.totalSize)
                    return 1;
                else
                    return 0;
            }
        }

        TypeDesc[] typeIndexToTypeDesc;

        ArrayList sortedTypeTable;

        private void AddToTypeTable(int typeIndex, int lifeTime)
        {
            TypeDesc t = typeIndexToTypeDesc[typeIndex];
            if (t == null)
            {
                t = new TypeDesc(typeName[typeIndex]);
                typeIndexToTypeDesc[typeIndex] = t;
            }
            t.totalSize += lifeTime;
        }

        private void ProcessChangeList(SampleObjectTable.SampleObject so)
        {
            int lastTickIndex = sampleObjectTable.lastTickIndex;
            for ( ; so != null; so = so.prev)
            {
                AddToTypeTable(so.typeIndex, lastTickIndex - so.changeTickIndex);
                lastTickIndex = so.changeTickIndex;
            }
        }

        private void BuildAddressRangesTypeTable(SampleObjectTable.SampleObject[][] masterTable)
        {
            rangeList = null;
            rangeCount = 0;

            if (typeIndexToTypeDesc == null || typeIndexToTypeDesc.Length < typeName.Length)
                typeIndexToTypeDesc = new TypeDesc[typeName.Length];
            else
            {
                foreach (TypeDesc t in typeIndexToTypeDesc)
                {
                    if (t != null)
                        t.totalSize = 0;
                }
            }

            for (uint i = 0; i < masterTable.Length; i++)
            {
                SampleObjectTable.SampleObject[] soa = masterTable[i];
                if (soa != null)
                {
                    for (uint j = 0; j < soa.Length; j++)
                    {
                        SampleObjectTable.SampleObject so = soa[j];
                        if (so != null)
                        {
                            AddAddress(((ulong)i<<SampleObjectTable.firstLevelShift)
                                            + (j<<SampleObjectTable.secondLevelShift));
                            ProcessChangeList(so);
                        }
                    }
                }
            }
            sortedTypeTable = new ArrayList();
            foreach (TypeDesc t in typeIndexToTypeDesc)
                if (t != null)
                    sortedTypeTable.Add(t);
            sortedTypeTable.Sort();
        }

        static Color[] firstColors =
        {
            Color.Red,
            Color.Yellow,
            Color.Green,
            Color.Cyan,
            Color.Blue,
            Color.Magenta,
        };

        static Color[] colors = new Color[16];

        Color MixColor(Color a, Color b)
        {
            int R = (a.R + b.R)/2;
            int G = (a.G + b.G)/2;
            int B = (a.B + b.B)/2;

            return Color.FromArgb(R, G, B);
        }

        static void GrowColors()
        {
            Color[] newColors = new Color[2*colors.Length];
            for (int i = 0; i < colors.Length; i++)
                newColors[i] = colors[i];
            colors = newColors;
        }

        private void ColorTypes()
        {
            int count = 0;

            foreach (TypeDesc t in sortedTypeTable)
            {
                if (count >= colors.Length)
                    GrowColors();
                if (count < firstColors.Length)
                    colors[count] = firstColors[count];
                else
                    colors[count] = MixColor(colors[count - firstColors.Length], colors[count - firstColors.Length + 1]);
                t.color = colors[count];
                if (t.typeName == "Free Space")
                    t.color = Color.White;
                else
                    count++;
            }
        }

        private void AssignBrushesPensToTypes()
        {
            bool anyTypeSelected = false;
            foreach (TypeDesc t in sortedTypeTable)
                anyTypeSelected |= t.selected;

            foreach (TypeDesc t in sortedTypeTable)
            {
                Color color = t.color;
                if (t.selected)
                    color = Color.Black;
                else if (anyTypeSelected)
                    color = MixColor(color, Color.White);
                t.brush = new SolidBrush(color);
                t.pen = new Pen(t.brush);
            }
        }

        int Scale(GroupBox groupBox, int pixelsAvailable, int rangeNeeded, bool firstTime)
        {
            if (!firstTime)
            {
                foreach (RadioButton rb in groupBox.Controls)
                {
                    if (rb.Checked)
                        return Int32.Parse(rb.Text);
                }
            }
            // No radio button was checked - let's come up with a suitable default
            RadioButton maxLowScaleRB = null;
            int maxLowRange = 0;
            RadioButton minHighScaleRB = null;
            int minHighRange = Int32.MaxValue;
            foreach (RadioButton rb in groupBox.Controls)
            {
                int range = pixelsAvailable*Int32.Parse(rb.Text);
                if (range < rangeNeeded)
                {
                    if (maxLowRange < range)
                    {
                        maxLowRange = range;
                        maxLowScaleRB = rb;
                    }
                }
                else
                {
                    if (minHighRange > range)
                    {
                        minHighRange = range;
                        minHighScaleRB = rb;
                    }
                }
            }
            if (minHighScaleRB != null)
            {
                minHighScaleRB.Checked = true;
                return Int32.Parse(minHighScaleRB.Text);
            }
            else
            {
                maxLowScaleRB.Checked = true;
                return Int32.Parse(maxLowScaleRB.Text);
            }
        }

        int verticalScale;
        int horizontalScale;

        const int leftMargin = 30;
        const int bottomMargin = 30;
        const int gap = 10;
        const int clickMargin = 20;
        const int topMargin = 30;
        int rightMargin = 80;
        const int minHeight = 400;
        int dotSize = 8;

        private int TimeToX(double time)
        {
            return leftMargin + addressLabelWidth + (int)(time*1000/horizontalScale);
        }

        private int AddrToY(ulong addr)
        {
            int y = topMargin;
            for (AddressRange r = rangeList; r != null; r = r.next)
            {
                if (r.loAddr <= addr && addr <= r.hiAddr)
                    return y + (int)((r.hiAddr - addr)/(uint)verticalScale);
                y += (int)((r.hiAddr - r.loAddr)/(uint)verticalScale);
                y += gap + timeLabelHeight;
            }
            return y;
        }

        void IntersectIntervals(int aLow, int aHigh, ref int bLow, ref int bHigh)
        {
            bLow = Math.Max(aLow, bLow);
            bHigh = Math.Min(aHigh, bHigh);
        }

        private void DrawChangeList(Graphics g, SampleObjectTable.SampleObject so, ulong addr)
        {
            double lastTime = lastLog.TickIndexToTime(sampleObjectTable.lastTickIndex);
            int y = AddrToY(addr);
            RectangleF clipRect = g.VisibleClipBounds;
            for ( ; so != null; so = so.prev)
            {
                TypeDesc t;
                if (firstAllocTickIndex <= so.origAllocTickIndex && so.origAllocTickIndex < lastAllocTickIndex)
                    t = typeIndexToTypeDesc[so.typeIndex];
                else
                    t = typeIndexToTypeDesc[0];

                double changeTime = lastLog.TickIndexToTime(so.changeTickIndex);
                int x1 = TimeToX(changeTime);
                int x2 = TimeToX(lastTime);
                IntersectIntervals((int)clipRect.Left, (int)clipRect.Right, ref x1, ref x2);
                if (x1 < x2)
                    g.DrawLine(t.pen, x1, y, x2, y);
                lastTime = changeTime;
            }
        }

        private void DrawSamples(Graphics g, SampleObjectTable.SampleObject[][] masterTable)
        {
            RectangleF clipRect = g.VisibleClipBounds;
            for (AddressRange r = rangeList; r != null; r = r.next)
            {
                int rangeTop = AddrToY(r.hiAddr);
                int rangeBottom = AddrToY(r.loAddr);
                IntersectIntervals((int)clipRect.Top, (int)clipRect.Bottom, ref rangeTop, ref rangeBottom);
                if (rangeTop < rangeBottom)
                {
                    ulong hiAddr = YToAddr(rangeTop);
                    for (ulong addr = (ulong)(YToAddr(rangeBottom) / (uint)verticalScale * (uint)verticalScale); addr <= hiAddr; addr += (ulong)verticalScale)
                    {
                        uint i = (uint)(addr >> SampleObjectTable.firstLevelShift);
                        SampleObjectTable.SampleObject[] soa = masterTable[i];
                        if (soa != null)
                        {
                            uint j = (uint)((addr >> SampleObjectTable.secondLevelShift) & (SampleObjectTable.secondLevelLength-1));
                            SampleObjectTable.SampleObject so = soa[j];
                            if (so != null)
                            {
                                int y = AddrToY(addr);
                                Debug.Assert(addr == ((ulong)i<<SampleObjectTable.firstLevelShift)
                                                          + (j<<SampleObjectTable.secondLevelShift));
                                if (clipRect.Top <= y && y <= clipRect.Bottom)
                                {
                                    DrawChangeList(g, so, addr);
                                }
                            }
                        }
                    }
                }
            }
        }

        int addressLabelWidth;

        private string FormatAddress(ulong addr)
        {
            if (addr > uint.MaxValue)
                return string.Format("{0:X2}.{1:X4}.{2:X4}", addr >> 32, (addr >> 16) & 0xffff, addr & 0xffff);
            else
                return string.Format("{0:X4}.{1:X4}", (addr >> 16) & 0xffff, addr & 0xffff);
        }

        private void DrawAddressLabel(Graphics g, Brush brush, Pen pen, AddressRange r, int y, ulong addr)
        {
            y += (int)((r.hiAddr - addr)/(uint)verticalScale);
            string s = FormatAddress(addr);
            int width = (int)g.MeasureString(s, font).Width + 2;
            g.DrawString(s, font, brush, leftMargin + addressLabelWidth - width, y - font.Height / 2);
            g.DrawLine(pen, leftMargin + addressLabelWidth - 3, y, leftMargin + addressLabelWidth, y);
        }

        private void DrawAddressLabels(Graphics g)
        {
            RectangleF clipRect = g.VisibleClipBounds;
            if (clipRect.Left > leftMargin + addressLabelWidth)
                return;
            Brush brush = new SolidBrush(Color.Black);
            Pen pen = new Pen(brush);
            const int minLabelPitchInPixels = 30;
            ulong minLabelPitch = (ulong)minLabelPitchInPixels * (ulong)verticalScale;

            ulong labelPitch = 1024;
            while (labelPitch < minLabelPitch)
                labelPitch *= 2;

            int y = topMargin;
            for (AddressRange r = rangeList; r != null; r = r.next)
            {
                DrawAddressLabel(g, brush, pen, r, y, r.loAddr);
                for (ulong addr = (r.loAddr + labelPitch * 3 / 2) & ~(labelPitch - 1); addr <= r.hiAddr; addr += labelPitch)
                    DrawAddressLabel(g, brush, pen, r, y, addr);
                y += (int)((r.hiAddr - r.loAddr)/(uint)verticalScale);
                y += gap + timeLabelHeight;
            }
        }

        int timeLabelHeight;

        private void DrawTimeLabels(Graphics g, double lastTime)
        {
            RectangleF clipRect = g.VisibleClipBounds;
            int labelPitchInPixels = 100;
            int timeLabelWidth = (int)g.MeasureString("999.9 sec ", font).Width;
            while (labelPitchInPixels < timeLabelWidth)
                labelPitchInPixels += 100;
            double labelPitch = labelPitchInPixels*horizontalScale*0.001;

            Brush brush = new SolidBrush(Color.DarkBlue);
            Pen pen = new Pen(brush);

            int y = topMargin;
            for (AddressRange r = rangeList; r != null; r = r.next)
            {
                y += (int)((r.hiAddr - r.loAddr)/(uint)verticalScale);
                if (y <= clipRect.Bottom && clipRect.Top <= y + timeLabelHeight)
                {
                    for (double time = 0; time < lastTime; time += labelPitch)
                    {
                        int x = TimeToX(time);
                        int x1 = x;
                        int x2 = x1 + timeLabelWidth;
                        IntersectIntervals((int)clipRect.Left, (int)clipRect.Right, ref x1, ref x2);
                        if (x1 < x2)
                        {
                            string s = string.Format("{0:f1} sec", time);
                            g.DrawLine(pen, x, y, x, y + font.Height + 6);
                            g.DrawString(s, font, brush, x, y + font.Height);
                        }
                    }
                }
                y += timeLabelHeight + gap;
            }
        }

        private int NextLabelX(Graphics g, SampleObjectTable.SampleObject gc, int gen, int[] pgcCount)
        {
            int[] gcCount = new int[3];
            gcCount[0] = pgcCount[0];
            gcCount[1] = pgcCount[1];
            gcCount[2] = pgcCount[2];
            if (gen < 2)
            {
                for (gc = gc.prev; gc != null; gc = gc.prev)
                {
                    gcCount[0]--;
                    if (gc.typeIndex >= 1)
                        gcCount[1]--;
                    if (gc.typeIndex >= 2)
                        gcCount[2]--;
                    if (gc.typeIndex > gen)
                    {
                        int x = TimeToX(lastLog.TickIndexToTime(gc.changeTickIndex));
                        string s = string.Format("gc #{0} (gen {1}#{2})", gcCount[0], gc.typeIndex, gcCount[gen]);
                        int minLabelPitch = (int)g.MeasureString(s, font).Width + 10;
                        return x + minLabelPitch;
                    }
                }
            }
            return int.MinValue;
        }

        private void DrawGcTicks(Graphics g, SampleObjectTable.SampleObject gcTickList)
        {
            RectangleF clipRect = g.VisibleClipBounds;
            Brush[] brushes = new Brush[3];
            Pen[] pens = new Pen[3];
            brushes[0] = new SolidBrush(Color.Red);
            brushes[1] = new SolidBrush(Color.Green);
            brushes[2] = new SolidBrush(Color.Blue);
            for (int i = 0; i < 3; i++)
                pens[i] = new Pen(brushes[i]);

            int[] totalGcCount = new int[3];
            for (SampleObjectTable.SampleObject gc = gcTickList; gc != null; gc = gc.prev)
            {
                totalGcCount[0]++;
                if (gc.typeIndex >= 1)
                    totalGcCount[1]++;
                if (gc.typeIndex >= 2)
                    totalGcCount[2]++;
            }

            int y = topMargin;
            for (AddressRange r = rangeList; r != null; r = r.next)
            {
                y += (int)((r.hiAddr - r.loAddr)/(uint)verticalScale);
                if (y <= clipRect.Bottom && clipRect.Top <= y + timeLabelHeight)
                {
                    for (int gen = 2; gen >= 0; gen--)
                    {
                        int[] gcCount = new int[3];
                        gcCount[0] = totalGcCount[0];
                        gcCount[1] = totalGcCount[1];
                        gcCount[2] = totalGcCount[2];
                        int lastLabelX = Int32.MaxValue;
                        for (SampleObjectTable.SampleObject gc = gcTickList; gc != null; gc = gc.prev)
                        {
                            int x = TimeToX(lastLog.TickIndexToTime(gc.changeTickIndex));
                            if (gc.typeIndex != gen)
                            {
                                if (gc.typeIndex > gen)
                                    lastLabelX = x;
                            }
                            else
                            {
                                string s;
                                if (gen == 0)
                                    s = string.Format("gc #{0}", gcCount[0]);
                                else
                                    s = string.Format("gc #{0} (gen {1}#{2})", gcCount[0], gen, gcCount[gen]);
                                int minLabelPitch = (int)g.MeasureString(s, font).Width + 10;
                                if (lastLabelX - x >= minLabelPitch && x > NextLabelX(g, gc, gen, gcCount))
                                {
                                    int x1 = x;
                                    int x2 = x1 + minLabelPitch;
                                    IntersectIntervals((int)clipRect.Left, (int)clipRect.Right, ref x1, ref x2);
                                    if (x1 < x2)
                                    {
                                        g.DrawLine(pens[gen], x, y, x, y+6);
                                        g.DrawString(s, font, brushes[gen], x, y);
                                    }
                                    lastLabelX = x;
                                }
                                else if (clipRect.Left <= x && x <= clipRect.Right)
                                {
                                    g.DrawLine(pens[gen], x, y, x, y+3);
                                }
                            }
                            gcCount[0]--;
                            if (gc.typeIndex >= 1)
                                gcCount[1]--;
                            if (gc.typeIndex >= 2)
                                gcCount[2]--;
                        }
                    }
                }
                y += timeLabelHeight + gap;
            }
        }

        SampleObjectTable sampleObjectTable;
        ReadNewLog lastLog;
        int lastTickIndex;
        bool initialized;

        private int RightMargin(Graphics g)
        {
            return (int)g.MeasureString("gc #999 (gen 2 #999)", font).Width;
        }

        private void DrawCommentLines(Graphics g, Rectangle clipRect)
        {
            Color color = Color.FromArgb(150, Color.LimeGreen);
            Pen pen = new Pen(color);
            int prevX = 0;
            for (int i = 0; i < lastLog.commentEventList.count; i++)
            {
                int x = TimeToX(lastLog.TickIndexToTime(lastLog.commentEventList.eventTickIndex[i]));
                if (x != prevX)
                {
                    Rectangle r = new Rectangle(x, commentVerticalMargin, 1, graphPanel.Height - commentVerticalMargin*2);
                    r.Intersect(clipRect);
                    if (r.Width != 0 && r.Height != 0)
                        g.DrawLine(pen, r.Left, r.Top, r.Left, r.Top + r.Height);
                    prevX = x;
                }
            }
        }

        private void graphPanel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            initialized = false;

            Graphics g = e.Graphics;

            rightMargin = RightMargin(g);

            lastTickIndex = sampleObjectTable.lastTickIndex;

            if (rangeList == null)
            {
                BuildAddressRangesTypeTable(sampleObjectTable.masterTable);
                ColorTypes();
                AssignBrushesPensToTypes();
            }

            ulong range = 0;
            for (AddressRange r = rangeList; r != null; r = r.next)
                range += r.hiAddr - r.loAddr;

            timeLabelHeight = font.Height*2;
            
            int availablePixels = graphPanel.Height - topMargin - bottomMargin - timeLabelHeight - (timeLabelHeight + gap)*(rangeCount-1);
            if (availablePixels < graphPanel.Height/2)
                availablePixels = graphPanel.Height/2;
            verticalScale = Scale(verticalScaleGroupBox, availablePixels, (int)(range/1024), verticalScale == 0)*1024;

            addressLabelWidth = (int)e.Graphics.MeasureString("0123456789A", font).Width;

            double lastTime = lastLog.TickIndexToTime(lastTickIndex);
            horizontalScale = Scale(horizontalScaleGroupBox, graphPanel.Width - leftMargin - addressLabelWidth - rightMargin, (int)(lastTime*1000.0), horizontalScale == 0);

            graphPanel.Width = leftMargin
                + addressLabelWidth
                + (int)(lastTime*1000/horizontalScale)
                + rightMargin;

            int height = topMargin;
            for (AddressRange r = rangeList; r != null; r = r.next)
            {
                height += (int)((r.hiAddr - r.loAddr)/(uint)verticalScale);
                height += gap + timeLabelHeight;
            }
            height -= gap;
            height += bottomMargin;

            graphPanel.Height = height;

            g.SetClip(e.ClipRectangle);

            SolidBrush backgroundBrush = new SolidBrush(graphPanel.BackColor);
            g.FillRectangle(backgroundBrush, e.ClipRectangle);

            DrawSamples(g, sampleObjectTable.masterTable);

            DrawCommentLines(g, e.ClipRectangle);

            DrawAddressLabels(g);

            DrawTimeLabels(g, lastTime);

            DrawGcTicks(g, sampleObjectTable.gcTickList);

            if (selectedStartTickIndex != selectedEndTickIndex)
                DrawSelectionVerticalLine(g, selectedStartTickIndex);
            DrawSelectionVerticalLine(g, selectedEndTickIndex);
            DrawSelectionHorizontalLines(g, selectedStartTickIndex, selectedEndTickIndex);

            initialized = true;
        }

        private void Refresh(object sender, System.EventArgs e)
        {
            graphPanel.Invalidate();
        }

        const int typeLegendSpacing = 3;

        private void CalculateAllocatedObjectSizes(int startTickIndex, int endTickIndex, ulong addr)
        {
            uint index = (uint)(addr >> SampleObjectTable.firstLevelShift);
            SampleObjectTable.SampleObject[] sot = sampleObjectTable.masterTable[index];
            if (sot == null)
                return;

            index = (uint)((addr >> SampleObjectTable.secondLevelShift) & (SampleObjectTable.secondLevelLength-1));

            for (SampleObjectTable.SampleObject so = sot[index]; so != null; so = so.prev)
            {
                if (startTickIndex <= so.origAllocTickIndex && so.origAllocTickIndex < endTickIndex && so.typeIndex != 0)
                {
                    if (so.prev == null || so.prev.typeIndex == 0)
                        typeIndexToTypeDesc[so.typeIndex].totalSize += SampleObjectTable.sampleGrain;
                }
            }
        }

        private void CalculateAllocatedObjectSizes(int startTick, int endTick)
        {
            for (AddressRange r = rangeList; r != null; r = r.next)
            {
                for (ulong addr = r.loAddr; addr < r.hiAddr; addr += SampleObjectTable.sampleGrain)
                {
                    CalculateAllocatedObjectSizes(startTick, endTick, addr);
                }
            }
        }
        
        private void CalculateLiveObjectSizes(int tick)
        {
            for (AddressRange r = rangeList; r != null; r = r.next)
            {
                for (ulong addr = r.loAddr; addr < r.hiAddr; addr += SampleObjectTable.sampleGrain)
                {
                    SampleObjectTable.SampleObject so = FindSampleObject(tick, addr);
                    if (so != null && so.typeIndex != 0)
                        typeIndexToTypeDesc[so.typeIndex].totalSize += SampleObjectTable.sampleGrain;
                }
            }
        }

        private void DrawTypeLegend(Graphics g)
        {
            dotSize = (int)g.MeasureString("0", font).Width;
            int maxWidth = 0;
            int x = leftMargin;
            int y = topMargin + font.Height + typeLegendSpacing;
            foreach (TypeDesc t in sortedTypeTable)
            {
                int typeWidth = (int)g.MeasureString(t.typeName + " - 999,999,999 bytes (100.00%)", font).Width+dotSize*2;
                t.rect = new Rectangle(x, y, typeWidth, font.Height);
                if (maxWidth < t.rect.Width)
                    maxWidth = t.rect.Width;
                y = t.rect.Bottom + typeLegendSpacing;
            }
            int height = y + bottomMargin;
            typeLegendPanel.Height = height;

            int width = leftMargin + maxWidth + rightMargin;
            typeLegendPanel.Width = width;

            x = leftMargin;
            y = topMargin;

            Brush blackBrush = new SolidBrush(Color.Black);

            long totalSize = 0;
            foreach (TypeDesc t in sortedTypeTable)
                totalSize += t.totalSize;
            if (totalSize == 0)
                totalSize = 1;

            string title = "Types:";
            if (selectedStartTickIndex != 0)
            {
                double startTime = lastLog.TickIndexToTime(selectedStartTickIndex);
                double endTime = lastLog.TickIndexToTime(selectedEndTickIndex);
                if (selectedEndTickIndex != selectedStartTickIndex)
                {
                    title = string.Format("Types - estimated sizes allocated between {0:f3} and {1:f3} seconds:", startTime, endTime);
                }
                else
                {
                    title = string.Format("Types - estimated sizes live at {0:f3} seconds:", startTime);
                }
            }
            g.DrawString(title, font, blackBrush, leftMargin, topMargin);
            int dotOffset = (font.Height - dotSize)/2;
            foreach (TypeDesc t in sortedTypeTable)
            {
                string caption = t.typeName;
                if (selectedStartTickIndex != 0)
                {
                    double percentage = 100.0*t.totalSize/totalSize;
                    caption += string.Format(" - {0:n0} bytes ({1:f2}%)", t.totalSize, percentage);
                }
                g.FillRectangle(t.brush, t.rect.Left, t.rect.Top+dotOffset, dotSize, dotSize);
                g.DrawString(caption, font, blackBrush, t.rect.Left + dotSize*2, t.rect.Top);
                y = t.rect.Bottom + typeLegendSpacing;
            }
        }

        private void typeLegendPanel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (rangeList == null)
            {
                BuildAddressRangesTypeTable(sampleObjectTable.masterTable);
                ColorTypes();
                AssignBrushesPensToTypes();
            }

            DrawTypeLegend(e.Graphics);     
        }

        private void typeLegendPanel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != 0)
            {
                if (sortedTypeTable != null)
                {
                    foreach (TypeDesc t in sortedTypeTable)
                    {
                        if (t.rect.Contains(e.X, e.Y) != t.selected)
                        {
                            t.selected = !t.selected;
                            AssignBrushesPensToTypes();
                            graphPanel.Invalidate();
                            typeLegendPanel.Invalidate();
                        }
                    }
                }
            }
            else if ((e.Button & MouseButtons.Right) != MouseButtons.None)
            {
                Point p = new Point(e.X, e.Y);
                contextMenu.Show(typeLegendPanel, p);
            }
        }

        private void versionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (font != MainForm.instance.font)
            {
                font = MainForm.instance.font;
                graphPanel.Invalidate();
                typeLegendPanel.Invalidate();
            }

            ReadLogResult lastLogResult = MainForm.instance.lastLogResult;
            if (lastLogResult != null && lastLogResult.sampleObjectTable != sampleObjectTable)
            {
                sampleObjectTable = lastLogResult.sampleObjectTable;
                lastLog = sampleObjectTable.readNewLog;
                typeName = lastLog.typeName;
                lastTickIndex = sampleObjectTable.lastTickIndex;

                rangeList = null;
                rangeCount = 0;

                graphPanel.Invalidate();
                typeLegendPanel.Invalidate();
            }
        }

        private int XToTickIndex(int x)
        {
            x -= leftMargin + addressLabelWidth;
            double time = x*horizontalScale*0.001;
            return sampleObjectTable.readNewLog.TimeToTickIndex(time);
        }

        private ulong YToAddr(int y)
        {
            int rY = topMargin;
            for (AddressRange r = rangeList; r != null; r = r.next)
            {
                int nextY = rY + (int)((r.hiAddr - r.loAddr)/(uint)verticalScale);
                if (rY <= y && y <= nextY)
                    return r.hiAddr - (ulong)(y - rY) * (ulong)verticalScale;
                rY = nextY;
                rY += gap + timeLabelHeight;
            }
            return 0;
        }

        private void DrawChangeList(Graphics g, SampleObjectTable.SampleObject so, ulong addr, int tickIndex)
        {
            int nextTickIndex = sampleObjectTable.lastTickIndex;
            for ( ; so != null; so = so.prev)
            {
                if (so.changeTickIndex <= tickIndex && tickIndex < nextTickIndex)
                {
                    TypeDesc t;
                    if (firstAllocTickIndex <= so.origAllocTickIndex && so.origAllocTickIndex < lastAllocTickIndex)
                        t = typeIndexToTypeDesc[so.typeIndex];
                    else
                        t = typeIndexToTypeDesc[0];

                    int x = TimeToX(lastLog.TickIndexToTime(tickIndex));
                    int y = AddrToY(addr);
                    g.DrawLine(t.pen, x-1, y, x, y);
                    break;
                }
                nextTickIndex = so.changeTickIndex;
            }
        }

        private void DrawSamples(Graphics g, SampleObjectTable.SampleObject[][] masterTable, int tick)
        {
            for (uint i = 0; i < masterTable.Length; i++)
            {
                SampleObjectTable.SampleObject[] soa = masterTable[i];
                if (soa != null)
                {
                    for (uint j = 0; j < soa.Length; j++)
                    {
                        SampleObjectTable.SampleObject so = soa[j];
                        if (so != null)
                        {
                            ulong addr = ((ulong)i<<SampleObjectTable.firstLevelShift)
                                              + (j<<SampleObjectTable.secondLevelShift);
                            if ((addr % (uint)verticalScale) == 0)
                                DrawChangeList(g, so, addr, tick);
                        }
                    }
                }
            }
        }

        const int selectionVerticalMargin = 5;
        const int commentVerticalMargin = 7;

        private void EraseSelectionVerticalLine(Graphics g, int tickIndex)
        {
            Pen backgroundPen = new Pen(graphPanel.BackColor);
            int x = TimeToX(lastLog.TickIndexToTime(tickIndex));
            g.DrawLine(backgroundPen, x-1, selectionVerticalMargin+1, x-1, graphPanel.Height-selectionVerticalMargin-1);
            g.DrawLine(backgroundPen, x, selectionVerticalMargin+1, x, graphPanel.Height-selectionVerticalMargin-1);
            DrawSamples(g, sampleObjectTable.masterTable, tickIndex);
            DrawCommentLines(g, new Rectangle(x-1, 0, 2, graphPanel.Height));
        }

        private void DrawSelectionVerticalLine(Graphics g, int tickIndex)
        {
            int x = TimeToX(lastLog.TickIndexToTime(tickIndex));
            Pen blackPen = new Pen(Color.Black);
            g.DrawLine(blackPen, x, selectionVerticalMargin+1, x, graphPanel.Height-selectionVerticalMargin-1);
        }

        private void DrawSelectionHorizontalLines(Graphics g, Pen pen, int startTickIndex, int endTickIndex)
        {
            int startX = TimeToX(lastLog.TickIndexToTime(startTickIndex));
            int endX = TimeToX(lastLog.TickIndexToTime(endTickIndex));
            g.DrawLine(pen, startX, selectionVerticalMargin, endX, selectionVerticalMargin);
            g.DrawLine(pen, startX, graphPanel.Height-selectionVerticalMargin, endX, graphPanel.Height-selectionVerticalMargin);
        }

        private void EraseSelectionHorizontalLines(Graphics g, int startTick, int endTick)
        {
            Pen backGroundPen = new Pen(graphPanel.BackColor);
            DrawSelectionHorizontalLines(g, backGroundPen, startTick, endTick);
        }

        private void DrawSelectionHorizontalLines(Graphics g, int startTick, int endTick)
        {
            Pen blackPen = new Pen(Color.Black);
            DrawSelectionHorizontalLines(g, blackPen, startTick, endTick);
        }

        private int selectedStartTickIndex, selectedEndTickIndex, selectionAnchorTickIndex;

        private void SetSelection(int newStartTickIndex, int newEndTickIndex)
        {
            Graphics g = graphPanel.CreateGraphics();
            if (newStartTickIndex != selectedStartTickIndex)
                EraseSelectionVerticalLine(g, selectedStartTickIndex);
            if (newEndTickIndex!= selectedEndTickIndex)
                EraseSelectionVerticalLine(g, selectedEndTickIndex);
            if (newStartTickIndex != newEndTickIndex)
                DrawSelectionVerticalLine(g, newStartTickIndex);
            DrawSelectionVerticalLine(g, newEndTickIndex);
            if (newStartTickIndex == selectedStartTickIndex)
            {
                if (newEndTickIndex < selectedEndTickIndex)
                    EraseSelectionHorizontalLines(g, newEndTickIndex, selectedEndTickIndex);
                else
                    DrawSelectionHorizontalLines(g, selectedEndTickIndex, newEndTickIndex);
            }
            else if (newEndTickIndex == selectedEndTickIndex)
            {
                if (newStartTickIndex < selectedStartTickIndex)
                    DrawSelectionHorizontalLines(g, newStartTickIndex, selectedStartTickIndex);
                else
                    EraseSelectionHorizontalLines(g, selectedStartTickIndex, newStartTickIndex);
            }
            else
            {
                EraseSelectionHorizontalLines(g, selectedStartTickIndex, selectedEndTickIndex);
                DrawSelectionHorizontalLines(g, newStartTickIndex, newEndTickIndex);
            }
            selectedStartTickIndex = newStartTickIndex;
            selectedEndTickIndex = newEndTickIndex;

            DrawAddressLabels(g);
            DrawTimeLabels(g, lastLog.TickIndexToTime(lastTickIndex));
            DrawGcTicks(g, sampleObjectTable.gcTickList);

            if (selectedStartTickIndex != 0)
            {
                double selectedStartTime = lastLog.TickIndexToTime(selectedStartTickIndex);
                double selectedEndTime = lastLog.TickIndexToTime(selectedEndTickIndex);
                if (selectedStartTime == selectedEndTime)
                {
                    Text = string.Format("{0} - selected: {1:f3} seconds", Title(), selectedStartTime);
                    showHistogramMenuItem.Enabled = true;
                    showHistogramMenuItem.Text = "Show Histogram by Size";
                    showObjectsMenuItem.Enabled = true;
                    showRelocatedMenuItem.Enabled = false;
                    whoAllocatedMenuItem.Enabled = true;
                    showAgeHistogramMenuItem.Enabled = true;
                }
                else
                {
                    Text = string.Format("{0} - selected: {1:f3} seconds - {2:f3} seconds", Title(), selectedStartTime, selectedEndTime);
                    showHistogramMenuItem.Enabled = true;
                    showHistogramMenuItem.Text = "Show Histogram Allocated Types";
                    showObjectsMenuItem.Enabled = false;
                    showRelocatedMenuItem.Enabled = true;
                    whoAllocatedMenuItem.Enabled = true;
                    showAgeHistogramMenuItem.Enabled = false;
                }
            }
            else
            {
                Text = Title();
                showHistogramMenuItem.Enabled = true;
                showObjectsMenuItem.Enabled = false;
                showRelocatedMenuItem.Enabled = true;
                whoAllocatedMenuItem.Enabled = true;
                showAgeHistogramMenuItem.Enabled = false;
            }
            if (selectedStartTickIndex != 0)
            {
                foreach (TypeDesc t in sortedTypeTable)
                    t.totalSize = 0;

                if (selectedStartTickIndex == selectedEndTickIndex)
                    CalculateLiveObjectSizes(selectedStartTickIndex);
                else
                    CalculateAllocatedObjectSizes(selectedStartTickIndex, selectedEndTickIndex);

                sortedTypeTable.Sort();
            }
            typeLegendPanel.Invalidate();
        }

        private void ExtendSelection(int x)
        {
            int selectedTickIndex = XToTickIndex(x);
            if (selectedTickIndex != 0)
            {
                if (selectedTickIndex < selectionAnchorTickIndex)
                    SetSelection(selectedTickIndex, selectionAnchorTickIndex);
                else
                    SetSelection(selectionAnchorTickIndex, selectedTickIndex);
            }
        }

        private void graphPanel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != 0)
            {
                if ((Control.ModifierKeys & Keys.Shift) != 0)
                {
                    ExtendSelection(e.X);
                }
                else
                {
                    selectionAnchorTickIndex = XToTickIndex(e.X);
                    SetSelection(selectionAnchorTickIndex, selectionAnchorTickIndex);
                }
            }
            else if ((e.Button & MouseButtons.Right) != MouseButtons.None)
            {
                Point p = new Point(e.X, e.Y);
                contextMenu.Show(graphPanel, p);
            }
        }

        SampleObjectTable.SampleObject FindSampleObject(int tickIndex, ulong addr)
        {
            uint index = (uint)(addr >> SampleObjectTable.firstLevelShift);
            SampleObjectTable.SampleObject[] sot = sampleObjectTable.masterTable[index];
            if (sot == null)
                return null;

            index = (uint)((addr >> SampleObjectTable.secondLevelShift) & (SampleObjectTable.secondLevelLength-1));
            
            int nextTickIndex = lastTickIndex;
            for (SampleObjectTable.SampleObject so = sot[index]; so != null; so = so.prev)
            {
                if (so.changeTickIndex <= tickIndex && tickIndex < nextTickIndex)
                {
                    if (firstAllocTickIndex <= so.origAllocTickIndex && so.origAllocTickIndex < lastAllocTickIndex)
                        return so;
                    else
                        return null;
                }
                nextTickIndex = so.changeTickIndex;
            }
            return null;
        }

        private ulong HeapSize(int tick)
        {
            ulong sum = 0;
            for (AddressRange r = rangeList; r != null; r = r.next)
            {
                for (ulong addr = r.loAddr; addr < r.hiAddr; addr += SampleObjectTable.sampleGrain)
                {
                    SampleObjectTable.SampleObject so = FindSampleObject(tick, addr);
                    if (so != null && so.typeIndex != 0)
                        sum += SampleObjectTable.sampleGrain;
                }
            }
            return sum;
        }
        
        private ToolTip toolTip;

        private string FindComment(int mouseX)
        {
            string result = null;
            int resultDist = 2;
            for (int i = 0; i < lastLog.commentEventList.count; i++)
            {
                int commentX = TimeToX(lastLog.TickIndexToTime(lastLog.commentEventList.eventTickIndex[i]));
                int dist = Math.Abs(commentX - mouseX);
                if (resultDist > dist)
                {
                    resultDist = dist;
                    result = sampleObjectTable.readNewLog.commentEventList.eventString[i];
                }
            }
            return result;
        }

        private void graphPanel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!initialized)
                return;

            if ((e.Button & MouseButtons.Left) != 0)
            {
                ExtendSelection(e.X);
            }
            else if (e.Button == MouseButtons.None)
            {
                if (Form.ActiveForm == this)
                {
                    int tickIndex = XToTickIndex(e.X);
                    ulong addr = YToAddr(e.Y);
                    ulong heapSize = HeapSize(tickIndex);
                    string caption = string.Format("{0:f3} seconds, {1:n0} bytes heap size", lastLog.TickIndexToTime(tickIndex), heapSize);
                    SampleObjectTable.SampleObject so = FindSampleObject(tickIndex, addr);
                    if (so != null && so.typeIndex != 0)
                        caption = string.Format("{0} at {1} - ", typeName[so.typeIndex], FormatAddress(addr)) + caption;
                    string comment = FindComment(e.X);
                    if (comment != null)
                        caption = caption + "\r\n" + comment;
                    toolTip.Active = true;
                    toolTip.SetToolTip(graphPanel, caption);
                }
                else
                    toolTip.Active = false;
            }
        }

        private void graphOuterPanel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != 0)
            {
                if ((Control.ModifierKeys & Keys.Shift) == 0)
                {
                    selectionAnchorTickIndex = 0;
                    SetSelection(0, 0);
                }
            }
        }

        private LiveObjectTable GetLiveObjectTable()
        {
            int endTickIndex = lastTickIndex;
            if (selectedEndTickIndex != 0)
            {
                endTickIndex = selectedEndTickIndex;
            }
            ReadNewLog log = sampleObjectTable.readNewLog;
            long endPos = log.TickIndexToPos(endTickIndex);

            // Read the selected portion of the log again
            ReadLogResult readLogResult = new ReadLogResult();
            readLogResult.liveObjectTable = new LiveObjectTable(log);
            log.ReadFile(0, endPos, readLogResult);

            return readLogResult.liveObjectTable;
        }

        private Histogram GetLiveHistogram()
        {
            LiveObjectTable liveObjectTable = GetLiveObjectTable();
            Histogram histogram = new Histogram(sampleObjectTable.readNewLog);
            LiveObjectTable.LiveObject o;
            for (liveObjectTable.GetNextObject(0, ulong.MaxValue, out o); o.id < ulong.MaxValue; liveObjectTable.GetNextObject(o.id + o.size, ulong.MaxValue, out o))
            {
                if (firstAllocTickIndex <= o.allocTickIndex && o.allocTickIndex < lastAllocTickIndex)
                    histogram.AddObject(o.typeSizeStacktraceIndex, 1);
            }
            return histogram;
        }

        private void whoAllocatedMenuItem_Click(object sender, System.EventArgs e)
        {
            int startTickIndex = 0;
            int endTickIndex = lastTickIndex;
            if (selectedStartTickIndex != 0)
            {
                startTickIndex = selectedStartTickIndex;
                endTickIndex = selectedEndTickIndex;
            }
            Histogram histogram;
            string title;
            if (startTickIndex != 0 && startTickIndex == endTickIndex)
            {
                histogram = GetLiveHistogram();
                title = string.Format("Allocation Graph for Objects at {0:f3} seconds", lastLog.TickIndexToTime(startTickIndex));
            }
            else
            {
                ReadNewLog log = sampleObjectTable.readNewLog;
                long startPos = log.TickIndexToPos(startTickIndex);
                long endPos = log.TickIndexToPos(endTickIndex);

                // Read the selected portion of the log again
                ReadLogResult readLogResult = new ReadLogResult();
                readLogResult.allocatedHistogram = new Histogram(log);
                log.ReadFile(startPos, endPos, readLogResult);
                histogram = readLogResult.allocatedHistogram;
                title = string.Format("Allocation Graph for Objects allocated between {0:f3} and {1:f3} seconds", lastLog.TickIndexToTime(startTickIndex), lastLog.TickIndexToTime(endTickIndex));
            }
            Graph graph = histogram.BuildAllocationGraph(new FilterForm());

            // And post it back to the main form - hardest part is to compute an appropriate title...

            GraphViewForm graphViewForm = new GraphViewForm(graph, title);
            graphViewForm.Visible = true;
        }

        private void showHistogramMenuItem_Click(object sender, System.EventArgs e)
        {
            int startTickIndex = 0;
            int endTickIndex = lastTickIndex;
            if (selectedStartTickIndex != 0)
            {
                startTickIndex = selectedStartTickIndex;
                endTickIndex = selectedEndTickIndex;
            }
            Histogram histogram;
            string title;
            if (startTickIndex != 0 && startTickIndex == endTickIndex)
            {
                histogram = GetLiveHistogram();
                title = string.Format("Histogram by Size for Objects at {0:f3}", lastLog.TickIndexToTime(startTickIndex));
            }
            else
            {
                ReadNewLog log = sampleObjectTable.readNewLog;
                long startPos = log.TickIndexToPos(startTickIndex);
                long endPos = log.TickIndexToPos(endTickIndex);

                // Read the selected portion of the log again
                ReadLogResult readLogResult = new ReadLogResult();
                readLogResult.allocatedHistogram = new Histogram(log);
                log.ReadFile(startPos, endPos, readLogResult);
                histogram = readLogResult.allocatedHistogram;
                title = string.Format("Histogram by Size for Objects allocated between {0:f3} and {1:f3} seconds", lastLog.TickIndexToTime(startTickIndex), lastLog.TickIndexToTime(endTickIndex));
            }
            // And post it to a new histogram form - hardest part is to compute an appropriate title...

            HistogramViewForm histogramViewForm = new HistogramViewForm(histogram, title);
            histogramViewForm.Show();
        }

        private void showObjectsMenuItem_Click(object sender, System.EventArgs e)
        {
            int endTickIndex = lastTickIndex;
            if (selectedEndTickIndex != 0)
            {
                endTickIndex = selectedEndTickIndex;
            }
            LiveObjectTable liveObjectTable = GetLiveObjectTable();

            string title = string.Format("Live Objects by Address at {0:f3} seconds", lastLog.TickIndexToTime(endTickIndex));
            ViewByAddressForm viewByAddressForm = new ViewByAddressForm(liveObjectTable, title);
            viewByAddressForm.Show();
        }

        private void showRelocatedMenuItem_Click(object sender, System.EventArgs e)
        {
            int startTickIndex = 0;
            int endTickIndex = lastTickIndex;
            if (selectedStartTickIndex != 0)
            {
                startTickIndex = selectedStartTickIndex;
                endTickIndex = selectedEndTickIndex;
            }
            ReadNewLog log = sampleObjectTable.readNewLog;
            long startPos = log.TickIndexToPos(startTickIndex);
            long endPos = log.TickIndexToPos(endTickIndex);

            // Read the selected portion of the log again

            ReadLogResult readLogResult = new ReadLogResult();
            readLogResult.relocatedHistogram = new Histogram(log);
            readLogResult.liveObjectTable = new LiveObjectTable(log);
            log.ReadFile(startPos, endPos, readLogResult);

            // And post it to a new histogram form - hardest part is to compute an appropriate title...

            string title = string.Format("Histogram by Size for Objects relocated between {0:f3} and {1:f3} seconds", lastLog.TickIndexToTime(startTickIndex), lastLog.TickIndexToTime(endTickIndex));
            HistogramViewForm histogramViewForm = new HistogramViewForm(readLogResult.relocatedHistogram, title);
            histogramViewForm.Show();
        }

        private void showAgeHistogramMenuItem_Click(object sender, System.EventArgs e)
        {
            int endTickIndex = lastTickIndex;
            if (selectedEndTickIndex != 0)
            {
                endTickIndex = selectedEndTickIndex;
            }

            // Get the live object table for the selected point in time
            LiveObjectTable liveObjectTable = GetLiveObjectTable();

            // And post it to a new Histogram by Age form - hardest part is to compute an appropriate title...
            string title = string.Format("Histogram by Age for Live Objects at {0:f3} seconds", lastLog.TickIndexToTime(endTickIndex));
            AgeHistogram ageHistogram = new AgeHistogram(liveObjectTable, title);
            ageHistogram.Show();        
        }

        private void setSelectionMenuItem_Click(object sender, System.EventArgs e)
        {
            CommentRangeForm commentRangeForm = new CommentRangeForm();
            if (commentRangeForm.ShowDialog() == DialogResult.OK)
            {
                SetSelection(commentRangeForm.startTickIndex, commentRangeForm.endTickIndex);
            }                        
        }

        private void showHeapGraphMenuItem_Click(object sender, System.EventArgs e)
        {
            int endTickIndex = lastTickIndex;
            if (selectedEndTickIndex != 0)
            {
                endTickIndex = selectedEndTickIndex;
            }
            ReadNewLog log = sampleObjectTable.readNewLog;
            long endPos = log.TickIndexToPos(endTickIndex);

            // Read the selected portion of the log again
            ReadLogResult readLogResult = new ReadLogResult();
            readLogResult.liveObjectTable = new LiveObjectTable(log);
            readLogResult.objectGraph = new ObjectGraph(log, 0);
            log.ReadFile(0, endPos, readLogResult);
            Graph graph = readLogResult.objectGraph.BuildTypeGraph(new FilterForm());
            string title = string.Format("Heap Graph at {0:f3} seconds", lastLog.TickIndexToTime(readLogResult.objectGraph.tickIndex));
            GraphViewForm graphViewForm = new GraphViewForm(graph, title);
            graphViewForm.Visible = true;
        }

        private void showTimeLineForSelectionMenuItem_Click(object sender, System.EventArgs e)
        {
            TimeLineViewForm timeLineViewForm = new TimeLineViewForm(selectedStartTickIndex, selectedEndTickIndex);
            timeLineViewForm.Show();
        }
    }
}
