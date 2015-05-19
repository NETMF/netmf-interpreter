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

static HAL_CALLBACK_FPN          g_ostask_completed   = NULL;
static BOOL                      g_ostask_initialized = FALSE;
static HAL_DblLinkedList<OSTASK> g_ostask_list; 
CRITICAL_SECTION                 g_ostask_lock; 

//--//

BOOL OSTASK_Initialize( HAL_CALLBACK_FPN completed )
{
    //
    // If one is using a thread pool, it would be initialized here... 
    //
    // 

    //
    // initialize the global task list
    //
    g_ostask_list.Initialize();
    
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

    // 
    // Empty the list of tasks
    // 
    OSTASK* ptr = NULL;
    while(TRUE)
    {
        // this will unlink from the list, so no notification for completed will be posted
        ptr = (OSTASK*)g_ostask_list.ExtractFirstNode();
        
        if(!ptr)
        {
            break;
        }
    }
    
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
        // Add task to list
        //
        g_ostask_list.LinkAtBack( task );
        
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

        LeaveCriticalSection( &g_ostask_lock );

        return TRUE;
    }
    
    LeaveCriticalSection( &g_ostask_lock );
    
    return FALSE;    
}


BOOL OSTASK_Cancel( OSTASK* task )
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();

    EnterCriticalSection( &g_ostask_lock );
    
    if(g_ostask_initialized == FALSE)
    {
        LeaveCriticalSection( &g_ostask_lock );
        
        return FALSE;
    }
    
    //
    // Tell the executing thread the task is cancelled, if needed
    // 
    // ...

    //
    // Remove task from list, it may have been removed already
    //
    if( task->IsLinked() ) 
    {
        task->Unlink();
    }
    
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

    //
    // Cancelled tasks are not linked...
    // 
    if(task->IsLinked()) 
    {
        task->SetCompleted();

        if(g_ostask_completed != NULL) 
        {
            g_ostask_completed( task );
        }  

        task->Unlink();
    }

    // 
    // release memory
    // 
    if( task->GetArgument() ) 
    {
        private_free( task->GetArgument() ); 
    }
    private_free( task );
    
    LeaveCriticalSection( &g_ostask_lock );
}

