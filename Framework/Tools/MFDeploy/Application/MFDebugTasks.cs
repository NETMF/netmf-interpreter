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
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.PlugIns;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using _DBG = Microsoft.SPOT.Debugger;
using _WP = Microsoft.SPOT.Debugger.WireProtocol;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Debug
{
    public class DebugPlugins
    {
        public class MFDeployDebug_RebootAndStop : MFPlugInMenuItem
        {
            public override string Name { get { return "Reboot and Stop"; } }
            public override void OnAction(IMFDeployForm form, MFDevice device)
            {
                if (form == null || device == null) return;

                _DBG.Engine engine = device.DbgEngine;

                engine.RebootDevice(Microsoft.SPOT.Debugger.Engine.RebootOption.EnterBootloader);

                device.ConnectToTinyBooter();
            }
        }

        public class MFDeployDebug_DeployWithPortBooter : MFPlugInMenuItem
        {
            private IMFDeployForm m_form;
            private int m_lastPercent = -1;

            public override bool RequiresConnection { get { return false; } }
            public override string Name { get { return "Deploy with PortBooter"; } }
            public override void OnAction(IMFDeployForm form, MFDevice device)
            {
                if (form == null || device == null) return;

                _DBG.Engine engine = device.DbgEngine;

                ReadOnlyCollection<string> files = form.Files;
                if (files.Count == 0) return;

                uint address = uint.MaxValue;

                _DBG.PortBooter fl = new _DBG.PortBooter(engine);
                fl.OnProgress += new _DBG.PortBooter.ProgressEventHandler(OnProgress);
                fl.Start();

                try
                {
                    foreach (string file in files)
                    {
                        if (!File.Exists(file))
                        {
                            form.DumpToOutput(string.Format("Error: File doesn't exist {0}", file));
                            continue;
                        }

                        ArrayList blocks = new ArrayList();

                        uint tmp = _DBG.SRecordFile.Parse(file, blocks, null);

                        m_form = form;

                        fl.Program(blocks);

                        if (address == uint.MaxValue)
                        {
                            if (tmp != 0)
                            {
                                address = tmp;
                            }
                        }
                    }

                    if (address == uint.MaxValue) address = 0;

                    m_form.DumpToOutput(string.Format("Executing address 0x{0:x08}", address));
                    fl.Execute(address);
                    System.Threading.Thread.Sleep(200);
                }
                finally
                {
                    if (fl != null)
                    {
                        fl.OnProgress -= new _DBG.PortBooter.ProgressEventHandler(OnProgress);
                        fl.Stop();
                        fl.Dispose();
                    }
                }
            }

            private void OnProgress(_DBG.SRecordFile.Block block, int offset, bool fLast)
            {
                int percent = (int)(offset * 100 / block.data.Length);
                if ((percent % 10) == 0 && m_lastPercent != percent)
                {
                    m_lastPercent = percent;
                    m_form.DumpToOutput(string.Format("Percent Complete {0}%", percent));
                }
            }
        }

        public class MFDeployDebug_DeploymentMap : MFPlugInMenuItem
        {
            public override string Name { get { return "Deployment Map"; } }
            public override void OnAction(IMFDeployForm form, MFDevice device)
            {
                if (form == null || device == null) return;

                _DBG.Engine engine = device.DbgEngine;

                _DBG.WireProtocol.Commands.Monitor_DeploymentMap.Reply reply = engine.DeploymentMap();

                if (reply != null)
                {
                    for (int i = 0; i < reply.m_count; i++)
                    {
                        _DBG.WireProtocol.Commands.Monitor_DeploymentMap.DeploymentData dd = reply.m_map[i];

                        form.DumpToOutput("Assembly " + i.ToString());
                        form.DumpToOutput("  Address: " + dd.m_address.ToString());
                        form.DumpToOutput("  Size   : " + dd.m_size.ToString());
                        form.DumpToOutput("  CRC    : " + dd.m_CRC.ToString());
                    }

                    if (reply.m_count == 0)
                    {
                        form.DumpToOutput("No deployed assemblies");
                    }
                }
                else
                {
                    form.DumpToOutput("Command Not Supported by Device");
                }
            }
        }

        public class MFDeployDebug_FlashSectorMap : MFPlugInMenuItem
        {
            public override string Name { get { return "Flash Sector Map"; } }
            public override void OnAction(IMFDeployForm form, MFDevice device)
            {
                if (form == null || device == null) return;

                _DBG.Engine engine = device.DbgEngine;

                _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.Reply reply = engine.GetFlashSectorMap();

                if (reply != null)
                {
                    form.DumpToOutput(" Sector    Start       Size        Usage");
                    form.DumpToOutput("-----------------------------------------------");
                    for (int i = 0; i < reply.m_map.Length; i++)
                    {
                        _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.FlashSectorData fsd = reply.m_map[i];

                        string usage = "";
                        switch (fsd.m_flags & _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK)
                        {
                            case _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_BOOTSTRAP:
                                usage = "Bootstrap";
                                break;
                            case _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_CODE:
                                usage = "Code";
                                break;
                            case _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_CONFIG:
                                usage = "Configuration";
                                break;
                            case _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_DEPLOYMENT:
                                usage = "Deployment";
                                break;
                            case _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_UPDATE:
                                usage = "Update Storage";
                                break;
                            case _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_FS:
                                usage = "File System";
                                break;
                            case _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_SIMPLE_A:
                                usage = "Simple Storage (A)";
                                break;
                            case _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_SIMPLE_B:
                                usage = "Simple Storage (B)";
                                break;
                            case _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_STORAGE_A:
                                usage = "EWR Storage (A)";
                                break;
                            case _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_STORAGE_B:
                                usage = "EWR Storage (B)";
                                break;
                        }

                        form.DumpToOutput(string.Format("{0,5}  {1,12}{2,12}   {3}", i, string.Format("0x{0:x08}", fsd.m_address), string.Format("0x{0:x08}", fsd.m_size), usage));
                    }
                }
            }
        }

        public class MFDeployDebug_MemoryMap : MFPlugInMenuItem
        {
            public override string Name { get { return "Memory Map"; } }
            public override void OnAction(IMFDeployForm form, MFDevice device)
            {
                if (device == null || form == null) return;

                _DBG.Engine engine = device.DbgEngine;

                _DBG.WireProtocol.Commands.Monitor_MemoryMap.Range[] range = engine.MemoryMap();

                if (range != null && range.Length > 0)
                {
                    form.DumpToOutput("Type     Start       Size");
                    form.DumpToOutput("--------------------------------");
                    for (int i = 0; i < range.Length; i++)
                    {
                        string mem = "";
                        switch (range[i].m_flags)
                        {
                            case _DBG.WireProtocol.Commands.Monitor_MemoryMap.c_FLASH:
                                mem = "FLASH";
                                break;
                            case _DBG.WireProtocol.Commands.Monitor_MemoryMap.c_RAM:
                                mem = "RAM";
                                break;
                        }
                        form.DumpToOutput(string.Format("{0,-6} 0x{1:x08}  0x{2:x08}", mem, range[i].m_address, range[i].m_length));
                    }
                }
            }
        }

        public class MFDeployDebug_EnumAndExecute : MFPlugInMenuItem
        {
            public override string Name { get { return "Clear BootLoader Flag"; } }
            public override void OnAction(IMFDeployForm form, MFDevice device)
            {
                _DBG.Engine engine = device.DbgEngine;

                if (device.ConnectToTinyBooter())
                {
                    // ExecuteMemory at address 0 performs an enumerate and execute, which
                    // will clear the bootloader entry flag
                    engine.ExecuteMemory(0);
                }
                else
                {
                    form.DumpToOutput("Unable to connect to TinyBooter!");
                }
            }
        }

        public class MFDeployDebug_CreateEmptyKey : MFPlugInMenuItem
        {
            public override string Name { get { return "Create Empty Key"; } }

            public override bool RequiresConnection { get { return false; } }
            public override bool RunInSeparateThread
            {
                get
                {
                    return false;
                }
            }

            public override void OnAction(IMFDeployForm form, MFDevice device)
            {
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
                sfd.DefaultExt = "*.key";
                sfd.CheckPathExists = true;
                sfd.Filter = "Key File (*.key)|*.key|All Files (*.*)|*.*";
                sfd.FilterIndex = 0;
                sfd.AddExtension = true;
                sfd.OverwritePrompt = true;
                sfd.Title = "Create Empty Key";

                if (System.Windows.Forms.DialogResult.OK == sfd.ShowDialog())
                {
                    MFKeyConfig cfg = new MFKeyConfig();
                    KeyPair emptyKey = cfg.CreateEmptyKeyPair();
                    cfg.SaveKeyPair(emptyKey, sfd.FileName);
                }
            }
        }

        public class MFDeployDebug_EraseFirmware : MFPlugInMenuItem
        {
            public override string Name { get { return "Erase Firmware"; } }

            public override bool RequiresConnection { get { return true; } }
            public override bool RunInSeparateThread { get { return false; } }

            public override void OnAction(IMFDeployForm form, MFDevice device)
            {
                if (device.ConnectToTinyBooter())
                {
                    _WP.Commands.Monitor_FlashSectorMap.Reply reply = device.DbgEngine.GetFlashSectorMap();

                    if (reply != null)
                    {
                        foreach (_WP.Commands.Monitor_FlashSectorMap.FlashSectorData sector in reply.m_map)
                        {
                            uint usage = sector.m_flags & _WP.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK;
                            if (usage == _WP.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_CODE ||
                                usage == _WP.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_CONFIG)
                            {
                                device.DbgEngine.EraseMemory(sector.m_address, sector.m_size);
                            }
                        }
                    }
                }
            }
        }

        public class MFDeployDebug_RebootClr : MFPlugInMenuItem
        {
            public override string Name { get { return "Reboot CLR"; } }

            public override bool RequiresConnection { get { return true; } }
            public override bool RunInSeparateThread { get { return false; } }

            public override void OnAction(IMFDeployForm form, MFDevice device)
            {
                device.Reboot(false);
            }
        }

        public class MFDeployDebug_ShowDeviceInfo : MFPlugInMenuItem
        {
            public override string Name { get { return "Show Device Info"; } }

            //public override bool RequiresConnection { get { return true; } }
            public override bool RunInSeparateThread { get { return false; } }

            public override void OnAction(IMFDeployForm form, MFDevice device)
            {
                MFDevice.IMFDeviceInfo info = device.MFDeviceInfo;
                
                if ( !info.Valid )
                {
                    form.DumpToOutput("DeviceInfo is not valid!");
                }
                else
                {
                    form.DumpToOutput("DeviceInfo:");
                    form.DumpToOutput(String.Format("  HAL build info: {0}, {1}", info.HalBuildVersion.ToString(), info.HalBuildInfo));
                    form.DumpToOutput(String.Format("  OEM Product codes (vendor, model, SKU): {0}, {1}, {2}", info.OEM.ToString(), info.Model.ToString(), info.SKU.ToString()));
                    form.DumpToOutput("  Serial Numbers (module, system):");
                    form.DumpToOutput("    " + info.ModuleSerialNumber);
                    form.DumpToOutput("    " + info.SystemSerialNumber);
                    form.DumpToOutput(String.Format("  Solution Build Info: {0}, {1}", info.SolutionBuildVersion.ToString(), info.SolutionBuildInfo));
                    
                    form.DumpToOutput("  AppDomains:");
                    foreach (MFDevice.IAppDomainInfo adi in info.AppDomains)
                    {
                    form.DumpToOutput(String.Format("    {0}, id={1}", adi.Name, adi.ID));
                    }

                    form.DumpToOutput("  Assemblies:");
                    foreach (MFDevice.IAssemblyInfo ai in info.Assemblies)
                    {
                    form.DumpToOutput(String.Format("    {0},{1}", ai.Name, ai.Version));
                    }
                }
            }
        }
      
        
        
    }
}
