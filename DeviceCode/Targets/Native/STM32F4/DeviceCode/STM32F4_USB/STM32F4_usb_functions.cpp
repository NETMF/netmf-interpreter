////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32F4: Copyright (c) Oberon microsystems, Inc.
//
//  *** USB OTG Full Speed Device Mode Driver ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include <tinyhal.h>
#include <pal\com\usb\USB.h>

#ifdef STM32F4XX
#include "..\stm32f4xx.h"
#else
#include "..\stm32f2xx.h"
#endif

#include "usb_def.h"

int USB_TRACE_STATE = 0;
#define USB_Debug(s) USB_TRACE_STATE = s//ITM_SendChar( s )

#ifdef DEBUG
#define _ASSERT(x) ASSERT(x)
#else
#define _ASSERT(x)
#endif


#if !defined(STM32F4_USB_FS_USE_ID_PIN)
#define STM32F4_USB_FS_USE_ID_PIN 0
#endif
#if !defined(STM32F4_USB_FS_USE_VB_PIN)
#define STM32F4_USB_FS_USE_VB_PIN 0
#endif
#if !defined(STM32F4_USB_HS_USE_ID_PIN)
#define STM32F4_USB_HS_USE_ID_PIN 0
#endif
#if !defined(STM32F4_USB_HS_USE_VB_PIN)
#define STM32F4_USB_HS_USE_VB_PIN 0
#endif


#define MAX_EP0_SIZE 64     // maximum control channel packet size
#define DEF_EP0_SIZE 8      // default control channel packet size
#define STM32F4_Num_EP_FS 4 // OTG FS supports 4 endpoints
#define STM32F4_Num_EP_HS 6 // OTG FS supports 6 endpoints


#if TOTAL_USB_CONTROLLER == 2
// USB OTG Full Speed is controller 0
// USB OTG High Speed is controller 1

#define STM32F4_USB_REGS(c) (c ? OTG_HS : OTG_FS)

#define STM32F4_USB_IS_HS(c) c
#define STM32F4_USB_IDX(c) c
#define STM32F4_USB_FS_IDX 0
#define STM32F4_USB_HS_IDX 1

#define STM32F4_USB_DM_PIN(c) (c ? 30 : 11) // B14,A11
#define STM32F4_USB_DP_PIN(c) (c ? 31 : 12) // B15,A12
#define STM32F4_USB_VB_PIN(c) (c ? 29 :  9) // B13,A9
#define STM32F4_USB_ID_PIN(c) (c ? 28 : 10) // B12,A10

#define STM32F4_USB_USE_ID_PIN(c) (c ? STM32F4_USB_HS_USE_ID_PIN : STM32F4_USB_FS_USE_ID_PIN)
#define STM32F4_USB_USE_VB_PIN(c) (c ? STM32F4_USB_HS_USE_VB_PIN : STM32F4_USB_FS_USE_VB_PIN)
#define STM32F4_USB_ALT_MODE(c) (GPIO_ALT_MODE)(c ? 0x2C2 : 0x2A2) // AF12,AF10, 50MHz

#if USB_MAX_QUEUES <= STM32F4_Num_EP_FS
#define USB_MAX_EP(c) USB_MAX_QUEUES
#define USB_MAX_BUFFERS (2 * USB_MAX_QUEUES - 2)
#elif USB_MAX_QUEUES <= STM32F4_Num_EP_HS
#define USB_MAX_EP(c) (c ? USB_MAX_QUEUES : STM32F4_Num_EP_FS)
#define USB_MAX_BUFFERS (USB_MAX_QUEUES + STM32F4_Num_EP_FS - 2)
#else
#define USB_MAX_EP(c) (c ? STM32F4_Num_EP_HS : STM32F4_Num_EP_FS)
#define USB_MAX_BUFFERS (STM32F4_Num_EP_HS + STM32F4_Num_EP_FS - 2)
#endif


#elif defined(STM32F4_USB_HS)
// use USB OTG High Speed

#define STM32F4_USB_REGS(c) OTG_HS

#define STM32F4_USB_IS_HS(c) TRUE
#define STM32F4_USB_IDX(c) 0
#define STM32F4_USB_FS_IDX 0
#define STM32F4_USB_HS_IDX 0

#define STM32F4_USB_DM_PIN(c) 30 // B14
#define STM32F4_USB_DP_PIN(c) 31 // B15
#define STM32F4_USB_VB_PIN(c) 29 // B13
#define STM32F4_USB_ID_PIN(c) 28 // B12

