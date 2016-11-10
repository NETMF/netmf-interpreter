////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//
__weak BOOL CPU_GPIO_Initialize()
{
    return FALSE;
}

__weak BOOL CPU_GPIO_Uninitialize()
{
    return FALSE;
}

__weak UINT32 CPU_GPIO_Attributes( GPIO_PIN Pin )
{
    return GPIO_ATTRIBUTE_NONE;
}

__weak void CPU_GPIO_DisablePin( GPIO_PIN Pin, GPIO_RESISTOR ResistorState, UINT32 Direction, GPIO_ALT_MODE AltFunction )
{
}

__weak void CPU_GPIO_EnableOutputPin( GPIO_PIN Pin, BOOL InitialState )
{
}

__weak BOOL CPU_GPIO_EnableInputPin( GPIO_PIN Pin, BOOL GlitchFilterEnable, GPIO_INTERRUPT_SERVICE_ROUTINE PIN_ISR, GPIO_INT_EDGE IntEdge, GPIO_RESISTOR ResistorState )
{
    return FALSE;
}

__weak BOOL CPU_GPIO_EnableInputPin2( GPIO_PIN Pin, BOOL GlitchFilterEnable, GPIO_INTERRUPT_SERVICE_ROUTINE PIN_ISR, void* ISR_Param, GPIO_INT_EDGE IntEdge, GPIO_RESISTOR ResistorState )
{
    return FALSE;
}

__weak BOOL CPU_GPIO_GetPinState( GPIO_PIN Pin )
{
    return FALSE;
}

__weak void CPU_GPIO_SetPinState( GPIO_PIN Pin, BOOL PinState )
{
}

__weak BOOL CPU_GPIO_PinIsBusy( GPIO_PIN Pin )
{
    return FALSE;
}

__weak BOOL CPU_GPIO_ReservePin( GPIO_PIN Pin, BOOL fReserve )
{
    return FALSE;
}

__weak UINT32 CPU_GPIO_GetDebounce()
{
    return 0;
}

__weak BOOL CPU_GPIO_SetDebounce( INT64 debounceTimeMilliseconds )
{
    return FALSE;
}

__weak INT32 CPU_GPIO_GetPinCount()
{
    return 0;
} 

__weak void CPU_GPIO_GetPinsMap( UINT8* pins, size_t size )
{
    pins = NULL;
}

__weak UINT8 CPU_GPIO_GetSupportedResistorModes( GPIO_PIN pin )
{
    // as it is stub, return 0;
   return 0;
}

__weak UINT8 CPU_GPIO_GetSupportedInterruptModes( GPIO_PIN pin )
{
    // as it is stub, return 0;
    return 0;
}
