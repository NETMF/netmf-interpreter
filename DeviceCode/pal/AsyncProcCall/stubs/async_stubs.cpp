////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
//--//
// continuation list
void HAL_CONTINUATION::InitializeList()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
}

void HAL_CONTINUATION::Uninitialize()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
}

void HAL_CONTINUATION::InitializeCallback( HAL_CALLBACK_FPN EntryPoint, void* Argument )
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
}

void HAL_CONTINUATION::Enqueue()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
}

void HAL_CONTINUATION::Abort  ()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
}

BOOL HAL_CONTINUATION::Dequeue_And_Execute()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
    return TRUE;
}

bool HAL_CONTINUATION::IsLinked()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
    return false;
}

//-//
// completion list
void HAL_COMPLETION::InitializeList()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
}

void HAL_COMPLETION::Uninitialize()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
}
void HAL_COMPLETION::EnqueueTicks(UINT64 EventTimeTicks)
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
}

void HAL_COMPLETION::EnqueueDelta( UINT32 uSecFromNow )
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
}

void HAL_COMPLETION::Abort()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
}

void HAL_COMPLETION::Execute()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
}

void HAL_COMPLETION::DequeueAndExec()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
}

void HAL_COMPLETION::WaitForInterrupts( UINT64 Expire, UINT32 sleepLevel, UINT64 wakeEvents )
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
}
