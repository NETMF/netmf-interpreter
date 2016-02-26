/**
 * @file
 * Stack-internal timers implementation.
 * This file includes timer callbacks for stack-internal timers as well as
 * functions to set up or stop timers and check for expired timers.
 *
 */

/*
 * Copyright (c) 2001-2004 Swedish Institute of Computer Science.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice,
 *    this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
 * SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
 * OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING
 * IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 * OF SUCH DAMAGE.
 *
 * This file is part of the lwIP TCP/IP stack.
 *
 * Author: Adam Dunkels <adam@sics.se>
 *         Simon Goldschmidt
 *
 */
// Changes
// Copyright (C),  Microsoft Open Technologies
// Released on the same BSD terms as the the LWIP stack listed above
//   - cloned this file into the sys_arch layer instead of the core stack
//     Although this file could eventually merge back to the core as the
//     key changes are in isolating out the implementation of ALL timer
//     support to the sys_arch layer instead of trying to handle periodic
//     timers in general (but ultimately broken) way. 
//   - added sys_periodic_timeout() to support periodic timeouts in the OS/SysArch
//     layer directly, if available.
//   - moved sys_xxx timeout functions out of this file and into the architecture
//     layer where an OS specific implementation can handle the timeouts. This
//     prevents iterative error accumulation from self rescheduling timeouts that
//     will reschedule themselves only after processing their timeout work, which 
//     adds the time from the point the timeout was triggered to the time the handler
//     completed into the delta for the next timeout on each iteration. This leads to
//     significant error drift over time. Using the new sys_periodic_timeout and allowing
//     the underlying sys_arch layer to handle the timeouts as absolute time deadlines
//     resolves the drift. Either the underlying OS/Arch supports periodic timers directly
//     or the implementation of periodic timers can simply track the deadline and on each
//     trigger. Each time the timeout triggers it can simply add the period to the previous
//     deadline to determine the next absolute time deadline. While there is (and always
//     will be) some jitter on the start time of the handler function, there is no iterative
//     acuumulation of error causing drift away from expected timeout periods. 
//   
#include "lwip/opt.h"

#include "lwip/timers.h"
#include "lwip/tcp_impl.h"

#if LWIP_TIMERS

#include "lwip/def.h"
#include "lwip/memp.h"
#include "lwip/tcpip.h"

#include "lwip/ip_frag.h"
#include "netif/etharp.h"
#include "lwip/dhcp.h"
#include "lwip/autoip.h"
#include "lwip/igmp.h"
#include "lwip/dns.h"
#include "lwip/sys.h"
#include "lwip/pbuf.h"
#include "lwip/netifapi.h"

extern void sys_periodic_timeout( u32_t msecs, sys_timeout_handler handler, void *arg );

// address of this "cookie' is passed as arg to callbacks
// that run on tcpip thread. This is used to prevent
// creating another wrapper layer as the OS timer callbacks
// require a function signature with a void* argument
// but most of the actual functions don't use one. (Lambdas
// and closures would elminate the need for doing this all
// manually, but C doesn't have them and C++ support for
// them with embedded compilers is not ready for prime time)
// While any non-NULL value could work, using the address of
// a known value ensures that random values don't cause trouble.
static MagicCookie = 0x12345678;

#if LWIP_TCP
/** global variable that shows if the tcp timer is currently scheduled or not */
static int tcpip_tcp_timer_active;

#define NO_BLOCK 0

/**
 * Timer callback function that calls tcp_tmr() and reschedules itself.
 *
 * @param arg unused argument
 */
static void tcpip_tcp_timer( void* arg )
{
    // if the arg is NULL then this is called from the OS timer callback on 
    // an OS or sys_arch provided thread. Post a call to this function on
    // the tcpip main thread with a well known value as the arg to skip this
    // check and handle the timer on the tcpip thread.
    if( arg == NULL )
        tcpip_callback_with_block( tcpip_tcp_timer, &MagicCookie, NO_BLOCK );
    else
    {
        /* call TCP timer handler */
        tcp_tmr( );
        /* timer still needed? */
        if( !tcp_active_pcbs && !tcp_tw_pcbs )
        {
            /* restart timer */
            sys_untimeout( tcpip_tcp_timer, NULL );
        }
        else
        {
            /* disable timer */
            tcpip_tcp_timer_active = 0;
        }
    }
}