#define STM32F4_USB_USE_ID_PIN(c) STM32F4_USB_HS_USE_ID_PIN
#define STM32F4_USB_USE_VB_PIN(c) STM32F4_USB_HS_USE_VB_PIN
#define STM32F4_USB_ALT_MODE(c) (GPIO_ALT_MODE)0x2C2; // AF12, 50MHz

#if USB_MAX_QUEUES <= STM32F4_Num_EP_HS
#define USB_MAX_EP(c) USB_MAX_QUEUES
#define USB_MAX_BUFFERS (USB_MAX_QUEUES - 1)
#else
#define USB_MAX_EP(c) STM32F4_Num_EP_HS
#define USB_MAX_BUFFERS (STM32F4_Num_EP_HS - 1)
#endif


#else
// use OTG Full Speed

#define STM32F4_USB_REGS(c) OTG_FS

#define STM32F4_USB_IS_HS(c) FALSE
#define STM32F4_USB_IDX(c) 0
#define STM32F4_USB_FS_IDX 0
#define STM32F4_USB_HS_IDX 0

#define STM32F4_USB_DM_PIN(c) 11 // A11
#define STM32F4_USB_DP_PIN(c) 12 // A12
#define STM32F4_USB_VB_PIN(c)  9 // A9
#define STM32F4_USB_ID_PIN(c) 10 // A10

#define STM32F4_USB_USE_ID_PIN(c) STM32F4_USB_FS_USE_ID_PIN
#define STM32F4_USB_USE_VB_PIN(c) STM32F4_USB_FS_USE_VB_PIN
#define STM32F4_USB_ALT_MODE(c) (GPIO_ALT_MODE)0x2A2; // AF10, 50MHz

#if USB_MAX_QUEUES <= STM32F4_Num_EP_FS
#define USB_MAX_EP(c) USB_MAX_QUEUES
#define USB_MAX_BUFFERS (USB_MAX_QUEUES - 1)
#else
#define USB_MAX_EP(c) STM32F4_Num_EP_FS
#define USB_MAX_BUFFERS (STM32F4_Num_EP_FS - 1)
#endif


#endif


// FIFO sizes (in 32 bit words)
#define USB_RXFIFO_SIZE  64 // 256 bytes
#define USB_TX0FIFO_SIZE 64 // 256 bytes
#define USB_TXnFIFO_SIZE 64 // 256 bytes

// PHY turnaround time
// (4 AHB clocks + 1 Phy clock in Phy clocks)
#define STM32F4_USB_TRDT ((4 * 48000000 - 1) / SYSTEM_CYCLE_CLOCK_HZ + 2)


// State variables for one controller
typedef struct
{
    USB_CONTROLLER_STATE state;
    UINT8 ep0Buffer[ MAX_EP0_SIZE ];
    UINT16 endpointStatus[ USB_MAX_QUEUES ];
    UINT16 EP_Type;
    UINT8 previousDeviceState;
    BOOL pinsProtected;
} STM32F4_USB_STATE;

/* State variables for the controllers */
static STM32F4_USB_STATE STM32F4_USB_ControllerState[ TOTAL_USB_CONTROLLER ];

/* Queues for all data endpoints */
Hal_Queue_KnownSize<USB_PACKET64, USB_QUEUE_PACKET_COUNT> QueueBuffers[ USB_MAX_BUFFERS ];


/*
 * Suspend Event Interrupt Handler
 */
void STM32F4_USB_Driver_SuspendEvent( OTG_TypeDef* OTG, USB_CONTROLLER_STATE* State )
{
    // SUSPEND event only happened when Host(PC) set the device to SUSPEND
    // as there is always SOF every 1ms on the BUS to keep the device from
    // suspending. Therefore, the REMOTE wake up is not necessary at the device side

    USB_Debug( 'S' );

    ( ( STM32F4_USB_STATE* )State )->previousDeviceState = State->DeviceState;
    State->DeviceState = USB_DEVICE_STATE_SUSPENDED;
    USB_StateCallback( State );
}

/*
 * Resume Event Interrupt Handler
 */
void STM32F4_USB_Driver_ResumeEvent( OTG_TypeDef* OTG, USB_CONTROLLER_STATE* State )
{
    USB_Debug( 'R' );

    OTG->DCTL &= ~OTG_DCTL_RWUSIG; // remove remote wakeup signaling

    State->DeviceState = ( ( STM32F4_USB_STATE* )State )->previousDeviceState;
    USB_StateCallback( State );
}

/*
 * Reset Event Interrupt Handler
 */
