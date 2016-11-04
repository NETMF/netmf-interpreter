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

    .global HeapBegin
    .global HeapEnd
    .global _end
    .global Reset_Handler

    .extern BootEntry
    .extern BootstrapCode


    @*************************************************************************

    /* start address for the initialization values of the .data section. 
    defined in linker script */
    .word  _sidata
    /* start address for the .data section. defined in linker script */  
    .word  _sdata
    /* end address for the .data section. defined in linker script */
    .word  _edata
    /* start address for the .bss section. defined in linker script */
    .word  _sbss
    /* end address for the .bss section. defined in linker script */
    .word  _ebss
    /* stack used for SystemInit_ExtMemCtl; always internal RAM used */

    /* start address of managed heap */
    .word HeapBegin
    /* end  address of managed heap */
    .word HeapEnd

/**
 * @brief  This is the code that gets called when the processor first
 *          starts execution following a reset event. Only the absolutely
 *          necessary set is performed, after which the application
 *          supplied main() routine is called. 
 * @param  None
 * @retval : None
*/

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
    ldr   sp, =_estack     /* set stack pointer */

    /* Copy the data segment initializers from flash to SRAM */  
    movs  r1, #0
    b  LoopCopyDataInit

    CopyDataInit:
    ldr  r3, =_sidata
    ldr  r3, [r3, r1]
    str  r3, [r0, r1]
    adds  r1, r1, #4
        
    LoopCopyDataInit:
    ldr  r0, =_sdata
    ldr  r3, =_edata
    adds  r2, r0, r1
    cmp  r2, r3
    bcc  CopyDataInit
    ldr  r2, =_sbss
    b  LoopFillZerobss
    /* Zero fill the bss segment. */  
    FillZerobss:
    movs  r3, #0
    str  r3, [r2], #4
        
    LoopFillZerobss:
    ldr  r3, = _ebss
    cmp  r2, r3
    bcc  FillZerobss

    /* Call the clock system intitialization function.*/
    bl  SystemInit 

    /* Call static constructors */
    bl __libc_init_array
    
    /* Call the application's entry point.*/
    bl  main
    bx  lr    

    .pool
    .size    Reset_Handler, . - Reset_Handler

    .balign   4

.end    
