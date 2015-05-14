////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

/***************************************************************************/

//--//

static HAL_CALLBACK_FPN g_ostask_completed   = NULL;
static BOOL             g_ostask_initialized = FALSE;


BOOL OSTASK_Initialize( HAL_CALLBACK_FPN completed )
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();

    //
    // If one is using a thread pool, it woudl be initialized here... 
    //
    // 

    g_ostask_completed   = completed;
    g_ostask_initialized = TRUE;
}

void OSTASK_Uninitialize()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();

    //
    // If one is using a thread pool, it woudl be destroyed here... 
    //
    // ...
    
    g_ostask_initialized = TRUE;
}

BOOL OSTASK_Post( OSTASK* task )
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();

    
    if(g_ostask_initialized == FALSE)
    {
        return FALSE;
    }
    
    if(task->GetEntryPoint() != NULL)
    {
        //
        // Create the executing thread
        // 
        
    }
    
    return TRUE;    
}

void OSTASK_SignalCompleted( OSTASK* task )
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();

    
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

