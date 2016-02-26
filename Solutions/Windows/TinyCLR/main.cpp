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
#include "WinPCAPDeviceList.h"
#include "Win32Settings.h"
#include "ScopeGuard.h"

void WaitForWinPCAPService( );

Microsoft::Win32::Win32Settings g_CommandLineParams;

int wmain( int argc, wchar_t const* argv[ ] )
{
    // on any exit of this scope (including errors and exceptions)
    // if option was set to wait on exit then propmpt user and wait
    ON_SCOPE_EXIT([](){
        if( g_CommandLineParams.m_fWaitOnExit )
        {
            std::cout<< std::endl << "Press any to exit" << std::endl;
            ::getchar();
        }
    });

    // CLR_SETINGS is a POD struct with no constructor so
    // zero init the structure before using it
    CLR_SETTINGS clrSet = { 0 };

    // parse command line options, any actions for the options that
    // require HAL_Initialize, must not run at this point. This is
    // primarily about parsing options and saving the parsed data in
    // the settings instance. This allows init code called from the
    // HAL_Initialize() implementation to have access to parsed args
    // to adjust behavior as specified in the args. 
    auto hr = g_CommandLineParams.ParseCmdLineOptions( argc, argv );
    if( FAILED( hr ) )
        return FALSE;

    // if user asked for no network, then don't bother checking for
    // or waiting on the WINPCAP service support.
    if( !g_CommandLineParams.m_fNoNetwork )
        WaitForWinPCAPService( );

    HAL_Initialize( );

    // perform second stage of settings processing
    // this executes additional actions that require
    // HAL_Initialize() to complete first. (i.e. loading
    // PE and DAT files and saving them, along with other
    // config data to the blockstorage 'FLASH' )
    hr = g_CommandLineParams.ProcessCommandLineSettings( );
    if( FAILED( hr ) )
        return FALSE;

    // don't initialize network until AFTER processing
    // command line args that can modify it's startup
    if( !g_CommandLineParams.m_fNoNetwork )
        Network_Initialize();

    clrSet.MaxContextSwitches = 50;
    clrSet.PerformGarbageCollection = g_CommandLineParams.m_fPerformGarbageCollection;
    clrSet.PerformHeapCompaction = g_CommandLineParams.m_fPerformHeapCompaction;
    clrSet.WaitForDebugger = g_CommandLineParams.m_fWaitForDebugger;

#if defined(TINYCLR_TRACE_MEMORY_STATS)
    // force tracing off as it is pretty noisy.
    s_CLR_RT_fTrace_MemoryStats = 0;
#endif

    // Unless noexecute was specified on cmdline, hand off to the CLR core to run the code
    if( !g_CommandLineParams.m_fNoExecuteIL )
        ClrStartup( clrSet );

    HAL_Uninitialize( );
    return 0;
}

void WaitForWinPCAPService( )
{
    WinPCAP::DeviceList deviceList;
    if( deviceList.empty( ) )
    {
        std::cout << "WinPCAP could not find any devices" << std::endl;
        std::cout << "Please ensure the WinPCAP driver service is running and you have a supported network interface adapter" << std::endl;
        std::cout << "To manually start the service open an administrative command prompt and run the following command" << std::endl;
        std::cout << "    sc start NPF" << std::endl;
        std::cout << "Waiting for WinPCAP devices..." << std::endl;
    }

    while( deviceList.empty( ) )
    {
        Sleep( 3000 );
        deviceList.ScanForDevices( );
    };
}


