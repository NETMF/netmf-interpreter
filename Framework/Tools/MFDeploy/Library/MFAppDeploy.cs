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

#define COMPACT_DEPLOYMENT

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.IO;

using Microsoft.SPOT.Debugger.WireProtocol;
using _DBG = Microsoft.SPOT.Debugger;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.PlugIns;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using dotNetMFCrypto;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
    public class MFApplicationDeployment
    {
        MFDevice m_device;

        public class MFApplicationDeploymentData
        {
            public byte[] BinaryData;
            public byte[] HexData;
        }

        public MFApplicationDeployment(MFDevice device)
        {
            m_device = device;
        }

#if COMPACT_DEPLOYMENT        
        [StructLayout(LayoutKind.Sequential)]
        private class CLR_RECORD_VERSION
        {
            public ushort iMajorVersion;
            public ushort iMinorVersion;
            public ushort iBuildNumber;
            public ushort iRevisionNumber;
        }

        [StructLayout(LayoutKind.Sequential, Size = 124)]
        private class CLR_RECORD_ASSEMBLY
        {
            public const int TBL_EndOfAssembly = 0x0F;
            public const int TBL_MAX = 0x10;

            public const long MARKER_ASSEMBLY_V1 = 0x0031746f7053534d; //"MSSpot1\0";

            public long marker;
            public uint headerCRC;
            public uint assemblyCRC;
            public uint flags;
            public uint nativeMethodsChecksum;
            public uint patchEntryOffset;
            public CLR_RECORD_VERSION version;
            public ushort assemblyName;
            public ushort stringTableVersion;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = TBL_MAX)]
            public uint[] startOfTables;

            [MarshalAs(UnmanagedType.ByValArray)]
            public byte[] extra;
        }

        private bool CreateDeploymentReadHelper(
            BackgroundWorker backgroundWorker,
            DoWorkEventArgs doWorkEventArgs,
            Microsoft.SPOT.Debugger.Engine engine,
            ref uint address,
            uint addressStart,
            uint addressEnd,
            uint bytes,
            out byte[] data
            )
        {
            data = new byte[bytes];
            uint iByte = 0;

            while (bytes > 0)
            {
                uint cBytes = System.Math.Min(1024, bytes);
                byte[] buf;

                if (!engine.ReadMemory(address, cBytes, out buf))
                    throw new ApplicationException("Cannot read data");

                buf.CopyTo(data, iByte);

                address += cBytes;
                iByte += cBytes;
                bytes -= cBytes;

                if (backgroundWorker.CancellationPending)
                {
                    doWorkEventArgs.Cancel = true;
                    return false;
                }
            }

            return true;
        }

        internal void CreateDeploymentData(BackgroundWorker backgroundWorker, DoWorkEventArgs doWorkEventArgs)
        {
            MFDevice device = doWorkEventArgs.Argument as MFDevice;
            _DBG.Engine eng = device.DbgEngine;

            Commands.Monitor_FlashSectorMap.Reply flashMap = eng.GetFlashSectorMap();

            //find deployment sectors.
            MemoryStream deploymentStream = new MemoryStream();

            //this duplicates LoadDeploymentAssemblies logic, as how to find assemblies in the deployment sectors
            int iSectorStart = -1, iSectorEnd = 0;
            uint address, addressAssemblyStart, addressStart = 0, addressEnd = 0;
            int iSector;

            //First, find out where the deployment sectors are
            for (iSector = 0; iSector < flashMap.m_map.Length; iSector++)
            {
                Commands.Monitor_FlashSectorMap.FlashSectorData flashSectorData = flashMap.m_map[iSector];
                if ((flashSectorData.m_flags & Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK) == Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_DEPLOYMENT)
                {
                    if (iSectorStart < 0)
                    {
                        iSectorStart = iSector;
                        addressStart = flashSectorData.m_address;
                    }

                    iSectorEnd = iSector;
                    addressEnd = flashSectorData.m_address + flashSectorData.m_size;
                }
            }

            if (iSectorStart < 0)
                throw new ApplicationException("Could not find deployment sectors");

            address = addressStart;
            iSector = iSectorStart;

            while (true)
            {
                if (backgroundWorker.WorkerReportsProgress)
                {
                    int progress = (int)(100.0 * (double)address / (double)addressEnd);
                    backgroundWorker.ReportProgress(progress);
                }

                //read assembly header
                uint assemblyHeaderSize = (uint)Marshal.SizeOf(typeof(CLR_RECORD_ASSEMBLY));
                byte[] assemblyHeaderBytes = null;
                byte[] assemblyDataBytes = null;

                if (address + assemblyHeaderSize >= addressEnd)
                    break;

                addressAssemblyStart = address;

                if (!CreateDeploymentReadHelper(backgroundWorker, doWorkEventArgs, eng, ref address, addressStart, addressEnd, assemblyHeaderSize, out assemblyHeaderBytes))
                    return;

                GCHandle gch = GCHandle.Alloc(assemblyHeaderBytes, GCHandleType.Pinned);
                CLR_RECORD_ASSEMBLY assemblyHeader = (CLR_RECORD_ASSEMBLY)Marshal.PtrToStructure(gch.AddrOfPinnedObject(), typeof(CLR_RECORD_ASSEMBLY));
                gch.Free();

                //check if valid header                
                //check marker
                bool fValidAssembly = assemblyHeader.marker == CLR_RECORD_ASSEMBLY.MARKER_ASSEMBLY_V1;

                if (fValidAssembly)
                {
                    //check header crc
                    uint crcHeader = assemblyHeader.headerCRC;

                    //clear headerCRC
                    int headerCRCOffset = 8;
                    int headerCRCSize = 4;

                    Array.Clear(assemblyHeaderBytes, headerCRCOffset, headerCRCSize);

                    uint crc = _DBG.CRC.ComputeCRC(assemblyHeaderBytes, 0);

                    //Reset headerCRC
                    Array.Copy(BitConverter.GetBytes(crcHeader), 0, assemblyHeaderBytes, headerCRCOffset, headerCRCSize);

                    fValidAssembly = (crcHeader == crc);
                }

                if (fValidAssembly)
                {
                    uint assemblyTotalSize = assemblyHeader.startOfTables[CLR_RECORD_ASSEMBLY.TBL_EndOfAssembly];
                    uint assemblyDataSize = assemblyTotalSize - assemblyHeaderSize;

                    if (address + assemblyDataSize >= addressEnd)
                        break;

                    //read body    
                    if (!CreateDeploymentReadHelper(backgroundWorker, doWorkEventArgs, eng, ref address, addressStart, addressEnd, assemblyDataSize, out assemblyDataBytes))
                        return;

                    //check if valid body (crc)
                    uint crc = _DBG.CRC.ComputeCRC(assemblyDataBytes, 0);

                    fValidAssembly = (crc == assemblyHeader.assemblyCRC);
                }

                if (fValidAssembly)
                {
                    // add to compact stream
                    deploymentStream.Write(assemblyHeaderBytes, 0, assemblyHeaderBytes.Length);
                    deploymentStream.Write(assemblyDataBytes, 0, assemblyDataBytes.Length);

                    // make sure we are on 4 byte boundary
                    if(0 != (address % sizeof(UInt32)))
                    {
                        byte[] buff = new byte[sizeof(UInt32) - (address % sizeof(UInt32))];
                        deploymentStream.Write(buff, 0, buff.Length);
                        address += sizeof(UInt32) - (address % sizeof(UInt32));
                    }
                }
                else
                {
                    //if no, clear assemblyData, jump to next sector (if in middle of sector), or finish (if at beginning of sector)
                    while (iSector < iSectorEnd)
                    {
                        Commands.Monitor_FlashSectorMap.FlashSectorData flashSectorData = flashMap.m_map[iSector];

                        if (addressAssemblyStart >= flashSectorData.m_address && addressAssemblyStart < flashSectorData.m_address + flashSectorData.m_size)
                        {
                            // jump to next sector
                            address = flashSectorData.m_address + flashSectorData.m_size;

                            System.Diagnostics.Debug.Assert(address == flashMap.m_map[iSector + 1].m_address);

                            break;
                        }

                        iSector++;
                    }
                }
            }

            //Finished reading

            //convert to srec
            MFApplicationDeploymentData data = new MFApplicationDeploymentData();
            long deploymentLength = deploymentStream.Seek(0, SeekOrigin.Current);

            MemoryStream streamSrec = new MemoryStream();
            deploymentStream.Seek(0, SeekOrigin.Begin);
            data.BinaryData = new byte[(int)deploymentLength];
            deploymentStream.Read(data.BinaryData, 0, (int)deploymentLength);
            deploymentStream.Seek(0, SeekOrigin.Begin);
            new BinToSrec().DoConversion(deploymentStream, streamSrec, flashMap.m_map[iSectorStart].m_address);

            //add zero bytes to all other deployment sectors?

            //Get bytes
            long pos = streamSrec.Seek(0, SeekOrigin.Current);
            data.HexData = new byte[pos];
            streamSrec.Seek(0, SeekOrigin.Begin);
            streamSrec.Read(data.HexData, 0, (int)pos);

            doWorkEventArgs.Result = data;
        }
