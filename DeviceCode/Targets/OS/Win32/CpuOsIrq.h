///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <OsIrqLock.h>

namespace
{
    class CriticalSection
    {
    public:
        CriticalSection( )
        {
            ::InitializeCriticalSection( &CritSect );
        }

        void lock( )
        {
            ::EnterCriticalSection( &CritSect );
            Owner = ::GetCurrentThreadId();
        }

        void unlock( )
        {
            ASSERT( owned() );
            if( !owned() )
                return;

            Owner = UINT32_MAX;
            ::LeaveCriticalSection( &CritSect );
        }

        bool owned()  { return ::GetCurrentThreadId() == Owner; }

        static bool InIsr()
        {
            return false;
        }

    private:
        DWORD Owner;
        CRITICAL_SECTION CritSect;
    };
}

// Mutex, and state flag used to indicate global IRQ enable state.
// The CLR thread will block on this as it is used to simulate
// disabling/enabling interrupts in the CPU as well as when an ISR
// runs. Background threads in the OS essentially simulate real world 
// external activity and interrupts to the CPU, so they must block
// on this mutex before they can modify CLR global data structs etc...
typedef MsOpenTech::NETMF::OsIrqLock< CriticalSection > Lock_t;
static Lock_t IrqsEnabledMutex;

// IRQ state modeling how most MCUs use bit(s) in a global status register
// this helps make the code easier to port and also allows for simple
// testing of the current state.
static uint32_t IrqsEnabledState = 0;
const uint32_t IrqsDisabledFlag = 1;

inline bool IrqsDisabled( uint32_t state )
{
    return ( state & IrqsDisabledFlag ) == IrqsDisabledFlag;
}

inline bool IrqsEnabled( uint32_t state )
{
    return ( state & IrqsDisabledFlag ) == 0;
}

// internal function to simulate common interrupt enable state query for embedded MCUs
inline bool irqs_enabled( )
{
    return IrqsEnabled( IrqsEnabledState );
}

// internal function to simulate common interrupt enable intrinsics for embedded MCUs
uint32_t enable_irqs( )
{
    // capture and update current state with lock held
    uint32_t retVal = IrqsEnabledState;
    IrqsEnabledState &= ~IrqsDisabledFlag;
    IrqsEnabledMutex.unlock( );
    return retVal;
}

// internal function to simulate common interrupt disable intrinsics for embedded MCUs
uint32_t disable_irqs( )
{
    IrqsEnabledMutex.lock( );
    // capture and update current state with lock held
    uint32_t retVal = IrqsEnabledState;
    IrqsEnabledState |= IrqsDisabledFlag;
    return retVal;
}

// allow an "interrupt" to occur
inline void irq_yield()
{
    //__NOP();
    if( !::SwitchToThread( ) )
        ::Sleep( 0 );
}
