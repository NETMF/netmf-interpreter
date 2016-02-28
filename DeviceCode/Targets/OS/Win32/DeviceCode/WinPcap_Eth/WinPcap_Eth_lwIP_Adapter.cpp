////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <lwip\netif.h>
#include <lwip\tcpip.h>
#include <lwip\dhcp.h>
#include <netif\etharp.h>

#include "WinPcap_Eth_lwIP_Adapter.h"

extern "C"
{
#include "pcapif.h"
}

extern WINPCAP_ETH_LWIP_DEVICE_CONFIG 	g_WINPCAP_ETH_LWIP_Config;
extern NETWORK_CONFIG           		g_NetworkConfig;

static BOOL				LwipNetworkStatus = FALSE;
static struct netif     g_WinPcap_ETH_NetIF;

static WINPCAP_ETH_LWIP_Driver g_WINPCAP_ETH_LWIP_Driver;

BOOL Network_Interface_Bind(int index)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();

    return g_WINPCAP_ETH_LWIP_Driver.Bind();
}

int  Network_Interface_Open(int index)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
	if(index >= ARRAYSIZE(g_WINPCAP_ETH_LWIP_Config.DeviceConfigs))
        return -1;    

    HAL_CONFIG_BLOCK::ApplyConfig( WINPCAP_ETH_LWIP_DEVICE_CONFIG::GetDriverName(), &g_WINPCAP_ETH_LWIP_Config, sizeof(g_WINPCAP_ETH_LWIP_Config) );

    return g_WINPCAP_ETH_LWIP_Driver.Open(&g_WINPCAP_ETH_LWIP_Config.DeviceConfigs[index], index);
}

BOOL Network_Interface_Close(int index)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
	
    return g_WINPCAP_ETH_LWIP_Driver.Close(index);
}

int WINPCAP_ETH_LWIP_Driver::Open(WINPCAP_ETH_LWIP_DRIVER_CONFIG* config, int index)
{
	/* Network interface variables */
    struct ip_addr ipaddr, subnetmask, gateway;
	BOOL isDHCPenabled;
	const SOCK_NetworkConfiguration *iface;

	if(config == NULL)
	{
		return -1;
	}
	
	/* Apply network configuration */
    iface = &g_NetworkConfig.NetworkInterfaces[index];
	isDHCPenabled = (iface->flags & SOCK_NETWORKCONFIGURATION_FLAGS_DHCP)? TRUE: FALSE;
	
	if(!isDHCPenabled)
    {
		/* Set network address variables for static ip configuration */
        ipaddr.addr  = iface->ipaddr;
        gateway.addr = iface->gateway;
        subnetmask.addr = iface->subnetmask;
    }
    else
    {
        /* Set network address variables - this will be set by either DHCP or when the configuration is applied */
        IP4_ADDR(&gateway,   0,   0,   0, 0);
        IP4_ADDR(&ipaddr ,   0,   0,   0, 0);
        IP4_ADDR(&subnetmask, 255, 255, 255, 0);
    }
	
	/* Configure the MAC address */
	if(iface->macAddressLen != ETHARP_HWADDR_LEN)
	{
		return -1;
	}

	memcpy(g_WinPcap_ETH_NetIF.hwaddr, iface->macAddressBuffer, iface->macAddressLen);
      
    auto pNetIf = netif_add( &g_WinPcap_ETH_NetIF, &ipaddr, &subnetmask, &gateway, (void*) config->adapterGuid, pcapif_init, tcpip_input );
    if( pNetIf == nullptr )
        return -1;
   
    netif_set_default( &g_WinPcap_ETH_NetIF );

	LwipNetworkStatus = TRUE;

    return g_WinPcap_ETH_NetIF.num;
}

BOOL WINPCAP_ETH_LWIP_Driver::Close(int index)
{
	const SOCK_NetworkConfiguration *iface;
	
    LwipNetworkStatus = FALSE;
    
	iface = &g_NetworkConfig.NetworkInterfaces[index];
	
    netif_remove( &g_WinPcap_ETH_NetIF );

	pcapif_shutdown(&g_WinPcap_ETH_NetIF);
  
    memset( &g_WinPcap_ETH_NetIF, 0, sizeof g_WinPcap_ETH_NetIF );

    return TRUE;
}

/* Bind function doesn't actually do anything for this implementation */
BOOL WINPCAP_ETH_LWIP_Driver::Bind(void)
{
    return TRUE;
}

    


