////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware.h"

        
HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Watchdog::get_Enabled___STATIC__BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    stack.SetResult_Boolean( ::Watchdog_GetSetEnabled( FALSE, FALSE ) != FALSE );
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Watchdog::set_Enabled___STATIC__VOID__BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    bool enable = stack.Arg0().NumericByRef().s1 != 0;
    
    stack.SetResult_Boolean( ::Watchdog_GetSetEnabled( enable ? TRUE : FALSE, TRUE ) != FALSE );
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Watchdog::get_Timeout___STATIC__mscorlibSystemTimeSpan( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_INT64 val = (CLR_INT64)::Watchdog_GetSetTimeout( 0, FALSE ) * TIME_CONVERSION__TO_MILLISECONDS;

    stack.SetResult_I8( val );
    
    stack.TopValue().ChangeDataType( DATATYPE_TIMESPAN );
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Watchdog::set_Timeout___STATIC__VOID__mscorlibSystemTimeSpan( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_INT32 val = (CLR_INT32)(CLR_INT64_TEMP_CAST) stack.Arg0().NumericByRef().s8 / TIME_CONVERSION__TO_MILLISECONDS;

    ::Watchdog_GetSetTimeout( val, TRUE );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Watchdog::get_Behavior___STATIC__MicrosoftSPOTHardwareWatchdogBehavior( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    stack.SetResult_I4( ::Watchdog_GetSetBehavior( Watchdog_Behavior__None, FALSE ) );
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Watchdog::set_Behavior___STATIC__VOID__MicrosoftSPOTHardwareWatchdogBehavior( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    Watchdog_Behavior behavior = (Watchdog_Behavior)stack.Arg0().NumericByRef().s4;
    
    stack.SetResult_I4( ::Watchdog_GetSetBehavior( behavior, TRUE ) );
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Watchdog::get_Log___STATIC__MicrosoftSPOTNativeMicrosoftSPOTILog( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Watchdog::set_Log___STATIC__VOID__MicrosoftSPOTNativeMicrosoftSPOTILog( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_Watchdog::GetLastOcurrenceDetails___STATIC__BOOLEAN__BYREF_mscorlibSystemDateTime__BYREF_mscorlibSystemTimeSpan__BYREF_mscorlibSystemReflectionMethodInfo( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    INT64  time     = 0;
    INT64  timeout  = 0; 
    UINT32 assembly = 0;
    UINT32 method   = 0;
    
    bool fRes = ::Watchdog_LastOccurence( time, timeout, assembly, method, FALSE ) != FALSE;

    if(fRes)
    {   
        CLR_RT_HeapBlock hbParam1;
        CLR_RT_HeapBlock hbParam2;
        CLR_RT_HeapBlock hbParam3;
        
        CLR_RT_MethodDef_Index methodIdx;
                
        hbParam1.SetInteger    ( time              );
        hbParam1.ChangeDataType( DATATYPE_DATETIME );
        hbParam2.SetInteger    ( timeout           );
        hbParam2.ChangeDataType( DATATYPE_TIMESPAN );

        if(assembly != 0 || method != 0)
        {
            CLR_RT_HeapBlock* hbObj;

            methodIdx.Set( assembly, method    );

            TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(hbParam3, g_CLR_RT_WellKnownTypes.m_MethodInfo));
            hbObj = hbParam3.Dereference();
            hbObj->SetReflection( methodIdx );

            TINYCLR_CHECK_HRESULT(hbParam3.StoreToReference( stack.Arg2(), 0 ));
        }

        TINYCLR_CHECK_HRESULT(hbParam1.StoreToReference( stack.Arg0(), 0 ));
        TINYCLR_CHECK_HRESULT(hbParam2.StoreToReference( stack.Arg1(), 0 ));
    }
    
    stack.SetResult_Boolean( fRes );
    
    TINYCLR_NOCLEANUP();
}

