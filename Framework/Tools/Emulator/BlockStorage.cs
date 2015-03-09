////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using System.Text;

namespace Microsoft.SPOT.Emulator.BlockStorage
{
    internal class BlockStorageDriver : HalDriver<IBlockStorageDriver>, IBlockStorageDriver
    {
        static uint s_BaseAddress = 0x10000000;
        
        private BlockStorageDevice GetBlockStorageDevice(uint context)
        {
            return this.Emulator.BlockStorageDevices[context];
        }

        static internal uint GetNextBaseAddress()
        {
            uint addr = s_BaseAddress;
            s_BaseAddress += 0x10000000;
            return addr;
        }

        internal static InternalBlockRegionInfo[] ConvertRegionsToInternalRegions(Region[] regions)
        {
            int numRegions = regions.Length;
            InternalBlockRegionInfo[] internalRegions = new InternalBlockRegionInfo[numRegions];

            for (int j = 0; j < numRegions; j++)
            {
                int numRanges = regions[j].BlockRanges.Length;

                if(regions[j].Address == 0)
                {
                    internalRegions[j].Start = GetNextBaseAddress();
                }
                else
                {
                    internalRegions[j].Start          = regions[j].Address;
                }
                internalRegions[j].NumBlockRanges = (uint)numRanges;
                internalRegions[j].BytesPerBlock  = regions[j].BytesPerBlock;
                internalRegions[j].NumBlocks      = regions[j].TotalBlockCount;

                internalRegions[j].BlockRanges    = new InternalBlockRange[numRanges];

                for(int k = 0; k < numRanges; k++)
                {
                    internalRegions[j].BlockRanges[k].RangeType  = (uint)regions[j].BlockRanges[k].RangeType;
                    internalRegions[j].BlockRanges[k].StartBlock =       regions[j].BlockRanges[k].StartBlock;
                    internalRegions[j].BlockRanges[k].EndBlock   =       regions[j].BlockRanges[k].EndBlock;                    
                }
                
            }

            return internalRegions;
        }

        #region IBlockStorageDriver Members

        InternalBlockDeviceInfo[] IBlockStorageDriver.GetDevicesInfo()
        {
            int numDevices = this.Emulator.BlockStorageDevices.Count;

            InternalBlockDeviceInfo[] devices = new InternalBlockDeviceInfo[numDevices];

            for (int i = 0; i < numDevices; i++)
            {
                BlockStorageDevice device = GetBlockStorageDevice((uint)i);
                
                devices[i].BytesPerSector      = device.BytesPerSector;
                devices[i].Removable           = device.Removable;
                devices[i].SupportsXIP         = device.SupportsXIP;
                devices[i].SupportsCopyBack    = device.SupportsCopyBack;
                devices[i].WriteProtected      = device.WriteProtected;
                devices[i].MaxBlockErase_uSec  = device.MaxBlockEraseTime;
                devices[i].MaxSectorWrite_uSec = device.MaxSectorWriteTime;
                devices[i].NumRegions          = (uint)device.Regions.Length;
                devices[i].Regions             = (device.Regions.Length == 0) ? null : ConvertRegionsToInternalRegions(device.Regions);
            }

            return devices;
        }

        void IBlockStorageDriver.MountInsertedRemovableDevices()
        {
            foreach (BlockStorageDevice bsd in Emulator.BlockStorageDevices)
            {
                if (bsd is RemovableBlockStorageDevice && bsd.Inserted)
                {
                    Emulator.BlockStorageDevices.InsertRemovableBlockStorage((RemovableBlockStorageDevice)bsd);
                }
            }
        }

        bool IBlockStorageDriver.Initialize(uint context)
        {
            try
            {
                GetBlockStorageDevice(context).Initialize();
            }
            catch
            {
                return false;
            }

            return true;
        }

        bool IBlockStorageDriver.Uninitialize(uint context)
        {
            try
            {
                GetBlockStorageDevice(context).Uninitialize();
            }
            catch 
            {
                return false;
            }

            return true;
        }

        bool IBlockStorageDriver.Read(uint context, uint address, uint length, IntPtr sectorBuff)
        {
            try
            {
                GetBlockStorageDevice(context).Read(address, length, sectorBuff);
            }
            catch 
            {
                return false;
            }

            return true;
        }

        bool IBlockStorageDriver.Write(uint context, uint address, uint length, IntPtr sectorBuff, bool readModifyWrite)
        {
            try
            {
                GetBlockStorageDevice(context).Write(address, length, sectorBuff, readModifyWrite);
            }
            catch 
            {
                return false;
            }

            return true;
        }

        bool IBlockStorageDriver.Memset(uint context, uint address, byte data, uint length)
        {
            try
            {
                GetBlockStorageDevice(context).Memset(address, data, length);
            }
            catch 
            {
                return false;
            }

            return true;
        }

        bool IBlockStorageDriver.GetSectorMetadata(uint context, uint address, ref BsSectorMetadata sectorMetadata)
        {
            try
            {
                GetBlockStorageDevice(context).GetSectorMetadata(address, ref sectorMetadata);
            }
            catch 
            {
                return false;
            }

            return true;
        }

        bool IBlockStorageDriver.SetSectorMetadata(uint context, uint address, ref BsSectorMetadata sectorMetadata)
        {
            try
            {
                GetBlockStorageDevice(context).SetSectorMetadata(address, ref sectorMetadata);
            }
            catch 
            {
                return false;
            }

            return true;
        }

        bool IBlockStorageDriver.IsBlockErased(uint context, uint address, uint blockLength)
        {
            try
            {
                return GetBlockStorageDevice(context).IsBlockErased(address, blockLength);
            }
            catch 
            {
            }

            return false;
        }

        bool IBlockStorageDriver.EraseBlock(uint context, uint address)
        {
            try
            {
                GetBlockStorageDevice(context).EraseBlock(address);
            }
            catch 
            {
                return false;
            }

            return true;
        }
        void IBlockStorageDriver.SetPowerState(uint context, uint state)
        {
            GetBlockStorageDevice(context).SetPowerState(state);
        }

        uint IBlockStorageDriver.MaxBlockErase_uSec(uint context)
        {
            return GetBlockStorageDevice(context).MaxBlockErase_uSec();
        }

