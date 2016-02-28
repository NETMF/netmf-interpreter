////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include "Win32TimerQueue.h"

using namespace Microsoft::Win32;

static Timer* pWatchDogTimer = nullptr;
static UINT32 s_TimeoutMilliseconds;

HRESULT Watchdog_Enable( UINT32 TimeoutMilliseconds, WATCHDOG_INTERRUPT_CALLBACK callback, void* context )
{
#ifndef _DEBUG
    s_TimeoutMilliseconds = TimeoutMilliseconds;
    pWatchDogTimer = new Timer( TimeoutMilliseconds
                              , [=]() 
                                {   GLOBAL_LOCK(irq)
                                    callback( context ); 
                                }
                              );
    return pWatchDogTimer != nullptr ? S_OK : CLR_E_OUT_OF_MEMORY;
#else
    return S_OK;
#endif
}

void Watchdog_Disable()
{
    if( pWatchDogTimer == nullptr )
        return;

    pWatchDogTimer->Cancel();
}

void Watchdog_ResetCpu()
{
    throw std::runtime_error( "Watchdog_ResetCpu" );
}

void Watchdog_ResetCounter()
{
    if( pWatchDogTimer == nullptr )
        return;

    pWatchDogTimer->Restart();
}

