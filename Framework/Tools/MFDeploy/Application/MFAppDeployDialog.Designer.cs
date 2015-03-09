namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
    partial class MFAppDeployConfigDialog
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.labelFile1 = new System.Windows.Forms.Label();
            this.buttonBrowseFile1 = new System.Windows.Forms.Button();
            this.buttonBrowsePrivateKey = new System.Windows.Forms.Button();
            this.labelPrivateKey = new System.Windows.Forms.Label();
            this.labelKeyIndex = new System.Windows.Forms.Label();
            this.comboBoxKeyIndex = new System.Windows.Forms.ComboBox();
            this.comboBoxFile1 = new System.Windows.Forms.ComboBox();
            this.comboBoxPrivateKey = new System.Windows.Forms.ComboBox();
            this.buttonCreate = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(386, 98);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(82, 23);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ButtonCancel;
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Location = new System.Drawing.Point(291, 98);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(89, 23);
            this.buttonOk.TabIndex = 6;
            this.buttonOk.Text = "&Ok";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // labelFile1
            // 
            this.labelFile1.AutoSize = true;
            this.labelFile1.Location = new System.Drawing.Point(14, 14);
            this.labelFile1.Name = "labelFile1";
            this.labelFile1.Size = new System.Drawing.Size(85, 13);
            this.labelFile1.TabIndex = 4;
            this.labelFile1.Text = "New Public Key:";
            // 
            // buttonBrowseFile1
            // 
            this.buttonBrowseFile1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowseFile1.Location = new System.Drawing.Point(439, 10);
            this.buttonBrowseFile1.Name = "buttonBrowseFile1";
            this.buttonBrowseFile1.Size = new System.Drawing.Size(29, 23);
            this.buttonBrowseFile1.TabIndex = 8;
            this.buttonBrowseFile1.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ButtonBrowseDotDotDot;
            this.buttonBrowseFile1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.buttonBrowseFile1.UseVisualStyleBackColor = true;
            this.buttonBrowseFile1.Click += new System.EventHandler(this.buttonBrowseFile1_Click);
            // 
            // buttonBrowsePrivateKey
            // 
            this.buttonBrowsePrivateKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowsePrivateKey.Location = new System.Drawing.Point(439, 37);
            this.buttonBrowsePrivateKey.Name = "buttonBrowsePrivateKey";
            this.buttonBrowsePrivateKey.Size = new System.Drawing.Size(29, 23);
            this.buttonBrowsePrivateKey.TabIndex = 12;
            this.buttonBrowsePrivateKey.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ButtonBrowseDotDotDot;
            this.buttonBrowsePrivateKey.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.buttonBrowsePrivateKey.UseVisualStyleBackColor = true;
            this.buttonBrowsePrivateKey.Click += new System.EventHandler(this.buttonBrowsePrivateKey_Click);
            // 
            // labelPrivateKey
            // 
            this.labelPrivateKey.AutoSize = true;
            this.labelPrivateKey.Location = new System.Drawing.Point(14, 41);
            this.labelPrivateKey.Name = "labelPrivateKey";
            this.labelPrivateKey.Size = new System.Drawing.Size(83, 13);
            this.labelPrivateKey.TabIndex = 10;
            this.labelPrivateKey.Text = "Old Private Key:";
            // 
            // labelKeyIndex
            // 
            this.labelKeyIndex.AutoSize = true;
            this.labelKeyIndex.Location = new System.Drawing.Point(14, 68);
            this.labelKeyIndex.Name = "labelKeyIndex";
            this.labelKeyIndex.Size = new System.Drawing.Size(89, 13);
            this.labelKeyIndex.TabIndex = 13;
            this.labelKeyIndex.Text = "Public Key Index:";
            // 
            // comboBoxKeyIndex
            // 
            this.comboBoxKeyIndex.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxKeyIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxKeyIndex.FormattingEnabled = true;
            this.comboBoxKeyIndex.Items.AddRange(new object[] {
            global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.KeyIndexFirmware,
            global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.KeyIndexDeployment});
            this.comboBoxKeyIndex.Location = new System.Drawing.Point(121, 65);
            this.comboBoxKeyIndex.Name = "comboBoxKeyIndex";
            this.comboBoxKeyIndex.Size = new System.Drawing.Size(312, 21);
            this.comboBoxKeyIndex.TabIndex = 14;
            this.comboBoxKeyIndex.SelectedIndexChanged += new System.EventHandler(this.comboBoxKeyIndex_SelectedIndexChanged);
            // 
            // comboBoxFile1
            // 
            this.comboBoxFile1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxFile1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.comboBoxFile1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.comboBoxFile1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
            this.comboBoxFile1.FormattingEnabled = true;
            this.comboBoxFile1.Location = new System.Drawing.Point(121, 11);
            this.comboBoxFile1.Name = "comboBoxFile1";
            this.comboBoxFile1.Size = new System.Drawing.Size(312, 21);
            this.comboBoxFile1.TabIndex = 15;
            // 
            // comboBoxPrivateKey
            // 
            this.comboBoxPrivateKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxPrivateKey.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.comboBoxPrivateKey.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.comboBoxPrivateKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
            this.comboBoxPrivateKey.FormattingEnabled = true;
            this.comboBoxPrivateKey.Location = new System.Drawing.Point(121, 38);
            this.comboBoxPrivateKey.Name = "comboBoxPrivateKey";
            this.comboBoxPrivateKey.Size = new System.Drawing.Size(312, 21);
            this.comboBoxPrivateKey.TabIndex = 16;
            // 
            // buttonCreate
            // 
            this.buttonCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCreate.Location = new System.Drawing.Point(121, 98);
            this.buttonCreate.Name = "buttonCreate";
            this.buttonCreate.Size = new System.Drawing.Size(105, 23);
            this.buttonCreate.TabIndex = 7;
            this.buttonCreate.Text = "C&reate Key";
            this.buttonCreate.UseVisualStyleBackColor = true;
            this.buttonCreate.Click += new System.EventHandler(this.buttonCreate_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(121, 68);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(312, 18);
            this.progressBar.TabIndex = 17;
            this.progressBar.Visible = false;
            // 
            // MFAppDeployConfigDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 133);
            this.ControlBox = false;
            this.Controls.Add(this.comboBoxPrivateKey);
            this.Controls.Add(this.comboBoxFile1);
            this.Controls.Add(this.labelKeyIndex);
            this.Controls.Add(this.buttonBrowsePrivateKey);
            this.Controls.Add(this.labelPrivateKey);
            this.Controls.Add(this.buttonBrowseFile1);
            this.Controls.Add(this.buttonCreate);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.labelFile1);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.comboBoxKeyIndex);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MFAppDeployConfigDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Public Key Configuration";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MFAppDeployConfigDialog_FormClosing);
            this.Load += new System.EventHandler(this.ConfigDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Label labelFile1;
        private System.Windows.Forms.Button buttonBrowseFile1;
        private System.Windows.Forms.Button buttonBrowsePrivateKey;
        private System.Windows.Forms.Label labelPrivateKey;
        private System.Windows.Forms.Label labelKeyIndex;
        private System.Windows.Forms.ComboBox comboBoxKeyIndex;
        private System.Windows.Forms.ComboBox comboBoxFile1;
        private System.Windows.Forms.ComboBox comboBoxPrivateKey;
        private System.Windows.Forms.Button buttonCreate;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}