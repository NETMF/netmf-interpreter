;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; Copyright (c) Microsoft Corporation.  All rights reserved.
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;


    EXPORT  EntryPoint

    EXPORT  PreStackInit_Exit_Pointer

    IMPORT  PreStackInit


    IF HAL_REDUCESIZE = "1"
        IMPORT BootEntryLoader
    ELSE
        IMPORT  BootEntry
    ENDIF
    IMPORT  BootstrapCode
    IMPORT  ARM_Vectors         ; Even if we don't use this symbol, it's required by the linker to properly include the Vector trampolines.


    PRESERVE8

    ;*************************************************************************

PSR_MODE_USER       EQU     0xD0
PSR_MODE_FIQ        EQU     0xD1
PSR_MODE_IRQ        EQU     0xD2
PSR_MODE_SUPERVISOR EQU     0xD3
PSR_MODE_ABORT      EQU     0xD7
PSR_MODE_UNDEF      EQU     0xDB
PSR_MODE_SYSTEM     EQU     0xDF

STACK_MODE_ABORT    EQU     16
STACK_MODE_UNDEF    EQU     16 
STACK_MODE_IRQ      EQU     2048
    IF :DEF: FIQ_SAMPLING_PROFILER
STACK_MODE_FIQ      EQU     2048
    ENDIF

    ;*************************************************************************

    AREA SectionForStackBottom,       DATA, NOINIT
StackBottom       DCD 0
    AREA SectionForStackTop,          DATA, NOINIT
StackTop          DCD 0
    AREA SectionForHeapBegin,         DATA, NOINIT
HeapBegin         DCD 0
    AREA SectionForHeapEnd,           DATA, NOINIT
HeapEnd           DCD 0
    AREA SectionForCustomHeapBegin,   DATA, NOINIT
CustomHeapBegin   DCD 0
    AREA SectionForCustomHeapEnd,     DATA, NOINIT
CustomHeapEnd     DCD 0


    EXPORT StackBottom
    EXPORT StackTop
    EXPORT HeapBegin
    EXPORT HeapEnd
    EXPORT CustomHeapBegin
    EXPORT CustomHeapEnd

  

    IF HAL_REDUCESIZE = "1" :LAND: TargetLocation != "RAM"
    ; -----------------------------------------------
    ; ADD BOOT MARKER HERE IF YOU NEED ONE
    ; -----------------------------------------------
    ENDIF       ;[HAL_REDUCESIZE = "1" :LAND: TargetLocation != "RAM"]

    AREA ||i.EntryPoint||, CODE, READONLY

    ; ARM directive is only valid for ARM/THUMB processor, but not CORTEX 
    IF :DEF:COMPILE_ARM :LOR: :DEF:COMPILE_THUMB  
    ARM
    ENDIF
    
    ENTRY

EntryPoint

    IF HAL_REDUCESIZE = "1" :LAND: TargetLocation != "RAM" :LAND:  TargetPlatformProcessor = "PLATFORM_ARM_LPC22XX"
    ; -----------------------------------------------
    ; ADD BOOT MARKER HERE IF YOU NEED ONE
    ; -----------------------------------------------
    orr     r0, pc,#0x80000000
    mov     pc, r0
    ENDIF       ;[HAL_REDUCESIZE = "1" :LAND: TargetLocation != "RAM"]

    ; designed to be a vector area for ARM
    ; RESET
    ; keep PortBooter signature the same
    msr     cpsr_c, #PSR_MODE_SYSTEM    ; go into System mode, interrupts off


	IF TargetPlatformProcessor = "PLATFORM_ARM_LPC24XX"
    ; LPC24XX on chip bootloader requires valid checksum in internal Flash
    ; location 0x14 ( ARM reserved vector ).
    B       PreStackEntry
    SPACE 20
    ENDIF


    ; allow per processor pre-stack initialization initialization

PreStackEntry
    B       PreStackInit

PreStackInit_Exit_Pointer 

    ldr     r0, =StackTop               ; new SYS stack pointer for a full decrementing stack


    msr     cpsr_c, #PSR_MODE_ABORT     ; go into ABORT mode, interrupts off
    mov     sp, r0                      ; stack top
    sub     r0, r0, #STACK_MODE_ABORT   ; ( take the size of abort stack off )
    
    msr     cpsr_c, #PSR_MODE_UNDEF     ; go into UNDEF mode, interrupts off
    mov     sp, r0                      ; stack top - abort stack 
    sub     r0, r0, #STACK_MODE_UNDEF   ; 
    
    
    IF :DEF: FIQ_SAMPLING_PROFILER
    msr     cpsr_c, #PSR_MODE_FIQ       ; go into FIQ mode, interrupts off
    mov     sp, r0                      ; stack top - abort stack - undef stack
    sub     r0, r0, #STACK_MODE_FIQ 
    ENDIF    

    msr     cpsr_c, #PSR_MODE_IRQ       ; go into IRQ mode, interrupts off
    mov     sp, r0                      ; stack top - abort stack - undef stack (- FIQ stack)
    sub     r0, r0, #STACK_MODE_IRQ
   
    msr     cpsr_c, #PSR_MODE_SYSTEM    ; go into System mode, interrupts off
    mov     sp,r0                       ; stack top - abort stack - undef stack (- FIQ stack) - IRQ stack


    
    ;******************************************************************************************
    ; This ensures that we execute from the real location, regardless of any remapping scheme *
    ;******************************************************************************************

    ldr     pc, EntryPoint_Restart_Pointer
EntryPoint_Restart_Pointer 
    DCD     EntryPoint_Restart
EntryPoint_Restart   

    ;*********************************************************************

    bl      BootstrapCode

    IF HAL_REDUCESIZE = "1"
        b   BootEntryLoader
    ELSE
        b   BootEntry
    ENDIF

    IF :DEF:COMPILE_THUMB  
    THUMB
    ENDIF

    IF TargetPlatformProcessor = "PLATFORM_ARM_LPC22XX"
    ;; Add space to align on vector remap size
    SPACE 8
    ENDIF

    END
