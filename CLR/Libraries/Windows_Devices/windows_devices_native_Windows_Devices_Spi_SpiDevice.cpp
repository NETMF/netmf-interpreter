////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "windows_devices.h"

typedef Library_windows_devices_native_Windows_Devices_Spi_SpiConnectionSettings SpiConnectionSettings;

// NOTE: Must match enum in SpiEnums.cs
enum SpiMode
{
    SpiMode_Mode0 = 0,
    SpiMode_Mode1,
    SpiMode_Mode2,
    SpiMode_Mode3,
};

HRESULT Library_windows_devices_native_Windows_Devices_Spi_SpiDevice::InitNative___VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); hr = S_OK;

    GPIO_PIN chipSelectPin = GPIO_PIN_NONE;
    GPIO_PIN mskPin = GPIO_PIN_NONE;
    GPIO_PIN misoPin = GPIO_PIN_NONE;
    GPIO_PIN mosiPin = GPIO_PIN_NONE;
    bool chipSelectReserved = false;
    bool mskPinReserved = false;
    bool misoPinReserved = false;
    bool mosiPinReserved = false;
    CLR_RT_HeapBlock* settings;

    CLR_RT_HeapBlock* pThis = stack.This(); FAULT_ON_NULL(pThis);

    if (pThis[ FIELD__m_disposed ].NumericByRef().u1)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    settings = pThis[ FIELD__m_settings ].Dereference(); FAULT_ON_NULL(settings);
    chipSelectPin = settings[ SpiConnectionSettings::FIELD__m_chipSelectionLine ].NumericByRef().u4;

    // Get the pin IDs for this device's chip select line. Then reserve all pins which have assigned IDs.
    CPU_SPI_GetPins( pThis[ FIELD__m_spiBus ].NumericByRef().u4, mskPin, misoPin, mosiPin );

    if (chipSelectPin != GPIO_PIN_NONE)
    {
        chipSelectReserved = !!CPU_GPIO_ReservePin( chipSelectPin, TRUE );
        if (!chipSelectReserved)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_PIN_UNAVAILABLE);
        }
    }

    if (mskPin != GPIO_PIN_NONE)
    {
        mskPinReserved = !!CPU_GPIO_ReservePin( mskPin, TRUE );
        if (!mskPinReserved)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_PIN_UNAVAILABLE);
        }
    }

    if (misoPin != GPIO_PIN_NONE)
    {
        misoPinReserved = !!CPU_GPIO_ReservePin( misoPin, TRUE );
        if (!misoPinReserved)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_PIN_UNAVAILABLE);
        }
    }

    if (mosiPin != GPIO_PIN_NONE)
    {
        mosiPinReserved = !!CPU_GPIO_ReservePin( mosiPin, TRUE );
        if (!mosiPinReserved)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_PIN_UNAVAILABLE);
        }
    }

    pThis[ FIELD__m_mskPin ].NumericByRef().u4 = mskPin;
    pThis[ FIELD__m_misoPin ].NumericByRef().u4 = misoPin;
    pThis[ FIELD__m_mosiPin ].NumericByRef().u4 = mosiPin;

    if (FAILED(hr))
    {
        if (chipSelectReserved)
        {
            CPU_GPIO_ReservePin( chipSelectPin, FALSE );
        }

        if (mskPinReserved)
        {
            CPU_GPIO_ReservePin( mskPin, FALSE );
        }

        if (misoPinReserved)
        {
            CPU_GPIO_ReservePin( misoPin, FALSE );
        }

        if (mosiPinReserved)
        {
            CPU_GPIO_ReservePin( mosiPin, FALSE );
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_windows_devices_native_Windows_Devices_Spi_SpiDevice::DisposeNative___VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis = stack.This(); FAULT_ON_NULL(pThis);

    if (!pThis[ FIELD__m_disposed ].NumericByRef().u1)
    {
        CLR_RT_HeapBlock* settings = pThis[ FIELD__m_settings ].Dereference();
        if (settings != NULL)
        {
            GPIO_PIN chipSelectPin = settings[ SpiConnectionSettings::FIELD__m_chipSelectionLine ].NumericByRef().u4;
            if (chipSelectPin != GPIO_PIN_NONE)
            {
                CPU_GPIO_ReservePin( chipSelectPin, FALSE );
            }
        }

        GPIO_PIN mskPin = pThis[ FIELD__m_mskPin ].NumericByRef().u4 ;
        if (mskPin != GPIO_PIN_NONE)
        {
            CPU_GPIO_ReservePin( mskPin, FALSE );
        }

        GPIO_PIN misoPin = pThis[ FIELD__m_misoPin ].NumericByRef().u4;
        if (misoPin != GPIO_PIN_NONE)
        {
            CPU_GPIO_ReservePin( misoPin, FALSE );
        }

        GPIO_PIN mosiPin = pThis[ FIELD__m_mosiPin ].NumericByRef().u4;
        if (mosiPin != GPIO_PIN_NONE)
        {
            CPU_GPIO_ReservePin( mosiPin, FALSE );
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_windows_devices_native_Windows_Devices_Spi_SpiDevice::TransferInternal___VOID__SZARRAY_U1__SZARRAY_U1__BOOLEAN( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    BYTE* writeData = NULL;
    int writeLength = 0;
    BYTE* readData = NULL;
    int readLength = 0;
    CLR_RT_HeapBlock* pThis = stack.This(); FAULT_ON_NULL(pThis);

    if (pThis[ FIELD__m_disposed ].NumericByRef().u1)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    {
        CLR_RT_HeapBlock_Array* writeBuffer = stack.Arg1().DereferenceArray();
        if (writeBuffer != NULL)
        {
            writeData = writeBuffer->GetFirstElement();
            writeLength = writeBuffer->m_numOfElements;
        }

        CLR_RT_HeapBlock_Array* readBuffer = stack.Arg2().DereferenceArray();
        if (readBuffer != NULL)
        {
            readData = readBuffer->GetFirstElement();
            readLength = readBuffer->m_numOfElements;
        }

        CLR_RT_HeapBlock* settings = pThis[ FIELD__m_settings ].Dereference();
        if (settings == NULL)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);
        }

        SPI_CONFIGURATION config = {0};

        // Always disable the busy pin.
        config.BusyPin.Pin = GPIO_PIN_NONE;
        config.BusyPin.ActiveState = FALSE;

        config.SPI_mod = pThis[ FIELD__m_spiBus ].NumericByRef().u4;
        config.DeviceCS = settings[ SpiConnectionSettings::FIELD__m_chipSelectionLine ].NumericByRef().u4;
        config.Clock_RateKHz = settings[ SpiConnectionSettings::FIELD__m_clockFrequency ].NumericByRef().s4 / 1000;

        SpiMode mode = static_cast<SpiMode>(settings[ SpiConnectionSettings::FIELD__m_mode ].NumericByRef().s4);
        switch (mode)
        {
        case SpiMode_Mode0:
            config.MSK_IDLE = FALSE;
            config.MSK_SampleEdge = TRUE;
            break;

        case SpiMode_Mode1:
            config.MSK_IDLE = FALSE;
            config.MSK_SampleEdge = FALSE;
            break;

        case SpiMode_Mode2:
            config.MSK_IDLE = TRUE;
            config.MSK_SampleEdge = FALSE;
            break;

        case SpiMode_Mode3:
            config.MSK_IDLE = TRUE;
            config.MSK_SampleEdge = TRUE;
            break;
        }

        int dataBitLength = settings[ SpiConnectionSettings::FIELD__m_dataBitLength ].NumericByRef().s4;
        if (dataBitLength > 8)
        {
            config.MD_16bits = TRUE;
        }

        // We call init before each transaction to match the behavior in the SPOT APIs.
        CPU_SPI_Initialize();

        // If full duplex, read start offset should be 0, otherwise start at the end of the "write" operation.
        int readStartOffset = stack.Arg3().NumericByRef().u1 ? 0 : writeLength;
        BOOL result = FALSE;

        if (config.MD_16bits)
        {
            result = CPU_SPI_nWrite16_nRead16( config, (CLR_UINT16*)writeData, writeLength / 2, (CLR_UINT16*)readData, readLength / 2, readStartOffset / 2 );
        }
        else
        {
            result = CPU_SPI_nWrite8_nRead8( config, writeData, writeLength, readData, readLength, readStartOffset );
        }

        if (!result)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);
        }
    }

    TINYCLR_NOCLEANUP();
}
