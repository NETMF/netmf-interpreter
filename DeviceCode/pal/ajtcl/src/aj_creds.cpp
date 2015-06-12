/**
 * @file
 */
/******************************************************************************
 * Copyright AllSeen Alliance. All rights reserved.
 *
 *    Permission to use, copy, modify, and/or distribute this software for any
 *    purpose with or without fee is hereby granted, provided that the above
 *    copyright notice and this permission notice appear in all copies.
 *
 *    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 *    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 *    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 *    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 *    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 *    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 *    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 ******************************************************************************/

/**
 * Per-module definition of the current module for debug logging.  Must be defined
 * prior to first inclusion of aj_debug.h
 */
#define AJ_MODULE CREDS

#include "aj_target.h"
#include "aj_creds.h"
#include "aj_status.h"
#include "aj_crypto.h"
#include "aj_nvram.h"
#include "aj_debug.h"
#include "aj_config.h"
#include "aj_util.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgCREDS = 0;
#endif

static AJ_Status FreeCredentialContent(AJ_PeerCred* cred)
{
    if (!cred) {
        return AJ_OK;
    }
    if ((cred->idLen > 0) && cred->id) {
        AJ_MemZeroSecure(cred->id, cred->idLen);
        AJ_Free(cred->id);
        cred->idLen = 0;
    }
    if ((cred->associationLen > 0) && cred->association) {
        AJ_MemZeroSecure(cred->association, cred->associationLen);
        AJ_Free(cred->association);
        cred->associationLen = 0;
    }
    if ((cred->dataLen > 0) && cred->data) {
        AJ_MemZeroSecure(cred->data, cred->dataLen);
        AJ_Free(cred->data);
        cred->dataLen = 0;
    }

    return AJ_OK;
}

static uint16_t FindCredsEmptySlot()
{
    uint16_t id = AJ_CREDS_NV_ID_BEGIN;

    AJ_InfoPrintf(("FindCredsEmptySlot()\n"));

    for (; id < AJ_CREDS_NV_ID_END; id++) {
        if (!AJ_NVRAM_Exist(id)) {
            return id;
        }
    }

    return 0;
}

static AJ_Status ReadRemainderOfCredential(AJ_NV_DATASET* handle, AJ_PeerCred* cred)
{
    size_t size, toRead;

    /* Read expiration */
    toRead = sizeof(cred->expiration);
    size = AJ_NVRAM_Read(&cred->expiration, toRead, handle);
    if (toRead != size) {
        AJ_ErrPrintf(("ReadRemainderOfCredential(): AJ_ERR_FAILURE on read failure of field expiration\n"));
        return AJ_ERR_FAILURE;
    }
    /* Read association length */
    toRead = sizeof(cred->associationLen);
    size = AJ_NVRAM_Read(&cred->associationLen, toRead, handle);
    if (toRead != size) {
        AJ_ErrPrintf(("ReadRemainderOfCredential(): AJ_ERR_FAILURE on read failure\n"));
        return AJ_ERR_FAILURE;
    }
    if (cred->associationLen > 0) {
        cred->association = (uint8_t*) AJ_Malloc(cred->associationLen);
        if (!cred->association) {
            AJ_ErrPrintf(("ReadRemainderOfCredential(): AJ_ERR_FAILURE on memory allocation failure of size %d\n", cred->associationLen));
            return AJ_ERR_FAILURE;
        }
        /* Read association */
        toRead = cred->associationLen;
        size = AJ_NVRAM_Read(cred->association, toRead, handle);
        if (toRead != size) {
            AJ_ErrPrintf(("ReadRemainderOfCredential(): AJ_ERR_FAILURE on read failure on field association\n"));
            return AJ_ERR_FAILURE;
        }
    }
    /* Read data length */
    toRead = sizeof(cred->dataLen);
    size = AJ_NVRAM_Read(&cred->dataLen, toRead, handle);
    if (toRead != size) {
        AJ_ErrPrintf(("ReadRemainderOfCredential(): AJ_ERR_FAILURE on read failure on field dataLen\n"));
        return AJ_ERR_FAILURE;
    }
    if (cred->dataLen > 0) {
        cred->data = (uint8_t*) AJ_Malloc(cred->dataLen);
        if (!cred->data) {
            AJ_ErrPrintf(("ReadRemainderOfCredential(): AJ_ERR_FAILURE on memory allocation failure for size %d\n", cred->dataLen));
            return AJ_ERR_FAILURE;
        }
        /* Read data */
        toRead = cred->dataLen;
        size = AJ_NVRAM_Read(cred->data, toRead, handle);
        if (toRead != size) {
            AJ_ErrPrintf(("ReadRemainderOfCredential(): AJ_ERR_FAILURE on read failure on field data\n"));
            return AJ_ERR_FAILURE;
        }
    }
    return AJ_OK;
}

