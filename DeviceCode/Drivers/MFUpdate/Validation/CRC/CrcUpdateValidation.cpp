////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CrcUpdateValidation.h"

IUpdateValidationProvider g_CrcUpdateValidationProvider = 
{
    UpdateValidationCRC::AuthCommand,
    UpdateValidationCRC::Authenticate,
    UpdateValidationCRC::ValidatePacket,
    UpdateValidationCRC::ValidateUpdate,

    NULL,
};

BOOL UpdateValidationCRC::AuthCommand( MFUpdate* pUpdate, UINT32 cmd, UINT8* pArgs, INT32 argsLen, UINT8* pResponse, INT32& responseLen )
{
    BOOL fRet = FALSE;
    
    switch(cmd)
    {
        case MFUPDATE_VALIDATION_COMMAND__GET_AUTH_TYPE:
            if(pResponse == NULL)
            {
                responseLen = sizeof(UINT32);
                fRet = TRUE;
            }
            else if(responseLen >= sizeof(UINT32))
            {
                *(UINT32*)pResponse = MFUPDATE_AUTHENTICATION_TYPE__CRC;

                fRet = TRUE;
            }
            break;
    }

    return fRet;    
}
BOOL UpdateValidationCRC::Authenticate( MFUpdate* pUpdate, UINT8* pAuthData, INT32 authLen )
{
    // rely on the application layer for authentication (SSL)
    return TRUE;
}

BOOL UpdateValidationCRC::ValidatePacket( MFUpdate* pUpdate, UINT8* pPacket, INT32 packetLen, UINT8* pValidation, INT32 validationLen )
{
    if(validationLen != sizeof(UINT32) || pPacket == NULL || pValidation == NULL) return FALSE;
    
    return (SUPPORT_ComputeCRC(pPacket, packetLen, 0) == *(UINT32*)pValidation);
}

BOOL UpdateValidationCRC::ValidateUpdate( MFUpdate* pUpdate, UINT8* pValidation, INT32 validationLen )
{
    UINT8 buff[512];
    INT32 len = sizeof(buff);
    INT32 updateSize = pUpdate->Header.UpdateSize;
    INT32 offset = 0;
    UINT32 crc = 0;

    if(pUpdate->Providers->Storage == NULL       ) return FALSE;
    if(validationLen               != sizeof(crc)) return FALSE;

    while(offset < updateSize)
    {
        if((offset + len) > updateSize)
        {
            len = updateSize - offset;
        }
        
        pUpdate->Providers->Storage->Read(pUpdate->StorageHandle, offset, buff, len);

        crc = SUPPORT_ComputeCRC(buff, len, crc);

        offset += len;
    }

    return crc == *(UINT32*)pValidation;    
}



