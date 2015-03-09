//
// enc28j60.C - device driver interface
//
// EBSNet - RTIP
//
// Copyright EBSNet , 2007
// All rights reserved.
// This code may not be redistributed in source or linkable object form
// without the consent of its author.
//

#include "xnconf.h"
#include "rtipconf.h"
#include "sock.h"
#include "rtip.h"
#include "sock.h"
#include "rtipapi.h"
#include "rtpirq.h"
#include "rtp.h"
#include "rtpprint.h"
#include "rtpnet.h"

#include <tinyhal.h>

#include "enc28j60.h"

/* ********************************************************************
   DEBUG AIDS
   ******************************************************************** */
#define DEBUG_ENC28J60 0

extern "C"
{
extern void rtp_thrd_interrupt_continuation(int);
extern void rtp_thrd_ip_continuation(int);
}   /* extern "C" */

RTP_BOOL    enc28j60_open       (PIFACE pi);
void        enc28j60_close      (PIFACE pi);
int         enc28j60_xmit       (PIFACE pi, DCU msg);
RTP_BOOL    enc28j60_xmit_done  (PIFACE pi, DCU msg, RTP_BOOL success);
int         enc28j60_recv       (PIFACE pi);
RTP_BOOL    enc28j60_statistics (PIFACE  pi);
void        enc28j60_interrupt  (PIFACE  pi);
void        enc28j60_pre_interrupt (GPIO_PIN Pin, BOOL PinState, UINT32 minorNumber);
void        enc28j60_setup_recv_buffer(PIFACE pi, SPI_CONFIGURATION  *SpiConf);

/* ********************************************************************
   GLOBAL DATA
   ******************************************************************** */
EDEVTABLE RTP_FAR enc28j60_device = 
{
     enc28j60_open, 
     enc28j60_close, 
     enc28j60_xmit, 
     NULLP_FUNC,
     NULLP_FUNC, 
     enc28j60_statistics, 
     NULLP_FUNC, 
     ENC28J60_DEVICE, 
     "ENC28J60", 
     MINOR_0, 
     ETHER_IFACE, 
     SNMP_DEVICE_INFO(CFG_OID_ENC28J60, CFG_SPEED_ENC28J60)
     CFG_ETHER_MAX_MTU, 
     CFG_ETHER_MAX_MSS, 
     CFG_ETHER_MAX_WIN_IN, 
     CFG_ETHER_MAX_WIN_OUT, 
     IOADD(0), 
     EN(0), 
     EN(0)
};


extern  NETWORK_CONFIG              g_NetworkConfig;
extern  ENC28J60_DEVICE_CONFIG      g_ENC28J60_Config;

// ********************************************************************
// EXTERNS
// ********************************************************************

ENC28J60_SOFTC RTP_FAR enc28j60softc[CFG_NUM_ENC28J60];


/* Function Prototypes */
void    enc28j60_soft_reset(SPI_CONFIGURATION *spiConf);

void    enc28j60_write_spi(SPI_CONFIGURATION *spiConf, 
                        RTP_UINT8 opcode, 
                        RTP_UINT8 address, 
                        RTP_PFUINT8 byteData, 
                        RTP_UINT32 numBytes);

void    enc28j60_read_spi(SPI_CONFIGURATION *spiConf, 
                        RTP_UINT8 opcode, 
                        RTP_UINT8 address, 
                        RTP_PFUINT8 byteData, 
                        RTP_UINT32 numBytes,
                        RTP_UINT8 offset);
                        
void    enc28j60_select_bank(SPI_CONFIGURATION *spiConf, 
                            RTP_UINT8 bankNumber);

void    enc28j60_write_phy_register(SPI_CONFIGURATION *spiConf, 
                                RTP_UINT8 registerAddress, 
                                unsigned short data);
                                
unsigned short enc28j60_read_phy_register(SPI_CONFIGURATION *spiConf, 
                                            RTP_UINT8 registerAddress);

RTP_BOOL    enc28j60_setup_device(PIFACE pi);

/* ********************************************************************
   
   RTIP_STATIC PENC28J60_SOFTC iface_to_softc(PIFACE pi)
   
   ******************************************************************** */
   
#if (CFG_NUM_ENC28J60 == 1)

#define iface_to_softc(X) (PENC28J60_SOFTC) &enc28j60softc[0]
#define off_to_softc(X)   (PENC28J60_SOFTC) &enc28j60softc[0]

#else

RTIP_STATIC PENC28J60_SOFTC iface_to_softc(PIFACE pi)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
RTP_UINT16 softc_off;

    if (!pi)
    {
        return((PENC28J60_SOFTC)0); 
    }
    
    softc_off = pi->minor_number;
    if (softc_off >= CFG_NUM_ENC28J60)
    {
        RTP_DEBUG_ERROR("iface_to_softc() - pi->minor_number, CFG_NUM_ENC28J60 =",
            EBS_INT2, pi->minor_number, CFG_NUM_ENC28J60);
        return((PENC28J60_SOFTC)0);
    }

    return((PENC28J60_SOFTC) &enc28j60softc[softc_off]);
}

/* ********************************************************************

   RTIP_STATIC PENC28J60_SOFTC off_to_softc(RTP_UINT16 softc_off)
    
   ******************************************************************** */
RTIP_STATIC PENC28J60_SOFTC off_to_softc(RTP_UINT16 softc_off)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    if (softc_off >= CFG_NUM_ENC28J60)
    {
        RTP_DEBUG_ERROR("off_to_softc() - softc_off, CFG_NUM_ENC28J60 =",
            EBS_INT2, softc_off, CFG_NUM_ENC28J60);
        return((PENC28J60_SOFTC)0);
    }
    return((PENC28J60_SOFTC) &enc28j60softc[softc_off]);
}

#endif

/* ********************************************************************
   open the ENC28J60 driver interface.
   
   This routine opens a ENC28J60 device driver
  
   The address of this function must be placed into the "devices" table in
   iface.c either at compile time or before a device open is called.
  
  ******************************************************************** */  
RTP_BOOL enc28j60_open(PIFACE pi)                            
    
{
    PENC28J60_SOFTC     sc;
    
    sc = iface_to_softc(pi);
    
    if (!sc)
    {
        RTP_DEBUG_ERROR("enc28j60_xmit: softc invalid", NOVAR, 0, 0);
        
        /* JRT */
        set_errno(ENUMDEVICE);
        return(RTP_FALSE);
    }
    
    sc->iface = pi;

    int macLen = __min(g_NetworkConfig.NetworkInterfaces[0].macAddressLen, sizeof(sc->mac_address));

    if(macLen > 0)
    {
        memcpy(&sc->mac_address[0], &g_NetworkConfig.NetworkInterfaces[0].macAddressBuffer[0], macLen);
    }
    else
    {
        debug_printf("Device initialize without MAC address!!!\r\n");
    }
    
    /* Now put in a dummy ethernet address */
    rtp_memcpy(&pi->addr.my_hw_addr[0], sc->mac_address, 6); // Get the ethernet address
    
    /* clear statistic information */
    sc->stats.packets_in     = 0L;
    sc->stats.packets_out    = 0L;
    sc->stats.bytes_in       = 0L;
    sc->stats.bytes_out      = 0L;
    sc->stats.errors_in      = 0L;
    sc->stats.errors_out     = 0L;    
    
    if(RTP_FALSE == enc28j60_setup_device(pi))
    {
        return RTP_FALSE;
    }
    
    rtp_irq_hook_interrupt( (RTP_PFVOID) pi, 
                            (RTP_IRQ_FN_POINTER)enc28j60_interrupt, 
                            (RTP_IRQ_FN_POINTER) 0);
    
    
#if (DEBUG_ENC28J60)
    {   
        DCU             msg;
        RTP_PFUINT16    dataTX;
   
        int             i;
        unsigned char   myBuf[] = {
            0x00,0x0b,0xdb,0xb1,0xe2,0x5e,0xba,0xad,0xca,0xab,0x04,0x05,
            0x08,0x06,0x00,0x01,0x08,0x00,0x06,0x04,0x00,0x02,0xba,0xad,
            0xca,0xab,0x04,0x05,0xc0,0xa8,0x00,0x0d,0x00,0x0b,0xdb,0xb1,
            0xe2,0x5e,0xc0,0xa8,0x00,0x05,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x9a };
        
        msg = os_alloc_packet_input(65, DRIVER_ALLOC); 
        
        
        msg->length = 65;
        
        
        for (i = 0; i < 70; i ++ )
        {
            myBuf[31] = i;
            dataTX = (RTP_PFUINT16)DCUTODATA(msg);
            rtp_memcpy(dataTX, myBuf, 65);
            enc28j60_xmit(pi, msg);
        }
    }
#endif

    return(RTP_TRUE);
}

