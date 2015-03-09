////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for MCBSTM32E board (STM32): Copyright (c) Oberon microsystems, Inc.
//
//  MCBSTM32E-specific definitions
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _PLATFORM_MCBSTM32E_SELECTOR_H_
#define _PLATFORM_MCBSTM32E_SELECTOR_H_ 1

/////////////////////////////////////////////////////////
//
// processor and features
//

#if defined(PLATFORM_ARM_MCBSTM32E)
#define HAL_SYSTEM_NAME                     "MCBSTM32E"

#define STM32F10X_HD  // STM32F103ZE cpu
#define PLATFORM_ARM_STM32

//#define USB_ALLOW_CONFIGURATION_OVERRIDE  1


//
// processor and features
//
/////////////////////////////////////////////////////////


/////////////////////////////////////////////////////////
//
// constants
//

#define SYSTEM_CLOCK_HZ                 72000000  // 72MHz
#define SYSTEM_CYCLE_CLOCK_HZ           72000000  // 72MHz
#define SYSTEM_APB1_CLOCK_HZ            36000000  // 36MHz
#define SYSTEM_APB2_CLOCK_HZ            72000000  // 72MHz
#define SYSTEM_CRYSTAL_CLOCK_HZ         8000000   // 8MHz external clock

#define CLOCK_COMMON_FACTOR             1000000   // GCD(SYSTEM_CLOCK_HZ, 1M)

#define SLOW_CLOCKS_PER_SECOND          1000000   // 1MHz
#define SLOW_CLOCKS_TEN_MHZ_GCD         1000000   // GCD(SLOW_CLOCKS_PER_SECOND, 10M)
#define SLOW_CLOCKS_MILLISECOND_GCD     1000      // GCD(SLOW_CLOCKS_PER_SECOND, 1k)

#define FLASH_MEMORY_Base               0x08000000 // internal Flash
#define FLASH_MEMORY_Size               0x00080000
#define SRAM1_MEMORY_Base               0x20000000 // region includes internal & external Ram
#define SRAM1_MEMORY_Size               0x48100000

#define TXPROTECTRESISTOR               RESISTOR_DISABLED
#define RXPROTECTRESISTOR               RESISTOR_DISABLED
#define CTSPROTECTRESISTOR              RESISTOR_DISABLED
#define RTSPROTECTRESISTOR              RESISTOR_DISABLED

#define INSTRUMENTATION_H_GPIO_PIN      GPIO_PIN_NONE

#define TOTAL_USART_PORT                3
#define USART_DEFAULT_PORT              COM2
#define USART_DEFAULT_BAUDRATE          115200

#define DEBUG_TEXT_PORT                 USB1
#define STDIO                           USB1
#define DEBUGGER_PORT                   USB1
#define MESSAGING_PORT                  USB1

#define STM32_USE_I2C2                  1

#define STM32_USB_Attach_Pin_Low        (16 + 14) // B14


#define DRIVER_PAL_BUTTON_MAPPING                  \
    { 6 * 16 + 15, BUTTON_B0 }, /* G15: Up */      \
    { 3 * 16 +  3, BUTTON_B1 }, /* D3:  Down */    \
    { 6 * 16 + 14, BUTTON_B2 }, /* G14: Left */    \
    { 6 * 16 + 13, BUTTON_B3 }, /* G13: Right */   \
    { 6 * 16 +  7, BUTTON_B4 }, /* G7:  Enter */   \
    { 6 * 16 +  8, BUTTON_B5 }, /* G8:  User */

//
// constants
/////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////
//
// global functions
//

//
// global functions
//
/////////////////////////////////////////////////////////

#include <processor_selector.h>

#endif // PLATFORM_ARM_MCBSTM32E
//
// drivers
/////////////////////////////////////////////////////////

#endif // _PLATFORM_MCBSTM32E_SELECTOR_H_
