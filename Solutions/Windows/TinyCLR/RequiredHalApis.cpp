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

static DWORD HalInitThreadId;

bool OnClrThread()
{
    return HalInitThreadId == ::GetCurrentThreadId();
}

void HAL_Initialize(void)
{
    HalInitThreadId = ::GetCurrentThreadId();

    // current configurations of NETMF don't assume static constructors 
    // are avaliable or run so the completion/continuation lists are
    // initialized here until that changes.
    HAL_CONTINUATION::InitializeList();
    HAL_COMPLETION::InitializeList();

    // Custom heap not needed as C runtime for the OS has one already
    //HAL_Init_Custom_Heap();

    Time_Initialize( );

    // no GPIO in this platform
    //CPU_GPIO_Initialize();

    // init blocks storage first so other components
    // can use the config sections
    BlockStorageList::Initialize();
    BlockStorage_AddDevices();
    BlockStorageList::InitializeDevices();

    CPU_InitializeCommunication();

    Watchdog_GetSetTimeout(WATCHDOG_TIMEOUT, TRUE);
    Watchdog_GetSetBehavior(WATCHDOG_BEHAVIOR, TRUE);
    Watchdog_GetSetEnabled(WATCHDOG_ENABLE, TRUE);

    // This enables the SNTP protocol to acquire current
    // time from a network server. Which isn't needed since
    // the underlying OS already has that support built-in
    //TimeService_Initialize();


    // File systems not currently enabled, though we could add it
    // via a standard VHD file when we get to testing FS support 
    // in this solution
    //FileSystemVolumeList::Initialize();
    //FS_AddVolumes();
    //FileSystemVolumeList::InitializeVolumes();
}

void HAL_Uninitialize(void)
{
    BlockStorageList::UnInitializeDevices();
    Watchdog_GetSetEnabled( FALSE, TRUE );
    Time_Uninitialize();
}

void HAL_EnterBooterMode(void)
{
}

// TODO: Rename and move this to a PAL level service for an OS
// This is used to indicate an external OS request to shutdown
// (e.g. clicked the close button on the window's title bar etc...
BOOL HAL_Windows_IsShutdownPending()
{
    return FALSE;
}

#ifdef PLATFORM_WINDOWS_EMULATOR
HAL_Configuration_Windows g_HAL_Configuration_Windows;

// This thing is in fact intended to slow down the execution of code on the emulator!
// The idea is that you can apply a scaling factor from real time to effectively stall
// the interpreter for a period of time (per instruction, per method call, etc..) so
// that the emulation runs at a more realistic speed for a real device. This prevents
// building inefficient apps that run fine on the emulator but crawl on a device.
// However, sometimes that's not desired and there is, at present, no easy option to
// configure the stall time and there are serious perf issues on the emulator debug
// story so this is disabled at the moment to eliminate that as a variable while we
// investigate the perf issues.
void HAL_Windows_FastSleep(INT64 ticks)
{
    //LARGE_INTEGER frequency;
    //LARGE_INTEGER countStart;
    //LARGE_INTEGER countEnd;

    //if(ticks > 0)
    //{
    //    ::QueryPerformanceFrequency( &frequency  );

    //    ::QueryPerformanceCounter  ( &countStart );

    //    double ratio = (double)TIME_CONVERSION__TO_SECONDS / (double)frequency.QuadPart;

    //    countEnd.QuadPart = (INT64)(((countStart.QuadPart * ratio) + ticks) / ratio);

    //    while(countStart.QuadPart < countEnd.QuadPart)
    //    {
    //        ::QueryPerformanceCounter( &countStart );
    //    }
    //}
}

