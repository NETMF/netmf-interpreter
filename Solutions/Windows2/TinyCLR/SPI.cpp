////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

using namespace Microsoft::SPOT::Emulator;

BOOL CPU_SPI_Initialize()
{
    return EmulatorNative::GetISpiDriver()->Initialize();    
}

void CPU_SPI_Uninitialize()
{
    EmulatorNative::GetISpiDriver()->Uninitialize();
}

BOOL CPU_SPI_nWrite16_nRead16( const SPI_CONFIGURATION& Configuration, UINT16* Write16, INT32 WriteCount, UINT16* Read16, INT32 ReadCount, INT32 ReadStartOffset )
{    
    return EmulatorNative::GetISpiDriver()->nWrite16_nRead16( (Spi::SpiConfiguration %)Configuration, (System::IntPtr)Write16, WriteCount, (System::IntPtr)Read16, ReadCount, ReadStartOffset );
}

BOOL CPU_SPI_nWrite8_nRead8( const SPI_CONFIGURATION& Configuration, UINT8* Write8, INT32 WriteCount, UINT8* Read8, INT32 ReadCount, INT32 ReadStartOffset )
{
    return EmulatorNative::GetISpiDriver()->nWrite8_nRead8( (Spi::SpiConfiguration %)Configuration, (System::IntPtr)Write8, WriteCount, (System::IntPtr)Read8, ReadCount, ReadStartOffset );
}

BOOL CPU_SPI_Xaction_Start( const SPI_CONFIGURATION& Configuration )
{
    return EmulatorNative::GetISpiDriver()->Xaction_Start( (Spi::SpiConfiguration %)Configuration );
}

BOOL CPU_SPI_Xaction_Stop( const SPI_CONFIGURATION& Configuration )
{
    return EmulatorNative::GetISpiDriver()->Xaction_Stop( (Spi::SpiConfiguration %)Configuration );
}

BOOL CPU_SPI_Xaction_nWrite16_nRead16( SPI_XACTION_16& Transaction )
{
    return EmulatorNative::GetISpiDriver()->Xaction_nWrite16_nRead16( (Spi::SpiXaction %)Transaction );
}

BOOL CPU_SPI_Xaction_nWrite8_nRead8( SPI_XACTION_8& Transaction )
{
    return EmulatorNative::GetISpiDriver()->Xaction_nWrite8_nRead8( (Spi::SpiXaction %)Transaction );
}

UINT32 CPU_SPI_PortsCount()
{
    return EmulatorNative::GetISpiDriver()->GetPortsCount();
}

void   CPU_SPI_GetPins( UINT32 spi_mod, GPIO_PIN& msk, GPIO_PIN& miso, GPIO_PIN& mosi )
{
    return EmulatorNative::GetISpiDriver()->GetPins( spi_mod, msk, miso, mosi );
}

