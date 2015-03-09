////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware.h"

//--// USART


HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetSerialPins___VOID__I4__BYREF_MicrosoftSPOTHardwareCpuPin__BYREF_MicrosoftSPOTHardwareCpuPin__BYREF_MicrosoftSPOTHardwareCpuPin__BYREF_MicrosoftSPOTHardwareCpuPin( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock  hbrxPin;
    CLR_RT_HeapBlock  hbtxPin;
    CLR_RT_HeapBlock  hbctsPin;
    CLR_RT_HeapBlock  hbrtsPin;

    CLR_UINT32 port, rxPin, txPin, ctsPin, rtsPin;

    port = stack.Arg1().NumericByRef().u4;

    rxPin  = (CLR_UINT32)-1; // GPIO_NONE
    txPin  = (CLR_UINT32)-1; // GPIO_NONE
    ctsPin = (CLR_UINT32)-1; // GPIO_NONE
    rtsPin = (CLR_UINT32)-1; // GPIO_NONE
    
    // COM ports are numbered from 0 up
    if(port >= CPU_USART_PortsCount())
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }
    
    ::CPU_USART_GetPins( port, rxPin, txPin, ctsPin, rtsPin );

    hbrxPin.SetInteger ( (CLR_INT32)rxPin  ); TINYCLR_CHECK_HRESULT(hbrxPin.StoreToReference ( stack.Arg2(),  0 ));
    hbtxPin.SetInteger ( (CLR_INT32)txPin  ); TINYCLR_CHECK_HRESULT(hbtxPin.StoreToReference ( stack.Arg3(),  0 ));
    hbctsPin.SetInteger( (CLR_INT32)ctsPin ); TINYCLR_CHECK_HRESULT(hbctsPin.StoreToReference( stack.Arg4(),  0 ));
    hbrtsPin.SetInteger( (CLR_INT32)rtsPin ); TINYCLR_CHECK_HRESULT(hbrtsPin.StoreToReference( stack.ArgN(5), 0 ));
    

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetSerialPortsCount___I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    stack.SetResult_U4( ::CPU_USART_PortsCount() );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeSupportsNonStandardBaudRate___BOOLEAN__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_UINT32 port = stack.Arg1().NumericByRef().u4;

    if(::CPU_USART_SupportNonStandardBaudRate( port )==TRUE)
    {
        stack.SetResult_Boolean( true );
    }
    else
    {
        stack.SetResult_Boolean( false );
    }
        
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetBaudRateBoundary___VOID__I4__BYREF_U4__BYREF_U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock  hbmaxBR;
    CLR_RT_HeapBlock  hbminBR;

    CLR_UINT32 maxBR = 0, minBR = 0, port = stack.Arg1().NumericByRef().u4;
    
    // COM ports are numbered from 0 up
    if(port >= CPU_USART_PortsCount())
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    ::CPU_USART_GetBaudrateBoundary( port, maxBR, minBR );

    hbmaxBR.SetInteger( (CLR_INT32)maxBR ); TINYCLR_CHECK_HRESULT(hbmaxBR.StoreToReference( stack.Arg2(), 0 ));
    hbminBR.SetInteger( (CLR_INT32)minBR ); TINYCLR_CHECK_HRESULT(hbminBR.StoreToReference( stack.Arg3(), 0 ));
  

    TINYCLR_NOCLEANUP();


}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeIsSupportedBaudRate___BOOLEAN__I4__BYREF_U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    CLR_RT_HeapBlock  hbBR;
    CLR_UINT32        port;
   
    CLR_UINT32        SupportedBaudrate;

    port              = stack.Arg1().NumericByRef().u4;

    TINYCLR_CHECK_HRESULT(hbBR.LoadFromReference( stack.Arg2() ));
    SupportedBaudrate = hbBR.NumericByRef().u4;

    if (CPU_USART_IsBaudrateSupported(port,SupportedBaudrate) == TRUE)
        stack.SetResult_Boolean( true );
    else
        stack.SetResult_Boolean( false );

    hbBR.SetInteger( (CLR_UINT32)SupportedBaudrate );
    TINYCLR_CHECK_HRESULT(hbBR.StoreToReference( stack.Arg2(), 0 ));        

    TINYCLR_NOCLEANUP();
}


