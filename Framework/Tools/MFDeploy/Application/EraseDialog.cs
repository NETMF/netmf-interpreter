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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using _DBG = Microsoft.SPOT.Debugger;
using _WP = Microsoft.SPOT.Debugger.WireProtocol;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
    public partial class EraseDialog : Form
    {
        MFDevice m_device;

        public EraseDialog(MFDevice dev)
        {
            InitializeComponent();

            m_device = dev;
        }

        public List<EraseOptions> m_eraseBlocks = new List<EraseOptions>();

        public EraseOptions[] EraseBlocks { get { return m_eraseBlocks.ToArray(); } }

        private void EraseDialog_Load(object sender, EventArgs e)
        {
            _DBG.Engine engine = m_device.DbgEngine;

            _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.Reply reply = engine.GetFlashSectorMap();

            if (reply != null)
            {
                Dictionary<EraseOptions, _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.FlashSectorData> hashBlockType = new Dictionary<EraseOptions, _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.FlashSectorData>();
                Dictionary<EraseOptions, string> usageNameHash = new Dictionary<EraseOptions, string>();

                for (int i = 0; i < reply.m_map.Length; i++)
                {
                    _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.FlashSectorData fsd = reply.m_map[i];

                    string usage = "";
                    EraseOptions eo = (EraseOptions)(-1);

                    switch (fsd.m_flags & _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK)
                    {
                        case _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_BOOTSTRAP:
                            //usage = "Bootstrap";
                            break;
                        case _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_CONFIG:
                        case _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_CODE:
                            if (m_device.DbgEngine.ConnectionSource == _DBG.ConnectionSource.TinyBooter)
                            {
                                usage = "Firmware";
                                eo = EraseOptions.Firmware;
                            }
                            break;
                        case _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_DEPLOYMENT:
                            usage = "Deployment";
                            eo = EraseOptions.Deployment;
                            break;
                        case _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_UPDATE:
                            usage = "Update Storage";
                            eo = EraseOptions.UpdateStorage;
                            break;
                        case _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_FS:
                            usage = "File System";
                            eo = EraseOptions.FileSystem;
                            break;
                        case _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_SIMPLE_B:
                        case _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_SIMPLE_A:
                            usage = "Simple Storage";
                            eo = EraseOptions.SimpleStorage;
                            break;
                        case _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_STORAGE_A:
                        case _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_STORAGE_B:
                            usage = "User Storage";
                            eo = EraseOptions.UserStorage;
                            break;
                    }

                    if (eo != (EraseOptions)(-1))
                    {
                        if (hashBlockType.ContainsKey(eo))
                        {
                            _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.FlashSectorData prev = hashBlockType[eo];
                            if (prev.m_address + prev.m_size == fsd.m_address)
                            {
                                prev.m_size += fsd.m_size;
                                hashBlockType[eo] = prev;
                            }
                        }
                        else
                        {
                            hashBlockType[eo] = fsd;
                            usageNameHash[eo] = usage;
                        }
                    }
                }

                foreach (EraseOptions eo in hashBlockType.Keys)
                {
                    ListViewItem lvi = listViewEraseSectors.Items.Add(new ListViewItem(new string[] { usageNameHash[eo], string.Format("0x{0:X08}", hashBlockType[eo].m_address), string.Format("0x{0:X08}", hashBlockType[eo].m_size) }));
                    lvi.Tag = eo;
                    if (eo != EraseOptions.Firmware)
                    {
                        lvi.Checked = true;
                    }
                }
            }
        }

        private void buttonEraseSectors_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listViewEraseSectors.Items.Count; i++)
            {
                ListViewItem lvi = listViewEraseSectors.Items[i];
                if (lvi.Checked)
                {
                    m_eraseBlocks.Add((EraseOptions)lvi.Tag);
                }
            }
        }
    }
}
