/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
// Based on the Implementation for (STM32F4) by Oberon microsystems, Inc.
//
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _PLATFORM_STM32F4DISCOVERY_SELECTOR_H_
#define _PLATFORM_STM32F4DISCOVERY_SELECTOR_H_

/////////////////////////////////////////////////////////
//
// processor and features
//

#if defined(PLATFORM_ARM_STM32F4DISCOVERY)
#define HAL_SYSTEM_NAME "STM32F4DISCOVERY"

// FIXME
// the following can be removed after complete migration to CMSIS HAL/PAL 
#define STM32F4XX 1
#define PLATFORM_ARM_STM32F4 1 // STM32F407 cpu
#define USB_ALLOW_CONFIGURATION_OVERRIDE 1

//
// processor and features
//
/////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////
//
// constants
//

#define GPIO_PINS {GPIO_PIN_0,GPIO_PIN_1,GPIO_PIN_2,GPIO_PIN_3,GPIO_PIN_4,GPIO_PIN_5,GPIO_PIN_6,GPIO_PIN_7,GPIO_PIN_8,GPIO_PIN_9,GPIO_PIN_10,GPIO_PIN_11,GPIO_PIN_12,GPIO_PIN_13,GPIO_PIN_14,GPIO_PIN_15}

// System clock
#define SYSTEM_CLOCK_HZ                 168000000   // 168 MHz
#define SYSTEM_CYCLE_CLOCK_HZ           168000000   // 168 MHz
#define SYSTEM_APB1_CLOCK_HZ             42000000   //  42 MHz
#define SYSTEM_APB2_CLOCK_HZ             84000000   //  84 MHz

#define SLOW_CLOCKS_PER_SECOND            1000000   // 1 MHz

#define FLASH_MEMORY_Base               0x08000000
#define FLASH_MEMORY_Size               0x00100000
#define SRAM1_MEMORY_Base               0x20000000
#define SRAM1_MEMORY_Size               0x00020000

#define TXPROTECTRESISTOR               RESISTOR_DISABLED
#define RXPROTECTRESISTOR               RESISTOR_DISABLED
#define CTSPROTECTRESISTOR              RESISTOR_DISABLED
#define RTSPROTECTRESISTOR              RESISTOR_DISABLED

#define TOTAL_GPIO_PORT                 (GPIO_PORTE + 1)
#define TOTAL_GPIO_PINS                 (TOTAL_GPIO_PORT*16)

#define INSTRUMENTATION_H_GPIO_PIN      GPIO_PIN_NONE

#define TOTAL_USART_PORT                6 // 6 physical UARTS

#define USART_DEFAULT_PORT              COM1
#define USART_DEFAULT_BAUDRATE          115200

#define TOTAL_GENERIC_PORTS             1 // 1 generic port extensions (ITM channel 0 )
#define ITM_GENERIC_PORTNUM             0 // ITM0 is index 0 in generic port interface table

#define DEBUG_TEXT_PORT                 ITM0
#define STDIO                           ITM0
#define DEBUGGER_PORT                   USB1
#define MESSAGING_PORT                  USB1

#define TOTAL_USB_CONTROLLER            1  // Silicon has 2, but only one supported in this port at this time...
#define USB_MAX_QUEUES                  4  // 4 endpoints (EP0 + 3)

// System Timer Configuration
// #define HAL_32B_TIMER 2
// #define HAL_16B_TIMER 3

/* Choose if wake-up from STOP mode is in fast mode or not.
This selects which regulator is on during stop mode.
By keeping the internal regulator ON during Stop mode, the consumption 
is higher although the startup time is reduced.  
When the voltage regulator operates in low power mode, an additional 
startup delay is incurred when waking up from Stop mode. */
//#define FAST_WAKEUP_FROM_STOP

/* GPIOs */
// PA.0  'User' button
// PD.13 LED3 (orange)
// PD.12 LED4 (green)
// PD.14 LED5 (red)
// PD.15 LED6 (blue)
    
#define GPIO_PORT_PINS \
{\
    {GPIOA, GPIO_PIN_0},\
    {GPIOD, GPIO_PIN_13},\
    {GPIOD, GPIO_PIN_12},\
    {GPIOD, GPIO_PIN_14},\
    {GPIOD, GPIO_PIN_15},\
}

/* ADC */
// Pin Configuration
#define USER_ADC 3
// choose a maximum of 8 channels and a minimum of one
// ** IMPORTANT ** internal channels 16 and 17 are not available in ADC3
#define AD_CHANNELS {0,1,2,3,4,5,6,7}//,8,9,10,11,12,13,14,15,16,17}

/* DAC */
// Uncomment only one of the following defines for DAC
//#define USE_DAC_CHANNEL_1
//#define USE_DAC_CHANNEL_2
#define USE_BOTH_DAC_CHANNEL

#define PWM_TIMER {4,4,4,4}
#define PWM_CHNL  {0,1,2,3}
#define PWM_PINS  {60,61,62,63} // PD12-PD15

/* SPI */
// add to SPI_PORTS array the SPI ports that are available in the solution 
// please check which SPI ports are available in the device datasheet
// uncomment the following define and leave the SPI ports that are available in your hardware
#define SPI_PORTS   {SPI1, SPI2, SPI3}

/* USART/UART */
// add to USART_PORTS array the USART ports that are available in the solution 
// please check which USART ports are available in the device datasheet
// uncomment the following define and leave the USART ports that are available in your hardware and you intend to use
// NOTE: UART5 is not supported because TX and RX are handled in different ports
#define USART_PORTS                 {USART1, USART2, USART6}
#define USART_PORTS_TX_RX           {GPIOA, GPIOD, GPIOC}
// to use flow control, uncomment below define and leave the USART ports that are available in your hardware and you intend to use
#define GPIO_NO_PORT                0
#define USART_PORTS_FLOW_CONTROL    {GPIO_NO_PORT, GPIO_NO_PORT, GPIO_NO_PORT}

#define STM32F4_UART_RXD_PINS {23, 54, 43} // PB7, D6, C11
#define STM32F4_UART_TXD_PINS {22, 53, 42} // PB6, D5, C10
#define STM32F4_UART_CTS_PINS {(BYTE)GPIO_PIN_NONE, 51, 59} // GPIO_PIN_NONE, D3, D11
#define STM32F4_UART_RTS_PINS {(BYTE)GPIO_PIN_NONE, 52, 60} // GPIO_PIN_NONE, D4, D12

// User LEDs are defined as GPIO ports above in GPIO_PORT_PINS @ the following indexes
#define LED3 1  // (orange)
#define LED4 2  // (green)
#define LED5 3  // (red)
#define LED6 4  // (blue)

// TinyBooter entry using GPIO
#define TINYBOOTER_ENTRY_GPIO_PIN       0                       // 'User' button being GPIO port @ index 0 above in GPIO_PORT_PINS
#define TINYBOOTER_ENTRY_GPIO_STATE     TRUE                    // Active high
#define TINYBOOTER_ENTRY_GPIO_RESISTOR  RESISTOR_DISABLED       // No internal resistor, there is external pull-down (R39)

//
// constants
/////////////////////////////////////////////////////////

#include "processor_selector.h"

#endif // PLATFORM_ARM_STM32F4DISCOVERY

#endif // _PLATFORM_STM32F4DISCOVERY_SELECTOR_H_
