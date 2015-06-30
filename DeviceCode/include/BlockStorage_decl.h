////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <CPU_EBIU_decl.h>

#ifndef _DRIVERS_PAL_BLOCKSTORAGE_H_
#define _DRIVERS_PAL_BLOCKSTORAGE_H_ 1

//--//

#if defined(__GNUC__)
#define HAL_Time_Sleep_MicroSeconds_BS(x) HAL_Time_Sleep_MicroSeconds_InRam(x)

#else
#define HAL_Time_Sleep_MicroSeconds_BS(x) HAL_Time_Sleep_MicroSeconds(x)

#endif 




// for convert byte size to storage sector size
#define CONVERTBYTETOSECTORSIZE(x,y)     ( (size_t)x + (size_t)y-1)/(size_t)y

#define CONVERTBYTETOSECTORSIZE_2(x,y,z)   ( (size_t)x + (size_t)y-1)/(size_t)z

/////////////////////////////////////////////////////////
// Description:
//   Definitions for the Block Storage device interface
//   for the .NET Micro Framework
// 
// Remarks:
//   The general design of the API is strongly influenced
//   by the Windows Embedded CE Flash Media Driver (FMD)
//   API. However, It is not intended to be a direct port
//   of the FMD architecture to the .NET MF. This design 
//   overcomes some of the limitations of the CE design
//   and includes functionalty an support that is specific
//   to the .NET MF.
//
//   In particular this design adds the following:
//   * Generalized block storage rather than assuming 
//     a Resident Flash Array (RFA)
//   * Support for multiple instances of block storage
//     devices instead of only one
//   * Support for legacy CLR Block usage information to 
//     Identify for the system where various types of
//     information reside in the storage device. 
//   * Support for direct use of NOR flash without forcing
//     additional layering for metadata if an FS is not 
//     needed or desired on the NOR flash.
//   * Support for XIP storage media like NOR and ROM.
//   * Use of an interface (as a structure of function
//     pointers) to allow layering and standardized support
//     for various common functionality. (e.g. treating 
//     NOR flash with varying sized sectors and no metadata
//     as if it was NAND with fixed sized sectors including
//     metadata. Thus this kind functionality need only be
//     implemented once)
//
// Terminology:
//  Block Storage Device - device that stores data addressable
//                         as a chunk as opposed to byte or WORD level
//
//  Sector - smallest unit of storage for a Block device. Data is
//           read or written as a complete sector(unless the device
//           supports XIP which allows byte addresssing and reading
//           directly from the processor memory bus)
//
//  Block  - smallest eraseable unit of the storage device. A block 
//           contains one or more sectors that are erased as a whole.
//           All sectors within a block are always the same size.
//
//  Region - Group of adjacent blocks that all contain sectors of the
//           same size.
//  
//  Execute-In-Place (XIP) - Attribute of some storage types, NOR, ROM,
//          RAM, that allows direct execution of code from the device.
//          XIP capable devices are at least read-only and byte addresable
//          on the CPU memory bus.
//


/////////////////////////////////////////////////////////
// Description:
//    Defines a Logical Sector Address
// 
// Remarks:
//    This is a typedef in case we want to support
//    larger devices in the future.
//
//    The Logical Sector Address is the address of a
//    sector on the device independent of geometry. That is
//    the address for the first sector on the device is 0, 
//    the next is 1 etc... crossing over any geometry
//    boundaries. The exact mapping of logical sector
//    addresses to a physical sector on the storage device
//    is driver implementation defined. Many storage media
//    types, like hard drives, have industry standard
//    mappings for interoperability since file systems deal
//    with logical addresses only.
//
//  NOTE:
//    Some systems (especially hard disks) refer to this
//    as a Logical Block Address (LBA). Since the block and
//    sector on a hard driver are essentially the same thing
//    there is little confusion. However, with flash media
//    the distinction between a block and a sector is important
//    so we define the sector address to make it clear.
//
typedef UINT32 ByteAddress; 
typedef UINT32 SectorAddress;

