;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; Copyright (c) Microsoft Corporation.  All rights reserved.
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

;*****************************************************************************
    
    IF :DEF: FIQ_SAMPLING_PROFILER

    EXPORT  Profiler_FIQ_Initialize

    IMPORT  EntryPoint
    IMPORT  HeapBegin
    IMPORT  ProfilerBufferBegin
    IMPORT  ProfilerBufferEnd

    AREA ||i.Profiler_FIQ_Initialize||, CODE, READONLY

Profiler_FIQ_Initialize

    stmfd   r13!, {r14}                                 ; save r14 (lr), since we will use it when branching

;; something else for cortex
    
    ldmfd   r13!, {pc}                                  ; return to caller, restore link register in to pc




    ENDIF
    
    END
