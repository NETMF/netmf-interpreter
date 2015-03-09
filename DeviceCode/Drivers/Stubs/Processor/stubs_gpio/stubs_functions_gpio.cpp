////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//
BOOL CPU_GPIO_Initialize()
{
    return FALSE;
}

BOOL CPU_GPIO_Uninitialize()
{
    return FALSE;
}

UINT32 CPU_GPIO_Attributes( GPIO_PIN Pin )
{
    return GPIO_ATTRIBUTE_NONE;
}

void CPU_GPIO_DisablePin( GPIO_PIN Pin, GPIO_RESISTOR ResistorState, UINT32 Direction, GPIO_ALT_MODE AltFunction )
{
}

void CPU_GPIO_EnableOutputPin( GPIO_PIN Pin, BOOL InitialState )
{
}

BOOL CPU_GPIO_EnableInputPin( GPIO_PIN Pin, BOOL GlitchFilterEnable, GPIO_INTERRUPT_SERVICE_ROUTINE PIN_ISR, GPIO_INT_EDGE IntEdge, GPIO_RESISTOR ResistorState )
{
    return FALSE;
}

BOOL CPU_GPIO_EnableInputPin2( GPIO_PIN Pin, BOOL GlitchFilterEnable, GPIO_INTERRUPT_SERVICE_ROUTINE PIN_ISR, void* ISR_Param, GPIO_INT_EDGE IntEdge, GPIO_RESISTOR ResistorState )
{
    return FALSE;
}

BOOL CPU_GPIO_GetPinState( GPIO_PIN Pin )
{
    return FALSE;
}

void CPU_GPIO_SetPinState( GPIO_PIN Pin, BOOL PinState )
{
}

BOOL CPU_GPIO_PinIsBusy( GPIO_PIN Pin )
{
    return FALSE;
}

BOOL CPU_GPIO_ReservePin( GPIO_PIN Pin, BOOL fReserve )
{
    return FALSE;
}

UINT32 CPU_GPIO_GetDebounce()
{
    return 0;
}

BOOL CPU_GPIO_SetDebounce( INT64 debounceTimeMilliseconds )
{
    return FALSE;
}

INT32 CPU_GPIO_GetPinCount()
{
    return 0;
} 

void CPU_GPIO_GetPinsMap( UINT8* pins, size_t size )
{
    pins = NULL;
}

UINT8 CPU_GPIO_GetSupportedResistorModes( GPIO_PIN pin )
{
    // as it is stub, return 0;
   return 0;
}
UINT8 CPU_GPIO_GetSupportedInterruptModes( GPIO_PIN pin )
{
    // as it is stub, return 0;
    return 0;
}

