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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.PlugIns;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using System.Threading;
using System.Runtime.InteropServices;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
    public partial class MFAppDeployConfigDialog : Form
    {
        MFApplicationDeployment m_appDeploy = null;
        MFKeyConfig m_keyTool = new MFKeyConfig();

        public enum ConfigDialogCommand
        {
            Configure,
            CreateDeployment,
            SignDeployment,
        }

        enum KeyFileType
        {
            PublicKey,
            PrivateKey,
            Signature,
            HexFile,
        }

        private PublicKeyUpdateInfo m_KeyUpdateInfo;
        private MFDevice m_device;
        private MFConfigHelper m_cfgHelper;
        private ConfigDialogCommand m_command;
        private BackgroundWorker m_backgroundWorker;

        public MFAppDeployConfigDialog(ConfigDialogCommand command) : this(null, command)
        {
        }

        public MFAppDeployConfigDialog(MFDevice device, ConfigDialogCommand command)
        {
            if (device != null)
            {
                m_cfgHelper = new MFConfigHelper(device);
                m_appDeploy = new MFApplicationDeployment(device);
            }
            m_command = command;
            m_device = device;

            InitializeComponent();
        }

        public PublicKeyUpdateInfo KeyUpdateInfo 
        { 
            get { return m_KeyUpdateInfo; } 
            set { m_KeyUpdateInfo = value; } 
        }

        private bool CheckKeySize(int size, KeyFileType type)
        {
            switch (type)
            {
                case KeyFileType.PublicKey:
                    return size == MFKeyConfig.PublicKeySize;
                case KeyFileType.PrivateKey:
                    return size == MFKeyConfig.PrivateKeySize;
                case KeyFileType.Signature:
                    return size == MFKeyConfig.SignatureSize;
                case KeyFileType.HexFile:
                    return true;
            }

            return false;
        }

        private bool CheckKey(byte[] key, KeyFileType type)
        {
            return CheckKeySize(key.Length, type);
        }

        private bool CheckFile(string file, KeyFileType type)
        {
            int size = 0;
            string errorMessage = "";

            switch (type)
            {
                case KeyFileType.PublicKey:                    
                case KeyFileType.PrivateKey:
                    try
                    {
                        KeyPair keyPair = m_keyTool.LoadKeyPair(file);
                        byte[] key = (type == KeyFileType.PublicKey) ? keyPair.PublicKey : keyPair.PrivateKey;
                        size = key.Length;
                    }
                    catch
                    {
                        MessageBox.Show(this, string.Format(Properties.Resources.ErrorFileInvalid, file), Properties.Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    errorMessage = Properties.Resources.ErrorKeyFileInvalid;
                    break;
                case KeyFileType.Signature:
                    try
                    {
                        FileInfo fi = new FileInfo(file);
                        size = (int)fi.Length;
                    }
                    catch
                    {
                        MessageBox.Show(this, string.Format(Properties.Resources.ErrorFileInvalid, file), Properties.Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    errorMessage = Properties.Resources.ErrorSignatureFileInvalid;
                    break;
                case KeyFileType.HexFile:
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
            }

            if(!CheckKeySize(size, type))
            {
                MessageBox.Show(this, string.Format(errorMessage, Path.GetFileName(file)), Properties.Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void buttonBrowseFile1_Click(object sender, EventArgs e)
        {
            FileDialog fd = null;
            OpenFileDialog ofd;
            SaveFileDialog sfd;
            KeyFileType kft = KeyFileType.PrivateKey;

            switch (m_command)
            {
                case ConfigDialogCommand.Configure:
                    ofd = new OpenFileDialog();
                    ofd.Filter = Properties.Resources.FileDialogFilterKeys;
                    ofd.FilterIndex = 0;
                    ofd.Multiselect = false;
                    ofd.RestoreDirectory = true;

                    kft = KeyFileType.PublicKey;

                    fd = ofd;
                    break;
                case ConfigDialogCommand.SignDeployment:
                    ofd = new OpenFileDialog();
                    ofd.Filter = Properties.Resources.FileDialogFilterDeploymentFiles;
                    ofd.FilterIndex = 0;
                    ofd.Multiselect = false;
                    ofd.RestoreDirectory = true;

                    kft = KeyFileType.HexFile;

                    fd = ofd;
                    break;
                case ConfigDialogCommand.CreateDeployment:
                    sfd = new SaveFileDialog();
                    sfd.Filter = Properties.Resources.FileDialogFilterDeploymentFiles;
                    sfd.FilterIndex = 0;
                    sfd.RestoreDirectory = true;
                    sfd.Title = Properties.Resources.CreateDeploymentTitle;

                    kft = KeyFileType.HexFile;

                    fd = sfd;
                    break;
            }

            if (DialogResult.OK == fd.ShowDialog(this))
            {
                comboBoxFile1.Text = fd.FileName;

                if (fd is OpenFileDialog)
                {
                    if (!CheckFile(comboBoxFile1.Text, kft))
                    {
                        comboBoxFile1.SelectAll();
                    }
                }
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {                        
            try
            {
                ComboBox comboBoxFailure = null;
                string errorMessage = null;

                string file1 = comboBoxFile1.Text;
                if (!File.Exists(file1) &&
                    ((m_command == ConfigDialogCommand.Configure && !CheckFile(file1, KeyFileType.PublicKey))
                    || (m_command == ConfigDialogCommand.SignDeployment && !CheckFile(file1, KeyFileType.HexFile)))
                    )                    
                {
                    comboBoxFailure = comboBoxFile1;
                    errorMessage = string.Format(Properties.Resources.ErrorFileInvalid, file1); 
                }

                string fileKey = comboBoxPrivateKey.Text;
                if (comboBoxPrivateKey.Enabled && m_command != ConfigDialogCommand.CreateDeployment)
                {
                    if (fileKey.Trim() == "")
                    {
                        comboBoxFailure = comboBoxPrivateKey;
                        errorMessage = Properties.Resources.ErrorKeyRequired;
                    }
                    else if (!File.Exists(fileKey) || !CheckFile(fileKey, KeyFileType.PublicKey))
                    {
                        comboBoxFailure = comboBoxPrivateKey;
                        errorMessage = string.Format(Properties.Resources.ErrorFileInvalid, fileKey); 
                    }
                }

                if (errorMessage != null)
                {
                    comboBoxFailure.SelectAll();
                    comboBoxFailure.Focus();
                    MessageBox.Show(this, errorMessage, Properties.Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                switch (m_command)
                {
                    case ConfigDialogCommand.Configure:
                        OnConfigure(sender, e);
                        break;
                    case ConfigDialogCommand.CreateDeployment:
                        OnCreateDeployment(sender, e);
                        break;
                    case ConfigDialogCommand.SignDeployment:
                        OnSignDeployment(sender, e);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Properties.Resources.MenuItemPublicKeyConfiguration, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void CreateSignatureFile(byte[] binData)
        {
            string keyFile = comboBoxPrivateKey.Text;
            string hexFile = comboBoxFile1.Text;

            if (binData.Length == 0)
            {
                MessageBox.Show(this, Properties.Resources.ErrorNoDeploymentAssemblies, Properties.Resources.TitleAppDeploy);
                return;
            }

            if(!string.IsNullOrEmpty(keyFile))
            {
                byte[] privateKey = m_keyTool.LoadKeyPair(keyFile).PrivateKey;
                byte[] sigData = m_keyTool.SignData(binData, privateKey);

                string sigFile = Path.ChangeExtension(this.HexFile, ".sig");

                using (FileStream fs = File.Create(sigFile))
                {
                    fs.Write(sigData, 0, sigData.Length);
                }
            }
        }

        private string PrivateKeyFile
        {
            get
            {
                return comboBoxPrivateKey.Text;
            }
        }

        private string PublicKeyFile
        {
            get
            {
                System.Diagnostics.Debug.Assert(m_command == ConfigDialogCommand.Configure);
                return comboBoxFile1.Text;
            }
        }

        private string HexFile
        {
            get
            {
                System.Diagnostics.Debug.Assert(m_command == ConfigDialogCommand.CreateDeployment || m_command == ConfigDialogCommand.SignDeployment);
                return comboBoxFile1.Text;
            }
        }

        private void OnCreateDeployment(object sender, EventArgs e)
        {
            m_backgroundWorker = new BackgroundWorker();            
            m_backgroundWorker.WorkerReportsProgress = true;
            m_backgroundWorker.WorkerSupportsCancellation = true;
            m_backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(OnCreateDeployment_Progress);
            m_backgroundWorker.DoWork += new DoWorkEventHandler(OnCreateDeployment_DoWork);
            m_backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnCreateDeployment_Completed);
            m_backgroundWorker.RunWorkerAsync(m_device);

            //Disable everything except the cancel button
            this.comboBoxFile1.Enabled = false;
            this.labelFile1.Enabled = false;
            this.comboBoxPrivateKey.Enabled = false;
            this.labelPrivateKey.Enabled = false;
            this.buttonBrowseFile1.Enabled = false;
            this.buttonBrowsePrivateKey.Enabled = false;
            this.buttonOk.Enabled = false;
            this.buttonCreate.Enabled = false;
            this.progressBar.Visible = true;
        }

        private void OnCreateDeployment_Progress(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar.Value = e.ProgressPercentage;
        }

        private void OnCreateDeployment_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                DialogResult = DialogResult.Cancel;
            }
            else
            {
                MFApplicationDeployment.MFApplicationDeploymentData data = e.Result as MFApplicationDeployment.MFApplicationDeploymentData;

                //save hex file
                using (FileStream fs = File.Create(this.HexFile))
                {
                    fs.Write(data.HexData, 0, data.HexData.Length);
                }

                CreateSignatureFile(data.BinaryData);

                DialogResult = DialogResult.OK;
            }

            m_backgroundWorker = null;
        }

        private void OnCreateDeployment_DoWork(object sender, DoWorkEventArgs e)
        {
            m_appDeploy.CreateDeploymentData(sender as BackgroundWorker, e);
        }

        private void OnSignDeployment(object sender, EventArgs e)
        {
            byte[] binData = new MFApplicationDeployment.BinToSrec().GetBinaryData(this.HexFile);
            
            CreateSignatureFile(binData);

            DialogResult = DialogResult.OK;
        }

        private void OnConfigure(object sender, EventArgs e)
        {
            string fileNewKey = comboBoxFile1.Text;
            string fileOldKey = comboBoxPrivateKey.Text;

            m_KeyUpdateInfo = new PublicKeyUpdateInfo();
            m_KeyUpdateInfo.PublicKeyIndex = (comboBoxKeyIndex.SelectedIndex == 1 ? PublicKeyUpdateInfo.KeyIndex.DeploymentKey : PublicKeyUpdateInfo.KeyIndex.FirmwareKey);
            try
            {
                m_KeyUpdateInfo.NewPublicKey = m_keyTool.LoadKeyPair(fileNewKey).PublicKey;
            }
            catch
            {
                MessageBox.Show(this, string.Format(Properties.Resources.ErrorFileInvalid, fileNewKey), Properties.Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (comboBoxPrivateKey.Enabled)
            {
                KeyPair keyPairSign = m_keyTool.LoadKeyPair(fileOldKey);
                byte[] oldKey = keyPairSign.PrivateKey;

                m_KeyUpdateInfo.NewPublicKeySignature = m_keyTool.SignData(m_KeyUpdateInfo.NewPublicKey, oldKey);
            }

            m_cfgHelper.MaintainConnection = false;

            if (!m_cfgHelper.UpdatePublicKey(m_KeyUpdateInfo))
            {
                MessageBox.Show((IWin32Window)this, Properties.Resources.ErrorUnableToUpdateKey, Name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
        }

        private void buttonBrowsePrivateKey_Click(System.Object sender, System.EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = Properties.Resources.FileDialogFilterKeys;
            ofd.FilterIndex = 0;
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;

            if (DialogResult.OK == ofd.ShowDialog(this))
            {
                comboBoxPrivateKey.Text = ofd.FileName;

                if (!CheckFile(comboBoxPrivateKey.Text, KeyFileType.PrivateKey))
                {
                    comboBoxPrivateKey.SelectAll();
                }
            }
        }

        private void ConfigDialog_Load(System.Object sender, System.EventArgs e)
        {
            if (m_cfgHelper != null)
            {
                m_cfgHelper.MaintainConnection = true;
            }

            switch (m_command)
            {
                case ConfigDialogCommand.Configure:
                    comboBoxKeyIndex.SelectedIndex = 1;
                    labelFile1.Text = Properties.Resources.LabelNewKey;
                    comboBoxPrivateKey.Enabled = m_cfgHelper.IsDeploymentKeyLocked;
                    buttonBrowsePrivateKey.Enabled = m_cfgHelper.IsDeploymentKeyLocked;
                    labelPrivateKey.Text = Properties.Resources.LabelOldPrivateKey;
                    this.Text = Properties.Resources.MenuItemPublicKeyConfiguration;
                    break;
                case ConfigDialogCommand.CreateDeployment:
                case ConfigDialogCommand.SignDeployment:
                    labelKeyIndex.Visible = false;
                    comboBoxKeyIndex.Visible = false;
                    labelPrivateKey.Text = Properties.Resources.LabelPrivateKey;
                    labelFile1.Text = Properties.Resources.LabelDeploymentFile;
                    this.Text = (m_command == ConfigDialogCommand.CreateDeployment) ?
                        Properties.Resources.MenuItemCreateApplicationDeployment
                        : Properties.Resources.MenuItemSignHexFile;
                    break;
            }             
        }

        private void comboBoxKeyIndex_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            System.Diagnostics.Debug.Assert(m_command == ConfigDialogCommand.Configure);

            switch (comboBoxKeyIndex.SelectedIndex)
            {
                case 0:
                    comboBoxPrivateKey.Enabled = m_cfgHelper.IsFirmwareKeyLocked;
                    buttonBrowsePrivateKey.Enabled = m_cfgHelper.IsFirmwareKeyLocked;
                    break;
                case 1:
                    comboBoxPrivateKey.Enabled = m_cfgHelper.IsDeploymentKeyLocked;
                    buttonBrowsePrivateKey.Enabled = m_cfgHelper.IsDeploymentKeyLocked;
                    break;
            }
        }

        private void buttonCreate_Click(System.Object sender, System.EventArgs e)
        {
            string keyPairFile = ShowCreateKeyPairFileDialog();

            if (m_command == ConfigDialogCommand.Configure)
            {
                comboBoxFile1.Text = keyPairFile;
            }
            else
            {
                comboBoxPrivateKey.Text = keyPairFile;
            }
        }

        private void buttonCancel_Click(System.Object sender, System.EventArgs e)
        {
            if (m_backgroundWorker != null)
            {
                m_backgroundWorker.CancelAsync();
            }
            else
            {
                this.DialogResult = DialogResult.Cancel;
            }
        }


        static internal string ShowCreateKeyPairFileDialog()
        {
            string fileName = "";

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = "*.key";
            sfd.CheckPathExists = true;
            sfd.Filter = Properties.Resources.FileDialogFilterKeys;
            sfd.FilterIndex = 0;
            sfd.AddExtension = true;
            sfd.OverwritePrompt = true;
            sfd.Title = Properties.Resources.SaveKeyTitle;

            if (DialogResult.OK == sfd.ShowDialog())
            {
                fileName = sfd.FileName;

                MFKeyConfig keyTool = new MFKeyConfig();

                KeyPair keyPair = keyTool.CreateKeyPair();
                keyTool.SaveKeyPair(keyPair, fileName);
            }

            return fileName;
        }

        private void MFAppDeployConfigDialog_FormClosing(System.Object sender, FormClosingEventArgs e)
        {
            if (m_cfgHelper != null)
            {
                m_cfgHelper.Dispose();
            }
        }

    }
}