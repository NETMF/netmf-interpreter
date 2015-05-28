////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#define DISABLED_MASK 0x80

#ifdef __GNUC__

extern "C"
{
UINT32   IRQ_LOCK_Release_asm();
void     IRQ_LOCK_Probe_asm();
UINT32   IRQ_LOCK_GetState_asm();
UINT32   IRQ_LOCK_ForceDisabled_asm();
UINT32   IRQ_LOCK_ForceEnabled_asm();
UINT32   IRQ_LOCK_Disable_asm();
void     IRQ_LOCK_Restore_asm();
}

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

    if((Cp & DISABLED_MASK) == DISABLED_MASK)
    {
        Disable();
    }
}

void SmartPtr_IRQ::Release()
{
    UINT32 Cp = m_state;

    if((Cp & DISABLED_MASK) == 0)
    {
        m_state = IRQ_LOCK_Release_asm();
    }
}

void SmartPtr_IRQ::Probe()
{
    UINT32 Cp = m_state;

    if((Cp & DISABLED_MASK) == 0)
    {
		IRQ_LOCK_Probe_asm();
    }
}

BOOL SmartPtr_IRQ::GetState(void* context)
{
	return IRQ_LOCK_GetState_asm();
}

BOOL SmartPtr_IRQ::ForceDisabled(void* context)
{
    return IRQ_LOCK_ForceDisabled_asm();
}

BOOL SmartPtr_IRQ::ForceEnabled(void* context)
{
    return IRQ_LOCK_ForceEnabled_asm();
}

void SmartPtr_IRQ::Disable()
{
    m_state = IRQ_LOCK_Disable_asm();
}

void SmartPtr_IRQ::Restore()
{
    UINT32 Cp = m_state;

    if((Cp & DISABLED_MASK) == 0)
    {
		IRQ_LOCK_Restore_asm();
    }
}
#endif //#ifdef __GNUC__

