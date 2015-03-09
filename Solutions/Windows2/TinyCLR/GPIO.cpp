////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "stdafx.h"

using namespace Microsoft::SPOT::Emulator;

////////////////////////////////////////////////////////////////////////////////////////////////

BOOL CPU_GPIO_Initialize()
{
    return EmulatorNative::GetIGpioDriver()->Initialize();
}

BOOL CPU_GPIO_Uninitialize()
{
    return EmulatorNative::GetIGpioDriver()->Uninitialize();
}

UINT32 CPU_GPIO_Attributes( GPIO_PIN Pin )
{
    return EmulatorNative::GetIGpioDriver()->Attributes( Pin );
}

void CPU_GPIO_DisablePin( GPIO_PIN Pin, GPIO_RESISTOR ResistorState, UINT32 Direction, GPIO_ALT_MODE AltFunction )
{
    EmulatorNative::GetIGpioDriver()->DisablePin( Pin, ResistorState, Direction, AltFunction );
}

void CPU_GPIO_EnableOutputPin( GPIO_PIN Pin, BOOL InitialState )
{
    EmulatorNative::GetIGpioDriver()->EnableOutputPin( Pin, INT_TO_BOOL(InitialState) );
}

BOOL CPU_GPIO_EnableInputPin( GPIO_PIN Pin, BOOL GlitchFilterEnable, GPIO_INTERRUPT_SERVICE_ROUTINE PIN_ISR, GPIO_INT_EDGE IntEdge, GPIO_RESISTOR ResistorState )
{
    return EmulatorNative::GetIGpioDriver()->EnableInputPin( Pin, INT_TO_BOOL(GlitchFilterEnable), 
        (System::IntPtr)(void*)PIN_ISR, System::IntPtr::Zero, IntEdge, ResistorState );
}

BOOL CPU_GPIO_EnableInputPin2( GPIO_PIN Pin, BOOL GlitchFilterEnable, GPIO_INTERRUPT_SERVICE_ROUTINE PIN_ISR, void* ISR_Param, GPIO_INT_EDGE IntEdge, GPIO_RESISTOR ResistorState )
{
    return EmulatorNative::GetIGpioDriver()->EnableInputPin( Pin, INT_TO_BOOL(GlitchFilterEnable), 
        (System::IntPtr)(void*)PIN_ISR, (System::IntPtr)ISR_Param, IntEdge, ResistorState );
}

BOOL CPU_GPIO_GetPinState( GPIO_PIN Pin )
{
    return EmulatorNative::GetIGpioDriver()->GetPinState( Pin );
}

void CPU_GPIO_SetPinState( GPIO_PIN Pin, BOOL PinState )
{
    EmulatorNative::GetIGpioDriver()->SetPinState( Pin, INT_TO_BOOL(PinState) );
}

BOOL CPU_GPIO_PinIsBusy( GPIO_PIN Pin )
{
    return EmulatorNative::GetIGpioDriver()->PinIsBusy( Pin );
}

BOOL CPU_GPIO_ReservePin( GPIO_PIN Pin, BOOL fReserve )
{
    return EmulatorNative::GetIGpioDriver()->ReservePin( Pin, INT_TO_BOOL(fReserve) );
}

UINT32 CPU_GPIO_GetDebounce()
{
    return EmulatorNative::GetIGpioDriver()->GetDebounce();
}

BOOL CPU_GPIO_SetDebounce( INT64 debounceTimeMilliseconds )
{
    return EmulatorNative::GetIGpioDriver()->SetDebounce( debounceTimeMilliseconds );
}

INT32 CPU_GPIO_GetPinCount( )
{
    return EmulatorNative::GetIGpioDriver()->GetPinCount();
}

void CPU_GPIO_GetPinsMap( UINT8* pins, size_t size )
{
    EmulatorNative::GetIGpioDriver()->GetPinsMap((System::IntPtr) pins, size );
}

UINT8 CPU_GPIO_GetSupportedResistorModes( GPIO_PIN pin )
{
    return EmulatorNative::GetIGpioDriver()->GetSupportedResistorModes( pin );
}

UINT8 CPU_GPIO_GetSupportedInterruptModes( GPIO_PIN pin )
{
    return EmulatorNative::GetIGpioDriver()->GetSupportedInterruptModes( pin );
}


