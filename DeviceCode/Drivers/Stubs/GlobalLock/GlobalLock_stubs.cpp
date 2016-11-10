////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#if !defined(ARM_V1_2)

__weak SmartPtr_IRQ::SmartPtr_IRQ(void* context)
{ 
}

__weak SmartPtr_IRQ::~SmartPtr_IRQ() 
{ 
}

#endif

__weak BOOL SmartPtr_IRQ::WasDisabled()
{
    return TRUE;
    
}

__weak void SmartPtr_IRQ::Acquire()
{
}

__weak void SmartPtr_IRQ::Release()
{
}

__weak void SmartPtr_IRQ::Probe()
{
}

__weak BOOL SmartPtr_IRQ::GetState(void* context)
{
    return TRUE;
}

__weak BOOL SmartPtr_IRQ::ForceDisabled(void* context)
{
    return TRUE;    
}

__weak BOOL SmartPtr_IRQ::ForceEnabled(void* context)
{
    return TRUE; 
}

__weak void SmartPtr_IRQ::Disable()
{
}

__weak void SmartPtr_IRQ::Restore()
{
}