#else //COMPACT_DEPLOYMENT        

        private bool CreateDeploymentReadHelper(
                BackgroundWorker backgroundWorker,
                DoWorkEventArgs doWorkEventArgs,
                Microsoft.SPOT.Debugger.Engine engine,
                uint address,                
                byte[] buf
        )
        {
            uint bytesTotal = (uint)buf.Length;
            uint bytesRemaining = bytesTotal;
            uint bytesRead = 0;

            while (bytesRemaining > 0)
            {
                uint cBytes = System.Math.Min(1024, bytesRemaining);
                byte[] bufT;

                if (!engine.ReadMemory(address, cBytes, out bufT))
                    throw new ApplicationException("Cannot read data");

                bufT.CopyTo(buf, bytesRead);

                address += cBytes;
                bytesRead += cBytes;
                bytesRemaining -= cBytes;

                if (backgroundWorker.WorkerReportsProgress)
                {
                    int progress = (int)(100.0 * (double)bytesRead / (double)bytesTotal);
                    backgroundWorker.ReportProgress(progress);
                }

                if (backgroundWorker.CancellationPending)
                {
                    doWorkEventArgs.Cancel = true;
                    return false;
                }
            }

            return true;
        }

        internal void CreateDeploymentData(BackgroundWorker backgroundWorker, DoWorkEventArgs doWorkEventArgs)
        {
            MFDevice device = doWorkEventArgs.Argument as MFDevice;
            _DBG.Engine eng = device.DbgEngine;

            Commands.Monitor_FlashSectorMap.Reply flashMap = eng.GetFlashSectorMap();

            //find deployment sectors.
            uint addressStart = 0;
            uint cBytes = 0;

            //First, find out where the deployment sectors are
            for (int iSector = flashMap.m_map.Length - 1; iSector >= 0; iSector--)
            {
                Commands.Monitor_FlashSectorMap.FlashSectorData flashSectorData = flashMap.m_map[iSector];
                if ((flashSectorData.m_flags & Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_MASK) == Commands.Monitor_FlashSectorMap.c_MEMORY_USAGE_DEPLOYMENT)
                {                        
                    addressStart = flashSectorData.m_address;
                    cBytes += flashSectorData.m_size;
                }
            }

            if (cBytes == 0)
                throw new ApplicationException("Could not find deployment sectorsd");

            byte[] deploymentData = new byte[cBytes];

            //Read deployment data
            if (!CreateDeploymentReadHelper(backgroundWorker, doWorkEventArgs, eng, addressStart, deploymentData))
                return;

            //create deployment stream
            MemoryStream deploymentStream = new MemoryStream();
            deploymentStream.Write(deploymentData, 0, deploymentData.Length);

            //convert to srec
            MFApplicationDeploymentData data = new MFApplicationDeploymentData();
            MemoryStream streamSrec = new MemoryStream();
            data.BinaryData = deploymentData;
            deploymentStream.Seek(0, SeekOrigin.Begin);
            new BinToSrec().DoConversion(deploymentStream, streamSrec, addressStart);

            //Get bytes
            long pos = streamSrec.Seek(0, SeekOrigin.Current);
            data.HexData = new byte[(int)pos];
            streamSrec.Seek(0, SeekOrigin.Begin);
            streamSrec.Read(data.HexData, 0, (int)pos);

            doWorkEventArgs.Result = data;
        }
        