void STM32F4_USB_Driver_ResetEvent( OTG_TypeDef* OTG, USB_CONTROLLER_STATE* State )
{
    USB_Debug( '!' );

    // reset interrupts and FIFOs
    OTG->GINTSTS = 0xFFFFFFFF; // clear global interrupts
    OTG->GRXFSIZ = USB_RXFIFO_SIZE; // Rx Fifo
    OTG->DIEPTXF0 = ( USB_TX0FIFO_SIZE << 16 ) | USB_RXFIFO_SIZE; // Tx Fifo 0
    UINT32 addr = USB_RXFIFO_SIZE + USB_TX0FIFO_SIZE;
    for( int i = 0; i < State->EndpointCount; i++ )
    {
        OTG->DIEPTXF[ i ] = ( USB_TXnFIFO_SIZE << 16 ) | addr; // Tx Fifo i
        addr += USB_TXnFIFO_SIZE;
        OTG->DIEP[ i ].INT = 0xFF; // clear endpoint interrupts
        OTG->DOEP[ i ].INT = 0xFF;
        OTG->DIEP[ i ].CTL = OTG_DIEPCTL_EPDIS; // deactivate endpoint
        OTG->DOEP[ i ].CTL = OTG_DOEPCTL_EPDIS;
    }

    // flush FIFOs
    OTG->GRSTCTL = OTG_GRSTCTL_RXFFLSH | OTG_GRSTCTL_TXFFLSH | OTG_GRSTCTL_TXF_ALL;

    // configure control endpoint
    OTG->DIEP[ 0 ].CTL = OTG_DIEPCTL_USBAEP; // Tx FIFO num = 0, max packet size = 64
    OTG->DOEP[ 0 ].CTL = OTG_DOEPCTL_USBAEP;
    OTG->DIEP[ 0 ].TSIZ = 0;
    OTG->DOEP[ 0 ].TSIZ = OTG_DOEPTSIZ_STUPCNT; // up to 3 setup packets

    // configure data endpoints
    UINT32 intMask = 0x00010001; // ep0 interrupts;
    UINT32 eptype = ( ( STM32F4_USB_STATE* )State )->EP_Type >> 2; // endpoint types (2 bits / endpoint)
    UINT32 i = 1, bit = 2;
    while( eptype )
    {
        UINT32 type = eptype & 3;
        if( type != 0 )
        { // data endpoint
            UINT32 ctrl = OTG_DIEPCTL_SD0PID | OTG_DIEPCTL_USBAEP;
            ctrl |= type << 18; // endpoint type
            ctrl |= State->MaxPacketSize[ i ]; // packet size
            if( State->IsTxQueue[ i ] )
            { // Tx (in) endpoint
                ctrl |= OTG_DIEPCTL_SNAK; // disable tx endpoint
                ctrl |= i << 22; // Tx FIFO number
                OTG->DIEP[ i ].CTL = ctrl; // configure in endpoint
                intMask |= bit; // enable in interrupt
            }
            else
            { // Rx (out) endpoint
                // Rx endpoints must be enabled here
                // Enabling after Set_Configuration does not work correctly
                OTG->DOEP[ i ].TSIZ = OTG_DOEPTSIZ_PKTCNT_1 | State->MaxPacketSize[ i ];
                ctrl |= OTG_DOEPCTL_EPENA | OTG_DOEPCTL_CNAK; // enable rx endpoint
                OTG->DOEP[ i ].CTL = ctrl; // configure out endpoint
                intMask |= bit << 16; // enable out interrupt
            }
        }
        i++;
        eptype >>= 2;
        bit <<= 1;
    }

    // enable interrupts
    OTG->DIEPMSK = OTG_DIEPMSK_XFRCM; // transfer complete
    OTG->DOEPMSK = OTG_DOEPMSK_XFRCM | OTG_DOEPMSK_STUPM; // setup stage done
    OTG->DAINTMSK = intMask;   // enable ep interrupts
    OTG->GINTMSK = OTG_GINTMSK_OEPINT | OTG_GINTMSK_IEPINT | OTG_GINTMSK_RXFLVLM
                 | OTG_GINTMSK_USBRST | OTG_GINTMSK_USBSUSPM | OTG_GINTMSK_WUIM;

    OTG->DCFG &= ~OTG_DCFG_DAD; // reset device address

    /* clear all flags */
    USB_ClearEvent( 0, USB_EVENT_ALL );

    State->FirstGetDescriptor = TRUE;

    State->DeviceState = USB_DEVICE_STATE_DEFAULT;
    State->Address = 0;
    USB_StateCallback( State );
}

/*
 * Data Endpoint Rx Interrupt Handler
 */