static uint16_t FindCredential(const uint16_t credType, const uint8_t* credId, uint8_t credIdLen, AJ_PeerCred** credHolder)
{
    AJ_Status status;
    uint16_t slot = AJ_CREDS_NV_ID_BEGIN;
    AJ_NV_DATASET* handle;
    uint16_t localCredType;
    uint8_t localCredIdLen;
    uint8_t* localCredId;
    AJ_PeerCred* cred;
    int32_t idMatch;

    AJ_InfoPrintf(("FindCredential(type=0x%04X, id=0x%p, len=%d)\n", credType, credId, credIdLen));

    for (; slot < AJ_CREDS_NV_ID_END; slot++) {
        if (AJ_NVRAM_Exist(slot)) {
            handle = AJ_NVRAM_Open(slot, "r", 0);
            if (!handle) {
                AJ_ErrPrintf(("FindCredential(): fail to open data set with slot = %d\n", slot));
                continue;
            }
            /* Read type */
            if (sizeof(localCredType) != AJ_NVRAM_Read(&localCredType, sizeof(localCredType), handle)) {
                AJ_ErrPrintf(("FindCredential(): fail to read credential type %zu bytes from data set with slot = %d\n", sizeof(credType), slot));
                AJ_NVRAM_Close(handle);
                continue;
            }
            if (localCredType != credType) {
                AJ_NVRAM_Close(handle);
                continue;
            }
            /* Read id length */
            if (sizeof(localCredIdLen) != AJ_NVRAM_Read(&localCredIdLen, sizeof(localCredIdLen), handle)) {
                AJ_ErrPrintf(("FindCredential(): fail to read slot length %zu bytes from data set with slot = %d\n", sizeof(localCredIdLen), slot));
                AJ_NVRAM_Close(handle);
                continue;
            }
            if (localCredIdLen != credIdLen) {
                AJ_NVRAM_Close(handle);
                continue;
            }
            if (!localCredIdLen) {
                AJ_NVRAM_Close(handle);
                continue;
            }
            localCredId = (uint8_t*) AJ_Malloc(localCredIdLen);
            if (!localCredId) {
                AJ_ErrPrintf(("FindCredential(): AJ_ERR_RESOURCES\n"));
                AJ_NVRAM_Close(handle);
                return AJ_ERR_RESOURCES;
            }
            /* Read id */
            if (localCredIdLen != AJ_NVRAM_Read(localCredId, localCredIdLen, handle)) {
                AJ_ErrPrintf(("FindCredential(): fail to read slot %u bytes from data set with slot = %d\n", localCredIdLen, slot));
                AJ_NVRAM_Close(handle);
                AJ_Free(localCredId);
                continue;
            }

            idMatch = memcmp(localCredId, credId, credIdLen);
            if (idMatch != 0) {
                /* no match */
                AJ_Free(localCredId);
                AJ_NVRAM_Close(handle);
                continue;
            }
            if (!credHolder) {
                /* short query */
                AJ_Free(localCredId);
                AJ_NVRAM_Close(handle);
                return slot;  /* short query */
            }
            /* full query */
            cred = (AJ_PeerCred*) AJ_Malloc(sizeof(AJ_PeerCred));
            if (!cred) {
                AJ_Free(localCredId);
                AJ_NVRAM_Close(handle);
                return AJ_ERR_RESOURCES;
            }
            cred->type = localCredType;
            cred->idLen = localCredIdLen;
            cred->id = localCredId;
            cred->dataLen = 0;
            cred->associationLen = 0;
            status = ReadRemainderOfCredential(handle, cred);
            AJ_NVRAM_Close(handle);
            if (status != AJ_OK) {
                AJ_ErrPrintf(("FindCredential(): AJ_ERR_FAILURE on read failure \n"));
                AJ_FreeCredential(cred);
                return 0;
            }
            *credHolder = cred;
            return slot;  /* found */
        }
    }
    return 0; /* not found */
}

