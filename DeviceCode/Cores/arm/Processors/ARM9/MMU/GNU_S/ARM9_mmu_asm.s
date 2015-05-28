@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ Copyright (c) Microsoft Corporation.  All rights reserved.
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

	.global CPU_InvalidateTLBs_asm
	.global CPU_EnableMMU_asm
	.global CPU_DisableMMU_asm
	.global CPU_IsMMUEnabled_asm
	
	.arm

    .section    SectionForBootstrapOperations, "xa", %progbits

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

CPU_InvalidateTLBs_asm: 
    mov		r0, #0
    mcr		p15, 0, r0, c8, c7, 0
    mrc     p15, 0, r1, c2, c0, 0
    nop          

    mov     pc, lr

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

CPU_EnableMMU_asm:
	mcr     p15, 0, r0, c2, c0, 0		@ Set the TTB address location to CP15
    mrc     p15, 0, r1, c2, c0, 0
    nop  
    mrc     p15, 0, r1, c1, c0, 0
    orr     r1, r1, #0x0001             @ Enable MMU
    mcr     p15, 0, r1, c1, c0, 0
    mrc     p15, 0, r1, c2, c0, 0
    nop 
        
    @ Note that the 2 preceeding instruction would still be prefetched
    @ under the physical address space instead of in virtual address space. 
    mov     pc, lr

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

CPU_DisableMMU_asm:
    mrc     p15, 0, r0, c1, c0, 0
    bic     r0, r0, #0x0001           @ Disable MMU
    mcr     p15, 0, r0, c1, c0, 0
    mrc     p15, 0, r0, c2, c0, 0
    nop 
        
    @ Note that the 2 preceeding instruction would still be prefetched
    @ under the physical address space instead of in virtual address space.
    mov     pc, lr
    
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    
CPU_IsMMUEnabled_asm:
    mrc     p15, 0, r0, c1, c0, 0
    mrc     p15, 0, r1, c2, c0, 0
    nop 
    
    and		r0, r0, #1
    mov     pc, lr    
	
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
	        
	.ifdef COMPILE_THUMB
	.thumb
	.endif
	
	
	
