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
    /// Summary description for SelectColumns.
    /// </summary>
    public class SelectColumns : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private ArrayList checkBoxes;

        public void Set(int id)
        {
            ((CheckBox)checkBoxes[id]).Checked = true;
        }

        public ArrayList GetCheckedColumns()
        {
            ArrayList r = new ArrayList();
            for(int i = 0; i < checkBoxes.Count; i++)
            {
                if(((CheckBox)checkBoxes[i]).Checked)
                {
                    r.Add(i);
                }
            }
            return r;
        }

        public SelectColumns()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // need to add any constructor code after InitializeComponent call
            //

            checkBoxes = new ArrayList();

            Hashtable underscored = new Hashtable();

            int numCounters = Statistics.GetNumberOfCounters();
            for(int i = 0; i < numCounters; i++)
            {
                string text = Statistics.GetCounterName(i);
                for(int j = 0; j < text.Length; j++)
                {
                    if(!Char.IsLetter(text, j))
                    {
                        continue;
                    }

                    char c = Char.ToLower(text[j]);
                    if(!underscored.ContainsKey(c))
                    {
                        underscored.Add(c, null);
                        text = text.Substring(0, j) + "&" + text.Substring(j);
                        break;
                    }
                }

                int px = (i % 2) == 1 ? Width / 2 : 16, py = 56;
                if(i > 1)
                {
                    py = ((CheckBox)checkBoxes[i - 2]).Bottom;
                }

                CheckBox box = new CheckBox();
                box.Parent = this;
                box.Visible = true;
                box.Location = new Point(px, py);
                box.Width = Width / 2 - 10;
                box.Text = text;
                box.BringToFront();

                checkBoxes.Add(box);
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
            if(checkBoxes != null)
            {
                foreach(Control c in checkBoxes)
                {
                    c.Dispose();
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(152, 200);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(72, 24);
            this.button1.TabIndex = 0;
            this.button1.Text = "OK";
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(232, 200);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(72, 24);
            this.button2.TabIndex = 1;
            this.button2.Text = "Cancel";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(288, 24);
            this.label1.TabIndex = 2;
            this.label1.Text = "Select the columns that will appear in the call tree view.";
            // 
            // SelectColumns
            // 
            this.ClientSize = new System.Drawing.Size(314, 239);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                          this.label1,
                                                                          this.button2,
                                                                          this.button1});
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "SelectColumns";
            this.Text = "SelectColumns";
            this.ResumeLayout(false);

        }
        #endregion
    }
}
