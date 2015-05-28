@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ Copyright (c) Microsoft Corporation.  All rights reserved.
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .extern  UNDEF_SubHandler
    .extern  ABORTP_SubHandler
    .extern  ABORTD_SubHandler

    .global  ARM_Vectors

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

   .section VectorsTrampolines, "xa", %progbits
   
	 .arm

ARM_Vectors:

    @ RESET
RESET_VECTOR:
    b       UNDEF_VECTOR

    @ UNDEF INSTR
UNDEF_VECTOR:
    ldr     pc, UNDEF_SubHandler_Trampoline

    @ SWI
SWI_VECTOR:
    .word   0xbaadf00d

    @ PREFETCH ABORT
PREFETCH_VECTOR:
    ldr     pc, ABORTP_SubHandler_Trampoline

    @ DATA ABORT
DATA_VECTOR:
    ldr     pc, ABORTD_SubHandler_Trampoline

    @ unused
USED_VECTOR:
    .word   0xbaadf00d

    @ IRQ
IRQ_VECTOR:
    ldr     pc, IRQ_SubHandler_Trampoline

    @ FIQ
    @ we place the FIQ handler where it was designed to go, immediately at the end of the vector table
    @ this saves an additional 3+ clock cycle branch to the handler
FIQ_Handler:
    .ifdef FIQ_SAMPLING_PROFILER
    ldr     pc,FIQ_SubHandler_Trampoline    

FIQ_SubHandler_Trampoline:
    .word   FIQ_SubHandler
    .endif

UNDEF_SubHandler_Trampoline:
    .word   UNDEF_SubHandler

ABORTP_SubHandler_Trampoline:
    .word   ABORTP_SubHandler

ABORTD_SubHandler_Trampoline:
    .word   ABORTD_SubHandler


        @ route the normal interupt handler to the proper lowest level driver
IRQ_SubHandler_Trampoline: 
    .word  	IRQ_Handler

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .ifdef COMPILE_THUMB
	  .thumb
    .endif

    .end
    
    
    
    

