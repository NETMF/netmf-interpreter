////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <mfupdate_decl.h>
#include <PAL\PKCS11\cryptokipal.h>

struct UpdateValidationSSL
{
    static BOOL AuthCommand   ( MFUpdate* pUpdate, UINT32 cmd        , UINT8* pArgs        , INT32  argsLen    , UINT8* pResponse   , INT32& responseLen );
    static BOOL Authenticate  ( MFUpdate* pUpdate, UINT8* pAuth      , INT32  authLen                                                                   );    
    static BOOL ValidatePacket( MFUpdate* pUpdate, UINT8* pPacket    , INT32  packetLen    , UINT8* pValidation, INT32 validationLen                    );
    static BOOL ValidateUpdate( MFUpdate* pUpdate, UINT8* pValidation, INT32  validationLen                                                             );    

private:
    UINT8* m_pPublicCertData;
    UINT32 m_publicCertLen;
};

//--//

extern IUpdateValidationProvider g_SslUpdateValidationProvider;

