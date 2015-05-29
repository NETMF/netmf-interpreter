;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;; This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
;; Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
;; 
;; Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
;; You may obtain a copy of the License at:
;; 
;; http://www.apache.org/licenses/LICENSE-2.0
;; 
;; Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
;; WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
;; permissions and limitations under the License.
;; 
;; !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
;; NOTE:
;; This is only an example for use as a template in creating SoC specific versions and is not intended to be used directly
;; for any particular SoC. Each SoC must provide a version of these handlers (and the corresponding VectorTables) that is
;; specific to the SoC.
;; !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

    PRESERVE8
    THUMB

    ; Initial Stack pointer for power on reset
    IMPORT __initial_sp

    ; Import standard Cortex-M handlers
    IMPORT Reset_Handler
    IMPORT NMI_Handler
    IMPORT HardFault_Handler
    IMPORT MemManage_Handler
    IMPORT BusFault_Handler
    IMPORT UsageFault_Handler
    IMPORT SVC_Handler
    IMPORT DebugMon_Handler
    IMPORT PendSV_Handler
    IMPORT SysTick_Handler
    ; Import external interrupt handlers (SOC Specific)
    IMPORT  WWDG_IRQHandler
    IMPORT  PVD_IRQHandler
    IMPORT  TAMP_STAMP_IRQHandler
    IMPORT  RTC_WKUP_IRQHandler
    IMPORT  FLASH_IRQHandler
    IMPORT  RCC_IRQHandler
    IMPORT  EXTI0_IRQHandler
    IMPORT  EXTI1_IRQHandler
    IMPORT  EXTI2_IRQHandler
    IMPORT  EXTI3_IRQHandler
    IMPORT  EXTI4_IRQHandler
    IMPORT  DMA1_Stream0_IRQHandler
    IMPORT  DMA1_Stream1_IRQHandler
    IMPORT  DMA1_Stream2_IRQHandler
    IMPORT  DMA1_Stream3_IRQHandler
    IMPORT  DMA1_Stream4_IRQHandler
    IMPORT  DMA1_Stream5_IRQHandler
    IMPORT  DMA1_Stream6_IRQHandler
    IMPORT  ADC_IRQHandler
    IMPORT  CAN1_TX_IRQHandler
    IMPORT  CAN1_RX0_IRQHandler
    IMPORT  CAN1_RX1_IRQHandler
    IMPORT  CAN1_SCE_IRQHandler
    IMPORT  EXTI9_5_IRQHandler
    IMPORT  TIM1_BRK_TIM9_IRQHandler
    IMPORT  TIM1_UP_TIM10_IRQHandler
    IMPORT  TIM1_TRG_COM_TIM11_IRQHandler
    IMPORT  TIM1_CC_IRQHandler
    IMPORT  TIM2_IRQHandler
    IMPORT  TIM3_IRQHandler
    IMPORT  TIM4_IRQHandler
    IMPORT  I2C1_EV_IRQHandler
    IMPORT  I2C1_ER_IRQHandler
    IMPORT  I2C2_EV_IRQHandler
    IMPORT  I2C2_ER_IRQHandler
    IMPORT  SPI1_IRQHandler
    IMPORT  SPI2_IRQHandler
    IMPORT  USART1_IRQHandler
    IMPORT  USART2_IRQHandler
    IMPORT  USART3_IRQHandler
    IMPORT  EXTI15_10_IRQHandler
    IMPORT  RTC_Alarm_IRQHandler
    IMPORT  OTG_FS_WKUP_IRQHandler
    IMPORT  TIM8_BRK_TIM12_IRQHandler
    IMPORT  TIM8_UP_TIM13_IRQHandler
    IMPORT  TIM8_TRG_COM_TIM14_IRQHandler
    IMPORT  TIM8_CC_IRQHandler
    IMPORT  DMA1_Stream7_IRQHandler
    IMPORT  FMC_IRQHandler
    IMPORT  SDIO_IRQHandler
    IMPORT  TIM5_IRQHandler
    IMPORT  SPI3_IRQHandler
    IMPORT  UART4_IRQHandler
    IMPORT  UART5_IRQHandler
    IMPORT  TIM6_DAC_IRQHandler
    IMPORT  TIM7_IRQHandler
    IMPORT  DMA2_Stream0_IRQHandler
    IMPORT  DMA2_Stream1_IRQHandler
    IMPORT  DMA2_Stream2_IRQHandler
    IMPORT  DMA2_Stream3_IRQHandler
    IMPORT  DMA2_Stream4_IRQHandler
    IMPORT  ETH_IRQHandler
    IMPORT  ETH_WKUP_IRQHandler
    IMPORT  CAN2_TX_IRQHandler
    IMPORT  CAN2_RX0_IRQHandler
    IMPORT  CAN2_RX1_IRQHandler
    IMPORT  CAN2_SCE_IRQHandler
    IMPORT  OTG_FS_IRQHandler
    IMPORT  DMA2_Stream5_IRQHandler
    IMPORT  DMA2_Stream6_IRQHandler
    IMPORT  DMA2_Stream7_IRQHandler
    IMPORT  USART6_IRQHandler
    IMPORT  I2C3_EV_IRQHandler
    IMPORT  I2C3_ER_IRQHandler
    IMPORT  OTG_HS_EP1_OUT_IRQHandler
    IMPORT  OTG_HS_EP1_IN_IRQHandler
    IMPORT  OTG_HS_WKUP_IRQHandler
    IMPORT  OTG_HS_IRQHandler
    IMPORT  DCMI_IRQHandler
    IMPORT  HASH_RNG_IRQHandler
    IMPORT  FPU_IRQHandler

    EXPORT  ARM_Vectors

