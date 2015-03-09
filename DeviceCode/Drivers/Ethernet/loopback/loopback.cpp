/* ********************************************************************

   LOOP.C - LOOP BACK device driver interface
  
   EBS - RTIP
  
   Copyright EBSNet , 2007
   All rights reserved.
   This code may not be redistributed in source or linkable object form
   without the consent of its author.
  
   ********************************************************************     */

#include <tinyhal.h>

#include "xnconf.h"
#include "rtipconf.h"
#include "sock.h"
#include "rtip.h"
#include "rtipext.h"
#include "rtpirq.h"
#include "rtp.h"
#include "rtpthrd.h"
#include "rtpprint.h"

/* ********************************************************************
   DEBUG AIDS
   ********************************************************************     */
#define DEBUG_LOOP 0

extern "C"
{
extern void rtp_thrd_interrupt_continuation(int);
extern void rtp_thrd_ip_continuation(int);

void        loop_interrupt(PIFACE pi);

/* ********************************************************************     */
RTP_BOOL    loop_open(PIFACE pi);
void        loop_close(PIFACE pi);
int         loop_xmit(PIFACE pi, DCU msg);
RTP_BOOL    loop_xmit_done(PIFACE pi, DCU msg, RTP_BOOL success);
RTP_BOOL    loop_statistics(PIFACE  pi);


/* ********************************************************************
   GLOBAL DATA
   ********************************************************************     */
PIFACE        RTP_FAR loop_pi;
unsigned long RTP_FAR loop_packets_in;
unsigned long RTP_FAR loop_packets_out;
unsigned long RTP_FAR loop_bytes_in;
unsigned long RTP_FAR loop_bytes_out;
unsigned long RTP_FAR loop_errors_in;
unsigned long RTP_FAR loop_errors_out;

/* used to register LOOPBACK init fnc   */
INIT_FNCS RTP_FAR loop_fnc;	
	
EDEVTABLE RTP_FAR loop_device = 
{
     loop_open, 
     loop_close, 
     loop_xmit, 
     NULLP_FUNC,
     NULLP_FUNC, 
     loop_statistics, 
     NULLP_FUNC, 
	 LOOP_DEVICE, 
	 "LOOPBACK", 
	 MINOR_0, 
	 LOOP_IFACE, 
 	 SNMP_DEVICE_INFO(CFG_OID_LOOP, CFG_SPEED_LOOP)
	 CFG_ETHER_MAX_MTU, 
	 CFG_ETHER_MAX_MSS, 
	 CFG_ETHER_MAX_WIN_IN, 
	 CFG_ETHER_MAX_WIN_OUT, 
	 IOADD(0), 
	 EN(0), 
	 EN(0)
};

}   /* extern "C" */


/* ********************************************************************
   EXTERNS
   ********************************************************************     */
   
/* Pointer to the interface structure. We 
   look at this inside the xmit routine 
 */
extern PIFACE        RTP_FAR loop_pi;
extern unsigned long RTP_FAR loop_packets_in;
extern unsigned long RTP_FAR loop_packets_out;
extern unsigned long RTP_FAR loop_bytes_in;
extern unsigned long RTP_FAR loop_bytes_out;
extern unsigned long RTP_FAR loop_errors_in;
extern unsigned long RTP_FAR loop_errors_out;
extern EDEVTABLE RTP_FAR loop_device;

RTP_CUINT8 phony_addr[ETH_ALEN] = { 1, 2, 3, 4, 5, 6 };
    
/* ********************************************************************
   init_loopback - initialize the data structures for the loop back driver
   			     - this function needs to be registered before calling 
                   xn_rtip_init
   ********************************************************************     */
void init_loopback(void)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
	loop_pi             = 0;
	loop_packets_in     = 0L;
	loop_packets_out    = 0L;
	loop_bytes_in       = 0L;
	loop_bytes_out      = 0L;
	loop_errors_in      = 0L;
	loop_errors_out     = 0L;
}

/* ********************************************************************
   open the loop back driver interface.
   
   This routine opens a loop back device driver
  
   The address of this function must be placed into the "devices" table in
   iface.c either at compile time or before a device open is called.

   ********************************************************************     */  
