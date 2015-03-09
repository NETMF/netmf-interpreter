//-----------------------------------------------------------------------------
//
//  <No description>
//
//  Microsoft dotNetMF Project
//  Copyright ©2004 Microsoft Corporation
//  One Microsoft Way, Redmond, Washington 98052-6399 U.S.A.
//  All rights reserved.
//  MICROSOFT CONFIDENTIAL
//
//-----------------------------------------------------------------------------

#include "MicroBooter.h"

static const IUpdateStorageProvider* s_UpdateStorageList[] =
{
    // Add your update storage provider here
};

const IUpdateStorageProvider** g_MicroBooter_UpdateStorageList      = s_UpdateStorageList;
UINT32                         g_MicroBooter_UpdateStorageListCount = ARRAYSIZE(s_UpdateStorageList);

BOOL EnterMicroBooter(INT32& timeout)
{
    CPU_GPIO_Initialize();
    
    Events_WaitForEvents(0,100); // wait for buttons to init

    // check up/down button state
    //if(!CPU_GPIO_GetPinState( MC9328MXL_GPIO::c_Port_B_10 ) && !CPU_GPIO_GetPinState( MC9328MXL_GPIO::c_Port_B_11 ))
    //{
    //    // user override, so lets stay forever
    //    timeout = -1;
    //    return TRUE;
    //}
    
    timeout = 0;
    return FALSE;
}

UINT32 MicroBooter_ProgramMarker()
{
    return 0xE321F0DF;
}

UINT32 MicroBooter_PrepareForExecution(UINT32 physicalEntryPointAddress)
{
    return physicalEntryPointAddress;
}



