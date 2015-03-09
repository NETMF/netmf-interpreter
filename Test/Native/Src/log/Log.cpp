////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Log.h"

//--//

Log Log::s_Log; 

//--//

Log& Log::InitializeLog( LOG_STREAM StreamPath, char* message )
{    
    s_Log.m_result       = false;
    s_Log.m_state        = 0;
    s_Log.m_logBuffer[0] = (char) '\0';
    
    switch(StreamPath)
    {
        case STREAM__LCD:
        {
            s_Log.m_handler = Log::Output_LCDStream;
            break;
        }
        case STREAM__DEFAULT_DEBUG_CHANNEL:
        {
             s_Log.m_handler = Log::Output_DefaultDebugChannelStream;
            break;
        }
        case STREAM__BLOCK_STORAGE:
        {
            s_Log.m_handler = Log::Output_BlockStream;
            break;
        }
        case STREAM__NULL  :
        case STREAM__USB   :
        case STREAM__SERIAL:
        case STREAM__MEMORY:
        default:
        {
            s_Log.m_handler = Log::Output_NullStream;
        }
    }

    s_Log.BeginTest( message );
    
    return s_Log;
}

void Log::BeginTest( char* message )
{
    char* fmt = "\r\nTEST: %s,";
    
    this->m_handler( message, fmt );
}

void Log::CloseLog( BOOL result, char* message )
{
    char buffer[40];
    
    char* result_s = ConvertResult( result );

    if(message)
    {
        hal_snprintf( buffer, sizeof(buffer), "%12s, %4s", message, result_s );        
    }
    else
    {
        hal_snprintf( buffer, sizeof(buffer), "%12s, %4s", m_logBuffer, result_s );
    }
    
    m_handler( buffer, NULL );
}

void Log::SetState(UINT32 value)
{
    m_state = value;
}

UINT32 Log::GetState()
{
    return m_state;
}

void Log::IncrementState()
{
   m_state += 1;
}

void Log::LogState( char* messageformat )
{
    hal_snprintf( m_logBuffer, sizeof(m_logBuffer), messageformat, m_state);
}

///////////////////////////////////////////////////////////////////////////////
//
// null LOG support
//
void Log::Output_NullStream( char* message, char* format )
{
}

///////////////////////////////////////////////////////////////////////////////
//
// LCD LOG support
//
void Log::Output_LCDStream( char* message, char* format )
{    
    if(!format) 
    {
        // LCD specific format is just a simple string with no carriage return
        format = "%s\r\n";
    }
    
    lcd_printf( format, message );
}

///////////////////////////////////////////////////////////////////////////////
//
// default debug channel LOG support
//
void Log::Output_DefaultDebugChannelStream( char* message, char* format )
{
    if(!format) 
    {
        // debug channel specific format is just a simple string with a carriage return
        format = "%s\r\n";
    }
    
    hal_printf( format, message );
}

///////////////////////////////////////////////////////////////////////////////
//
// Block Stream LOG support
//
// First implementation using blockstorage logging
// 1. Identify deployment region (size and starting sectoraddress)
// 2. write log once in sector units, when full, stop.
//
// Improve behavior with requirements from users
//
//
void Log::Output_BlockStream( char* message, char* format )
{   
    if(!format) 
    {
        // debug channel specific format is just a simple string with a carriage return
        format = "%s\r\n";
    }
    
    // TODO locate and call block storage driver 
}

//--//

char* Log::ConvertResult(BOOL status)
{
    if(status)
    {
        return "Pass";
    }
    else
    {
        return "Fail";
    }
}

