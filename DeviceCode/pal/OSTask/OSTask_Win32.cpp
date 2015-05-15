////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

/***************************************************************************/

//--//

DWORD ThreadProc(LPVOID lpdwThreadParam ) 
{
    OSTASK* task = (OSTASK*)lpdwThreadParam;

    if(task != NULL) 
    {
        task->Execute();

        OSTASK_SignalCompleted( task );
    }

    return 0;
}

//--//

static HAL_CALLBACK_FPN g_ostask_completed   = NULL;
static BOOL             g_ostask_initialized = FALSE;

//--//

BOOL OSTASK_Initialize( HAL_CALLBACK_FPN completed )
{
    //
    // If one is using a thread pool, it would be initialized here... 
    //
    // 

    g_ostask_completed   = completed;
    g_ostask_initialized = TRUE;

    return TRUE;
}

void OSTASK_Uninitialize()
{
    //
    // If one is using a thread pool, it would be destroyed here... 
    //
    // ...
    
    g_ostask_initialized = TRUE;
}

BOOL OSTASK_Post( OSTASK* task )
{
    if(g_ostask_initialized == FALSE)
    {
        return FALSE;
    }
    
    if(task->GetEntryPoint() != NULL)
    {
        //
        // Create the executing thread
        // 

        DWORD dwThreadId = 0;
        
        HANDLE h = CreateThread(   NULL,                                   // Choose default security
                        0,                                      // Default stack size
                        (LPTHREAD_START_ROUTINE)&ThreadProc,    // Routine to execute                                                        
                        (LPVOID) task,                          // Thread parameter
                        0,                                      // Immediately run the thread
                        &dwThreadId                             // Thread Id 
                    );

        CloseHandle( h );

        return TRUE;
    }
    
    return FALSE;    
}

void OSTASK_SignalCompleted( OSTASK* task )
{   
    if(g_ostask_initialized == FALSE)
    {
        // 
        // Shutting down...
        //
        return;
    }

    task->SetCompleted();


    if(g_ostask_completed != NULL) 
    {
        g_ostask_completed( task );
    }
}

