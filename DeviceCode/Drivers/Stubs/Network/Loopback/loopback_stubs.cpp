////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "net_decl.h"
#include "loopback_driver.h"
#include <rtipapi.h>


//--//

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_LOOPBACK_Driver"
#endif

LOOPBACK_Driver g_LOOPBACK_Driver;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

//--//

INIT_FNCS RTP_FAR loop_fnc;

void init_loopback(void)
{
}

int LOOPBACK_Driver::Open( )
{
    return 0;
}

BOOL LOOPBACK_Driver::Close( )
{
    return FALSE;
}

BOOL  LOOPBACK_Driver::Bind  ( )
{
    return FALSE;
}
