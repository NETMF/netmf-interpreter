////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32: Copyright (c) Oberon microsystems, Inc.
//
//  CORTEX-M3 Interrupt Disable Handling 
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#ifndef __GNUC__

#pragma arm section code = "SectionForFlashOperations"


/*
 *  Usage:
 *  constructor { Release Acquire | Probe } destructor
 *
 *  legal states:     m_state    Primask
 *  WAS_DISABLED         1       1 (disabled)
 *  WAS_ENABLED          0       1 (disabled)
 *  RELEASED             0       0 (enabled)
 */


/*
 *  disabled -> WAS_DISABLED
 *  enabled  -> WAS_ENABLED
 */
__asm SmartPtr_IRQ::SmartPtr_IRQ(void* context)
{ 
/*
    m_context = context; // never used
    Disable(); 
*/
//  STR      r1,[r0,#__cpp(offsetof(SmartPtr_IRQ, m_context))] // never used
    MRS      r2,PRIMASK
    CPSID    i
    STR      r2,[r0,#__cpp(offsetof(SmartPtr_IRQ, m_state))]
    BX       lr
}

/*
 *  WAS_DISABLED -> disable
 *  WAS_ENABLED  -> enable
 *  RELEASED     -> enable
 */
__asm SmartPtr_IRQ::~SmartPtr_IRQ() 
{ 
/*
    Restore(); 
*/
    LDR      r1,[r0,#__cpp(offsetof(SmartPtr_IRQ, m_state))]
    MSR      PRIMASK,r1
    BX       lr
}

__asm BOOL SmartPtr_IRQ::WasDisabled()
{
// Also check for interrupt state != 0
/*
    register UINT32 IPSR __asm("ipsr");
    return m_state || IPSR != 0;
*/
    MRS      r1,IPSR
    LDR      r0,[r0,#__cpp(offsetof(SmartPtr_IRQ, m_state))]
    CBZ      r1,L1
    MOV      r0,#1
L1  BX       lr
}

/*
 *  WAS_DISABLED -> WAS_DISABLED
 *  WAS_ENABLED  -> RELEASED
 *  RELEASED     -> RELEASED
 */
__asm void SmartPtr_IRQ::Release()
{
/*
    Restore(); 
*/
    LDR      r1,[r0,#__cpp(offsetof(SmartPtr_IRQ, m_state))]
    MSR      PRIMASK,r1
    BX       lr
}

/*
 *  WAS_DISABLED -> WAS_DISABLED
 *  WAS_ENABLED  -> WAS_ENABLED
 *  RELEASED     -> WAS_ENABLED
 */
__asm void SmartPtr_IRQ::Acquire()
{
/*
    __disable_irq();
*/
    CPSID    i
    BX       lr
}

/*
 *  WAS_DISABLED -> WAS_DISABLED
 *  WAS_ENABLED  -> enabled -> WAS_ENABLED
 *  RELEASED     -> RELEASED
 */
__asm void SmartPtr_IRQ::Probe()
{
/*
    register UINT32 Primask __asm("primask");
    UINT32 m = Primask;
    Primask = m_state;
    Primask = m;
*/
    LDR      r1,[r0,#__cpp(offsetof(SmartPtr_IRQ, m_state))]
    MRS      r2,PRIMASK
    MSR      PRIMASK,r1
    NOP
    MSR      PRIMASK,r2
    BX       lr
}


// static members

__asm BOOL SmartPtr_IRQ::GetState(void* context)
{
// Also check for interrupt state == 0
/*
    register UINT32 Primask __asm("primask");
    register UINT32 IPSR __asm("ipsr");
    return !Primask && IPSR == 0; 
*/
    MRS      r0,PRIMASK
    MRS      r1,IPSR
    EOR      r0,r0,#1
    CBZ      r1,L2
    MOV      r0,#0
L2  BX       lr
}

__asm BOOL SmartPtr_IRQ::ForceDisabled(void* context)
{
/*
    register UINT32 Primask __asm("primask");
    UINT32 m = Primask;
    __disable_irq();
    return m ^ 1;
*/
    MRS      r0,PRIMASK
    CPSID    i
    EOR      r0,r0,#1
    BX       lr
}

__asm BOOL SmartPtr_IRQ::ForceEnabled(void* context)
{
/*
    register UINT32 Primask __asm("primask");
    UINT32 m = Primask;
    __enable_irq();
    return m ^ 1;
*/
    MRS      r0,PRIMASK
    CPSIE    i
    EOR      r0,r0,#1
    BX       lr
}


// private members (not used)

__asm void SmartPtr_IRQ::Disable()
{
/*
    register UINT32 Primask __asm("primask");
    m_state = Primask;
    __disable_irq();
*/
    MRS      r1,PRIMASK
    CPSID    i
    STR      r1,[r0,#__cpp(offsetof(SmartPtr_IRQ, m_state))]
    BX       lr
}

__asm void SmartPtr_IRQ::Restore()
{
/*
    register UINT32 Primask __asm("primask");
    Primask = m_state;
*/
    LDR      r1,[r0,#__cpp(offsetof(SmartPtr_IRQ, m_state))]
    MSR      PRIMASK,r1
    BX       lr
}


#pragma arm section code 


#endif //#ifndef __GNUC__
