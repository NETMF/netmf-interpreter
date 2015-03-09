////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <MFUpdate_decl.h>

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "s_Updates"
#endif

static MFUpdate s_Updates[4];
static BOOL     s_updatesInitialized = FALSE;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

static void MFUpdate_RebootHandler()
{
    s_updatesInitialized = FALSE;    
}


void MFUpdate_Initialize()
{
    if(s_updatesInitialized) return;
    
    memset(s_Updates, 0, sizeof(s_Updates));
    
    HAL_AddSoftRebootHandler(MFUpdate_RebootHandler);

    s_updatesInitialized = TRUE;
}

static void ReleaseUpdateHandle(int handle)
{
    if(handle < 0 || handle > ARRAYSIZE(s_Updates)) return;
    
    memset(&s_Updates[handle], 0, sizeof(MFUpdate));
}

static INT32 GetNewUpdateHandle(MFUpdate** ppUpdate, MFUpdateHeader* pHdr)
{
    INT32 retVal = -1;
    
    for(int i=0; i<ARRAYSIZE(s_Updates); i++)
    {
        if(0 != (s_Updates[i].Flags & MFUPDATE_FLAGS__INUSE))
        {
            MFUpdateHeader hdr;
            if((pHdr->UpdateType == s_Updates[i].Header.UpdateType && pHdr->UpdateSubType == s_Updates[i].Header.UpdateSubType) || 
                s_Updates[i].Providers->Storage == NULL ||  
               !s_Updates[i].Providers->Storage->GetHeader(s_Updates[i].StorageHandle, &hdr))
            {
                ReleaseUpdateHandle(i);
            }
        }

        if(0 == (s_Updates[i].Flags & MFUPDATE_FLAGS__INUSE))
        {
            *ppUpdate = &s_Updates[i];
            (*ppUpdate)->Flags |= MFUPDATE_FLAGS__INUSE;
            retVal = i;
            break;
        }
    }

    return retVal;
}

static MFUpdate* GetUpdate(int handle)
{
    if(handle < 0 || handle > ARRAYSIZE(s_Updates)) return NULL;
    
    MFUpdate& update = s_Updates[handle];

    if(0 == (update.Flags & MFUPDATE_FLAGS__INUSE)) return NULL;

    return &update;    
}


static const IUpdatePackage* GetProviderByName(LPCSTR szProvider)
{
    for(int i=0; i<g_UpdatePackageCount; i++)
    {
        if(g_UpdatePackages[i].ProviderName != NULL && hal_stricmp(szProvider, g_UpdatePackages[i].ProviderName) == 0)
        {
            return &g_UpdatePackages[i];
        }
    }

    return NULL;
}

INT32 MFUpdate_InitUpdate( LPCSTR szProvider, MFUpdateHeader& update )
{
    MFUpdate*             pUpdate;

    if(!s_updatesInitialized) MFUpdate_Initialize();
    
    const IUpdatePackage* providers    = GetProviderByName(szProvider        ); if(providers    == NULL) return -1;
    INT32                 updateHandle = GetNewUpdateHandle(&pUpdate, &update); if(updateHandle ==   -1) return -1;

    memcpy( &pUpdate->Header, &update, sizeof(update) );

    pUpdate->Providers = providers;

    if(providers->Update->InitializeUpdate != NULL && !providers->Update->InitializeUpdate( pUpdate ))
    {
        ReleaseUpdateHandle(updateHandle);

        return -1;
    }

    return updateHandle;
}

BOOL MFUpdate_AuthCommand( INT32 updateHandle, UINT32 cmd, UINT8* pArgs, INT32 argsLen, UINT8* pResponse, INT32& responseLen )
{
    MFUpdate*                        update = GetUpdate(updateHandle)      ; if(update == NULL) return FALSE;
    const IUpdateValidationProvider* valid  = update->Providers->Validation; if(valid  == NULL) return FALSE;

    if(valid->AuthCommand == NULL) return FALSE;

    return valid->AuthCommand( update, cmd, pArgs, argsLen, pResponse, responseLen );
}

