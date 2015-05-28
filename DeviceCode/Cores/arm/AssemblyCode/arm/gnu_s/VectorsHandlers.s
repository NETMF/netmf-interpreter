@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ Copyright (c) Microsoft Corporation.  All rights reserved.
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .global UNDEF_SubHandler
    .global ABORTP_SubHandler
    .global ABORTD_SubHandler

    .extern UNDEF_Handler               @ void UNDEF_Handler  (unsigned int*, unsigned int, unsigned int)
    .extern ABORTP_Handler              @ void ABORTP_Handler (unsigned int*, unsigned int, unsigned int)
    .extern ABORTD_Handler              @ void ABORTD_Handler (unsigned int*, unsigned int, unsigned int)

    .global HARD_Breakpoint
    .extern HARD_Breakpoint_Handler     @ HARD_Breakpoint_Handler(unsigned int*, unsigned int, unsigned int)

    .extern StackBottom
    .extern StackOverflow               @ StackOverflow(unsigned int)



    .ifdef FIQ_SAMPLING_PROFILER
        LEAVE_IRQ_DISABLED = 1
    .endif

    .ifdef FIQ_LATENCY_PROFILER
        LEAVE_IRQ_DISABLED = 1
    .endif

.ifdef LEAVE_IRQ_DISABLED

    @ here we always leave the IRQ disabled, FIQ enabled for profiling

PSR_MODE_USER       =   0x90
PSR_MODE_FIQ        =   0x91
PSR_MODE_IRQ        =   0x92
PSR_MODE_SUPERVISOR =   0x93
PSR_MODE_ABORT      =   0x97
PSR_MODE_UNDEF      =   0x9B
PSR_MODE_SYSTEM     =   0x9F

.else

    @ here we always leave the IRQ disabled, FIQ disabled for safety

PSR_MODE_USER       =   0xD0
PSR_MODE_FIQ        =   0xD1
PSR_MODE_IRQ        =   0xD2
PSR_MODE_SUPERVISOR =   0xD3
PSR_MODE_ABORT      =   0xD7
PSR_MODE_UNDEF      =   0xDB
PSR_MODE_SYSTEM     =   0xDF

.endif

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

@ the UNDEF Instruction conditions on the ARM7TDMI are catastrophic, so there is no return, but it is supported in non-RTM builds

    .section    .text.UNDEF_SubHandler, "xa", %progbits

  .ifdef COMPILE_THUMB
	.arm
  .endif

UNDEF_SubHandler:
@ on entry, were are in UNDEF mode, without a usable stack
    stmfd   r13!, {r0}                  @ store the r0 at undef stack first
    mrs     r0, spsr                    @ get the previous mode.
    orr     r0, r0, #0xC0               @ keep interrupts disabled.
    msr     cpsr_c, r0                  @ go back to the previous mode.
  
    stmfd   r13!, {r1-r12}              @ push unbanked registers on the stack
    msr     cpsr_c, #PSR_MODE_UNDEF     @ go back into UNDEF mode, but keep IRQs off
    mov     r3,r0
    mrs     r0, spsr                    @ now save the spsr_UNDEF register
    mov     r1, r14                     @ now save the r14_UNDEF  register
    LDMFD   r13!,{r2}                   @ r2 <= previous r0

    msr     cpsr_c, r3                  @ go back to the previous mode.
    stmfd   r13!, {r0-r2}               @ push spsr_UNDEF and r14_UNDEF on stack and the old r0 value

    mov     r0, r13                     @ ARG1 of handler: the stack location of registers
    add     r1, r13, #60                @ ARG2 of handler: SYSTEM mode stack at time of exception (without saved registers = 15*4 back in stack)
    mov     r2, r14                     @ ARG3 of handler: get the link register of SYSTEM mode: r14_SYSTEM

    ldr     pc,UNDEF_Handler_Ptr        @ address of vector routine in C to jump to, never expect to return

UNDEF_Handler_Ptr:
    .word   UNDEF_Handler

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

@ the ABORT conditions on the ARM7TDMI are catastrophic, so there is no return, but it is supported in non-RTM builds

    .section    .text.ABORTP_SubHandler, "xa", %progbits

  .ifdef COMPILE_THUMB
	.arm
  .endif

