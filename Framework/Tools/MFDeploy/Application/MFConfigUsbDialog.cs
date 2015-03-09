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
using System.Globalization;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.CodeDom.Compiler;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Microsoft.NetMicroFramework.Tools;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.PlugIns;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
    internal partial class MFUsbConfigDialog : Form
    {
        private MFConfigHelper m_cfgHelper;
        private MFUsbConfiguration m_cfg;
        private const string c_name = "USB_NAME_CONFIG";

        public MFUsbConfigDialog( MFDevice device )
        {
            m_cfgHelper = new MFConfigHelper(device);
            m_cfg = new MFUsbConfiguration(device);

            InitializeComponent();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            Exception ex   = null;
            Cursor old     = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                m_cfg.Name = textBoxName.Text;
                m_cfgHelper.MaintainConnection = false;
                m_cfg.Save();
            }
            catch (MFInvalidConfigurationDataException e1)
            {
                ex = e1;
            }
            catch (MFConfigSectorEraseFailureException e2)
            {
                ex = e2;
            }
            catch (MFConfigSectorWriteFailureException e3)
            {
                ex = e3;
            }
            catch (MFConfigurationSectorOutOfMemoryException e4)
            {
                ex = e4;
            }
            finally
            {
                Cursor.Current = old;
            }

            if (ex != null)
            {
                MessageBox.Show(this, ex.Message, Properties.Resources.UsbTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void UsbConfigDialog_Load(object sender, System.EventArgs e)
        {
            m_cfg.Load();

            m_cfgHelper.MaintainConnection = true;

            textBoxName.Text = m_cfg.Name;
        }

        private void button2_Click(System.Object sender, System.EventArgs e)
        {
            m_cfgHelper.MaintainConnection = false;
        }

        private void MFUsbConfigDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_cfgHelper != null)
            {
                m_cfgHelper.Dispose();
            }
        }
    }
}