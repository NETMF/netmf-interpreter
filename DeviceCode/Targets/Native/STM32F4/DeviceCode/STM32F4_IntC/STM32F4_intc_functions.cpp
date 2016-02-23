////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32F4: Copyright (c) Oberon microsystems, Inc.
//
//  *** Interrupt Handling ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "..\..\..\..\..\FreeRTOS\CMSIS_RTOS\cmsis_os.h"

#ifdef STM32F4XX
#include "..\stm32f4xx.h"
#else
#include "..\stm32f2xx.h"
#endif

extern "C" void PendSV_Handler( void );
extern "C" void SysTick_Handler( void );
extern "C" void SVC_Handler( void );

extern UINT32 ARM_Vectors[84];  // the interrupt vector table
extern UINT32 FAULT_SubHandler; // the standard fault handler
#ifdef FIQ_SAMPLING_PROFILER
extern UINT32 FIQ_Handler;      // the profiler NMI handler
#endif

void CPU_INTC_Initialize()
{
    // disable all interrupts
    NVIC->ICER[0] = 0xFFFFFFFF;
    NVIC->ICER[1] = 0xFFFFFFFF;
    NVIC->ICER[2] = 0xFFFFFFFF;
    // clear pending bits
    NVIC->ICPR[0] = 0xFFFFFFFF;
    NVIC->ICPR[1] = 0xFFFFFFFF;
    NVIC->ICPR[2] = 0xFFFFFFFF;

#if !PLATFORM_ARM_OS_PORT    
#ifdef FIQ_SAMPLING_PROFILER
    ARM_Vectors[2]  = (UINT32)&FIQ_Handler;      // NMI
#else
    ARM_Vectors[2]  = (UINT32)&FAULT_SubHandler; // NMI
#endif
    ARM_Vectors[3]  = (UINT32)&FAULT_SubHandler; // Hard Fault
    ARM_Vectors[4]  = (UINT32)&FAULT_SubHandler; // MMU Fault
    ARM_Vectors[5]  = (UINT32)&FAULT_SubHandler; // Bus Fault
    ARM_Vectors[6]  = (UINT32)&FAULT_SubHandler; // Usage Fault
    ARM_Vectors[11] = (UINT32)&FAULT_SubHandler; // SVC
    ARM_Vectors[12] = (UINT32)&FAULT_SubHandler; // Debug
    ARM_Vectors[14] = (UINT32)&FAULT_SubHandler; // PendSV
    ARM_Vectors[15] = (UINT32)&FAULT_SubHandler; // Systick
    
    __DMB(); // ensure table is written
#else

#if (__FREE_RTOS)
    // NETMF handles the int vectors and FreeRTOS requires that the following handlers are set so we need to set them by code 
    void (*SVC_Handler_ptr)() = SVC_Handler;
     ARM_Vectors[11] = (UINT32)SVC_Handler_ptr; // SVC

    void (*PendSV_Handler_ptr)() = PendSV_Handler;
    ARM_Vectors[14] = (UINT32)PendSV_Handler_ptr; // PendSV

    void (*SysTick_Handler_ptr)() = SysTick_Handler;
    ARM_Vectors[15] = (UINT32)SysTick_Handler_ptr; // Systick   
    
    __DMB(); // ensure table is written
#endif // __FREE_RTOS
     
#endif        

    SCB->AIRCR = (0x5FA << SCB_AIRCR_VECTKEY_Pos) // unlock key
               | (7 << SCB_AIRCR_PRIGROUP_Pos);   // no priority group bits
    SCB->VTOR = (UINT32)ARM_Vectors; // vector table base
    SCB->SHCSR |= SCB_SHCSR_USGFAULTENA_Msk  // enable faults
                | SCB_SHCSR_BUSFAULTENA_Msk
                | SCB_SHCSR_MEMFAULTENA_Msk;
}

BOOL CPU_INTC_ActivateInterrupt( UINT32 Irq_Index, HAL_CALLBACK_FPN ISR, void* ISR_Param )
{
    ARM_Vectors[Irq_Index + 16] = (UINT32)ISR; // exception = irq + 16
    __DMB(); // asure table is written
    NVIC->ICPR[Irq_Index >> 5] = 1 << (Irq_Index & 0x1F); // clear pending bit
    NVIC->ISER[Irq_Index >> 5] = 1 << (Irq_Index & 0x1F); // set enable bit
    return TRUE;
}

BOOL CPU_INTC_DeactivateInterrupt( UINT32 Irq_Index )
{
    NVIC->ICER[Irq_Index >> 5] = 1 << (Irq_Index & 0x1F); // clear enable bit */
    return TRUE;
}

BOOL CPU_INTC_InterruptEnable( UINT32 Irq_Index )
{
    UINT32 ier = NVIC->ISER[Irq_Index >> 5]; // old state
    NVIC->ISER[Irq_Index >> 5] = 1 << (Irq_Index & 0x1F); // set enable bit
    return (ier >> (Irq_Index & 0x1F)) & 1; // old enable bit
}

BOOL CPU_INTC_InterruptDisable( UINT32 Irq_Index )
{
    UINT32 ier = NVIC->ISER[Irq_Index >> 5]; // old state
    NVIC->ICER[Irq_Index >> 5] = 1 << (Irq_Index & 0x1F); // clear enable bit
    return (ier >> (Irq_Index & 0x1F)) & 1; // old enable bit
}

BOOL CPU_INTC_InterruptEnableState( UINT32 Irq_Index )
{
    // return enabled bit
    return (NVIC->ISER[Irq_Index >> 5] >> (Irq_Index & 0x1F)) & 1;
}

BOOL CPU_INTC_InterruptState( UINT32 Irq_Index )
{
    // return pending bit
    return (NVIC->ISPR[Irq_Index >> 5] >> (Irq_Index & 0x1F)) & 1;
}
