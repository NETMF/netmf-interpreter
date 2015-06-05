;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;
;  Licensed under the Apache License, Version 2.0 (the "License");
;  you may not use this file except in compliance with the License.
;  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
;
;  Copyright (c) Microsoft Corporation. All rights reserved.
;  Implementation for STM32: Copyright (c) Oberon microsystems, Inc.
;
;  CORTEX-M3 Standard Entry Code 
;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

    EXPORT  EntryPoint
    EXPORT __initial_sp
    EXPORT Reset_Handler

    EXPORT StackBottom
    EXPORT StackTop
    EXPORT HeapBegin
    EXPORT HeapEnd
    EXPORT CustomHeapBegin
    EXPORT CustomHeapEnd

    IMPORT PreStackInit

    IMPORT BootEntry
    IMPORT BootstrapCode

    PRESERVE8

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

    AREA ||SectionForPowerOnReset||, CODE, READONLY
    ; Power On Reset vector table for the device
    ; This is placed at physical address 0 by the
    ; linker. The first entry is the initial stack
    ; pointer as defined for the ARMv6-M and ARMv7-M
    ; architecture profiles. Therefore, all such 
    ; devices MUST have some amount of SRAM available
    ; for booting
PowerOnReset
    DCD     __initial_sp
    DCD     Reset_Handler ; Reset
    DCD     Fault_Handler ; NMI
    DCD     Fault_Handler ; Hard Fault
    DCD     Fault_Handler ; MMU Fault
    DCD     Fault_Handler ; Bus Fault
    DCD     Fault_Handler ; Usage Fault

    AREA ||i.EntryPoint||, CODE, READONLY
    ENTRY
EntryPoint

Reset_Handler
    bl      BootstrapCode
    b       BootEntry
    
    LTORG

    ALIGN   4

Fault_Handler
    b       Fault_Handler

    END