ABORTP_SubHandler:
    @ on entry, were are in ABORT mode, without a usable stack

    stmfd   r13!, {r0}                  @ store the r0 at undef stack first
    mrs     r0, spsr                    @ get the previous mode.
    orr     r0, r0, #0xC0               @ keep interrupts disabled.
    msr     cpsr_c, r0                  @ go back to the previous mode.
  
    stmfd   r13!, {r1-r12}              @ push unbanked registers on the stack
    msr     cpsr_c, #PSR_MODE_ABORT     @ go back into ABORT mode, but keep IRQs off
    mov     r3,r0
    mrs     r0, spsr                    @ now save the spsr_ABORT register
    mov     r1, r14                     @ now save the r14_ABORT  register
    LDMFD   r13!,{r2}                   @ r2 <= previous r0

    msr     cpsr_c, r3                  @ go back to the previous mode.
    stmfd   r13!, {r0-r2}               @ push spsr_ABORT and r14_ABORT on stack and the old r0 value

    mov     r0, r13                     @ ARG1 of handler: the stack location of registers
    add     r1, r13, #60                @ ARG2 of handler: SYSTEM mode stack at time of exception (without saved registers = 15*4 back in stack)
    mov     r2, r14                     @ ARG3 of handler: get the link register of SYSTEM mode: r14_SYSTEM

    ldr     pc,ABORTP_Handler_Ptr       @ address of vector routine in C to jump to, never expect to return

ABORTP_Handler_Ptr:
    .word   ABORTP_Handler

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .section    .text.ABORTD_SubHandler, "xa", %progbits

  .ifdef COMPILE_THUMB
	.arm
  .endif
	
ABORTD_SubHandler:
    @ on entry, were are in ABORT mode, without a usable stack

    stmfd   r13!, {r0}                  @ store the r0 at undef stack first
    mrs     r0, spsr                    @ get the previous mode.
    orr     r0, r0, #0xC0               @ keep interrupts disabled.
    msr     cpsr_c, r0                  @ go back to the previous mode.
  
    stmfd   r13!, {r1-r12}              @ push unbanked registers on the stack
    msr     cpsr_c, #PSR_MODE_ABORT     @ go back into ABORT mode, but keep IRQs off
    mov     r3,r0
    mrs     r0, spsr                    @ now save the spsr_ABORT register
    mov     r1, r14                     @ now save the r14_ABORT  register
    LDMFD   r13!,{r2}                   @ r2 <= previous r0

    msr     cpsr_c, r3                  @ go back to the previous mode.
    stmfd   r13!, {r0-r2}               @ push spsr_ABORT and r14_ABORT on stack and the old r0 value

    mov     r0, r13                     @ ARG1 of handler: the stack location of registers
    add     r1, r13, #60                @ ARG2 of handler: SYSTEM mode stack at time of exception (without saved registers = 15*4 back in stack)
    mov     r2, r14                     @ ARG3 of handler: get the link register of SYSTEM mode: r14_SYSTEM

    ldr     pc,ABORTD_Handler_Ptr       @ address of vector routine in C to jump to, never expect to return

ABORTD_Handler_Ptr:
    .word   ABORTD_Handler

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .section    .text.HARD_Breakpoint, "xa", %progbits

  .ifdef COMPILE_THUMB
	.arm
  .endif
	
HARD_Breakpoint:
    @ on entry, were are being called from C/C++ in system mode

    msr     cpsr_c, #PSR_MODE_SYSTEM    @ go into SYSTEM mode, giving us a usable stack, but keep IRQs off
    stmfd   r13!, {r0-r12}              @ push unbanked registers on the stack
    mrs     r0, cpsr                    @ now save the cpsr_SYSTEM register
    mov     r1, r14                     @ now save the r14_SYSTEM  register
    stmfd   r13!, {r0-r1}               @ push spsr_ABORT and r14_ABORT on stack

    mov     r0, r13                     @ ARG1 of handler: the stack location of registers
    add     r1, r13, #60                @ ARG2 of handler: SYSTEM mode stack at time of call (without saved registers = 15*4 back in stack)
    mov     r2, r14                     @ ARG3 of handler: get the link register of SYSTEM mode: r14_SYSTEM

    ldr     pc,HARD_Breakpoint_Handler_Ptr @ address of vector routine in C to jump to, never expect to return

HARD_Breakpoint_Handler_Ptr:
    .word   HARD_Breakpoint_Handler

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

  .ifdef COMPILE_THUMB
	.thumb
  .endif




	
	
	
