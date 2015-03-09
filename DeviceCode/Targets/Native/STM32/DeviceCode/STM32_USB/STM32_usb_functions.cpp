////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32: Copyright (c) Oberon microsystems, Inc.
//
//  *** USB Driver ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <pal\com\usb\USB.h>
#include "..\stm32f10x.h"


/////////////////// Definitions Missing From stm32f10x.h ///////////////////

typedef struct
{
  __IO uint32_t EPiR[8];
  __IO uint32_t RESERVED0[8];
  __IO uint32_t CNTR;
  __IO uint32_t ISTR;
  __IO uint32_t FNR;
  __IO uint32_t DADDR;
  __IO uint32_t BTABLE;
} USB_TypeDef;

#define USB_BASE            (APB1PERIPH_BASE + 0x5C00)
#define USB_PMA_BASE        (APB1PERIPH_BASE + 0x6000)

#define USB                 ((USB_TypeDef *) USB_BASE)

#define USB_EP_BULK         0
#define USB_EP_CONTROL      USB_EP0R_EP_TYPE_0
#define USB_EP_ISOCHRONOUS  USB_EP0R_EP_TYPE_1
#define USB_EP_INTERRUPT    (USB_EP0R_EP_TYPE_0 | USB_EP0R_EP_TYPE_1)

#define USB_EP_TX_DIS       0
#define USB_EP_TX_STALL     USB_EP0R_STAT_TX_0
#define USB_EP_TX_NAK       USB_EP0R_STAT_TX_1
#define USB_EP_TX_VALID     (USB_EP0R_STAT_TX_0 | USB_EP0R_STAT_TX_1)

#define USB_EP_RX_DIS       0
#define USB_EP_RX_STALL     USB_EP0R_STAT_RX_0
#define USB_EP_RX_NAK       USB_EP0R_STAT_RX_1
#define USB_EP_RX_VALID     (USB_EP0R_STAT_RX_0 | USB_EP0R_STAT_RX_1)

// non toggle fields
#define USB_EP_NT           (USB_EP0R_CTR_RX | USB_EP0R_SETUP | USB_EP0R_EP_TYPE \
                             | USB_EP0R_EP_KIND | USB_EP0R_CTR_TX | USB_EP0R_EA)


/////////////////// Packet Memory Area Layout ///////////////////

// arm  usb
// 000  000  TxBuf0
// 040  020  RxBuf0
// 080  040  Buf1
// 100  080  Buf2
// 180  0C0  Buf3
// 200  100  Buf4
// 280  140  Buf5
// 300  180  Buf6
// 380  1C0  descriptor table

typedef struct
{
  uint32_t EP_BUF[7][32];   // 7 endpoint buffers (64 bytes)
  struct {
    uint32_t ADDR_TX;
    uint32_t COUNT_TX;
    uint32_t ADDR_RX;
    uint32_t COUNT_RX;
  } EP_BUF_DESC[7];         // 7 endpoint buffer descriptors
} USB_PMA_TypeDef;

#define USB_PMA             ((USB_PMA_TypeDef *) USB_PMA_BASE)


//////////////////////////////////////////////////////////////


#define USB_Debug(s)

#ifdef DEBUG
#define _ASSERT(x) ASSERT(x)
#else
#define _ASSERT(x)
#endif


#define USB_MAX_EP USB_MAX_QUEUES
#define MAX_EP0_SIZE 8


