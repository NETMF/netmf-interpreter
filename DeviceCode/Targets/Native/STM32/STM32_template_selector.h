////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32: Copyright (c) Oberon microsystems, Inc.
//
//  STM32 template solution descriptor
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _PLATFORM_<TEMPLATE>_SELECTOR_H_
#define _PLATFORM_<TEMPLATE>_SELECTOR_H_ 1

/////////////////////////////////////////////////////////
//
// processor and features
//

#if defined(PLATFORM_ARM_<TEMPLATE>)
#define HAL_SYSTEM_NAME             "<TEMPLATE>"

#define STM32F10X_HD  // STM32F103RET cpu
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

#define SYSTEM_CLOCK_HZ                 72000000  // max 72MHz
#define SYSTEM_CYCLE_CLOCK_HZ           72000000  // max 72MHz
#define SYSTEM_APB1_CLOCK_HZ            36000000  // max 36MHz
#define SYSTEM_APB2_CLOCK_HZ            72000000  // max 72MHz
#define SYSTEM_CRYSTAL_CLOCK_HZ         8000000   // 8MHz external clock

#define CLOCK_COMMON_FACTOR             1000000   // GCD(SYSTEM_CLOCK_HZ, 1M)

#define SLOW_CLOCKS_PER_SECOND          1000000   // 1MHz
#define SLOW_CLOCKS_TEN_MHZ_GCD         1000000   // GCD(SLOW_CLOCKS_PER_SECOND, 10M)
#define SLOW_CLOCKS_MILLISECOND_GCD     1000      // GCD(SLOW_CLOCKS_PER_SECOND, 1k)

#define FLASH_MEMORY_Base               0x08000000
#define FLASH_MEMORY_Size               0x00080000
#define SRAM1_MEMORY_Base               0x20000000
#define SRAM1_MEMORY_Size               0x00010000

#define TXPROTECTRESISTOR               RESISTOR_DISABLED
#define RXPROTECTRESISTOR               RESISTOR_DISABLED
#define CTSPROTECTRESISTOR              RESISTOR_DISABLED
#define RTSPROTECTRESISTOR              RESISTOR_DISABLED

#define INSTRUMENTATION_H_GPIO_PIN      GPIO_PIN_NONE

#define TOTAL_USART_PORT                3
#define USART_DEFAULT_PORT              COM1
#define USART_DEFAULT_BAUDRATE          115200

#if 1
    #define DEBUG_TEXT_PORT                 COM1
    #define STDIO                           COM1
    #define DEBUGGER_PORT                   COM1
    #define MESSAGING_PORT                  COM1
#else
    #define DEBUG_TEXT_PORT                 USB1
    #define STDIO                           USB1
    #define DEBUGGER_PORT                   USB1
    #define MESSAGING_PORT                  USB1
#endif

//#define STM32_USB_Attach_Pin_High       0 // active high USB attach pin
//#define STM32_USB_Attach_Pin_Low        0 // active low USB attach pin
//#define STM32_USB_Attach_Pin_Direct     0 // direct USB pull-up connection


#define DRIVER_PAL_BUTTON_MAPPING               \
    { GPIO_PIN_NONE, BUTTON_B0 }, /* Up */      \
    { GPIO_PIN_NONE, BUTTON_B1 }, /* Down */    \
    { GPIO_PIN_NONE, BUTTON_B2 }, /* Left */    \
    { GPIO_PIN_NONE, BUTTON_B3 }, /* Right */   \
    { GPIO_PIN_NONE, BUTTON_B4 }, /* Enter */   \
    { GPIO_PIN_NONE, BUTTON_B5 }, /* User */

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

#endif // PLATFORM_ARM_<TEMPLATE>
//
// drivers
/////////////////////////////////////////////////////////

#endif // _PLATFORM_<TEMPLATE>_SELECTOR_H_ 1
