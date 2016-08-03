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

#ifndef _PLATFORM_STM32F429IDISCOVERY_SELECTOR_H_
#define _PLATFORM_STM32F429IDISCOVERY_SELECTOR_H_

/////////////////////////////////////////////////////////
//
// processor and features
//

#if defined(PLATFORM_ARM_STM32F429IDISCOVERY)

#define HAL_SYSTEM_NAME "STM32F429IDISCOVERY"

#define PLATFORM_ARM_STM32F4 1

#define USB_ALLOW_CONFIGURATION_OVERRIDE 1

#define STM32F429_439xx

//
// processor and features
//
/////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////
//
// constants
//

// System clock
#define SYSTEM_CLOCK_HZ                 168000000   // 168 MHz
#define SYSTEM_CYCLE_CLOCK_HZ           168000000   // 168 MHz
#define SYSTEM_APB1_CLOCK_HZ             42000000   //  42 MHz
#define SYSTEM_APB2_CLOCK_HZ             84000000   //  84 MHz

#define SYSTEM_CRYSTAL_CLOCK_HZ           8000000   // 8 MHz external clock

#define SUPPLY_VOLTAGE_MV                    3000   // 3.0V supply

#define CLOCK_COMMON_FACTOR               1000000   // GCD(SYSTEM_CLOCK_HZ, 1M)

#define SLOW_CLOCKS_PER_SECOND            1000000   // 1 MHz
#define SLOW_CLOCKS_TEN_MHZ_GCD           1000000   // GCD(SLOW_CLOCKS_PER_SECOND, 10M)
#define SLOW_CLOCKS_MILLISECOND_GCD          1000   // GCD(SLOW_CLOCKS_PER_SECOND, 1k)

// Memory
#define FLASH_MEMORY_Base               0x08000000
#define FLASH_MEMORY_Size               0x00200000  // 2 MB
#define SRAM1_MEMORY_Base               0x20000000
#define SRAM1_MEMORY_Size               0x00030000  // 192 KB

// System Timer Configuration
#define STM32F4_32B_TIMER               2 // or 5
#define STM32F4_16B_TIMER               3 // or 1

// Peripherals and Pin Configuration

#define TOTAL_GENERIC_PORTS             1 // 1 generic port extensions (ITM channel 0)
#define ITM_GENERIC_PORTNUM             0 // ITM0 is index 0 in generic port interface table

#define TOTAL_USB_CONTROLLER            1
#define STM32F4_USB_HS                  1  // USB device on USB HS (PB12-15)
#define USB_MAX_QUEUES                  4  // 4 endpoints (EP0 + 3)

#define DEBUG_TEXT_PORT                 ITM0
#define STDIO                           ITM0
#define DEBUGGER_PORT                   USB1
#define MESSAGING_PORT                  USB1

// General I/O
#define GPIO_PORTA  0
#define GPIO_PORTB  1
#define GPIO_PORTC  2
#define GPIO_PORTD  3
#define GPIO_PORTE  4
#define GPIO_PORTF  5
#define GPIO_PORTG  6
// Ports H - K are not supported: pins PH0, PH1 were deliberately omitted,
// ports I, J and K are not available (broken out) on the board connectors.

#define TOTAL_GPIO_PORT                 (GPIO_PORTG + 1)
#define TOTAL_GPIO_PINS                 (TOTAL_GPIO_PORT*16)

#define INSTRUMENTATION_H_GPIO_PIN      GPIO_PIN_NONE

#define PORT_PIN(port,pin)  (((int)port)*16 + (pin))
#define _P(port, pin) PORT_PIN(GPIO_PORT##port, pin)
#define _P_NONE_ GPION_PIN_NONE

// USART/UART
#define TOTAL_USART_PORT                4 // of 8 (4x USART + 4x UART)
//                                         USART1    USART2    USART3    UART4
#define STM32F4_UART_RXD_PINS           { _P(A,10), _P(D, 6), _P(B,11), _P(C,11) }
#define STM32F4_UART_TXD_PINS           { _P(A, 9), _P(D, 5), _P(B,10), _P(C,10) }

#define USART_DEFAULT_PORT              COM1
#define USART_DEFAULT_BAUDRATE          115200

// I2C (STMPE811 touchscreen controller on I2C3)
#define STM32F4_I2C_PORT                3
#define STM32F4_I2C_SCL_PIN             _P(A, 8)
#define STM32F4_I2C_SDA_PIN             _P(C, 9)

// SPI (LCD-SPI and L3GD20 gyroscope on SPI5)
//                                         SPI1      SPI2     SPI3      SPI4      SPI5      SPI6
#define STM32F4_SPI_SCLK_PINS           { _P(A, 5), _P_NONE_,_P_NONE_, _P_NONE_, _P(F, 7), _P_NONE_ }
#define STM32F4_SPI_MISO_PINS           { _P(A, 6), _P_NONE_,_P_NONE_, _P_NONE_, _P(F, 8), _P_NONE_ }
#define STM32F4_SPI_MOSI_PINS           { _P(A, 7), _P_NONE_,_P_NONE_, _P_NONE_, _P(F, 9), _P_NONE_ }
//
// Note: ACP/RF is not supported due to conflict with SPI1_MOSI on PA7.
//

// User & Wake-up Button
#define USER_BUTTON                     _P(A, 0)  // Blue

// User LEDs
#define LED3                            _P(G, 13) // Green
#define LED4                            _P(G, 14) // Red


// TinyBooter entry using GPIO
#define TINYBOOTER_ENTRY_GPIO_PIN       USER_BUTTON
#define TINYBOOTER_ENTRY_GPIO_STATE     TRUE                // Active high
#define TINYBOOTER_ENTRY_GPIO_RESISTOR  RESISTOR_DISABLED   // No internal resistor, there is external pull-down (R22)

//
// constants
/////////////////////////////////////////////////////////

#include <processor_selector.h>

#endif // PLATFORM_ARM_STM32F429IDISCOVERY

#endif // _PLATFORM_STM32F429IDISCOVERY_SELECTOR_H_
