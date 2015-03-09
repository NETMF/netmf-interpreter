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
using System.Diagnostics;

namespace Microsoft.SPOT
{
    [Serializable]
    public class CLRCapabilities
    {
        [Flags]
        public enum Capability : ulong
        {
            None                  = 0x00000000,
            FloatingPoint         = 0x00000001,
            SourceLevelDebugging  = 0x00000002,
            AppDomains            = 0x00000004,
            ExceptionFilters      = 0x00000008,
            IncrementalDeployment = 0x00000010,
            SoftReboot            = 0x00000020,
            Profiling             = 0x00000040,
            Profiling_Allocations = 0x00000080,
            Profiling_Calls       = 0x00000100,
            ThreadCreateEx        = 0x00000400,
        }

        [Serializable]
        public struct LCDCapabilities
        {
            public readonly uint Width;
            public readonly uint Height;
            public readonly uint BitsPerPixel;

            public LCDCapabilities( uint width, uint height, uint bitsPerPixel )
            {
                this.Width = width;
                this.Height = height;
                this.BitsPerPixel = bitsPerPixel;
            }
        }

        [Serializable]
        public struct SoftwareVersionProperties
        {
            public readonly string BuildDate;
            public readonly uint   CompilerVersion;

            public SoftwareVersionProperties( byte[] buildDate, uint compVersion )
            {
                char[] chars = new char[buildDate.Length];
                int i = 0;
                for(i = 0; i <chars.Length && buildDate[i] != 0; i++)
                {
                    chars[i] = (char)buildDate[i];
                }
                this.BuildDate = new string(chars, 0, i);
                this.CompilerVersion = compVersion;
            }
        }

        [Serializable]
        public struct HalSystemInfoProperties
        {
            public readonly Version halVersion;
            public readonly string halVendorInfo;
            public readonly byte oemCode;
            public readonly byte modelCode;
            public readonly ushort skuCode;
            public readonly string moduleSerialNumber;
            public readonly string systemSerialNumber;
            
            public HalSystemInfoProperties(
                    Version hv, string hvi,
                    byte oc, byte mc, ushort sc,
                    byte[] mSerNumBytes, byte[] sSerNumBytes
                    )
            {
                halVersion = hv; halVendorInfo = hvi;
                oemCode = oc; modelCode = mc; skuCode = sc;

                moduleSerialNumber = BytesToHexString(mSerNumBytes);
                systemSerialNumber = BytesToHexString(sSerNumBytes);
            }
            
            private static string BytesToHexString(byte[] bytes)
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder();

                foreach (byte b in bytes)
                {
                    builder.Append ( String.Format("{0:X}", b));
                }

                return builder.ToString();
            }
        }
        
        [Serializable]
        public struct ClrInfoProperties
        {
            public readonly Version clrVersion;
            public readonly string clrVendorInfo;
            public readonly Version targetFrameworkVersion;
            
            public ClrInfoProperties(Version cv, string cvi, Version tfv)
            {
                this.clrVersion = cv;
                this.clrVendorInfo = cvi;
                this.targetFrameworkVersion = tfv;
            }
        }

        [Serializable]
        public struct SolutionInfoProperties
        {
            public readonly Version solutionVersion;
            public readonly string solutionVendorInfo;
            
            public SolutionInfoProperties(Version v, string i)
            {
                this.solutionVersion = v;
                this.solutionVendorInfo = i;
            }
        }

        private Capability m_capabilities;
        private LCDCapabilities m_lcd;
        private SoftwareVersionProperties m_swVersion;
        private HalSystemInfoProperties m_halSystemInfo;
        private ClrInfoProperties m_clrInfo;
        private SolutionInfoProperties m_solutionReleaseInfo;

        private bool m_fUnknown;

        public CLRCapabilities()
            : this(Capability.None, new LCDCapabilities(), new SoftwareVersionProperties(), 
                new HalSystemInfoProperties(), new ClrInfoProperties(), new SolutionInfoProperties())
            {
            }
            
        public CLRCapabilities(
            Capability capability, 
            LCDCapabilities lcd, 
            SoftwareVersionProperties ver, 
            HalSystemInfoProperties halSystemInfo,
            ClrInfoProperties clrInfo,
            SolutionInfoProperties solutionReleaseInfo
            )
        {
            m_fUnknown = (capability == Capability.None);
            m_capabilities = capability;
            m_lcd = lcd;
            m_swVersion = ver;
            
            m_halSystemInfo = halSystemInfo;
            m_clrInfo = clrInfo;
            m_solutionReleaseInfo = solutionReleaseInfo;
        }

        public HalSystemInfoProperties HalSystemInfo
        {
            get
            { 
                Debug.Assert(!m_fUnknown);
                return m_halSystemInfo;
            }
        }
        
        public ClrInfoProperties ClrInfo
        {
            get
            {
                Debug.Assert(!m_fUnknown);
                return m_clrInfo;
            }
        }
        
        public SolutionInfoProperties SolutionReleaseInfo
        {
            get
            {
                Debug.Assert(!m_fUnknown);
                return m_solutionReleaseInfo;
            }
        }
        
        public SoftwareVersionProperties SoftwareVersion
        {
            get
            {
                Debug.Assert( !m_fUnknown );
                return m_swVersion;                
            }
        }

        public bool FloatingPoint
        {
            get
            {
                Debug.Assert( !m_fUnknown );
                return (m_capabilities & Capability.FloatingPoint) != 0;
            }
        }

        public bool SourceLevelDebugging
        {
            get
            {
                Debug.Assert( !m_fUnknown );
                return (m_capabilities & Capability.SourceLevelDebugging) != 0;
            }
        }

        public bool ThreadCreateEx
        {
            get
            {
                Debug.Assert(!m_fUnknown);
                return (m_capabilities & Capability.ThreadCreateEx) != 0;
            }
        }

        public LCDCapabilities LCD
        {
            get
            {
                Debug.Assert( !m_fUnknown );
                return m_lcd;
            }
        }

        public bool AppDomains
        {
            get
            {
                Debug.Assert( !m_fUnknown );
                return (m_capabilities & Capability.AppDomains) != 0;
            }
        }

        public bool ExceptionFilters
        {
            get
            {
                Debug.Assert(!m_fUnknown);
                return (m_capabilities & Capability.ExceptionFilters) != 0;
            }
        }

        public bool IncrementalDeployment
        {
            get
            {
                Debug.Assert(!m_fUnknown);
                return (m_capabilities & Capability.IncrementalDeployment) != 0;
            }
        }

        public bool SoftReboot
        {
            get
            {
                Debug.Assert(!m_fUnknown);
                return (m_capabilities & Capability.SoftReboot) != 0;
            }
        }

        public bool Profiling
        {
            get
            {
                Debug.Assert(!m_fUnknown);
                return (m_capabilities & Capability.Profiling) != 0;
            }
        }

        public bool ProfilingAllocations
        {
            get
            {
                Debug.Assert(!m_fUnknown);
                return (m_capabilities & Capability.Profiling_Allocations) != 0;
            }
        }

        public bool ProfilingCalls
        {
            get
            {
                Debug.Assert(!m_fUnknown);
                return (m_capabilities & Capability.Profiling_Calls) != 0;
            }
        }

        public bool IsUnknown
        {
            get { return m_fUnknown; }
        }
    }
}