BOOL MFUpdate_Authenticate( INT32 updateHandle, UINT8* pAuthData, INT32 authLen )
{
    BOOL fRet;
    MFUpdate*                        update = GetUpdate(updateHandle)      ; if(update == NULL) return FALSE;
    const IUpdateValidationProvider* valid  = update->Providers->Validation; if(valid  == NULL) return FALSE;

    if(valid->Authenticate == NULL) return TRUE;

    fRet = valid->Authenticate( update, pAuthData, authLen );

    if(fRet)
    {
        update->Flags |= MFUPDATE_FLAGS__AUTHENTICATED;
    }
    else
    {
        update->Flags &= ~MFUPDATE_FLAGS__AUTHENTICATED;
    }

    return fRet;
}


BOOL MFUpdate_GetProperty( UINT32 updateHandle, LPCSTR szPropName, UINT8* pPropValue, INT32* pPropValueSize )
{
    MFUpdate*              update   = GetUpdate(updateHandle)  ; if(update   == NULL) return FALSE;
    const IUpdateProvider* provider = update->Providers->Update; if(provider == NULL) return FALSE;

    if(provider->GetProperty == NULL) return FALSE;

    return provider->GetProperty( update, szPropName, pPropValue, pPropValueSize );
}

BOOL MFUpdate_SetProperty( UINT32 updateHandle, LPCSTR szPropName, UINT8* pPropValue, INT32 pPropValueSize )
{
    MFUpdate*              update   = GetUpdate(updateHandle)  ; if(update   == NULL) return FALSE;
    const IUpdateProvider* provider = update->Providers->Update; if(provider == NULL) return FALSE;

    if(provider->GetProperty == NULL) return FALSE;

    return provider->SetProperty( update, szPropName, pPropValue, pPropValueSize );
}


BOOL MFUpdate_Open( INT32 updateHandle )
{
    MFUpdate*              update   = GetUpdate(updateHandle)  ; if(update   == NULL) return FALSE;
    const IUpdateProvider* provider = update->Providers->Update; if(provider == NULL) return FALSE;
    MFUpdateHeader         header;

    if(!update->IsAuthenticated()) return FALSE;

    if(update->Providers->Storage == NULL) return FALSE;

    update->StorageHandle = update->Providers->Storage->Open(MFUPDATE_UPDATEID_ANY, update->Header.UpdateType, update->Header.UpdateSubType);

    if(update->StorageHandle == -1) return FALSE;

    update->Providers->Storage->GetHeader( update->StorageHandle, &header );

    // We only support ONE update per update type (delete any other versions)
    if(header.UpdateID           != update->Header.UpdateID        ||
       header.Version.usMajor    != update->Header.Version.usMajor ||
       header.Version.usMinor    != update->Header.Version.usMinor ||
       header.Version.usBuild    != update->Header.Version.usBuild ||
       header.Version.usRevision != update->Header.Version.usRevision)
    {
        update->Providers->Storage->Delete(header.UpdateID, header.UpdateType, header.UpdateSubType);
        return FALSE;
    }

    memcpy(&update->Header, &header, sizeof(header));

    return TRUE;
}

BOOL MFUpdate_Create( INT32 updateHandle )
{
    MFUpdate*              pUpdate  = GetUpdate(updateHandle)   ; if(pUpdate  == NULL) return FALSE;
    const IUpdateProvider* provider = pUpdate->Providers->Update; if(provider == NULL) return FALSE;
    UINT32 flags = pUpdate->Flags;

    if(!pUpdate->IsAuthenticated()) return FALSE;
    if(pUpdate->Providers->Storage == NULL) return FALSE;

    if(!provider->InitializeUpdate(pUpdate))
    {
        ReleaseUpdateHandle(updateHandle);
        return FALSE;
    }

    pUpdate->StorageHandle = pUpdate->Providers->Storage->Create(pUpdate->Header, flags);

    if(pUpdate->StorageHandle == -1)
    {
        ReleaseUpdateHandle(updateHandle);
        return FALSE;
    }

    return TRUE;
}

