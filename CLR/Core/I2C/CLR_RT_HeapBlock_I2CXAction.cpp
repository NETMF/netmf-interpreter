////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

CLR_RT_DblLinkedList CLR_RT_HeapBlock_I2CXAction::m_i2cPorts; //FIX ME -change location

void CLR_RT_HeapBlock_I2CXAction::HandlerMethod_Initialize()
{
    NATIVE_PROFILE_CLR_I2C();
    CLR_RT_HeapBlock_I2CXAction::m_i2cPorts.DblLinkedList_Initialize();
}

void CLR_RT_HeapBlock_I2CXAction::HandlerMethod_RecoverFromGC()
{
    NATIVE_PROFILE_CLR_I2C();
    TINYCLR_FOREACH_NODE(CLR_RT_HeapBlock_I2CXAction,i2cPort,CLR_RT_HeapBlock_I2CXAction::m_i2cPorts)
    {
        i2cPort->RecoverFromGC();
    }
    TINYCLR_FOREACH_NODE_END();
}

void CLR_RT_HeapBlock_I2CXAction::HandlerMethod_CleanUp()
{
    NATIVE_PROFILE_CLR_I2C();
     CLR_RT_HeapBlock_I2CXAction* i2cPort;

        while(NULL != (i2cPort = (CLR_RT_HeapBlock_I2CXAction*)CLR_RT_HeapBlock_I2CXAction::m_i2cPorts.FirstValidNode()))
        {
            i2cPort->DetachAll();
            i2cPort->ReleaseWhenDeadEx();
        }
}

HRESULT CLR_RT_HeapBlock_I2CXAction::CreateInstance( CLR_RT_HeapBlock& owner, CLR_RT_HeapBlock& xActionRef )
{
    NATIVE_PROFILE_CLR_I2C();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_I2CXAction* xAction = NULL;

    xAction = EVENTCACHE_EXTRACT_NODE(g_CLR_RT_EventCache,CLR_RT_HeapBlock_I2CXAction,DATATYPE_I2C_XACTION); CHECK_ALLOCATION(xAction);

    {
        CLR_RT_ProtectFromGC gc( *xAction );

        xAction->Initialize();

        xAction->m_HalXAction      = NULL;
        xAction->m_HalXActionUnits = NULL;
        xAction->m_dataBuffers     = NULL;
        xAction->m_xActionUnits    = 0;

        m_i2cPorts.LinkAtBack( xAction );

        TINYCLR_CHECK_HRESULT(CLR_RT_ObjectToEvent_Source::CreateInstance( xAction, owner, xActionRef ));
    }

    

    TINYCLR_CLEANUP();

    if(FAILED(hr))
    {
        if(xAction) xAction->ReleaseWhenDeadEx();
    }

    TINYCLR_CLEANUP_END();
}

HRESULT CLR_RT_HeapBlock_I2CXAction::ExtractInstance( CLR_RT_HeapBlock& ref, CLR_RT_HeapBlock_I2CXAction*& xAction )
{
    NATIVE_PROFILE_CLR_I2C();
    TINYCLR_HEADER();

    CLR_RT_ObjectToEvent_Source* src = CLR_RT_ObjectToEvent_Source::ExtractInstance( ref ); FAULT_ON_NULL(src);

    xAction = (CLR_RT_HeapBlock_I2CXAction*)src->m_eventPtr;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_I2CXAction::AllocateXAction( CLR_UINT32 numXActionUnits )
{
    NATIVE_PROFILE_CLR_I2C();
    TINYCLR_HEADER();

    CLR_UINT32 index;

    if(IsPending())
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);
    }

    m_xActionUnits = numXActionUnits;

    // allocate memory for the transaction
    m_HalXAction = (I2C_HAL_XACTION*)CLR_RT_Memory::Allocate_And_Erase( sizeof(I2C_HAL_XACTION) );  CHECK_ALLOCATION(m_HalXAction);

    ((HAL_CONTINUATION*)m_HalXAction)->Initialize();

    ::I2C_XAction_SetState( m_HalXAction, I2C_HAL_XACTION::c_Status_Idle );

    // initialize pointers to data buffers
    m_dataBuffers = (I2C_WORD**)CLR_RT_Memory::Allocate_And_Erase( sizeof(I2C_WORD*) * numXActionUnits );  CHECK_ALLOCATION(m_dataBuffers);

    // allocate memory for the transaction units
    m_HalXActionUnits = (I2C_HAL_XACTION_UNIT**)CLR_RT_Memory::Allocate_And_Erase( sizeof(I2C_HAL_XACTION_UNIT*) * numXActionUnits );  CHECK_ALLOCATION(m_HalXActionUnits);

    for(index = 0; index < numXActionUnits; ++index)
    {            
        m_HalXActionUnits[ index ] = (I2C_HAL_XACTION_UNIT*)CLR_RT_Memory::Allocate_And_Erase( sizeof(I2C_HAL_XACTION_UNIT) );  CHECK_ALLOCATION(m_HalXActionUnits[ index ]);
    }
    
    TINYCLR_CLEANUP();

    if(FAILED(hr))
    {   
        ReleaseBuffers();
    }
    
    TINYCLR_CLEANUP_END();
}

