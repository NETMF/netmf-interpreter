////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "SPOT_Hardware.h"


HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::EnableInterrupt___VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_NativeEventDispatcher *pNativeDisp = NULL;
    
    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);

    if(pThis[ FIELD__m_disposed ].NumericByRef().s1 != 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }
    
    TINYCLR_CHECK_HRESULT(GetEventDispatcher( stack, pNativeDisp ));

    // Calls driver to enable interrupts.  Consider that there could be no driver 
    // associated to this object so check that the driver methods are set 
    if(pNativeDisp->m_DriverMethods == NULL)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);
    }
    
    TINYCLR_CHECK_HRESULT(pNativeDisp->m_DriverMethods->m_EnableProc( pNativeDisp, true ));
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::DisableInterrupt___VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
   
    CLR_RT_HeapBlock_NativeEventDispatcher *pNativeDisp = NULL;
    
    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);
    
    if(pThis[ FIELD__m_disposed ].NumericByRef().s1 != 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OBJECT_DISPOSED);
    }

    TINYCLR_CHECK_HRESULT(GetEventDispatcher( stack, pNativeDisp ));
    
    // Calls driver to enable interrupts.  Consider that there could be no driver 
    // associated to this object so check that the driver methods are set 
    // we will be tolerant in this case and not throw any exception
    if(pNativeDisp->m_DriverMethods != NULL)
    {
        TINYCLR_CHECK_HRESULT(pNativeDisp->m_DriverMethods->m_EnableProc( pNativeDisp, false ));
    }
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::Dispose___VOID__BOOLEAN( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
        
    CLR_RT_HeapBlock_NativeEventDispatcher *pNativeDisp = NULL;
    
    CLR_RT_HeapBlock*  pThis = stack.This();  FAULT_ON_NULL(pThis);
    
    TINYCLR_CHECK_HRESULT(GetEventDispatcher( stack, pNativeDisp ));
    
    // Cleanup the HAL queue from the instance of assiciated CLR_RT_HeapBlock_NativeEventDispatcher 
    pNativeDisp->RemoveFromHALQueue();
    
    // Calls driver to enable interrupts.  Consider that there could be no driver 
    // associated to this object so check that the driver methods are set 
    // we will be tolerant in this case and not throw any exception
    if(pNativeDisp->m_DriverMethods != NULL)
    {
        TINYCLR_CHECK_HRESULT(pNativeDisp->m_DriverMethods->m_CleanupProc( pNativeDisp )); 
    }
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::_ctor___VOID__STRING__U8( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    CLR_RT_DriverInterruptMethods*          pDriverMethods;
    const CLR_RT_NativeAssemblyData*        pNativeDriverData;
    CLR_RT_HeapBlock_NativeEventDispatcher* pNativeDisp = NULL;
    
    LPCSTR                                  lpszDriverName;    
    UINT64                                  driverData;
    
    CLR_RT_HeapBlock*  pThis = stack.This();  FAULT_ON_NULL(pThis);
    
    // Retrieve paramenters; 
    if (stack.Arg1().DataType() != DATATYPE_OBJECT) 
    {   TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);                      
    }
    lpszDriverName = stack.Arg1().RecoverString();  FAULT_ON_NULL(lpszDriverName);

    driverData = stack.Arg2().NumericByRef().u8;

    // Throw NULL exception if string is empty. 
    if(hal_strlen_s( lpszDriverName ) == 0)
    {  
        TINYCLR_CHECK_HRESULT(CLR_E_ARGUMENT_NULL);
    }
    
    // Retrives pointers to driver implemented functions.
    pNativeDriverData = GetAssemblyNativeData( lpszDriverName );
    
    // Validates check sum and presence of the structure.
    if(pNativeDriverData == NULL || pNativeDriverData->m_checkSum != DRIVER_INTERRUPT_METHODS_CHECKSUM)
    {
       TINYCLR_CHECK_HRESULT(CLR_E_DRIVER_NOT_REGISTERED);
    }

    // Get pointer to CLR_RT_DriverInterruptMethods
    pDriverMethods = (CLR_RT_DriverInterruptMethods *)pNativeDriverData->m_pNativeMethods;
    // Check that all methods are present:
    if(pDriverMethods->m_InitProc == NULL || pDriverMethods->m_EnableProc == NULL || pDriverMethods->m_CleanupProc == NULL)
    {
       TINYCLR_CHECK_HRESULT(CLR_E_DRIVER_NOT_REGISTERED);
    }

    // So we found driver by name and now we create instance of CLR_RT_HeapBlock_NativeEventDispatcher
    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_NativeEventDispatcher::CreateInstance( *pThis, pThis[ FIELD__m_NativeEventDispatcher ] ));

    // Initialize the driver with and provide the instance of CLR_RT_HeapBlock_NativeEventDispatcher 
    TINYCLR_CHECK_HRESULT(GetEventDispatcher( stack, pNativeDisp ));

    // Now call the driver. First save pointer to driver data.
    pNativeDisp->m_DriverMethods = pDriverMethods;
    TINYCLR_CHECK_HRESULT(pDriverMethods->m_InitProc( pNativeDisp, driverData ));
    
    TINYCLR_NOCLEANUP();
}


//--//

CLR_RT_ObjectToEvent_Source* Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::GetEventDispReference( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    CLR_RT_HeapBlock* pThis = stack.This();

    return CLR_RT_ObjectToEvent_Source::ExtractInstance( pThis[ FIELD__m_NativeEventDispatcher ] );
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::GetEventDispatcher( CLR_RT_StackFrame& stack, CLR_RT_HeapBlock_NativeEventDispatcher*& port )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    port = GetEventDispatcher( stack );
    if(port == NULL)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    TINYCLR_NOCLEANUP();
}

CLR_RT_HeapBlock_NativeEventDispatcher* Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::GetEventDispatcher( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    CLR_RT_ObjectToEvent_Source* src = GetEventDispReference( stack );

    return (src == NULL) ? NULL : (CLR_RT_HeapBlock_NativeEventDispatcher*)src->m_eventPtr;
}

CLR_RT_HeapBlock_NativeEventDispatcher *CreateNativeEventInstance( CLR_RT_StackFrame& stack )

{   NATIVE_PROFILE_CLR_IOPORT();
    CLR_RT_HeapBlock_NativeEventDispatcher *pNativeDisp = NULL;
    CLR_RT_HeapBlock* pThis = stack.This();
    
    // Creates intstance of CLR_RT_HeapBlock_NativeEventDispatcher and saves it in pThis
    if(FAILED(CLR_RT_HeapBlock_NativeEventDispatcher::CreateInstance( *pThis, pThis[ Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::FIELD__m_NativeEventDispatcher ] )))
    {
        return NULL;
    }
    // Retrieves instance of 
    Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher::GetEventDispatcher( stack, pNativeDisp );
    return pNativeDisp;
}




