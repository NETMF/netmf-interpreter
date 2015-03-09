////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"

/***************************************************************************/

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_HAL_Completion_List"
#endif

HAL_DblLinkedList<HAL_CONTINUATION> g_HAL_Completion_List;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

/***************************************************************************/

void HAL_COMPLETION::Execute()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
    ASSERT_IRQ_MUST_BE_OFF();

#if defined(_DEBUG)
    this->Start_RTC_Ticks = 0;
#endif

    if(this->ExecuteInISR)
    {
        HAL_CONTINUATION* cont = this;

        cont->Execute();
    }
    else
    {
        this->Enqueue();
    }
}

//--//

static const UINT64 HAL_Completion_IdleValue = 0x0000FFFFFFFFFFFFull;

void HAL_COMPLETION::InitializeList()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
    g_HAL_Completion_List.Initialize();
}

//--//

void HAL_COMPLETION::DequeueAndExec()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
    GLOBAL_LOCK(irq);

    HAL_COMPLETION* ptr     = (HAL_COMPLETION*)g_HAL_Completion_List.FirstNode();
    HAL_COMPLETION* ptrNext = (HAL_COMPLETION*)ptr->Next();

    // waitforevents does not have an associated completion, therefore we need to verify
    // than their is a next completion and that the current one has expired.
    if(ptrNext)
    {
        Events_Set(SYSTEM_EVENT_FLAG_SYSTEM_TIMER);

        ptr->Unlink();

        //
        // In case there's no other request to serve, set the next interrupt to be 356 years since last powerup (@25kHz).
        //
        HAL_Time_SetCompare( ptrNext->Next() ? ptrNext->EventTimeTicks : HAL_Completion_IdleValue );

#if defined(_DEBUG)
        ptr->EventTimeTicks = 0;
#endif  // defined(_DEBUG)

        // let the ISR turn on interrupts, if it needs to
        ptr->Execute();
    }
}

//--//

void HAL_COMPLETION::EnqueueTicks( UINT64 EventTimeTicks )
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
    ASSERT(EventTimeTicks != 0);

    GLOBAL_LOCK(irq);

    this->EventTimeTicks  = EventTimeTicks;
#if defined(_DEBUG)
    this->Start_RTC_Ticks = HAL_Time_CurrentTicks();
#endif

    HAL_COMPLETION* ptr     = (HAL_COMPLETION*)g_HAL_Completion_List.FirstNode();
    HAL_COMPLETION* ptrNext;

    for(;(ptrNext = (HAL_COMPLETION*)ptr->Next()); ptr = ptrNext)
    {
        if(EventTimeTicks < ptr->EventTimeTicks) break;
    }

    g_HAL_Completion_List.InsertBeforeNode( ptr, this );
    
    if(this == g_HAL_Completion_List.FirstNode())
    {
        HAL_Time_SetCompare( EventTimeTicks );
    }
}

void HAL_COMPLETION::EnqueueDelta64( UINT64 uSecFromNow )
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
    // grab time first to be closest to now as possible from when this function was called
    UINT64 Now            = HAL_Time_CurrentTicks();
    UINT64 EventTimeTicks = CPU_MicrosecondsToTicks( uSecFromNow );

    EnqueueTicks( Now + EventTimeTicks );
}

void HAL_COMPLETION::EnqueueDelta( UINT32 uSecFromNow )
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
    EnqueueDelta64( (UINT64)uSecFromNow );
}

//--//

void HAL_COMPLETION::Abort()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
    GLOBAL_LOCK(irq);

    HAL_COMPLETION* firstNode = (HAL_COMPLETION*)g_HAL_Completion_List.FirstNode();

    this->Unlink();

#if defined(_DEBUG)
    this->Start_RTC_Ticks = 0;
#endif

    if(firstNode == this)
    {
        UINT64 nextTicks;

        if(g_HAL_Completion_List.IsEmpty())
        {
            //
            // In case there's no other request to serve, set the next interrupt to be 356 years since last powerup (@25kHz).
            //
            nextTicks = HAL_Completion_IdleValue;
        }
        else
        {
            firstNode = (HAL_COMPLETION*)g_HAL_Completion_List.FirstNode();

            nextTicks = firstNode->EventTimeTicks;
        }

        HAL_Time_SetCompare( nextTicks );
    }
}

//--//

void HAL_COMPLETION::WaitForInterrupts( UINT64 Expire, UINT32 sleepLevel, UINT64 wakeEvents )
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
    const int c_SetCompare   = 1;
    const int c_ResetCompare = 2;
    const int c_NilCompare   = 4;

    ASSERT_IRQ_MUST_BE_OFF();

    HAL_COMPLETION* ptr = (HAL_COMPLETION*)g_HAL_Completion_List.FirstNode();
    int             state;

    if(ptr->Next() == NULL)
    {
        state = c_SetCompare | c_NilCompare;
    }
    else if(ptr->EventTimeTicks > Expire)
    {
        state = c_SetCompare | c_ResetCompare;
    }
    else
    {
        state = 0;
    }

    if(state & c_SetCompare) HAL_Time_SetCompare( Expire );

    CPU_Sleep( (SLEEP_LEVEL)sleepLevel, wakeEvents );

    if(state & (c_ResetCompare | c_NilCompare))
    {   
        // let's get the first node again
        // it could have changed since CPU_Sleep re-enabled interrupts
        ptr = (HAL_COMPLETION*)g_HAL_Completion_List.FirstNode();
        HAL_Time_SetCompare( (state & c_ResetCompare) ? ptr->EventTimeTicks : HAL_Completion_IdleValue );
    }
}

void HAL_COMPLETION::Uninitialize()
{
    NATIVE_PROFILE_PAL_ASYNC_PROC_CALL();
    GLOBAL_LOCK(irq);
    
    HAL_COMPLETION* ptr;

    while(TRUE)
    {
        ptr = (HAL_COMPLETION*)g_HAL_Completion_List.ExtractFirstNode();

        if(!ptr) 
        {
            break;
        }
        
    }
}

