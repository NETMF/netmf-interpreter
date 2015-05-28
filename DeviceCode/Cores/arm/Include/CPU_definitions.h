////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _CPU_DEFINITIONS_H_
#define _CPU_DEFINITIONS_H_ 1


/***************************************************************************/

extern "C"
{
void CPU_ARM9_BootstrapCode();
void CPU_ARM9_FlushCaches(int segCnt, int indexCnt);
}

void CPU_ARM9_Pause();



struct ARM9_MMU
{
    static const UINT32 c_TTB_size = 0x4000;

    static const UINT32 c_MMU_L1_Fault   = 0x00;
    static const UINT32 c_MMU_L1_Coarse  = 0x11;
    static const UINT32 c_MMU_L1_Section = 0x12;
    static const UINT32 c_MMU_L1_Fine    = 0x13;
    static const UINT32 c_MMU_L1_size    = 1 << 20;

    static const UINT32 c_AP__NoAccess = 0;
    static const UINT32 c_AP__Client   = 1;
    static const UINT32 c_AP__Reserved = 2;
    static const UINT32 c_AP__Manager  = 3;

    //--//

    static UINT32* GetL1Entry( UINT32* base, UINT32 address );
    static void    InitializeL1(UINT32* baseOfTTBs);
    static UINT32  GenerateL1_Section (                     UINT32 address,                                       UINT32 AP, UINT32 domain, BOOL Cachable, BOOL Buffered, BOOL Xtended = FALSE );
    static void    GenerateL1_Sections( UINT32* baseOfTTBs, UINT32 mappedAddress, UINT32 physAddress, INT32 size, UINT32 AP, UINT32 domain, BOOL Cachable, BOOL Buffered, BOOL Xtended = FALSE );
};

#endif  // _CPU_DEFINITIONS_H_