AJ_Status AJ_GetCredential(const uint16_t credType, const uint8_t* id, uint8_t idLen, AJ_PeerCred** customCredHolder)
{
    uint16_t slot = FindCredential(credType, id, idLen, customCredHolder);

    AJ_InfoPrintf(("AJ_GetCredential(type=0x%04X, id=0x%p, len=%d)\n", credType, id, idLen));

    if (slot) {
        return AJ_OK;
    }

    return AJ_ERR_FAILURE;
}

static size_t GetCredentialSize(AJ_PeerCred* cred)
{
    // type(2):idLen(1):id(idLen):expiration(4):assLen(1):ass(assLen):dataLen(2):data(dataLen)
    return sizeof(cred->type) +
           sizeof(cred->idLen) + cred->idLen +
           sizeof(cred->expiration) +
           sizeof(cred->associationLen) + cred->associationLen +
           sizeof(cred->dataLen) + cred->dataLen;
}

static AJ_Status UpdateCred(AJ_PeerCred* aCred, uint16_t slot)
{
    AJ_Status status = AJ_OK;
    AJ_NV_DATASET* handle;
    size_t len;
    size_t size, toWrite;

    AJ_InfoPrintf(("UpdateCred(aCred=0x%p, slot=%d)\n", aCred, slot));

    if (!aCred) {
        AJ_ErrPrintf(("UpdateCred(): AJ_ERR_FAILURE on null credential\n"));
        return AJ_ERR_FAILURE;
    }
    len = GetCredentialSize(aCred);
    handle = AJ_NVRAM_Open(slot, "w", len);
    if (!handle) {
        AJ_ErrPrintf(("UpdateCred(): AJ_ERR_FAILURE\n"));
        return AJ_ERR_FAILURE;
    }

    /* Write type */
    toWrite = sizeof(aCred->type);
    size = AJ_NVRAM_Write(&aCred->type, toWrite, handle);
    if (toWrite != size) {
        AJ_ErrPrintf(("UpdateCred(): AJ_ERR_FAILURE on write failure on type field\n"));
        goto Exit;
    }
    /* Write id length and optional id */
    toWrite = sizeof(aCred->idLen);
    size = AJ_NVRAM_Write(&aCred->idLen, toWrite, handle);
    if (toWrite != size) {
        AJ_ErrPrintf(("UpdateCred(): AJ_ERR_FAILURE on write failure on idLen field\n"));
        goto Exit;
    }
    if (aCred->idLen > 0) {
        toWrite = aCred->idLen;
        size = AJ_NVRAM_Write(aCred->id, toWrite, handle);
        if (toWrite != size) {
            AJ_ErrPrintf(("UpdateCred(): AJ_ERR_FAILURE on write failure on id field\n"));
            goto Exit;
        }
    }
    /* Write expiration */
    toWrite = sizeof(aCred->expiration);
    size = AJ_NVRAM_Write(&aCred->expiration, toWrite, handle);
    if (toWrite != size) {
        AJ_ErrPrintf(("UpdateCred(): AJ_ERR_FAILURE on write failure on field expiration\n"));
        goto Exit;
    }
    /* Write association length and optional association */
    toWrite = sizeof(aCred->associationLen);
    size = AJ_NVRAM_Write(&aCred->associationLen, toWrite, handle);
    if (toWrite != size) {
        AJ_ErrPrintf(("UpdateCred(): AJ_ERR_FAILURE on write failure on field associationLen\n"));
        goto Exit;
    }
    if (aCred->associationLen > 0) {
        toWrite = aCred->associationLen;
        size = AJ_NVRAM_Write(aCred->association, toWrite, handle);
        if (toWrite != size) {
            AJ_ErrPrintf(("UpdateCred(): AJ_ERR_FAILURE on write failure on field association\n"));
            goto Exit;
        }
    }
    /* Write data length and optional data */
    toWrite = sizeof(aCred->dataLen);
    size = AJ_NVRAM_Write(&aCred->dataLen, toWrite, handle);
    if (toWrite != size) {
        AJ_ErrPrintf(("UpdateCred(): AJ_ERR_FAILURE on write failure on field dataLen\n"));
        goto Exit;
    }
    if (aCred->dataLen > 0) {
        toWrite = aCred->dataLen;
        size = AJ_NVRAM_Write(aCred->data, toWrite, handle);
        if (toWrite != size) {
            AJ_ErrPrintf(("UpdateCred(): AJ_ERR_FAILURE on write failure on field data\n"));
            goto Exit;
        }
    }
    status = AJ_NVRAM_Close(handle);
    return status;

Exit:
    status = AJ_NVRAM_Close(handle);
    status = AJ_ERR_FAILURE;

    return status;
}

