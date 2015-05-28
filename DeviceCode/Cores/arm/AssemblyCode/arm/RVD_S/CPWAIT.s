;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; Copyright (c) Microsoft Corporation.  All rights reserved.
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

    EXPORT  CPWAIT

    AREA ||i.CPWAIT||, CODE, READONLY ; void CPWAIT(int reg)
    ; ARM directive is only valid for ARM/THUMB processor, but not CORTEX 
    IF :DEF:COMPILE_ARM :LOR: :DEF:COMPILE_THUMB
    ARM
    ENDIF
    
    IF TargetPlatformProcessor = "PLATFORM_ARM_PXA271"


CPWAIT
    mrc     p15,0, r0,c2,c0,0       ; arbitary read of CP15
    mov     r0,r0                   ; wait for it (= NOP)
    
    mov     pc, lr                  ; return to the caller, since we are not able to make asm macro for C code

    ELSE
CPWAIT

    ENDIF

    IF :DEF:COMPILE_THUMB  
    THUMB
    ENDIF

    END


