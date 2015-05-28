@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ Copyright (c) Microsoft Corporation.  All rights reserved.
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

	.global IRQ_LOCK_Release_asm
	.global IRQ_LOCK_Probe_asm
	.global IRQ_LOCK_GetState_asm
	.global IRQ_LOCK_ForceDisabled_asm
	.global IRQ_LOCK_ForceEnabled_asm
	.global IRQ_LOCK_Disable_asm
	.global IRQ_LOCK_Restore_asm
	
	.arm

    .section   SectionForFlashOperations,  "xa", %progbits

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

IRQ_LOCK_Release_asm:
    mrs     r0, CPSR
    bic     r1, r0, #0x80
    msr     CPSR_c, r1
    
    mov     pc, lr

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

IRQ_LOCK_Probe_asm:
    mrs		r0, CPSR
    bic		r1, r0, #0x80
    msr		CPSR_c, r1
    msr		CPSR_c, r0

    mov     pc, lr

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

IRQ_LOCK_GetState_asm:
    mrs		r0, CPSR
    mvn		r0, r0
    and		r0, r0, #0x80
    
    mov     pc, lr
    
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
  
IRQ_LOCK_ForceDisabled_asm: 
    mrs		r0, CPSR
    orr		r1, r0, #0x80
    msr		CPSR_c, r1
    mvn		r0, r0
    and		r0, r0, #0x80
  
    mov     pc, lr    

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

IRQ_LOCK_ForceEnabled_asm:
    mrs		r0, CPSR
    bic		r1, r0, #0x80
    msr		CPSR_c, r1
    mvn		r0, r0
    and		r0, r0, #0x80
  
	mov     pc, lr   
	
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
	    
IRQ_LOCK_Disable_asm:
    mrs		r0, CPSR
    orr		r1, r0, #0x80
    msr		CPSR_c, r1

	mov     pc, lr   
	
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

IRQ_LOCK_Restore_asm:
	mrs		r0, CPSR
    bic		r0, r0, #0x80
    msr		CPSR_c, r0

	mov     pc, lr   
	
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
	        
	.ifdef COMPILE_THUMB
	.thumb
	.endif
	
	
	
	
