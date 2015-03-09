////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#if !defined(ARM_V1_2)

SmartPtr_IRQ::SmartPtr_IRQ(void* context)
{ 
}

SmartPtr_IRQ::~SmartPtr_IRQ() 
{ 
}

#endif

BOOL SmartPtr_IRQ::WasDisabled()
{
    return TRUE;
    
}

void SmartPtr_IRQ::Acquire()
{
}

void SmartPtr_IRQ::Release()
{
}

void SmartPtr_IRQ::Probe()
{
}

BOOL SmartPtr_IRQ::GetState(void* context)
{
    return TRUE;
}

BOOL SmartPtr_IRQ::ForceDisabled(void* context)
{
    return TRUE;    
}

BOOL SmartPtr_IRQ::ForceEnabled(void* context)
{
    return TRUE; 
}

void SmartPtr_IRQ::Disable()
{
}

void SmartPtr_IRQ::Restore()
{
}