; Vector Table For the application
;
; bootloaders place this at offset 0 and the hardware uses
; it from there at power on reset. Applications (or the boot
;  loader itself) can place a copy in RAM to allow dynamically
; "hooking" interrupts at run-time
;
; It is expected ,though not required, that the imported handlers
; have a default empty implementation declared with WEAK linkage
; thus allowing applications to override the default by simply
; defining a function with the same name and proper behavior
; [ NOTE:
;   This standardized handler naming is an essential part of the
;   CMSIS-Core specification. It is relied upon by the CMSIS-RTX
;   implementation as well as much of the mbed framework.
; ]
    AREA    |VectorTable|, DATA, READWRITE, ALIGN=9

; The first 16 entries are all architecturally defined by ARM
ARM_Vectors
    DCD     __initial_sp                      ; Top of Stack
    DCD     Reset_Handler                     ; Reset Handler
    DCD     NMI_Handler                       ; NMI Handler
    DCD     HardFault_Handler                 ; Hard Fault Handler
    DCD     MemManage_Handler                 ; MPU Fault Handler
    DCD     BusFault_Handler                  ; Bus Fault Handler
    DCD     UsageFault_Handler                ; Usage Fault Handler
    DCD     0                                 ; Reserved
    DCD     0                                 ; Reserved
    DCD     0                                 ; Reserved
    DCD     0                                 ; Reserved
    DCD     SVC_Handler                       ; SVCall Handler
    DCD     DebugMon_Handler                  ; Debug Monitor Handler
    DCD     0                                 ; Reserved
    DCD     PendSV_Handler                    ; PendSV Handler
    DCD     SysTick_Handler                   ; SysTick Handler

