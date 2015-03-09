////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

using _DBG = Microsoft.SPOT.Debugger;

namespace AbortHandlerClient
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
    public class FormAbortHandler : System.Windows.Forms.Form
    {
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox listBoxRegisters;
        private System.Windows.Forms.TextBox textBoxGotoAddress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelMemory;
        private AbortHandlerClient.MemoryControl memoryControl;
        private System.Windows.Forms.VScrollBar vScrollBarMemory;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuItem_File;
        private System.Windows.Forms.MenuItem menuItem_LoadSym;
        private System.Windows.Forms.MenuItem menuItem_SaveMemory;
        private System.Windows.Forms.MenuItem menuItem_Exit;
        private System.Windows.Forms.MenuItem menuItem_View;
        private System.Windows.Forms.MenuItem menuItem_Bytes;
        private System.Windows.Forms.MenuItem menuItem_Words;
        private System.Windows.Forms.MenuItem menuItem_Disassembly;
        private System.Windows.Forms.MenuItem menuItem_Shorts;
        private System.Windows.Forms.MenuItem menuItem_HeapBlocks;
        private System.Windows.Forms.MenuItem menuItem_Window;
        private System.Windows.Forms.MenuItem menuItem_Window_New;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;


        Settings      m_settings = new Settings();
        MemoryHandler m_mh;

        Hashtable     m_symbols_AddressToString = new Hashtable();
        Hashtable     m_symbols_StringToAddress = new Hashtable();


        public FormAbortHandler()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // Need to add any constructor code after InitializeComponent call
            //
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

                m_settings.Dispose();
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.listBoxRegisters = new System.Windows.Forms.ListBox();
            this.textBoxGotoAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panelMemory = new System.Windows.Forms.Panel();
            this.memoryControl = new AbortHandlerClient.MemoryControl();
            this.vScrollBarMemory = new System.Windows.Forms.VScrollBar();
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuItem_File = new System.Windows.Forms.MenuItem();
            this.menuItem_LoadSym = new System.Windows.Forms.MenuItem();
            this.menuItem_SaveMemory = new System.Windows.Forms.MenuItem();
            this.menuItem_Exit = new System.Windows.Forms.MenuItem();
            this.menuItem_View = new System.Windows.Forms.MenuItem();
            this.menuItem_Bytes = new System.Windows.Forms.MenuItem();
            this.menuItem_Shorts = new System.Windows.Forms.MenuItem();
            this.menuItem_Words = new System.Windows.Forms.MenuItem();
            this.menuItem_Disassembly = new System.Windows.Forms.MenuItem();
            this.menuItem_HeapBlocks = new System.Windows.Forms.MenuItem();
            this.menuItem_Window = new System.Windows.Forms.MenuItem();
            this.menuItem_Window_New = new System.Windows.Forms.MenuItem();
            this.groupBox1.SuspendLayout();
            this.panelMemory.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.listBoxRegisters);
            this.groupBox1.Location = new System.Drawing.Point(8, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 480);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Registers";
            // 
            // listBoxRegisters
            // 
            this.listBoxRegisters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxRegisters.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.listBoxRegisters.ItemHeight = 14;
            this.listBoxRegisters.Location = new System.Drawing.Point(3, 16);
            this.listBoxRegisters.Name = "listBoxRegisters";
            this.listBoxRegisters.Size = new System.Drawing.Size(194, 452);
            this.listBoxRegisters.TabIndex = 0;
            this.listBoxRegisters.DoubleClick += new System.EventHandler(this.listBoxRegisters_DoubleClick);
            // 
            // textBoxGotoAddress
            // 
            this.textBoxGotoAddress.Location = new System.Drawing.Point(320, 16);
            this.textBoxGotoAddress.Name = "textBoxGotoAddress";
            this.textBoxGotoAddress.Size = new System.Drawing.Size(248, 20);
            this.textBoxGotoAddress.TabIndex = 3;
            this.textBoxGotoAddress.Text = "";
            this.textBoxGotoAddress.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxGotoAddress_KeyUp);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(224, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 23);
            this.label1.TabIndex = 4;
            this.label1.Text = "Go To Address:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelMemory
            // 
            this.panelMemory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.panelMemory.BackColor = System.Drawing.SystemColors.Window;
            this.panelMemory.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelMemory.Controls.Add(this.memoryControl);
            this.panelMemory.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.panelMemory.Location = new System.Drawing.Point(216, 48);
            this.panelMemory.Name = "panelMemory";
            this.panelMemory.Size = new System.Drawing.Size(672, 440);
            this.panelMemory.TabIndex = 5;
            // 
            // memoryControl
            // 
            this.memoryControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.memoryControl.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.memoryControl.Location = new System.Drawing.Point(0, 0);
            this.memoryControl.Name = "memoryControl";
            this.memoryControl.Size = new System.Drawing.Size(668, 436);
            this.memoryControl.TabIndex = 0;
            // 
            // vScrollBarMemory
            // 
            this.vScrollBarMemory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.vScrollBarMemory.LargeChange = 30;
            this.vScrollBarMemory.Location = new System.Drawing.Point(888, 48);
            this.vScrollBarMemory.Maximum = 250;
            this.vScrollBarMemory.Minimum = -250;
            this.vScrollBarMemory.Name = "vScrollBarMemory";
            this.vScrollBarMemory.Size = new System.Drawing.Size(16, 440);
            this.vScrollBarMemory.TabIndex = 6;
            this.vScrollBarMemory.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBarMemory_Scroll);
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItem_File,
                                                                                      this.menuItem_View,
                                                                                      this.menuItem_Window});
            // 
            // menuItem_File
            // 
            this.menuItem_File.Index = 0;
            this.menuItem_File.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                          this.menuItem_LoadSym,
                                                                                          this.menuItem_SaveMemory,
                                                                                          this.menuItem_Exit});
            this.menuItem_File.Text = "&File";
            // 
            // menuItem_LoadSym
            // 
            this.menuItem_LoadSym.Index = 0;
            this.menuItem_LoadSym.Text = "Load S&ymdefs...";
            this.menuItem_LoadSym.Click += new System.EventHandler(this.menuItem_LoadSym_Click);
            // 
            // menuItem_SaveMemory
            // 
            this.menuItem_SaveMemory.Index = 1;
            this.menuItem_SaveMemory.Text = "Save Memory...";
            this.menuItem_SaveMemory.Click += new System.EventHandler(this.menuItem_SaveMemory_Click);
            // 
            // menuItem_Exit
            // 
            this.menuItem_Exit.Index = 2;
            this.menuItem_Exit.Text = "E&xit";
            this.menuItem_Exit.Click += new System.EventHandler(this.menuItem_Exit_Click);
            // 
            // menuItem_View
            // 
            this.menuItem_View.Index = 1;
            this.menuItem_View.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                          this.menuItem_Bytes,
                                                                                          this.menuItem_Shorts,
                                                                                          this.menuItem_Words,
                                                                                          this.menuItem_Disassembly,
                                                                                          this.menuItem_HeapBlocks});
            this.menuItem_View.Text = "&View";
            // 
            // menuItem_Bytes
            // 
            this.menuItem_Bytes.Index = 0;
            this.menuItem_Bytes.RadioCheck = true;
            this.menuItem_Bytes.Text = "&Bytes";
            this.menuItem_Bytes.Click += new System.EventHandler(this.menuItem_Bytes_Click);
            // 
            // menuItem_Shorts
            // 
            this.menuItem_Shorts.Index = 1;
            this.menuItem_Shorts.RadioCheck = true;
            this.menuItem_Shorts.Text = "&Shorts";
            this.menuItem_Shorts.Click += new System.EventHandler(this.menuItem_Shorts_Click);
            // 
            // menuItem_Words
            // 
            this.menuItem_Words.Index = 2;
            this.menuItem_Words.RadioCheck = true;
            this.menuItem_Words.Text = "&Words";
            this.menuItem_Words.Click += new System.EventHandler(this.menuItem_Words_Click);
            // 
            // menuItem_Disassembly
            // 
            this.menuItem_Disassembly.Checked = true;
            this.menuItem_Disassembly.Index = 3;
            this.menuItem_Disassembly.RadioCheck = true;
            this.menuItem_Disassembly.Text = "&Disassembly";
            this.menuItem_Disassembly.Click += new System.EventHandler(this.menuItem_Disassembly_Click);
            // 
            // menuItem_HeapBlocks
            // 
            this.menuItem_HeapBlocks.Index = 4;
            this.menuItem_HeapBlocks.Text = "&Heap Blocks";
            this.menuItem_HeapBlocks.Click += new System.EventHandler(this.menuItem_HeapBlocks_Click);
            // 
            // menuItem_Window
            // 
            this.menuItem_Window.Index = 2;
            this.menuItem_Window.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                            this.menuItem_Window_New});
            this.menuItem_Window.Text = "&Window";
            // 
            // menuItem_Window_New
            // 
            this.menuItem_Window_New.Index = 0;
            this.menuItem_Window_New.Text = "&New Window";
            this.menuItem_Window_New.Click += new System.EventHandler(this.menuItem_Window_New_Click);
            // 
            // FormAbortHandler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 5, 13 );
            this.ClientSize = new System.Drawing.Size(912, 493);
            this.Controls.Add(this.vScrollBarMemory);
            this.Controls.Add(this.panelMemory);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxGotoAddress);
            this.Controls.Add(this.groupBox1);
            this.Menu = this.mainMenu1;
            this.Name = "FormAbortHandler";
            this.Text = "Abort Handler Client";
            this.Load += new System.EventHandler(this.FormAbortHandler_Load);
            this.groupBox1.ResumeLayout(false);
            this.panelMemory.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.Run(new FormAbortHandler());
        }

        private void FormAbortHandler_Load(object sender, System.EventArgs e)
        {
            if(m_mh == null)
            {
                m_settings.ShowDialog();

                m_mh = new MemoryHandler( m_settings.m_ah );
            }

            memoryControl.Link( m_mh, m_symbols_AddressToString, m_symbols_StringToAddress );


            MemorySnapshot mem = m_settings.m_snapshot;

            m_mh.PopulateFromSnapshot( mem );

            for(int i=0; i<=12; i++)
            {
                listBoxRegisters.Items.Add( String.Format( "R{0,-2}  = 0x{1,8:X8}", i, mem.m_registers[i] ) );
            }

            listBoxRegisters.Items.Add( String.Format( "SP   = 0x{0,8:X8}", mem.m_registers[13] ) );
            listBoxRegisters.Items.Add( String.Format( "LR   = 0x{0,8:X8}", mem.m_registers[14] ) );
            listBoxRegisters.Items.Add( String.Format( "PC   = 0x{0,8:X8}", mem.m_registers[15] ) );
            listBoxRegisters.Items.Add( String.Format( "CPSR = 0x{0,8:X8}", mem.m_cpsr          ) );
            listBoxRegisters.Items.Add( String.Format( "BWA  = 0x{0,8:X8}", mem.m_BWA           ) );
            listBoxRegisters.Items.Add( String.Format( "BWC  = 0x{0,8:X8}", mem.m_BWC           ) );

            memoryControl.ShowMemory( mem.m_registers[15] );
        }

        private void textBoxGotoAddress_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Return)
            {
                string text = textBoxGotoAddress.Text;

                if(m_symbols_StringToAddress.Contains( text ))
                {
                    memoryControl.ShowMemory( (uint)m_symbols_StringToAddress[ text ] );
                    return;
                }

                try
                {
                    uint address = UInt32.Parse( text, System.Globalization.NumberStyles.HexNumber );

                    memoryControl.ShowMemory( address );
                }
                catch
                {
                    textBoxGotoAddress.Focus();
                }
            }
        }

        private void listBoxRegisters_DoubleClick(object sender, System.EventArgs e)
        {
            int idx = listBoxRegisters.SelectedIndex;

            if(idx >= 0 && idx < 16)
            {
                memoryControl.ShowMemory( m_settings.m_snapshot.m_registers[idx] );
            }
        }


        private void vScrollBarMemory_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
        {
            switch(e.Type)
            {
                case ScrollEventType.EndScroll:
                    e.NewValue = 0;
                    break;

                case ScrollEventType.LargeDecrement:
                case ScrollEventType.LargeIncrement:
                case ScrollEventType.SmallDecrement:
                case ScrollEventType.SmallIncrement:
                case ScrollEventType.ThumbTrack:
                    memoryControl.Scroll( e.NewValue - vScrollBarMemory.Value );
                    break;
            }
        }

        private void menuItem_LoadSym_Click(object sender, System.EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Filter   = "Build Symbols (*.symdefs)|*.symdefs";
            dlg.Title    = "Load Build Symbols from file";
            dlg.FileName = "";

            if(dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Hashtable code = new Hashtable();
                    Hashtable data = new Hashtable();

                    _DBG.SymDef.Parse( dlg.FileName, code, data );

                    m_symbols_AddressToString.Clear();
                    m_symbols_StringToAddress.Clear();

                    foreach(string strCode in code.Keys)
                    {
                        uint address = (uint)code[strCode];

                        m_symbols_AddressToString[address] = strCode;
                        m_symbols_StringToAddress[strCode] = address;
                    }

                    foreach(string strData in data.Keys)
                    {
                        uint address = (uint)data[strData];

                        m_symbols_AddressToString[address] = strData;
                        m_symbols_StringToAddress[strData] = address;
                    }

                    memoryControl.Reload();
                }
                catch(Exception ex)
                {
                    MessageBox.Show( String.Format( "Error while loading symbolds:\n\n{0}", ex.ToString() ), "Error" );
                }
            }
        }

        private void menuItem_SaveMemory_Click(object sender, System.EventArgs e)
        {
            MemoryRange mr = new MemoryRange();

            mr.ShowDialog();

            if(mr.m_ok)
            {
                byte[] buf = m_mh.ReadRegionAsBuffer( mr.m_beginAddress, (int)(mr.m_endAddress - mr.m_beginAddress) );

                using(System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog())
                {
                    dlg.Filter = "SREC format (*.hex)|*.hex|Raw Memory (*.bin)|*.bin";
                    dlg.Title  = "Save memory range to a file";

                    if(dlg.ShowDialog() == DialogResult.OK)
                    {
                        using(FileStream s = new FileStream( dlg.FileName, FileMode.Create, FileAccess.Write ))
                        {
                            if(dlg.FilterIndex == 1)
                            {
                                Microsoft.SPOT.Debugger.SRecordFile.Encode( s, buf, mr.m_beginAddress );
                            }
                            else
                            {
                                s.Write( buf, 0, buf.Length );
                            }
                        }
                    }
                }
            }
        }

        private void menuItem_Exit_Click(object sender, System.EventArgs e)
        {
            Application.Exit();
        }


        private void SelectViewMode( MemoryControl.ViewMode vm )
        {
            menuItem_Bytes      .Checked = (vm == MemoryControl.ViewMode.Bytes      );
            menuItem_Shorts     .Checked = (vm == MemoryControl.ViewMode.Shorts     );
            menuItem_Words      .Checked = (vm == MemoryControl.ViewMode.Words      );
            menuItem_Disassembly.Checked = (vm == MemoryControl.ViewMode.Disassembly);
            menuItem_HeapBlocks .Checked = (vm == MemoryControl.ViewMode.HeapBlocks );

            memoryControl.SetViewMode( vm );
        }

        private void menuItem_Bytes_Click(object sender, System.EventArgs e)
        {
            SelectViewMode( MemoryControl.ViewMode.Bytes );
        }

        private void menuItem_Shorts_Click(object sender, System.EventArgs e)
        {
            SelectViewMode( MemoryControl.ViewMode.Shorts );
        }

        private void menuItem_Words_Click(object sender, System.EventArgs e)
        {
            SelectViewMode( MemoryControl.ViewMode.Words );
        }

        private void menuItem_Disassembly_Click(object sender, System.EventArgs e)
        {
            SelectViewMode( MemoryControl.ViewMode.Disassembly );
        }

        private void menuItem_HeapBlocks_Click(object sender, System.EventArgs e)
        {
            SelectViewMode( MemoryControl.ViewMode.HeapBlocks );
        }

        private void menuItem_Window_New_Click(object sender, System.EventArgs e)
        {
            FormAbortHandler form = new FormAbortHandler();

            form.m_settings                = m_settings;
            form.m_mh                      = m_mh;
            form.m_symbols_AddressToString = m_symbols_AddressToString;
            form.m_symbols_StringToAddress = m_symbols_StringToAddress;

            form.Show();
        }
    }
}