HRESULT CLR_RT_HeapBlock_I2CXAction::PrepareXAction( I2C_USER_CONFIGURATION& config, size_t numXActions )
{
    NATIVE_PROFILE_CLR_I2C();
    TINYCLR_HEADER();

    _ASSERTE(numXActions > 0);
    
    if(IsPending())
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);
    }

    if(m_HalXAction == NULL || m_HalXActionUnits == NULL)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_NULL_REFERENCE);
    }
    
    I2C_InitializeTransaction( 
                                m_HalXAction, 
                                config, 
                                m_HalXActionUnits, 
                                numXActions     
                              );

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_I2CXAction::PrepareXActionUnit( CLR_UINT8* src, size_t length, size_t unit, bool fRead )
{
    NATIVE_PROFILE_CLR_I2C();
    TINYCLR_HEADER();

    _ASSERTE(src != NULL);    
    _ASSERTE(length > 0);
    
    if(IsPending())
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);
    }

    if(m_HalXActionUnits == NULL || m_dataBuffers == NULL)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_NULL_REFERENCE);
    }

    if(unit > m_xActionUnits-1)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }
    
    m_dataBuffers[ unit ] = (I2C_WORD*)CLR_RT_Memory::Allocate_And_Erase( length * sizeof(I2C_WORD) );  CHECK_ALLOCATION(m_dataBuffers[ unit ]);
    
    I2C_InitializeTransactionUnit( 
                                    m_HalXActionUnits[ unit ], 
                                    src, 
                                    m_dataBuffers[ unit ], 
                                    length, 
                                    fRead ? TRUE : FALSE
                                  );
    
    TINYCLR_NOCLEANUP();
}

void CLR_RT_HeapBlock_I2CXAction::CopyBuffer( CLR_UINT8* dst, size_t length, size_t unit )
{
    NATIVE_PROFILE_CLR_I2C();
    ASSERT(unit<m_xActionUnits);
        
    if(!m_HalXAction || !IsCompleted())
    {
        return;
    }

    ::I2C_XActionUnit_CopyBuffer( m_HalXActionUnits[ unit ], dst, length );
}

void CLR_RT_HeapBlock_I2CXAction::ReleaseBuffers()
{
    NATIVE_PROFILE_CLR_I2C();
    CLR_UINT32 index = 0;

    if(m_HalXAction != NULL) 
    { 
        I2C_Cancel( m_HalXAction, false );
        
        CLR_RT_Memory::Release( m_HalXAction );  m_HalXAction = NULL; 
    }
    
    if(m_dataBuffers != NULL) 
    { 
        for(index = 0; index < m_xActionUnits; ++index)
        {
            if(m_dataBuffers[ index ] != NULL)
            {
                CLR_RT_Memory::Release( m_dataBuffers[ index ] );  m_dataBuffers[ index ] = NULL;
            }
        }
        
        CLR_RT_Memory::Release( m_dataBuffers );  m_dataBuffers = NULL; 
    }

    if(m_HalXActionUnits != NULL)
    {
        for(index = 0; index < m_xActionUnits; ++index)
        {
            if(m_HalXActionUnits[ index ] != NULL)
            {
                CLR_RT_Memory::Release( m_HalXActionUnits[ index ] );  m_HalXActionUnits[ index ] = NULL;
            }
        }
        
        CLR_RT_Memory::Release( m_HalXActionUnits );  m_HalXActionUnits = NULL;
    }
}

