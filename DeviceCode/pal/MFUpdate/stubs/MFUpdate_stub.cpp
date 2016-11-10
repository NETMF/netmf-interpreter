////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <MFUpdate_decl.h>

__weak void  MFUpdate_Initialize(void)
{
}

__weak BOOL MFUpdate_GetProperty( UINT32 updateHandle, LPCSTR szPropName, UINT8* pPropValue, INT32* pPropValueSize )
{
    return FALSE;
}

__weak BOOL MFUpdate_SetProperty( UINT32 updateHandle, LPCSTR szPropName, UINT8* pPropValue, INT32 pPropValueSize )
{
    return FALSE;
}

__weak INT32 MFUpdate_InitUpdate( LPCSTR szProvider, MFUpdateHeader& update )
{
    return -1;
}

__weak BOOL MFUpdate_AuthCommand( INT32 updateHandle, UINT32 cmd, UINT8* pArgs, INT32 argsLen, UINT8* pResponse, INT32& responseLen )
{
    return FALSE;
}

__weak BOOL MFUpdate_Authenticate( INT32 updateHandle, UINT8* pAuthData, INT32 authLen )
{
    return FALSE;
}


__weak BOOL MFUpdate_Open( INT32 updateHandle )
{
    return FALSE;
}

__weak BOOL MFUpdate_Create( INT32 updateHandle )
{
    return FALSE;
}

__weak BOOL MFUpdate_GetMissingPackets( INT32 updateHandle, UINT32* pPacketBits, INT32* pCount )
{
    return FALSE;
}

__weak BOOL MFUpdate_AddPacket( INT32 updateHandle, INT32 packetIndex, UINT8* packetData, INT32 packetLen, UINT8* pValidationData, INT32 validationLen )
{
    return FALSE;
}

__weak BOOL MFUpdate_Validate( INT32 updateHandle, UINT8* pValidationData, INT32 validationLen )
{
    return FALSE;
}

__weak BOOL MFUpdate_Install( INT32 updateHandle, UINT8* pValidationData, INT32 validationLen )
{
    return FALSE;
}

__weak BOOL MFUpdate_Delete( INT32 updateHandle )
{
    return FALSE;
}