//////////////////////////////////////////////////////////
// Description:
//   Flags to Indicate the status of a particular block in
//   flash. ALL sectors for a block *MUST* share the same
//   status.
//
// Remarks:
//   When the block is reserved the lower 16 bits specifies a
//   USAGE_XXXX value to indicate the purpose of the block.
// 
//   The BlockRange information is technically dynamic in the 
//   sense that it is not fixed or defined in hardware. Rather,
//   it is defined by the system designer. However, the BlockRange
//   info does not and *MUST* not change at run-time.  The only
//   exceptions to this rule is in the boot loader where the
//   storage device is being formated or re-laid out as part
//   of an update etc... and when an FS discovers a bad block
//   it should mark it as bad. WIthout this restriction
//   implementing an FS is virtually impossible since the FS
//   would have to deal with the possibility that the usage for
//   a Block could change at any point in time.
//
struct BlockUsage
{
    static const UINT32 BOOTSTRAP   = 0x0010;
    static const UINT32 CODE        = 0x0020;
    static const UINT32 CONFIG      = 0x0030;
    static const UINT32 FILESYSTEM  = 0x0040;    

    static const UINT32 DEPLOYMENT  = 0x0050; 

    static const UINT32 UPDATE      = 0x0060;

    static const UINT32 SIMPLE_A    = 0x0090;
    static const UINT32 SIMPLE_B    = 0x00A0;
    
    static const UINT32 STORAGE_A   = 0x00E0;
    static const UINT32 STORAGE_B   = 0x00F0;


    static const UINT32 ANY         = 0x0000;
};

struct BlockRange
{
    
    // upper 4 bits of USAGE_XXXX identifies the base type of info stored in the block
    // using these values it is possible to scan a device for all managed code or all
    // native code without worrying about the finer details of what it's for.

private:    
    static const UINT32 DATATYPE_NATIVECODE  = 0x1000;  // Block contains XIP system native code
    static const UINT32 DATATYPE_MANAGEDCODE = 0x2000;  // Block contains managed code assemblies
    static const UINT32 DATATYPE_RAW         = 0x4000;  // Block contains raw data

    static const UINT32 ATTRIB_PRIMARY       = 0x10000; // use to mark the block is used for special purpose

    
    static const UINT32 EXECUTABLE = 0x80000000;
    static const UINT32 RESERVED   = 0x40000000;
    static const UINT32 READONLY   = 0x20000000;
    

public:
    // Values for the Usage information (This helps map the new storage APIs to the needs of existing code)
    static const UINT32 ALL_MASK           = 0xFFFFFFFF;
    static const UINT32 USAGE_MASK         = 0x000000FF;
    static const UINT32 NON_USAGE_MASK     = 0xFFFFFF00;

    static const UINT32 BLOCKTYPE_RESERVED   = RESERVED;
    static const UINT32 BLOCKTYPE_DIRTYBIT   =                  RESERVED | DATATYPE_RAW         | BlockUsage::CONFIG;     // for secondary devices to set dirtybits         
    static const UINT32 BLOCKTYPE_CONFIG     = ATTRIB_PRIMARY | RESERVED | DATATYPE_RAW         | BlockUsage::CONFIG;     // Configuration data that contains all the unique data

    static const UINT32 BLOCKTYPE_BOOTSTRAP  = EXECUTABLE     | RESERVED | DATATYPE_NATIVECODE  | BlockUsage::BOOTSTRAP;  // Boot loader and boot strap code
    static const UINT32 BLOCKTYPE_CODE       = EXECUTABLE     | RESERVED | DATATYPE_NATIVECODE  | BlockUsage::CODE;       // CLR or other native code "application"
    static const UINT32 BLOCKTYPE_DEPLOYMENT =                  RESERVED | DATATYPE_MANAGEDCODE | BlockUsage::DEPLOYMENT; // Deployment area for MFdeploy & Visual Studio
    static const UINT32 BLOCKTYPE_SIMPLE_A   =                  RESERVED | DATATYPE_RAW         | BlockUsage::SIMPLE_A;   // Part A of Simple Storage
    static const UINT32 BLOCKTYPE_SIMPLE_B   =                  RESERVED | DATATYPE_RAW         | BlockUsage::SIMPLE_B;   // Part B of Simple Storage
    static const UINT32 BLOCKTYPE_STORAGE_A  =                  RESERVED | DATATYPE_RAW         | BlockUsage::STORAGE_A;  // Part A of EWR Storage
    static const UINT32 BLOCKTYPE_STORAGE_B  =                  RESERVED | DATATYPE_RAW         | BlockUsage::STORAGE_B;  // Part B of EWR Storage
    static const UINT32 BLOCKTYPE_FILESYSTEM =                             DATATYPE_RAW         | BlockUsage::FILESYSTEM; // File System
    static const UINT32 BLOCKTYPE_UPDATE     =                  RESERVED | DATATYPE_RAW         | BlockUsage::UPDATE;     // Used for MFUpdate for firmware/assembly/etc updates

