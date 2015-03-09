////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "MicroBooterUpdate.h"
#include <TinyCLR_Types.h>

IUpdateProvider g_MicroBooterUpdateProvider = 
{
    MicroBooterUpdateProvider::InitializeUpdate,
    MicroBooterUpdateProvider::GetProperty,
    MicroBooterUpdateProvider::SetProperty,
    MicroBooterUpdateProvider::InstallUpdate
};

extern void ClrReboot(void);

BOOL MicroBooterUpdateProvider::InitializeUpdate( MFUpdate* pUpdate )
{
    if(pUpdate == NULL) return FALSE;

    switch(pUpdate->Header.UpdateType)
    {
        case MFUPDATE_UPDATETYPE_FIRMWARE:
        case MFUPDATE_UPDATETYPE_ASSEMBLY:
            break;

        default:
            return FALSE;
    }

    return TRUE;
}

BOOL MicroBooterUpdateProvider::GetProperty( MFUpdate* pUpdate, LPCSTR szPropName , UINT8* pPropValue, INT32* pPropValueSize )
{
    return FALSE;
}

BOOL MicroBooterUpdateProvider::SetProperty( MFUpdate* pUpdate, LPCSTR szPropName , UINT8* pPropValue, INT32 pPropValueSize )
{
    return FALSE;
}


BOOL MicroBooterUpdateProvider::InstallUpdate( MFUpdate* pUpdate, UINT8* pValidation, INT32 validationLen )
{
    if(pUpdate->Providers->Storage == NULL) return FALSE;

    if(!pUpdate->IsValidated()) return FALSE;

    switch(pUpdate->Header.UpdateType)
    {
        case MFUPDATE_UPDATETYPE_FIRMWARE:
            {
                HAL_UPDATE_CONFIG cfg;

                if(sizeof(cfg.UpdateSignature) < validationLen) return FALSE;
                
                cfg.Header.Enable = TRUE;
                cfg.UpdateID = pUpdate->Header.UpdateID;

                if(validationLen == sizeof(UINT32))
                {
                    cfg.UpdateSignType = HAL_UPDATE_CONFIG_SIGN_TYPE__CRC;
                }
                else
                {
                    cfg.UpdateSignType = HAL_UPDATE_CONFIG_SIGN_TYPE__SIGNATURE;
                }
                
                memcpy( cfg.UpdateSignature, pValidation, validationLen );

                if(HAL_CONFIG_BLOCK::UpdateBlockWithName(cfg.GetDriverName(), &cfg, sizeof(cfg), FALSE))
                {
                    CPU_Reset();
                }
            }
            break;
            
        case MFUPDATE_UPDATETYPE_ASSEMBLY:
            {
                BlockStorageStream stream;
                
                if(NULL == BlockStorageList::GetFirstDevice())
                {
                    BlockStorageList::Initialize();
                
                    BlockStorage_AddDevices();
                
                    BlockStorageList::InitializeDevices();
                }

                if(stream.Initialize(BlockUsage::DEPLOYMENT))
                {
                    if(pUpdate->Header.UpdateSubType == MFUPDATE_UPDATESUBTYPE_ASSEMBLY_REPLACE_DEPLOY)
                    {
                        do
                        {
                            stream.Erase(stream.Length);
                        }
                        while(stream.NextStream());

                        stream.Initialize(BlockUsage::DEPLOYMENT);
                    }
                    
                    do
                    {
                        UINT8 buf[512];
                        INT32 offset = 0;
                        INT32 len = sizeof(buf);
                        const BlockDeviceInfo* deviceInfo = stream.Device->GetDeviceInfo();
                        BOOL isXIP = deviceInfo->Attribute.SupportsXIP;
                        
                        const CLR_RECORD_ASSEMBLY* header;
                        INT32  headerInBytes = sizeof(CLR_RECORD_ASSEMBLY);
                        BYTE * headerBuffer  = NULL;
                        
                        if(!isXIP)
                        {
                            headerBuffer = (BYTE*)private_malloc( headerInBytes );  if(!headerBuffer) return FALSE;
                            memset( headerBuffer, 0,  headerInBytes );
                        }
                        
                        while(TRUE)
                        {
                            if(!stream.Read( &headerBuffer, headerInBytes )) break;
                        
                            header = (const CLR_RECORD_ASSEMBLY*)headerBuffer;
                        
                            // check header first before read
                            if(!header->GoodHeader())
                            {
                                stream.Seek(-headerInBytes);
                                
                                if(stream.IsErased(pUpdate->Header.UpdateSize))
                                {
                                    while(offset < pUpdate->Header.UpdateSize)
                                    {
                                        if((pUpdate->Header.UpdateSize - offset) < len)
                                        {
                                            len = pUpdate->Header.UpdateSize - offset;
                                        }
                                        
                                        offset += pUpdate->Providers->Storage->Read(pUpdate->StorageHandle, offset, buf, len);

                                        stream.Write(buf, len);
                                    }

                                    ClrReboot();
                                    return TRUE;
                                }
                                break;
                            }
                        
                            UINT32 AssemblySizeInByte = ROUNDTOMULTIPLE(header->TotalSize(), CLR_UINT32);
                        
                            stream.Seek( AssemblySizeInByte );
                        }
                        if(!isXIP) private_free( headerBuffer );
                    
                    }
                    while(stream.NextStream());
                }

            }
            break;
    }
    
    return FALSE;
}
 
