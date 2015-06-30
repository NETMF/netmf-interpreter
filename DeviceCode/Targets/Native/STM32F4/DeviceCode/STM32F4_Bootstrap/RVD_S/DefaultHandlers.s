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

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;                
    AREA    |SectionForBootstrapOperations|, CODE, READONLY

; Dummy Exception Handlers (infinite loops which can be modified)

NMI_Handler     PROC
                EXPORT  NMI_Handler                [WEAK]
                B       .
                ENDP
HardFault_Handler\
                PROC
                EXPORT  HardFault_Handler          [WEAK]
                B       .
                ENDP
MemManage_Handler\
                PROC
                EXPORT  MemManage_Handler          [WEAK]
                B       .
                ENDP
BusFault_Handler\
                PROC
                EXPORT  BusFault_Handler           [WEAK]
                B       .
                ENDP
UsageFault_Handler\
                PROC
                EXPORT  UsageFault_Handler         [WEAK]
                B       .
                ENDP
SVC_Handler     PROC
                EXPORT  SVC_Handler                [WEAK]
                B       .
                ENDP
DebugMon_Handler\
                PROC
                EXPORT  DebugMon_Handler           [WEAK]
                B       .
                ENDP
PendSV_Handler  PROC
                EXPORT  PendSV_Handler             [WEAK]
                B       .
                ENDP
SysTick_Handler PROC
                EXPORT  SysTick_Handler            [WEAK]
                B       .
                ENDP

