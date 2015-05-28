@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ Copyright (c) Microsoft Corporation.  All rights reserved.
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .global CPWAIT

    .section    i.CPWAIT, "xa", %progbits       @ void CPWAIT(int reg)

	  .arm

    .ifdef PLATFORM_ARM_PXA271

CPWAIT:
    mrc     p15,0, r0,c2,c0,0       @ arbitary read of CP15
    mov     r0,r0                   @ wait for it (= NOP)
    
    mov     pc, lr                  @ return to the caller, since we are not able to make asm macro for C code

    .else
CPWAIT:

    .endif

    .ifdef COMPILE_THUMB
	  .thumb
    .endif

    .end

