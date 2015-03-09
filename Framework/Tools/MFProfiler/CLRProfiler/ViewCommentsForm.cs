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
    /// Summary description for ViewCommentsForm.
    /// </summary>
    public class ViewCommentsForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.TextBox commentTextBox;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        internal ViewCommentsForm(ReadNewLog log)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            string[] lines = new string[log.commentEventList.count];
            for (int i = 0; i < log.commentEventList.count; i++)
                lines[i] = string.Format("{0} ({1:f3} secs)", log.commentEventList.eventString[i], log.TickIndexToTime(log.commentEventList.eventTickIndex[i]));
            this.commentTextBox.Lines = lines;
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
            this.commentTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // commentTextBox
            // 
            this.commentTextBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right);
            this.commentTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.commentTextBox.Multiline = true;
            this.commentTextBox.Name = "commentTextBox";
            this.commentTextBox.ReadOnly = true;
            this.commentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.commentTextBox.Size = new System.Drawing.Size(880, 312);
            this.commentTextBox.TabIndex = 0;
            this.commentTextBox.Text = "";
            this.commentTextBox.WordWrap = false;
            // 
            // ViewCommentsForm
            // 
            this.ClientSize = new System.Drawing.Size(880, 310);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                          this.commentTextBox});
            this.Name = "ViewCommentsForm";
            this.Text = "Comments";
            this.ResumeLayout(false);

        }
        #endregion
    }
}
