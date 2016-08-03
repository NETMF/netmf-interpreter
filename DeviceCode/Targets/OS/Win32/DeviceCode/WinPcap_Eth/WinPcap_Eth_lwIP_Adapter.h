////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#ifndef _WINPCAP_ETH_LWIP_ADAPTER_H_
#define _WINPCAP_ETH_LWIP_ADAPTER_H_ 1
//          1         2         3
//01234567890123456789012345678901234567
//{00000000-0000-0000-0000-000000000000}
const size_t GuidStringLen = 39; //38 chars + 1 for terminating \0

struct WINPCAP_ETH_LWIP_DRIVER_CONFIG
{
    char adapterGuid[GuidStringLen];
};

#ifndef NETWORK_INTERFACE_COUNT
#define NETWORK_INTERFACE_COUNT 1
#endif

struct WINPCAP_ETH_LWIP_DEVICE_CONFIG
{
    WINPCAP_ETH_LWIP_DRIVER_CONFIG DeviceConfigs[ NETWORK_INTERFACE_COUNT ];

    static LPCSTR GetDriverName( )
    {
        return "WINPCAP_ETH_LWIP";
    }
};

struct WINPCAP_ETH_LWIP_Driver
{
    static int Open( WINPCAP_ETH_LWIP_DRIVER_CONFIG* config, int index );
    static BOOL Close( int index );
    static BOOL Bind( void );
};

//
// _WINPCAP_ETH_LWIP_ADAPTER_H_
//////////////////////////////////////////////////////////////////////////////

#endif

