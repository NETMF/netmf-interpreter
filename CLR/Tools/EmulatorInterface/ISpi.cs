////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace Microsoft.SPOT.Emulator.Spi
{    
    [StructLayout(LayoutKind.Sequential)]
    public struct SpiConfiguration
    {
        public uint DeviceCS;
        public bool CS_Active;             // False = LOW active,      TRUE = HIGH active
        public bool MSK_IDLE;              // False = LOW during idle, TRUE = HIGH during idle
        public bool MSK_SampleEdge;        // False = sample falling edge,  TRUE = samples on rising
        public bool MD_16bits;
        public uint Clock_RateKHz;
        public uint CS_Setup_uSecs;
        public uint CS_Hold_uSecs;       
        public uint SPI_mod;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SpiXaction
    {
        public IntPtr WritePtr;
        public int WriteCount;
        public IntPtr ReadPtr;
        public int ReadCount;
        public int ReadStartOffset;

        public SpiXaction(IntPtr writePtr, int writeCount, IntPtr readPtr, int readCount, int readStartOffset)
        {
            this.WritePtr = writePtr;
            this.WriteCount = writeCount;
            this.ReadPtr = readPtr;
            this.ReadCount = readCount;
            this.ReadStartOffset = readStartOffset;
        }
    }

    public interface ISpiDriver
    {
        bool Initialize      ();
        void Uninitialize    ();
        bool nWrite16_nRead16(ref SpiConfiguration Configuration, IntPtr Write16, int WriteCount, IntPtr Read16, int ReadCount, int ReadStartOffset);
        bool nWrite8_nRead8(ref SpiConfiguration Configuration, IntPtr Write8, int WriteCount, IntPtr Read8, int ReadCount, int ReadStartOffset);
        bool Xaction_Start(ref SpiConfiguration Configuration);
        bool Xaction_Stop(ref SpiConfiguration Configuration);
        bool Xaction_nWrite16_nRead16(ref SpiXaction Transaction);
        bool Xaction_nWrite8_nRead8(ref SpiXaction Transaction);
        uint GetPortsCount();
        void GetPins( uint spi_mod, out uint msk, out uint  miso, out uint mosi );

    }
}