/* ********************************************************************
   close the packet driver interface.
   
   This routine is called when the device interface is no longer needed
   it should stop the driver from delivering packets to the upper levels
   and shut off packet delivery to the network.
  
   The address of this function must be placed into the "devices" table in
   iface.c either at compile time or before a device open is called.
  
 */  
  
  
void enc28j60_close(PIFACE pi)                                /*__fn__*/
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    PENC28J60_SOFTC     sc;
    SPI_CONFIGURATION  *SpiConf;
    UINT8               byteData;

    sc = iface_to_softc(pi);
    SpiConf = &g_ENC28J60_Config.DeviceConfigs[pi->minor_number].SPI_Config;

    /* turn off interrupts */
    byteData = ((1 << ENC28J60_EIE_INTIE_BIT) | (1 << ENC28J60_EIE_PKTIE_BIT) | (1 << ENC28J60_EIE_TXIE_BIT) |(1 << ENC28J60_EIE_TXERIE_BIT));
    enc28j60_write_spi(SpiConf, ENC28J60_SPI_BIT_FIELD_CLEAR_OPCODE, ENC28J60_EIE, &byteData, 1);    
}

/* ********************************************************************
   Interrupt handling routine at the task level
   
   ******************************************************************** */
#define TRANSMIT_RETRIES 5
static RTP_UINT8 s_retriesTransmit = TRANSMIT_RETRIES;


void enc28j60_handle_recv_error(PIFACE pi, SPI_CONFIGURATION  *SpiConf)
{
    RTP_UINT8 byteData;

    /* Reset the receiver logic */
    byteData = (1 << ENC28J60_ECON1_RXRST_BIT);
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_BIT_FIELD_SET_OPCODE, ENC28J60_ECON1, &byteData, 1);
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_BIT_FIELD_CLEAR_OPCODE, ENC28J60_ECON1, &byteData, 1);

    enc28j60_read_spi(SpiConf,  ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_ESTAT, &byteData, 1, 0);  

    /* buffer is corrupted flush it */
    if(byteData & (1 << ENC28J60_ESTAT_BUFER))
    {
        enc28j60_setup_recv_buffer(pi, SpiConf);
    }    

    byteData = (1 << ENC28J60_EIR_RXERIF_BIT);
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_BIT_FIELD_CLEAR_OPCODE, ENC28J60_EIR, &byteData, 1);

    byteData = ((1 << ENC28J60_EIE_RXERIE_BIT) | (1 << ENC28J60_EIE_INTIE_BIT));
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_BIT_FIELD_SET_OPCODE, ENC28J60_EIE, &byteData, 1);

    byteData = (1 << ENC28J60_ECON1_RXEN_BIT);
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_BIT_FIELD_SET_OPCODE, ENC28J60_ECON1, &byteData, 1);

    /* Is there an interrupt pending */
    enc28j60_read_spi(SpiConf,  ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_ESTAT, &byteData, 1, 0);
}

void enc28j60_handle_xmit_error(PIFACE pi, SPI_CONFIGURATION  *SpiConf)
{
    RTP_UINT8 byteData;
    
    byteData = (1 << ENC28J60_ECON1_TXRST_BIT);
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_BIT_FIELD_SET_OPCODE, ENC28J60_ECON1, &byteData, 1);
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_BIT_FIELD_CLEAR_OPCODE, ENC28J60_ECON1, &byteData, 1);

    byteData = (1 << ENC28J60_EIR_TXERIF_BIT) | (1 << ENC28J60_EIR_TXIF_BIT);
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_BIT_FIELD_CLEAR_OPCODE, ENC28J60_EIR, &byteData, 1);        

    byteData = ((1 << ENC28J60_EIE_TXERIE_BIT) | (1 << ENC28J60_EIE_INTIE_BIT));
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_BIT_FIELD_SET_OPCODE, ENC28J60_EIE, &byteData, 1);

    byteData = (1 << ENC28J60_ECON1_TXRTS_BIT);

    /* CLEAR the TXRTS bit to cancel the last transmission */
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_BIT_FIELD_CLEAR_OPCODE, ENC28J60_ECON1, &byteData, 1);                

    if(s_retriesTransmit--)
    {
        /* SET the TXRTS bit to retry the last transmission */
        enc28j60_write_spi(SpiConf,  ENC28J60_SPI_BIT_FIELD_SET_OPCODE, ENC28J60_ECON1, &byteData, 1);
    }
    else
    {
        s_retriesTransmit = TRANSMIT_RETRIES;
    }
}

void        enc28j60_pre_interrupt  (GPIO_PIN Pin, BOOL PinState, UINT32 minorNumber)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    PIFACE              pi;
    PENC28J60_SOFTC     sc;
    RTP_UINT8           byteData;
    RTP_UINT8           eirData;
    SPI_CONFIGURATION  *SpiConf = &g_ENC28J60_Config.DeviceConfigs[minorNumber].SPI_Config;

    
    GLOBAL_LOCK(encIrq);
    
    /* Check the minor number */
    if (minorNumber > CFG_NUM_ENC28J60)
    {
        return;
    }
    
    /* get pi structure based on the minor number   */
    sc = off_to_softc(minorNumber);
    
    pi = sc->iface;
    
    if (!pi)
    {
        return;
    }
    
    /* After an interrupt occurs, the host controller should
        clear the global enable bit for the interrupt pin before
        servicing the interrupt. Clearing the enable bit will
        cause the interrupt pin to return to the non-asserted
        state (high). Doing so will prevent the host controller
        from missing a falling edge should another interrupt
        occur while the immediate interrupt is being serviced.
     */
    
    byteData = (1 << ENC28J60_EIE_INTIE_BIT);
    enc28j60_write_spi(SpiConf, ENC28J60_SPI_BIT_FIELD_CLEAR_OPCODE, ENC28J60_EIE, &byteData, 1);

    enc28j60_read_spi(SpiConf,  ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_EIR, &eirData, 1, 0);  

    /* recover from tx error */
    if (eirData & (1 << ENC28J60_EIR_TXERIF_BIT))
    {            
        enc28j60_handle_xmit_error(pi, SpiConf);
    }

    
#if DEBUG_ENC28J60
    enc28j60_read_spi(SpiConf,  ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_ESTAT, &byteData, 1, 0);
    enc28j60_read_spi(SpiConf,  ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_ECON1, &byteData, 1, 0);
#endif

    rtp_thrd_interrupt_continuation(pi->ctrl.index);

    /* Disable ISR calls */
    //CPU_GPIO_EnableInputPin2(g_ENC28J60_Config.INT_Pin, FALSE, NULL, NULL, GPIO_INT_NONE, RESISTOR_DISABLED );
    
    
}

