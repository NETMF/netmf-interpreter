@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@ This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
@@ Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
@@ 
@@ Licensed under the Apache License, Version 2.0 (the "License")@ you may not use these files except in compliance with the License.
@@ You may obtain a copy of the License at:
@@ 
@@ http://www.apache.org/licenses/LICENSE-2.0
@@ 
@@ Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
@@ WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
@@ permissions and limitations under the License.
@@
@@ !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
@@ NOTE:
@@ This is only an example for use as a template in creating SoC specific versions and is not intended to be used directly
@@ for any particular SoC. Each SoC must provide a version of these handlers (and the corresponding VectorTables) that is
@@ specific to the SoC.
@@ !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

    .syntax unified
    .arch armv7-m
    .thumb

    @ Initial Stack pointer for power on reset
    .extern __initial_sp

    @ Import standard Cortex-M handlers
    .extern Reset_Handler
    .extern NMI_Handler
    .extern HardFault_Handler
    .extern MemManage_Handler
    .extern BusFault_Handler
    .extern UsageFault_Handler
    .extern SVC_Handler
    .extern DebugMon_Handler
    .extern PendSV_Handler
    .extern SysTick_Handler

    @ Import external interrupt handlers (SOC Specific)
    .extern  WWDG_IRQHandler
    .extern  PVD_IRQHandler
    .extern  TAMP_STAMP_IRQHandler
    .extern  RTC_WKUP_IRQHandler
    .extern  FLASH_IRQHandler
    .extern  RCC_IRQHandler
    .extern  EXTI0_IRQHandler
    .extern  EXTI1_IRQHandler
    .extern  EXTI2_IRQHandler
    .extern  EXTI3_IRQHandler
    .extern  EXTI4_IRQHandler
    .extern  DMA1_Stream0_IRQHandler
    .extern  DMA1_Stream1_IRQHandler
    .extern  DMA1_Stream2_IRQHandler
    .extern  DMA1_Stream3_IRQHandler
    .extern  DMA1_Stream4_IRQHandler
    .extern  DMA1_Stream5_IRQHandler
    .extern  DMA1_Stream6_IRQHandler
    .extern  ADC_IRQHandler
    .extern  CAN1_TX_IRQHandler
    .extern  CAN1_RX0_IRQHandler
    .extern  CAN1_RX1_IRQHandler
    .extern  CAN1_SCE_IRQHandler
    .extern  EXTI9_5_IRQHandler
    .extern  TIM1_BRK_TIM9_IRQHandler
    .extern  TIM1_UP_TIM10_IRQHandler
    .extern  TIM1_TRG_COM_TIM11_IRQHandler
    .extern  TIM1_CC_IRQHandler
    .extern  TIM2_IRQHandler
    .extern  TIM3_IRQHandler
    .extern  TIM4_IRQHandler
    .extern  I2C1_EV_IRQHandler
    .extern  I2C1_ER_IRQHandler
    .extern  I2C2_EV_IRQHandler
    .extern  I2C2_ER_IRQHandler
    .extern  SPI1_IRQHandler
    .extern  SPI2_IRQHandler
    .extern  USART1_IRQHandler
    .extern  USART2_IRQHandler
    .extern  USART3_IRQHandler
    .extern  EXTI15_10_IRQHandler
    .extern  RTC_Alarm_IRQHandler
    .extern  OTG_FS_WKUP_IRQHandler
    .extern  TIM8_BRK_TIM12_IRQHandler
    .extern  TIM8_UP_TIM13_IRQHandler
    .extern  TIM8_TRG_COM_TIM14_IRQHandler
    .extern  TIM8_CC_IRQHandler
    .extern  DMA1_Stream7_IRQHandler
    .extern  FMC_IRQHandler
    .extern  SDIO_IRQHandler
    .extern  TIM5_IRQHandler
    .extern  SPI3_IRQHandler
    .extern  UART4_IRQHandler
    .extern  UART5_IRQHandler
    .extern  TIM6_DAC_IRQHandler
    .extern  TIM7_IRQHandler
    .extern  DMA2_Stream0_IRQHandler
    .extern  DMA2_Stream1_IRQHandler
    .extern  DMA2_Stream2_IRQHandler
    .extern  DMA2_Stream3_IRQHandler
    .extern  DMA2_Stream4_IRQHandler
    .extern  ETH_IRQHandler
    .extern  ETH_WKUP_IRQHandler
    .extern  CAN2_TX_IRQHandler
    .extern  CAN2_RX0_IRQHandler
    .extern  CAN2_RX1_IRQHandler
    .extern  CAN2_SCE_IRQHandler
    .extern  OTG_FS_IRQHandler
    .extern  DMA2_Stream5_IRQHandler
    .extern  DMA2_Stream6_IRQHandler
    .extern  DMA2_Stream7_IRQHandler
    .extern  USART6_IRQHandler
    .extern  I2C3_EV_IRQHandler
    .extern  I2C3_ER_IRQHandler
    .extern  OTG_HS_EP1_OUT_IRQHandler
    .extern  OTG_HS_EP1_IN_IRQHandler
    .extern  OTG_HS_WKUP_IRQHandler
    .extern  OTG_HS_IRQHandler
    .extern  DCMI_IRQHandler
    .extern  HASH_RNG_IRQHandler
    .extern  FPU_IRQHandler

    .global  ARM_Vectors

