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
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

using _DBG = Microsoft.SPOT.Debugger;
using _WP  = Microsoft.SPOT.Debugger.WireProtocol;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct CONFIG_SECTOR_VERSION
    {
        internal const int c_CurrentTinyBooterVersion = 4;

        internal byte Major;
        internal byte Minor;
        internal byte TinyBooter;
        internal byte Extra;
    };

    [StructLayout(LayoutKind.Sequential)]
    internal struct OEM_MODEL_SKU
    {
        internal byte   OEM;
        internal byte   Model;
        internal UInt16  SKU;
    };

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct OEM_SERIAL_NUMBERS
    {
        internal fixed byte module_serial_number[32];
        internal fixed byte system_serial_number[16];
    };

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct TINYBOOTER_KEY_CONFIG
    {
        internal const int c_KeySignatureLength = 128;
        internal const int c_RSAKeyLength       = 260;

        //internal fixed byte KeySignature[c_KeySignatureLength];
        internal fixed byte SectorKey[c_RSAKeyLength]; //RSAKey 4 bytes (exponent) + 128 bytes (module) + 128 bytes (exponent)
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct SECTOR_BIT_FIELD
    {
        const int c_MaxSectorCount = 287; // pxa271 has 259 sectors, 287 == 9 * sizeof(UINT32) - 1, which is the next biggest whole 
        const int c_MaxFieldUnits  = (c_MaxSectorCount + 1) / (8 * sizeof(UInt32)); // bits

        internal fixed UInt32 BitField[c_MaxFieldUnits];
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct SECTOR_BIT_FIELD_TB
    {
        const int c_MaxBitCount    = 8640; 
        const int c_MaxFieldUnits  = (c_MaxBitCount / (8 * sizeof(UInt32))); // bits

        internal fixed UInt32 BitField[c_MaxFieldUnits];
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct HAL_CONFIGURATION_SECTOR
    {
        const int c_MaxBootEntryFlags = 50;
        const int c_BackwardsCompatibilityBufferSize = 88;

        internal UInt32                 ConfigurationLength;

        internal CONFIG_SECTOR_VERSION  Version;

        internal fixed byte             Buffer[c_BackwardsCompatibilityBufferSize];

        internal fixed UInt32           BooterFlagArray[c_MaxBootEntryFlags];

        internal SECTOR_BIT_FIELD       SignatureCheck1; // 8 changes before erase
        internal SECTOR_BIT_FIELD       SignatureCheck2; // 8 changes before erase
        internal SECTOR_BIT_FIELD       SignatureCheck3; // 8 changes before erase
        internal SECTOR_BIT_FIELD       SignatureCheck4; // 8 changes before erase
        internal SECTOR_BIT_FIELD       SignatureCheck5; // 8 changes before erase
        internal SECTOR_BIT_FIELD       SignatureCheck6; // 8 changes before erase
        internal SECTOR_BIT_FIELD       SignatureCheck7; // 8 changes before erase
        internal SECTOR_BIT_FIELD       SignatureCheck8; // 8 changes before erase
        
        internal TINYBOOTER_KEY_CONFIG  PublicKeyFirmware;
        internal TINYBOOTER_KEY_CONFIG  PublicKeyDeployment;

        internal OEM_MODEL_SKU          OEM_Model_SKU;

        internal OEM_SERIAL_NUMBERS     OemSerialNumbers;

        internal SECTOR_BIT_FIELD_TB    CLR_ConfigData;

        internal HAL_CONFIG_BLOCK       FirstConfigBlock;
    };


    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct HAL_CONFIG_BLOCK
    {
        public UInt32 Signature;
        public UInt32 HeaderCRC;
        public UInt32 DataCRC;
        public UInt32 Size;
        public fixed byte DriverName[64];

        public string DriverNameString
        {
            get
            {
                StringBuilder sb = new StringBuilder(66);
                
                fixed (byte* data = DriverName)
                {
                    for (int i = 0; i < 64; i++)
                    {
                        if ((char)data[i] == '\0') break;
                        sb.Append((char)data[i]);
                    }
                }
             
                return sb.ToString();
            }

            set
            {
                fixed (byte* data = DriverName)
                {
                    int len = value.Length;

                    if (len > 64) len = 64;

                    for (int i = 0; i < len; i++)
                    {
                        data[i] = (byte)value[i];
                    }
                }
            }
        }
    }

    public interface IHAL_CONFIG_BASE
    {
        HAL_CONFIG_BLOCK ConfigHeader
        {
            get;
            set;
        }

        int Size
        {
            get;
        }
    }

    public struct PublicKeyUpdateInfo
    {
        public enum KeyIndex
        {
            FirmwareKey   = 0,
            DeploymentKey = 1,
        };

        public KeyIndex  PublicKeyIndex;
        public byte[]    NewPublicKey;
        public byte[]    NewPublicKeySignature;
    };


    public class MFConfigHelper : IDisposable
    {
        internal struct ConfigIndexData
        {
            internal ConfigIndexData(int idx, int size)
            {
                Index = idx;
                Size = size;
            }
            internal int Index;
            internal int Size;
        }

        private HAL_CONFIGURATION_SECTOR m_StaticConfig;
        private bool                     m_init              = false;
        private Hashtable                m_cfgHash           = new Hashtable();
        private int                      m_lastCfgIndex      = -1;
        private byte[]                   m_all_cfg_data      = null;
        private MFDevice                 m_device            = null;
        private bool                     m_firmwareKeyLocked = true;
        private bool                     m_deployKeyLocked   = true;
        private Thread                   m_thread            = null;
        private bool                     m_fValidConfig      = false;
        private bool                     m_isDisposed        = false;
        private bool                     m_fRestartClr       = true;
        private bool                     m_fStaticCfgOK      = false;
        
        private _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.FlashSectorData m_cfg_sector;
        
        private const UInt32       c_Version_V2             = 0x324C4148; // HAL2
        private const UInt32       c_Seed                   = 1;          // HAL_STRUCT_VERSION
        private const UInt32       c_EnumerateAndLaunchAddr = 0x0;
        private const int          c_MaxDriverNameLength    = 63;


        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.m_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    try
                    {
                        // restart the clr if we weren't starting from tinybooter
                        if (m_fRestartClr && m_device.DbgEngine != null &&
                            m_device.DbgEngine.ConnectionSource == Microsoft.SPOT.Debugger.ConnectionSource.TinyBooter
                            )
                        {
                            m_device.Execute(0);
                        }
                    }
                    catch
                    {
                    }
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                try
                {
                    MaintainConnection = false;
                }
                catch
                {
                }

                // Note disposing has been done.
                m_isDisposed = true;

            }
        }

        ~MFConfigHelper()
        {
            Dispose(false);
        }

        public MFConfigHelper(MFDevice device)
        {
            m_device = device;

            Microsoft.SPOT.Debugger.ConnectionSource src = device.DbgEngine.ConnectionSource;

            if(src == _DBG.ConnectionSource.Unknown)
            {
                device.Connect(500, true);
            }

            m_fRestartClr = device.DbgEngine.ConnectionSource == Microsoft.SPOT.Debugger.ConnectionSource.TinyCLR;
        }

        private byte[] MarshalData(object obj)
        {
            int cBytes = Marshal.SizeOf(obj);
            byte[] data = new byte[cBytes];
            GCHandle gch = GCHandle.Alloc(data, GCHandleType.Pinned);

            Marshal.StructureToPtr(obj, gch.AddrOfPinnedObject(), false);

            gch.Free();
            return data;
        }

        /// <summary>
        /// UnmarshalData - This method allows plug-in libraries to unmarshal configuration data from a byte array to an object.  
        /// This only works if the data type contains only value type or fixed size buffers of value types.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object UnmarshalData(byte[] data, Type type)
        {
            GCHandle gch = GCHandle.Alloc(data, GCHandleType.Pinned);

            object obj = Marshal.PtrToStructure(gch.AddrOfPinnedObject(), type);

            gch.Free();

            return obj;
        }

        /// <summary>
        /// MaintainConnection - TinyBooter will timeout and enter the main application if it doesn't receive a message in the allotted time.
        /// This property will start a thread that continually pings the device to maintain connection to tinyBooter.  Please note that you should 
        /// terminate the connection by setting this value to false before communicating with the device to avoid threading issues.
        /// </summary>
        public bool MaintainConnection
        {
            set
            {
                if (value)
                {
                    if (m_thread == null)
                    {
                        m_thread = new Thread(new ThreadStart(TickleWireProtocol));
                        m_thread.Start();
                    }
                }
                else
                {
                    if (m_thread != null)
                    {
                        m_thread.Abort();
                        m_thread.Join();
                        m_thread = null;
                    }
                }
            }

            get
            {
                return (m_thread != null);
            }
        }

        private void TickleWireProtocol()
        {
            while (true)
            {
                m_device.Ping();
                Thread.Sleep(2000);
            }
        }

        public byte[] DeploymentPublicKey
        {
            get
            {
                byte[] deploymentKey = new byte[TINYBOOTER_KEY_CONFIG.c_RSAKeyLength];

                unsafe
                {
                    fixed (byte* key = m_StaticConfig.PublicKeyDeployment.SectorKey)
                    {
                        for (int i = 0; i < TINYBOOTER_KEY_CONFIG.c_RSAKeyLength; i++)
                        {
                            deploymentKey[i] = key[i];
                        }
                    }
                }

                return deploymentKey;
            }
        }

        // TinyBooter uses 2 security keys one to control the deployment sector and the other for everything else (firmwareKey)
        // This method determines if the key is set for the given sector type.  
        private bool CheckKeyLocked(HAL_CONFIGURATION_SECTOR cfg, bool firmwareKey)
        {
            bool locked = false;

            unsafe
            {
                byte* key = (firmwareKey ? cfg.PublicKeyFirmware.SectorKey : cfg.PublicKeyDeployment.SectorKey);

                // check to see if the key is un-initialized (all 0xff bytes - uninitialized FLASH memory)
                for (int i = 0; i < TINYBOOTER_KEY_CONFIG.c_RSAKeyLength; i++)
                {
                    if (key[i] != 0xFF)
                    {
                        locked = true;
                        break;
                    }
                }
            }

            return locked;
        }

        // Initialize the internal structures.
        private void InitializeConfigData()
        {
            uint hal_config_block_size = 0;
            uint hal_config_static_size = 0;
            int index = 0;

            unsafe
            {
                hal_config_block_size = (uint)sizeof(HAL_CONFIG_BLOCK);
                hal_config_static_size = (uint)sizeof(HAL_CONFIGURATION_SECTOR) - hal_config_block_size;
            }

            m_cfg_sector.m_address = uint.MaxValue;
            m_cfg_sector.m_size = 0;

            _DBG.Engine engine = m_device.DbgEngine;

            if (!engine.TryToConnect(10, 100, true, Microsoft.SPOT.Debugger.ConnectionSource.Unknown))
            {
                throw new MFDeviceNoResponseException();
            }

            if(m_device.DbgEngine.PortDefinition is _DBG.PortDefinition_Tcp)
            {
                m_device.UpgradeToSsl();
            }

            _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.Reply reply = engine.GetFlashSectorMap();

            // Find the config sector
            foreach (_DBG.WireProtocol.Commands.Monitor_FlashSectorMap.FlashSectorData fsd in reply.m_map)
            {
                const uint usage_mask = _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK;
                const uint usage_cfg = _DBG.WireProtocol.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_CONFIG;

                if (usage_cfg == (fsd.m_flags & usage_mask))
                {
                    m_cfg_sector = fsd;
                    break;
                }
            }

            // read in the configuration data if the config sector was found
            if (m_cfg_sector.m_address != uint.MaxValue)
            {
                // read in the static portion of the config sector
                engine.ReadMemory( m_cfg_sector.m_address, ( uint )hal_config_static_size, out m_all_cfg_data );

                m_StaticConfig = (HAL_CONFIGURATION_SECTOR)UnmarshalData(m_all_cfg_data, typeof(HAL_CONFIGURATION_SECTOR));

                // uninitialized config sector, lets try to fix it
                if (m_StaticConfig.ConfigurationLength == 0xFFFFFFFF)
                {
                    m_StaticConfig.ConfigurationLength = ( uint )hal_config_static_size;
                    m_StaticConfig.Version.Major = 3;
                    m_StaticConfig.Version.Minor = 0;
                    m_StaticConfig.Version.Extra = 0;
                    m_StaticConfig.Version.TinyBooter = 4;
                }

                if (m_StaticConfig.ConfigurationLength >= m_cfg_sector.m_size)
                    throw new MFInvalidConfigurationSectorException();

                if (m_StaticConfig.Version.TinyBooter == 4)
                {
                    m_fStaticCfgOK = true;
                }
                else
                {
                    m_fStaticCfgOK = (m_StaticConfig.ConfigurationLength != hal_config_static_size);
                }

                // determine if we have a new or old version of the config (security keys are only supported in the new version)
                m_fValidConfig = (m_StaticConfig.Version.TinyBooter == CONFIG_SECTOR_VERSION.c_CurrentTinyBooterVersion);

                if (m_fStaticCfgOK)
                {
                    m_firmwareKeyLocked = CheckKeyLocked(m_StaticConfig, true);
                    m_deployKeyLocked = CheckKeyLocked(m_StaticConfig, false);
                }

                // move to the dynamic configuration section
                index = (int)m_StaticConfig.ConfigurationLength;

                m_lastCfgIndex = index;

                while (true)
                {
                    byte[] data;

                    // read the next configuration block
                    engine.ReadMemory((uint)(m_cfg_sector.m_address + index), hal_config_block_size, out data);

                    HAL_CONFIG_BLOCK cfg_header = (HAL_CONFIG_BLOCK)UnmarshalData(data, typeof(HAL_CONFIG_BLOCK));

                    // out of memory or last record
                    if (cfg_header.Size > m_cfg_sector.m_size)
                    {
                        // last record or bogus entry
                        m_lastCfgIndex = index;

                        // save the configuration data for later use
                        m_all_cfg_data = new byte[m_lastCfgIndex];

                        int idx = 0;
                        byte[] tmp;

                        while (idx < index)
                        {
                            int size = 512;

                            if ((index - idx) < size) size = index - idx;

                            engine.ReadMemory((uint)(m_cfg_sector.m_address + idx), (uint)size, out tmp);

                            Array.Copy(tmp, 0, m_all_cfg_data, idx, tmp.Length);

                            idx += size;
                        }

                        break; // no more configs
                    }

                    // move to the next configuration block
                    if (cfg_header.Size + hal_config_block_size + index > m_cfg_sector.m_size)
                    {
                        // end of config sector
                        break;
                    }

                    m_cfgHash[cfg_header.DriverNameString] = new ConfigIndexData(index, (int)(cfg_header.Size + hal_config_block_size));

                    index += (int)(cfg_header.Size + hal_config_block_size);

                    while (0 != (index % 4))
                    {
                        index++;
                    }
                }
            }
            m_init = true;
        }

        /// <summary>
        /// IsValidConfig - This property determines if the device configuration version matches this version of MFDeploy.
        /// </summary>
        public bool IsValidConfig
        {
            get
            {
                if (!m_init)
                {
                    try
                    {
                        InitializeConfigData();
                    }
                    catch
                    {
                    }
                }

                return (m_fValidConfig && m_fStaticCfgOK);
            }
        }

        /// <summary>
        /// Determines if the firmware security key has been programmed (and therefore requires unlocking to change)
        /// </summary>
        public bool IsFirmwareKeyLocked
        {
            get
            {
                if (!m_init)
                {
                    InitializeConfigData();
                }

                if (!m_fStaticCfgOK) throw new MFInvalidConfigurationSectorException();

                return m_firmwareKeyLocked;
            }
        }

        /// <summary>
        /// Determines if the deployment sector security key has been programmed (and therefore requires unlocking to change)
        /// </summary>
        public bool IsDeploymentKeyLocked
        {
            get
            {
                if (!m_init)
                {
                    InitializeConfigData();
                }

                if (!m_fStaticCfgOK) throw new MFInvalidConfigurationSectorException();

                return m_deployKeyLocked;
            }
        }

        /// <summary>
        /// Updates the given public signature key
        /// </summary>
        /// <param name="newKeyInfo">Information that is used to update the key: including the new key, 
        /// the new key signature (if the key has already been set), and the key index</param>
        /// <returns>true if successful false otherwise</returns>
        public bool UpdatePublicKey(PublicKeyUpdateInfo newKeyInfo)
        {
            bool fRequiresReboot = false;
            bool retVal = false;
            _DBG.PublicKeyIndex keyIndex;

            if(!m_init)
            {
                InitializeConfigData();
            }

            if (!m_init)                                                                  throw new MFInvalidConfigurationSectorException();
            if (!m_fStaticCfgOK)                                                          throw new MFInvalidConfigurationSectorException();
            if ((newKeyInfo.NewPublicKey.Length) != TINYBOOTER_KEY_CONFIG.c_RSAKeyLength) throw new MFInvalidKeyLengthException();

            if(newKeyInfo.PublicKeyIndex == PublicKeyUpdateInfo.KeyIndex.DeploymentKey)
            {
                keyIndex = _DBG.PublicKeyIndex.DeploymentKey;
                fRequiresReboot = m_deployKeyLocked;
            }
            else
            {
                keyIndex = _DBG.PublicKeyIndex.FirmwareKey;
                fRequiresReboot = m_firmwareKeyLocked;
            }

            if (m_device.DbgEngine.ConnectionSource != Microsoft.SPOT.Debugger.ConnectionSource.TinyBooter)
            {
                if (!m_device.ConnectToTinyBooter())
                {
                    throw new MFDeviceNoResponseException();
                }
            }

            if(m_device.DbgEngine.UpdateSignatureKey(keyIndex, newKeyInfo.NewPublicKeySignature, newKeyInfo.NewPublicKey, null))
            {
                // if the key was previously set it requires an erase and a write which requires a reboot on some devices
                if (fRequiresReboot)
                {
                    m_device.Reboot(true);
                }

                retVal = true;
            }

            PingConnectionType conn = m_device.Ping();
            if (PingConnectionType.TinyBooter == conn)
            {
                m_device.Execute(c_EnumerateAndLaunchAddr);
            }

            return retVal;
        }

        /// <summary>
        /// The FindConfig method searches the dynamic portion of the configuration sector for the given configuraiton.
        /// The configuration is generally used by the device HAL for changing driver configurations.  However, the configuration
        /// can be used solely by the PC application to track data.
        /// </summary>
        /// <param name="configName">Name of the configuration (determine by the HAL for updating HAL driver configurations)</param>
        /// <returns>returns null if the driver could not be found
        /// returns the byte array of the configuration otherwise (including the HAL_CONFIG_BLOCK header)</returns>
        public byte[] FindConfig( string configName )
        {
            byte[] retVal = null;

            if (!m_init)
            {
                InitializeConfigData();
            }

            // see if we have seen this configuration.
            if(m_cfgHash.ContainsKey(configName))
            {
                ConfigIndexData cid = (ConfigIndexData) m_cfgHash[configName];

                retVal = new byte[cid.Size];

                Array.Copy(m_all_cfg_data, cid.Index, retVal, 0, cid.Size);
            }

            return retVal;
        }

        /// <summary>
        /// The WriteConfig method is used to update or create a device configuration.  If the name of the configuration exists 
        /// on the device, then the configuration is updated.  Otherwise, a new configuration is added.
        /// </summary>
        /// <param name="configName">Unique case-sensitive name of the configuration</param>
        /// <param name="config">The configuration object to be written</param>
        public void WriteConfig(string configName, IHAL_CONFIG_BASE config)
        {
            WriteConfig(configName, config, true);
        }

        public void WriteConfig(string configName, IHAL_CONFIG_BASE config, bool updateConfigSector)
        {
            uint hal_config_block_size = 0;

            HAL_CONFIG_BLOCK header = config.ConfigHeader;

            unsafe
            {
                hal_config_block_size = (uint)sizeof(HAL_CONFIG_BLOCK);

                header.DriverNameString = configName;
            }

            // set up the configuration data
            header.HeaderCRC = 0;
            header.DataCRC = 0;
            header.Size = (uint)config.Size - hal_config_block_size;
            header.Signature = c_Version_V2;

            config.ConfigHeader = header;

            // calculate the data crc 
            byte[] data = MarshalData(config);
            header.DataCRC = CRC.ComputeCRC(data, (int)hal_config_block_size, (int)(header.Size/* - hal_config_block_size*/), 0);
            // this enables the data type to update itself with the crc (required because there is no class inheritence in structs and therefore no polymorphism)
            config.ConfigHeader = header;

            // calculate the header crc
            data = MarshalData(config);
            header.HeaderCRC = CRC.ComputeCRC(data, 2 * sizeof(UInt32), (int)hal_config_block_size - (2 * sizeof(UInt32)), c_Seed);
            // this enables the data type to update itself with the crc (required because there is no class inheritence in structs and therefore no polymorphism)
            config.ConfigHeader = header;

            data = MarshalData(config);

            WriteConfig(configName, data, true, updateConfigSector);
        }

        /// <summary>
        /// The WriteConfig method is used to update or create a device configuration.  If the name of the configuration exists 
        /// on the device, then the configuration is updated.  Otherwise, a new configuration is added.
        /// </summary>
        /// <param name="configName">Unique case-sensitive name of the configuration</param>
        /// <param name="data">Data to be written for the given name (not including the header)</param>
        public void WriteConfig(string configName, byte[] data)
        {
            uint hal_config_block_size = 0;
            HAL_CONFIG_BLOCK header = new HAL_CONFIG_BLOCK();

            // Create a header for the configuration data
            unsafe
            {
                hal_config_block_size = (uint)sizeof(HAL_CONFIG_BLOCK);

                header.DriverNameString = configName;
            }
            header.HeaderCRC = 0;
            header.DataCRC = CRC.ComputeCRC(data, 0, data.Length, 0); ;
            header.Size = (uint)data.Length;
            header.Signature = c_Version_V2;

            // Calculate CRC information for header and data
            header.DataCRC = CRC.ComputeCRC(data, 0, (int)data.Length, 0);

            byte[] headerBytes = MarshalData(header);
            header.HeaderCRC = CRC.ComputeCRC(headerBytes, (2 * sizeof(UInt32)), (int)hal_config_block_size - (2 * sizeof(UInt32)), c_Seed);
            headerBytes = MarshalData(header);

            // Concatonate the header and data
            byte[] allData = new byte[hal_config_block_size + data.Length];

            Array.Copy(headerBytes, allData, hal_config_block_size);
            Array.Copy(data, 0, allData, hal_config_block_size, data.Length);

            WriteConfig(configName, allData, false, true);
        }

        // Write the concatonated header and configuration data to the Flash config sector
        private void WriteConfig(string configName, byte[] data, bool staticSize, bool updateConfigSector)
        {
            _DBG.Engine engine = m_device.DbgEngine;

            if (!m_init)
            {
                InitializeConfigData();
            }

            // updating the config
            if (m_cfgHash.ContainsKey(configName))
            {
                ConfigIndexData cid = (ConfigIndexData)m_cfgHash[configName];

                // If old and new data are different sizes
                if (cid.Size != data.Length)
                {
                    // If data comes from a well defined structure, its size cannot vary
                    if (staticSize)                                                           throw new MFInvalidConfigurationDataException();

                    uint newNextIndex, oldNextIndex;
                    byte[] temp;
                    int diff = 0;

                    // Figure out where any following configuration data will start
                    newNextIndex = (uint)(cid.Index + data.Length);
                    while (0 != (newNextIndex % 4))
                    {
                        newNextIndex++;        // Force a 4 byte boundary
                    }

                    // Figure out where any following configuration data previously started
                    oldNextIndex = (uint)(cid.Index + cid.Size);
                    while (0 != (oldNextIndex % 4))
                    {
                        oldNextIndex++;        // Force a 4 byte boundary
                    }


                    diff = (int)newNextIndex - (int)oldNextIndex;           // Find the adjusted difference in size between old and new config data
                    temp = new byte[m_lastCfgIndex + diff];                 // Create a new byte array to contain all the configuration data

                    Array.Copy(m_all_cfg_data, temp, cid.Index);            // Copy all preceding data to new array
                    Array.Copy(data, 0, temp, cid.Index, data.Length);      // Copy new configuration to new array
                    if (oldNextIndex < m_lastCfgIndex)                      // Copy all following data (if it exists) to new array
                    {
                        Array.Copy(m_all_cfg_data, oldNextIndex, temp, newNextIndex, (m_all_cfg_data.Length - oldNextIndex));
                    }

                    // Update the local copy of the configuration list
                    m_all_cfg_data = temp;
                    m_lastCfgIndex += diff;
                }
                else
                {
                    // Copy the new configuration data on top of the old
                    Array.Copy(data, 0, m_all_cfg_data, cid.Index, data.Length);
                }
            }
            else        // adding a new configuration to the end of the current list
            {
                uint newLastIndex;

                if (m_lastCfgIndex == -1) throw new MFConfigurationSectorOutOfMemoryException();

                // Find the new size of the whole configuration list
                newLastIndex = (uint)(m_lastCfgIndex + data.Length);

                while (0 != (newLastIndex % 4))
                {
                    newLastIndex++;        // Force a 4 byte boundary
                }

                byte[] temp = new byte[m_lastCfgIndex + data.Length >= m_all_cfg_data.Length ? m_lastCfgIndex + data.Length : m_all_cfg_data.Length];

                Array.Copy(m_all_cfg_data, 0, temp,              0, m_all_cfg_data.Length);
                Array.Copy(          data, 0, temp, m_lastCfgIndex,           data.Length);

                // Update the local copy of the configuration list
                m_all_cfg_data = temp;
                m_lastCfgIndex = (int)newLastIndex;
            }

            if (!updateConfigSector) return;

            // Rewrite entire configuration list to Flash
            if (!engine.EraseMemory(m_cfg_sector.m_address, (uint)m_all_cfg_data.Length)) throw new MFConfigSectorEraseFailureException();
            if (!engine.WriteMemory(m_cfg_sector.m_address, m_all_cfg_data))              throw new MFConfigSectorWriteFailureException();

            // Rebuild hash table
            m_cfgHash.Clear();
            uint hal_config_block_size = 0;
            unsafe
            {
                hal_config_block_size = (uint)sizeof(HAL_CONFIG_BLOCK);
            }
            int index = (int)m_StaticConfig.ConfigurationLength;
            byte[] headerData = new byte[hal_config_block_size];
            HAL_CONFIG_BLOCK cfg_header;
            while (index < m_lastCfgIndex)
            {
                // Read in next configuration header
                Array.Copy(m_all_cfg_data, index, headerData, 0, hal_config_block_size);
                cfg_header = (HAL_CONFIG_BLOCK)UnmarshalData(headerData, typeof(HAL_CONFIG_BLOCK));

                m_cfgHash[cfg_header.DriverNameString] = new ConfigIndexData(index, (int)(cfg_header.Size + hal_config_block_size));

                // Index of next configuration header must lie on a 4 byte boundary
                index += (int)(cfg_header.Size + hal_config_block_size);
                while (0 != (index % 4))
                {
                    index++;        // Force a 4 byte boundary
                }
            }

            // we need to perform signature check regardless of key update, in order for the device to write from ram buffer to flash
            if (!engine.CheckSignature(new byte[TINYBOOTER_KEY_CONFIG.c_KeySignatureLength], 0))
            {
                if(engine.ConnectionSource == Microsoft.SPOT.Debugger.ConnectionSource.TinyBooter) throw new MFConfigSectorWriteFailureException();
            }

            if (engine.ConnectionSource == Microsoft.SPOT.Debugger.ConnectionSource.TinyBooter && m_fRestartClr)
            {
                engine.ExecuteMemory(c_EnumerateAndLaunchAddr);
            }
        }

        internal void SwapAllConfigData(MFConfigHelper srcConfigHelper)
        {
            byte[] newAllConfigData = srcConfigHelper.m_all_cfg_data;

            if (newAllConfigData == null)
                throw new ArgumentNullException();

            if (m_all_cfg_data != null)
            {
                if (m_all_cfg_data.Length != newAllConfigData.Length)
                    throw new ArgumentException("Invalid swap target");
            }

            m_all_cfg_data = newAllConfigData;
        }
    }
}
