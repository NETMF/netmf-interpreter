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
using System.Text;
using System.Threading;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.PlugIns;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using System.Runtime.InteropServices;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct USB_CONFIG : IHAL_CONFIG_BASE
    {
        const int c_MaxNameLength = 30;

        public HAL_CONFIG_BLOCK Header;
        public fixed byte FriendlyName[c_MaxNameLength+1];
        public byte buffer;  // make it 4-byte aligned

        public int Size
        {
            get
            {
                int size = 0;

                unsafe
                {
                    size = sizeof(USB_CONFIG);
                }
                return size;
            }
        }

        public HAL_CONFIG_BLOCK ConfigHeader
        {
            get { return Header; }
            set { Header = value; }
        }

        public string FriendlyNameString()
        {
            int i = 0;
            char[] str = new char[c_MaxNameLength+1];

            fixed (byte* FName = FriendlyName)
            {
                for (i = 0; i < c_MaxNameLength; i++)
                {
                    if ((char)FName[i] == '\0') break;

                    str[i] = (char)FName[i];
                }
            }
            return new string(str, 0, i);
        }

        public void SetName(string name)
        {
            if(name == null || name.Equals( "" ))
            {
                throw new ArgumentException( "The device USB name is invalid" );
            }
            
            int min = name.Length;

            if (c_MaxNameLength < min) min = c_MaxNameLength;

            fixed (byte* FName = FriendlyName)
            {
                int i = 0;
                for (i=0; i<min; i++)
                {
                    FName[i] = (byte)name[i];
                }
                FName[i] = (byte)'\0';
            }
        }
    }

    public class MFUsbConfiguration
    {
        private USB_CONFIG     m_cfg = new USB_CONFIG();
        private MFConfigHelper m_cfgHelper;
        private const string   c_name = "USB_NAME_CONFIG";

        public MFUsbConfiguration(MFDevice dev)
        {
            m_cfgHelper = new MFConfigHelper(dev);
        }

        public string Name
        {
            get
            {
                return m_cfg.FriendlyNameString();
            }
            set
            {
                m_cfg.SetName(value);
            }
        }

        public void Load()
        {
            byte[] data = m_cfgHelper.FindConfig(c_name);

            if (data != null)
            {
                m_cfg = (USB_CONFIG)MFConfigHelper.UnmarshalData(data, typeof(USB_CONFIG));
            }
        }

        public void Save()
        {
            m_cfgHelper.WriteConfig(c_name, m_cfg);
        }
    }
}
