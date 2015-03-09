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
    /// Summary description for WaitingForConnectionn.
    /// </summary>
    public class WaitingForConnectionForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Button cancelButton;
        internal System.Windows.Forms.Label textLabel;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public WaitingForConnectionForm()
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
            this.cancelButton = new System.Windows.Forms.Button();
            this.textLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(192, 80);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 0;
            this.cancelButton.Text = "Cancel";
            // 
            // textLabel
            // 
            this.textLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.textLabel.Location = new System.Drawing.Point(48, 32);
            this.textLabel.Name = "textLabel";
            this.textLabel.Size = new System.Drawing.Size(368, 32);
            this.textLabel.TabIndex = 1;
            this.textLabel.Text = "Waiting for application to start common language runtime";
            // 
            // WaitingForConnectionForm
            // 
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(464, 125);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                          this.textLabel,
                                                                          this.cancelButton});
            this.Name = "WaitingForConnectionForm";
            this.Text = "Waiting For Connection...";
            this.ResumeLayout(false);

        }
        #endregion
    }
}