; External Interrupts
; The remaining entries all SOC specific
; NOTE: Each SOC has a fixed bumber of interrrupt 
; sources so the actual number of entries is not architecturally
; defined for all systems (but there is a MAX defined)
    DCD     WWDG_IRQHandler                   ; Window WatchDog
    DCD     PVD_IRQHandler                    ; PVD through EXTI Line detection
    DCD     TAMP_STAMP_IRQHandler             ; Tamper and TimeStamps through the EXTI line
    DCD     RTC_WKUP_IRQHandler               ; RTC Wakeup through the EXTI line
    DCD     FLASH_IRQHandler                  ; FLASH
    DCD     RCC_IRQHandler                    ; RCC
    DCD     EXTI0_IRQHandler                  ; EXTI Line0
    DCD     EXTI1_IRQHandler                  ; EXTI Line1
    DCD     EXTI2_IRQHandler                  ; EXTI Line2
    DCD     EXTI3_IRQHandler                  ; EXTI Line3
    DCD     EXTI4_IRQHandler                  ; EXTI Line4
    DCD     DMA1_Stream0_IRQHandler           ; DMA1 Stream 0
    DCD     DMA1_Stream1_IRQHandler           ; DMA1 Stream 1
    DCD     DMA1_Stream2_IRQHandler           ; DMA1 Stream 2
    DCD     DMA1_Stream3_IRQHandler           ; DMA1 Stream 3
    DCD     DMA1_Stream4_IRQHandler           ; DMA1 Stream 4
    DCD     DMA1_Stream5_IRQHandler           ; DMA1 Stream 5
    DCD     DMA1_Stream6_IRQHandler           ; DMA1 Stream 6
    DCD     ADC_IRQHandler                    ; ADC1, ADC2 and ADC3s
    DCD     CAN1_TX_IRQHandler                ; CAN1 TX
    DCD     CAN1_RX0_IRQHandler               ; CAN1 RX0
    DCD     CAN1_RX1_IRQHandler               ; CAN1 RX1
    DCD     CAN1_SCE_IRQHandler               ; CAN1 SCE
    DCD     EXTI9_5_IRQHandler                ; External Line[9:5]s
    DCD     TIM1_BRK_TIM9_IRQHandler          ; TIM1 Break and TIM9
    DCD     TIM1_UP_TIM10_IRQHandler          ; TIM1 Update and TIM10
    DCD     TIM1_TRG_COM_TIM11_IRQHandler     ; TIM1 Trigger and Commutation and TIM11
    DCD     TIM1_CC_IRQHandler                ; TIM1 Capture Compare
    DCD     TIM2_IRQHandler                   ; TIM2
    DCD     TIM3_IRQHandler                   ; TIM3
    DCD     TIM4_IRQHandler                   ; TIM4
    DCD     I2C1_EV_IRQHandler                ; I2C1 Event
    DCD     I2C1_ER_IRQHandler                ; I2C1 Error
    DCD     I2C2_EV_IRQHandler                ; I2C2 Event
    DCD     I2C2_ER_IRQHandler                ; I2C2 Error
    DCD     SPI1_IRQHandler                   ; SPI1
    DCD     SPI2_IRQHandler                   ; SPI2
    DCD     USART1_IRQHandler                 ; USART1
    DCD     USART2_IRQHandler                 ; USART2
    DCD     USART3_IRQHandler                 ; USART3
    DCD     EXTI15_10_IRQHandler              ; External Line[15:10]s
    DCD     RTC_Alarm_IRQHandler              ; RTC Alarm (A and B) through EXTI Line
    DCD     OTG_FS_WKUP_IRQHandler            ; USB OTG FS Wakeup through EXTI line
    DCD     TIM8_BRK_TIM12_IRQHandler         ; TIM8 Break and TIM12
    DCD     TIM8_UP_TIM13_IRQHandler          ; TIM8 Update and TIM13
    DCD     TIM8_TRG_COM_TIM14_IRQHandler     ; TIM8 Trigger and Commutation and TIM14
    DCD     TIM8_CC_IRQHandler                ; TIM8 Capture Compare
    DCD     DMA1_Stream7_IRQHandler           ; DMA1 Stream7
    DCD     FMC_IRQHandler                    ; FMC
    DCD     SDIO_IRQHandler                   ; SDIO
    DCD     TIM5_IRQHandler                   ; TIM5
    DCD     SPI3_IRQHandler                   ; SPI3
    DCD     UART4_IRQHandler                  ; UART4
    DCD     UART5_IRQHandler                  ; UART5
    DCD     TIM6_DAC_IRQHandler               ; TIM6 and DAC1&2 underrun errors
    DCD     TIM7_IRQHandler                   ; TIM7
    DCD     DMA2_Stream0_IRQHandler           ; DMA2 Stream 0
    DCD     DMA2_Stream1_IRQHandler           ; DMA2 Stream 1
    DCD     DMA2_Stream2_IRQHandler           ; DMA2 Stream 2
    DCD     DMA2_Stream3_IRQHandler           ; DMA2 Stream 3
    DCD     DMA2_Stream4_IRQHandler           ; DMA2 Stream 4
    DCD     ETH_IRQHandler                    ; Ethernet
    DCD     ETH_WKUP_IRQHandler               ; Ethernet Wakeup through EXTI line
    DCD     CAN2_TX_IRQHandler                ; CAN2 TX
    DCD     CAN2_RX0_IRQHandler               ; CAN2 RX0
    DCD     CAN2_RX1_IRQHandler               ; CAN2 RX1
    DCD     CAN2_SCE_IRQHandler               ; CAN2 SCE
    DCD     OTG_FS_IRQHandler                 ; USB OTG FS
    DCD     DMA2_Stream5_IRQHandler           ; DMA2 Stream 5
    DCD     DMA2_Stream6_IRQHandler           ; DMA2 Stream 6
    DCD     DMA2_Stream7_IRQHandler           ; DMA2 Stream 7
    DCD     USART6_IRQHandler                 ; USART6
    DCD     I2C3_EV_IRQHandler                ; I2C3 event
    DCD     I2C3_ER_IRQHandler                ; I2C3 error
    DCD     OTG_HS_EP1_OUT_IRQHandler         ; USB OTG HS End Point 1 Out
    DCD     OTG_HS_EP1_IN_IRQHandler          ; USB OTG HS End Point 1 In
    DCD     OTG_HS_WKUP_IRQHandler            ; USB OTG HS Wakeup through EXTI
    DCD     OTG_HS_IRQHandler                 ; USB OTG HS
    DCD     DCMI_IRQHandler                   ; DCMI
    DCD     0                                 ; Reserved
    DCD     HASH_RNG_IRQHandler               ; Hash and Rng
    DCD     FPU_IRQHandler                    ; FPU

    END