        uint IBlockStorageDriver.MaxSectorWrite_uSec(uint context)
        {
            return GetBlockStorageDevice(context).MaxSectorWrite_uSec();
        }

        #endregion
    }

    public class BlockStorageCollection : EmulatorComponentCollection
    {
        List<BlockStorageDevice> _blockStorageDevices;

        public BlockStorageCollection()
            : base(typeof(BlockStorageDevice))
        {
            _blockStorageDevices = new List<BlockStorageDevice>();
        }

        public BlockStorageDevice this[uint context]
        {
            get
            {
                if (context >= _blockStorageDevices.Count)
                {
                    throw new ArgumentException("The specified context, " + context + ", does not exist.");
                }

                return _blockStorageDevices[(int)context];
            }
        }

        public override System.Collections.IEnumerator GetEnumerator()
        {
            return _blockStorageDevices.GetEnumerator();
        }

        public override int Count
        {
            get { return _blockStorageDevices.Count; }
        }

        public override void CopyTo(Array array, int index)
        {
            BlockStorageDevice[] bsdArray = array as BlockStorageDevice[];
            if (bsdArray == null)
            {
                throw new ArgumentException("Cannot cast array into BlockStorageDevice[]");
            }
            _blockStorageDevices.CopyTo(bsdArray, index);
        }

        internal int GetContext(BlockStorageDevice bsd)
        {
            return _blockStorageDevices.FindIndex(delegate(BlockStorageDevice b) { return b.Equals(bsd); });
        }

        internal override void RegisterInternal(EmulatorComponent ec)
        {
            // The list of block storage devices cannot change after configuration is completed
            // or the context will get mixed up
            ThrowIfNotSetup();

            BlockStorageDevice bsd = ec as BlockStorageDevice;

            if (bsd == null)
            {
                throw new Exception("Attempt to register a non BlockStorageDevice with BlockStorageCollection.");
            }

            _blockStorageDevices.Add(bsd);

            base.RegisterInternal(ec);
        }

        internal override void UnregisterInternal(EmulatorComponent ec)
        {
            // The list of block storage devices cannot change after configuration is completed
            // or the context will get mixed up
            ThrowIfNotSetup();

            BlockStorageDevice bsd = ec as BlockStorageDevice;

            if (bsd != null)
            {
                if (_blockStorageDevices.Remove(bsd) == false)
                {
                    Debug.Assert(false);
                    return;
                }

                base.UnregisterInternal(ec);
            }
        }

        public override bool IsReplaceableBy(EmulatorComponent ec)
        {
            return (ec is BlockStorageCollection);
        }

        public void InsertRemovableBlockStorage(RemovableBlockStorageDevice bsd)
        {
            if (bsd == null) throw new ArgumentNullException("bsd");

            int context = Emulator.BlockStorageDevices.GetContext(bsd);

            if (context < 0) throw new ArgumentException("Invalid bsd");

            InternalBlockRegionInfo[] regions = BlockStorageDriver.ConvertRegionsToInternalRegions(bsd.Regions);

            this.Emulator.EmulatorNative.InsertRemovableBlockStorage((uint)context, regions, Encoding.ASCII.GetBytes(bsd.Namespace), bsd.SerialNumber, bsd.DeviceFlags, bsd.BytesPerSector);
        }

        public void EjectRemovableBlockStorage(RemovableBlockStorageDevice bsd)
        {
            int context = Emulator.BlockStorageDevices.GetContext(bsd);

            if (context < 0) throw new ArgumentException("Invalid bsd");

            this.Emulator.EmulatorNative.EjectRemovableBlockStorage((uint)context);
        }
    }

    public class Region
    {
        uint _address;
        uint _bytesPerBlock;
        BlockRange[] _blocks;

        bool _isConfigurable;

        public Region()
        {
            _isConfigurable = true;
            _address = BlockStorageDriver.GetNextBaseAddress();
            _bytesPerBlock = 0;
            _blocks = null;
        }

        public Region(uint bytesPerBlock, BlockRange[] blockRanges)
        {
            _isConfigurable = false;
            _blocks         = blockRanges;
            _bytesPerBlock  = bytesPerBlock;

            ValidateRegion();
        }

        internal void ConfigurationDone()
        {
            _isConfigurable = false;
        }

        public uint BytesPerBlock
        {
            get { return _bytesPerBlock; }
            set
            {
                ThrowIfNotConfigurable();
                _bytesPerBlock= value;
            }
        }

        public BlockRange[] BlockRanges
        {
            get { return _blocks; }
            set
            {
                ThrowIfNotConfigurable();
                _blocks = value;
            }
        }

        public uint TotalBlockCount
        {
            get { return _blocks[_blocks.Length - 1].EndBlock + 1; }
        }

        public virtual bool Inserted
        {
            get { return true; }
        }

        public uint Address
        {
            get { return _address; }
        }

        internal uint AddressInternal
        {
            set { _address = value; }
        }

        public uint AddressEnd
        {
            get { return (uint)(_address + _bytesPerBlock * TotalBlockCount); }
        }


        private void ThrowIfNotConfigurable()
        {
            if (!_isConfigurable)
            {
                throw new Exception("This operation is not allowed while the emulator is past the configuration stage.");
            }
        }

        public void ValidateRegion()
        {
            if (_bytesPerBlock == 0)
            {
                throw new Exception("BytesPerSector cannot be 0");
            }

            if (_blocks == null || _blocks.Length == 0)
            {
                throw new Exception("There must be at least 1 block in a region");
            }
        }
    }



    public class BlockRange
    {
        BlockType _type;
        uint _startBlock;
        uint _blockCount;
        Region _region;

        public BlockRange() 
        {
            _startBlock = 0;
            _blockCount = 0;
            _region     = null;
        }

        public BlockRange(Region region, BlockType type, uint blockIndex, uint blockCount)
        {
            _type       = type;
            _startBlock = blockIndex;
            _blockCount = blockCount;
            _region     = region;
        }

        public Region Region
        {
            get { return _region; }
        }

        public BlockType RangeType
        {
            get { return _type; }
            set { _type = value; }
        }

        public uint StartBlock
        {
            set { _startBlock = value; }
            get { return _startBlock; }
        }

        public uint EndBlock
        {
            get { return _startBlock + BlockCount - 1; }
        }

