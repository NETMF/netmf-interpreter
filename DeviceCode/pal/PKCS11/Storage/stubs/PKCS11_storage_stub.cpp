////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#include <Tinyhal.h>
#include "PKCS11_storage_stub.h"

//--//
BOOL  SecureStorage_Stub::CreateFile( LPCSTR fileName , LPCSTR groupName, UINT32  fileType, UINT8* data, UINT32  dataLength )
{
    return FALSE;
}
BOOL  SecureStorage_Stub::ReadFile( LPCSTR fileName , LPCSTR groupName, UINT32& fileType, UINT8* data, UINT32& dataLength )
{
    return FALSE;
}
BOOL  SecureStorage_Stub::GetFileEnum( LPCSTR groupName, UINT32 fileType , FileEnumCtx& enumCtx )
{
    return FALSE;
}
BOOL  SecureStorage_Stub::GetNextFile( FileEnumCtx& enumCtx, CHAR*fileName,UINT32 fileNameLen )
{
    return FALSE;
}
BOOL  SecureStorage_Stub::Delete( LPCSTR fileName , LPCSTR groupName )
{
    return FALSE;
}

