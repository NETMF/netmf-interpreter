////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.SPOT.Emulator.BlockStorage
{    
    [StructLayout(LayoutKind.Sequential)]
    public struct InternalBlockRange
    {
        public uint RangeType;
        public uint StartBlock;
        public uint EndBlock;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct InternalBlockRegionInfo
    {
        public uint Start;                  // Starting address
        public uint NumBlocks;              // total number of blocks in this region
        public uint BytesPerBlock;          // Total number of bytes per block (MUST be SectorsPerBlock * DataBytesPerSector
        public uint NumBlockRanges;         // number of block ranges in this region

        public InternalBlockRange[] BlockRanges;
    }

    //--//

    [StructLayout(LayoutKind.Sequential)]
    public struct InternalBlockDeviceInfo
    {
        // Media Attributes
        public bool Removable;
        public bool SupportsXIP;
        public bool WriteProtected;
        public bool SupportsCopyBack;

        // Maximum Sector Write Time.
        public uint MaxSectorWrite_uSec;    

        // Maximum erase time for the a block
        public uint MaxBlockErase_uSec;    

        // Bytes Per Sector
        public uint BytesPerSector;         

        // count of regions in the flash.
        public uint NumRegions;

        // pointer to an array (NumRegions long) of region information
        public InternalBlockRegionInfo[] Regions;
    }

    //--//

    [StructLayout(LayoutKind.Sequential)]
    public struct BsSectorMetadata
    {
        public uint reserved1;     // Used by the FAL to hold the logical to physical sector mapping information.
        public byte oemReserved;   // For use by OEM. See OEMReservedBits for more information.
        public byte badBlock;      // Indicates if a block is bad.
        public ushort reserved2;   // Used by the FAL to maintain state information about the sector.
        
        //Check ECC algorithm is not implementated on CE to see what data type works most conveniently for this
        public uint ECC0;     // Error Correction Code [Should be all 0xFF if not used]
        public uint ECC1;

        // Remarks:
        //    Any sectors that the OEM does not want the wear leveling
        //    code to touch should have both of these bits set. This
        //    includes the sectors that include the boot loader and any
        //    other flash data that exists at fixed locations.
        //
        // Note:
        //   Because only full blocks can be erased, all sectors within
        //   a block should have the same values for these flags.
        //
        const byte OEM_BLOCK_RESERVED = 0x01;
        const byte OEM_BLOCK_READONLY = 0x02;
    }

    //--//

	// the IBlockStorageDriver class in the extensible emulator takes one extra parameter
	// that allows the meulaotr extension to understand what device the API actaully talks to 
    public interface IBlockStorageDriver
    {
        bool Initialize(uint context);
        bool Uninitialize(uint context);
        InternalBlockDeviceInfo[] GetDevicesInfo();
        bool Read(uint context, uint address, uint length, IntPtr sectorBuff);
        bool Write(uint context, uint address, uint length, IntPtr sectorBuff, bool readModifyWrite);
        bool Memset(uint context, uint address, byte data, uint length);
        bool GetSectorMetadata(uint context, uint address, ref BsSectorMetadata sectorMetadata);
        bool SetSectorMetadata(uint context, uint address, ref BsSectorMetadata sectorMetadata);
        bool IsBlockErased(uint context, uint address, uint blockLength);
        bool EraseBlock(uint context, uint address);
        void SetPowerState(uint context, uint state);
        uint MaxSectorWrite_uSec(uint context);
        uint MaxBlockErase_uSec(uint context);

        void MountInsertedRemovableDevices();

    }
}
