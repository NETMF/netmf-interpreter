#include <tinyhal.h>
#include "net_decl.h"
#include "enc28j60_driver.h"

#include "xnconf.h"
#include "rtipconf.h"
#include "rtipapi.h"
#include "socket.h"
#include "pollos.h"
#include "bget.h"
#include "rtpnet.h"
#include "dhcpcapi.h"

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_ENC28J60_Driver"
#endif

ENC28J60_Driver g_ENC28J60_Driver;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif


extern ENC28J60_DEVICE_CONFIG   g_ENC28J60_Config;

extern void enc28j60_pre_interrupt(GPIO_PIN Pin, BOOL PinState, UINT32 Param);

BOOL Network_Interface_Bind(int index)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    if(index >= ARRAYSIZE(g_ENC28J60_Config.DeviceConfigs)) return FALSE;

    return g_ENC28J60_Driver.Bind(&g_ENC28J60_Config.DeviceConfigs[index], index);
}
int  Network_Interface_Open(int index)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    if(index >= ARRAYSIZE(g_ENC28J60_Config.DeviceConfigs)) return -1;
    
    HAL_CONFIG_BLOCK::ApplyConfig( ENC28J60_DEVICE_CONFIG::GetDriverName(), &g_ENC28J60_Config, sizeof(g_ENC28J60_Config) );
    
    return g_ENC28J60_Driver.Open(&g_ENC28J60_Config.DeviceConfigs[index], index);
}
BOOL Network_Interface_Close(int index)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    if(index >= ARRAYSIZE(g_ENC28J60_Config.DeviceConfigs)) return FALSE;

    return g_ENC28J60_Driver.Close(&g_ENC28J60_Config.DeviceConfigs[index], index);
}

int ENC28J60_Driver::Open( ENC28J60_DRIVER_CONFIG* config, int index )
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    int use_default_multicast = 1;

    if(config == NULL) return -1;
    
    memset(&g_ENC28J60_Driver.m_currentDhcpSession, 0, sizeof(g_ENC28J60_Driver.m_currentDhcpSession));

    /* Enable the CHIP SELECT pin */
    if (CPU_GPIO_EnableInputPin (config->SPI_Config.DeviceCS, 
                            FALSE, 
                            NULL,
                            GPIO_INT_NONE,
                            RESISTOR_PULLUP) == FALSE)
    {
        return -1;                          
    }
    
    /* Open the interface first */
    g_ENC28J60_Driver.m_interfaceNumber = xn_interface_open_config(ENC28J60_DEVICE, 
                                                    index,          /*  minor_number        */
                                                    0,              /*  ioaddress           */
                                                    0,              /*  irq value           */
                                                    0               /*  mem_address)        */
                                                    );
    
    if (g_ENC28J60_Driver.m_interfaceNumber == -1)
    {
        return -1;    
    }

    if(index == 0) // default debugger port is index 0
    {
        if (xn_interface_opt(g_ENC28J60_Driver.m_interfaceNumber, 
                            IO_DEFAULT_MCAST,
                            (RTP_PFCCHAR)&use_default_multicast,
                            sizeof(int)) == -1)
        {
            /* Failed to set the default multicast interface */
        }
    }
    
       
    /* Enable the INTERRUPT pin */                            
    if (CPU_GPIO_EnableInputPin2(config->INT_Pin, 
                              FALSE,                                                    /* Glitch filter enable */
                              (GPIO_INTERRUPT_SERVICE_ROUTINE) &enc28j60_pre_interrupt, /* ISR                  */
                              0,                                                        /* minor number         */
                              GPIO_INT_EDGE_LOW ,                                       /* Interrupt edge       */
                              RESISTOR_PULLUP) == FALSE)                                /* Resistor State       */
    {
        return -1;
    }
    
    return g_ENC28J60_Driver.m_interfaceNumber;
    
}

BOOL ENC28J60_Driver::Close( ENC28J60_DRIVER_CONFIG* config, int index )
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    int retVal = -1;

    if(config == NULL) return FALSE;
    
    /* Disable the INTERRUPT pin */                            
    CPU_GPIO_EnableInputPin2(config->INT_Pin, 
                              FALSE,                         /* Glitch filter enable */
                              NULL,                          /* ISR                  */
                              0,                             /* minor number         */
                              GPIO_INT_NONE,                 /* Interrupt edge       */
                              RESISTOR_PULLUP);              /* Resistor State       */
    
    /* JRT - Wait not necessary since doing a HARD_CLOSE below
    xn_wait_pkts_output(RTP_TRUE, 10);
    */
    
    {
        int option_value;

        option_value = 1;
        if (xn_interface_opt(g_ENC28J60_Driver.m_interfaceNumber, IO_HARD_CLOSE,
                             (RTP_PFCHAR)&option_value, sizeof(int)) < 0)       
        {
            RTP_DEBUG_ERROR("ifrtip.c: restart test: xn_interface_opt: HARD_CLOSE failed",
                NOVAR, 0, 0);
        }
    }
    
    /* JRT - changed interface number from 0 */
    retVal = xn_interface_close(g_ENC28J60_Driver.m_interfaceNumber);
    
    
    if (retVal == 0)
    {
        /* JRT - Wait not necessary since just did a HARD_CLOSE above
        xn_wait_pkts_output(RTP_TRUE, 60);
        */
        return TRUE;
    }
    else
    {
        return FALSE;
    }

}

BOOL  ENC28J60_Driver::Bind  ( ENC28J60_DRIVER_CONFIG* config, int index )
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    int retVal;

    if(config == NULL) return FALSE;
    
    retVal = xn_bind_enc28j60(index);
    
    if (retVal != -1)
    {
        return TRUE;
    }
    else
    {
        return FALSE;
    }
}

    


