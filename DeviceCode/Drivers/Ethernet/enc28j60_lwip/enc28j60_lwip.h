//
// enc28j60.h - device driver interface
//
// EBSNet - RTIP
//
// Copyright EBSNet , 2007
// All rights reserved.
// This code may not be redistributed in source or linkable object form
// without the consent of its author.
//

#ifndef __ENC28J60_H__
#define __ENC28J60_H__

#include <tinyhal.h>
#include <enc28j60_lwip_driver.h>

bool        enc28j60_lwip_open( struct netif *pNetIF );
void        enc28j60_lwip_close ( struct netif *pNetIF  );
err_t       enc28j60_lwip_xmit ( struct netif *pNetIF , struct pbuf *pPBuf );
int         enc28j60_lwip_recv ( struct netif *pNetIF );
void        enc28j60_lwip_interrupt ( struct netif *pNetIF );
void        enc28j60_lwip_pre_interrupt (GPIO_PIN Pin, BOOL PinState, void *pArg );
void        enc28j60_lwip_setup_recv_buffer( struct netif *pNetIF, SPI_CONFIGURATION  *SpiConf);


/*          FUNCTION PROTOTYPE                                          */


/*          STRUCTURES                                                  */

#define ETHERSIZE        ((ENC28J60_TRANSMIT_BUFFER_END - ENC28J60_TRANSMIT_BUFFER_START)>>1)   /* maximum number of bytes in ETHERNET packet */
                                /* (used by ethernet drivers)   */

#define ETHER_MIN_LEN    64  /* minimum number of bytes in an ETHERNET */
                             /* packet   */



/*              ENC28J60 CONFIGURATION                                  */
#define     CFG_NUM_ENC28J60            NETWORK_INTERFACE_COUNT                                    
#define     CFG_MAX_PACKETS_PROCESSED   10          /* The maximum number of packets to process in one
                                                        shot */

#define     ENC28J60_FULL_DUPLEX        0           /* DON'T CHANGE adds support for full duplex */
#define     ENC28J60_MAXIMUM_FRAME_SIZE 1530        /* maximum frame sizes to be transmitted */
                                    
#define     ENC28J60_MAC_ADDRESS0       0xBA        /* Configures the MAC address to */
#define     ENC28J60_MAC_ADDRESS1       0xAD        /* these values */
#define     ENC28J60_MAC_ADDRESS2       0xCA
#define     ENC28J60_MAC_ADDRESS3       0xAB
#define     ENC28J60_MAC_ADDRESS4       0x04
#define     ENC28J60_MAC_ADDRESS5       0x05 

#define     ENC28J60_PHYID1              0x0083      /* Unique device id */
#define     ENC28J60_PHYID2              0x1400      /* Unique device id */

        
/*              BUFFER ALLOCATION SPACES                                */

#define ENC28J60_RECEIVE_BUFFER_START       0x0000
#define ENC28J60_RECEIVE_BUFFER_END         0x0FFF
#define ENC28J60_TRANSMIT_BUFFER_START      0x1000
#define ENC28J60_TRANSMIT_BUFFER_END        0x1FFF

/*              ENC28J60 control register banks                         */
#define ENC28J60_CONTROL_REGISTER_BANK0     0
#define ENC28J60_CONTROL_REGISTER_BANK1     1
#define ENC28J60_CONTROL_REGISTER_BANK2     2
#define ENC28J60_CONTROL_REGISTER_BANK3     3

/*              ENC28J60 control register map                           */

#define ENC28J60_ERDPTL             0x00
                                    
#define ENC28J60_EHT0               0x00
                                    
#define ENC28J60_MACON1             0x00
#define ENC28J60_MACON1_LOOPBK_BIT  4
#define ENC28J60_MACON1_TXPAUS_BIT  3
#define ENC28J60_MACON1_RXPAUS_BIT  2
#define ENC28J60_MACON1_PASSALL_BIT 1
#define ENC28J60_MACON1_MARXEN_BIT  0                              
                                    
