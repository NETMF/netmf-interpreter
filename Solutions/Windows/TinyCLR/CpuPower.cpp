//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"

void CPU_Reset()
{
    ::ExitProcess(0);
}

typedef std::unique_lock< std::recursive_mutex > ScopedLock;

// The pending state is used to implement CPU_Sleep() which will
// block until an interrupt event occurs. When __notify_pendingIRQ()
// is called it will set the pending flag and notify one thread
// (Which should only be the CLR thread - DO NOT CALL CPU_SLEEP
// from any background threads!)
static std::recursive_mutex IrqPendingMutex;
static std::condition_variable_any IrqPendingConditionVar;
static bool IrqPendingState = false;

extern bool OnClrThread();

void CPU_Sleep( SLEEP_LEVEL level, UINT64 wakeEvents )
{
    ASSERT( OnClrThread() );
    ASSERT_IRQ_MUST_BE_OFF();
    ENABLE_INTERRUPTS();
    {
        // Take hold of the mutex to prevent other threads from
        // changing state - auto releases on scope exit (RAII)
        ScopedLock lock( IrqPendingMutex );

        // if no events are already pending - use condition var to atomically
        // release, wait and reacquire mutex when an int is pending
        if( !IrqPendingState )
        {   
            IrqPendingConditionVar.wait( IrqPendingMutex
                                       , [ ]( )
                                         { 
                                             return IrqPendingState;
                                         }
                                       );
        }

        // clear the pending state while the mutex is owned
        IrqPendingState = false;
    }
    DISABLE_INTERRUPTS();
}

// this will notify the CLR thread (via CPU_Sleep() ) when
// an interrupt event has occured and the CLR thread should
// wake up to process any required changes in state.
void __notify_pendingIRQ( )
{
    bool oldState;
    {
        ScopedLock lock( IrqPendingMutex );
        oldState = IrqPendingState;        
        IrqPendingState = true;
    }

    if( !oldState )
        IrqPendingConditionVar.notify_one( );
}

static class EventsCallbackHandler
{
public:
    EventsCallbackHandler()
    {
        // hook up callback so that whenever the
        // Events_Set is called to change the events state
        // a pending IRQ is triggered to wake up CPU_Sleep()
        Events_SetCallback( Callback, nullptr );
    }

    static void Callback(void* /*arg*/)
    {
        __notify_pendingIRQ();
    }
} GlobalEventsCallbackHandler;
