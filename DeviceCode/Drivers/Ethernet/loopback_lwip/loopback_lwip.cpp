/* ********************************************************************

   LOOP.C - LOOP BACK device driver interface
  
   EBS - RTIP
  
   Copyright EBSNet , 2007
   All rights reserved.
   This code may not be redistributed in source or linkable object form
   without the consent of its author.
  
   ********************************************************************     */

#include <tinyhal.h>

#include "lwip\netif.h"
#include "lwip\pbuf.h"
#include "netif\etharp.h"
#include "ipv4\lwip\ip.h"

#include "loopback_lwip_driver.h"

DRIVERROUTINES  loop_driverRoutines;
PDRIVERROUTINES loop_lwip = &loop_driverRoutines;

// comment this to disable the loopback ISR emulation
#define FAKE_ASYNC_LOOPBACK_PROCESSING


#if defined(FAKE_ASYNC_LOOPBACK_PROCESSING)
#define PBUF_CHAIN_DEPTH 128

pbuf* g_dummy_pbuf_chain[PBUF_CHAIN_DEPTH];
UINT32 g_dummy_pbuf_current = 0;

HAL_COMPLETION g_fake_loopback_ISR;

void loop_lwip_send_packet(void* arg)
{
    netif* network_interface = (netif*)arg;

    UINT32 current = g_dummy_pbuf_current == 0 ? PBUF_CHAIN_DEPTH-1 : g_dummy_pbuf_current - 1;
    pbuf* last_packet = g_dummy_pbuf_chain[ current ];

    network_interface->input(last_packet, network_interface);

    HAL_SOCK_EventsSet( SOCKET_EVENT_FLAG_SOCKET );
}

#endif

extern "C"
{

/* ********************************************************************     */
bool  loop_lwip_open( struct netif *pNetIF );
void  loop_lwip_close( struct netif *pNetIF );
err_t loop_lwip_xmit( struct netif *pNetIF ,  struct pbuf *pPBuf );
bool  loop_lwip_xmit_done( struct netif *pNetIF ,  struct pbuf *pPBuf , bool success);
bool  loop_lwip_statistics( struct netif *pNetIF );

/* ********************************************************************
   GLOBAL DATA
   ********************************************************************     */
	
}   /* extern "C" */

const unsigned char phony_addr[ ETHARP_HWADDR_LEN ] = { 1, 2, 3, 4, 5, 6 };
    

/* ********************************************************************
   open the loop back driver interface.
   
   This routine opens a loop back device driver
  
   The address of this function must be placed into the "devices" table in
   iface.c either at compile time or before a device open is called.

   ********************************************************************     */  
bool loop_lwip_open( struct netif *pNetIF )                            
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    
    /* Now put in a dummy ethernet address get the ethernet address */
   
    memcpy( pNetIF->hwaddr, phony_addr, ETHARP_HWADDR_LEN ); 
    
#if defined(FAKE_ASYNC_LOOPBACK_PROCESSING)
    memset(&g_dummy_pbuf_chain,0,PBUF_CHAIN_DEPTH*sizeof(pbuf*));

    g_fake_loopback_ISR.InitializeForISR( loop_lwip_send_packet, pNetIF );
#endif

    /* JJM Possibly initialize Continuation here. loop_lwip_interrupt_continuation */

    return( TRUE );
}

/* ********************************************************************
   close the packet driver interface.
   
   This routine is called when the device interface is no longer needed
   it should stop the driver from delivering packets to the upper levels
   and shut off packet delivery to the network.
  
   The address of this function must be placed into the "devices" table in
   iface.c either at compile time or before a device open is called.
  
   ********************************************************************     */
void loop_lwip_close( struct netif *pNetIF )                                
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();

}

/* ********************************************************************
   Transmit. a packet over the packet driver interface.
   
   This routine is called when a packet needs sending. The packet contains a
   full ethernet frame to be transmitted. The length of the packet is 
   provided.
  
   Returns 0 if successful or errno if unsuccessful
  
   ********************************************************************     */
err_t loop_lwip_xmit( struct netif *pNetIF, struct pbuf *pPBuf)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();

#if defined(FAKE_ASYNC_LOOPBACK_PROCESSING)
    // store the packet and process it asynchronously by calling SOCKETS_RestartTcpIpProcessor()
    // this would also require fakign an interrupt to 'receive' the packet
    
    g_dummy_pbuf_chain[g_dummy_pbuf_current] = pPBuf;
    if(++g_dummy_pbuf_current == PBUF_CHAIN_DEPTH) g_dummy_pbuf_current = 0;

    g_fake_loopback_ISR.EnqueueDelta(10 * 1000);

    SOCKETS_RestartTcpIpProcessor(10 * 1000);
    
#else
    pNetIF->input( pPBuf, pNetIF );
#endif

    return( ERR_OK );    
}

/* ********************************************************************
   Statistic. return statistics about the device interface
   
   This routine is called by user code that wishes to inspect driver statistics.
   We call this routine in the demo program. It is not absolutely necessary
   to implement such a function (Leave it empty.), but it is a handy debugging
   tool.
  
   The address of this function must be placed into the "devices" table in
   iface.c either at compile time or before a device open is called.
  
   
   Non packet drivers should behave the same way.
  
   ********************************************************************     */
bool loop_lwip_statistics( struct netif *pNetIF )                       
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();

    return( TRUE );
}

PDRIVERROUTINES loop_lwip_bind( void )
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();

    if ( loop_lwip == NULL )
    {
        return NULL;  // Out of memory
    }

    loop_lwip->open = loop_lwip_open;
    loop_lwip->close = loop_lwip_close;
    loop_lwip->xmit = loop_lwip_xmit;

    return loop_lwip;

}


