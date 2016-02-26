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
#include <cstdint>
#include <OSIrqLock.h>
#include <CpuOsIrq.h>

using namespace std;
using namespace MsOpenTech::NETMF;


// disables interrupts if not already disabled
// saves current interrupt state so it can be
// restored in the destructor.
SmartPtr_IRQ::SmartPtr_IRQ( void* context )
{
    m_context = context;
    Disable( );
}

// restores the previous interrupt state
SmartPtr_IRQ::~SmartPtr_IRQ( )
{
    Restore( );
}

// determines if the interrupts were disabled before 
// this instance was created.
BOOL SmartPtr_IRQ::WasDisabled( )
{
    return IrqsDisabled( m_state );
}

// Disables interrupts if they were not already disabled
// and stores the previous state for restoration in
// destructor or  Release()
void SmartPtr_IRQ::Acquire( )
{
    if( IrqsDisabled( m_state ) )
        Disable( );
}

// restores the IRQ enabled state to the previous value
// Like Restore() except this updates the internally saved
// state to the current state (before enabling) - use carefully!
void SmartPtr_IRQ::Release( )
{
    if( IrqsEnabled( m_state ) )
        m_state = enable_irqs( );
}

// probe is basically a release and re-acquire to allow interrupts
// to occur. This is a design smell in the design and use of the
// "SmartPtr_IRQ" class, as it means that the interrutps are being
// held off for significantly longer stretches than they should be
// which causes troubles with overall responsiveness and simplicity
// of code. There are usually always better ways to deal with this
// sort of thing then disabling interrupts for extended periods...
// This is a prime target for re-work in a future release.
void SmartPtr_IRQ::Probe( )
{
    if( IrqsEnabled( m_state ) )
    {
        enable_irqs( );

        // allow an "interupt" to occur
        irq_yield();

        // restore irq state
        disable_irqs( );
    }
}

// returns non-zero value if interrupts are currently enabled; otherwise, 0 (FALSE).
BOOL SmartPtr_IRQ::GetState( void* context )
{
    return irqs_enabled( );
}

BOOL SmartPtr_IRQ::ForceDisabled( void* context )
{
    disable_irqs( );
    return true;
}

BOOL SmartPtr_IRQ::ForceEnabled( void* context )
{
    enable_irqs( );
    return true;
}

void SmartPtr_IRQ::Disable( )
{
    m_state = disable_irqs( );
}

void SmartPtr_IRQ::Restore( )
{
    // don't enable on restore unless ints were enabled when constructed
    if( IrqsEnabled( m_state ) )
    {
        enable_irqs( );
    }
}
