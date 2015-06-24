////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_SPI_DECL_H_
#define _DRIVERS_SPI_DECL_H_ 1

#define SPI_CTRL_DEFAULT 0

struct SPI_CONFIGURATION
{
    GPIO_PIN       DeviceCS;
    BOOL           CS_Active;             // False = LOW active,      TRUE = HIGH active
    BOOL           MSK_IDLE;              // False = LOW during idle, TRUE = HIGH during idle
    BOOL           MSK_SampleEdge;        // False = sample falling edge,  TRUE = samples on rising
    BOOL           MD_16bits;
    UINT32         Clock_RateKHz;
    UINT32         CS_Setup_uSecs;
    UINT32         CS_Hold_uSecs;
    UINT32         SPI_mod;
    GPIO_FLAG      BusyPin;
};

struct SPI_XACTION_16
{
    UINT16* Write16;
    INT32   WriteCount;
    UINT16* Read16;
    INT32   ReadCount;
    INT32   ReadStartOffset;
    UINT32  SPI_mod;
    GPIO_FLAG BusyPin;
};

struct SPI_XACTION_8
{
    UINT8* Write8;
    INT32  WriteCount;
    UINT8* Read8;
    INT32  ReadCount;
    INT32  ReadStartOffset;
    UINT32 SPI_mod;
    GPIO_FLAG BusyPin;
};

BOOL   CPU_SPI_Initialize      ();
void   CPU_SPI_Uninitialize    ();
BOOL   CPU_SPI_nWrite16_nRead16( const SPI_CONFIGURATION& Configuration, UINT16* Write16, INT32 WriteCount, UINT16* Read16, INT32 ReadCount, INT32 ReadStartOffset );
BOOL   CPU_SPI_nWrite8_nRead8  ( const SPI_CONFIGURATION& Configuration, UINT8* Write8, INT32 WriteCount, UINT8* Read8, INT32 ReadCount, INT32 ReadStartOffset );
BOOL   CPU_SPI_Xaction_Start   ( const SPI_CONFIGURATION& Configuration );
BOOL   CPU_SPI_Xaction_Stop    ( const SPI_CONFIGURATION& Configuration );
BOOL   CPU_SPI_Xaction_nWrite16_nRead16( SPI_XACTION_16& Transaction );
BOOL   CPU_SPI_Xaction_nWrite8_nRead8  ( SPI_XACTION_8& Transaction  );
UINT32 CPU_SPI_PortsCount      ();
void   CPU_SPI_GetPins         ( UINT32 spi_mod, GPIO_PIN& msk, GPIO_PIN& miso, GPIO_PIN& mosi );

UINT32 CPU_SPI_MinClockFrequency( UINT32 spi_mod );
UINT32 CPU_SPI_MaxClockFrequency( UINT32 spi_mod );
UINT32 CPU_SPI_ChipSelectLineCount( UINT32 spi_mod );

#endif // _DRIVERS_SPI_DECL_H_

