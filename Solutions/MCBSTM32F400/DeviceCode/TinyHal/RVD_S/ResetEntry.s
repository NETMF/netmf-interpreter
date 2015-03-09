;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;
;  Licensed under the Apache License, Version 2.0 (the "License");
;  you may not use this file except in compliance with the License.
;  You may obtain a copy of the License at http:;www.apache.org/licenses/LICENSE-2.0
;
;  Copyright (c) Microsoft Corporation. All rights reserved.
;  Implementation for STM32: Copyright (c) Oberon microsystems, Inc.
;
;  CORTEX-M3 Bootloader Entry Code 
;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;
; This version of the "first entry" support is used when the application is 
; is the bootloader or otherwise started from the power on reset for the CPU.
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

    AREA    |.text|, CODE, READONLY

    EXPORT  EntryPoint

    EXPORT StackBottom
    EXPORT StackTop
    EXPORT HeapBegin
    EXPORT HeapEnd
    EXPORT CustomHeapBegin
    EXPORT CustomHeapEnd
    EXPORT __initial_sp

    IMPORT  BootEntry
    IMPORT  BootstrapCodeMinimal

    PRESERVE8

    ;*************************************************************************

    ; these define the region to zero initialize
    IMPORT  ||Image$$ER_RAM_RW$$ZI$$Base||
    IMPORT  ||Image$$ER_RAM_RW$$ZI$$Length||
    
    ;*************************************************************************

    AREA SectionForStackBottom,       DATA
StackBottom       DCD 0
    AREA SectionForStackTop,          DATA
__initial_sp 
StackTop          DCD 0
    AREA SectionForHeapBegin,         DATA
HeapBegin         DCD 0
    AREA SectionForHeapEnd,           DATA
HeapEnd           DCD 0
    AREA SectionForCustomHeapBegin,   DATA
CustomHeapBegin   DCD 0
    AREA SectionForCustomHeapEnd,     DATA
CustomHeapEnd     DCD 0


    AREA ||i.EntryPoint||, CODE, READONLY
    
    ENTRY

EntryPoint

; The first word has a dual role:
; - out of reset it contains the initial stack pointer value
; - it is the first entry of the initial exception handler table

    DCD     __initial_sp
    DCD     Start         ; Reset
    DCD     Fault_Handler ; NMI
    DCD     Fault_Handler ; Hard Fault
    DCD     Fault_Handler ; MMU Fault
    DCD     Fault_Handler ; Bus Fault
    DCD     Fault_Handler ; Usage Fault

Start PROC
    bl      BootstrapCodeMinimal

    ;*************************************************************************
    ; clear the Zero Initialized RAM, .bss 
    ; do this last just in case there is overlap on the load area
    ; and the execution ZI area
    ; this is not a failsafe, but helps some situations
    ; it would be nice to trap non-workable scatter files somehow - more thought required here @todo
    
    ldr     r0, =||Image$$ER_RAM_RW$$ZI$$Length||
    cmp     r0, #0
    beq     NoClearZI_ER_RAM
    ldr     r1, =||Image$$ER_RAM_RW$$ZI$$Base||
    mov     r2, #0

ClearZI_ER_RAM

    stmia   r1!, { r2 }
    subs    r0, r0, #4                  ; 4 bytes filled per loop 
    bne     ClearZI_ER_RAM

NoClearZI_ER_RAM
    ;*************************************************************************
    ; done moving and clearing RAM, so continue on in C 
    b       BootEntry
    b       Fault_Handler
    ENDP

    ALIGN   4


Fault_Handler
    b       Fault_Handler

    END