void STM32F4_USB_Driver_EP_RX_Int( OTG_TypeDef* OTG
                                 , USB_CONTROLLER_STATE* State
                                 , UINT32 ep
                                 , UINT32 count
                                 )
{
    ASSERT_IRQ_MUST_BE_OFF( );

    UINT32* pd;

    if( ep == 0 )
    { // control endpoint
        USB_Debug( count ? 'c' : '0' );

        pd = ( UINT32* )( ( STM32F4_USB_STATE* )State )->ep0Buffer;
        State->Data = ( BYTE* )pd;
        State->DataSize = count;
    }
    else
    { // data endpoint
        USB_Debug( 'r' );

        BOOL full;
        USB_PACKET64* Packet64 = USB_RxEnqueue( State, ep, full );
        if( Packet64 == NULL )
        {  // should not happen
            USB_Debug( '?' );
            _ASSERT( 0 );
        }
        pd = ( UINT32* )Packet64->Buffer;
        Packet64->Size = count;
    }

    // read data
    uint32_t volatile* ps = OTG->DFIFO[ ep ];
    for( int c = count; c > 0; c -= 4 )
    {
        *pd++ = *ps;
    }

    // data handling & Rx reenabling delayed to transfer completed interrupt
}

/*
 * Data In (Tx) Endpoint Interrupt Handler
 */
void STM32F4_USB_Driver_EP_In_Int( OTG_TypeDef* OTG, USB_CONTROLLER_STATE* State, UINT32 ep )
{
    ASSERT_IRQ_MUST_BE_OFF( );

    UINT32 bits = OTG->DIEP[ ep ].INT;
    if( bits & OTG_DIEPINT_XFRC )
    { // transfer completed
        USB_Debug( '3' );
        OTG->DIEP[ ep ].INT = OTG_DIEPINT_XFRC; // clear interrupt
    }

    if( !( OTG->DIEP[ ep ].CTL & OTG_DIEPCTL_EPENA ) )
    { // Tx idle

        UINT32* ps = NULL;
        UINT32 count;

        if( ep == 0 )
        { // control endpoint
            if( State->DataCallback )
            { // data to send
                State->DataCallback( State );  // this call can't fail
                ps = ( UINT32* )State->Data;
                count = State->DataSize;

                USB_Debug( count ? 'x' : 'n' );
            }
        }
        else if( State->Queues[ ep ] != NULL && State->IsTxQueue[ ep ] )
        { // Tx data endpoint
            USB_PACKET64* Packet64 = USB_TxDequeue( State, ep, TRUE );
            if( Packet64 )
            {  // data to send
                ps = ( UINT32* )Packet64->Buffer;
                count = Packet64->Size;

                USB_Debug( 's' );
            }
        }

        if( ps )
        { // data to send
            // enable endpoint
            OTG->DIEP[ ep ].TSIZ = OTG_DIEPTSIZ_PKTCNT_1 | count;
            OTG->DIEP[ ep ].CTL |= OTG_DIEPCTL_EPENA | OTG_DIEPCTL_CNAK;

            // write data
            uint32_t volatile* pd = OTG->DFIFO[ ep ];
            for( int c = count; c > 0; c -= 4 )
            {
                *pd = *ps++;
            }
        }
        else
        { // no data
            // disable endpoint
            OTG->DIEP[ ep ].CTL |= OTG_DIEPCTL_SNAK;
        }
    }
}

/*
 * Handle Setup Data Received on Control Endpoint
 */
void STM32F4_USB_Driver_Handle_Setup( OTG_TypeDef* OTG, USB_CONTROLLER_STATE* State )
{
    /* send last setup packet to the upper layer */
    UINT8 result = USB_ControlCallback( State );

    switch( result )
    {

    case USB_STATE_DATA:
        /* setup packet was handled and the upper layer has data to send */
        break;

    case USB_STATE_ADDRESS:
        /* upper layer needs us to change the address */
        USB_Debug( 'a' );
        OTG->DCFG |= State->Address << 4; // set device address
        break;

    case USB_STATE_DONE:
        State->DataCallback = NULL;
        break;

    case USB_STATE_STALL:
        // setup packet failed to process successfully
        // set stall condition on the control endpoint
        OTG->DIEP[ 0 ].CTL |= OTG_DIEPCTL_STALL;
        OTG->DOEP[ 0 ].CTL |= OTG_DOEPCTL_STALL;
        USB_Debug( 'l' );
        // ********** skip rest of function **********
        return;

    case USB_STATE_STATUS:
        break;

    case USB_STATE_CONFIGURATION:
        break;

    case USB_STATE_REMOTE_WAKEUP:
        // It is not using currently as the device side won't go into SUSPEND mode unless
        // the PC is purposely to select it to SUSPEND, as there is always SOF in the bus
        // to keeping the device from SUSPEND.
        USB_Debug( 'w' );
        break;

    default:
        _ASSERT( 0 );
        break;
    }

    // check ep0 for replies
    STM32F4_USB_Driver_EP_In_Int( OTG, State, 0 );

    // check all Tx endpoints after configuration setup
    if( result == USB_STATE_CONFIGURATION )
    {
        for( int ep = 1; ep < State->EndpointCount; ep++ )
        {
            if( State->Queues[ ep ] && State->IsTxQueue[ ep ] )
            {
                STM32F4_USB_Driver_EP_In_Int( OTG, State, ep );
            }
        }
    }
}

