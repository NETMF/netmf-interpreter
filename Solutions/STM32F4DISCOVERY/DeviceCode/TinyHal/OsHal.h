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
#include <cmsis_os.h>

extern bool OnClrThread();

// OS CLR Thread signal bit to indicate events to the CLR thread
// The CLR reserves the first 8 such signals. Custom ports may
// use additional events up to the maximum supported by the underlying
// CMSIS_RTOS implementation. 
const int32_t UserEventMin = 0x00000100;

//a CLR event has occured that should wake
// the CLR from "sleep"
const int32_t ClrEventSignal = 0x00000001;


extern osThreadId GetClrThreadId();

inline bool OnClrThread()
{
    return osThreadGetId() == GetClrThreadId();    
}