/* ********************************************************************
   Interrupt handling routine at the task level
   
   ******************************************************************** */
void enc28j60_interrupt(PIFACE pi)

{                       
    RTP_UINT8        byteData;
    RTP_UINT8        cntPkts;
    RTP_UINT8        eirData;
    RTP_UINT8        status;
    PENC28J60_SOFTC  sc;
    int              packetsLeft = 0;
    //int              statusVectorPointer;
    //RTP_UINT8        statusVector[7];
    SPI_CONFIGURATION  *SpiConf;
    
    GLOBAL_LOCK(encIrq);
    
    if (!pi)
    {
        return;
    }

    sc = iface_to_softc(pi);

    SpiConf = &g_ENC28J60_Config.DeviceConfigs[pi->minor_number].SPI_Config;
    
    /* Is there an interrupt pending */
    enc28j60_read_spi(SpiConf,  ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_ESTAT, &status, 1, 0);
    
    
    
    /*  FROM ERRATA 
        Module: Interrupts The Receive Packet Pending Interrupt Flag (EIR.PKTIF) does not reliably/accurately report
        the status of pending packets. Work around In the Interrupt Service Routine, if it is unknown if
        a packet is pending and the source of the interrupt is unknown, switch to Bank 1 and check the value
        in EPKTCNT. If polling to see if a packet is pending, check the value in EPKTCNT.
        
        //enc28j60_read_spi(SpiConf,  ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_EIR, &byteData, 1, 0);        
        //if (byteData & (1 << ENC28J60_EIR_PKTIF_BIT))
    */
    
    /* Read the number of packets remaining */
    
    enc28j60_select_bank(SpiConf, ENC28J60_CONTROL_REGISTER_BANK1);
    enc28j60_read_spi(SpiConf, ENC28J60_SPI_READ_BUFFER_MEMORY_OPCODE, ENC28J60_EPKTCNT, &cntPkts, 1, 0);  
    
    
    if ((status & (1 << ENC28J60_ESTAT_INT)) || cntPkts)
    {
        enc28j60_read_spi(SpiConf,  ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_EIR, &eirData, 1, 0);  
        
        if (eirData & (1 << ENC28J60_EIR_TXERIF_BIT))
        {            
            enc28j60_handle_xmit_error(pi, SpiConf);
        }
        else if (eirData & (1 << ENC28J60_EIR_TXIF_BIT)) /* A packet is xmited */
        {
            s_retriesTransmit = TRANSMIT_RETRIES;

            /* signal IP task that xmit has completed */
            rtp_net_invoke_output(pi, 1);
        }

        /* recover from rx error */
        if (eirData & (1 << ENC28J60_EIR_RXERIF_BIT))
        {
            enc28j60_handle_recv_error(pi, SpiConf);
            
            sc->recv_start_buffer  = ENC28J60_RECEIVE_BUFFER_START;
        }

        if (cntPkts)
        {
            packetsLeft = enc28j60_recv(pi);
        }
    }
    
    /* enable packet reception */
    enc28j60_read_spi(SpiConf, ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_ECON1, &byteData, 1, 0);
    
    /* Is the receiver or transmitter in reset state */
    if ( (byteData & (1 << ENC28J60_ECON1_TXRST_BIT)) || (byteData & (1 << ENC28J60_ECON1_TXRST_BIT)) )
    {
        /* If so, take it out of reset */
        byteData &= ~((1 << ENC28J60_ECON1_TXRST_BIT) | (1 << ENC28J60_ECON1_TXRST_BIT));
    }

    byteData |= (1 << ENC28J60_ECON1_RXEN_BIT);
    enc28j60_write_spi(SpiConf, ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE, ENC28J60_ECON1, &byteData, 1);        
        
    /* clear errors if there are any */
    byteData = 0xFF;
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_BIT_FIELD_CLEAR_OPCODE, ENC28J60_ESTAT, &byteData, 1);
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_BIT_FIELD_CLEAR_OPCODE, ENC28J60_EIR, &byteData, 1);
    
    
    /* re-enable interrupts when a packet is received and when transmit is done  */ 
    byteData = (1 << ENC28J60_EIE_INTIE_BIT); 
    enc28j60_write_spi(SpiConf, ENC28J60_SPI_BIT_FIELD_SET_OPCODE, ENC28J60_EIE, &byteData, 1); 
        
    /* If no packets are left   */
    /* Enable the INTERRUPT pin */                            
    //CPU_GPIO_EnableInputPin2(g_ENC28J60_Config.INT_Pin, 
    //                      FALSE,                                                     /* Glitch filter enable */
    //                      (GPIO_INTERRUPT_SERVICE_ROUTINE) &enc28j60_pre_interrupt, /* ISR                  */
    //                      0,                                                        /* minor number         */
    //                      GPIO_INT_EDGE_BOTH,                                       /* Interrupt edge       */
    //                      RESISTOR_PULLUP);                                         /* Resistor State       */
    
    if (packetsLeft)
    {
        /* If there are more packets left re-queue the continuation */
        rtp_thrd_interrupt_continuation(pi->ctrl.index);
    }
    
}

/* ********************************************************************
   Receive. a packet over the packet driver interface.  This is called
   from the ISR or task ISR.
   
   This routine is called when a packet is received. 
    
   Upon successful reading of received packet, the IP is
   signalled to process the packet.
   
   Returns the number of packets that remain to be processed

  ******************************************************************** */
