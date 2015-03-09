namespace Microsoft.SPOT.AutomatedBuild.BuildSigner
{
    partial class NotificationDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.instructionLabel = new System.Windows.Forms.Label();
            this.signedStageText = new System.Windows.Forms.TextBox();
            this.unsignedStageText = new System.Windows.Forms.TextBox();
            this.unsignedLabel = new System.Windows.Forms.Label();
            this.signedLabel = new System.Windows.Forms.Label();
            this.resumeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // instructionLabel
            // 
            this.instructionLabel.AutoSize = true;
            this.instructionLabel.Location = new System.Drawing.Point(28, 19);
            this.instructionLabel.Name = "instructionLabel";
            this.instructionLabel.Size = new System.Drawing.Size(375, 65);
            this.instructionLabel.TabIndex = 0;
            this.instructionLabel.Text = "Files are ready to be signed.  \r\n\r\nPlace the signed files in the signed folder di" +
                "splayed below.\r\n\r\nClicking Resume will complete the build whether or not files h" +
                "ave been signed.";
            // 
            // signedStageText
            // 
            this.signedStageText.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.signedStageText.Location = new System.Drawing.Point(28, 175);
            this.signedStageText.Name = "signedStageText";
            this.signedStageText.ReadOnly = true;
            this.signedStageText.Size = new System.Drawing.Size(372, 20);
            this.signedStageText.TabIndex = 1;
            // 
            // unsignedStageText
            // 
            this.unsignedStageText.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.unsignedStageText.Location = new System.Drawing.Point(28, 120);
            this.unsignedStageText.Name = "unsignedStageText";
            this.unsignedStageText.ReadOnly = true;
            this.unsignedStageText.Size = new System.Drawing.Size(372, 20);
            this.unsignedStageText.TabIndex = 2;
            // 
            // unsignedLabel
            // 
            this.unsignedLabel.AutoSize = true;
            this.unsignedLabel.Location = new System.Drawing.Point(28, 104);
            this.unsignedLabel.Name = "unsignedLabel";
            this.unsignedLabel.Size = new System.Drawing.Size(123, 13);
            this.unsignedLabel.TabIndex = 3;
            this.unsignedLabel.Text = "Unsigned Staging Folder";
            // 
            // signedLabel
            // 
            this.signedLabel.AutoSize = true;
            this.signedLabel.Location = new System.Drawing.Point(28, 159);
            this.signedLabel.Name = "signedLabel";
            this.signedLabel.Size = new System.Drawing.Size(111, 13);
            this.signedLabel.TabIndex = 4;
            this.signedLabel.Text = "Signed Staging Folder";
            // 
            // resumeButton
            // 
            this.resumeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.resumeButton.Location = new System.Drawing.Point(162, 216);
            this.resumeButton.Name = "resumeButton";
            this.resumeButton.Size = new System.Drawing.Size(104, 23);
            this.resumeButton.TabIndex = 5;
            this.resumeButton.Text = "Resume Build";
            this.resumeButton.UseVisualStyleBackColor = true;
            // 
            // NotificationDialog
            // 
            this.AcceptButton = this.resumeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.resumeButton;
            this.ClientSize = new System.Drawing.Size(428, 259);
            this.ControlBox = false;
            this.Controls.Add(this.resumeButton);
            this.Controls.Add(this.signedLabel);
            this.Controls.Add(this.unsignedLabel);
            this.Controls.Add(this.unsignedStageText);
            this.Controls.Add(this.signedStageText);
            this.Controls.Add(this.instructionLabel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NotificationDialog";
            this.Text = "Automated Build System Notification";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label instructionLabel;
        private System.Windows.Forms.Label unsignedLabel;
        private System.Windows.Forms.Label signedLabel;
        private System.Windows.Forms.Button resumeButton;
        public System.Windows.Forms.TextBox signedStageText;
        public System.Windows.Forms.TextBox unsignedStageText;
    }
}
