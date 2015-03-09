using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Gui.Wizard
{

	/// <summary>
	/// An inherited <see cref="InfoContainer"/> that contains a <see cref="Label"/> 
	/// with the description of the page.
	/// </summary>
	public class InfoPage : InfoContainer
	{
		private System.Windows.Forms.Label lblDescription;
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public InfoPage()
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lblDescription = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblDescription
			// 
			this.lblDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblDescription.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblDescription.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblDescription.Location = new System.Drawing.Point(172, 56);
			this.lblDescription.Name = "lblDescription";
			this.lblDescription.Size = new System.Drawing.Size(304, 328);
			this.lblDescription.TabIndex = 8;
			this.lblDescription.Text = "This wizard enables you to...";
			// 
			// InfoPage
			// 
			this.Controls.Add(this.lblDescription);
			this.Name = "InfoPage";
			this.Controls.SetChildIndex(this.lblDescription, 0);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Gets/Sets the text on the info page
		/// </summary>
		[Category("Appearance")]
		public string PageText
		{
			get
			{
				return lblDescription.Text;
			}
			set
			{
				lblDescription.Text = value;
			}
		}
	}
}

