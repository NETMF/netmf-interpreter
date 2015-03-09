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
    /// Summary description for KillProcessForm.
    /// </summary>
    public class KillProcessForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        internal System.Windows.Forms.Label processFileNameLabel;
        private System.Windows.Forms.Button yesButton;
        private System.Windows.Forms.Button noButton;
        private System.Windows.Forms.Button cancelButton;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public KillProcessForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            //need to add any constructor code after InitializeComponent call
            //
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
            this.label1 = new System.Windows.Forms.Label();
            this.processFileNameLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.yesButton = new System.Windows.Forms.Button();
            this.noButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label1.Location = new System.Drawing.Point(40, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(264, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "The process you are profiling is still running:";
            // 
            // processFileNameLabel
            // 
            this.processFileNameLabel.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.processFileNameLabel.Location = new System.Drawing.Point(48, 64);
            this.processFileNameLabel.Name = "processFileNameLabel";
            this.processFileNameLabel.Size = new System.Drawing.Size(368, 32);
            this.processFileNameLabel.TabIndex = 1;
            this.processFileNameLabel.Text = "<profiled process>";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label3.Location = new System.Drawing.Point(48, 112);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(328, 23);
            this.label3.TabIndex = 2;
            this.label3.Text = "Do you want to terminate it and optionally save the profile?";
            // 
            // yesButton
            // 
            this.yesButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.yesButton.Location = new System.Drawing.Point(48, 152);
            this.yesButton.Name = "yesButton";
            this.yesButton.Size = new System.Drawing.Size(88, 23);
            this.yesButton.TabIndex = 3;
            this.yesButton.Text = "Yes";
            // 
            // noButton
            // 
            this.noButton.DialogResult = System.Windows.Forms.DialogResult.No;
            this.noButton.Location = new System.Drawing.Point(192, 152);
            this.noButton.Name = "noButton";
            this.noButton.TabIndex = 4;
            this.noButton.Text = "No";
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(320, 152);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            // 
            // KillProcessForm
            // 
            this.AcceptButton = this.yesButton;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(456, 205);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                          this.cancelButton,
                                                                          this.noButton,
                                                                          this.yesButton,
                                                                          this.label3,
                                                                          this.processFileNameLabel,
                                                                          this.label1});
            this.Name = "KillProcessForm";
            this.Text = "Kill Process?";
            this.ResumeLayout(false);

        }
        #endregion
    }
}
