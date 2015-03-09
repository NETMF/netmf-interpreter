////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace CLRProfiler
{
	/// <summary>
	/// Summary description for FunctionFilter.
	/// </summary>
	internal class DlgFunctionFilter : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button btnIncludeFn0;
		private System.Windows.Forms.Button btnIncludeFn1;
		private System.Windows.Forms.Button btnExcludeFn0;
		private System.Windows.Forms.Button btnExcludeFn1;
		private System.Windows.Forms.Button btnClear;

		private ITreeOwner m_treeOwner;

		private CallTreeForm.FnViewFilter[] includeFns;
		private System.Windows.Forms.TextBox tbIncludeFn0;
		private System.Windows.Forms.TextBox tbIncludeFn1;
		private System.Windows.Forms.TextBox tbExcludeFn0;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox tbExcludeFn1;
		private CallTreeForm.FnViewFilter[] excludeFns;

		private TreeNode node;

		internal DlgFunctionFilter( ITreeOwner TreeOwner )
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			m_treeOwner = TreeOwner;
			node = new TreeNode(TreeNode.NodeType.Call, 0 );

			includeFns = m_treeOwner.GetIncludeFilters();
			tbIncludeFn0.Text = GetName( includeFns[0].nodetype, includeFns[0].functionId );
			tbIncludeFn0.Enabled = true;
			tbIncludeFn1.Text = GetName( includeFns[1].nodetype, includeFns[1].functionId );
			tbIncludeFn1.Enabled = true;

			excludeFns = m_treeOwner.GetExcludeFilters();
			tbExcludeFn0.Text = GetName( excludeFns[0].nodetype, excludeFns[0].functionId );
			tbExcludeFn0.Enabled = true;
			tbExcludeFn1.Text = GetName( excludeFns[1].nodetype, excludeFns[1].functionId );
			tbExcludeFn1.Enabled = true;
		}

		private string GetName( TreeNode.NodeType nodetype, int id )
		{
			if (id == -1)
			{
				return "";
			}
			else
			{
				if (nodetype == TreeNode.NodeType.Call)
					return m_treeOwner.MakeNameForFunction( id );
				else
					return m_treeOwner.MakeNameForAllocation( id, 0 );					
			}
		}

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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnIncludeFn0 = new System.Windows.Forms.Button();
			this.btnIncludeFn1 = new System.Windows.Forms.Button();
			this.btnExcludeFn1 = new System.Windows.Forms.Button();
			this.btnExcludeFn0 = new System.Windows.Forms.Button();
			this.btnClear = new System.Windows.Forms.Button();
			this.tbIncludeFn0 = new System.Windows.Forms.TextBox();
			this.tbIncludeFn1 = new System.Windows.Forms.TextBox();
			this.tbExcludeFn1 = new System.Windows.Forms.TextBox();
			this.tbExcludeFn0 = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 32);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(152, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Must Include Functions";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(24, 136);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(168, 16);
			this.label2.TabIndex = 3;
			this.label2.Text = "Exclude Functions (prune)";
			// 
			// btnOk
			// 
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Location = new System.Drawing.Point(104, 304);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(72, 32);
			this.btnOk.TabIndex = 6;
			this.btnOk.Text = "OK";
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(224, 304);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(72, 32);
			this.btnCancel.TabIndex = 7;
			this.btnCancel.Text = "Cancel";
			// 
			// btnIncludeFn0
			// 
			this.btnIncludeFn0.Location = new System.Drawing.Point(320, 56);
			this.btnIncludeFn0.Name = "btnIncludeFn0";
			this.btnIncludeFn0.Size = new System.Drawing.Size(48, 24);
			this.btnIncludeFn0.TabIndex = 9;
			this.btnIncludeFn0.Text = "Find...";
			this.btnIncludeFn0.Click += new System.EventHandler(this.btnIncludeFn0_Click);
			// 
			// btnIncludeFn1
			// 
			this.btnIncludeFn1.Location = new System.Drawing.Point(320, 96);
			this.btnIncludeFn1.Name = "btnIncludeFn1";
			this.btnIncludeFn1.Size = new System.Drawing.Size(48, 24);
			this.btnIncludeFn1.TabIndex = 11;
			this.btnIncludeFn1.Text = "Find...";
			this.btnIncludeFn1.Click += new System.EventHandler(this.btnIncludeFn1_Click);
			// 
			// btnExcludeFn1
			// 
			this.btnExcludeFn1.Location = new System.Drawing.Point(320, 200);
			this.btnExcludeFn1.Name = "btnExcludeFn1";
			this.btnExcludeFn1.Size = new System.Drawing.Size(48, 24);
			this.btnExcludeFn1.TabIndex = 15;
			this.btnExcludeFn1.Text = "Find...";
			this.btnExcludeFn1.Click += new System.EventHandler(this.btnExcludeFn1_Click);
			// 
			// btnExcludeFn0
			// 
			this.btnExcludeFn0.Location = new System.Drawing.Point(320, 160);
			this.btnExcludeFn0.Name = "btnExcludeFn0";
			this.btnExcludeFn0.Size = new System.Drawing.Size(48, 24);
			this.btnExcludeFn0.TabIndex = 13;
			this.btnExcludeFn0.Text = "Find...";
			this.btnExcludeFn0.Click += new System.EventHandler(this.btnExcludeFn0_Click);
			// 
			// btnClear
			// 
			this.btnClear.Location = new System.Drawing.Point(136, 240);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(56, 24);
			this.btnClear.TabIndex = 16;
			this.btnClear.Text = "Clear";
			this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
			// 
			// tbIncludeFn0
			// 
			this.tbIncludeFn0.Location = new System.Drawing.Point(24, 56);
			this.tbIncludeFn0.Name = "tbIncludeFn0";
			this.tbIncludeFn0.Size = new System.Drawing.Size(280, 20);
			this.tbIncludeFn0.TabIndex = 18;
			this.tbIncludeFn0.Text = "";
			// 
			// tbIncludeFn1
			// 
			this.tbIncludeFn1.Location = new System.Drawing.Point(24, 96);
			this.tbIncludeFn1.Name = "tbIncludeFn1";
			this.tbIncludeFn1.Size = new System.Drawing.Size(280, 20);
			this.tbIncludeFn1.TabIndex = 19;
			this.tbIncludeFn1.Text = "";
			// 
			// tbExcludeFn1
			// 
			this.tbExcludeFn1.Location = new System.Drawing.Point(24, 200);
			this.tbExcludeFn1.Name = "tbExcludeFn1";
			this.tbExcludeFn1.Size = new System.Drawing.Size(280, 20);
			this.tbExcludeFn1.TabIndex = 21;
			this.tbExcludeFn1.Text = "";
			// 
			// tbExcludeFn0
			// 
			this.tbExcludeFn0.Location = new System.Drawing.Point(24, 160);
			this.tbExcludeFn0.Name = "tbExcludeFn0";
			this.tbExcludeFn0.Size = new System.Drawing.Size(280, 20);
			this.tbExcludeFn0.TabIndex = 20;
			this.tbExcludeFn0.Text = "";
			// 
			// groupBox1
			// 
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(376, 272);
			this.groupBox1.TabIndex = 22;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Filters";
			// 
			// DlgFunctionFilter
			// 
			this.AcceptButton = this.btnOk;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(400, 358);
			this.Controls.Add(this.tbExcludeFn1);
			this.Controls.Add(this.tbExcludeFn0);
			this.Controls.Add(this.tbIncludeFn1);
			this.Controls.Add(this.tbIncludeFn0);
			this.Controls.Add(this.btnClear);
			this.Controls.Add(this.btnExcludeFn1);
			this.Controls.Add(this.btnExcludeFn0);
			this.Controls.Add(this.btnIncludeFn1);
			this.Controls.Add(this.btnIncludeFn0);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.groupBox1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DlgFunctionFilter";
			this.Text = "FunctionFilter";
			this.ResumeLayout(false);

		}
		#endregion

		private void btnIncludeFn0_Click(object sender, System.EventArgs e)
		{
			CallTreeForm.FnViewFilter viewFilter = FindFunction( tbIncludeFn0 );
			if (viewFilter.functionId > 0)
			{
				includeFns[0] = viewFilter;
			}

		}

		private void btnIncludeFn1_Click(object sender, System.EventArgs e)
		{
			CallTreeForm.FnViewFilter viewFilter = FindFunction( tbIncludeFn1 );
			if (viewFilter.functionId > 0)
			{
				includeFns[1] = viewFilter;
			}
		
		}

		private void btnExcludeFn0_Click(object sender, System.EventArgs e)
		{
			CallTreeForm.FnViewFilter viewFilter = FindFunction( tbExcludeFn0 );
			if (viewFilter.functionId > 0)
			{
				excludeFns[0] = viewFilter;
			}
		
		}

		private void btnExcludeFn1_Click(object sender, System.EventArgs e)
		{
			CallTreeForm.FnViewFilter viewFilter = FindFunction( tbExcludeFn1 );
			if (viewFilter.functionId > 0)
			{
				excludeFns[1] = viewFilter;
			}
		}

		//
		//  Popup a dialog to let the user select a function name from 
		//  the list of all functions in the current view.
		//
		//  Returns:
		//     -1:  Dialog cancelled
		//	   >=0: Fn id.
		//

		private CallTreeForm.FnViewFilter FindFunction( TextBox tb )
		{
			int id = -2;
			TreeNode.NodeType nodetype = TreeNode.NodeType.Call;
			FunctionFind functionFind = new FunctionFind( m_treeOwner, tb.Text );
			CallTreeForm.FnViewFilter viewFilter = new CallTreeForm.FnViewFilter();

			if (functionFind.ShowDialog() == DialogResult.OK)
			{
				id = functionFind.SelectedFunctionId;
				if (id >= 0)
				{
					nodetype = functionFind.SelectedNodeType;
					tb.Text = GetName( nodetype, id );
				}
			}

			viewFilter.functionId = id;
			viewFilter.nodetype = nodetype;

			return viewFilter;
		}

		private void btnClear_Click(object sender, System.EventArgs e)
		{
			includeFns[0].functionId = -1;
			includeFns[1].functionId = -1;
			excludeFns[0].functionId = -1;
			excludeFns[1].functionId = -1;

			tbIncludeFn0.Text = GetName( includeFns[0].nodetype, includeFns[0].functionId );
			tbIncludeFn1.Text = GetName( includeFns[1].nodetype, includeFns[1].functionId );
			tbExcludeFn0.Text = GetName( excludeFns[0].nodetype, excludeFns[0].functionId );
			tbExcludeFn1.Text = GetName( excludeFns[1].nodetype, excludeFns[1].functionId );
		}
		


	}
}
