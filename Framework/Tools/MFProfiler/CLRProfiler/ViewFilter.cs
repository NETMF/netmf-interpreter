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
    /// Summary description for ViewFilter.
    /// </summary>
    public class ViewFilter : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.CheckBox callsCheckbox;
        public System.Windows.Forms.CheckBox allocationsCheckbox;
        public System.Windows.Forms.CheckBox assembliesCheckbox;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public ViewFilter(bool in_calls, bool in_allocs, bool in_assemblies)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // need to add any constructor code after InitializeComponent call
            //
            callsCheckbox.Checked = in_calls;
            allocationsCheckbox.Checked = in_allocs;
            assembliesCheckbox.Checked = in_assemblies;
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
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.callsCheckbox = new System.Windows.Forms.CheckBox();
            this.allocationsCheckbox = new System.Windows.Forms.CheckBox();
            this.assembliesCheckbox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(168, 160);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(72, 24);
            this.button2.TabIndex = 5;
            this.button2.Text = "Cancel";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(88, 160);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(72, 24);
            this.button1.TabIndex = 4;
            this.button1.Text = "OK";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(216, 16);
            this.label1.TabIndex = 6;
            this.label1.Text = "Select the events displayed in the call tree view.";
            // 
            // callsCheckbox
            // 
            this.callsCheckbox.Checked = true;
            this.callsCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.callsCheckbox.Enabled = false;
            this.callsCheckbox.Location = new System.Drawing.Point(24, 40);
            this.callsCheckbox.Name = "callsCheckbox";
            this.callsCheckbox.Size = new System.Drawing.Size(216, 16);
            this.callsCheckbox.TabIndex = 7;
            this.callsCheckbox.Text = "Function &calls";
            // 
            // allocationsCheckbox
            // 
            this.allocationsCheckbox.Location = new System.Drawing.Point(24, 64);
            this.allocationsCheckbox.Name = "allocationsCheckbox";
            this.allocationsCheckbox.Size = new System.Drawing.Size(216, 16);
            this.allocationsCheckbox.TabIndex = 8;
            this.allocationsCheckbox.Text = "&Allocations";
            // 
            // assembliesCheckbox
            // 
            this.assembliesCheckbox.Location = new System.Drawing.Point(24, 88);
            this.assembliesCheckbox.Name = "assembliesCheckbox";
            this.assembliesCheckbox.Size = new System.Drawing.Size(216, 16);
            this.assembliesCheckbox.TabIndex = 9;
            this.assembliesCheckbox.Text = "A&ssembly loads";
            // 
            // ViewFilter
            // 
            this.ClientSize = new System.Drawing.Size(250, 199);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                          this.assembliesCheckbox,
                                                                          this.allocationsCheckbox,
                                                                          this.callsCheckbox,
                                                                          this.label1,
                                                                          this.button2,
                                                                          this.button1});
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ViewFilter";
            this.Text = "View Filter";
            this.ResumeLayout(false);

        }
        #endregion
    }
}
