;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; Copyright (c) Microsoft Corporation.  All rights reserved.
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

;*****************************************************************************
    IF TargetPlatformProcessor = "PLATFORM_ARM_MC9328"    
    IF :DEF: FIQ_SAMPLING_PROFILER

    EXPORT  Profiler_FIQ_Initialize

    IMPORT  EntryPoint
    IMPORT  HeapBegin
    IMPORT  ProfilerBufferBegin
    IMPORT  ProfilerBufferEnd

MC9328MXL_ARM_TIMER_2_STATUS_REG   EQU     0x00203014
MC9328MXL_INTC_FIQ_ENABLE_CLEAR    EQU     0x00223018

    AREA ||i.Profiler_FIQ_Initialize||, CODE, READONLY
    ; ARM directive is only valid for ARM/THUMB processor, but not CORTEX 
    IF :DEF:COMPILE_ARM :LOR: :DEF:COMPILE_THUMB
    ARM
    ENDIF

Profiler_FIQ_Initialize

    stmfd   r13!, {r14}                                 ; save r14 (lr), since we will use it when branching

    msr     cpsr_c, #0xD1                               ; go into FIQ mode, but keep IRQs & FIQs off
    ldr     r8,     =ProfilerBufferBegin             ; load RAM address
    ldr	    r9,     =ProfilerBufferEnd
    ldr     r10,    =ProfilerBufferBegin
    sub     r9, r9, r10
    lsr     r9, r9, #2
    ldr     r10,      =EntryPoint                       ; load the entry point
    str     r10,[r8],#4
    sub     r9,r9,#1    
    ldr     r11,      =MC9328MXL_ARM_TIMER_2_STATUS_REG ; ARM Timer 2 Clear location 
    msr     cpsr_c, #0xDF                               ; go into SYSTEM mode, giving us a usable stack, but keep IRQs & FIQs off 
    
    ldmfd   r13!, {pc}                                  ; return to caller, restore link register in to pc


    IF :DEF:COMPILE_THUMB  
    THUMB
    ENDIF


    ENDIF
    ENDIF
    
    END
