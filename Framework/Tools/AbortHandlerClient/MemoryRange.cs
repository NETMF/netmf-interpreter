using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace AbortHandlerClient
{
	/// <summary>
	/// Summary description for MemoryRange.
	/// </summary>
	public class MemoryRange : System.Windows.Forms.Form
	{
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_BeginAddress;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_EndAddress;
        private System.Windows.Forms.Button button_Ok;
        private System.Windows.Forms.Button button_Cancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;


        public uint m_beginAddress;
        public uint m_endAddress;
        public bool m_beginAddressSet;
        public bool m_endAddressSet;
        public bool m_ok;

		public MemoryRange()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// Need to add any constructor code after InitializeComponent call
			//
            UpdateState();
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
            this.textBox_BeginAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_EndAddress = new System.Windows.Forms.TextBox();
            this.button_Ok = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox_BeginAddress
            // 
            this.textBox_BeginAddress.Location = new System.Drawing.Point(96, 16);
            this.textBox_BeginAddress.Name = "textBox_BeginAddress";
            this.textBox_BeginAddress.TabIndex = 0;
            this.textBox_BeginAddress.Text = "";
            this.textBox_BeginAddress.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_BeginAddress_Validating);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 23);
            this.label1.TabIndex = 1;
            this.label1.Text = "Begin Address:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(8, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 23);
            this.label2.TabIndex = 3;
            this.label2.Text = "End Address:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_EndAddress
            // 
            this.textBox_EndAddress.Location = new System.Drawing.Point(96, 48);
            this.textBox_EndAddress.Name = "textBox_EndAddress";
            this.textBox_EndAddress.TabIndex = 2;
            this.textBox_EndAddress.Text = "";
            this.textBox_EndAddress.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_EndAddress_Validating);
            // 
            // button_Ok
            // 
            this.button_Ok.Location = new System.Drawing.Point(8, 88);
            this.button_Ok.Name = "button_Ok";
            this.button_Ok.Size = new System.Drawing.Size(80, 23);
            this.button_Ok.TabIndex = 7;
            this.button_Ok.Text = "&Ok";
            this.button_Ok.Click += new System.EventHandler(this.button_Ok_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Location = new System.Drawing.Point(200, 88);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(80, 23);
            this.button_Cancel.TabIndex = 8;
            this.button_Cancel.Text = "&Cancel";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // MemoryRange
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 5, 13 );
            this.ClientSize = new System.Drawing.Size(292, 117);
            this.ControlBox = false;
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_Ok);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_EndAddress);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_BeginAddress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "MemoryRange";
            this.Text = "Select Memory Range";
            this.ResumeLayout(false);

        }
		#endregion

        private void textBox_BeginAddress_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_beginAddressSet = ValidateHex( sender as TextBox, ref m_beginAddress );

            if(m_beginAddressSet == false)
            {
                e.Cancel = true;
            }

            UpdateState();
        }

        private void textBox_EndAddress_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_endAddressSet = ValidateHex( sender as TextBox, ref m_endAddress );

            if(m_endAddressSet == false)
            {
                e.Cancel = true;
            }

            UpdateState();
        }

        private bool ValidateHex( TextBox tb, ref uint address )
        {
            if(tb != null)
            {
                try
                {
                    string s = tb.Text;

                    while(s.StartsWith( "0x" ) || s.StartsWith( "0X" ))
                    {
                        s = s.Substring( 2 );
                    }

                    address = UInt32.Parse( s, System.Globalization.NumberStyles.HexNumber );
                }
                catch
                {
                    tb.Focus();
                    return false;
                }
            }

            return true;
        }

        private void UpdateState()
        {
            bool fEnable = false;

            if(m_beginAddressSet && m_endAddressSet)
            {
                if(m_beginAddress < m_endAddress) fEnable = true;
            }

            button_Ok.Enabled = fEnable;

            if(fEnable)
            {
                button_Ok.Focus();
            }
        }

        private void button_Ok_Click(object sender, System.EventArgs e)
        {
            m_ok = true;

            this.Close();
        }

        private void button_Cancel_Click(object sender, System.EventArgs e)
        {
            m_ok = false;

            this.Close();
        }
	}
}

