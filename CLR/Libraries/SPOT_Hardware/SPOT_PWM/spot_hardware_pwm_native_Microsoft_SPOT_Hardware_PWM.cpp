////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "spot_hardware_pwm.h"

//--//

HRESULT Library_spot_hardware_pwm_native_Microsoft_SPOT_Hardware_PWM::Start___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);

    ::PWM_Start( (PWM_CHANNEL)pThis[FIELD__m_channel].NumericByRef().s4, 
                              pThis[FIELD__m_pin    ].NumericByRef().s4 );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_pwm_native_Microsoft_SPOT_Hardware_PWM::Stop___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);

    ::PWM_Stop( (PWM_CHANNEL)pThis[FIELD__m_channel].NumericByRef().s4, 
                             pThis[FIELD__m_pin    ].NumericByRef().s4 );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_pwm_native_Microsoft_SPOT_Hardware_PWM::Commit___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    
    CLR_RT_HeapBlock* pThis = stack.This();

    const CLR_UINT32 period   = pThis[FIELD__m_period  ].NumericByRef().u4;
    const CLR_UINT32 duration = pThis[FIELD__m_duration].NumericByRef().u4;
    const CLR_UINT32 scale    = pThis[FIELD__m_scale   ].NumericByRef().u4;

    CLR_UINT32       period_requested   = period;
    CLR_UINT32       duration_requested = duration;
    PWM_SCALE_FACTOR scale_requested    = (PWM_SCALE_FACTOR)scale;


    bool fRes = ::PWM_ApplyConfiguration( (PWM_CHANNEL)pThis[FIELD__m_channel ].NumericByRef().s4, 
                                           pThis[FIELD__m_pin     ].NumericByRef().s4, 
                                           period_requested,
                                           duration_requested,
                                           scale_requested,
                                           pThis[FIELD__m_invert  ].NumericByRef().s1 != 0 ) != 0;

    if(!fRes) 
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_OPERATION);
    }

    if(period != period_requested) 
    {
        pThis[FIELD__m_period].SetInteger(period_requested);
    }
    if(duration != duration_requested) 
    {
        pThis[FIELD__m_duration].SetInteger(duration_requested);
    }
    if(scale != scale_requested) 
    {
        pThis[FIELD__m_scale].SetInteger(scale_requested);
    }

    TINYCLR_NOCLEANUP();
}


HRESULT Library_spot_hardware_pwm_native_Microsoft_SPOT_Hardware_PWM::Init___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);

    ::PWM_Initialize( (PWM_CHANNEL)pThis[FIELD__m_channel].NumericByRef().s4 );
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_pwm_native_Microsoft_SPOT_Hardware_PWM::Uninit___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis = stack.This();  FAULT_ON_NULL(pThis);

    ::PWM_Uninitialize( (PWM_CHANNEL)pThis[FIELD__m_channel].NumericByRef().s4 );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_pwm_native_Microsoft_SPOT_Hardware_PWM::Start___STATIC__VOID__SZARRAY_MicrosoftSPOTHardwarePWM( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    PWM_CHANNEL channels[c_MaxPwmChannels];
    GPIO_PIN    pins    [c_MaxPwmChannels];
    CLR_UINT32 channelCount = 0;

    CLR_RT_HeapBlock_Array* ports = stack.Arg0().DereferenceArray(); FAULT_ON_NULL(ports);

    TINYCLR_CHECK_HRESULT(GetChannelsAndPins(ports, channels, pins, channelCount));

    ::PWM_Start(channels, pins, channelCount);

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_pwm_native_Microsoft_SPOT_Hardware_PWM::Stop___STATIC__VOID__SZARRAY_MicrosoftSPOTHardwarePWM( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    PWM_CHANNEL channels[c_MaxPwmChannels];
    GPIO_PIN    pins    [c_MaxPwmChannels];
    CLR_UINT32 channelCount = 0;
    
    CLR_RT_HeapBlock_Array* ports = stack.Arg0().DereferenceArray(); FAULT_ON_NULL(ports);

    TINYCLR_CHECK_HRESULT(GetChannelsAndPins(ports, channels, pins, channelCount));

    ::PWM_Stop(channels, pins, channelCount);
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_pwm_native_Microsoft_SPOT_Hardware_PWM::GetChannelsAndPins(CLR_RT_HeapBlock_Array* ports, PWM_CHANNEL* const channels, GPIO_PIN* const pins, CLR_UINT32& channelCount)
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    
    channelCount = ports->m_numOfElements;
    
    for(CLR_UINT32 ch = 0; ch < channelCount; ++ch) 
    {
        CLR_RT_HeapBlock* pwmPort = ((CLR_RT_HeapBlock*)(ports->GetElement(ch)))->Dereference();  FAULT_ON_NULL(pwmPort);
            
        channels[ch] = (PWM_CHANNEL)pwmPort[FIELD__m_channel].NumericByRef().s4;
        pins    [ch] = (PWM_CHANNEL)pwmPort[FIELD__m_pin    ].NumericByRef().s4;
    }
    
    TINYCLR_NOCLEANUP();
}