RTP_BOOL loop_open(PIFACE pi)                            
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    
    /* say interface structure so xmit knows where to find it */
    loop_pi = pi;

    /* Now put in a dummy ethernet address  
       Get the ethernet address */
   
    rtp_memcpy(pi->addr.my_hw_addr, phony_addr, ETH_ALEN); 

    /* clear statistic information */
    loop_packets_in     = 0L;
    loop_packets_out    = 0L;
    loop_bytes_in       = 0L;
    loop_bytes_out      = 0L;
    loop_errors_in      = 0L;
    loop_errors_out     = 0L;

    rtp_irq_hook_interrupt( (RTP_PFVOID) pi, 
                            (RTP_IRQ_FN_POINTER)loop_interrupt, 
                            (RTP_IRQ_FN_POINTER) 0);
    
    return(RTP_TRUE);
}

/* ********************************************************************
   close the packet driver interface.
   
   This routine is called when the device interface is no longer needed
   it should stop the driver from delivering packets to the upper levels
   and shut off packet delivery to the network.
  
   The address of this function must be placed into the "devices" table in
   iface.c either at compile time or before a device open is called.
  
   ********************************************************************     */
void loop_close(PIFACE pi)                                
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    /* keep compiler happy */
    ARGSUSED_PVOID( pi );  
}

/* ********************************************************************
   Transmit. a packet over the packet driver interface.
   
   This routine is called when a packet needs sending. The packet contains a
   full ethernet frame to be transmitted. The length of the packet is 
   provided.
  
   Returns 0 if successful or errno if unsuccessful
  
   ********************************************************************     */
int loop_xmit(PIFACE pi, DCU msg)    
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    DCU lb_msg;
    int length;
    PIPPKT pip;
    PIFACE pi_send;
    RTP_BOOL is_802_2;
    
#if (DEBUG_LOOP)
    static int cnt = 0;
    static DCU msg1, msg2, msg3 = 0;

	if (msg != 0)
	{
		if (cnt == 0)
		{
			msg1 = msg;
			cnt++;
			/*  return xmit done     */
			return(REPORT_XMIT_MORE);			
		}
		if (cnt == 1)
		{
			msg2 = msg;
			cnt++;
			/*  return xmit done     */
			return(REPORT_XMIT_MORE);	
		}
		if (cnt == 2)
		{
			msg3 = msg;
			cnt++;
			/*  return xmit done     */
			return(REPORT_XMIT_MORE);	
		}
	}
	if (cnt == 3 || msg == 0)
	{
		cnt = -1;
		if (msg1)
			loop_xmit(pi, msg1);
		if (msg2)
			loop_xmit(pi, msg2);
		if (msg3)
			loop_xmit(pi, msg3);
		cnt = 0;
		msg1 = msg2 = msg3 = 0;
		if (msg == 0)
			return(0);
	}
#endif

  length = msg->length;
  
#if (DEBUG_LOOP)
    DEBUG_ERROR("loop_xmit: packet,len = ", DINT2, DCUTODATA(msg), length);
    DEBUG_ERROR("loop_xmit: PACKET = ", PKT, DCUTODATA(msg), length);
#endif

	

    /* allocate DCU for msg; 
       For non loopback case, msg will be queued and freed when
       sent out only if PKT_FLAG_KEEP is not set.
       But for loopback, copy of msg will not go thru the output queue, but
       will go straight to IP list which will free it
       after interpreting it.  Therefore, copy it to
       a new allocated DCU and the new DCU will be freed after
       it is processed on the input side.  The origional DCU
	   will be freed according to dcu_flags.  The origional DCU
	   is on the output list, therefore, it will signal according
	   to the flags in the origional DCU, therefore, signal flag
	   does not have to be set in the new DCU.
	   IN OTHER WORDS, the packet could have been allocated by the
	   application who would be the only task which should free the
	   packet and the IP task will always free an incoming packet when
	   done, therefore, allocate a new one
       NOTE: the new packet has flags 0, i.e. do not keep, do not signal
    // NOTE: netwrite() will call tc_release_message for the origional packet
             which will signal and free it if requested
           
    */
    
    lb_msg = os_alloc_packet(length, DRIVER_ALLOC); 

    if (!lb_msg)
    {
        RTP_DEBUG_LOG("loop_xmit - os_alloc_packet failed", LEVEL_3, NOVAR, 0, 0);
        return(ENOPKTS);
    }

    /* copy msg to lbmsg which was just allocated 
       (set flags to no keep, no signal)
	 */
	COPY_DCU_FLAGS(lb_msg, msg)

#if (DEBUG_LOOP)
	DEBUG_ERROR("loop_xmit: new pkt = ", PKT, DCUTODATA(lb_msg), length);
	DEBUG_ERROR("           addr = ", DINT1, DCUTODATA(lb_msg), 0);
	DEBUG_ERROR("           length = ", EBS_INT1, length, 0);
