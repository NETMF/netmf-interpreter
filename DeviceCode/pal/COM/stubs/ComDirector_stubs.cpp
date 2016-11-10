////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//////////////////////////////////////////////////////////////////////////////////

__weak BOOL DebuggerPort_Initialize( COM_HANDLE ComPortNum )
{
    NATIVE_PROFILE_PAL_COM();
    return true;
}

__weak BOOL DebuggerPort_Uninitialize( COM_HANDLE ComPortNum )
{
    NATIVE_PROFILE_PAL_COM();
    return true;
}


__weak int DebuggerPort_Write( COM_HANDLE ComPortNum, const char* Data, size_t size, int maxRetries )
{
    NATIVE_PROFILE_PAL_COM();
    return 0;
}


__weak int DebuggerPort_Read( COM_HANDLE ComPortNum, char* Data, size_t size )
{
    NATIVE_PROFILE_PAL_COM();
    return 0;
}

__weak BOOL DebuggerPort_Flush( COM_HANDLE ComPortNum )
{
    NATIVE_PROFILE_PAL_COM();
    return true;
}

__weak BOOL DebuggerPort_IsSslSupported( COM_HANDLE ComPortNum )
{
    NATIVE_PROFILE_PAL_COM();
    return FALSE;
}

__weak BOOL DebuggerPort_UpgradeToSsl( COM_HANDLE ComPortNum, UINT32 flags ) 
{ 
    NATIVE_PROFILE_PAL_COM();
    return FALSE; 
}

__weak BOOL DebuggerPort_IsUsingSsl( COM_HANDLE ComPortNum )
{
    NATIVE_PROFILE_PAL_COM();
    return FALSE;
}

//////////////////////////////////////////////////////////////////////////////////

__weak void CPU_InitializeCommunication()
{
    NATIVE_PROFILE_PAL_COM();
}

__weak void CPU_UninitializeCommunication()
{
    NATIVE_PROFILE_PAL_COM();
}


__weak void CPU_ProtectCommunicationGPIOs( BOOL On )
{
    NATIVE_PROFILE_PAL_COM();
}
