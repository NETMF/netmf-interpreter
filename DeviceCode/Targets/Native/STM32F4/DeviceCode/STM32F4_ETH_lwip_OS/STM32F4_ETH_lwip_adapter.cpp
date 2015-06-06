/*
 * Portions Copyright (c) CSA Engineering AG, Switzerland
 * www.csa.ch, info@csa.ch
 */

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <lwip\netifapi.h>
#include <lwip\tcp.h>
#include <lwip\tcpip.h>
#include <lwip\dhcp.h>
#include <lwip\pbuf.h>
#include <netif\etharp.h>

#include "..\STM32F4_Power\STM32F4_Power_functions.h"

#include "STM32F4_ETH_lwip_adapter.h"
#include "STM32F4_ETH_lwip.h"
#include "STM32F4_ETH_driver.h"

static struct netif          g_STM32F4_ETH_NetIF;

HAL_CONTINUATION    InterruptTaskContinuation;
HAL_COMPLETION      LwipUpTimeCompletion;
static BOOL         LinkStatus = FALSE;
static int          nAttempts = 0;
static BOOL         isPhyPoweringUp = FALSE;

extern void ZeroRxDesc();
extern void ZeroTxDesc();

extern NETWORK_CONFIG g_NetworkConfig;

err_t STM32F4_ETH_ethhw_init( struct netif *myNetIf )
{
    /* Initialize netif */
    myNetIf->name[ 0 ] = 'e';
    myNetIf->name[ 1 ] = '0';
    myNetIf->mtu = 1500;
    myNetIf->output = etharp_output;
    myNetIf->linkoutput = STM32F4_ETH_LWIP_xmit;
    myNetIf->flags = NETIF_FLAG_IGMP | NETIF_FLAG_BROADCAST | NETIF_FLAG_ETHARP | NETIF_FLAG_LINK_UP;

    /* Open ethernet driver */
    LinkStatus = STM32F4_ETH_LWIP_open( myNetIf );

    return ERR_OK;
}

// ISR for Receive interrupts
void lwip_interrupt_continuation( void )
{
    NATIVE_PROFILE_PAL_NETWORK();
    STM32F4_ETH_LWIP_recv( &g_STM32F4_ETH_NetIF );
}

// completion used when system is wired up in such a way that there is
// no link status interrupt available in hardware. Thus this continuation
// is used to poll the PHY to determine the current state at regular
// intervals.
//
// NOTE: 
// This is not a recommended design for hardware as it requires waking
// the system from sleep for polling, thus wasting power on battery
// operated systems.
void lwip_network_uptime_completion( void *arg )
{
    NATIVE_PROFILE_PAL_NETWORK( );
    BOOL status;

    /* Power up PHY and wait while it starts */
    if (!LinkStatus && !isPhyPoweringUp)
    {
        eth_powerUpPhy(TRUE);
        isPhyPoweringUp = TRUE;
        LwipUpTimeCompletion.EnqueueDelta64( 4000000 );
        nAttempts++;
        return;
    }

    /* PHY should now have started, get the network status */
    isPhyPoweringUp = FALSE;
    status = eth_isPhyLinkValid(FALSE);

    /* Check whether network status has changed */
    if( status != LinkStatus )
    {
        struct netif* pNetIf = ( struct netif* )arg;

        /* Check status */
        if( status )
        {
            /* Network is up, open ethernet driver */
            SOCK_NetworkConfiguration *pNetCfg = &g_NetworkConfig.NetworkInterfaces[ 0 ];
            STM32F4_ETH_LWIP_open( pNetIf );
            eth_netif_set_link_up( pNetIf );
            if( pNetCfg->flags & SOCK_NETWORKCONFIGURATION_FLAGS_DHCP )
                eth_netif_dhcp_start( pNetIf );

            nAttempts = 0;
        }
        else
        {
            /* Network is down, close ethernet driver */
            STM32F4_ETH_LWIP_close( FALSE );
            netifapi_netif_set_down( pNetIf );
            eth_netif_set_link_down( pNetIf );
        }

        /* Save new network status */
        LinkStatus = status;
    }

    /* Power down PHY if network is down */
    if( !LinkStatus )
    {
        eth_powerUpPhy( FALSE );
    }

    /* Schedule the next link status check */
    if( LinkStatus || ( nAttempts < 12 ) )
    {
        /* When link is up or has been up, check every second */
        LwipUpTimeCompletion.EnqueueDelta64( 1000000 );
    }
    else
    {
        /* When link is down, check only once every 15s */
        LwipUpTimeCompletion.EnqueueDelta64( 15000000 );
    }
}

