////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "UART.h"

//------------------------------------------------------------------------------


BOOL UART::UART_Receive(char * RecvBuffer, int MaxExpected, int MaxLoop)
{
   INT16    CountReceived = 0;
   INT16    LoopCount=0;
   
   do
   {
       if (LoopCount++ > MaxLoop)
       {
           break;
       }

       CountReceived += USART_Read(UART::ComPort, &RecvBuffer[CountReceived], 
		                   (sizeof (char)*(MaxExpected-CountReceived)));

       if (CountReceived >= MaxExpected)
       {
           return true;
       }
       if (!USART_Flush(UART::ComPort))
       {
           Comment("Failed to flush");
           return false;
       }
   } while(1);

   if (CountReceived < MaxExpected)
   {
       UART::Comment("wrong rcv count");     
       return false;
   }

   return true;
}

BOOL UART::UART_Transmit(char * XmitBuffer, int MaxToSend, int MaxLoop)
{
    INT16 CountSent=0;
    INT16 LoopCount=0;
    
    do
    {
        if (LoopCount++ > MaxLoop)
        {
            break;
        }
        CountSent += USART_Write(UART::ComPort, &XmitBuffer[CountSent], (MaxToSend-CountSent)*sizeof(char));
        if (CountSent >= MaxToSend)
        {            
            return true;
        }
    }
    while(1);

    //if (!USART_Flush(UART::ComPort))
   // {
    //    Comment("Failed to flush");       
     //   return false;
    //}
    return true;
}