@ Vector Table For the application
@
@ bootloaders place this at offset 0 and the hardware uses
@ it from there at power on reset. Applications (or the boot
@  loader itself) can place a copy in RAM to allow dynamically
@ "hooking" interrupts at run-time
@
@ It is expected ,though not required, that the .externed handlers
@ have a default empty implementation declared with WEAK linkage
@ thus allowing applications to override the default by simply
@ defining a function with the same name and proper behavior
@ [ NOTE:
@   This standardized handler naming is an essential part of the
@   CMSIS-Core specification. It is relied upon by the CMSIS-RTX
@   implementation as well as much of the mbed framework.
@ ]
    .section VectorTable
    .align 9

; The first 16 entries are all architecturally defined by ARM
ARM_Vectors:
    .long     __initial_sp                      @ Top of Stack
    .long     Reset_Handler                     @ Reset Handler
    .long     NMI_Handler                       @ NMI Handler
    .long     HardFault_Handler                 @ Hard Fault Handler
    .long     MemManage_Handler                 @ MPU Fault Handler
    .long     BusFault_Handler                  @ Bus Fault Handler
    .long     UsageFault_Handler                @ Usage Fault Handler
    .long     0                                 @ Reserved
    .long     0                                 @ Reserved
    .long     0                                 @ Reserved
    .long     0                                 @ Reserved
    .long     SVC_Handler                       @ SVCall Handler
    .long     DebugMon_Handler                  @ Debug Monitor Handler
    .long     0                                 @ Reserved
    .long     PendSV_Handler                    @ PendSV Handler
    .long     SysTick_Handler                   @ SysTick Handler

