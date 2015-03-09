;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; Copyright (c) Microsoft Corporation.  All rights reserved.
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;


    AREA    |.text|, CODE, READONLY

    EXPORT  EntryPoint

    EXPORT  ARM_Vectors
    EXPORT  IRQ_SubHandler_Trampoline

    IMPORT  BootEntry

    IMPORT  UNDEF_SubHandler
    IMPORT  ABORTP_SubHandler
    IMPORT  ABORTD_SubHandler


    PRESERVE8

    IMPORT  IRQ_Handler        ; stubbed version with assert

    ;*************************************************************************

    ; these define the region to zero initialize
    IMPORT  ||Image$$ER_RAM_RW$$ZI$$Base||
    IMPORT  ||Image$$ER_RAM_RW$$ZI$$Length||
    
    ;*************************************************************************

PSR_MODE_USER       EQU     0xD0
PSR_MODE_FIQ        EQU     0xD1
PSR_MODE_IRQ        EQU     0xD2
PSR_MODE_SUPERVISOR EQU     0xD3
PSR_MODE_ABORT      EQU     0xD7
PSR_MODE_UNDEF      EQU     0xDB
PSR_MODE_SYSTEM     EQU     0xDF

STACK_MODE_ABORT    EQU     16
STACK_MODE_UNDEF    EQU     16 
STACK_MODE_IRQ      EQU     2048
    
    ;*************************************************************************
    
	AREA SectionForStackBottom, DATA, NOINIT
StackBottom DCD 0
    AREA SectionForStackTop,    DATA, NOINIT
StackTop    DCD 0
    AREA SectionForHeapBegin,   DATA, NOINIT
HeapBegin   DCD 0
    AREA SectionForHeapEnd,     DATA, NOINIT
HeapEnd     DCD 0
    AREA SectionForCustomHeapBegin,   DATA, NOINIT
CustomHeapBegin   DCD 0
    AREA SectionForCustomHeapEnd,     DATA, NOINIT
CustomHeapEnd     DCD 0

    EXPORT StackBottom
    EXPORT StackTop
    EXPORT HeapBegin
    EXPORT HeapEnd
    EXPORT CustomHeapBegin
    EXPORT CustomHeapEnd


    AREA ||i.EntryPoint||, CODE, READONLY
    ; ARM directive is only valid for ARM/THUMB processor, but not CORTEX 
    IF :DEF:COMPILE_ARM :LOR: :DEF:COMPILE_THUMB
    ARM
    ENDIF
    
    ENTRY

EntryPoint

ARM_Vectors                
    ; RESET
    b       VectorAreaSkip
    ; UNDEF INSTR
    ldr     pc, UNDEF_SubHandler_Trampoline
    ; SWI
    IF TargetPlatformProcessor = "PLATFORM_ARM_AT91SAM9261" || TargetPlatformProcessor = "PLATFORM_ARM_AT91SAM9RL64"
swi_vect
    B   swi_vect
    ELSE
    DCD     0xbaadf00d
    ENDIF
    ; PREFETCH ABORT
    ldr     pc, ABORTP_SubHandler_Trampoline
    ; DATA ABORT
    ldr     pc, ABORTD_SubHandler_Trampoline
    ; unused
    DCD     0xbaadf00d
    ; IRQ
    ldr     pc, IRQ_SubHandler_Trampoline

    ; FIQ
    IF TargetPlatformProcessor = "PLATFORM_ARM_AT91SAM9261" || TargetPlatformProcessor = "PLATFORM_ARM_AT91SAM9RL64"

fiq_vect
    B   fiq_vect
    ELSE
    DCD     0xbaadf00d
    ENDIF

UNDEF_SubHandler_Trampoline
    DCD     UNDEF_SubHandler
    
ABORTP_SubHandler_Trampoline
    DCD     ABORTP_SubHandler

ABORTD_SubHandler_Trampoline
    DCD     ABORTD_SubHandler