    static BOOL IsBlockTinyBooterAgnostic( UINT32 BlockType )
    {
        // The only blocks that should be distinguished by TinyBooter are CONFIG, 
        // Bootstrap and reserved blocks (DirtyBit is another version of CONFIG).
        if( BlockType == BlockRange::BLOCKTYPE_BOOTSTRAP || 
            BlockType == BlockRange::BLOCKTYPE_CONFIG    ||
            BlockType == BlockRange::BLOCKTYPE_RESERVED  ||
            BlockType == BlockRange::BLOCKTYPE_DIRTYBIT)
        {
            return FALSE;
        }    

        return TRUE;        
    }
    
    BOOL IsReserved() const      { return ((RangeType & RESERVED) == RESERVED);       }
    BOOL IsReadOnly() const      { return ((RangeType & READONLY) == READONLY);       }

    BOOL IsReservedData() const    { return ((RangeType & (RESERVED | DATATYPE_RAW )) == (RESERVED | DATATYPE_RAW )); }
    BOOL HasManagedCode() const { return ((RangeType & (DATATYPE_MANAGEDCODE    )) == (DATATYPE_MANAGEDCODE    )); }

    BOOL IsCode() const               { return ((RangeType & USAGE_MASK) == BlockUsage::CODE);     }
    BOOL IsBootstrap() const        { return ((RangeType & USAGE_MASK) == BlockUsage::BOOTSTRAP);}
    BOOL IsDirtyBit() const         { return ((RangeType & BLOCKTYPE_CONFIG) == BLOCKTYPE_DIRTYBIT);}
    BOOL IsConfig() const           { return ((RangeType & BLOCKTYPE_CONFIG) == BLOCKTYPE_CONFIG); }
    BOOL IsDeployment() const  { return ((RangeType & USAGE_MASK) == BlockUsage::DEPLOYMENT);}
    BOOL IsFileSystem() const     { return ((RangeType & USAGE_MASK) == BlockUsage::FILESYSTEM); }
    UINT32 GetBlockCount() const    { return (EndBlock - StartBlock + 1); }

    // NOTE: This is the application native code only (not including the managed DAT section)
    //       and thus is different from the old MEMORY_USAGE_CODE which contained both 
    //       the Native code application and the DAT section. Obviously these inlines can be
    //       altered or added upon to test for any combination of the flags as desired but seperating
    //       the DAT region out on it's own allows for locating ANY managed code sections of storage
    BOOL IsLegacyCode() const    { return (IsCode());     }
    UINT32 GetBlockUsage() const { return (RangeType & USAGE_MASK); }

        
    /*
       Due to the lack of a defined bit ordering for bit fields in the C/C++
       languages a bit field structure isn't actually used but this should
       help clarify the layout and intent of the constant declarations below.
        
        //MSB
        unsigned EXECUTABLE:1;
        unsigned RESERVED:1;
        unsigned READONLY:1;
        unsigned UNUSEDBITS:13;
        
        // The lower 16 bits are used to define the specific
        // usage for blocks when the RESERVED bit set
        unsigned BlockType:4
        unsigned Usage:12;
        //LSB
    */    
    UINT32 RangeType;
    UINT32 StartBlock;
    UINT32 EndBlock;
};    

/////////////////////////////////////////////////////////
// Description:
//    This structure defines characteristics of a particular
//    region of a block device.
// 
// Remarks:
//    There is often more than one instance of this structure for each 
//    block device. 
//
//    The BytesPerBlock value is an optimization to prevent the need
//    to routinely caclulate it from SectorsPerBlock * DataBytesPerSector
//
struct BlockRegionInfo
{
    UINT32      Size()                                const { return (NumBlocks * BytesPerBlock);            }
    ByteAddress BlockAddress(UINT32 blockIndex)       const { return (Start + (blockIndex * BytesPerBlock)); }
    UINT32      OffsetFromBlock(UINT32 Address)       const { return ((Address - Start) % BytesPerBlock);    }
    UINT32      BlockIndexFromAddress(UINT32 Address) const { return ((Address - Start) / BytesPerBlock);    }