/*
 * Finds the oldest credential and deletes it
 * @param[out] deleteSlot holder for the slot of deleted credential or zero if not deleted
 * @return AJ_OK if there is no reading error.
 */
static AJ_Status AJ_DeleteOldestCredential(uint16_t* deleteSlot)
{
    AJ_Status status = AJ_ERR_INVALID;
    uint16_t slot = AJ_CREDS_NV_ID_BEGIN;
    AJ_NV_DATASET* handle;
    uint16_t oldestslot = 0;
    uint32_t oldestexp = 0xFFFFFFFF;
    uint16_t localCredType;
    uint8_t localCredIdLen;
    uint8_t* localCredId;
    AJ_PeerCred cred;

    AJ_InfoPrintf(("AJ_DeleteOldestCredential()\n"));

    *deleteSlot = 0;
    for (; slot < AJ_CREDS_NV_ID_END; slot++) {
        if (!AJ_NVRAM_Exist(slot)) {
            continue;
        }
        handle = AJ_NVRAM_Open(slot, "r", 0);
        if (!handle) {
            AJ_ErrPrintf(("AJ_DeleteOldestCredential(): fail to open data set with slot = %d\n", slot));
            continue;
        }
        /* Read type */
        if (sizeof(localCredType) != AJ_NVRAM_Read(&localCredType, sizeof(localCredType), handle)) {
            AJ_ErrPrintf(("AJ_DeleteOldestCredential(): fail to read credential type %zu bytes from data set with slot = %d\n", sizeof(localCredType), slot));
            AJ_NVRAM_Close(handle);
            continue;
        }
        /* Read id length */
        if (sizeof(localCredIdLen) != AJ_NVRAM_Read(&localCredIdLen, sizeof(localCredIdLen), handle)) {
            AJ_ErrPrintf(("AJ_DeleteOldestCredential(): fail to read slot length %zu bytes from data set with slot = %d\n", sizeof(localCredIdLen), slot));
            AJ_NVRAM_Close(handle);
            continue;
        }
        if (!localCredIdLen) {
            AJ_ErrPrintf(("AJ_DeleteOldestCredential(): id length is zero in slot %d\n", slot));
            AJ_NVRAM_Close(handle);
            continue;
        }
        localCredId = (uint8_t*) AJ_Malloc(localCredIdLen);
        if (!localCredId) {
            AJ_ErrPrintf(("AJ_DeleteOldestCredential(): AJ_ERR_RESOURCES\n"));
            AJ_NVRAM_Close(handle);
            continue;
        }
        /* Read id */
        if (localCredIdLen != AJ_NVRAM_Read(localCredId, localCredIdLen, handle)) {
            AJ_ErrPrintf(("AJ_DeleteOldestCredential(): fail to read slot %u bytes from data set with slot = %d\n", localCredIdLen, slot));
            AJ_NVRAM_Close(handle);
            AJ_Free(localCredId);
            continue;
        }

        /* full query */
        cred.type = localCredType;
        cred.idLen = localCredIdLen;
        cred.id = localCredId;
        cred.dataLen = 0;
        cred.associationLen = 0;
        status = ReadRemainderOfCredential(handle, &cred);
        AJ_NVRAM_Close(handle);
        if (status != AJ_OK) {
            AJ_ErrPrintf(("AJ_DeleteOldestCredential(): AJ_ERR_FAILURE on read failure \n"));
            FreeCredentialContent(&cred);
            if (localCredId) {
                AJ_Free(localCredId);
            }
            return status;
        }

        /* if older and type GENERIC (master secret) */
        if ((cred.expiration <= oldestexp) && (AJ_CRED_TYPE_GENERIC == cred.type)) {
            oldestexp = cred.expiration;
            oldestslot = slot;
        }
        FreeCredentialContent(&cred);
        if (localCredId) {
            AJ_Free(localCredId);
        }
    }

    if (oldestslot) {
        AJ_InfoPrintf(("AJ_DeleteOldestCredential(): slot=%d exp=0x%08X\n", oldestslot, oldestexp));
        AJ_NVRAM_Delete(oldestslot);
    }

    *deleteSlot = oldestslot;
    return AJ_OK;
}

