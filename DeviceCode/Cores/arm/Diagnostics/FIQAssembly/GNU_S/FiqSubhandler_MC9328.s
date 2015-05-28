@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ Copyright (c) Microsoft Corporation.  All rights reserved.
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    .ifdef PLATFORM_ARM_MC9328
    .ifdef FIQ_SAMPLING_PROFILER

    .global FIQ_SubHandler
    
    .section SectionForProfilerBufferBegin, "a", %progbits
ProfilerBufferBegin:
    .word   0
    .global ProfilerBufferBegin
    .section SectionForProfilerBufferEnd, "a", %progbits
ProfilerBufferEnd:
    .word   0
    .global ProfilerBufferEnd

MC9328MXL_ARM_INTDISNUM            =     0x0022300C

    .section i.FIQ_SubHandler, "ax", %progbits

FIQ_SubHandler:
    @ FIQ
    @ we place the FIQ handler where it was designed to go, immediately at the end of the vector table
    @ this saves an additional 3+ clock cycle branch to the handler
    @ on entry, were are in FIQ mode, without a usable FIQ stack (r13 is trash), r14 is previous PC+4
    @ and FIQs and IRQs are disabled
    @ the banked register r8-14 are unique for this mode
    ldr     r12, [r11]
    and     r12, r12,#1
    cmp     r12, #0                     @ compare and clear the corresponding timer interrupt
    beq     Return
    mov     r12, #0
    str     r12, [r11]
    sub     r12, r14, #4                @ get true PC (prior to next instruction at time of FIQ)
    str     r12,[r8],#4
    subs    r9,r9,#1
    bne     Return
    mov     r9,#0x1
    str     r9,[r8]
    ldr     r8,=MC9328MXL_ARM_INTDISNUM @ INTDISNUM
    mov     r9,#58                      @ disable this interrupt while we do dump
    str     r9,[r8]
    ldr     r9,=ProfilerBufferEnd        @ reload the start of profile area address
    ldr     r8,=ProfilerBufferBegin
    sub     r9,r9,r8
    lsr     r9,r9,#2
Return:
    subs    pc, lr, #4                  @ return from interrupt, restoring SPSR to CPSR

    .endif
    .endif

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .end