    ByteAddress     Start;                  // Starting Sector address
    UINT32          NumBlocks;              // total number of blocks in this region
    UINT32          BytesPerBlock;          // Total number of bytes per block

    UINT32            NumBlockRanges;
    const BlockRange *BlockRanges;
};

/////////////////////////////////////////////////////////
// Description:
//    This structure defines characteristics of a particular
//    block device.
// 
// Remarks:
//    THere is only one instance of this structure for each 
//    block device. 
//

struct MediaAttribute
{
    BOOL Removable :1;
    BOOL SupportsXIP:1;
    BOOL WriteProtected:1;
    BOOL SupportsCopyBack:1;
    BOOL ErasedBitsAreZero:1;
};
    
///////////////////////////////////////////////////////////
    
struct BlockDeviceInfo
{
    // indicates if the storage media is removeable

    MediaAttribute Attribute;

    // Maximum Sector Write Time.
    UINT32 MaxSectorWrite_uSec;    

    // Maximum erase time for the a block
    UINT32 MaxBlockErase_uSec;    

    // Bytes Per Sector
    UINT32 BytesPerSector;         

    // Total Size
    ByteAddress Size;

    // count of regions in the flash.
    UINT32 NumRegions;

    // pointer to an array (NumRegions long) of region information
    const BlockRegionInfo *Regions;

    SectorAddress PhysicalToSectorAddress( const BlockRegionInfo* pRegion, ByteAddress phyAddress ) const;

    BOOL FindRegionFromAddress(ByteAddress Address, UINT32 &BlockRegionIndex, UINT32 &BlockRangeIndex ) const;

    BOOL FindForBlockUsage(UINT32 BlockUsage, ByteAddress &Address, UINT32 &BlockRegionIndex, UINT32 &BlockRangeIndex ) const;

    BOOL FindNextUsageBlock(UINT32 BlockUsage, ByteAddress &Address, UINT32 &BlockRegionIndex, UINT32 &BlockRangeIndex ) const;
};

///////////////////////////////////////////////////////////////
// Description:
//    This structure describes the sector Metadata used for wear
//    leveling. 
//
// Remarks:
//    This structure emulates the typical physical layout of the
//    extra area of flash. The wear leveling layer for NAND and
//    NOR flash supplied by Microsoft for Windows CE makes use
//    of 8 bytes of the Sector Extra Info area.
//
//    This information is designed to match that used in
//    Windows Embedded CE systems so that the FAL algorithms from
//    CE can be more easily ported to the .NET MF. 
//
// The following is a typical representation of how the extra area
// is utilized:
//- - - - - - - - - - - - - - - - 
//|R|R|R|R|O|V|R|R|E|E|E|E|E|E|E|E|
//- - - - - - - - - - - - - - - -
//
//The following table describes each element.
//
//Element  Description  
//   R     Reserved bytes used by the FAL
//   O     Byte for use by the OEM
//   V     Byte indicating if the block is valid (a.k.a. bad)
//   E     Bytes typically used for by a NAND driver for ECC
//
ADS_PACKED 
struct GNU_PACKED SectorMetadata
{
    DWORD dwReserved1; // Used by the FAL to hold the logical to physical sector mapping information.
    BYTE bOEMReserved; // For use by OEM. See OEMReservedBits for more information.
    BYTE bBadBlock;    // Indicates if a block is bad.
    WORD wReserved2;   // Used by the FAL to maintain state information about the sector.
    
    // TODO: Check ECC algorithm implementations on CE to see what data type works most conveniently for this
    UINT32 ECC[2];     // Error Correction Code [Should be all 0xFF if not used]

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
    static const BYTE OEM_BLOCK_RESERVED = 0x01;
    static const BYTE OEM_BLOCK_READONLY = 0x02;
};

//--//

struct BLOCK_CONFIG
{
    GPIO_FLAG               WriteProtectionPin;
    const BlockDeviceInfo*  BlockDeviceInformation;
};

struct MEMORY_MAPPED_NOR_BLOCK_CONFIG
{
    BLOCK_CONFIG            BlockConfig;
    CPU_MEMORY_CONFIG       Memory;
    UINT32                  ChipProtection;
    UINT32                  ManufacturerCode;
    UINT32                  DeviceCode;
};

