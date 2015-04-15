/******************************************************************************
 * Copyright (c) 2014, AllSeen Alliance. All rights reserved.
 *
 *    Permission to use, copy, modify, and/or distribute this software for any
 *    purpose with or without fee is hereby granted, provided that the above
 *    copyright notice and this permission notice appear in all copies.
 *
 *    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 *    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 *    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 *    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 *    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 *    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 *    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 ******************************************************************************/
/******************************************************************************
 * This code statically links to code available from
 * http://www.st.com/web/en/catalog/tools/ and that code is subject to a license
 * agreement with terms and conditions that you will be responsible for from
 * STMicroelectronics if you employ that code. Use of such code is your responsibility.
 * Neither AllSeen Alliance nor any contributor to this AllSeen code base has any
 * obligations with respect to the STMicroelectronics code that to which you will be
 * statically linking this code. One requirement in the license is that the
 * STMicroelectronics code may only be used with STMicroelectronics processors as set
 * forth in their agreement."
 *******************************************************************************/

#include "aj_target.h"
#include "aj_target_platform.h"
#include "stm32f4xx.h"

GPIO_InitTypeDef SPI_PWD_PIN;
GPIO_InitTypeDef SPI_FET_PIN;

/** TX interrupt occurred */
volatile uint8_t g_b_spi_interrupt_tx_ready = FALSE;
/** RX interrupt occurred */
volatile uint8_t g_b_spi_interrupt_rx_ready = FALSE;

volatile uint8_t g_b_spi_interrupt_data_ready = FALSE;

static volatile uint8_t read_buf;
static uint8_t write_buf;


aj_spi_status AJ_SPI_WRITE(uint8_t* spi_device, uint8_t byte, uint8_t pcs, uint8_t cont)
{
    SPI_SSOutputCmd(SPI1, ENABLE);
    while (!SPI_I2S_GetFlagStatus(SPI1, SPI_I2S_FLAG_TXE)) ;
    SPI_I2S_SendData(SPI1, byte);
    while (!SPI_I2S_GetFlagStatus(SPI1, SPI_I2S_FLAG_RXNE)) ;
    read_buf = SPI_I2S_ReceiveData(SPI1) & 0xFF;

    if (cont == 1) {
        SPI_SSOutputCmd(SPI1, DISABLE);
    }
    return SPI_OK;
}
aj_spi_status AJ_SPI_READ(uint8_t spi_device, uint8_t* data, uint8_t pcs)
{
    memcpy(data, &read_buf, 1);
    return SPI_OK;
}
AJ_Status AJ_WSL_SPI_DMATransfer(uint8_t* buffer, uint16_t len, uint8_t direction)
{
    DMA_InitTypeDef DMA_InitStructure;

    RCC_AHB1PeriphClockCmd(RCC_AHB1Periph_DMA2, ENABLE);
    SPI_SSOutputCmd(SPI1, ENABLE);
    DMA_StructInit(&DMA_InitStructure);
    DMA_InitStructure.DMA_Channel = DMA_Channel_3;
    DMA_InitStructure.DMA_PeripheralBaseAddr = (uint32_t) &(SPI1->DR);
    DMA_InitStructure.DMA_MemoryDataSize = DMA_MemoryDataSize_Byte;
    DMA_InitStructure.DMA_PeripheralDataSize = DMA_PeripheralDataSize_Byte;
    DMA_InitStructure.DMA_Mode = DMA_Mode_Normal;
    DMA_InitStructure.DMA_Priority = DMA_Priority_VeryHigh;
    DMA_InitStructure.DMA_FIFOMode = DMA_FIFOMode_Disable;
    DMA_InitStructure.DMA_MemoryBurst = DMA_MemoryBurst_Single;
    DMA_InitStructure.DMA_PeripheralBurst = DMA_PeripheralBurst_Single;
    if (direction == 1) {
        // Configure DMA for transmit

        DMA_DeInit(DMA2_Stream3);


        DMA_InitStructure.DMA_Memory0BaseAddr = (uint32_t)buffer;
        DMA_InitStructure.DMA_DIR = DMA_DIR_MemoryToPeripheral;
        DMA_InitStructure.DMA_BufferSize = len;
        DMA_InitStructure.DMA_PeripheralInc = DMA_PeripheralInc_Disable;
        DMA_InitStructure.DMA_MemoryInc = DMA_MemoryInc_Enable;

        DMA_Init(DMA2_Stream3, &DMA_InitStructure);

        SPI_I2S_DMACmd(SPI1, SPI_I2S_DMAReq_Tx, ENABLE);

        DMA_Cmd(DMA2_Stream3, ENABLE);

        // Wait for DMA to finish
        while (DMA_GetCmdStatus(DMA2_Stream3) == ENABLE) ;
        while (!DMA_GetFlagStatus(DMA2_Stream3, DMA_FLAG_TCIF3)) ;

    } else {
        uint8_t zero = 0;
        // Configure DMA for receive

        DMA_DeInit(DMA2_Stream0);
        DMA_DeInit(DMA2_Stream3);


        DMA_InitStructure.DMA_Memory0BaseAddr = (uint32_t)buffer;
        DMA_InitStructure.DMA_DIR = DMA_DIR_PeripheralToMemory;
        DMA_InitStructure.DMA_BufferSize = len;
        DMA_InitStructure.DMA_PeripheralInc = DMA_PeripheralInc_Disable;
        DMA_InitStructure.DMA_MemoryInc = DMA_MemoryInc_Enable;

        DMA_Init(DMA2_Stream0, &DMA_InitStructure);

        DMA_InitStructure.DMA_DIR = DMA_DIR_MemoryToPeripheral;
        DMA_InitStructure.DMA_Memory0BaseAddr = (uint32_t)&zero;
        DMA_InitStructure.DMA_MemoryInc = DMA_MemoryInc_Disable;
        DMA_Init(DMA2_Stream3, &DMA_InitStructure);

        SPI_I2S_DMACmd(SPI1, SPI_I2S_DMAReq_Rx | SPI_I2S_DMAReq_Tx, ENABLE);

        DMA_Cmd(DMA2_Stream0, ENABLE);
        DMA_Cmd(DMA2_Stream3, ENABLE);

        // Wait for DMA to finish
        while (DMA_GetCmdStatus(DMA2_Stream3) == ENABLE) ;
        while (!DMA_GetFlagStatus(DMA2_Stream3, DMA_FLAG_TCIF3)) ;
        DMA_ClearFlag(DMA2_Stream3, DMA_FLAG_TCIF3);
        DMA_ClearFlag(DMA2_Stream0, DMA_FLAG_TCIF0);

    }
    SPI_SSOutputCmd(SPI1, DISABLE);
}

