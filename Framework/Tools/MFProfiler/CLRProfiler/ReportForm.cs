using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using System.Data;
using System.IO;
using System.Text;


namespace CLRProfiler
{
	/// <summary>
	/// Summary description for AllocationDiffForm.
	/// Table usage:
	/// All Tables created by AllocationDiff object been used by ReportForms 
	///		basedatatable - appears in Function Allocation Diff datagrid
	///		caller - appears in Caller datagrid
	///		callee - appears in Callee datagrid (if selected Item was function)
	///		typeAlloction - appears in Callee datagrid also (if selected Item was data type)
	/// UI interface:
	///		DataGrid object binding above internal tables
	///		DataGridTableStyle controls the appearance of the columns for each table
	///		DataViewManager used for customized view of the entire DataSet
	///		DataViewSettings used to set filters and sort options for a given table. 
	///		DataGrid alignment been controlled by datagrid column width
	///		table content order based on diff Inclusive size (DESC order)
	///		
	/// Detail Definitions:
	///		Memory allocation report can show 9 different details by choose 9 different RadioButton
	///		detail0 shows everything and detail20 only shows diff 
	///	
	///		
	/// </summary>
	public class ReportForm : System.Windows.Forms.Form
	{
		private const int idx_id = 2;
		private const int idx_name = 3;
		private const int idx_mapname = 4;
		private const int idx_prevIncl = 5;
		private const int idx_currIncl = 6;
		private const int idx_diffIncl = 7;
		private const int idx_depth = 14;

		private DiffCallTreeForm diffcallTreeForm = null;
		private AllocationDiff	_allocDiff= null;
		private bool runaswindow = false;
		private bool iscoarse = false;
		private string strFilter = null;
		private string strtypeFilter = null;
		private string CallerCaption = "Caller table";
		private string SelectedCaption = "Selected item";
		private string CalleeCaption = "Callee table";
		private Graph.GraphType graphtype = 0;
		private MainForm f = null;
		private DataViewManager dvm;
		private DataViewManager dvm_caller;
		private DataViewManager dvm_callee;
		private DataGridTableStyle styleBase = new DataGridTableStyle();
		private DataGridTableStyle styleCaller = new DataGridTableStyle();
		private DataGridTableStyle styleCallee = new DataGridTableStyle();
		private DataGridTableStyle styleSelected = new DataGridTableStyle();

		
		private System.Windows.Forms.TabPage tabpallocdiff;
		private System.Windows.Forms.DataGridTableStyle dataGridTableStyle1;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.Splitter splitter2;
		private System.Windows.Forms.GroupBox gpboption;
		private System.Windows.Forms.TextBox txtbPrevlog;
		private System.Windows.Forms.TextBox txtbCurrlog;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.Splitter splitter3;
		private System.Windows.Forms.Splitter splitter4;
		private System.Windows.Forms.DataGrid dgToplevelDiff;
		private System.Windows.Forms.DataGrid dgCallee;
		private System.Windows.Forms.DataGrid dgSelected;
		private System.Windows.Forms.DataGrid dgCaller;
		private System.Windows.Forms.RadioButton rdbDetail20;
		private System.Windows.Forms.RadioButton rdbDetail0;
		private System.Windows.Forms.RadioButton rdbDetail01;
		private System.Windows.Forms.RadioButton rdbDetail02;
		private System.Windows.Forms.RadioButton rdbDetail05;
		private System.Windows.Forms.RadioButton rdbDetail1;
		private System.Windows.Forms.RadioButton rdbDetail2;
		private System.Windows.Forms.RadioButton rdbDetail5;
		private System.Windows.Forms.RadioButton rdbDetail10;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnBrowse1;
		private System.Windows.Forms.Button btnRun;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private string [] columName = {"name", 
								"prevIncl", "currIncl", "diffIncl", 
								"prevExcl", "currExcl", "diffExcl", 
								"prevChildIncl", "currChildIncl", "diffChildIncl",
								"prevTimesCalled", "currTimesCalled", "diffTimesCalled",
								"prevTimesMakecalls", "currTimesMakecalls", "diffTimesMakecalls"};
		private enum columnIdx {name, prevIncl, currIncl, diffIncl,
										prevExcl,currExcl,diffExcl, 
										prevChildIncl,currChildIncl,diffChildIncl,
										prevTimesCalled,currTimesCalled,diffTimesCalled,
										prevTimesMakecalls,currTimesMakecalls,diffTimesMakecalls};

		private string [] coarsecolumName = {"name", 
										  "diffIncl", 
										  "diffExcl", 
										  "diffTimesCalled",
										  "diffTimesMakecalls"};
		private System.Windows.Forms.Button btnBrowse2;
		private System.Windows.Forms.Panel panel2;
	
		private enum coarsecolumnIdx {name, diffIncl,diffExcl,diffTimesCalled,diffTimesMakecalls};

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#region Report -- Entrance
		internal ReportForm(MainForm f)
		{
			this.f = f;
			this.runaswindow  = f.runaswindow;
			this.graphtype = f.graphtype;
			if(	runaswindow)
			{
				InitializeComponent();
			}
			try
			{
				buildReports();
			}
			catch(Exception e)
			{
				throw new Exception(e.Message + "\n");
			}
		}
		private void buildReports()
		{
			try
			{
				switch(graphtype)
				{
					case Graph.GraphType.AllocationGraph:
						_allocDiff = new AllocationDiff ();
						_allocDiff.PrevLogFileName = f.prevlogFileName;
						_allocDiff.CurrLogFileName = f.currlogFileName;
						_allocDiff.BuildAllocationDiffTable();
						if(f.runaswindow)
						{
							this.txtbPrevlog.Text = f.prevlogFileName;
							this.txtbCurrlog.Text = f.currlogFileName;
							AlloccationDiff2Window();
						}
						else
						{
							AlloccationDiff2Console();
						}					
						break;
				

				}
			}
			catch(Exception e)
			{
				if(f.runaswindow)
				{
					MessageBox.Show(e.Message, "Report profiler Error message");
					throw new Exception(e.Message + "\n");
				}
				else
				{
					if(_allocDiff.diffLogFileName != null)
					{
						int at = _allocDiff.diffLogFileName.LastIndexOf(".");
						_allocDiff.diffLogFileName = _allocDiff.diffLogFileName.Substring(0,at);
						_allocDiff.diffLogFileName += ".err";
						if(File.Exists(_allocDiff.diffLogFileName))
							File.Delete(_allocDiff.diffLogFileName);
						FileStream fs = new FileStream(_allocDiff.diffLogFileName,
							FileMode.CreateNew, FileAccess.Write, FileShare.None);
						StreamWriter sw = new StreamWriter(fs);
						sw.WriteLine("Report profiler Error message: \n{0}\n",  e.Message);
						sw.Close();
						
					}
					else
					{
						Console.WriteLine("Report profiler Error message: \n{0}\n",  e.Message);
					}
					throw new Exception(e.Message + "\n");
				}
			}
		}

