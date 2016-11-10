__weak ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//
struct USB_CONTROLLER_STATE; 

__weak USB_CONTROLLER_STATE *CPU_USB_GetState( int Controller )
{
    return NULL;
}

__weak HRESULT CPU_USB_Initialize( int Controller )
{
    return S_OK;
}

__weak HRESULT CPU_USB_Uninitialize( int Controller )
{
    return S_OK;
}

__weak BOOL CPU_USB_StartOutput( USB_CONTROLLER_STATE* State, int endpoint )
{
    return FALSE;
}

__weak BOOL CPU_USB_RxEnable( USB_CONTROLLER_STATE* State, int endpoint )
{
    return FALSE;
}

__weak BOOL CPU_USB_GetInterruptState()
{
    return FALSE;
}

__weak BOOL CPU_USB_ProtectPins( int Controller, BOOL On )
{
    return FALSE;
}
