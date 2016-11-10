////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

//--//

__weak BOOL Buttons_Initialize()
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return TRUE;
}

__weak BOOL Buttons_Uninitialize()
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return TRUE;
}

__weak BOOL Buttons_RegisterStateChange( UINT32 ButtonsPressed, UINT32 ButtonsReleased )
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return FALSE;
}

__weak BOOL Buttons_GetNextStateChange( UINT32& ButtonsPressed, UINT32& ButtonsReleased )
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return FALSE;
}

__weak UINT32 Buttons_CurrentState()
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return 0;
}

__weak UINT32 Buttons_HW_To_Hal_Button( UINT32 HW_Buttons )
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return 0;
}

__weak UINT32 Buttons_CurrentHWState()
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return 0;
}

