namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.groupBoxDevice = new System.Windows.Forms.GroupBox();
            this.buttonBrowseCert = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxCertPwd = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxCert = new System.Windows.Forms.TextBox();
            this.checkBoxUseSSL = new System.Windows.Forms.CheckBox();
            this.comboBoxTransport = new System.Windows.Forms.ComboBox();
            this.buttonErase = new System.Windows.Forms.Button();
            this.buttonPing = new System.Windows.Forms.Button();
            this.comboBoxDevice = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonDeploy = new System.Windows.Forms.Button();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.comboBoxImageFile = new System.Windows.Forms.ComboBox();
            this.richTextBoxOutput = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.targetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.applicationDeploymentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createApplicationDeploymentToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.signDeploymentFileToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.publicKeyConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createKeyPairToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateDeviceKeysToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateSSLKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uSBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uSBConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.networkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deviceCapabilitiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cancelToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.pluginToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.cancelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defaultSerialPortToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearFileListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timeStampToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enalbePermiscuousWinUSBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpTopicsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutMFDeployToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listViewFiles = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBaseAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderTimeStamp = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBoxDevice.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxDevice
            // 
            this.groupBoxDevice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDevice.Controls.Add(this.buttonBrowseCert);
            this.groupBoxDevice.Controls.Add(this.label2);
            this.groupBoxDevice.Controls.Add(this.textBoxCertPwd);
            this.groupBoxDevice.Controls.Add(this.label1);
            this.groupBoxDevice.Controls.Add(this.textBoxCert);
            this.groupBoxDevice.Controls.Add(this.checkBoxUseSSL);
            this.groupBoxDevice.Controls.Add(this.comboBoxTransport);
            this.groupBoxDevice.Controls.Add(this.buttonErase);
            this.groupBoxDevice.Controls.Add(this.buttonPing);
            this.groupBoxDevice.Controls.Add(this.comboBoxDevice);
            this.groupBoxDevice.Location = new System.Drawing.Point(12, 34);
            this.groupBoxDevice.Name = "groupBoxDevice";
            this.groupBoxDevice.Size = new System.Drawing.Size(673, 110);
            this.groupBoxDevice.TabIndex = 0;
            this.groupBoxDevice.TabStop = false;
            this.groupBoxDevice.Text = "De&vice";
            // 
            // buttonBrowseCert
            // 
            this.buttonBrowseCert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowseCert.Location = new System.Drawing.Point(511, 56);
            this.buttonBrowseCert.Name = "buttonBrowseCert";
            this.buttonBrowseCert.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseCert.TabIndex = 9;
            this.buttonBrowseCert.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ButtonBrowse;
            this.buttonBrowseCert.UseVisualStyleBackColor = true;
            this.buttonBrowseCert.Click += new System.EventHandler(this.buttonBrowseCert_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(99, 86);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Password:";
            // 
            // textBoxCertPwd
            // 
            this.textBoxCertPwd.Location = new System.Drawing.Point(161, 83);
            this.textBoxCertPwd.Name = "textBoxCertPwd";
            this.textBoxCertPwd.PasswordChar = '*';
            this.textBoxCertPwd.Size = new System.Drawing.Size(344, 20);
            this.textBoxCertPwd.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(99, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Certificate:";
            // 
            // textBoxCert
            // 
            this.textBoxCert.Location = new System.Drawing.Point(161, 57);
            this.textBoxCert.Name = "textBoxCert";
            this.textBoxCert.Size = new System.Drawing.Size(344, 20);
            this.textBoxCert.TabIndex = 5;
            // 
            // checkBoxUseSSL
            // 
            this.checkBoxUseSSL.AutoSize = true;
            this.checkBoxUseSSL.Enabled = false;
            this.checkBoxUseSSL.Location = new System.Drawing.Point(6, 59);
            this.checkBoxUseSSL.Name = "checkBoxUseSSL";
            this.checkBoxUseSSL.Size = new System.Drawing.Size(68, 17);
            this.checkBoxUseSSL.TabIndex = 4;
            this.checkBoxUseSSL.Text = "Use SSL";
            this.checkBoxUseSSL.UseVisualStyleBackColor = true;
            this.checkBoxUseSSL.CheckedChanged += new System.EventHandler(this.checkBoxUseSSL_CheckedChanged);
            this.checkBoxUseSSL.EnabledChanged += new System.EventHandler(this.checkBoxUseSSL_EnabledChanged);
            // 
            // comboBoxTransport
            // 
            this.comboBoxTransport.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTransport.FormattingEnabled = true;
            this.comboBoxTransport.Items.AddRange(new object[] {
            global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.TransportSerial,
            global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.TransportUsb,
            global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.TransportTcpIp});
            this.comboBoxTransport.Location = new System.Drawing.Point(6, 19);
            this.comboBoxTransport.Name = "comboBoxTransport";
            this.comboBoxTransport.Size = new System.Drawing.Size(87, 21);
            this.comboBoxTransport.TabIndex = 3;
            this.comboBoxTransport.SelectedIndexChanged += new System.EventHandler(this.comboBoxTransport_SelectedIndexChanged);
            // 
            // buttonErase
            // 
            this.buttonErase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonErase.Location = new System.Drawing.Point(592, 17);
            this.buttonErase.Name = "buttonErase";
            this.buttonErase.Size = new System.Drawing.Size(75, 23);
            this.buttonErase.TabIndex = 2;
            this.buttonErase.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ButtonErase;
            this.buttonErase.UseVisualStyleBackColor = true;
            this.buttonErase.Click += new System.EventHandler(this.buttonErase_Click);
            // 
            // buttonPing
            // 
            this.buttonPing.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPing.Location = new System.Drawing.Point(511, 17);
            this.buttonPing.Name = "buttonPing";
            this.buttonPing.Size = new System.Drawing.Size(75, 23);
            this.buttonPing.TabIndex = 1;
            this.buttonPing.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ButtonPing;
            this.buttonPing.UseVisualStyleBackColor = true;
            this.buttonPing.Click += new System.EventHandler(this.buttonPing_Click);
            // 
            // comboBoxDevice
            // 
            this.comboBoxDevice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxDevice.FormattingEnabled = true;
            this.comboBoxDevice.Location = new System.Drawing.Point(99, 19);
            this.comboBoxDevice.Name = "comboBoxDevice";
            this.comboBoxDevice.Size = new System.Drawing.Size(406, 21);
            this.comboBoxDevice.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.buttonDeploy);
            this.groupBox1.Controls.Add(this.buttonBrowse);
            this.groupBox1.Controls.Add(this.comboBoxImageFile);
            this.groupBox1.Location = new System.Drawing.Point(12, 150);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(673, 50);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "&Image File";
            // 
            // buttonDeploy
            // 
            this.buttonDeploy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDeploy.Location = new System.Drawing.Point(592, 17);
            this.buttonDeploy.Name = "buttonDeploy";
            this.buttonDeploy.Size = new System.Drawing.Size(75, 23);
            this.buttonDeploy.TabIndex = 2;
            this.buttonDeploy.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ButtonDeploy;
            this.buttonDeploy.UseVisualStyleBackColor = true;
            this.buttonDeploy.Click += new System.EventHandler(this.buttonDeploy_Click);
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowse.Location = new System.Drawing.Point(511, 17);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowse.TabIndex = 1;
            this.buttonBrowse.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ButtonBrowse;
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // comboBoxImageFile
            // 
            this.comboBoxImageFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxImageFile.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.comboBoxImageFile.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.comboBoxImageFile.FormattingEnabled = true;
            this.comboBoxImageFile.Location = new System.Drawing.Point(6, 19);
            this.comboBoxImageFile.Name = "comboBoxImageFile";
            this.comboBoxImageFile.Size = new System.Drawing.Size(499, 21);
            this.comboBoxImageFile.TabIndex = 0;
            this.comboBoxImageFile.DropDown += new System.EventHandler(this.comboBoxImageFile_DropDown);
            this.comboBoxImageFile.SelectedIndexChanged += new System.EventHandler(this.comboBoxImageFile_SelectedIndexChanged);
            this.comboBoxImageFile.KeyDown += new System.Windows.Forms.KeyEventHandler(this.comboBoxImageFile_KeyDown);
            // 
            // richTextBoxOutput
            // 
            this.richTextBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxOutput.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxOutput.Location = new System.Drawing.Point(18, 324);
            this.richTextBoxOutput.Name = "richTextBoxOutput";
            this.richTextBoxOutput.ReadOnly = true;
            this.richTextBoxOutput.Size = new System.Drawing.Size(661, 295);
            this.richTextBoxOutput.TabIndex = 2;
            this.richTextBoxOutput.Text = "";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(18, 625);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ButtonClear;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.targetToolStripMenuItem,
            this.pluginToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(697, 24);
            this.menuStrip1.TabIndex = 8;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // targetToolStripMenuItem
            // 
            this.targetToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.applicationDeploymentToolStripMenuItem,
            this.publicKeyConfigurationToolStripMenuItem,
            this.configurationToolStripMenuItem,
            this.deviceCapabilitiesToolStripMenuItem,
            this.toolStripSeparator4,
            this.connectToolStripMenuItem,
            this.cancelToolStripMenuItem1});
            this.targetToolStripMenuItem.Name = "targetToolStripMenuItem";
            this.targetToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.targetToolStripMenuItem.Text = "Target";
            // 
            // applicationDeploymentToolStripMenuItem
            // 
            this.applicationDeploymentToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createApplicationDeploymentToolStripMenuItem1,
            this.signDeploymentFileToolStripMenuItem1});
            this.applicationDeploymentToolStripMenuItem.Name = "applicationDeploymentToolStripMenuItem";
            this.applicationDeploymentToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.applicationDeploymentToolStripMenuItem.Text = "Application Deployment";
            // 
            // createApplicationDeploymentToolStripMenuItem1
            // 
            this.createApplicationDeploymentToolStripMenuItem1.Name = "createApplicationDeploymentToolStripMenuItem1";
            this.createApplicationDeploymentToolStripMenuItem1.Size = new System.Drawing.Size(240, 22);
            this.createApplicationDeploymentToolStripMenuItem1.Text = "Create Application Deployment";
            this.createApplicationDeploymentToolStripMenuItem1.Click += new System.EventHandler(this.OnMenuItem_Click);
            // 
            // signDeploymentFileToolStripMenuItem1
            // 
            this.signDeploymentFileToolStripMenuItem1.Name = "signDeploymentFileToolStripMenuItem1";
            this.signDeploymentFileToolStripMenuItem1.Size = new System.Drawing.Size(240, 22);
            this.signDeploymentFileToolStripMenuItem1.Text = "Sign Deployment File";
            this.signDeploymentFileToolStripMenuItem1.Click += new System.EventHandler(this.OnMenuItem_Click);
            // 
            // publicKeyConfigurationToolStripMenuItem
            // 
            this.publicKeyConfigurationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createKeyPairToolStripMenuItem,
            this.updateDeviceKeysToolStripMenuItem,
            this.updateSSLKeyToolStripMenuItem});
            this.publicKeyConfigurationToolStripMenuItem.Name = "publicKeyConfigurationToolStripMenuItem";
            this.publicKeyConfigurationToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.publicKeyConfigurationToolStripMenuItem.Text = "Manage Device Keys";
            // 
            // createKeyPairToolStripMenuItem
            // 
            this.createKeyPairToolStripMenuItem.Name = "createKeyPairToolStripMenuItem";
            this.createKeyPairToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.createKeyPairToolStripMenuItem.Text = "Create Key Pair";
            this.createKeyPairToolStripMenuItem.Click += new System.EventHandler(this.createKeyPairToolStripMenuItem_Click);
            // 
            // updateDeviceKeysToolStripMenuItem
            // 
            this.updateDeviceKeysToolStripMenuItem.Name = "updateDeviceKeysToolStripMenuItem";
            this.updateDeviceKeysToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.updateDeviceKeysToolStripMenuItem.Text = "Update Device Keys";
            this.updateDeviceKeysToolStripMenuItem.Click += new System.EventHandler(this.OnMenuItem_Click);
            // 
            // updateSSLKeyToolStripMenuItem
            // 
            this.updateSSLKeyToolStripMenuItem.Name = "updateSSLKeyToolStripMenuItem";
            this.updateSSLKeyToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.updateSSLKeyToolStripMenuItem.Text = "Update SSL Seed";
            this.updateSSLKeyToolStripMenuItem.Click += new System.EventHandler(this.updateSSLKeyToolStripMenuItem_Click);
            // 
            // configurationToolStripMenuItem
            // 
            this.configurationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.uSBToolStripMenuItem,
            this.uSBConfigurationToolStripMenuItem,
            this.networkToolStripMenuItem});
            this.configurationToolStripMenuItem.Name = "configurationToolStripMenuItem";
            this.configurationToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.configurationToolStripMenuItem.Text = "Configuration";
            // 
            // uSBToolStripMenuItem
            // 
            this.uSBToolStripMenuItem.Name = "uSBToolStripMenuItem";
            this.uSBToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.uSBToolStripMenuItem.Text = "USB Name";
            this.uSBToolStripMenuItem.Click += new System.EventHandler(this.OnMenuItem_Click);
            // 
            // uSBConfigurationToolStripMenuItem
            // 
            this.uSBConfigurationToolStripMenuItem.Name = "uSBConfigurationToolStripMenuItem";
            this.uSBConfigurationToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.uSBConfigurationToolStripMenuItem.Text = "USB Configuration";
            this.uSBConfigurationToolStripMenuItem.Click += new System.EventHandler(this.OnMenuItem_Click);
            // 
            // networkToolStripMenuItem
            // 
            this.networkToolStripMenuItem.Name = "networkToolStripMenuItem";
            this.networkToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.networkToolStripMenuItem.Text = "Network";
            this.networkToolStripMenuItem.Click += new System.EventHandler(this.OnMenuItem_Click);
            // 
            // deviceCapabilitiesToolStripMenuItem
            // 
            this.deviceCapabilitiesToolStripMenuItem.Name = "deviceCapabilitiesToolStripMenuItem";
            this.deviceCapabilitiesToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.C)));
            this.deviceCapabilitiesToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.deviceCapabilitiesToolStripMenuItem.Text = "Device Capabilities";
            this.deviceCapabilitiesToolStripMenuItem.Click += new System.EventHandler(this.cLRCapabilitiesToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(244, 6);
            // 
            // connectToolStripMenuItem
            // 
            this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            this.connectToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.connectToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.connectToolStripMenuItem.Text = "Connect";
            this.connectToolStripMenuItem.Click += new System.EventHandler(this.listenToolStripMenuItem_Click);
            // 
            // cancelToolStripMenuItem1
            // 
            this.cancelToolStripMenuItem1.Name = "cancelToolStripMenuItem1";
            this.cancelToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F5)));
            this.cancelToolStripMenuItem1.Size = new System.Drawing.Size(247, 22);
            this.cancelToolStripMenuItem1.Text = "Disconnect";
            this.cancelToolStripMenuItem1.Click += new System.EventHandler(this.cancelToolStripMenuItem1_Click_1);
            // 
            // pluginToolStripMenuItem
            // 
            this.pluginToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.cancelToolStripMenuItem});
            this.pluginToolStripMenuItem.Name = "pluginToolStripMenuItem";
            this.pluginToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.pluginToolStripMenuItem.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ToolStripMenuPlugIn;
            this.pluginToolStripMenuItem.Visible = false;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(64, 6);
            // 
            // cancelToolStripMenuItem
            // 
            this.cancelToolStripMenuItem.Name = "cancelToolStripMenuItem";
            this.cancelToolStripMenuItem.Size = new System.Drawing.Size(67, 22);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.defaultSerialPortToolStripMenuItem,
            this.clearFileListToolStripMenuItem,
            this.timeStampToolStripMenuItem,
            this.enalbePermiscuousWinUSBToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ToolStripMenuOptions;
            // 
            // defaultSerialPortToolStripMenuItem
            // 
            this.defaultSerialPortToolStripMenuItem.Name = "defaultSerialPortToolStripMenuItem";
            this.defaultSerialPortToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.defaultSerialPortToolStripMenuItem.Text = "Bootloader Serial Port";
            // 
            // clearFileListToolStripMenuItem
            // 
            this.clearFileListToolStripMenuItem.Name = "clearFileListToolStripMenuItem";
            this.clearFileListToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.clearFileListToolStripMenuItem.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ToolStripMenuItemImageFileList;
            this.clearFileListToolStripMenuItem.Click += new System.EventHandler(this.clearFileListToolStripMenuItem_Click);
            // 
            // timeStampToolStripMenuItem
            // 
            this.timeStampToolStripMenuItem.CheckOnClick = true;
            this.timeStampToolStripMenuItem.Name = "timeStampToolStripMenuItem";
            this.timeStampToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.timeStampToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.timeStampToolStripMenuItem.Text = "Time Stamp";
            // 
            // enalbePermiscuousWinUSBToolStripMenuItem
            // 
            this.enalbePermiscuousWinUSBToolStripMenuItem.Name = "enalbePermiscuousWinUSBToolStripMenuItem";
            this.enalbePermiscuousWinUSBToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.enalbePermiscuousWinUSBToolStripMenuItem.Text = "Enable Permiscuous WinUSB";
            this.enalbePermiscuousWinUSBToolStripMenuItem.ToolTipText = "Enables enumeration of legacy WinUSB devices that don\'t advertise the .NET Micro " +
    "Framework debug interface explicitly (can cause false positives)";
            this.enalbePermiscuousWinUSBToolStripMenuItem.Click += new System.EventHandler(this.OnPermiscuousWinUsbClicked);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpTopicsToolStripMenuItem,
            this.aboutMFDeployToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // helpTopicsToolStripMenuItem
            // 
            this.helpTopicsToolStripMenuItem.Name = "helpTopicsToolStripMenuItem";
            this.helpTopicsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.helpTopicsToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.helpTopicsToolStripMenuItem.Text = "&Help Topics";
            this.helpTopicsToolStripMenuItem.Click += new System.EventHandler(this.helpTopicsToolStripMenuItem_Click);
            // 
            // aboutMFDeployToolStripMenuItem
            // 
            this.aboutMFDeployToolStripMenuItem.Name = "aboutMFDeployToolStripMenuItem";
            this.aboutMFDeployToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.aboutMFDeployToolStripMenuItem.Text = "&About MFDeploy";
            this.aboutMFDeployToolStripMenuItem.Click += new System.EventHandler(this.aboutMFDeployToolStripMenuItem_Click);
            // 
            // listViewFiles
            // 
            this.listViewFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewFiles.BackColor = System.Drawing.SystemColors.Window;
            this.listViewFiles.CheckBoxes = true;
            this.listViewFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderFile,
            this.columnHeaderBaseAddress,
            this.columnHeaderSize,
            this.columnHeaderTimeStamp});
            this.listViewFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewFiles.FullRowSelect = true;
            this.listViewFiles.GridLines = true;
            this.listViewFiles.HideSelection = false;
            this.listViewFiles.Location = new System.Drawing.Point(18, 206);
            this.listViewFiles.Name = "listViewFiles";
            this.listViewFiles.Size = new System.Drawing.Size(661, 112);
            this.listViewFiles.TabIndex = 9;
            this.listViewFiles.UseCompatibleStateImageBehavior = false;
            this.listViewFiles.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Tag = "";
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 123;
            // 
            // columnHeaderFile
            // 
            this.columnHeaderFile.Tag = "";
            this.columnHeaderFile.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ColumnHeaderFile;
            this.columnHeaderFile.Width = 249;
            // 
            // columnHeaderBaseAddress
            // 
            this.columnHeaderBaseAddress.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ColumnHeaderBaseAddr;
            this.columnHeaderBaseAddress.Width = 80;
            // 
            // columnHeaderSize
            // 
            this.columnHeaderSize.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ColumnHeaderSize;
            this.columnHeaderSize.Width = 68;
            // 
            // columnHeaderTimeStamp
            // 
            this.columnHeaderTimeStamp.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ColumnHeaderTimeStamp;
            this.columnHeaderTimeStamp.Width = 135;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(697, 657);
            this.Controls.Add(this.listViewFiles);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBoxDevice);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.richTextBoxOutput);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(300, 250);
            this.Name = "Form1";
            this.Text = ".NET Micro Framework Deployment Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBoxDevice.ResumeLayout(false);
            this.groupBoxDevice.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxDevice;
        private System.Windows.Forms.Button buttonErase;
        private System.Windows.Forms.Button buttonPing;
        private System.Windows.Forms.ComboBox comboBoxDevice;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonDeploy;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.ComboBox comboBoxImageFile;
        private System.Windows.Forms.RichTextBox richTextBoxOutput;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem pluginToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cancelToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ListView listViewFiles;
        private System.Windows.Forms.ColumnHeader columnHeaderFile;
        private System.Windows.Forms.ColumnHeader columnHeaderBaseAddress;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
        private System.Windows.Forms.ColumnHeader columnHeaderTimeStamp;
        private System.Windows.Forms.ComboBox comboBoxTransport;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defaultSerialPortToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearFileListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem targetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem publicKeyConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem configurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uSBToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem networkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem applicationDeploymentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createApplicationDeploymentToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem signDeploymentFileToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem createKeyPairToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateDeviceKeysToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpTopicsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutMFDeployToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem timeStampToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cancelToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ToolStripMenuItem deviceCapabilitiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uSBConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateSSLKeyToolStripMenuItem;
        private System.Windows.Forms.Button buttonBrowseCert;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxCertPwd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxCert;
        private System.Windows.Forms.CheckBox checkBoxUseSSL;
        private System.Windows.Forms.ToolStripMenuItem enalbePermiscuousWinUSBToolStripMenuItem;
    }
}