// USB attach pin handling
#if defined(STM32_USB_Attach_Pin_High)
#define STM32_USB_Init_Pin  CPU_GPIO_EnableOutputPin(STM32_USB_Attach_Pin_High, FALSE)
#define STM32_USB_Attach    CPU_GPIO_SetPinState(STM32_USB_Attach_Pin_High, TRUE)
#define STM32_USB_Detach    CPU_GPIO_SetPinState(STM32_USB_Attach_Pin_High, FALSE)
#elif defined(STM32_USB_Attach_Pin_Low)
#define STM32_USB_Init_Pin  CPU_GPIO_EnableOutputPin(STM32_USB_Attach_Pin_Low, TRUE)
#define STM32_USB_Attach    CPU_GPIO_SetPinState(STM32_USB_Attach_Pin_Low, FALSE)
#define STM32_USB_Detach    CPU_GPIO_SetPinState(STM32_USB_Attach_Pin_Low, TRUE)
#elif defined(STM32_USB_Attach_Pin_Direct)
#define STM32_USB_Init_Pin  CPU_GPIO_DisablePin(STM32_USB_Attach_Pin_Direct, \
                                                RESISTOR_DISABLED, FALSE, GPIO_ALT_PRIMARY)
#define STM32_USB_Attach    CPU_GPIO_EnableOutputPin(STM32_USB_Attach_Pin_Direct, TRUE)
#define STM32_USB_Detach    CPU_GPIO_DisablePin(STM32_USB_Attach_Pin_Direct, \
                                                RESISTOR_DISABLED, FALSE, GPIO_ALT_PRIMARY)
#else
#define STM32_USB_Init_Pin
#define STM32_USB_Attach
#define STM32_USB_Detach
#endif




/* State variables for the controller (one controller only) */
static USB_CONTROLLER_STATE STM32_USB_ControllerState;

/* Queues for all data endpoints */
Hal_Queue_KnownSize<USB_PACKET64,USB_QUEUE_PACKET_COUNT> QueueBuffers[USB_MAX_EP - 1];

/* Auxiliary state variables */
static UINT8 STM32_USB_EP0Buffer[MAX_EP0_SIZE];
static UINT16 STM32_USB_EndpointStatus[USB_MAX_EP];
static BOOL STM32_USB_PinsProtected;
static UINT8 STM32_USB_PreviousDeviceState;
static UINT16 STM32_USB_EP_Type = 0;


void STM32_USB_Driver_EP_In_Int (UINT32 epreg, USB_CONTROLLER_STATE* State, int endpoint);
void STM32_USB_Driver_EP0_TX_Int (UINT32 epreg, USB_CONTROLLER_STATE* State);


/*
 * Set Rx status bits
 */
void STM32_USB_SetRxStatus (int ep, int status)
{
    USB->EPiR[ep] = ((USB->EPiR[ep] & (USB_EP_NT | USB_EP0R_STAT_RX)) ^ status)
                    | (USB_EP0R_CTR_RX | USB_EP0R_CTR_TX);
}

/*
 * Set Tx status bits
 */
void STM32_USB_SetTxStatus (int ep, int status)
{
    USB->EPiR[ep] = ((USB->EPiR[ep] & (USB_EP_NT | USB_EP0R_STAT_TX)) ^ status)
                    | (USB_EP0R_CTR_RX | USB_EP0R_CTR_TX);
}

/*
 * Suspend Event Interrupt Handler
 */
void STM32_USB_Driver_SuspendEvent (USB_CONTROLLER_STATE* State)
{
    // SUSPEND event only happened when Host(PC) set the device to SUSPEND
    // as there is always SOF every 1ms on the BUS to keep the device from
    // suspending. Therefore, the REMOTE wake up is not necessary at the device side
    
    USB_Debug("S");
    
    USB->CNTR |= USB_CNTR_FSUSP; // force suspend
    //USB->CNTR |= USB_CNTR_LP_MODE; // set to low power
    
    STM32_USB_PreviousDeviceState = State->DeviceState;
    State->DeviceState = USB_DEVICE_STATE_SUSPENDED;
    USB_StateCallback( State );
}

/*
 * Resume Event Interrupt Handler
 */
void STM32_USB_Driver_ResumeEvent (USB_CONTROLLER_STATE* State)
{
    USB_Debug("R");
    
    USB->CNTR &= ~USB_CNTR_FSUSP; // wake up
    
    State->DeviceState = STM32_USB_PreviousDeviceState;
    USB_StateCallback( State );
}

