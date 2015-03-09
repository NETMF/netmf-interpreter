////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.SPOT.Emulator.Memory
{
    internal class MemoryDriver : HalDriver<IMemoryDriver>, IMemoryDriver
    {
        private RamManager RamManager
        {
            [DebuggerHidden]
            get { return this.Emulator.RamManager; }
        }

        #region IMemoryDriver Members

        void IMemoryDriver.HeapLocation(out IntPtr BaseAddress, out uint SizeInBytes)
        {
            RamManager ram = this.RamManager;

            BaseAddress = ram.BaseAddress;
            SizeInBytes = ram.Size / 2;
        }

        void IMemoryDriver.CustomHeapLocation(out IntPtr BaseAddress, out uint SizeInBytes)
        {
            RamManager ram = this.RamManager;

            BaseAddress = (IntPtr)((long)ram.BaseAddress + ram.Size / 2);
            SizeInBytes = ram.Size / 2;
        }

        #endregion
    }

    public class RamManager : EmulatorComponent
    {                
        protected uint _size;
        protected GCHandle _handle;
        protected byte[] _memory;
        protected IntPtr _baseAddress;

        public RamManager()
        {
            _size = 1024 * 1024;
        }

        internal IntPtr BaseAddress
        {
            get { return _baseAddress; }
        }

        public uint Size
        {
            get { return _size; }
            set
            { 
                ThrowIfNotConfigurable();
                _size = value;
            }
        }

        public override void SetupComponent()
        {
            _memory = new byte[_size];
            _handle = GCHandle.Alloc(_memory, GCHandleType.Pinned);
            _baseAddress = Marshal.UnsafeAddrOfPinnedArrayElement(_memory, 0);
        }

        public byte[] Memory
        {
            get { return _memory; }
        }

        public override void UninitializeComponent()
        {
            base.UninitializeComponent();

            _memory = null;
            _handle.Free();
        }

        public override bool IsReplaceableBy(EmulatorComponent ec)
        {
            return (ec is RamManager);
        }
    }
}
