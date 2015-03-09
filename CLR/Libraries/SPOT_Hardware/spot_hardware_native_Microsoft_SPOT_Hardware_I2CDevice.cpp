////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware.h"

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_I2CDevice::Initialize___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    
    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_I2CXAction::CreateInstance( *pThis, pThis[ FIELD__m_xAction ] ));

    I2C_Initialize();
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_I2CDevice::Execute___I4__SZARRAY_MicrosoftSPOTHardwareI2CDeviceI2CTransaction__I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array*      xActionRefs;
    CLR_RT_HeapBlock_Array*      data; 
    CLR_RT_HeapBlock*            xActionRef;
    I2C_USER_CONFIGURATION       config;
    CLR_INT64*                   timeout;
    size_t                       transactedBytes;
    CLR_UINT32                   unit;
    CLR_UINT32                   numUnits;    
    bool                         fRes;

    CLR_RT_HeapBlock_I2CXAction* xAction = NULL;
    
    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);

    // check if object has been disposed
    if(pThis[ FIELD__m_disposed ].NumericByRef().s1 != 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    xActionRefs  = stack.Arg1().DereferenceArray();  FAULT_ON_NULL(xActionRefs);

    numUnits = xActionRefs->m_numOfElements; 

    if(numUnits == 0)
    {        
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }
        
    TINYCLR_CHECK_HRESULT(stack.SetupTimeout( stack.Arg2(), timeout ));

    if(stack.m_customState == 1)
    {
        TINYCLR_CHECK_HRESULT(Library_spot_hardware_native_Microsoft_SPOT_Hardware_I2CDevice__Configuration::GetInitialConfig( pThis[ FIELD__Config ], config ));
        
        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_I2CXAction::ExtractInstance( pThis[ FIELD__m_xAction ], xAction ));

        if(xAction->IsPending())
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);
        }
        
        TINYCLR_CHECK_HRESULT(xAction->AllocateXAction( numUnits ));

        TINYCLR_CHECK_HRESULT(xAction->PrepareXAction( config, numUnits ));

        for(unit = 0; unit < numUnits; ++unit)
        {
            CLR_RT_TypeDescriptor desc;
            bool                  fRead;
            CLR_UINT8*            src;
            CLR_UINT32            length;
                        
            xActionRef = (CLR_RT_HeapBlock*)xActionRefs->GetElement( unit );  FAULT_ON_NULL(xActionRef);
            xActionRef = xActionRef->Dereference();                           FAULT_ON_NULL(xActionRef); 
            
            TINYCLR_CHECK_HRESULT(desc.InitializeFromObject( *xActionRef ));

            if     (desc.m_handlerCls.m_data == g_CLR_RT_WellKnownTypes.m_I2CDevice__I2CWriteTransaction.m_data) fRead = false;
            else if(desc.m_handlerCls.m_data == g_CLR_RT_WellKnownTypes.m_I2CDevice__I2CReadTransaction.m_data ) fRead = true;
            else   
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
            }

            data = xActionRef[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_I2CDevice__I2CTransaction::FIELD__Buffer ].DereferenceArray();  FAULT_ON_NULL(data);
            
            src    = data->GetFirstElement(); 
            length = data->m_numOfElements;

            if(length == 0)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
            }
            
            TINYCLR_CHECK_HRESULT(xAction->PrepareXActionUnit( src, length, unit, fRead ));
        }   
        
        TINYCLR_CHECK_HRESULT(xAction->Enqueue());            

        stack.m_customState = 2;
    }
    else
    {
        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_I2CXAction::ExtractInstance( pThis[ FIELD__m_xAction ], xAction ));        
    }

    transactedBytes = 0;
    fRes            = true;

    while(fRes)
    {   
        if(xAction->IsTerminated()) break;

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.WaitEvents( stack.m_owningThread, *timeout, CLR_RT_ExecutionEngine::c_Event_I2C, fRes ));     
    }

    stack.PopValue();  //Timeout

    if(fRes)
    {
        transactedBytes = xAction->TransactedBytes();

        if(xAction->IsCompleted())
        {
            for(unit = 0; unit < numUnits; ++unit)
            {
                if(xAction->IsReadXActionUnit( unit ))
                {
                    xActionRef = (CLR_RT_HeapBlock*)xActionRefs->GetElement( unit );  FAULT_ON_NULL(xActionRef);
                    xActionRef = xActionRef->Dereference();                           FAULT_ON_NULL(xActionRef); 
                    
                    data = xActionRef[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_I2CDevice__I2CTransaction::FIELD__Buffer ].DereferenceArray();    FAULT_ON_NULL(data);

                    xAction->CopyBuffer( data->GetFirstElement(), data->m_numOfElements, unit );
                }
            }
        }
    }
    else
    {
        xAction->Cancel( false );
    }

    stack.SetResult_I4( (CLR_INT32)transactedBytes );
    
    TINYCLR_CLEANUP();

    if(hr != CLR_E_THREAD_WAITING && xAction)
    {
        xAction->ReleaseBuffers();
    }
    
    TINYCLR_CLEANUP_END();
}
