////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _GPIO_BUTTON_H_
#define _GPIO_BUTTON_H_ 1

//--//

struct GPIO_BUTTON_Driver
{
    static const int c_MAX_FIFO_ENTRIES = 64;

    struct BUTTON_STATE_CHANGE
    {
        UINT32  Down;
        UINT32  Up;
        //UINT64  TimeStamp;
    };

    Hal_Queue_KnownSize< BUTTON_STATE_CHANGE, c_MAX_FIFO_ENTRIES > m_ButtonFifo;
    UINT32                                                         m_CurrentButtonState;    // indicate initial state is all buttons are up

    //--//

    static BOOL Initialize();

    static BOOL Uninitialize();

    static BOOL RegisterStateChange( UINT32  ButtonsPressed, UINT32  ButtonsReleased );
    static BOOL GetNextStateChange ( UINT32& ButtonsPressed, UINT32& ButtonsReleased );

    static UINT32 CurrentState();

    static UINT32 HW_To_Hal_Button( UINT32 HW_Buttons );

    static UINT32 CurrentHWState();

private:
    static void ISR( GPIO_PIN Pin, BOOL PinState, void* Param );
};

extern GPIO_BUTTON_Driver g_GPIO_BUTTON_Driver;

//--//

#endif  //_GPIO_BUTTON_H_
