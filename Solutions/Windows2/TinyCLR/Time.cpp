////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

using namespace Microsoft::SPOT::Emulator;

void HAL_Time_Sleep_MicroSeconds( UINT32 uSec )
{
    return EmulatorNative::GetITimeDriver()->Sleep_MicroSeconds( uSec );    
}

void HAL_Time_Sleep_MicroSeconds_InterruptEnabled( UINT32 uSec )
{
    return EmulatorNative::GetITimeDriver()->Sleep_MicroSecondsInterruptsEnabled( uSec );    
}

UINT64 HAL_Time_CurrentTicks()
{
    return EmulatorNative::GetITimeDriver()->CurrentTicks();
}

INT64 HAL_Time_TicksToTime( UINT64 Ticks )
{
    _ASSERTE(Ticks <= 0x7FFFFFFFFFFFFFFF);
    
    //No need to go to managed code just to return Time.  

    return Ticks;
}
       
INT64 HAL_Time_CurrentTime()
{
    return EmulatorNative::GetITimeDriver()->CurrentTime();
}

void HAL_Time_GetDriftParameters  ( INT32* a, INT32* b, INT64* c )
{
    *a = 1;
    *b = 1;
    *c = 0;
}

UINT32 CPU_SystemClock()
{
    return EmulatorNative::GetITimeDriver()->SystemClock;    
}

UINT32 CPU_TicksPerSecond()
{
    return EmulatorNative::GetITimeDriver()->TicksPerSecond;
}

//Completions

void HAL_COMPLETION::EnqueueDelta( UINT32 uSecFromNow )
{
    EmulatorNative::GetITimeDriver()->EnqueueCompletion( (IntPtr)this, uSecFromNow ); 
}

void HAL_COMPLETION::EnqueueTicks( UINT64 EventTimeTicks )
{
    _ASSERTE(FALSE);
}

void HAL_COMPLETION::Abort()
{
    EmulatorNative::GetITimeDriver()->AbortCompletion( (IntPtr)this );
}

void HAL_COMPLETION::Execute()
{
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

//Continuations

bool HAL_CONTINUATION::IsLinked()
{
    return EmulatorNative::GetITimeDriver()->IsLinked( (IntPtr)this );
}

BOOL HAL_CONTINUATION::Dequeue_And_Execute()
{
    return EmulatorNative::GetITimeDriver()->DequeueAndExecuteContinuation();
}

void HAL_CONTINUATION::InitializeCallback( HAL_CALLBACK_FPN EntryPoint, void* Argument )
{
    Initialize();

    Callback.Initialize( EntryPoint, Argument );
}

void HAL_CONTINUATION::Enqueue()
{    
    _ASSERTE(this->GetEntryPoint() != NULL);
    
    EmulatorNative::GetITimeDriver()->EnqueueContinuation( (IntPtr) this );        
}

void HAL_CONTINUATION::Abort()
{
    EmulatorNative::GetITimeDriver()->AbortContinuation( (IntPtr) this );
}

//various

void CLR_RT_EmulatorHooks::Notify_ExecutionStateChanged()
{    
    EmulatorNative::GetITimeDriver()->IsExecutionPaused = ClrIsDebuggerStopped();
}

