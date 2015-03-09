////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <Cores\arm\Include\cpu.h>

///////////////////////////////////////////////////////////////////////////////

// the arm3.0 compiler optimizes out much of the boot strap code which causes
// the device not to boot for RTM builds (optimization level 3), adding this pragma 
// assures that the compiler will use the proper optimization level for this code
#if !defined(DEBUG)
#pragma O2
#endif

///////////////////////////////////////////////////////////////////////////////


#pragma arm section code = "SectionForBootstrapOperations"


#if defined(COMPILE_ARM) || defined(COMPILE_THUMB)

#pragma ARM


#define ARM9_BOOTSTRAP_ASM_WAIT()   \
    mrc     p15, 0, reg, c2, c0, 0; \
    nop \


void __section(SectionForBootstrapOperations) CPU_ARM9_BootstrapCode()
{
    UINT32 reg;

    //--//

    reg = 0x2001;

#ifdef __GNUC__
	asm("MCR p15, 0, %0, c15, c1, 0" :: "r" (reg));
	asm("NOP");
	asm("NOP");
	asm("NOP");
#else
    __asm
    {
        mcr     p15, 0, reg, c15, c1, 0 // Test register.
        ARM9_BOOTSTRAP_ASM_WAIT()
    }
#endif

    //--//

    //
    // MMU                              : disabled
    // Alignment fault checking         : disabled.
    // Data Cache                       : disabled.
    // Instruction Cache                : disabled.
    // Write Buffer                     : enabled.
    // Exception handlers               : 32bits
    // 26-bit address exception checking: disabled.
    // Late Abort Model selected.
    // Configured for little-endian memory system.
    //
#if !defined(BIG_ENDIAN)
    reg = 0x78;
#else
    reg = 0xF8;
#endif

#ifdef __GNUC__
	asm("MCR p15, 0, %0, c1, c0, 0" :: "r" (reg));
	asm("NOP");
	asm("NOP");
	asm("NOP");
#else
    __asm
    {
        mcr     p15, 0, reg, c1, c0, 0 // Control register.
        ARM9_BOOTSTRAP_ASM_WAIT()
    }
#endif

    //--//

    reg = 0;

#ifdef __GNUC__
	asm("MCR p15, 0, %0, c8, c7, 0" :: "r" (reg));		// Invalidate all TLBs.
	asm("MCR p15, 0, %0, c7, c7, 0" :: "r" (reg));		// Invalidate all caches.
	asm("MCR p15, 0, %0, c7, c10, 4" :: "r" (reg));		// Drain write buffers.
	asm("NOP");
	asm("NOP");
	asm("NOP");
#else
    __asm
    {
        mcr     p15, 0, reg, c8, c7, 0  // Invalidate all TLBs.
        mcr     p15, 0, reg, c7, c7, 0  // Invalidate all caches.
        mcr     p15, 0, reg, c7, c10, 4 // Drain write buffers.
        ARM9_BOOTSTRAP_ASM_WAIT()
    }
#endif

    //--//

    reg = 0xFFFFFFFF;

#ifdef __GNUC__
	asm("MCR p15, 0, %0, c3, c0, 0" :: "r" (reg));
	asm("NOP");
	asm("NOP");
	asm("NOP");
#else
    __asm
    {
        mcr     p15, 0, reg, c3, c0, 0  // Domain access control.
        ARM9_BOOTSTRAP_ASM_WAIT()
    }
#endif
}

#if defined(COMPILE_THUMB)
#pragma THUMB
#endif

#elif defined(COMPILE_THUMB2)
void CPU_ARM9_BootstrapCode()
{
}

#endif

#pragma arm section code

