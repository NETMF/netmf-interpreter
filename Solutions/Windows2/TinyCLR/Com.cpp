////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "sddl.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

using namespace Microsoft::SPOT::Emulator;

BOOL DebuggerPort_Initialize( COM_HANDLE ComPortNum )
{
    return EmulatorNative::GetIComDriver()->Initialize( ComPortNum );
}

BOOL DebuggerPort_Uninitialize( COM_HANDLE ComPortNum )
{        
    return EmulatorNative::GetIComDriver()->Uninitialize( ComPortNum );
}

int DebuggerPort_Write( COM_HANDLE ComPortNum, const char* Data, size_t size, int maxRetries )
{        
    return EmulatorNative::GetIComDriver()->Write( ComPortNum, (IntPtr)(char*)Data, (unsigned int)size );
}

int DebuggerPort_Read( COM_HANDLE ComPortNum, char* Data, size_t size )
{        
    return EmulatorNative::GetIComDriver()->Read( ComPortNum, (IntPtr)Data, (unsigned int)size );
}

BOOL DebuggerPort_Flush( COM_HANDLE ComPortNum )
{        
    return EmulatorNative::GetIComDriver()->Flush( ComPortNum );
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

