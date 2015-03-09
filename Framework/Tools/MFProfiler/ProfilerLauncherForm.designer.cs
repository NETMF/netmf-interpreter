namespace Microsoft.NetMicroFramework.Tools.MFProfilerTool
{
    partial class ProfilerLauncherForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProfilerLauncherForm));
            this.groupBoxDevice = new System.Windows.Forms.GroupBox();
            this.comboBoxDevice = new Microsoft.NetMicroFramework.Tools.MFProfilerTool.MFDeviceSelectorComboBox();
            this.comboBoxTransport = new Microsoft.NetMicroFramework.Tools.MFProfilerTool.MFPortFilterComboBox();
            this.textLog = new System.Windows.Forms.TextBox();
            this.bwConnecter = new System.ComponentModel.BackgroundWorker();
            this.groupBoxOutput = new System.Windows.Forms.GroupBox();
            this.labelFile = new System.Windows.Forms.Label();
            this.buttonLogFileBrowse = new System.Windows.Forms.Button();
            this.textLogFile = new System.Windows.Forms.TextBox();
            this.radioOffProf = new System.Windows.Forms.RadioButton();
            this.radioCLRProfiler = new System.Windows.Forms.RadioButton();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.buttonConnect = new System.Windows.Forms.Button();
            this.groupBoxOptions = new System.Windows.Forms.GroupBox();
            this.checkAllocations = new System.Windows.Forms.CheckBox();
            this.checkCalls = new System.Windows.Forms.CheckBox();
            this.checkReboot = new System.Windows.Forms.CheckBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewSummaryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewHistogramAllocatedMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewHistogramRelocatedMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewHistogramFinalizerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewHistogramCriticalFinalizerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewObjectsByAddressMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewHistogramByAgeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewAllocationGraphMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewAssemblyGraphMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewFunctionGraphMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewModuleGraphMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewClassGraphMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewHeapGraphMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewCallGraphMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewTimeLineMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewCommentsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewCallTreeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnClear = new System.Windows.Forms.Button();
            this.groupBoxDevice.SuspendLayout();
            this.groupBoxOutput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.groupBoxOptions.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxDevice
            // 
            this.groupBoxDevice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDevice.Controls.Add(this.comboBoxDevice);
            this.groupBoxDevice.Controls.Add(this.comboBoxTransport);
            this.groupBoxDevice.Location = new System.Drawing.Point(12, 27);
            this.groupBoxDevice.Name = "groupBoxDevice";
            this.groupBoxDevice.Size = new System.Drawing.Size(518, 53);
            this.groupBoxDevice.TabIndex = 0;
            this.groupBoxDevice.TabStop = false;
            this.groupBoxDevice.Text = "De&vice";
            this.groupBoxDevice.Validating += new System.ComponentModel.CancelEventHandler(this.groupBoxDevice_Validating);
            // 
            // comboBoxDevice
            // 
            this.comboBoxDevice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxDevice.FormattingEnabled = true;
            this.comboBoxDevice.Location = new System.Drawing.Point(123, 19);
            this.comboBoxDevice.Name = "comboBoxDevice";
            this.comboBoxDevice.Size = new System.Drawing.Size(389, 21);
            this.comboBoxDevice.TabIndex = 2;
            this.comboBoxDevice.SelectedValueChanged += new System.EventHandler(this.comboBoxDevice_SelectedValueChanged);
            // 
            // comboBoxTransport
            // 
            this.comboBoxTransport.FormattingEnabled = true;
            this.comboBoxTransport.Location = new System.Drawing.Point(6, 19);
            this.comboBoxTransport.Name = "comboBoxTransport";
            this.comboBoxTransport.Size = new System.Drawing.Size(111, 21);
            this.comboBoxTransport.TabIndex = 1;
            this.comboBoxTransport.SelectedValueChanged += new System.EventHandler(this.comboBoxTransport_SelectedValueChanged);
            // 
            // textLog
            // 
            this.textLog.AcceptsReturn = true;
            this.textLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textLog.BackColor = System.Drawing.SystemColors.Control;
            this.textLog.Location = new System.Drawing.Point(12, 252);
            this.textLog.Multiline = true;
            this.textLog.Name = "textLog";
            this.textLog.ReadOnly = true;
            this.textLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textLog.Size = new System.Drawing.Size(518, 94);
            this.textLog.TabIndex = 0;
            this.textLog.TabStop = false;
            // 
            // bwConnecter
            // 
            this.bwConnecter.WorkerSupportsCancellation = true;
            this.bwConnecter.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwConnecter_DoWork);
            this.bwConnecter.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bwConnecter_RunWorkerCompleted);
            // 
            // groupBoxOutput
            // 
            this.groupBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOutput.Controls.Add(this.labelFile);
            this.groupBoxOutput.Controls.Add(this.buttonLogFileBrowse);
            this.groupBoxOutput.Controls.Add(this.textLogFile);
            this.groupBoxOutput.Controls.Add(this.radioOffProf);
            this.groupBoxOutput.Controls.Add(this.radioCLRProfiler);
            this.groupBoxOutput.Location = new System.Drawing.Point(12, 86);
            this.groupBoxOutput.Name = "groupBoxOutput";
            this.groupBoxOutput.Size = new System.Drawing.Size(518, 69);
            this.groupBoxOutput.TabIndex = 4;
            this.groupBoxOutput.TabStop = false;
            this.groupBoxOutput.Text = "&Output";
            this.groupBoxOutput.Validating += new System.ComponentModel.CancelEventHandler(this.groupBoxOutput_Validating);
            // 
            // labelFile
            // 
            this.labelFile.AutoSize = true;
            this.labelFile.Location = new System.Drawing.Point(6, 20);
            this.labelFile.Name = "labelFile";
            this.labelFile.Size = new System.Drawing.Size(61, 13);
            this.labelFile.TabIndex = 3;
            this.labelFile.Text = "Output File:";
            // 
            // buttonLogFileBrowse
            // 
            this.buttonLogFileBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLogFileBrowse.Location = new System.Drawing.Point(437, 15);
            this.buttonLogFileBrowse.Name = "buttonLogFileBrowse";
            this.buttonLogFileBrowse.Size = new System.Drawing.Size(75, 23);
            this.buttonLogFileBrowse.TabIndex = 5;
            this.buttonLogFileBrowse.Text = "&Browse...";
            this.buttonLogFileBrowse.UseVisualStyleBackColor = true;
            this.buttonLogFileBrowse.Click += new System.EventHandler(this.buttonLogFileBrowse_Click);
            // 
            // textLogFile
            // 
            this.textLogFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textLogFile.Location = new System.Drawing.Point(73, 17);
            this.textLogFile.Name = "textLogFile";
            this.textLogFile.Size = new System.Drawing.Size(358, 20);
            this.textLogFile.TabIndex = 4;
            // 
            // radioOffProf
            // 
            this.radioOffProf.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.radioOffProf.AutoSize = true;
            this.radioOffProf.Location = new System.Drawing.Point(90, 43);
            this.radioOffProf.Name = "radioOffProf";
            this.radioOffProf.Size = new System.Drawing.Size(58, 17);
            this.radioOffProf.TabIndex = 7;
            this.radioOffProf.Text = "OffProf";
            this.radioOffProf.UseVisualStyleBackColor = true;
            // 
            // radioCLRProfiler
            // 
            this.radioCLRProfiler.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.radioCLRProfiler.AutoSize = true;
            this.radioCLRProfiler.Checked = true;
            this.radioCLRProfiler.Location = new System.Drawing.Point(6, 43);
            this.radioCLRProfiler.Name = "radioCLRProfiler";
            this.radioCLRProfiler.Size = new System.Drawing.Size(78, 17);
            this.radioCLRProfiler.TabIndex = 6;
            this.radioCLRProfiler.TabStop = true;
            this.radioCLRProfiler.Text = "CLRProfiler";
            this.radioCLRProfiler.UseVisualStyleBackColor = true;
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // buttonConnect
            // 
            this.buttonConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConnect.Location = new System.Drawing.Point(449, 194);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(75, 52);
            this.buttonConnect.TabIndex = 12;
            this.buttonConnect.Text = "&Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // groupBoxOptions
            // 
            this.groupBoxOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOptions.Controls.Add(this.checkAllocations);
            this.groupBoxOptions.Controls.Add(this.checkCalls);
            this.groupBoxOptions.Controls.Add(this.checkReboot);
            this.groupBoxOptions.Location = new System.Drawing.Point(12, 161);
            this.groupBoxOptions.Name = "groupBoxOptions";
            this.groupBoxOptions.Size = new System.Drawing.Size(431, 85);
            this.groupBoxOptions.TabIndex = 8;
            this.groupBoxOptions.TabStop = false;
            this.groupBoxOptions.Text = "&Profiling Options";
            // 
            // checkAllocations
            // 
            this.checkAllocations.AutoSize = true;
            this.checkAllocations.Checked = true;
            this.checkAllocations.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkAllocations.Location = new System.Drawing.Point(6, 62);
            this.checkAllocations.Name = "checkAllocations";
            this.checkAllocations.Size = new System.Drawing.Size(143, 17);
            this.checkAllocations.TabIndex = 11;
            this.checkAllocations.Text = "Profile Object &Allocations";
            this.checkAllocations.UseVisualStyleBackColor = true;
            this.checkAllocations.CheckedChanged += new System.EventHandler(this.SetProfilingOptions);
            // 
            // checkCalls
            // 
            this.checkCalls.AutoSize = true;
            this.checkCalls.Checked = true;
            this.checkCalls.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkCalls.Location = new System.Drawing.Point(6, 39);
            this.checkCalls.Name = "checkCalls";
            this.checkCalls.Size = new System.Drawing.Size(124, 17);
            this.checkCalls.TabIndex = 10;
            this.checkCalls.Text = "Profile &Function Calls";
            this.checkCalls.UseVisualStyleBackColor = true;
            this.checkCalls.CheckedChanged += new System.EventHandler(this.SetProfilingOptions);
            // 
            // checkReboot
            // 
            this.checkReboot.AutoSize = true;
            this.checkReboot.Checked = true;
            this.checkReboot.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkReboot.Location = new System.Drawing.Point(6, 19);
            this.checkReboot.Name = "checkReboot";
            this.checkReboot.Size = new System.Drawing.Size(149, 17);
            this.checkReboot.TabIndex = 9;
            this.checkReboot.Text = "Force &Reboot on Connect";
            this.checkReboot.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(542, 24);
            this.menuStrip1.TabIndex = 13;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.toolStripSeparator,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(143, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewSummaryMenuItem,
            this.viewHistogramAllocatedMenuItem,
            this.viewHistogramRelocatedMenuItem,
            this.viewHistogramFinalizerMenuItem,
            this.viewHistogramCriticalFinalizerMenuItem,
            this.viewObjectsByAddressMenuItem,
            this.viewHistogramByAgeMenuItem,
            this.viewAllocationGraphMenuItem,
            this.viewAssemblyGraphMenuItem,
            this.viewFunctionGraphMenuItem,
            this.viewModuleGraphMenuItem,
            this.viewClassGraphMenuItem,
            this.viewHeapGraphMenuItem,
            this.viewCallGraphMenuItem,
            this.viewTimeLineMenuItem,
            this.viewCommentsMenuItem,
            this.viewCallTreeMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // viewSummaryMenuItem
            // 
            this.viewSummaryMenuItem.Name = "viewSummaryMenuItem";
            this.viewSummaryMenuItem.Size = new System.Drawing.Size(253, 22);
            this.viewSummaryMenuItem.Text = "Summary";
            this.viewSummaryMenuItem.Click += new System.EventHandler(this.viewSummaryMenuItem_Click);
            // 
            // viewHistogramAllocatedMenuItem
            // 
            this.viewHistogramAllocatedMenuItem.Name = "viewHistogramAllocatedMenuItem";
            this.viewHistogramAllocatedMenuItem.Size = new System.Drawing.Size(253, 22);
            this.viewHistogramAllocatedMenuItem.Text = "Histogram Allocated Types";
            this.viewHistogramAllocatedMenuItem.Click += new System.EventHandler(this.viewHistogram_Click);
            // 
            // viewHistogramRelocatedMenuItem
            // 
            this.viewHistogramRelocatedMenuItem.Name = "viewHistogramRelocatedMenuItem";
            this.viewHistogramRelocatedMenuItem.Size = new System.Drawing.Size(253, 22);
            this.viewHistogramRelocatedMenuItem.Text = "Histogram Relocated Types";
            this.viewHistogramRelocatedMenuItem.Click += new System.EventHandler(this.viewHistogramRelocatedMenuItem_Click);
            // 
            // viewHistogramFinalizerMenuItem
            // 
            this.viewHistogramFinalizerMenuItem.Name = "viewHistogramFinalizerMenuItem";
            this.viewHistogramFinalizerMenuItem.Size = new System.Drawing.Size(253, 22);
            this.viewHistogramFinalizerMenuItem.Text = "Histogram Finalized Types";
            this.viewHistogramFinalizerMenuItem.Click += new System.EventHandler(this.viewHistogramFinalizerMenuItem_Click);
            // 
            // viewHistogramCriticalFinalizerMenuItem
            // 
            this.viewHistogramCriticalFinalizerMenuItem.Name = "viewHistogramCriticalFinalizerMenuItem";
            this.viewHistogramCriticalFinalizerMenuItem.Size = new System.Drawing.Size(253, 22);
            this.viewHistogramCriticalFinalizerMenuItem.Text = "Histogram Critical Finalized Types";
            this.viewHistogramCriticalFinalizerMenuItem.Click += new System.EventHandler(this.viewHistogramCriticalFinalizerMenuItem_Click);
            // 
            // viewObjectsByAddressMenuItem
            // 
            this.viewObjectsByAddressMenuItem.Name = "viewObjectsByAddressMenuItem";
            this.viewObjectsByAddressMenuItem.Size = new System.Drawing.Size(253, 22);
            this.viewObjectsByAddressMenuItem.Text = "Objects by Address";
            this.viewObjectsByAddressMenuItem.Click += new System.EventHandler(this.viewByAddressMenuItem_Click);
            // 
            // viewHistogramByAgeMenuItem
            // 
            this.viewHistogramByAgeMenuItem.Name = "viewHistogramByAgeMenuItem";
            this.viewHistogramByAgeMenuItem.Size = new System.Drawing.Size(253, 22);
            this.viewHistogramByAgeMenuItem.Text = "Histogram by Age";
            this.viewHistogramByAgeMenuItem.Click += new System.EventHandler(this.viewAgeHistogram_Click);
            // 
            // viewAllocationGraphMenuItem
            // 
            this.viewAllocationGraphMenuItem.Name = "viewAllocationGraphMenuItem";
            this.viewAllocationGraphMenuItem.Size = new System.Drawing.Size(253, 22);
            this.viewAllocationGraphMenuItem.Text = "Allocation Graph";
            this.viewAllocationGraphMenuItem.Click += new System.EventHandler(this.viewAllocationGraphmenuItem_Click);
            // 
            // viewAssemblyGraphMenuItem
            // 
            this.viewAssemblyGraphMenuItem.Name = "viewAssemblyGraphMenuItem";
            this.viewAssemblyGraphMenuItem.Size = new System.Drawing.Size(253, 22);
            this.viewAssemblyGraphMenuItem.Text = "Assembly Graph";
            this.viewAssemblyGraphMenuItem.Click += new System.EventHandler(this.viewAssemblyGraphmenuItem_Click);
            // 
            // viewFunctionGraphMenuItem
            // 
            this.viewFunctionGraphMenuItem.Name = "viewFunctionGraphMenuItem";
            this.viewFunctionGraphMenuItem.Size = new System.Drawing.Size(253, 22);
            this.viewFunctionGraphMenuItem.Text = "Function Graph";
            this.viewFunctionGraphMenuItem.Click += new System.EventHandler(this.viewFunctionGraphMenuItem_Click);
            // 
            // viewModuleGraphMenuItem
            // 
            this.viewModuleGraphMenuItem.Name = "viewModuleGraphMenuItem";
            this.viewModuleGraphMenuItem.Size = new System.Drawing.Size(253, 22);
            this.viewModuleGraphMenuItem.Text = "Module Graph";
            this.viewModuleGraphMenuItem.Click += new System.EventHandler(this.viewModuleGraphMenuItem_Click);
            // 
            // viewClassGraphMenuItem
            // 
            this.viewClassGraphMenuItem.Name = "viewClassGraphMenuItem";
            this.viewClassGraphMenuItem.Size = new System.Drawing.Size(253, 22);
            this.viewClassGraphMenuItem.Text = "Class Graph";
            this.viewClassGraphMenuItem.Click += new System.EventHandler(this.viewClassGraphMenuItem_Click);
            // 
            // viewHeapGraphMenuItem
            // 
            this.viewHeapGraphMenuItem.Name = "viewHeapGraphMenuItem";
            this.viewHeapGraphMenuItem.Size = new System.Drawing.Size(253, 22);
            this.viewHeapGraphMenuItem.Text = "Heap Graph";
            this.viewHeapGraphMenuItem.Click += new System.EventHandler(this.viewHeapGraphMenuItem_Click);
            // 
            // viewCallGraphMenuItem
            // 
            this.viewCallGraphMenuItem.Name = "viewCallGraphMenuItem";
            this.viewCallGraphMenuItem.Size = new System.Drawing.Size(253, 22);
            this.viewCallGraphMenuItem.Text = "Call Graph";
            this.viewCallGraphMenuItem.Click += new System.EventHandler(this.viewCallGraphMenuItem_Click);
            // 
            // viewTimeLineMenuItem
            // 
            this.viewTimeLineMenuItem.Name = "viewTimeLineMenuItem";
            this.viewTimeLineMenuItem.Size = new System.Drawing.Size(253, 22);
            this.viewTimeLineMenuItem.Text = "Time Line";
            this.viewTimeLineMenuItem.Click += new System.EventHandler(this.viewTimeLineMenuItem_Click);
            // 
            // viewCommentsMenuItem
            // 
            this.viewCommentsMenuItem.Name = "viewCommentsMenuItem";
            this.viewCommentsMenuItem.Size = new System.Drawing.Size(253, 22);
            this.viewCommentsMenuItem.Text = "Comments";
            this.viewCommentsMenuItem.Click += new System.EventHandler(this.viewCommentsMenuItem_Click);
            // 
            // viewCallTreeMenuItem
            // 
            this.viewCallTreeMenuItem.Name = "viewCallTreeMenuItem";
            this.viewCallTreeMenuItem.Size = new System.Drawing.Size(253, 22);
            this.viewCallTreeMenuItem.Text = "Call Tree";
            this.viewCallTreeMenuItem.Click += new System.EventHandler(this.viewCallTreeMenuItem_Click);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClear.Location = new System.Drawing.Point(12, 352);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(91, 22);
            this.btnClear.TabIndex = 14;
            this.btnClear.Text = "&Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // ProfilerLauncherForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(542, 386);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.groupBoxOptions);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.groupBoxOutput);
            this.Controls.Add(this.groupBoxDevice);
            this.Controls.Add(this.textLog);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(300, 250);
            this.Name = "ProfilerLauncherForm";
            this.Text = ".NET Micro Framework Profiling Tool";
            this.Load += new System.EventHandler(this.ProfilerLauncherForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProfilerLauncherForm_FormClosing);
            this.groupBoxDevice.ResumeLayout(false);
            this.groupBoxOutput.ResumeLayout(false);
            this.groupBoxOutput.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.groupBoxOptions.ResumeLayout(false);
            this.groupBoxOptions.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxDevice;
        private MFPortFilterComboBox comboBoxTransport;
        private MFDeviceSelectorComboBox comboBoxDevice;
        private System.Windows.Forms.GroupBox groupBoxOutput;
        private System.Windows.Forms.Label labelFile;
        private System.Windows.Forms.TextBox textLogFile;
        private System.Windows.Forms.Button buttonLogFileBrowse;
        private System.Windows.Forms.RadioButton radioCLRProfiler;
        private System.Windows.Forms.RadioButton radioOffProf;
        private System.Windows.Forms.Button buttonConnect;
        private System.ComponentModel.BackgroundWorker bwConnecter;
        private System.Windows.Forms.GroupBox groupBoxOptions;
        private System.Windows.Forms.CheckBox checkReboot;
        private System.Windows.Forms.CheckBox checkAllocations;
        private System.Windows.Forms.CheckBox checkCalls;
        private System.Windows.Forms.TextBox textLog;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewSummaryMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewTimeLineMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewHistogramAllocatedMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewHistogramRelocatedMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewHistogramFinalizerMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewHistogramCriticalFinalizerMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewHeapGraphMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewCallGraphMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewAllocationGraphMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewObjectsByAddressMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewHistogramByAgeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewFunctionGraphMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewModuleGraphMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewClassGraphMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewCommentsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewCallTreeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewAssemblyGraphMenuItem;
        private System.Windows.Forms.Button btnClear;
    }
}

