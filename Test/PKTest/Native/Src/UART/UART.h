////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <usart_decl.h>
#include "..\Log\Log.h"

#ifndef _UART_NMT_
#define _UART_NMT_  1

#define BUFFER_SIZE 8

#define FlowCtrlNone USART_FLOW_NONE
#define FlowCtrlSW   USART_FLOW_SW_IN_EN | USART_FLOW_SW_OUT_EN
#define FlowCtrlHW   USART_FLOW_HW_IN_EN | USART_FLOW_HW_OUT_EN

typedef
struct _UART : Log
{
    int ComPort;                                 // Com port, begins at 0
    int BaudRate;                                //{1200, 9600, 57600, 115200, 230400, ...};
    int Parity;
    int Stop;                                    // Stop bit
    int Data;                                    // Data bit
    int FlowValue; 
    char XmitBuffer[BUFFER_SIZE];
    char RecvBuffer[BUFFER_SIZE];

    _UART(int ComNum,
          int BaudNum,
          int ParityNum,
          int StopNum,
          int DataNum,
          int FlowNum)
    {
        ComPort   =    ComNum;
	BaudRate  =    BaudNum;
	Parity    =    ParityNum;
	Stop      =    StopNum;
	Data      =    DataNum;
	FlowValue =    FlowNum;

        InitXmitBuffer();
    };

    BOOL UART_Transmit(char *, int, int);

    BOOL UART_Receive(char *, int, int);

    BOOL UARTTest(NMT_STREAM Stream)
    {
	Log::Result=false;

	//
	//  Initialize the Log object
	//
	Initialize(Stream);
	BeginTest("UART");

        if (!USART_Initialize(ComPort, BaudRate, Parity, Stop, Data, FlowValue ))
	{
            return false;
	}
        
        if (UART_Transmit(XmitBuffer,BUFFER_SIZE, 3) &&
            UART_Receive(RecvBuffer,BUFFER_SIZE, 3))

        {
           Log::Result = Validate(BUFFER_SIZE);
        }
	EndTest(Log::Result);
        return Result;
    };


    INT8 SomeValue(int i)
    {
        return ((i + 0x80)  % 0x100);
    }
    BOOL Validate(int CountReceived)
    {
        if (CountReceived > BUFFER_SIZE)
            CountReceived = BUFFER_SIZE;
        for(INT16 i=0; i<CountReceived; i++)
        {
            
            if (RecvBuffer[i] != (char) SomeValue(i) )
            {
                return false;
            }
        }
        return true;
    };

    void InitXmitBuffer()
    {
    //
    // Initialize entire buffer in strips of 128-255
    //
        for(INT16 i=0; i<BUFFER_SIZE; i++)
            XmitBuffer[i] = SomeValue(i);
    };
    

} UART;

#endif