        public uint BlockCount
        {
            set { _blockCount = value; }
            get { return _blockCount; }
        }
    }

    [Flags]
    public enum MediaAttribute : uint
    {
        None = 0,
        Removable = 0x1,
        SupportsXIP = 0x2,
        WriteProtected = 0x4,
        SupportsCopyBack = 0x8,
    }

    [Flags]
    public enum BlockUsage : uint
    {
        Bootstrap = 0x0010,
        Code = 0x0020,
        Config = 0x0030,
        FileSystem = 0x0040,
        Deployment = 0x0050,
        Dat = 0x0080,
        Storage_A = 0x00E0,
        Storage_B = 0x00F0,
        Simple_A = 0x90,
        Simple_B = 0xA0,
    }

    [Flags]
    public enum BlockType : uint
    {
        // Values for the Usage information (This helps map the new storage APIs to the needs of existing code)
        // Please keep in sync with BlockRange struct in BlockStorage_decl.h
        All_Mask = 0xFFFFFFFF,
        Usage_Mask = 0x000000FF,
        NonUsage_Mask = 0xFFFFFF00,

        Usage_NativeCode = 0x1000,  // Block contains c_Xip system native code
        Usage_ManagedCode = 0x2000,  // Block contains managed code assemblies
        Usage_Data = 0x4000,  // Block contains raw dat
        Usage_SpecialData = 0x10000, // use to mark the block is used for special purpose

        Bad = 0x80000000,
        Reserved = 0x40000000,
        ReadOnly = 0x20000000,
        Xip = Reserved | Usage_NativeCode, // Xip blocks are always Reserved and contain NATIVE CODE

        Usage_Bootstrap = Xip | BlockUsage.Bootstrap,                 // Boot loader and boot strap code
        Usage_Config = Reserved | Usage_Data | BlockUsage.Config,              // Configuration data
        Usage_PrimaryConfig = Reserved | Usage_SpecialData | Usage_Data | BlockUsage.Config,         // Configuration data that contains all the unique data
        Usage_Code = Xip | BlockUsage.Code,                                // CLR or other native code "application"
        Usage_Dat = Reserved | Usage_ManagedCode | BlockUsage.Dat,        // Built-In managed code
        Usage_Deployment = Reserved | Usage_ManagedCode | BlockUsage.Deployment, // Deployment area for MFdeploy & Visual Studio
        Usage_Storage_A = Reserved | Usage_Data | BlockUsage.Storage_A,         // Part A of EWR Storage
        Usage_Storage_B = Reserved | Usage_Data | BlockUsage.Storage_B,         // Part B of EWR Storage
        Usage_SimpleStorage_A = Reserved | Usage_Data | BlockUsage.Simple_A,         // Part A of Simple Storage
        Usage_SimpleStorage_B = Reserved | Usage_Data | BlockUsage.Simple_B,         // Part B of Simple Storage
        Usage_FileSystem = Usage_Data | BlockUsage.FileSystem,                  // Debug logging data
    }

    public abstract class BlockStorageDevice : EmulatorComponent
    {
        MediaAttribute _mediaAttribute;
        uint _maxSectorWrite_uSec;
        uint _maxBlockErase_uSec;
        uint _bytesPerSector;
        uint _powerState;
        Region[] _regions;
        bool _inserted;
        protected uint _baseAddress;

        public BlockStorageDevice()
        {
            _mediaAttribute = MediaAttribute.None;
            _maxSectorWrite_uSec = 10;
            _maxBlockErase_uSec = 10;
            _powerState = 1;
            _bytesPerSector = 4;
            _regions = new Region[0];
            _inserted = true;
        }

        #region Configurable Properties
        public MediaAttribute MediaAttribute
        {
            get { return _mediaAttribute; }
            set
            {
                ThrowIfNotConfigurable();
                if (!(this is RemovableBlockStorageDevice) && ((value & MediaAttribute.Removable) == MediaAttribute.Removable))
                {
                    throw new Exception("Device can only be Removable if it's a RemovableBlockStorageDevice");
                }

                if ((this is RemovableBlockStorageDevice) && ((value & MediaAttribute.Removable) != MediaAttribute.Removable))
                {
                    throw new Exception("RemovableBlockStorageDevice must have the Removable attribute.");
                }

                _mediaAttribute = value;
            }
        }

        public uint BytesPerSector
        {
            get { return _bytesPerSector; }
            set 
            {
                ThrowIfNotConfigurable();
                _bytesPerSector = value;
            }
        }

        public uint MaxSectorWriteTime
        {
            get { return _maxSectorWrite_uSec; }
            set
            {
                ThrowIfNotConfigurable();
                _maxSectorWrite_uSec = value;
            }
        }

        public uint MaxBlockEraseTime
        {
            get { return _maxBlockErase_uSec; }
            set
            {
                ThrowIfNotConfigurable();
                _maxBlockErase_uSec = value;
            }
        }

        public Region[] Regions
        {
            get { return _regions; }
            set
            {
                if ((this is RemovableBlockStorageDevice) == false)
                {
                    ThrowIfNotConfigurable();
                }

                if (value == null)
                {
                    _regions = new Region[0];
                }
                else
                {
                    _regions = value;
                }
            }
        }
        #endregion

        public uint PowerState
        {
            get { return _powerState; }
        }

        public bool SupportsXIP
        {
            get { return (_mediaAttribute & MediaAttribute.SupportsXIP) == MediaAttribute.SupportsXIP; }
        }

        public bool Removable
        {
            get { return (_mediaAttribute & MediaAttribute.Removable) == MediaAttribute.Removable; }
        }

        public bool WriteProtected
        {
            get { return (_mediaAttribute & MediaAttribute.WriteProtected) == MediaAttribute.WriteProtected; }
        }

        public bool SupportsCopyBack
        {
            get { return (_mediaAttribute & MediaAttribute.SupportsCopyBack) == MediaAttribute.SupportsCopyBack; }
        }

        public bool Inserted
        {
            get
            {
                return _inserted;
            }
            protected set
            {
                if (this is RemovableBlockStorageDevice)
                {
                    _inserted = value;
                }
                else
                {
                    throw new Exception("You can only set the Inserted property on a RemovableBlockStorageDevice");
                }
            }
        }

        public virtual void Initialize()
        {
        }