/**
 * Called from TCP_REG when registering a new PCB:
 * the reason is to have the TCP timer only running when
 * there are active (or time-wait) PCBs.
 */
void tcp_timer_needed( void )
{
    /* timer is off but needed again? */
    if( !tcpip_tcp_timer_active && ( tcp_active_pcbs || tcp_tw_pcbs ) )
    {
        /* enable and start timer */
        tcpip_tcp_timer_active = 1;
        sys_periodic_timeout( TCP_TMR_INTERVAL, tcpip_tcp_timer, NULL );
    }
}
#endif /* LWIP_TCP */

#if IP_REASSEMBLY
/**
 * Timer callback function that calls ip_reass_tmr() and reschedules itself.
 *
 * @param arg unused argument
 */
static void ip_reass_timer( void *arg )
{
    LWIP_DEBUGF( TIMERS_DEBUG, ( "tcpip: ip_reass_tmr()\n" ) );

    // if the arg is NULL then this is called from the OS timer callback on 
    // an OS or sys_arch provided thread. Post a call to this function on
    // the tcpip main thread with a well known value as the arg to skip this
    // check and handle the timer on the tcpip thread.
    if( arg == NULL )
        tcpip_callback_with_block( ip_reass_timer, &MagicCookie, NO_BLOCK );
    else
        ip_reass_tmr();
}
#endif /* IP_REASSEMBLY */

#if LWIP_ARP
/**
 * Timer callback function that calls etharp_tmr() and reschedules itself.
 *
 * @param arg unused argument
 */
static void arp_timer( void *arg )
{
    LWIP_DEBUGF( TIMERS_DEBUG, ( "tcpip: etharp_tmr()\n" ) );
    // if the arg is NULL then this is called from the OS timer callback on 
    // an OS or sys_arch provided thread. Post a call to this function on
    // the tcpip main thread with a well known value as the arg to skip this
    // check and handle the timer on the tcpip thread.
    if( arg == NULL )
        tcpip_callback_with_block( arp_timer, &MagicCookie, NO_BLOCK );
    else
        etharp_tmr( );
}
#endif /* LWIP_ARP */

#if LWIP_DHCP
/**
 * Timer callback function that calls dhcp_coarse_tmr() and reschedules itself.
 *
 * @param arg unused argument
 */
static void dhcp_timer_coarse( void *arg )
{
    LWIP_DEBUGF( TIMERS_DEBUG, ( "tcpip: dhcp_coarse_tmr()\n" ) );
    // if the arg is NULL then this is called from the OS timer callback on 
    // an OS or sys_arch provided thread. Post a call to this function on
    // the tcpip main thread with a well known value as the arg to skip this
    // check and handle the timer on the tcpip thread.
    if( arg == NULL )
        tcpip_callback_with_block( dhcp_timer_coarse, &MagicCookie, NO_BLOCK );
    else
        dhcp_coarse_tmr( );
}

/**
 * Timer callback function that calls dhcp_fine_tmr() and reschedules itself.
 *
 * @param arg unused argument
 */
static void dhcp_timer_fine( void *arg )
{
    LWIP_DEBUGF( TIMERS_DEBUG, ( "tcpip: dhcp_fine_tmr()\n" ) );
    // if the arg is NULL then this is called from the OS timer callback on 
    // an OS or sys_arch provided thread. Post a call to this function on
    // the tcpip main thread with a well known value as the arg to skip this
    // check and handle the timer on the tcpip thread.
    if( arg == NULL )
        tcpip_callback_with_block( dhcp_timer_fine, &MagicCookie, NO_BLOCK );
    else
        dhcp_fine_tmr( );
}
#endif /* LWIP_DHCP */

