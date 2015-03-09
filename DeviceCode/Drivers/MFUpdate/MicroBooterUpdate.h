////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <TinyHal.h>
#include <MFUpdate_decl.h>
#include "Storage\BlockStorageUpdate.h"
#include "Storage\FS\FSUpdateStorage.h"
#include "Validation\SSL\SslUpdateValidation.h"
#include "Validation\CRC\CrcUpdateValidation.h"
#include "Validation\X509\X509UpdateValidation.h"

struct MicroBooterUpdateProvider
{
    static BOOL InitializeUpdate( MFUpdate* pUpdate );
    static BOOL GetProperty     ( MFUpdate* pUpdate, LPCSTR szPropName , UINT8* pPropValue, INT32* pPropValueSize                       );
    static BOOL SetProperty     ( MFUpdate* pUpdate, LPCSTR szPropName , UINT8* pPropValue, INT32  pPropValueSize                       );
    static BOOL InstallUpdate   ( MFUpdate* pUpdate, UINT8* pValidation, INT32  validationLen                                           );
};

extern IUpdateProvider g_MicroBooterUpdateProvider;

