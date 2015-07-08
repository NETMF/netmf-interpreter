////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"
#include <cmsis_os_cpp.h>

/***************************************************************************/

//--//

static HAL_CALLBACK_FPN g_ostask_completed   = NULL;
static BOOL             g_ostask_initialized = FALSE;

osThreadId  g_threadId = NULL;
void *      g_threadPool = NULL;
osMutexId   g_ostask_lock;

osMutexDef(g_ostask_lock);

#define OSTASK_THREAD_COUNT     6

osPoolDef( ostask_pool_thrd, OSTASK_THREAD_COUNT, osThreadDef_t );
osPoolId    ostask_pool_thrd;

void ThreadProc(void * lpdwThreadParam ) 
{
    OSTASK* task = (OSTASK*)lpdwThreadParam;

    if(task != NULL) 
    {
        task->Execute();
        OSTASK_SignalCompleted( task );
    }
    return;
}


BOOL OSTASK_Initialize( HAL_CALLBACK_FPN completed )
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();

    //
    // initialize the global task list
    //
    g_ostask_lock = osMutexCreate(osMutex(g_ostask_lock));
    ostask_pool_thrd = osPoolCreate( osPool( ostask_pool_thrd ) );
    g_ostask_completed   = completed;
    g_ostask_initialized = TRUE;
}


void OSTASK_Uninitialize()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
    
    osMutexWait(g_ostask_lock, osWaitForever);
    OSTASK* ptr = NULL;
    if (g_threadPool!=NULL)
        osPoolFree(ostask_pool_thrd,g_threadPool);    
    g_ostask_initialized = FALSE;
    osMutexRelease(g_ostask_lock);
}


BOOL OSTASK_Post( OSTASK* task )
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
    
    BOOL state = FALSE;

    if(g_ostask_initialized == FALSE)
    {
        return FALSE;
    }

    osMutexWait(g_ostask_lock, osWaitForever);
    
    if(task->GetEntryPoint() != NULL) 
    {
    
        //
        // Create the executing thread
        // 
        
        osThreadDef_t *def;

        if (g_threadPool == NULL)
            g_threadPool = osPoolAlloc( ostask_pool_thrd );
        
        def = (osThreadDef_t *)g_threadPool ;
        if( def == NULL )
        {
            osMutexRelease(g_ostask_lock);
            return FALSE;
        }

        def->pthread = ( os_pthread )ThreadProc;
        def->tpriority = osPriorityNormal;
        def->stacksize = 0;
        def->instances = 1; 

        g_threadId= osThreadCreate( def, (void *)task);
        if (g_threadId == NULL)
        {
            debug_printf(" failed to create thread \r\n");
            osMutexRelease(g_ostask_lock);
            return FALSE;
        }
        
        Events_Clear( SYSTEM_EVENT_FLAG_OSTASK );

        osMutexRelease(g_ostask_lock);
        return TRUE;
    }

    osMutexRelease(g_ostask_lock);    
    return FALSE;    
}


BOOL OSTASK_Cancel( OSTASK* task )
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
   
    if(g_ostask_initialized == FALSE)
    {
        return FALSE;
    }

    osMutexWait(g_ostask_lock, osWaitForever); 

    // as task was not ended by itself, we have to 
    // force ending it! 
    if (g_threadId !=NULL)
        osThreadTerminate (g_threadId);

    if (g_threadPool!=NULL)
        osPoolFree(ostask_pool_thrd,g_threadPool);

    osMutexRelease(g_ostask_lock);
    return TRUE;    
}

void OSTASK_SignalCompleted( OSTASK* task )
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();

    if(g_ostask_initialized == FALSE)
    {
        return;
    }

    osMutexWait(g_ostask_lock, osWaitForever);     
    task->SetCompleted();
    Events_Set( SYSTEM_EVENT_FLAG_OSTASK );
    
    if(g_ostask_completed != NULL) 
    {
        g_ostask_completed( task );
    }  
/*    if (g_threadPool!=NULL)
        osPoolFree(ostask_pool_thrd,g_threadPool);    
*/
    osMutexRelease(g_ostask_lock);
}

