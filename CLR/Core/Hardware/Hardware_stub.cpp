////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_HW_Hardware::CreateInstance()
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT CLR_HW_Hardware::Hardware_Initialize()
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    TINYCLR_NOCLEANUP_NOLABEL();
}

//--//

HRESULT CLR_HW_Hardware::DeleteInstance()
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();
    TINYCLR_NOCLEANUP_NOLABEL();
}

void CLR_HW_Hardware::Hardware_Cleanup()
{
    NATIVE_PROFILE_CLR_HARDWARE();
}

//--//

void CLR_HW_Hardware::PrepareForGC()
{
    NATIVE_PROFILE_CLR_HARDWARE();
}

void CLR_HW_Hardware::ProcessActivity()
{
    NATIVE_PROFILE_CLR_HARDWARE();
}


//--//

void CLR_HW_Hardware::Screen_Flush( CLR_GFX_Bitmap& bitmap, CLR_UINT16 x, CLR_UINT16 y, CLR_UINT16 width, CLR_UINT16 height )
{
    NATIVE_PROFILE_CLR_HARDWARE();
}

//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

