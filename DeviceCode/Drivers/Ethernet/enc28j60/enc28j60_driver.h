#include "tinyhal.h" 
#include "dhcpcapi.h"

//--//

#ifndef _ENC28J60_DRIVER_H_
#define _ENC28J60_DRIVER_H_ 1

struct ENC28J60_DRIVER_CONFIG
{
    SPI_CONFIGURATION   SPI_Config;
    GPIO_PIN            INT_Pin;

};

#ifndef NETWORK_INTERFACE_COUNT
#define NETWORK_INTERFACE_COUNT 1
#endif

struct ENC28J60_DEVICE_CONFIG
{
    ENC28J60_DRIVER_CONFIG DeviceConfigs[NETWORK_INTERFACE_COUNT];

    static LPCSTR GetDriverName() { return "ENC28J60"; }
};

struct ENC28J60_Driver
{
    int          m_interfaceNumber;
    DHCP_session m_currentDhcpSession;

    //--//
    
    static int  Open   ( ENC28J60_DRIVER_CONFIG* config, int index );
    static BOOL Close  ( ENC28J60_DRIVER_CONFIG* config, int index );
    static BOOL Bind   ( ENC28J60_DRIVER_CONFIG* config, int index );
    
    
};

#endif /* _ENC28J60_DRIVER_H_ */
