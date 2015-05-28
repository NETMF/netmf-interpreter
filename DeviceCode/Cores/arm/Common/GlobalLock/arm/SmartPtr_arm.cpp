////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#ifndef __GNUC__

#define DISABLED_MASK 0x80
#define SYSTEM_MODE_MASK 0x0F

#define ASSERT_SYSTEM_IRQ_MODE(x) //ASSERT((x & SYSTEM_MODE_MASK)==SYSTEM_MODE_MASK);

#pragma ARM


#pragma arm section code = "SectionForFlashOperations"

#if !defined(ARM_V1_2)
SmartPtr_IRQ::SmartPtr_IRQ(void* context)
{ 
    m_context = context; 
    Disable(); 
}

SmartPtr_IRQ::~SmartPtr_IRQ() 
{ 
    Restore(); 
}
#endif

BOOL SmartPtr_IRQ::WasDisabled()
{
    return (m_state & DISABLED_MASK) == DISABLED_MASK;
}

void SmartPtr_IRQ::Acquire()
{
    UINT32 Cp = m_state;

    if((Cp & DISABLED_MASK) == DISABLED_MASK)
    {
        Disable();
    }
}

void SmartPtr_IRQ::Release()
{
    UINT32 Cp = m_state;

    ASSERT_SYSTEM_IRQ_MODE(Cp);

    if((Cp & DISABLED_MASK) == 0)
    {
        UINT32 Cs;

        __asm
        {
            MRS     Cp, CPSR
            BIC     Cs, Cp, #0x80
            MSR     CPSR_c, Cs
        }

        m_state = Cp;
    }
}

void SmartPtr_IRQ::Probe()
{
    UINT32 Cp = m_state;

    ASSERT_SYSTEM_IRQ_MODE(Cp);

    if((Cp & DISABLED_MASK) == 0)
    {
        UINT32 Cs;

        __asm
        {
            MRS     Cp, CPSR
            BIC     Cs, Cp, #0x80
            MSR     CPSR_c, Cs
            MSR     CPSR_c, Cp
        }
    }
}

BOOL SmartPtr_IRQ::GetState(void* context)
{
    UINT32 Cp;
	
    __asm {
        MRS     Cp, CPSR
        MVN     Cp, Cp
        AND     Cp, Cp, #0x80
    }

    return Cp;
}

BOOL SmartPtr_IRQ::ForceDisabled(void* context)
{
    UINT32 Cp;
    UINT32 Cs;

    __asm
    {
        MRS     Cp, CPSR
        ORR     Cs, Cp, #0x80
        MSR     CPSR_c, Cs
        MVN     Cp, Cp
        AND     Cp, Cp, #0x80
    }

    return Cp;
}

BOOL SmartPtr_IRQ::ForceEnabled(void* context)
{
    UINT32 Cp;
    UINT32 Cs;

    __asm
    {
        MRS     Cp, CPSR
        BIC     Cs, Cp, #0x80
        MSR     CPSR_c, Cs
        MVN     Cp, Cp
        AND     Cp, Cp, #0x80
    }

    return Cp;
}

void SmartPtr_IRQ::Disable()
{
    UINT32 Cp;
    UINT32 Cs;

    __asm
    {
        MRS     Cp, CPSR
        ORR     Cs, Cp, #0x80
        MSR     CPSR_c, Cs
    }

    m_state = Cp;
}

void SmartPtr_IRQ::Restore()
{
    UINT32 Cp = m_state;

    if((Cp & DISABLED_MASK) == 0)
    {
        ASSERT_SYSTEM_IRQ_MODE(Cp);
        __asm
        {
            MRS     Cp, CPSR
            BIC     Cp, Cp, #0x80
            MSR     CPSR_c, Cp
        }
    }
}

#pragma arm section code 


#endif //#ifndef __GNUC__