#endif //COMPACT_DEPLOYMENT

        #region BinToSrec
        //copied from BinToSrec
        internal class BinToSrec
        {
            // 2 bytes for rec type, 2 bytes for rec len, 8 bytes for target address, 2 bytes for rec CRC
            private const int c_nHeaderLength = 2 + 2 + 8;
            private const int c_nTailLength = 2;
            private const int c_nBaseLength = c_nHeaderLength + c_nTailLength;

            public void DoConversion(Stream readStream, Stream writeStream, uint nBaseAddress) //, uint nEndaddress, bool bEndAddressSet)
            {
                int nChunkLength = 16;

                //Now read chunk of 16 bytes - 
                int nRead = 0;
                byte[] arrByte = new byte[nChunkLength];
                uint nCurrentOffSet = nBaseAddress;
                while ((nRead = readStream.Read(arrByte, 0, nChunkLength)) != 0)
                {
                    char[] arrRecord = ConstructSrecRecord("S3", arrByte, nRead, nCurrentOffSet);
                    if (arrRecord != null)
                    {
                        for (int i = 0; i < arrRecord.Length; i++)
                        {
                            writeStream.WriteByte((byte)arrRecord[i]);
                        }

                        writeStream.WriteByte((byte)'\r');
                        writeStream.WriteByte((byte)'\n');
                    }
                    else
                        throw new Exception("Problem in Code");

                    nCurrentOffSet = nCurrentOffSet + 16;
                }
            }

            // parse the binary data from the SREC file
            public byte[] GetBinaryData(string hexFile)
            {
                List<byte> bytes = new List<byte>();
                TextReader tr = File.OpenText(hexFile);
                string line;

                while (null != (line = tr.ReadLine()))
                {
                    for (int i = c_nHeaderLength; i < line.Length-c_nTailLength; i += 2)
                    {
                        bytes.Add(byte.Parse(line.Substring(i, 2), System.Globalization.NumberStyles.HexNumber));
                    }
                }

                return bytes.ToArray();
            }



            private char[] ConstructSrecRecord(string szType, byte[] arrByteInput, int nRead, uint nCurrentAddress)
            {
                char[] arrByte = new char[c_nBaseLength + (nRead * 2)];

                if (szType == null || szType.Length != 2)
                {
                    return null;
                }

                //Put in the type
                arrByte[0] = szType[0];
                arrByte[1] = szType[1];

                string sHex = null;

                //Put in the count
                byte byCount = (byte)((arrByte.Length - 4) / 2);
                sHex = String.Format("{0:X2}", byCount);
                arrByte[2] = sHex[0];
                arrByte[3] = sHex[1];

                int nSum = 0;
                nSum = (int)byCount;

                //Get the address
                sHex = String.Format("{0:X8}", nCurrentAddress);
                for (int i = 0; i < sHex.Length; i++)
                {
                    arrByte[4 + i] = sHex[i];
                }

                //for check sum
                for (int j = 0; j < 4; j++)
                {
                    nSum = (int)(nSum + (nCurrentAddress & 255));
                    nCurrentAddress = nCurrentAddress >> 8;
                }

                for (int s = 0; s < nRead; s++)
                {
                    nSum = nSum + arrByteInput[s];
                }
                nSum = int.MaxValue - nSum;
                //end check sum

                //Put in the Data
                ConvertToHexAscii(arrByte, 12, arrByteInput, nRead);

                //Check Sum Right now
                sHex = String.Format("{0:X8}", nSum);
                arrByte[arrByte.Length - 2] = sHex[sHex.Length - 2];
                arrByte[arrByte.Length - 1] = sHex[sHex.Length - 1];
                //Then Put in the new line thing

                return arrByte;
            }

            private void ConvertToHexAscii(char[] arrToWriteTo, int nIndexToStart, byte[] arrInput, int nRead)
            {
                string sHex = null;
                for (int i = 0; i < nRead; i++)
                {
                    sHex = String.Format("{0:X2}", arrInput[i]);
                    arrToWriteTo[nIndexToStart] = sHex[0];
                    arrToWriteTo[nIndexToStart + 1] = sHex[1];
                    nIndexToStart = nIndexToStart + 2;
                }
            }
        }

        #endregion

        public MFApplicationDeploymentData CreateDeploymentData()
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            DoWorkEventArgs doWorkEventArgs = new DoWorkEventArgs(m_device);

            CreateDeploymentData(backgroundWorker, doWorkEventArgs);

            return doWorkEventArgs.Result as MFApplicationDeploymentData;
        }
    }
}
