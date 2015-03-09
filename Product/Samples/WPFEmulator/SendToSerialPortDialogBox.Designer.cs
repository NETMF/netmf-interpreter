namespace Microsoft.SPOT.Emulator.Sample
{
    partial class SendToSerialPortDialogBox
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textTextBox = new System.Windows.Forms.TextBox();
            this.sendButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textTextBox
            // 
            this.textTextBox.Location = new System.Drawing.Point(12, 12);
            this.textTextBox.Name = "textTextBox";
            this.textTextBox.Size = new System.Drawing.Size(222, 20);
            this.textTextBox.TabIndex = 0;
            // 
            // sendButton
            // 
            this.sendButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.sendButton.Location = new System.Drawing.Point(240, 10);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(73, 23);
            this.sendButton.TabIndex = 1;
            this.sendButton.Text = "Send";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // SendToSerialPortDialogBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(325, 44);
            this.Controls.Add(this.sendButton);
            this.Controls.Add(this.textTextBox);
            this.Name = "SendToSerialPortDialogBox";
            this.Text = "Send to Serial Port";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textTextBox;
        private System.Windows.Forms.Button sendButton;
    }
}