namespace Microsoft.SPOT.Emulator.Sample
{
    partial class InsertMediaDialogBox
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
            this.openFileDialogBox = new System.Windows.Forms.OpenFileDialog();
            this.CreateNewRadio = new System.Windows.Forms.RadioButton();
            this.OpenExistingRadio = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.MediaSizeLabel = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.BytesPerSectorComboBox = new System.Windows.Forms.ComboBox();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.FilePathTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SerialNumberTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.NumberOfBlocksTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SectorsPerBlockTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.saveFileDialogBox = new System.Windows.Forms.SaveFileDialog();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialogBox
            // 
            this.openFileDialogBox.Filter = "Block Storage Files|*.dat";
            // 
            // CreateNewRadio
            // 
            this.CreateNewRadio.AutoSize = true;
            this.CreateNewRadio.Checked = true;
            this.CreateNewRadio.Location = new System.Drawing.Point(24, 35);
            this.CreateNewRadio.Name = "CreateNewRadio";
            this.CreateNewRadio.Size = new System.Drawing.Size(113, 17);
            this.CreateNewRadio.TabIndex = 0;
            this.CreateNewRadio.TabStop = true;
            this.CreateNewRadio.Text = "Create New Media";
            this.CreateNewRadio.UseVisualStyleBackColor = true;
            // 
            // OpenExistingRadio
            // 
            this.OpenExistingRadio.AutoSize = true;
            this.OpenExistingRadio.Location = new System.Drawing.Point(24, 12);
            this.OpenExistingRadio.Name = "OpenExistingRadio";
            this.OpenExistingRadio.Size = new System.Drawing.Size(136, 17);
            this.OpenExistingRadio.TabIndex = 1;
            this.OpenExistingRadio.Text = "Open existing media file";
            this.OpenExistingRadio.UseVisualStyleBackColor = true;
            this.OpenExistingRadio.CheckedChanged += new System.EventHandler(this.OpenExistingRadio_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.MediaSizeLabel);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.BytesPerSectorComboBox);
            this.groupBox1.Controls.Add(this.BrowseButton);
            this.groupBox1.Controls.Add(this.FilePathTextBox);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.SerialNumberTextBox);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.NumberOfBlocksTextBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.SectorsPerBlockTextBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(24, 58);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(403, 229);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "New Media Configuration";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // MediaSizeLabel
            // 
            this.MediaSizeLabel.AutoSize = true;
            this.MediaSizeLabel.Location = new System.Drawing.Point(103, 126);
            this.MediaSizeLabel.Name = "MediaSizeLabel";
            this.MediaSizeLabel.Size = new System.Drawing.Size(13, 13);
            this.MediaSizeLabel.TabIndex = 13;
            this.MediaSizeLabel.Text = "0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 126);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(59, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Media Size";
            // 
            // BytesPerSectorComboBox
            // 
            this.BytesPerSectorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BytesPerSectorComboBox.FormattingEnabled = true;
            this.BytesPerSectorComboBox.Items.AddRange(new object[] {
            "2",
            "4",
            "8",
            "16",
            "32",
            "64",
            "128",
            "256",
            "512",
            "1024",
            "2048",
            "4096"});
            this.BytesPerSectorComboBox.Location = new System.Drawing.Point(106, 19);
            this.BytesPerSectorComboBox.Name = "BytesPerSectorComboBox";
            this.BytesPerSectorComboBox.Size = new System.Drawing.Size(125, 21);
            this.BytesPerSectorComboBox.TabIndex = 11;
            this.BytesPerSectorComboBox.SelectedIndexChanged += new System.EventHandler(this.BytesPerSectorComboBox_SelectedIndexChanged);
            this.BytesPerSectorComboBox.SelectedValueChanged += new System.EventHandler(this.BytesPerSectorComboBox_SelectedValueChanged);
            // 
            // BrowseButton
            // 
            this.BrowseButton.Location = new System.Drawing.Point(322, 175);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(75, 23);
            this.BrowseButton.TabIndex = 10;
            this.BrowseButton.Text = "Browse";
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // FilePathTextBox
            // 
            this.FilePathTextBox.Location = new System.Drawing.Point(106, 149);
            this.FilePathTextBox.Name = "FilePathTextBox";
            this.FilePathTextBox.Size = new System.Drawing.Size(291, 20);
            this.FilePathTextBox.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(5, 152);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "File Path";
            // 
            // SerialNumberTextBox
            // 
            this.SerialNumberTextBox.Location = new System.Drawing.Point(106, 97);
            this.SerialNumberTextBox.Name = "SerialNumberTextBox";
            this.SerialNumberTextBox.Size = new System.Drawing.Size(125, 20);
            this.SerialNumberTextBox.TabIndex = 7;
            this.SerialNumberTextBox.Text = "10000";
            this.SerialNumberTextBox.TextChanged += new System.EventHandler(this.SerialNumberTextBox_TextChanged);
            this.SerialNumberTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SerialNumberTextBox_KeyDown);
            this.SerialNumberTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.SerialNumberTextBox_Validating);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 100);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(73, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Serial Number";
            // 
            // NumberOfBlocksTextBox
            // 
            this.NumberOfBlocksTextBox.Location = new System.Drawing.Point(106, 71);
            this.NumberOfBlocksTextBox.Name = "NumberOfBlocksTextBox";
            this.NumberOfBlocksTextBox.Size = new System.Drawing.Size(125, 20);
            this.NumberOfBlocksTextBox.TabIndex = 5;
            this.NumberOfBlocksTextBox.Text = "2048";
            this.NumberOfBlocksTextBox.TextChanged += new System.EventHandler(this.NumberOfBlocksTextBox_TextChanged);
            this.NumberOfBlocksTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NumberOfBlocksTextBox_KeyDown);
            this.NumberOfBlocksTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.NumberOfBlocksTextBox_Validating);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(91, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Number of Blocks";
            // 
            // SectorsPerBlockTextBox
            // 
            this.SectorsPerBlockTextBox.Location = new System.Drawing.Point(106, 45);
            this.SectorsPerBlockTextBox.Name = "SectorsPerBlockTextBox";
            this.SectorsPerBlockTextBox.Size = new System.Drawing.Size(125, 20);
            this.SectorsPerBlockTextBox.TabIndex = 3;
            this.SectorsPerBlockTextBox.Text = "64";
            this.SectorsPerBlockTextBox.TextChanged += new System.EventHandler(this.SectorsPerBlockTextBox_TextChanged);
            this.SectorsPerBlockTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SectorsPerBlockTextBox_KeyDown);
            this.SectorsPerBlockTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.SectorsPerBlockTextBox_Validating);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Sectors per Block";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Bytes Per Sector";
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(352, 293);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(271, 293);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 4;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // saveFileDialogBox
            // 
            this.saveFileDialogBox.Filter = "Block Storage Files|*.dat";
            // 
            // InsertMediaDialogBox
            // 
            this.AcceptButton = this.cancelButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(439, 328);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.OpenExistingRadio);
            this.Controls.Add(this.CreateNewRadio);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "InsertMediaDialogBox";
            this.Text = "InsertMediaDialogBox";
            this.Load += new System.EventHandler(this.InsertMediaDialogBox_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialogBox;
        private System.Windows.Forms.RadioButton CreateNewRadio;
        private System.Windows.Forms.RadioButton OpenExistingRadio;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox SectorsPerBlockTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.TextBox FilePathTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox SerialNumberTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox NumberOfBlocksTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.SaveFileDialog saveFileDialogBox;
        private System.Windows.Forms.ComboBox BytesPerSectorComboBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label MediaSizeLabel;
    }
}