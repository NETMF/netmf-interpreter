;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; Copyright (c) Microsoft Corporation.  All rights reserved.
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

;*****************************************************************************

    EXPORT  UNDEF_SubHandler
    EXPORT  ABORTP_SubHandler
    EXPORT  ABORTD_SubHandler

    IMPORT  UNDEF_Handler             ; void UNDEF_Handler  (unsigned int*, unsigned int, unsigned int)
    IMPORT  ABORTP_Handler            ; void ABORTP_Handler (unsigned int*, unsigned int, unsigned int)
    IMPORT  ABORTD_Handler            ; void ABORTD_Handler (unsigned int*, unsigned int, unsigned int)

    EXPORT  HARD_Breakpoint
    IMPORT  HARD_Breakpoint_Handler   ; HARD_Breakpoint_Handler(unsigned int*, unsigned int, unsigned int)

    IMPORT  StackBottom
    IMPORT  StackOverflow             ; StackOverflow(unsigned int)

    PRESERVE8

    IF :DEF: FIQ_SAMPLING_PROFILER :LOR: :DEF: FIQ_LATENCY_PROFILER

    ; here we always leave the IRQ disabled, FIQ enabled for profiling

PSR_MODE_USER       EQU     0x90
PSR_MODE_FIQ        EQU     0x91
PSR_MODE_IRQ        EQU     0x92
PSR_MODE_SUPERVISOR EQU     0x93
PSR_MODE_ABORT      EQU     0x97
PSR_MODE_UNDEF      EQU     0x9B
PSR_MODE_SYSTEM     EQU     0x9F

    ELSE

    ; here we always leave the IRQ disabled, FIQ disabled for safety

PSR_MODE_USER       EQU     0xD0
PSR_MODE_FIQ        EQU     0xD1
PSR_MODE_IRQ        EQU     0xD2
PSR_MODE_SUPERVISOR EQU     0xD3
PSR_MODE_ABORT      EQU     0xD7
PSR_MODE_UNDEF      EQU     0xDB
PSR_MODE_SYSTEM     EQU     0xDF

    ENDIF

;*****************************************************************************

; the UNDEF Instruction conditions on the ARM7TDMI are catastrophic, so there is no return, but it is supported in non-RTM builds

    AREA ||i.UNDEF_SubHandler||, CODE, READONLY

UNDEF_SubHandler
; on entry, were are in UNDEF mode, without a usable stack
    b       UNDEF_Handler               ; address of vector routine in C to jump to, never expect to return

;*****************************************************************************

; the ABORT conditions on the ARM7TDMI are catastrophic, so there is no return, but it is supported in non-RTM builds

    AREA ||i.ABORTP_SubHandler||, CODE, READONLY
    ; ARM directive is only valid for ARM/THUMB processor, but not CORTEX 
ABORTP_SubHandler
    ; on entry, were are in ABORT mode, without a usable stack
    b       ABORTP_Handler               ; address of vector routine in C to jump to, never expect to return


;*****************************************************************************

    AREA ||i.ABORTD_SubHandler||, CODE, READONLY
    ; ARM directive is only valid for ARM/THUMB processor, but not CORTEX 
ABORTD_SubHandler
    ; on entry, were are in ABORT mode, without a usable stack
    b       ABORTD_Handler               ; address of vector routine in C to jump to, never expect to return

;*****************************************************************************

    AREA ||i.HARD_Breakpoint||, CODE, READONLY     ; void HARD_Breakpoint()
HARD_Breakpoint
    ; on entry, were are being called from C/C++ in system mode


    b       HARD_Breakpoint_Handler     ; address of vector routine in C to jump to, never expect to return


;*****************************************************************************


    END

