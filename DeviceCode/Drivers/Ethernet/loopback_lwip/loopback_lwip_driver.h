#include "tinyhal.h" 

#include "lwip\netif.h"
#include "lwip\pbuf.h"

//--//

#ifndef _LOOPBACK_LWIP_DRIVER_H_
#define _LOOPBACK_LWIP_DRIVER_H_ 1

/* Function table for driver routines needed to bind to stack   */
typedef struct _DriverRoutines
{
    bool  ( * open )      ( struct netif *pNetIF );
    void  ( * close )     ( struct netif *pNetIF );
    err_t ( * xmit )      ( struct netif *pNetIF, struct pbuf *pPBuf);

} DRIVERROUTINES, *PDRIVERROUTINES;


PDRIVERROUTINES loop_lwip_bind( void );  // Defined in loopback_lwip.cpp

        
struct LOOPBACK_LWIP_DRIVER_CONFIG
{
    SPI_CONFIGURATION   SPI_Config;
    GPIO_PIN            CS_Pin;
    GPIO_PIN            INT_Pin;

};

struct LOOPBACK_LWIP_Driver
{
    static int  Open   (   void          );
    static BOOL Close  (   void          );
    static BOOL Bind   (   void          );
};

#endif /* _LOOPBACK_LWIP_DRIVER_H_ */