        public virtual void Uninitialize()
        {
        }

        internal void Read(uint address, uint length, IntPtr sectorBuff)
        {
            Region r = FindRegionFromAddress(address);

            if (address + length > r.AddressEnd)
            {
                throw new Exception("ReadSector crosses the region boundary");
            }

            byte[] buffer = new byte[length];

            Read(address, length, buffer);

            Marshal.Copy(buffer, 0, sectorBuff, (int)length);
        }

        public abstract void Read(uint address, uint count, byte[] buffer);

        internal void Write(uint address, uint length, IntPtr sectorBuff, bool readModifyWrite)
        {
            if (this.WriteProtected)
            {
                throw new Exception(ComponentId + ": Attempt to write on a write protected block storage.");
            }

            Region r = FindRegionFromAddress(address);

            if (address + length > r.AddressEnd)
            {
                throw new Exception("WriteSector crosses the region boundary");
            }

            byte[] buffer = new byte[length];

            Marshal.Copy(sectorBuff, buffer, 0, (int)length);

            Write(address, length, buffer, readModifyWrite);
        }

        public abstract void Write(uint address, uint count, byte[] buffer, bool readModifyWrite);

        public abstract void Memset(uint address, byte data, uint count);

        public abstract void GetSectorMetadata(uint address, ref BsSectorMetadata sectorMetadata);

        public abstract void SetSectorMetadata(uint address, ref BsSectorMetadata sectorMetadata);
            

        public abstract bool IsBlockErased(uint address, uint blockLength);

        public abstract void EraseBlock(uint address);

        public virtual void SetPowerState(uint state)
        {
            _powerState = state;
        }

        public uint MaxSectorWrite_uSec()
        {
            return _maxSectorWrite_uSec;
        }

        public uint MaxBlockErase_uSec()
        {
            return _maxBlockErase_uSec;
        }
        protected Region FindRegionFromAddress(uint address)
        {
            return FindFromAddress(address, typeof(Region)) as Region;
        }

        private Object FindFromAddress(uint address, Type type)
        {
            int numRegions = _regions.Length;

            for (int i = 0; i < numRegions; i++)
            {
                Region curRegion = _regions[i];
                if (address >= curRegion.Address && address < curRegion.AddressEnd)
                {
                    if (type == typeof(Region))
                    {
                        return curRegion;
                    }
                }
            }

            return null;
        }

        protected virtual void PopulateAddresses()
        {
            int numRegions = _regions.Length;
            _baseAddress = BlockStorageDriver.GetNextBaseAddress();
            uint Address = _baseAddress;

            for (int i = 0; i < numRegions; i++)
            {
                Region curRegion = _regions[i];

                curRegion.AddressInternal = Address;

                Address += (uint)(curRegion.BytesPerBlock * curRegion.TotalBlockCount);
            }
        }

        public override void SetupComponent()
        {
            if (Inserted)
            {
                Debug.Assert(_regions != null);

                if (_regions.Length == 0)
                {
                    throw new Exception(this.ComponentId + ": There must be at least one region in a block storage device.");
                }

                foreach (Region r in _regions)
                {
                    r.ConfigurationDone();
                    r.ValidateRegion();
                }

                PopulateAddresses();
            }
            else
            {
                Debug.Assert(_regions != null);

                if (_regions.Length > 0)
                {
                    throw new Exception(this.ComponentId + ": Invalid region info for a non-inserted block storage device.");
                }
            }

            base.SetupComponent();
        }

        public override bool IsReplaceableBy(EmulatorComponent ec)
        {
            return (ec is BlockStorageDevice) && (ec.ComponentId == this.ComponentId);
        }
    }

    public class EmulatorBlockStorageDevice : BlockStorageDevice
    {
        protected EmulatorBlockStorageMemory _memory;
        protected EmulatorBlockStorageMemory _sectorMetaData;

        private String _persistanceFilename = String.Empty;

        public String PersistanceFilename
        {
            get { return _persistanceFilename; }
            set
            {
                ThrowIfNotConfigurable();

                if (String.IsNullOrEmpty(value))
                {
                    _persistanceFilename = String.Empty;
                }
                else
                {
                    try
                    {
                        FileInfo fi = new FileInfo(value);
                    }
                    catch
                    {
                        throw new Exception(ComponentId + ": Invalid filename for persistance storage: " + value);
                    }

                    _persistanceFilename = value;
                }
            }
        }

        public override void SetupComponent()
        {
            base.SetupComponent();

            uint sectorSize;

            unsafe
            {
                sectorSize = (uint)sizeof( BsSectorMetadata );
            }

            uint numSectors = Regions[0].TotalBlockCount * Regions[0].BytesPerBlock / BytesPerSector;

            _memory = EmulatorBlockStorageMemory.CreateInstance(_persistanceFilename, Regions[0].BytesPerBlock / BytesPerSector, BytesPerSector, (uint)Regions[0].TotalBlockCount, (uint)0, this.SupportsXIP);
            _sectorMetaData = EmulatorBlockStorageMemory.CreateInstance( _persistanceFilename + ".smd", 1, sectorSize, numSectors, (uint)0, false );
        }

        public override void Read(uint address, uint count, byte[] buffer)
        {
            _memory.Read(address - _baseAddress, count, buffer);
        }

        public override void Write(uint address, uint count, byte[] buffer, bool readModifyWrite)
        {
            _memory.Write(address - _baseAddress, count, buffer, readModifyWrite);
        }

        public override void Memset(uint address, byte data, uint count)
        {
            _memory.Memset(address - _baseAddress, data, count);
        }

