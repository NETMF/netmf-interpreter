////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

//--//


__weak BOOL USART_Initialize( int ComPortNum, int BaudRate, int Parity, int DataBits, int StopBits, int FlowValue )
{
    return TRUE;
}

__weak BOOL USART_Uninitialize( int ComPortNum )
{
    return TRUE;
}

__weak int USART_Write( int ComPortNum, const char* Data, size_t size )
{
    return 0;
}

__weak int USART_Read( int ComPortNum, char* Data, size_t size )
{
    return 0;
}

__weak BOOL USART_Flush( int ComPortNum )
{
    return TRUE;
}

__weak BOOL USART_AddCharToRxBuffer( int ComPortNum, char c )
{
    return TRUE;
}

__weak BOOL USART_RemoveCharFromTxBuffer( int ComPortNum, char& c )
{
    return TRUE;
}

__weak INT8 USART_PowerSave( int ComPortNum, INT8 Enable )
{
    return 0;
}

__weak void USART_PrepareForClockStop()
{
}

__weak void USART_ClockStopFinished()
{
}

__weak void USART_ForceXON(int ComPortNum) 
{
}

__weak void USART_CloseAllPorts()
{
}

__weak int  USART_BytesInBuffer( int ComPortNum, BOOL fRx )
{
    return 0;
}

__weak void USART_DiscardBuffer( int ComPortNum, BOOL fRx )
{
}

__weak BOOL USART_ConnectEventSink( int ComPortNum, int EventType, void* pContext, PFNUsartEvent pfnUsartEvtHandler, void** ppArg )
{
    return TRUE;
}

__weak void USART_SetEvent( int ComPortNum, unsigned int event )
{
}

