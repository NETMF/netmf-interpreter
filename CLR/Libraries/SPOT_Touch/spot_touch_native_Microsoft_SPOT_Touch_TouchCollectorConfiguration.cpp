////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "spot_touch_native.h"
#include "spot_touch.h"
#include "spot_graphics_native.h"

HRESULT Library_spot_touch_native_Microsoft_SPOT_Touch_TouchCollectorConfiguration::EnableTouchCollection___STATIC__VOID__I4__I4__I4__I4__I4__MicrosoftSPOTGraphicsMicrosoftSPOTBitmap( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    TINYCLR_NOCLEANUP_NOLABEL();   
}

HRESULT Library_spot_touch_native_Microsoft_SPOT_Touch_TouchCollectorConfiguration::GetTouchPoints___STATIC__VOID__BYREF_I4__SZARRAY_I2__SZARRAY_I2( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    CLR_RT_HeapBlock hbCount;
    CLR_RT_HeapBlock_Array* sx;
    CLR_RT_HeapBlock_Array* sy;
    int count = 0;
    CLR_RT_HeapBlock* pArgs = NULL;
    CLR_RT_HeapBlock* pArgs0      = &(stack.Arg0());
    TINYCLR_CHECK_HRESULT(hbCount.LoadFromReference( pArgs0[ 0 ] ));
    count = hbCount.NumericByRef().s4;

    pArgs = &stack.Arg1();

    sx = pArgs[ 0 ].DereferenceArray();  FAULT_ON_NULL(sx);        
    sy = pArgs[ 1 ].DereferenceArray();  FAULT_ON_NULL(sy);

    TINYCLR_CHECK_HRESULT(TOUCH_PANEL_GetTouchPoints( 
                                                     &count, 
                                                     (CLR_INT16*)sx->GetFirstElement(), 
                                                     (CLR_INT16*)sy->GetFirstElement()
                                                    ));

    hbCount.SetInteger( count ); 
    
    TINYCLR_SET_AND_LEAVE(hbCount.StoreToReference( stack.Arg0(), 0 ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_touch_native_Microsoft_SPOT_Touch_TouchCollectorConfiguration::GetTouchInput___STATIC__VOID__MicrosoftSPOTTouchTouchCollectorConfigurationTouchInput__BYREF_I4__BYREF_I4__BYREF_I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    CLR_RT_HeapBlock hbParam1;
    CLR_RT_HeapBlock hbParam2;
    CLR_RT_HeapBlock hbParam3;
    UINT32 flags = 0;
    INT32 param1 = 0;
    INT32 param2 = 0;
    INT32 param3 = 0;

    CLR_RT_HeapBlock* pArgs0 = &(stack.Arg0());
    TINYCLR_CHECK_HRESULT(hbParam1.LoadFromReference( pArgs0[ 1 ] ));
    TINYCLR_CHECK_HRESULT(hbParam2.LoadFromReference( pArgs0[ 2 ] ));
    TINYCLR_CHECK_HRESULT(hbParam3.LoadFromReference( pArgs0[ 3 ] ));
    
    flags = stack.Arg0().NumericByRef().s4;
    param1 = hbParam1.NumericByRef().s4;    
    param2 = hbParam2.NumericByRef().s4;    
    param3 = hbParam3.NumericByRef().s4;    

    flags &= ~TouchInfo_Set;
    
    TINYCLR_CHECK_HRESULT(TOUCH_PANEL_GetSetTouchInfo( 
                                                     flags,
                                                     &param1,
                                                     &param2,
                                                     &param3
                                                    ));

    hbParam1.SetInteger( param1 ); 
    hbParam2.SetInteger( param2 ); 
    hbParam3.SetInteger( param3 ); 

    TINYCLR_CHECK_HRESULT(hbParam1.StoreToReference( stack.Arg1(), 0 ));
    TINYCLR_CHECK_HRESULT(hbParam2.StoreToReference( stack.Arg2(), 0 ));
    TINYCLR_CHECK_HRESULT(hbParam3.StoreToReference( stack.Arg3(), 0 ));

    TINYCLR_NOCLEANUP();
}


HRESULT Library_spot_touch_native_Microsoft_SPOT_Touch_TouchCollectorConfiguration::SetTouchInput___STATIC__VOID__MicrosoftSPOTTouchTouchCollectorConfigurationTouchInput__I4__I4__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    CLR_RT_HeapBlock hbParam1;
    CLR_RT_HeapBlock hbParam2;
    CLR_RT_HeapBlock hbParam3;
    UINT32 flags = 0;
    INT32 param1 = 0;
    INT32 param2 = 0;
    INT32 param3 = 0;

    CLR_RT_HeapBlock* pArgs0      = &(stack.Arg0());
    TINYCLR_CHECK_HRESULT(hbParam1.LoadFromReference( pArgs0[ 1 ] ));
    TINYCLR_CHECK_HRESULT(hbParam2.LoadFromReference( pArgs0[ 2 ] ));
    TINYCLR_CHECK_HRESULT(hbParam3.LoadFromReference( pArgs0[ 3 ] ));
    
    flags = stack.Arg0().NumericByRef().s4;
    param1 = hbParam1.NumericByRef().s4;    
    param2 = hbParam2.NumericByRef().s4;    
    param3 = hbParam3.NumericByRef().s4;    

    flags |= TouchInfo_Set;
    
    TINYCLR_CHECK_HRESULT(TOUCH_PANEL_GetSetTouchInfo( 
                                                     flags,
                                                     &param1,
                                                     &param2,
                                                     &param3
                                                    ));

    TINYCLR_NOCLEANUP();
}