#endif

	/* set up phoney dest addr to make sure it is not broadcast; 
	    this is done so icmp will not drop the packet
	 */
	SETUP_LL_HDR_DEST(pi, lb_msg, phony_addr)

	// determine if need 802.2 (SNAP,LLC) header
	is_802_2 = RTP_FALSE;
#if (INCLUDE_802_2)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
	is_802_2 = (RTP_BOOL)(SEND_802_2_PI(pi));
}

#endif

	DCUSETUPPTRS(lb_msg, is_802_2)			

	pip = DCUTOIPPKT(lb_msg);
	pi_send = tc_get_local_pi(pip->ip_dest);
	if (!pi_send)
	{
#if (DEBUG_LOOP)
		DEBUG_ERROR("loop_xmit: pi_send is null", NOVAR, 0, 0);
#endif
		pi_send = pi;
	}

    /* send packet to ip exchange 
       NOTE: interface number is put in DCU by ip tasks
     */
    
    OS_SNDX_IP_EXCHG(pi_send, lb_msg);
    
    rtp_thrd_interrupt_continuation(pi->ctrl.index);
    
    RTP_DEBUG_LOG("loop_xmit - sent lb to ip ex", LEVEL_3, NOVAR, 0, 0);

    /* update statistics */
    loop_packets_out    += 1;
    loop_bytes_out      += length;
    loop_packets_in     += 1;
    loop_bytes_in       += length;

#if 0
    /* TBD
	   signal IP layer that send is done this is exactly 
	   ks_invoke_output except ks_invoke_output needs
	   to work from an interrupt service routine
	 */
	 
	/* Disable interrupts   */
	rtp_irq_disable();			
	pi_send->xmit_done_counter++;
	/* enable interrupts    */
    rtp_irq_enable();			
	OS_SET_IP_SIGNAL(pi_send);
    return (0);
#else
    return(REPORT_XMIT_DONE);            // return xmit done
#endif
    
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
RTP_BOOL loop_statistics(PIFACE pi)                       
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
#if (!INCLUDE_KEEP_STATS)
    ARGSUSED_PVOID(pi)
#endif

    UPDATE_SET_INFO(pi, interface_packets_in, loop_packets_in)
    UPDATE_SET_INFO(pi, interface_packets_out, loop_packets_out)
    UPDATE_SET_INFO(pi, interface_bytes_in, loop_bytes_in)
    UPDATE_SET_INFO(pi, interface_bytes_out, loop_bytes_out)
    UPDATE_SET_INFO(pi, interface_errors_in, loop_errors_in)
    UPDATE_SET_INFO(pi, interface_errors_out, loop_errors_out)
    UPDATE_SET_INFO(pi, interface_packets_lost, 0L)
    return(RTP_TRUE);
}

/* ********************************************************************
    Interrupt
   ********************************************************************     */
void loop_interrupt(PIFACE pi)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
     rtp_thrd_ip_continuation(pi->ctrl.index);
}

/* ********************************************************************
   API
   ********************************************************************     */
int xn_bind_loop(int minor_number)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
	return(xn_device_table_add(loop_device.device_id, 
						minor_number, 
						loop_device.iface_type,
						loop_device.device_name,
					    SNMP_DEVICE_INFO(loop_device.media_mib, 
						                 loop_device.speed)   				        
   				        (DEV_OPEN)loop_device.open,
					    (DEV_CLOSE)loop_device.close,
					    (DEV_XMIT)loop_device.xmit,
					    (DEV_XMIT_DONE)loop_device.xmit_done,
					    (DEV_PROCESS_INTERRUPTS)loop_device.proc_interrupts,
					    (DEV_STATS)loop_device.statistics,
					    (DEV_SETMCAST)loop_device.setmcast));
}


int xn_bind_loop_ether(int minor_number)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
	return(xn_device_table_add(LOOP_ETHER_DEVICE,
						minor_number, 
						loop_device.iface_type,
						loop_device.device_name,
					    SNMP_DEVICE_INFO(loop_device.media_mib, 
						                 loop_device.speed)   				        
   				        (DEV_OPEN)loop_device.open,
					    (DEV_CLOSE)loop_device.close,
					    (DEV_XMIT)loop_device.xmit,
					    (DEV_XMIT_DONE)loop_device.xmit_done,
					    (DEV_PROCESS_INTERRUPTS)loop_device.proc_interrupts,
					    (DEV_STATS)loop_device.statistics,
					    (DEV_SETMCAST)loop_device.setmcast));
}