/////////////////////////////////////////////////////////
// Description:
//    This structure defines an interface for block devices
// 
// Remarks:
//    It is possible a given system might have more than one
//    storage device type. This interface abstracts the
//    hardware sepcifics from the rest of the system.
//
//    All of the functions take at least one void* parameter
//    that normally points to a driver specific data structure
//    containing hardware specific settings to use. This
//    allows a single driver to support multiple instances of
//    the same type of storage device in the system.
//
//    The sector read and write functions provide a parameter
//    for Sector Metadata. The metadata is used for flash arrays
//    without special controllers to manage wear leveling etc...
//    (mostly for directly attached NOR and NAND). The metadata
//    is used by upper layers for wear leveling to ensure that
//    data is moved around on the flash when writing to prevent
//    failure of the device from too many erase cycles on a sector. 
// 
// TODO:
//    Define standard method of notification that media is
//    removed for all removeable media. This will likely
//    be a continuation so that the FS Manager can mount 
//    an FS and then notify the managed app of the new FS.
//
struct IBlockStorageDevice
{
    /////////////////////////////////////////////////////////
    // Description:
    //    Initializes a given block device for use
    // 
    // Input:
    //
    // Returns:
    //   true if succesful; false if not
    //
    // Remarks:
    //    No other functions in this interface may be called
    //    until after Init returns.
    //
    BOOL (*InitializeDevice)(void*);
    
    /////////////////////////////////////////////////////////
    // Description:
    //    Initializes a given block device for use
    // 
    // Returns:
    //   true if succesful; false if not
    //
    // Remarks:
    //   De initializes the device when no longer needed
    //
    BOOL (*UninitializeDevice)(void*);

    /////////////////////////////////////////////////////////
    // Description:
    //    Gets the information describing the device
    //
    const BlockDeviceInfo*  (*GetDeviceInfo)(void*);
    
    /////////////////////////////////////////////////////////
    // Description:
    //    Reads data from a set of sectors
    //
    // Input:
    //    StartSector - Starting Sector for the read
    //    NumSectors  - Number of sectors to read
    //    pSectorBuff - pointer to buffer to read the data into.
    //                  Must be large enough to hold all of the data
    //                  being read.
    //
    //    pSectorMetadata - pointer to an array of structured (one for each sector)
    //                      for the extra sector information.
    // 
    // Returns:
    //   true if succesful; false if not
    //
    // Remarks:
    //   This function reads the number of sectors specified from the device.
    //   
    //   pSectorBuff may be NULL. This is to allow for reading just the metadata.
    // 
    //   pSectorMetadata can be set to NULL if the caller does not need the extra
    //   data.
    //
    //   If the device does not support sector Metadata it should fail if the 
    //   pSectorMetadata parameter is not NULL.
    //
    BOOL (*Read)(void*, ByteAddress StartSector, UINT32 NumBytes, BYTE* pSectorBuff);

    /////////////////////////////////////////////////////////
    // Description:
    //    Writes data to a set of sectors
    //
    // Input:
    //    StartSector - Starting Sector for the write
    //    NumSectors  - Number of sectors to write
    //    pSectorBuff - pointer to data to write.
    //                  Must be large enough to hold complete sectors
    //                  for the number of sectors being written.
    //
    //    pSectorMetadata - pointer to an array of structures (one for each sector)
    //                      for the extra sector information.
    // 
    // Returns:
    //   true if succesful; false if not
    //
    // Remarks:
    //   This function reads the number of sectors specified from the device.
    //   The SectorMetadata is used for flash arrays without special controllers
    //   to manage wear leveling etc... (mostly for NOR and NAND). The metadata
    //   is used by upper layers to ensure that data is moved around on the flash
    //   when writing to prevent failure of the device from too many erase cycles
    //   on a sector. 
    //   
    //   If the device does not support sector Metadata it should fail if the 
    //   pSectorMetadata parameter is not NULL.
    //
    //   pSectorMetadata can be set to NULL if the caller does not need the extra
    //   data. Implementations must not attempt to write data through a NULL pointer! 
    //
    BOOL (*Write)(void*, ByteAddress Address, UINT32 NumBytes, BYTE* pSectorBuf, BOOL ReadModifyWrite);

    BOOL (*Memset)(void*, ByteAddress Address, UINT8 Data, UINT32 NumBytes);

    BOOL (*GetSectorMetadata)(void*, ByteAddress SectorStart, SectorMetadata* pSectorMetadata);

    BOOL (*SetSectorMetadata)(void*, ByteAddress SectorStart, SectorMetadata* pSectorMetadata);