// unfortunately the NETMF build system and code assume x86 == PLATFORM_WINDOWS_EMULATOR == MSVC
// so when the tools binaries are built many x86 binary libs are built but the build for
// them is not set to go to a platform specific output. Thus, building this project gets
// the libraries built when the tools were built, which are based on the windows2 headers
// so we still need this since the windows2 platform selector uses this in the
// ASSERT_IRQ_xx macros.
// [We really need to split this mess of a build up...]
BOOL HAL_Windows_HasGlobalLock()
{
    return !SmartPtr_IRQ::GetState();
}
#endif

// required by profiling APIs in Debugger component when WIN32 defined
// longer term this should be moved out to allow any platform with a
// high resolution performance counter to use it.
UINT64 HAL_Windows_GetPerformanceTicks()
{
    return 0;
}

// TODO: remove dependency on this from the CLR - it's a silly dependency
void BackLight_Set(BOOL On)
{
}

// TODO: remove dependency on this from the CLR - it's a silly dependency
BOOL Piezo_Tone(UINT32 Frequency_Hertz, UINT32 Duration_Milliseconds)
{
    return TRUE;
}

int hal_vfprintf( COM_HANDLE stream, const char* format, va_list arg )
{
    NATIVE_PROFILE_PAL_CRT();
    char buffer[512];
    int chars = 0;

    chars = hal_vsnprintf( buffer, sizeof(buffer), format, arg );

    switch(ExtractTransport(stream))
    {
    case USART_TRANSPORT:
    case USB_TRANSPORT:
    case SOCKET_TRANSPORT:
    case DEBUG_TRANSPORT:
        DebuggerPort_Write( stream, buffer, chars ); // skip null terminator
        break;

    //case LCD_TRANSPORT:
    //    {
    //        for(int i = 0; i < chars; i++)
    //        {
    //            LCD_WriteFormattedChar( buffer[i] );
    //        }
    //    }
    //    break;
    }

    return chars;
}

int hal_printf( char const* format, ...)
{
    va_list arg_ptr;
    va_start(arg_ptr, format);
    auto retVal = hal_vfprintf( COM_DEBUG, format, arg_ptr);
    va_end(arg_ptr);
    return retVal;
}

int hal_snprintf(char* buffer, size_t len, const char* format, ...)
{
    va_list arg_ptr;
    int     chars;

    va_start(arg_ptr, format);

    chars = hal_vsnprintf(buffer, len, format, arg_ptr);

    va_end(arg_ptr);

    return chars;
}

int hal_vsnprintf(char* buffer, size_t len, const char* format, va_list arg)
{
    return _vsnprintf_s(buffer, len, len - 1/* force space for trailing zero*/, format, arg);
}

HAL_SYSTEM_CONFIG HalSystemConfig =
{
    { TRUE }, // HAL_DRIVER_CONFIG_HEADER Header;
    { DEBUGGER_PORT }, // UINT32      DebuggerPorts[MAX_DEBUGGERS];
    { MESSAGING_PORT },
    DEBUG_TEXT_PORT,
    115200,
    DEBUG_TEXT_PORT,  // STDIO = COM2 or COM1
    { 0, 0 },   // { SRAM1_MEMORY_Base, SRAM1_MEMORY_Size },
    { 0, 0 },   // { FLASH_MEMORY_Base, FLASH_MEMORY_Size },
};

OEM_MODEL_SKU OEM_Model_SKU = { 0xFF, 2, 0xFFFF };

// All solutions are expected to provide an implementation of this
unsigned int Solution_GetReleaseInfo(MfReleaseInfo& releaseInfo)
{
    MfReleaseInfo::Init( releaseInfo
                       , VERSION_MAJOR
                       , VERSION_MINOR
                       , VERSION_BUILD
                       , VERSION_REVISION
                       , OEMSYSTEMINFOSTRING
                       , hal_strlen_s(OEMSYSTEMINFOSTRING)
                       );
    return 1; // alternatively, return false if you didn't initialize the releaseInfo structure.
}

void debug_printf(const char* format, ...)
{
    va_list args;
    va_start( args, format);
    hal_vfprintf(COM_DEBUG, format, args);
    va_end( args );
}

