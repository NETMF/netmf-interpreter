@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ Copyright (c) Microsoft Corporation.  All rights reserved.
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@


    .section    .text, "ax", %progbits	

    .global  EntryPoint

    .global  ARM_Vectors
    .global  IRQ_SubHandler_Trampoline

    .extern  BootEntry

    .extern  UNDEF_SubHandler
    .extern  ABORTP_SubHandler
    .extern  ABORTD_SubHandler


    @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    @ these define the region to zero initialize
    .extern  Image$$ER_RAM_RW$$ZI$$Base
    .extern  Image$$ER_RAM_RW$$ZI$$Length

    @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

PSR_MODE_USER       =     0xD0
PSR_MODE_FIQ        =     0xD1
PSR_MODE_IRQ        =     0xD2
PSR_MODE_SUPERVISOR =     0xD3
PSR_MODE_ABORT      =     0xD7
PSR_MODE_UNDEF      =     0xDB
PSR_MODE_SYSTEM     =     0xDF

STACK_MODE_ABORT    =     16
STACK_MODE_UNDEF    =     16 
STACK_MODE_IRQ      =     2048

    @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .section SectionForStackBottom,     "a", %progbits
StackBottom: .word 0
    .section SectionForStackTop,        "a", %progbits
StackTop:    .word 0
    .section SectionForHeapBegin,       "a", %progbits
HeapBegin:   .word 0
    .section SectionForHeapEnd,         "a", %progbits
HeapEnd:     .word 0
    .section SectionForCustomHeapBegin, "a", %progbits
CustomHeapBegin:   .word 0
    .section SectionForCustomHeapEnd,   "a", %progbits
CustomHeapEnd:     .word 0

    .global StackBottom
    .global StackTop
    .global HeapBegin
    .global HeapEnd
    .global CustomHeapBegin
    .global CustomHeapEnd


    .section i.EntryPoint, "ax", %progbits

	.arm

EntryPoint:

ARM_Vectors:
    @ RESET
    b       VectorAreaSkip
    @ UNDEF INSTR
    ldr     pc, UNDEF_SubHandler_Trampoline
    @ SWI    
    .ifdef PLATFORM_ARM_AT91SAM9261
swi_vect:
    b   swi_vect

    .else 

     .ifdef PLATFORM_ARM_AT91SAM9RL64
swi_vect:
    b   swi_vect

    .else
    .word   0xbaadf00d
    .endif   
    .endif
    
    @ PREFETCH ABORT
    ldr     pc, ABORTP_SubHandler_Trampoline
    @ DATA ABORT
    ldr     pc, ABORTD_SubHandler_Trampoline
    @ unused
    .word   0xbaadf00d
    @ IRQ
    ldr     pc, IRQ_SubHandler_Trampoline
    @ FIQ
    .ifdef PLATFORM_ARM_AT91SAM9261
fiq_vect:
    b   fiq_vect
    .else

    .ifdef PLATFORM_ARM_AT91SAM9RL64
fiq_vect:
    b   fiq_vect
    .else
    .word   0xbaadf00d
    .endif  
    .endif
UNDEF_SubHandler_Trampoline:
    .word   UNDEF_SubHandler
    
ABORTP_SubHandler_Trampoline:
    .word   ABORTP_SubHandler

ABORTD_SubHandler_Trampoline:
    .word   ABORTD_SubHandler

IRQ_SubHandler_Trampoline:
    .word	IRQ_Handler

VectorAreaSkip:
    ldr     r0, =StackTop               @ new svc stack pointer for a full decrementing stack
    msr     cpsr_c, #PSR_MODE_ABORT     @ go into ABORT mode, interrupts off
    mov     sp, r0                      @ stack top
    sub     r0, r0, #STACK_MODE_ABORT   @ ( take the size of abort stack off )
    
    msr     cpsr_c, #PSR_MODE_UNDEF     @ go into UNDEF mode, interrupts off
    mov     sp, r0                      @ stack top - abort stack 
    sub     r0, r0, #STACK_MODE_UNDEF   @ 
    
    msr     cpsr_c, #PSR_MODE_IRQ       @ go into IRQ mode, interrupts off
    mov     sp, r0                      @ stack top - abort stack - undef stack 
    sub     r0, r0, #STACK_MODE_IRQ
   
    msr     cpsr_c, #PSR_MODE_SYSTEM    @ go into System mode, interrupts off
    mov     sp,r0                       @ stack top - abort stack - undef stack - IRQ stack



	@@ move the IRQ vector to offset 0x24 in SRAM
	.ifdef PLATFORM_ARM_AT91SAM9261
	
    mov	r0, #0x24
    ldr	r1, =IRQ_Handler
    str	r1, [r0, #0]
	.endif @@@@[IF TargetPlatformProcessor = "PLATFORM_ARM_AT91SAM9261"]

	@@ move the IRQ vector to offset 0x24 in SRAM
	.ifdef PLATFORM_ARM_AT91SAM9RL64
	
    mov	r0, #0x24
    ldr	r1, =IRQ_Handler
    str	r1, [r0, #0]
	.endif @@@@[IF TargetPlatformProcessor = "PLATFORM_ARM_AT91SAM9261"]


    @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    @ clear the Zero Initialized RAM, .bss 
    @ do this last just in case there is overlap on the load area
    @ and the execution ZI area
    @ this is not a failsafe, but helps some situations
    @ it would be nice to trap non-workable scatter files somehow - more thought required here @todo
    
    ldr     r0, =Image$$ER_RAM_RW$$ZI$$Length
    cmp     r0, #0
    beq     NoClearZI_ER_RAM
    ldr     r1, =Image$$ER_RAM_RW$$ZI$$Base
    mov     r2, #0

ClearZI_ER_RAM:

    stmia   r1!, { r2 }
    subs    r0, r0, #4                  @ 4 bytes filled per loop 
    bne     ClearZI_ER_RAM

NoClearZI_ER_RAM:


    .ifdef PLATFORM_ARM_LPC22XX     

    @ Manually copy Vector Table to Internal RAM
    mov r0,#0x40

    ldr r1, =0x81000000
    ldr r2, =Image$$ER_IRAM$$Base
loop:
    ldr  r3,[r1],#4
    subs r0,r0,#4
    str  r3,[r2],#4
    bne  loop

    .else 
    .ifdef PLATFORM_ARM_LPC24XX
    
   @ Manually copy Vector Table to Internal RAM
    mov r0,#0x40

    ldr r1, = 0xA0000000

    ldr r2, =Image$$ER_IRAM$$Base
loop:
    ldr  r3,[r1],#4
    subs r0,r0,#4
    str  r3,[r2],#4
    bne  loop
    .endif @[IF TargetPlatformProcessor = "PLATFORM_ARM_LPC24XX"]

    .endif  @[IF TargetPlatformProcessor = "PLATFORM_ARM_LPC22XX"]



    @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    @ done moving and clearing RAM, so continue on in C 


  .ifdef COMPILE_THUMB
        ldr     r0,BootEntryPointer
        bx      r0
   .else        
        ldr     pc,BootEntryPointer
   .endif        

    
BootEntryPointer:
    .word   BootEntry

    .ifdef COMPILE_THUMB
	  .thumb
    .endif

    .end