    /////////////////////////////////////////////////////////
    // Description:
    //    Check a block is erased or not.
    // 
    // Input:
    //    BlockStartAddress - Logical Sector Address
    //
    // Returns:
    //   true if it is erassed, otherwise false
    //
    // Remarks:
    //    Check  the block containing the sector address specified.
    //    
    BOOL (*IsBlockErased)(void*, ByteAddress BlockStartAddress, UINT32 BlockLength);

    /////////////////////////////////////////////////////////
    // Description:
    //    Erases a block
    // 
    // Input:
    //    Address - Logical Sector Address
    //
    // Returns:
    //   true if succesful; false if not
    //
    // Remarks:
    //    Erases the block containing the sector address specified.
    //    
    BOOL (*EraseBlock)(void*, ByteAddress Address);
    
    /////////////////////////////////////////////////////////
    // Description:
    //   Changes the power state of the device
    // 
    // Input:
    //    State   - true= power on; false = power off
    //
    // Remarks:
    //   This function allows systems to conserve power by 
    //   shutting down the hardware when the system is 
    //   going into low power states.
    //
    void (*SetPowerState)(void*, UINT32 State);

    UINT32 (*MaxSectorWrite_uSec)(void*);

    UINT32 (*MaxBlockErase_uSec)(void*);
};


////////////////////////////////////////////////
// Description:
//   Binding context for a driver and the physical device
//
// Remarks:
//   The design pattern here effectively mimics a C++ class
//   with virtuals. The reason virtuals are not used is that
//   the .NET MF supports a wide variety of compiler/Linker
//   tool chains and some of them bring in a large Run-time
//   library footprint when Certain C++ language features are
//   used. Since a major goal of the .NET MF is to reduce
//   code footprint we avoid anything that brings in additional
//   library code. 
//

struct BlockStorageDevice : public HAL_DblLinkedNode<BlockStorageDevice>
{

public:

    /////////////////////////////////////////////////////////
    // Description:
    //    Initializes a given block device for use
    // 
    // Returns:
    //   true if succesful; false if not
    //
    // Remarks:
    //    No other functions in this interface may be called
    //    until after Init returns.
    //
    BOOL InitializeDevice() { return this->m_BSD->InitializeDevice( this->m_context ); }
    
    /////////////////////////////////////////////////////////
    // Description:
    //    Initializes a given block device for use
    // 
    // Returns:
    //   true if succesful; false if not
    //
    // Remarks:
    //   De initializes the device when no longer needed
    //
    BOOL UninitializeDevice() { return this->m_BSD->UninitializeDevice( this->m_context ); }

    /////////////////////////////////////////////////////////
    // Description:
    //    Gets the information describing the device
    //
    const BlockDeviceInfo* GetDeviceInfo() { return this->m_BSD->GetDeviceInfo( this->m_context ); }    

    /////////////////////////////////////////////////////////
    // Description:
    //    Reads data from a set of sectors
    //
    // Input:
    //    StartSector - Starting Sector for the read
    //    NumSectors  - Number of sectors to read
    //    pSectorBuff - pointer to buffer to read the data into.
    //                  Must be large enough to hold all of the data
    //                  being read.
    //
    //    pSectorMetadata - pointer to an array of structured (one for each sector)
    //                      for the extra sector information.
    // 
    // Returns:
    //   true if succesful; false if not
    //
    // Remarks:
    //   This function reads the number of sectors specified from the device.
    //   
    //   pSectorBuff may be NULL. This is to allow for reading just the metadata.
    // 
    //   pSectorMetadata can be set to NULL if the caller does not need the extra
    //   data.
    //
    //   If the device does not support sector Metadata it should fail if the 
    //   pSectorMetadata parameter is not NULL.
    //
    BOOL Read(ByteAddress Address, UINT32 NumBytes, BYTE* pSectorBuff) 
    {
        return this->m_BSD->Read(this->m_context, Address, NumBytes, pSectorBuff);
    }