//--// SPI
HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetSpiPins___VOID__MicrosoftSPOTHardwareSPISPImodule__BYREF_MicrosoftSPOTHardwareCpuPin__BYREF_MicrosoftSPOTHardwareCpuPin__BYREF_MicrosoftSPOTHardwareCpuPin( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock  hbmsk;
    CLR_RT_HeapBlock  hbmiso;
    CLR_RT_HeapBlock  hbmosi;
    
    CLR_UINT32        port, msk, miso, mosi;

    msk  = (CLR_UINT32)-1; // GPIO_NONE
    miso = (CLR_UINT32)-1; // GPIO_NONE
    mosi = (CLR_UINT32)-1; // GPIO_NONE
    
    port = stack.Arg1().NumericByRef().u4;

    // SPI ports are numbered from 0 up
    if(port >= CPU_SPI_PortsCount())
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }
    
    ::CPU_SPI_GetPins( port, msk, miso, mosi );

    hbmsk.SetInteger ( (CLR_INT32)msk  ); TINYCLR_CHECK_HRESULT(hbmsk .StoreToReference( stack.Arg2(), 0 ));
    hbmiso.SetInteger( (CLR_INT32)miso ); TINYCLR_CHECK_HRESULT(hbmiso.StoreToReference( stack.Arg3(), 0 ));
    hbmosi.SetInteger( (CLR_INT32)mosi ); TINYCLR_CHECK_HRESULT(hbmosi.StoreToReference( stack.Arg4(), 0 ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetSpiPortsCount___I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    stack.SetResult_U4( ::CPU_SPI_PortsCount() );

    TINYCLR_NOCLEANUP_NOLABEL();
}

//--// I2C
HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetI2CPins___VOID__BYREF_MicrosoftSPOTHardwareCpuPin__BYREF_MicrosoftSPOTHardwareCpuPin( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    CLR_RT_HeapBlock  hbscl;
    CLR_RT_HeapBlock  hbsda;
    CLR_UINT32        scl, sda;

    scl = (CLR_UINT32)-1; // GPIO_NONE
    sda = (CLR_UINT32)-1; // GPIO_NONE

    ::I2C_Internal_GetPins( scl, sda );

    hbscl.SetInteger( (CLR_INT32)scl ); TINYCLR_CHECK_HRESULT(hbscl.StoreToReference( stack.Arg1(), 0 ));
    hbsda.SetInteger( (CLR_INT32)sda ); TINYCLR_CHECK_HRESULT(hbsda.StoreToReference( stack.Arg2(), 0 ));

    TINYCLR_NOCLEANUP();

}


//--// GPIO
HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetPinsCount___I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    stack.SetResult_U4( ::CPU_GPIO_GetPinCount() );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetPinsMap___VOID__SZARRAY_MicrosoftSPOTHardwareCpuPinUsage( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* map;
    CLR_INT32               mapLength;
    CLR_UINT8 *             ptr;
    map = stack.Arg1().DereferenceArray();  FAULT_ON_NULL(map);

    mapLength   = map->m_numOfElements;
    ptr         = map->GetFirstElement();

   ::CPU_GPIO_GetPinsMap( ptr, mapLength );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetPinUsage___MicrosoftSPOTHardwareCpuPinUsage__MicrosoftSPOTHardwareCpuPin( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    stack.SetResult_U4( ::CPU_GPIO_Attributes( stack.Arg1().NumericByRef().u4 ) );

    TINYCLR_NOCLEANUP_NOLABEL();
}
HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetSupportedResistorModes___MicrosoftSPOTHardwareCpuPinValidResistorMode__MicrosoftSPOTHardwareCpuPin( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    stack.SetResult_U4( ::CPU_GPIO_GetSupportedResistorModes( stack.Arg1().NumericByRef().u4 ) );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetSupportedInterruptModes___MicrosoftSPOTHardwareCpuPinValidInterruptMode__MicrosoftSPOTHardwareCpuPin( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    stack.SetResult_U4( ::CPU_GPIO_GetSupportedInterruptModes( stack.Arg1().NumericByRef().u4 ) );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetButtonPins___MicrosoftSPOTHardwareCpuPin__MicrosoftSPOTHardwareButton( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    stack.SetResult_U4( ::VirtualKey_GetPins( stack.Arg1().NumericByRef().u4 ) );

    TINYCLR_NOCLEANUP_NOLABEL();
}


HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetLCDMetrics___VOID__BYREF_I4__BYREF_I4__BYREF_I4__BYREF_I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();


    CLR_RT_HeapBlock  hbLength;
    CLR_RT_HeapBlock  hbWidth;
    CLR_RT_HeapBlock  hbBitPP;
    CLR_RT_HeapBlock  hbOrientation;

    CLR_UINT32        length, width, bitPP, orientation;

    width  = LCD_SCREEN_WIDTH;
    length = LCD_SCREEN_HEIGHT;
    bitPP  = LCD_SCREEN_BPP;
    orientation = LCD_SCREEN_ORIENTATION;

    hbLength.SetInteger( (CLR_INT32)length );TINYCLR_CHECK_HRESULT(hbLength.StoreToReference( stack.Arg1(), 0 ));
    hbWidth.SetInteger (( CLR_INT32)width  );TINYCLR_CHECK_HRESULT(hbWidth.StoreToReference ( stack.Arg2(), 0 ));

    hbBitPP.SetInteger( (CLR_INT32)bitPP );             TINYCLR_CHECK_HRESULT(hbBitPP.StoreToReference( stack.Arg3(), 0 ));
    hbOrientation.SetInteger( (CLR_INT32)orientation ); TINYCLR_CHECK_HRESULT(hbOrientation.StoreToReference( stack.Arg4(), 0 ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetPWMChannelsCount___I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    stack.SetResult_I4( ::PWM_PWMChannels() );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetPWMPinForChannel___MicrosoftSPOTHardwareCpuPin__MicrosoftSPOTHardwareCpuPWMChannel( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    
    GPIO_PIN pin = ::PWM_GetPinForChannel( (PWM_CHANNEL)stack.Arg1().NumericByRef().s4 );

    stack.SetResult_I4( (CLR_INT32)pin );
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetAnalogChannelsCount___I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    stack.SetResult_I4( ::AD_ADChannels() );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetAnalogPinForChannel___MicrosoftSPOTHardwareCpuPin__MicrosoftSPOTHardwareCpuAnalogChannel( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    
    GPIO_PIN pin = ::AD_GetPinForChannel( (ANALOG_CHANNEL)stack.Arg1().NumericByRef().s4 );

    if(pin == GPIO_PIN_NONE)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    stack.SetResult_I4( (CLR_INT32)pin );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetAvailablePrecisionInBitsForChannel___SZARRAY_I4__MicrosoftSPOTHardwareCpuAnalogChannel( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    
    CLR_INT32 precisions[32];
    CLR_UINT32 size = 32;

    if(::AD_GetAvailablePrecisionsForChannel( (ANALOG_CHANNEL)stack.Arg1().NumericByRef().s4, precisions, size ) == TRUE)
    {
        CLR_INT32* pPrecision = NULL;
        CLR_RT_HeapBlock& top = stack.PushValue();
        
        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( top, size, g_CLR_RT_WellKnownTypes.m_Int32));
        
        pPrecision = (CLR_INT32*)top.DereferenceArray()->GetFirstElement();
        
        memcpy(pPrecision, precisions, size * sizeof(CLR_INT32));        
    }
    else 
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);
    }

    TINYCLR_NOCLEANUP();
    
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetAnalogOutputChannelsCount___I4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    stack.SetResult_I4( ::DA_DAChannels() );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetAnalogOutputPinForChannel___MicrosoftSPOTHardwareCpuPin__MicrosoftSPOTHardwareCpuAnalogOutputChannel( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    
    GPIO_PIN pin = ::DA_GetPinForChannel((DA_CHANNEL)stack.Arg1().NumericByRef().s4);
    
    if(pin == GPIO_PIN_NONE)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    stack.SetResult_I4( (CLR_INT32)pin );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider::NativeGetAvailableAnalogOutputPrecisionInBitsForChannel___SZARRAY_I4__MicrosoftSPOTHardwareCpuAnalogOutputChannel( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    
    CLR_INT32 precisions[32];
    CLR_UINT32 size = 32;

    if(::DA_GetAvailablePrecisionsForChannel( (DA_CHANNEL)stack.Arg1().NumericByRef().s4, precisions, size ) == TRUE) 
    {
        CLR_INT32* pPrecision = NULL;
        CLR_RT_HeapBlock& top = stack.PushValue();
        
        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( top, size, g_CLR_RT_WellKnownTypes.m_Int32));
        
        pPrecision = (CLR_INT32*)top.DereferenceArray()->GetFirstElement();
        
        memcpy(pPrecision, precisions, size * sizeof(CLR_INT32));        
    } 
    else 
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);
    }

    TINYCLR_NOCLEANUP();
}
