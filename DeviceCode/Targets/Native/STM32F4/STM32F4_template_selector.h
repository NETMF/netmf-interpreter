////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32F4: Copyright (c) Oberon microsystems, Inc.
//
//  STM32F4 template solution descriptor
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _PLATFORM_<TEMPLATE>_SELECTOR_H_
#define _PLATFORM_<TEMPLATE>_SELECTOR_H_ 1

/////////////////////////////////////////////////////////
//
// processor and features
//

#if defined(PLATFORM_ARM_<TEMPLATE>)
#define HAL_SYSTEM_NAME                     "<TEMPLATE>"

#define PLATFORM_ARM_STM32F4 // STM32F407 cpu

//#define USB_ALLOW_CONFIGURATION_OVERRIDE  1


//
// processor and features
//
/////////////////////////////////////////////////////////


/////////////////////////////////////////////////////////
//
// constants
//

// legal SYSCLK: 24, 30, 32, 36, 40, 42, 48, 54, 56, 60, 64, 72, 84, 96, 108, 120, 144, 168MHz
#define SYSTEM_CLOCK_HZ                 168000000  // 168MHz
#define SYSTEM_CYCLE_CLOCK_HZ           168000000  // 168MHz
#define SYSTEM_APB1_CLOCK_HZ            42000000   // 42MHz
#define SYSTEM_APB2_CLOCK_HZ            84000000   // 84MHz
#define SYSTEM_CRYSTAL_CLOCK_HZ         8000000    // 8MHz external clock
#define SUPPLY_VOLTAGE_MV               3300       // 3.3V supply

#define CLOCK_COMMON_FACTOR             1000000    // GCD(SYSTEM_CLOCK_HZ, 1M)

#define SLOW_CLOCKS_PER_SECOND          1000000    // 1MHz
#define SLOW_CLOCKS_TEN_MHZ_GCD         1000000    // GCD(SLOW_CLOCKS_PER_SECOND, 10M)
#define SLOW_CLOCKS_MILLISECOND_GCD     1000       // GCD(SLOW_CLOCKS_PER_SECOND, 1k)

#define FLASH_MEMORY_Base               0x08000000
#define FLASH_MEMORY_Size               0x00100000
#define SRAM1_MEMORY_Base               0x20000000
#define SRAM1_MEMORY_Size               0x00020000

#define TXPROTECTRESISTOR               RESISTOR_DISABLED
#define RXPROTECTRESISTOR               RESISTOR_DISABLED
#define CTSPROTECTRESISTOR              RESISTOR_DISABLED
#define RTSPROTECTRESISTOR              RESISTOR_DISABLED

#define TOTAL_GPIO_PORT                 9 // PA - PI
#define INSTRUMENTATION_H_GPIO_PIN      GPIO_PIN_NONE

#define TOTAL_USART_PORT                6
#define USART_DEFAULT_PORT              COM3
#define USART_DEFAULT_BAUDRATE          115200

#if 1
    #define DEBUG_TEXT_PORT                 COM3
    #define STDIO                           COM3
    #define DEBUGGER_PORT                   COM3
    #define MESSAGING_PORT                  COM3
#else
    #define DEBUG_TEXT_PORT                 USB1
    #define STDIO                           USB1
    #define DEBUGGER_PORT                   USB1
    #define MESSAGING_PORT                  USB1
#endif

#define TOTAL_USB_CONTROLLER            1
#define USB_MAX_QUEUES                  4  // 4 endpoints (EP0 + 3)

#define TOTAL_SOCK_PORT                 0


// System Timer Configuration
#define STM32F4_32B_TIMER 2 // |   2   |   5    |
#define STM32F4_16B_TIMER 3 // |3,4,8,9|1,3,8,12|


// Pin Configuration

// AD CHANNELS (ADC1 or ADC3)
#define STM32F4_ADC 1
// channel:   0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17
// AD pins: PA0,PA1,PA2,PA3,PA4,PA5,PA6,PA7,PB0,PB1,PC0,PC1,PC2,PC3,PC4,PC5
//#define STM32F4_ADC 3
// channel:   0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17
// AD pins: PA0,PA1,PA2,PA3,PF6,PF7,PF8,PF9,F10,PF3,PC0,PC1,PC2,PC3,PF4,PF5
#define STM32F4_AD_CHANNELS {10,11,12,13} // PC0-PC3

// PWM CHANNELS
#define STM32F4_PWM_TIMER {1,1,1,1,4,4,4,4}
#define STM32F4_PWM_CHNL  {0,1,2,3,0,1,2,3}
#define STM32F4_PWM_PINS  {8,9,10,11,22,23,24,25} // PA8-PA11,PB6-PB9

// SPI CHANNELS (SPI1 - SPIn)
#define STM32F4_SPI_SCLK_PINS {19, 29, 42} // PB3, PB13, PC10
#define STM32F4_SPI_MISO_PINS {20, 30, 43} // PB4, PB14, PC11
#define STM32F4_SPI_MOSI_PINS {21, 31, 44} // PB5, PB15, PC12

// I2C PORT (1, 2, or 3)
#define STM32F4_I2C_PORT     1
#define STM32F4_I2C_SCL_PIN  22 // PB6
#define STM32F4_I2C_SDA_PIN  23 // PB7

// UART PORTS (UART1 - UARTn)
#define STM32F4_UART_RXD_PINS {10, 54, 57, 43, 50, 39} // A10, D6, D9, C11, D2, C7
#define STM32F4_UART_TXD_PINS { 9, 53, 56, 42, 44, 38} // A9, D5, D8, C10, C12, C6
#define STM32F4_UART_CTS_PINS {11, 51, 59} // A11, D3, D11
#define STM32F4_UART_RTS_PINS {12, 52, 60} // A12, D4, D12

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

#endif // _PLATFORM_<TEMPLATE>_SELECTOR_H_