    /////////////////////////////////////////////////////////
    // Description:
    //    Writes data to a set of sectors
    //
    // Input:
    //    StartSector - Starting Sector for the write
    //    NumSectors  - Number of sectors to write
    //    pSectorBuff - pointer to data to write.
    //                  Must be large enough to hold complete sectors
    //                  for the number of sectors being written.
    //
    //    pSectorMetadata - pointer to an array of structures (one for each sector)
    //                      for the extra sector information.
    // 
    // Returns:
    //   true if succesful; false if not
    //
    // Remarks:
    //   This function reads the number of sectors specified from the device.
    //   The SectorMetadata is used for flash arrays without special controllers
    //   to manage wear leveling etc... (mostly for NOR and NAND). The metadata
    //   is used by upper layers to ensure that data is moved around on the flash
    //   when writing to prevent failure of the device from too many erase cycles
    //   on a sector. 
    //   
    //   If the device does not support sector Metadata it should fail if the 
    //   pSectorMetadata parameter is not NULL.
    //
    //   pSectorMetadata can be set to NULL if the caller does not need the extra
    //   data. Implementations must not attempt to write data through a NULL pointer! 
    //
    BOOL Write(ByteAddress Address, UINT32 NumBytes, BYTE* pSectorBuf, BOOL ReadModifyWrite) 
    {
        return this->m_BSD->Write(this->m_context, Address, NumBytes, pSectorBuf, ReadModifyWrite);
    }

    BOOL Memset(ByteAddress Address, UINT8 Data, UINT32 NumBytes)
    {
        return this->m_BSD->Memset(this->m_context, Address, Data, NumBytes);
    }

    BOOL GetSectorMetadata(ByteAddress SectorStart, SectorMetadata* pSectorMetadata)
    {
        return this->m_BSD->GetSectorMetadata(this->m_context, SectorStart, pSectorMetadata);
    }

    BOOL SetSectorMetadata(ByteAddress SectorStart, SectorMetadata* pSectorMetadata)
    {
        return this->m_BSD->SetSectorMetadata(this->m_context, SectorStart, pSectorMetadata);
    }

    /////////////////////////////////////////////////////////
    // Description:
    //    Check a block is erased or not
    // 
    // Input:
    //    Address - Logical Sector Address
    //
    // Returns:
    //   true it is erased; false if not
    //
    // Remarks:
    //    check the block containing the sector address specified.
    //    
    BOOL IsBlockErased(ByteAddress BlockStartAddress, UINT32 BlockLength)  { return this->m_BSD->IsBlockErased(this->m_context, BlockStartAddress, BlockLength); }


    /////////////////////////////////////////////////////////
    // Description:
    //    Erases a block
    // 
    // Input:
    //    Address - Logical Sector Address
    //
    // Returns:
    //   true if succesful; false if not
    //
    // Remarks:
    //    Erases the block containing the sector address specified.
    //    
    BOOL EraseBlock(ByteAddress Address) const { return this->m_BSD->EraseBlock(this->m_context, Address); }


    /////////////////////////////////////////////////////////
    // Description:
    //   Changes the power state of the device
    // 
    // Input:
    //    State   - true= power on; false = power off
    //
    // Remarks:
    //   This function allows systems to conserve power by 
    //   shutting down the hardware when the system is 
    //   going into low power states.
    //
    void SetPowerState( UINT32 State ) { this->m_BSD->SetPowerState(this->m_context, State);  }

    BOOL FindRegionFromAddress(ByteAddress Address, UINT32 &BlockRegionIndex, UINT32 &BlockRangeIndex ) 
    {
        const BlockDeviceInfo* pDevInfo = GetDeviceInfo();

        return pDevInfo->FindRegionFromAddress( Address, BlockRegionIndex, BlockRangeIndex );
    }

    BOOL FindForBlockUsage(UINT32 blockUsage, ByteAddress &Address, UINT32 &BlockRegionIndex, UINT32 &BlockRangeIndex ) 
    {
        const BlockDeviceInfo* pDevInfo = GetDeviceInfo();

        return pDevInfo->FindForBlockUsage( blockUsage, Address, BlockRegionIndex, BlockRangeIndex );
    }

    BOOL FindNextUsageBlock(UINT32 blockUsage, ByteAddress &Address, UINT32 &BlockRegionIndex, UINT32 &BlockRangeIndex ) 
    {
        const BlockDeviceInfo* pDevInfo = GetDeviceInfo();

        return pDevInfo->FindNextUsageBlock( blockUsage, Address, BlockRegionIndex, BlockRangeIndex );
    }
    

    UINT32 MaxSectorWrite_uSec()
    {
        return this->m_BSD->MaxSectorWrite_uSec(this->m_context);
    }

    UINT32 MaxBlockErase_uSec()
    {
        return this->m_BSD->MaxBlockErase_uSec(this->m_context);
    }