        public override void GetSectorMetadata(uint address, ref BsSectorMetadata sectorMetadata)
        {
            uint addr = _sectorMetaData.BytesPerSector * (( address - _baseAddress ) / BytesPerSector);

            byte[] data = new byte[_sectorMetaData.BytesPerSector];

/*
            // fake bad blocks
            if(addr == ( (52+2 ) * 64 * _sectorMetaData.BytesPerSector ) || addr == ((52+17) * 64 * _sectorMetaData.BytesPerSector ) ||
               addr == ( (52+18) * 64 * _sectorMetaData.BytesPerSector ) || addr == ((52+23) * 64 * _sectorMetaData.BytesPerSector ) ||
               addr == ( (52+24) * 64 * _sectorMetaData.BytesPerSector ) || addr == ((52+25) * 64 * _sectorMetaData.BytesPerSector ) ||
               addr == ( (52+27) * 64 * _sectorMetaData.BytesPerSector ) || addr == ((52+30) * 64 * _sectorMetaData.BytesPerSector )  )
            {
                sectorMetadata.badBlock = 0;
                sectorMetadata.ECC0 = 0;
                sectorMetadata.ECC1 = 0;
                sectorMetadata.oemReserved = 0;
                sectorMetadata.reserved1 = 0;
                sectorMetadata.reserved2 = 0;

                return;
            }
*/

            _sectorMetaData.Read(addr, _sectorMetaData.BytesPerSector, data);

            GCHandle pinnedMD = GCHandle.Alloc( data, GCHandleType.Pinned );
            sectorMetadata = (BsSectorMetadata)Marshal.PtrToStructure(
                pinnedMD.AddrOfPinnedObject(),
                typeof( BsSectorMetadata ) );
            pinnedMD.Free();
        }

        public override void SetSectorMetadata(uint address, ref BsSectorMetadata sectorMetadata)
        {
            uint addr = _sectorMetaData.BytesPerSector * ( ( address - _baseAddress ) / BytesPerSector );

            byte[] data = new byte[_sectorMetaData.BytesPerSector];

            GCHandle pinnedMD = GCHandle.Alloc( data, GCHandleType.Pinned );
            Marshal.StructureToPtr(
                sectorMetadata,
                pinnedMD.AddrOfPinnedObject(), 
                true);
            pinnedMD.Free();

            _sectorMetaData.Write( addr, (uint)data.Length, data, true );
        }

        public override bool IsBlockErased(uint address, uint blockLength)
        {
            return _memory.IsBlockErased(address - _baseAddress, blockLength);
        }

        public override void EraseBlock(uint address)
        {
            Region r = FindRegionFromAddress(address);
            
            _memory.EraseBlock(address - _baseAddress, r.BytesPerBlock);

            // erase the sector metadata
            uint addr = _sectorMetaData.BytesPerSector * ( ( address - _baseAddress ) / BytesPerSector );
            uint len  = _sectorMetaData.BytesPerSector * _memory.SectorsPerBlock;

            _sectorMetaData.EraseBlock( addr, len );
            
            Thread.Sleep((int)(MaxBlockEraseTime / 1000));
        }

        public override void UninitializeComponent()
        {
            // This will force persistance to memory, if we gave it a filename earlier
            _memory.Dispose();
            _sectorMetaData.Dispose();

            base.UninitializeComponent();
        }
    }

    public abstract class RemovableBlockStorageDevice : BlockStorageDevice
    {
        String _namespace;
        uint _deviceFlags;

        public RemovableBlockStorageDevice()
        {
            Inserted = false;
            MediaAttribute = MediaAttribute.Removable;
        }

        public override void SetupComponent()
        {
            if (this.Removable == false)
            {
                throw new Exception("RemovableBlockStorageDevice must have the Removable MediaAttribute");
            }

            if (String.IsNullOrEmpty(_namespace) || _namespace.Length > FS.FSDriver.FsNameDefaultLength)
            {
                throw new Exception("RemovableBlockStorageDevice must have a valid namespace");
            }

            base.SetupComponent();
        }

        public String Namespace
        {
            get { return _namespace; }
            set
            {
                ThrowIfNotConfigurable();
                _namespace = value;
            }
        }

        public uint DeviceFlags
        {
            get { return _deviceFlags; }
            set
            {
                ThrowIfNotConfigurable();
                _deviceFlags = value;
            }
        }

        public virtual uint SerialNumber
        {
            get { return 0; }
        }

        protected virtual void Insert(Region[] config, bool triggerInsert)
        {
            if (Inserted) throw new Exception("It's already inserted.");

            try
            {
                Regions = config;
                
                Inserted = true;

                foreach (Region r in Regions)
                {
                    r.ValidateRegion();
                }

                PopulateAddresses();

                if (triggerInsert)
                {
                    Emulator.BlockStorageDevices.InsertRemovableBlockStorage(this);
                }
            }
            catch
            {
                Inserted = false;
            }

        }

        public virtual void Eject()
        {
            Inserted = false;

            Regions = null;

            Emulator.BlockStorageDevices.EjectRemovableBlockStorage(this);
        }
    }

    public class EmulatorRemovableBlockStorageDevice : RemovableBlockStorageDevice
    {
        public class InsertOnInit
        {
            public String Filename { get; set; }
            public uint SectorsPerBlock { get; set; }
            public uint BytesPerSector { get; set; }
            public uint NumBlocks { get; set; }
            public uint SerialNumber { get; set; }
        }

        protected EmulatorBlockStorageMemory _memory = null;
        protected uint _serialNumber = 0;
        protected InsertOnInit _insertOnInit = null;

        public override uint SerialNumber
        {
            get
            {
                return _serialNumber;
            }
        }

        public InsertOnInit InsertOnInitialize
        {
            get
            {
                return _insertOnInit;
            }
            set
            {
                ThrowIfNotConfigurable();
                _insertOnInit = value;
            }
        }

        public override void SetupComponent()
        {
            base.SetupComponent();

            if (_insertOnInit != null)
            {
                Insert(_insertOnInit.Filename, _insertOnInit.SectorsPerBlock, _insertOnInit.BytesPerSector, _insertOnInit.NumBlocks, _insertOnInit.SerialNumber, false);
            }
        }

        public virtual void Insert(String filename, uint sectorsPerBlock, uint bytesPerSector, uint numBlocks, uint serialNumber)
        {
            Insert(filename, sectorsPerBlock, bytesPerSector, numBlocks, serialNumber, true);
        }

        private void Insert(String filename, uint sectorsPerBlock, uint bytesPerSector, uint numBlocks, uint serialNumber, bool triggerInsert)
        {
            if (_memory != null)
            {
                throw new InvalidOperationException("There's an media inserted already.");
            }

            try
            {
                uint size = sectorsPerBlock * bytesPerSector * numBlocks;
                _memory = EmulatorBlockStorageMemory.CreateInstance(filename, sectorsPerBlock, bytesPerSector, numBlocks, serialNumber, this.SupportsXIP);

                Region[] regions = new Region[1];
                BlockRange[] blocks = new BlockRange[1];

                regions[0] = new Region(sectorsPerBlock*bytesPerSector, blocks);

                blocks[0] = new BlockRange(regions[0], BlockType.Usage_FileSystem, 0, numBlocks);

                _serialNumber = serialNumber;

                Insert(regions, triggerInsert);
            }
            catch
            {
                if (_memory != null)
                {
                    _memory.Dispose();
                    _memory = null;
                }
            }
        }

