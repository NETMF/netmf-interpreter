/**
 * @file
 */
/******************************************************************************
 * Copyright (c) 2014 AllSeen Alliance. All rights reserved.
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
#define AJ_MODULE KEYEXCHANGE

#include "aj_target.h"
#include "aj_debug.h"
#include "aj_keyexchange.h"
#include "aj_crypto_ecc.h"
#include "aj_cert.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgKEYEXCHANGE = 0;
#endif

/*
 * ECC curve paramaters (version number)
 * In this code, we only support NIST P256
 */
#define ECC_NIST_P256 0

static AJ_Status ECDHE_Init(AJ_SHA256_Context* hash);
static AJ_Status ECDHE_Marshal(AJ_Message* msg);
static AJ_Status ECDHE_Unmarshal(AJ_Message* msg, uint8_t** secret, size_t* secretlen);

AJ_KeyExchange AJ_KeyExchangeECDHE = {
    ECDHE_Init,
    ECDHE_Marshal,
    ECDHE_Unmarshal
};

typedef struct _AJ_ECDHEContext {
    ecc_publickey publickey;
    ecc_privatekey privatekey;
    uint8_t secret[sizeof (ecc_secret)];
    AJ_SHA256_Context* hash;
} AJ_ECDHEContext;


static AJ_ECDHEContext ecdhectx;

static AJ_Status ECDHE_Init(AJ_SHA256_Context* hash)
{
    ecdhectx.hash = hash;
    return AJ_GenerateDHKeyPair(&ecdhectx.publickey, &ecdhectx.privatekey);
}

static AJ_Status ECDHE_Marshal(AJ_Message* msg)
{
    AJ_Status status;
    uint8_t b8[1 + sizeof (ecc_publickey)];

    AJ_InfoPrintf(("AJ_ECDHE_Marshal(msg=0x%p)\n", msg));

    status = AJ_MarshalVariant(msg, "ay");
    if (AJ_OK != status) {
        AJ_ErrPrintf(("AJ_ECDHE_Marshal(msg=0x%p): Marshal variant error\n", msg));
        return status;
    }
    b8[0] = ECC_NIST_P256;
    AJ_BigEndianEncodePublicKey(&ecdhectx.publickey, &b8[1]);
    status = AJ_MarshalArgs(msg, "ay", b8, sizeof (b8));
    if (AJ_OK != status) {
        AJ_ErrPrintf(("AJ_ECDHE_Marshal(msg=0x%p): Marshal key material error\n", msg));
        return status;
    }
    AJ_SHA256_Update(ecdhectx.hash, b8, sizeof (b8));

    return AJ_OK;
}

static AJ_Status ECDHE_Unmarshal(AJ_Message* msg, uint8_t** secret, size_t* secretlen)
{
    AJ_Status status;
    char* variant;
    uint8_t* data;
    size_t datalen;
    ecc_publickey publickey;
    ecc_secret tmp;

    AJ_InfoPrintf(("AJ_ECDHE_Unmarshal(msg=0x%p)\n", msg));

    status = AJ_UnmarshalVariant(msg, (const char**) &variant);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("AJ_ECDHE_Unmarshal(msg=0x%p): Unmarshal variant error\n", msg));
        return status;
    }
    if (0 != strncmp(variant, "ay", 2)) {
        AJ_InfoPrintf(("AJ_ECDHE_Unmarshal(msg=0x%p): Invalid variant\n", msg));
        return AJ_ERR_SECURITY;
    }
    status = AJ_UnmarshalArgs(msg, "ay", &data, &datalen);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("AJ_ECDHE_Unmarshal(msg=0x%p): Unmarshal key material error\n", msg));
        return status;
    }
    if (1 + sizeof (ecc_publickey) != datalen) {
        AJ_InfoPrintf(("AJ_ECDHE_Unmarshal(msg=0x%p): Invalid key material\n", msg));
        return AJ_ERR_SECURITY;
    }
    if (ECC_NIST_P256 != data[0]) {
        AJ_InfoPrintf(("AJ_ECDHE_Unmarshal(msg=0x%p): Invalid curve\n", msg));
        return AJ_ERR_SECURITY;
    }
    AJ_SHA256_Update(ecdhectx.hash, data, datalen);

    AJ_BigEndianDecodePublicKey(&publickey, &data[1]);
    status = AJ_GenerateShareSecret(&publickey, &ecdhectx.privatekey, &tmp);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("AJ_ECDHE_Unmarshal(msg=0x%p): Generate secret error\n", msg));
        return status;
    }
    AJ_BigEndianEncodePublicKey(&tmp, ecdhectx.secret);
    *secret = ecdhectx.secret;
    *secretlen = sizeof (ecdhectx.secret);

    return status;
}
