////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#include <Tinyhal.h>
#include <PAL\PKCS11\CryptokiPAL.h>

//--//

struct SecureStorage_Stub
{
    BOOL  CreateFile( LPCSTR fileName , LPCSTR groupName, UINT32  fileType, UINT8* data, UINT32  dataLength );
    BOOL  ReadFile( LPCSTR fileName , LPCSTR groupName, UINT32& fileType, UINT8* data, UINT32& dataLength );
    BOOL  GetFileEnum( LPCSTR groupName, UINT32 fileType , FileEnumCtx& enumCtx );
    BOOL  GetNextFile( FileEnumCtx& enumCtx, CHAR*fileName,UINT32 fileNameLen );
    BOOL  Delete( LPCSTR fileName , LPCSTR groupName );
};