AJ_Status AJ_StoreCredential(AJ_PeerCred* aCred)
{
    AJ_Status status = AJ_OK;
    uint16_t slot;
    uint32_t len;

    AJ_InfoPrintf(("AJ_StoreCredential(aCred=0x%p)\n", aCred));

    slot = FindCredential(aCred->type, aCred->id, aCred->idLen, NULL);
    if (!slot) {
        /*
         * Check there is sufficient space left.
         * If there isn't, keep deleting oldest credential until there is.
         */
        len = GetCredentialSize(aCred);
        len = WORD_ALIGN(len);
        slot = FindCredsEmptySlot();
        while ((AJ_OK == status) && (!slot || (len >= AJ_NVRAM_GetSizeRemaining()))) {
            AJ_InfoPrintf(("AJ_StoreCredential(aCred=0x%p): Remaining %d Required %d Slot %d\n", aCred, AJ_NVRAM_GetSizeRemaining(), len, slot));
            status = AJ_DeleteOldestCredential(&slot);
        }
    }

    if (slot) {
        status = UpdateCred(aCred, slot);
    } else {
        status = AJ_ERR_FAILURE;
        AJ_ErrPrintf(("AJ_StoreCredential(aCred=0x%p): AJ_ERR_FAILURE\n", aCred));
    }

    return status;
}

AJ_Status AJ_StorePeerSecret(const AJ_GUID* peerGuid, const uint8_t* secret,
                             const uint8_t len, uint32_t expiration)
{
    AJ_PeerCred cred;
    AJ_Status status;

    AJ_InfoPrintf(("AJ_StorePeerSecret(peerGuid=0x%p, secret=0x%p, len=%d, expiration=0x%08X)\n", peerGuid, secret, len, expiration));

    cred.type = AJ_CRED_TYPE_GENERIC;
    cred.idLen = sizeof(AJ_GUID);
    cred.id = (uint8_t*) peerGuid;
    cred.expiration = expiration;
    cred.associationLen = 0;
    cred.association = NULL;
    cred.dataLen = len;
    cred.data = (uint8_t*) secret;
    status = AJ_StoreCredential(&cred);

    return status;
}

AJ_Status AJ_DeleteCredential(const uint16_t credType, const uint8_t* id, uint8_t idLen)
{
    AJ_Status status = AJ_ERR_FAILURE;
    uint16_t slot = FindCredential(credType, id, idLen, NULL);

    AJ_InfoPrintf(("AJ_DeleteCredential(type=0x%04X, id=0x%p, len=%d\n", credType, id, idLen));

    if (slot > 0) {
        status = AJ_NVRAM_Delete(slot);
    }

    return status;
}

AJ_Status AJ_DeletePeerCredential(const AJ_GUID* peerGuid)
{
    return AJ_DeleteCredential(AJ_CRED_TYPE_GENERIC, peerGuid->val, sizeof(AJ_GUID));
}

AJ_Status AJ_ClearCredentials(void)
{
    AJ_Status status = AJ_OK;
    uint16_t id = AJ_CREDS_NV_ID_BEGIN;

    AJ_InfoPrintf(("AJ_ClearCredentials()\n"));

    for (; id < AJ_CREDS_NV_ID_END; ++id) {
        if (AJ_NVRAM_Exist(id)) {
            AJ_NVRAM_Delete(id);
        }
    }

    return status;
}

