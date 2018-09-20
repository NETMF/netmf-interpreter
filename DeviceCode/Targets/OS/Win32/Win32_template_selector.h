////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _PLATFORM_<TEMPLATE>_SELECTOR_H_
#define _PLATFORM_<TEMPLATE>_SELECTOR_H_ 1

/////////////////////////////////////////////////////////
//
// macros
//
     
#define GLOBAL_LOCK(x)             SmartPtr_IRQ x
//#define DISABLE_INTERRUPTS()       SmartPtr_IRQ::ForceDisabled()
//#define ENABLE_INTERRUPTS()        SmartPtr_IRQ::ForceEnabled()
//#define INTERRUPTS_ENABLED_STATE() SmartPtr_IRQ::GetState()
#define GLOBAL_LOCK_SOCKETS(x)     SmartPtr_IRQ x

#if defined(_DEBUG)
#define ASSERT_IRQ_MUST_BE_OFF()   ASSERT( HAL_Windows_HasGlobalLock())
#define ASSERT_IRQ_MUST_BE_ON()    ASSERT(!HAL_Windows_HasGlobalLock())
#else
#define ASSERT_IRQ_MUST_BE_OFF()
#define ASSERT_IRQ_MUST_BE_ON()
#endif

//
// macros
//
/////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////
//
// constants
//
// Port definitions
#define TOTAL_USART_PORT                    2
#define COM1                                ConvertCOM_ComHandle(0)
#define COM2                                ConvertCOM_ComHandle(1)

#define TOTAL_USB_CONTROLLER                1
#define USB1                                ConvertCOM_UsbHandle(0)

#define TOTAL_DEBUG_PORT                    1
#define COM_DEBUG                           ConvertCOM_DebugHandle(0)

#define TOTAL_MESSAGING_PORT                1
#define COM_MESSAGING                       ConvertCOM_MessagingHandle(0)

#define DEBUG_TEXT_PORT                     COM_DEBUG
#define DEBUGGER_PORT                       COM_DEBUG
#define MESSAGING_PORT                      COM_MESSAGING

#define PLATFORM_DEPENDENT_TX_USART_BUFFER_SIZE    512  // there is one TX for each usart port
#define PLATFORM_DEPENDENT_RX_USART_BUFFER_SIZE    512  // there is one RX for each usart port
#define PLATFORM_DEPENDENT_USB_QUEUE_PACKET_COUNT  2    // there is one queue for each pipe of each endpoint and the size of a single packet is sizeof(USB_PACKET64) == 68 bytes
//
// constants
//
/////////////////////////////////////////////////////////

#include <processor_selector.h>


#endif // _PLATFORM_<TEMPLATE>_SELECTOR_H_ 1

