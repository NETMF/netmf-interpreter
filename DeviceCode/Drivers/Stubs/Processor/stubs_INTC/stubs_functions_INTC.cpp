////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//
__weak void __irq IRQ_Handler()
{
}

__weak void CPU_INTC_Initialize()
{
}

__weak BOOL CPU_INTC_ActivateInterrupt( UINT32 Irq_Index, HAL_CALLBACK_FPN ISR, void* ISR_Param )
{
    return FALSE;
}

__weak BOOL CPU_INTC_DeactivateInterrupt( UINT32 Irq_Index )
{
    return FALSE;
}

__weak BOOL CPU_INTC_InterruptEnable( UINT32 Irq_Index )
{
    return FALSE;
}

__weak BOOL CPU_INTC_InterruptDisable( UINT32 Irq_Index )
{
    return FALSE;
}

__weak BOOL CPU_INTC_InterruptEnableState( UINT32 Irq_Index )
{
    return FALSE;
}

__weak BOOL CPU_INTC_InterruptState( UINT32 Irq_Index )
{
    return FALSE;
}