		#endregion

		#region Clean up
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
		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabpallocdiff = new System.Windows.Forms.TabPage();
			this.dgToplevelDiff = new System.Windows.Forms.DataGrid();
			this.splitter4 = new System.Windows.Forms.Splitter();
			this.dgCaller = new System.Windows.Forms.DataGrid();
			this.splitter3 = new System.Windows.Forms.Splitter();
			this.dgSelected = new System.Windows.Forms.DataGrid();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.panel1 = new System.Windows.Forms.Panel();
			this.btnRun = new System.Windows.Forms.Button();
			this.btnBrowse2 = new System.Windows.Forms.Button();
			this.btnBrowse1 = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.txtbPrevlog = new System.Windows.Forms.TextBox();
			this.txtbCurrlog = new System.Windows.Forms.TextBox();
			this.gpboption = new System.Windows.Forms.GroupBox();
			this.rdbDetail10 = new System.Windows.Forms.RadioButton();
			this.rdbDetail5 = new System.Windows.Forms.RadioButton();
			this.rdbDetail2 = new System.Windows.Forms.RadioButton();
			this.rdbDetail1 = new System.Windows.Forms.RadioButton();
			this.rdbDetail05 = new System.Windows.Forms.RadioButton();
			this.rdbDetail02 = new System.Windows.Forms.RadioButton();
			this.rdbDetail01 = new System.Windows.Forms.RadioButton();
			this.rdbDetail20 = new System.Windows.Forms.RadioButton();
			this.rdbDetail0 = new System.Windows.Forms.RadioButton();
			this.splitter2 = new System.Windows.Forms.Splitter();
			this.dgCallee = new System.Windows.Forms.DataGrid();
			this.dataGridTableStyle1 = new System.Windows.Forms.DataGridTableStyle();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.panel2 = new System.Windows.Forms.Panel();
			this.tabControl1.SuspendLayout();
			this.tabpallocdiff.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgToplevelDiff)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dgCaller)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dgSelected)).BeginInit();
			this.panel1.SuspendLayout();
			this.gpboption.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgCallee)).BeginInit();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabpallocdiff);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.ItemSize = new System.Drawing.Size(79, 18);
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(1216, 886);
			this.tabControl1.TabIndex = 1;
			// 
			// tabpallocdiff
			// 
			this.tabpallocdiff.Controls.Add(this.dgToplevelDiff);
			this.tabpallocdiff.Controls.Add(this.splitter4);
			this.tabpallocdiff.Controls.Add(this.dgCaller);
			this.tabpallocdiff.Controls.Add(this.splitter3);
			this.tabpallocdiff.Controls.Add(this.dgSelected);
			this.tabpallocdiff.Controls.Add(this.splitter1);
			this.tabpallocdiff.Controls.Add(this.panel1);
			this.tabpallocdiff.Controls.Add(this.splitter2);
			this.tabpallocdiff.Controls.Add(this.dgCallee);
			this.tabpallocdiff.Location = new System.Drawing.Point(4, 22);
			this.tabpallocdiff.Name = "tabpallocdiff";
			this.tabpallocdiff.Size = new System.Drawing.Size(1208, 860);
			this.tabpallocdiff.TabIndex = 0;
			this.tabpallocdiff.Text = "Allocation Diff";
			// 
			// dgToplevelDiff
			// 
			this.dgToplevelDiff.DataMember = "";
			this.dgToplevelDiff.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgToplevelDiff.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dgToplevelDiff.Location = new System.Drawing.Point(0, 75);
			this.dgToplevelDiff.Name = "dgToplevelDiff";
			this.dgToplevelDiff.Size = new System.Drawing.Size(1208, 327);
			this.dgToplevelDiff.TabIndex = 15;
			this.dgToplevelDiff.DoubleClick += new System.EventHandler(this.dgToplevelDiff_DoubleClick);
			this.dgToplevelDiff.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dgToplevelDiff_MouseMove);
			// 
			// splitter4
			// 
			this.splitter4.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter4.Location = new System.Drawing.Point(0, 402);
			this.splitter4.Name = "splitter4";
			this.splitter4.Size = new System.Drawing.Size(1208, 3);
			this.splitter4.TabIndex = 14;
			this.splitter4.TabStop = false;
			// 
			// dgCaller
			// 
			this.dgCaller.BackgroundColor = System.Drawing.SystemColors.Window;
			this.dgCaller.CaptionBackColor = System.Drawing.SystemColors.ActiveBorder;
			this.dgCaller.DataMember = "";
			this.dgCaller.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.dgCaller.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dgCaller.Location = new System.Drawing.Point(0, 405);
			this.dgCaller.Name = "dgCaller";
			this.dgCaller.Size = new System.Drawing.Size(1208, 120);
			this.dgCaller.TabIndex = 13;
			this.dgCaller.DoubleClick += new System.EventHandler(this.dgCaller_DoubleClick);
			this.dgCaller.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dgCaller_MouseMove);
			// 
			// splitter3
			// 
			this.splitter3.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter3.Location = new System.Drawing.Point(0, 525);
			this.splitter3.Name = "splitter3";
			this.splitter3.Size = new System.Drawing.Size(1208, 8);
			this.splitter3.TabIndex = 12;
			this.splitter3.TabStop = false;
			// 
			// dgSelected
			// 
			this.dgSelected.BackgroundColor = System.Drawing.SystemColors.Window;
			this.dgSelected.CaptionBackColor = System.Drawing.SystemColors.ActiveBorder;
			this.dgSelected.DataMember = "";
			this.dgSelected.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.dgSelected.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dgSelected.Location = new System.Drawing.Point(0, 533);
			this.dgSelected.Name = "dgSelected";
			this.dgSelected.Size = new System.Drawing.Size(1208, 64);
			this.dgSelected.TabIndex = 11;
			this.dgSelected.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dgSelected_MouseMove);
			// 
			// splitter1
			// 
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter1.Location = new System.Drawing.Point(0, 72);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(1208, 3);
			this.splitter1.TabIndex = 10;
			this.splitter1.TabStop = false;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.btnRun);
			this.panel1.Controls.Add(this.btnBrowse2);
			this.panel1.Controls.Add(this.btnBrowse1);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.txtbPrevlog);
			this.panel1.Controls.Add(this.txtbCurrlog);
			this.panel1.Controls.Add(this.gpboption);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1208, 72);
			this.panel1.TabIndex = 9;
			// 
			// btnRun
			// 
			this.btnRun.Location = new System.Drawing.Point(1056, 32);
			this.btnRun.Name = "btnRun";
			this.btnRun.TabIndex = 10;
			this.btnRun.Text = "Run";
			this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
			// 
			// btnBrowse2
			// 
			this.btnBrowse2.Location = new System.Drawing.Point(464, 48);
			this.btnBrowse2.Name = "btnBrowse2";
			this.btnBrowse2.Size = new System.Drawing.Size(56, 23);
			this.btnBrowse2.TabIndex = 9;
			this.btnBrowse2.Text = "Browse...";
			this.btnBrowse2.Click += new System.EventHandler(this.btnBrowse2_Click);
			// 
			// btnBrowse1
			// 
			this.btnBrowse1.Location = new System.Drawing.Point(464, 16);
			this.btnBrowse1.Name = "btnBrowse1";
			this.btnBrowse1.Size = new System.Drawing.Size(56, 23);
			this.btnBrowse1.TabIndex = 8;
			this.btnBrowse1.Text = "Browse...";
			this.btnBrowse1.Click += new System.EventHandler(this.btnBrowse1_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 23);
			this.label2.TabIndex = 7;
			this.label2.Text = "New Logfile: ";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 23);
			this.label1.TabIndex = 6;
			this.label1.Text = "Old Logfile: ";
			// 
			// txtbPrevlog
			// 
			this.txtbPrevlog.Location = new System.Drawing.Point(88, 16);
			this.txtbPrevlog.Name = "txtbPrevlog";
			this.txtbPrevlog.Size = new System.Drawing.Size(368, 20);
			this.txtbPrevlog.TabIndex = 2;
			this.txtbPrevlog.Text = "textBox1";
			// 
			// txtbCurrlog
			// 
			this.txtbCurrlog.Location = new System.Drawing.Point(88, 48);
			this.txtbCurrlog.Name = "txtbCurrlog";
			this.txtbCurrlog.Size = new System.Drawing.Size(368, 20);
			this.txtbCurrlog.TabIndex = 4;
			this.txtbCurrlog.Text = "textBox2";
			// 
			// gpboption
			// 
			this.gpboption.Controls.Add(this.rdbDetail10);
			this.gpboption.Controls.Add(this.rdbDetail5);
			this.gpboption.Controls.Add(this.rdbDetail2);
			this.gpboption.Controls.Add(this.rdbDetail1);
			this.gpboption.Controls.Add(this.rdbDetail05);
			this.gpboption.Controls.Add(this.rdbDetail02);
			this.gpboption.Controls.Add(this.rdbDetail01);
			this.gpboption.Controls.Add(this.rdbDetail20);
			this.gpboption.Controls.Add(this.rdbDetail0);
			this.gpboption.Location = new System.Drawing.Point(536, 16);
			this.gpboption.Name = "gpboption";
			this.gpboption.Size = new System.Drawing.Size(496, 56);
			this.gpboption.TabIndex = 5;
			this.gpboption.TabStop = false;
			this.gpboption.Text = "option";
			// 
			// rdbDetail10
			// 
			this.rdbDetail10.Location = new System.Drawing.Point(368, 16);
			this.rdbDetail10.Name = "rdbDetail10";
			this.rdbDetail10.Size = new System.Drawing.Size(40, 24);
			this.rdbDetail10.TabIndex = 8;
			this.rdbDetail10.Text = "10";
			this.rdbDetail10.CheckedChanged += new System.EventHandler(this.rdbDetail10_CheckedChanged);
			// 
			// rdbDetail5
			// 
			this.rdbDetail5.Location = new System.Drawing.Point(328, 16);
			this.rdbDetail5.Name = "rdbDetail5";
			this.rdbDetail5.Size = new System.Drawing.Size(32, 24);
			this.rdbDetail5.TabIndex = 7;
			this.rdbDetail5.Text = "5";
			this.rdbDetail5.CheckedChanged += new System.EventHandler(this.rdbDetail5_CheckedChanged);
			// 
			// rdbDetail2
			// 
			this.rdbDetail2.Location = new System.Drawing.Point(288, 16);
			this.rdbDetail2.Name = "rdbDetail2";
			this.rdbDetail2.Size = new System.Drawing.Size(32, 24);
			this.rdbDetail2.TabIndex = 6;
			this.rdbDetail2.Text = "2";
			this.rdbDetail2.CheckedChanged += new System.EventHandler(this.rdbDetail2_CheckedChanged);
			// 
			// rdbDetail1
			// 
			this.rdbDetail1.Location = new System.Drawing.Point(248, 16);
			this.rdbDetail1.Name = "rdbDetail1";
			this.rdbDetail1.Size = new System.Drawing.Size(32, 24);
			this.rdbDetail1.TabIndex = 5;
			this.rdbDetail1.Text = "1";
			this.rdbDetail1.CheckedChanged += new System.EventHandler(this.rdbDetail1_CheckedChanged);
			// 
			// rdbDetail05
			// 
			this.rdbDetail05.Location = new System.Drawing.Point(200, 16);
			this.rdbDetail05.Name = "rdbDetail05";
			this.rdbDetail05.Size = new System.Drawing.Size(40, 24);
			this.rdbDetail05.TabIndex = 4;
			this.rdbDetail05.Text = "0.5";
			this.rdbDetail05.CheckedChanged += new System.EventHandler(this.rdbDetail05_CheckedChanged);
			// 
			// rdbDetail02
			// 
			this.rdbDetail02.Location = new System.Drawing.Point(152, 16);
			this.rdbDetail02.Name = "rdbDetail02";
			this.rdbDetail02.Size = new System.Drawing.Size(40, 24);
			this.rdbDetail02.TabIndex = 3;
			this.rdbDetail02.Text = "0.2";
			this.rdbDetail02.CheckedChanged += new System.EventHandler(this.rdbDetail02_CheckedChanged);
			// 
			// rdbDetail01
			// 
			this.rdbDetail01.Location = new System.Drawing.Point(104, 16);
			this.rdbDetail01.Name = "rdbDetail01";
			this.rdbDetail01.Size = new System.Drawing.Size(40, 24);
			this.rdbDetail01.TabIndex = 2;
			this.rdbDetail01.Text = "0.1";
			this.rdbDetail01.CheckedChanged += new System.EventHandler(this.rdbDetail01_CheckedChanged);
			// 
			// rdbDetail20
			// 
			this.rdbDetail20.Location = new System.Drawing.Point(408, 16);
			this.rdbDetail20.Name = "rdbDetail20";
			this.rdbDetail20.Size = new System.Drawing.Size(80, 24);
			this.rdbDetail20.TabIndex = 1;
			this.rdbDetail20.Text = "20(coarse)";
			this.rdbDetail20.CheckedChanged += new System.EventHandler(this.rdbDetail20_CheckedChanged);
			// 
			// rdbDetail0
			// 
			this.rdbDetail0.Checked = true;
			this.rdbDetail0.Location = new System.Drawing.Point(8, 16);
			this.rdbDetail0.Name = "rdbDetail0";
			this.rdbDetail0.Size = new System.Drawing.Size(96, 24);
			this.rdbDetail0.TabIndex = 0;
			this.rdbDetail0.TabStop = true;
			this.rdbDetail0.Text = "0 (Everything)";
			this.rdbDetail0.CheckedChanged += new System.EventHandler(this.rdbDetail0_CheckedChanged);
			// 
			// splitter2
			// 
			this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter2.Location = new System.Drawing.Point(0, 597);
			this.splitter2.Name = "splitter2";
			this.splitter2.Size = new System.Drawing.Size(1208, 3);
			this.splitter2.TabIndex = 7;
			this.splitter2.TabStop = false;
			// 
			// dgCallee
			// 
			this.dgCallee.BackgroundColor = System.Drawing.SystemColors.Window;
			this.dgCallee.CaptionBackColor = System.Drawing.SystemColors.ActiveBorder;
			this.dgCallee.DataMember = "";
			this.dgCallee.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.dgCallee.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dgCallee.Location = new System.Drawing.Point(0, 600);
			this.dgCallee.Name = "dgCallee";
			this.dgCallee.Size = new System.Drawing.Size(1208, 260);
			this.dgCallee.TabIndex = 0;
			this.dgCallee.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
																								 this.dataGridTableStyle1});
			this.dgCallee.DoubleClick += new System.EventHandler(this.dgCallee_DoubleClick);
			this.dgCallee.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dgCallee_MouseMove);
			// 
			// dataGridTableStyle1
			// 
			this.dataGridTableStyle1.DataGrid = this.dgCallee;
			this.dataGridTableStyle1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGridTableStyle1.MappingName = "";
			this.dataGridTableStyle1.PreferredColumnWidth = 200;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.tabControl1);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(0, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(1216, 886);
			this.panel2.TabIndex = 2;
			// 
			// ReportForm
			// 
			this.ClientSize = new System.Drawing.Size(1216, 886);
			this.Controls.Add(this.panel2);
			this.Name = "ReportForm";
			this.Text = "Comparing Allocations and Calls";
			this.tabControl1.ResumeLayout(false);
			this.tabpallocdiff.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgToplevelDiff)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dgCaller)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dgSelected)).EndInit();
			this.panel1.ResumeLayout(false);
			this.gpboption.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgCallee)).EndInit();
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		
		#region Diff -- Runas console
		private void AlloccationDiff2Console()
		{
			try
			{
				DataRow[] topdiff = _allocDiff.basedatatable.Select("diffIncl <> 0", "diffIncl DESC");
				if(_allocDiff.diffLogFileName != null)
				{
					if( _allocDiff.diffLogFileName != _allocDiff.PrevLogFileName &&  _allocDiff.diffLogFileName != _allocDiff.CurrLogFileName)
					{
						StringBuilder sb = new StringBuilder();
						sb.AppendFormat("{0}\t {1}\t {2}\t {3}\t {4}\t {4}\t {6}\r\n",  "Function/type Name", "Inclusive", "Diff_Inclusive(KB)", "timesBeenCalled", "Diff_timesBeenCalled", "timesCalls", "Diff_timesCalls");
						for(int i = 0; i< topdiff.Length; i++)
						{
							sb.AppendFormat("{0}\t {1}\t {2}\t {3}\t {4}\t {4}\t {6}\r\n",  topdiff[i][1], topdiff[i][3],topdiff[i][4], topdiff[i][12],topdiff[i][13],topdiff[i][15],topdiff[i][16]);
						}
						
						if(File.Exists(_allocDiff.diffLogFileName))
							File.Delete(_allocDiff.diffLogFileName);
						FileStream fs = new FileStream(_allocDiff.diffLogFileName,
							FileMode.CreateNew, FileAccess.Write, FileShare.None);
						StreamWriter sw = new StreamWriter(fs);
						sw.Write(sb);
						sw.Close();
					}
					else
					{
						if(f.runaswindow)
						{
							MessageBox.Show("log file name are same!","Report profiler Error message");
						}
						else
						{
							Console.WriteLine("Report profiler Error message: \n{0}\n",  "log file name are same!");
						}
					}
				}
				else
				{
					Console.WriteLine("{0}\t {1}\t {2}\t {3}\t {4}\t {4}\t {6}\r\n",  "Function/type Name", "Inclusive", "Diff_Inclusive(KB)", "timesBeenCalled", "Diff_timesBeenCalled", "timesCalls", "Diff_timesCalls");
					for(int i = 0; i< topdiff.Length; i++)
					{
						Console.WriteLine("{0}\t {1}\t {2}\t {3}\t {4}\t {4}\t {6}\r\n",  topdiff[i][1], topdiff[i][3],topdiff[i][4], topdiff[i][12],topdiff[i][13],topdiff[i][15],topdiff[i][16]);
					}
				}
			}
			catch(Exception e)
			{
				if(f.runaswindow)
				{
					MessageBox.Show(e.Message, "Report profiler Error message");
				}
				else
				{
					Console.WriteLine("Report profiler Error message: \n{0}\n",  e.Message);
				}
			}

		}
		#endregion

		#region Diff -- Runas window
		private void AlloccationDiff2Window()
		{
			clearTableStyle();
			createTableStyle();
			strFilter = "prevIncl >= 0"; 
			strtypeFilter = "prevExcl >= 0"; 
			rdbDetail0.Checked = true;
			InitDataGridBinding();
			diffcallTreeForm = new DiffCallTreeForm(_allocDiff.Root, _allocDiff);
			diffcallTreeForm.Visible = true;
			
		}
		#endregion
	
		#region Table Styles	
		// DataGridTableStyle controls the appearance of the columns for each table 
		private void clearTableStyle()
		{
			if(dgToplevelDiff.TableStyles.Contains(this.styleBase))
			{
				styleBase.GridColumnStyles.Clear();
				dgToplevelDiff.TableStyles.Remove(this.styleBase);
			}
			if(dgCallee.TableStyles.Contains(this.styleCallee))
			{
				this.styleCallee.GridColumnStyles.Clear();
				dgCallee.TableStyles.Remove(this.styleCallee);
			}
			
			if(dgCaller.TableStyles.Contains(this.styleCaller))
			{
				this.styleCaller.GridColumnStyles.Clear();
				dgCaller.TableStyles.Remove(this.styleCaller);
			}

			if(dgSelected.TableStyles.Contains(this.styleSelected))
			{
				this.styleSelected.GridColumnStyles.Clear();
				dgSelected.TableStyles.Remove(this.styleSelected);
			}
		
		}

		private void createTableStyle()
		{
			Detail0Style(dgToplevelDiff,_allocDiff.basedatatable, styleBase, 2);
			Detail0Style(dgCaller, _allocDiff.ContriTocallertbl, styleCaller, 7);
			Detail0Style(dgSelected, _allocDiff.basedatatable,styleSelected, 10);
			Detail0Style(dgCallee, _allocDiff.ContriTocalleetbl, styleCallee, 7);
		}
		private void Detail0Style(DataGrid dg, DataTable dt, DataGridTableStyle style, int dghightf)
		{
			if(!dg.TableStyles.Contains(style))
			{
				int with = this.Size.Width;
				dg.Height = this.Height * 1/dghightf;
				
				style.MappingName = dt.TableName;
				style.AlternatingBackColor = System.Drawing.Color.Beige;
				DataGridTextBoxColumn name = new  DataGridTextBoxColumn();
				name.HeaderText = "Function names";
				name.MappingName = "name";
				name.Width = with * 55/100;

				DataGridTextBoxColumn prev = new DataGridTextBoxColumn();
				prev.HeaderText = "Old Incl (KB)";
				prev.MappingName = "prevIncl";
			
				DataGridTextBoxColumn curr = new DataGridTextBoxColumn();
				curr.HeaderText = " New Incl (KB)";
				curr.MappingName = "currIncl";
			
				DataGridTextBoxColumn diff = new DataGridTextBoxColumn();
				diff.HeaderText = "Diff Incl (KB)";
				diff.MappingName = "diffIncl";
	
				DataGridTextBoxColumn prevExcl = new DataGridTextBoxColumn();
				prevExcl.HeaderText = "Old Excl (KB)";
				prevExcl.MappingName = "prevExcl";
			
				DataGridTextBoxColumn currExcl= new DataGridTextBoxColumn();
				currExcl.HeaderText = "New Excl (KB)";
				currExcl.MappingName = "currExcl";
			
				DataGridTextBoxColumn diffExcl = new DataGridTextBoxColumn();
				diffExcl.HeaderText = "Diff Excl (KB)";
				diffExcl.MappingName = "diffExcl";

				DataGridTextBoxColumn prevChildIncl = new DataGridTextBoxColumn();
				prevChildIncl.HeaderText = "Old ChildIncl (KB)";
				prevChildIncl.MappingName = "prevChildIncl";

				DataGridTextBoxColumn currChildIncl = new DataGridTextBoxColumn();
				currChildIncl.HeaderText = "New ChildIncl (KB)";
				currChildIncl.MappingName = "currChildIncl";

				DataGridTextBoxColumn diffChildIncl = new DataGridTextBoxColumn();
				diffChildIncl.HeaderText = "Diff ChildIncl (KB)";
				diffChildIncl.MappingName = "diffChildIncl";

				DataGridTextBoxColumn prevTimesCalled = new DataGridTextBoxColumn();
				prevTimesCalled.HeaderText = "# Old Called";
				prevTimesCalled.MappingName = "prevTimesCalled";

				DataGridTextBoxColumn currTimesCalled = new DataGridTextBoxColumn();
				currTimesCalled.HeaderText = "# New  Called";
				currTimesCalled.MappingName = "currTimesCalled";

				DataGridTextBoxColumn diffTimesCalled = new DataGridTextBoxColumn();
				diffTimesCalled.HeaderText = "# Diff Called";
				diffTimesCalled.MappingName = "diffTimesCalled";

				DataGridTextBoxColumn prevTimesMakecalls = new DataGridTextBoxColumn();
				prevTimesMakecalls.HeaderText = "# Old Calls";
				prevTimesMakecalls.MappingName = "prevTimesMakecalls";

				DataGridTextBoxColumn currTimesMakecalls = new DataGridTextBoxColumn();
				currTimesMakecalls.HeaderText = "# New Calls";
				currTimesMakecalls.MappingName = "currTimesMakecalls";

				DataGridTextBoxColumn diffTimesMakecalls = new DataGridTextBoxColumn();
				diffTimesMakecalls.HeaderText = "# Diff Calls";
				diffTimesMakecalls.MappingName = "diffTimesMakecalls";

				
				if( !rdbDetail20.Checked)
				{

					style.GridColumnStyles.AddRange(new DataGridColumnStyle[]{name,
																				 prev, curr, diff,
																				 prevExcl, currExcl, diffExcl,
																				 prevChildIncl, currChildIncl, diffChildIncl,
																				 prevTimesCalled, currTimesCalled, diffTimesCalled,
																				 prevTimesMakecalls, currTimesMakecalls, diffTimesMakecalls});
				}
				else
				{
					style.GridColumnStyles.AddRange(new DataGridColumnStyle[]{name,diff, diffExcl,diffTimesCalled,diffTimesMakecalls});
				}
				dg.TableStyles.Add(style);
			}
			
		}
		#endregion

		#region Table filter
		private void rdbDetail0_CheckedChanged(object sender, System.EventArgs e)
		{
			if( rdbDetail0.Checked && iscoarse)
			{
				iscoarse = false;
				ResetTableStyle();
			}
			TableLevelfilter();
		}

		private void rdbDetail20_CheckedChanged(object sender, System.EventArgs e)
		{
			if( rdbDetail20.Checked)
			{
				iscoarse = true;
				ResetTableStyle();
			}
			TableLevelfilter();
		}
		
	
		private void rdbDetail01_CheckedChanged(object sender, System.EventArgs e)
		{
			if( rdbDetail01.Checked && iscoarse)
			{
				iscoarse = false;
				ResetTableStyle();
			}
			TableLevelfilter();
		}

		private void rdbDetail02_CheckedChanged(object sender, System.EventArgs e)
		{
			if( rdbDetail02.Checked && iscoarse)
			{
				iscoarse = false;
				ResetTableStyle();
			}
			TableLevelfilter();

		}

		private void rdbDetail05_CheckedChanged(object sender, System.EventArgs e)
		{
			if( rdbDetail05.Checked && iscoarse)
			{
				iscoarse = false;
				ResetTableStyle();
			}
			TableLevelfilter();
		}

		private void rdbDetail1_CheckedChanged(object sender, System.EventArgs e)
		{
			if( rdbDetail1.Checked && iscoarse)
			{
				iscoarse = false;
				ResetTableStyle();
			}
			TableLevelfilter();
		}

		private void rdbDetail2_CheckedChanged(object sender, System.EventArgs e)
		{
			if( rdbDetail2.Checked && iscoarse)
			{
				iscoarse = false;
				ResetTableStyle();
			}
			TableLevelfilter();
		}

		private void rdbDetail5_CheckedChanged(object sender, System.EventArgs e)
		{
			if( rdbDetail5.Checked && iscoarse)
			{
				iscoarse = false;
				ResetTableStyle();
			}
			TableLevelfilter();
		}

		private void rdbDetail10_CheckedChanged(object sender, System.EventArgs e)
		{
			if( rdbDetail10.Checked && iscoarse)
			{
				iscoarse = false;
				ResetTableStyle();
			}
			TableLevelfilter();
		}
		private void ResetTableStyle()
		{
			clearTableStyle();
			createTableStyle();
			dgToplevelDiff.Visible = true;
			dgCaller.Visible = true;
			dgCallee.Visible = true;
			dgSelected.Visible = true;
		}

		private void TableLevelfilter()
		{
			if( rdbDetail0.Checked)
			{
				strFilter = "prevIncl >= 0"; 
				strtypeFilter = "prevExcl >= 0"; 
			}
			else if(rdbDetail01.Checked)
			{
				strFilter = "(prevIncl > " + _allocDiff.prevFilter.detail01 + ") or (currIncl > " + _allocDiff.currFilter.detail01 + ")"; 
				strtypeFilter = "(prevExcl > " + _allocDiff.prevTypedeFilter.detail01 + ") or (currExcl > " + _allocDiff.currTypedeFilter.detail01 + ")"; 
			}
			else if(rdbDetail02.Checked)
			{
				strFilter = "(prevIncl > " + _allocDiff.prevFilter.detail02 + ") or (currIncl > " + _allocDiff.currFilter.detail02 + ")";
				strtypeFilter = "(prevExcl > " + _allocDiff.prevTypedeFilter.detail02 + ") or (currExcl > " + _allocDiff.currTypedeFilter.detail02 + ")";
			}
			else if(rdbDetail05.Checked)
			{
				strFilter = "(prevIncl > " + _allocDiff.prevFilter.detail05 + ") or (currIncl > " + _allocDiff.currFilter.detail05 + ")";
				strtypeFilter = "(prevExcl > " + _allocDiff.prevTypedeFilter.detail05 + ") or (currExcl > " + _allocDiff.currTypedeFilter.detail05 + ")";
			}
			else if(rdbDetail1.Checked)
			{
				strFilter = "(prevIncl > " + _allocDiff.prevFilter.detail1 + ") or (currIncl > " + _allocDiff.currFilter.detail1 + ")";
				strtypeFilter = "(prevExcl > " + _allocDiff.prevTypedeFilter.detail1 + ") or (currExcl > " + _allocDiff.currTypedeFilter.detail1 + ")";
			}
			else if(rdbDetail2.Checked)
			{
				strFilter = "(prevIncl > " + _allocDiff.prevFilter.detail2 + ") or (currIncl > " + _allocDiff.currFilter.detail2 + ")";
				strtypeFilter = "(prevExcl > " + _allocDiff.prevTypedeFilter.detail2 + ") or (currExcl > " + _allocDiff.currTypedeFilter.detail2 + ")";
			}
			else if(rdbDetail5.Checked)
			{
				strFilter = "(prevIncl > " + _allocDiff.prevFilter.detail5 + ") or (currIncl > " + _allocDiff.currFilter.detail5 + ")";
				strtypeFilter = "(prevExcl > " + _allocDiff.prevTypedeFilter.detail5 + ") or (currExcl > " + _allocDiff.currTypedeFilter.detail5 + ")";
			}
			else if(rdbDetail10.Checked)
			{
				strFilter = "(prevIncl > " + _allocDiff.prevFilter.detail10 + ") or (currIncl > " + _allocDiff.currFilter.detail10 + ")";
				strtypeFilter = "(prevExcl > " + _allocDiff.prevTypedeFilter.detail10 + ") or (currExcl > " + _allocDiff.currTypedeFilter.detail10 + ")";
			}
			InitDataGridBinding();			
		}
		#endregion

		#region View -- DataGrids
		private void dgToplevelDiff_DoubleClick(object sender, System.EventArgs e)
		{
			int roeNumber = dgToplevelDiff.CurrentCell.RowNumber;
			string name = (string)dgToplevelDiff[roeNumber, 0];
			int prevIncl = (int)dgToplevelDiff[roeNumber, 1];
			int currIncl = (int)dgToplevelDiff[roeNumber, 2];
			int diffIncl = (int)dgToplevelDiff[roeNumber, 3];
			
			
			ViewSelectedInfo(dgToplevelDiff);
			ViewCallTrace(name, prevIncl, currIncl, diffIncl);
		}
		private void dgCaller_DoubleClick(object sender, System.EventArgs e)
		{
			ViewSelectedInfo(dgCaller);
		}

		private void dgCallee_DoubleClick(object sender, System.EventArgs e)
		{
			int roeNumber = dgCallee.CurrentCell.RowNumber;
			string name = (string)dgCallee[roeNumber, 0];
			int prevIncl = (int)dgCallee[roeNumber, 1];
			int currIncl = (int)dgCallee[roeNumber, 2];
			int diffIncl = (int)dgCallee[roeNumber, 3];
			
			ViewSelectedInfo(dgCallee);
			ViewCallTrace(name, prevIncl, currIncl, diffIncl);
		}

		private void ViewSelectedInfo(DataGrid dg)
		{
			if( dg.DataMember == "")
				return;
			dvm = new DataViewManager(_allocDiff.ds);
			dvm_caller = new DataViewManager(_allocDiff.ds);
			dvm_callee = new DataViewManager(_allocDiff.ds);
			
			int roeNumber = dg.CurrentCell.RowNumber;
            		
			string name = (string)dg[roeNumber, 0];
			int id = (int)_allocDiff.basedataId[name];
			DataRow[] caller = _allocDiff.callertbl.Select("id =" + id);
			DataRow[] callee = _allocDiff.calleetbl.Select("id =" + id);
			
			//======== caller table============
			if(name != "<root>")
			{
				dgCaller.CaptionText = CallerCaption;
				dvm_caller.DataViewSettings[_allocDiff.ContriTocallertbl.TableName].RowFilter = "id = " + id;
				dgCaller.SetDataBinding(dvm_caller ,_allocDiff.ContriTocallertbl.TableName);
			}
			else
			{
				dgCaller.SetDataBinding(null, null);
			}

			dgCaller.Visible = true;

			//========= selected table ==========
			dgSelected.CaptionText = SelectedCaption;
			dvm.DataViewSettings[_allocDiff.basedatatable.TableName].RowFilter = "id" + '=' + id;
			dgSelected.SetDataBinding(dvm,_allocDiff.basedatatable.TableName);
			dgSelected.Visible = true;

			//========= callee Table ===============
			if(name != "<bottom>")
			{
				dgCallee.CaptionText = CalleeCaption;
				dvm_callee.DataViewSettings[_allocDiff.ContriTocalleetbl.TableName].RowFilter = "id = " + id;
				dgCallee.SetDataBinding(dvm_callee ,_allocDiff.ContriTocalleetbl.TableName);
			}
			else
			{
				dgCallee.SetDataBinding(null, null);
			}
			dgCallee.Visible = true;
			this.Invalidate();
				
				
				
			
		}
		#endregion

		#region View -- ViewCallTrace
		private void ViewCallTrace(string name, int prevIncl, int currIncl, int diffIncl)
		{
			if(name == "<root>")
			{
				diffcallTreeForm = new DiffCallTreeForm(_allocDiff.Root, _allocDiff);
				return;
			}
						
			DiffDataNode root = new DiffDataNode(name);
			DiffDataNode tmproot = new DiffDataNode(name);
			tmproot.mapname = name;
			root.mapname = name;
			root.prevIncl = prevIncl;
			root.currIncl = currIncl;
			root.diffIncl = diffIncl;

			root.currFunId = -1;
			root.prevFunId = -1;
			string diffkey = null;
			string filter = null;
			DataRow[] rRoot = null;
			
			if(_allocDiff._currcallTrace.LogResult.allocatedHistogram.readNewLog.funcSignatureIdHash.ContainsKey(name))
			{
				root.currFunId = _allocDiff._currcallTrace.LogResult.allocatedHistogram.readNewLog.funcSignatureIdHash[name];
			}
			if(_allocDiff._prevcallTrace.LogResult.allocatedHistogram.readNewLog.funcSignatureIdHash.ContainsKey(name))
			{
				root.prevFunId = _allocDiff._prevcallTrace.LogResult.allocatedHistogram.readNewLog.funcSignatureIdHash[name];
			}
			if(!(root.prevFunId == -1 && root.currFunId == -1))
			{
				filter = "(prevIncl = " + prevIncl + ") and (currIncl = " + currIncl + ") and (prevFunId = " + root.prevFunId + ") and (currFunId = " + root.currFunId + ")";
				rRoot = _allocDiff.summaryTracetbl.Select(filter, "depth asc");
				
				if(rRoot.Length ==0 )
				{
					filter = "(prevFunId = " + root.prevFunId + ") or (currFunId = " + root.currFunId + ")";
					rRoot = _allocDiff.summaryTracetbl.Select(filter, "depth asc");
					
					if(rRoot.Length == 0)
					{
						filter = "(currFunId = " + root.currFunId + ") and (prevFunId = -1)";
						rRoot = _allocDiff.summaryTracetbl.Select(filter, "depth asc");
						
						if(rRoot.Length == 0)
						{
							filter = "(prevFunId = " + root.prevFunId + ") and (currFunId = -1)";
							rRoot = _allocDiff.summaryTracetbl.Select(filter, "depth asc");
							if(rRoot.Length == 0)
							{
								filter = null;
							}
						}
					}
				}
			}
		
			if(filter != null)
			{
				if(rRoot.Length == 1)
				{
					tmproot = _allocDiff.Row2Node(rRoot[0]);
					diffkey = tmproot.mapname + tmproot.prevIncl + tmproot.currIncl + tmproot.diffIncl + tmproot.prevFunId + tmproot.currFunId;
					root = _allocDiff.diffCallTreeNodes[diffkey] as DiffDataNode;
				}
				else if(rRoot.Length > 1 )
				{
					long sum = 0;
					string depth = null;
					long diffincl = 0;
					Hashtable depLst = new Hashtable();
					Hashtable depSum = new Hashtable();
					for(int i = 0; i < rRoot.Length; i++)
					{
						depth = rRoot[i][idx_depth].ToString();
						diffincl= long.Parse(rRoot[i][idx_diffIncl].ToString());
						sum += diffincl;
						if(depLst.Contains(depth))
						{
							ArrayList alst = (ArrayList)depLst[depth];
							alst.Add(rRoot[i]);
							depLst[depth] = alst;
							diffincl += (long)depSum[depth];
						}
						else
						{
							ArrayList alst = new ArrayList();
							alst.Add(rRoot[i]);							
							depLst.Add(depth, alst);
							depSum.Add(depth, diffincl);
						}
						if(diffincl == root.diffIncl)
							break;
					}
					
					if(sum != root.diffIncl && diffincl == root.diffIncl )
					{
						if(depLst.ContainsKey(depth))
						{
							ArrayList lst = (ArrayList)depLst[depth];
							for(int i = 0; i < lst.Count; i++)
							{
								DataRow r = (DataRow)lst[i];
								tmproot = _allocDiff.Row2Node(r);
								diffkey = tmproot.mapname + tmproot.prevIncl + tmproot.currIncl + tmproot.diffIncl + tmproot.prevFunId + tmproot.currFunId;
								DiffDataNode subRoot =  _allocDiff.diffCallTreeNodes[diffkey] as DiffDataNode;
								root.allkids.Add(subRoot);
							}
						}
					}
					else
					{

						for(int i = 0; i < rRoot.Length; i++)
						{
							tmproot = _allocDiff.Row2Node(rRoot[i]);
							if(tmproot.depth > 0)
							{
								diffkey = tmproot.mapname + tmproot.prevIncl + tmproot.currIncl + tmproot.diffIncl + tmproot.prevFunId + tmproot.currFunId;
								DiffDataNode subRoot =  _allocDiff.diffCallTreeNodes[diffkey] as DiffDataNode;
								root.allkids.Add(subRoot);
							}
						}
					}
					root.HasKids = true;
					root.depth = 0;

				}
				diffcallTreeForm.Close();
				diffcallTreeForm = new DiffCallTreeForm(root, _allocDiff);
			}
			
		}
		
	
		#endregion

		#region Aligement -- DataGrid columns 
		// DataGrid alignment been controlled by datagrid column width
		private void InitDataGridBinding()
		{
			dvm = new DataViewManager(_allocDiff.ds);
			dvm.DataViewSettings[_allocDiff.basedatatable.TableName].RowFilter = strFilter;
			dvm.DataViewSettings[_allocDiff.basedatatable.TableName].Sort = "diffIncl desc";
			
			dgToplevelDiff.CaptionText = "Function allocation diff";
			dgToplevelDiff.SetDataBinding(dvm, _allocDiff.basedatatable.TableName);
			dgCallee.SetDataBinding(null, null);
			dgCaller.SetDataBinding(null, null);
			dgSelected.SetDataBinding(null, null);
		}
		
		private void dgToplevelDiff_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			aligDataGrids(sender, e, styleBase);
		}

		private void dgCaller_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			aligDataGrids(sender, e, styleCaller);
		}

		private void dgSelected_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			aligDataGrids(sender, e, styleSelected);
		}

		private void dgCallee_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			aligDataGrids(sender, e, styleCallee);
		}

		private void aligDataGrids(object sender, System.Windows.Forms.MouseEventArgs e, DataGridTableStyle sty)
		{
			System.Windows.Forms.DataGrid.HitTestInfo hi;   
			hi=((DataGrid) sender).HitTest(e.X, e.Y);
			if(!iscoarse)
			{
				columnIdx idx = (columnIdx)hi.Column-1;
				if(idx >= columnIdx.name && idx <= columnIdx.diffTimesMakecalls)
				{
					aligEverythingDataGrid(sty, idx);
				}
			}
			else
			{
				coarsecolumnIdx idx = (coarsecolumnIdx)hi.Column-1;
				if(idx >= coarsecolumnIdx.name && idx <= coarsecolumnIdx.diffTimesMakecalls)
				{
					aligcoarseDataGrid(sty, idx);
				}
				
			}
		}
	

		private void aligEverythingDataGrid(DataGridTableStyle sty, columnIdx idx)
		{
			string colName = columName[(int)idx];
			styleBase.GridColumnStyles[colName].Width = sty.GridColumnStyles[colName].Width;
			styleCaller.GridColumnStyles[colName].Width = sty.GridColumnStyles[colName].Width;
			styleCallee.GridColumnStyles[colName].Width = sty.GridColumnStyles[colName].Width;
			styleSelected.GridColumnStyles[colName].Width = sty.GridColumnStyles[colName].Width;
		}

		private void aligcoarseDataGrid(DataGridTableStyle sty, coarsecolumnIdx idx)
		{
			string colName = coarsecolumName[(int)idx];
			styleBase.GridColumnStyles[colName].Width = sty.GridColumnStyles[colName].Width;
			styleCaller.GridColumnStyles[colName].Width = sty.GridColumnStyles[colName].Width;
			styleCallee.GridColumnStyles[colName].Width = sty.GridColumnStyles[colName].Width;
			styleSelected.GridColumnStyles[colName].Width = sty.GridColumnStyles[colName].Width;
		}
		#endregion

		#region Event -- Button

		private void btnBrowse1_Click(object sender, System.EventArgs e)
		{
			openFileDialog1.FileName = "*.log";
			openFileDialog1.Filter = "Allocation Logs | *.log";
			if (   openFileDialog1.ShowDialog() == DialogResult.OK && openFileDialog1.CheckFileExists)
			{
				f.prevlogFileName = openFileDialog1.FileName;
				this.txtbPrevlog.Text = f.prevlogFileName;
			}
		}

		private void btnBrowse2_Click(object sender, System.EventArgs e)
		{
			openFileDialog1.FileName = "*.log";
			openFileDialog1.Filter = "Allocation Logs | *.log";
			if (   openFileDialog1.ShowDialog() == DialogResult.OK && openFileDialog1.CheckFileExists)
			{
				f.currlogFileName = openFileDialog1.FileName;
				this.txtbCurrlog.Text = f.currlogFileName;
			}
		
		}

		private void btnRun_Click(object sender, System.EventArgs e)
		{
			string prevfile = this.txtbPrevlog.Text;
			string currfile = this.txtbCurrlog.Text;
			if(File.Exists(prevfile) && File.Exists(currfile))
			{
				f.currlogFileName = currfile;
				f.prevlogFileName = prevfile;
				buildReports();			
			}
		}

		#endregion


		
		
	}
	
}
