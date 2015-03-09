////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//

#ifndef _LOG_H_
#define _LOG_H_ 1

enum LOG_STREAM   
{
    STREAM__NULL                  = 0x00, 
    STREAM__LCD                   = 0x01, 
    STREAM__DEFAULT_DEBUG_CHANNEL = 0x02, 
    STREAM__BLOCK_STORAGE         = 0x04, 
    STREAM__USB                   = 0x08, 
    STREAM__SERIAL                = 0x10, 
    STREAM__MEMORY                = 0x20 
};

//--//

typedef void (*StreamHandler)( char* message, char* format );

class Log
{

    //--//

    static const UINT32 c_LogBufferSize = 512;
    
    char          m_logBuffer[c_LogBufferSize];
    BOOL          m_result;
    UINT32        m_state;
    StreamHandler m_handler;

    //--//
    
    static Log s_Log;
    
    //--//
    
    char* ConvertResult( BOOL result );
    void  BeginTest    ( char* message );
    
    void static Output_NullStream               ( char* message, char* format );
    void static Output_LCDStream                ( char* message, char* format );
    void static Output_DefaultDebugChannelStream( char* message, char* format );
    void static Output_BlockStream              ( char* message, char* format );

public:
    
    static Log& InitializeLog( LOG_STREAM stream, char* message ); 

    //--//
    
    void CloseLog     ( BOOL result, char* message );
    void Comment      ( char* message );

    
    //
    // Use state variable to get,set, or increment a UINT32 counter
    //
    void   SetState( UINT32 );
    UINT32 GetState();
    void   IncrementState();
    void   LogState( char *);
    
};

#endif  //_LOG_H_ 