/*
 * Reset Event Interrupt Handler
 */
void STM32_USB_Driver_ResetEvent (USB_CONTROLLER_STATE* State)
{
    USB_Debug("!");
    
    USB->ISTR = 0; // clear pending bits
    USB->CNTR = USB_CNTR_CTRM | USB_CNTR_RESETM | USB_CNTR_SUSPM | USB_CNTR_WKUPM;
    
    USB->BTABLE = 0x1C0; // buffer descriptor table at end of buffer memory
    
    // configure control endpoint
    USB->EPiR[0] = USB_EP_CONTROL | USB_EP_RX_VALID | USB_EP_TX_NAK;
    
    //  configure data endpoints
    UINT32 eptype = STM32_USB_EP_Type >> 2; // endpoint types (2 bits / endpoint)
    for (int i = 1; i < USB_MAX_EP; i++) {
        UINT32 val = USB_EP_BULK, type = eptype & 3;
        if (type != 0) { // data endpoint
            // type (Bulk is default)
            if (type == USB_ENDPOINT_ATTRIBUTE_INTERRUPT) {
                val = USB_EP_INTERRUPT;
            } else if (type == USB_ENDPOINT_ATTRIBUTE_ISOCHRONOUS) {
                val = USB_EP_ISOCHRONOUS;
            }
            // state
            if (State->IsTxQueue[i]) { // Tx endpoint
                val |= USB_EP_RX_DIS | USB_EP_TX_NAK; // no tx data yet
            } else { // Rx endpoint
                val |= USB_EP_RX_VALID | USB_EP_TX_DIS;
            }
        }
        USB->EPiR[i] = val | i; // set type, state, & addr
        eptype >>= 2;
    }

    USB->DADDR = USB_DADDR_EF; // reset device addr to 0
    
    /* clear all flags */
    USB_ClearEvent(0, USB_EVENT_ALL);

    State->FirstGetDescriptor = TRUE;

    State->DeviceState = USB_DEVICE_STATE_DEFAULT;
    State->Address = 0;
    USB_StateCallback( State );
}


/*
 * Control Endpoint Rx Interrupt Handler
 */
void STM32_USB_Driver_EP0_RX_Int (UINT32 epreg, USB_CONTROLLER_STATE* State)
{
    ASSERT_IRQ_MUST_BE_OFF();

    _ASSERT((epreg & USB_EP0R_STAT_RX) == USB_EP_RX_NAK);
    
    int count = USB_PMA->EP_BUF_DESC[0].COUNT_RX & 0x03FF; // received count
    
    if (epreg & USB_EP0R_SETUP)
        USB_Debug("C");
    else if (count)
        USB_Debug("c");
    else
        USB_Debug("0");
    
    // copy buffer
    UINT32* ps = &(USB_PMA->EP_BUF[0][16]); // second half of buffer 0
    UINT16* pd = (UINT16*)STM32_USB_EP0Buffer;
    for (int n = count; n > 0; n -= 2) {
        *pd++ = *ps++;
    }
    
    /* send the data to the upper layer */
    State->Data = &STM32_USB_EP0Buffer[0];
    State->DataSize = count;
    UINT8 result = USB_ControlCallback(State);

    switch(result) {

    case USB_STATE_DATA:
        /* setup packet was handled and the upper layer has data to send */
        break;

    case USB_STATE_ADDRESS:
        /* upper layer needs us to change the address */
        // address change delayed to end of reply packet
        break;

    case USB_STATE_DONE:
        State->DataCallback = NULL;
        break;

    case USB_STATE_STALL:
        // setup packet failed to process successfully
        // set stall condition on the control endpoint
        STM32_USB_SetRxStatus(0, USB_EP_RX_STALL);
        USB_Debug("l");
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
        USB_Debug("w");
        break;

    default:
        _ASSERT(0);
        break;            
    }

    // reenable Rx
    STM32_USB_SetRxStatus(0, USB_EP_RX_VALID);

    // check for replies
    STM32_USB_Driver_EP0_TX_Int(epreg, State);

    // check all Tx endpoints // !!!
    for( int ep = 1; ep < USB_MAX_EP; ep++ ) {
        // If this is an active transmit endpoint
        if( State->Queues[ep] && State->IsTxQueue[ep] )
            STM32_USB_Driver_EP_In_Int(USB->EPiR[ep], State, ep);
    }

}

