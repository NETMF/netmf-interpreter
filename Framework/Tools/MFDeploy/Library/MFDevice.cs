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
using System.IO;
using _DBG = Microsoft.SPOT.Debugger;
using _WP = Microsoft.SPOT.Debugger.WireProtocol;
using System.Threading;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
    [Flags]
    public enum EraseOptions
    {
        Deployment    = 0x01,
        UserStorage   = 0x02,
        FileSystem    = 0x04,
        Firmware      = 0x08,
        UpdateStorage = 0x10,
        SimpleStorage = 0x20,
    }

    public enum PingConnectionType
    {
        TinyCLR,
        TinyBooter,
        NoConnection,
        MicroBooter,
    }

    public class DebugOutputEventArgs : EventArgs
    {
        private string m_txt;

        public DebugOutputEventArgs(string text)
        {
            m_txt = text;
        }

        public string Text { get{ return m_txt; } }
    }

    public class OemMonitorInfo
    {
        public OemMonitorInfo(_WP.Commands.Monitor_OemInfo.Reply reply)
        {
            m_releaseInfo = reply.m_releaseInfo;
        }
        
        public Version Version
        {
            get { return m_releaseInfo.Version; }
        }
        
        public string OemString
        {
            get { return m_releaseInfo.Info; }
        }

        public override string ToString()
        {
            return String.Format("Bootloader build info: {0}\nVersion {1}\n", OemString, Version);
        }

        public bool Valid
        {
            get { return true; }
        }

        private _DBG.WireProtocol.ReleaseInfo m_releaseInfo;
    }

    public class MFDevice : IDisposable
    {
        private _DBG.Engine m_eng;
        private _DBG.PortDefinition m_port;
        private _DBG.PortDefinition m_portTinyBooter;
        private bool disposed;
        private string m_CurrentNoise = "";

        public delegate void OnProgressHandler(long value, long total, string status);
        public event OnProgressHandler OnProgress;
        public AutoResetEvent EventCancel = new AutoResetEvent(false);
        private AutoResetEvent m_evtMicroBooter = new AutoResetEvent(false);
        private AutoResetEvent m_evtMicroBooterError = new AutoResetEvent(false);
        private ManualResetEvent m_evtMicroBooterStart = new ManualResetEvent(false);

        private Dictionary<uint, string> m_execSrecHash = new Dictionary<uint, string>();

        private Dictionary<uint, int> m_srecHash = new Dictionary<uint, int>();

        private X509Certificate2 m_serverCert = null;
        private bool m_requireClientCert = false;

        private MFDevice()
        {
        }

        ~MFDevice()
        {
            Dispose(false);
        }

        internal _DBG.Engine DbgEngine 
        { 
            get 
            {
                if (m_eng == null)
                {
                    Connect(500, true);
                }

                return m_eng; 
            } 
        }

        private Regex m_srecExpr = new Regex(@"<MB>([\d\w]+)</MB>");

        private void OnNoiseHandler(byte[] data, int index, int count)
        {
            string text = System.Text.ASCIIEncoding.ASCII.GetString(data, index, count);

            if (m_evtMicroBooterStart.WaitOne(0))
            {
                m_CurrentNoise += text;
                int idxEnd = 0;

                Match m = m_srecExpr.Match(m_CurrentNoise);

                while(m.Success)
                {
                    string key = m.Groups[1].Value;

                    if (string.Compare(key, "ERROR", true) == 0)
                    {
                        m_evtMicroBooterError.Set();
                    }
                    else
                    {
                        try
                        {
                            uint addr;

                            if (uint.TryParse(key, System.Globalization.NumberStyles.HexNumber, null, out addr))
                            {
                                if (!m_srecHash.ContainsKey(addr) && m_minSrecAddr <= addr && m_maxSrecAddr >= addr)
                                {
                                    m_srecHash[addr] = 1;

                                    if (OnProgress != null)
                                    {
                                        OnProgress(m_srecHash.Count, m_totalSrecs, string.Format(Properties.Resources.StatusFlashing, key));
                                    }

                                }
                            }
                            else
                            {
                                Console.WriteLine(key);
                            }

                            m_evtMicroBooter.Set();
                        }
                        catch { }
                    }

                    idxEnd = m.Index + m.Length;

                    m = m.NextMatch();
                }

                m_CurrentNoise = m_CurrentNoise.Substring(idxEnd);
            }
            else
            {
                m_CurrentNoise = "";

                if (OnDebugText != null)
                {
                    OnDebugText(this, new DebugOutputEventArgs(text));
                }
            }
        }
        public void OnMessage(_DBG.WireProtocol.IncomingMessage msg, string text)
        {
            if (OnDebugText != null)
            {
                OnDebugText(this, new DebugOutputEventArgs(text));
            }
        }
        private void PrepareForDeploy(ArrayList blocks)
        {
            const uint c_DeploySector = _WP.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_DEPLOYMENT;
            const uint c_SectorUsageMask = _WP.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK;

            bool fEraseDeployment     = false;

            // if vsdebug is not enabled then we cannot write/erase
            if (!IsClrDebuggerEnabled())
            {
                if (OnProgress != null) OnProgress(0, 1, Properties.Resources.StatusConnectingToTinyBooter);

                // only check for signature file if we are uploading firmware
                if (!ConnectToTinyBooter()) throw new MFDeviceNoResponseException();
            }

            _WP.Commands.Monitor_FlashSectorMap.Reply map = m_eng.GetFlashSectorMap();

            if (map == null) throw new MFDeviceNoResponseException();

            foreach (_DBG.SRecordFile.Block bl in blocks)
            {
                foreach (_WP.Commands.Monitor_FlashSectorMap.FlashSectorData sector in map.m_map)
                {
                    if (sector.m_address == bl.address)
                    {
                        // only support writing with CLR to the deployment sector and RESERVED sector (for digi)
                        if (c_DeploySector == (c_SectorUsageMask & sector.m_flags))
                        {
                            fEraseDeployment        = true;
                        }
                        else
                        {
                            if (m_eng.ConnectionSource != _DBG.ConnectionSource.TinyBooter)
                            {
                                if (OnProgress != null) OnProgress(0, 1, Properties.Resources.StatusConnectingToTinyBooter);

                                // only check for signature file if we are uploading firmware
                                if (!ConnectToTinyBooter()) throw new MFDeviceNoResponseException();
                            }
                        }
                        break;
                    }
                }
            }
            if (fEraseDeployment)
            {
                this.Erase(EraseOptions.Deployment);
            }
            else if(m_eng.ConnectionSource != _DBG.ConnectionSource.TinyBooter) 
            {
                //if we are not writing to the deployment sector then assure that we are talking with TinyBooter
                ConnectToTinyBooter();
            }
            if (m_eng.ConnectionSource == _DBG.ConnectionSource.TinyCLR)
            {
                m_eng.PauseExecution();
            }
        }

        internal MFDevice(_DBG.PortDefinition port, _DBG.PortDefinition tinyBooterPort)
        {
            m_port = port;
            m_portTinyBooter = tinyBooterPort;
        }

        internal bool CheckForMicroBooter()
        {
            if(m_eng == null) return false;

            try
            {
                m_evtMicroBooterStart.Set();
                m_evtMicroBooterError.Reset();

                // try to see if we are connected to MicroBooter
                for (int retry = 0; retry < 5; retry++)
                {
                    m_eng.SendRawBuffer(System.Text.ASCIIEncoding.ASCII.GetBytes("xx\n"));

                    if (m_evtMicroBooterError.WaitOne(100))
                    {
                        return true;
                    }
                }
            }
            finally
            {
                m_evtMicroBooterStart.Reset();
            }

            return false;
        }

        internal bool ConnectTo(int timeout_ms, bool tryToConnect, _DBG.ConnectionSource target)
        {
            int retries = m_port is _DBG.PortDefinition_Tcp ? 2 : timeout_ms / 300;
            if (retries == 0) retries = 1;

            try
            {
                if (m_eng == null)
                {
                    m_eng = new _DBG.Engine(m_port);

                    m_eng.OnNoise += new _DBG.NoiseEventHandler(OnNoiseHandler);
                    m_eng.OnMessage += new _DBG.MessageEventHandler(OnMessage);

                    m_eng.Start();
                }

                if (tryToConnect)
                {
                    for (int j = retries; j > 0; j--)
                    {
                        switch (target)
                        {
                            case _DBG.ConnectionSource.MicroBooter:
                                if (CheckForMicroBooter()) return true;
                                Thread.Sleep(timeout_ms / retries);
                                break;

                            default:
                                if (m_eng.TryToConnect(0, timeout_ms / retries, true, _DBG.ConnectionSource.Unknown))
                                {
                                    if (m_eng.ConnectionSource == _DBG.ConnectionSource.TinyCLR)
                                    {
                                        m_eng.UnlockDevice(m_data);
                                    }
                                    else if (m_eng.ConnectionSource == _DBG.ConnectionSource.TinyBooter)
                                    {
                                        if (target == _DBG.ConnectionSource.TinyCLR)
                                        {
                                            m_eng.ExecuteMemory(0);
                                            Thread.Sleep(100);
                                        }
                                    }
                                    break;
                                }

                                if (EventCancel.WaitOne(0, false)) throw new MFUserExitException();
                                break;
                        }
                        if (m_eng.IsConnected && target == m_eng.ConnectionSource)
                        {
                            break;
                        }
                    }

                    if (target != m_eng.ConnectionSource)
                    {
                        Disconnect();
                    }
                }

            }
            catch (ThreadAbortException)
            {
            }
            catch (MFUserExitException)
            {
                Disconnect();
                throw;
            }
            catch
            {
                Disconnect();
            }

            return (m_eng != null && (!tryToConnect || m_eng.IsConnected));
        }

        internal bool Connect( int timeout_ms, bool tryConnect )
        {
            // to use user cancel event, so that cancel button is more responsive
            int retries = m_port is _DBG.PortDefinition_Tcp ? 2 : timeout_ms/100;
            int loops = 1;
            
            if (retries == 0) retries = 1;

            if (m_portTinyBooter != null && m_port.UniqueId != m_portTinyBooter.UniqueId)
            {
                retries /= 2;
                loops   =  2;
            }
            for (int i = 0; i < loops; i++)
            {
                _DBG.PortDefinition pd = i == 0 ? m_port : m_portTinyBooter;

                if (EventCancel.WaitOne(0, false)) throw new MFUserExitException();

                try
                {
                    if (m_eng == null)
                    {
                        m_eng = new _DBG.Engine(pd);

                        m_eng.OnNoise += new _DBG.NoiseEventHandler(OnNoiseHandler);
                        m_eng.OnMessage += new _DBG.MessageEventHandler(OnMessage);

                        m_eng.Start();
                    }

                    if (tryConnect)
                    {
                        if (CheckForMicroBooter())
                            return true;

                        for (int j = retries; j > 0; j--)
                        {
                            if (m_eng.TryToConnect(0, timeout_ms / retries, true, _DBG.ConnectionSource.Unknown))
                            {
                                //UNLOCK DEVICE in secure way?
                                m_eng.UnlockDevice(m_data);
                                break;
                            }

                            if (EventCancel.WaitOne(0, false)) throw new MFUserExitException();
                        }
                        if (m_eng.IsConnected)
                        {
                            /*
                            if (m_serverCert != null && m_eng.UpgradeConnectionToSsl(0, m_serverCert, m_requireClientCert))
                            {
                                if (OnDebugText != null)
                                {
                                    OnDebugText(this, new DebugOutputEventArgs("Using SSL Connection!!!"));
                                }
                            }
                            */

                            break;
                        }
                        else
                        {
                            Disconnect();
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (ThreadAbortException)
                {
                }
                catch (MFUserExitException)
                {
                    Disconnect();
                    throw;
                }
                catch
                {
                    Disconnect();
                }
            }
            return (m_eng != null && (!tryConnect || m_eng.IsConnected));
        }
        internal bool Disconnect()
        {
            if (m_eng != null)
            {
                m_eng.OnNoise   -= new _DBG.NoiseEventHandler(OnNoiseHandler);
                m_eng.OnMessage -= new _DBG.MessageEventHandler(OnMessage);

                m_eng.Stop();
                m_eng.Dispose();
                m_eng = null;
            }
            return true;
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // release managed components
                    Disconnect();
                }
                
                disposed = true;
            }
        }

        private bool IsClrDebuggerEnabled()
        {
            try
            {
                if (m_eng.IsConnectedToTinyCLR)
                {
                    return (m_eng.Capabilities.SourceLevelDebugging);
                }
            }
            catch
            {
            }
            return false;
        }

        /// <summary>
        /// Event for notifying user of debug text coming from the device.
        /// </summary>
        public event EventHandler<DebugOutputEventArgs> OnDebugText;
        /// <summary>
        /// Standard Dispose method for releasing resources such as the connection to the device.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public bool UseSsl( X509Certificate2 cert, bool fRequireClientCert )
        {
            m_serverCert = cert;
            m_requireClientCert = fRequireClientCert;

            /*
            if (m_serverCert != null && m_eng != null && m_eng.IsConnected)
            {
                if (m_eng.UpgradeConnectionToSsl(0, m_serverCert, m_requireClientCert))
                {
                    if (OnDebugText != null)
                    {
                        OnDebugText(this, new DebugOutputEventArgs("Using SSL Connection!!!"));
                    }

                    return true;
                }
            }
            */

            return false;
        }

        /// <summary>
        /// Attempt to establish a connection with TinyBooter (with reboot if necessary)
        /// </summary>
        /// <returns>true connection was made, false otherwise</returns>
        public bool ConnectToTinyBooter()
        {
            bool ret = false;

            if (!Connect(500, true)) return false;

            if (m_eng != null)
            {
                if (m_eng.ConnectionSource == _DBG.ConnectionSource.TinyBooter) return true;

                m_eng.RebootDevice(_DBG.Engine.RebootOption.EnterBootloader);

                // tinyBooter is only com port so
                if (m_port is _DBG.PortDefinition_Tcp)
                {
                    _DBG.PortDefinition pdTmp = m_port;

                    Disconnect();

                    try
                    {
                        m_port = m_portTinyBooter;

                        // digi takes forever to reset
                        if (!Connect(60000, true))
                        {
                            Console.WriteLine(Properties.Resources.ErrorUnableToConnectToTinyBooterSerial);
                            return false;
                        }
                    }
                    finally
                    {
                        m_port = pdTmp;
                    }
                }
                bool fConnected = false;
                for(int i = 0; i<40; i++)
                {
                    if (EventCancel.WaitOne(0, false)) throw new MFUserExitException();

                    if (fConnected = m_eng.TryToConnect(0, 500, true, _DBG.ConnectionSource.Unknown))
                    {
                        _WP.Commands.Monitor_Ping.Reply reply = m_eng.GetConnectionSource();
                        ret = (reply.m_source == _WP.Commands.Monitor_Ping.c_Ping_Source_TinyBooter);

                        break;
                    }
                }
                if(!fConnected)
                {
                    Console.WriteLine(Properties.Resources.ErrorUnableToConnectToTinyBooter);
                }
            }
            return ret;
        }
        
        /// <summary>
        /// Erases the deployment sectors of the connected .Net Micro Framework device
        /// </summary>
        /// <param name="options">Identifies which areas are to be erased, if no options are given, all 
        /// user sectors will be erased.
        /// </param>
        /// <returns>Returns false if the erase fails, true otherwise
        /// Possible exceptions: MFUserExitException, MFDeviceNoResponseException
        /// </returns>
        public bool Erase(params EraseOptions[] options)
        {
            bool ret = false;
            bool fReset = false;
            if (m_eng == null) throw new MFDeviceNoResponseException();
            EraseOptions optionFlags = 0;

            if (options == null || options.Length == 0)
            {
                optionFlags = (EraseOptions.Deployment | EraseOptions.FileSystem | EraseOptions.UserStorage | EraseOptions.SimpleStorage | EraseOptions.UpdateStorage);
            }
            else
            {
                foreach (EraseOptions opt in options)
                {
                    optionFlags |= opt;
                }
            }

            if (!Connect(500, true))
            {
                throw new MFDeviceNoResponseException();
            }

            if (!IsClrDebuggerEnabled() || 0 != (optionFlags & EraseOptions.Firmware))
            {
                fReset = (Ping() == PingConnectionType.TinyCLR);
                if (!ConnectToTinyBooter())
                {
                    throw new MFTinyBooterConnectionFailureException();
                }
            }

            _WP.Commands.Monitor_FlashSectorMap.Reply reply = m_eng.GetFlashSectorMap();

            if (reply == null) throw new MFDeviceNoResponseException();

            _WP.Commands.Monitor_Ping.Reply ping = m_eng.GetConnectionSource();

            ret = true;


            long total = 0;
            long value = 0;

            bool isConnectedToCLR = ((ping != null) && (ping.m_source == _WP.Commands.Monitor_Ping.c_Ping_Source_TinyCLR));


            if (isConnectedToCLR)
            {
                m_eng.PauseExecution();
            }
            
            List<_WP.Commands.Monitor_FlashSectorMap.FlashSectorData> eraseSectors = new List<_WP.Commands.Monitor_FlashSectorMap.FlashSectorData>();

            foreach (_WP.Commands.Monitor_FlashSectorMap.FlashSectorData fsd in reply.m_map)
            {
                if (EventCancel.WaitOne(0, false)) throw new MFUserExitException();

                switch (fsd.m_flags & _WP.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK)
                {
                    case _WP.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_DEPLOYMENT:
                        if (EraseOptions.Deployment == (optionFlags & EraseOptions.Deployment))
                        {
                            eraseSectors.Add(fsd);
                            total++;
                        }
                        break;
                    case _WP.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_UPDATE:
                        if (EraseOptions.UpdateStorage == (optionFlags & EraseOptions.UpdateStorage))
                        {
                            eraseSectors.Add(fsd);
                            total++;
                        }
                        break;

                    case _WP.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_SIMPLE_A:
                    case _WP.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_SIMPLE_B:
                        if (EraseOptions.SimpleStorage == (optionFlags & EraseOptions.SimpleStorage))
                        {
                            eraseSectors.Add(fsd);
                            total++;
                        }
                        break;

                    case _WP.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_STORAGE_A:
                    case _WP.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_STORAGE_B:
                        if (EraseOptions.UserStorage == (optionFlags & EraseOptions.UserStorage))
                        {
                            eraseSectors.Add(fsd);
                            total++;
                        }
                        break;

                    case _WP.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_FS :
                        if (EraseOptions.FileSystem == (optionFlags & EraseOptions.FileSystem))
                        {
                            eraseSectors.Add(fsd);
                            total++;
                        }
                        break;

                    case _WP.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_CONFIG:
                    case _WP.Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_CODE:
                        if (EraseOptions.Firmware == (optionFlags & EraseOptions.Firmware))
                        {
                            eraseSectors.Add(fsd);
                            total++;
                        }
                        break;
                }

            }

            foreach (_WP.Commands.Monitor_FlashSectorMap.FlashSectorData fsd in eraseSectors)
            {
                if (OnProgress != null) OnProgress(value, total, string.Format(Properties.Resources.StatusEraseSector, fsd.m_address));

                ret &= m_eng.EraseMemory(fsd.m_address, fsd.m_size);

                value++;
            }

            // reset if we specifically entered tinybooter for the erase
            if(fReset)
            {
                m_eng.ExecuteMemory(0);
            }
            // reboot if we are talking to the clr
            if (isConnectedToCLR)
            {
                if (OnProgress != null) OnProgress(0, 0, Properties.Resources.StatusRebooting);
                
                m_eng.RebootDevice(_DBG.Engine.RebootOption.RebootClrOnly);
            }

            return ret;
        }
        /// <summary>
        /// Attempts to talk to the connected .Net Micro Framework device
        /// </summary>
        /// <returns>returns ConnectionType if the device was responsive, false otherwise</returns>
        public PingConnectionType Ping()
        {            
            PingConnectionType ret = PingConnectionType.NoConnection;
            if (m_eng == null)
            {
                Connect(1000, true);
            }
            else
            {
                m_eng.OnNoise -= new _DBG.NoiseEventHandler(OnNoiseHandler);
                m_eng.OnMessage -= new _DBG.MessageEventHandler(OnMessage);

                m_eng.OnNoise += new _DBG.NoiseEventHandler(OnNoiseHandler);
                m_eng.OnMessage += new _DBG.MessageEventHandler(OnMessage);
            }

            if (m_eng != null)
            {
                try
                {
                    if (CheckForMicroBooter())
                    {
                        return PingConnectionType.MicroBooter;
                    }

                    _WP.Commands.Monitor_Ping.Reply reply = m_eng.GetConnectionSource();

                    if (reply != null)
                    {
                        switch (reply.m_source)
                        {
                            case _WP.Commands.Monitor_Ping.c_Ping_Source_TinyCLR:
                                ret = PingConnectionType.TinyCLR;
                                break;
                            case _WP.Commands.Monitor_Ping.c_Ping_Source_TinyBooter:
                                ret = PingConnectionType.TinyBooter;
                                break;
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                }
                catch
                {
                    Disconnect();
                    //Connect(1000, true);
                }
            }
            return ret;
        }

        /// <summary>
        /// Gets TinyBooter's OEM-specified OEMInfo string and compile-time build version
        /// number. 
        /// </summary>
        /// <returns>returns an OemMonitorInfo object, or null if we are not connected or 
        /// connected to the CLR rather than TinyBooter.</returns>
        public OemMonitorInfo GetOemMonitorInfo()
        {
            if (m_eng == null || !m_eng.IsConnected || m_eng.IsConnectedToTinyCLR) return null;
            
            _WP.Commands.Monitor_OemInfo.Reply reply = m_eng.GetMonitorOemInfo();
            return reply == null ? null : new OemMonitorInfo(reply);
        }

        private bool PreProcesSREC(string srecFile)
        {
            if (!File.Exists(srecFile)) return false;

            try
            {
                using (TextReader tr = File.OpenText(srecFile))
                {
                    using (TextWriter tw = File.CreateText(srecFile + ".ext"))
                    {
                        const int c_MaxRecords = 8; 
                        int iRecord = 0;
                        int currentCRC = 0;
                        int iDataLength = 0;
                        string s7rec = "";
                        StringBuilder sb = new StringBuilder();

                        while (tr.Peek() != -1)
                        {
                            string s1 = tr.ReadLine();

                            // we only support s7, s3 records
                            if (s1.ToLower().StartsWith("s7"))
                            {
                                s7rec = s1;
                                continue;
                            }

                            if (!s1.ToLower().StartsWith("s3")) continue;

                            string crcData;

                            if (iRecord == 0)
                            {
                                crcData = s1.Substring(4, s1.Length - 6);
                            }
                            else
                            {
                                crcData = s1.Substring(12, s1.Length - 14);
                            }
                            
                            iDataLength += crcData.Length / 2; // 2 chars per byte

                            if (iRecord == 0)
                            {
                                sb.Append(s1.Substring(0, 2));
                            }
                            sb.Append(crcData);

                            iRecord++;

                            for (int i = 0; i < crcData.Length - 1; i += 2)
                            {
                                currentCRC += Byte.Parse(crcData.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
                            }

                            if (iRecord == c_MaxRecords)
                            {
                                iDataLength += 1; // crc

                                sb = sb.Insert(2, string.Format("{0:X02}", iDataLength));

                                currentCRC += (iDataLength & 0xFF) + ((iDataLength >> 8) & 0xFF);

                                // write crc
                                sb.Append(string.Format("{0:X02}", (0xFF - (0xFF & currentCRC))));

                                tw.WriteLine(sb.ToString());

                                currentCRC  = 0;
                                iRecord     = 0;
                                iDataLength = 0;
                                sb.Length   = 0;
                            }
                        }

                        if (iRecord != 0)
                        {
                            iDataLength += 1; // crc

                            sb = sb.Insert(2, string.Format("{0:X02}", iDataLength));

                            currentCRC += (iDataLength & 0xFF) + ((iDataLength >> 8) & 0xFF);

                            // write crc
                            sb.Append(string.Format("{0:X02}", (0xFF - (0xFF & currentCRC))));

                            tw.WriteLine(sb.ToString());
                        }

                        if (s7rec != "")
                        {
                            tw.WriteLine(s7rec);
                        }
                    }
                }
            }
            catch
            {
                if (File.Exists(srecFile + ".ext"))
                {
                    File.Delete(srecFile + ".ext");
                }
                return false;
            }

            return true;
        }

        private Dictionary<uint, string> ParseSrecFile(string srecFile, out uint entryPoint, out uint imageSize)
        {
            entryPoint = 0;
            imageSize = 0;
            Dictionary<uint, string> recs = new Dictionary<uint, string>();

            if (!File.Exists(srecFile)) return null;

            FileInfo fi = new FileInfo(srecFile);

            try
            {
                int total = 0;
                using (TextReader tr = File.OpenText(srecFile))
                {
                    while (tr.Peek() != -1)
                    {
                        string s1 = tr.ReadLine();

                        string addr = s1.Substring(4, 8);

                        // we only support s7, s3 records
                        if (s1.ToLower().StartsWith("s7"))
                        {
                            entryPoint = uint.Parse(addr, System.Globalization.NumberStyles.HexNumber);
                        }
                        else if (s1.ToLower().StartsWith("s3"))
                        {
                            total += s1.Length - 14;
                            recs[uint.Parse(addr, System.Globalization.NumberStyles.HexNumber)] = s1;
                        }
                    }
                }

                imageSize = (uint)total;
            }
            catch
            {
                return null;
            }

            return recs;
        }

        private int m_totalSrecs = 0;
        private uint m_minSrecAddr = 0;
        private uint m_maxSrecAddr = 0;

        private bool DeploySREC(string srecFile, ref uint entryPoint)
        {
            entryPoint = 0;
            uint imageSize = 0;
            m_srecHash.Clear();
            m_execSrecHash.Clear();
            m_totalSrecs = 0;
            m_minSrecAddr = uint.MaxValue;
            m_maxSrecAddr = 0;

            if (File.Exists(srecFile))
            {
                if (File.Exists(srecFile + ".ext"))
                {
                    File.Delete(srecFile + ".ext");
                }

                if (PreProcesSREC(srecFile) && File.Exists(srecFile + ".ext"))
                {
                    srecFile += ".ext";
                }

                Dictionary<uint, string> recs = ParseSrecFile(srecFile, out entryPoint, out imageSize);

                try
                {
                    int sleepTime = 5000;
                    UInt32 imageAddr = 0xFFFFFFFF;

                    m_totalSrecs = recs.Count;

                    m_evtMicroBooterStart.Set();
                    m_evtMicroBooter.Reset();
                    m_evtMicroBooterError.Reset();

                    while(recs.Count > 0)
                    {
                        List<uint> remove = new List<uint>();

                        const int c_MaxPipeline = 4;
                        int pipe = c_MaxPipeline;

                        uint []keys = new uint[recs.Count];
                        
                        recs.Keys.CopyTo(keys, 0);

                        Array.Sort<uint>(keys);

                        if (keys[0] < imageAddr) imageAddr = keys[0];

                        foreach(uint key in keys)
                        {
                            if (key < m_minSrecAddr) m_minSrecAddr = key;
                            if (key > m_maxSrecAddr) m_maxSrecAddr = key;
                            if (m_srecHash.ContainsKey(key))
                            {
                                remove.Add(key);
                                continue;
                            }

                            m_eng.SendRawBuffer(System.Text.ASCIIEncoding.ASCII.GetBytes("\n"));

                            m_eng.SendRawBuffer(System.Text.ASCIIEncoding.ASCII.GetBytes(recs[key]));

                            m_eng.SendRawBuffer(System.Text.ASCIIEncoding.ASCII.GetBytes("\n"));

                            if (pipe-- <= 0)
                            {
                                m_evtMicroBooter.WaitOne(sleepTime);
                                pipe = c_MaxPipeline;
                            }
                        }

                        int cnt = remove.Count;

                        if(cnt > 0)
                        {
                            for(int i=0; i<cnt; i++)
                            {
                                recs.Remove(remove[i]);
                            }
                        }
                    }


                    if (imageAddr != 0)
                    {
                        string basefile = Path.GetFileNameWithoutExtension(srecFile);

                        // srecfile might be .bin.ext (for srec updates)
                        if (!string.IsNullOrEmpty(Path.GetExtension(basefile)))
                        {
                            basefile = Path.GetFileNameWithoutExtension(basefile);
                        }
                        string path = Path.GetDirectoryName(srecFile);
                        string binFile = "";
                        string symdefFile = "";

                        if (path.ToLower().EndsWith("\\tinyclr.hex"))
                        {
                            binFile    = Path.GetDirectoryName(path) + "\\tinyclr.bin\\" + basefile;
                            symdefFile = Path.GetDirectoryName(path) + "\\tinyclr.symdefs";
                        }
                        else
                        {
                            binFile    = Path.GetDirectoryName(srecFile) + "\\" + basefile + ".bin";
                            symdefFile = Path.GetDirectoryName(srecFile) + "\\" + basefile + ".symdefs";
                        }

                        // send image crc
                        if (File.Exists(binFile) && File.Exists(symdefFile))
                        {
                            FileInfo fiBin = new FileInfo(binFile);
                            UInt32 imageCRC = 0;

                            using (TextReader trSymdef = File.OpenText(symdefFile))
                            {
                                while (trSymdef.Peek() != -1)
                                {
                                    string line = trSymdef.ReadLine();
                                    if (line.Contains("LOAD_IMAGE_CRC"))
                                    {
                                        int idxEnd = line.IndexOf(' ', 2);
                                        imageCRC = UInt32.Parse(line.Substring(2, idxEnd-2), System.Globalization.NumberStyles.HexNumber);
                                    }
                                }
                            }

                            m_execSrecHash[entryPoint] = string.Format("<CRC>{0:X08},{1:X08},{2:X08},{3:X08}</CRC>\n", imageAddr, fiBin.Length, imageCRC, entryPoint);
                        }
                    }

                    return true;
                }
                finally
                {
                    m_evtMicroBooterStart.Reset();
                }
            }

            return false;
        }

        private bool CanUpgradeToSsl()
        {
            if(m_port == null || m_eng == null) return false;

            if (m_port is _DBG.PortDefinition_Tcp && m_serverCert != null)
            {
                return m_eng.IsUsingSsl || m_eng.CanUpgradeToSsl();
            }

            return false;
        }

        public bool UpgradeToSsl()
        {
            if (CanUpgradeToSsl())
            {
                if (m_eng.IsUsingSsl)
                {
                    return true;
                }
                else
                {
                    IAsyncResult iar = m_eng.UpgradeConnectionToSsl_Begin(m_serverCert, m_requireClientCert);

                    if (0 == WaitHandle.WaitAny(new WaitHandle[] { iar.AsyncWaitHandle, EventCancel }, 10000))
                    {
                        try
                        {
                            if (m_eng.UpgradeConnectionToSSL_End(iar))
                            {
                                return true;
                            }
                        }
                        catch
                        {
                        }

                        // dispose the engine since we attempted an ssl connection
                        m_eng.Dispose();
                        m_eng = null;
                    }
                }
            }

            return false;
        }

        private bool DeployMFUpdate(string zipFile)
        {
            const int c_PacketSize = 1024;

            if (File.Exists(zipFile))
            {
                byte []packet = new byte[c_PacketSize];
                try
                {
                    int handle = -1;
                    int idx = 0;
                    FileInfo fi = new FileInfo(zipFile);
                    int numPkts = ((int)fi.Length + c_PacketSize - 1) / c_PacketSize;

                    byte[] hashData = UTF8Encoding.UTF8.GetBytes(fi.Name + fi.LastWriteTimeUtc.ToString());

                    uint updateId = CRC.ComputeCRC(hashData, 0, hashData.Length, 0);
                    uint imageCRC = 0;

                    byte[] sig = null;

                    Console.WriteLine(updateId);

                    if (m_eng.StartUpdate("NetMF", 4, 2, updateId, 0, 0, (uint)fi.Length, (uint)c_PacketSize, 0, ref handle))
                    {
                        byte[] resp = new byte[4];
                        uint authType;
                        IAsyncResult iar = null;

                        if(!m_eng.UpdateAuthCommand(handle, 1, null, ref resp) || resp.Length < 4) return false;

                        using (MemoryStream ms = new MemoryStream(resp))
                        using (BinaryReader br = new BinaryReader(ms)) 
                        {
                            authType = br.ReadUInt32();
                        }


                        byte[] pubKey = null;

                        if (m_serverCert != null)
                        {
                            RSACryptoServiceProvider rsa = m_serverCert.PrivateKey as RSACryptoServiceProvider;

                            if(rsa != null)
                            {
                                pubKey = rsa.ExportCspBlob(false);
                            }
                        }

                        if (!m_eng.UpdateAuthenticate(handle, pubKey))
                        {
                            return false;
                        }

                        if (authType == 1 && m_serverCert != null)
                        {
                            iar = m_eng.UpgradeConnectionToSsl_Begin(m_serverCert, m_requireClientCert);

                            if(0 == WaitHandle.WaitAny(new WaitHandle[]{ iar.AsyncWaitHandle, EventCancel }, 10000))
                            {
                                try
                                {
                                    if (!m_eng.UpgradeConnectionToSSL_End(iar))
                                    {
                                        m_eng.Dispose();
                                        m_eng = null;
                                        return false;
                                    }
                                }
                                catch
                                {
                                    m_eng.Dispose();
                                    m_eng = null;
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }

                        RSAPKCS1SignatureFormatter alg = null;
                        HashAlgorithm hash = null;
                        byte[] hashValue = null;
                        
                        try
                        {
                            if (m_serverCert != null)
                            {
                                alg = new RSAPKCS1SignatureFormatter(m_serverCert.PrivateKey);
                                alg.SetHashAlgorithm("SHA1");
                                hash = new SHA1CryptoServiceProvider();
                                hashValue = new byte[hash.HashSize/8];
                            }
                        }
                        catch
                        {
                        }

                        using (FileStream fs = File.OpenRead(zipFile))
                        {
                            int read;

                            while (0 != (read = fs.Read(packet, 0, c_PacketSize)))
                            {
                                byte[] pkt = packet;
                                if (read < c_PacketSize)
                                {
                                    pkt = new byte[read];

                                    Array.Copy(packet, pkt, read);
                                }

                                uint crc = CRC.ComputeCRC(pkt, 0, pkt.Length, 0);

                                if (!m_eng.AddPacket(handle, (uint)idx++, pkt, CRC.ComputeCRC(pkt, 0, pkt.Length, 0))) return false;

                                imageCRC = CRC.ComputeCRC(pkt, 0, pkt.Length, imageCRC);

                                if(hash != null)
                                {
                                    if(idx == numPkts)
                                    {
                                        hash.TransformFinalBlock(pkt, 0, read);
                                    }
                                    else
                                    {
                                        byte[] hashTmp = new byte[read];
                                        hash.TransformBlock(pkt, 0, read, hashTmp, 0);
                                    }
                                }

                                if (OnProgress != null)
                                {
                                    OnProgress(idx, numPkts, string.Format(Properties.Resources.StatusFlashing, idx));
                                }
                            }
                        }

                        if (alg != null)
                        {
                            sig = alg.CreateSignature(hash);
                        }
                        else
                        {
                            sig = new byte[4];
                            using (MemoryStream ms = new MemoryStream(sig))
                            using (BinaryWriter br = new BinaryWriter(ms)) 
                            {
                                br.Write(imageCRC);
                            }
                        }

                        if (m_eng.InstallUpdate(handle, sig))
                        {
                            return true;
                        }
                    }
                }
                catch
                {
                }
            }


            return false;
        }

        public bool DeployUpdate(string comprFilePath)
        {
            if (m_eng.ConnectionSource == _DBG.ConnectionSource.TinyCLR)
            {
                if (DeployMFUpdate(comprFilePath)) return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to deploy an SREC (.hex) file to the connected .Net Micro Framework device.  The 
        /// signatureFile is used to validate the image once it has been deployed to the device.  If 
        /// the signature does not match the image is erased.
        /// </summary>
        /// <param name="filePath">Full path to the SREC (.hex) file</param>
        /// <param name="signatureFile">Full path to the signature file (.sig) for the SREC file identified by the filePath parameter</param>
        /// <param name="entrypoint">Out parameter that is set to the entry point address for the given SREC file</param>
        /// <returns>Returns false if the deployment fails, true otherwise
        /// Possible exceptions: MFFileNotFoundException, MFDeviceNoResponseException, MFUserExitException
        /// </returns>
        public bool Deploy(string filePath, string signatureFile, ref uint entryPoint)
        {
            entryPoint = 0;

            if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);
            if (m_eng == null) throw new MFDeviceNoResponseException();

            // make sure we know who we are talking to
            if (CheckForMicroBooter())
            {
                return DeploySREC(filePath, ref entryPoint);
            }

            m_eng.TryToConnect(1, 100, true, _DBG.ConnectionSource.Unknown);

            bool sigExists = File.Exists(signatureFile);
            FileInfo fi = new FileInfo(filePath);

            ArrayList blocks = new ArrayList();
            entryPoint = _DBG.SRecordFile.Parse(filePath, blocks, sigExists? signatureFile: null);

            if (blocks.Count > 0)
            {
                long total = 0;
                long value = 0;

                for (int i = 0;i < blocks.Count;i++)
                {
                    total += (blocks[i] as _DBG.SRecordFile.Block).data.Length;
                }

                PrepareForDeploy(blocks);

                foreach (_DBG.SRecordFile.Block block in blocks)
                {
                    long len  = block.data.Length;
                    uint addr = block.address;

                    if (EventCancel.WaitOne(0, false)) throw new MFUserExitException();

                    block.data.Seek(0, SeekOrigin.Begin);

                    if (OnProgress != null)
                    {
                        OnProgress(0, total, string.Format(Properties.Resources.StatusEraseSector, block.address));
                    }

                    // the clr requires erase before writing
                    if (!m_eng.EraseMemory(block.address, (uint)len)) return false;

                    while(len > 0 )
                    {
                        if (EventCancel.WaitOne(0, false)) throw new MFUserExitException();

                        int buflen = len > 1024? 1024: (int)len;
                        byte[] data = new byte[buflen];

                        if (block.data.Read(data, 0, buflen) <= 0)  return false;

                        if (!m_eng.WriteMemory(addr, data)) return false;

                        value += buflen;
                        addr += (uint)buflen;
                        len  -= buflen;

                        if (OnProgress != null)
                        {
                            OnProgress(value, total, string.Format(Properties.Resources.StatusFlashing, fi.Name));
                        }
                    }
                    if (_DBG.ConnectionSource.TinyCLR != m_eng.ConnectionSource)
                    {
                        byte[] emptySig = new byte[128];

                        if (OnProgress != null) OnProgress(value, total, Properties.Resources.StatusCheckingSignature);

                        if (!m_eng.CheckSignature(((block.signature == null || block.signature.Length == 0)? emptySig: block.signature), 0))  throw new MFSignatureFailureException(signatureFile);
                    }
                }
            }

            return true;
        }
        /// <summary>
        /// Starts execution on the connected .Net Micro Framework device at the supplied address (parameter entrypoint).
        /// This method is generally used after the Deploy method to jump into the code that was deployed.
        /// </summary>
        /// <param name="entrypoint">Entry point address for execution to begin</param>
        /// <returns>Returns false if execution fails, true otherwise
        /// Possible exceptions: MFDeviceNoResponseException
        /// </returns>
        public bool Execute(uint entryPoint)
        {
            if (m_eng == null) throw new MFDeviceNoResponseException(); 
            if (CheckForMicroBooter())
            {
                if (m_execSrecHash.ContainsKey(entryPoint))
                {
                    string execRec = (string)m_execSrecHash[entryPoint];
                    bool fRet = false;

                    for (int retry = 0; retry < 10; retry++)
                    {
                        m_eng.SendRawBuffer(System.Text.ASCIIEncoding.ASCII.GetBytes(execRec));
                        m_eng.SendRawBuffer(System.Text.ASCIIEncoding.ASCII.GetBytes("\n"));

                        if (m_evtMicroBooter.WaitOne(1000))
                        {
                            fRet = true;
                            break;
                        }
                    }

                    return fRet;
                }

                return false;
            }
            
            _WP.Commands.Monitor_Ping.Reply reply = m_eng.GetConnectionSource();

            if (reply == null) throw new MFDeviceNoResponseException();

            // only execute if we are talking to the tinyBooter, otherwise reboot
            if (reply.m_source == _WP.Commands.Monitor_Ping.c_Ping_Source_TinyBooter)
            {
                return m_eng.ExecuteMemory(entryPoint);
            }
            else // if we are talking to the CLR then we simply did a deployment update, so reboot
            {
                m_eng.RebootDevice(_DBG.Engine.RebootOption.RebootClrOnly);
            }
            return true;
        }
        /// <summary>
        /// Reboots the connected .Net Micro Framework device
        /// Possible exceptions: MFDeviceNoResponseException
        /// </summary>
        /// <param name="coldBoot">Determines whether a cold/hardware reboot or a warm/software reboot is performed</param>
        public void Reboot(bool coldBoot)
        {
            if (m_eng == null) throw new MFDeviceNoResponseException(); 

            m_eng.RebootDevice(coldBoot? _DBG.Engine.RebootOption.NoReconnect: _DBG.Engine.RebootOption.RebootClrOnly);

            if(!coldBoot)
            {
                Thread.Sleep(200);

                this.ConnectTo(1000, true, _DBG.ConnectionSource.TinyCLR);                
            }
        }
        /// <summary>
        /// Determines if the device is connected
        /// </summary>
        public bool IsConnected { get { return (m_eng != null ? m_eng.IsConnected : false); } }


        public interface IAppDomainInfo
        {
            string Name { get; }
            uint ID { get; }
            uint[] AssemblyIndicies { get; }
        }

        public interface IAssemblyInfo
        {
            string Name { get; }
            System.Version Version { get; }
            uint Index { get; }
            List<IAppDomainInfo> InAppDomains { get; }            
        }

        private class AppDomainInfo : IAppDomainInfo
        {
            private uint m_id;
            _WP.Commands.Debugging_Resolve_AppDomain.Reply m_reply;

            public AppDomainInfo(uint id, _WP.Commands.Debugging_Resolve_AppDomain.Reply reply)
            {
                m_id = id;
                m_reply = reply;
            }

            public string Name
            {
                get { return m_reply.Name; }
            }

            public uint ID
            {
                get { return m_id; }
            }

            public uint[] AssemblyIndicies
            {
                get { return m_reply.m_data; }
            }

        }

        private class AssemblyInfoFromResolveAssembly : IAssemblyInfo
        {
            private _WP.Commands.Debugging_Resolve_Assembly m_dra;
            private List<IAppDomainInfo> m_AppDomains = new List<IAppDomainInfo>();
            
            public AssemblyInfoFromResolveAssembly(_WP.Commands.Debugging_Resolve_Assembly dra)
            {
                m_dra = dra;
            }
            
            public string Name
            {
                get { return m_dra.m_reply.Name; }
            }
            
            public System.Version Version
            {
                get
                {
                    _WP.Commands.Debugging_Resolve_Assembly.Version draver = m_dra.m_reply.m_version;
                    return new System.Version(draver.iMajorVersion, draver.iMinorVersion, draver.iBuildNumber, draver.iRevisionNumber);
                }
            }
            
            public uint Index
            {
                get { return m_dra.m_idx; }
            }

            public List<IAppDomainInfo> InAppDomains
            {
                get { return m_AppDomains; }
            }

            public void AddDomain(IAppDomainInfo adi)
            {
                if ( adi != null )
                {
                    m_AppDomains.Add(adi);
                }
            }
        }


        public delegate void AppDomainAction(IAppDomainInfo adi);
        public void DoForEachAppDomain(AppDomainAction appDomainAction)
        {
            if (m_eng.Capabilities.AppDomains)
            {
                _WP.Commands.Debugging_TypeSys_AppDomains.Reply domainsReply = m_eng.GetAppDomains();
                if ( domainsReply != null )
                {
                    foreach (uint id in domainsReply.m_data)
                    {
                        _WP.Commands.Debugging_Resolve_AppDomain.Reply reply = m_eng.ResolveAppDomain(id);
                        if (reply != null)
                        {
                            appDomainAction(new AppDomainInfo(id, reply));
                        }
                    }                
                }
            }
        }

        public delegate void AssemblyAction(IAssemblyInfo ai);
        public void DoForEachAssembly(AssemblyAction assemblyAction)
        {
            List<IAppDomainInfo> theDomains = new List<IAppDomainInfo>();
            
            this.DoForEachAppDomain(
                delegate(IAppDomainInfo adi)
                {
                    theDomains.Add(adi);
                }
            );

            _WP.Commands.Debugging_Resolve_Assembly[] reply = m_eng.ResolveAllAssemblies();
            if ( reply != null )
                foreach (_WP.Commands.Debugging_Resolve_Assembly resolvedAssm in reply)
                {
                    AssemblyInfoFromResolveAssembly ai = new AssemblyInfoFromResolveAssembly(resolvedAssm);
                    
                    foreach (IAppDomainInfo adi in theDomains)
                    {
                        if (Array.IndexOf<uint>(adi.AssemblyIndicies, ai.Index) != -1 )
                        {
                            ai.AddDomain(adi);
                        }
                    }
                    
                    assemblyAction(ai);
                }
        }

        public interface IMFDeviceInfo
        {
            bool                        Valid { get; }
            System.Version              HalBuildVersion { get; }
            string                      HalBuildInfo { get; }
            byte                        OEM { get; }
            byte                        Model { get; }
            ushort                      SKU { get; }
            string                      ModuleSerialNumber { get; }
            string                      SystemSerialNumber { get; }
            System.Version              ClrBuildVersion { get; }
            string                      ClrBuildInfo { get; }
            System.Version              TargetFrameworkVersion { get; }
            System.Version              SolutionBuildVersion { get; }
            string                      SolutionBuildInfo { get; }
            IAppDomainInfo[]            AppDomains { get; }
            IAssemblyInfo[]             Assemblies { get; }
        }

        private class MFDeviceInfoImpl : IMFDeviceInfo
        {
            private MFDevice m_self;

            private bool m_fValid = false;            
            private List<IAppDomainInfo> m_Domains = new List<IAppDomainInfo>();
            private List<IAssemblyInfo> m_AssemblyInfos = new List<IAssemblyInfo>();
            
            
            public MFDeviceInfoImpl(MFDevice dev)
            {
                m_self = dev;

                if ( !Dbg.IsConnectedToTinyCLR ) return;

                m_self.DoForEachAppDomain(
                    delegate(IAppDomainInfo adi)
                    {
                        m_Domains.Add(adi);
                    }
                );
               
                m_self.DoForEachAssembly(
                    delegate(MFDevice.IAssemblyInfo ai) 
                    {
                        m_AssemblyInfos.Add(ai);
                    }
                );                
                
                m_fValid = true;
            }

            private _DBG.Engine Dbg { get { return m_self.DbgEngine; } }
            
            public bool Valid { get { return m_fValid; } }

            public System.Version HalBuildVersion
            {
                get { return Dbg.Capabilities.HalSystemInfo.halVersion; }
            }
            
            public string HalBuildInfo
            {
                get { return Dbg.Capabilities.HalSystemInfo.halVendorInfo; }
            }
            
            public byte OEM
            {
                get { return Dbg.Capabilities.HalSystemInfo.oemCode; }
            }
            
            public byte Model
            {
                get { return Dbg.Capabilities.HalSystemInfo.modelCode; }
            }
            
            public ushort SKU
            {
                get { return Dbg.Capabilities.HalSystemInfo.skuCode; }
            }
            
            public string ModuleSerialNumber
            {
                get { return Dbg.Capabilities.HalSystemInfo.moduleSerialNumber; }
            }
            
            public string SystemSerialNumber
            {
                get { return Dbg.Capabilities.HalSystemInfo.systemSerialNumber; }
            }

            public System.Version ClrBuildVersion
            {
                get { return Dbg.Capabilities.ClrInfo.clrVersion; }
            }

            public string ClrBuildInfo
            {
                get { return Dbg.Capabilities.ClrInfo.clrVendorInfo; }
            }

            public System.Version TargetFrameworkVersion
            {
                get { return Dbg.Capabilities.ClrInfo.targetFrameworkVersion; }
            }

            public System.Version SolutionBuildVersion
            {
                get { return Dbg.Capabilities.SolutionReleaseInfo.solutionVersion; }
            }

            public string SolutionBuildInfo
            {
                get { return Dbg.Capabilities.SolutionReleaseInfo.solutionVendorInfo; }
            }

            public IAppDomainInfo[] AppDomains
            {
                get { return m_Domains.ToArray(); }
            }

            public IAssemblyInfo[] Assemblies
            {
                get { return m_AssemblyInfos.ToArray(); }
            }
        }
        
        private IMFDeviceInfo m_deviceInfoCache = null;
        public IMFDeviceInfo MFDeviceInfo
        {
            get
            {
                if ( m_deviceInfoCache == null )
                {
                    m_deviceInfoCache = new MFDeviceInfoImpl(this);
                }
                return m_deviceInfoCache;
            }
        }
                
        private readonly byte[] m_data =
                    {
                        67, 111, 112, 121, 114, 105, 103, 104, 116, 32, 50, 48, 48, 51, 13, 10,
                        77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 67, 111, 114, 112, 13, 10,
                        49, 32, 77, 105, 99, 114, 111, 115, 111, 102, 116, 32, 87, 97, 121, 13,
                        10, 82, 101, 100, 109, 111, 110, 100, 44, 32, 87, 65, 13, 10, 57, 56,
                        48, 53, 50, 45, 54, 51, 57, 57, 13, 10, 85, 46, 83, 46, 65, 46,
                        13, 10, 65, 108, 108, 32, 114, 105, 103, 104, 116, 115, 32, 114, 101, 115,
                        101, 114, 118, 101, 100, 46, 13, 10, 77, 73, 67, 82, 79, 83, 79, 70,
                        84, 32, 67, 79, 78, 70, 73, 68, 69, 78, 84, 73, 65, 76, 13, 10,
                        55, 231, 64, 0, 118, 157, 50, 129, 173, 196, 117, 75, 87, 255, 238, 223,
                        181, 114, 130, 29, 130, 170, 89, 70, 194, 108, 71, 230, 192, 61, 9, 29,
                        216, 23, 196, 204, 21, 89, 242, 196, 143, 255, 49, 65, 179, 224, 237, 213,
                        15, 250, 92, 181, 77, 10, 200, 21, 219, 202, 181, 127, 64, 172, 101, 87,
                        166, 35, 162, 28, 70, 172, 138, 40, 35, 215, 207, 160, 195, 119, 187, 95,
                        239, 213, 127, 201, 46, 15, 60, 225, 19, 252, 227, 17, 211, 80, 209, 52,
                        74, 122, 115, 2, 144, 20, 153, 241, 244, 57, 139, 10, 57, 65, 248, 204,
                        149, 252, 17, 159, 244, 11, 186, 176, 59, 187, 167, 107, 83, 163, 62, 122
                    };

    }
}