Default_Handler PROC
                EXPORT  Default_Handler                   [WEAK]
                EXPORT  WWDG_IRQHandler                   [WEAK]
                EXPORT  PVD_IRQHandler                    [WEAK]
                EXPORT  TAMP_STAMP_IRQHandler             [WEAK]
                EXPORT  RTC_WKUP_IRQHandler               [WEAK]
                EXPORT  FLASH_IRQHandler                  [WEAK]
                EXPORT  RCC_IRQHandler                    [WEAK]
                EXPORT  EXTI0_IRQHandler                  [WEAK]
                EXPORT  EXTI1_IRQHandler                  [WEAK]
                EXPORT  EXTI2_IRQHandler                  [WEAK]
                EXPORT  EXTI3_IRQHandler                  [WEAK]
                EXPORT  EXTI4_IRQHandler                  [WEAK]
                EXPORT  DMA1_Stream0_IRQHandler           [WEAK]
                EXPORT  DMA1_Stream1_IRQHandler           [WEAK]
                EXPORT  DMA1_Stream2_IRQHandler           [WEAK]
                EXPORT  DMA1_Stream3_IRQHandler           [WEAK]
                EXPORT  DMA1_Stream4_IRQHandler           [WEAK]
                EXPORT  DMA1_Stream5_IRQHandler           [WEAK]
                EXPORT  DMA1_Stream6_IRQHandler           [WEAK]
                EXPORT  ADC_IRQHandler                    [WEAK]
                EXPORT  CAN1_TX_IRQHandler                [WEAK]
                EXPORT  CAN1_RX0_IRQHandler               [WEAK]
                EXPORT  CAN1_RX1_IRQHandler               [WEAK]
                EXPORT  CAN1_SCE_IRQHandler               [WEAK]
                EXPORT  EXTI9_5_IRQHandler                [WEAK]
                EXPORT  TIM1_BRK_TIM9_IRQHandler          [WEAK]
                EXPORT  TIM1_UP_TIM10_IRQHandler          [WEAK]
                EXPORT  TIM1_TRG_COM_TIM11_IRQHandler     [WEAK]
                EXPORT  TIM1_CC_IRQHandler                [WEAK]
                EXPORT  TIM2_IRQHandler                   [WEAK]
                EXPORT  TIM3_IRQHandler                   [WEAK]
                EXPORT  TIM4_IRQHandler                   [WEAK]
                EXPORT  I2C1_EV_IRQHandler                [WEAK]
                EXPORT  I2C1_ER_IRQHandler                [WEAK]
                EXPORT  I2C2_EV_IRQHandler                [WEAK]
                EXPORT  I2C2_ER_IRQHandler                [WEAK]
                EXPORT  SPI1_IRQHandler                   [WEAK]
                EXPORT  SPI2_IRQHandler                   [WEAK]
                EXPORT  USART1_IRQHandler                 [WEAK]
                EXPORT  USART2_IRQHandler                 [WEAK]
                EXPORT  USART3_IRQHandler                 [WEAK]
                EXPORT  EXTI15_10_IRQHandler              [WEAK]
                EXPORT  RTC_Alarm_IRQHandler              [WEAK]
                EXPORT  OTG_FS_WKUP_IRQHandler            [WEAK]
                EXPORT  TIM8_BRK_TIM12_IRQHandler         [WEAK]
                EXPORT  TIM8_UP_TIM13_IRQHandler          [WEAK]
                EXPORT  TIM8_TRG_COM_TIM14_IRQHandler     [WEAK]
                EXPORT  TIM8_CC_IRQHandler                [WEAK]
                EXPORT  DMA1_Stream7_IRQHandler           [WEAK]
                EXPORT  FMC_IRQHandler                    [WEAK]
                EXPORT  SDIO_IRQHandler                   [WEAK]
                EXPORT  TIM5_IRQHandler                   [WEAK]
                EXPORT  SPI3_IRQHandler                   [WEAK]
                EXPORT  UART4_IRQHandler                  [WEAK]
                EXPORT  UART5_IRQHandler                  [WEAK]
                EXPORT  TIM6_DAC_IRQHandler               [WEAK]
                EXPORT  TIM7_IRQHandler                   [WEAK]
                EXPORT  DMA2_Stream0_IRQHandler           [WEAK]
                EXPORT  DMA2_Stream1_IRQHandler           [WEAK]
                EXPORT  DMA2_Stream2_IRQHandler           [WEAK]
                EXPORT  DMA2_Stream3_IRQHandler           [WEAK]
                EXPORT  DMA2_Stream4_IRQHandler           [WEAK]
                EXPORT  ETH_IRQHandler                    [WEAK]
                EXPORT  ETH_WKUP_IRQHandler               [WEAK]
                EXPORT  CAN2_TX_IRQHandler                [WEAK]
                EXPORT  CAN2_RX0_IRQHandler               [WEAK]
                EXPORT  CAN2_RX1_IRQHandler               [WEAK]
                EXPORT  CAN2_SCE_IRQHandler               [WEAK]
                EXPORT  OTG_FS_IRQHandler                 [WEAK]
                EXPORT  DMA2_Stream5_IRQHandler           [WEAK]
                EXPORT  DMA2_Stream6_IRQHandler           [WEAK]
                EXPORT  DMA2_Stream7_IRQHandler           [WEAK]
                EXPORT  USART6_IRQHandler                 [WEAK]
                EXPORT  I2C3_EV_IRQHandler                [WEAK]
                EXPORT  I2C3_ER_IRQHandler                [WEAK]
                EXPORT  OTG_HS_EP1_OUT_IRQHandler         [WEAK]
                EXPORT  OTG_HS_EP1_IN_IRQHandler          [WEAK]
                EXPORT  OTG_HS_WKUP_IRQHandler            [WEAK]
                EXPORT  OTG_HS_IRQHandler                 [WEAK]
                EXPORT  DCMI_IRQHandler                   [WEAK]
                EXPORT  HASH_RNG_IRQHandler               [WEAK]
                EXPORT  FPU_IRQHandler                    [WEAK]
                
