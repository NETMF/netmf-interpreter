@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@
@  Licensed under the Apache License, Version 2.0 (the "License")@
@  you may not use this file except in compliance with the License.
@  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
@
@  Copyright (c) Microsoft Corporation. All rights reserved.
@  Implementation for STM32: Copyright (c) Oberon microsystems, Inc.
@
@  CORTEX-M3 Standard Entry Code 
@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@
@ This version of the "boot entry" support is used when the application is 
@ loaded or otherwise started from a bootloader. (e.g. this application isn't 
@ a boot loader). More specifically this version is used whenever the application
@ does NOT run from the power on reset vector because some other code is already
@ there.
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@
    .syntax unified
    .arch armv7-m
    .thumb
    
    .global  EntryPoint

    .global StackBottom
    .global StackTop
    .global HeapBegin
    .global HeapEnd
    .global CustomHeapBegin
    .global CustomHeapEnd
    .global __initial_sp
    .global Reset_Handler

    .extern BootEntry
    .extern BootstrapCode


    @*************************************************************************

    .section SectionForStackBottom, "w", %nobits
StackBottom:
       .word 0

    .section SectionForStackTop, "w", %nobits
__initial_sp:
StackTop:
      .word 0

    .section SectionForHeapBegin, "w", %nobits
HeapBegin:
     .word 0

    .section SectionForHeapEnd, "w", %nobits
HeapEnd:
    .word 0

    .section SectionForCustomHeapBegin, "w", %nobits
CustomHeapBegin:
    .word 0

    .section SectionForCustomHeapEnd, "w", %nobits
CustomHeapEnd:
    .word 0

.section i.EntryPoint, "ax", %progbits

   @ENTRY

EntryPoint:

@ The first word has a dual role:
@ - It is the entry point of the application loaded or discovered by
@   the bootloader and therfore must be valid executable code
@ - it contains a signature word used to identify application blocks
@   in TinyBooter (see: Tinybooter_ProgramWordCheck() for more details )
@ * The additional entries in this table are completely ignored and
@   remain for backwards compatibility. Since the boot loader is hard
@   coded to look for the signature, half of which is an actual relative
@   branch instruction, removing the unused entries would require all
@   bootloaders to be updated as well. [sic.]
@   [ NOTE:
@     In the next major release where we can afford to break backwards
@     compatibility this will almost certainly change, as the whole
@     init/startup for NETMF is overly complex. The various tools used
@     for building the CLR have all come around to supporting simpler
@     init sequences we should leverage directly
@   ]
@ The actual word used is 0x2000E00C

    b.n       Reset_Handler @ 0xE00C - Force short (16-bit) encoding
    .hword    0x2000        @ Booter signature is 0x2000E00C
    .word     0 @ [ UNUSED ]
    .word     0 @ [ UNUSED ]
    .word     0 @ [ UNUSED ]
    .word     0 @ [ UNUSED ]
    .word     0 @ [ UNUSED ]
    .word     0 @ [ UNUSED ]

Reset_Handler:
    @@ reload the stack pointer as there's no returning to the loader
    ldr     sp, =__initial_sp
    bl  BootstrapCode
    b   BootEntry

    .pool
    .size    Reset_Handler, . - Reset_Handler

    .balign   4

.end    
