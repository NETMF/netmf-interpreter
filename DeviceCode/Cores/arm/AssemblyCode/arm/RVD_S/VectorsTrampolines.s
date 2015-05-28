;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; Copyright (c) Microsoft Corporation.  All rights reserved.
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

;*****************************************************************************

    IMPORT  UNDEF_SubHandler
    IMPORT  ABORTP_SubHandler
    IMPORT  ABORTD_SubHandler  

    IMPORT  IRQ_Handler      ; from stubs.cpp 

    EXPORT  ARM_Vectors

;*****************************************************************************

    AREA |.text|, CODE, READONLY

    ; ARM directive is only valid for ARM/THUMB processor, but not CORTEX 
    IF :DEF:COMPILE_ARM :LOR: :DEF:COMPILE_THUMB
    ARM
    ENDIF
    
ARM_Vectors

    ; RESET
RESET_VECTOR
    b       UNDEF_VECTOR

    ; UNDEF INSTR
UNDEF_VECTOR
    ldr     pc, UNDEF_SubHandler_Trampoline

    ; SWI
SWI_VECTOR
    DCD     0xbaadf00d

    ; PREFETCH ABORT
PREFETCH_VECTOR
    ldr     pc, ABORTP_SubHandler_Trampoline

    ; DATA ABORT
DATA_VECTOR
    ldr     pc, ABORTD_SubHandler_Trampoline

    ; unused
USED_VECTOR
    DCD     0xbaadf00d

    ; IRQ
IRQ_VECTOR
    ldr     pc, IRQ_SubHandler_Trampoline

    ; FIQ
    ; we place the FIQ handler where it was designed to go, immediately at the end of the vector table
    ; this saves an additional 3+ clock cycle branch to the handler
FIQ_Handler
    IF :DEF: FIQ_SAMPLING_PROFILER    
    ldr     pc,FIQ_SubHandler_Trampoline    
    
FIQ_SubHandler_Trampoline    
    DCD     FIQ_SubHandler
    ENDIF
    
UNDEF_SubHandler_Trampoline
    DCD     UNDEF_SubHandler

ABORTP_SubHandler_Trampoline
    DCD     ABORTP_SubHandler

ABORTD_SubHandler_Trampoline
    DCD     ABORTD_SubHandler

    
        ; route the normal interupt handler to the proper lowest level driver
IRQ_SubHandler_Trampoline
    DCD  	IRQ_Handler

;*****************************************************************************

    IF :DEF:COMPILE_THUMB  
    THUMB
    ENDIF

    END

