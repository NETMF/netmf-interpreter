@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@
@  Licensed under the Apache License, Version 2.0 (the "License")@
@  you may not use this file except in compliance with the License.
@  You may obtain a copy of the License at http:@www.apache.org/licenses/LICENSE-2.0
@
@  Copyright (c) Microsoft Corporation. All rights reserved.
@  Implementation for STM32: Copyright (c) Oberon microsystems, Inc.
@
@  CORTEX-M3 Standard Entry Code 
@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .syntax unified
    .arch armv7-m

    .global  EntryPoint
    .global  __initial_sp
    .global Reset_Handler
    .global StackBottom
    .global StackTop
    .global HeapBegin
    .global HeapEnd
    .global CustomHeapBegin
    .global CustomHeapEnd

    .extern  PreStackInit


    .extern BootEntry
    .extern BootstrapCode

    @*************************************************************************

    .section SectionForStackBottom, "a", %progbits
StackBottom:
    .word 0

    .section SectionForStackTop, "a", %progbits
__initial_sp:
StackTop:
    .word 0
    
    .section SectionForHeapBegin, "a", %progbits
HeapBegin:
    .word 0

    .section SectionForHeapEnd, "a", %progbits
HeapEnd:
    .word 0

    .section SectionForCustomHeapBegin, "a", %progbits
ARM_Vectors:
    .space   84*4   @ exception vector table (max 84 entries)

CustomHeapBegin:
    .word 0

    .section SectionForCustomHeapEnd, "a", %progbits
CustomHeapEnd:
    .word 0

    @ Power On Reset vector table for the device
    @ This is placed at physical address 0 by the
    @ linker. THe first entry is the initial stack
    @ pointer as defined for the ARMv6-M and ARMv7-M
    @ architecture profiles. Therefore, all such 
    @ devices MUST have some amount of SRAM available
    @ for booting
    .section i.EntryPoint, "ax", %progbits
    .thumb_func

EntryPoint:
    .word    __initial_sp
    .word     Reset_Handler @ Reset
    .word     Fault_Handler @ NMI
    .word     Fault_Handler @ Hard Fault
    .word     Fault_Handler @ MMU Fault
    .word     Fault_Handler @ Bus Fault
    .word     Fault_Handler @ Usage Fault

    .text
    .thumb
    .thumb_func
    .align   2
    .global   Reset_Handler
    .type    Reset_Handler, %function

Reset_Handler:
    bl  BootstrapCode
    
    IF HAL_REDUCESIZE = "1"
        b   BootEntryLoader
    ELSE
        b   BootEntry
    ENDIF

    .pool
    .size    Reset_Handler, . - Reset_Handler

    .balign   4

Fault_Handler:
    .thumb_func
    b       Fault_Handler

    .end

