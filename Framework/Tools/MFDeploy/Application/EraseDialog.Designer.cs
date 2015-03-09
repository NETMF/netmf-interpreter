namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
    partial class EraseDialog
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
            this.listViewEraseSectors = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonErase = new System.Windows.Forms.Button();
            this.buttonEraseSectors = new System.Windows.Forms.Button();
            this.columnHeaderSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listViewEraseSectors
            // 
            this.listViewEraseSectors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewEraseSectors.BackColor = System.Drawing.SystemColors.Window;
            this.listViewEraseSectors.CheckBoxes = true;
            this.listViewEraseSectors.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderAddress,
            this.columnHeaderSize});
            this.listViewEraseSectors.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewEraseSectors.FullRowSelect = true;
            this.listViewEraseSectors.GridLines = true;
            this.listViewEraseSectors.HideSelection = false;
            this.listViewEraseSectors.Location = new System.Drawing.Point(12, 12);
            this.listViewEraseSectors.Name = "listViewEraseSectors";
            this.listViewEraseSectors.Size = new System.Drawing.Size(359, 131);
            this.listViewEraseSectors.TabIndex = 10;
            this.listViewEraseSectors.UseCompatibleStateImageBehavior = false;
            this.listViewEraseSectors.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Tag = "";
            this.columnHeaderName.Text = "Block Usage";
            this.columnHeaderName.Width = 120;
            // 
            // columnHeaderAddress
            // 
            this.columnHeaderAddress.Text = "Address";
            this.columnHeaderAddress.Width = 115;
            // 
            // buttonErase
            // 
            this.buttonErase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonErase.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonErase.Location = new System.Drawing.Point(296, 149);
            this.buttonErase.Name = "buttonErase";
            this.buttonErase.Size = new System.Drawing.Size(75, 23);
            this.buttonErase.TabIndex = 12;
            this.buttonErase.Text = "&Cancel";
            this.buttonErase.UseVisualStyleBackColor = true;
            // 
            // buttonEraseSectors
            // 
            this.buttonEraseSectors.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEraseSectors.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonEraseSectors.Location = new System.Drawing.Point(215, 149);
            this.buttonEraseSectors.Name = "buttonEraseSectors";
            this.buttonEraseSectors.Size = new System.Drawing.Size(75, 23);
            this.buttonEraseSectors.TabIndex = 11;
            this.buttonEraseSectors.Text = "&Erase";
            this.buttonEraseSectors.UseVisualStyleBackColor = true;
            this.buttonEraseSectors.Click += new System.EventHandler(this.buttonEraseSectors_Click);
            // 
            // columnHeaderSize
            // 
            this.columnHeaderSize.Text = "Size";
            this.columnHeaderSize.Width = 115;
            // 
            // EraseDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(383, 184);
            this.Controls.Add(this.listViewEraseSectors);
            this.Controls.Add(this.buttonErase);
            this.Controls.Add(this.buttonEraseSectors);
            this.Name = "EraseDialog";
            this.Text = "Erase Sectors";
            this.Load += new System.EventHandler(this.EraseDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listViewEraseSectors;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.Button buttonErase;
        private System.Windows.Forms.Button buttonEraseSectors;
        private System.Windows.Forms.ColumnHeader columnHeaderAddress;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
    }
}