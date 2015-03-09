//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.PlugIns;
using System.Reflection;
using System.Configuration;
using System.Threading;
using System.Security;
using _DBG = Microsoft.SPOT.Debugger;
using System.Security.Cryptography.X509Certificates;


namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
    public partial class Form1 : Form, IMFDeployForm 
    {
        private MFDeploy m_deploy = new MFDeploy();
        private Thread m_pluginThread = null;
        private bool m_fShuttingDown = false;
        private MFPlugInMenuItem m_currentPlugInObject;
        private ToolStripMenuItem m_defaultSerialPort;

        private ToolStripMenuItem m_pluginMenu = new ToolStripMenuItem(Properties.Resources.ToolStripMenuPlugIn);

        private MFDevice m_device = null;
        private int m_deviceRefCount = 0;

        private List<MFPlugInMenuItem> m_plugins = new List<MFPlugInMenuItem>();

        private Hashtable m_FileHash = new Hashtable();

        public Form1()
        {
            InitializeComponent();
            enalbePermiscuousWinUSBToolStripMenuItem.Checked = m_deploy.EnablePermiscuousWinUsb;
        }

        #region IFormMethods
        // IForm methods
        public ReadOnlyCollection<string> Files
        {
            get
            {
                ReadOnlyCollection<string> files = null;

                comboBoxDevice.Invoke((MethodInvoker)delegate
                {
                    Collection<string> selFiles = new Collection<string>();

                    foreach(ListViewItem lvi in listViewFiles.Items)
                    {
                        if(lvi.Checked)
                        {
                            selFiles.Add(lvi.SubItems[1].Text);
                        }
                    }
                    files = new ReadOnlyCollection<string>(selFiles);
                });

                return files;
            }
        }

        public void DumpToOutput(string text)
        {
            DumpToOutput(text, true);
        }

        public bool m_fWaitForNewline = false;
        public void DumpToOutput(string text, bool newLine)
        {
            if (!m_fShuttingDown)
            {
                richTextBoxOutput.Invoke((MethodInvoker)delegate
                {
                    string timestamp = "[" + DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToShortDateString() + "] ";

                    if (timeStampToolStripMenuItem.Checked && !m_fWaitForNewline)
                    {
                        richTextBoxOutput.AppendText(timestamp);
                    }
                    newLine = newLine || text.EndsWith("\n");
                    m_fWaitForNewline = !newLine;

                    if (timeStampToolStripMenuItem.Checked) richTextBoxOutput.AppendText(text.TrimEnd('\r','\n').Replace("\n", "\n" + timestamp));
                    else                                    richTextBoxOutput.AppendText(text.TrimEnd('\r','\n'));
                    if (newLine)                            richTextBoxOutput.AppendText("\r\n");

                    richTextBoxOutput.ScrollToCaret();
                });
            }
        }
        #endregion // IForm Methods

        private MFPortDefinition m_transport = null;
        private MFPortDefinition m_transportTinyBooter = null;

        public MFPortDefinition Transport
        {
            set { m_transport = value; }
            get { return m_transport; }
        }

        public MFPortDefinition TransportTinyBooter
        {
            set { m_transportTinyBooter = value; }
            get { return m_transportTinyBooter;  }
        }

        private enum TransportComboBoxType
        {
            Serial = 0,
            Usb,
            TcpIp,
        }


        private ReadOnlyCollection<MFPortDefinition> GeneratePortList()
        {
            ReadOnlyCollection<MFPortDefinition> list = null;

            switch (comboBoxTransport.SelectedIndex)
            {
                case (int)TransportComboBoxType.Usb:
                    list = m_deploy.EnumPorts(TransportType.USB);
                    break;

                case (int)TransportComboBoxType.Serial:
                    list = m_deploy.EnumPorts(TransportType.Serial);
                    break;

                case (int)TransportComboBoxType.TcpIp:
                    Cursor old = Cursor.Current;
                    Cursor.Current = Cursors.WaitCursor;
                    list = m_deploy.EnumPorts(TransportType.TCPIP);
                    Cursor.Current = old;
                    break;
            }

            return list;
        }

        private void OnDeviceListUpdate(object sender, EventArgs e)
        {
            comboBoxDevice.Invoke((MethodInvoker)delegate
            {
                ReadOnlyCollection<MFPortDefinition> list = GeneratePortList();
                comboBoxDevice.DataSource = null;
                if ((list != null) && (list.Count > 0))
                {
                    comboBoxDevice.DataSource = list;
                    if (list[0] is MFPortDefinition)
                    {
                        comboBoxDevice.DisplayMember = "Name";
                    }
                    comboBoxDevice.SelectedIndex = 0;
                }
                comboBoxDevice.Update();
            });
        }

        // MF PlugIn Menu click
        private void OnMenuClick(object sender, EventArgs ea)
        {
            ToolStripItem tsi = sender as ToolStripItem;
            if (tsi != null)
            {
                MFPlugInMenuItem mfdp = tsi.Tag as MFPlugInMenuItem;
                if (m_pluginThread == null || !m_pluginThread.IsAlive)
                {
                    m_currentPlugInObject = mfdp;

                    // Modal dialogs need to run on the same thread
                    if (!mfdp.RunInSeparateThread)
                    {
                        OnPlugInExecuteThread();
                    }
                    else
                    {
                        m_pluginThread = new Thread(new ThreadStart(OnPlugInExecuteThread));
                        m_pluginThread.SetApartmentState(ApartmentState.STA);
                        m_pluginThread.Start();
                    }
                }
                else
                {
                    DumpToOutput(Properties.Resources.WarningPlugInPending);
                }
            }
        }

        private void OnDefaultSerialPortChange(object sender, EventArgs ea)
        {
            ToolStripMenuItem tsi = sender as ToolStripMenuItem;
            if (tsi != null)
            {
                MFSerialPort port = tsi.Tag as MFSerialPort;
                if (port != null)
                {
                    m_defaultSerialPort.Checked = false;
                    Properties.Settings.Default.DefaultSerialPort = port.Name;
                    tsi.Checked = true;
                    m_defaultSerialPort = tsi;
                    m_transportTinyBooter = port;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBoxTransport.SelectedIndex = 0;

            if (m_transport != null)
            {
                comboBoxDevice.Text = m_transport.Name;
            }

            richTextBoxOutput.ScrollToCaret();

            foreach (MFSerialPort pd in m_deploy.EnumPorts(TransportType.Serial))
            {
                ToolStripMenuItem ti = new ToolStripMenuItem(pd.Name);
                ti.Tag = pd;
                ti.Click += new EventHandler(OnDefaultSerialPortChange);

                defaultSerialPortToolStripMenuItem.DropDownItems.Add(ti);

                if (Properties.Settings.Default.DefaultSerialPort == pd.Name)
                {
                    m_defaultSerialPort = ti;
                    ti.Checked = true;

                    m_transportTinyBooter = pd;
                }
            }


            AddPlugIns(typeof(Debug.DebugPlugins).GetNestedTypes(), "Debug");

            foreach (string file in Properties.Settings.Default.MRUFiles)
            {
                bool fAddFiles = true;
                // validate the file set
                foreach (string f in file.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string f1 = f.Trim();
                    if (f1.Length > 0 && !File.Exists(f1))
                    {
                        fAddFiles = false;
                    }
                }
                if (fAddFiles) comboBoxImageFile.Items.Add(file);
            }

            try   { m_deploy.DiscoveryMulticastAddress     = System.Net.IPAddress.Parse(Properties.Settings.Default.DiscoveryMulticastAddress); }
            catch { richTextBoxOutput.AppendText(string.Format(Properties.Resources.ErrorAppSettings, "DiscoveryMulticastAddress")); }
            
            m_deploy.DiscoveryMulticastPort  = Properties.Settings.Default.DiscoveryMulticastPort;
            m_deploy.DiscoveryMulticastToken = Properties.Settings.Default.DiscoveryMulticastToken;

            try   { m_deploy.DiscoveryMulticastAddressRecv = System.Net.IPAddress.Parse(Properties.Settings.Default.DiscoveryMulticastAddressRecv); }
            catch { richTextBoxOutput.AppendText(string.Format(Properties.Resources.ErrorAppSettings, "DiscoveryMulticastAddressRecv")); }
            
            m_deploy.DiscoveryMulticastTimeout = Properties.Settings.Default.DiscoveryMulticastTimeout;
            m_deploy.DiscoveryTTL              = Properties.Settings.Default.DiscoveryTTL;

            try
            {
                string dir = Properties.Settings.Default.PlugIns;
                if (dir != null && dir.Length > 0)
                {
                    string plugin_dir = AppDomain.CurrentDomain.BaseDirectory + dir;

                    if (Directory.Exists(plugin_dir))
                    {
                        foreach (string file in Directory.GetFiles(plugin_dir, "*.dll"))
                        {
                            FileInfo fi = new FileInfo(file);

                            try
                            {
                                Assembly asm = Assembly.LoadFrom(file);

                                AddPlugIns(asm.GetTypes(), fi.Name.Substring(0, fi.Name.Length - 4));
                            }
                            catch
                            {
                                DumpToOutput(string.Format(Properties.Resources.ErrorUnableToInstallPlugIn, fi.Name));
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        void OnCancelMenuClick(object sender, EventArgs e)
        {
            cancelToolStripMenuItem1_Click_1(sender, e);
        }

        private int AddPlugIns(Type []types, string menuName)
        {
            int cnt = 0;

            if (!menuStrip1.Items.Contains(m_pluginMenu))
            {
                menuStrip1.Items.Insert(3, m_pluginMenu);
                m_pluginMenu.DropDownItems.Add(toolStripSeparator1);
                ToolStripItem tsi = m_pluginMenu.DropDownItems.Add(Properties.Resources.MenuItemCancel);
                tsi.Click += new EventHandler(OnCancelMenuClick);
            }

            ToolStripMenuItem parentMenu = new ToolStripMenuItem(menuName);
            m_pluginMenu.DropDownItems.Insert(0,parentMenu);

            foreach (Type t in types)
            {
                if (t.IsSubclassOf(typeof(MFPlugInMenuItem)))
                {
                    ConstructorInfo ci = t.GetConstructor(new Type[0] { });
                    MFPlugInMenuItem plugin = ci.Invoke(new object[0] { }) as MFPlugInMenuItem;
                    if (plugin != null)
                    {
                        m_plugins.Add(plugin);
                        ToolStripMenuItem tsi = new ToolStripMenuItem(plugin.Name);
                        tsi.Tag = plugin;
                        tsi.Click += new EventHandler(OnMenuClick);
                        parentMenu.DropDownItems.Insert(0,tsi);

                        cnt++;

                        // any plugin submenu items?
                        if (plugin.Submenus != null)
                        {
                            foreach (MFPlugInMenuItem mi in plugin.Submenus)
                            {
                                ToolStripMenuItem ti = new ToolStripMenuItem(mi.Name);
                                ti.Tag = mi;
                                ti.Click += new EventHandler(OnMenuClick);

                                tsi.DropDownItems.Add(ti);
                            }
                        }
                    }
                }
            }
            return cnt;
        }

        private MFPortDefinition GetSelectedItem()
        {
            if (comboBoxDevice.SelectedItem == null && comboBoxDevice.Text.Length == 0) return null;

            MFPortDefinition port = comboBoxDevice.SelectedItem as MFPortDefinition;

            if (port == null)
            {
                ArgumentParser parser = new ArgumentParser();
                string err;

                if (parser.ValidateArgs("/I:TcpIp:" + comboBoxDevice.Text, out err))
                {
                    port = parser.Interface;
                }
            }

            if (port == null)
            {
                MessageBox.Show(Properties.Resources.ErrorInvalidDevice, Properties.Resources.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBoxDevice.SelectionStart = 0;
                comboBoxDevice.SelectionLength = comboBoxDevice.Text.Length;
                comboBoxDevice.Focus();
            }
            return port;
        }

        private MFDevice ConnectToSelectedDevice()
        {
            if (m_device != null)
            {
                Interlocked.Increment(ref m_deviceRefCount);
            }
            else
            {
                MFPortDefinition port = null;
                this.Invoke((MethodInvoker)delegate
                {
                    port = GetSelectedItem();
                });

                if (port != null)
                {
                    try
                    {
                        m_deploy.OpenWithoutConnect = true;
                        
                        m_device = m_deploy.Connect(port, port is MFTcpIpPort ? m_transportTinyBooter : null);

                        m_deploy.OpenWithoutConnect = false;

                        if (m_device != null)
                        {
                            m_device.OnDebugText += new EventHandler<DebugOutputEventArgs>(OnDbgTxt);

                            if (checkBoxUseSSL.Checked)
                            {
                                string certFile = textBoxCert.Text;

                                if (!Path.IsPathRooted(certFile))
                                {
                                    certFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, certFile);
                                }

                                if (!File.Exists(certFile))
                                {
                                    MessageBox.Show(this, "The certificate '" + textBoxCert.Text + "' could not be found.", "MFDeploy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                    m_device.Disconnect();
                                    return null;
                                }
                                try
                                {
                                    X509Certificate2 cert = new X509Certificate2(certFile, textBoxCertPwd.Text);

                                    m_device.UseSsl(cert, Properties.Settings.Default.SslRequireClientCert);
                                }
                                catch
                                {
                                    MessageBox.Show(this, "Invalid password or certificate!", "MFDeploy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    m_device.Disconnect();
                                    return null;
                                }
                            }

                            comboBoxTransport.Invoke((MethodInvoker)delegate
                            {
                                comboBoxDevice.Enabled = false;
                                comboBoxTransport.Enabled = false;
                                connectToolStripMenuItem.Enabled = false;
                            });
                            Interlocked.Increment(ref m_deviceRefCount);
                        }
                    }
                    catch (Exception exc)
                    {
                        DumpToOutput(Properties.Resources.ErrorPrefix + exc.Message);
                    }
                }
            }
            return m_device;
        }

        private bool DisconnectFromSelectedDevice()
        {
            bool fDisconnected = (m_device == null);
            if (m_device != null)
            {
                if (Interlocked.Decrement(ref m_deviceRefCount) <= 0)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        connectToolStripMenuItem.Checked = false;
                        comboBoxDevice.Enabled = true;
                        comboBoxTransport.Enabled = true;
                        connectToolStripMenuItem.Enabled = true;
                    });

                    fDisconnected = true;

                    m_device.OnDebugText -= new EventHandler<DebugOutputEventArgs>(OnDbgTxt);
                    m_device.Dispose();
                    m_device = null;
                }
            }
            else
            {
                this.Invoke((MethodInvoker)delegate
                {
                    connectToolStripMenuItem.Checked = false;
                    comboBoxDevice.Enabled = true;
                    comboBoxTransport.Enabled = true;
                    connectToolStripMenuItem.Enabled = true;
                });
            }

            return fDisconnected;
        }

        private void buttonPing_Click(object sender, EventArgs e)
        {
            DumpToOutput(Properties.Resources.StatusPinging, false);
            Thread.Sleep(0);

            Cursor old = Cursor;
            this.Cursor = Cursors.WaitCursor;

            MFDevice dev = ConnectToSelectedDevice();
            if (dev != null)
            {
                DumpToOutput(dev.Ping().ToString());
                
                OemMonitorInfo omi = dev.GetOemMonitorInfo();
                if ( omi != null && omi.Valid )
                    DumpToOutput(omi.ToString());
                DisconnectFromSelectedDevice();
            }
            Cursor = old;
        }

        private void buttonErase_Click(object sender, EventArgs e)
        {
            MFDevice dev = ConnectToSelectedDevice();
            if (dev != null)
            {
                EraseDialog ed = new EraseDialog(dev);
                ed.StartPosition = FormStartPosition.CenterParent;

                if (DialogResult.OK == ed.ShowDialog(this))
                {
                    DeploymentStatusDialog dlg = new DeploymentStatusDialog(dev, ed.EraseBlocks);
                    dlg.StartPosition = FormStartPosition.CenterParent;
                    dlg.ShowDialog(this);
                }

                DisconnectFromSelectedDevice();
            }
        }

        // remove a file set from the image file combo box and the MRU list
        private void RemoveMRUFiles(string files)
        {
            int idx = comboBoxImageFile.Items.IndexOf(files);
            if (idx != -1)
            {
                Properties.Settings.Default.MRUFiles.Remove(files);
                comboBoxImageFile.Items.RemoveAt(idx);
                Properties.Settings.Default.Save();

                if (comboBoxImageFile.Items.Count > 0)
                {
                    comboBoxImageFile.SelectedIndex = 0;
                }
                else
                {
                    listViewFiles.Items.Clear();
                }
            }
        }

        // add a file to the MRU list and the combo box
        private void AddMRUFiles(string files)
        {
            files = files.Trim();
            
            // update MRU items
            if (Properties.Settings.Default.MRUFiles.Contains(files))
            {
                Properties.Settings.Default.MRUFiles.Remove(files);
                comboBoxImageFile.Items.Remove(files);
            }

            Properties.Settings.Default.MRUFiles.Insert(0, files);
            Properties.Settings.Default.Save();

            comboBoxImageFile.Items.Insert(0, files);
            comboBoxImageFile.SelectedIndex = 0;
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = "*.hex";
            ofd.CheckFileExists = true;
            ofd.Filter = Properties.Resources.OpenFileDialogFilterSREC;
            ofd.FilterIndex = 0;
            ofd.Multiselect = true;
            ofd.InitialDirectory = comboBoxImageFile.Text;
            if (DialogResult.OK == ofd.ShowDialog(this))
            {
                comboBoxImageFile.Text = String.Empty;
                string files = "";
                foreach (string file in ofd.FileNames)
                {
                    files += file + "; ";
                }
                files = files.TrimEnd(' ', ';');
                comboBoxImageFile.Text = files;
                AddMRUFiles(comboBoxImageFile.Text);
            }
        }

        private void OnDbgTxt(object sender, DebugOutputEventArgs e)
        {
            DumpToOutput(e.Text, false);
        }

        private void buttonDeploy_Click(object sender, EventArgs e)
        {
            ReadOnlyCollection<string> files = this.Files;
            string[] sigfiles = new string[files.Count];
            int cnt=0;

            if (files.Count <= 0)
            {
                MessageBox.Show(Properties.Resources.WarningNoFilesForDeploy, Properties.Resources.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (string file in files)
            {
                if (file.Trim().Length == 0) continue;

                if (!File.Exists(file))
                {
                    MessageBox.Show(this, string.Format( Properties.Resources.ErrorFileCantOpen, file ), Properties.Resources.ErrorTitleImageFile, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string signature_file = GetSignatureFileName(file);

                sigfiles[cnt++] = signature_file;
            }

            // update MRU items
            AddMRUFiles(comboBoxImageFile.Text.Trim());

            MFDevice dev = ConnectToSelectedDevice();

            if (dev != null)
            {
                DeploymentStatusDialog dlg = new DeploymentStatusDialog(dev, files, sigfiles);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.ShowDialog(this);

                DisconnectFromSelectedDevice();
            }
            else
            {
                MessageBox.Show(this, "Please select a valid device!", "No device selected");
            }
        }

        private void button1_Click(System.Object sender, System.EventArgs e)
        {
            richTextBoxOutput.Text = String.Empty;
        }

        private void comboBoxImageFile_DropDown(System.Object sender, System.EventArgs e)
        {
            System.Drawing.Graphics gfx = Graphics.FromHwnd(comboBoxImageFile.Handle);
            int len = comboBoxImageFile.Width;
            foreach (string str in comboBoxImageFile.Items)
            {
                int size = (int)gfx.MeasureString(str, comboBoxImageFile.Font).Width;
                if (size > len)
                {
                    len = (size<1000? size: 1000);
                }
            }
            gfx.Dispose();

            comboBoxImageFile.DropDownWidth = len;
        }

        private void OnPlugInExecuteThread()
        {
            MFDevice dev = null;
            if (m_currentPlugInObject == null) return;

            try
            {
                m_deploy.OpenWithoutConnect = !m_currentPlugInObject.RequiresConnection;

                dev = ConnectToSelectedDevice();

                if (dev != null)
                {
                    if (m_currentPlugInObject.RunInSeparateThread)
                    {
                        DumpToOutput(string.Format(Properties.Resources.XCommand, m_currentPlugInObject.Name));
                    }

                    if (m_currentPlugInObject.RequiresConnection && (!dev.DbgEngine.TryToConnect(5, 100)))
                    {
                        throw new MFDeviceNoResponseException();
                    }

                    m_currentPlugInObject.OnAction(this, dev);
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
            }
            catch (Exception exc)
            {
                DumpToOutput(Properties.Resources.ErrorPrefix + exc.Message);
            }
            finally
            {
                DisconnectFromSelectedDevice();

                m_deploy.OpenWithoutConnect = false;

                if (m_currentPlugInObject.RunInSeparateThread)
                {
                    DumpToOutput(string.Format(Properties.Resources.StatusXComplete, m_currentPlugInObject.Name));
                }

                m_currentPlugInObject = null;
            }
        }

        private void Form1_FormClosing(System.Object sender, FormClosingEventArgs e)
        {
            m_fShuttingDown = true;

            if (m_pluginThread != null && m_pluginThread.IsAlive)
            {
                m_pluginThread.Abort();
            }

            if (m_device != null)
            {
                m_device.Dispose();
                m_device = null;
            }

            if (m_deploy != null)
            {
                m_deploy.Dispose();
                m_deploy = null;
            }
        }

        private string GetSignatureFileName(string hexFile)
        {
            string signature_file = hexFile;
            FileInfo fi = new FileInfo(hexFile);

            if (fi.Extension != null || fi.Extension.Length > 0)
            {
                int index = hexFile.LastIndexOf(fi.Extension);
                signature_file = hexFile.Remove(index, fi.Extension.Length);
            }

            signature_file += ".sig";

            return signature_file;
        }

        private void comboBoxImageFile_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            bool fUpdatedFiles = false;
            string filestr = (comboBoxImageFile.SelectedItem == null ? comboBoxImageFile.SelectedText : comboBoxImageFile.SelectedItem as string);
            Hashtable checkStateLookup = new Hashtable();

            listViewFiles.Items.Clear();

            if (m_FileHash.ContainsKey(filestr))
            {
                ArrayList ar = m_FileHash[filestr] as ArrayList;
                foreach (ListViewItem lvi in ar)
                {
                    string key = lvi.SubItems[1].Text;
                    FileInfo fi = new FileInfo(key);

                    checkStateLookup[key] = lvi.Checked;

                    if (lvi.SubItems[4].Text != fi.LastWriteTime.ToString())
                    {
                        fUpdatedFiles = true;
                    }

                    if (!fUpdatedFiles)
                    {
                        listViewFiles.Items.Add(lvi);
                    }
                }
            }
            else
            {
                fUpdatedFiles = true;
            }
            
            if(fUpdatedFiles)
            {
                ArrayList cache = new ArrayList();
                string[] files = filestr.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                listViewFiles.Items.Clear();

                try
                {
                    foreach (string file in files)
                    {
                        if (file.Trim().Length == 0) continue;

                        FileInfo fi = new FileInfo(file);

                        if (fi.Extension.ToLower() == ".nmf")
                        {
                            ListViewItem lvi = new ListViewItem(fi.Name);
                            lvi.SubItems.Add(fi.FullName);

                            uint dest = 0;
                            uint size = 0;

                            using (FileStream fs = File.OpenRead(file))
                            { 
                                byte []hdr = new byte[3*4];
                                uint comp;

                                fs.Read(hdr, 0, 12);

                                dest = BitConverter.ToUInt32(hdr, 0);
                                comp = BitConverter.ToUInt32(hdr, 4);
                                size = BitConverter.ToUInt32(hdr, 8);

                                while ((comp % 4) != 0)
                                {
                                    comp++;
                                }
                                fs.Seek(comp, SeekOrigin.Current);

                                while(fs.Position < fs.Length-1)
                                {
                                    fs.Read(hdr, 0, 12);

                                    uint destT = BitConverter.ToUInt32(hdr, 0);
                                    if (destT < dest)
                                    {
                                        dest = destT;
                                    }
                                    comp  = BitConverter.ToUInt32(hdr, 4);
                                    size += BitConverter.ToUInt32(hdr, 8);

                                    while ((comp % 4) != 0)
                                    {
                                        comp++;
                                    }
                                    fs.Seek(comp, SeekOrigin.Current);
                                }
                            }

                            lvi.SubItems.Add("0x" + dest.ToString("x08"));
                            lvi.SubItems.Add("0x" + size.ToString("x08"));
                            lvi.SubItems.Add(fi.LastWriteTime.ToString());
                            lvi.Checked = checkStateLookup.ContainsKey(fi.FullName) ? (bool)checkStateLookup[fi.FullName] : true;

                            cache.Add(lvi);
                            listViewFiles.Items.Add(lvi);
                        }
                        else
                        {
                            ArrayList blocks = new ArrayList();
                            _DBG.SRecordFile.Parse(fi.FullName, blocks, null);

                            foreach (_DBG.SRecordFile.Block blk in blocks)
                            {
                                ListViewItem lvi = new ListViewItem(fi.Name);
                                lvi.SubItems.Add(fi.FullName);
                                lvi.SubItems.Add("0x" + blk.address.ToString("x08"));
                                lvi.SubItems.Add("0x" + blk.data.Length.ToString("x08"));
                                lvi.SubItems.Add(fi.LastWriteTime.ToString());
                                lvi.Checked = checkStateLookup.ContainsKey(fi.FullName) ? (bool)checkStateLookup[fi.FullName] : true;

                                cache.Add(lvi);
                                listViewFiles.Items.Add(lvi);
                            }
                        }
                    }

                    m_FileHash[filestr] = cache;
                }
                catch (Exception ex)
                {
                    DumpToOutput(string.Format(Properties.Resources.Exception, ex.Message));
                }
            }
        }

        private void comboBoxImageFile_KeyDown(System.Object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (comboBoxImageFile.Text.Length > 0)
                    {
                        // update file view and MRU
                        comboBoxImageFile_SelectedIndexChanged(null, null);
                    }
                    else
                    {
                        listViewFiles.Items.Clear();
                    }
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;

                case Keys.Delete:
                    if (e.Control && comboBoxImageFile.SelectedItem != null && !comboBoxImageFile.DroppedDown)
                    {
                        RemoveMRUFiles(comboBoxImageFile.SelectedItem as string);
                        e.Handled = true;
                    }
                    break;
            }
        }

        int m_prevTransport = -1;

        private void comboBoxTransport_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            if (comboBoxTransport.SelectedIndex == (int)TransportComboBoxType.Usb)
            {
                if (m_prevTransport != (int)TransportComboBoxType.Usb)
                {
                    m_deploy.OnDeviceListUpdate += new EventHandler<EventArgs>(OnDeviceListUpdate);
                }
            }
            else
            {
                if (m_prevTransport == (int)TransportComboBoxType.Usb)
                {
                    m_deploy.OnDeviceListUpdate -= new EventHandler<EventArgs>(OnDeviceListUpdate);
                }
            }

            if (comboBoxTransport.SelectedIndex == (int)TransportComboBoxType.TcpIp)
            {
                if (m_prevTransport != (int)TransportComboBoxType.TcpIp)
                {
                    checkBoxUseSSL.Enabled = true;
                    if (Properties.Settings.Default.SslCert != null)
                    {
                        textBoxCert.Text = Properties.Settings.Default.SslCert;
                    }
                }
            }
            else
            {
                if (m_prevTransport == (int)TransportComboBoxType.TcpIp)
                {
                    checkBoxUseSSL.Checked = false;
                    checkBoxUseSSL.Enabled = false;
                }
            }


            m_prevTransport = comboBoxTransport.SelectedIndex;

            OnDeviceListUpdate(null, null);
        }


        private void clearFileListToolStripMenuItem_Click(System.Object sender, System.EventArgs e)
        {
            Properties.Settings.Default.MRUFiles.Clear();
            Properties.Settings.Default.Save();
            comboBoxImageFile.Items.Clear();
            comboBoxImageFile.Text = String.Empty;
            comboBoxImageFile_SelectedIndexChanged(null, null);
        }

        private void OnMenuItem_Click(System.Object sender, System.EventArgs e)
        {
            ToolStripMenuItem mi = sender as ToolStripMenuItem;
            Form dlg = null;

            if( mi == null ) return;

            MFDevice device = null;
            MFConfigHelper cfgHelper = null;

            Cursor old = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                if (mi != signDeploymentFileToolStripMenuItem1)
                {
                    device = ConnectToSelectedDevice();

                    if (device == null)
                    {
                        MessageBox.Show(this, string.Format(Properties.Resources.ErrorDeviceNotResponding, comboBoxDevice.Text), Properties.Resources.ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    cfgHelper = new MFConfigHelper(device);

                    if (!cfgHelper.IsValidConfig)
                    {
                        MessageBox.Show(this, Properties.Resources.ErrorUnsupportedConfiguration, Properties.Resources.ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                }

                if (mi == signDeploymentFileToolStripMenuItem1)
                {
                    dlg = new MFAppDeployConfigDialog(MFAppDeployConfigDialog.ConfigDialogCommand.SignDeployment);
                }
                else if (mi == updateDeviceKeysToolStripMenuItem)
                {
                    dlg = new MFAppDeployConfigDialog(device, MFAppDeployConfigDialog.ConfigDialogCommand.Configure);
                }
                else if (mi == createApplicationDeploymentToolStripMenuItem1)
                {
                    dlg = new MFAppDeployConfigDialog(device, MFAppDeployConfigDialog.ConfigDialogCommand.CreateDeployment);
                }
                else if (mi == uSBToolStripMenuItem)
                {
                    dlg = new MFUsbConfigDialog(device);
                }
                else if (mi == networkToolStripMenuItem)
                {
                    dlg = new MFNetworkConfigDialog(device);
                }

                if (dlg != null)
                {
                    dlg.StartPosition = FormStartPosition.CenterParent;
                    if (DialogResult.OK == dlg.ShowDialog())
                    {
                        if (dlg is MFUsbConfigDialog)
                        {
                            cfgHelper.Dispose();
                            cfgHelper = null;
                            while (!DisconnectFromSelectedDevice()) ;
                            Thread.Sleep(500); // wait for device to reset
                            OnDeviceListUpdate(null, null);
                        }
                    }
                }

                else if (mi == uSBConfigurationToolStripMenuItem)
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.Title = "Load USB Config File";
                    ofd.DefaultExt = "*.xml";
                    ofd.Filter = "USB Config File (*.xml)|*.xml";
                    ofd.CheckFileExists = true;
                    ofd.Multiselect = false;

                    cfgHelper.MaintainConnection = true;

                    if (DialogResult.OK == ofd.ShowDialog())
                    {
                        UsbXMLConfigSet xmlConfig = new UsbXMLConfigSet(ofd.FileName);
                        byte[] nativeConfig = xmlConfig.Read();
                        if (nativeConfig != null)
                        {
                            ofd.DefaultExt = "*.txt";
                            ofd.CheckFileExists = false;
                            ofd.CheckPathExists = true;
                            ofd.FileName = "USB Hex dump.txt";
                            ofd.Title = "Dump file for native config data";
                            if (DialogResult.OK == ofd.ShowDialog())
                            {
                                FileStream dumpFile;
                                try
                                {
                                    dumpFile = File.Create(ofd.FileName);
                                }
                                catch (Exception e2)
                                {
                                    MessageBox.Show("Text dump file could not be opened due to exception: " + e2.Message);
                                    return;
                                }

                                // Dump byte array to text file for review
                                byte[] buffer = new byte[16 * 3 + 2];
                                int blockSize = 0;
                                int bufIndex = 0;
                                for (int index = 0; index < nativeConfig.Length; index += blockSize)
                                {
                                    blockSize = nativeConfig.Length - index;
                                    if (blockSize > 16)
                                    {
                                        blockSize = 16;
                                    }
                                    bufIndex = 0;
                                    for (int i = 0; i < blockSize; i++)
                                    {
                                        byte c;

                                        c = (byte)((nativeConfig[index + i] >> 4) & 0x0F);
                                        c += (byte)'0';       // Convert to ASCII
                                        if (c > '9')
                                        {
                                            c += (byte)('A' - '9' - 1);
                                        }
                                        buffer[bufIndex++] = c;
                                        c = (byte)(nativeConfig[index + i] & 0x0F);
                                        c += (byte)'0';       // Convert to ASCII
                                        if (c > '9')
                                        {
                                            c += (byte)('A' - '9' - 1);
                                        }
                                        buffer[bufIndex++] = c;
                                        buffer[bufIndex++] = (byte)' ';
                                    }
                                    buffer[bufIndex++] = (byte)'\r';
                                    buffer[bufIndex++] = (byte)'\n';
                                    dumpFile.Write(buffer, 0, bufIndex);
                                }
                                dumpFile.Close();
                                MessageBox.Show("Written " + nativeConfig.Length.ToString() + " configuration bytes to '" + ofd.FileName + "'.");
                            }

                            try
                            {
                                if (!cfgHelper.IsValidConfig)
                                {
                                    MessageBox.Show(null, Properties.Resources.ErrorUnsupportedConfiguration, Properties.Resources.ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    return;
                                }
                                // NOTE:
                                // This assumes there is only one USB controller:  controller 0.  In the future, if
                                // support is needed for multiple USB controllers, the user must be allowed to select
                                // which controller the XML configuration is for.  Then, the correct configuration
                                // name needs to be used for the selected controller.  "USB1" is used for controller 0,
                                // "USB2" is used for controller 1, etc.
                                cfgHelper.WriteConfig("USB1", nativeConfig);
                            }
                            catch (Exception ex2)
                            {
                                MessageBox.Show(null, string.Format(Properties.Resources.ErrorX, ex2.Message), Properties.Resources.ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, string.Format(Properties.Resources.ErrorX, ex.Message), Properties.Resources.ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                Cursor.Current = old;

                if (cfgHelper != null)
                {
                    cfgHelper.Dispose();
                }

                DisconnectFromSelectedDevice();

                if (dlg != null)
                {
                    dlg.Dispose();
                }
            }
        }

        private void createKeyPairToolStripMenuItem_Click(System.Object sender, System.EventArgs e)
        {
            string file = MFAppDeployConfigDialog.ShowCreateKeyPairFileDialog();
        }

        private void aboutMFDeployToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MFAboutBox aboutbox = new MFAboutBox();
            aboutbox.ShowDialog();
        }

        private void helpTopicsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Help.ShowHelp(this, Properties.Resources.MFHelpFilename);
            }
            catch
            {
                // ShowHelp doesn't seem to throw an exception
                MessageBox.Show(Properties.Resources.MFHelpError);
            }
        }

        private void cLRCapabilitiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MFDevice device = ConnectToSelectedDevice();

                if (device != null)
                {
                    _DBG.Engine engine = device.DbgEngine;

                    engine.TryToConnect(0, 100, true, Microsoft.SPOT.Debugger.ConnectionSource.Unknown);

                    Microsoft.SPOT.CLRCapabilities caps = engine.Capabilities;

                    if (caps == null || caps.IsUnknown)
                    {
                        DumpToOutput(Properties.Resources.ErrorNotSupported);
                        DisconnectFromSelectedDevice();
                        return;
                    }

                    Type t = typeof(Microsoft.SPOT.CLRCapabilities);
                    foreach (PropertyInfo pi in t.GetProperties())
                    {
                        object o = pi.GetValue(caps, null);

                        try
                        {
                            if (o is Microsoft.SPOT.CLRCapabilities.LCDCapabilities ||
                                o is Microsoft.SPOT.CLRCapabilities.SoftwareVersionProperties ||
                                o is Microsoft.SPOT.CLRCapabilities.HalSystemInfoProperties ||
                                o is Microsoft.SPOT.CLRCapabilities.ClrInfoProperties ||
                                o is Microsoft.SPOT.CLRCapabilities.SolutionInfoProperties
                                )
                            {
                                foreach (FieldInfo fi in pi.PropertyType.GetFields())
                                {
                                    DumpToOutput(string.Format("{0,-40}{1}", pi.Name + "." + fi.Name + ":", fi.GetValue(o)));
                                }
                            }
                            else
                            {
                                DumpToOutput(string.Format("{0,-40}{1}", pi.Name + ":", o));
                            }
                        }
                        catch
                        {
                            DumpToOutput(Properties.Resources.ErrorNotSupported);
                        }
                    }
                    DisconnectFromSelectedDevice();
                }
            }
            catch
            {
                DumpToOutput(Properties.Resources.ErrorNotSupported);
            }
        }

        private void listenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DumpToOutput(string.Format(Properties.Resources.ConnectingToX, comboBoxDevice.Text), false);
            try
            {
                Cursor old = Cursor;
                this.Cursor = Cursors.WaitCursor;

                if (comboBoxTransport.Text == Properties.Resources.Serial) m_deploy.OpenWithoutConnect = true;

                ConnectToSelectedDevice();

                this.Cursor = old;
            }
            catch
            {
                DisconnectFromSelectedDevice();
            }

            if (m_device != null)
            {
                DumpToOutput(Properties.Resources.Connected);
            }
            else
            {
                DumpToOutput(string.Format(Properties.Resources.ErrorDeviceNotResponding, comboBoxDevice.Text));
            }
        }

        private void cancelToolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            DumpToOutput(Properties.Resources.UserCanceled);

            if (m_currentPlugInObject != null)
            {
                if (m_pluginThread != null && m_pluginThread.IsAlive)
                {
                    m_pluginThread.Abort();
                    m_pluginThread = null;
                }
            }
            else
            {
                while (!DisconnectFromSelectedDevice()) ;
            }

            m_deploy.OpenWithoutConnect = false;

        }

        private void updateSSLKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DumpToOutput("Updating SSL seed...", true);
            Thread.Sleep(0);

            Cursor old = Cursor;
            this.Cursor = Cursors.WaitCursor;

            try
            {

                MFDevice dev = ConnectToSelectedDevice();
                if (dev != null)
                {
                    MFSslKeyConfig cfg = new MFSslKeyConfig(dev);

                    cfg.Save();
                }
            }
            catch
            {
                MessageBox.Show(Properties.Resources.ErrorDeviceNotResponding, Properties.Resources.TitleAppDeploy, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                DisconnectFromSelectedDevice();
                Cursor = old;
                DumpToOutput("Update Complete!", true);
            }
        }

        private void checkBoxUseSSL_CheckedChanged(object sender, EventArgs e)
        {
            bool enabled = checkBoxUseSSL.Checked;
        }

        private void checkBoxUseSSL_EnabledChanged(object sender, EventArgs e)
        {
        }

        private void buttonBrowseCert_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = "*.pfx";
            ofd.CheckFileExists = true;
            ofd.Filter = "Certificate files (*.pfx;*.pem)|*.pfx;*.pem";
            ofd.FilterIndex = 0;
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;

            string initPath = string.IsNullOrEmpty(textBoxCert.Text) ?  "" : Path.GetDirectoryName(textBoxCert.Text);
            if (initPath == "") initPath = AppDomain.CurrentDomain.BaseDirectory;

            ofd.InitialDirectory = initPath;

            if (DialogResult.OK == ofd.ShowDialog(this))
            {
                textBoxCert.Text = ofd.FileName;
            }
        }

        private void OnPermiscuousWinUsbClicked( object sender, EventArgs e )
        {
            var item = sender as ToolStripMenuItem;
            System.Diagnostics.Debug.Assert( item != null );
            item.Checked = !item.Checked;
            m_deploy.EnablePermiscuousWinUsb = item.Checked;
        }
    }
}