IRQ_SubHandler_Trampoline
    DCD  	IRQ_Handler

VectorAreaSkip
   
    ldr     r0, =StackTop               ; new svc stack pointer for a full decrementing stack
    msr     cpsr_c, #PSR_MODE_ABORT     ; go into ABORT mode, interrupts off
    mov     sp, r0                      ; stack top
    sub     r0, r0, #STACK_MODE_ABORT   ; ( take the size of abort stack off )
    
    msr     cpsr_c, #PSR_MODE_UNDEF     ; go into UNDEF mode, interrupts off
    mov     sp, r0                      ; stack top - abort stack
    sub     r0, r0, #STACK_MODE_UNDEF   ; 
    
    msr     cpsr_c, #PSR_MODE_IRQ       ; go into IRQ mode, interrupts off
    mov     sp, r0                      ; stack top - abort stack - undef stack
    sub     r0, r0, #STACK_MODE_IRQ
   
    msr     cpsr_c, #PSR_MODE_SYSTEM    ; go into System mode, interrupts off
    mov     sp,r0                       ; stack top - abort stack - undef stack - IRQ stack


	; move the IRQ vector to offset 0x24 in SRAM
	IF TargetPlatformProcessor = "PLATFORM_ARM_AT91SAM9261" || TargetPlatformProcessor = "PLATFORM_ARM_AT91SAM9RL64"
	
    mov	r0, #0x24
    ldr	r1, =IRQ_Handler
    str	r1, [r0, #0]
	ENDIF ;[IF TargetPlatformProcessor = "PLATFORM_ARM_AT91SAM9261"]
	


    ;*************************************************************************
    ; clear the Zero Initialized RAM, .bss 
    ; do this last just in case there is overlap on the load area
    ; and the execution ZI area
    ; this is not a failsafe, but helps some situations
    ; it would be nice to trap non-workable scatter files somehow - more thought required here @todo
    
    ldr     r0, =||Image$$ER_RAM_RW$$ZI$$Length||
    cmp     r0, #0
    beq     NoClearZI_ER_RAM
    ldr     r1, =||Image$$ER_RAM_RW$$ZI$$Base||
    mov     r2, #0

ClearZI_ER_RAM

    stmia   r1!, { r2 }
    subs    r0, r0, #4                  ; 4 bytes filled per loop 
    bne     ClearZI_ER_RAM

NoClearZI_ER_RAM

    IF TargetPlatformProcessor = "PLATFORM_ARM_LPC22XX" :LOR: TargetPlatformProcessor = "PLATFORM_ARM_LPC24XX"
    IMPORT ||Image$$ER_IRAM$$Base||
    IF TargetLocation="FLASH"
    IMPORT ||Image$$ER_FLASH$$Base|| 
    ELSE
    IMPORT ||Image$$ER_RAM$$Base|| 
    ENDIF
    ;; Manually copy Vector Table to Internal RAM
    mov r0,#0x40

    IF TargetPlatformProcessor = "PLATFORM_ARM_LPC22XX"

;; have to verify later, whether this hardcode number is same for LPC22xx and LPC24xx
    ldr r1, =0x81000000
    ELSE
    IF TargetLocation="FLASH"
    ldr r1, =||Image$$ER_FLASH$$Base||
    ELSE
    ldr r1, =||Image$$ER_RAM$$Base||
    ENDIF

    ENDIF
    ldr r2, =||Image$$ER_IRAM$$Base||

loop
    ldr  r3,[r1],#4
    subs r0,r0,#4
    str  r3,[r2],#4
    bne  loop
    ENDIF ;[IF TargetPlatformProcessor = "PLATFORM_ARM_LPC22XX"]

    ;*************************************************************************
    ; done moving and clearing RAM, so continue on in C 

    b       BootEntry

    IF :DEF:COMPILE_THUMB  
    ARM
    ENDIF
  

    END