/*
 * Data Out (Rx) Endpoint Interrupt Handler
 */
void STM32F4_USB_Driver_EP_Out_Int( OTG_TypeDef* OTG, USB_CONTROLLER_STATE* State, UINT32 ep )
{
    ASSERT_IRQ_MUST_BE_OFF( );
    UINT32 bits = OTG->DOEP[ ep ].INT;
    if( bits & OTG_DOEPINT_XFRC )
    { // transfer completed
        USB_Debug( '1' );
        OTG->DOEP[ ep ].INT = OTG_DOEPINT_XFRC; // clear interrupt
    }

    if( bits & OTG_DOEPINT_STUP )
    { // setup phase done
        USB_Debug( '2' );
        OTG->DOEP[ ep ].INT = OTG_DOEPINT_STUP; // clear interrupt
    }

    if( ep == 0 )
    { // control endpoint
        USB_Debug( '$' );
        // enable endpoint
        OTG->DOEP[ 0 ].TSIZ = OTG_DOEPTSIZ_STUPCNT | OTG_DOEPTSIZ_PKTCNT_1 | State->PacketSize;
        OTG->DOEP[ 0 ].CTL |= OTG_DOEPCTL_EPENA | OTG_DOEPCTL_CNAK;
        // Handle Setup data in upper layer
        STM32F4_USB_Driver_Handle_Setup( OTG, State );
    }
    else if( !State->Queues[ ep ]->IsFull( ) )
    {
        // enable endpoint
        OTG->DOEP[ ep ].TSIZ = OTG_DOEPTSIZ_PKTCNT_1 | State->MaxPacketSize[ ep ];
        OTG->DOEP[ ep ].CTL |= OTG_DOEPCTL_EPENA | OTG_DOEPCTL_CNAK;
        USB_Debug( 'E' );
    }
    else
    {
        // disable endpoint
        OTG->DOEP[ ep ].CTL |= OTG_DOEPCTL_SNAK;
        USB_Debug( 'v' );
    }
}


/*
 * Main Interrupt Handler
 */
void STM32F4_USB_Driver_Interrupt( OTG_TypeDef* OTG, USB_CONTROLLER_STATE* State )
{
    INTERRUPT_START;

    UINT32 intPend = OTG->GINTSTS; // get pending bits

    while( intPend & OTG_GINTSTS_RXFLVL )
    { // RxFifo non empty
        UINT32 status = OTG->GRXSTSP; // read and pop status word from fifo
        int ep = status & OTG_GRXSTSP_EPNUM;
        int count = ( status & OTG_GRXSTSP_BCNT ) >> 4;
        status &= OTG_GRXSTSP_PKTSTS;
        if( status == OTG_GRXSTSP_PKTSTS_PR // data received
         || status == OTG_GRXSTSP_PKTSTS_SR // setup received
          )
        { 
            STM32F4_USB_Driver_EP_RX_Int( OTG, State, ep, count );
        }
        else
        {
            // others: nothing to do
        }
        intPend = OTG->GINTSTS; // update pending bits
    }

    if( intPend & OTG_GINTSTS_IEPINT )
    { // IN endpoint
        UINT32 bits = OTG->DAINT & 0xFFFF; // pending IN endpoints
        int ep = 0;
        while( bits )
        {
            if( bits & 1 )
                STM32F4_USB_Driver_EP_In_Int( OTG, State, ep );
            ep++;
            bits >>= 1;
        }
    }

    if( intPend & OTG_GINTSTS_OEPINT )
    { // OUT endpoint
        UINT32 bits = OTG->DAINT >> 16; // pending OUT endpoints
        int ep = 0;
        while( bits )
        {
            if( bits & 1 )
                STM32F4_USB_Driver_EP_Out_Int( OTG, State, ep );

            ep++;
            bits >>= 1;
        }
    }

    if( intPend & OTG_GINTSTS_USBRST )
    { // reset
        STM32F4_USB_Driver_ResetEvent( OTG, State );
        // OTG->GINTSTS = OTG_GINTSTS_USBRST; // clear interrupt
    }
    else
    {
        if( intPend & OTG_GINTSTS_USBSUSP )
        { // suspend
            STM32F4_USB_Driver_SuspendEvent( OTG, State );
            OTG->GINTSTS = OTG_GINTSTS_USBSUSP; // clear interrupt
        }

        if( intPend & OTG_GINTSTS_WKUPINT )
        { // wakeup
            STM32F4_USB_Driver_ResumeEvent( OTG, State );
            OTG->GINTSTS = OTG_GINTSTS_WKUPINT; // clear interrupt
        }
    }

    INTERRUPT_END;
}

