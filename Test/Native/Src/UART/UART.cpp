////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "UART.h"

//--//

BOOL UART::Execute( LOG_STREAM Stream )
{
    //  Transmit a buffer of 8 bytes to end-point; ... end-point expected to echo byte stream. 
    //  Following transmit, read 8 bytes from end-point and validate as same  
    //  See test\tools for echo tool
  
    Log& log = Log::InitializeLog( Stream, "USART" );

    if(!USART_Initialize( m_com, m_baud, m_parity, m_stop, m_data, m_flow ))
    {
        log.CloseLog( FALSE, "USART init fail" );
        return FALSE;
    }

    if(!USART_Write( UART::m_com, UART::m_xmitBuffer, BUFFER_SIZE ))
    {
        log.CloseLog( FALSE, "write failed" );
    }
    else
    {
       Events_WaitForEvents( 0, 100 );

        USART_Read( UART::m_com, UART::m_recvBuffer, BUFFER_SIZE );

       if(Validate())
       {
           log.CloseLog( TRUE, NULL );
       }
       else
       {
           log.CloseLog( FALSE, "validate failed" );
       }
    }
    return TRUE;
}

UART::UART( int com, int baud, int parity, int stop, int data, int flow )
{
    m_com     = ConvertCOM_ComPort(com);
    m_baud    = baud;
    m_parity  = parity;
    m_stop    = stop;
    m_data    = data;
    m_flow    = flow;
    
    InitializeXmitBuffer( );
}

INT8 SomeValue( int value )
{
        return ((value + 0x50)  % 0x100);
}

BOOL UART::Validate()
{
    for(INT16 i=0; i<BUFFER_SIZE; i++)
    { 
        if(m_recvBuffer[i] != (char) SomeValue(i))
        {
            return false;
        }
    }
    return true;
}

void UART::InitializeXmitBuffer()
{
        for(INT16 i=0; i<BUFFER_SIZE; i++)
            m_xmitBuffer[i] = SomeValue(i);
}

//--//

