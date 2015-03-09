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
    /// Summary description for SaveFileForm.
    /// </summary>
    public class SaveFileForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label label1;
        internal System.Windows.Forms.Label processFileNameLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button yesButton;
        private System.Windows.Forms.Button noButton;
        private System.Windows.Forms.Button alwaysNoButton;
        private System.Windows.Forms.Button cancelButton;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public SaveFileForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // need to add any constructor code after InitializeComponent call
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
            this.label2 = new System.Windows.Forms.Label();
            this.yesButton = new System.Windows.Forms.Button();
            this.noButton = new System.Windows.Forms.Button();
            this.alwaysNoButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label1.Location = new System.Drawing.Point(32, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(248, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "You still have a profile for:";
            // 
            // processFileNameLabel
            // 
            this.processFileNameLabel.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.processFileNameLabel.Location = new System.Drawing.Point(40, 56);
            this.processFileNameLabel.Name = "processFileNameLabel";
            this.processFileNameLabel.Size = new System.Drawing.Size(384, 32);
            this.processFileNameLabel.TabIndex = 1;
            this.processFileNameLabel.Text = "label2";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label2.Location = new System.Drawing.Point(40, 96);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(232, 23);
            this.label2.TabIndex = 2;
            this.label2.Text = "Do want to save it?";
            // 
            // yesButton
            // 
            this.yesButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.yesButton.Location = new System.Drawing.Point(40, 128);
            this.yesButton.Name = "yesButton";
            this.yesButton.TabIndex = 3;
            this.yesButton.Text = "Yes";
            // 
            // noButton
            // 
            this.noButton.DialogResult = System.Windows.Forms.DialogResult.No;
            this.noButton.Location = new System.Drawing.Point(144, 128);
            this.noButton.Name = "noButton";
            this.noButton.TabIndex = 4;
            this.noButton.Text = "No";
            // 
            // alwaysNoButton
            // 
            this.alwaysNoButton.DialogResult = System.Windows.Forms.DialogResult.Retry;
            this.alwaysNoButton.Location = new System.Drawing.Point(248, 128);
            this.alwaysNoButton.Name = "alwaysNoButton";
            this.alwaysNoButton.TabIndex = 5;
            this.alwaysNoButton.Text = "Always No";
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(352, 128);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            // 
            // SaveFileForm
            // 
            this.ClientSize = new System.Drawing.Size(456, 181);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                          this.cancelButton,
                                                                          this.alwaysNoButton,
                                                                          this.noButton,
                                                                          this.yesButton,
                                                                          this.label2,
                                                                          this.processFileNameLabel,
                                                                          this.label1});
            this.Name = "SaveFileForm";
            this.Text = "Save Profile?";
            this.ResumeLayout(false);

        }
        #endregion
    }
}