/*
 * Control Endpoint Tx Interrupt Handler
 */
void STM32_USB_Driver_EP0_TX_Int (UINT32 epreg, USB_CONTROLLER_STATE* State)
{
    ASSERT_IRQ_MUST_BE_OFF();

    if (State->DataCallback && (epreg & USB_EP0R_STAT_TX) != USB_EP_TX_VALID) { // ready to reply
        _ASSERT((epreg & USB_EP0R_STAT_TX) == USB_EP_TX_NAK);
        
        State->DataCallback( State );  // this call can't fail
        int count = State->DataSize;
        
        if (count > 0) {
            USB_Debug("X");
            // load buffer
            UINT16* ps = (UINT16*)State->Data;
            UINT32* pd = &(USB_PMA->EP_BUF[0][0]); // first half of buffer 0
            for (int n = count; n > 0; n -= 2) {
                *pd++ = *ps++;
            }
        } else {
            USB_Debug("N");
        }
        
        // start Tx
        USB_PMA->EP_BUF_DESC[0].COUNT_TX = count; // tx byte count
        STM32_USB_SetTxStatus(0, USB_EP_TX_VALID);
        
    } else if (State->CurrentState == USB_DEVICE_STATE_ADDRESS && (USB->DADDR & 0x7F) == 0) {
        // delayed address change
        USB->DADDR = State->Address | USB_DADDR_EF;
        USB_Debug("a");
    }
}


/*
 * Data Out (Rx) Endpoint Interrupt Handler
 */
void STM32_USB_Driver_EP_Out_Int (UINT32 epreg, USB_CONTROLLER_STATE* State, int endpoint)
{
    ASSERT_IRQ_MUST_BE_OFF();

    _ASSERT((epreg & USB_EP0R_STAT_RX) == USB_EP_RX_NAK);
    
    BOOL full;
    USB_PACKET64* Packet64 = USB_RxEnqueue( State, endpoint, full );

    /* copy packet in, making sure that Packet64->Buffer is never overflowed */
    if (Packet64) {
        USB_Debug("r");
        
        // copy buffer
        int count = USB_PMA->EP_BUF_DESC[endpoint].COUNT_RX & 0x03FF; // received count
        UINT32* ps = USB_PMA->EP_BUF[endpoint];
        UINT16* pd = (UINT16*)Packet64->Buffer;
        for (int n = count; n > 0; n -= 2) {
            *pd++ = *ps++;
        }
        
        Packet64->Size = count;
        if (!full) { // reenable Rx
            STM32_USB_SetRxStatus(endpoint, USB_EP_RX_VALID);
        } else {
            USB_Debug("v");
        }
    } else {  // should not happen
        USB_Debug("?");
        _ASSERT(0);
    }

}

/*
 * Data In (Tx) Endpoint Interrupt Handler
 */
