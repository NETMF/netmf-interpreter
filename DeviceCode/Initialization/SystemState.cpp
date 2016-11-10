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
#include <tinyhal.h>

volatile INT32 SystemStates[SYSTEM_STATE_TOTAL_STATES];

void SystemState_SetNoLock( SYSTEM_STATE State )
{
    InterlockedIncrement( &SystemStates[State] );
}

void SystemState_ClearNoLock( SYSTEM_STATE State )
{
    InterlockedDecrement( &SystemStates[State] );
}

BOOL SystemState_QueryNoLock( SYSTEM_STATE State )
{
    return (SystemStates[State] > 0) ? TRUE : FALSE;
}

// since the no-lock versions are now using InterlockedXXX
// there is no reason to distinguish between locked or unlocked
void SystemState_Set( SYSTEM_STATE State )
{
    SystemState_SetNoLock( State );
}

void SystemState_Clear( SYSTEM_STATE State )
{
    SystemState_ClearNoLock( State );
}

BOOL SystemState_Query( SYSTEM_STATE State )
{
    return SystemState_QueryNoLock( State );
}
