////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace CLRProfiler
{
    /// <summary>
    /// Summary description for SortAndHighlightSelector.
    /// </summary>
    internal class SortAndHighlightSelector : System.Windows.Forms.Form
    {
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox sortCounter;
        private System.Windows.Forms.ComboBox sortOrder;
        private System.Windows.Forms.ComboBox highlightOrder;
        private System.Windows.Forms.ComboBox highlightCounter;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        internal SortAndHighlightSelector(CallTreeForm.SortingBehaviour sort, CallTreeForm.SortingBehaviour highlight)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // need to add any constructor code after InitializeComponent call
            //
            int i, numCounters = Statistics.GetNumberOfCounters();
            sortCounter.Items.Add("in order of execution");
            for(i = 0; i < numCounters; i++)
            {
                sortCounter.Items.Add("by " + Statistics.GetCounterName(i).ToLower());
            }
            sortCounter.SelectedIndex = 1 + sort.counterId;
            sortOrder.SelectedIndex = (1 + sort.sortingOrder) / 2;

            for(i = 0; i < numCounters; i++)
            {
                highlightCounter.Items.Add(Statistics.GetCounterName(i).ToLower());
            }
            highlightCounter.SelectedIndex = highlight.counterId;
            highlightOrder.SelectedIndex = (1 + highlight.sortingOrder) / 2;
        }

        internal void GetSortResults(CallTreeForm.SortingBehaviour s,
                                   CallTreeForm.SortingBehaviour h)
        {
            s.counterId = sortCounter.SelectedIndex - 1;
            s.sortingOrder = sortOrder.SelectedIndex * 2 - 1;

            h.counterId = highlightCounter.SelectedIndex;
            h.sortingOrder = highlightOrder.SelectedIndex * 2 - 1;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.sortCounter = new System.Windows.Forms.ComboBox();
            this.sortOrder = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.highlightOrder = new System.Windows.Forms.ComboBox();
            this.highlightCounter = new System.Windows.Forms.ComboBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                                    this.sortOrder,
                                                                                    this.sortCounter});
            this.groupBox1.Location = new System.Drawing.Point(8, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(296, 56);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Sort...";
            // 
            // sortCounter
            // 
            this.sortCounter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sortCounter.Location = new System.Drawing.Point(8, 24);
            this.sortCounter.Name = "sortCounter";
            this.sortCounter.Size = new System.Drawing.Size(144, 21);
            this.sortCounter.TabIndex = 0;
            // 
            // sortOrder
            // 
            this.sortOrder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sortOrder.Items.AddRange(new object[] {
                                                           "in ascending order",
                                                           "in descending order"});
            this.sortOrder.Location = new System.Drawing.Point(160, 24);
            this.sortOrder.Name = "sortOrder";
            this.sortOrder.Size = new System.Drawing.Size(128, 21);
            this.sortOrder.TabIndex = 1;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                                    this.highlightCounter,
                                                                                    this.highlightOrder});
            this.groupBox2.Location = new System.Drawing.Point(8, 72);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(296, 56);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Highlight...";
            // 
            // highlightOrder
            // 
            this.highlightOrder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.highlightOrder.Items.AddRange(new object[] {
                                                                "most",
                                                                "least"});
            this.highlightOrder.Location = new System.Drawing.Point(8, 24);
            this.highlightOrder.Name = "highlightOrder";
            this.highlightOrder.Size = new System.Drawing.Size(80, 21);
            this.highlightOrder.TabIndex = 0;
            // 
            // highlightCounter
            // 
            this.highlightCounter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.highlightCounter.Location = new System.Drawing.Point(96, 24);
            this.highlightCounter.Name = "highlightCounter";
            this.highlightCounter.Size = new System.Drawing.Size(192, 21);
            this.highlightCounter.TabIndex = 1;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(232, 136);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(72, 24);
            this.button2.TabIndex = 3;
            this.button2.Text = "Cancel";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(152, 136);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(72, 24);
            this.button1.TabIndex = 2;
            this.button1.Text = "OK";
            // 
            // SortAndHighlightSelector
            // 
            this.ClientSize = new System.Drawing.Size(312, 173);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                          this.button2,
                                                                          this.button1,
                                                                          this.groupBox2,
                                                                          this.groupBox1});
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "SortAndHighlightSelector";
            this.Text = "Sort options";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
