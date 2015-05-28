;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; Copyright (c) Microsoft Corporation.  All rights reserved.
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

    PRESERVE8
    THUMB

    EXPORT  EntryPoint
    IMPORT  BootEntry
    IMPORT  BootstrapCode

    IMPORT  ARM_Vectors         ; Even if we don't use this symbol, it's required by the linker to properly include the Vector trampolines.
    IMPORT  Boot_Vectors        ; ditto

    IF HAL_REDUCESIZE = "1"
    IMPORT    Prot_Bytes            ; ditto
    ENDIF       ;[HAL_REDUCESIZE = "1" :]
    
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

    EXPORT StackBottom
    EXPORT StackTop
    EXPORT HeapBegin
    EXPORT HeapEnd
    EXPORT CustomHeapBegin
    EXPORT CustomHeapEnd

    AREA ||i.EntryPoint||, CODE, READONLY

    ; signature & mini vector table
    DCD     0xDEADBEEF
    DCD     EntryPoint

EntryPoint
    bl      BootstrapCode
    bl      BootEntry

    END


