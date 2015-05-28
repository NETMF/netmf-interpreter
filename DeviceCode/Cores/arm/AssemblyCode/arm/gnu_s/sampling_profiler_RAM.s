@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ Copyright (c) Microsoft Corporation.  All rights reserved.
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    .ifdef PLATFORM_ARM_MC9328

    .ifdef FIQ_SAMPLING_PROFILER

    .global Profiler_FIQ_Initialize

    .extern  EntryPoint
    .extern  HeapBegin
    .extern  ProfilerBufferBegin
    .extern  ProfilerBufferEnd


MC9328MXL_ARM_TIMER_2_STATUS_REG   =     0x00203014
MC9328MXL_INTC_FIQ_ENABLE_CLEAR    =     0x00223018

    .section    i.Profiler_FIQ_Initialize, "ax", %progbits

	  .arm

Profiler_FIQ_Initialize:

    stmfd   r13!, {r14}                                 @ save r14 (lr), since we will use it when branching

    msr     cpsr_c, #0xD1                               @ go into FIQ mode, but keep IRQs & FIQs off
    ldr     r8,       =ProfilerBufferBegin              @ load RAM address
    ldr     r9,       =ProfilerBufferEnd                @ load max profile word count  
    ldr     r10,       =ProfilerBufferBegin
    sub     r9,r9,r10
    lsr     r9,r9,#2
    ldr     r10,      =EntryPoint                       @ load the entry point
    str     r10,[r8],#4
    sub     r9,r9,#1
    ldr     r11,      =MC9328MXL_ARM_TIMER_2_STATUS_REG @ ARM Timer 2 Clear location 
    msr     cpsr_c, #0xDF                               @ go into SYSTEM mode, giving us a usable stack, but keep IRQs & FIQs off 

    ldmfd   r13!, {pc}                                  @ return to caller, restore link register in to pc

    .endif
    .endif
    
    .ifdef COMPILE_THUMB
	  .thumb
    .endif

