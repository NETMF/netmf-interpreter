////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"
#include "ostask_decl.h"

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

static HAL_CALLBACK_FPN          g_ostask_completed   = NULL;
static BOOL                      g_ostask_initialized = FALSE;
static HANDLE                    g_Hthread;
CRITICAL_SECTION                 g_ostask_lock; 

//--//

BOOL OSTASK_Initialize( HAL_CALLBACK_FPN completed )
{
    //
    // If one is using a thread pool, it would be initialized here... 
    //
    // 
  
    InitializeCriticalSection( &g_ostask_lock );

    g_ostask_completed   = completed;
    g_ostask_initialized = TRUE;

    return TRUE;
}

void OSTASK_Uninitialize()
{
    EnterCriticalSection( &g_ostask_lock );
    
    //
    // If one is using a thread pool, it would be destroyed here... 
    //
    // ...

    g_ostask_initialized = FALSE;

    LeaveCriticalSection( &g_ostask_lock );
    
    DeleteCriticalSection( &g_ostask_lock );
}

BOOL OSTASK_Post( OSTASK* task )
{
    EnterCriticalSection( &g_ostask_lock );
    
    if(g_ostask_initialized == FALSE)
    {
        LeaveCriticalSection( &g_ostask_lock );
        
        return FALSE;
    }
    
    if(task->GetEntryPoint() != NULL)
    {
        
        //
        // Create the executing thread
        // 

        DWORD dwThreadId = 0;
        
        g_Hthread = CreateThread(   NULL,                        // Choose default security
                        0,                                      // Default stack size
                        (LPTHREAD_START_ROUTINE)&ThreadProc,    // Routine to execute                                                        
                        (LPVOID) task,                          // Thread parameter
                        0,                                      // Immediately run the thread
                        &dwThreadId                             // Thread Id 
                    );
        LeaveCriticalSection( &g_ostask_lock );

        return TRUE;
    }
    
    LeaveCriticalSection( &g_ostask_lock );
    
    return FALSE;    
}

// OSTASK_Cancel is called when the Task is run away that it is 
// completely lost or loop forever in something that it is timeout
// from itself, so we need to forcefully to kill the thread ungracefully
BOOL OSTASK_Cancel( OSTASK* task )
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();

    EnterCriticalSection( &g_ostask_lock );
    
    if(g_ostask_initialized == FALSE)
    {
        LeaveCriticalSection( &g_ostask_lock );
        
        return FALSE;
    }
    
    TerminateThread(  g_Hthread, 0);
    CloseHandle( g_Hthread );
    LeaveCriticalSection( &g_ostask_lock );

    return TRUE;    
}

void OSTASK_SignalCompleted( OSTASK* task )
{   
    EnterCriticalSection( &g_ostask_lock );
        
    if(g_ostask_initialized == FALSE)
    {
        // 
        // Shutting down...
        //

        LeaveCriticalSection( &g_ostask_lock );
        
        return;
    }

    task->SetCompleted();
    if(g_ostask_completed != NULL) 
    {
        g_ostask_completed( task );
    }  
    Events_Set(SYSTEM_EVENT_FLAG_OSTASK);
    //
    // Let the creator of the task to release memory
    //
    // ...
    
    LeaveCriticalSection( &g_ostask_lock );
}

