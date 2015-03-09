#include <tinyhal.h>
#include "net_decl_lwip.h"
#include "loopback_lwip_driver.h"
 
#include "netif\etharp.h"


extern "C" err_t netif_loopif_init(struct netif *netif);


#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_LOOPBACK_LWIP_Driver"
#endif

LOOPBACK_LWIP_Driver g_LOOPBACK_LWIP_Driver;
PDRIVERROUTINES      g_pLOOPBACK_LWIP_Driver_Routines;

struct netif         g_Loopback_NetIF;


#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

err_t   loop_ethhw_init( netif * myNetIf) 
{ 
    myNetIf->mtu = 1024;

    myNetIf->output = etharp_output;
    
    /*  NOTE: If loopback is enabled in lwipopts.h, ip_output will 
            eventually call netif_loop_output instead of our xmit routine. */ 
    /*  Assign the netif_loop_output routine to this netif */
    /*  WRONG: this is handled directly per the above comment */
#if LWIP_HAVE_LOOPIF
    netif_loopif_init( myNetIf );
#else
    /* Assign the xmit routine to the stack's netif  */
    myNetIf->linkoutput = g_pLOOPBACK_LWIP_Driver_Routines->xmit;
    g_pLOOPBACK_LWIP_Driver_Routines->open( myNetIf );
#endif

    return 0; 
}
    

int LOOPBACK_LWIP_Driver::Open( )
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();

    /* Network interface variables */
    struct ip_addr ipaddr, netmask, gw;
    struct netif *pNetIF;

    /* Set network address variables */
    IP4_ADDR(&gw, 127,0,0,1);
    IP4_ADDR(&ipaddr, 127,0,0,1);
    IP4_ADDR(&netmask, 255,255,255,0);

    pNetIF = netif_add( &g_Loopback_NetIF, &ipaddr, &netmask, &gw, NULL, loop_ethhw_init, ethernet_input );

    /* ethhw_init() is user-defined */
    /* use ip_input instead of ethernet_input for non-ethernet hardware */
    /* (this function is assigned to netif.input and should be called by the hardware driver) */

    netif_set_default( pNetIF );

    netif_set_up( pNetIF );
    
    return g_Loopback_NetIF.num;
}

BOOL LOOPBACK_LWIP_Driver::Close( )
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();

    netif_set_down( &g_Loopback_NetIF );

    netif_remove( &g_Loopback_NetIF );
    
    memset(&g_Loopback_NetIF, 0, sizeof(g_Loopback_NetIF));
    
    return TRUE;
}

BOOL  LOOPBACK_LWIP_Driver::Bind  ( )
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    
    g_pLOOPBACK_LWIP_Driver_Routines = loop_lwip_bind( );
    
    if ( g_pLOOPBACK_LWIP_Driver_Routines != NULL)
    {
        return TRUE;
    }
    else
    {
        return FALSE;
    }
}

    


