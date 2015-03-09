////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "sddl.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

using namespace Microsoft::SPOT::Emulator;

/* Compiling C++-CLI code with structs without definitions yields linker warnings. As this type isn't exposed
 * to managed code, suppress the warning by defining an empty struct definition. */
struct USB_DYNAMIC_CONFIGURATION {};

int USB_GetControllerCount()
{
    // This will prevent all access to USB calls from managed code during emulation
    return 0;
}

BOOL USB_Initialize( int Controller )
{
    return EmulatorNative::GetIUsbDriver()->Initialize();    
}

int USB_Configure( int Controller, const USB_DYNAMIC_CONFIGURATION *config )
{
    return 0;
}

const USB_DYNAMIC_CONFIGURATION * USB_GetConfiguration( int Controller )
{
    return NULL;
}

BOOL USB_Uninitialize( int Controller )
{
    return EmulatorNative::GetIUsbDriver()->Uninitialize();    
}

BOOL USB_OpenStream( int UsbStream, int writeEP, int readEP )
{
    return TRUE;
}

BOOL USB_CloseStream( int UsbStream )
{
    return TRUE;
}

int USB_Write( int UsbStream, const char* Data, size_t size )
{
    return EmulatorNative::GetIUsbDriver()->Write( UsbStream, (IntPtr)(void*)Data, (unsigned int)size );    
}

int USB_Read( int UsbStream, char* Data, size_t size )
{
    return EmulatorNative::GetIUsbDriver()->Read( UsbStream, (IntPtr)(void*)Data, (unsigned int)size );    
}

BOOL USB_Flush( int UsbStream )
{
    return EmulatorNative::GetIUsbDriver()->Flush( UsbStream );    
}

UINT32 USB_GetEvent( int Controller, UINT32 Mask )
{
    return 0;
}

UINT32 USB_SetEvent( int Controller, UINT32 Event )
{
    return 0;
}

UINT32 USB_ClearEvent( int Controller, UINT32 Event )
{
    return 0;
}

UINT8 USB_GetStatus( int Controller )
{
    return 0;
}

void USB_DiscardData( int UsbStream, BOOL fTx )
{
}