void AJ_WSL_SPI_PowerCycleChip(void)
{
    SPI_SSOutputCmd(SPI1, DISABLE);
    /* PWD */
    GPIO_ResetBits(GPIOE, GPIO_Pin_7);
    AJ_Sleep(10);
    GPIO_SetBits(GPIOE, GPIO_Pin_7);
}

void AJ_WSL_SPI_InitializeSPIController(void)
{
    GPIO_InitTypeDef SPI_Pins;
    GPIO_InitTypeDef PowerPin;
    EXTI_InitTypeDef SPI_Int;
    NVIC_InitTypeDef Int_NVIC;

    RCC_AHB1PeriphClockCmd(RCC_AHB1Periph_GPIOA | RCC_AHB1Periph_GPIOE, ENABLE);
    RCC_AHB1PeriphClockCmd(RCC_AHB1Periph_GPIOC | RCC_AHB1Periph_GPIOB, ENABLE);
    RCC_AHB1PeriphClockCmd(RCC_AHB1Periph_GPIOD, ENABLE);
    RCC_APB2PeriphClockCmd(RCC_APB2Periph_SYSCFG, ENABLE);
    RCC_AHB1PeriphClockCmd(RCC_AHB1Periph_DMA2, ENABLE);
    RCC_APB1PeriphClockCmd(RCC_APB1Periph_TIM3, ENABLE);

    SPI_Pins.GPIO_Pin = 0xF0;
    SPI_Pins.GPIO_Mode = GPIO_Mode_AF;
    SPI_Pins.GPIO_Speed = GPIO_Speed_50MHz;
    SPI_Pins.GPIO_OType = GPIO_OType_PP;
    SPI_Pins.GPIO_PuPd = GPIO_PuPd_UP;

    GPIO_Init(GPIOA, &SPI_Pins);

    SPI_Pins.GPIO_Pin = GPIO_Pin_7;
    SPI_Pins.GPIO_Mode = GPIO_Mode_OUT;
    SPI_Pins.GPIO_Speed = GPIO_Speed_50MHz;
    SPI_Pins.GPIO_OType = GPIO_OType_PP;
    SPI_Pins.GPIO_PuPd = GPIO_PuPd_UP;

    GPIO_Init(GPIOE, &SPI_Pins);

    GPIOE->ODR = 0;

    RCC_APB2PeriphClockCmd(RCC_APB2Periph_SPI1, ENABLE);

    AJ_SPIHandle.SPI_BaudRatePrescaler = SPI_BaudRatePrescaler_4;
    AJ_SPIHandle.SPI_CPHA = SPI_CPHA_2Edge;
    AJ_SPIHandle.SPI_CPOL = SPI_CPOL_High;
    AJ_SPIHandle.SPI_DataSize = SPI_DataSize_8b;
    AJ_SPIHandle.SPI_Direction = SPI_Direction_2Lines_FullDuplex;
    AJ_SPIHandle.SPI_FirstBit = SPI_FirstBit_MSB;
    AJ_SPIHandle.SPI_Mode = SPI_Mode_Master;
    AJ_SPIHandle.SPI_NSS = SPI_NSS_Soft;
    AJ_SPIHandle.SPI_CRCPolynomial = 7;

    SPI_Init(SPI1, &AJ_SPIHandle);

    /* Configure the SPI interrupt line */
    SPI_Pins.GPIO_Pin = GPIO_Pin_1;
    SPI_Pins.GPIO_Mode = GPIO_Mode_IN;
    SPI_Pins.GPIO_Speed = GPIO_Speed_2MHz;
    SPI_Pins.GPIO_OType = 0xA5;
    SPI_Pins.GPIO_PuPd = GPIO_PuPd_NOPULL;

    NVIC_PriorityGroupConfig(NVIC_PriorityGroup_4);

    SYSCFG_EXTILineConfig(EXTI_PortSourceGPIOA, EXTI_PinSource1);

    GPIO_Init(GPIOA, &SPI_Pins);

    SPI_Int.EXTI_Line = EXTI_Line1;
    SPI_Int.EXTI_LineCmd = ENABLE;
    SPI_Int.EXTI_Mode = EXTI_Mode_Interrupt;
    SPI_Int.EXTI_Trigger = EXTI_Trigger_Falling;

    EXTI_Init(&SPI_Int);

    Int_NVIC.NVIC_IRQChannel = EXTI1_IRQn;
    Int_NVIC.NVIC_IRQChannelPreemptionPriority = 6;
    Int_NVIC.NVIC_IRQChannelSubPriority = 6;
    Int_NVIC.NVIC_IRQChannelCmd = ENABLE;

    NVIC_Init(&Int_NVIC);

    GPIO_PinAFConfig(GPIOA, GPIO_PinSource4, GPIO_AF_SPI1);
    GPIO_PinAFConfig(GPIOA, GPIO_PinSource5, GPIO_AF_SPI1);
    GPIO_PinAFConfig(GPIOA, GPIO_PinSource6, GPIO_AF_SPI1);
    GPIO_PinAFConfig(GPIOA, GPIO_PinSource7, GPIO_AF_SPI1);

    AJ_WSL_SPI_PowerCycleChip();

    SPI_Cmd(SPI1, ENABLE);
}
void AJ_WSL_SPI_ShutdownSPIController(void)
{

}
void AJ_WSL_SPI_ISR(void)
{

}
void EXTI1_IRQHandler(void)
{
    AJ_EnterCriticalRegion();
    if (EXTI_GetITStatus(EXTI_Line1) != RESET) {
        //Handle the interrupt
        EXTI_ClearITPendingBit(EXTI_Line1);
        EXTI_ClearFlag(EXTI_Line1);
        AJ_WSL_SPI_CHIP_SPI_ISR(1, 1);
    }
    AJ_LeaveCriticalRegion();
}
extern struct AJ_TaskHandle* AJ_WSL_MBoxListenHandle;
void AJ_WSL_SPI_CHIP_SPI_ISR(uint32_t id, uint32_t mask)
{
    //__disable_irq();
    g_b_spi_interrupt_data_ready = TRUE;
    AJ_ResumeTask(AJ_WSL_MBoxListenHandle, TRUE);
    //__enable_irq();
}
