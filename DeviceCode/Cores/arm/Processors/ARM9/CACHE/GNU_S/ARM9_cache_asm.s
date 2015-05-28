@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ Copyright (c) Microsoft Corporation.  All rights reserved.
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .global CPU_ARM9_FlushCaches_asm
    .global CPU_DrainWriteBuffers_asm
    .global CPU_InvalidateCaches_asm
    .global CPU_EnableCaches_asm
    .global CPU_DisableCaches_asm
    
    .arm

    .section    SectionForBootstrapOperations, "xa", %progbits

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@


CPU_ARM9_FlushCaches_asm:
    push    {r4,r5,r6}

    CMP       r0,#0
    mov       r5,#0
    
    BLE        SegLoopExit

SegLoop:
    CMP        r1, #0
    mov        r6, #0

    BLE        IndexLoopExit
    
IndexLoop:

        LSL     r4, r6, #26
        orr     r4, r4, r5, lsl #5

        mcr     p15, #0, r4, c7, c10, #2
        
        mrc     p15, #0, r4, c2, c0, #0
        nop 
                        
        ADD     r6, r6, #1           
        cmp     r6, r1  
        BLT     IndexLoop   
IndexLoopExit:
        
    ADD     r5, r5, #1           
    cmp     r5, r0
    BLT     SegLoop   

SegLoopExit:
    mov     r4, #0
    mcr     p15, #0, r4, c7, c10, #4
    
    mrc     p15, #0, r1, c2, c0, #0
    nop         
    
    mcr     p15, #0, r4, c7, c7,# 0

    mrc     p15, #0, r4, c2, c0, #0
    nop  
   
    pop    {r4,r5,r6}
    mov     pc, lr

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

CPU_DrainWriteBuffers_asm:
    mov     r0, #0
    mcr     p15, 0, r0, c7, c10, 4
    mrc     p15, 0, r1, c2, c0, 0
    nop     

    mov     pc, lr

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

CPU_InvalidateCaches_asm:
    mov     r0, #0
    mcr     p15, 0, r0, c7, c7, 0   
    mrc     p15, 0, r1, c2, c0, 0
    nop 

    mov     pc, lr
    
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    
CPU_EnableCaches_asm:
    mcr     p15, 0, r0, c7, c7, 0   @ Invalidate I & D Cache and Branch Target Buffer
    mrc     p15, 0, r0, c1, c0, 0
    orr     r0, r0, #0x1000         @ Enable ICache
    orr     r0, r0, #0x0004         @ Enable DCache  
    orr     r0, r0, #0x0800         @ Enable Branch Target Buffer 
    mcr     p15, 0, r0, c1, c0, 0   
    mrc     p15, 0, r1, c2, c0, 0
    nop 

    mov     pc, lr    

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    
CPU_DisableCaches_asm:
    mrc     p15, 0, r0, c1, c0, 0   
    bic     r0, r0, #0x1000         @ Disable ICache
    bic     r0, r0, #0x0004         @ Disable DCache  
    bic     r0, r0, #0x0800         @ Disable Branch Target Buffer 
    mcr     p15, 0, r0, c1, c0, 0
    mrc     p15, 0, r1, c2, c0, 0
    nop 

    mov     pc, lr   
    
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            
    .ifdef COMPILE_THUMB
    .thumb
    .endif
    
    
    