/*
 * OTG FS Interrupt Handler
 */
void STM32F4_USB_Driver_FS_Interrupt( void* param )
{
    STM32F4_USB_Driver_Interrupt( OTG_FS, &STM32F4_USB_ControllerState[ STM32F4_USB_FS_IDX ].state );
}

/*
 * OTG HS Interrupt Handler
 */
void STM32F4_USB_Driver_HS_Interrupt( void* param )
{
    STM32F4_USB_Driver_Interrupt( OTG_HS, &STM32F4_USB_ControllerState[ STM32F4_USB_HS_IDX ].state );
}


USB_CONTROLLER_STATE * CPU_USB_GetState( int Controller )
{
    if( ( UINT32 )Controller >= TOTAL_USB_CONTROLLER )
        return NULL;
    
    return &STM32F4_USB_ControllerState[ STM32F4_USB_IDX( Controller ) ].state;
}

HRESULT CPU_USB_Initialize( int Controller )
{
    if( ( UINT32 )Controller >= TOTAL_USB_CONTROLLER )
        return S_FALSE;

    // enable USB clock
    if( STM32F4_USB_IS_HS( Controller ) )
    { // HS on AHB1
        RCC->AHB1ENR |= RCC_AHB1ENR_OTGHSEN;
        // this is needed to enable the FS phy clock when the CPU is sleeping
        RCC->AHB1LPENR &= ~RCC_AHB1LPENR_OTGHSULPILPEN;
    }
    else
    { // FS on AHB2
        RCC->AHB2ENR |= RCC_AHB2ENR_OTGFSEN;
    }

    USB_CONTROLLER_STATE *State = &STM32F4_USB_ControllerState[ STM32F4_USB_IDX( Controller ) ].state;

    OTG_TypeDef* OTG = STM32F4_USB_REGS( Controller );

    GLOBAL_LOCK( irq );

    // Detach usb port for a while to enforce re-initialization
    OTG->DCTL = OTG_DCTL_SDIS; // soft disconnect

    OTG->GAHBCFG = OTG_GAHBCFG_TXFELVL;     // int on TxFifo completely empty, int off
    OTG->GUSBCFG = OTG_GUSBCFG_FDMOD        // force device mode
                 | STM32F4_USB_TRDT << 10   // turnaround time
                 | OTG_GUSBCFG_PHYSEL;      // internal PHY

    OTG->GCCFG = OTG_GCCFG_VBUSBSEN       // B device Vbus sensing
               | OTG_GCCFG_PWRDWN;        // transceiver enabled

    OTG->DCFG |= OTG_DCFG_DSPD;           // device speed = HS

    if( STM32F4_USB_USE_VB_PIN( Controller ) == 0 )
    { // no Vbus pin
        OTG->GCCFG |= OTG_GCCFG_NOVBUSSENS; // disable vbus sense
    }

    // setup usb state variables
    ( ( STM32F4_USB_STATE* )State )->pinsProtected = TRUE;
    State->EndpointStatus = ( ( STM32F4_USB_STATE* )State )->endpointStatus;
    State->EndpointCount = USB_MAX_EP( Controller );
    //State->DeviceStatus = USB_STATUS_DEVICE_SELF_POWERED;

    // get max ep0 packet size from actual configuration
    const USB_DEVICE_DESCRIPTOR* desc = ( USB_DEVICE_DESCRIPTOR* )USB_FindRecord( State, USB_DEVICE_DESCRIPTOR_MARKER, 0 );
    State->PacketSize = desc ? desc->bMaxPacketSize0 : DEF_EP0_SIZE;

    // use defaults for unused endpoints
    int idx;
    for( idx = 1; idx < State->EndpointCount; idx++ )
    {
        State->IsTxQueue[ idx ] = FALSE;
        State->MaxPacketSize[ idx ] = USB_MAX_DATA_PACKET_SIZE;  // 64
    }
    UINT32 epType = 0; // set all endpoints to unused
    UINT32 queueIdx = STM32F4_USB_IDX( Controller ) ? USB_MAX_EP( 0 ) - 1 : 0; // first queue buffer

    // get endpoint configuration
    const USB_ENDPOINT_DESCRIPTOR  *ep = NULL;
    const USB_INTERFACE_DESCRIPTOR *itfc = NULL;
    while( USB_NextEndpoint( State, ep, itfc ) )
    {
        // Figure out which endpoint we are initializing
        idx = ep->bEndpointAddress & 0x7F;

        // Check interface and endpoint numbers against hardware capability
        if( idx >= State->EndpointCount || itfc->bInterfaceNumber > 3 )
            return S_FALSE;

        if( ep->bEndpointAddress & 0x80 )
            State->IsTxQueue[ idx ] = TRUE;

        // Set the maximum size of the endpoint hardware FIFO
        int endpointSize = ep->wMaxPacketSize;

        // If the endpoint maximum size in the configuration list is bogus
        if( endpointSize != 8 && endpointSize != 16 && endpointSize != 32 && endpointSize != 64 )
            return S_FALSE;

        State->MaxPacketSize[ idx ] = endpointSize;

        // assign queues
        QueueBuffers[ queueIdx ].Initialize( );           // Clear queue before use
        State->Queues[ idx ] = &QueueBuffers[ queueIdx ];  // Attach queue to endpoint
        queueIdx++;

        // *****************************************
        // iso endpoints are currently not supported
        // *****************************************
        if( ( ep->bmAttributes & 3 ) == USB_ENDPOINT_ATTRIBUTE_ISOCHRONOUS )
            return FALSE;

        // remember endpoint types
        epType |= ( ep->bmAttributes & 3 ) << ( idx * 2 );
    }

    // If no endpoints were initialized, something is seriously wrong with the configuration list
    if( epType == 0 )
        return S_FALSE;

    ( ( STM32F4_USB_STATE* )State )->EP_Type = epType;

    HAL_Time_Sleep_MicroSeconds( 1000 ); // asure host recognizes reattach

    // setup hardware
    CPU_USB_ProtectPins( Controller, FALSE );
    if( STM32F4_USB_IS_HS( Controller ) )
    { // HS
        CPU_INTC_ActivateInterrupt( OTG_HS_IRQn, STM32F4_USB_Driver_HS_Interrupt, 0 );
        CPU_INTC_ActivateInterrupt( OTG_HS_WKUP_IRQn, STM32F4_USB_Driver_HS_Interrupt, 0 );
    }
    else
    { // FS
        CPU_INTC_ActivateInterrupt( OTG_FS_IRQn, STM32F4_USB_Driver_FS_Interrupt, 0 );
        CPU_INTC_ActivateInterrupt( OTG_FS_WKUP_IRQn, STM32F4_USB_Driver_FS_Interrupt, 0 );
    }

    // allow interrupts
    OTG->GINTSTS = 0xFFFFFFFF;           // clear all interrupts
    OTG->GINTMSK = OTG_GINTMSK_USBRST;   // enable reset only
    OTG->DIEPEMPMSK = 0;                 // disable Tx FIFO empty interrupts
    OTG->GAHBCFG |= OTG_GAHBCFG_GINTMSK; // gloabl interrupt enable

    // rest of initializations done in reset interrupt handler

    USB_Debug( '*' );

    return S_OK;
}

