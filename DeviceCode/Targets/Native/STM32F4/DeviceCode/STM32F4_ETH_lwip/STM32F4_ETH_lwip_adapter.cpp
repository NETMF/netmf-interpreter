/*
 * Portions Copyright (c) CSA Engineering AG, Switzerland
 * www.csa.ch, info@csa.ch
 */
 
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <net_decl_lwip.h>
#include <lwip\netif.h>
#include <lwip\tcp.h>
#include <lwip\tcpip.h>
#include <lwip\dhcp.h>
#include <lwip\pbuf.h>
#include <netif\etharp.h>

#include "..\STM32F4_Power\STM32F4_Power_functions.h"

#include "STM32F4_ETH_lwip_adapter.h"
#include "STM32F4_ETH_lwip.h"
#include "STM32F4_ETH_driver.h"

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_STM32F4_ETH_lwip_adapter"
#endif

static struct netif          g_STM32F4_ETH_NetIF;

HAL_CONTINUATION    InterruptTaskContinuation;
HAL_COMPLETION      LwipUpTimeCompletion;
static BOOL         LwipNetworkStatus = 0;
static UINT32       LwipLastIpAddress = 0;
static int          nAttempts = 0;
static BOOL         isPhyPoweringUp = FALSE;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

/* these define the region to zero initialize */
extern UINT32 Image$$ER_ETHERNET$$ZI$$Base;
extern UINT32 Image$$ER_ETHERNET$$ZI$$Length;

extern NETWORK_CONFIG           g_NetworkConfig;

// -- //

void STM32F4_ETH__status_callback(struct netif *netif)
{
    if(LwipLastIpAddress != netif->ip_addr.addr)
    {
        Network_PostEvent( NETWORK_EVENT_TYPE_ADDRESS_CHANGED, 0 );
        LwipLastIpAddress = netif->ip_addr.addr;
    }

#if defined(_DEBUG)
    lcd_printf("\f\n\n\n\n\n\nLink Update: \n");
    lcd_printf("         IP: %d.%d.%d.%d\n", (netif->ip_addr.addr >>  0) & 0xFF, 
                                             (netif->ip_addr.addr >>  8) & 0xFF,
                                             (netif->ip_addr.addr >> 16) & 0xFF,
                                             (netif->ip_addr.addr >> 24) & 0xFF);
    lcd_printf("         GW: %d.%d.%d.%d\n", (netif->gw.addr >>  0) & 0xFF, 
                                             (netif->gw.addr >>  8) & 0xFF,
                                             (netif->gw.addr >> 16) & 0xFF,
                                             (netif->gw.addr >> 24) & 0xFF);
    debug_printf("\nLink Update: \r\n");
    debug_printf("         IP: %d.%d.%d.%d\r\n", (netif->ip_addr.addr >>  0) & 0xFF, 
                                             (netif->ip_addr.addr >>  8) & 0xFF,
                                             (netif->ip_addr.addr >> 16) & 0xFF,
                                             (netif->ip_addr.addr >> 24) & 0xFF);
    debug_printf("         GW: %d.%d.%d.%d\r\n", (netif->gw.addr >>  0) & 0xFF, 
                                             (netif->gw.addr >>  8) & 0xFF,
                                             (netif->gw.addr >> 16) & 0xFF,
                                             (netif->gw.addr >> 24) & 0xFF);
#endif
}


err_t STM32F4_ETH_ethhw_init(struct netif *myNetIf) 
{ 
    /* Initialize netif */
    myNetIf->name[0] = 'e';
    myNetIf->name[1] = '0';
    myNetIf->mtu = 1500;
    myNetIf->output = etharp_output;
    myNetIf->linkoutput = STM32F4_ETH_LWIP_xmit;
    myNetIf->status_callback = STM32F4_ETH__status_callback;
    myNetIf->flags = NETIF_FLAG_IGMP | NETIF_FLAG_BROADCAST | NETIF_FLAG_ETHARP | NETIF_FLAG_LINK_UP;
  
    /* Open ethernet driver */
    LwipNetworkStatus = STM32F4_ETH_LWIP_open(myNetIf);

    return ERR_OK; 
}

void lwip_interrupt_continuation( void )
{
    NATIVE_PROFILE_PAL_NETWORK();
    GLOBAL_LOCK(irq);
    
    if(!InterruptTaskContinuation.IsLinked())
    {
        InterruptTaskContinuation.Enqueue();
    }
}

void lwip_network_uptime_completion(void *arg)
{
    NATIVE_PROFILE_PAL_NETWORK();
    BOOL status;
    
    /* Power up PHY and wait while it starts */
    if (!LwipNetworkStatus && !isPhyPoweringUp)
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
    if (status != LwipNetworkStatus)
    {
        struct netif* pNetIf = (struct netif*)arg;

        /* Check status */
        if (status)
        {   
            /* Network is up, open ethernet driver */
            SOCK_NetworkConfiguration *pNetCfg = &g_NetworkConfig.NetworkInterfaces[0];
            STM32F4_ETH_LWIP_open(pNetIf);
            netif_set_up( pNetIf );
            
            if(pNetCfg->flags & SOCK_NETWORKCONFIGURATION_FLAGS_DHCP)
            {
              dhcp_start( pNetIf );
            }
            
            nAttempts = 0;
            
            Network_PostEvent( NETWORK_EVENT_TYPE__AVAILABILITY_CHANGED, NETWORK_EVENT_FLAGS_IS_AVAILABLE );
        }
        else
        {
            /* Network is down, close ethernet driver */
            STM32F4_ETH_LWIP_close(FALSE);
            netif_set_down( (struct netif*)arg );
            Network_PostEvent( NETWORK_EVENT_TYPE__AVAILABILITY_CHANGED, 0);
        }
    
        /* Save new network status */
        LwipNetworkStatus = status;
    }

    /* Power down PHY if network is down */
    if (!LwipNetworkStatus)
    {
        eth_powerUpPhy(FALSE);
    }    
    
    /* Schedule the next network status check */
    if (LwipNetworkStatus || (nAttempts < 12) )
    {
        /* When network is up or has been up, check every 5 seconds */
        LwipUpTimeCompletion.EnqueueDelta64(  1000000 );
    }
    else
    {
        /* When network is down, check only once every minute */
        LwipUpTimeCompletion.EnqueueDelta64( 56000000 );
    }   
}

