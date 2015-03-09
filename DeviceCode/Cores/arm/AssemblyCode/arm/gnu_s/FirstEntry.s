@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ Copyright (c) Microsoft Corporation.  All rights reserved.
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@


    .global  EntryPoint
    .global  PreStackInit_Exit_Pointer

    .extern  PreStackInit


    .ifdef HAL_REDUCESIZE

    .extern BootEntryLoader

    .else
 
    .extern BootEntry

    .endif

    .extern  BootstrapCode
    .extern  ARM_Vectors         @ Even if we don't use this symbol, it's required by the linker to properly include the Vector trampolines.

@   PRESERVE8

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

PSR_MODE_USER       =     0xD0
PSR_MODE_FIQ        =     0xD1
PSR_MODE_IRQ        =     0xD2
PSR_MODE_SUPERVISOR =     0xD3
PSR_MODE_ABORT      =     0xD7
PSR_MODE_UNDEF      =     0xDB
PSR_MODE_SYSTEM     =     0xDF


STACK_MODE_ABORT    =     16
STACK_MODE_UNDEF    =     16 
    .ifdef FIQ_SAMPLING_PROFILER
STACK_MODE_FIQ      =     2048
    .else
STACK_MODE_IRQ      =     2048
    .endif

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .section SectionForStackBottom,       "a", %progbits
StackBottom:
    .word   0
    .section SectionForStackTop,          "a", %progbits
StackTop:
    .word   0
    .section SectionForHeapBegin,         "a", %progbits
HeapBegin:
    .word   0
    .section SectionForHeapEnd,           "a", %progbits
HeapEnd:
    .word   0
    .section SectionForCustomHeapBegin,   "a", %progbits
CustomHeapBegin:
    .word   0
    .section SectionForCustomHeapEnd,     "a", %progbits
CustomHeapEnd:
    .word   0


    .global StackBottom
    .global StackTop
    .global HeapBegin
    .global HeapEnd
    .global CustomHeapBegin
    .global CustomHeapEnd


    .section i.EntryPoint, "xa", %progbits

    .arm

EntryPoint:


    .ifdef  HAL_REDUCESIZE
    .ifndef TARGETLOCATION_RAM
    @ -----------------------------------------------
    @ ADD BOOT MARKER HERE IF YOU NEED ONE
    @ -----------------------------------------------
    .ifdef  PLATFORM_ARM_LPC22XX
     orr     r0, pc,#0x80000000
     mov     pc, r0
    .endif

    .endif
    .endif


    @ designed to be a vector area for ARM7
    @ RESET
    @ keep PortBooter signature the same
    
    msr     cpsr_c, #PSR_MODE_SYSTEM    @ go into System mode, interrupts off

    @--------------------------------------------------------------------------------
    @ ALLOW pre-stack initilization
    @ Use the relative address of PreStackInit (because memory may need to be remapped)
    @--------------------------------------------------------------------------------


    .ifdef PLATFORM_ARM_LPC24XX
    @ LPC24XX on chip bootloader requires valid checksum in internal Flash
    @ location 0x14 ( ARM reserved vector ).
    B       PreStackEntry
    .space 20
    .endif


PreStackEntry:
    B       PreStackInit


PreStackInit_Exit_Pointer:

    ldr     r0, =StackTop               @ new SYS stack pointer for a full decrementing stack

    msr     cpsr_c, #PSR_MODE_ABORT     @ go into ABORT mode, interrupts off
    mov     sp, r0                      @ stack top
    sub     r0, r0, #STACK_MODE_ABORT   @ ( take the size of abort stack off )
    
    msr     cpsr_c, #PSR_MODE_UNDEF     @ go into UNDEF mode, interrupts off
    mov     sp, r0                      @ stack top - abort stack
    sub     r0, r0, #STACK_MODE_UNDEF   @ 
    
    
    .ifdef FIQ_SAMPLING_PROFILER        
    msr     cpsr_c, #PSR_MODE_FIQ       @ go into FIQ mode, interrupts off
    mov     sp, r0                      @ stack top - abort stack - undef stack
    sub     r0, r0, #STACK_MODE_FIQ 
    .endif

    msr     cpsr_c, #PSR_MODE_IRQ       @ go into IRQ mode, interrupts off
    mov     sp, r0                      @ stack top - abort stack - undef stack (- FIQ stack) 
    sub     r0, r0, #STACK_MODE_IRQ
   
    msr     cpsr_c, #PSR_MODE_SYSTEM    @ go into System mode, interrupts off
    mov     sp,r0                       @ stack top - abort stack - undef stack (- FIQ stack) - IRQ stack


        @******************************************************************************************
        @ This ensures that we execute from the real location, regardless of any remapping scheme *
        @******************************************************************************************

    ldr     pc, EntryPoint_Restart_Pointer
EntryPoint_Restart_Pointer:
    .word   EntryPoint_Restart
EntryPoint_Restart:

        @*********************************************************************

    bl      BootstrapCode

    ldr     r0, =StackTop               @ new svc stack pointer for a full decrementing stack
    
    .ifdef FIQ_SAMPLING_PROFILER
    sub     sp, r0, #STACK_MODE_FIQ     @
    .else
    sub     sp, r0, #STACK_MODE_IRQ     @
    .endif


    .ifdef HAL_REDUCESIZE

  .ifdef COMPILE_THUMB
        ldr   r0,BootEntryLoaderPointer
        bx      r0
   .else        
        ldr   pc,BootEntryLoaderPointer
    .endif        

BootEntryLoaderPointer:
        .word   BootEntryLoader


   .else


  .ifdef COMPILE_THUMB
        ldr     r0,BootEntryPointer
        bx      r0
   .else        
        ldr     pc,BootEntryPointer
    .endif        
BootEntryPointer:
        .word   BootEntry
    .endif

  .ifdef COMPILE_THUMB
    .thumb
  .endif

    .end

