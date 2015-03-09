////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

#include "GPIO_Buttons.h"

//--//

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_GPIO_BUTTON_Driver"
#endif

GPIO_BUTTON_Driver g_GPIO_BUTTON_Driver;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

//--//

BOOL Buttons_Initialize()
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return GPIO_BUTTON_Driver::Initialize();
}

BOOL Buttons_Uninitialize()
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return GPIO_BUTTON_Driver::Uninitialize();
}

BOOL Buttons_RegisterStateChange( UINT32 ButtonsPressed, UINT32 ButtonsReleased )
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return GPIO_BUTTON_Driver::RegisterStateChange( ButtonsPressed, ButtonsReleased );
}

BOOL Buttons_GetNextStateChange( UINT32& ButtonsPressed, UINT32& ButtonsReleased )
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return GPIO_BUTTON_Driver::GetNextStateChange( ButtonsPressed, ButtonsReleased );
}

UINT32 Buttons_CurrentState()
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return GPIO_BUTTON_Driver::CurrentState();
}

UINT32 Buttons_HW_To_Hal_Button( UINT32 HW_Buttons )
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return GPIO_BUTTON_Driver::HW_To_Hal_Button( HW_Buttons );
}

UINT32 Buttons_CurrentHWState()
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return GPIO_BUTTON_Driver::CurrentHWState();
}

//--//

BOOL GPIO_BUTTON_Driver::Initialize()
{
    NATIVE_PROFILE_PAL_BUTTONS();
    GPIO_BUTTON_CONFIG* Config = &g_GPIO_BUTTON_Config;

    HAL_CONFIG_BLOCK::ApplyConfig( Config->GetDriverName(), Config, sizeof(GPIO_BUTTON_CONFIG) );

    // reset button fifo queue
    g_GPIO_BUTTON_Driver.m_ButtonFifo.Initialize();

    // Buttons down, read as a 0 on the pins, and up buttons read as a 1,
    // for simplicity, we invert that notion when reading the pins

    // if we start up with keys pressed, store them as such
    // do all buttons
    GPIO_HW_TO_HAL_MAPPING* Mapping    = &Config->Mapping[0                          ];
    GPIO_HW_TO_HAL_MAPPING* MappingEnd = &Config->Mapping[GPIO_BUTTON_CONFIG::c_COUNT];

    for(; Mapping < MappingEnd; Mapping++)
    {
        GPIO_PIN hw = Mapping->m_HW;

        if(hw != GPIO_PIN_NONE)
        {
            CPU_GPIO_EnableInputPin( hw, TRUE, ISR, GPIO_INT_EDGE_BOTH, RESISTOR_PULLUP );

            if(!CPU_GPIO_GetPinState( hw ))
            {
                Buttons_RegisterStateChange( Mapping->m_HAL, 0 );
            }
        }
    }

    return TRUE;
}

BOOL GPIO_BUTTON_Driver::Uninitialize()
{
    NATIVE_PROFILE_PAL_BUTTONS();
    GPIO_BUTTON_CONFIG* Config = &g_GPIO_BUTTON_Config;

    GPIO_HW_TO_HAL_MAPPING* Mapping    = &Config->Mapping[0                          ];
    GPIO_HW_TO_HAL_MAPPING* MappingEnd = &Config->Mapping[GPIO_BUTTON_CONFIG::c_COUNT];

    for(; Mapping < MappingEnd; Mapping++)
    {
        if(Mapping->m_HW != GPIO_PIN_NONE)
        {
            CPU_GPIO_EnableInputPin( Mapping->m_HW, FALSE, NULL, GPIO_INT_NONE, RESISTOR_PULLUP );
        }
    }

    return TRUE;
}

UINT32 GPIO_BUTTON_Driver::HW_To_Hal_Button( UINT32 HW_To_Hal_Button )
{
    NATIVE_PROFILE_PAL_BUTTONS();
    GPIO_BUTTON_CONFIG* Config = &g_GPIO_BUTTON_Config;

    GPIO_HW_TO_HAL_MAPPING* Mapping    = &Config->Mapping[0                          ];
    GPIO_HW_TO_HAL_MAPPING* MappingEnd = &Config->Mapping[GPIO_BUTTON_CONFIG::c_COUNT];

    for(; Mapping < MappingEnd; Mapping++)
    {
        if(Mapping->m_HW == HW_To_Hal_Button)
        {
            return Mapping->m_HAL;
        }
    }

    return 0;
}