#define ENC28J60_MAADR2             0x05
                                    
#define ENC28J60_ERDPTH             0x01
                                    
#define ENC28J60_EHT1               0x01
                                    
#define ENC28J60_MACON2             0x01
#define ENC28J60_MACON2_MARST_BIT   7
#define ENC28J60_MACON2_RNDRST_BIT  6
#define ENC28J60_MACON2_MARXRST_BIT 3
#define ENC28J60_MACON2_RFUNRST_BIT 2
#define ENC28J60_MACON2_MATXRST_BIT 1
#define ENC28J60_MACON2_TFUNRST_BIT 0

#define ENC28J60_MAADR1             0x04
                                    
#define ENC28J60_EWRPTL             0x02
                                    
#define ENC28J60_EHT2               0x02
                                    
#define ENC28J60_MACON3             0x02
#define ENC28J60_MACON3_PADCFG2_BIT 7
#define ENC28J60_MACON3_PADCFG1_BIT 6
#define ENC28J60_MACON3_PADCFG0_BIT 5
#define ENC28J60_MACON3_TXCRCEN_BIT 4
#define ENC28J60_MACON3_PHDRLEN_BIT 3
#define ENC28J60_MACON3_HFRMEN_BIT  2
#define ENC28J60_MACON3_FRMLNEN_BIT 1
#define ENC28J60_MACON3_FULDPX_BIT  0
                                  
#define ENC28J60_MAADR4             0x03
                                    
#define ENC28J60_EWRPTH             0x03
                                    
#define ENC28J60_EHT3               0x03
                                    
#define ENC28J60_MACON4             0x03
#define ENC28J60_MACON4_DEFER_BIT   6
#define ENC28J60_MACON4_BPEN_BIT    5
#define ENC28J60_MACON4_NOBKOFF_BIT 4
#define ENC28J60_MACON4_LONGPRE_BIT 1
#define ENC28J60_MACON4_PUREPRE_BIT 0
                                    
#define ENC28J60_MAADR3             0x02
                                    
#define ENC28J60_ETXSTL             0x04
                                    
#define ENC28J60_EHT4               0x04
                                    
#define ENC28J60_MABBIPG            0x04
                                    
#define ENC28J60_MAADR6             0x01
                                    
#define ENC28J60_ETXSTH             0x05
                                    
#define ENC28J60_EHT5               0x05
                                    
#define ENC28J60_MAADR5             0x00
                                    
#define ENC28J60_ETXNDL             0x06
                                    
#define ENC28J60_EHT6               0x06
                                    
#define ENC28J60_MAIPGL             0x06
                                    
#define ENC28J60_EBSTSD             0x06
                                    
#define ENC28J60_ETXNDH             0x07
                                    
#define ENC28J60_EHT7               0x07
                                    
#define ENC28J60_MAIPGH             0x07
                                    
#define ENC28J60_EBSTCON            0x07
                                    
#define ENC28J60_ERXSTL             0x08

#define ENC28J60_EPMM0              0x08
                                    
#define ENC28J60_MACLCON1           0x08
                                    
#define ENC28J60_EBSTCSL            0x08
                                    
#define ENC28J60_ERXSTH             0x09
                                    
#define ENC28J60_EPMM1              0x09
                                    
#define ENC28J60_MACLCON2           0x09
                                    
#define ENC28J60_EBSTCSH            0x09
                                    
#define ENC28J60_ERXNDL             0x0A
                                    
#define ENC28J60_EPMM2              0x0A
                                    
#define ENC28J60_MAMXFLL            0x0A
                                    
#define ENC28J60_MISTAT             0x0A
#define ENC28J60_MISTAT_NVALID_BIT  2
#define ENC28J60_MISTAT_SCAN_BIT    1
#define ENC28J60_MISTAT_BUSY_BIT    0
                                     
#define ENC28J60_ERXNDH             0x0B
                                    
#define ENC28J60_EPMM3              0x0B
                                    
