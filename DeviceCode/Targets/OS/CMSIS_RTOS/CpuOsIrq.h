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
#include <cmsis_os_cpp.h>

const uint32_t IrqsDisabledFlag = 1;

inline bool IrqsDisabled( uint32_t state )
{
    return ( state & IrqsDisabledFlag ) == IrqsDisabledFlag;
}

inline bool IrqsEnabled( uint32_t state )
{
    return ( state & IrqsDisabledFlag ) == 0;
}

#if PLATFORM_ARM_OS_PORT
static const osThreadId NoOwner = (osThreadId) 0xFFFFFFFF;

class CriticalSection
{
public:
    CriticalSection( )
        : osMutexInitMember( MutexDef )
        , MutexId( 0 )
        , Owner( NoOwner )
        , RefCount( 0 )
    {
        osMutexConstructMember( MutexDef );
        MutexId = osMutexCreate( osMutex( MutexDef ) );
        ASSERT( MutexId != NULL);
    }

    void lock( )
    {
        // refcounting employed for recursive locks since
        // CMSIS-RTOS APIs don't specify recursive behavior
        // either way.
        if( !owned() )
        {
            osMutexWait( MutexId, osWaitForever );
            Owner = osThreadGetId( );
        }
        ++RefCount;
    }

    void unlock( )
    {
        ASSERT( owned( ) && RefCount > 0 );
        if( !owned( ) )
            return;

        --RefCount;
        if( RefCount == 0 )
        {
            Owner = NoOwner;
            osMutexRelease( MutexId );
        }
    }

    bool owned()
    { 
        return osThreadGetId( ) == Owner;
    }
        
private:
    osMutexDefMember( MutexDef );
    osMutexId MutexId;
    osThreadId Owner;
    int RefCount;
};

#endif

// determines if the current system context is that of a physical interrupt
inline bool sys_InIsr()
{
    return __get_IPSR() != 0;
}

// internal function to simulate common interrupt enable state query for embedded MCUs
inline bool irqs_enabled( )
{
    unsigned int retVal = __get_PRIMASK();
    return retVal;
}

// internal function to simulate common interrupt enable intrinsics for embedded MCUs
uint32_t enable_irqs( )
{
    unsigned int retVal = __get_PRIMASK();
    __enable_irq();
    return retVal;
}

// internal function to simulate common interrupt disable intrinsics for embedded MCUs
uint32_t disable_irqs( )
{
    unsigned int retVal = __get_PRIMASK();
    __disable_irq();
    return retVal;
}

inline void irq_yield()
{
    __NOP();
}
