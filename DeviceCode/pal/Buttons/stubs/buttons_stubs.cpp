////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

//--//

BOOL Buttons_Initialize()
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return TRUE;
}

BOOL Buttons_Uninitialize()
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return TRUE;
}

BOOL Buttons_RegisterStateChange( UINT32 ButtonsPressed, UINT32 ButtonsReleased )
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return FALSE;
}

BOOL Buttons_GetNextStateChange( UINT32& ButtonsPressed, UINT32& ButtonsReleased )
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return FALSE;
}

UINT32 Buttons_CurrentState()
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return 0;
}

UINT32 Buttons_HW_To_Hal_Button( UINT32 HW_Buttons )
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return 0;
}

UINT32 Buttons_CurrentHWState()
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return 0;
}