#define ENC28J60_MAMXFLH            0x0B
                                    
#define ENC28J60_ERXRDPTL           0x0C
                                    
#define ENC28J60_EPMM4              0x0C
                                    
#define ENC28J60_ERXRDPTH           0x0D
                                    
#define ENC28J60_EPMM5              0x0D
                                    
#define ENC28J60_MAPHSUP            0x0D
                                    
#define ENC28J60_ERXWRPTL           0x0E
                                    
#define ENC28J60_EPMM6              0x0E
                                    
#define ENC28J60_ERXWRPTH           0x0F
                                    
#define ENC28J60_EPMM7              0x0F
                                    
#define ENC28J60_EDMASTL            0x10
                                    
#define ENC28J60_EPMCSL             0x10
                                    
#define ENC28J60_EDMASTH            0x11
                                    
#define ENC28J60_EPMCSH             0x11
                                    
#define ENC28J60_MICON              0x11
                                    
#define ENC28J60_EDMANDL            0x12
                                    
#define ENC28J60_MICMD              0x12
#define ENC28J60_MICMD_MISCAN_BIT   1
#define ENC28J60_MICMD_MIIRD_BIT    0
                                   
#define ENC28J60_EREVID             0x12
                                    
#define ENC28J60_EDMANDH            0x13
                                    
#define ENC28J60_EDMADSTL           0x14
                                    
#define ENC28J60_EPMOL              0x14
                                    
#define ENC28J60_MIREGADR           0x14
                                    
#define ENC28J60_EDMADSTH           0x15
                                    
#define ENC28J60_EPMOH              0x15
                                    
#define ENC28J60_ECOCON             0x15
                                    
#define ENC28J60_EDMACSL            0x16
                                    
#define ENC28J60_EWOLIE             0x16
                                    
#define ENC28J60_MIWRL              0x16
                                    
#define ENC28J60_EDMACSH            0x17
                                    
#define ENC28J60_EWOLIR             0x17
                                    
#define ENC28J60_MIWRH              0x17
                                    
#define ENC28J60_EFLOCON            0x17
                                    
#define ENC28J60_ERXFCON            0x18
#define ENC28J60_ERXFCON_UCEN_BIT   7
#define ENC28J60_ERXFCON_ANDOR_BIT  6
#define ENC28J60_ERXFCON_CRCEN_BIT  5
#define ENC28J60_ERXFCON_PMEN_BIT   4
#define ENC28J60_ERXFCON_MPEN_BIT   3
#define ENC28J60_ERXFCON_HTEN_BIT   2   
#define ENC28J60_ERXFCON_MCEN_BIT   1
#define ENC28J60_ERXFCON_BCEN_BIT   0
                                    
#define ENC28J60_MIRDL              0x18
                                    
#define ENC28J60_EPAUSL             0x18
                                    
#define ENC28J60_EPKTCNT            0x19
                                    
#define ENC28J60_MIRDH              0x19
                                    
#define ENC28J60_EPAUSH             0x19
                                    
#define ENC28J60_EIE                0x1B
#define ENC28J60_EIE_INTIE_BIT      7
#define ENC28J60_EIE_PKTIE_BIT      6
#define ENC28J60_EIE_DMAIE_BIT      5
#define ENC28J60_EIE_LINKIE_BIT     4
#define ENC28J60_EIE_TXIE_BIT       3
#define ENC28J60_EIE_WOLIE_BIT      2
#define ENC28J60_EIE_TXERIE_BIT     1
#define ENC28J60_EIE_RXERIE_BIT     0
                                    
#define ENC28J60_EIR                0x1C
#define ENC28J60_EIR_PKTIF_BIT      6
#define ENC28J60_EIR_DMAIF_BIT      5
#define ENC28J60_EIR_LINKIF_BIT     4
#define ENC28J60_EIR_TXIF_BIT       3
#define ENC28J60_EIR_WOLIF_BIT      2
#define ENC28J60_EIR_TXERIF_BIT     1
#define ENC28J60_EIR_RXERIF_BIT     0                          

