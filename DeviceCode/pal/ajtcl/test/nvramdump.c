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

#include <alljoyn.h>
#include <aj_creds.h>
#include <aj_nvram.h>
#include <aj_crypto_ecc.h>

extern void AJ_NVRAM_Layout_Print();
AJ_Status DumpNVRAM();

void printhex(uint8_t*x, size_t n)
{
    size_t i;
    for (i = 0; i < n; i++) {
        AJ_AlwaysPrintf(("%02X", x[i]));
    }
}

static AJ_Status ReadRemainderOfCredential(AJ_NV_DATASET* handle, AJ_PeerCred* cred) {
    size_t size, toRead;
    toRead = sizeof(cred->expiration);
    size = AJ_NVRAM_Read(&cred->expiration, toRead, handle);
    if (toRead != size) {
        return AJ_ERR_FAILURE;
    }
    toRead = sizeof(cred->associationLen);
    size = AJ_NVRAM_Read(&cred->associationLen, toRead, handle);
    if (toRead != size) {
        return AJ_ERR_FAILURE;
    }
    if (cred->associationLen > 0) {
        cred->association = AJ_Malloc(cred->associationLen);
        if (!cred->association) {
            return AJ_ERR_FAILURE;
        }
        toRead = cred->associationLen;
        size = AJ_NVRAM_Read(cred->association, toRead, handle);
        if (toRead != size) {
            return AJ_ERR_FAILURE;
        }
    }
    toRead = sizeof(cred->dataLen);
    size = AJ_NVRAM_Read(&cred->dataLen, toRead, handle);
    if (toRead != size) {
        return AJ_ERR_FAILURE;
    }
    if (cred->dataLen > 0) {
        cred->data = AJ_Malloc(cred->dataLen);
        if (!cred->data) {
            return AJ_ERR_FAILURE;
        }
        toRead = cred->dataLen;
        size = AJ_NVRAM_Read(cred->data, toRead, handle);
        if (toRead != size) {
            return AJ_ERR_FAILURE;
        }
    }
    return AJ_OK;
}
AJ_Status DumpNVRAM()
{
    uint16_t slot = AJ_CREDS_NV_ID_BEGIN;
    AJ_NV_DATASET* handle;
    uint16_t localCredType;
    uint8_t localCredIdLen;
    uint8_t* localCredId;
    AJ_PeerCred* cred;
    AJ_Status status;

    AJ_NVRAM_Layout_Print();
    AJ_AlwaysPrintf(("Remaining Size %d\n", AJ_NVRAM_GetSizeRemaining()));

    AJ_AlwaysPrintf(("SLOT | TYPE | ID | EXPIRATION | ASSOCIATION | DATA\n"));
    for (; slot < AJ_CREDS_NV_ID_END; slot++) {
        if (!AJ_NVRAM_Exist(slot)) {
            continue;
        }
        handle = AJ_NVRAM_Open(slot, "r", 0);
        if (!handle) {
            continue;
        }
        if (sizeof(localCredType) != AJ_NVRAM_Read(&localCredType, sizeof(localCredType), handle)) {
            AJ_NVRAM_Close(handle);
            continue;
        }
        if (sizeof(localCredIdLen) != AJ_NVRAM_Read(&localCredIdLen, sizeof(localCredIdLen), handle)) {
            AJ_NVRAM_Close(handle);
            continue;
        }
        localCredId = AJ_Malloc(localCredIdLen);
        if (!localCredId) {
            AJ_NVRAM_Close(handle);
            return AJ_ERR_RESOURCES;
        }
        if (localCredIdLen != AJ_NVRAM_Read(localCredId, localCredIdLen, handle)) {
            AJ_Free(localCredId);
            AJ_NVRAM_Close(handle);
            continue;
        }

        cred = AJ_Malloc(sizeof(AJ_PeerCred));
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
            AJ_FreeCredential(cred);
            continue;
        }

        AJ_AlwaysPrintf(("%04X | %04X | ", slot, cred->type));
        printhex(cred->id, cred->idLen);
        AJ_AlwaysPrintf((" | %08X | ", cred->expiration));
        printhex(cred->association, cred->associationLen);
        AJ_AlwaysPrintf((" | "));
        printhex(cred->data, cred->dataLen);
        AJ_AlwaysPrintf(("\n"));
        AJ_FreeCredential(cred);
    }
    return AJ_OK;
}


int AJ_Main()
{
    AJ_Status status = AJ_OK;
    AJ_Initialize();
    //AJ_NVRAM_Clear();
    //AJ_AlwaysPrintf(("Clearing NVRAM\n"));
    status = DumpNVRAM();
    AJ_ASSERT(status == AJ_OK);
    return 0;
}

#ifdef AJ_MAIN
int main()
{
    return AJ_Main();
}
#endif