HRESULT CLR_RT_HeapBlock_I2CXAction::Enqueue()
{
    NATIVE_PROFILE_CLR_I2C();
    TINYCLR_HEADER();
    
    if(!m_HalXAction || IsPending())
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);
    }

    if(!::I2C_Enqueue( m_HalXAction ))
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);
    }

    TINYCLR_NOCLEANUP();
}

void CLR_RT_HeapBlock_I2CXAction::Cancel( bool signal )
{
    NATIVE_PROFILE_CLR_I2C();
    if(!m_HalXAction)
    {
        return;
    }
    
    ::I2C_Cancel( m_HalXAction, true );
}

bool CLR_RT_HeapBlock_I2CXAction::IsPending()
{
    NATIVE_PROFILE_CLR_I2C();
    if(!m_HalXAction)
    {
        return false;
    }
    
    return ::I2C_XAction_CheckState( m_HalXAction, I2C_HAL_XACTION::c_Status_Scheduled | I2C_HAL_XACTION::c_Status_Processing ) == TRUE ? true : false;
}

bool CLR_RT_HeapBlock_I2CXAction::IsTerminated()
{
    NATIVE_PROFILE_CLR_I2C();
    if(!m_HalXAction)
    {
        return false;
    }
    
    return ::I2C_XAction_CheckState( m_HalXAction, I2C_HAL_XACTION::c_Status_Completed | I2C_HAL_XACTION::c_Status_Aborted ) == TRUE ? true : false;
}

bool CLR_RT_HeapBlock_I2CXAction::IsCompleted()
{
    NATIVE_PROFILE_CLR_I2C();
    if(!m_HalXAction)
    {
        return false;
    }
    
    return ::I2C_XAction_CheckState( m_HalXAction, I2C_HAL_XACTION::c_Status_Completed ) == TRUE ? true : false;
}

bool CLR_RT_HeapBlock_I2CXAction::IsReadXActionUnit( size_t unit )
{
    NATIVE_PROFILE_CLR_I2C();
    if(m_HalXActionUnits == NULL)
    {
        return false;
    }
    
    return ::I2C_XActionUnit_IsRead( m_HalXActionUnits[ unit ] ) == FALSE ? false : true;
    
}

size_t CLR_RT_HeapBlock_I2CXAction::TransactedBytes()
{
    NATIVE_PROFILE_CLR_I2C();
    if(!m_HalXAction || m_HalXAction->CheckState( I2C_HAL_XACTION::c_Status_Idle ))
    {
        return 0;
    }

    return ::I2C_XAction_TransactedBytes( m_HalXAction );
}

CLR_UINT8 CLR_RT_HeapBlock_I2CXAction::GetStatus()
{
    NATIVE_PROFILE_CLR_I2C();
    if(!m_HalXAction)
    {
        return I2C_HAL_XACTION::c_Status_Idle;
    }
    
    return ::I2C_XAction_GetState( m_HalXAction );
}

void CLR_RT_HeapBlock_I2CXAction::RecoverFromGC()
{
    NATIVE_PROFILE_CLR_I2C();
    CheckAll();

    ReleaseWhenDeadEx();
}

bool CLR_RT_HeapBlock_I2CXAction::ReleaseWhenDeadEx()
{
    NATIVE_PROFILE_CLR_I2C();
    if(!IsReadyForRelease()) return false;

    ReleaseBuffers();
    
    return ReleaseWhenDead();
}