void InitContinuations( struct netif *pNetIf )
{

    LwipUpTimeCompletion.InitializeForUserMode( ( HAL_CALLBACK_FPN )lwip_network_uptime_completion, ( void* )pNetIf );
    LwipUpTimeCompletion.EnqueueDelta64( 2000000 );
}

void DeInitContinuations( )
{
    LwipUpTimeCompletion.Abort( );
}

void EthernetPrepareZero( )
{
    ZeroRxDesc();
    ZeroTxDesc();
}

void EthernetWakeUp( )
{
    CPU_INTC_ActivateInterrupt( ETH_IRQn, ( HAL_CALLBACK_FPN )STM32F4_ETH_LWIP_interrupt, &g_STM32F4_ETH_NetIF );
    InitContinuations( &g_STM32F4_ETH_NetIF );
}

void EthernetDeepSleep( )
{
    netifapi_netif_set_down( &g_STM32F4_ETH_NetIF );
    STM32F4_ETH_LWIP_close( FALSE );
    LinkStatus = 0;
    eth_powerUpPhy( FALSE );

    DeInitContinuations( );
    CPU_INTC_DeactivateInterrupt( ETH_IRQn );
}

int STM32F4_ETH_LWIP_Driver::Open( int index )
{
    /* Network interface variables */
    ip_addr_t ipaddr, subnetmask, gateway;
    struct netif *pNetIF;
    int len;
    const SOCK_NetworkConfiguration *iface;

    EthernetPrepareZero( );

    /* Enable Phy Powerdown on Deepsleep */
    STM32F4_SetPowerHandlers( EthernetDeepSleep, EthernetWakeUp );

    /* Apply network configuration */
    iface = &g_NetworkConfig.NetworkInterfaces[ index ];

    len = g_STM32F4_ETH_NetIF.hwaddr_len;
    if( len == 0 || iface->macAddressLen < len )
    {
        len = iface->macAddressLen;
        g_STM32F4_ETH_NetIF.hwaddr_len = len;
    }
    memcpy( g_STM32F4_ETH_NetIF.hwaddr, iface->macAddressBuffer, len );

    ipaddr.addr = iface->ipaddr;
    gateway.addr = iface->gateway;
    subnetmask.addr = iface->subnetmask;

    pNetIF = netif_add( &g_STM32F4_ETH_NetIF, &ipaddr, &subnetmask, &gateway, NULL, STM32F4_ETH_ethhw_init, tcpip_input );

    CPU_INTC_ActivateInterrupt( ETH_IRQn, ( HAL_CALLBACK_FPN )STM32F4_ETH_LWIP_interrupt, &g_STM32F4_ETH_NetIF );

    netif_set_default( pNetIF );

    /* Initialize the continuation routine for the driver interrupt and receive */
    InitContinuations( pNetIF );

    /* Return LWIP's net interface number */
    return g_STM32F4_ETH_NetIF.num;
}

BOOL STM32F4_ETH_LWIP_Driver::Close( void )
{
    CPU_INTC_DeactivateInterrupt( ETH_IRQn );

    DeInitContinuations( );

    LinkStatus = 0;
    eth_powerUpPhy( FALSE );

    netifapi_netif_set_down( &g_STM32F4_ETH_NetIF );
    netifapi_netif_remove( &g_STM32F4_ETH_NetIF );

    STM32F4_ETH_LWIP_close( TRUE );

    memset( &g_STM32F4_ETH_NetIF, 0, sizeof g_STM32F4_ETH_NetIF );

    return TRUE;
}

BOOL STM32F4_ETH_LWIP_Driver::Bind( void )
{
    return TRUE;
}

BOOL STM32F4_ETH_LWIP_Driver::IsNetworkConnected( void )
{
    return LinkStatus;
}