BOOL GPIO_BUTTON_Driver::RegisterStateChange( UINT32 ButtonPressed, UINT32 ButtonReleased )
{
    NATIVE_PROFILE_PAL_BUTTONS();
    GLOBAL_LOCK(irq);

    // limit to legitimate transitions from previous state
    UINT32 TransitionDown = ButtonPressed  & ~g_GPIO_BUTTON_Driver.m_CurrentButtonState;
    UINT32 TransitionUp   = ButtonReleased &  g_GPIO_BUTTON_Driver.m_CurrentButtonState;

    // do we have any buttons remaining after debouncing?
    // also, only limit to changes to previous state
    if(TransitionDown | TransitionUp)
    {
        BUTTON_STATE_CHANGE* ptr = g_GPIO_BUTTON_Driver.m_ButtonFifo.Push();

        if(ptr)
        {
            ptr->Down = TransitionDown;
            ptr->Up   = TransitionUp;

            // update current state
            g_GPIO_BUTTON_Driver.m_CurrentButtonState |=  TransitionDown;
            g_GPIO_BUTTON_Driver.m_CurrentButtonState &= ~TransitionUp;
        }

        else
        {
            // if the queue is full, just return 
            lcd_printf  ( "\fBUTTON QUEUE FULL\r\n" );
            debug_printf( "BUTTON QUEUE FULL\r\n"   );

            // let's make sure the app knows that it is supposed to drain the queue
            Events_Set( SYSTEM_EVENT_FLAG_BUTTON ); 

            return FALSE;
        }

        // set the event for the waiter, as the queue may be full (how?)
        Events_Set( SYSTEM_EVENT_FLAG_BUTTON );
    }

#if defined(HAL_TIMEWARP)
    if(ButtonReleased)
    {
        switch(s_timewarp_armingState)
        {
        case 0:
        case 1:
            if(ButtonReleased & BUTTON_B0)
            {
                s_timewarp_armingState++;
            }
            else
            {
                s_timewarp_armingState = 0;
            }
            break;

        case 2:
            if(ButtonReleased & BUTTON_B0) // Backlight == DISABLE
            {
                s_timewarp_lastButton = TIMEWARP_DISABLE;
            }

            if(ButtonReleased & BUTTON_B4) // Select == ARM
            {
                s_timewarp_lastButton = Time_TicksToTime( Time_CurrentTicks() );
            }

            s_timewarp_armingState = 0;
            break;
        }
    }
#endif

    return TRUE;
}


BOOL GPIO_BUTTON_Driver::GetNextStateChange( UINT32& ButtonPressed, UINT32& ButtonReleased )
{
    NATIVE_PROFILE_PAL_BUTTONS();
    GLOBAL_LOCK(irq);

    BUTTON_STATE_CHANGE* ptr = g_GPIO_BUTTON_Driver.m_ButtonFifo.Pop();

    if(ptr)
    {
        ButtonPressed  = ptr->Down;
        ButtonReleased = ptr->Up;

        return TRUE;
    }

    return FALSE;
}

UINT32 GPIO_BUTTON_Driver::CurrentState()
{
    NATIVE_PROFILE_PAL_BUTTONS();
    return g_GPIO_BUTTON_Driver.m_CurrentButtonState;
}

UINT32 GPIO_BUTTON_Driver::CurrentHWState()
{
    NATIVE_PROFILE_PAL_BUTTONS();
    GPIO_BUTTON_CONFIG* Config      = &g_GPIO_BUTTON_Config;
    UINT32              HAL_Buttons = 0;

    // down = FALSE, up = TRUE, invert that notion here
    // do all buttons
    GPIO_HW_TO_HAL_MAPPING* Mapping    = &Config->Mapping[0                          ];
    GPIO_HW_TO_HAL_MAPPING* MappingEnd = &Config->Mapping[GPIO_BUTTON_CONFIG::c_COUNT];

    for(; Mapping < MappingEnd; Mapping++)
    {
        if(Mapping->m_HW != GPIO_PIN_NONE)
        {
            if(!CPU_GPIO_GetPinState( Mapping->m_HW ))
            {
                HAL_Buttons |= Mapping->m_HAL;
            }
        }
    }

    return HAL_Buttons;
}

//--//

void GPIO_BUTTON_Driver::ISR( GPIO_PIN Pin, BOOL PinState, void* Param )
{
    NATIVE_PROFILE_PAL_BUTTONS();
    UINT32 mask = Buttons_HW_To_Hal_Button( Pin );

    // down = 0, up = 1, invert that notion here
    if(PinState)   // released, up
    {
        Buttons_RegisterStateChange( 0, mask );
    }
    else            // pressed, down
    {
        Buttons_RegisterStateChange( mask, 0 );
    }
}