HRESULT CPU_USB_Uninitialize( int Controller )
{
    if( STM32F4_USB_IS_HS( Controller ) )
    { // HS
        CPU_INTC_DeactivateInterrupt( OTG_HS_WKUP_IRQn );
        CPU_INTC_DeactivateInterrupt( OTG_HS_IRQn );
    }
    else
    { // FS
        CPU_INTC_DeactivateInterrupt( OTG_FS_WKUP_IRQn );
        CPU_INTC_DeactivateInterrupt( OTG_FS_IRQn );
    }

    CPU_USB_ProtectPins( Controller, TRUE );

    // disable USB clock
    if( STM32F4_USB_IS_HS( Controller ) )
    { // HS on AHB1
        RCC->AHB1ENR &= ~RCC_AHB1ENR_OTGHSEN;
    }
    else
    { // FS on AHB2
        RCC->AHB2ENR &= ~RCC_AHB2ENR_OTGFSEN;
    }

    return S_OK;
}

BOOL CPU_USB_StartOutput( USB_CONTROLLER_STATE* State, int ep )
{
    if( State == NULL || ep >= State->EndpointCount )
        return FALSE;

    OTG_TypeDef* OTG = STM32F4_USB_REGS( State->ControllerNum );

    USB_Debug( 't' );

    GLOBAL_LOCK( irq );

    // If endpoint is not an output
    if( State->Queues[ ep ] == NULL || !State->IsTxQueue[ ep ] )
        return FALSE;

    /* if the halt feature for this endpoint is set, then just clear all the characters */
    if( State->EndpointStatus[ ep ] & USB_STATUS_ENDPOINT_HALT )
    {
        while( USB_TxDequeue( State, ep, TRUE ) != NULL )
        {
        } // clear TX queue

        return TRUE;
    }

    if( irq.WasDisabled( ) )
    { // check all endpoints for pending actions
        STM32F4_USB_Driver_Interrupt( OTG, State );
    }
    // write first packet if not done yet
    STM32F4_USB_Driver_EP_In_Int( OTG, State, ep );

    return TRUE;
}

