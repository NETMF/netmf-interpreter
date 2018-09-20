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

// Minimalist set of CPU_xxx APIs to link and run CLR
UINT32 CPU_SystemClock( )
{
    return 0;
}

UINT32 CPU_TicksPerSecond( )
{
    return 10000000; // Ticks are in 100ns intervals
}

UINT64 CPU_MicrosecondsToTicks( UINT64 uSec )
{
    return uSec * 10;
}

UINT64  CPU_MillisecondsToTicks( UINT64 mSec )
{
    return CPU_MicrosecondsToTicks( mSec * 1000 );
}

UINT64  CPU_MillisecondsToTicks( UINT32 mSec )
{
    return CPU_MicrosecondsToTicks( (UINT64)mSec * 1000 );
}

void CPU_InitializeCommunication()
{
    NATIVE_PROFILE_PAL_COM();

    ::DebuggerPort_Initialize( DEBUG_TEXT_PORT );
}

void CPU_UninitializeCommunication()
{
    NATIVE_PROFILE_PAL_COM();

    ::DebuggerPort_Uninitialize( DEBUG_TEXT_PORT );
    Network_Uninitialize();
}



