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

@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@                
@ Dummy Exception Handlers (infinite loops which can be overloaded since they are exported with weak linkage )

    .section SectionForBootstrapOperations, "ax", %progbits

    .align 1
    .thumb_func
    .weak Default_Handler
    .type Default_Handler, %function
Default_Handler:
    b  .
    .size Default_Handler, . - Default_Handler

/*    Macro to define default handlers. Default handler
 *    will be weak symbol and just dead loops. They can be
 *    overwritten by other handlers */
    .macro    def_irq_handler    handler_name
    .weak    \handler_name
    .set    \handler_name, Default_Handler
    .endm

    def_irq_handler    NMI_Handler
    def_irq_handler    HardFault_Handler
    def_irq_handler    MemManage_Handler
    def_irq_handler    BusFault_Handler
    def_irq_handler    UsageFault_Handler
    def_irq_handler    SVC_Handler
    def_irq_handler    DebugMon_Handler
    def_irq_handler    PendSV_Handler
    def_irq_handler    SysTick_Handler

@ SOC Specific default handlers
    def_irq_handler    WWDG_IRQHandler
    def_irq_handler    PVD_IRQHandler
    def_irq_handler    TAMP_STAMP_IRQHandler
    def_irq_handler    RTC_WKUP_IRQHandler
    def_irq_handler    FLASH_IRQHandler
    def_irq_handler    RCC_IRQHandler
    def_irq_handler    EXTI0_IRQHandler
    def_irq_handler    EXTI1_IRQHandler
    def_irq_handler    EXTI2_IRQHandler
    def_irq_handler    EXTI3_IRQHandler
    def_irq_handler    EXTI4_IRQHandler
    def_irq_handler    DMA1_Stream0_IRQHandler
    def_irq_handler    DMA1_Stream1_IRQHandler
    def_irq_handler    DMA1_Stream2_IRQHandler
    def_irq_handler    DMA1_Stream3_IRQHandler
    def_irq_handler    DMA1_Stream4_IRQHandler
    def_irq_handler    DMA1_Stream5_IRQHandler
    def_irq_handler    DMA1_Stream6_IRQHandler
    def_irq_handler    ADC_IRQHandler
    def_irq_handler    CAN1_TX_IRQHandler
    def_irq_handler    CAN1_RX0_IRQHandler
    def_irq_handler    CAN1_RX1_IRQHandler
    def_irq_handler    CAN1_SCE_IRQHandler
    def_irq_handler    EXTI9_5_IRQHandler
    def_irq_handler    TIM1_BRK_TIM9_IRQHandler
    def_irq_handler    TIM1_UP_TIM10_IRQHandler
    def_irq_handler    TIM1_TRG_COM_TIM11_IRQHandler
    def_irq_handler    TIM1_CC_IRQHandler
    def_irq_handler    TIM2_IRQHandler
    def_irq_handler    TIM3_IRQHandler
    def_irq_handler    TIM4_IRQHandler
    def_irq_handler    I2C1_EV_IRQHandler
    def_irq_handler    I2C1_ER_IRQHandler
    def_irq_handler    I2C2_EV_IRQHandler
    def_irq_handler    I2C2_ER_IRQHandler
    def_irq_handler    SPI1_IRQHandler
    def_irq_handler    SPI2_IRQHandler
    def_irq_handler    USART1_IRQHandler
    def_irq_handler    USART2_IRQHandler
    def_irq_handler    USART3_IRQHandler
    def_irq_handler    EXTI15_10_IRQHandler
    def_irq_handler    RTC_Alarm_IRQHandler
    def_irq_handler    OTG_FS_WKUP_IRQHandler
    def_irq_handler    TIM8_BRK_TIM12_IRQHandler
    def_irq_handler    TIM8_UP_TIM13_IRQHandler
    def_irq_handler    TIM8_TRG_COM_TIM14_IRQHandler
    def_irq_handler    TIM8_CC_IRQHandler
    def_irq_handler    DMA1_Stream7_IRQHandler
    def_irq_handler    FMC_IRQHandler
    def_irq_handler    SDIO_IRQHandler
    def_irq_handler    TIM5_IRQHandler
    def_irq_handler    SPI3_IRQHandler
    def_irq_handler    UART4_IRQHandler
    def_irq_handler    UART5_IRQHandler
    def_irq_handler    TIM6_DAC_IRQHandler
    def_irq_handler    TIM7_IRQHandler
    def_irq_handler    DMA2_Stream0_IRQHandler
    def_irq_handler    DMA2_Stream1_IRQHandler
    def_irq_handler    DMA2_Stream2_IRQHandler
    def_irq_handler    DMA2_Stream3_IRQHandler
    def_irq_handler    DMA2_Stream4_IRQHandler
    def_irq_handler    ETH_IRQHandler
    def_irq_handler    ETH_WKUP_IRQHandler
    def_irq_handler    CAN2_TX_IRQHandler
    def_irq_handler    CAN2_RX0_IRQHandler
    def_irq_handler    CAN2_RX1_IRQHandler
    def_irq_handler    CAN2_SCE_IRQHandler
    def_irq_handler    OTG_FS_IRQHandler
    def_irq_handler    DMA2_Stream5_IRQHandler
    def_irq_handler    DMA2_Stream6_IRQHandler
    def_irq_handler    DMA2_Stream7_IRQHandler
    def_irq_handler    USART6_IRQHandler
    def_irq_handler    I2C3_EV_IRQHandler
    def_irq_handler    I2C3_ER_IRQHandler
    def_irq_handler    OTG_HS_EP1_OUT_IRQHandler
    def_irq_handler    OTG_HS_EP1_IN_IRQHandler
    def_irq_handler    OTG_HS_WKUP_IRQHandler
    def_irq_handler    OTG_HS_IRQHandler
    def_irq_handler    DCMI_IRQHandler
    def_irq_handler    HASH_RNG_IRQHandler
    def_irq_handler    FPU_IRQHandler

    .global  FAULT_SubHandler
    .extern  FAULT_Handler             @ void FAULT_Handler(UINT32*, UINT32)

    .global  HARD_Breakpoint
    .extern  HARD_Breakpoint_Handler   @ void HARD_Breakpoint_Handler(UINT32*)

