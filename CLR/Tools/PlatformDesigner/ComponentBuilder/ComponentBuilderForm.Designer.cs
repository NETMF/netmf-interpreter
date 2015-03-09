namespace ComponentBuilder
{
    partial class ComponentBuilderForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Assemblies");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Build Parameters");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Build Tools");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Features");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Libraries");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Library Categories");
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Processors");
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("Solutions");
            System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("Inventory", new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3,
            treeNode4,
            treeNode5,
            treeNode6,
            treeNode7,
            treeNode8});
            this.contextMenuStripAddBuildTools = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addBuildToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadBuildToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadAllBuildToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripAddFeature = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addFeatureToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.loadFeaturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripAddLibrary = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addLibraryToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.loadLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadLibrariesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripLibraryCategorys = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addLibraryCategoryToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.loadLibraryCategoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadLibraryCategoriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveLibraryCategoriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripAddProcessor = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addProcessorToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.loadProcessorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadAllProcessorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripAddProject = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addProjectToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.loadSolutionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadAllSolutionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripInventory = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addFeatureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addProcessorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addBuildToolToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.addLibraryCategoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripAddProcType = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addISAToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.Property = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.treeViewInventory = new System.Windows.Forms.TreeView();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadAllComponentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadFromXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripDependency = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addDependencyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripMemoryRegions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addMemoryRegionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripMemorySections = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addMemorySectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripMemoryMaps = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addMemoryMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.contextMenuStripEnvVarCollection = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addEnvironmentalVariableSetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripProject = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.generateProjectToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.cloneSolutionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadMemoryMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripAddMiscTool = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addMiscToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripAddBuildToolsRef = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addBuildToolOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripProcessor = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveProcessorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripLibrary = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripLibraryCategory = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveLibraryCategoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripAddAssembly = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveAssemblyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadAllAssembliesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAssembliesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripSave = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripAddBuildTools.SuspendLayout();
            this.contextMenuStripAddFeature.SuspendLayout();
            this.contextMenuStripAddLibrary.SuspendLayout();
            this.contextMenuStripLibraryCategorys.SuspendLayout();
            this.contextMenuStripAddProcessor.SuspendLayout();
            this.contextMenuStripAddProject.SuspendLayout();
            this.contextMenuStripInventory.SuspendLayout();
            this.contextMenuStripAddProcType.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.contextMenuStripDependency.SuspendLayout();
            this.contextMenuStripMemoryRegions.SuspendLayout();
            this.contextMenuStripMemorySections.SuspendLayout();
            this.contextMenuStripMemoryMaps.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.contextMenuStripEnvVarCollection.SuspendLayout();
            this.contextMenuStripProject.SuspendLayout();
            this.contextMenuStripAddMiscTool.SuspendLayout();
            this.contextMenuStripAddBuildToolsRef.SuspendLayout();
            this.contextMenuStripProcessor.SuspendLayout();
            this.contextMenuStripLibrary.SuspendLayout();
            this.contextMenuStripLibraryCategory.SuspendLayout();
            this.contextMenuStripAddAssembly.SuspendLayout();
            this.contextMenuStripSave.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStripAddBuildTools
            // 
            this.contextMenuStripAddBuildTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addBuildToolToolStripMenuItem,
            this.loadBuildToolToolStripMenuItem,
            this.loadAllBuildToolsToolStripMenuItem});
            this.contextMenuStripAddBuildTools.Name = "contextMenuStripAddBuildTools";
            this.contextMenuStripAddBuildTools.Size = new System.Drawing.Size(176, 70);
            // 
            // addBuildToolToolStripMenuItem
            // 
            this.addBuildToolToolStripMenuItem.Name = "addBuildToolToolStripMenuItem";
            this.addBuildToolToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.addBuildToolToolStripMenuItem.Text = "Add Build Tool";
            this.addBuildToolToolStripMenuItem.Click += new System.EventHandler(this.addBuildToolToolStripMenuItem_Click);
            // 
            // loadBuildToolToolStripMenuItem
            // 
            this.loadBuildToolToolStripMenuItem.Name = "loadBuildToolToolStripMenuItem";
            this.loadBuildToolToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.loadBuildToolToolStripMenuItem.Text = "Load Build Tool";
            this.loadBuildToolToolStripMenuItem.Click += new System.EventHandler(this.loadBuildToolToolStripMenuItem_Click);
            // 
            // loadAllBuildToolsToolStripMenuItem
            // 
            this.loadAllBuildToolsToolStripMenuItem.Name = "loadAllBuildToolsToolStripMenuItem";
            this.loadAllBuildToolsToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.loadAllBuildToolsToolStripMenuItem.Text = "Load All Build Tools";
            this.loadAllBuildToolsToolStripMenuItem.Click += new System.EventHandler(this.loadAllBuildToolsToolStripMenuItem_Click);
            // 
            // contextMenuStripAddFeature
            // 
            this.contextMenuStripAddFeature.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addFeatureToolStripMenuItem1,
            this.loadFeaturesToolStripMenuItem});
            this.contextMenuStripAddFeature.Name = "contextMenuStripAddFeature";
            this.contextMenuStripAddFeature.Size = new System.Drawing.Size(167, 48);
            // 
            // addFeatureToolStripMenuItem1
            // 
            this.addFeatureToolStripMenuItem1.Name = "addFeatureToolStripMenuItem1";
            this.addFeatureToolStripMenuItem1.Size = new System.Drawing.Size(166, 22);
            this.addFeatureToolStripMenuItem1.Text = "Add Feature";
            this.addFeatureToolStripMenuItem1.Click += new System.EventHandler(this.addFeatureToolStripMenuItem_Click);
            // 
            // loadFeaturesToolStripMenuItem
            // 
            this.loadFeaturesToolStripMenuItem.Name = "loadFeaturesToolStripMenuItem";
            this.loadFeaturesToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.loadFeaturesToolStripMenuItem.Text = "Load Features...";
            this.loadFeaturesToolStripMenuItem.Click += new System.EventHandler(this.loadFeaturesToolStripMenuItem_Click);
            // 
            // contextMenuStripAddLibrary
            // 
            this.contextMenuStripAddLibrary.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addLibraryToolStripMenuItem1,
            this.loadLibraryToolStripMenuItem,
            this.loadLibrariesToolStripMenuItem});
            this.contextMenuStripAddLibrary.Name = "contextMenuStripAddLibrary";
            this.contextMenuStripAddLibrary.Size = new System.Drawing.Size(152, 70);
            // 
            // addLibraryToolStripMenuItem1
            // 
            this.addLibraryToolStripMenuItem1.Name = "addLibraryToolStripMenuItem1";
            this.addLibraryToolStripMenuItem1.Size = new System.Drawing.Size(151, 22);
            this.addLibraryToolStripMenuItem1.Text = "Add Library";
            this.addLibraryToolStripMenuItem1.Click += new System.EventHandler(this.addLibraryToolStripMenuItem_Click);
            // 
            // loadLibraryToolStripMenuItem
            // 
            this.loadLibraryToolStripMenuItem.Name = "loadLibraryToolStripMenuItem";
            this.loadLibraryToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.loadLibraryToolStripMenuItem.Text = "Load Library";
            this.loadLibraryToolStripMenuItem.Click += new System.EventHandler(this.loadLibraryToolStripMenuItem_Click);
            // 
            // loadLibrariesToolStripMenuItem
            // 
            this.loadLibrariesToolStripMenuItem.Name = "loadLibrariesToolStripMenuItem";
            this.loadLibrariesToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.loadLibrariesToolStripMenuItem.Text = "Load Libraries";
            this.loadLibrariesToolStripMenuItem.Click += new System.EventHandler(this.loadLibrariesToolStripMenuItem_Click);
            // 
            // contextMenuStripLibraryCategorys
            // 
            this.contextMenuStripLibraryCategorys.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addLibraryCategoryToolStripMenuItem1,
            this.loadLibraryCategoryToolStripMenuItem,
            this.loadLibraryCategoriesToolStripMenuItem,
            this.saveLibraryCategoriesToolStripMenuItem});
            this.contextMenuStripLibraryCategorys.Name = "contextMenuStripLibraryCategorys";
            this.contextMenuStripLibraryCategorys.Size = new System.Drawing.Size(201, 92);
            // 
            // addLibraryCategoryToolStripMenuItem1
            // 
            this.addLibraryCategoryToolStripMenuItem1.Name = "addLibraryCategoryToolStripMenuItem1";
            this.addLibraryCategoryToolStripMenuItem1.Size = new System.Drawing.Size(200, 22);
            this.addLibraryCategoryToolStripMenuItem1.Text = "Add Library Category";
            this.addLibraryCategoryToolStripMenuItem1.Click += new System.EventHandler(this.addLibraryCategoryToolStripMenuItem_Click);
            // 
            // loadLibraryCategoryToolStripMenuItem
            // 
            this.loadLibraryCategoryToolStripMenuItem.Name = "loadLibraryCategoryToolStripMenuItem";
            this.loadLibraryCategoryToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.loadLibraryCategoryToolStripMenuItem.Text = "Load Library Category";
            this.loadLibraryCategoryToolStripMenuItem.Click += new System.EventHandler(this.loadLibraryCategoryToolStripMenuItem_Click);
            // 
            // loadLibraryCategoriesToolStripMenuItem
            // 
            this.loadLibraryCategoriesToolStripMenuItem.Name = "loadLibraryCategoriesToolStripMenuItem";
            this.loadLibraryCategoriesToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.loadLibraryCategoriesToolStripMenuItem.Text = "Load Library Categories";
            this.loadLibraryCategoriesToolStripMenuItem.Click += new System.EventHandler(this.loadLibraryCategoriesToolStripMenuItem_Click);
            // 
            // saveLibraryCategoriesToolStripMenuItem
            // 
            this.saveLibraryCategoriesToolStripMenuItem.Name = "saveLibraryCategoriesToolStripMenuItem";
            this.saveLibraryCategoriesToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.saveLibraryCategoriesToolStripMenuItem.Text = "Save Library Categories";
            this.saveLibraryCategoriesToolStripMenuItem.Click += new System.EventHandler(this.saveLibraryCategoriesToolStripMenuItem_Click);
            // 
            // contextMenuStripAddProcessor
            // 
            this.contextMenuStripAddProcessor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addProcessorToolStripMenuItem1,
            this.loadProcessorToolStripMenuItem,
            this.loadAllProcessorsToolStripMenuItem});
            this.contextMenuStripAddProcessor.Name = "contextMenuStripAddProcessor";
            this.contextMenuStripAddProcessor.Size = new System.Drawing.Size(178, 70);
            // 
            // addProcessorToolStripMenuItem1
            // 
            this.addProcessorToolStripMenuItem1.Name = "addProcessorToolStripMenuItem1";
            this.addProcessorToolStripMenuItem1.Size = new System.Drawing.Size(177, 22);
            this.addProcessorToolStripMenuItem1.Text = "Add Processor";
            this.addProcessorToolStripMenuItem1.Click += new System.EventHandler(this.addProcessorToolStripMenuItem_Click);
            // 
            // loadProcessorToolStripMenuItem
            // 
            this.loadProcessorToolStripMenuItem.Name = "loadProcessorToolStripMenuItem";
            this.loadProcessorToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.loadProcessorToolStripMenuItem.Text = "Load Processor";
            this.loadProcessorToolStripMenuItem.Click += new System.EventHandler(this.loadProcessorToolStripMenuItem_Click);
            // 
            // loadAllProcessorsToolStripMenuItem
            // 
            this.loadAllProcessorsToolStripMenuItem.Name = "loadAllProcessorsToolStripMenuItem";
            this.loadAllProcessorsToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.loadAllProcessorsToolStripMenuItem.Text = "Load All Processors";
            this.loadAllProcessorsToolStripMenuItem.Click += new System.EventHandler(this.loadAllProcessorsToolStripMenuItem_Click);
            // 
            // contextMenuStripAddProject
            // 
            this.contextMenuStripAddProject.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addProjectToolStripMenuItem1,
            this.loadSolutionToolStripMenuItem,
            this.loadAllSolutionsToolStripMenuItem});
            this.contextMenuStripAddProject.Name = "contextMenuStripAddProject";
            this.contextMenuStripAddProject.Size = new System.Drawing.Size(169, 70);
            // 
            // addProjectToolStripMenuItem1
            // 
            this.addProjectToolStripMenuItem1.Name = "addProjectToolStripMenuItem1";
            this.addProjectToolStripMenuItem1.Size = new System.Drawing.Size(168, 22);
            this.addProjectToolStripMenuItem1.Text = "Add Solution";
            this.addProjectToolStripMenuItem1.Click += new System.EventHandler(this.addPlatformToolStripMenuItem_Click);
            // 
            // loadSolutionToolStripMenuItem
            // 
            this.loadSolutionToolStripMenuItem.Name = "loadSolutionToolStripMenuItem";
            this.loadSolutionToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.loadSolutionToolStripMenuItem.Text = "Load Solution";
            this.loadSolutionToolStripMenuItem.Click += new System.EventHandler(this.loadSolutionToolStripMenuItem_Click);
            // 
            // loadAllSolutionsToolStripMenuItem
            // 
            this.loadAllSolutionsToolStripMenuItem.Name = "loadAllSolutionsToolStripMenuItem";
            this.loadAllSolutionsToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.loadAllSolutionsToolStripMenuItem.Text = "Load All Solutions";
            this.loadAllSolutionsToolStripMenuItem.Click += new System.EventHandler(this.loadAllSolutionsToolStripMenuItem_Click);
            // 
            // contextMenuStripInventory
            // 
            this.contextMenuStripInventory.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addProjectToolStripMenuItem,
            this.addFeatureToolStripMenuItem,
            this.addLibraryToolStripMenuItem,
            this.addProcessorToolStripMenuItem,
            this.addBuildToolToolStripMenuItem1,
            this.addLibraryCategoryToolStripMenuItem,
            this.refreshToolStripMenuItem1});
            this.contextMenuStripInventory.Name = "contextMenuStripInventory";
            this.contextMenuStripInventory.Size = new System.Drawing.Size(168, 158);
            // 
            // addProjectToolStripMenuItem
            // 
            this.addProjectToolStripMenuItem.Name = "addProjectToolStripMenuItem";
            this.addProjectToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.addProjectToolStripMenuItem.Text = "Add Solution";
            this.addProjectToolStripMenuItem.Click += new System.EventHandler(this.addPlatformToolStripMenuItem_Click);
            // 
            // addFeatureToolStripMenuItem
            // 
            this.addFeatureToolStripMenuItem.Name = "addFeatureToolStripMenuItem";
            this.addFeatureToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.addFeatureToolStripMenuItem.Text = "Add Feature";
            this.addFeatureToolStripMenuItem.Click += new System.EventHandler(this.addFeatureToolStripMenuItem_Click);
            // 
            // addLibraryToolStripMenuItem
            // 
            this.addLibraryToolStripMenuItem.Name = "addLibraryToolStripMenuItem";
            this.addLibraryToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.addLibraryToolStripMenuItem.Text = "Add Library";
            this.addLibraryToolStripMenuItem.Click += new System.EventHandler(this.addLibraryToolStripMenuItem_Click);
            // 
            // addProcessorToolStripMenuItem
            // 
            this.addProcessorToolStripMenuItem.Name = "addProcessorToolStripMenuItem";
            this.addProcessorToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.addProcessorToolStripMenuItem.Text = "Add Processor";
            this.addProcessorToolStripMenuItem.Click += new System.EventHandler(this.addProcessorToolStripMenuItem_Click);
            // 
            // addBuildToolToolStripMenuItem1
            // 
            this.addBuildToolToolStripMenuItem1.Name = "addBuildToolToolStripMenuItem1";
            this.addBuildToolToolStripMenuItem1.Size = new System.Drawing.Size(167, 22);
            this.addBuildToolToolStripMenuItem1.Text = "Add Build Tool";
            this.addBuildToolToolStripMenuItem1.Click += new System.EventHandler(this.addBuildToolToolStripMenuItem_Click);
            // 
            // addLibraryCategoryToolStripMenuItem
            // 
            this.addLibraryCategoryToolStripMenuItem.Name = "addLibraryCategoryToolStripMenuItem";
            this.addLibraryCategoryToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.addLibraryCategoryToolStripMenuItem.Text = "Add Library Type";
            this.addLibraryCategoryToolStripMenuItem.Click += new System.EventHandler(this.addLibraryCategoryToolStripMenuItem_Click);
            // 
            // refreshToolStripMenuItem1
            // 
            this.refreshToolStripMenuItem1.Name = "refreshToolStripMenuItem1";
            this.refreshToolStripMenuItem1.Size = new System.Drawing.Size(167, 22);
            this.refreshToolStripMenuItem1.Text = "Refresh";
            this.refreshToolStripMenuItem1.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // contextMenuStripAddProcType
            // 
            this.contextMenuStripAddProcType.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addISAToolStripMenuItem1});
            this.contextMenuStripAddProcType.Name = "contextMenuStripAddProcType";
            this.contextMenuStripAddProcType.Size = new System.Drawing.Size(79, 26);
            // 
            // addISAToolStripMenuItem1
            // 
            this.addISAToolStripMenuItem1.Name = "addISAToolStripMenuItem1";
            this.addISAToolStripMenuItem1.Size = new System.Drawing.Size(78, 22);
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Property,
            this.Value});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView.Location = new System.Drawing.Point(-3, -2);
            this.dataGridView.Name = "dataGridView";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridView.Size = new System.Drawing.Size(416, 454);
            this.dataGridView.TabIndex = 0;
            this.dataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellValueChanged);
            this.dataGridView.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dataGridView_CellBeginEdit);
            this.dataGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellDoubleClick);
            this.dataGridView.Leave += new System.EventHandler(this.dataGridView_Leave);
            this.dataGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridView_KeyDown);
            this.dataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellContentClick);
            // 
            // Property
            // 
            this.Property.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Property.HeaderText = "Property";
            this.Property.Name = "Property";
            this.Property.ReadOnly = true;
            this.Property.Width = 71;
            // 
            // Value
            // 
            this.Value.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Value.HeaderText = "Value";
            this.Value.Name = "Value";
            this.Value.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // treeViewInventory
            // 
            this.treeViewInventory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewInventory.ContextMenuStrip = this.contextMenuStripInventory;
            this.treeViewInventory.HideSelection = false;
            this.treeViewInventory.LabelEdit = true;
            this.treeViewInventory.Location = new System.Drawing.Point(-2, -2);
            this.treeViewInventory.Name = "treeViewInventory";
            treeNode1.Name = "Assemblies";
            treeNode1.Text = "Assemblies";
            treeNode2.Name = "BuildParameters";
            treeNode2.Text = "Build Parameters";
            treeNode3.ContextMenuStrip = this.contextMenuStripAddBuildTools;
            treeNode3.Name = "BuildTools";
            treeNode3.Text = "Build Tools";
            treeNode4.ContextMenuStrip = this.contextMenuStripAddFeature;
            treeNode4.Name = "Features";
            treeNode4.Text = "Features";
            treeNode5.ContextMenuStrip = this.contextMenuStripAddLibrary;
            treeNode5.Name = "Libraries";
            treeNode5.Text = "Libraries";
            treeNode6.ContextMenuStrip = this.contextMenuStripLibraryCategorys;
            treeNode6.Name = "LibraryCategories";
            treeNode6.Text = "Library Categories";
            treeNode7.ContextMenuStrip = this.contextMenuStripAddProcessor;
            treeNode7.Name = "Processors";
            treeNode7.Text = "Processors";
            treeNode8.ContextMenuStrip = this.contextMenuStripAddProject;
            treeNode8.Name = "Solutions";
            treeNode8.Text = "Solutions";
            treeNode9.ContextMenuStrip = this.contextMenuStripInventory;
            treeNode9.Name = "Inventory";
            treeNode9.Text = "Inventory";
            this.treeViewInventory.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode9});
            this.treeViewInventory.ShowRootLines = false;
            this.treeViewInventory.Size = new System.Drawing.Size(294, 454);
            this.treeViewInventory.TabIndex = 1;
            this.treeViewInventory.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeViewInventory_AfterLabelEdit);
            this.treeViewInventory.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewInventory_AfterSelect);
            this.treeViewInventory.KeyUp += new System.Windows.Forms.KeyEventHandler(this.treeViewInventory_KeyUp);
            this.treeViewInventory.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewInventory_NodeMouseClick);
            this.treeViewInventory.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeViewInventory_BeforeLabelEdit);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(740, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip1_ItemClicked);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.closeToolStripMenuItem,
            this.refreshToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadAllComponentsToolStripMenuItem,
            this.loadFromXMLToolStripMenuItem});
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.loadToolStripMenuItem.Text = "Load";
            // 
            // loadAllComponentsToolStripMenuItem
            // 
            this.loadAllComponentsToolStripMenuItem.Name = "loadAllComponentsToolStripMenuItem";
            this.loadAllComponentsToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.loadAllComponentsToolStripMenuItem.Text = "Load All Components";
            this.loadAllComponentsToolStripMenuItem.Click += new System.EventHandler(this.loadAllComponentsToolStripMenuItem_Click);
            // 
            // loadFromXMLToolStripMenuItem
            // 
            this.loadFromXMLToolStripMenuItem.Name = "loadFromXMLToolStripMenuItem";
            this.loadFromXMLToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.loadFromXMLToolStripMenuItem.Text = "Load From XML";
            this.loadFromXMLToolStripMenuItem.Click += new System.EventHandler(this.loadFromXMLToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveAsToolStripMenuItem,
            this.allToolStripMenuItem});
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.saveAsToolStripMenuItem.Text = "Save New As ...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // allToolStripMenuItem
            // 
            this.allToolStripMenuItem.Name = "allToolStripMenuItem";
            this.allToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.allToolStripMenuItem.Text = "Save All As ...";
            this.allToolStripMenuItem.Click += new System.EventHandler(this.allToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allToolStripMenuItem1});
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.closeToolStripMenuItem.Text = "Close";
            // 
            // allToolStripMenuItem1
            // 
            this.allToolStripMenuItem1.Name = "allToolStripMenuItem1";
            this.allToolStripMenuItem1.Size = new System.Drawing.Size(96, 22);
            this.allToolStripMenuItem1.Text = "All";
            this.allToolStripMenuItem1.Click += new System.EventHandler(this.allToolStripMenuItem1_Click);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // contextMenuStripDependency
            // 
            this.contextMenuStripDependency.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addDependencyToolStripMenuItem});
            this.contextMenuStripDependency.Name = "contextMenuStripDependency";
            this.contextMenuStripDependency.Size = new System.Drawing.Size(168, 48);
            // 
            // addDependencyToolStripMenuItem
            // 
            this.addDependencyToolStripMenuItem.Name = "addDependencyToolStripMenuItem";
            this.addDependencyToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.addDependencyToolStripMenuItem.Text = "Add Dependency";
            this.addDependencyToolStripMenuItem.Click += new System.EventHandler(this.addDependencyToolStripMenuItem_Click);
            // 
            // contextMenuStripMemoryRegions
            // 
            this.contextMenuStripMemoryRegions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addMemoryRegionToolStripMenuItem});
            this.contextMenuStripMemoryRegions.Name = "contextMenuStripMemoryMap";
            this.contextMenuStripMemoryRegions.Size = new System.Drawing.Size(182, 26);
            // 
            // addMemoryRegionToolStripMenuItem
            // 
            this.addMemoryRegionToolStripMenuItem.Name = "addMemoryRegionToolStripMenuItem";
            this.addMemoryRegionToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.addMemoryRegionToolStripMenuItem.Text = "Add Memory Region";
            this.addMemoryRegionToolStripMenuItem.Click += new System.EventHandler(this.addMemoryRegionToolStripMenuItem_Click);
            // 
            // contextMenuStripMemorySections
            // 
            this.contextMenuStripMemorySections.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addMemorySectionToolStripMenuItem});
            this.contextMenuStripMemorySections.Name = "contextMenuStripMemoryRegion";
            this.contextMenuStripMemorySections.Size = new System.Drawing.Size(184, 26);
            // 
            // addMemorySectionToolStripMenuItem
            // 
            this.addMemorySectionToolStripMenuItem.Name = "addMemorySectionToolStripMenuItem";
            this.addMemorySectionToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.addMemorySectionToolStripMenuItem.Text = "Add Memory Section";
            this.addMemorySectionToolStripMenuItem.Click += new System.EventHandler(this.addMemorySectionToolStripMenuItem_Click);
            // 
            // contextMenuStripMemoryMaps
            // 
            this.contextMenuStripMemoryMaps.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addMemoryMapToolStripMenuItem});
            this.contextMenuStripMemoryMaps.Name = "contextMenuStrip1";
            this.contextMenuStripMemoryMaps.Size = new System.Drawing.Size(169, 26);
            // 
            // addMemoryMapToolStripMenuItem
            // 
            this.addMemoryMapToolStripMenuItem.Name = "addMemoryMapToolStripMenuItem";
            this.addMemoryMapToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.addMemoryMapToolStripMenuItem.Text = "Add Memory Map";
            this.addMemoryMapToolStripMenuItem.Click += new System.EventHandler(this.addMemoryMapToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(12, 27);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeViewInventory);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dataGridView);
            this.splitContainer1.Size = new System.Drawing.Size(716, 454);
            this.splitContainer1.SplitterDistance = 297;
            this.splitContainer1.TabIndex = 11;
            // 
            // contextMenuStripEnvVarCollection
            // 
            this.contextMenuStripEnvVarCollection.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addEnvironmentalVariableSetToolStripMenuItem});
            this.contextMenuStripEnvVarCollection.Name = "contextMenuStripEnvVarCollection";
            this.contextMenuStripEnvVarCollection.Size = new System.Drawing.Size(236, 26);
            // 
            // addEnvironmentalVariableSetToolStripMenuItem
            // 
            this.addEnvironmentalVariableSetToolStripMenuItem.Name = "addEnvironmentalVariableSetToolStripMenuItem";
            this.addEnvironmentalVariableSetToolStripMenuItem.Size = new System.Drawing.Size(235, 22);
            this.addEnvironmentalVariableSetToolStripMenuItem.Text = "Add Environmental Variable Set";
            this.addEnvironmentalVariableSetToolStripMenuItem.Click += new System.EventHandler(this.addEnvironmentalVariableSetToolStripMenuItem_Click);
            // 
            // contextMenuStripProject
            // 
            this.contextMenuStripProject.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.generateProjectToolStripMenuItem1,
            this.cloneSolutionToolStripMenuItem,
            this.loadMemoryMapToolStripMenuItem});
            this.contextMenuStripProject.Name = "contextMenuStripProject";
            this.contextMenuStripProject.Size = new System.Drawing.Size(173, 70);
            // 
            // generateProjectToolStripMenuItem1
            // 
            this.generateProjectToolStripMenuItem1.Name = "generateProjectToolStripMenuItem1";
            this.generateProjectToolStripMenuItem1.Size = new System.Drawing.Size(172, 22);
            this.generateProjectToolStripMenuItem1.Text = "Save Solution";
            this.generateProjectToolStripMenuItem1.Click += new System.EventHandler(this.generateProjectToolStripMenuItem1_Click);
            // 
            // cloneSolutionToolStripMenuItem
            // 
            this.cloneSolutionToolStripMenuItem.Name = "cloneSolutionToolStripMenuItem";
            this.cloneSolutionToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.cloneSolutionToolStripMenuItem.Text = "Clone Solution";
            this.cloneSolutionToolStripMenuItem.Click += new System.EventHandler(this.cloneSolutionToolStripMenuItem_Click_1);
            // 
            // loadMemoryMapToolStripMenuItem
            // 
            this.loadMemoryMapToolStripMenuItem.Name = "loadMemoryMapToolStripMenuItem";
            this.loadMemoryMapToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.loadMemoryMapToolStripMenuItem.Text = "Load Memory Map";
            this.loadMemoryMapToolStripMenuItem.Click += new System.EventHandler(this.loadMemoryMapToolStripMenuItem_Click);
            // 
            // contextMenuStripAddMiscTool
            // 
            this.contextMenuStripAddMiscTool.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addMiscToolToolStripMenuItem});
            this.contextMenuStripAddMiscTool.Name = "contextMenuStripAddMiscTool";
            this.contextMenuStripAddMiscTool.Size = new System.Drawing.Size(151, 26);
            // 
            // addMiscToolToolStripMenuItem
            // 
            this.addMiscToolToolStripMenuItem.Name = "addMiscToolToolStripMenuItem";
            this.addMiscToolToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.addMiscToolToolStripMenuItem.Text = "Add Misc Tool";
            this.addMiscToolToolStripMenuItem.Click += new System.EventHandler(this.addMiscToolToolStripMenuItem_Click);
            // 
            // contextMenuStripAddBuildToolsRef
            // 
            this.contextMenuStripAddBuildToolsRef.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addBuildToolOptionsToolStripMenuItem});
            this.contextMenuStripAddBuildToolsRef.Name = "contextMenuStripAddBuildToolsRef";
            this.contextMenuStripAddBuildToolsRef.Size = new System.Drawing.Size(193, 26);
            // 
            // addBuildToolOptionsToolStripMenuItem
            // 
            this.addBuildToolOptionsToolStripMenuItem.Name = "addBuildToolOptionsToolStripMenuItem";
            this.addBuildToolOptionsToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.addBuildToolOptionsToolStripMenuItem.Text = "Add Build Tool Options";
            this.addBuildToolOptionsToolStripMenuItem.Click += new System.EventHandler(this.addBuildToolOptionsToolStripMenuItem_Click);
            // 
            // contextMenuStripProcessor
            // 
            this.contextMenuStripProcessor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveProcessorToolStripMenuItem});
            this.contextMenuStripProcessor.Name = "contextMenuStripProcessor";
            this.contextMenuStripProcessor.Size = new System.Drawing.Size(160, 26);
            // 
            // saveProcessorToolStripMenuItem
            // 
            this.saveProcessorToolStripMenuItem.Name = "saveProcessorToolStripMenuItem";
            this.saveProcessorToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.saveProcessorToolStripMenuItem.Text = "Save Processor";
            this.saveProcessorToolStripMenuItem.Click += new System.EventHandler(this.saveProcessorToolStripMenuItem_Click);
            // 
            // contextMenuStripLibrary
            // 
            this.contextMenuStripLibrary.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveLibraryToolStripMenuItem});
            this.contextMenuStripLibrary.Name = "contextMenuStripLibrary";
            this.contextMenuStripLibrary.Size = new System.Drawing.Size(79, 26);
            // 
            // saveLibraryToolStripMenuItem
            // 
            this.saveLibraryToolStripMenuItem.Name = "saveLibraryToolStripMenuItem";
            this.saveLibraryToolStripMenuItem.Size = new System.Drawing.Size(78, 22);
            // 
            // contextMenuStripLibraryCategory
            // 
            this.contextMenuStripLibraryCategory.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveLibraryCategoryToolStripMenuItem});
            this.contextMenuStripLibraryCategory.Name = "contextMenuStripLibraryCategory";
            this.contextMenuStripLibraryCategory.Size = new System.Drawing.Size(79, 26);
            // 
            // saveLibraryCategoryToolStripMenuItem
            // 
            this.saveLibraryCategoryToolStripMenuItem.Name = "saveLibraryCategoryToolStripMenuItem";
            this.saveLibraryCategoryToolStripMenuItem.Size = new System.Drawing.Size(78, 22);
            // 
            // contextMenuStripAddAssembly
            // 
            this.contextMenuStripAddAssembly.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveAssemblyToolStripMenuItem,
            this.loadAllAssembliesToolStripMenuItem,
            this.saveAssembliesToolStripMenuItem});
            this.contextMenuStripAddAssembly.Name = "contextMenuStripAssembly";
            this.contextMenuStripAddAssembly.Size = new System.Drawing.Size(179, 70);
            // 
            // saveAssemblyToolStripMenuItem
            // 
            this.saveAssemblyToolStripMenuItem.Name = "saveAssemblyToolStripMenuItem";
            this.saveAssemblyToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.saveAssemblyToolStripMenuItem.Text = "Load Assembly";
            // 
            // loadAllAssembliesToolStripMenuItem
            // 
            this.loadAllAssembliesToolStripMenuItem.Name = "loadAllAssembliesToolStripMenuItem";
            this.loadAllAssembliesToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.loadAllAssembliesToolStripMenuItem.Text = "Load All Assemblies";
            this.loadAllAssembliesToolStripMenuItem.Click += new System.EventHandler(this.loadAllAssembliesToolStripMenuItem_Click);
            // 
            // saveAssembliesToolStripMenuItem
            // 
            this.saveAssembliesToolStripMenuItem.Name = "saveAssembliesToolStripMenuItem";
            this.saveAssembliesToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.saveAssembliesToolStripMenuItem.Text = "Save All Assemblies";
            this.saveAssembliesToolStripMenuItem.Click += new System.EventHandler(this.saveAssembliesToolStripMenuItem_Click);
            // 
            // contextMenuStripSave
            // 
            this.contextMenuStripSave.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem1});
            this.contextMenuStripSave.Name = "contextMenuStripSave";
            this.contextMenuStripSave.Size = new System.Drawing.Size(110, 26);
            // 
            // saveToolStripMenuItem1
            // 
            this.saveToolStripMenuItem1.Name = "saveToolStripMenuItem1";
            this.saveToolStripMenuItem1.Size = new System.Drawing.Size(109, 22);
            this.saveToolStripMenuItem1.Text = "Save";
            this.saveToolStripMenuItem1.Click += new System.EventHandler(this.saveToolStripMenuItem1_Click);
            // 
            // ComponentBuilderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(740, 493);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.splitContainer1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ComponentBuilderForm";
            this.Text = "Component Builder";
            this.Load += new System.EventHandler(this.ComponentBuilderForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ComponentBuilderForm_FormClosing);
            this.contextMenuStripAddBuildTools.ResumeLayout(false);
            this.contextMenuStripAddFeature.ResumeLayout(false);
            this.contextMenuStripAddLibrary.ResumeLayout(false);
            this.contextMenuStripLibraryCategorys.ResumeLayout(false);
            this.contextMenuStripAddProcessor.ResumeLayout(false);
            this.contextMenuStripAddProject.ResumeLayout(false);
            this.contextMenuStripInventory.ResumeLayout(false);
            this.contextMenuStripAddProcType.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.contextMenuStripDependency.ResumeLayout(false);
            this.contextMenuStripMemoryRegions.ResumeLayout(false);
            this.contextMenuStripMemorySections.ResumeLayout(false);
            this.contextMenuStripMemoryMaps.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.contextMenuStripEnvVarCollection.ResumeLayout(false);
            this.contextMenuStripProject.ResumeLayout(false);
            this.contextMenuStripAddMiscTool.ResumeLayout(false);
            this.contextMenuStripAddBuildToolsRef.ResumeLayout(false);
            this.contextMenuStripProcessor.ResumeLayout(false);
            this.contextMenuStripLibrary.ResumeLayout(false);
            this.contextMenuStripLibraryCategory.ResumeLayout(false);
            this.contextMenuStripAddAssembly.ResumeLayout(false);
            this.contextMenuStripSave.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.TreeView treeViewInventory;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripInventory;
        private System.Windows.Forms.ToolStripMenuItem addFeatureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addProjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addLibraryToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripDependency;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripAddProject;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripAddLibrary;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripAddFeature;
        private System.Windows.Forms.ToolStripMenuItem addProjectToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem addLibraryToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem addFeatureToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripAddProcType;
        private System.Windows.Forms.ToolStripMenuItem addISAToolStripMenuItem1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripAddBuildTools;
        private System.Windows.Forms.ToolStripMenuItem addBuildToolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addBuildToolToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addProcessorToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripAddProcessor;
        private System.Windows.Forms.ToolStripMenuItem addProcessorToolStripMenuItem1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripMemoryRegions;
        private System.Windows.Forms.ToolStripMenuItem addMemoryRegionToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripMemorySections;
        private System.Windows.Forms.ToolStripMenuItem addMemorySectionToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripMemoryMaps;
        private System.Windows.Forms.ToolStripMenuItem addMemoryMapToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripEnvVarCollection;
        private System.Windows.Forms.ToolStripMenuItem addEnvironmentalVariableSetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addLibraryCategoryToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripLibraryCategorys;
        private System.Windows.Forms.ToolStripMenuItem addLibraryCategoryToolStripMenuItem1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripProject;
        private System.Windows.Forms.ToolStripMenuItem generateProjectToolStripMenuItem1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripAddMiscTool;
        private System.Windows.Forms.ToolStripMenuItem addMiscToolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadMemoryMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadBuildToolToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripAddBuildToolsRef;
        private System.Windows.Forms.ToolStripMenuItem loadFeaturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveLibraryCategoriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addDependencyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cloneSolutionToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripProcessor;
        private System.Windows.Forms.ToolStripMenuItem saveProcessorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadProcessorToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripLibrary;
        private System.Windows.Forms.ToolStripMenuItem saveLibraryToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripLibraryCategory;
        private System.Windows.Forms.ToolStripMenuItem saveLibraryCategoryToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripAddAssembly;
        private System.Windows.Forms.ToolStripMenuItem saveAssemblyToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripSave;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem addBuildToolOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadSolutionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadLibraryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadLibraryCategoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadLibraryCategoriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadLibrariesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadAllBuildToolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadAllProcessorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadAllAssembliesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAssembliesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadAllComponentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadFromXMLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadAllSolutionsToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn Property;
        private System.Windows.Forms.DataGridViewTextBoxColumn Value;
    }
}

