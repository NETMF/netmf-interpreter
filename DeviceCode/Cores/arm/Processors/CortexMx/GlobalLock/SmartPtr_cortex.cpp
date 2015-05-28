////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//
//  CORTEX-Mx Interrupt Disable Handling 
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#include <tinyhal.h>

#define __CMSIS_GENERIC              /* disable NVIC and Systick functions */

#if defined (CORTEX_M7)
    #include "core_cm7.h"
#elif defined (CORTEX_M4)
    #include "core_cm4.h"
#elif defined (CORTEX_M3)
    #include "core_cm3.h"
#elif defined (CORTEX_M0)
    #include "core_cm0.h"
#elif defined (CORTEX_M0PLUS)
    #include "core_cm0plus.h"
#else
    #error "Processor not specified or unsupported."
#endif

const uint32_t DISABLED_MASK = 0x1;

SmartPtr_IRQ::SmartPtr_IRQ(void* context)
{
    m_context = context;
    Disable();
}

SmartPtr_IRQ::~SmartPtr_IRQ()
{
    Restore();
}

BOOL SmartPtr_IRQ::WasDisabled()
{
    return (m_state & DISABLED_MASK) == DISABLED_MASK;
}

void SmartPtr_IRQ::Acquire()
{
    UINT32 Cp = m_state;

    if ((Cp & DISABLED_MASK) == DISABLED_MASK)
    {
        Disable();
    }
}

void SmartPtr_IRQ::Release()
{
    UINT32 Cp = m_state;

    if ((Cp & DISABLED_MASK) == 0)
    {
        m_state = __get_PRIMASK();
        __enable_irq();
    }
}

void SmartPtr_IRQ::Probe()
{
    UINT32 Cp = m_state;

    if ((Cp & DISABLED_MASK) == 0)
    {
        register UINT32 state = __get_PRIMASK();

        __enable_irq();

        // just to allow an interupt to an occur
        __NOP();

        // restore irq state
        __set_PRIMASK( state );
    }
}

BOOL SmartPtr_IRQ::GetState(void* context)
{
    register UINT32 Cp = __get_PRIMASK();
    return (0 == (Cp & DISABLED_MASK));
}

BOOL SmartPtr_IRQ::ForceDisabled(void* context)
{
    __disable_irq();
    return true;
}

BOOL SmartPtr_IRQ::ForceEnabled(void* context)
{
    __enable_irq();
    return true;
}

void SmartPtr_IRQ::Disable()
{
    m_state = __get_PRIMASK();

    __disable_irq();
}

void SmartPtr_IRQ::Restore()
{
    UINT32 Cp = m_state;

    if ((Cp & DISABLED_MASK) == 0)
    {
        __enable_irq();
    }
}