BOOL MFUpdate_GetMissingPackets( INT32 updateHandle, UINT32* pPacketBits, INT32* pCount )
{
    if(pCount == NULL) return FALSE;

    MFUpdate* update = GetUpdate(updateHandle); if(update == NULL) return FALSE;
    INT32 partIdx = 0;
    INT32 pktSize    = update->Header.PacketSize;
    INT32 updateSize = update->Header.UpdateSize;
    INT32 cnt        = pPacketBits == NULL ? (updateSize + 31) >> 5 : *pCount;
    INT32 offset     = 0;

    if(update->Providers->Storage == NULL) return FALSE;
    if(!update->IsAuthenticated()        ) return FALSE;

    *pCount = 0;

    updateSize += offset;

    while(offset < updateSize && partIdx < cnt)
    {
        for(int i=0; i<32 && offset < updateSize; i++)
        {
            if(offset + pktSize > updateSize)
            {
                pktSize = updateSize - offset;
            }
            
            if(update->Providers->Storage->IsErased != NULL && update->Providers->Storage->IsErased(update->StorageHandle, offset, pktSize))
            {
                *pCount++;
                
                if(pPacketBits != NULL)
                {
                    pPacketBits[partIdx] |= 1u << i;
                }
            }
            else
            {
                pPacketBits[partIdx] &= ~(1u <<i);
            }

            offset += pktSize;
        }

        partIdx++;
    }

    return TRUE;
}

BOOL MFUpdate_AddPacket( INT32 updateHandle, INT32 packetIndex, UINT8* packetData, INT32 packetLen, UINT8* pValidationData, INT32 validationLen )
{
    MFUpdate* update = GetUpdate(updateHandle); if(update  == NULL) return FALSE;
    BOOL ret = FALSE;

    if(update->Providers->Storage == NULL) return FALSE;    
    if(!update->IsAuthenticated()        ) return FALSE;

    if( pValidationData != NULL && validationLen > 0 && 
        update->Providers->Validation != NULL && 
        update->Providers->Validation->ValidatePacket != NULL)
    {
        if(!update->Providers->Validation->ValidatePacket(update, packetData, packetLen, pValidationData, validationLen ))
        {
            return FALSE;
        }
    }

    ret = packetLen == update->Providers->Storage->Write( update->StorageHandle, packetIndex * update->Header.PacketSize, packetData, packetLen );

    return ret;
}

BOOL MFUpdate_Validate( INT32 updateHandle, UINT8* pValidationData, INT32 validationLen )
{
    BOOL fValid = TRUE;
    MFUpdate* update = GetUpdate(updateHandle);  if(update == NULL) return FALSE;

    if(!update->IsAuthenticated()) return FALSE;

    if(update->IsValidated()) return TRUE;

    if(update->Providers->Validation != NULL && update->Providers->Validation->ValidateUpdate != NULL)
    {
        fValid = update->Providers->Validation->ValidateUpdate( update, pValidationData, validationLen );       

        if(fValid) update->Flags |= MFUPDATE_FLAGS__VALIDATED;
        else       update->Flags &= ~MFUPDATE_FLAGS__VALIDATED;
    }

    if(update->Providers->Storage != NULL && update->Providers->Storage->Close != NULL)
    {
        update->Providers->Storage->Close(update->StorageHandle);
    }

    update->Flags |= MFUPDATE_FLAGS__VALIDATED;

    return fValid;
}

BOOL MFUpdate_Install( INT32 updateHandle, UINT8* pValidationData, INT32 validationLen )
{
    MFUpdate* update = GetUpdate(updateHandle);  if(update == NULL) return FALSE;

    if(!update->IsAuthenticated()) return FALSE;

    if(!update->IsValidated())
    {
        if(!MFUpdate_Validate( updateHandle, pValidationData, validationLen ))
        {
            return FALSE;
        }
    }

    return update->Providers->Update->InstallUpdate( update, pValidationData, validationLen );
}

BOOL MFUpdate_Delete( INT32 updateHandle )
{
    MFUpdate* update = GetUpdate(updateHandle); if(update == NULL) return FALSE;
    BOOL fRet;

    if(update->Providers->Storage == NULL) return FALSE;
    if(!update->IsAuthenticated()        ) return FALSE;

    if(update->Providers->Storage->Delete != NULL)
    {
        fRet = update->Providers->Storage->Delete( update->Header.UpdateID, update->Header.UpdateType, update->Header.UpdateSubType );
    }

    ReleaseUpdateHandle( updateHandle );

    return fRet;
}