@ External Interrupts
@ The remaining entries all SOC specific
@ NOTE: Each SOC has a fixed bumber of interrrupt 
@ sources so the actual number of entries is not architecturally
@ defined for all systems (but there is a MAX defined)
    .long     WWDG_IRQHandler                   @ Window WatchDog
    .long     PVD_IRQHandler                    @ PVD through EXTI Line detection
    .long     TAMP_STAMP_IRQHandler             @ Tamper and TimeStamps through the EXTI line
    .long     RTC_WKUP_IRQHandler               @ RTC Wakeup through the EXTI line
    .long     FLASH_IRQHandler                  @ FLASH
    .long     RCC_IRQHandler                    @ RCC
    .long     EXTI0_IRQHandler                  @ EXTI Line0
    .long     EXTI1_IRQHandler                  @ EXTI Line1
    .long     EXTI2_IRQHandler                  @ EXTI Line2
    .long     EXTI3_IRQHandler                  @ EXTI Line3
    .long     EXTI4_IRQHandler                  @ EXTI Line4
    .long     DMA1_Stream0_IRQHandler           @ DMA1 Stream 0
    .long     DMA1_Stream1_IRQHandler           @ DMA1 Stream 1
    .long     DMA1_Stream2_IRQHandler           @ DMA1 Stream 2
    .long     DMA1_Stream3_IRQHandler           @ DMA1 Stream 3
    .long     DMA1_Stream4_IRQHandler           @ DMA1 Stream 4
    .long     DMA1_Stream5_IRQHandler           @ DMA1 Stream 5
    .long     DMA1_Stream6_IRQHandler           @ DMA1 Stream 6
    .long     ADC_IRQHandler                    @ ADC1, ADC2 and ADC3s
    .long     CAN1_TX_IRQHandler                @ CAN1 TX
    .long     CAN1_RX0_IRQHandler               @ CAN1 RX0
    .long     CAN1_RX1_IRQHandler               @ CAN1 RX1
    .long     CAN1_SCE_IRQHandler               @ CAN1 SCE
    .long     EXTI9_5_IRQHandler                @ External Line[9:5]s
    .long     TIM1_BRK_TIM9_IRQHandler          @ TIM1 Break and TIM9
    .long     TIM1_UP_TIM10_IRQHandler          @ TIM1 Update and TIM10
    .long     TIM1_TRG_COM_TIM11_IRQHandler     @ TIM1 Trigger and Commutation and TIM11
    .long     TIM1_CC_IRQHandler                @ TIM1 Capture Compare
    .long     TIM2_IRQHandler                   @ TIM2
    .long     TIM3_IRQHandler                   @ TIM3
    .long     TIM4_IRQHandler                   @ TIM4
    .long     I2C1_EV_IRQHandler                @ I2C1 Event
    .long     I2C1_ER_IRQHandler                @ I2C1 Error
    .long     I2C2_EV_IRQHandler                @ I2C2 Event
    .long     I2C2_ER_IRQHandler                @ I2C2 Error
    .long     SPI1_IRQHandler                   @ SPI1
    .long     SPI2_IRQHandler                   @ SPI2
    .long     USART1_IRQHandler                 @ USART1
    .long     USART2_IRQHandler                 @ USART2
    .long     USART3_IRQHandler                 @ USART3
    .long     EXTI15_10_IRQHandler              @ External Line[15:10]s
    .long     RTC_Alarm_IRQHandler              @ RTC Alarm (A and B) through EXTI Line
    .long     OTG_FS_WKUP_IRQHandler            @ USB OTG FS Wakeup through EXTI line
    .long     TIM8_BRK_TIM12_IRQHandler         @ TIM8 Break and TIM12
    .long     TIM8_UP_TIM13_IRQHandler          @ TIM8 Update and TIM13
    .long     TIM8_TRG_COM_TIM14_IRQHandler     @ TIM8 Trigger and Commutation and TIM14
    .long     TIM8_CC_IRQHandler                @ TIM8 Capture Compare
    .long     DMA1_Stream7_IRQHandler           @ DMA1 Stream7
    .long     FMC_IRQHandler                    @ FMC
    .long     SDIO_IRQHandler                   @ SDIO
    .long     TIM5_IRQHandler                   @ TIM5
    .long     SPI3_IRQHandler                   @ SPI3
    .long     UART4_IRQHandler                  @ UART4
    .long     UART5_IRQHandler                  @ UART5
    .long     TIM6_DAC_IRQHandler               @ TIM6 and DAC1&2 underrun errors
    .long     TIM7_IRQHandler                   @ TIM7
    .long     DMA2_Stream0_IRQHandler           @ DMA2 Stream 0
    .long     DMA2_Stream1_IRQHandler           @ DMA2 Stream 1
    .long     DMA2_Stream2_IRQHandler           @ DMA2 Stream 2
    .long     DMA2_Stream3_IRQHandler           @ DMA2 Stream 3
    .long     DMA2_Stream4_IRQHandler           @ DMA2 Stream 4
    .long     ETH_IRQHandler                    @ Ethernet
    .long     ETH_WKUP_IRQHandler               @ Ethernet Wakeup through EXTI line
    .long     CAN2_TX_IRQHandler                @ CAN2 TX
    .long     CAN2_RX0_IRQHandler               @ CAN2 RX0
    .long     CAN2_RX1_IRQHandler               @ CAN2 RX1
    .long     CAN2_SCE_IRQHandler               @ CAN2 SCE
    .long     OTG_FS_IRQHandler                 @ USB OTG FS
    .long     DMA2_Stream5_IRQHandler           @ DMA2 Stream 5
    .long     DMA2_Stream6_IRQHandler           @ DMA2 Stream 6
    .long     DMA2_Stream7_IRQHandler           @ DMA2 Stream 7
    .long     USART6_IRQHandler                 @ USART6
    .long     I2C3_EV_IRQHandler                @ I2C3 event
    .long     I2C3_ER_IRQHandler                @ I2C3 error
    .long     OTG_HS_EP1_OUT_IRQHandler         @ USB OTG HS End Point 1 Out
    .long     OTG_HS_EP1_IN_IRQHandler          @ USB OTG HS End Point 1 In
    .long     OTG_HS_WKUP_IRQHandler            @ USB OTG HS Wakeup through EXTI
    .long     OTG_HS_IRQHandler                 @ USB OTG HS
    .long     DCMI_IRQHandler                   @ DCMI
    .long     0                                 @ Reserved
    .long     HASH_RNG_IRQHandler               @ Hash and Rng
    .long     FPU_IRQHandler                    @ FPU
.end