int        enc28j60_recv(PIFACE pi)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    DCU                     msg;
    RTP_PFUINT8             dataTX;
    RTP_UINT8               nextPktAndRecvStatusVector[6];
    RTP_UINT16              length;
    RTP_UINT8               byteData;
    RTP_UINT8               packetsLeft;
    PENC28J60_SOFTC         sc;
    RTP_UINT16              lastReceiveBuffer;
    SPI_CONFIGURATION*      SpiConf;
    
    int                     numPacketsProcessed = 0;
    
    if (!pi)
    {
        return 1;
    }
    
    sc = iface_to_softc(pi);

    SpiConf = &g_ENC28J60_Config.DeviceConfigs[pi->minor_number].SPI_Config;

    do 
    {
        
        {
            /* Disable interrupt for each loop and only each loop */
            GLOBAL_LOCK(encIrq);

            /* Set the read buffer pointer to the beginning of the packet */
            enc28j60_select_bank(SpiConf, ENC28J60_CONTROL_REGISTER_BANK0);
            byteData = sc->recv_start_buffer & 0xFF;
            enc28j60_write_spi(SpiConf, ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE, ENC28J60_ERDPTL, &byteData, 1);
            byteData = (sc->recv_start_buffer >> 8) & 0xFF;
            enc28j60_write_spi(SpiConf, ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE, ENC28J60_ERDPTH, &byteData, 1);

            /* Get the next packet pointer */
            enc28j60_read_spi(SpiConf, ENC28J60_SPI_READ_BUFFER_MEMORY_OPCODE, 
                                ENC28J60_SPI_READ_BUFFER_MEMORY_ARGUMENT, 
                                nextPktAndRecvStatusVector, 6, 0);   
            
            lastReceiveBuffer = (nextPktAndRecvStatusVector[1] << 8) | nextPktAndRecvStatusVector[0];
            length = (nextPktAndRecvStatusVector[3] << 8) | nextPktAndRecvStatusVector[2];

            /* corrupted */
            if(lastReceiveBuffer > ENC28J60_RECEIVE_BUFFER_END)
            {
                enc28j60_handle_recv_error(pi, SpiConf);
                packetsLeft = 0;
                sc->recv_start_buffer  = ENC28J60_RECEIVE_BUFFER_START;
                break;
            }
            else
            {
                sc->recv_start_buffer = lastReceiveBuffer;
            }

            if (length != 0)
            {
                msg = os_alloc_packet_input(length, DRIVER_ALLOC); 
                
                if (msg)
                {
                    dataTX = (RTP_PFUINT8)DCUTODATA(msg);

                    //remove the checksum trailing bytes
                    if (length > 63)
                    {
                        length -= 4;
                    }
                    
                    /* Get the packet */
                    enc28j60_read_spi(SpiConf, ENC28J60_SPI_READ_BUFFER_MEMORY_OPCODE, 
                                    ENC28J60_SPI_READ_BUFFER_MEMORY_ARGUMENT, 
                                    dataTX, 
                                    length, 
                                    0);   
                                    
                    /* signal IP layer that a packet is on its exchange */
                    rtp_net_invoke_input(pi, msg, length);                         
                }
                else
                {
                    RTP_DEBUG_ERROR("enc28j60_recv: input alloc packet failed", NOVAR, 0, 0);
                }   
            }

            if(0 == (sc->recv_start_buffer % 2))
            {
                /* from errata rev.b5 - circular buffer doesn't handle even numbers well -> nextPkt is guarranteed to be even */
                if(((sc->recv_start_buffer - 1) < ENC28J60_RECEIVE_BUFFER_START) || 
                    ((sc->recv_start_buffer - 1) > ENC28J60_RECEIVE_BUFFER_END))
                {
                    lastReceiveBuffer = ENC28J60_RECEIVE_BUFFER_END;
                }
                else
                {
                    lastReceiveBuffer = sc->recv_start_buffer - 1;
                }
            }
            else
            {
                lastReceiveBuffer = sc->recv_start_buffer;
            }

            /* Free the packet from the ethernet */            
            byteData = lastReceiveBuffer & 0xFF;
            enc28j60_write_spi(SpiConf, ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE, ENC28J60_ERXRDPTL, &byteData, 1);
            
            byteData = lastReceiveBuffer >> 8;
            enc28j60_write_spi(SpiConf, ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE, ENC28J60_ERXRDPTH, &byteData, 1);
                                  
            /* the host controller must write a 1 to the ECON2.PKTDEC bit. 
                Doing so will cause the EPKTCNT register to decrement by 1 */
            byteData = (1 << ENC28J60_ECON2_PKTDEC_BIT);
            enc28j60_write_spi(SpiConf, ENC28J60_SPI_BIT_FIELD_SET_OPCODE, ENC28J60_ECON2, &byteData, 1); 
            
            /* Read the number of packets remaining*/
            enc28j60_select_bank(SpiConf, ENC28J60_CONTROL_REGISTER_BANK1);
            enc28j60_read_spi(SpiConf, ENC28J60_SPI_READ_BUFFER_MEMORY_OPCODE, ENC28J60_EPKTCNT, &packetsLeft, 1, 0);   
                                
            if ( (++numPacketsProcessed > CFG_MAX_PACKETS_PROCESSED) && packetsLeft)
            {
                break;
            }
            
        }
    } while (packetsLeft);    
        
#if (DEBUG_ENC28J60)
    enc28j60_read_spi(SpiConf,  ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_EIR, &byteData, 1, 0);
#endif

    return packetsLeft;
    
}
    
/* ********************************************************************
   Transmit. a packet over the packet driver interface.
   
   This routine is called when a packet needs sending. The packet contains a
   full ethernet frame to be transmitted. The length of the packet is 
   provided.
  
   Returns 0 if successful or errno if unsuccessful

 */
  
int enc28j60_xmit(PIFACE pi, DCU msg)    /*__fn__*/
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    RTP_UINT16              length = 0;
    PENC28J60_SOFTC         sc;
    RTP_PFUINT8             dataToSPI;
    RTP_PFUINT16            dataTX;
    RTP_UINT8               perPacketControlByte;
    RTP_UINT8               dataByte;
    SPI_CONFIGURATION*      SpiConf;
    DCU                     tmpMsg;
        
    GLOBAL_LOCK(encIrq);
    
    if (!pi)
    {
        return (-1);
    }
    
    sc = iface_to_softc(pi);
    
    if (!sc)
    {
        RTP_DEBUG_ERROR("enc28j60_xmit: softc invalid", NOVAR, 0, 0);
        return(ENUMDEVICE);
    }

    SpiConf = &g_ENC28J60_Config.DeviceConfigs[pi->minor_number].SPI_Config;

     
    enc28j60_read_spi(SpiConf, ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_ECON1, &dataByte, 1, 0);
    
    
    length = msg->length;
    
    if (length < ETHER_MIN_LEN)
    {
        length = ETHER_MIN_LEN;
    }
    
    if (length > (ETHERSIZE+4))
    {
        RTP_DEBUG_ERROR("xmit - length is too large, truncated", NOVAR, 0, 0);
        length = ETHERSIZE+4;         /* what a terriable hack! */
    }
    
    tmpMsg = os_alloc_packet(length+2, DRIVER_ALLOC);
    
    if (!tmpMsg)
    {
        return (ENOPKTS);
    }
    
    dataTX = (RTP_PFUINT16)DCUTODATA(msg);

    /* First see if there is enough space in the remainder of the transmit buffer */
    /* 7 for the status bytes and 1 for the control byte */
    if ((length + 7 + 1 + (RTP_UINT16)sc->xmit_start_buffer) > ENC28J60_TRANSMIT_BUFFER_END )
    {
        sc->xmit_start_buffer = ENC28J60_TRANSMIT_BUFFER_START;
    }
    
    /*  1. Appropriately program the ETXST pointer to
            point to an unused location in memory. It will
            point to the per packet control byte. 
        2. Use the WBM SPI command to write the per
            packet control byte, the destination address, the
            source MAC address, the type/length and the
            data payload.
        3. Appropriately program the ETXND pointer. It
            should point to the last byte in the data payload.  
        4. Clear EIR.TXIF, set EIE.TXIE and set EIE.INTIE
            to enable an interrupt when done (if desired).
        5. Start the transmission process by setting
            ECON1.TXRTS.
    */
    
    
    /* Making sure to select the right bank */
    enc28j60_select_bank(SpiConf, ENC28J60_CONTROL_REGISTER_BANK0);
    
    /* setup the EWRPTL AND EWRPTH to the beginning of the transmit buffer */
    dataByte = sc->xmit_start_buffer & 0xFF;
    enc28j60_write_spi(SpiConf, ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE, ENC28J60_EWRPTL, &dataByte, 1);
    dataByte = (sc->xmit_start_buffer >> 8) & 0xFF;
    enc28j60_write_spi(SpiConf, ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE, ENC28J60_EWRPTH, &dataByte, 1);
    
    /* 1. */
    dataByte = sc->xmit_start_buffer & 0xFF;
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_ETXSTL, &dataByte, 1);
    dataByte = (sc->xmit_start_buffer >> 8) & 0xFF;
    enc28j60_write_spi(SpiConf, ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_ETXSTH, &dataByte, 1);   
     
    /* 2. */
    /* Write per byte control byte and the first byte of data */
    perPacketControlByte = (1 << ENC28J60_XMIT_CONTROL_PPADEN_BIT) |
                           (1 << ENC28J60_XMIT_CONTROL_PCRCEN_BIT) ;
    
    
    dataToSPI = (RTP_PFUINT8)DCUTODATA(tmpMsg);
    dataToSPI[1] =  perPacketControlByte;
    rtp_memcpy(&dataToSPI[2], dataTX, length);
    enc28j60_write_spi(SpiConf, ENC28J60_SPI_WRITE_BUFFER_MEMORY_OPCODE, ENC28J60_SPI_WRITE_BUFFER_MEMORY_ARGUMENT, dataToSPI, length+1);
    os_free_packet(tmpMsg);
    
    sc->xmit_start_buffer += length;
    
    /* 3. */
    /* Making sure to select the right bank */
    enc28j60_select_bank(SpiConf, ENC28J60_CONTROL_REGISTER_BANK0);
    dataByte = sc->xmit_start_buffer & 0xFF;
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_ETXNDL, &dataByte, 1);
    dataByte = (sc->xmit_start_buffer >> 8) & 0xFF;
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_ETXNDH, &dataByte, 1);
    
    /* TAKE CARE OF WRAP HERE */
    /* Leave 7 bytes for the status Vector + 1 for control byte */
    sc->xmit_start_buffer += 8;
    
    /* Make sure that it is on an even address */
    if ((sc->xmit_start_buffer % 2) != 0)  sc->xmit_start_buffer++;
    
    /* 4. */
    dataByte = 1 << ENC28J60_EIR_TXIF_BIT;
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_BIT_FIELD_CLEAR_OPCODE, ENC28J60_EIR, &dataByte, 1);
    
    dataByte = ((1 << ENC28J60_EIE_TXIE_BIT) | (1 << ENC28J60_EIE_INTIE_BIT) | ((1 << ENC28J60_EIE_TXERIE_BIT)));
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_BIT_FIELD_SET_OPCODE, ENC28J60_EIE, &dataByte, 1);

    /* 5. */
    dataByte = (1 << ENC28J60_ECON1_TXRTS_BIT);
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_BIT_FIELD_SET_OPCODE, ENC28J60_ECON1, &dataByte, 1);

    /* update statistics */
    sc->stats.packets_out += 1;
    sc->stats.bytes_out += length;

    sc->stats.packets_in += 1;
    sc->stats.bytes_in += length;

    
    return (0);
}

