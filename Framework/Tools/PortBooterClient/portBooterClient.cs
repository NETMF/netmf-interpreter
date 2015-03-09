using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

using _DBG = Microsoft.SPOT.Debugger;

namespace PortBooterClient
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class Form1 : System.Windows.Forms.Form
    {
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListView listViewFiles;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Button buttonReload;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonRemoveAll;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button buttonAction;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.ProgressBar progressBar1;
        private ComboBox comboBoxPort;
        private System.Windows.Forms.ComboBox comboBoxBaud;
        private System.Windows.Forms.CheckBox checkBoxWait;
        private System.Windows.Forms.CheckBox checkBoxDisconnect;
        private System.Windows.Forms.RadioButton radioButtonPortBooter;
        private System.Windows.Forms.RadioButton radioButtonTinyBooter;
        private System.Windows.Forms.Button buttonDeployMap;
        private System.Windows.Forms.Button buttonRebootAndStop;
        private System.Windows.Forms.Button buttonEraseDeployment;
        private System.Windows.Forms.Button buttonPing;
        private System.Windows.Forms.Button buttonCLRCap;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;


        _DBG.Engine             m_eng;
        _DBG.PortDefinition     m_lastValid;
        _DBG.PortBooter          m_fl;
        _DBG.UsbDeviceDiscovery m_usbDiscovery;
        Thread                  m_worker;
        ArrayList               m_blocks;
        bool                    m_fWait;
        bool                    m_fDisconnect;


        delegate void Callback_Bool( bool fFlag );

        
        public Form1() : base()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // need to add any constructor code after InitializeComponent call
            //
            m_callback_NewText  = new NewTextCallback ( this.Callback_NewText  );
            m_callback_Progress = new ProgressCallback( this.Callback_Progress );


            m_usbDiscovery = new _DBG.UsbDeviceDiscovery();

            m_usbDiscovery.OnDeviceChanged += new _DBG.UsbDeviceDiscovery.DeviceChangedEventHandler( OnDeviceChanged );
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                Stop();

                if (components != null)
                {
                    components.Dispose();
                }

                if( m_usbDiscovery != null )
                {
                    m_usbDiscovery.Dispose();
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonReload = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.buttonRemoveAll = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.listViewFiles = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.buttonAction = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxPort = new System.Windows.Forms.ComboBox();
            this.comboBoxBaud = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonClear = new System.Windows.Forms.Button();
            this.checkBoxWait = new System.Windows.Forms.CheckBox();
            this.checkBoxDisconnect = new System.Windows.Forms.CheckBox();
            this.radioButtonPortBooter = new System.Windows.Forms.RadioButton();
            this.radioButtonTinyBooter = new System.Windows.Forms.RadioButton();
            this.buttonDeployMap = new System.Windows.Forms.Button();
            this.buttonRebootAndStop = new System.Windows.Forms.Button();
            this.buttonEraseDeployment = new System.Windows.Forms.Button();
            this.buttonPing = new System.Windows.Forms.Button();
            this.buttonCLRCap= new System.Windows.Forms.Button();            
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.groupBox1.Controls.Add( this.buttonReload );
            this.groupBox1.Controls.Add( this.progressBar1 );
            this.groupBox1.Controls.Add( this.buttonRemoveAll );
            this.groupBox1.Controls.Add( this.buttonRemove );
            this.groupBox1.Controls.Add( this.buttonBrowse );
            this.groupBox1.Controls.Add( this.listViewFiles );
            this.groupBox1.Location = new System.Drawing.Point( 8, 8 );
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size( 716, 177 );
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Files";
            // 
            // buttonReload
            // 
            this.buttonReload.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.buttonReload.Location = new System.Drawing.Point( 8, 145 );
            this.buttonReload.Name = "buttonReload";
            this.buttonReload.Size = new System.Drawing.Size( 75, 23 );
            this.buttonReload.TabIndex = 5;
            this.buttonReload.Text = "Re&load";
            this.buttonReload.Click += new System.EventHandler( this.buttonReload_Click );
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.progressBar1.Location = new System.Drawing.Point( 256, 145 );
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size( 368, 23 );
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 4;
            // 
            // buttonRemoveAll
            // 
            this.buttonRemoveAll.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.buttonRemoveAll.Location = new System.Drawing.Point( 168, 145 );
            this.buttonRemoveAll.Name = "buttonRemoveAll";
            this.buttonRemoveAll.Size = new System.Drawing.Size( 75, 23 );
            this.buttonRemoveAll.TabIndex = 3;
            this.buttonRemoveAll.Text = "Remove &All";
            this.buttonRemoveAll.Click += new System.EventHandler( this.buttonRemoveAll_Click );
            // 
            // buttonRemove
            // 
            this.buttonRemove.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.buttonRemove.Location = new System.Drawing.Point( 88, 145 );
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size( 75, 23 );
            this.buttonRemove.TabIndex = 2;
            this.buttonRemove.Text = "&Remove";
            this.buttonRemove.Click += new System.EventHandler( this.buttonRemove_Click );
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.buttonBrowse.Location = new System.Drawing.Point( 632, 145 );
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size( 75, 23 );
            this.buttonBrowse.TabIndex = 1;
            this.buttonBrowse.Text = "&Browse...";
            this.buttonBrowse.Click += new System.EventHandler( this.buttonBrowse_Click );
            // 
            // listViewFiles
            // 
            this.listViewFiles.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                        | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.listViewFiles.CheckBoxes = true;
            this.listViewFiles.Columns.AddRange( new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4} );
            this.listViewFiles.FullRowSelect = true;
            this.listViewFiles.GridLines = true;
            this.listViewFiles.HideSelection = false;
            this.listViewFiles.Location = new System.Drawing.Point( 5, 17 );
            this.listViewFiles.Name = "listViewFiles";
            this.listViewFiles.Size = new System.Drawing.Size( 704, 121 );
            this.listViewFiles.TabIndex = 0;
            this.listViewFiles.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "File";
            this.columnHeader1.Width = 374;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Base Address";
            this.columnHeader2.Width = 78;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Size";
            this.columnHeader3.Width = 78;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "TimeStamp";
            this.columnHeader4.Width = 128;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                        | System.Windows.Forms.AnchorStyles.Left )
                        | System.Windows.Forms.AnchorStyles.Right ) ) );
            this.richTextBox1.Font = new System.Drawing.Font( "Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
            this.richTextBox1.Location = new System.Drawing.Point( 8, 248 );
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size( 712, 199 );
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            // 
            // buttonAction
            // 
            this.buttonAction.Font = new System.Drawing.Font( "Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 0 ) ) );
            this.buttonAction.Location = new System.Drawing.Point( 184, 192 );
            this.buttonAction.Name = "buttonAction";
            this.buttonAction.Size = new System.Drawing.Size( 120, 48 );
            this.buttonAction.TabIndex = 2;
            this.buttonAction.Text = "&Start";
            this.buttonAction.Click += new System.EventHandler( this.buttonAction_Click );
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point( 8, 192 );
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size( 64, 23 );
            this.label1.TabIndex = 3;
            this.label1.Text = "COM Port:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBoxPort
            // 
            this.comboBoxPort.FormattingEnabled = true;
            this.comboBoxPort.Location = new System.Drawing.Point( 64, 192 );
            this.comboBoxPort.MaxDropDownItems = 20;
            this.comboBoxPort.Name = "comboBoxPort";
            this.comboBoxPort.Size = new System.Drawing.Size( 112, 21 );
            this.comboBoxPort.TabIndex = 4;
            this.comboBoxPort.SelectionChangeCommitted += new System.EventHandler( this.comboBoxPort_SelectionChangeCommitted );
            // 
            // comboBoxBaud
            // 
            this.comboBoxBaud.FormattingEnabled = true;
            this.comboBoxBaud.Items.AddRange( new object[] {
            "115200",
            "57600",
            "28800",
            "14400",
            "9600",
            "2400"} );
            this.comboBoxBaud.Location = new System.Drawing.Point( 64, 216 );
            this.comboBoxBaud.Name = "comboBoxBaud";
            this.comboBoxBaud.Size = new System.Drawing.Size( 112, 21 );
            this.comboBoxBaud.TabIndex = 5;
            this.comboBoxBaud.Text = "115200";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point( 8, 216 );
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size( 64, 23 );
            this.label2.TabIndex = 6;
            this.label2.Text = "Speed:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonClear
            // 
            this.buttonClear.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.buttonClear.Location = new System.Drawing.Point( 8, 453 );
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size( 88, 23 );
            this.buttonClear.TabIndex = 7;
            this.buttonClear.Text = "&Clear display";
            this.buttonClear.Click += new System.EventHandler( this.buttonClear_Click );
            // 
            // checkBoxWait
            // 
            this.checkBoxWait.Location = new System.Drawing.Point( 410, 192 );
            this.checkBoxWait.Name = "checkBoxWait";
            this.checkBoxWait.Size = new System.Drawing.Size( 96, 24 );
            this.checkBoxWait.TabIndex = 8;
            this.checkBoxWait.Text = "&Wait For Sync";
            // 
            // checkBoxDisconnect
            // 
            this.checkBoxDisconnect.Location = new System.Drawing.Point( 410, 216 );
            this.checkBoxDisconnect.Name = "checkBoxDisconnect";
            this.checkBoxDisconnect.Size = new System.Drawing.Size( 112, 24 );
            this.checkBoxDisconnect.TabIndex = 9;
            this.checkBoxDisconnect.Text = "Stop After &Upload";
            // 
            // radioButtonPortBooter
            // 
            this.radioButtonPortBooter.Checked = true;
            this.radioButtonPortBooter.Location = new System.Drawing.Point( 311, 197 );
            this.radioButtonPortBooter.Name = "radioButtonPortBooter";
            this.radioButtonPortBooter.Size = new System.Drawing.Size( 83, 17 );
            this.radioButtonPortBooter.TabIndex = 10;
            this.radioButtonPortBooter.Text = "&PortBooter";
            this.radioButtonPortBooter.CheckedChanged += new System.EventHandler( this.radioButtonPortBooter_CheckedChanged );
            // 
            // radioButtonTinyBooter
            // 
            this.radioButtonTinyBooter.Location = new System.Drawing.Point( 311, 219 );
            this.radioButtonTinyBooter.Name = "radioButtonTinyBooter";
            this.radioButtonTinyBooter.Size = new System.Drawing.Size( 83, 17 );
            this.radioButtonTinyBooter.TabIndex = 11;
            this.radioButtonTinyBooter.Text = "&TinyBooter";
            this.radioButtonTinyBooter.CheckedChanged += new System.EventHandler( this.radioButtonTinyBooter_CheckedChanged );
            // 
            // buttonDeployMap
            // 
            this.buttonDeployMap.Location = new System.Drawing.Point( 640, 190 );
            this.buttonDeployMap.Name = "buttonDeployMap";
            this.buttonDeployMap.Size = new System.Drawing.Size( 75, 23 );
            this.buttonDeployMap.TabIndex = 12;
            this.buttonDeployMap.Text = "&DeployMap";
            this.buttonDeployMap.Click += new System.EventHandler( this.buttonDeployMap_Click );
            // 
            // buttonRebootAndStop
            // 
            this.buttonRebootAndStop.Location = new System.Drawing.Point( 530, 191 );
            this.buttonRebootAndStop.Name = "buttonRebootAndStop";
            this.buttonRebootAndStop.Size = new System.Drawing.Size( 102, 23 );
            this.buttonRebootAndStop.TabIndex = 13;
            this.buttonRebootAndStop.Text = "R&ebootAndStop";
            this.buttonRebootAndStop.Click += new System.EventHandler( this.buttonRebootAndStop_Click );
            // 
            // buttonEraseDeployment
            // 
            this.buttonEraseDeployment.Location = new System.Drawing.Point( 530, 220 );
            this.buttonEraseDeployment.Name = "buttonEraseDeployment";
            this.buttonEraseDeployment.Size = new System.Drawing.Size( 185, 23 );
            this.buttonEraseDeployment.TabIndex = 14;
            this.buttonEraseDeployment.Text = "Erase Deployment";
            this.buttonEraseDeployment.Click += new System.EventHandler( this.buttonEraseDeployment_Click );
            // 
            // buttonPing
            // 
            this.buttonPing.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.buttonPing.Location = new System.Drawing.Point( 102, 453 );
            this.buttonPing.Name = "buttonPing";
            this.buttonPing.Size = new System.Drawing.Size( 88, 23 );
            this.buttonPing.TabIndex = 15;
            this.buttonPing.Text = "Ping";
            this.buttonPing.Click += new System.EventHandler( this.buttonPing_Click );
            // 
            // buttonCLRCap
            // 
            this.buttonCLRCap.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left ) ) );
            this.buttonCLRCap.Location = new System.Drawing.Point( 196, 453 );
            this.buttonCLRCap.Name = "buttonCLRCap";
            this.buttonCLRCap.Size = new System.Drawing.Size( 88, 23 );
            this.buttonCLRCap.TabIndex = 15;
            this.buttonCLRCap.Text = "CLR Capabilities";
            this.buttonCLRCap.Click += new System.EventHandler( this.buttonCLRCap_Click );
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size( 736, 485 );
            this.Controls.Add( this.buttonCLRCap );
            this.Controls.Add( this.buttonPing );
            this.Controls.Add( this.buttonEraseDeployment );
            this.Controls.Add( this.buttonRebootAndStop );
            this.Controls.Add( this.buttonDeployMap );
            this.Controls.Add( this.radioButtonTinyBooter );
            this.Controls.Add( this.radioButtonPortBooter );
            this.Controls.Add( this.checkBoxDisconnect );
            this.Controls.Add( this.checkBoxWait );
            this.Controls.Add( this.buttonClear );
            this.Controls.Add( this.comboBoxBaud );
            this.Controls.Add( this.label2 );
            this.Controls.Add( this.comboBoxPort );
            this.Controls.Add( this.label1 );
            this.Controls.Add( this.buttonAction );
            this.Controls.Add( this.richTextBox1 );
            this.Controls.Add( this.groupBox1 );
            this.MinimumSize = new System.Drawing.Size( 504, 328 );
            this.Name = "Form1";
            this.Text = "PortBooter Client";
            this.Load += new System.EventHandler( this.Form1_Load );
            this.groupBox1.ResumeLayout( false );
            this.ResumeLayout( false );

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.Run(new Form1());
        }


        void Stop()
        {
            if(m_worker != null)
            {
                m_worker.Abort();
                m_worker.Join ();
                m_worker = null;
            }

            if(m_fl != null)
            {
                m_fl.Stop();
                m_fl = null;
            }

            if(m_eng != null)
            {
                m_eng.Stop();
                m_eng = null;
            }
        }


        protected override void OnClosed(EventArgs e)
        {
            Stop();

            base.OnClosed(e);
        }

        delegate void UpdateStatusCallback();

        void GenericCallback( System.Delegate callback, params object[] list )
        {
            this.Invoke( callback, list );
        }


        delegate void NewTextCallback( string text );

        NewTextCallback m_callback_NewText;

        void Callback_NewText( string text )
        {
            if(text != null && text.Length > 0)
            {
                richTextBox1.AppendText( text );
                richTextBox1.ScrollToCaret();
            }
        }

        void NewText( string text )
        {
            GenericCallback( m_callback_NewText, text );
        }


        delegate void ProgressCallback( _DBG.SRecordFile.Block bl, int offset, bool fLast );

        ProgressCallback m_callback_Progress;

        void Callback_Progress( _DBG.SRecordFile.Block bl, int offset, bool fLast )
        {
            foreach(ListViewItem item in this.listViewFiles.CheckedItems)
            {
                if( item.Tag == bl && fLast == false )
                {
                    item.Selected = true;
                }
                else 
                {
                    item.Selected = false;
                }
            }

            progressBar1.Value = fLast ? 0 : Math.Min( (int)(offset * 100 / bl.data.Length), 100 );
        }

        void OnProgress( _DBG.SRecordFile.Block bl, int offset, bool fLast )
        {
            GenericCallback( m_callback_Progress, bl, offset, fLast );
        }


        string m_last = null;

        void OnNoise( byte[] buf, int offset, int count )
        {
            AppendText( Encoding.ASCII.GetString( buf, offset, count ) );
        }

        void OnMessage( _DBG.WireProtocol.IncomingMessage msg, string text )
        {
            AppendText( text );
        }

        void AppendText( string text )
        {
            int idx;

            m_last += text;

            while((idx = m_last.IndexOf( '\n' )) >= 0)
            {
                this.NewText( m_last.Substring( 0, idx ) );

                m_last = m_last.Substring( idx + 1 );
            }
        }


        private ArrayList GeneratePortList()
        {
            ArrayList ports = _DBG.PortDefinition.Enumerate( _DBG.PortFilter.Serial, _DBG.PortFilter.Usb );

            foreach (string ip in Settings.Default.TcpIpAddresses.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                _DBG.PortDefinition pd = _DBG.PortDefinition.CreateInstanceForTcp(ip);
                if (pd != null)
                {
                    ports.Add(pd);
                }
            }

            return ports;
        }


        private void UploadWithPortBooter()
        {
            m_eng.OnNoise -= new _DBG.NoiseEventHandler( OnNoise );

            if(m_fWait)
            {
                m_fl.WaitBanner( Int32.MaxValue, 2000 );
            }

            m_fl.Program( m_blocks );

            uint address = uint.MaxValue;

            foreach(_DBG.SRecordFile.Block bl in m_blocks)
            {
                if(bl.executable)
                {
                    address = bl.address;
                }
            }

            if(address != uint.MaxValue)
            {
                m_fl.Execute( address );
            }

            m_eng.OnNoise += new _DBG.NoiseEventHandler( OnNoise );

            if(m_fDisconnect)
            {
                this.Invoke( new Callback_Bool( this.StartOrStop), new object[] { false } );
            }
        }

        private void UploadWithTinyBooter()
        {
            m_eng.OnNoise -= new _DBG.NoiseEventHandler( OnNoise );

            foreach(_DBG.SRecordFile.Block bl in m_blocks )
            {
                long offset   = 0;
                uint location = bl.address;
                byte[] buf    = bl.data.ToArray();
                long count    = bl.data.Length;

                OnProgress( bl, (int)offset, false );

                byte[] chunk = new byte[4096];

                while(count != 0)
                {
                    long length = System.Math.Min( 4096, count );

                    if(length != 4096)
                    {
                        chunk = new byte[length];
                    }

                    Array.Copy( buf, offset, chunk, 0, length );

                    if(!m_eng.WriteMemory( location, chunk ))
                    {
                        return;
                    }

                    location += (uint)length;                    
                    offset   +=       length;
                    count    -=       length;

                    OnProgress( bl, (int)offset, false );
                }
                if(!m_eng.CheckSignature( bl.signature, 0 ))
                {
                    this.NewText( "Check Signature failed!" );
                    
                    return;
                }
                
                OnProgress( bl, (int)count, true );
            }
            
            uint address = uint.MaxValue;

            foreach(_DBG.SRecordFile.Block bl in m_blocks)
            {
                if(bl.executable)
                {
                    address = bl.address;
                }
            }

            if(address != uint.MaxValue)
            {
                m_eng.ExecuteMemory( address );
            }

            m_eng.OnNoise += new _DBG.NoiseEventHandler( OnNoise );

            if(m_fDisconnect)
            {
                this.Invoke( new Callback_Bool( this.StartOrStop), new object[] { false } );
            }
        }

        public void OnDeviceChanged( _DBG.UsbDeviceDiscovery.DeviceChanged change )
        {
            // lets make this thread safe
            comboBoxPort.Invoke( (MethodInvoker)delegate{ 
                IList ports = GeneratePortList();
                comboBoxPort.DataSource = ports;
                comboBoxPort.SelectedItem = HandleDisplayDevice( ports );
            });
        }


        private string ParseSignature( string fullpath )
        {
            try
            {
                // retrieve the signature file
                FileInfo fi = new FileInfo( fullpath );
                int extIndex = fullpath.Length;
                
                if(!fi.Extension.Equals( "" ))
                {
                    extIndex = fullpath.LastIndexOf( fi.Extension );
                }

                string sigFile = fullpath.Substring( 0, extIndex ) + ".sig"; 

                if(!File.Exists( sigFile ))
                {
                    OpenFileDialog dlg = new OpenFileDialog();

                    dlg.Filter          = "Signature Files (*.sig)|*.sig|All files (*.*)|*.*";
                    dlg.FilterIndex     = 1;
                    dlg.Multiselect     = false;
                    dlg.CheckFileExists = true;
                    dlg.CheckPathExists = true;

                    if(dlg.ShowDialog() == DialogResult.OK)
                    {
                        sigFile = dlg.FileName;
                    }
                    else
                    {
                        sigFile = null;
                    }
                }
                
                return sigFile; 
             
            }
            catch(Exception ex)
            {
                MessageBox.Show( String.Format( "{0}", ex.ToString() ) );
                return null;
            }
        }

        private void ParseFile( string fullpath, bool retrieveSignature )
        {
            try
            {
                // parse signature
                string sigFile = null;
                if(retrieveSignature)
                {
                    sigFile = ParseSignature( fullpath );

                    if(sigFile == null)
                    {
                        throw new InvalidOperationException( "TinyBooter upload requires a signature file" );
                    }
                }

                // parse the data file
                ArrayList blocks     = new ArrayList();
                uint      entrypoint = Microsoft.SPOT.Debugger.SRecordFile.Parse( fullpath, blocks, sigFile );

                DateTime dt = File.GetLastWriteTime( fullpath );

                foreach(_DBG.SRecordFile.Block bl in blocks)
                {
                    ListViewItem item = new ListViewItem( fullpath.ToLower() );

                    item.Tag     = bl;
                    item.Checked = true;
                    item.SubItems.Add( String.Format( "0x{0:X8}", bl.address     ) );
                    item.SubItems.Add( String.Format( "0x{0:X8}", bl.data.Length ) );
                    item.SubItems.Add( String.Format( "{0}"     , dt             ) );

                    this.listViewFiles.Items.Add( item );
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show( String.Format( "{0}", ex.ToString() ) );
                return;
            }
        }


        private void buttonRemove_Click(object sender, System.EventArgs e)
        {
            foreach(ListViewItem item in this.listViewFiles.SelectedItems)
            {
                item.Remove();
            }
        }

        private void buttonRemoveAll_Click(object sender, System.EventArgs e)
        {
            this.listViewFiles.Items.Clear();
        }

        private void buttonReload_Click(object sender, System.EventArgs e)
        {
            Hashtable files = new Hashtable();

            foreach(ListViewItem item in this.listViewFiles.Items)
            {
                files[item.Text] = item;
            }

            this.listViewFiles.Items.Clear();

            foreach(string fullpath in files.Keys)
            {
                ParseFile( fullpath, radioButtonTinyBooter.Checked );
            }
        }

        private void buttonBrowse_Click(object sender, System.EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Filter          = "Flash Files (*.hex)|*.hex; ER*|All files (*.*)|*.*";
            dlg.FilterIndex     = 1;
            dlg.Multiselect     = true;
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            
            if(dlg.ShowDialog() == DialogResult.OK)
            {
                foreach(string fullpath in dlg.FileNames)
                {
                    ParseFile( fullpath, radioButtonTinyBooter.Checked );
                }
            }
        }

        private void buttonAction_Click(object sender, System.EventArgs e)
        {
            StartOrStop( m_eng == null );
        }

        private _DBG.PortDefinition GetSelectedPortDefinition()
        {
            _DBG.PortDefinition pd = comboBoxPort.SelectedItem as _DBG.PortDefinition;

            if (pd == null)
            {
                string hostname = (comboBoxPort.SelectedItem != null? comboBoxPort.SelectedItem as string: comboBoxPort.Text);

                pd = _DBG.PortDefinition.CreateInstanceForTcp(hostname);

                if (pd != null)
                {
                    ArrayList ips = new ArrayList(Settings.Default.TcpIpAddresses.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

                    if (!ips.Contains(hostname))
                    {
                        IList ports = GeneratePortList();

                        Settings.Default.TcpIpAddresses += hostname + ";";
                        Settings.Default.Save();

                        int idx = ports.Add(pd);
                        comboBoxPort.DataSource = ports;
                        comboBoxPort.SelectedIndex = idx;
                    }
                }
            }

            if (pd is _DBG.PortDefinition_Serial)
            {
                _DBG.PortDefinition_Serial pds = (_DBG.PortDefinition_Serial)pd;

                pds.BaudRate = uint.Parse((string)comboBoxBaud.SelectedItem);
            }

            return pd;
        }
        
        private void StartOrStop( bool fStart ) 
        {
            bool fButtonState;

            if(fStart)
            {
                if(m_eng != null) return;

                m_blocks      = new ArrayList();
                m_fWait       = checkBoxWait.Checked;
                m_fDisconnect = checkBoxDisconnect.Checked;

                listViewFiles.SelectedIndices.Clear();

                try
                {
                    _DBG.PortDefinition pd = GetSelectedPortDefinition();
                    
                    m_eng = new _DBG.Engine( pd );

                    m_eng.Silent = true;

                    m_eng.OnNoise   += new _DBG.NoiseEventHandler  ( OnNoise   );
                    m_eng.OnMessage += new _DBG.MessageEventHandler( OnMessage );

                    m_eng.Start();

                    if (m_eng.TryToConnect(5, 100))
                    {
                        if (m_fWait)
                        {
                            // if w are talking to the booter, have it to stop and wait for upload
                            // PortBooter will wait to secs no matter what
                            m_eng.RebootDevice(radioButtonPortBooter.Checked ? _DBG.Engine.RebootOption.EnterBootloader : _DBG.Engine.RebootOption.NormalReboot);
                        }
                    }
                    


                    foreach(ListViewItem item in this.listViewFiles.CheckedItems)
                    {
                        m_blocks.Add( item.Tag );
                    }

                    if(m_blocks.Count > 0)
                    {
                        if(radioButtonPortBooter.Checked)
                        {   
                            m_fl = new _DBG.PortBooter( m_eng );

                            m_fl.OnProgress += new _DBG.PortBooter.ProgressEventHandler( this.OnProgress );

                            m_fl.Start();

                            m_worker = new Thread( new ThreadStart( this.UploadWithPortBooter ) );
                        }
                        else
                        {
                            m_worker = new Thread( new ThreadStart( this.UploadWithTinyBooter ) );
                        }

                        m_worker.Start();
                    }

                    buttonAction.Text = "Stop";
                    richTextBox1.Focus();
                }
                catch(Exception ex)
                {
                    MessageBox.Show( ex.Message );
                    return;
                }

                fButtonState = false;
            }
            else
            {
                if(m_eng == null) return;

                Stop();

                buttonAction.Text = "Start";

                fButtonState = true;
            }

            buttonReload         .Enabled = fButtonState;
            buttonRemove         .Enabled = fButtonState;
            buttonRemoveAll      .Enabled = fButtonState;
            buttonBrowse         .Enabled = fButtonState;
            comboBoxPort         .Enabled = fButtonState;
            comboBoxBaud         .Enabled = fButtonState;
            listViewFiles        .Enabled = fButtonState;
        }

        private void buttonClear_Click(object sender, System.EventArgs e)
        {
            richTextBox1.Clear();
        }
        
        private void comboBoxPort_SelectionChangeCommitted(object sender, System.EventArgs e)
        {
            m_lastValid = GetSelectedPortDefinition() as _DBG.PortDefinition;
            comboBoxBaud.Enabled = (m_lastValid is _DBG.PortDefinition_Serial);
        }

        private _DBG.PortDefinition HandleDisplayDevice( IList ports )
        {
            foreach( _DBG.PortDefinition pd in ports)
            {
                object uniqueId = m_lastValid.UniqueId;

                if (Object.Equals(pd.UniqueId, uniqueId)) return pd;
            }

            return  null;
        }

        private void radioButtonPortBooter_CheckedChanged(object sender, EventArgs e)
        {
            bool fChecked = radioButtonPortBooter.Checked;

            if(fChecked)
            {
                radioButtonTinyBooter.Checked = false;
            }
        }

        private void radioButtonTinyBooter_CheckedChanged(object sender, EventArgs e)
        {            
            bool fChecked = radioButtonTinyBooter.Checked;

            if(fChecked)
            {
                radioButtonPortBooter.Checked = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns the previous conneciton state of the debugger engine</returns>
        private bool EnsureDebuggerConnection()
        {
            if( m_eng == null )
            {
                _DBG.PortDefinition pd = GetSelectedPortDefinition();

                m_eng = new _DBG.Engine( pd );

                m_eng.OnNoise += new _DBG.NoiseEventHandler( OnNoise );
                m_eng.OnMessage += new _DBG.MessageEventHandler( OnMessage );

                m_eng.Start();

                m_eng.TryToConnect(5, 100);

                return false;
            }

            // TryToConnect calls GetCLRCapabilities which prevents a bunch of asserts
            m_eng.TryToConnect(5, 100);

            return true;
        }

        private void buttonDeployMap_Click( object sender, EventArgs e )
        {
            bool bWasStarted = EnsureDebuggerConnection();

            Microsoft.SPOT.Debugger.WireProtocol.Commands.Monitor_DeploymentMap.Reply deployMap = m_eng.DeploymentMap();
            if( deployMap != null )
            {
                NewText( "Deployment Map\r\n" );
                for(int i=0; i<deployMap.m_count; i++ )
                {
                    NewText( "Assembly " + i.ToString() + ":\r\n" );
                    NewText( string.Format( "\tAddress: 0x{0:x}\r\n", deployMap.m_map[i].m_address ) );
                    NewText( string.Format( "\tCRC:     0x{0:x}\r\n", deployMap.m_map[i].m_CRC ) );
                    NewText( string.Format( "\tSize:    0x{0:x}\r\n", deployMap.m_map[i].m_size ) );
                }
                if( deployMap.m_count == 0 )
                {
                    NewText( "No assemblies in deployment sector\r\n" );
                }
            }
            else
            {
                NewText( "Nothing in deployment sector - or - deployment map not supported\r\n" );
            }
            if( !bWasStarted )
            {
                m_eng.Stop();
                m_eng = null;
            }
        }


        private void buttonCLRCap_Click( object sender, EventArgs e )
        {
            bool bWasStarted = EnsureDebuggerConnection();

            Microsoft.SPOT.CLRCapabilities CLRCapabilities = m_eng.Capabilities;
            NewText( "CLR Capabilities\r\n" );
            if (CLRCapabilities.FloatingPoint) 
            {
                NewText( "Floating Point \r\n" ); 
            }

            if (CLRCapabilities.SourceLevelDebugging)
            {
                NewText( "Source Level Debugging \r\n" ); 
            }

            if (CLRCapabilities.AppDomains)
            {
                NewText( "Application Domain \r\n" ); 
            }

            if (CLRCapabilities.ExceptionFilters)
            {
                NewText( "Exception filter\r\n" ); 
            }

            if (CLRCapabilities.IncrementalDeployment)
            {
                NewText( "Incremental Deployment \r\n" ); 
            }

            if (CLRCapabilities.SoftReboot)
            {
                NewText( "Soft Reboot \r\n" ); 
            }

            NewText( "Done\r\n" );                 
            if( !bWasStarted )
            {
                m_eng.Stop();
                m_eng = null;
            }
        }

        private void buttonRebootAndStop_Click( object sender, EventArgs e )
        {
            bool bWasStarted = EnsureDebuggerConnection( );

            m_eng.RebootDevice(_DBG.Engine.RebootOption.EnterBootloader);

            if(!bWasStarted)
            {
                m_eng.Stop();
                m_eng = null;
            }
        }

        private void buttonEraseDeployment_Click( object sender, EventArgs e )
        {
            bool bWasStarted = EnsureDebuggerConnection();
            Cursor old = Cursor.Current;
            Cursor = Cursors.WaitCursor;
            buttonEraseDeployment.Text = "Erasing...";
            buttonEraseDeployment.Update();
            NewText( "Erasing Deployment Sector...\r\n" );

            try
            {
                _DBG.WireProtocol.Commands.Monitor_Ping.Reply ping = m_eng.GetConnectionSource();
                if(ping == null) 
                {
                    NewText("Unable to connect to device\r\n");
                    return;
                }
                
                bool fClrConnection = ping.m_source == _DBG.WireProtocol.Commands.Monitor_Ping.c_Ping_Source_TinyCLR;

                if (fClrConnection)
                {
                    m_eng.PauseExecution();
                }

                _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.Reply status = m_eng.GetFlashSectorMap() as _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.Reply;

                if (status == null)
                {
                    NewText( "Erase Deployment may Not be supported on this device build\r\n" );                
                }
                else
                {
                    const uint c_deployFlag = _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_DEPLOYMENT;
                    const uint c_usageMask  = _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK;

                    foreach( _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.FlashSectorData sector in status.m_map)
                    {
                        if (c_deployFlag == (c_usageMask & sector.m_flags))
                        {
                            NewText(string.Format("Erasing sector at 0x{0:x08}\r\n", sector.m_address));
                            m_eng.EraseMemory(sector.m_address, sector.m_size);
                        }
                    }

                    if (fClrConnection)
                    {
                        m_eng.RebootDevice(_DBG.Engine.RebootOption.RebootClrOnly);
                    }
                    NewText("Erase Deployment Successfull");
                }
            }
            catch( Exception ex )
            {
                NewText( "Exception: " + ex.Message + "\r\n" );
            }
            finally
            {
                buttonEraseDeployment.Text = "Erase Deployment";

                Cursor = old;
                if( !bWasStarted )
                {
                    m_eng.Stop();
                    m_eng = null;
                }
            }
        }

        private void Form1_Load( object sender, EventArgs e )
        {
            comboBoxPort.DisplayMember = "DisplayName";
            comboBoxPort.DataSource = GeneratePortList();
            m_lastValid = comboBoxPort.SelectedItem as _DBG.PortDefinition;
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        private void buttonPing_Click(object sender, EventArgs e)
        {
            bool bWasStarted = EnsureDebuggerConnection();
            _DBG.WireProtocol.Commands.Monitor_Ping.Reply reply;
            reply = m_eng.GetConnectionSource();
            if (reply != null)
            {
                switch (reply.m_source)
                {
                    case _DBG.WireProtocol.Commands.Monitor_Ping.c_Ping_Source_TinyCLR:
                        NewText("Connected to TinyCLR\r\n");
                        break;
                    case _DBG.WireProtocol.Commands.Monitor_Ping.c_Ping_Source_TinyBooter:
                        NewText("Connected to TinyBooter\r\n");
                        break;
                    default:
                        NewText("Connected to unknown source\r\n");
                        break;
                }
            }
            else
            {
                NewText("No connection\r\n");
            }
            if (!bWasStarted)
            {
                m_eng.Stop();
                m_eng = null;
            }
        }
    }
}

