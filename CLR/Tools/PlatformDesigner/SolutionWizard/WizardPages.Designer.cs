namespace SolutionWizard
{
    partial class SolutionWizardForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SolutionWizardForm));
            this.fbd_PlatformFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.sfd_NewPlatform = new System.Windows.Forms.SaveFileDialog();
            this.Wiz = new Gui.Wizard.Wizard();
            this.wpWelcome = new Gui.Wizard.WizardPage();
            this.label1 = new System.Windows.Forms.Label();
            this.pidLabel = new System.Windows.Forms.Label();
            this.buttonSpoClientBrowse = new System.Windows.Forms.Button();
            this.textBoxSpoClientPath = new System.Windows.Forms.TextBox();
            this.ipWelcome = new Gui.Wizard.InfoPage();
            this.wpChooseTask = new Gui.Wizard.WizardPage();
            this.EditDesc = new System.Windows.Forms.Label();
            this.CloneDesc = new System.Windows.Forms.Label();
            this.CreateDesc = new System.Windows.Forms.Label();
            this.rbCloneSolution = new System.Windows.Forms.RadioButton();
            this.rbEditSolution = new System.Windows.Forms.RadioButton();
            this.rbCreatePlatform = new System.Windows.Forms.RadioButton();
            this.hChooseTask = new Gui.Wizard.Header();
            this.wpSelectExisting = new Gui.Wizard.WizardPage();
            this.tb_SelectedPlatform = new System.Windows.Forms.TextBox();
            this.b_Browse = new System.Windows.Forms.Button();
            this.rb_BrowsePlatformFile = new System.Windows.Forms.RadioButton();
            this.rb_SelectFromList = new System.Windows.Forms.RadioButton();
            this.tv_PlatformList = new System.Windows.Forms.TreeView();
            this.rtb_PlatformFeatures = new System.Windows.Forms.RichTextBox();
            this.h_SelectExisting = new Gui.Wizard.Header();
            this.wpCreatePlatform = new Gui.Wizard.WizardPage();
            this.label15 = new System.Windows.Forms.Label();
            this.tb_SolutionAuthor = new System.Windows.Forms.TextBox();
            this.tb_PlatformDescription = new System.Windows.Forms.TextBox();
            this.l_PlatformDescription = new System.Windows.Forms.Label();
            this.l_PlatformName = new System.Windows.Forms.Label();
            this.tb_PlatformName = new System.Windows.Forms.TextBox();
            this.h_CreatePlatform = new Gui.Wizard.Header();
            this.wpSolutionCfg = new Gui.Wizard.WizardPage();
            this.header3 = new Gui.Wizard.Header();
            this.tbSlowClock = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.tbFlashSize = new System.Windows.Forms.TextBox();
            this.tbFlashBase = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.tbClockSpeed = new System.Windows.Forms.TextBox();
            this.tbRamSize = new System.Windows.Forms.TextBox();
            this.tbRamBase = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.cbDbgPort = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.cbRuntimeMemCfg = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cbProcessor = new System.Windows.Forms.ComboBox();
            this.wpChooseProjects = new Gui.Wizard.WizardPage();
            this.rtb_ProjectDescription = new System.Windows.Forms.RichTextBox();
            this.tv_SelectedProjects = new System.Windows.Forms.TreeView();
            this.header2 = new Gui.Wizard.Header();
            this.wpChooseFeatures = new Gui.Wizard.WizardPage();
            this.cbFeatureCond = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbProjectSelect_Feature = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.rtb_FeatureDescription = new System.Windows.Forms.RichTextBox();
            this.hChooseFeatures = new Gui.Wizard.Header();
            this.tv_FeatureView = new System.Windows.Forms.TreeView();
            this.wpChooseLibraries = new Gui.Wizard.WizardPage();
            this.cbLibraryCond = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cbShowAllLibCatChoices = new System.Windows.Forms.CheckBox();
            this.tv_LibraryView = new System.Windows.Forms.TreeView();
            this.cbProjectSelect_Library = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.rtb_LibraryDescription = new System.Windows.Forms.RichTextBox();
            this.header1 = new Gui.Wizard.Header();
            this.wpFinish = new Gui.Wizard.WizardPage();
            this.header4 = new Gui.Wizard.Header();
            this.rtb_Finish = new System.Windows.Forms.RichTextBox();
            this.Wiz.SuspendLayout();
            this.wpWelcome.SuspendLayout();
            this.wpChooseTask.SuspendLayout();
            this.wpSelectExisting.SuspendLayout();
            this.wpCreatePlatform.SuspendLayout();
            this.wpSolutionCfg.SuspendLayout();
            this.wpChooseProjects.SuspendLayout();
            this.wpChooseFeatures.SuspendLayout();
            this.wpChooseLibraries.SuspendLayout();
            this.wpFinish.SuspendLayout();
            this.SuspendLayout();
            // 
            // Wiz
            // 
            this.Wiz.Controls.Add(this.wpSolutionCfg);
            this.Wiz.Controls.Add(this.wpCreatePlatform);
            this.Wiz.Controls.Add(this.wpSelectExisting);
            this.Wiz.Controls.Add(this.wpChooseTask);
            this.Wiz.Controls.Add(this.wpWelcome);
            this.Wiz.Controls.Add(this.wpFinish);
            this.Wiz.Controls.Add(this.wpChooseLibraries);
            this.Wiz.Controls.Add(this.wpChooseFeatures);
            this.Wiz.Controls.Add(this.wpChooseProjects);
            this.Wiz.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Wiz.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Wiz.Location = new System.Drawing.Point(0, 0);
            this.Wiz.Name = "Wiz";
            this.Wiz.Pages.AddRange(new Gui.Wizard.WizardPage[] {
            this.wpWelcome,
            this.wpChooseTask,
            this.wpSelectExisting,
            this.wpCreatePlatform,
            this.wpSolutionCfg,
            this.wpChooseProjects,
            this.wpChooseFeatures,
            this.wpChooseLibraries,
            this.wpFinish});
            this.Wiz.Size = new System.Drawing.Size(678, 435);
            this.Wiz.TabIndex = 0;
            this.Wiz.Load += new System.EventHandler(this.Wiz_Load);
            // 
            // wpWelcome
            // 
            this.wpWelcome.Controls.Add(this.label1);
            this.wpWelcome.Controls.Add(this.pidLabel);
            this.wpWelcome.Controls.Add(this.buttonSpoClientBrowse);
            this.wpWelcome.Controls.Add(this.textBoxSpoClientPath);
            this.wpWelcome.Controls.Add(this.ipWelcome);
            this.wpWelcome.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wpWelcome.IsFinishPage = false;
            this.wpWelcome.Location = new System.Drawing.Point(0, 0);
            this.wpWelcome.Name = "wpWelcome";
            this.wpWelcome.Size = new System.Drawing.Size(678, 387);
            this.wpWelcome.TabIndex = 1;
            this.wpWelcome.CloseFromNext += new Gui.Wizard.PageEventHandler(this.wpWelcome_CloseFromNext);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(176, 327);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Porting Kit Directory";
            // 
            // pidLabel
            // 
            this.pidLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pidLabel.AutoSize = true;
            this.pidLabel.BackColor = System.Drawing.Color.White;
            this.pidLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pidLabel.Location = new System.Drawing.Point(176, 267);
            this.pidLabel.Name = "pidLabel";
            this.pidLabel.Size = new System.Drawing.Size(70, 13);
            this.pidLabel.TabIndex = 5;
            this.pidLabel.Text = "Product ID:";
            // 
            // buttonSpoClientBrowse
            // 
            this.buttonSpoClientBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSpoClientBrowse.Location = new System.Drawing.Point(590, 346);
            this.buttonSpoClientBrowse.Name = "buttonSpoClientBrowse";
            this.buttonSpoClientBrowse.Size = new System.Drawing.Size(76, 21);
            this.buttonSpoClientBrowse.TabIndex = 1;
            this.buttonSpoClientBrowse.Text = "&Browse";
            this.buttonSpoClientBrowse.UseVisualStyleBackColor = true;
            this.buttonSpoClientBrowse.Click += new System.EventHandler(this.buttonSpoClientBrowse_Click);
            // 
            // textBoxSpoClientPath
            // 
            this.textBoxSpoClientPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSpoClientPath.Location = new System.Drawing.Point(176, 346);
            this.textBoxSpoClientPath.Name = "textBoxSpoClientPath";
            this.textBoxSpoClientPath.Size = new System.Drawing.Size(408, 21);
            this.textBoxSpoClientPath.TabIndex = 0;
            // 
            // ipWelcome
            // 
            this.ipWelcome.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ipWelcome.BackColor = System.Drawing.Color.White;
            this.ipWelcome.Image = ((System.Drawing.Image)(resources.GetObject("ipWelcome.Image")));
            this.ipWelcome.Location = new System.Drawing.Point(0, 0);
            this.ipWelcome.Name = "ipWelcome";
            this.ipWelcome.PageText = resources.GetString("ipWelcome.PageText");
            this.ipWelcome.PageTitle = "Welcome to the .NET Micro Framework Solution Wizard";
            this.ipWelcome.Size = new System.Drawing.Size(678, 387);
            this.ipWelcome.TabIndex = 2;
            this.ipWelcome.Load += new System.EventHandler(this.ipWelcome_Load);
            // 
            // wpChooseTask
            // 
            this.wpChooseTask.Controls.Add(this.EditDesc);
            this.wpChooseTask.Controls.Add(this.CloneDesc);
            this.wpChooseTask.Controls.Add(this.CreateDesc);
            this.wpChooseTask.Controls.Add(this.rbCloneSolution);
            this.wpChooseTask.Controls.Add(this.rbEditSolution);
            this.wpChooseTask.Controls.Add(this.rbCreatePlatform);
            this.wpChooseTask.Controls.Add(this.hChooseTask);
            this.wpChooseTask.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wpChooseTask.IsFinishPage = false;
            this.wpChooseTask.Location = new System.Drawing.Point(0, 0);
            this.wpChooseTask.Name = "wpChooseTask";
            this.wpChooseTask.Size = new System.Drawing.Size(678, 387);
            this.wpChooseTask.TabIndex = 2;
            this.wpChooseTask.CloseFromNext += new Gui.Wizard.PageEventHandler(this.wpChooseTask_CloseFromNext);
            // 
            // EditDesc
            // 
            this.EditDesc.AutoSize = true;
            this.EditDesc.Location = new System.Drawing.Point(41, 203);
            this.EditDesc.Name = "EditDesc";
            this.EditDesc.Size = new System.Drawing.Size(492, 13);
            this.EditDesc.TabIndex = 6;
            this.EditDesc.Text = "Modify a solution without making a copy. Use the Clone option if you are modifyin" +
                "g a sample solution.";
            // 
            // CloneDesc
            // 
            this.CloneDesc.AutoSize = true;
            this.CloneDesc.Location = new System.Drawing.Point(41, 156);
            this.CloneDesc.Name = "CloneDesc";
            this.CloneDesc.Size = new System.Drawing.Size(609, 13);
            this.CloneDesc.TabIndex = 5;
            this.CloneDesc.Text = "Create a solution in a new directory, based on an existing project. This lets you" +
                " to customize this solution before it is created.";
            // 
            // CreateDesc
            // 
            this.CreateDesc.AutoSize = true;
            this.CreateDesc.Location = new System.Drawing.Point(41, 109);
            this.CreateDesc.Name = "CreateDesc";
            this.CreateDesc.Size = new System.Drawing.Size(280, 13);
            this.CreateDesc.TabIndex = 4;
            this.CreateDesc.Text = "Generate a new solution from scratch in a new directory.";
            // 
            // rbCloneSolution
            // 
            this.rbCloneSolution.AutoSize = true;
            this.rbCloneSolution.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbCloneSolution.Location = new System.Drawing.Point(24, 136);
            this.rbCloneSolution.Name = "rbCloneSolution";
            this.rbCloneSolution.Size = new System.Drawing.Size(169, 17);
            this.rbCloneSolution.TabIndex = 2;
            this.rbCloneSolution.Text = "Clone an Existing Solution";
            this.rbCloneSolution.UseVisualStyleBackColor = true;
            // 
            // rbEditSolution
            // 
            this.rbEditSolution.AutoSize = true;
            this.rbEditSolution.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbEditSolution.Location = new System.Drawing.Point(24, 183);
            this.rbEditSolution.Name = "rbEditSolution";
            this.rbEditSolution.Size = new System.Drawing.Size(159, 17);
            this.rbEditSolution.TabIndex = 3;
            this.rbEditSolution.TabStop = true;
            this.rbEditSolution.Text = "Edit an Existing Solution";
            this.rbEditSolution.UseVisualStyleBackColor = true;
            // 
            // rbCreatePlatform
            // 
            this.rbCreatePlatform.AutoSize = true;
            this.rbCreatePlatform.Checked = true;
            this.rbCreatePlatform.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbCreatePlatform.Location = new System.Drawing.Point(24, 89);
            this.rbCreatePlatform.Name = "rbCreatePlatform";
            this.rbCreatePlatform.Size = new System.Drawing.Size(148, 17);
            this.rbCreatePlatform.TabIndex = 1;
            this.rbCreatePlatform.TabStop = true;
            this.rbCreatePlatform.Text = "Create a New Solution";
            this.rbCreatePlatform.UseVisualStyleBackColor = true;
            // 
            // hChooseTask
            // 
            this.hChooseTask.BackColor = System.Drawing.SystemColors.Control;
            this.hChooseTask.CausesValidation = false;
            this.hChooseTask.Description = "Select how to start configuring your firmware image.";
            this.hChooseTask.Dock = System.Windows.Forms.DockStyle.Top;
            this.hChooseTask.Image = ((System.Drawing.Image)(resources.GetObject("hChooseTask.Image")));
            this.hChooseTask.Location = new System.Drawing.Point(0, 0);
            this.hChooseTask.Name = "hChooseTask";
            this.hChooseTask.Size = new System.Drawing.Size(678, 64);
            this.hChooseTask.TabIndex = 0;
            this.hChooseTask.Title = "Choose a Task";
            // 
            // wpSelectExisting
            // 
            this.wpSelectExisting.Controls.Add(this.tb_SelectedPlatform);
            this.wpSelectExisting.Controls.Add(this.b_Browse);
            this.wpSelectExisting.Controls.Add(this.rb_BrowsePlatformFile);
            this.wpSelectExisting.Controls.Add(this.rb_SelectFromList);
            this.wpSelectExisting.Controls.Add(this.tv_PlatformList);
            this.wpSelectExisting.Controls.Add(this.rtb_PlatformFeatures);
            this.wpSelectExisting.Controls.Add(this.h_SelectExisting);
            this.wpSelectExisting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wpSelectExisting.IsFinishPage = false;
            this.wpSelectExisting.Location = new System.Drawing.Point(0, 0);
            this.wpSelectExisting.Name = "wpSelectExisting";
            this.wpSelectExisting.Size = new System.Drawing.Size(678, 387);
            this.wpSelectExisting.TabIndex = 4;
            this.wpSelectExisting.CloseFromNext += new Gui.Wizard.PageEventHandler(this.wpSelectExisting_CloseFromNext);
            this.wpSelectExisting.CloseFromBack += new Gui.Wizard.PageEventHandler(this.wpSelectExisting_CloseFromBack);
            this.wpSelectExisting.ShowFromNext += new System.EventHandler(this.wpSelectExisting_ShowFromNext);
            // 
            // tb_SelectedPlatform
            // 
            this.tb_SelectedPlatform.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tb_SelectedPlatform.Location = new System.Drawing.Point(13, 349);
            this.tb_SelectedPlatform.Name = "tb_SelectedPlatform";
            this.tb_SelectedPlatform.Size = new System.Drawing.Size(361, 21);
            this.tb_SelectedPlatform.TabIndex = 4;
            this.tb_SelectedPlatform.Text = "Find a Solution Document...";
            // 
            // b_Browse
            // 
            this.b_Browse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.b_Browse.Location = new System.Drawing.Point(271, 320);
            this.b_Browse.Name = "b_Browse";
            this.b_Browse.Size = new System.Drawing.Size(103, 23);
            this.b_Browse.TabIndex = 3;
            this.b_Browse.Text = "Browse...";
            this.b_Browse.UseVisualStyleBackColor = true;
            this.b_Browse.Click += new System.EventHandler(this.b_Browse_Click);
            // 
            // rb_BrowsePlatformFile
            // 
            this.rb_BrowsePlatformFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rb_BrowsePlatformFile.AutoSize = true;
            this.rb_BrowsePlatformFile.Location = new System.Drawing.Point(12, 323);
            this.rb_BrowsePlatformFile.Name = "rb_BrowsePlatformFile";
            this.rb_BrowsePlatformFile.Size = new System.Drawing.Size(92, 17);
            this.rb_BrowsePlatformFile.TabIndex = 2;
            this.rb_BrowsePlatformFile.TabStop = true;
            this.rb_BrowsePlatformFile.Text = "Open Solution";
            this.rb_BrowsePlatformFile.UseVisualStyleBackColor = true;
            this.rb_BrowsePlatformFile.CheckedChanged += new System.EventHandler(this.rb_BrowsePlatformFile_CheckedChanged);
            // 
            // rb_SelectFromList
            // 
            this.rb_SelectFromList.AutoSize = true;
            this.rb_SelectFromList.Location = new System.Drawing.Point(13, 71);
            this.rb_SelectFromList.Name = "rb_SelectFromList";
            this.rb_SelectFromList.Size = new System.Drawing.Size(104, 17);
            this.rb_SelectFromList.TabIndex = 0;
            this.rb_SelectFromList.TabStop = true;
            this.rb_SelectFromList.Text = "Select a Solution";
            this.rb_SelectFromList.UseVisualStyleBackColor = true;
            this.rb_SelectFromList.CheckedChanged += new System.EventHandler(this.rb_SelectFromList_CheckedChanged);
            // 
            // tv_PlatformList
            // 
            this.tv_PlatformList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tv_PlatformList.HideSelection = false;
            this.tv_PlatformList.Location = new System.Drawing.Point(13, 94);
            this.tv_PlatformList.Name = "tv_PlatformList";
            this.tv_PlatformList.Size = new System.Drawing.Size(361, 220);
            this.tv_PlatformList.TabIndex = 1;
            this.tv_PlatformList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tv_PlatformList_AfterSelect);
            // 
            // rtb_PlatformFeatures
            // 
            this.rtb_PlatformFeatures.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtb_PlatformFeatures.BackColor = System.Drawing.SystemColors.Info;
            this.rtb_PlatformFeatures.Location = new System.Drawing.Point(380, 94);
            this.rtb_PlatformFeatures.Name = "rtb_PlatformFeatures";
            this.rtb_PlatformFeatures.Size = new System.Drawing.Size(286, 276);
            this.rtb_PlatformFeatures.TabIndex = 5;
            this.rtb_PlatformFeatures.Text = "";
            // 
            // h_SelectExisting
            // 
            this.h_SelectExisting.BackColor = System.Drawing.SystemColors.Control;
            this.h_SelectExisting.CausesValidation = false;
            this.h_SelectExisting.Description = "Choose the solution you\'d like to base your new solution on. ";
            this.h_SelectExisting.Dock = System.Windows.Forms.DockStyle.Top;
            this.h_SelectExisting.Image = ((System.Drawing.Image)(resources.GetObject("h_SelectExisting.Image")));
            this.h_SelectExisting.Location = new System.Drawing.Point(0, 0);
            this.h_SelectExisting.Name = "h_SelectExisting";
            this.h_SelectExisting.Size = new System.Drawing.Size(678, 64);
            this.h_SelectExisting.TabIndex = 0;
            this.h_SelectExisting.Title = "Select an existing solution";
            // 
            // wpCreatePlatform
            // 
            this.wpCreatePlatform.Controls.Add(this.label15);
            this.wpCreatePlatform.Controls.Add(this.tb_SolutionAuthor);
            this.wpCreatePlatform.Controls.Add(this.tb_PlatformDescription);
            this.wpCreatePlatform.Controls.Add(this.l_PlatformDescription);
            this.wpCreatePlatform.Controls.Add(this.l_PlatformName);
            this.wpCreatePlatform.Controls.Add(this.tb_PlatformName);
            this.wpCreatePlatform.Controls.Add(this.h_CreatePlatform);
            this.wpCreatePlatform.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wpCreatePlatform.IsFinishPage = false;
            this.wpCreatePlatform.Location = new System.Drawing.Point(0, 0);
            this.wpCreatePlatform.Name = "wpCreatePlatform";
            this.wpCreatePlatform.Size = new System.Drawing.Size(678, 387);
            this.wpCreatePlatform.TabIndex = 5;
            this.wpCreatePlatform.CloseFromNext += new Gui.Wizard.PageEventHandler(this.wpCreatePlatform_CloseFromNext);
            this.wpCreatePlatform.CloseFromBack += new Gui.Wizard.PageEventHandler(this.wpCreatePlatform_CloseFromBack);
            this.wpCreatePlatform.ShowFromNext += new System.EventHandler(this.wp_CreatePlatform_ShowFromNext);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(3, 114);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(44, 13);
            this.label15.TabIndex = 6;
            this.label15.Text = "Author:";
            // 
            // tb_SolutionAuthor
            // 
            this.tb_SolutionAuthor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tb_SolutionAuthor.Location = new System.Drawing.Point(80, 111);
            this.tb_SolutionAuthor.Name = "tb_SolutionAuthor";
            this.tb_SolutionAuthor.Size = new System.Drawing.Size(585, 21);
            this.tb_SolutionAuthor.TabIndex = 1;
            // 
            // tb_PlatformDescription
            // 
            this.tb_PlatformDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tb_PlatformDescription.Location = new System.Drawing.Point(80, 138);
            this.tb_PlatformDescription.Multiline = true;
            this.tb_PlatformDescription.Name = "tb_PlatformDescription";
            this.tb_PlatformDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tb_PlatformDescription.Size = new System.Drawing.Size(585, 246);
            this.tb_PlatformDescription.TabIndex = 2;
            // 
            // l_PlatformDescription
            // 
            this.l_PlatformDescription.AutoSize = true;
            this.l_PlatformDescription.Location = new System.Drawing.Point(3, 141);
            this.l_PlatformDescription.Name = "l_PlatformDescription";
            this.l_PlatformDescription.Size = new System.Drawing.Size(64, 13);
            this.l_PlatformDescription.TabIndex = 4;
            this.l_PlatformDescription.Text = "Description:";
            // 
            // l_PlatformName
            // 
            this.l_PlatformName.AutoSize = true;
            this.l_PlatformName.Location = new System.Drawing.Point(3, 87);
            this.l_PlatformName.Name = "l_PlatformName";
            this.l_PlatformName.Size = new System.Drawing.Size(38, 13);
            this.l_PlatformName.TabIndex = 3;
            this.l_PlatformName.Text = "Name:";
            // 
            // tb_PlatformName
            // 
            this.tb_PlatformName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tb_PlatformName.Location = new System.Drawing.Point(80, 84);
            this.tb_PlatformName.Name = "tb_PlatformName";
            this.tb_PlatformName.Size = new System.Drawing.Size(585, 21);
            this.tb_PlatformName.TabIndex = 0;
            this.tb_PlatformName.TextChanged += new System.EventHandler(this.tb_PlatformName_TextChanged);
            // 
            // h_CreatePlatform
            // 
            this.h_CreatePlatform.BackColor = System.Drawing.SystemColors.Control;
            this.h_CreatePlatform.CausesValidation = false;
            this.h_CreatePlatform.Description = "Choose a name and description for the solution";
            this.h_CreatePlatform.Dock = System.Windows.Forms.DockStyle.Top;
            this.h_CreatePlatform.Image = ((System.Drawing.Image)(resources.GetObject("h_CreatePlatform.Image")));
            this.h_CreatePlatform.Location = new System.Drawing.Point(0, 0);
            this.h_CreatePlatform.Name = "h_CreatePlatform";
            this.h_CreatePlatform.Size = new System.Drawing.Size(678, 64);
            this.h_CreatePlatform.TabIndex = 5;
            this.h_CreatePlatform.Title = "Solution Properties";
            // 
            // wpSolutionCfg
            // 
            this.wpSolutionCfg.Controls.Add(this.header3);
            this.wpSolutionCfg.Controls.Add(this.tbSlowClock);
            this.wpSolutionCfg.Controls.Add(this.label14);
            this.wpSolutionCfg.Controls.Add(this.tbFlashSize);
            this.wpSolutionCfg.Controls.Add(this.tbFlashBase);
            this.wpSolutionCfg.Controls.Add(this.label12);
            this.wpSolutionCfg.Controls.Add(this.label13);
            this.wpSolutionCfg.Controls.Add(this.tbClockSpeed);
            this.wpSolutionCfg.Controls.Add(this.tbRamSize);
            this.wpSolutionCfg.Controls.Add(this.tbRamBase);
            this.wpSolutionCfg.Controls.Add(this.label11);
            this.wpSolutionCfg.Controls.Add(this.label10);
            this.wpSolutionCfg.Controls.Add(this.label9);
            this.wpSolutionCfg.Controls.Add(this.cbDbgPort);
            this.wpSolutionCfg.Controls.Add(this.label8);
            this.wpSolutionCfg.Controls.Add(this.cbRuntimeMemCfg);
            this.wpSolutionCfg.Controls.Add(this.label7);
            this.wpSolutionCfg.Controls.Add(this.label6);
            this.wpSolutionCfg.Controls.Add(this.cbProcessor);
            this.wpSolutionCfg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wpSolutionCfg.IsFinishPage = false;
            this.wpSolutionCfg.Location = new System.Drawing.Point(0, 0);
            this.wpSolutionCfg.Name = "wpSolutionCfg";
            this.wpSolutionCfg.Size = new System.Drawing.Size(678, 387);
            this.wpSolutionCfg.TabIndex = 0;
            this.wpSolutionCfg.CloseFromNext += new Gui.Wizard.PageEventHandler(this.wpSolutionCfg_CloseFromNext);
            this.wpSolutionCfg.CloseFromBack += new Gui.Wizard.PageEventHandler(this.wpSolutionCfg_CloseFromBack);
            this.wpSolutionCfg.ShowFromNext += new System.EventHandler(this.wpSolutionCfg_ShowFromNext);
            // 
            // header3
            // 
            this.header3.BackColor = System.Drawing.SystemColors.Control;
            this.header3.CausesValidation = false;
            this.header3.Description = "Choose the processor and settings for the solution";
            this.header3.Dock = System.Windows.Forms.DockStyle.Top;
            this.header3.Image = ((System.Drawing.Image)(resources.GetObject("header3.Image")));
            this.header3.Location = new System.Drawing.Point(0, 0);
            this.header3.Name = "header3";
            this.header3.Size = new System.Drawing.Size(678, 64);
            this.header3.TabIndex = 0;
            this.header3.Title = "Processor Properties";
            // 
            // tbSlowClock
            // 
            this.tbSlowClock.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSlowClock.Location = new System.Drawing.Point(213, 329);
            this.tbSlowClock.Name = "tbSlowClock";
            this.tbSlowClock.Size = new System.Drawing.Size(349, 21);
            this.tbSlowClock.TabIndex = 18;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(92, 332);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(84, 13);
            this.label14.TabIndex = 17;
            this.label14.Text = "Slow Clock (Hz):";
            // 
            // tbFlashSize
            // 
            this.tbFlashSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFlashSize.Location = new System.Drawing.Point(213, 275);
            this.tbFlashSize.Name = "tbFlashSize";
            this.tbFlashSize.Size = new System.Drawing.Size(349, 21);
            this.tbFlashSize.TabIndex = 14;
            // 
            // tbFlashBase
            // 
            this.tbFlashBase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFlashBase.Location = new System.Drawing.Point(213, 248);
            this.tbFlashBase.Name = "tbFlashBase";
            this.tbFlashBase.Size = new System.Drawing.Size(349, 21);
            this.tbFlashBase.TabIndex = 12;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(92, 278);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(99, 13);
            this.label12.TabIndex = 13;
            this.label12.Text = "Flash Memory Size:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(92, 251);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(103, 13);
            this.label13.TabIndex = 11;
            this.label13.Text = "Flash Memory Base:";
            // 
            // tbClockSpeed
            // 
            this.tbClockSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbClockSpeed.Location = new System.Drawing.Point(213, 302);
            this.tbClockSpeed.Name = "tbClockSpeed";
            this.tbClockSpeed.Size = new System.Drawing.Size(349, 21);
            this.tbClockSpeed.TabIndex = 16;
            // 
            // tbRamSize
            // 
            this.tbRamSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbRamSize.Location = new System.Drawing.Point(213, 221);
            this.tbRamSize.Name = "tbRamSize";
            this.tbRamSize.Size = new System.Drawing.Size(349, 21);
            this.tbRamSize.TabIndex = 10;
            // 
            // tbRamBase
            // 
            this.tbRamBase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbRamBase.Location = new System.Drawing.Point(213, 194);
            this.tbRamBase.Name = "tbRamBase";
            this.tbRamBase.Size = new System.Drawing.Size(349, 21);
            this.tbRamBase.TabIndex = 8;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(92, 305);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(97, 13);
            this.label11.TabIndex = 15;
            this.label11.Text = "System Clock (Hz):";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(92, 224);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(96, 13);
            this.label10.TabIndex = 9;
            this.label10.Text = "RAM Memory Size:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(92, 197);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(100, 13);
            this.label9.TabIndex = 7;
            this.label9.Text = "RAM Memory Base:";
            // 
            // cbDbgPort
            // 
            this.cbDbgPort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbDbgPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDbgPort.FormattingEnabled = true;
            this.cbDbgPort.Location = new System.Drawing.Point(213, 140);
            this.cbDbgPort.Name = "cbDbgPort";
            this.cbDbgPort.Size = new System.Drawing.Size(349, 21);
            this.cbDbgPort.TabIndex = 4;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(92, 143);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(81, 13);
            this.label8.TabIndex = 3;
            this.label8.Text = "Debugger Port:";
            // 
            // cbRuntimeMemCfg
            // 
            this.cbRuntimeMemCfg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbRuntimeMemCfg.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbRuntimeMemCfg.FormattingEnabled = true;
            this.cbRuntimeMemCfg.Items.AddRange(new object[] {
            "Minimal",
            "Small",
            "Medium",
            "Large"});
            this.cbRuntimeMemCfg.Location = new System.Drawing.Point(213, 167);
            this.cbRuntimeMemCfg.Name = "cbRuntimeMemCfg";
            this.cbRuntimeMemCfg.Size = new System.Drawing.Size(349, 21);
            this.cbRuntimeMemCfg.TabIndex = 6;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(92, 170);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(82, 13);
            this.label7.TabIndex = 5;
            this.label7.Text = "Memory Profile:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(92, 113);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(76, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Processor/OS:";
            // 
            // cbProcessor
            // 
            this.cbProcessor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbProcessor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbProcessor.FormattingEnabled = true;
            this.cbProcessor.Location = new System.Drawing.Point(213, 113);
            this.cbProcessor.Name = "cbProcessor";
            this.cbProcessor.Size = new System.Drawing.Size(349, 21);
            this.cbProcessor.TabIndex = 2;
            this.cbProcessor.SelectedIndexChanged += new System.EventHandler(this.cbProcessor_SelectedIndexChanged);
            // 
            // wpChooseProjects
            // 
            this.wpChooseProjects.Controls.Add(this.rtb_ProjectDescription);
            this.wpChooseProjects.Controls.Add(this.tv_SelectedProjects);
            this.wpChooseProjects.Controls.Add(this.header2);
            this.wpChooseProjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wpChooseProjects.IsFinishPage = false;
            this.wpChooseProjects.Location = new System.Drawing.Point(0, 0);
            this.wpChooseProjects.Name = "wpChooseProjects";
            this.wpChooseProjects.Size = new System.Drawing.Size(678, 387);
            this.wpChooseProjects.TabIndex = 8;
            this.wpChooseProjects.CloseFromNext += new Gui.Wizard.PageEventHandler(this.wpChooseProjects_CloseFromNext);
            this.wpChooseProjects.CloseFromBack += new Gui.Wizard.PageEventHandler(this.wpChooseProjects_CloseFromBack);
            this.wpChooseProjects.ShowFromNext += new System.EventHandler(this.wpChooseProjects_ShowFromNext);
            // 
            // rtb_ProjectDescription
            // 
            this.rtb_ProjectDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtb_ProjectDescription.BackColor = System.Drawing.SystemColors.Info;
            this.rtb_ProjectDescription.Location = new System.Drawing.Point(379, 70);
            this.rtb_ProjectDescription.Name = "rtb_ProjectDescription";
            this.rtb_ProjectDescription.Size = new System.Drawing.Size(287, 314);
            this.rtb_ProjectDescription.TabIndex = 1;
            this.rtb_ProjectDescription.Text = "";
            // 
            // tv_SelectedProjects
            // 
            this.tv_SelectedProjects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tv_SelectedProjects.CheckBoxes = true;
            this.tv_SelectedProjects.HideSelection = false;
            this.tv_SelectedProjects.Location = new System.Drawing.Point(12, 70);
            this.tv_SelectedProjects.Name = "tv_SelectedProjects";
            this.tv_SelectedProjects.Size = new System.Drawing.Size(361, 314);
            this.tv_SelectedProjects.TabIndex = 0;
            this.tv_SelectedProjects.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tv_SelectedProjects_AfterCheck);
            this.tv_SelectedProjects.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tv_SelectedProjects_AfterSelect);
            this.tv_SelectedProjects.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.tv_SelectedProjects_BeforeCheck);
            // 
            // header2
            // 
            this.header2.BackColor = System.Drawing.SystemColors.Control;
            this.header2.CausesValidation = false;
            this.header2.Description = "Choose the projects to include in this solution";
            this.header2.Dock = System.Windows.Forms.DockStyle.Top;
            this.header2.Image = ((System.Drawing.Image)(resources.GetObject("header2.Image")));
            this.header2.Location = new System.Drawing.Point(0, 0);
            this.header2.Name = "header2";
            this.header2.Size = new System.Drawing.Size(678, 64);
            this.header2.TabIndex = 3;
            this.header2.Title = "Project Selection";
            // 
            // wpChooseFeatures
            // 
            this.wpChooseFeatures.Controls.Add(this.cbFeatureCond);
            this.wpChooseFeatures.Controls.Add(this.label4);
            this.wpChooseFeatures.Controls.Add(this.cbProjectSelect_Feature);
            this.wpChooseFeatures.Controls.Add(this.label2);
            this.wpChooseFeatures.Controls.Add(this.rtb_FeatureDescription);
            this.wpChooseFeatures.Controls.Add(this.hChooseFeatures);
            this.wpChooseFeatures.Controls.Add(this.tv_FeatureView);
            this.wpChooseFeatures.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wpChooseFeatures.IsFinishPage = false;
            this.wpChooseFeatures.Location = new System.Drawing.Point(0, 0);
            this.wpChooseFeatures.Name = "wpChooseFeatures";
            this.wpChooseFeatures.Size = new System.Drawing.Size(678, 387);
            this.wpChooseFeatures.TabIndex = 3;
            this.wpChooseFeatures.CloseFromNext += new Gui.Wizard.PageEventHandler(this.wpChooseFeatures_CloseFromNext);
            this.wpChooseFeatures.CloseFromBack += new Gui.Wizard.PageEventHandler(this.wpChooseFeatures_CloseFromBack);
            this.wpChooseFeatures.ShowFromNext += new System.EventHandler(this.wpChooseFeatures_ShowFromNext);
            // 
            // cbFeatureCond
            // 
            this.cbFeatureCond.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbFeatureCond.FormattingEnabled = true;
            this.cbFeatureCond.Items.AddRange(new object[] {
            "",
            "Debug",
            "Release",
            "RTM",
            "NotRTM",
            "BootLoaderOnly",
            "NotBootLoader"});
            this.cbFeatureCond.Location = new System.Drawing.Point(69, 362);
            this.cbFeatureCond.Name = "cbFeatureCond";
            this.cbFeatureCond.Size = new System.Drawing.Size(304, 21);
            this.cbFeatureCond.TabIndex = 1;
            this.cbFeatureCond.Visible = false;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 366);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Condition:";
            this.label4.Visible = false;
            // 
            // cbProjectSelect_Feature
            // 
            this.cbProjectSelect_Feature.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbProjectSelect_Feature.DropDownWidth = 260;
            this.cbProjectSelect_Feature.FormattingEnabled = true;
            this.cbProjectSelect_Feature.Location = new System.Drawing.Point(66, 70);
            this.cbProjectSelect_Feature.Name = "cbProjectSelect_Feature";
            this.cbProjectSelect_Feature.Size = new System.Drawing.Size(307, 21);
            this.cbProjectSelect_Feature.TabIndex = 0;
            this.cbProjectSelect_Feature.SelectedIndexChanged += new System.EventHandler(this.cbProjectSelect_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Project: ";
            // 
            // rtb_FeatureDescription
            // 
            this.rtb_FeatureDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtb_FeatureDescription.BackColor = System.Drawing.SystemColors.Info;
            this.rtb_FeatureDescription.Location = new System.Drawing.Point(379, 98);
            this.rtb_FeatureDescription.Name = "rtb_FeatureDescription";
            this.rtb_FeatureDescription.Size = new System.Drawing.Size(287, 289);
            this.rtb_FeatureDescription.TabIndex = 2;
            this.rtb_FeatureDescription.Text = "";
            this.rtb_FeatureDescription.VisibleChanged += new System.EventHandler(this.rtb_FeatureDescription_VisibleChanged);
            // 
            // hChooseFeatures
            // 
            this.hChooseFeatures.BackColor = System.Drawing.SystemColors.Control;
            this.hChooseFeatures.CausesValidation = false;
            this.hChooseFeatures.Description = "Choose the features for the project";
            this.hChooseFeatures.Dock = System.Windows.Forms.DockStyle.Top;
            this.hChooseFeatures.Image = ((System.Drawing.Image)(resources.GetObject("hChooseFeatures.Image")));
            this.hChooseFeatures.Location = new System.Drawing.Point(0, 0);
            this.hChooseFeatures.Name = "hChooseFeatures";
            this.hChooseFeatures.Size = new System.Drawing.Size(678, 64);
            this.hChooseFeatures.TabIndex = 0;
            this.hChooseFeatures.Title = "Feature Selection";
            // 
            // tv_FeatureView
            // 
            this.tv_FeatureView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tv_FeatureView.CheckBoxes = true;
            this.tv_FeatureView.HideSelection = false;
            this.tv_FeatureView.Location = new System.Drawing.Point(12, 98);
            this.tv_FeatureView.Name = "tv_FeatureView";
            this.tv_FeatureView.Size = new System.Drawing.Size(361, 285);
            this.tv_FeatureView.TabIndex = 1;
            this.tv_FeatureView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tv_FeatureView_AfterCheck);
            this.tv_FeatureView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tv_FeatureView_AfterSelect);
            this.tv_FeatureView.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.tv_FeatureView_BeforeCheck);
            // 
            // wpChooseLibraries
            // 
            this.wpChooseLibraries.Controls.Add(this.cbLibraryCond);
            this.wpChooseLibraries.Controls.Add(this.label5);
            this.wpChooseLibraries.Controls.Add(this.cbShowAllLibCatChoices);
            this.wpChooseLibraries.Controls.Add(this.tv_LibraryView);
            this.wpChooseLibraries.Controls.Add(this.cbProjectSelect_Library);
            this.wpChooseLibraries.Controls.Add(this.label3);
            this.wpChooseLibraries.Controls.Add(this.rtb_LibraryDescription);
            this.wpChooseLibraries.Controls.Add(this.header1);
            this.wpChooseLibraries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wpChooseLibraries.IsFinishPage = false;
            this.wpChooseLibraries.Location = new System.Drawing.Point(0, 0);
            this.wpChooseLibraries.Name = "wpChooseLibraries";
            this.wpChooseLibraries.Size = new System.Drawing.Size(678, 387);
            this.wpChooseLibraries.TabIndex = 7;
            this.wpChooseLibraries.CloseFromNext += new Gui.Wizard.PageEventHandler(this.wpChooseLibraries_CloseFromNext);
            this.wpChooseLibraries.CloseFromBack += new Gui.Wizard.PageEventHandler(this.wpChooseLibraries_CloseFromBack);
            this.wpChooseLibraries.ShowFromNext += new System.EventHandler(this.wpChooseLibraries_ShowFromNext);
            // 
            // cbLibraryCond
            // 
            this.cbLibraryCond.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbLibraryCond.FormattingEnabled = true;
            this.cbLibraryCond.Items.AddRange(new object[] {
            "",
            "RTM",
            "Not RTM"});
            this.cbLibraryCond.Location = new System.Drawing.Point(69, 362);
            this.cbLibraryCond.Name = "cbLibraryCond";
            this.cbLibraryCond.Size = new System.Drawing.Size(304, 21);
            this.cbLibraryCond.TabIndex = 2;
            this.cbLibraryCond.SelectionChangeCommitted += new System.EventHandler(this.cbLibraryCond_SelectedIndexCommitted);
            this.cbLibraryCond.Leave += new System.EventHandler(this.cbLibraryCond_Leave);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 366);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Condition:";
            // 
            // cbShowAllLibCatChoices
            // 
            this.cbShowAllLibCatChoices.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbShowAllLibCatChoices.AutoSize = true;
            this.cbShowAllLibCatChoices.Location = new System.Drawing.Point(379, 72);
            this.cbShowAllLibCatChoices.Name = "cbShowAllLibCatChoices";
            this.cbShowAllLibCatChoices.Size = new System.Drawing.Size(106, 17);
            this.cbShowAllLibCatChoices.TabIndex = 8;
            this.cbShowAllLibCatChoices.Text = "Show All Choices";
            this.cbShowAllLibCatChoices.UseVisualStyleBackColor = true;
            this.cbShowAllLibCatChoices.CheckedChanged += new System.EventHandler(this.cbShowAllLibCatChoices_CheckedChanged);
            // 
            // tv_LibraryView
            // 
            this.tv_LibraryView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tv_LibraryView.CheckBoxes = true;
            this.tv_LibraryView.HideSelection = false;
            this.tv_LibraryView.Location = new System.Drawing.Point(12, 98);
            this.tv_LibraryView.Name = "tv_LibraryView";
            this.tv_LibraryView.Size = new System.Drawing.Size(361, 260);
            this.tv_LibraryView.TabIndex = 1;
            this.tv_LibraryView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tv_LibraryView_AfterCheck);
            this.tv_LibraryView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tv_LibraryView_AfterSelect);
            this.tv_LibraryView.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.tv_LibraryView_BeforeCheck);
            // 
            // cbProjectSelect_Library
            // 
            this.cbProjectSelect_Library.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbProjectSelect_Library.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbProjectSelect_Library.FormattingEnabled = true;
            this.cbProjectSelect_Library.Location = new System.Drawing.Point(66, 70);
            this.cbProjectSelect_Library.Name = "cbProjectSelect_Library";
            this.cbProjectSelect_Library.Size = new System.Drawing.Size(307, 21);
            this.cbProjectSelect_Library.TabIndex = 0;
            this.cbProjectSelect_Library.SelectedIndexChanged += new System.EventHandler(this.cbProjectSelect_Library_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 73);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Project: ";
            // 
            // rtb_LibraryDescription
            // 
            this.rtb_LibraryDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtb_LibraryDescription.BackColor = System.Drawing.SystemColors.Info;
            this.rtb_LibraryDescription.Location = new System.Drawing.Point(379, 98);
            this.rtb_LibraryDescription.Name = "rtb_LibraryDescription";
            this.rtb_LibraryDescription.Size = new System.Drawing.Size(287, 289);
            this.rtb_LibraryDescription.TabIndex = 3;
            this.rtb_LibraryDescription.Text = "";
            // 
            // header1
            // 
            this.header1.BackColor = System.Drawing.SystemColors.Control;
            this.header1.CausesValidation = false;
            this.header1.Description = "Choose the implementation to include for these unresolved libraries.";
            this.header1.Dock = System.Windows.Forms.DockStyle.Top;
            this.header1.Image = ((System.Drawing.Image)(resources.GetObject("header1.Image")));
            this.header1.Location = new System.Drawing.Point(0, 0);
            this.header1.Name = "header1";
            this.header1.Size = new System.Drawing.Size(678, 64);
            this.header1.TabIndex = 3;
            this.header1.Title = "Unresolved Library Selection";
            // 
            // wpFinish
            // 
            this.wpFinish.Controls.Add(this.header4);
            this.wpFinish.Controls.Add(this.rtb_Finish);
            this.wpFinish.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wpFinish.IsFinishPage = true;
            this.wpFinish.Location = new System.Drawing.Point(0, 0);
            this.wpFinish.Name = "wpFinish";
            this.wpFinish.Size = new System.Drawing.Size(678, 387);
            this.wpFinish.TabIndex = 6;
            this.wpFinish.CloseFromNext += new Gui.Wizard.PageEventHandler(this.wpFinish_CloseFromNext);
            this.wpFinish.CloseFromBack += new Gui.Wizard.PageEventHandler(this.wpFinish_CloseFromBack);
            this.wpFinish.ShowFromNext += new System.EventHandler(this.wpFinish_ShowFromNext);
            // 
            // header4
            // 
            this.header4.BackColor = System.Drawing.SystemColors.Control;
            this.header4.CausesValidation = false;
            this.header4.Description = "Review the details of the new solution.";
            this.header4.Dock = System.Windows.Forms.DockStyle.Top;
            this.header4.Image = ((System.Drawing.Image)(resources.GetObject("header4.Image")));
            this.header4.Location = new System.Drawing.Point(0, 0);
            this.header4.Name = "header4";
            this.header4.Size = new System.Drawing.Size(678, 64);
            this.header4.TabIndex = 4;
            this.header4.Title = "Solution Summary";
            // 
            // rtb_Finish
            // 
            this.rtb_Finish.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtb_Finish.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.rtb_Finish.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtb_Finish.Location = new System.Drawing.Point(12, 81);
            this.rtb_Finish.Name = "rtb_Finish";
            this.rtb_Finish.Size = new System.Drawing.Size(654, 292);
            this.rtb_Finish.TabIndex = 1;
            this.rtb_Finish.Text = "";
            // 
            // SolutionWizardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(678, 435);
            this.Controls.Add(this.Wiz);
            this.Icon = global::SolutionWizard.Properties.Resources.SolutionWizard1;
            this.MinimumSize = new System.Drawing.Size(684, 461);
            this.Name = "SolutionWizardForm";
            this.Text = ".NET Micro Framework Solution Wizard";
            this.Load += new System.EventHandler(this.SolutionWizardForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SolutionWizardForm_FormClosing);
            this.Wiz.ResumeLayout(false);
            this.wpWelcome.ResumeLayout(false);
            this.wpWelcome.PerformLayout();
            this.wpChooseTask.ResumeLayout(false);
            this.wpChooseTask.PerformLayout();
            this.wpSelectExisting.ResumeLayout(false);
            this.wpSelectExisting.PerformLayout();
            this.wpCreatePlatform.ResumeLayout(false);
            this.wpCreatePlatform.PerformLayout();
            this.wpSolutionCfg.ResumeLayout(false);
            this.wpSolutionCfg.PerformLayout();
            this.wpChooseProjects.ResumeLayout(false);
            this.wpChooseFeatures.ResumeLayout(false);
            this.wpChooseFeatures.PerformLayout();
            this.wpChooseLibraries.ResumeLayout(false);
            this.wpChooseLibraries.PerformLayout();
            this.wpFinish.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private Gui.Wizard.Wizard Wiz;
        private Gui.Wizard.WizardPage wpChooseTask;
        private Gui.Wizard.WizardPage wpWelcome;
        private Gui.Wizard.InfoPage ipWelcome;
        private Gui.Wizard.WizardPage wpChooseFeatures;
        private Gui.Wizard.WizardPage wpSelectExisting;
        private Gui.Wizard.Header hChooseTask;
        private System.Windows.Forms.RadioButton rbCreatePlatform;
        private System.Windows.Forms.RadioButton rbEditSolution;
        private Gui.Wizard.Header hChooseFeatures;
        private System.Windows.Forms.RichTextBox rtb_FeatureDescription;
        private Gui.Wizard.WizardPage wpCreatePlatform;
        private Gui.Wizard.Header h_CreatePlatform;
        private System.Windows.Forms.TextBox tb_PlatformName;
        private System.Windows.Forms.Label l_PlatformDescription;
        private System.Windows.Forms.Label l_PlatformName;
        private System.Windows.Forms.TextBox tb_PlatformDescription;
        private System.Windows.Forms.FolderBrowserDialog fbd_PlatformFolder;
        private System.Windows.Forms.SaveFileDialog sfd_NewPlatform;
        private Gui.Wizard.WizardPage wpFinish;
        private System.Windows.Forms.RichTextBox rtb_PlatformFeatures;
        private Gui.Wizard.Header h_SelectExisting;
        private System.Windows.Forms.RadioButton rb_BrowsePlatformFile;
        private System.Windows.Forms.RadioButton rb_SelectFromList;
        private System.Windows.Forms.TreeView tv_PlatformList;
        private System.Windows.Forms.Button b_Browse;
        private System.Windows.Forms.TextBox tb_SelectedPlatform;
        private System.Windows.Forms.RadioButton rbCloneSolution;
        private Gui.Wizard.WizardPage wpChooseLibraries;
        private System.Windows.Forms.RichTextBox rtb_LibraryDescription;
        private System.Windows.Forms.TreeView tv_LibraryView;
        private Gui.Wizard.Header header1;
        private System.Windows.Forms.Button buttonSpoClientBrowse;
        private System.Windows.Forms.TextBox textBoxSpoClientPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label pidLabel;
        private Gui.Wizard.WizardPage wpChooseProjects;
        private System.Windows.Forms.RichTextBox rtb_ProjectDescription;
        private System.Windows.Forms.TreeView tv_SelectedProjects;
        private Gui.Wizard.Header header2;
        private System.Windows.Forms.ComboBox cbProjectSelect_Feature;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbProjectSelect_Library;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RichTextBox rtb_Finish;
        private System.Windows.Forms.CheckBox cbShowAllLibCatChoices;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbFeatureCond;
        private System.Windows.Forms.ComboBox cbLibraryCond;
        private Gui.Wizard.WizardPage wpSolutionCfg;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cbProcessor;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cbDbgPort;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cbRuntimeMemCfg;
        private System.Windows.Forms.TextBox tbFlashSize;
        private System.Windows.Forms.TextBox tbFlashBase;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox tbClockSpeed;
        private System.Windows.Forms.TextBox tbRamSize;
        private System.Windows.Forms.TextBox tbRamBase;
        private System.Windows.Forms.TextBox tbSlowClock;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox tb_SolutionAuthor;
        private Gui.Wizard.Header header3;
        private System.Windows.Forms.Label CloneDesc;
        private System.Windows.Forms.Label CreateDesc;
        private System.Windows.Forms.Label EditDesc;
        private System.Windows.Forms.TreeView tv_FeatureView;
        private Gui.Wizard.Header header4;
    }
}

