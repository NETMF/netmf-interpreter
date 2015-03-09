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
    /// Summary description for CommentRangeForm.
    /// </summary>
    public class CommentRangeForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.ComboBox startComboBox;
        private System.Windows.Forms.ComboBox endComboBox;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        internal const string startCommentString = "Start of Application";
        internal const string shutdownCommentString = "Shutdown of Application";

        private void FillComboBoxes()
        {
            ReadNewLog log = MainForm.instance.log;

            startComboBox.Items.Add(startCommentString);
            endComboBox.Items.Add(startCommentString);

            for (int i = 0; i < log.commentEventList.count; i++)
            {
                string comment = log.commentEventList.eventString[i];
                startComboBox.Items.Add(comment);
                endComboBox.Items.Add(comment);
            }

            startComboBox.Items.Add(shutdownCommentString);
            endComboBox.Items.Add(shutdownCommentString);

            startComboBox.SelectedIndex = 0;
            endComboBox.SelectedIndex = endComboBox.Items.Count - 1;
            if (startComboBox.Items.Count > 2)
                startComboBox.SelectedIndex = 1;
            if (endComboBox.Items.Count > 2)
                endComboBox.SelectedIndex = 2;
        }

        internal CommentRangeForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            FillComboBoxes();
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
            this.startComboBox = new System.Windows.Forms.ComboBox();
            this.endComboBox = new System.Windows.Forms.ComboBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // startComboBox
            // 
            this.startComboBox.Location = new System.Drawing.Point(128, 32);
            this.startComboBox.Name = "startComboBox";
            this.startComboBox.Size = new System.Drawing.Size(368, 21);
            this.startComboBox.TabIndex = 0;
            this.startComboBox.Text = "Start of Application";
            // 
            // endComboBox
            // 
            this.endComboBox.Location = new System.Drawing.Point(128, 88);
            this.endComboBox.Name = "endComboBox";
            this.endComboBox.Size = new System.Drawing.Size(368, 21);
            this.endComboBox.TabIndex = 1;
            this.endComboBox.Text = "Shutdown of Application";
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(552, 32);
            this.okButton.Name = "okButton";
            this.okButton.TabIndex = 2;
            this.okButton.Text = "OK";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(552, 88);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(32, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 23);
            this.label1.TabIndex = 4;
            this.label1.Text = "Range starts at:";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(32, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 23);
            this.label2.TabIndex = 5;
            this.label2.Text = "Range ends at:";
            // 
            // CommentRangeForm
            // 
            this.AcceptButton = this.okButton;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(672, 150);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                          this.label2,
                                                                          this.label1,
                                                                          this.cancelButton,
                                                                          this.okButton,
                                                                          this.endComboBox,
                                                                          this.startComboBox});
            this.Name = "CommentRangeForm";
            this.Text = "Select Range...";
            this.ResumeLayout(false);

        }
        #endregion

        internal int startTickIndex;
        internal int endTickIndex;
        internal string startComment;
        internal string endComment;

        private int GetTickIndexOfSelectedIndex(int selectedIndex)
        {
            ReadNewLog log = MainForm.instance.log;
            if (selectedIndex == 0)
                return 0;
            else if (selectedIndex - 1 < log.commentEventList.count)
                return log.commentEventList.eventTickIndex[selectedIndex - 1];
            else
                return log.maxTickIndex;
        }

        private void okButton_Click(object sender, System.EventArgs e)
        {
            int selectedIndex = startComboBox.SelectedIndex;
            startComment = (string)startComboBox.Items[selectedIndex];
            startTickIndex = GetTickIndexOfSelectedIndex(selectedIndex);
            selectedIndex = endComboBox.SelectedIndex;
            endComment = (string)endComboBox.Items[selectedIndex];
            endTickIndex = GetTickIndexOfSelectedIndex(selectedIndex);
        }
    }
}
