////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "i2c.h"

///////////////////////////////////////////////////////////////////////////////

#undef  TRACE_ALWAYS
#define TRACE_ALWAYS 0x00000001

#undef  DEBUG_TRACE
#define DEBUG_TRACE (TRACE_ALWAYS)

///////////////////////////////////////////////////////////////////////////////

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_I2C_Driver"
#endif

I2C_Driver g_I2C_Driver;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

///////////////////////////////////////////////////////////////////////////////

BOOL I2C_Initialize()
{
    NATIVE_PROFILE_PAL_COM();
    return I2C_Driver::Initialize();
}

BOOL I2C_Uninitialize()
{
    NATIVE_PROFILE_PAL_COM();
    return I2C_Driver::Uninitialize();
}

BOOL I2C_Enqueue( I2C_HAL_XACTION* xAction )
{
    NATIVE_PROFILE_PAL_COM();
    return I2C_Driver::Enqueue( xAction );
}

void I2C_Cancel( I2C_HAL_XACTION* xAction, bool signal )
{
    NATIVE_PROFILE_PAL_COM();
    I2C_Driver::Cancel( xAction, signal );
}

void I2C_InitializeTransaction( I2C_HAL_XACTION* xAction, I2C_USER_CONFIGURATION& config, I2C_HAL_XACTION_UNIT** xActions, size_t numXActions )
{
    NATIVE_PROFILE_PAL_COM();
    xAction->Initialize( config, xActions, numXActions );
}

void I2C_InitializeTransactionUnit( I2C_HAL_XACTION_UNIT* xActionUnit, I2C_WORD* src, I2C_WORD* dst, size_t size, BOOL fRead )
{
    NATIVE_PROFILE_PAL_COM();
    xActionUnit->Initialize( src, dst, size, fRead );
}

//--//

void I2C_XAction_SetState( I2C_HAL_XACTION* xAction, UINT8 state )
{
    NATIVE_PROFILE_PAL_COM();
    xAction->SetState( state );
}

UINT8 I2C_XAction_GetState( I2C_HAL_XACTION* xAction )
{
    NATIVE_PROFILE_PAL_COM();
    return xAction->GetState();
}

BOOL I2C_XAction_CheckState( I2C_HAL_XACTION* xAction, UINT8 state )
{
    NATIVE_PROFILE_PAL_COM();
    return xAction->CheckState( state );
}

size_t I2C_XAction_TransactedBytes( I2C_HAL_XACTION* xAction )
{
    NATIVE_PROFILE_PAL_COM();
    return xAction->TransactedBytes();
}

void I2C_XActionUnit_CopyBuffer( I2C_HAL_XACTION_UNIT* xActionUnit, I2C_WORD* data, size_t length )
{
    NATIVE_PROFILE_PAL_COM();
    xActionUnit->CopyBuffer( data, length );
}

BOOL I2C_XActionUnit_IsRead( I2C_HAL_XACTION_UNIT* xActionUnit )
{
    NATIVE_PROFILE_PAL_COM();
    return xActionUnit->IsReadXActionUnit();
}

///////////////////////////////////////////////////////////////////////////////