        public virtual void Insert(String filename)
        {
            Insert(filename, true);
        }

        private void Insert(String filename, bool triggerInsert)
        {
            if (_memory != null)
            {
                throw new InvalidOperationException("There's an media inserted already.");
            }

            try
            {
                _memory = EmulatorBlockStorageMemory.CreateInstance(filename, this.SupportsXIP);

                Region[] regions = new Region[1];
                BlockRange[] blocks = new BlockRange[1];

                regions[0] = new Region(_memory.SectorsPerBlock * _memory.BytesPerSector, blocks);

                blocks[0] = new BlockRange(regions[0], BlockType.Usage_FileSystem, 0, _memory.NumBlocks);

                _serialNumber = _memory.SerialNumber;

                Insert(regions, triggerInsert);
            }
            catch
            {
                if (_memory != null)
                {
                    _memory.Dispose();
                    _memory = null;
                }
            }
        }

        public override void Eject()
        {
            _memory.Dispose();
            _memory = null;

            _serialNumber = 0;

            base.Eject();
        }

        public override void Read(uint address, uint count, byte[] buffer)
        {
            ThrowIfNotInserted();
            _memory.Read(address - _baseAddress, count, buffer);
        }

        public override void Write(uint address, uint count, byte[] buffer, bool readModifyWrite)
        {
            ThrowIfNotInserted();
            _memory.Write(address - _baseAddress, count, buffer, readModifyWrite);
        }

        public override void Memset(uint address, byte data, uint count)
        {
            ThrowIfNotInserted();
            _memory.Memset(address - _baseAddress, data, count);
        }

        public override void GetSectorMetadata(uint address, ref BsSectorMetadata sectorMetadata)
        {
            // Do we want to test wear leveling in the emulator?
        }

        public override void SetSectorMetadata(uint address, ref BsSectorMetadata sectorMetadata)
        {
            // Do we want to test wear leveling in the emulator?
        }

        public override bool IsBlockErased(uint address, uint blockLength)
        {
            ThrowIfNotInserted();
            return _memory.IsBlockErased(address - _baseAddress, blockLength);
        }

        public override void EraseBlock(uint address)
        {
            ThrowIfNotInserted();

            Region r = FindRegionFromAddress(address);

            _memory.EraseBlock(address - _baseAddress, r.BytesPerBlock);

            Thread.Sleep((int)(MaxBlockEraseTime / 1000));
        }

        public override void UninitializeComponent()
        {
            // This will force persistance to memory, if we gave it a filename earlier
            if (Inserted)
            {
                Debug.Assert(_memory != null);

                _memory.Dispose();
                _memory = null;
            }

            base.UninitializeComponent();
        }