void InitContinuations( struct netif *pNetIf )
{
    InterruptTaskContinuation.InitializeCallback( (HAL_CALLBACK_FPN)STM32F4_ETH_LWIP_recv, (void*)pNetIf );

    LwipUpTimeCompletion.InitializeForUserMode( (HAL_CALLBACK_FPN)lwip_network_uptime_completion, (void*)pNetIf );
    LwipUpTimeCompletion.EnqueueDelta64( 2000000 );
}

void DeInitContinuations()
{
    LwipUpTimeCompletion.Abort();
    InterruptTaskContinuation.Abort();
}

void EthernetPrepareZero()
{
    /* The ethernet section (TX/RX descriptors and their buffers) is in a separate memory zone
       which is not zero initialized. It must be done manually */
    void* base = (void*)&Image$$ER_ETHERNET$$ZI$$Base;
    UINT32  length = (UINT32)&Image$$ER_ETHERNET$$ZI$$Length;

    memset(base, 0x00, length);
}

void EthernetWakeUp()
{
    CPU_INTC_ActivateInterrupt(ETH_IRQn, (HAL_CALLBACK_FPN)STM32F4_ETH_LWIP_interrupt, &g_STM32F4_ETH_NetIF);
    InitContinuations(&g_STM32F4_ETH_NetIF);
}

void EthernetDeepSleep()
{
    netif_set_down(&g_STM32F4_ETH_NetIF);
    STM32F4_ETH_LWIP_close(FALSE);
    LwipNetworkStatus = 0;
    eth_powerUpPhy(FALSE);

    DeInitContinuations();
    CPU_INTC_DeactivateInterrupt(ETH_IRQn);
}

int STM32F4_ETH_LWIP_Driver::Open(int index)
{
    /* Network interface variables */
    ip_addr_t ipaddr, subnetmask, gateway;
    struct netif *pNetIF;
    int len;
    const SOCK_NetworkConfiguration *iface;

    EthernetPrepareZero();
    
    /* Enable Phy Powerdown on Deepsleep */
    STM32F4_SetPowerHandlers(EthernetDeepSleep, EthernetWakeUp);
    
    /* Apply network configuration */
    iface = &g_NetworkConfig.NetworkInterfaces[index];
    
    len = g_STM32F4_ETH_NetIF.hwaddr_len;
    if (len == 0 || iface->macAddressLen < len)
    {
        len = iface->macAddressLen;
        g_STM32F4_ETH_NetIF.hwaddr_len = len;
    }
    memcpy(g_STM32F4_ETH_NetIF.hwaddr, iface->macAddressBuffer, len);
    
    ipaddr.addr = iface->ipaddr;
    gateway.addr = iface->gateway;
    subnetmask.addr = iface->subnetmask;
      
    pNetIF = netif_add( &g_STM32F4_ETH_NetIF, &ipaddr, &subnetmask, &gateway, NULL, STM32F4_ETH_ethhw_init, ethernet_input );
    
    CPU_INTC_ActivateInterrupt(ETH_IRQn, (HAL_CALLBACK_FPN)STM32F4_ETH_LWIP_interrupt, &g_STM32F4_ETH_NetIF);
    
    netif_set_default( pNetIF );

    if (LwipNetworkStatus)
    {
        netif_set_up( pNetIF );
    }
    else
    {
        netif_set_down( pNetIF);
        
        /* When started within a visual studio debug studio, ethernet driver cannot be opened during the
           initialization. Because of unknown reason, it seems that the PHY must be explicitly powered up */
        eth_powerUpPhy(TRUE);
        isPhyPoweringUp = TRUE;
    }
    
    /* Initialize the continuation routine for the driver interrupt and receive */    
    InitContinuations( pNetIF );
    
    /* Return LWIP's net interface number */
    return g_STM32F4_ETH_NetIF.num;    
}

// -- //

BOOL STM32F4_ETH_LWIP_Driver::Close(void)
{
    CPU_INTC_DeactivateInterrupt(ETH_IRQn);

    DeInitContinuations();

    LwipNetworkStatus = 0;
    eth_powerUpPhy(FALSE);
    
    netif_set_down( &g_STM32F4_ETH_NetIF );
    netif_remove( &g_STM32F4_ETH_NetIF );

    STM32F4_ETH_LWIP_close(TRUE);
  
    memset( &g_STM32F4_ETH_NetIF, 0, sizeof g_STM32F4_ETH_NetIF );

    return TRUE;
}

BOOL STM32F4_ETH_LWIP_Driver::Bind(void)
{
    return TRUE;
}

BOOL STM32F4_ETH_LWIP_Driver::IsNetworkConnected(void)
{
    return LwipNetworkStatus;
}