void STM32_USB_Driver_EP_In_Int (UINT32 epreg, USB_CONTROLLER_STATE* State, int endpoint)
{
    ASSERT_IRQ_MUST_BE_OFF();

    // If this is not a legal transmit endpoint, there is nothing more to do
    if (State->Queues[endpoint] != NULL && State->IsTxQueue[endpoint]) {

        if ((epreg & USB_EP0R_STAT_TX) != USB_EP_TX_VALID) { // ready to write next packet
            _ASSERT((epreg & USB_EP0R_STAT_TX) == USB_EP_TX_NAK);

            USB_PACKET64* Packet64 = USB_TxDequeue( State, endpoint, TRUE );
            if (Packet64) {  // more data to send
                USB_Debug("s");
                
                // copy buffer
                int count = Packet64->Size;
                UINT16* ps = (UINT16*)Packet64->Buffer;
                UINT32* pd = USB_PMA->EP_BUF[endpoint];
                for (int n = count; n > 0; n -= 2) {
                    *pd++ = *ps++;
                }
                
                // start Tx
                USB_PMA->EP_BUF_DESC[endpoint].COUNT_TX = count; // tx byte count
                STM32_USB_SetTxStatus(endpoint, USB_EP_TX_VALID);
            }
        }
    }
}

/*
 * Main Interrupt Handler
 */
void STM32_USB_Driver_Interrupt (void* param)
{
    INTERRUPT_START;
    
    USB_CONTROLLER_STATE *State = &STM32_USB_ControllerState;

    UINT32 intPend = USB->ISTR; // get pending bits

    while (intPend & USB_ISTR_CTR) { // ep interrupt pending
        int ep = intPend & USB_ISTR_EP_ID; // actual endpoint
        INT32 epreg = USB->EPiR[ep];
        if (epreg & USB_EP0R_CTR_RX) {
            // reset interrupt
            USB->EPiR[ep] = epreg & (USB_EP_NT & ~USB_EP0R_CTR_RX) | USB_EP0R_CTR_TX;
            if (ep == 0) {
                STM32_USB_Driver_EP0_RX_Int(epreg, State);
            } else {
                STM32_USB_Driver_EP_Out_Int(epreg, State, ep);
            }
        }
        if (epreg & USB_EP0R_CTR_TX) {
            // reset interrupt
            USB->EPiR[ep] = epreg & (USB_EP_NT & ~USB_EP0R_CTR_TX) | USB_EP0R_CTR_RX;
            if (ep == 0) {
                STM32_USB_Driver_EP0_TX_Int(epreg, State);
            } else {
                STM32_USB_Driver_EP_In_Int(epreg, State, ep);
            }
        }
        intPend = USB->ISTR; // reload pending bits
    }

    if (intPend & USB_ISTR_RESET) {
        STM32_USB_Driver_ResetEvent(State);
        USB->ISTR = ~USB_ISTR_RESET; // reset interrupt
    }
    if (intPend & USB_ISTR_SUSP) {
        STM32_USB_Driver_SuspendEvent(State);
        USB->ISTR = ~USB_ISTR_SUSP; // reset interrupt
    }
    if (intPend & USB_ISTR_WKUP) {
        STM32_USB_Driver_ResumeEvent(State);
        USB->ISTR = ~USB_ISTR_WKUP; // reset interrupt
    }
    
    INTERRUPT_END;
}


USB_CONTROLLER_STATE * CPU_USB_GetState( int Controller )
{
    if (Controller != 0) return NULL;
    return &STM32_USB_ControllerState;
}

