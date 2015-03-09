////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "stdafx.h"

using namespace Microsoft::SPOT::Emulator;
using namespace Microsoft::SPOT::Emulator::I2c;
using namespace System::Diagnostics;

I2cXAction^ ToManagedXAction( I2C_HAL_XACTION* xAction )
{
    int numXActionUnits = (int)xAction->m_numXActionUnits;
    array<I2cXActionUnit^>^ xActionUnits = gcnew array<I2cXActionUnit^>(numXActionUnits);

    for(int i = 0; i < numXActionUnits; i++)
    {
        I2C_HAL_XACTION_UNIT* xActionUnit = xAction->m_xActionUnits[i];
        int dataLen = (int)xActionUnit->m_bytesToTransfer;
        array<unsigned char>^ data = gcnew array<unsigned char>(dataLen);
        bool isRead = BOOL_TO_INT( xActionUnit->m_fRead );

        if(!isRead)
        {
            for(int j = 0; j < dataLen; j++)
            {
                data[j] = *(xActionUnit->m_dataQueue.Pop());
            }
        }

        I2cXActionUnit^ managedXActionUnit = gcnew I2cXActionUnit( isRead, data );

        xActionUnits[i] = managedXActionUnit;
    }

    return gcnew I2cXAction( xActionUnits, xAction->m_address, xAction->m_clockRate, (I2cStatus)xAction->m_status );
}

void UpdateNativeXAction( I2C_HAL_XACTION* xAction, I2cXAction^ managedXAction )
{
    xAction->m_status = (UINT8)managedXAction->Status;

    if(managedXAction->Status == I2cStatus::Completed)
    {
        int numXActionUnits = (int)xAction->m_numXActionUnits;
        for(int i = 0; i < numXActionUnits; i++)
        {
            I2C_HAL_XACTION_UNIT* xActionUnit = xAction->m_xActionUnits[i];
            I2cXActionUnit^ managedXActionUnit = managedXAction->XActionUnits[i];

            ASSERT( managedXActionUnit->IsRead == (bool)(BOOL_TO_INT( xActionUnit->m_fRead )) );
            ASSERT( managedXActionUnit->Data->Length == (int)(xActionUnit->m_bytesTransferred + xActionUnit->m_bytesToTransfer) );

            if(managedXActionUnit->IsRead)
            {
                I2C_WORD* slot;
                int j = 0;

                while((slot = xActionUnit->m_dataQueue.Push()) != NULL)
                {
                    *slot = managedXActionUnit->Data[j++];
                }
            }

            xActionUnit->m_bytesTransferred = xActionUnit->m_bytesToTransfer;
            xActionUnit->m_bytesToTransfer = 0;
        }
    }
}

//--//

BOOL I2C_Initialize()
{
    return EmulatorNative::GetII2cDriver()->Initialize();
}

BOOL I2C_Uninitialize()
{
    return EmulatorNative::GetII2cDriver()->Uninitialize();
}

BOOL I2C_Enqueue( I2C_HAL_XACTION* xAction )
{
    I2cXAction^ managedXAction = ToManagedXAction( xAction );

    bool result = EmulatorNative::GetII2cDriver()->Enqueue( managedXAction );

    UpdateNativeXAction( xAction, managedXAction );

    return result;
}

void I2C_Cancel( I2C_HAL_XACTION* xAction, bool signal )
{
    return EmulatorNative::GetII2cDriver()->Cancel( (IntPtr)(void*)xAction, signal );
}

void I2C_Internal_GetPins( GPIO_PIN& scl, GPIO_PIN& sda )
{
    EmulatorNative::GetII2cDriver()->GetPins( scl, sda );
    return;
}

//--//

void I2C_InitializeTransaction( I2C_HAL_XACTION* xAction, I2C_USER_CONFIGURATION& config, I2C_HAL_XACTION_UNIT** xActions, size_t numXActions )
{
    xAction->Initialize( config, xActions, numXActions );
}

void I2C_InitializeTransactionUnit( I2C_HAL_XACTION_UNIT* xActionUnit, I2C_WORD* src, I2C_WORD* dst, size_t size, BOOL fRead )
{
    xActionUnit->Initialize( src, dst, size, fRead );
}

//--//

void I2C_XAction_SetState( I2C_HAL_XACTION* xAction, UINT8 state )
{
    xAction->SetState( state );
}

UINT8 I2C_XAction_GetState( I2C_HAL_XACTION* xAction )
{
    return xAction->GetState();
}

BOOL I2C_XAction_CheckState( I2C_HAL_XACTION* xAction, UINT8 state )
{
    return xAction->CheckState( state );
}

size_t I2C_XAction_TransactedBytes( I2C_HAL_XACTION* xAction )
{
    return xAction->TransactedBytes();
}

void I2C_XActionUnit_CopyBuffer( I2C_HAL_XACTION_UNIT* xActionUnit, I2C_WORD* data, size_t length )
{
    xActionUnit->CopyBuffer( data, length );
}

BOOL I2C_XActionUnit_IsRead( I2C_HAL_XACTION_UNIT* xActionUnit )
{
    return xActionUnit->IsReadXActionUnit();
}

///////////////////////////////////////////////////////////////////////////////

// Taken from the PAL implmentation (DeviceCode\pal\com\i2c.cpp)
void I2C_HAL_XACTION_UNIT::Initialize( I2C_WORD* src, I2C_WORD* dst, size_t size, BOOL fRead )
{
    m_bytesToTransfer  = size;
    m_bytesTransferred = 0;
    m_fRead            = fRead;

    m_dataQueue.Initialize( dst, size );

    if(!fRead)
    {
        I2C_WORD* slot;
        
        while((slot = m_dataQueue.Push()) != NULL)
        {
            *slot = *src++;
        }
    }
}

///////////////////////////////////////////////////////////////////////////////

// Taken from the PAL implmentation (DeviceCode\pal\com\i2c.cpp)
void I2C_HAL_XACTION::Initialize( I2C_USER_CONFIGURATION& config, I2C_HAL_XACTION_UNIT** xActionUnits, size_t numXActionUnits )
{
    m_xActionUnits    = xActionUnits;
    m_numXActionUnits = numXActionUnits;
    m_current         = 0;
    //m_clockRate       = I2C_Internal_GetClockRate( config.ClockRate );     // Don't care about clockrate
    m_address         = (UINT8)config.Address;
    m_status          = c_Status_Idle;
}

void I2C_HAL_XACTION::Signal( UINT8 state, BOOL signal )
{
}
