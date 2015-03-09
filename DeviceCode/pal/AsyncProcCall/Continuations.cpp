////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

/***************************************************************************/

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_HAL_Continuation_List"
#endif

HAL_DblLinkedList<HAL_CONTINUATION> g_HAL_Continuation_List;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

extern HAL_DblLinkedList<HAL_CONTINUATION> g_HAL_Completion_List;

//--//

bool HAL_CONTINUATION::IsLinked()
{
    return ((HAL_DblLinkedNode<HAL_CONTINUATION>*)this)->IsLinked();
}

void HAL_CONTINUATION::InitializeList()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
    g_HAL_Continuation_List.Initialize();
}

void HAL_CONTINUATION::Enqueue()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
    if(this->GetEntryPoint() != NULL)
    {
        GLOBAL_LOCK(irq);

        g_HAL_Continuation_List.LinkAtBack( this );
    }
}

void HAL_CONTINUATION::Abort()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
    GLOBAL_LOCK(irq);

    this->Unlink();
}

BOOL HAL_CONTINUATION::Dequeue_And_Execute()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
    GLOBAL_LOCK(irq);
    
    HAL_CONTINUATION* ptr = g_HAL_Continuation_List.ExtractFirstNode();
    if(ptr == NULL )
        return FALSE;
    
    //SystemState_SetNoLock( SYSTEM_STATE_NO_CONTINUATIONS );

    HAL_CALLBACK call = ptr->Callback;

    irq.Release();
    call.Execute();
    irq.Acquire();

    //SystemState_ClearNoLock( SYSTEM_STATE_NO_CONTINUATIONS );   // nestable

    return TRUE;
}

void HAL_CONTINUATION::InitializeCallback( HAL_CALLBACK_FPN EntryPoint, void* Argument )
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
    Initialize();
    
    Callback.Initialize( EntryPoint, Argument );
}

void HAL_CONTINUATION::Uninitialize()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
    GLOBAL_LOCK(irq);
    
    HAL_CONTINUATION* ptr;

    while(TRUE)
    {
        ptr = (HAL_CONTINUATION*)g_HAL_Continuation_List.ExtractFirstNode();
        
        if(!ptr)
        {
            break;
        }
    }
}


