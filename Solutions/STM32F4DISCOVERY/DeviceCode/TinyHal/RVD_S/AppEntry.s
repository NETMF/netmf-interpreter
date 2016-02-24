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
;
; This version of the "first entry" support is used when the application is 
; loaded or otherwise started from a bootloader. (e.g. this application isn't 
; a boot loader). More specifically this version is used whenever the application
; does NOT run from the power on reset vector because some other code is already
; there.
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

    EXPORT  EntryPoint

    EXPORT StackBottom
    EXPORT StackTop
    EXPORT HeapBegin
    EXPORT HeapEnd
    EXPORT CustomHeapBegin
    EXPORT CustomHeapEnd
    EXPORT __initial_sp
    EXPORT Reset_Handler

    IMPORT  SystemInit
    IMPORT  __main

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

    AREA ||i.EntryPoint||, CODE, READONLY

    ENTRY

EntryPoint

; The first word has a dual role:
; - It is the entry point of the application loaded or discovered by
;   the bootloader and therfore must be valid executable code
; - it contains a signature word used to identify application blocks
;   in TinyBooter (see: Tinybooter_ProgramWordCheck() for more details )
; * The additional entries in this table are completely ignored and
;   remain for backwards compatibility. Since the boot loader is hard
;   coded to look for the signature, half of which is an actual relative
;   branch instruction, removing the unused entries would require all
;   bootloaders to be updated as well. [sic.]
;   [ NOTE:
;     In the next major release where we can afford to break backwards
;     compatibility this will almost certainly change, as the whole
;     init/startup for NETMF is overly complex. The various tools used
;     for building the CLR have all come around to supporting simpler
;     init sequences we should leverage directly
;   ]
; The actual word used is 0x2000E00C

    b       Reset_Handler ; 0xE00C
    DCW     0x2000        ; Booter signature is 0x2000E00C
    DCD     0 ; [ UNUSED ]
    DCD     0 ; [ UNUSED ]
    DCD     0 ; [ UNUSED ]
    DCD     0 ; [ UNUSED ]
    DCD     0 ; [ UNUSED ]
    DCD     0 ; [ UNUSED ]

Reset_Handler
    ;; reload the stack pointer as there's no returning to the loader
    ldr     sp, =__initial_sp
    LDR     R0, =SystemInit
    BLX     R0
    LDR     R0, =__main
    BX      R0
    
    ALIGN

;*******************************************************************************
; User Stack and Heap initialization for ARMCC CRT
;*******************************************************************************
    EXPORT  __user_setup_stackheap
                 
__user_setup_stackheap
    ;; enforce 8 byte alignment
    LDR     R0, = HeapBegin
    ADD     R0, R0, #8 ; adjust up to 8 byte alignment
    BIC     R0, #3     ; mask down to get final aligned value
    LDR     R2, = HeapEnd

    LDR     SP, = StackTop
    BX      LR

    ALIGN

    END