/* ********************************************************************
                                                                          
   enc28j60_xmit_done() - process a completed transmit                      
                                                                          
   This routine is called as a result of the transmit complete            
   interrupt occuring (see ks_invoke_output).                             
                                                                          
   Inputs:                                                                
     pi     - interface structure                                         
     DCU    - packet transmitting                                         
     status - RTP_TRUE indicates the xmit completed successfully, RTP_FALSE
              indicates it did not (possible errors include               
              timeout etc)                                                
                                                                          
   Returns: RTP_TRUE if xmit done or error                                
            RTP_FALSE if xmit not done; if it is not done when the        
                  next xmit interrupt occurs, enc28j60_xmit_done will       
                  be called again                                         
                                                                          
   ********************************************************************   */

RTP_BOOL    enc28j60_xmit_done  (PIFACE pi, DCU msg, RTP_BOOL success)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    PENC28J60_SOFTC   sc;

    ARGSUSED_PVOID(msg);

    if (!pi)
    {
        return (RTP_FALSE);
    }
    
    sc = iface_to_softc(pi);
    if (!sc)
        return(RTP_TRUE);

    if (!success)
    {
        sc->stats.errors_out++;
        sc->stats.tx_other_errors++;
    }
    else
    {
        /* Update total number of successfully transmitted packets. */
        sc->stats.packets_out++;
        sc->stats.bytes_out += msg->length;
    }
    return(RTP_TRUE);
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
  
   ******************************************************************** */
RTP_BOOL enc28j60_statistics(PIFACE pi)                       /*__fn__*/
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
#if (!INCLUDE_KEEP_STATS)
    ARGSUSED_PVOID(pi)
#endif

    if (!pi)
    {
        return RTP_FALSE;
    }
    
    UPDATE_SET_INFO(pi, interface_packets_in, enc28j60_packets_in)
    UPDATE_SET_INFO(pi, interface_packets_out, enc28j60_packets_out)
    UPDATE_SET_INFO(pi, interface_bytes_in, enc28j60_bytes_in)
    UPDATE_SET_INFO(pi, interface_bytes_out, enc28j60_bytes_out)
    UPDATE_SET_INFO(pi, interface_errors_in, enc28j60_errors_in)
    UPDATE_SET_INFO(pi, interface_errors_out, enc28j60_errors_out)
    UPDATE_SET_INFO(pi, interface_packets_lost, 0L)
    return(RTP_TRUE);
}

int xn_bind_enc28j60(int minor_number)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
	return(xn_device_table_add(enc28j60_device.device_id,
						minor_number, 
						enc28j60_device.iface_type,
						enc28j60_device.device_name,
					    SNMP_DEVICE_INFO(enc28j60_device.media_mib, 
						                 enc28j60_device.speed)   				        
   				        (DEV_OPEN)enc28j60_device.open,
					    (DEV_CLOSE)enc28j60_device.close,
					    (DEV_XMIT)enc28j60_device.xmit,
					    (DEV_XMIT_DONE)enc28j60_device.xmit_done,
					    (DEV_PROCESS_INTERRUPTS)enc28j60_device.proc_interrupts,
					    (DEV_STATS)enc28j60_device.statistics,
					    (DEV_SETMCAST)enc28j60_device.setmcast));
}

void enc28j60_setup_recv_buffer(PIFACE pi, SPI_CONFIGURATION  *SpiConf)
{
    RTP_UINT8 byteData;
    PENC28J60_SOFTC         sc;
    
    sc = iface_to_softc(pi);
    
    /* Making sure to select the right bank */
    enc28j60_select_bank( SpiConf, ENC28J60_CONTROL_REGISTER_BANK0);
    
    /* keep track of the receive pointer */    
    sc->recv_start_buffer = ENC28J60_RECEIVE_BUFFER_START;
    
    /* Specifying the receive buffer size */
        /* First, write the low byte of the starting address*/
    byteData = ENC28J60_RECEIVE_BUFFER_START & 0xFF;
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_ERXSTL, &byteData, 1);

        /* Then, write the upper byte of the starting address*/
    byteData = (ENC28J60_RECEIVE_BUFFER_START >> 8) & 0xFF;
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_ERXSTH, &byteData, 1);
                        
        /* First, write the low byte of the ending address*/
    byteData = ENC28J60_RECEIVE_BUFFER_END & 0xFF;
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_ERXNDL, &byteData, 1);
        /* Then, write the upper byte of the starting address*/
    byteData = ((ENC28J60_RECEIVE_BUFFER_END >> 8) & 0xFF);
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_ERXNDH, &byteData, 1);                            

    enc28j60_read_spi(SpiConf, ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_ERXWRPTL, &byteData, 1, 0);    
    enc28j60_read_spi(SpiConf, ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_ERXWRPTH, &byteData, 1, 0);    

    /* The ERXRDPT register should be program with the same value
        as ERXST in the beginning */
    /* LOOK AT ERRATA */    
    //byteData = (ENC28J60_RECEIVE_BUFFER_START-1) & 0xFF;
    byteData = ENC28J60_RECEIVE_BUFFER_END & 0xFF;
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_ERXRDPTL, &byteData, 1);

    //byteData = (((ENC28J60_RECEIVE_BUFFER_START-1)>> 8) & 0xFF);
    byteData = ((ENC28J60_RECEIVE_BUFFER_END) >> 8 ) & 0xFF;
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_ERXRDPTH, &byteData, 1);
    
}

