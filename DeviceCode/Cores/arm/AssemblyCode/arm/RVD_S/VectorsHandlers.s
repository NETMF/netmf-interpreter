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
    ; ARM directive is only valid for ARM/THUMB processor, but not CORTEX 
    IF :DEF:COMPILE_ARM :LOR: :DEF:COMPILE_THUMB
    ARM
    ENDIF
UNDEF_SubHandler
; on entry, were are in UNDEF mode, without a usable stack
    stmfd   r13!, {r0}                  ; store the r0 at undef stack first
    mrs     r0, spsr                    ; get the previous mode.
    orr     r0, r0, #0xC0               ; keep interrupts disabled.
    msr     cpsr_c, r0                  ; go back to the previous mode.
  
    stmfd   r13!, {r1-r12}              ; push unbanked registers on the stack
    msr     cpsr_c, #PSR_MODE_UNDEF     ; go back into UNDEF mode, but keep IRQs off
    mov     r3,r0
    mrs     r0, spsr                    ; now save the spsr_UNDEF register
    mov     r1, r14                     ; now save the r14_UNDEF  register
    LDMFD   r13!,{r2}                   ; r2 <= previous r0

    msr     cpsr_c, r3                  ; go back to the previous mode.
    stmfd   r13!, {r0-r2}               ; push spsr_UNDEF and r14_UNDEF on stack and the old r0 value

    mov     r0, r13                     ; ARG1 of handler: the stack location of registers
    add     r1, r13, #60                ; ARG2 of handler: SYSTEM mode stack at time of exception (without saved registers = 15*4 back in stack)
    mov     r2, r14                     ; ARG3 of handler: get the link register of SYSTEM mode: r14_SYSTEM
    b       UNDEF_Handler               ; address of vector routine in C to jump to, never expect to return

;*****************************************************************************

; the ABORT conditions on the ARM7TDMI are catastrophic, so there is no return, but it is supported in non-RTM builds

    AREA ||i.ABORTP_SubHandler||, CODE, READONLY
    ; ARM directive is only valid for ARM/THUMB processor, but not CORTEX 
    IF :DEF:COMPILE_ARM :LOR: :DEF:COMPILE_THUMB
    ARM
    ENDIF
ABORTP_SubHandler
    ; on entry, were are in ABORT mode, without a usable stack
    stmfd   r13!, {r0}                  ; store the r0 at undef stack first
    mrs     r0, spsr                    ; get the previous mode.
    orr     r0, r0, #0xC0               ; keep interrupts disabled.
    msr     cpsr_c, r0                  ; go back to the previous mode.
  
    stmfd   r13!, {r1-r12}              ; push unbanked registers on the stack
    msr     cpsr_c, #PSR_MODE_ABORT     ; go back into ABORT mode, but keep IRQs off
    mov     r3,r0
    mrs     r0, spsr                    ; now save the spsr_ABORT register
    mov     r1, r14                     ; now save the r14_ABORT  register
    LDMFD   r13!,{r2}                   ; r2 <= previous r0

    msr     cpsr_c, r3                  ; go back to the previous mode.
    stmfd   r13!, {r0-r2}               ; push spsr_ABORT and r14_ABORT on stack and the old r0 value

    mov     r0, r13                     ; ARG1 of handler: the stack location of registers
    add     r1, r13, #60                ; ARG2 of handler: SYSTEM mode stack at time of exception (without saved registers = 15*4 back in stack)
    mov     r2, r14                     ; ARG3 of handler: get the link register of SYSTEM mode: r14_SYSTEM
    b       ABORTP_Handler               ; address of vector routine in C to jump to, never expect to return


;*****************************************************************************

    AREA ||i.ABORTD_SubHandler||, CODE, READONLY
    ; ARM directive is only valid for ARM/THUMB processor, but not CORTEX 
    IF :DEF:COMPILE_ARM :LOR: :DEF:COMPILE_THUMB
    ARM
    ENDIF
ABORTD_SubHandler
    ; on entry, were are in ABORT mode, without a usable stack
    stmfd   r13!, {r0}                  ; store the r0 at undef stack first
    mrs     r0, spsr                    ; get the previous mode.
    orr     r0, r0, #0xC0               ; keep interrupts disabled.
    msr     cpsr_c, r0                  ; go back to the previous mode.
  
    stmfd   r13!, {r1-r12}              ; push unbanked registers on the stack
    msr     cpsr_c, #PSR_MODE_ABORT     ; go back into ABORT mode, but keep IRQs off
    mov     r3,r0
    mrs     r0, spsr                    ; now save the spsr_ABORT register
    mov     r1, r14                     ; now save the r14_ABORT  register
    LDMFD   r13!,{r2}                   ; r2 <= previous r0

    msr     cpsr_c, r3                  ; go back to the previous mode.
    stmfd   r13!, {r0-r2}               ; push spsr_ABORT and r14_ABORT on stack and the old r0 value

    mov     r0, r13                     ; ARG1 of handler: the stack location of registers
    add     r1, r13, #60                ; ARG2 of handler: SYSTEM mode stack at time of exception (without saved registers = 15*4 back in stack)
    mov     r2, r14                     ; ARG3 of handler: get the link register of SYSTEM mode: r14_SYSTEM
    b       ABORTD_Handler               ; address of vector routine in C to jump to, never expect to return

;*****************************************************************************

    AREA ||i.HARD_Breakpoint||, CODE, READONLY     ; void HARD_Breakpoint()
    ; ARM directive is only valid for ARM/THUMB processor, but not CORTEX 
    IF :DEF:COMPILE_ARM :LOR: :DEF:COMPILE_THUMB
    ARM
    ENDIF
HARD_Breakpoint
    ; on entry, were are being called from C/C++ in system mode

    msr     cpsr_c, #PSR_MODE_SYSTEM    ; go into SYSTEM mode, giving us a usable stack, but keep IRQs off
    stmfd   r13!, {r0-r12}              ; push unbanked registers on the stack
    mrs     r0, cpsr                    ; now save the cpsr_SYSTEM register
    mov     r1, r14                     ; now save the r14_SYSTEM  register
    stmfd   r13!, {r0-r1}               ; push spsr_ABORT and r14_ABORT on stack

    mov     r0, r13                     ; ARG1 of handler: the stack location of registers
    add     r1, r13, #60                ; ARG2 of handler: SYSTEM mode stack at time of call (without saved registers = 15*4 back in stack)
    mov     r2, r14                     ; ARG3 of handler: get the link register of SYSTEM mode: r14_SYSTEM

    b       HARD_Breakpoint_Handler     ; address of vector routine in C to jump to, never expect to return


;*****************************************************************************

    IF :DEF:COMPILE_THUMB  
    THUMB
    ENDIF

    END

