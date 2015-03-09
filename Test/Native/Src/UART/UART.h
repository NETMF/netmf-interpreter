////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <usart_decl.h>
#include "..\Log\Log.h"

//--//

#ifndef _UART_TEST_
#define _UART_TEST_  1

//--//

#define BUFFER_SIZE    8
#define FlowCtrlNone   USART_FLOW_NONE
#define FlowCtrlSW     USART_FLOW_SW_IN_EN | USART_FLOW_SW_OUT_EN
#define FlowCtrlHW     USART_FLOW_HW_IN_EN | USART_FLOW_HW_OUT_EN

//--//

class UART 
{
    int     m_com;                        // Com port may enumerate from 0, ...
    int     m_baud;                       //{1200, 9600, 57600, 115200, 230400, ...}
    int     m_parity;                     // parity
    int     m_stop;                       // stop bit
    int     m_data;                       // data bit count
    int     m_flow;                       // flow-control, see USART_decl.h
    char    m_xmitBuffer[BUFFER_SIZE];    // for out-bound messages
    char    m_recvBuffer[BUFFER_SIZE];    // for in-bound messages

//--//

public:

            UART                  ( int com, int baud, int parity, int stop, int data, int flow );
    BOOL    Execute               ( LOG_STREAM Stream );
    void    InitializeXmitBuffer  ( );
    BOOL    Validate              ( );       
};

#endif

//--//

