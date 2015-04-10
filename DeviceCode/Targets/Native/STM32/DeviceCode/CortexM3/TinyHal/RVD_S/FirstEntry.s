;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;
;  Licensed under the Apache License, Version 2.0 (the "License");
;  you may not use this file except in compliance with the License.
;  You may obtain a copy of the License at http:;www.apache.org/licenses/LICENSE-2.0
;
;  Copyright (c) Microsoft Corporation. All rights reserved.
;  Implementation for STM32: Copyright (c) Oberon microsystems, Inc.
;
;  CORTEX-M3 Standard Entry Code 
;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

    EXPORT  EntryPoint

    EXPORT StackBottom
    EXPORT StackTop
    EXPORT HeapBegin
    EXPORT HeapEnd
    EXPORT CustomHeapBegin
    EXPORT CustomHeapEnd

    IMPORT  PreStackInit

    IF HAL_REDUCESIZE = "1"
        IMPORT BootEntryLoader
    ELSE
        IMPORT BootEntry
    ENDIF
    IMPORT  BootstrapCode


    PRESERVE8

    ;*************************************************************************

    AREA SectionForStackBottom,       DATA
StackBottom       DCD 0
    
    AREA SectionForStackTop,          DATA
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

; The first word has several functions:
; - It is the entry point of the application
; - it contains a signature word used to identify application blocks
; - out of reset it contains the initial stack pointer value
; - it is the first entry of the initial exception handler table
; The actual word used is 0x2000E00C

    b       Start         ; 0xE00C
    DCW     0x2000        ; Booter signature is 0x2000E00C
    DCD     Start         ; Reset
    DCD     Fault_Handler ; NMI
    DCD     Fault_Handler ; Hard Fault
    DCD     Fault_Handler ; MMU Fault
    DCD     Fault_Handler ; Bus Fault
    DCD     Fault_Handler ; Usage Fault

Start

    ; allow per processor pre-stack initialization

PreStackEntry
    bl      PreStackInit
    ldr     sp,StackTop_Ptr
    bl      BootstrapCode

    IF HAL_REDUCESIZE = "1"
        b   BootEntryLoader
    ELSE
        b   BootEntry
    ENDIF


    ALIGN   4

StackTop_Ptr
    DCD     StackTop

Fault_Handler
    b       Fault_Handler

    END