WWDG_IRQHandler
PVD_IRQHandler
TAMP_STAMP_IRQHandler
RTC_WKUP_IRQHandler
FLASH_IRQHandler
RCC_IRQHandler
EXTI0_IRQHandler
EXTI1_IRQHandler
EXTI2_IRQHandler
EXTI3_IRQHandler
EXTI4_IRQHandler
DMA1_Stream0_IRQHandler
DMA1_Stream1_IRQHandler
DMA1_Stream2_IRQHandler
DMA1_Stream3_IRQHandler
DMA1_Stream4_IRQHandler
DMA1_Stream5_IRQHandler
DMA1_Stream6_IRQHandler
ADC_IRQHandler
CAN1_TX_IRQHandler
CAN1_RX0_IRQHandler
CAN1_RX1_IRQHandler
CAN1_SCE_IRQHandler
EXTI9_5_IRQHandler
TIM1_BRK_TIM9_IRQHandler
TIM1_UP_TIM10_IRQHandler
TIM1_TRG_COM_TIM11_IRQHandler
TIM1_CC_IRQHandler
TIM2_IRQHandler
TIM3_IRQHandler
TIM4_IRQHandler
I2C1_EV_IRQHandler
I2C1_ER_IRQHandler
I2C2_EV_IRQHandler
I2C2_ER_IRQHandler
SPI1_IRQHandler
SPI2_IRQHandler
USART1_IRQHandler
USART2_IRQHandler
USART3_IRQHandler
EXTI15_10_IRQHandler
RTC_Alarm_IRQHandler
OTG_FS_WKUP_IRQHandler
TIM8_BRK_TIM12_IRQHandler
TIM8_UP_TIM13_IRQHandler
TIM8_TRG_COM_TIM14_IRQHandler
TIM8_CC_IRQHandler
DMA1_Stream7_IRQHandler
FMC_IRQHandler
SDIO_IRQHandler
TIM5_IRQHandler
SPI3_IRQHandler
UART4_IRQHandler
UART5_IRQHandler
TIM6_DAC_IRQHandler
TIM7_IRQHandler
DMA2_Stream0_IRQHandler
DMA2_Stream1_IRQHandler
DMA2_Stream2_IRQHandler
DMA2_Stream3_IRQHandler
DMA2_Stream4_IRQHandler
ETH_IRQHandler
ETH_WKUP_IRQHandler
CAN2_TX_IRQHandler
CAN2_RX0_IRQHandler
CAN2_RX1_IRQHandler
CAN2_SCE_IRQHandler
OTG_FS_IRQHandler
DMA2_Stream5_IRQHandler
DMA2_Stream6_IRQHandler
DMA2_Stream7_IRQHandler
USART6_IRQHandler
I2C3_EV_IRQHandler
I2C3_ER_IRQHandler
OTG_HS_EP1_OUT_IRQHandler
OTG_HS_EP1_IN_IRQHandler
OTG_HS_WKUP_IRQHandler
OTG_HS_IRQHandler
DCMI_IRQHandler
HASH_RNG_IRQHandler
FPU_IRQHandler
           
                B       .

                ENDP

    EXPORT  FAULT_SubHandler
    IMPORT  FAULT_Handler             ; void FAULT_Handler(UINT32*, UINT32)

    EXPORT  HARD_Breakpoint
    IMPORT  HARD_Breakpoint_Handler   ; HARD_Breakpoint_Handler(UINT32*)

; This serves as an adapter from the Cortex-M exception signature
; to map back to the original CLR API designed around the older ARM
; mode exception architecture.
    AREA ||i.FAULT_SubHandler||, CODE, READONLY
FAULT_SubHandler
        ; on entry, we have an exception frame on the stack:
        ; SP+00: R0
        ; SP+04: R1
        ; SP+08: R2
        ; SP+12: R3
        ; SP+16: R12
        ; SP+20: LR
        ; SP+24: PC
        ; SP+28: PSR
        ; R0-R12 are not overwritten yet
            add      sp,sp,#16             ; remove R0-R3
            push     {r0-r11}              ; store R0-R11
            mov      r0,sp
        ; R0+00: R0-R12
        ; R0+52: LR
        ; R0+56: PC
        ; R0+60: PSR
            mrs      r1,IPSR               ; exception number
            b        FAULT_Handler
        ; never expect to return

    ;*****************************************************************************

        AREA ||i.HARD_Breakpoint||, CODE, READONLY
HARD_Breakpoint
    ; on entry, were are being called from C/C++ in Thread mode
        add      sp,sp,#-4            ; space for PSR
        push     {r14}                ; store original PC
        push     {r0-r12,r14}         ; store R0 - R12, LR
        mov      r0,sp
        mrs      r1,XPSR
        str      r1,[r0,#60]          ; store PSR
    ; R0+00: R0-R12
    ; R0+52: LR
    ; R0+56: PC
    ; R0+60: PSR
        b        HARD_Breakpoint_Handler
    ; never expect to return

    ;*****************************************************************************

    END