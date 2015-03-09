////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Timers.h"

//---//

Timers::Timers( UINT32 DisplayIntervalSeconds, UINT32 TimerDurationSeconds )
{
    m_displayInterval = DisplayIntervalSeconds;
    m_timerDuration   = TimerDurationSeconds;
    thorp[0]          = '-';
    thorp[1]          = '/';
    thorp[2]          = '|';
    thorp[3]          = '\\';
};

BOOL Timers::Execute( LOG_STREAM Stream )
{

    Log& log = Log::InitializeLog( Stream, "Timers" );

    lcd_printf( "\r\n" );

    hal_printf( "\r\n" );

    for(UINT32 seconds=0; seconds<m_timerDuration; seconds++)
    {

        lcd_printf( "\r        %c",thorp[seconds % 4] );

        hal_printf( "\r        %c",thorp[seconds % 4] );

        if((seconds % m_displayInterval) == 0)
        {
            lcd_printf( "        %2d",seconds );

            hal_printf( "        %2d",seconds );
        }

        Events_WaitForEvents ( 0, 1000 );
    }
    lcd_printf( "\r\n" );

    hal_printf( "\r\n" );

    return TRUE;
}; //Execute


//--//