    IBlockStorageDevice* m_BSD;     // Vtable for this device
    void*                m_context; // configuration for this instance of this driver

};


struct BlockStorageStream
{
    static const INT32 STREAM_SEEK_NEXT_BLOCK = 0x7FFFFFFF;
    static const INT32 STREAM_SEEK_PREV_BLOCK = 0x7FFFFFFE;

    static const UINT32 c_BlockStorageStream__XIP          = 0x00000001;
    static const UINT32 c_BlockStorageStream__ReadModWrite = 0x00000002;
    
    enum SeekOrigin
    {
        SeekBegin   = 0,
        SeekCurrent = 1,
        SeekEnd     = 2,
    };


    //--//
    
    ByteAddress BaseAddress;
    UINT32 CurrentIndex;
    UINT32 Length;
    UINT32 BlockLength;
    UINT32 Usage;
    UINT32 RegionIndex;
    UINT32 RangeIndex;
    UINT32 Flags;
    UINT32 CurrentUsage;
    BlockStorageDevice *Device;

    //--//

    BOOL IsXIP()            { return 0 != (Flags & c_BlockStorageStream__XIP);          }
    BOOL IsReadModifyWrite() { return 0 != (Flags & c_BlockStorageStream__ReadModWrite); }
    void SetReadModifyWrite(){ Flags |= c_BlockStorageStream__ReadModWrite;              }

    BOOL Initialize(UINT32 blockUsage); 
    BOOL Initialize(UINT32 blockUsage, BlockStorageDevice* pDevice); 
    BOOL NextStream();
    BOOL PrevStream();
    BOOL Seek( INT32 offset, SeekOrigin origin = SeekCurrent );
    BOOL Write( UINT8* data  , UINT32 length );
    BOOL Erase( UINT32 length );
    BOOL ReadIntoBuffer( UINT8*  pBuffer, UINT32 length );
    BOOL Read( UINT8** ppBuffer, UINT32 length );
    UINT32 CurrentAddress();
    BOOL IsErased( UINT32 length );
};
    
// -- global List

struct BlockStorageList
{
    // initailize the storage
    static void Initialize();
    
    // walk through list of devices and calls Init() function
    static BOOL InitializeDevices();

    // walk through list of devices and calls UnInit() function
    static BOOL UnInitializeDevices();

    // add pBSD to the list
    // If Init=true, the Init() will be called.
    static BOOL AddDevice( BlockStorageDevice* pBSD, IBlockStorageDevice* vtable, void* config, BOOL Init);

    // remove pBSD from the list
    // Uninit = true, UnInit() will be called.
    static BOOL RemoveDevice( BlockStorageDevice* pBSD, BOOL UnInit);

    // Find the right Device with the corresponding phyiscal address.
    // 
    static BOOL FindDeviceForPhysicalAddress( BlockStorageDevice** pBSD, UINT32 PhysicalAddress, ByteAddress &BlockAddress);

    static BlockStorageDevice* GetFirstDevice();
    
    static BlockStorageDevice* GetNextDevice( BlockStorageDevice& device );

    // returns number of devices has been declared in the system
    static UINT32 GetNumDevices();

    // pointer to the BlockStorageDevice which is the primary device with CONFIG block
    static BlockStorageDevice* s_primaryDevice;
    
private:
    // global pointer of all the storage devices
    static HAL_DblLinkedList<BlockStorageDevice> s_deviceList; 

    static BOOL s_Initialized;
};

////////////////////////////////////////////////////////////////////////////////

// functions to included all the devices to be added in the system
void BlockStorage_AddDevices();

///////////////////////////////////////////////////////////////////////////////

#define FLASH_PROTECTION_KEY      0x1055AADD



#define FLASH_BEGIN_PROGRAMMING_FAST()             { GLOBAL_LOCK(FlashIrq)
#define FLASH_BEGIN_PROGRAMMING(x)                 { UINT32 FlashOperationStatus = Flash_StartOperation( x )
#define FLASH_SLEEP_IF_INTERRUPTS_ENABLED(u)       if(!FlashOperationStatus) { Events_WaitForEventsInternal( 0, u/1000 ); }
#define FLASH_END_PROGRAMMING(banner,address)      Flash_EndOperation( FlashOperationStatus ); }
#define FLASH_END_PROGRAMMING_FAST(banner,address) }


//--//

#endif // #if define _DRIVERS_PAL_BLOCKSTORAGE_H_