RTP_BOOL enc28j60_setup_device(PIFACE pi)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    RTP_UINT8               byteData;
    PENC28J60_SOFTC         sc;
    RTP_UINT16              duplex;
    RTP_UINT16              phyID1;
    RTP_UINT16              phyID2;
    RTP_UINT16              shortData;
    RTP_INT16               nLoopCnt = 0;
    SPI_CONFIGURATION*      SpiConf;
    
    sc = iface_to_softc(pi);

    SpiConf = &g_ENC28J60_Config.DeviceConfigs[pi->minor_number].SPI_Config;

    
    /* ---------------------------------------------------------------------------------------------------- */                        
    /*                                          CHECK IF THE PHY IS READY                                   */
    
    /* Making sure to select the right bank */
    enc28j60_select_bank( SpiConf, ENC28J60_CONTROL_REGISTER_BANK0);
    do
    {
        enc28j60_read_spi(SpiConf,  ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE,ENC28J60_ESTAT, &byteData, 1, 0);

        // either the chip doesn't return a valid status bit or the chip is not connected 
        if(50 < nLoopCnt++) 
        {
            debug_printf("Ethernet PHY NOT IN READY STATE\n");
            break; // try any way
        }

        HAL_Time_Sleep_MicroSeconds(100);
    } while((0x1 != (byteData & 0x1)) || (0xFF == byteData));

    /* Making sure to select the right bank */
    enc28j60_select_bank( SpiConf, ENC28J60_CONTROL_REGISTER_BANK3);
    enc28j60_read_spi( SpiConf, ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_EREVID, &byteData, 1, 0);
    
    /* ---------------------------------------------------------------------------------------------------- */                        
    /*                                                  SOFT RESET                                          */
    
    enc28j60_soft_reset(SpiConf);

    /* ---------------------------------------------------------------------------------------------------- */                        
    /*                                              VERIFY THE DEVICE ID                                    */
    
    phyID1 = enc28j60_read_phy_register(SpiConf, ENC28J60_PHID1);
    phyID2 = enc28j60_read_phy_register(SpiConf, ENC28J60_PHID2);

    if ( (phyID1 != ENC28J60_PHYID1) || (phyID2 != ENC28J60_PHYID2))
    {
        rtp_term_puts("enc28j60_open: Wrong Device");
    }
    
#if DEBUG_ENC28J60
    /* ---------------------------------------------------------------------------------------------------- */                        
    /*                                             READ THE REGISTER VALUES                                 */
    
    {
        int             i, j;
        unsigned char   byteData;
                
        for (j = 0; j < 4; j++)
        {
            enc28j60_select_bank(SpiConf, j);
            for (i = 0; i < 0x1F; i++)
            {
                enc28j60_read_spi(SpiConf, ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, i, &byteData, 1, (j>0));
                
            }   
        }
    }
#endif    
    
    /* ---------------------------------------------------------------------------------------------------- */
    /*                                          SETUP RECEIVE  BUFFER                                       */

    enc28j60_setup_recv_buffer(pi, SpiConf);
                        
    /* ---------------------------------------------------------------------------------------------------- */                        
    /*                                          SETUP RECEIVE FILETER                                       */
    
    /* Making sure to select the right bank */
    enc28j60_select_bank(SpiConf, ENC28J60_CONTROL_REGISTER_BANK1);
    /* Disable packet filtering (Promiscuous mode) */
    byteData = 0;
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_ERXFCON, &byteData, 1);
    
    
    
    /* ---------------------------------------------------------------------------------------------------- */                        
    /*                                          MAC INITIALIZATION SETTINGS                                 */
    
    /* Making sure to select the right bank */
    enc28j60_select_bank(SpiConf, ENC28J60_CONTROL_REGISTER_BANK2);
    
    /* Clear the MARST bit in MACON2 */
    //NEW MANUAL DOESN'T DO THIS
    //enc28j60_read_spi(SpiConf, ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_MACON2, &byteData, 1, 1);
    //byteData &= ~(1 << ENC28J60_MACON2_MARST_BIT);
    //enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_MACON2, &byteData, 1);
                        
    /* Making sure to select the right bank */
    enc28j60_select_bank(SpiConf, ENC28J60_CONTROL_REGISTER_BANK2);                        
    
    /* Set the MARXEN bit in MACON1 */
    enc28j60_read_spi(SpiConf, ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_MACON1, &byteData, 1, 1);
    byteData = (1 << ENC28J60_MACON1_MARXEN_BIT);
#if (ENC28J60_FULL_DUPLEX)
    byteData |= (1 << ENC28J60_MACON1_TXPAUS_BIT);
    byteData |= (1 << ENC28J60_MACON1_RXPAUS_BIT);
#endif
    
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_MACON1, &byteData, 1);

    /* Configure the PADCFT, TXCRECEN, AND FULLDPS bits
        of MACON3 */

    byteData = (1 << ENC28J60_MACON3_PADCFG2_BIT) |
                (1 << ENC28J60_MACON3_PADCFG1_BIT) |
                (1 << ENC28J60_MACON3_PADCFG0_BIT);
    //(1 << ENC28J60_MACON3_TXCRCEN_BIT);
#if (ENC28J60_FULL_DUPLEX)
    byteData |= (1 << ENC28J60_MACON3_FULDPX_BIT);
#endif    
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_MACON3, &byteData, 1);
        
    byteData = 0;
    /* Configure the bits in MACON4     */
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_MACON4, &byteData, 1);
    
    /* Configure the MAMXFL registers */
    byteData = ENC28J60_MAXIMUM_FRAME_SIZE & 0xFF;
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_MAMXFLL, &byteData, 1);
    byteData = (ENC28J60_MAXIMUM_FRAME_SIZE >> 8) & 0xFF;
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_MAMXFLH, &byteData, 1);
    
    /* Configure the Back-to-back inter-packet gap */
#if (ENC28J60_FULL_DUPLEX)
    byteData = 0x15;
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_MABBIPG, &byteData, 1);
#else
    byteData = 0x12;
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_MABBIPG, &byteData, 1);
#endif    

    /* Configure the non-back-to-back inter-packet gap */
    byteData = 0x12;
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_MAIPGL, &byteData, 1);

#if (!ENC28J60_FULL_DUPLEX)
    byteData = 0xC;
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_MAIPGH, &byteData, 1);
#endif
        
    /* Making sure to select the right bank */
    enc28j60_select_bank(SpiConf, ENC28J60_CONTROL_REGISTER_BANK3);
    
    /* Setup the MAC address */
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_MAADR1, &pi->addr.my_hw_addr[0], 1);
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_MAADR2, &pi->addr.my_hw_addr[1], 1);
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_MAADR3, &pi->addr.my_hw_addr[2], 1);
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_MAADR4, &pi->addr.my_hw_addr[3], 1);
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_MAADR5, &pi->addr.my_hw_addr[4], 1);
    enc28j60_write_spi(SpiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_MAADR6, &pi->addr.my_hw_addr[5], 1);
    
#if DEBUG_ENC28J60
    {
        unsigned char bytesData;

        enc28j60_read_spi(SpiConf,  ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE,ENC28J60_MAADR1, &bytesData, 1, 1);        
        enc28j60_read_spi(SpiConf,  ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE,ENC28J60_MAADR2, &bytesData, 1, 1);        
        enc28j60_read_spi(SpiConf,  ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE,ENC28J60_MAADR3, &bytesData, 1, 1);        
        enc28j60_read_spi(SpiConf,  ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE,ENC28J60_MAADR4, &bytesData, 1, 1);        
        enc28j60_read_spi(SpiConf,  ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE,ENC28J60_MAADR5, &bytesData, 1, 1);        
        enc28j60_read_spi(SpiConf,  ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE,ENC28J60_MAADR6, &bytesData, 1, 1);        
                
        enc28j60_select_bank(SpiConf, ENC28J60_CONTROL_REGISTER_BANK2);
        enc28j60_read_spi(SpiConf,  ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE,ENC28J60_MACON3, &bytesData, 1, 1);        
    }