#if LWIP_AUTOIP
/**
 * Timer callback function that calls autoip_tmr() and reschedules itself.
 *
 * @param arg unused argument
 */
static void autoip_timer( void *arg )
{
    LWIP_DEBUGF( TIMERS_DEBUG, ( "tcpip: autoip_tmr()\n" ) );
    // if the arg is NULL then this is called from the OS timer callback on 
    // an OS or sys_arch provided thread. Post a call to this function on
    // the tcpip main thread with a well known value as the arg to skip this
    // check and handle the timer on the tcpip thread.
    if( arg == NULL )
        tcpip_callback_with_block( autoip_timer, &MagicCookie, NO_BLOCK );
    else
        autoip_tmr( );
}
#endif /* LWIP_AUTOIP */

#if LWIP_IGMP
/**
 * Timer callback function that calls igmp_tmr() and reschedules itself.
 *
 * @param arg unused argument
 */
static void igmp_timer( void *arg )
{
    LWIP_DEBUGF( TIMERS_DEBUG, ( "tcpip: igmp_tmr()\n" ) );
    // if the arg is NULL then this is called from the OS timer callback on 
    // an OS or sys_arch provided thread. Post a call to this function on
    // the tcpip main thread with a well known value as the arg to skip this
    // check and handle the timer on the tcpip thread.
    if( arg == NULL )
        tcpip_callback_with_block( igmp_timer, &MagicCookie, NO_BLOCK );
    else
        igmp_tmr( );
}
#endif /* LWIP_IGMP */

#if LWIP_DNS
/**
 * Timer callback function that calls dns_tmr() and reschedules itself.
 *
 * @param arg unused argument
 */
static void dns_timer( void *arg )
{
    LWIP_DEBUGF( TIMERS_DEBUG, ( "tcpip: dns_tmr()\n" ) );
    // if the arg is NULL then this is called from the OS timer callback on 
    // an OS or sys_arch provided thread. Post a call to this function on
    // the tcpip main thread with a well known value as the arg to skip this
    // check and handle the timer on the tcpip thread.
    if( arg == NULL )
        tcpip_callback_with_block( dns_timer, &MagicCookie, NO_BLOCK );
    else
        dns_tmr( );
}
#endif /* LWIP_DNS */

/** Initialize this module */
void sys_timeouts_init( void )
{
#if IP_REASSEMBLY
    sys_periodic_timeout( IP_TMR_INTERVAL, ip_reass_timer, NULL );
#endif /* IP_REASSEMBLY */
#if LWIP_ARP
    sys_periodic_timeout( ARP_TMR_INTERVAL, arp_timer, NULL );
#endif /* LWIP_ARP */
#if LWIP_DHCP
    sys_periodic_timeout( DHCP_COARSE_TIMER_MSECS, dhcp_timer_coarse, NULL );
    sys_periodic_timeout( DHCP_FINE_TIMER_MSECS, dhcp_timer_fine, NULL );
#endif /* LWIP_DHCP */
#if LWIP_AUTOIP
    sys_periodic_timeout( AUTOIP_TMR_INTERVAL, autoip_timer, NULL );
#endif /* LWIP_AUTOIP */
#if LWIP_IGMP
    sys_periodic_timeout( IGMP_TMR_INTERVAL, igmp_timer, NULL );
#endif /* LWIP_IGMP */
#if LWIP_DNS
    sys_periodic_timeout( DNS_TMR_INTERVAL, dns_timer, NULL );
#endif /* LWIP_DNS */

#if NO_SYS
    /* Initialise timestamp for sys_check_timeouts */
    timeouts_last_time = sys_now( );
#endif
}


#else /* LWIP_TIMERS */
/* Satisfy the TCP code which calls this function */
void
tcp_timer_needed( void )
{
}
#endif /* LWIP_TIMERS */