void I2C_HAL_XACTION_UNIT::Initialize( I2C_WORD* src, I2C_WORD* dst, size_t size, BOOL fRead )
{
    NATIVE_PROFILE_PAL_COM();
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

void I2C_HAL_XACTION::Initialize( I2C_USER_CONFIGURATION& config, I2C_HAL_XACTION_UNIT** xActionUnits, size_t numXActionUnits )
{
    NATIVE_PROFILE_PAL_COM();
    m_xActionUnits    = xActionUnits;
    m_numXActionUnits = numXActionUnits;
    m_current         = 0;
    m_address         = config.Address;        
    m_status          = c_Status_Idle;
    I2C_Internal_GetClockRate( config.ClockRate, m_clockRate, m_clockRate2 );        
}

void I2C_HAL_XACTION::Signal( UINT8 state, BOOL signal )
{
    NATIVE_PROFILE_PAL_COM();
    ASSERT_IRQ_MUST_BE_OFF();
    
    m_status = state;
    
    // this will automatically remove this node from the I2C_Driver 
    // list and place it in the global continuation list 
    if(signal) Enqueue();

    // stop the flow if the transaction was not cancelled
    // in whihc case the Stop command would have been issued already 
    if(!(m_status & c_Status_Cancelled))
    {
        I2C_Internal_XActionStop();
    }
}
    
///////////////////////////////////////////////////////////////////////////////

BOOL I2C_Driver::Initialize()
{
    NATIVE_PROFILE_PAL_COM();
    GLOBAL_LOCK(irq);

    // initialize the PAL driver only once
    if(!g_I2C_Driver.m_initialized)
    {
        g_I2C_Driver.m_xActions.Initialize();
        
        g_I2C_Driver.m_initialized = TRUE;
    }

    // give always a chance to the HAL driver to initialize
    // this will resurrect the GPIO pins in case some other 
    // entity has been using them
    if(!I2C_Internal_Initialize()) return FALSE;

    return TRUE;
}

BOOL I2C_Driver::Uninitialize()
{
    NATIVE_PROFILE_PAL_COM();
    GLOBAL_LOCK(irq);

    if(!g_I2C_Driver.m_initialized)
    {
        return FALSE;
    }
    else
    {
        I2C_Internal_Uninitialize();

        g_I2C_Driver.m_initialized = FALSE;

        while(g_I2C_Driver.m_xActions.ExtractFirstNode());

        return TRUE;
    }
}

BOOL I2C_Driver::Enqueue( I2C_HAL_XACTION* xAction )
{
    NATIVE_PROFILE_PAL_COM();
    ASSERT(xAction);
 
    if(xAction == NULL) return FALSE;
       
    GLOBAL_LOCK(irq);

    xAction->SetCallback( I2C_Driver::CompletedCallback );

    xAction->SetState( I2C_HAL_XACTION::c_Status_Scheduled );

    g_I2C_Driver.m_xActions.LinkAtBack( xAction );

    StartNext();

    return TRUE;
}

void I2C_Driver::Cancel( I2C_HAL_XACTION* xAction, bool signal )
{
    NATIVE_PROFILE_PAL_COM();
    ASSERT(xAction);
    
    if(xAction == NULL) return;
    
    GLOBAL_LOCK(irq);
   
    switch(xAction->GetState())
    {
        // only one xAction will efer be in processing for every call to Abort
        case I2C_HAL_XACTION::c_Status_Processing:
            
            I2C_Internal_XActionStop();

            // fall through...

        case I2C_HAL_XACTION::c_Status_Scheduled:
        case I2C_HAL_XACTION::c_Status_Completed:
        case I2C_HAL_XACTION::c_Status_Aborted:

            xAction->Abort();

            xAction->SetState(I2C_HAL_XACTION::c_Status_Cancelled);

            StartNext();
            
            break;

        case I2C_HAL_XACTION::c_Status_Idle: // do nothing since we aren't enqueued yet
            break;
    }
}

void I2C_Driver::CompletedCallback( void* arg )
{
    NATIVE_PROFILE_PAL_COM();
    I2C_HAL_XACTION* xAction = (I2C_HAL_XACTION*)arg;

    // is the transaction is the last of a series of related
    // transactions then free the bus for whoever comes next
    if((
         xAction->m_current == (xAction->m_numXActionUnits))                                        || 
         xAction->CheckState( I2C_HAL_XACTION::c_Status_Aborted | I2C_HAL_XACTION::c_Status_Cancelled 
      ))
    {        
        // signal the waiting thread; since continuations are executed one at a time for every round 
        // of the scheduler there is no need to check for lost events on the waiting thread
        Events_Set( SYSTEM_EVENT_I2C_XACTION );
        
        StartNext();    
    }
}

void I2C_Driver::StartNext()
{
    NATIVE_PROFILE_PAL_COM();
    I2C_HAL_XACTION* xAction = (I2C_HAL_XACTION*)g_I2C_Driver.m_xActions.FirstValidNode();

    if(xAction == NULL)
    {
        return;       
    }

    xAction->SetState( I2C_HAL_XACTION::c_Status_Processing );

    I2C_Internal_XActionStart( xAction, false );
}

