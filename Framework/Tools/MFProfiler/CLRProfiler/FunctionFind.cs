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
	/// Summary description for FunctionFind.
	/// </summary>
	internal class FunctionFind : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tbSearchString;
		private System.Windows.Forms.ListBox lbFunctions;
		private System.Windows.Forms.Button btnSearch;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label label2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private ITreeOwner TreeOwner;
		internal int SelectedFunctionId;
		internal TreeNode.NodeType SelectedNodeType;

		class LineItem 
		{
			internal TreeNode.NodeType nodeType;
			internal int id;
			internal string Name;

			public override string ToString()
			{
				return Name;
			}
		};

		internal FunctionFind( ITreeOwner treeOwner, string SearchString )
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			TreeOwner = treeOwner;
			tbSearchString.Text = SearchString;
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
			this.tbSearchString = new System.Windows.Forms.TextBox();
			this.lbFunctions = new System.Windows.Forms.ListBox();
			this.btnSearch = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(248, 32);
			this.label1.TabIndex = 0;
			this.label1.Text = "Enter a search string, then click the \'Search\' button.";
			// 
			// tbSearchString
			// 
			this.tbSearchString.Location = new System.Drawing.Point(24, 56);
			this.tbSearchString.Name = "tbSearchString";
			this.tbSearchString.Size = new System.Drawing.Size(240, 20);
			this.tbSearchString.TabIndex = 1;
			this.tbSearchString.Text = "";
			// 
			// lbFunctions
			// 
			this.lbFunctions.Location = new System.Drawing.Point(24, 136);
			this.lbFunctions.Name = "lbFunctions";
			this.lbFunctions.Size = new System.Drawing.Size(240, 82);
			this.lbFunctions.TabIndex = 2;
			this.lbFunctions.SelectedIndexChanged += new System.EventHandler(this.lbFunctions_SelectedIndexChanged);
			// 
			// btnSearch
			// 
			this.btnSearch.Location = new System.Drawing.Point(72, 96);
			this.btnSearch.Name = "btnSearch";
			this.btnSearch.Size = new System.Drawing.Size(136, 24);
			this.btnSearch.TabIndex = 3;
			this.btnSearch.Text = "Search";
			this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(40, 288);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(56, 24);
			this.btnOk.TabIndex = 4;
			this.btnOk.Text = "OK";
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(160, 288);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(56, 24);
			this.btnCancel.TabIndex = 5;
			this.btnCancel.Text = "Cancel";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(32, 240);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(232, 24);
			this.label2.TabIndex = 6;
			this.label2.Text = "Select your function from the list above.";
			// 
			// FunctionFind
			// 
			this.AcceptButton = this.btnSearch;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(292, 326);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnSearch);
			this.Controls.Add(this.lbFunctions);
			this.Controls.Add(this.tbSearchString);
			this.Controls.Add(this.label1);
			this.Name = "FunctionFind";
			this.Text = "FunctionFind";
			this.ResumeLayout(false);

		}
		#endregion


		private void btnSearch_Click(object sender, System.EventArgs e)
		{
			int i;
			LineItem lineItem;
			string fn;
			string matchString = tbSearchString.Text;

			lbFunctions.Items.Clear();

			//  Add functions
			for( i = 0; ; i++)
			{
				fn = TreeOwner.MakeNameForFunction(i);
				if (fn == null)
				{
					break;
				}

				if ( fn.IndexOf( matchString ) != -1 )
				{
					lineItem = new LineItem();
					lineItem.id = i;
					lineItem.Name = fn;
					lineItem.nodeType = TreeNode.NodeType.Call;

					lbFunctions.Items.Add( lineItem );
				}
			}

			//  Add allocations
			for( i = 0; ; i++)
			{
				fn = TreeOwner.MakeNameForAllocation(i, 0);
				if (fn == null)
				{
					break;
				}

				if ( fn.IndexOf( matchString ) != -1 )
				{
					lineItem = new LineItem();
					lineItem.id = i;
					lineItem.Name = fn;
					lineItem.nodeType = TreeNode.NodeType.Allocation;

					lbFunctions.Items.Add( lineItem );
				}
			}
		}

		private void btnOk_Click(object sender, System.EventArgs e)
		{
			if (lbFunctions.SelectedIndex < 0)
			{
				// Nothing selected
				if (lbFunctions.Items.Count == 0)
				{
					MessageBox.Show( "To populate the list box, enter a search string and click the search button." );
				}
				else
				{
					MessageBox.Show( "Please select a function from the list box." );
				}
				return;
			}

			SelectedFunctionId = ((LineItem)lbFunctions.SelectedItem).id;
			SelectedNodeType = ((LineItem)lbFunctions.SelectedItem).nodeType;
			DialogResult = DialogResult.OK;
			Close();
		}

		private void lbFunctions_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// User selected function.  Make the OK button the default button.
			this.AcceptButton = this.btnOk;
		}
	}
}
