////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32: Copyright (c) Oberon microsystems, Inc.
//
//  STM32-specific definitions
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _STM32_PROCESSOR_SELECTOR_H_
#define _STM32_PROCESSOR_SELECTOR_H_ 1

    #define PLATFORM_ARM_DEFINED

#if !defined(PLATFORM_ARM_STM32)
ERROR - WE SHOULD NOT INCLUDE THIS HEADER IF NOT BUILDING A STM32 PLATFORM
#endif

/////////////////////////////////////////////////////////
//
// macros
//

#define GLOBAL_LOCK(x)             SmartPtr_IRQ x
#define DISABLE_INTERRUPTS()       SmartPtr_IRQ::ForceDisabled()
#define ENABLE_INTERRUPTS()        SmartPtr_IRQ::ForceEnabled()
#define INTERRUPTS_ENABLED_STATE() SmartPtr_IRQ::GetState()
#define GLOBAL_LOCK_SOCKETS(x)     SmartPtr_IRQ x

#if defined(_DEBUG)
#define ASSERT_IRQ_MUST_BE_OFF()   /*ASSERT(!SmartPtr_IRQ::GetState())*/
#define ASSERT_IRQ_MUST_BE_ON()    /*ASSERT( SmartPtr_IRQ::GetState())*/
#else
#define ASSERT_IRQ_MUST_BE_OFF()
#define ASSERT_IRQ_MUST_BE_ON()
#endif

#define INTERRUPT_START SystemState_SetNoLock( SYSTEM_STATE_ISR              );   \
                        SystemState_SetNoLock( SYSTEM_STATE_NO_CONTINUATIONS );
#define INTERRUPT_END   SystemState_ClearNoLock( SYSTEM_STATE_NO_CONTINUATIONS ); \
                        SystemState_ClearNoLock( SYSTEM_STATE_ISR              );


//
// macros
//
/////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////
// communicaiton facilities
//

// Port definitions
#define COM1                   ConvertCOM_ComHandle(0)
#define COM2                   ConvertCOM_ComHandle(1)
#define COM3                   ConvertCOM_ComHandle(2)

#define TOTAL_USB_CONTROLLER   1
#define USB1                   ConvertCOM_UsbHandle(0)

#define USB_MAX_QUEUES         7  // 7 endpoints (EP0 + 6)

#define TOTAL_SOCK_PORT        0

#define TOTAL_DEBUG_PORT       1
#define COM_DEBUG              ConvertCOM_DebugHandle(0)

#define COM_MESSAGING          ConvertCOM_MessagingHandle(0)

#define USART_TX_IRQ_INDEX(x)  (37 + x) // NVIC USARTx index

#define USB_IRQ_INDEX          20  // NVIC USB low priority index

#define PLATFORM_DEPENDENT_TX_USART_BUFFER_SIZE    256  // there is one TX for each usart port
#define PLATFORM_DEPENDENT_RX_USART_BUFFER_SIZE    256  // there is one RX for each usart port
#define PLATFORM_DEPENDENT_USB_QUEUE_PACKET_COUNT  8    // there is one queue for each pipe of each endpoint and the size of a single packet is sizeof(USB_PACKET64) == 68 bytes

//
// communicaiton facilities
/////////////////////////////////////////////////////////


#endif
