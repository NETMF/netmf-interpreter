/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#include <tinyhal.h>

// The standard CLR libraries disable these if PLATFORM_OS_PORT 
// is used, apparently under the assumption that an OS with
// full CRT and multiple process support is used. However,
// in the case of a kernel library like CMSIS-RTOS that's
// not really true so these low level handlers are required.
#if PLATFORM_ARM_OS_PORT && defined( __CC_ARM )
extern "C"
{
    void __rt_div0()
    {
        NATIVE_PROFILE_PAL_CRT();
    #if defined(BUILD_RTM)
        // failure, reset immediately
        CPU_Reset();
    #else
        lcd_printf("\fERROR:\r\n__rt_div0\r\n");
        debug_printf("ERROR: __rt_div0\r\n");

        HARD_BREAKPOINT();
    #endif
    }

    void __rt_exit( int /*returncode*/ )
    {
    }

    void __rt_raise( int sig, int type )
    {
        NATIVE_PROFILE_PAL_CRT();
    #if defined(BUILD_RTM)
        // failure, reset immediately
        CPU_Reset();
    #else
        lcd_printf("\fERROR:\r\n__rt_raise(%d, %d)\r\n", sig, type);
        debug_printf("ERROR: __rt_raise(%d, %d)\r\n", sig, type);

        HARD_BREAKPOINT();
    #endif
    }
}
#endif
