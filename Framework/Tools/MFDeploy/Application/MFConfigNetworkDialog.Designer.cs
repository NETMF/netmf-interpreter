namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
    partial class MFNetworkConfigDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxIPAddress = new System.Windows.Forms.TextBox();
            this.textBoxSubnetMask = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxDefaultGateway = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxMACAddress = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.checkBoxDHCPEnable = new System.Windows.Forms.CheckBox();
            this.textBoxDnsPrimary = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxDnsSecondary = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBoxNetKey = new System.Windows.Forms.ComboBox();
            this.checkBox80211n = new System.Windows.Forms.CheckBox();
            this.checkBox80211g = new System.Windows.Forms.CheckBox();
            this.checkBox80211b = new System.Windows.Forms.CheckBox();
            this.checkBox80211a = new System.Windows.Forms.CheckBox();
            this.checkBoxEncryptConfigData = new System.Windows.Forms.CheckBox();
            this.comboBoxEncryption = new System.Windows.Forms.ComboBox();
            this.comboBoxAuthentication = new System.Windows.Forms.ComboBox();
            this.textBoxSSID = new System.Windows.Forms.TextBox();
            this.labelSSID = new System.Windows.Forms.Label();
            this.textBoxNetworkKey = new System.Windows.Forms.TextBox();
            this.labelNetworkKey = new System.Windows.Forms.Label();
            this.textBoxPassPhrase = new System.Windows.Forms.TextBox();
            this.labelPassPhrase = new System.Windows.Forms.Label();
            this.labelRadio = new System.Windows.Forms.Label();
            this.labelEncryption = new System.Windows.Forms.Label();
            this.labelAuthentication = new System.Windows.Forms.Label();
            this.textBoxReKeyInternal = new System.Windows.Forms.TextBox();
            this.labelReKeyInternal = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Static &IP address:";
            // 
            // textBoxIPAddress
            // 
            this.textBoxIPAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxIPAddress.Location = new System.Drawing.Point(142, 6);
            this.textBoxIPAddress.Name = "textBoxIPAddress";
            this.textBoxIPAddress.Size = new System.Drawing.Size(241, 20);
            this.textBoxIPAddress.TabIndex = 1;
            // 
            // textBoxSubnetMask
            // 
            this.textBoxSubnetMask.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSubnetMask.Location = new System.Drawing.Point(142, 32);
            this.textBoxSubnetMask.Name = "textBoxSubnetMask";
            this.textBoxSubnetMask.Size = new System.Drawing.Size(241, 20);
            this.textBoxSubnetMask.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "&Subnet Mask:";
            // 
            // textBoxDefaultGateway
            // 
            this.textBoxDefaultGateway.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDefaultGateway.Location = new System.Drawing.Point(142, 58);
            this.textBoxDefaultGateway.Name = "textBoxDefaultGateway";
            this.textBoxDefaultGateway.Size = new System.Drawing.Size(241, 20);
            this.textBoxDefaultGateway.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 61);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Default &Gateway:";
            // 
            // textBoxMACAddress
            // 
            this.textBoxMACAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxMACAddress.Location = new System.Drawing.Point(142, 84);
            this.textBoxMACAddress.Name = "textBoxMACAddress";
            this.textBoxMACAddress.Size = new System.Drawing.Size(241, 20);
            this.textBoxMACAddress.TabIndex = 7;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 87);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(74, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "&MAC Address:";
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUpdate.Location = new System.Drawing.Point(226, 484);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(75, 23);
            this.buttonUpdate.TabIndex = 14;
            this.buttonUpdate.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ButtonUpdate;
            this.buttonUpdate.UseVisualStyleBackColor = true;
            this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(307, 484);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 15;
            this.buttonCancel.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ButtonCancel;
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 165);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(40, 13);
            this.label10.TabIndex = 12;
            this.label10.Text = "D&HCP:";
            // 
            // checkBoxDHCPEnable
            // 
            this.checkBoxDHCPEnable.AutoSize = true;
            this.checkBoxDHCPEnable.Location = new System.Drawing.Point(142, 165);
            this.checkBoxDHCPEnable.Name = "checkBoxDHCPEnable";
            this.checkBoxDHCPEnable.Size = new System.Drawing.Size(59, 17);
            this.checkBoxDHCPEnable.TabIndex = 13;
            this.checkBoxDHCPEnable.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.CheckBoxEnable;
            this.checkBoxDHCPEnable.UseVisualStyleBackColor = true;
            this.checkBoxDHCPEnable.CheckedChanged += new System.EventHandler(this.checkBoxDHCPEnable_CheckedChanged);
            // 
            // textBoxDnsPrimary
            // 
            this.textBoxDnsPrimary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDnsPrimary.Location = new System.Drawing.Point(142, 110);
            this.textBoxDnsPrimary.Name = "textBoxDnsPrimary";
            this.textBoxDnsPrimary.Size = new System.Drawing.Size(241, 20);
            this.textBoxDnsPrimary.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 113);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(111, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "&DNS Primary Address:";
            // 
            // textBoxDnsSecondary
            // 
            this.textBoxDnsSecondary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDnsSecondary.Location = new System.Drawing.Point(142, 136);
            this.textBoxDnsSecondary.Name = "textBoxDnsSecondary";
            this.textBoxDnsSecondary.Size = new System.Drawing.Size(241, 20);
            this.textBoxDnsSecondary.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 139);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(128, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "D&NS Secondary Address:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.comboBoxNetKey);
            this.groupBox1.Controls.Add(this.checkBox80211n);
            this.groupBox1.Controls.Add(this.checkBox80211g);
            this.groupBox1.Controls.Add(this.checkBox80211b);
            this.groupBox1.Controls.Add(this.checkBox80211a);
            this.groupBox1.Controls.Add(this.checkBoxEncryptConfigData);
            this.groupBox1.Controls.Add(this.comboBoxEncryption);
            this.groupBox1.Controls.Add(this.comboBoxAuthentication);
            this.groupBox1.Controls.Add(this.textBoxSSID);
            this.groupBox1.Controls.Add(this.labelSSID);
            this.groupBox1.Controls.Add(this.textBoxReKeyInternal);
            this.groupBox1.Controls.Add(this.labelReKeyInternal);
            this.groupBox1.Controls.Add(this.textBoxNetworkKey);
            this.groupBox1.Controls.Add(this.labelNetworkKey);
            this.groupBox1.Controls.Add(this.textBoxPassPhrase);
            this.groupBox1.Controls.Add(this.labelPassPhrase);
            this.groupBox1.Controls.Add(this.labelRadio);
            this.groupBox1.Controls.Add(this.labelEncryption);
            this.groupBox1.Controls.Add(this.labelAuthentication);
            this.groupBox1.Location = new System.Drawing.Point(15, 194);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(367, 284);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Wireless Configuration";
            // 
            // comboBoxNetKey
            // 
            this.comboBoxNetKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxNetKey.Enabled = false;
            this.comboBoxNetKey.FormattingEnabled = true;
            this.comboBoxNetKey.Items.AddRange(new object[] {
            "64-bit",
            "128-bit",
            "256-bit",
            "512-bit",
            "1024-bit",
            "2048-bit"});
            this.comboBoxNetKey.Location = new System.Drawing.Point(127, 171);
            this.comboBoxNetKey.Name = "comboBoxNetKey";
            this.comboBoxNetKey.Size = new System.Drawing.Size(220, 21);
            this.comboBoxNetKey.TabIndex = 26;
            this.comboBoxNetKey.SelectionChangeCommitted += new System.EventHandler(this.comboBoxNetKey_SelectionChangeCommitted);
            // 
            // checkBox80211n
            // 
            this.checkBox80211n.AutoSize = true;
            this.checkBox80211n.Location = new System.Drawing.Point(239, 99);
            this.checkBox80211n.Name = "checkBox80211n";
            this.checkBox80211n.Size = new System.Drawing.Size(65, 17);
            this.checkBox80211n.TabIndex = 25;
            this.checkBox80211n.Text = "802.11n";
            this.checkBox80211n.UseVisualStyleBackColor = true;
            // 
            // checkBox80211g
            // 
            this.checkBox80211g.AutoSize = true;
            this.checkBox80211g.Location = new System.Drawing.Point(127, 99);
            this.checkBox80211g.Name = "checkBox80211g";
            this.checkBox80211g.Size = new System.Drawing.Size(65, 17);
            this.checkBox80211g.TabIndex = 24;
            this.checkBox80211g.Text = "802.11g";
            this.checkBox80211g.UseVisualStyleBackColor = true;
            // 
            // checkBox80211b
            // 
            this.checkBox80211b.AutoSize = true;
            this.checkBox80211b.Location = new System.Drawing.Point(239, 76);
            this.checkBox80211b.Name = "checkBox80211b";
            this.checkBox80211b.Size = new System.Drawing.Size(65, 17);
            this.checkBox80211b.TabIndex = 23;
            this.checkBox80211b.Text = "802.11b";
            this.checkBox80211b.UseVisualStyleBackColor = true;
            // 
            // checkBox80211a
            // 
            this.checkBox80211a.AutoSize = true;
            this.checkBox80211a.Location = new System.Drawing.Point(127, 76);
            this.checkBox80211a.Name = "checkBox80211a";
            this.checkBox80211a.Size = new System.Drawing.Size(65, 17);
            this.checkBox80211a.TabIndex = 22;
            this.checkBox80211a.Text = "802.11a";
            this.checkBox80211a.UseVisualStyleBackColor = true;
            // 
            // checkBoxEncryptConfigData
            // 
            this.checkBoxEncryptConfigData.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBoxEncryptConfigData.Location = new System.Drawing.Point(5, 115);
            this.checkBoxEncryptConfigData.Name = "checkBoxEncryptConfigData";
            this.checkBoxEncryptConfigData.Size = new System.Drawing.Size(136, 24);
            this.checkBoxEncryptConfigData.TabIndex = 21;
            this.checkBoxEncryptConfigData.Text = "Encrypt Config Data";
            this.checkBoxEncryptConfigData.UseVisualStyleBackColor = true;
            // 
            // comboBoxEncryption
            // 
            this.comboBoxEncryption.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEncryption.Enabled = false;
            this.comboBoxEncryption.FormattingEnabled = true;
            this.comboBoxEncryption.Items.AddRange(new object[] {
            "None",
            "WEP",
            "WPA",
            "WPAPSK",
            "Certificate"});
            this.comboBoxEncryption.Location = new System.Drawing.Point(127, 46);
            this.comboBoxEncryption.Name = "comboBoxEncryption";
            this.comboBoxEncryption.Size = new System.Drawing.Size(220, 21);
            this.comboBoxEncryption.TabIndex = 19;
            // 
            // comboBoxAuthentication
            // 
            this.comboBoxAuthentication.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAuthentication.Enabled = false;
            this.comboBoxAuthentication.FormattingEnabled = true;
            this.comboBoxAuthentication.Items.AddRange(new object[] {
            "None",
            "EAP",
            "PEAP",
            "WCN",
            "Open",
            "Shared"});
            this.comboBoxAuthentication.Location = new System.Drawing.Point(127, 19);
            this.comboBoxAuthentication.Name = "comboBoxAuthentication";
            this.comboBoxAuthentication.Size = new System.Drawing.Size(220, 21);
            this.comboBoxAuthentication.TabIndex = 18;
            // 
            // textBoxSSID
            // 
            this.textBoxSSID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSSID.Enabled = false;
            this.textBoxSSID.Location = new System.Drawing.Point(127, 250);
            this.textBoxSSID.MaxLength = 31;
            this.textBoxSSID.Name = "textBoxSSID";
            this.textBoxSSID.Size = new System.Drawing.Size(220, 20);
            this.textBoxSSID.TabIndex = 17;
            // 
            // labelSSID
            // 
            this.labelSSID.AutoSize = true;
            this.labelSSID.Enabled = false;
            this.labelSSID.Location = new System.Drawing.Point(7, 253);
            this.labelSSID.Name = "labelSSID";
            this.labelSSID.Size = new System.Drawing.Size(32, 13);
            this.labelSSID.TabIndex = 16;
            this.labelSSID.Text = "SSID";
            // 
            // textBoxNetworkKey
            // 
            this.textBoxNetworkKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxNetworkKey.Enabled = false;
            this.textBoxNetworkKey.Location = new System.Drawing.Point(127, 198);
            this.textBoxNetworkKey.MaxLength = 16;
            this.textBoxNetworkKey.Name = "textBoxNetworkKey";
            this.textBoxNetworkKey.Size = new System.Drawing.Size(220, 20);
            this.textBoxNetworkKey.TabIndex = 13;
            // 
            // labelNetworkKey
            // 
            this.labelNetworkKey.AutoSize = true;
            this.labelNetworkKey.Enabled = false;
            this.labelNetworkKey.Location = new System.Drawing.Point(6, 176);
            this.labelNetworkKey.Name = "labelNetworkKey";
            this.labelNetworkKey.Size = new System.Drawing.Size(68, 13);
            this.labelNetworkKey.TabIndex = 12;
            this.labelNetworkKey.Text = "Network Key";
            // 
            // textBoxPassPhrase
            // 
            this.textBoxPassPhrase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPassPhrase.Enabled = false;
            this.textBoxPassPhrase.Location = new System.Drawing.Point(127, 145);
            this.textBoxPassPhrase.MaxLength = 63;
            this.textBoxPassPhrase.Name = "textBoxPassPhrase";
            this.textBoxPassPhrase.Size = new System.Drawing.Size(220, 20);
            this.textBoxPassPhrase.TabIndex = 11;
            // 
            // labelPassPhrase
            // 
            this.labelPassPhrase.AutoSize = true;
            this.labelPassPhrase.Enabled = false;
            this.labelPassPhrase.Location = new System.Drawing.Point(6, 148);
            this.labelPassPhrase.Name = "labelPassPhrase";
            this.labelPassPhrase.Size = new System.Drawing.Size(65, 13);
            this.labelPassPhrase.TabIndex = 10;
            this.labelPassPhrase.Text = "Pass phrase";
            // 
            // labelRadio
            // 
            this.labelRadio.AutoSize = true;
            this.labelRadio.Enabled = false;
            this.labelRadio.Location = new System.Drawing.Point(6, 76);
            this.labelRadio.Name = "labelRadio";
            this.labelRadio.Size = new System.Drawing.Size(35, 13);
            this.labelRadio.TabIndex = 3;
            this.labelRadio.Text = "Radio";
            // 
            // labelEncryption
            // 
            this.labelEncryption.AutoSize = true;
            this.labelEncryption.Enabled = false;
            this.labelEncryption.Location = new System.Drawing.Point(6, 49);
            this.labelEncryption.Name = "labelEncryption";
            this.labelEncryption.Size = new System.Drawing.Size(57, 13);
            this.labelEncryption.TabIndex = 2;
            this.labelEncryption.Text = "Encryption";
            // 
            // labelAuthentication
            // 
            this.labelAuthentication.AutoSize = true;
            this.labelAuthentication.Enabled = false;
            this.labelAuthentication.Location = new System.Drawing.Point(6, 22);
            this.labelAuthentication.Name = "labelAuthentication";
            this.labelAuthentication.Size = new System.Drawing.Size(75, 13);
            this.labelAuthentication.TabIndex = 1;
            this.labelAuthentication.Text = "Authentication";
            // 
            // textBoxReKeyInternal
            // 
            this.textBoxReKeyInternal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxReKeyInternal.Enabled = false;
            this.textBoxReKeyInternal.Location = new System.Drawing.Point(127, 224);
            this.textBoxReKeyInternal.MaxLength = 64;
            this.textBoxReKeyInternal.Name = "textBoxReKeyInternal";
            this.textBoxReKeyInternal.Size = new System.Drawing.Size(220, 20);
            this.textBoxReKeyInternal.TabIndex = 15;
            // 
            // labelReKeyInternal
            // 
            this.labelReKeyInternal.AutoSize = true;
            this.labelReKeyInternal.Enabled = false;
            this.labelReKeyInternal.Location = new System.Drawing.Point(6, 227);
            this.labelReKeyInternal.Name = "labelReKeyInternal";
            this.labelReKeyInternal.Size = new System.Drawing.Size(77, 13);
            this.labelReKeyInternal.TabIndex = 14;
            this.labelReKeyInternal.Text = "ReKey Internal";
            // 
            // MFNetworkConfigDialog
            // 
            this.AcceptButton = this.buttonUpdate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(395, 519);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.textBoxDnsSecondary);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxDnsPrimary);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.checkBoxDHCPEnable);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonUpdate);
            this.Controls.Add(this.textBoxMACAddress);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBoxDefaultGateway);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxSubnetMask);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxIPAddress);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MFNetworkConfigDialog";
            this.ShowInTaskbar = false;
            this.Text = "Network Configuration";
            this.Load += new System.EventHandler(this.ConfigDialog_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MFNetworkConfigDialog_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxIPAddress;
        private System.Windows.Forms.TextBox textBoxSubnetMask;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxDefaultGateway;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxMACAddress;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox checkBoxDHCPEnable;
        private System.Windows.Forms.TextBox textBoxDnsPrimary;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxDnsSecondary;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBoxSSID;
        private System.Windows.Forms.Label labelSSID;
        private System.Windows.Forms.TextBox textBoxNetworkKey;
        private System.Windows.Forms.Label labelNetworkKey;
        private System.Windows.Forms.TextBox textBoxPassPhrase;
        private System.Windows.Forms.Label labelPassPhrase;
        private System.Windows.Forms.Label labelRadio;
        private System.Windows.Forms.Label labelEncryption;
        private System.Windows.Forms.Label labelAuthentication;
        private System.Windows.Forms.ComboBox comboBoxEncryption;
        private System.Windows.Forms.ComboBox comboBoxAuthentication;
        private System.Windows.Forms.CheckBox checkBoxEncryptConfigData;
        private System.Windows.Forms.CheckBox checkBox80211n;
        private System.Windows.Forms.CheckBox checkBox80211g;
        private System.Windows.Forms.CheckBox checkBox80211b;
        private System.Windows.Forms.CheckBox checkBox80211a;
        private System.Windows.Forms.ComboBox comboBoxNetKey;
        private System.Windows.Forms.TextBox textBoxReKeyInternal;
        private System.Windows.Forms.Label labelReKeyInternal;
    }
}