AJ_Status AJ_GetPeerCredential(const AJ_GUID* peerGuid, AJ_PeerCred** peerCredHolder)
{
    return AJ_GetCredential(AJ_CRED_TYPE_GENERIC, peerGuid->val, sizeof(peerGuid->val), peerCredHolder);
}

AJ_Status AJ_GetLocalGUID(AJ_GUID* localGuid)
{
    AJ_Status status = AJ_ERR_FAILURE;
    AJ_NV_DATASET* handle;

    AJ_InfoPrintf(("AJ_GetLocalGUID(localGuid=0x%p)\n", localGuid));

    if (AJ_NVRAM_Exist(AJ_LOCAL_GUID_NV_ID)) {
        handle = AJ_NVRAM_Open(AJ_LOCAL_GUID_NV_ID, "r", 0);
        if (handle) {
            if (sizeof(AJ_GUID) == AJ_NVRAM_Read(localGuid, sizeof(AJ_GUID), handle)) {
                status = AJ_OK;
            } else {
                AJ_ErrPrintf(("AJ_GetLocalGUID(): fail to read slot length %zu bytes from slot = %d\n", sizeof(AJ_GUID), AJ_LOCAL_GUID_NV_ID));
            }
            status = AJ_NVRAM_Close(handle);
        }
    } else {
        /* define AJ_CreateNewGUID to AJ_RandBytes in order to generate a random GUID */
        AJ_CreateNewGUID((uint8_t*)localGuid, sizeof(AJ_GUID));
        handle = AJ_NVRAM_Open(AJ_LOCAL_GUID_NV_ID, "w", sizeof(AJ_GUID));
        if (handle) {
            if (sizeof(AJ_GUID) == AJ_NVRAM_Write(localGuid, sizeof(AJ_GUID), handle)) {
                status = AJ_OK;
            } else {
                AJ_ErrPrintf(("AJ_GetLocalGUID(): fail to write slot length %zu bytes to slot = %d\n", sizeof(AJ_GUID), AJ_LOCAL_GUID_NV_ID));
            }
            status = AJ_NVRAM_Close(handle);
        }
    }

    return status;
}

AJ_Status AJ_FreeCredential(AJ_PeerCred* cred)
{
    if (!cred) {
        return AJ_OK;
    }
    FreeCredentialContent(cred);
    AJ_Free(cred);

    return AJ_OK;
}

AJ_Status AJ_StoreLocalCredential(const uint16_t credType, const uint16_t id, const uint8_t* data, const uint8_t len, uint32_t expiration)
{
    AJ_PeerCred cred;

    AJ_InfoPrintf(("AJ_StoreLocalCredential(type=0x%04X, id=0x%p, data=0x%p, len=%d, expiration=0x%08X)\n", credType, &id, data, len, expiration));

    cred.type = credType;
    cred.idLen = sizeof(id);
    cred.id = (uint8_t*) &id;
    cred.expiration = expiration;
    cred.associationLen = 0;
    cred.association = 0;
    cred.dataLen = len;
    cred.data = (uint8_t*) data;

    return AJ_StoreCredential(&cred);
}

AJ_Status AJ_GetLocalCredential(const uint16_t credType, const uint16_t id, AJ_PeerCred** credHolder)
{
    return AJ_GetCredential(credType, (const uint8_t*) &id, sizeof(id), credHolder);
}

AJ_Status AJ_DeleteLocalCredential(const uint16_t credType, const uint16_t id)
{
    return AJ_DeleteCredential(credType, (const uint8_t*) &id, sizeof(id));
}

AJ_Status AJ_CredentialExpired(AJ_PeerCred* cred)
{
    AJ_Time now;

    AJ_InitTimer(&now);
    if (now.seconds == 0) {
        /* don't know the current time so can't check the credential expriy */
        return AJ_ERR_INVALID;
    }

    if (cred->expiration > now.seconds) {
        return AJ_OK;
    }

    return AJ_ERR_KEY_EXPIRED; /* expires */
}
