////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "spot_touch_native.h"
#include "spot_touch.h"


HRESULT Library_spot_touch_native_Microsoft_SPOT_Touch_TouchPanel::EnableInternal___VOID__BOOLEAN( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    bool fInit = stack.Arg1().NumericByRef().s1 != 0;

    if(fInit)
    {
        TINYCLR_SET_AND_LEAVE(TOUCH_PANEL_Initialize());  
    }
    else
    {
        TINYCLR_SET_AND_LEAVE(TOUCH_PANEL_Uninitialize());         
    }
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_touch_native_Microsoft_SPOT_Touch_TouchPanel::GetCalibrationPointCount___VOID__BYREF_I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    CLR_RT_HeapBlock hbCalibration;
    TOUCH_PANEL_CALIBRATION_POINT_COUNT tcpc;
        
    TINYCLR_CHECK_HRESULT(TOUCH_PANEL_GetDeviceCaps( TOUCH_PANEL_CALIBRATION_POINT_COUNT_ID, (void *)&tcpc ));

    hbCalibration.SetInteger( tcpc.cCalibrationPoints ); TINYCLR_CHECK_HRESULT(hbCalibration.StoreToReference( stack.Arg1(), 0 ));
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_touch_native_Microsoft_SPOT_Touch_TouchPanel::StartCalibration___VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
        
    TINYCLR_SET_AND_LEAVE(TOUCH_PANEL_ResetCalibration());

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_touch_native_Microsoft_SPOT_Touch_TouchPanel::GetCalibrationPoint___VOID__I4__BYREF_I4__BYREF_I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 
    
    TOUCH_PANEL_CALIBRATION_POINT tcp;
    CLR_RT_HeapBlock              hbX;
    CLR_RT_HeapBlock              hbY;

    tcp.PointNumber    = stack.Arg1().NumericByRef().s4;
    tcp.cDisplayWidth  = LCD_GetWidth();
    tcp.cDisplayHeight = LCD_GetHeight();
    tcp.CalibrationX   = 0;
    tcp.CalibrationY   = 0;

    TINYCLR_CHECK_HRESULT(TOUCH_PANEL_GetDeviceCaps( TOUCH_PANEL_CALIBRATION_POINT_ID, &tcp ));

    hbX.SetInteger( tcp.CalibrationX );  TINYCLR_CHECK_HRESULT(hbX.StoreToReference( stack.Arg2(), 0 ));
    hbY.SetInteger( tcp.CalibrationY );  TINYCLR_CHECK_HRESULT(hbY.StoreToReference( stack.Arg3(), 0 ));
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_touch_native_Microsoft_SPOT_Touch_TouchPanel::SetCalibration___VOID__I4__SZARRAY_I2__SZARRAY_I2__SZARRAY_I2__SZARRAY_I2( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    CLR_RT_HeapBlock_Array* screenX;
    CLR_RT_HeapBlock_Array* screenY;
    CLR_RT_HeapBlock_Array* uncalX;
    CLR_RT_HeapBlock_Array* uncalY;

    CLR_RT_HeapBlock* pArgs = &stack.Arg2();

    screenX = pArgs[ 0 ].DereferenceArray();  FAULT_ON_NULL(screenX);        
    screenY = pArgs[ 1 ].DereferenceArray();  FAULT_ON_NULL(screenY);        
    uncalX  = pArgs[ 2 ].DereferenceArray();  FAULT_ON_NULL(uncalX);        
    uncalY  = pArgs[ 3 ].DereferenceArray();  FAULT_ON_NULL(uncalY);        
        
    TINYCLR_SET_AND_LEAVE(TOUCH_PANEL_SetCalibration( 
                                                     stack.Arg1().NumericByRef().s4, 
                                                     (CLR_INT16*)screenX->GetFirstElement(), 
                                                     (CLR_INT16*)screenY->GetFirstElement(), 
                                                     (CLR_INT16*)uncalX ->GetFirstElement(), 
                                                     (CLR_INT16*)uncalY ->GetFirstElement()
                                                    ));

    TINYCLR_NOCLEANUP();
}


