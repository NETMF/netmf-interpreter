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
    .thumb

    .global  EntryPoint
    .global Reset_Handler
    .global HeapBegin
    .global HeapEnd
    .global _end
    .global PowerOnReset

    .extern SystemInit

    @*************************************************************************

    /* start address for the initialization values of the .data section. 
    defined in linker script */
    .word  _sidata
    /* start address for the .data section. defined in linker script */  
    .word  _sdata
    /* end address for the .data section. defined in linker script */
    .word  _edata
    /* start address for the .bss section. defined in linker script */
    .word  _sbss
    /* end address for the .bss section. defined in linker script */
    .word  _ebss
    /* stack used for SystemInit_ExtMemCtl; always internal RAM used */

    /* start address of managed heap */
    .word HeapBegin
    /* end  address of managed heap */
    .word HeapEnd

    @ Power On Reset vector table for the device
    @ This is placed at physical address 0 by the
    @ linker. The first entry is the initial stack
    @ pointer as defined for the ARMv6-M and ARMv7-M
    @ architecture profiles. Therefore, all such 
    @ devices MUST have some amount of SRAM available
    @ for booting

    .section SectionForPowerOnReset, "x", %progbits
PowerOnReset:
    .word     _estack
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

    .section i.EntryPoint, "ax", %progbits
EntryPoint:
Reset_Handler:
    ldr   sp, =_estack     /* set stack pointer */

    /* Copy the data segment initializers from flash to SRAM */  
    movs  r1, #0
    b  LoopCopyDataInit

CopyDataInit:
    ldr  r3, =_sidata
    ldr  r3, [r3, r1]
    str  r3, [r0, r1]
    adds  r1, r1, #4
        
LoopCopyDataInit:
    ldr  r0, =_sdata
    ldr  r3, =_edata
    adds  r2, r0, r1
    cmp  r2, r3
    bcc  CopyDataInit
    ldr  r2, =_sbss
    b  LoopFillZerobss
/* Zero fill the bss segment. */  
FillZerobss:
    movs  r3, #0
    str  r3, [r2], #4
        
LoopFillZerobss:
    ldr  r3, = _ebss
    cmp  r2, r3
    bcc  FillZerobss

    /* Call the clock system intitialization function.*/
    bl  SystemInit   
    
    /* Call static constructors */
    bl __libc_init_array
    
    /* Call the application's entry point.*/
    bl  main
    bx  lr 

    .pool
    .size    Reset_Handler, . - Reset_Handler

    .balign   4

    .thumb_func
Fault_Handler:
    b       Fault_Handler

    .end