@ This serves as an adapter from the Cortex-M exception signature
@ to map back to the original CLR API designed around the older ARM
@ mode exception architecture.
    .section i.FAULT_SubHandler, "ax", %progbits
    .thumb_func

FAULT_SubHandler:
        @ on entry, we have an exception frame on the stack:
        @ SP+00: R0
        @ SP+04: R1
        @ SP+08: R2
        @ SP+12: R3
        @ SP+16: R12
        @ SP+20: LR
        @ SP+24: PC
        @ SP+28: PSR
        @ R0-R12 are not overwritten yet
            add      sp,sp,#16             @ remove R0-R3
            push     {r0-r11}              @ store R0-R11
            mov      r0,sp
        @ R0+00: R0-R12
        @ R0+52: LR
        @ R0+56: PC
        @ R0+60: PSR
            mrs      r1,IPSR               @ exception number
            b        FAULT_Handler
        @ never expect to return

    .section    i.HARD_Breakpoint, "ax", %progbits
HARD_Breakpoint:
    @ on entry, were are being called from C/C++ in Thread mode
        add      sp,sp,#-4            @ space for PSR
        push     {r14}                @ store original PC
        push     {r0-r12,r14}         @ store R0 - R12, LR
        mov      r0,sp
        mrs      r1,XPSR
        str      r1,[r0,#60]          @ store PSR
    @ R0+00: R0-R12
    @ R0+52: LR
    @ R0+56: PC
    @ R0+60: PSR
        b        HARD_Breakpoint_Handler
    @ never expect to return

.end