BOOL CPU_USB_RxEnable( USB_CONTROLLER_STATE* State, int ep )
{
    // If this is not a legal Rx queue
    if( State == NULL || State->Queues[ ep ] == NULL || State->IsTxQueue[ ep ] )
        return FALSE;

    OTG_TypeDef* OTG = STM32F4_USB_REGS( State->ControllerNum );

    USB_Debug( 'e' );

    GLOBAL_LOCK( irq );

    // enable Rx
    if( !( OTG->DOEP[ ep ].CTL & OTG_DOEPCTL_EPENA ) )
    {
        OTG->DOEP[ ep ].TSIZ = OTG_DOEPTSIZ_PKTCNT_1 | State->MaxPacketSize[ ep ];
        OTG->DOEP[ ep ].CTL |= OTG_DOEPCTL_EPENA | OTG_DOEPCTL_CNAK; // enable endpoint
    }

    return TRUE;
}

BOOL CPU_USB_GetInterruptState( ) // Controller missing!
{
    return FALSE;
}

BOOL CPU_USB_ProtectPins( int Controller, BOOL On )
{
    USB_CONTROLLER_STATE *State = &STM32F4_USB_ControllerState[ STM32F4_USB_IDX( Controller ) ].state;

    if( ( ( STM32F4_USB_STATE* )State )->EP_Type == 0 )
        return FALSE;  // not yet initialized

    OTG_TypeDef* OTG = STM32F4_USB_REGS( Controller );

    GLOBAL_LOCK( irq );

    if( On )
    {
        if( !( ( STM32F4_USB_STATE* )State )->pinsProtected )
        {
            USB_Debug( '+' );

            ( ( STM32F4_USB_STATE* )State )->pinsProtected = TRUE;

            // detach usb port
            OTG->DCTL |= OTG_DCTL_SDIS; // soft disconnect

            // clear USB Txbuffer
            for( int ep = 1; ep < State->EndpointCount; ep++ )
            {
                if( State->Queues[ ep ] && State->IsTxQueue[ ep ] )
                {
                    while( USB_TxDequeue( State, ep, TRUE ) != NULL );  // clear TX queue
                }
            }

            State->DeviceState = USB_DEVICE_STATE_DETACHED;
            USB_StateCallback( State );
        }
    }
    else
    {
        if( ( ( STM32F4_USB_STATE* )State )->pinsProtected )
        {

            USB_Debug( '-' );

            ( ( STM32F4_USB_STATE* )State )->pinsProtected = FALSE;

            GPIO_ALT_MODE altMode = STM32F4_USB_ALT_MODE( Controller );
            CPU_GPIO_DisablePin( STM32F4_USB_DM_PIN( Controller ), RESISTOR_DISABLED, 0, altMode );
            CPU_GPIO_DisablePin( STM32F4_USB_DP_PIN( Controller ), RESISTOR_DISABLED, 0, altMode );
            if( STM32F4_USB_USE_ID_PIN( Controller ) )
            {
                CPU_GPIO_DisablePin( STM32F4_USB_ID_PIN( Controller ), RESISTOR_DISABLED, 0, altMode );
            }

            // attach usb port
            OTG->DCTL &= ~OTG_DCTL_SDIS; // remove soft disconnect

            State->DeviceState = USB_DEVICE_STATE_ATTACHED;
            USB_StateCallback( State );
        }
    }

    return TRUE;
}

