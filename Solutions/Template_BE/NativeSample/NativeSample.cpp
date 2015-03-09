////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <Tests.h>
#include "nativesample.h"

//--//

HAL_DECLARE_NULL_HEAP();

void ApplicationEntryPoint()
{
    BOOL result;
   
    TimedEvents eventsTest;
    UART        usartTest  ( COMTestPort, 9600, USART_PARITY_NONE, 8, USART_STOP_BITS_ONE, USART_FLOW_NONE );
    GPIO        gpioTest   ( GPIOTestPin );

    do
    {    
        result = eventsTest.Execute( STREAM__LCD );
        result = usartTest.Execute ( STREAM__LCD );
        result = gpioTest.Execute  ( STREAM__LCD );

    } while(FALSE); // run only once!

    while(TRUE);
}

