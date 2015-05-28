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


//--//

#pragma arm section code = "SectionForBootstrapOperations"

UINT32* __section("SectionForBootstrapOperations") ARM9_MMU::GetL1Entry( UINT32* base, UINT32 address )
{
    return &base[address >> 20];
}

void __section("SectionForBootstrapOperations") ARM9_MMU::InitializeL1(UINT32* baseOfTTBs)
{
    UINT32* dst = baseOfTTBs;
    UINT32* end = (UINT32*)((UINT32)baseOfTTBs + ARM9_MMU::c_TTB_size);

    do
    {
        *dst++ = 0;
    } while(dst < end);
}

UINT32 __section("SectionForBootstrapOperations") ARM9_MMU::GenerateL1_Section( UINT32 address, UINT32 AP, UINT32 domain, BOOL Cachable, BOOL Buffered, BOOL Xtended )
{
    UINT32 ret;

    ret  = (address & 0xFFF00000)      ;
    ret |= (Xtended ? 1 : 0     ) << 12;
    ret |= (AP      & 0x00000003) << 10;
    ret |= (domain  & 0x0000000F) <<  5;
    ret |= (Cachable ? 1 : 0    ) <<  3;
    ret |= (Buffered ? 1 : 0    ) <<  2;
    ret |= c_MMU_L1_Section;

    return ret;
}

void __section("SectionForBootstrapOperations") ARM9_MMU::GenerateL1_Sections( UINT32* baseOfTTBs, UINT32 mappedAddress, UINT32 physAddress, INT32 size, UINT32 AP, UINT32 domain, BOOL Cachable, BOOL Buffered, BOOL Xtended )
{
    UINT32* dst = ARM9_MMU::GetL1Entry( baseOfTTBs, mappedAddress );

    do
    {
        *dst++ = ARM9_MMU::GenerateL1_Section( physAddress, AP, domain, Cachable, Buffered, Xtended);

        physAddress += ARM9_MMU::c_MMU_L1_size;
        size        -= ARM9_MMU::c_MMU_L1_size;
    } while(size > 0);
}

#if defined(__GNUC__)

extern "C"
{
void	CPU_InvalidateTLBs_asm();
void	CPU_EnableMMU_asm(void* TTB);
void	CPU_DisableMMU_asm();
BOOL	CPU_IsMMUEnabled_asm();
}

void __section("SectionForBootstrapOperations") CPU_InvalidateTLBs()
{
	CPU_InvalidateTLBs_asm();
}

void __section("SectionForBootstrapOperations") CPU_EnableMMU( void* TTB )
{
	CPU_EnableMMU_asm(TTB);
	CPU_InvalidateTLBs_asm();
}

void __section("SectionForBootstrapOperations") CPU_DisableMMU()
{
	CPU_DisableMMU_asm();
}

BOOL __section("SectionForBootstrapOperations") CPU_IsMMUEnabled()
{
    return CPU_IsMMUEnabled_asm();
}

#elif (defined(COMPILE_ARM) || defined(COMPILE_THUMB))

#pragma ARM

#define ARM9_MMU_ASM_START() UINT32 regTmp
#define ARM9_MMU_ASM_WAIT() \
        mrc     p15, 0, regTmp, c2, c0, 0; \
        nop \

void CPU_InvalidateTLBs()
{
    ARM9_MMU_ASM_START();
    UINT32 reg = 0;

    __asm
    {
        mov     reg,#0;
        mcr     p15, 0, reg, c8, c7, 0;             // Invalidate MMU TLBs.

        ARM9_MMU_ASM_WAIT();
    }
}

void CPU_EnableMMU( void* TTB )
{
    ARM9_MMU_ASM_START();
    UINT32 reg;

    __asm
    {
        mcr     p15, 0, TTB, c2, c0, 0;        // Set the TTB address location to CP15

        ARM9_MMU_ASM_WAIT();

        mrc     p15, 0, reg, c1, c0, 0;
        orr     reg, reg, #0x0001;             // Enable MMU
        mcr     p15, 0, reg, c1, c0, 0;

        ARM9_MMU_ASM_WAIT();
        
        // Note that the 2 preceeding instruction would still be prefetched
        // under the physical address space instead of in virtual address space.
    }

    CPU_InvalidateTLBs();
}

void CPU_DisableMMU()
{
    ARM9_MMU_ASM_START();
    UINT32 reg;

    __asm
    {
        mrc     p15, 0, reg, c1, c0, 0;
        bic     reg, reg, #0x0001;             // Disable MMU
        mcr     p15, 0, reg, c1, c0, 0;

        ARM9_MMU_ASM_WAIT();
        
        // Note that the 2 preceeding instruction would still be prefetched
        // under the physical address space instead of in virtual address space.
    }  
}

BOOL CPU_IsMMUEnabled()
{
    ARM9_MMU_ASM_START();
    UINT32 reg;

    __asm
    {
        mrc     p15, 0, reg, c1, c0, 0;

        ARM9_MMU_ASM_WAIT();
    }

    return (reg & 0x1);
}

#if defined(COMPILE_THUMB)
#pragma THUMB
#endif

#pragma arm section code

#elif defined(COMPILE_THUMB2)

void CPU_InvalidateTLBs()
{
}

void CPU_EnableMMU( void* TTB )
{
}

void CPU_DisableMMU()
{
}

BOOL CPU_IsMMUEnabled()
{
}

#endif // #if defined(COMPILE_ARM) || defined(COMPILE_THUMB)

