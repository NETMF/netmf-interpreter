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

#ifndef _PLATFORM_MCBSTM32F400_SELECTOR_H_
#define _PLATFORM_MCBSTM32F400_SELECTOR_H_ 1

/////////////////////////////////////////////////////////
//
// processor and features
//

#if defined(PLATFORM_ARM_MCBSTM32F400)
#define HAL_SYSTEM_NAME "MCBSTM32F400"

#define PLATFORM_ARM_STM32F4 1 // STM32F407 cpu
#define STM32F4_ETH_PHY_RMII 1
#define USB_ALLOW_CONFIGURATION_OVERRIDE 1

//
// processor and features
//
/////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////
//
// constants
//

#define GPIO_PORTA 0
#define GPIO_PORTB 1
#define GPIO_PORTC 2
#define GPIO_PORTD 3
#define GPIO_PORTE 4
#define GPIO_PORTF 5
#define GPIO_PORTG 6
#define GPIO_PORTH 7
#define GPIO_PORTI 8

#define PORT_PIN(port,pin) ( ( (int)port) * 16 + ( pin ) )

// legal SYSCLK: 24, 30, 32, 36, 40, 42, 48, 54, 56, 60, 64, 72, 84, 96, 108, 120, 144, 168MHz
#define SYSTEM_CLOCK_HZ                 168000000  // 168MHz
#define SYSTEM_CYCLE_CLOCK_HZ           168000000  // 168MHz
#define SYSTEM_APB1_CLOCK_HZ            42000000   // 42MHz
#define SYSTEM_APB2_CLOCK_HZ            84000000   // 84MHz
#define SYSTEM_CRYSTAL_CLOCK_HZ         25000000   // 25MHz external clock
#define SUPPLY_VOLTAGE_MV               3300       // 3.3V supply

#define CLOCK_COMMON_FACTOR             1000000    // GCD(SYSTEM_CLOCK_HZ, 1M)

#define SLOW_CLOCKS_PER_SECOND          1000000    // 1MHz
#define SLOW_CLOCKS_TEN_MHZ_GCD         1000000    // GCD(SLOW_CLOCKS_PER_SECOND, 10M)
#define SLOW_CLOCKS_MILLISECOND_GCD     1000       // GCD(SLOW_CLOCKS_PER_SECOND, 1k)

#define FLASH_MEMORY_Base               0x08000000
#define FLASH_MEMORY_Size               0x00100000
#define SRAM1_MEMORY_Base               0x68000000
#define SRAM1_MEMORY_Size               0x00200000

#define TXPROTECTRESISTOR               RESISTOR_DISABLED
#define RXPROTECTRESISTOR               RESISTOR_DISABLED
#define CTSPROTECTRESISTOR              RESISTOR_DISABLED
#define RTSPROTECTRESISTOR              RESISTOR_DISABLED

#define TOTAL_GPIO_PORT                 9 // PA - PI
#define TOTAL_GPIO_PINS                 TOTAL_GPIO_PORT * 16
#define INSTRUMENTATION_H_GPIO_PIN      GPIO_PIN_NONE

#define TOTAL_USART_PORT                6 // 6 physical UARTS

#define USART_DEFAULT_PORT              COM1
#define USART_DEFAULT_BAUDRATE          115200

#define TOTAL_GENERIC_PORTS             1 // 1 generic port extensions (ITM channel 0 )
#define ITM_GENERIC_PORTNUM             0 // ITM0 is index 0 in generic port interface table

#define DEBUG_TEXT_PORT                 ITM0
#define STDIO                           USB1
#define DEBUGGER_PORT                   USB1
#define MESSAGING_PORT                  USB1

#define TOTAL_USB_CONTROLLER            1  // Silicon has 2, but only one supported in this port at this time...
#define USB_MAX_QUEUES                  4  // 4 endpoints (EP0 + 3)

// System Timer Configuration
#define STM32F4_32B_TIMER 2
#define STM32F4_16B_TIMER 3

// Pin Configuration
#define STM32F4_ADC 3
#define STM32F4_AD_CHANNELS {0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15}

#define STM32F4_PWM_TIMER {4,4,4,4}
#define STM32F4_PWM_CHNL  {0,1,2,3}
#define STM32F4_PWM_PINS  {60,61,62,63} // PD12-PD15

#define STM32F4_SPI_SCLK_PINS {5, 29, 42} // PA5, PB13, PC10
#define STM32F4_SPI_MISO_PINS {6, 30, 43} // PA6, PB14, PC11
#define STM32F4_SPI_MOSI_PINS {7, 31, 44} // PA7, PB15, PC12

#define STM32F4_I2C_PORT     1
#define STM32F4_I2C_SCL_PIN  PORT_PIN( GPIO_PORTB, 8 ) // PB8
#define STM32F4_I2C_SDA_PIN  PORT_PIN( GPIO_PORTB, 9 ) // PB9

#define STM32F4_UART_RXD_PINS {23, 54, 43} // PB7, D6, C11
#define STM32F4_UART_TXD_PINS {22, 53, 42} // PB6, D5, C10
#define STM32F4_UART_CTS_PINS {(BYTE)GPIO_PIN_NONE, 51, 59} // GPIO_PIN_NONE, D3, D11
#define STM32F4_UART_RTS_PINS {(BYTE)GPIO_PIN_NONE, 52, 60} // GPIO_PIN_NONE, D4, D12

#define STMPE811_INT_PIN ()

#define DRIVER_PAL_BUTTON_MAPPING                  \
        { 0, BUTTON_NONE }, /* Up */      \
        { 0, BUTTON_NONE }, /* Down */    \
        { 0, BUTTON_NONE }, /* Left */    \
        { 0, BUTTON_NONE }, /* Right */   \
        { 0, BUTTON_NONE }, /* Enter */   \
        { PORT_PIN( GPIO_PORTG, 15 ), BUTTON_B5 }, /* User */

#define LED1 PORT_PIN( GPIO_PORTG, 6 ) // PG.6
#define LED2 PORT_PIN( GPIO_PORTG, 7 ) // PG.7
#define LED3 PORT_PIN( GPIO_PORTG, 8 ) // PG.8

#define LED4 PORT_PIN( GPIO_PORTH, 2 ) // PH.2
#define LED5 PORT_PIN( GPIO_PORTH, 3 ) // PH.3
#define LED6 PORT_PIN( GPIO_PORTH, 6 ) // PH.6
#define LED7 PORT_PIN( GPIO_PORTH, 7 ) // PH.7

#define LED8 PORT_PIN( GPIO_PORTI, 10 ) // PI.10

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

#endif // PLATFORM_ARM_MCBSTM32F400
//
// drivers
/////////////////////////////////////////////////////////

#endif // _PLATFORM_MCBSTM32F400_SELECTOR_H_