#endif
    

    /* ---------------------------------------------------------------------------------------------------- */         
    /*                                  CHECK DUPLEX MODE                                                   */
    duplex = enc28j60_read_phy_register(SpiConf, ENC28J60_PHCON1);
    
    enc28j60_select_bank(SpiConf, ENC28J60_CONTROL_REGISTER_BANK2);
    enc28j60_read_spi(SpiConf, ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_MACON3, &byteData, 1, 1);
    
    if ( ((duplex >> ENC28J60_PHCON1_PDPXMD) & 1) != 
         ((byteData >> ENC28J60_MACON3_FULDPX_BIT) & 1)
       )
    {
        rtp_term_puts("enc28j60_open: check duplex setting");
    }
    
    /* ---------------------------------------------------------------------------------------------------- */         
    /*                                                                                                      */
    
    /* The first time the ethernet is initialized, let us
        point the ETXST pointer to the beginning of the 
        buffer space. Subsequent increments of this register
        will be done in the transmit function */

    sc->xmit_start_buffer = ENC28J60_TRANSMIT_BUFFER_START;
    
    /* enabling reception */
    
    /* Making sure to select the right bank */
    enc28j60_select_bank(SpiConf, ENC28J60_CONTROL_REGISTER_BANK0);   
    
    /* setup the ERDPTL AND ERDPTH to the beginning of the receive buffer */
    byteData = ENC28J60_RECEIVE_BUFFER_START & 0xFF;
    enc28j60_write_spi(SpiConf, ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE, ENC28J60_ERDPTL, &byteData, 1);
    byteData = (ENC28J60_RECEIVE_BUFFER_START >> 8) & 0xFF;
    enc28j60_write_spi(SpiConf, ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE, ENC28J60_ERDPTH, &byteData, 1);

    /* -----------------------------------DISABLE LOOPBACK FOR HALF DUPLEX--------------------------------- */         
    /*                                                                                                      */    
#if (!ENC28J60_FULL_DUPLEX)    
    shortData = enc28j60_read_phy_register(SpiConf, ENC28J60_PHCON2);
    shortData = 1 << ENC28J60_PHCON2_HDLDIS;
    enc28j60_write_phy_register(SpiConf, ENC28J60_PHCON2, shortData);
    shortData = enc28j60_read_phy_register(SpiConf, ENC28J60_PHCON2);
#endif
    /* -----------------------------------DISABLE PHY INTERRUPTS ------------------------------------------ */         
    /*                                                                                                      */    
    shortData = 0x0;
    enc28j60_write_phy_register(SpiConf, ENC28J60_PHIE, shortData);
    
    /* -------------------------------------- START THE DEVICE -------------------------------------------- */         
    /*                                                                                                      */
    /* enable interrupts when a packet is received and when transmit is done */
    byteData = ((1 << ENC28J60_EIE_INTIE_BIT) | (1 << ENC28J60_EIE_PKTIE_BIT) | (1 << ENC28J60_EIE_TXIE_BIT) |(1 << ENC28J60_EIE_TXERIE_BIT));
    enc28j60_write_spi(SpiConf, ENC28J60_SPI_BIT_FIELD_SET_OPCODE, ENC28J60_EIE, &byteData, 1);
    
    /* enable auto- increment */
    byteData = (1 << ENC28J60_ECON2_AUTOINC_BIT);
    enc28j60_write_spi(SpiConf, ENC28J60_SPI_BIT_FIELD_SET_OPCODE, ENC28J60_ECON2, &byteData, 1);
   
    /* enable packet reception */
    byteData = (1 << ENC28J60_ECON1_RXEN_BIT);
    enc28j60_write_spi(SpiConf, ENC28J60_SPI_BIT_FIELD_SET_OPCODE, ENC28J60_ECON1, &byteData, 1);

    return RTP_TRUE;
}

/*
    unsigned short enc28j60_read_phy_register (SPI_CONFIGURATION *spiConf,
                                                unsigned long registerAddress)
    
    This helper function reads from a PHY register given the address of 
    the register to read from.  
    
    RETURNS - The 16bit register value
    
    NOTES - 
        To read from a PHY register:
        1. Write the address of the PHY register to read from into 
            the MIREGADR register.
        2. Set the MICMD.MIIRD bit. The read operation begins and 
            the MISTAT.BUSY bit is set.
        3. Wait 10.24 ìs. Poll the MISTAT.BUSY bit to be certain 
            that the operation is complete. While busy, the host 
            controller should not start any MIISCAN operations or 
            write to the MIWRH register. When the MAC has obtained 
            the register contents, the BUSY bit will clear itself.
        4. Clear the MICMD.MIIRD bit.
        5. Read the desired data from the MIRDL and MIRDH registers. 
            The order that these bytes are accessed is unimportant
*/
unsigned short enc28j60_read_phy_register(SPI_CONFIGURATION *spiConf, RTP_UINT8 registerAddress)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    unsigned char   byteData;
    unsigned short  returnValue;

    /* Select Bank 2 */
    enc28j60_select_bank(spiConf, ENC28J60_CONTROL_REGISTER_BANK2);

    /* 1. */
    enc28j60_write_spi(spiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE, ENC28J60_MIREGADR, &registerAddress, 1);
    
    /* 2. */
    byteData = (0x1 << ENC28J60_MICMD_MIIRD_BIT);
    enc28j60_write_spi(spiConf,  ENC28J60_SPI_BIT_FIELD_SET_OPCODE, ENC28J60_MICMD, &byteData, 1);
    
    /* 3. */
    /* Select Bank 3 */
    enc28j60_select_bank(spiConf, ENC28J60_CONTROL_REGISTER_BANK3);
    
    do
    {
        enc28j60_read_spi(spiConf, ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_MISTAT, &byteData, 1, 1);
        byteData &= (0x1 << ENC28J60_MISTAT_BUSY_BIT);
    } while (byteData);
    
    /* Select Bank 2 */                                               
    enc28j60_select_bank(spiConf, ENC28J60_CONTROL_REGISTER_BANK2);   

    /* 4. */
    enc28j60_read_spi(spiConf, ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_MICMD, &byteData, 1, 1);
    byteData = 0;//&= ~(0x1 << ENC28J60_MICMD_MIIRD_BIT);
    enc28j60_write_spi(spiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_MICMD,  &byteData, 1);

     /* Select Bank 2 */                                               
    enc28j60_select_bank(spiConf, ENC28J60_CONTROL_REGISTER_BANK2);   
    
    /* 5. */
    enc28j60_read_spi(spiConf, ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_MIRDH, &byteData, 1, 1);
    returnValue = byteData << 8;
    
    enc28j60_read_spi(spiConf, ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_MIRDL, &byteData, 1, 1);
    returnValue |= byteData;
    
    return (returnValue);
}

/*  void enc28j60_write_phy_register(unsigned long registerAddress,
                                     unsigned short data)
                                     
    This function write the parameter data to the register given
    by the registerAddress parameter.
    
    RETURNS - nothing
    
    NOTES -                                      
    1. Write the address of the PHY register to write to
        into the MIREGADR register.
    2. Write the lower 8 bits of data to write into the
        MIWRL register.
    3. Write the upper 8 bits of data to write into the
        MIWRH register. Writing to this register automatically
        begins the MII transaction, so it must
        be written to after MIWRL. The MISTAT.BUSY
        bit becomes set.
*/
void enc28j60_write_phy_register(SPI_CONFIGURATION *spiConf, RTP_UINT8 registerAddress, unsigned short data)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    unsigned char byteData;
    
    /* All the registers for this purpose are on bank 2 */
    enc28j60_select_bank(spiConf, ENC28J60_CONTROL_REGISTER_BANK2);
    
    /* 1. */
    enc28j60_write_spi(spiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE, ENC28J60_MIREGADR, &registerAddress, 1);
    byteData = data & 0xFF;
    
    /* 2. */
    enc28j60_write_spi(spiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_MIWRL, &byteData, 1);
    byteData = (data >> 8) & 0xFF;
    
    /* 3. */
    enc28j60_write_spi(spiConf,  ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE,ENC28J60_MIWRH, &byteData, 1);
}