#define ENC28J60_ESTAT              0x1D
#define ENC28J60_ESTAT_INT          7
#define ENC28J60_ESTAT_BUFER        6
#define ENC28J60_ESTAT_LATECOL      4
#define ENC28J60_ESTAT_RXBUSY       2
#define ENC28J60_ESTAT_TXABRT       1
#define ENC28J60_ESTAT_CLKRDY       0
                                    
#define ENC28J60_ECON2              0x1E
#define ENC28J60_ECON2_AUTOINC_BIT  7
#define ENC28J60_ECON2_PKTDEC_BIT   6
#define ENC28J60_ECON2_PWRSV_BIT    5
#define ENC28J60_ECON2_VRPS_BIT     4
    
#define ENC28J60_ECON1              0x1F
#define ENC28J60_ECON1_TXRST_BIT    7
#define ENC28J60_ECON1_RXRST_BIT    6
#define ENC28J60_ECON1_DMAST_BIT    5
#define ENC28J60_ECON1_CSUMEN_BIT   4
#define ENC28J60_ECON1_TXRTS_BIT    3
#define ENC28J60_ECON1_RXEN_BIT     2
#define ENC28J60_ECON1_BSEL1_BIT    1
#define ENC28J60_ECON1_BSEL0_BIT    0

#define ENC28J60_PHCON1             0x0           
#define ENC28J60_PHCON1_PRST        15
#define ENC28J60_PHCON1_PLOOPBK     14
#define ENC28J60_PHCON1_PPWRSV      11
#define ENC28J60_PHCON1_PDPXMD      8

#define ENC28J60_PHSTAT1            0x1

#define ENC28J60_PHID1              0x2

#define ENC28J60_PHID2              0x3

#define ENC28J60_PHCON2             0x10
#define ENC28J60_PHCON2_FRCLNK      14
#define ENC28J60_PHCON2_TXDIS       13
#define ENC28J60_PHCON2_JABBER      10
#define ENC28J60_PHCON2_HDLDIS      8

#define ENC28J60_PHSTAT2            0x11
#define ENC28J60_PHSTAT2_LSTAT_BIT    10

#define ENC28J60_PHIE               0x12
#define ENC28J60_PHIR               0x13
#define ENC28J60_PHLCON             0x14
/*                  SPI Instruction set                                 */
#define ENC28J60_SPI_READ_CONTROL_REGISTER_OPCODE       0x0
            
#define ENC28J60_SPI_READ_BUFFER_MEMORY_OPCODE          0x1
#define ENC28J60_SPI_READ_BUFFER_MEMORY_ARGUMENT        0x1A
            
#define ENC28J60_SPI_WRITE_CONTROL_REGISTER_OPCODE      0x2
            
#define ENC28J60_SPI_WRITE_BUFFER_MEMORY_OPCODE         0x3
#define ENC28J60_SPI_WRITE_BUFFER_MEMORY_ARGUMENT       0x1A
            
#define ENC28J60_SPI_BIT_FIELD_SET_OPCODE               0x4
            
#define ENC28J60_SPI_BIT_FIELD_CLEAR_OPCODE             0x5
            
#define ENC28J60_SPI_SYSTEM_COMMAND_SOFT_RESET_OPCODE   0x7
#define ENC28J60_SPI_SYSTEM_COMMAND_SOFT_RESET_ARGUMENT 0x1F

#define ENC28J60_SPI_OPCODE_ARGUMENT(OP, ARG)           (OP << 5) | (ARG &0x1F)

/*                  XMIT format for per packet control                  */

#define ENC28J60_XMIT_CONTROL_PHUGEEN_BIT           3
#define ENC28J60_XMIT_CONTROL_PPADEN_BIT            2
#define ENC28J60_XMIT_CONTROL_PCRCEN_BIT            1
#define ENC28J60_XMIT_CONTROL_POVERRIDE_BIT         0

#endif  /* #ifndef __ENC28J60_H__   */
