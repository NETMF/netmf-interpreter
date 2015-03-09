#include <tinyhal.h>
#include "net_decl.h"
#include "loopback_driver.h"

#include "xnconf.h"
#include "rtipconf.h"
#include "rtipapi.h"
#include "socket.h"
#include "pollos.h"
#include "bget.h"
#include "rtpnet.h"
#include "dhcpcapi.h"

//--//

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_LOOPBACK_Driver"
#endif

LOOPBACK_Driver g_LOOPBACK_Driver;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

//--//

int LOOPBACK_Driver::Open( )
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    RTP_UINT8  MyIpLoopAddress[4]      = {127,0,0,1};
    RTP_UINT8  MyBroadCastAddress[4]   = {255,255,255,0}; 

    g_LOOPBACK_Driver.m_interfaceNumber = xn_interface_open_config(LOOP_DEVICE, 
                                      0,              /*  minor_number        */
                                      0,              /*  ioaddress           */
                                      0,              /*  irq value           */
                                      0               /*  mem_address)        */
                                      );

    if (rtp_net_set_ip(g_LOOPBACK_Driver.m_interfaceNumber,
                        MyIpLoopAddress,
                        MyBroadCastAddress))
    {
        debug_printf("rtp_net_set_ip failed for Loopback driver");
    }
    
    return g_LOOPBACK_Driver.m_interfaceNumber;
}

BOOL LOOPBACK_Driver::Close( )
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    int retVal = -1;
    
    /* JRT - changed interface number from 0 */
    retVal = xn_interface_close(g_LOOPBACK_Driver.m_interfaceNumber);
    
    
    if (retVal == 0)
    {
        return TRUE;
    }
    else
    {
        return FALSE;
    }
}

BOOL  LOOPBACK_Driver::Bind  ( )
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    int retVal;
    
    retVal = xn_bind_loop(0);
    
    if (retVal != -1)
    {
        return TRUE;
    }
    else
    {
        return FALSE;
    }
}

    


