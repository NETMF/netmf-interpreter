////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

//--//


int USB_GetControllerCount()
{
    return 0;
}

BOOL USB_Initialize( int Controller )
{
    return TRUE;
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
    return TRUE;
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
    return 0;
}

int USB_Read( int UsbStream, char* Data, size_t size )
{
    return 0;
}

BOOL USB_Flush( int UsbStream )
{
    return TRUE;
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
    return USB_DEVICE_STATE_NO_CONTROLLER;
}

void USB_DiscardData( int UsbStream, BOOL fTx )
{
}


