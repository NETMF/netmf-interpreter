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
    /// Summary description for Settings.
    /// </summary>
    public class Settings : System.Windows.Forms.Form
    {
        private System.Windows.Forms.ComboBox comboBoxBaud;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ProgressBar progressBarDownload;
        private System.Windows.Forms.Button button_DebugOnline;
        private System.Windows.Forms.Button button_FullSnapshot;
        private System.Windows.Forms.Button button_Exit;
        private System.Windows.Forms.Button button_RamSnapshot;
        private System.Windows.Forms.Button button_DebugOffline;
        private System.Windows.Forms.GroupBox groupBox1;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;


        public  _DBG.Engine                                          m_eng;
        public  _DBG.AbortHandler                                    m_ah;
        public  Thread                                               m_worker;
        public  MemorySnapshot                                       m_snapshot = new MemorySnapshot();
        private bool                                                 m_snapshot_RAM;
        private bool                                                 m_snapshot_FLASH;
        private string                                               m_snapshot_File;
        private int                                                  m_snapshot_Kind;
        private bool                                                 m_deviceRunning;
        private _DBG.WireProtocol.Commands.Monitor_MemoryMap.Range[] m_ranges;


        public Settings()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // need to add any constructor code after InitializeComponent call
            //
            comboBoxPort.DataSource    = _DBG.PortDefinition.Enumerate( _DBG.PortFilter.Serial );
            comboBoxPort.DisplayMember = "DisplayName";
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

                Thread worker = m_worker;
                if(worker != null)
                {
                    worker.Abort();
                    worker.Join ();
                }

                Disconnect();
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
            this.comboBoxBaud = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxPort = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_DebugOnline = new System.Windows.Forms.Button();
            this.progressBarDownload = new System.Windows.Forms.ProgressBar();
            this.labelStatus = new System.Windows.Forms.Label();
            this.button_FullSnapshot = new System.Windows.Forms.Button();
            this.button_Exit = new System.Windows.Forms.Button();
            this.button_RamSnapshot = new System.Windows.Forms.Button();
            this.button_DebugOffline = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            //
            // comboBoxBaud
            //
            this.comboBoxBaud.Items.AddRange(new object[] {
                                                              "115200",
                                                              "57600",
                                                              "28800",
                                                              "14400",
                                                              "9600",
                                                              "2400"});
            this.comboBoxBaud.Location = new System.Drawing.Point(192, 176);
            this.comboBoxBaud.Name = "comboBoxBaud";
            this.comboBoxBaud.Size = new System.Drawing.Size(80, 21);
            this.comboBoxBaud.TabIndex = 1;
            this.comboBoxBaud.Text = "115200";
            //
            // label2
            //
            this.label2.Location = new System.Drawing.Point(152, 176);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 23);
            this.label2.TabIndex = 10;
            this.label2.Text = "Speed:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // comboBoxPort
            //
            this.comboBoxPort.Location = new System.Drawing.Point(64, 176);
            this.comboBoxPort.Name = "comboBoxPort";
            this.comboBoxPort.Size = new System.Drawing.Size(80, 21);
            this.comboBoxPort.TabIndex = 0;
            //
            // label1
            //
            this.label1.Location = new System.Drawing.Point(8, 176);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 23);
            this.label1.TabIndex = 7;
            this.label1.Text = "COM Port:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // button_DebugOnline
            //
            this.button_DebugOnline.Location = new System.Drawing.Point(16, 120);
            this.button_DebugOnline.Name = "button_DebugOnline";
            this.button_DebugOnline.Size = new System.Drawing.Size(240, 23);
            this.button_DebugOnline.TabIndex = 5;
            this.button_DebugOnline.Text = "&Debug Online";
            this.button_DebugOnline.Click += new System.EventHandler(this.button_DebugOnline_Click);
            //
            // progressBarDownload
            //
            this.progressBarDownload.Location = new System.Drawing.Point(8, 232);
            this.progressBarDownload.Name = "progressBarDownload";
            this.progressBarDownload.Size = new System.Drawing.Size(272, 23);
            this.progressBarDownload.TabIndex = 12;
            //
            // labelStatus
            //
            this.labelStatus.Location = new System.Drawing.Point(8, 208);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(272, 23);
            this.labelStatus.TabIndex = 13;
            //
            // button_FullSnapshot
            //
            this.button_FullSnapshot.Location = new System.Drawing.Point(16, 56);
            this.button_FullSnapshot.Name = "button_FullSnapshot";
            this.button_FullSnapshot.Size = new System.Drawing.Size(240, 23);
            this.button_FullSnapshot.TabIndex = 3;
            this.button_FullSnapshot.Text = "Take &Full Snapshot (Slow)";
            this.button_FullSnapshot.Click += new System.EventHandler(this.button_FullSnapshot_Click);
            //
            // button_Exit
            //
            this.button_Exit.Location = new System.Drawing.Point(200, 264);
            this.button_Exit.Name = "button_Exit";
            this.button_Exit.Size = new System.Drawing.Size(80, 23);
            this.button_Exit.TabIndex = 6;
            this.button_Exit.Text = "&Exit";
            this.button_Exit.Click += new System.EventHandler(this.button_Exit_Click);
            //
            // button_RamSnapshot
            //
            this.button_RamSnapshot.Location = new System.Drawing.Point(16, 24);
            this.button_RamSnapshot.Name = "button_RamSnapshot";
            this.button_RamSnapshot.Size = new System.Drawing.Size(240, 23);
            this.button_RamSnapshot.TabIndex = 2;
            this.button_RamSnapshot.Text = "Take &RAM Snapshot";
            this.button_RamSnapshot.Click += new System.EventHandler(this.button_RamSnapshot_Click);
            //
            // button_DebugOffline
            //
            this.button_DebugOffline.Location = new System.Drawing.Point(16, 88);
            this.button_DebugOffline.Name = "button_DebugOffline";
            this.button_DebugOffline.Size = new System.Drawing.Size(240, 23);
            this.button_DebugOffline.TabIndex = 4;
            this.button_DebugOffline.Text = "Debug &Offline";
            this.button_DebugOffline.Click += new System.EventHandler(this.button_DebugOffline_Click);
            //
            // groupBox1
            //
            this.groupBox1.Controls.Add(this.button_FullSnapshot);
            this.groupBox1.Controls.Add(this.button_DebugOffline);
            this.groupBox1.Controls.Add(this.button_DebugOnline);
            this.groupBox1.Controls.Add(this.button_RamSnapshot);
            this.groupBox1.Location = new System.Drawing.Point(8, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(272, 160);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Actions";
            //
            // Settings
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF( 5, 13 );
            this.ClientSize = new System.Drawing.Size(290, 295);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button_Exit);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.progressBarDownload);
            this.Controls.Add(this.comboBoxBaud);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBoxPort);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Settings";
            this.Text = "Crash Dump Control Panel";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion


        private void Disconnect()
        {
            if(m_ah != null)
            {
                m_ah.Stop   ();
                m_ah.Dispose();

                m_ah = null;
            }

            if(m_eng != null)
            {
                if(m_deviceRunning)
                {
                    m_eng.ResumeExecution();
                }

                m_eng.Stop();

                m_eng = null;
            }

            m_deviceRunning = false;
        }

        private bool Connect()
        {
            Disconnect();

            try
            {
                _DBG.PortDefinition pd = (_DBG.PortDefinition)comboBoxPort.SelectedItem;

                string port     =                      pd.Port;
                uint   baudrate = uint.Parse( ((string)comboBoxBaud.SelectedItem) );

                using(_DBG.AsyncSerialStream stream = new _DBG.AsyncSerialStream( port, baudrate ))
                {
                    stream.Close();
                }

                m_eng = new _DBG.Engine( new Microsoft.SPOT.Debugger.PortDefinition_Serial( port, port, baudrate ) );

                m_eng.Silent = true;

                m_eng.Start();

                if(m_eng.TryToConnect( 4, 250 ))
                {
                    m_deviceRunning = true;

                    m_eng.PauseExecution();

                    m_ranges = m_eng.MemoryMap();
                    if(m_ranges == null)
                    {
                        //
                        // Fallback to some defaults.
                        //
                        m_ranges = new _DBG.WireProtocol.Commands.Monitor_MemoryMap.Range[2];

                        m_ranges[0] = new _DBG.WireProtocol.Commands.Monitor_MemoryMap.Range();
                        m_ranges[0].m_address = 0x00000000;
                        m_ranges[0].m_length  = 0x00060000;
                        m_ranges[0].m_flags   = _DBG.WireProtocol.Commands.Monitor_MemoryMap.c_RAM;

                        m_ranges[1] = new _DBG.WireProtocol.Commands.Monitor_MemoryMap.Range();
                        m_ranges[1].m_address = 0x10000000;
                        m_ranges[1].m_length  = 1024*1024;
                        m_ranges[1].m_flags   = _DBG.WireProtocol.Commands.Monitor_MemoryMap.c_FLASH;
                    }
                }
                else
                {
                    m_deviceRunning = false;
                }


                m_ah = new _DBG.AbortHandler( m_eng, m_deviceRunning );

                m_ah.Start();

                
                bool connected = false;

                for(int tries=0; tries < 5; tries++)
                {
                    if(m_ah.ReadRegisters( m_snapshot.m_registers, out m_snapshot.m_cpsr, out m_snapshot.m_BWA, out m_snapshot.m_BWC ))
                    {
                        connected = true; break;
                    }

                    Thread.Sleep( 1000 );
                }

                if(connected)
                {
                    for(int tries=0; tries < 5; tries++)
                    {
                        if(m_ah.ReadLayout( out m_snapshot.m_flashBase, out m_snapshot.m_flashSize, out m_snapshot.m_ramBase, out m_snapshot.m_ramSize ))
                        {
                            break;
                        }

                        Thread.Sleep( 1000 );
                    }

                    return true;
                }

                if(!connected)
                {
                    MessageBox.Show( "Cannot connect to device" );
                }

                return connected;
            }
            catch(Exception ex)
            {
                MessageBox.Show( ex.Message );

                return false;
            }
        }


        private void SetControlsStatus( bool fEnabled )
        {
            comboBoxPort       .Enabled = fEnabled;
            comboBoxBaud       .Enabled = fEnabled;
            button_DebugOnline .Enabled = fEnabled;
            button_DebugOffline.Enabled = fEnabled;
            button_RamSnapshot .Enabled = fEnabled;
            button_FullSnapshot.Enabled = fEnabled;
        }

        private void TakeSnapshot()
        {
            if(Connect())
            {
                using(System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog())
                {
                    dlg.Filter = "Snapshot files (*.spodump)|*.spodump|Raw files (*.sporawdump)|*.sporawdump";
                    dlg.Title  = "Save snapshot to a file";

                    if(dlg.ShowDialog() == DialogResult.OK)
                    {
                        m_snapshot_File = dlg.FileName;
                        m_snapshot_Kind = dlg.FilterIndex;


                        m_worker = new Thread( new ThreadStart( this.TakeSnapshot_Worker ) );

                        m_worker.Start();

                        SetControlsStatus( false );
                    }
                }
            }
        }

        private void TakeSnapshot_Worker()
        {
            try
            {
                int size;

                if(m_snapshot_RAM)
                {
                    MemorySnapshot.MemoryBlock mb = new MemorySnapshot.MemoryBlock();

                    mb.m_address = m_snapshot.m_ramBase;

                    size = ProbeMemory( mb.m_address, (int)m_snapshot.m_ramSize, 128 * 1024 );

                    mb.m_data = new byte[size];

                    if(GetMemory( mb.m_address, mb.m_data, 0, size, "Loading RAM" ) != size)
                    {
                        // ??
                    }

                    m_snapshot.m_block_RAM = mb;
                }
                else
                {
                    m_snapshot.m_block_RAM = null;
                }


                if(m_snapshot_FLASH)
                {
                    MemorySnapshot.MemoryBlock mb = new MemorySnapshot.MemoryBlock();

                    mb.m_address = m_snapshot.m_flashBase;

                    size = ProbeMemory( mb.m_address, (int)m_snapshot.m_flashSize, 1024 * 1024 );

                    mb.m_data = new byte[size];

                    if(GetMemory( mb.m_address, mb.m_data, 0, size, "Loading FLASH" ) != size)
                    {
                        // ??
                    }

                    m_snapshot.m_block_FLASH = mb;
                }
                else
                {
                    m_snapshot.m_block_FLASH = null;
                }

                try
                {
                    using(FileStream s = new FileStream( m_snapshot_File, FileMode.Create, FileAccess.Write ))
                    {
                        if(m_snapshot_Kind == 1)
                        {
                            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter fmt = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                            fmt.Serialize( s, m_snapshot );
                        }
                        else
                        {
                            BinaryWriter writer = new BinaryWriter( s );

                            writer.Write( (uint)0xDEADBEEF );

                            if(m_snapshot_RAM)
                            {
                                MemorySnapshot.MemoryBlock mb = m_snapshot.m_block_RAM;

                                writer.Write( mb.m_address                         );
                                writer.Write(               (uint)mb.m_data.Length );
                                writer.Write( mb.m_data, 0,       mb.m_data.Length );
                            }

                            if(m_snapshot_FLASH)
                            {
                                MemorySnapshot.MemoryBlock mb = m_snapshot.m_block_FLASH;

                                writer.Write( mb.m_address                         );
                                writer.Write(               (uint)mb.m_data.Length );
                                writer.Write( mb.m_data, 0,       mb.m_data.Length );
                            }

                            writer.Flush();
                        }

                        s.Flush();
                    }
                }
                catch
                {
                }
            }
            finally
            {
                try
                {
                    Disconnect();
                }
                catch
                {
                }

                m_worker = null;

                labelStatus        .Text  = "";
                progressBarDownload.Value = 0;

                SetControlsStatus( true );
            }
        }

        private int ProbeMemory( uint address, int size, int granularity )
        {
            if(m_deviceRunning)
            {
                foreach(_DBG.WireProtocol.Commands.Monitor_MemoryMap.Range rng in m_ranges)
                {
                    if(rng.m_address == address)
                    {
                        return (int)rng.m_length;
                    }
                }

                return 0;
            }
            else
            {
                const int chunk = 128;
                byte[]    tmp   = new byte[chunk];

                while(size > 0)
                {
                    for(int tries=0; tries < 5; tries++)
                    {
                        if(m_ah.ReadMemory( (uint)(address + size - chunk), tmp, 0, chunk ))
                        {
                            return size;
                        }
                    }

                    size -= granularity;
                }
            }

            return size;
        }

        private int GetMemory( uint address, byte[] data, int offset, int size, string text )
        {
            int      read            = 0;
            DateTime lastSampleTime  = DateTime.Now;
            int      lastSampleRead  = read;
            string   estimate        = "--";

            progressBarDownload.Value   = 0;
            progressBarDownload.Minimum = 0;
            progressBarDownload.Maximum = size;

            while(read < size)
            {
                int      tries;
                DateTime now  = DateTime.Now;
                TimeSpan diff = now - lastSampleTime;

                if((read - lastSampleRead) > 10000)
                {
                    diff = now.AddSeconds( diff.TotalSeconds * (size - read) / (read - lastSampleRead) ) - now;

                    estimate = "";

                    if(diff.Hours   > 0                       ) estimate += String.Format( "{0}h", diff.Hours   );
                    if(diff.Minutes > 0 || estimate.Length > 0) estimate += String.Format( "{0}m", diff.Minutes );
                    if(diff.Seconds > 0 || estimate.Length > 0) estimate += String.Format( "{0}s", diff.Seconds );

                    lastSampleTime = now;
                    lastSampleRead = read;
                }

                labelStatus        .Text  = String.Format( "{0} ({1}/{2})   {3}", text, read, size, estimate );
                progressBarDownload.Value = read;

                for(tries=0; tries < 5; tries++)
                {
                    const int chunk = 1024;

                    if(m_ah.ReadMemory( address, data, offset, chunk ))
                    {
                        address += chunk;
                        offset  += chunk;
                        read    += chunk;
                        break;
                    }
                }

                if(tries == 5) break;
            }

            return read;
        }


        private void button_DebugOnline_Click(object sender, System.EventArgs e)
        {
            if(Connect())
            {
                this.Close();
            }
        }

        private void button_DebugOffline_Click(object sender, System.EventArgs e)
        {
            try
            {
                using(System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog())
                {
                    dlg.Filter = "Snapshot files (*.spodump)|*.spodump|SREC format (*.hex)|*.hex";
                    dlg.Title  = "Load snapshot from a file";

                    if(dlg.ShowDialog() == DialogResult.OK)
                    {
                        if(dlg.FilterIndex == 1)
                        {
                            using(FileStream s = new FileStream( dlg.FileName, FileMode.Open, FileAccess.Read ))
                            {
                                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter fmt = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                                m_snapshot = (MemorySnapshot)fmt.Deserialize( s );

                                if(m_snapshot.m_block_FLASH == null)
                                {
                                    DialogResult res = MessageBox.Show( "No FLASH image in the dump.\nDo you want to connect to a device?", "Snapshot", MessageBoxButtons.YesNoCancel );

                                    if(res == DialogResult.Cancel)
                                    {
                                        m_snapshot = new MemorySnapshot();
                                        return;
                                    }

                                    if(res == DialogResult.Yes)
                                    {
                                        if(Connect() == false)
                                        {
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            ArrayList blocks     = new ArrayList();
                            uint      entrypoint = Microsoft.SPOT.Debugger.SRecordFile.Parse( dlg.FileName, blocks, null );

                            MemorySnapshot.MemoryBlock blk = new MemorySnapshot.MemoryBlock();

                            m_snapshot = new MemorySnapshot();
                            m_snapshot.m_block_FLASH = blk;

                            foreach(_DBG.SRecordFile.Block bl in blocks)
                            {
                                blk.m_address = bl.address;
                                blk.m_data    = bl.data.ToArray();
                            }

                            m_snapshot.m_registers[15] = blk.m_address;
                        }

                        this.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show( ex.Message );
            }
        }

        private void button_RamSnapshot_Click(object sender, System.EventArgs e)
        {
            m_snapshot_RAM   = true;
            m_snapshot_FLASH = false;

            TakeSnapshot();
        }

        private void button_FullSnapshot_Click(object sender, System.EventArgs e)
        {
            m_snapshot_RAM   = true;
            m_snapshot_FLASH = true;

            TakeSnapshot();
        }

        private void button_Exit_Click(object sender, System.EventArgs e)
        {
            Application.Exit();
        }
    }
}
