@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ Copyright (c) Microsoft Corporation.  All rights reserved.
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .section VectorsTrampolines, "ax", %progbits

    .ifdef PLATFORM_ARM_AT91SAM9261
    .extern  EntryPoint

    .endif

    .ifdef PLATFORM_ARM_AT91SAM9RL64
    .extern  EntryPoint
    .global ARM_Vectors

    .else

    .extern  EntryPoint
    .extern  UNDEF_SubHandler
    .extern  ABORTP_SubHandler
    .extern  ABORTD_SubHandler

     .endif
    
	  .arm
	
ARM_Vectors:


	.ifdef PLATFORM_ARM_AT91SAM9261 

RESET_VECTOR:
    ldr      pc, reset_vct

UNDEF_VECTOR:
    b       UNDEF_VECTOR

SWI_VECTOR:
    b       SWI_VECTOR

PREFETCH_VECTOR:
    b       PREFETCH_VECTOR

DATA_VECTOR:
    b       DATA_VECTOR

USED_VECTOR:
    b       USED_VECTOR

IRQ_VECTOR:
    ldr    pc, irq_vct

FIQ_VECTOR:
    b       FIQ_VECTOR

reset_vct:
    .word  EntryPoint

irq_vct:
    .word  0xbaadf00d
   .endif



   	.ifdef PLATFORM_ARM_AT91SAM9RL64 

RESET_VECTOR:
    ldr      pc, reset_vct

UNDEF_VECTOR:
    b       UNDEF_VECTOR

SWI_VECTOR:
    b       SWI_VECTOR

PREFETCH_VECTOR:
    b       PREFETCH_VECTOR

DATA_VECTOR:
    b       DATA_VECTOR

USED_VECTOR:
    b       USED_VECTOR

IRQ_VECTOR:
    ldr    pc, irq_vct

FIQ_VECTOR:
    b       FIQ_VECTOR

reset_vct:
    .word    EntryPoint
irq_vct:
    .word    0xbaadf00d

;;;;;;;;

	.else

    @ RESET
    b       VectorAreaSkip

    @ UNDEF INSTR
    ldr     pc, UNDEF_SubHandler_Trampoline

    @ SWI
    .word   0xbaadf00d

    @ PREFETCH ABORT
    ldr     pc, ABORTP_SubHandler_Trampoline

    @ DATA ABORT
    ldr     pc, ABORTD_SubHandler_Trampoline

    @ unused
    .word   0xbaadf00d

    @ IRQ
    ldr     pc, IRQ_SubHandler_Trampoline

    @ FIQ
    .word   0xbaadf00d

UNDEF_SubHandler_Trampoline:
    .word   UNDEF_SubHandler
    
ABORTP_SubHandler_Trampoline:
    .word   ABORTP_SubHandler

ABORTD_SubHandler_Trampoline:
    .word   ABORTD_SubHandler

IRQ_SubHandler_Trampoline:
      .word  IRQ_Handler  

VectorAreaSkip:
	  b		EntryPoint


    .endif @@@ .ifdef PLATFORM_ARM_AT91SAM9261 || PLATFORM_ARM_AT91SAM9RL64


    .ifdef COMPILE_THUMB
		.thumb
    .endif

    .end