HRESULT CPU_USB_Initialize( int Controller )
{
    if (Controller != 0) return S_FALSE;
    
    // initialize attach pin and detach usb port
    STM32_USB_Init_Pin;
    
    RCC->APB1ENR |= RCC_APB1ENR_USBEN; // enable USB clock
    
    GLOBAL_LOCK(irq);

    USB_CONTROLLER_STATE *State = &STM32_USB_ControllerState;

    USB->CNTR = USB_CNTR_FRES; // set reset, clear power down, disable interrupts
    
    USB_PMA->EP_BUF_DESC[0].ADDR_TX = 0; // Tx Buffer 0
    USB_PMA->EP_BUF_DESC[0].ADDR_RX = 0x20; // Rx Buffer 0
    USB_PMA->EP_BUF_DESC[0].COUNT_RX = MAX_EP0_SIZE << 9; // Rx Count 0, block size = 8bytes
    for (int i = 1; i < USB_MAX_EP; i++) {
        int bufAdr = i << 6;
        USB_PMA->EP_BUF_DESC[i].ADDR_TX = bufAdr; // Tx Buffer i
        USB_PMA->EP_BUF_DESC[i].ADDR_RX = bufAdr; // Rx Buffer i
        USB_PMA->EP_BUF_DESC[i].COUNT_RX = 0x8400; // Rx Count i, block size = 64bytes
    }
    
    
    // setup usb state variables
    STM32_USB_PinsProtected = TRUE;
    State->EndpointStatus = &STM32_USB_EndpointStatus[0];
    State->EndpointCount  = USB_MAX_EP;
    //State->DeviceStatus = USB_STATUS_DEVICE_SELF_POWERED;
    State->PacketSize = MAX_EP0_SIZE;  // default control port packet size

    // use defaults for unused endpoints
    int idx;
    for (idx = 1; idx < USB_MAX_EP; idx++) {
        State->IsTxQueue[idx] = FALSE;
        State->MaxPacketSize[idx] = USB_MAX_DATA_PACKET_SIZE;  // 64
    }
    STM32_USB_EP_Type = 0; // set all endpoints to unused

    // get endpoint configuration
    const USB_ENDPOINT_DESCRIPTOR  *ep    = NULL;
    const USB_INTERFACE_DESCRIPTOR *itfc  = NULL;
    while( USB_NextEndpoint( State, ep, itfc) )
    {
        // Figure out which endpoint we are initializing
        idx = ep->bEndpointAddress & 0x7F;

        // Check interface and endpoint numbers against hardware capability
        if( idx >= USB_MAX_EP || itfc->bInterfaceNumber > 3 )  // ???
            return S_FALSE;
        
        if (ep->bEndpointAddress & 0x80) State->IsTxQueue[idx] = TRUE;

        // Set the maximum size of the endpoint hardware FIFO
        int endpointSize = ep->wMaxPacketSize;
        // If the endpoint maximum size in the configuration list is bogus
        if( endpointSize != 8 && endpointSize != 16 && endpointSize != 32 && endpointSize != 64 )
            return S_FALSE;
        State->MaxPacketSize[idx] = endpointSize;

        // Since endpoint 0 is only used for control, there is never a need to allocate a buffer for it
        // In fact State->Queues[0] is always NULL - it is a cheap placeholder to make the queueIndex = endpointIndex
        QueueBuffers[idx-1].Initialize();           // Clear queue before use
        State->Queues[idx] = &QueueBuffers[idx-1];  // Attach queue to endpoint
        
        // *****************************************
        // iso endpoints are currently not supported
        // *****************************************
        if ((ep->bmAttributes & 3) == USB_ENDPOINT_ATTRIBUTE_ISOCHRONOUS) return FALSE;
        
        // remember endpoint types
        STM32_USB_EP_Type |= (ep->bmAttributes & 3) << (idx * 2);
    }

    // If no endpoints were initialized, something is seriously wrong with the configuration list
    if (STM32_USB_EP_Type == 0) return S_FALSE;

    HAL_Time_Sleep_MicroSeconds(1000); // asure host recognizes dettach/attach

    // setup hardware
    CPU_USB_ProtectPins(Controller, FALSE);
    CPU_INTC_ActivateInterrupt(USB_HP_CAN1_TX_IRQn,  STM32_USB_Driver_Interrupt, 0);
    CPU_INTC_ActivateInterrupt(USB_LP_CAN1_RX0_IRQn, STM32_USB_Driver_Interrupt, 0);
    CPU_INTC_ActivateInterrupt(USBWakeUp_IRQn,       STM32_USB_Driver_Interrupt, 0);
    
    // allow interrupts & clear reset bit (>1us after exit of power down mode!)
    USB->CNTR = USB_CNTR_RESETM; // enable reset only
    
    // rest of initializations done in reset interrupt handler
    
    USB_Debug("*");
    
    return S_OK;
}