        private void ThrowIfNotInserted()
        {
            if (!Inserted || _memory == null)
            {
                throw new InvalidOperationException("There's no media in this removable block storage device.");
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EmulatorBlockStorageMetadata
    {
        public uint SectorsPerBlock;
        public uint BytesPerSector;
        public uint NumBlocks;
        public uint SerialNumber;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EmulatorBlockStorageFileHeader
    {
        public const ushort c_Magic = 0xB10C; /// Block (storage). 
        public ushort Magic;
        public ushort Version; /// 0.
        public uint MetadataSize;                           
    }

    public abstract class EmulatorBlockStorageMemory : IDisposable
    {
        protected bool _disposed = false;
        protected EmulatorBlockStorageFileHeader _header;
        protected EmulatorBlockStorageMetadata _metadata;

        public static EmulatorBlockStorageMemory CreateInstance(String filename, bool supportsXIP)
        {
            if (string.IsNullOrEmpty(filename) || supportsXIP)
            {
                return new EmulatorBlockStorageMemoryRam(filename);
            }
            else
            {
                return new EmulatorBlockStorageMemoryFile(filename);
            }
        }

        public static EmulatorBlockStorageMemory CreateInstance(String filename, uint sectorsPerBlock, uint bytesPerSector, uint numBlocks, uint serialNumber, bool supportsXIP)
        {
            if (string.IsNullOrEmpty(filename) || supportsXIP)
            {
                return new EmulatorBlockStorageMemoryRam(filename, sectorsPerBlock, bytesPerSector, numBlocks, serialNumber);
            }
            else
            {
                return new EmulatorBlockStorageMemoryFile(filename, sectorsPerBlock, bytesPerSector, numBlocks, serialNumber);
            }
        }

        public abstract void Read(uint address, uint count, byte[] buffer);
        
        public abstract void Write(uint address, uint count, byte[] buffer, bool readModifyWrite);

        public abstract void Memset(uint address, byte data, uint count);

        public abstract void GetSectorMetadata(uint address, ref BsSectorMetadata sectorMetadata);

        public abstract void SetSectorMetadata(uint address, ref BsSectorMetadata sectorMetadata);


        public abstract bool IsBlockErased(uint address, uint blockLength);

        public abstract void EraseBlock(uint address, uint blockLength);

        public EmulatorBlockStorageFileHeader Header
        {
            get
            {
                return _header;
            }
        }

        public uint Size
        {
            get
            {
                return SectorsPerBlock * BytesPerSector * NumBlocks;
            }
        }

        public uint SectorsPerBlock
        {
            get
            {
                return _metadata.SectorsPerBlock;
            }
        }

        public uint BytesPerSector
        {
            get
            {
                return _metadata.BytesPerSector;
            }
        }

        public uint NumBlocks
        {
            get
            {
                return _metadata.NumBlocks;
            }
        }

        public uint SerialNumber
        {
            get
            {
                return _metadata.SerialNumber;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _disposed = true;
        }

        protected object ToStruct(byte[] buffer, Type type)
        {
            int size = Marshal.SizeOf(type);
            if (size != buffer.Length)
                return null;

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(buffer, 0, ptr, size);

            object obj = Marshal.PtrToStructure(ptr, type);
            Marshal.FreeHGlobal(ptr);

            return obj;
        }

        protected byte[] ToByteArray(object obj)
        {
            int size = Marshal.SizeOf(obj);
            byte[] buffer = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, buffer, 0, size);
            Marshal.FreeHGlobal(ptr);

            return buffer;
        }

        protected virtual void ReadHeader(FileStream fs)
        {
            fs.Seek(0, SeekOrigin.Begin);
            EmulatorBlockStorageFileHeader header = (EmulatorBlockStorageFileHeader)ReadObjectFromFile(fs, typeof(EmulatorBlockStorageFileHeader));
            if (header.Magic != EmulatorBlockStorageFileHeader.c_Magic)
                throw new FormatException();    

            if (header.Version != 0)
                throw new FormatException();    
        
            if (header.MetadataSize != Marshal.SizeOf(typeof(EmulatorBlockStorageMetadata)))
                throw new FormatException();    

            _header = header;
        }

        protected virtual void ReadMetadata(FileStream fs)
        {
            fs.Seek(Marshal.SizeOf(typeof(EmulatorBlockStorageFileHeader)), SeekOrigin.Begin);
            EmulatorBlockStorageMetadata metadata = (EmulatorBlockStorageMetadata)ReadObjectFromFile(fs, typeof(EmulatorBlockStorageMetadata));
            _metadata = metadata;
        }

        protected virtual void ReadBlockStorageInfo(FileStream fs)
        {
            ReadHeader(fs);
            ReadMetadata(fs);
        }

        protected object ReadObjectFromFile(FileStream fs, Type type)
        {
            int size = Marshal.SizeOf(type);
            byte[] buffer = new byte[size];
            fs.Read(buffer, 0, buffer.Length);

            return ToStruct(buffer, type);
        }

        protected virtual void WriteHeader(FileStream fs)
        {
            _header.Magic = EmulatorBlockStorageFileHeader.c_Magic;
            _header.Version = 0;
            _header.MetadataSize = (uint)Marshal.SizeOf(typeof(EmulatorBlockStorageMetadata));

            fs.Seek(0, SeekOrigin.Begin);
            WriteObjectToFile(fs, _header);
        }

        protected virtual void WriteMetadata(FileStream fs)
        {
            fs.Seek(Marshal.SizeOf(typeof(EmulatorBlockStorageFileHeader)), SeekOrigin.Begin);
            WriteObjectToFile(fs, _metadata);
        }

        protected virtual void WriteBlockStorageInfo(FileStream fs)
        {
            WriteHeader(fs);
            WriteMetadata(fs);
        }

        protected void WriteObjectToFile(FileStream fs, object obj)
        {
            byte[] buffer = ToByteArray(obj);
            fs.Write(buffer, 0, buffer.Length);
        }

        internal uint FileSize
        {
            get
            {
                return (FileInfoSize + Size);
            }
        }

        internal uint FileInfoSize
        {
            get
            {
                return (uint)(Marshal.SizeOf(_header) + Marshal.SizeOf(_metadata));
            }
        }

        ~EmulatorBlockStorageMemory()
        {
            Dispose(false);
        }
    }

    public class EmulatorBlockStorageMemoryRam : EmulatorBlockStorageMemory
    {
        protected GCHandle _handle;
        protected byte[] _memory;
        protected String _filename;

        private EmulatorBlockStorageMemoryRam() { }

        internal protected EmulatorBlockStorageMemoryRam(String filename, uint sectorsPerBlock, uint bytesPerSector, uint numBlocks, uint serialNumber)
        {
            _filename = filename;
            bool eraseMemory = true;

            if (File.Exists(filename))
            {
                // if load from file failed, we need to erase the memory
                if (LoadFromFile(filename) &&
                    _metadata.SectorsPerBlock == sectorsPerBlock &&
                    _metadata.BytesPerSector == bytesPerSector &&
                    _metadata.NumBlocks == numBlocks &&
                    Size == _memory.Length)
                {
                    eraseMemory = false;                    
                }
                else
                {
                    eraseMemory = true;
                }
            }

            if (eraseMemory)
            {
                _metadata.BytesPerSector = bytesPerSector;
                _metadata.NumBlocks = numBlocks;
                _metadata.SectorsPerBlock = sectorsPerBlock;
                _metadata.SerialNumber = serialNumber;

                _memory = new byte[Size];
                _handle = GCHandle.Alloc(_memory, GCHandleType.Pinned);

                for (int i = 0; i < Size; i++)
                {
                    _memory[i] = 0xff;
                }
            }
        }


        internal protected EmulatorBlockStorageMemoryRam(String filename)
        {
            _filename = filename;
            if (!File.Exists(filename))
            {
                throw new ArgumentException(String.Format("{0} does not exist.", filename));
            }

            if (!LoadFromFile(filename))
            {
                throw new ArgumentException(String.Format("{0} is not a valid block storage.", filename));
            }
        }

        public override void Read(uint address, uint count, byte[] buffer)
        {
            Array.Copy(_memory, address, buffer, 0, buffer.Length);
        }

        public override void Write(uint address, uint count, byte[] buffer, bool readModifyWrite)
        {
            Array.Copy(buffer, 0, _memory, address, buffer.Length);
        }

        public override void Memset(uint address, byte data, uint count)
        {
            while(0 != count)
            {
                count--;
                _memory[count + address] = data;
            }
        }

        public override void GetSectorMetadata(uint address, ref BsSectorMetadata sectorMetadata)
        {
            // Do we want to test wear leveling in the emulator?
        }

        public override void SetSectorMetadata(uint address, ref BsSectorMetadata sectorMetadata)
        {
            // Do we want to test wear leveling in the emulator?
        }


        public override bool IsBlockErased(uint address, uint blockLength)
        {
            uint end = address + blockLength;

            for (uint i = address; i < end; i++)
            {
                if (_memory[i] != 0xff)
                {
                    return false;
                }
            }

            return true;
        }

        public override void EraseBlock(uint address, uint blockLength)
        {
            uint end = blockLength + address;
            
            for (uint i = address; i < end; i++)
            {
                _memory[i] = 0xff;
            }
        }

        protected virtual bool LoadFromFile(String filename)
        {
            try
            {
                // Try to open the disk file where the flash data is stored.  If it's not found, do nothing.
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    ReadBlockStorageInfo(fs);

                    _memory = new byte[Size];
                    _handle = GCHandle.Alloc(_memory, GCHandleType.Pinned);

                    // Make sure that the saved data length matches the size of the emulator's flash memory
                    int readSize = fs.Read(_memory, 0, _memory.Length);
                    if (readSize != Size)
                        throw new FormatException();
                }
                return true;
            }
            catch (FileNotFoundException)
            {
                // File not found, do nothing (flash is empty).
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        protected virtual void SaveToFile(String filename)
        {
            if (String.IsNullOrEmpty(filename) == false)
            {
                using (FileStream fs = new FileStream(filename, FileMode.Create))
                {
                    WriteBlockStorageInfo(fs);
                    fs.Write(_memory, 0, _memory.Length);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    if (!String.IsNullOrEmpty(_filename))
                    {
                        SaveToFile(_filename);
                    }

                    _handle.Free();
                    _memory = null;
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

    }

    public class EmulatorBlockStorageMemoryFile : EmulatorBlockStorageMemory
    {
        protected FileStream _memory;
        private static object s_lock = new object();

        static EmulatorBlockStorageMemoryFile()
        {
            if(s_eraseBlock == null)
            {
                lock(s_lock)
                {
                    if(s_eraseBlock == null)
                    {
                        s_eraseBlock = new byte[0x20000];

                        for(int i = 0; i < s_eraseBlock.Length; i++)
                        {
                            s_eraseBlock[i] = 0xFF;
                        }
                    }
                }
            }

            if(s_readBlock == null)
            {
                lock(s_lock)
                {
                    if(s_readBlock == null)
                    {
                        s_readBlock = new byte[0x20000];
                    }
                }
            }
        }

        private EmulatorBlockStorageMemoryFile() 
        {
        }

        internal protected EmulatorBlockStorageMemoryFile(String filename, uint sectorsPerBlock, uint bytesPerSector, uint numBlocks, uint serialNumber)
        {
            bool fileAlreadyExisted = File.Exists(filename);
            bool metadataLoaded = false;

            if (String.IsNullOrEmpty(filename))
            {
                String tempFilename = Path.GetTempFileName();
                _memory = File.Open(tempFilename, FileMode.Create, FileAccess.ReadWrite, FileShare.Delete);
                File.Delete(tempFilename);
            }
            else
            {
                _memory = File.Open(filename, FileMode.OpenOrCreate);
            }

             if (fileAlreadyExisted)
             {
                 //File exists, but it may not be a real dat file or may be not in
                 //the right format. We will make an attempt to load it.

                 try
                 {
                     ReadBlockStorageInfo(_memory);
                     if (_metadata.BytesPerSector == bytesPerSector &&
                         _metadata.NumBlocks == numBlocks &&
                         _metadata.SectorsPerBlock == sectorsPerBlock &&
                         FileSize == _memory.Length)
                     {
                         metadataLoaded = true;
                     }
                 }
                 catch (Exception)
                 {
                 }
             }

            if (!metadataLoaded)
            {
                /// Either file did not existed or was not in right format,
                /// accept callers settings as our metadata.
                _metadata.BytesPerSector = bytesPerSector;
                _metadata.NumBlocks = numBlocks;
                _metadata.SectorsPerBlock = sectorsPerBlock;
                _metadata.SerialNumber = serialNumber;

                _memory.SetLength(FileSize);
                WriteBlockStorageInfo(_memory);
                int size = (int)Size;
                /// Effectively erase everything.
                for (int i = 0; i < size; i++)
                {
                    _memory.WriteByte(0xff);
                }
            }
        }

        internal protected EmulatorBlockStorageMemoryFile(String filename)
        {
            _memory = null;

            if (!File.Exists(filename))
                throw new ArgumentException(String.Format("{0} file does not exist.", filename));
            
            _memory = File.Open(filename, FileMode.Open);
            ReadBlockStorageInfo(_memory);                
        }

        public override void Read(uint address, uint count, byte[] buffer)
        {
            _memory.Seek(FileInfoSize + address, SeekOrigin.Begin);
            _memory.Read(buffer, 0, (int)count);
        }

        public override void Write(uint address, uint count, byte[] buffer, bool readModifyWrite)
        {
            _memory.Seek(FileInfoSize + address, SeekOrigin.Begin);
            _memory.Write(buffer, 0, (int)count);
        }

        public override void Memset(uint address, byte data, uint count)
        {
            _memory.Seek(FileInfoSize + address, SeekOrigin.Begin);

            byte []arr = new byte[count];

            for(int i=0; i<count; i++)
            {
                arr[i] = data;
            }

            _memory.Write(arr, 0, (int)count);
        }

        public override void GetSectorMetadata(uint address, ref BsSectorMetadata sectorMetadata)
        {
            // Do we want to test wear leveling in the emulator?
        }

        public override void SetSectorMetadata(uint address, ref BsSectorMetadata sectorMetadata)
        {
            // Do we want to test wear leveling in the emulator?
        }


        public override bool IsBlockErased(uint address, uint blockLength)
        {
            _memory.Seek(FileInfoSize + address, SeekOrigin.Begin);

            uint len = blockLength > s_readBlock.Length ? (uint)s_readBlock.Length : blockLength;
            
            while(blockLength > 0)
            {
                if(_memory.Read(s_readBlock, 0, (int)len) != len)
                {
                    return false;
                }

                for(int i = 0; i < len; i++)
                {
                    if(s_readBlock[i] != 0xFF)
                    {
                        return false;
                    }
                }

                blockLength -= len;

                if(blockLength < len)
                {
                    len = blockLength;
                }
            }

            return true;
        }

        static byte[] s_eraseBlock = null;
        static byte[] s_readBlock = null;

        public override void EraseBlock(uint address, uint blockLength)
        {
            _memory.Seek(FileInfoSize + address, SeekOrigin.Begin);

            uint len = blockLength > s_eraseBlock.Length ? (uint)s_eraseBlock.Length : blockLength; 

            while(blockLength > 0)
            {
                _memory.Write(s_eraseBlock, 0, (int)len);

                blockLength -= len;

                if(blockLength < len)
                {
                    len = blockLength;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    _memory.Close();
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }

}
