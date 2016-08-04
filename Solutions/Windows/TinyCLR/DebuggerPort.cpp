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
#include <winioctl.h>
#include "NamedPipe.h"

using namespace std;

void HAL_Windows_Debug_Print( LPSTR szText )
{
    DebuggerPort_Write( HalSystemConfig.DebugTextPort, szText, strlen( szText ) ); // skip null terminator
    DebuggerPort_Flush( HalSystemConfig.DebugTextPort );                    // skip null terminator
}

// for now these ignore the handle, everything goes to the same place.
// A future update could use a unique pipe name for each type of port

#if !USE_NAMED_PIPE
BOOL DebuggerPort_Initialize( COM_HANDLE ComPortNum )
{
    return TRUE;
}

BOOL DebuggerPort_Uninitialize( COM_HANDLE ComPortNum )
{        
    return TRUE;
}

int DebuggerPort_Write( COM_HANDLE ComPortNum, const char* Data, size_t size, int maxRetries /*= 99*/ )
{
    // send everything to standard cout stream (i.e. the console)
    cout.write( Data, size );
    return size;
}

int DebuggerPort_Read( COM_HANDLE ComPortNum, char* Data, size_t size )
{        
    return 0;
}

BOOL DebuggerPort_Flush( COM_HANDLE ComPortNum )
{        
    cout.flush();
    return TRUE;
}

BOOL DebuggerPort_IsSslSupported( COM_HANDLE ComPortNum )
{
    return FALSE;
}

BOOL DebuggerPort_UpgradeToSsl( COM_HANDLE ComPortNum, UINT32 flags ) 
{ 
    return FALSE; 
}

BOOL DebuggerPort_IsUsingSsl( COM_HANDLE ComPortNum )
{
    return FALSE;
}
#else
// while this code works, the current implementation has some limitations
// that keep it from being used just yet:
// * It is blocking in that it will stop the entire system and wait until 
//   something is connected. 
// * a VS MF Project debugging session or DebuggerPortConsole.exe is
//   required to start and make the connection.
// 
// AS we progress with this code base, particularly on the VS Debugging
// stories for NETMF apps we'll deal with those in turn but other higher
// priority issues in Networking don't need this now.

using namespace Microsoft::Win32;

NamedPipe DebuggerPipe;

BOOL DebuggerPort_Initialize( COM_HANDLE ComPortNum )
{
    if( DebuggerPipe.IsValid() )
        return TRUE;

    // match naming used by standard emulator to ease use with MFDeploy and VS debugger.
    std::wstringstream name;
    name << NamedPipe::LocalPipeNamePrefix << L"TinyClr_" << ::GetCurrentProcessId() << L"_Port1";
    
    if( !DebuggerPipe.Create( name.str().c_str() ) )
        return FALSE;
    
    // NOTE: At the moment this is a blocking call
    // so there must be a listener
    return DebuggerPipe.Connect( );
}

BOOL DebuggerPort_Uninitialize( COM_HANDLE ComPortNum )
{        
    return DebuggerPipe.Close();
}

int DebuggerPort_Write( COM_HANDLE ComPortNum, const char* Data, size_t size, int maxRetries /*= 99*/ )
{        
    if( !DebuggerPipe.IsValid() )
        return 0;

    uint32_t numWritten = 0;
    DebuggerPipe.Write( Data, size, &numWritten );
    return numWritten;
}

int DebuggerPort_Read( COM_HANDLE ComPortNum, char* Data, size_t size )
{        
    if( !DebuggerPipe.IsValid() )
        return 0;

    uint32_t numRead = 0;
    DebuggerPipe.Read( Data, size, &numRead );
    return numRead;
}

BOOL DebuggerPort_Flush( COM_HANDLE ComPortNum )
{        
    if( !DebuggerPipe.IsValid() )
        return FALSE;

    return DebuggerPipe.Flush();
}

BOOL DebuggerPort_IsSslSupported( COM_HANDLE ComPortNum )
{
    return FALSE;
}

BOOL DebuggerPort_UpgradeToSsl( COM_HANDLE ComPortNum, UINT32 flags ) 
{ 
    return FALSE; 
}

BOOL DebuggerPort_IsUsingSsl( COM_HANDLE ComPortNum )
{
    return FALSE;
}
#endif
