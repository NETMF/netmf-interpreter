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
    /// Summary description for ProfileServiceForm.
    /// </summary>
    public class ProfileServiceForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label label1;
        internal System.Windows.Forms.TextBox serviceNameTextBox;
        private System.Windows.Forms.Label label2;
        internal System.Windows.Forms.TextBox startCommandTextBox;
        private System.Windows.Forms.Label label3;
        internal System.Windows.Forms.TextBox stopCommandTextBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public ProfileServiceForm()
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
            this.serviceNameTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.startCommandTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.stopCommandTextBox = new System.Windows.Forms.TextBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.176471F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label1.Location = new System.Drawing.Point(40, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(240, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name of service to profile:";
            // 
            // serviceNameTextBox
            // 
            this.serviceNameTextBox.Location = new System.Drawing.Point(40, 48);
            this.serviceNameTextBox.Name = "serviceNameTextBox";
            this.serviceNameTextBox.Size = new System.Drawing.Size(368, 20);
            this.serviceNameTextBox.TabIndex = 1;
            this.serviceNameTextBox.Text = "";
            this.serviceNameTextBox.TextChanged += new System.EventHandler(this.serviceNameTextBox_TextChanged);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.176471F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label2.Location = new System.Drawing.Point(40, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(232, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Command to start the service:";
            // 
            // startCommandTextBox
            // 
            this.startCommandTextBox.Location = new System.Drawing.Point(40, 112);
            this.startCommandTextBox.Name = "startCommandTextBox";
            this.startCommandTextBox.Size = new System.Drawing.Size(368, 20);
            this.startCommandTextBox.TabIndex = 3;
            this.startCommandTextBox.Text = "";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.176471F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label3.Location = new System.Drawing.Point(40, 152);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(240, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "Command to stop the service:";
            // 
            // stopCommandTextBox
            // 
            this.stopCommandTextBox.Location = new System.Drawing.Point(40, 176);
            this.stopCommandTextBox.Name = "stopCommandTextBox";
            this.stopCommandTextBox.Size = new System.Drawing.Size(368, 20);
            this.stopCommandTextBox.TabIndex = 5;
            this.stopCommandTextBox.Text = "";
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(464, 48);
            this.okButton.Name = "okButton";
            this.okButton.TabIndex = 6;
            this.okButton.Text = "OK";
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(464, 112);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 7;
            this.cancelButton.Text = "Cancel";
            // 
            // ProfileServiceForm
            // 
            this.AcceptButton = this.okButton;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(568, 220);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                          this.cancelButton,
                                                                          this.okButton,
                                                                          this.stopCommandTextBox,
                                                                          this.label3,
                                                                          this.startCommandTextBox,
                                                                          this.label2,
                                                                          this.serviceNameTextBox,
                                                                          this.label1});
            this.Name = "ProfileServiceForm";
            this.Text = "Profile Service ...";
            this.ResumeLayout(false);

        }
        #endregion

        private void serviceNameTextBox_TextChanged(object sender, System.EventArgs e)
        {
            startCommandTextBox.Text = "net start " + serviceNameTextBox.Text;
            stopCommandTextBox .Text = "net stop "  + serviceNameTextBox.Text;
        }
    }
}
