////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace CLRProfiler
{
    /// <summary>
    /// Summary description for ListViewer.
    /// </summary>
    public class ListViewer : System.Windows.Forms.Form, IComparer
    {
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem2;
        public System.Windows.Forms.ListView list;
        private IContainer components;

        private int sortColumn, sorting;

        public ListViewer()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            sortColumn = 0;
            sorting = 0;

            list.ListViewItemSorter = this;

            //
            // need to add any constructor code after InitializeComponent call
            //
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
            this.components = new System.ComponentModel.Container();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.list = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem2});
            this.menuItem1.Text = "&File";
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 0;
            this.menuItem2.Text = "&Save as text...";
            this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
            // 
            // list
            // 
            this.list.AllowColumnReorder = true;
            this.list.Dock = System.Windows.Forms.DockStyle.Fill;
            this.list.FullRowSelect = true;
            this.list.GridLines = true;
            this.list.Location = new System.Drawing.Point(0, 0);
            this.list.Name = "list";
            this.list.Size = new System.Drawing.Size(292, 273);
            this.list.TabIndex = 0;
            this.list.UseCompatibleStateImageBehavior = false;
            this.list.View = System.Windows.Forms.View.Details;
            this.list.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.list_ColumnClick);
            // 
            // ListViewer
            // 
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.list);
            this.Menu = this.mainMenu1;
            this.Name = "ListViewer";
            this.Text = "ListViewer";
            this.ResumeLayout(false);

        }
        #endregion

        private void list_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
        {
            if(e.Column == sortColumn)
            {
                sorting *= -1;
            }
            else
            {
                sortColumn = e.Column;
                sorting = (sortColumn == 0 ? 1 : -1);
            }
            list.Sort();
        }
        #region IComparer Members

        public int Compare(object x, object y)
        {
            int res = 0;
            ListViewItem a = x as ListViewItem, b = y as ListViewItem;
            string aa = a.SubItems[sortColumn].Text, bb = b.SubItems[sortColumn].Text;
            if(sortColumn != 0)
            {
                long a1 = Int64.Parse(aa);
                long b1 = Int64.Parse(bb);
                res = (a1 > b1 ? 1 : (a1 == b1 ? 0 : -1));
            }
            else
            {
                res = aa.CompareTo(bb);
            }
            return sorting * res;
        }

        #endregion

        private void menuItem2_Click(object sender, System.EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "Formatted text|*.txt|Comma-Separated Values|*.csv";
            DialogResult r = sf.ShowDialog();
            if(r == DialogResult.OK)
            {
                try
                {
                    if(File.Exists(sf.FileName))
                    {
                        File.Delete(sf.FileName);
                    }
                }
                catch
                {
                    MessageBox.Show(this, "Cannot delete existing file " + sf.FileName, "Failure");
                    return;
                }

                bool formatNicely = !sf.FileName.ToLower().EndsWith(".csv");

                int i, j, columns = list.Columns.Count;
                try
                {
                    StreamWriter s = new StreamWriter(sf.FileName);
                    string[] formats = new string[columns];
                    if(formatNicely)
                    {
                        // figure out widths of the columns
                        int[] widths = new int[columns];
                        for(i = 0; i < columns; i++)
                        {
                            widths[i] = list.Columns[i].Text.Length;
                        }
                        for(i = 0; i < list.Items.Count; i++)
                        {
                            ListViewItem m = list.Items[i];
                            for(j = 0; j < columns; j++)
                            {
                                int l = m.SubItems[j].Text.Length;
                                if(l > widths[j])
                                {
                                    widths[j] = l;
                                }
                            }
                        }

                        // create formats
                        for(i = 0; i < columns; i++)
                        {
                            formats[i] = "{0," + (i == 0 ? "-" : "") + widths[i] + '}' + (i == columns - 1 ? '\n' : ' ');
                        }
                    }
                    else
                    {
                        for(i = 0; i < columns - 1; i++)
                        {
                            formats[i] = "{0},";
                        }
                        formats[columns - 1] = "{0}\n";
                    }

                    for(i = 0; i < columns; i++)
                    {
                        s.Write(formats[i], list.Columns[i].Text);
                    }
                    if(formatNicely)
                    {
                        s.Write('\n');
                    }
                    for(i = 0; i < list.Items.Count; i++)
                    {
                        ListViewItem m = list.Items[i];
                        for(j = 0; j < columns; j++)
                        {
                            s.Write(formats[j], m.SubItems[j].Text);
                        }
                    }
                    s.Close();
                }
                catch
                {
                    MessageBox.Show(this, "Error saving table to a file named " + sf.FileName, "Failure");
                    return;
                }
            }
        }
    }
}