/*
    void enc28j60_write_spi(PENC28J60_SOFTC sc, 
                            RTP_UINT8 opcode,
                            RTP_UINT8 address, 
                            RTP_PUINT8 &byteData,
                            RTP_UINT32 numBytes);
    
    This function writes the SPI interface using the configuration parameter
    within the sc structure.  The opcode parameter is used to specify which
    control register to access.  This controller has the following seven control
    register commands
        1. read control register
        2. read buffer memory
        3. write control register
        4. write buffer memory
        5. bit field set
        6. bit field clear
        7. system command (soft reset)
    The opcode field selects one of the above commands.  Within each control register
    command, there are a number of registers to be accessed.  The address of a register
    is specified using the address parameter.  The data pointed by byteData field 
    is written to the register
 */    
                              
void enc28j60_write_spi(SPI_CONFIGURATION *spiConf, 
                    RTP_UINT8 opcode,
                    RTP_UINT8 address, 
                    RTP_PFUINT8 byteData,
                    RTP_UINT32 numBytes)
{
    RTP_UINT8 opcodeArg;
    RTP_UINT8 commandWithData[2];
    
    opcodeArg = ENC28J60_SPI_OPCODE_ARGUMENT(opcode, address);
    
    if (numBytes == 1)
    {
        /* Combine the command and the data */
        commandWithData[0] = opcodeArg;
        commandWithData[1] = *byteData;
        
        CPU_SPI_nWrite8_nRead8 (*spiConf, commandWithData, numBytes+1, 0, 0, 0);
    }
    else
    {
        byteData[0] = opcodeArg;
        CPU_SPI_nWrite8_nRead8 (*spiConf, byteData, numBytes+1, 0, 0, 0);
    }
}

/* void enc28j60_soft_reset(SPI_CONFIGURATION *spiConf)
    
    A function to perform a soft reset on the ethernet board 
 */
 
void enc28j60_soft_reset(SPI_CONFIGURATION* spiConf)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    UINT8 byteData;
    UINT16 shortData;

    /* First turn off RX/TX */
    enc28j60_select_bank( spiConf, ENC28J60_CONTROL_REGISTER_BANK0);

    byteData = (1 << ENC28J60_ECON1_RXEN_BIT);
    enc28j60_write_spi(spiConf, ENC28J60_SPI_BIT_FIELD_CLEAR_OPCODE, ENC28J60_ECON1, &byteData, 1);

    /* Wait for RX to flush */
    do
    {
        HAL_Time_Sleep_MicroSeconds(100);
        
        enc28j60_read_spi(spiConf, ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_ESTAT, &byteData, 1, 0);
    }
    while((byteData & (1 << ENC28J60_ESTAT_RXBUSY)) != 0);

    /* Wait for TX to flush */
    do 
    {
        HAL_Time_Sleep_MicroSeconds(100);

        enc28j60_read_spi(spiConf, ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_ECON1, &byteData, 1, 0);
    }
    while((byteData & (1 << ENC28J60_ECON1_TXRTS_BIT)) != 0);

    /* make sure voltage regulator is normal */
    byteData = 1 << ENC28J60_ECON2_VRPS_BIT;
    enc28j60_write_spi(spiConf, ENC28J60_SPI_BIT_FIELD_CLEAR_OPCODE, ENC28J60_ECON2, &byteData, 1);
    
    /* Reset PHY */
    enc28j60_write_phy_register(spiConf, ENC28J60_PHCON1, (UINT16)(1ul << ENC28J60_PHCON1_PRST));

    do
    {
        HAL_Time_Sleep_MicroSeconds(100);

        shortData = enc28j60_read_phy_register(spiConf, ENC28J60_PHCON1);
    } while((shortData & (1 << ENC28J60_PHCON1_PRST)) != 0);

    byteData = ((1 << ENC28J60_EIE_INTIE_BIT) | (1 << ENC28J60_EIE_PKTIE_BIT) | (1 << ENC28J60_EIE_TXIE_BIT) |(1 << ENC28J60_EIE_TXERIE_BIT));
    enc28j60_write_spi(spiConf, ENC28J60_SPI_BIT_FIELD_CLEAR_OPCODE, ENC28J60_EIE, &byteData, 1);    
    
    /* Combine the command and the data */
    byteData = (ENC28J60_SPI_SYSTEM_COMMAND_SOFT_RESET_OPCODE << 5) | 
                ENC28J60_SPI_SYSTEM_COMMAND_SOFT_RESET_ARGUMENT;
    CPU_SPI_nWrite8_nRead8 (*spiConf, (UINT8 *)&byteData, 1, 0, 0, 0);

    /* Errata : After reset wait for 100 ms */
    for(byteData=0; byteData<100; byteData++)
    {
        HAL_Time_Sleep_MicroSeconds(1000);
    }
}
 
/*
    void enc28j60_read_spi(PENC28J60_SOFTC sc, 
                            RTP_UINT8 opcode,
                            RTP_UINT8 address, 
                            RTP_PUINT8 &byteData,
                            RTP_UINT32 numBytes)
    
    This function reads from the SPI interface using the configuration parameter
    within the sc structure.  The opcode parameter is used to specify which
    control register to access.  This controller has the following seven control
    register commands
        1. read control register
        2. read buffer memory
        3. write control register
        4. write buffer memory
        5. bit field set
        6. bit field clear
        7. system command (soft reset)
    The opcode field selects one of the above commands.  Within each control register
    command, there are a number of registers to be accessed.  The address of a register
    is specified using the address parameter.  Contents of the register are written into
    the byteData
 */    
                              
void enc28j60_read_spi(SPI_CONFIGURATION *spiConf, 
                    RTP_UINT8 opcode,
                    RTP_UINT8 address, 
                    RTP_PFUINT8 byteData,
                    RTP_UINT32 numBytes, 
                    RTP_UINT8  offset)
{
    RTP_UINT8 opcodeArg;
    
    opcodeArg = ENC28J60_SPI_OPCODE_ARGUMENT(opcode, address);
    
    /* Write the command and read*/
    CPU_SPI_nWrite8_nRead8 (*spiConf, &opcodeArg, 1, byteData, numBytes+offset, offset+1);
    
}



/*
    void enc28j60_select_bank(sc, unsigned short bankNumber)
    
    Given the bank number this function selects one of the 
    four banks.  
*/

void enc28j60_select_bank(SPI_CONFIGURATION *spiConf, RTP_UINT8 bankNumber)
{
    NATIVE_PROFILE_HAL_DRIVERS_ETHERNET();
    RTP_UINT8   byteData = 0;
    
    bankNumber &= 0x3;
        
    enc28j60_read_spi(spiConf, ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_ECON1, &byteData, 1, 0);
    byteData &= 0xFC;
    byteData |= bankNumber;
    enc28j60_write_spi( spiConf, ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE, ENC28J60_ECON1, &byteData, 1);
    
#if DEBUG_ENC28J60
    /* JUST CHECKING WHAT WE WROTE */
    enc28j60_read_spi( spiConf, ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE, ENC28J60_ECON1, &byteData, 1, 0);
#endif
}

