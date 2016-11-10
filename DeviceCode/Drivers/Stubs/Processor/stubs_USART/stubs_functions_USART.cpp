////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//
__weak BOOL CPU_USART_Initialize( int ComPortNum, int BaudRate, int Parity, int DataBits, int StopBits, int FlowValue )
{
    return TRUE;
}

__weak BOOL CPU_USART_Uninitialize( int ComPortNum )
{
    return TRUE;
}

__weak BOOL CPU_USART_TxBufferEmpty( int ComPortNum )
{
    return TRUE;
}

__weak BOOL CPU_USART_TxShiftRegisterEmpty( int ComPortNum )
{
    return TRUE;
}

__weak void CPU_USART_WriteCharToTxBuffer( int ComPortNum, UINT8 c )
{
}

__weak void CPU_USART_TxBufferEmptyInterruptEnable( int ComPortNum, BOOL Enable )
{
}

__weak BOOL CPU_USART_TxBufferEmptyInterruptState( int ComPortNum )
{
    return TRUE;
}

__weak void CPU_USART_RxBufferFullInterruptEnable( int ComPortNum, BOOL Enable )
{
}

__weak BOOL CPU_USART_RxBufferFullInterruptState( int ComPortNum )
{
    return TRUE;
}

__weak void CPU_USART_ProtectPins( int ComPortNum, BOOL On )
{
}

__weak UINT32 CPU_USART_PortsCount()
{
    return 0;
}

__weak void CPU_USART_GetPins( int ComPortNum, GPIO_PIN& rxPin, GPIO_PIN& txPin,GPIO_PIN& ctsPin, GPIO_PIN& rtsPin )
{   
    rxPin = GPIO_PIN_NONE; 
    txPin = GPIO_PIN_NONE; 
    ctsPin= GPIO_PIN_NONE; 
    rtsPin= GPIO_PIN_NONE; 

    return;
}

__weak BOOL CPU_USART_SupportNonStandardBaudRate ( int ComPortNum )
{
    return FALSE;
}

__weak void CPU_USART_GetBaudrateBoundary( int ComPortNum, UINT32& maxBaudrateHz, UINT32& minBaudrateHz )
{
    maxBaudrateHz = 0;
    minBaudrateHz = 0;
}

__weak BOOL CPU_USART_IsBaudrateSupported( int ComPortNum, UINT32 & BaudrateHz )
{   
    return FALSE;
}

__weak BOOL CPU_USART_TxHandshakeEnabledState( int comPort )
{
    return TRUE;
}