HRESULT CPU_USB_Uninitialize( int Controller )
{
    CPU_INTC_DeactivateInterrupt(USB_HP_CAN1_TX_IRQn);
    CPU_INTC_DeactivateInterrupt(USB_LP_CAN1_RX0_IRQn);
    CPU_INTC_DeactivateInterrupt(USBWakeUp_IRQn);
    USB->CNTR = USB_CNTR_FRES | USB_CNTR_PDWN; // switch USB off
    CPU_USB_ProtectPins(Controller, TRUE);
    STM32_USB_EP_Type = 0; // reset endpoint types
    
    RCC->APB1ENR &= ~RCC_APB1ENR_USBEN; // disable USB clock
    
    return S_OK;
}

BOOL CPU_USB_StartOutput( USB_CONTROLLER_STATE* State, int endpoint )
{
    USB_Debug("t");
    
    if (State == NULL || endpoint >= USB_MAX_EP) return FALSE;

    GLOBAL_LOCK(irq);

    // If endpoint is not an output
    if( State->Queues[endpoint] == NULL || !State->IsTxQueue[endpoint] )
        return FALSE;

    /* if the halt feature for this endpoint is set, then just clear all the characters */
    if (STM32_USB_EndpointStatus[endpoint] & USB_STATUS_ENDPOINT_HALT) {
        while (USB_TxDequeue( State, endpoint, TRUE ) != NULL);  // clear TX queue
        return TRUE;
    }

    if (irq.WasDisabled()) { // check all endpoints for pending actions
        STM32_USB_Driver_Interrupt(0);
    }
    // write first packet if not done yet
    STM32_USB_Driver_EP_In_Int(USB->EPiR[endpoint], State, endpoint); 

    return TRUE;
}

BOOL CPU_USB_RxEnable( USB_CONTROLLER_STATE* State, int endpoint )
{
    USB_Debug("e");
    
    // If this is not a legal Rx queue
    if( State == NULL || State->Queues[endpoint] == NULL || State->IsTxQueue[endpoint] )
        return FALSE;

    // enable Rx
    if ((USB->EPiR[endpoint] & (USB_EP0R_CTR_RX | USB_EP0R_STAT_RX)) == USB_EP_RX_NAK) {
        // Nak & no int pending // !!!
        STM32_USB_SetRxStatus(endpoint, USB_EP_RX_VALID);
    }
    return TRUE;
}

BOOL CPU_USB_GetInterruptState()
{
    return USB->ISTR & USB->CNTR & 0xFF00; // return any unmasked pending interrupt
}

BOOL CPU_USB_ProtectPins( int Controller, BOOL On )
{
    if (STM32_USB_EP_Type == 0) return FALSE;  // not yet initialized

    USB_CONTROLLER_STATE *State = &STM32_USB_ControllerState;

    GLOBAL_LOCK(irq);

    if (On) {
        if (!STM32_USB_PinsProtected) {

            USB_Debug("+");

            STM32_USB_PinsProtected = TRUE;

            // detach usb port
            STM32_USB_Detach;

            // clear USB Txbuffer
            for(int ep = 1; ep < USB_MAX_EP; ep++) {
                if (State->Queues[ep] && State->IsTxQueue[ep]) {
                    while (USB_TxDequeue( State, ep, TRUE ) != NULL);  // clear TX queue
                }
            }

            State->DeviceState = USB_DEVICE_STATE_DETACHED;
            USB_StateCallback( State );

        }
    } else {
        if (STM32_USB_PinsProtected) {

            USB_Debug("-");

            STM32_USB_PinsProtected = FALSE;

            // attach usb port
            STM32_USB_Attach;

            State->DeviceState = USB_DEVICE_STATE_ATTACHED;
            USB_StateCallback( State );
        }
    }

    return TRUE;
}

