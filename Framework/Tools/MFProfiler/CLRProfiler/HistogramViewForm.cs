////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace CLRProfiler
{
    /// <summary>
    /// Summary description for HistogramViewForm.
    /// </summary>
    public class HistogramViewForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.RadioButton radioButton5;
        private System.Windows.Forms.RadioButton radioButton6;
        private System.Windows.Forms.RadioButton radioButton7;
        private System.Windows.Forms.RadioButton radioButton8;
        private System.Windows.Forms.RadioButton radioButton9;
        private System.Windows.Forms.RadioButton radioButton10;
        private System.Windows.Forms.Panel graphPanel;
        private System.Windows.Forms.Panel typeLegendPanel;
        private System.Windows.Forms.GroupBox verticalScaleGroupBox;
        private System.Timers.Timer versionTimer;
        private System.Windows.Forms.GroupBox horizontalScaleGroupBox;
        private System.Windows.Forms.RadioButton coarseRadioButton;
        private System.Windows.Forms.RadioButton veryFineRadioButton;
        private System.Windows.Forms.RadioButton mediumRadioButton;
        private System.Windows.Forms.RadioButton fineRadioButton;
        private System.Windows.Forms.ContextMenu contextMenu;
        private System.Windows.Forms.MenuItem exportMenuItem;
        private System.Windows.Forms.SaveFileDialog exportSaveFileDialog;
        private System.Windows.Forms.MenuItem showWhoAllocatedMenuItem;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        ReadLogResult lastLogResult;
        private System.Windows.Forms.Splitter splitter1;
        private RadioButton radioButton13;
        private RadioButton radioButton12;
        private RadioButton radioButton11;
        private RadioButton radioButton14;

        private Font font;

        public HistogramViewForm()
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

            autoUpdate = true;
            lastLogResult = MainForm.instance.lastLogResult;

            if (lastLogResult != null)
            {
                histogram = lastLogResult.allocatedHistogram;
                typeName = histogram.readNewLog.typeName;
            }
            Text = "Histogram by Size for Allocated Objects";
            font = MainForm.instance.font;
        }

        internal HistogramViewForm(Histogram histogram, string title) : this()
        {
            this.histogram = histogram;
            autoUpdate = false;
            Text = title;
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
            this.veryFineRadioButton = new System.Windows.Forms.RadioButton();
            this.fineRadioButton = new System.Windows.Forms.RadioButton();
            this.mediumRadioButton = new System.Windows.Forms.RadioButton();
            this.coarseRadioButton = new System.Windows.Forms.RadioButton();
            this.verticalScaleGroupBox = new System.Windows.Forms.GroupBox();
            this.radioButton14 = new System.Windows.Forms.RadioButton();
            this.radioButton13 = new System.Windows.Forms.RadioButton();
            this.radioButton12 = new System.Windows.Forms.RadioButton();
            this.radioButton11 = new System.Windows.Forms.RadioButton();
            this.radioButton10 = new System.Windows.Forms.RadioButton();
            this.radioButton9 = new System.Windows.Forms.RadioButton();
            this.radioButton8 = new System.Windows.Forms.RadioButton();
            this.radioButton7 = new System.Windows.Forms.RadioButton();
            this.radioButton6 = new System.Windows.Forms.RadioButton();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.graphPanel = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.typeLegendPanel = new System.Windows.Forms.Panel();
            this.versionTimer = new System.Timers.Timer();
            this.contextMenu = new System.Windows.Forms.ContextMenu();
            this.showWhoAllocatedMenuItem = new System.Windows.Forms.MenuItem();
            this.exportMenuItem = new System.Windows.Forms.MenuItem();
            this.exportSaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel1.SuspendLayout();
            this.horizontalScaleGroupBox.SuspendLayout();
            this.verticalScaleGroupBox.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
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
            this.panel1.Size = new System.Drawing.Size(1000, 72);
            this.panel1.TabIndex = 0;
            // 
            // horizontalScaleGroupBox
            // 
            this.horizontalScaleGroupBox.Controls.Add(this.veryFineRadioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.fineRadioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.mediumRadioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.coarseRadioButton);
            this.horizontalScaleGroupBox.Location = new System.Drawing.Point(692, 8);
            this.horizontalScaleGroupBox.Name = "horizontalScaleGroupBox";
            this.horizontalScaleGroupBox.Size = new System.Drawing.Size(292, 48);
            this.horizontalScaleGroupBox.TabIndex = 1;
            this.horizontalScaleGroupBox.TabStop = false;
            this.horizontalScaleGroupBox.Text = "Horizontal Scale";
            // 
            // veryFineRadioButton
            // 
            this.veryFineRadioButton.Location = new System.Drawing.Point(212, 16);
            this.veryFineRadioButton.Name = "veryFineRadioButton";
            this.veryFineRadioButton.Size = new System.Drawing.Size(72, 24);
            this.veryFineRadioButton.TabIndex = 3;
            this.veryFineRadioButton.Text = "Very Fine";
            this.veryFineRadioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // fineRadioButton
            // 
            this.fineRadioButton.Location = new System.Drawing.Point(158, 16);
            this.fineRadioButton.Name = "fineRadioButton";
            this.fineRadioButton.Size = new System.Drawing.Size(48, 24);
            this.fineRadioButton.TabIndex = 2;
            this.fineRadioButton.Text = "Fine";
            this.fineRadioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // mediumRadioButton
            // 
            this.mediumRadioButton.Location = new System.Drawing.Point(88, 16);
            this.mediumRadioButton.Name = "mediumRadioButton";
            this.mediumRadioButton.Size = new System.Drawing.Size(64, 24);
            this.mediumRadioButton.TabIndex = 1;
            this.mediumRadioButton.Text = "Medium";
            this.mediumRadioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // coarseRadioButton
            // 
            this.coarseRadioButton.Checked = true;
            this.coarseRadioButton.Location = new System.Drawing.Point(18, 16);
            this.coarseRadioButton.Name = "coarseRadioButton";
            this.coarseRadioButton.Size = new System.Drawing.Size(64, 24);
            this.coarseRadioButton.TabIndex = 0;
            this.coarseRadioButton.TabStop = true;
            this.coarseRadioButton.Text = "Coarse";
            this.coarseRadioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // verticalScaleGroupBox
            // 
            this.verticalScaleGroupBox.Controls.Add(this.radioButton14);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton13);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton12);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton11);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton10);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton9);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton8);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton7);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton6);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton5);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton4);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton3);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton2);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton1);
            this.verticalScaleGroupBox.Location = new System.Drawing.Point(16, 8);
            this.verticalScaleGroupBox.Name = "verticalScaleGroupBox";
            this.verticalScaleGroupBox.Size = new System.Drawing.Size(670, 48);
            this.verticalScaleGroupBox.TabIndex = 0;
            this.verticalScaleGroupBox.TabStop = false;
            this.verticalScaleGroupBox.Text = "Vertical Scale: Kilobytes/Pixel";
            // 
            // radioButton14
            // 
            this.radioButton14.Location = new System.Drawing.Point(601, 16);
            this.radioButton14.Name = "radioButton14";
            this.radioButton14.Size = new System.Drawing.Size(63, 24);
            this.radioButton14.TabIndex = 13;
            this.radioButton14.Text = "20000";
            this.radioButton14.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton13
            // 
            this.radioButton13.Location = new System.Drawing.Point(541, 16);
            this.radioButton13.Name = "radioButton13";
            this.radioButton13.Size = new System.Drawing.Size(63, 24);
            this.radioButton13.TabIndex = 12;
            this.radioButton13.Text = "10000";
            this.radioButton13.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton12
            // 
            this.radioButton12.Location = new System.Drawing.Point(487, 16);
            this.radioButton12.Name = "radioButton12";
            this.radioButton12.Size = new System.Drawing.Size(57, 24);
            this.radioButton12.TabIndex = 11;
            this.radioButton12.Text = "5000";
            this.radioButton12.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton11
            // 
            this.radioButton11.Location = new System.Drawing.Point(433, 16);
            this.radioButton11.Name = "radioButton11";
            this.radioButton11.Size = new System.Drawing.Size(55, 24);
            this.radioButton11.TabIndex = 10;
            this.radioButton11.Text = "2000";
            this.radioButton11.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton10
            // 
            this.radioButton10.Location = new System.Drawing.Point(376, 16);
            this.radioButton10.Name = "radioButton10";
            this.radioButton10.Size = new System.Drawing.Size(56, 24);
            this.radioButton10.TabIndex = 9;
            this.radioButton10.Text = "1000";
            this.radioButton10.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton9
            // 
            this.radioButton9.Location = new System.Drawing.Point(328, 16);
            this.radioButton9.Name = "radioButton9";
            this.radioButton9.Size = new System.Drawing.Size(48, 24);
            this.radioButton9.TabIndex = 8;
            this.radioButton9.Text = "500";
            this.radioButton9.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton8
            // 
            this.radioButton8.Location = new System.Drawing.Point(280, 16);
            this.radioButton8.Name = "radioButton8";
            this.radioButton8.Size = new System.Drawing.Size(48, 24);
            this.radioButton8.TabIndex = 7;
            this.radioButton8.Text = "200";
            this.radioButton8.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton7
            // 
            this.radioButton7.Location = new System.Drawing.Point(232, 16);
            this.radioButton7.Name = "radioButton7";
            this.radioButton7.Size = new System.Drawing.Size(48, 24);
            this.radioButton7.TabIndex = 6;
            this.radioButton7.Text = "100";
            this.radioButton7.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton6
            // 
            this.radioButton6.Location = new System.Drawing.Point(192, 16);
            this.radioButton6.Name = "radioButton6";
            this.radioButton6.Size = new System.Drawing.Size(40, 24);
            this.radioButton6.TabIndex = 5;
            this.radioButton6.Text = "50";
            this.radioButton6.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton5
            // 
            this.radioButton5.Location = new System.Drawing.Point(152, 16);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(40, 24);
            this.radioButton5.TabIndex = 4;
            this.radioButton5.Text = "20";
            this.radioButton5.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton4
            // 
            this.radioButton4.Location = new System.Drawing.Point(112, 16);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(40, 24);
            this.radioButton4.TabIndex = 3;
            this.radioButton4.Text = "10";
            this.radioButton4.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton3
            // 
            this.radioButton3.Location = new System.Drawing.Point(80, 16);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(32, 24);
            this.radioButton3.TabIndex = 2;
            this.radioButton3.Text = "5";
            this.radioButton3.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton2
            // 
            this.radioButton2.Location = new System.Drawing.Point(48, 16);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(32, 24);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.Text = "2";
            this.radioButton2.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton1
            // 
            this.radioButton1.Location = new System.Drawing.Point(16, 16);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(24, 24);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.Text = "1";
            this.radioButton1.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // panel2
            // 
            this.panel2.AutoScroll = true;
            this.panel2.BackColor = System.Drawing.SystemColors.Control;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.graphPanel);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 72);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(692, 533);
            this.panel2.TabIndex = 1;
            // 
            // graphPanel
            // 
            this.graphPanel.BackColor = System.Drawing.SystemColors.Control;
            this.graphPanel.Location = new System.Drawing.Point(0, 0);
            this.graphPanel.Name = "graphPanel";
            this.graphPanel.Size = new System.Drawing.Size(496, 520);
            this.graphPanel.TabIndex = 0;
            this.graphPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.graphPanel_MouseDown);
            this.graphPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.graphPanel_MouseMove);
            this.graphPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.graphPanel_Paint);
            // 
            // panel3
            // 
            this.panel3.AutoScroll = true;
            this.panel3.BackColor = System.Drawing.SystemColors.Control;
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.typeLegendPanel);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(696, 72);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(304, 533);
            this.panel3.TabIndex = 3;
            // 
            // typeLegendPanel
            // 
            this.typeLegendPanel.BackColor = System.Drawing.SystemColors.Control;
            this.typeLegendPanel.Location = new System.Drawing.Point(0, 0);
            this.typeLegendPanel.Name = "typeLegendPanel";
            this.typeLegendPanel.Size = new System.Drawing.Size(262, 531);
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
            this.showWhoAllocatedMenuItem,
            this.exportMenuItem});
            // 
            // showWhoAllocatedMenuItem
            // 
            this.showWhoAllocatedMenuItem.Index = 0;
            this.showWhoAllocatedMenuItem.Text = "Show Who Allocated";
            this.showWhoAllocatedMenuItem.Click += new System.EventHandler(this.showWhoAllocatedMenuItem_Click);
            // 
            // exportMenuItem
            // 
            this.exportMenuItem.Index = 1;
            this.exportMenuItem.Text = "Export Data to File...";
            this.exportMenuItem.Click += new System.EventHandler(this.exportMenuItem_Click);
            // 
            // exportSaveFileDialog
            // 
            this.exportSaveFileDialog.FileName = "doc1";
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter1.Location = new System.Drawing.Point(692, 72);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(4, 533);
            this.splitter1.TabIndex = 4;
            this.splitter1.TabStop = false;
            // 
            // HistogramViewForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(1000, 605);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Name = "HistogramViewForm";
            this.Text = "HistogramViewForm";
            this.panel1.ResumeLayout(false);
            this.horizontalScaleGroupBox.ResumeLayout(false);
            this.verticalScaleGroupBox.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.versionTimer)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        class TypeDesc : IComparable
        {
            internal string typeName;
            internal ulong totalSize;
            internal int count;
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

        struct Bucket
        {
            internal int minSize;
            internal int maxSize;
            internal ulong totalSize;
            internal Dictionary<TypeDesc, SizeCount> typeDescToSizeCount;
            internal bool selected;
        };

        Bucket[] buckets;
        double currentScaleFactor;

        void BuildBuckets()
        {
            double scaleFactor = 2.0;
            if (coarseRadioButton.Checked)
                scaleFactor = 2.0;
            else if (mediumRadioButton.Checked)
                scaleFactor = Math.Sqrt(2.0);
            else if (fineRadioButton.Checked)
                scaleFactor = Math.Pow(2.0, 0.25);
            else if (veryFineRadioButton.Checked)
                scaleFactor = Math.Pow(2.0, 0.125);

            if (currentScaleFactor == scaleFactor)
            {
                for (int i = 0; i < buckets.Length; i++)
                {
                    buckets[i].typeDescToSizeCount = new Dictionary<TypeDesc, SizeCount>();
                    buckets[i].totalSize = 0;
                }
                return;
            }

            currentScaleFactor = scaleFactor;

            int count = 0;
            int startSize = 8;
            double minSize;
            for (minSize = startSize; minSize < int.MaxValue; minSize *= scaleFactor)
                count++;

            buckets = new Bucket[count-1];
            minSize = startSize;
            for (int i = 0; i < buckets.Length; i++)
            {
                buckets[i].minSize = (int)Math.Round(minSize);
                minSize *= scaleFactor;
                buckets[i].maxSize = (int)Math.Round(minSize)-1;
                buckets[i].typeDescToSizeCount = new Dictionary<TypeDesc, SizeCount>();
                buckets[i].selected = false;
            }
        }

        private class SizeCount
        {
            internal ulong size;
            internal int count;
        }

        void AddToBuckets(TypeDesc t, int size, int count)
        {
            for (int i = 0; i < buckets.Length; i++)
            {
                if (buckets[i].minSize <= size && size <= buckets[i].maxSize)
                {
                    ulong totalSize = (ulong)size*(ulong)count;
                    buckets[i].totalSize += totalSize;
                    SizeCount sizeCount;
                    if (!buckets[i].typeDescToSizeCount.TryGetValue(t, out sizeCount))
                    {
                        sizeCount = new SizeCount();
                        buckets[i].typeDescToSizeCount[t] = sizeCount;
                    }
                    sizeCount.size += totalSize;
                    sizeCount.count += count;
                    break;
                }
            }
        }

        void TrimEmptyBuckets()
        {
            int lo = 0;
            for (int i = 0; i < buckets.Length-1; i++)
            {
                if (buckets[i].totalSize != 0 || buckets[i+1].totalSize != 0)
                    break;
                lo++;
            }
            int hi = buckets.Length-1;
            for (int i = buckets.Length-1; i >= 0; i--)
            {
                if (buckets[i].totalSize != 0)
                    break;
                hi--;
            }
            if (lo <= hi)
            {
                Bucket[] newBuckets = new Bucket[hi-lo+1];
                for (int i = lo; i <= hi; i++)
                    newBuckets[i-lo] = buckets[i];
                buckets = newBuckets;
            }
        }

        ulong totalSize;
        int totalCount;

        void BuildSizeRangesAndTypeTable(int[] typeSizeStacktraceToCount)
        {
            BuildBuckets();

            totalSize = 0;
            totalCount = 0;

            if (typeIndexToTypeDesc == null)
                typeIndexToTypeDesc = new TypeDesc[histogram.readNewLog.typeName.Length];
            else
            {
                foreach (TypeDesc t in typeIndexToTypeDesc)
                {
                    if (t != null)
                    {
                        t.totalSize = 0;
                        t.count = 0;
                    }
                }
            }

            for (int i = 0; i < typeSizeStacktraceToCount.Length; i++)
            {
                int count = typeSizeStacktraceToCount[i];
                if (count == 0)
                    continue;

                int[] stacktrace = histogram.readNewLog.stacktraceTable.IndexToStacktrace(i);
                int typeIndex = stacktrace[0];
                int size = stacktrace[1];

                TypeDesc t = (TypeDesc)typeIndexToTypeDesc[typeIndex];
                if (t == null)
                {
                    t = new TypeDesc(typeName[typeIndex]);
                    typeIndexToTypeDesc[typeIndex] = t;
                }
                t.totalSize += (ulong)size*(ulong)count;
                t.count += count;

                totalSize += (ulong)size * (ulong)count;
                totalCount += count;

                AddToBuckets(t, size, count);
            }

            if (totalSize == 0)
                totalSize = 1;
            if (totalCount == 0)
                totalCount = 1;

            TrimEmptyBuckets();

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

        private TypeDesc FindSelectedType()
        {
            foreach (TypeDesc t in sortedTypeTable)
                if (t.selected)
                    return t;
            return null;
        }

        private void ColorTypes()
        {
            int count = 0;

            bool anyTypeSelected = FindSelectedType() != null;

            foreach (TypeDesc t in sortedTypeTable)
            {
                if (count >= colors.Length)
                    GrowColors();
                if (count < firstColors.Length)
                    colors[count] = firstColors[count];
                else
                    colors[count] = MixColor(colors[count - firstColors.Length], colors[count - firstColors.Length + 1]);
                t.color = colors[count];
                if (anyTypeSelected)
                    t.color = MixColor(colors[count], Color.White);
                t.brush = new SolidBrush(t.color);
                t.pen = new Pen(t.brush);
                count++;
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

        int verticalScale = 0;

        int VerticalScale(int pixelsAvailable, ulong rangeNeeded, bool firstTime)
        {
            return Scale(verticalScaleGroupBox, pixelsAvailable, (int)(rangeNeeded/1024), firstTime)*1024;
        }

        const int leftMargin = 30;
        int bottomMargin = 50;
        const int gap = 20;
        int bucketWidth = 50;
        const int topMargin = 30;
        const int rightMargin = 30;
        const int minHeight = 400;
        int dotSize = 8;

        string FormatSize(ulong size)
        {
            double w = size;
            string byteString = "bytes";
            if (w >= 1024)
            {
                w /= 1024;
                byteString = "kB";
            }
            if (w >= 1024)
            {
                w /= 1024;
                byteString = "MB";
            }
            if (w >= 1024)
            {
                w /= 1024;
                byteString = "GB";
            }
            string format = "{0:f0} {1}";
            if (w < 10)
                format = "{0:f1} {1}";
            return string.Format(format, w, byteString);
        }

        private void DrawBuckets(Graphics g)
        {
            Debug.Assert(verticalScale != 0);
            bool noBucketSelected = true;
            foreach (Bucket b in buckets)
            {
                if (b.selected)
                {
                    noBucketSelected = false;
                    break;
                }
            }

            using (Brush blackBrush = new SolidBrush(Color.Black))
            {
                int x = leftMargin;
                foreach (Bucket b in buckets)
                {
                    string s = "< " + FormatSize((ulong)b.maxSize+1);
                    int y = graphPanel.Height - bottomMargin;
                    g.DrawString(s, font, blackBrush, x, y + 3);
                    s = FormatSize(b.totalSize);
                    g.DrawString(s, font, blackBrush, x, y + 3 + font.Height);
                    s = string.Format("({0:f2}%)", 100.0*b.totalSize/totalSize);
                    g.DrawString(s, font, blackBrush, x, y + 3 + font.Height*2);
                    foreach (KeyValuePair<TypeDesc, SizeCount> d in b.typeDescToSizeCount)
                    {
                        TypeDesc t = d.Key;
                        SizeCount sizeCount = d.Value;
                        ulong size = sizeCount.size;
                        int height = (int)(size / (ulong)verticalScale);

                        y -= height;

                        Brush brush = t.brush;
                        if (t.selected && (b.selected || noBucketSelected))
                            brush = blackBrush;
                        g.FillRectangle(brush, x, y, bucketWidth, height);
                    }

                    x += bucketWidth + gap;
                }
            }
        }

        const int typeLegendSpacing = 3;

        private void DrawTypeLegend(Graphics g)
        {
            dotSize = (int)g.MeasureString("0", font).Width;
            int maxWidth = 0;
            int x = leftMargin;
            int y = topMargin + font.Height + typeLegendSpacing*2;
            foreach (TypeDesc t in sortedTypeTable)
            {
                int typeNameWidth = (int)g.MeasureString(t.typeName, font).Width;
                int sizeWidth = (int)g.MeasureString(" (999,999,999 bytes, 100.00% - 999,999 instances, 999 bytes average size)", font).Width;
                t.rect = new Rectangle(x, y, Math.Max(typeNameWidth, sizeWidth)+dotSize*2, font.Height*2);
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

            string s = string.Format("Grand total: {0:n0} bytes - {1:n0} instances, {2:n0} bytes average size", totalSize, totalCount, totalSize/(ulong)totalCount);
            g.DrawString(s, font, blackBrush, x, y);

            y += font.Height + typeLegendSpacing*2;

            int dotOffset = (font.Height - dotSize)/2;
            foreach (TypeDesc t in sortedTypeTable)
            {
                Brush brush = t.brush;
                if (t.selected)
                    brush = blackBrush;
                g.FillRectangle(brush, t.rect.Left, t.rect.Top+dotOffset, dotSize, dotSize);
                g.DrawString(t.typeName, font, blackBrush, t.rect.Left + dotSize*2, t.rect.Top);
                s = string.Format(" ({0:n0} bytes, {1:f2}% - {2:n0} instances, {3:n0} bytes average size)", t.totalSize, (double)t.totalSize/totalSize*100.0, t.count, t.totalSize/(ulong)t.count);
                g.DrawString(s, font, blackBrush, t.rect.Left + dotSize*2, t.rect.Top + font.Height);
                y = t.rect.Bottom + typeLegendSpacing;
            }
        }

        bool initialized = false;

        private Histogram histogram;
        private string[] typeName;

        private int BucketWidth(Graphics g)
        {
            int width1 = (int)g.MeasureString("< 999.9 sec", font).Width;
            int width2 = (int)g.MeasureString("999 MB", font).Width;
            width1 = Math.Max(width1, width2);
            return Math.Max(width1, bucketWidth);
        }

        private int BottomMargin()
        {
            return font.Height*3 + 10;
        }

        private void graphPanel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            initialized = false;

            if (histogram == null || typeName == null)
                return;

            Graphics g = e.Graphics;

            bucketWidth = BucketWidth(g);
            bottomMargin = BottomMargin();

            BuildSizeRangesAndTypeTable(histogram.typeSizeStacktraceToCount);
            ColorTypes();

            ulong maxTotalSize = 0;
            foreach (Bucket b in buckets)
            {
                if (maxTotalSize < b.totalSize)
                    maxTotalSize = b.totalSize;
            }

            verticalScale = VerticalScale(graphPanel.Height - topMargin - bottomMargin, maxTotalSize, verticalScale == 0);

            int maxBucketHeight = (int)(maxTotalSize/(ulong)verticalScale);
            int height = topMargin + maxBucketHeight + bottomMargin;
            if (height < minHeight)
                height = minHeight;

            graphPanel.Height = height;
            
            int width = leftMargin + buckets.Length*bucketWidth + (buckets.Length-1)*gap + rightMargin;
            graphPanel.Width = width;

            DrawBuckets(g);

            initialized = true;
        }

        private void typeLegendPanel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            initialized = false;

            if (histogram == null || typeName == null)
                return;

            BuildSizeRangesAndTypeTable(histogram.typeSizeStacktraceToCount);
            ColorTypes();

            Graphics g = e.Graphics;

            DrawTypeLegend(g);      

            initialized = true;
        }

        private void Refresh(object sender, System.EventArgs e)
        {
            graphPanel.Invalidate();
        }

        private void typeLegendPanel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != MouseButtons.None)
            {
                for (int i = 0; i < buckets.Length; i++)
                {
                    if (buckets[i].selected)
                    {
                        graphPanel.Invalidate();
                        typeLegendPanel.Invalidate();
                        buckets[i].selected = false;
                    }
                }
                if (sortedTypeTable != null)
                {
                    foreach (TypeDesc t in sortedTypeTable)
                    {
                        if (t.rect.Contains(e.X, e.Y) != t.selected)
                        {
                            t.selected = !t.selected;
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

        private void graphPanel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!initialized || verticalScale == 0)
                return;

            if ((e.Button & MouseButtons.Left) != MouseButtons.None)
            {
                if (sortedTypeTable != null)
                {
                    foreach (TypeDesc t in sortedTypeTable)
                        t.selected = false;
                }

                int x = leftMargin;
                for (int i = 0; i < buckets.Length; i++)
                {
                    buckets[i].selected = false;
                    int y = graphPanel.Height - bottomMargin;
                    foreach (TypeDesc t in buckets[i].typeDescToSizeCount.Keys)
                    {
                        SizeCount sizeCount = buckets[i].typeDescToSizeCount[t];
                        ulong size = sizeCount.size;
                        int height = (int)(size / (ulong)verticalScale);

                        y -= height;

                        Rectangle r = new Rectangle(x, y, bucketWidth, height);
                        if (r.Contains(e.X, e.Y))
                        {
                            t.selected = true;
                            buckets[i].selected = true;
                        }
                    }

                    x += bucketWidth + gap;
                }       
                graphPanel.Invalidate();
                typeLegendPanel.Invalidate();
            }
            else if ((e.Button & MouseButtons.Right) != MouseButtons.None)
            {
                Point p = new Point(e.X, e.Y);
                contextMenu.Show(graphPanel, p);
            }
        }

        private ToolTip toolTip;

        private void graphPanel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!initialized || verticalScale == 0)
                return;

            if (Form.ActiveForm == this)
            {
                int x = leftMargin;
                foreach (Bucket b in buckets)
                {
                    int y = graphPanel.Height - bottomMargin;
                    foreach (KeyValuePair<TypeDesc, SizeCount> d in b.typeDescToSizeCount)
                    {
                        TypeDesc t = d.Key;
                        SizeCount sizeCount = d.Value;
                        ulong size = sizeCount.size;
                        int height = (int)(size / (ulong)verticalScale);

                        y -= height;

                        Rectangle bucketRect = new Rectangle(x, y, bucketWidth, height);
                        if (bucketRect.Contains(e.X, e.Y))
                        {
                            string caption = string.Format("{0} {1} ({2:f2}%) - {3:n0} instances, {4} average size", t.typeName, FormatSize(size), 100.0*size/totalSize, sizeCount.count, FormatSize(sizeCount.size/(ulong)sizeCount.count));
                            toolTip.Active = true;
                            toolTip.SetToolTip(graphPanel, caption);
                            return;
                        }
                    }
                    x += bucketWidth + gap;
                }
            }
            toolTip.Active = false;
            toolTip.SetToolTip(graphPanel, "");
        }

        private bool autoUpdate;

        private void versionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (font != MainForm.instance.font)
            {
                font = MainForm.instance.font;
                graphPanel.Invalidate();
                typeLegendPanel.Invalidate();
            }

            ReadLogResult readLogResult = MainForm.instance.lastLogResult;

            if (autoUpdate && readLogResult != null && readLogResult.allocatedHistogram != histogram)
            {
                histogram = readLogResult.allocatedHistogram;
                typeName = histogram.readNewLog.typeName;
                graphPanel.Invalidate();
                typeLegendPanel.Invalidate();
            }       
        }

        private void exportMenuItem_Click(object sender, System.EventArgs e)
        {
            exportSaveFileDialog.FileName = "HistogramBySize.csv";
            exportSaveFileDialog.Filter = "Comma separated files | *.csv";
            if (exportSaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                StreamWriter w = new StreamWriter(exportSaveFileDialog.FileName);

                TypeDesc selectedType = FindSelectedType();

                string title = "Histogram by Size";
                if (selectedType != null)
                    title += " of " + selectedType.typeName + " objects";

                w.WriteLine(title);
                w.WriteLine();

                w.WriteLine("{0},{1},{2},{3},{4}", "Min Size", "Max Size", "# Instances", "Total Size", "Type");

                bool noBucketSelected = true;
                int minSize = 0;
                int maxSize = int.MaxValue;
                foreach (Bucket b in buckets)
                {
                    if (b.selected)
                    {
                        noBucketSelected = false;
                        minSize = b.minSize;
                        maxSize = b.maxSize;
                    }
                }
                foreach (Bucket b in buckets)
                {
                    if (noBucketSelected || b.selected)
                    {
                        foreach (KeyValuePair<TypeDesc, SizeCount> d in b.typeDescToSizeCount)
                        {
                            TypeDesc t = d.Key;
                            SizeCount sizeCount = d.Value;

                            if (selectedType == null || t == selectedType)
                                w.WriteLine("{0},{1},{2},{3},{4}", b.minSize, b.maxSize, sizeCount.count, sizeCount.size, t.typeName);
                        }
                    }
                }

                w.WriteLine();
                w.WriteLine();
                w.WriteLine("Raw data:");
                w.WriteLine();

                w.WriteLine("{0},{1},{2},{3}", "Instance Size", "# Instances", "Total Size", "Type");
                for (int i = 0; i < histogram.typeSizeStacktraceToCount.Length; i++)
                {
                    int count = histogram.typeSizeStacktraceToCount[i];
                    if (count == 0)
                        continue;
                    int[] stacktrace = histogram.readNewLog.stacktraceTable.IndexToStacktrace(i);
                    int typeIndex = stacktrace[0];
                    int size = stacktrace[1];
                    
                    if (minSize <= size && size <= maxSize)
                    {
                        TypeDesc t = (TypeDesc)typeIndexToTypeDesc[typeIndex];
                
                        if (selectedType == null || t == selectedType)
                        {
                            w.WriteLine("{0},{1},{2},{3}", size, count, size*count, t.typeName);
                        }
                    }
                }

                w.Close();
            }
        }

        private void showWhoAllocatedMenuItem_Click(object sender, System.EventArgs e)
        {
            Histogram selectedHistogram;
            string title;
            TypeDesc selectedType = FindSelectedType();
            if (selectedType == null)
            {
                title = "Allocation Graph";
                selectedHistogram = histogram;
            }
            else
            {
                int minSize = 0;
                int maxSize = int.MaxValue;
                foreach (Bucket b in buckets)
                {
                    if (b.selected)
                    {
                        minSize = b.minSize;
                        maxSize = b.maxSize;
                    }
                }
                title = string.Format("Allocation Graph for {0} objects", selectedType.typeName);
                if (minSize > 0)
                    title += string.Format(" of size between {0:n0} and {1:n0} bytes", minSize, maxSize);
                selectedHistogram = new Histogram(histogram.readNewLog);
                for (int i = 0; i < histogram.typeSizeStacktraceToCount.Length; i++)
                {
                    int count = histogram.typeSizeStacktraceToCount[i];
                    if (count > 0)
                    {
                        int[] stacktrace = histogram.readNewLog.stacktraceTable.IndexToStacktrace(i);
                        int typeIndex = stacktrace[0];
                        int size = stacktrace[1];

                        if (minSize <= size && size <= maxSize)
                        {
                            TypeDesc t = (TypeDesc)typeIndexToTypeDesc[typeIndex];
                        
                            if (t == selectedType)
                            {
                                selectedHistogram.AddObject(i, count);
                            }
                        }
                    }
                }
            }

            Graph graph = selectedHistogram.BuildAllocationGraph(new FilterForm());

            GraphViewForm graphViewForm = new GraphViewForm(graph, title);
            graphViewForm.Visible = true;
        }
    }
}
