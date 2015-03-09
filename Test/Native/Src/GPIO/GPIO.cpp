////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "GPIO.h"

//---//

//
// Note: CPU_GPIO_Initialization is called elsewhere
//

BOOL GPIO::Execute( LOG_STREAM Stream )
{
    BOOL offPinState = TRUE;     // If test succeeds, states
    BOOL onPinState  = FALSE;    // are reversed

    Log& log = Log::InitializeLog( Stream, "GPIO" );
   
    if((GPIO_PIN_NONE == m_gpioPin) || CPU_GPIO_PinIsBusy(m_gpioPin))
    {
        log.CloseLog( FALSE, "pin unavailable" );
    }
    else
    {
        onPinState = TRUE;        

        CPU_GPIO_SetPinState( m_gpioPin, onPinState );

        onPinState = CPU_GPIO_GetPinState( m_gpioPin );

        if(!onPinState )
        {
            log.CloseLog( FALSE, "on->off fails" );
        }
        else
        {
            offPinState  = FALSE;
            
            CPU_GPIO_SetPinState( m_gpioPin, offPinState );

            offPinState = CPU_GPIO_GetPinState( m_gpioPin );

            if(offPinState)
            {
                log.CloseLog( FALSE, "off->on fails" );
            }
        }     
    }  
    return (onPinState && !offPinState); 
} //Execute


GPIO::GPIO( GPIO_PIN testPin )
{
    m_gpioPin = testPin;
}

//--//

