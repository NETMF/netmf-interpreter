;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; Copyright (c) Microsoft Corporation.  All rights reserved.
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;


    AREA    |.text|, CODE, READONLY

    EXPORT  EntryPoint

    EXPORT  ARM_Vectors
    EXPORT  IRQ_SubHandler_Trampoline

    IMPORT  BootEntry

    IMPORT  UNDEF_SubHandler
    IMPORT  ABORTP_SubHandler
    IMPORT  ABORTD_SubHandler

    PRESERVE8

    IMPORT  IRQ_Handler        ; stubbed version with assert

    ;*************************************************************************

    ; these define the region to zero initialize
    IMPORT  ||Image$$ER_RAM_RW$$ZI$$Base||
    IMPORT  ||Image$$ER_RAM_RW$$ZI$$Length||
    
    ;*************************************************************************

PSR_MODE_USER       EQU     0xD0
PSR_MODE_FIQ        EQU     0xD1
PSR_MODE_IRQ        EQU     0xD2
PSR_MODE_SUPERVISOR EQU     0xD3
PSR_MODE_ABORT      EQU     0xD7
PSR_MODE_UNDEF      EQU     0xDB
PSR_MODE_SYSTEM     EQU     0xDF

; main can be use for exception
STACK_MODE_MAIN     EQU     2048
STACK_MODE_PROCESS  EQU     2048
    
    ;*************************************************************************
    AREA SectionForStackBottom, DATA, NOINIT
StackBottom DCD 0
    AREA SectionForStackTop,    DATA, NOINIT
StackTop    DCD 0
    AREA SectionForHeapBegin,   DATA, NOINIT
HeapBegin   DCD 0
    AREA SectionForHeapEnd,     DATA, NOINIT
HeapEnd     DCD 0
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

    AREA ||i.EntryPoint||, CODE, READONLY

    
    ENTRY

EntryPoint

ARM_Vectors                
    ; RESET
    b       VectorAreaSkip
    ; UNDEF INSTR
    ldr     pc, UNDEF_SubHandler_Trampoline
    ; SWI
    DCD     0xbaadf00d
    ; PREFETCH ABORT
    ldr     pc, ABORTP_SubHandler_Trampoline
    ; DATA ABORT
    ldr     pc, ABORTD_SubHandler_Trampoline
    ; unused
    DCD     0xbaadf00d
    ; IRQ
    ldr     pc, IRQ_SubHandler_Trampoline
    ; FIQ
    DCD     0xbaadf00d

UNDEF_SubHandler_Trampoline
    DCD     UNDEF_SubHandler
    
ABORTP_SubHandler_Trampoline
    DCD     ABORTP_SubHandler

ABORTD_SubHandler_Trampoline
    DCD     ABORTD_SubHandler

IRQ_SubHandler_Trampoline
    DCD     IRQ_Handler
  
		
VectorAreaSkip    

    ldr     r0, =StackTop               ; new SYS stack pointer for a full decrementing stack
    msr     msp, r0                      ; stack top
    sub     r0, r0, #STACK_MODE_MAIN    ; ( take the size of main stack, usually for the exception)
    msr     psp,r0                      ; sets for the process stack


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


  

    END
