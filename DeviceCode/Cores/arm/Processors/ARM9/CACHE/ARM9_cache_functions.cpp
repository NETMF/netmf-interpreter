////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <Cores\arm\Include\cpu.h>

// the arm3.0 compiler optimizes out much of the boot strap code which causes
// the device not to boot for RTM builds (optimization level 3), adding this pragma 
// assures that the compiler will use the proper optimization level for this code
#if !defined(DEBUG)
#pragma O2
#endif

#pragma arm section code = "SectionForBootstrapOperations"

#if defined(__GNUC__)

extern "C"
{
void CPU_ARM9_FlushCaches_asm(int segCnt, int indexCnt);
void CPU_DrainWriteBuffers_asm();
void CPU_InvalidateCaches_asm();
void CPU_EnableCaches_asm();
void CPU_DisableCaches_asm();
}

void __section(SectionForBootstrapOperations) CPU_ARM9_FlushCaches(int segCnt, int indexCnt)
{
	CPU_ARM9_FlushCaches_asm(segCnt, indexCnt);
}

void __section(SectionForBootstrapOperations) CPU_DrainWriteBuffers()
{
	CPU_DrainWriteBuffers_asm();
}

void __section(SectionForBootstrapOperations) CPU_InvalidateCaches()
{
	CPU_InvalidateCaches_asm();
}

void __section(SectionForBootstrapOperations) CPU_EnableCaches()
{
    CPU_InvalidateCaches_asm();
	CPU_EnableCaches_asm();
}

void __section(SectionForBootstrapOperations) CPU_DisableCaches()
{
	CPU_DisableCaches_asm();

    CPU_FlushCaches();
}

#elif (defined(COMPILE_ARM) || defined(COMPILE_THUMB))

#pragma ARM

#define ARM9_CACHE_ASM_START() UINT32 regTmp
#define ARM9_CACHE_ASM_WAIT() \
        mrc     p15, 0, regTmp, c2, c0, 0; \
        nop \

void CPU_ARM9_FlushCaches(int segCnt, int indexCnt)
{
    ARM9_CACHE_ASM_START();
    UINT32 reg;

    for(int seg=0; seg<segCnt; seg++)
    {
        for(int index=0; index<indexCnt; index++)
        {
            reg = (index << 26) | (seg << 5);

            __asm
            {
                mcr     p15, 0, reg, c7, c10, 2; // Clean DCache single entry (using index).

                ARM9_CACHE_ASM_WAIT();
            }
        }
    }

    __asm
    {
        mov     reg,#0;
        mcr     p15, 0, reg, c7, c10, 4; // Drain Write Buffers.

        ARM9_CACHE_ASM_WAIT();

        mov     reg,#0;
        mcr     p15, 0, reg, c7, c7 , 0; // Invalidate caches.

        ARM9_CACHE_ASM_WAIT();
    }
}

void CPU_DrainWriteBuffers()
{
    ARM9_CACHE_ASM_START();
    UINT32  reg = 0;        

    __asm
    {
        mcr     p15, 0, reg, c7, c10, 4; // Drain Write Buffers.

        ARM9_CACHE_ASM_WAIT();
    } 
}

void CPU_InvalidateCaches()
{
    ARM9_CACHE_ASM_START();
    UINT32 reg;

    __asm
    {
        mov     reg,#0;
        mcr     p15, 0, reg, c7, c7 , 0; // Invalidate caches.

        ARM9_CACHE_ASM_WAIT();
    }    
}

void CPU_EnableCaches()
{
    ARM9_CACHE_ASM_START();
    UINT32 reg = 0;

    CPU_InvalidateCaches();

    __asm
    {
        mcr     p15, 0, reg, c7, c7, 0;        // Invalidate I & D Cache and Branch Target Buffer
        mrc     p15, 0, reg, c1, c0, 0;
        orr     reg, reg, #0x1000;             // Enable ICache
        orr     reg, reg, #0x0004;             // Enable DCache
        orr     reg, reg, #0x0800;             // Enable Branch Target Buffer
        mcr     p15, 0, reg, c1, c0, 0;

        ARM9_CACHE_ASM_WAIT();
    }   
}

void CPU_DisableCaches()
{
    ARM9_CACHE_ASM_START();
    UINT32 reg;

    __asm
    {
        mrc     p15, 0, reg, c1, c0, 0;
        bic     reg, reg, #0x1000;             // Disable ICache
        bic     reg, reg, #0x0004;             // Disable DCache
        bic     reg, reg, #0x0800;             // Disable Branch Target Buffer
        mcr     p15, 0, reg, c1, c0, 0;

        ARM9_CACHE_ASM_WAIT();
    }

    CPU_FlushCaches();
}

#if defined(COMPILE_THUMB)
#pragma THUMB
#endif
#pragma arm section code

#elif defined(COMPILE_THUMB2)

void CPU_ARM9_FlushCaches(int segCnt, int indexCnt)
{
}

void CPU_DrainWriteBuffers()
{
}

void CPU_InvalidateCaches()
{
}

void CPU_EnableCaches()
{
}

void CPU_DisableCaches()
{
}

#endif //#if defined(COMPILE_ARM) || defined(COMPILE_THUMB)

