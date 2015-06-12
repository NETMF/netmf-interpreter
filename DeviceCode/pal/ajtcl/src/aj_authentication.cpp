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
#define AJ_MODULE AUTHENTICATION

#include "aj_target.h"
#include "aj_debug.h"
#include "aj_authentication.h"
#include "aj_cert.h"
#include "aj_peer.h"
#include "aj_creds.h"
#include "aj_auth_listener.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgAUTHENTICATION = 0;
#endif

// Enabled suites (this should initialize to zero)
static uint32_t suites[AJ_AUTH_SUITES_NUM];

#define ECC_NIST_P256      0
#define SIG_FMT            0
#define CERT_FMT_X509_DER  0
#define AUTH_VERIFIER_LEN  SHA256_DIGEST_LENGTH

static AJ_Status ComputeMasterSecret(AJ_AuthenticationContext* ctx, uint8_t* pms, size_t len)
{
    const uint8_t* data[2];
    uint8_t lens[2];

    AJ_InfoPrintf(("ComputeMasterSecret(ctx=%p, pms=%p, len=%d)\n", ctx, pms, len));

    data[0] = pms;
    lens[0] = len;
    data[1] = (uint8_t*) "master secret";
    lens[1] = 13;

    return AJ_Crypto_PRF_SHA256(data, lens, ArraySize(data), ctx->mastersecret, AJ_MASTER_SECRET_LEN);
}

static AJ_Status ComputeVerifier(AJ_AuthenticationContext* ctx, const char* label, uint8_t* buffer, size_t bufferlen)
{
    const uint8_t* data[3];
    uint8_t lens[3];
    uint8_t digest[SHA256_DIGEST_LENGTH];

    AJ_SHA256_GetDigest(&ctx->hash, digest, 1);

    data[0] = ctx->mastersecret;
    lens[0] = AJ_MASTER_SECRET_LEN;
    data[1] = (uint8_t*) label;
    lens[1] = (uint8_t) strlen(label);
    data[2] = digest;
    lens[2] = sizeof (digest);

    return AJ_Crypto_PRF_SHA256(data, lens, ArraySize(data), buffer, bufferlen);
}

static AJ_Status ECDHEMarshalV1(AJ_AuthenticationContext* ctx, AJ_Message* msg)
{
    AJ_Status status;
    uint8_t buf[1 + sizeof (ecc_publickey)];

    AJ_InfoPrintf(("ECDHEMarshalV1(ctx=%p, msg=%p)\n", ctx, msg));

    // Encode the public key
    buf[0] = ECC_NIST_P256;
    AJ_BigEndianEncodePublicKey(&ctx->kectx.pub, &buf[1]);
    // Marshal the encoded key
    status = AJ_MarshalArgs(msg, "v", "ay", buf, sizeof (buf));
    AJ_SHA256_Update(&ctx->hash, buf, sizeof (buf));

    return status;
}

static AJ_Status ECDHEMarshalV2(AJ_AuthenticationContext* ctx, AJ_Message* msg)
{
    AJ_Status status;
    uint8_t buf[KEY_ECC_PUB_SZ];
    uint8_t fmt = ECC_NIST_P256;

    AJ_InfoPrintf(("ECDHEMarshalV2(ctx=%p, msg=%p)\n", ctx, msg));

    // Encode the public key
    AJ_BigvalEncode(&ctx->kectx.pub.x, buf, KEY_ECC_SZ);
    AJ_BigvalEncode(&ctx->kectx.pub.y, buf + KEY_ECC_SZ, KEY_ECC_SZ);
    // Marshal the encoded key
    status = AJ_MarshalArgs(msg, "v", "(yay)", fmt, buf, sizeof (buf));
    AJ_SHA256_Update(&ctx->hash, &fmt, sizeof (fmt));
    AJ_SHA256_Update(&ctx->hash, buf, sizeof (buf));

    return status;
}

static AJ_Status ECDHEMarshal(AJ_AuthenticationContext* ctx, AJ_Message* msg)
{
    AJ_Status status;

    AJ_InfoPrintf(("ECDHEMarshal(ctx=%p, msg=%p)\n", ctx, msg));

    // Generate key pair if client
    if (AUTH_CLIENT == ctx->role) {
        status = AJ_GenerateDHKeyPair(&ctx->kectx.pub, &ctx->kectx.prv);
        if (AJ_OK != status) {
            AJ_InfoPrintf(("ECDHEMarshal(ctx=%p, msg=%p): Key generation failed\n", ctx, msg));
            return status;
        }
    }

    if ((ctx->version >> 16) < 3) {
        status = ECDHEMarshalV1(ctx, msg);
    } else {
        status = ECDHEMarshalV2(ctx, msg);
    }

    return status;
}

static AJ_Status ECDHEUnmarshalV1(AJ_AuthenticationContext* ctx, AJ_Message* msg)
{
    AJ_Status status;
    uint8_t* data;
    size_t size;
    ecc_publickey pub;
    ecc_secret secret;

    AJ_InfoPrintf(("ECDHEUnmarshalV1(ctx=%p, msg=%p)\n", ctx, msg));

    // Unmarshal the encoded key
    status = AJ_UnmarshalArgs(msg, "v", "ay", &data, &size);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("ECDHEUnmarshalV1(ctx=%p, msg=%p): Unmarshal error\n", ctx, msg));
        return status;
    }
    if (1 + sizeof (ecc_publickey) != size) {
        AJ_InfoPrintf(("ECDHEUnmarshalV1(ctx=%p, msg=%p): Invalid key material\n", ctx, msg));
        return AJ_ERR_SECURITY;
    }
    if (ECC_NIST_P256 != data[0]) {
        AJ_InfoPrintf(("ECDHEUnmarshalV1(ctx=%p, msg=%p): Invalid curve\n", ctx, msg));
        return AJ_ERR_SECURITY;
    }
    // Decode the public key
    AJ_BigEndianDecodePublicKey(&pub, &data[1]);

    AJ_SHA256_Update(&ctx->hash, data, size);

    // Generate shared secret
    status = AJ_GenerateShareSecret(&pub, &ctx->kectx.prv, &secret);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("ECDHEUnmarshalV1(ctx=%p, msg=%p): Generate secret error\n", ctx, msg));
        return status;
    }

    // Encode the shared secret
    size = sizeof (ecc_secret);
    data = (uint8_t *)AJ_Malloc(size);
    if (NULL == data) {
        return AJ_ERR_RESOURCES;
    }
    AJ_BigEndianEncodePublicKey(&secret, data);
    status = ComputeMasterSecret(ctx, data, size);
    AJ_Free(data);

    return status;
}

static AJ_Status ECDHEUnmarshalV2(AJ_AuthenticationContext* ctx, AJ_Message* msg)
{
    AJ_Status status;
    uint8_t fmt;
    uint8_t* data;
    size_t size;
    ecc_publickey pub;
    ecc_secret secret;
    AJ_SHA256_Context sha;

    AJ_InfoPrintf(("ECDHEUnmarshalV2(ctx=%p, msg=%p)\n", ctx, msg));

    // Unmarshal the encoded key
    status = AJ_UnmarshalArgs(msg, "v", "(yay)", &fmt, &data, &size);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("ECDHEUnmarshalV2(ctx=%p, msg=%p): Unmarshal error\n", ctx, msg));
        return status;
    }
    if (ECC_NIST_P256 != fmt) {
        AJ_InfoPrintf(("ECDHEUnmarshalV2(ctx=%p, msg=%p): Invalid curve\n", ctx, msg));
        return AJ_ERR_SECURITY;
    }
    if (KEY_ECC_PUB_SZ != size) {
        AJ_InfoPrintf(("ECDHEUnmarshalV2(ctx=%p, msg=%p): Invalid key material\n", ctx, msg));
        return AJ_ERR_SECURITY;
    }
    // Decode the public key
    memset(&pub, 0, sizeof (ecc_publickey));
    AJ_BigvalDecode(data, &pub.x, KEY_ECC_SZ);
    AJ_BigvalDecode(data + KEY_ECC_SZ, &pub.y, KEY_ECC_SZ);
    AJ_SHA256_Update(&ctx->hash, &fmt, sizeof (fmt));

    AJ_SHA256_Update(&ctx->hash, data, size);

    // Generate shared secret
    status = AJ_GenerateShareSecret(&pub, &ctx->kectx.prv, &secret);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("ECDHEUnmarshalV2(ctx=%p, msg=%p): Generate secret error\n", ctx, msg));
        return status;
    }

    // Encode the shared secret
    size = KEY_ECC_PRV_SZ;
    data = (uint8_t*)AJ_Malloc(size);
    if (NULL == data) {
        return AJ_ERR_RESOURCES;
    }
    // Only use x-coordinate for secret
    AJ_BigvalEncode(&secret.x, data, KEY_ECC_SZ);
    // Reuse the data buffer - hash of the point
    AJ_ASSERT(SHA256_DIGEST_LENGTH <= size);
    AJ_SHA256_Init(&sha);
    AJ_SHA256_Update(&sha, data, size);
    AJ_SHA256_Final(&sha, data);
    size = SHA256_DIGEST_LENGTH;
    status = ComputeMasterSecret(ctx, data, size);
    AJ_Free(data);

    return status;
}

static AJ_Status ECDHEUnmarshal(AJ_AuthenticationContext* ctx, AJ_Message* msg)
{
    AJ_Status status;

    AJ_InfoPrintf(("ECDHEUnmarshal(ctx=%p, msg=%p)\n", ctx, msg));

    // Generate key pair if server
    if (AUTH_SERVER == ctx->role) {
        status = AJ_GenerateDHKeyPair(&ctx->kectx.pub, &ctx->kectx.prv);
        if (AJ_OK != status) {
            AJ_InfoPrintf(("ECDHEUnmarshal(ctx=%p, msg=%p): Key generation failed\n", ctx, msg));
            return status;
        }
    }

    if ((ctx->version >> 16) < 3) {
        status = ECDHEUnmarshalV1(ctx, msg);
    } else {
        status = ECDHEUnmarshalV2(ctx, msg);
    }

    return status;
}

AJ_Status AJ_KeyExchangeMarshal(AJ_AuthenticationContext* ctx, AJ_Message* msg)
{
    AJ_Status status = AJ_ERR_SECURITY;
    switch (0xFFFF0000 & ctx->suite) {
    case AUTH_KEYX_ECDHE:
        status = ECDHEMarshal(ctx, msg);
        break;
    }
    return status;
}

AJ_Status AJ_KeyExchangeUnmarshal(AJ_AuthenticationContext* ctx, AJ_Message* msg)
{
    AJ_Status status = AJ_ERR_SECURITY;
    switch (0xFFFF0000 & ctx->suite) {
    case AUTH_KEYX_ECDHE:
        status = ECDHEUnmarshal(ctx, msg);
        break;
    }
    return status;
}

static AJ_Status NULLMarshal(AJ_AuthenticationContext* ctx, AJ_Message* msg)
{
    AJ_Status status;
    AJ_Credential cred;
    uint8_t verifier[AUTH_VERIFIER_LEN];

    AJ_InfoPrintf(("NULLMarshal(ctx=%p, msg=%p)\n", ctx, msg));

    if (ctx->bus->authListenerCallback) {
        status = ctx->bus->authListenerCallback(AUTH_SUITE_ECDHE_NULL, 0, &cred);
        if (AJ_OK == status) {
            ctx->expiration = cred.expiration;
        }
    }
    if (AUTH_CLIENT == ctx->role) {
        status = ComputeVerifier(ctx, "client finished", verifier, sizeof (verifier));
    } else {
        status = ComputeVerifier(ctx, "server finished", verifier, sizeof (verifier));
    }
    if (AJ_OK != status) {
        return AJ_ERR_SECURITY;
    }
    status = AJ_MarshalArgs(msg, "v", "ay", verifier, sizeof (verifier));
    AJ_SHA256_Update(&ctx->hash, verifier, sizeof (verifier));

    return status;
}

static AJ_Status NULLUnmarshal(AJ_AuthenticationContext* ctx, AJ_Message* msg)
{
    AJ_Status status;
    uint8_t local[AUTH_VERIFIER_LEN];
    uint8_t* remote;
    size_t len;

    AJ_InfoPrintf(("NULLUnmarshal(ctx=%p, msg=%p)\n", ctx, msg));

    if (AUTH_CLIENT == ctx->role) {
        status = ComputeVerifier(ctx, "server finished", local, sizeof (local));
    } else {
        status = ComputeVerifier(ctx, "client finished", local, sizeof (local));
    }
    if (AJ_OK != status) {
        return AJ_ERR_SECURITY;
    }
    status = AJ_UnmarshalArgs(msg, "v", "ay", &remote, &len);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("NULLUnmarshal(ctx=%p, msg=%p): Unmarshal error\n", ctx, msg));
        return AJ_ERR_SECURITY;
    }
    if (AUTH_VERIFIER_LEN != len) {
        AJ_InfoPrintf(("NULLUnmarshal(ctx=%p, msg=%p): Invalid signature size\n", ctx, msg));
        return AJ_ERR_SECURITY;
    }
    if (0 != memcmp(local, remote, AUTH_VERIFIER_LEN)) {
        AJ_InfoPrintf(("NULLUnmarshal(ctx=%p, msg=%p): Invalid verifier\n", ctx, msg));
        return AJ_ERR_SECURITY;
    }
    AJ_SHA256_Update(&ctx->hash, local, sizeof (local));

    return status;
}

static AJ_Status PSKCallbackV1(AJ_AuthenticationContext* ctx, AJ_Message* msg)
{
    uint8_t data[128];
    size_t size = sizeof (data);

    /*
     * Assume application does not copy in more than this size buffer
     * Expiration not set by application
     */
    size = ctx->bus->pwdCallback(data, size);
    if (sizeof (data) < size) {
        return AJ_ERR_RESOURCES;
    }
    ctx->expiration = 0xFFFFFFFF;
    AJ_SHA256_Update(&ctx->hash, ctx->kactx.psk.hint, ctx->kactx.psk.size);
    AJ_SHA256_Update(&ctx->hash, data, size);

    return AJ_OK;
}

static AJ_Status PSKCallbackV2(AJ_AuthenticationContext* ctx, AJ_Message* msg)
{
    AJ_Status status;
    AJ_Credential cred;

    switch (ctx->role) {
    case AUTH_CLIENT:
        cred.direction = AJ_CRED_REQUEST;
        status = ctx->bus->authListenerCallback(AUTH_SUITE_ECDHE_PSK, AJ_CRED_PUB_KEY, &cred);
        if (AJ_OK == status) {
            ctx->kactx.psk.hint = cred.data;
            ctx->kactx.psk.size = cred.len;
        }
        break;

    case AUTH_SERVER:
        cred.direction = AJ_CRED_RESPONSE;
        cred.data = ctx->kactx.psk.hint;
        cred.len = ctx->kactx.psk.size;
        status = ctx->bus->authListenerCallback(AUTH_SUITE_ECDHE_PSK, AJ_CRED_PUB_KEY, &cred);
        break;
    }
    cred.direction = AJ_CRED_REQUEST;
    status = ctx->bus->authListenerCallback(AUTH_SUITE_ECDHE_PSK, AJ_CRED_PRV_KEY, &cred);
    if (AJ_OK != status) {
        return AJ_ERR_SECURITY;
    }
    ctx->expiration = cred.expiration;
    // Hash in psk hint, then psk
    AJ_SHA256_Update(&ctx->hash, ctx->kactx.psk.hint, ctx->kactx.psk.size);
    AJ_SHA256_Update(&ctx->hash, cred.data, cred.len);

    return status;
}

static AJ_Status PSKCallback(AJ_AuthenticationContext* ctx, AJ_Message* msg)
{
    AJ_Status status;

    AJ_InfoPrintf(("PSKCallback(ctx=%p, msg=%p)\n", ctx, msg));

    if (ctx->bus->authListenerCallback) {
        status = PSKCallbackV2(ctx, msg);
    } else if (ctx->bus->pwdCallback) {
        status = PSKCallbackV1(ctx, msg);
    } else {
        status = AJ_ERR_SECURITY;
    }

    return status;
}

static AJ_Status PSKMarshal(AJ_AuthenticationContext* ctx, AJ_Message* msg)
{
    AJ_Status status = AJ_ERR_SECURITY;
    const char* anon = "<anonymous>";
    uint8_t verifier[AUTH_VERIFIER_LEN];

    AJ_InfoPrintf(("PSKMarshal(ctx=%p, msg=%p)\n", ctx, msg));

    switch (ctx->role) {
    case AUTH_CLIENT:
        // Default to anonymous
        ctx->kactx.psk.hint = (uint8_t*) anon;
        ctx->kactx.psk.size = strlen(anon);
        status = PSKCallback(ctx, msg);
        if (AJ_OK != status) {
            return AJ_ERR_SECURITY;
        }
        status = ComputeVerifier(ctx, "client finished", verifier, sizeof (verifier));
        AJ_SHA256_Update(&ctx->hash, verifier, sizeof (verifier));
        break;

    case AUTH_SERVER:
        status = ComputeVerifier(ctx, "server finished", verifier, sizeof (verifier));
        break;
    }
    if (AJ_OK != status) {
        return AJ_ERR_SECURITY;
    }

    status = AJ_MarshalArgs(msg, "v", "(ayay)", ctx->kactx.psk.hint, ctx->kactx.psk.size, verifier, sizeof (verifier));

    return status;
}

static AJ_Status PSKUnmarshal(AJ_AuthenticationContext* ctx, AJ_Message* msg)
{
    AJ_Status status;
    uint8_t verifier[AUTH_VERIFIER_LEN];
    uint8_t* data;
    size_t size;

    AJ_InfoPrintf(("PSKUnmarshal(ctx=%p, msg=%p)\n", ctx, msg));

    status = AJ_UnmarshalArgs(msg, "v", "(ayay)", &ctx->kactx.psk.hint, &ctx->kactx.psk.size, &data, &size);
    if (AJ_OK != status) {
        return AJ_ERR_SECURITY;
    }
    if (AUTH_VERIFIER_LEN != size) {
        return AJ_ERR_SECURITY;
    }

    switch (ctx->role) {
    case AUTH_CLIENT:
        status = ComputeVerifier(ctx, "server finished", verifier, sizeof (verifier));
        break;

    case AUTH_SERVER:
        status = PSKCallback(ctx, msg);
        if (AJ_OK != status) {
            return AJ_ERR_SECURITY;
        }
        status = ComputeVerifier(ctx, "client finished", verifier, sizeof (verifier));
        AJ_SHA256_Update(&ctx->hash, verifier, sizeof (verifier));
        break;
    }
    if (AJ_OK != status) {
        return AJ_ERR_SECURITY;
    }

    if (0 != memcmp(verifier, data, AUTH_VERIFIER_LEN)) {
        AJ_InfoPrintf(("PSKUnmarshal(ctx=%p, msg=%p): Invalid verifier\n", ctx, msg));
        return AJ_ERR_SECURITY;
    }

    return status;
}

typedef struct _SigInfoCtx {
    ecc_privatekey prv;
    ecc_signature sig;
    uint8_t sig_r[KEY_ECC_SZ];
    uint8_t sig_s[KEY_ECC_SZ];
} SigInfoCtx;

static AJ_Status ECDSAMarshal(AJ_AuthenticationContext* ctx, AJ_Message* msg)
{
    AJ_Status status;
    AJ_Arg container1;
    AJ_Arg container2;
    AJ_Credential cred;
    SigInfoCtx* sig = NULL;
    uint8_t verifier[SHA256_DIGEST_LENGTH];
    X509CertificateChain* chain = NULL;
    uint8_t fmt;

    AJ_InfoPrintf(("AJ_ECDSA_Marshal(ctx=%p, msg=%p)\n", ctx, msg));

    if (NULL == ctx->bus->authListenerCallback) {
        status = AJ_ERR_INVALID;
        goto Exit;
    }

    if (AUTH_CLIENT == ctx->role) {
        status = ComputeVerifier(ctx, "client finished", verifier, sizeof (verifier));
    } else {
        status = ComputeVerifier(ctx, "server finished", verifier, sizeof (verifier));
    }
    if (AJ_OK != status) {
        goto Exit;
    }

    // Request private key from application
    cred.direction = AJ_CRED_REQUEST;
    status = ctx->bus->authListenerCallback(AUTH_SUITE_ECDHE_ECDSA, AJ_CRED_PRV_KEY, &cred);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_ECDSA_Marshal(msg=%p): Private key required\n", msg));
        goto Exit;
    }

    // Put the signature context on the heap
    sig = (SigInfoCtx*) AJ_Malloc(sizeof (SigInfoCtx));
    if (NULL == sig) {
        status = AJ_ERR_RESOURCES;
        goto Exit;
    }
    // The credential holds a pointer to an ecc_privatekey
    if (sizeof (ecc_privatekey) != cred.len) {
        status = AJ_ERR_INVALID;
        goto Exit;
    }
    memcpy((uint8_t*) &sig->prv, cred.data, cred.len);
    ctx->expiration = cred.expiration;

    // Sign verifier
    status = AJ_DSASignDigest(verifier, &sig->prv, &sig->sig);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_ECDSA_Marshal(msg=%p): Sign verifier error\n", msg));
        goto Exit;
    }
    AJ_BigvalEncode(&sig->sig.r, sig->sig_r, KEY_ECC_SZ);
    AJ_BigvalEncode(&sig->sig.s, sig->sig_s, KEY_ECC_SZ);
    AJ_SHA256_Update(&ctx->hash, sig->sig_r, KEY_ECC_SZ);
    AJ_SHA256_Update(&ctx->hash, sig->sig_s, KEY_ECC_SZ);

    // Marshal signature
    status = AJ_MarshalVariant(msg, "(vyv)");
    if (AJ_OK != status) {
        goto Exit;
    }
    status = AJ_MarshalContainer(msg, &container1, AJ_ARG_STRUCT);
    if (AJ_OK != status) {
        goto Exit;
    }
    status = AJ_MarshalArgs(msg, "v", "(yv)", SIG_FMT, "(ayay)", sig->sig_r, KEY_ECC_SZ, sig->sig_s, KEY_ECC_SZ);
    if (AJ_OK != status) {
        goto Exit;
    }
    AJ_Free(sig);
    sig = NULL;

    // Marshal certificate chain
    fmt = CERT_FMT_X509_DER;
    AJ_SHA256_Update(&ctx->hash, &fmt, 1);
    status = AJ_MarshalArgs(msg, "y", fmt);
    if (AJ_OK != status) {
        goto Exit;
    }
    status = AJ_MarshalVariant(msg, "a(ay)");
    if (AJ_OK != status) {
        goto Exit;
    }
    status = AJ_MarshalContainer(msg, &container2, AJ_ARG_ARRAY);
    if (AJ_OK != status) {
        goto Exit;
    }

    // Request certificate chain from application
    cred.direction = AJ_CRED_REQUEST;
    status = ctx->bus->authListenerCallback(AUTH_SUITE_ECDHE_ECDSA, AJ_CRED_CERT_CHAIN, &cred);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_ECDSA_Marshal(msg=%p): Certificate chain required\n", msg));
        goto Exit;
    }
    // The credential holds a pointer to a certificate chain
    chain = (X509CertificateChain*) cred.data;
    while (chain) {
        status = AJ_MarshalArgs(msg, "(ay)", chain->certificate.der.data, chain->certificate.der.size);
        if (AJ_OK != status) {
            AJ_WarnPrintf(("AJ_ECDSA_Marshal(msg=%p): Marshal certificate error\n", msg));
            goto Exit;
        }
        AJ_SHA256_Update(&ctx->hash, chain->certificate.der.data, chain->certificate.der.size);
        chain = chain->next;
    }
    status = AJ_MarshalCloseContainer(msg, &container2);
    if (AJ_OK != status) {
        goto Exit;
    }
    status = AJ_MarshalCloseContainer(msg, &container1);

Exit:
    if (sig) {
        AJ_Free(sig);
    }
    return status;
}

static AJ_Status ECDSAUnmarshal(AJ_AuthenticationContext* ctx, AJ_Message* msg)
{
    AJ_Status status;
    AJ_Credential cred;
    AJ_Arg container1;
    AJ_Arg container2;
    uint8_t digest[SHA256_DIGEST_LENGTH];
    const char* variant;
    uint8_t fmt;
    DER_Element der;
    ecc_signature sig;
    uint8_t* sig_r;
    uint8_t* sig_s;
    size_t len_r;
    size_t len_s;
    X509CertificateChain* head = NULL;
    X509CertificateChain* node = NULL;
    uint8_t trusted = 0;

    AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=%p)\n", msg));

    if (NULL == ctx->bus->authListenerCallback) {
        return AJ_ERR_SECURITY;
    }

    if (AUTH_CLIENT == ctx->role) {
        status = ComputeVerifier(ctx, "server finished", digest, sizeof (digest));
    } else {
        status = ComputeVerifier(ctx, "client finished", digest, sizeof (digest));
    }
    if (AJ_OK != status) {
        goto Exit;
    }

    status = AJ_UnmarshalVariant(msg, &variant);
    if (AJ_OK != status) {
        goto Exit;
    }
    if (0 != strncmp(variant, "(vyv)", 5)) {
        goto Exit;
    }
    status = AJ_UnmarshalContainer(msg, &container1, AJ_ARG_STRUCT);
    if (AJ_OK != status) {
        goto Exit;
    }

    // Unmarshal signature
    status = AJ_UnmarshalArgs(msg, "v", "(yv)", &fmt, "(ayay)", &sig_r, &len_r, &sig_s, &len_s);
    if (AJ_OK != status) {
        goto Exit;
    }
    if (SIG_FMT != fmt) {
        goto Exit;
    }
    if ((KEY_ECC_SZ != len_r) || (KEY_ECC_SZ != len_s)) {
        goto Exit;
    }
    AJ_BigvalDecode(sig_r, &sig.r, KEY_ECC_SZ);
    AJ_BigvalDecode(sig_s, &sig.s, KEY_ECC_SZ);
    AJ_SHA256_Update(&ctx->hash, sig_r, KEY_ECC_SZ);
    AJ_SHA256_Update(&ctx->hash, sig_s, KEY_ECC_SZ);

    // Unmarshal certificate chain
    status = AJ_UnmarshalArgs(msg, "y", &fmt);
    if (AJ_OK != status) {
        goto Exit;
    }
    if (CERT_FMT_X509_DER != fmt) {
        AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=%p): DER encoding expected\n", msg));
        goto Exit;
    }
    AJ_SHA256_Update(&ctx->hash, &fmt, 1);
    status = AJ_UnmarshalVariant(msg, &variant);
    if (AJ_OK != status) {
        goto Exit;
    }
    if (0 != strncmp(variant, "a(ay)", 5)) {
        goto Exit;
    }
    status = AJ_UnmarshalContainer(msg, &container2, AJ_ARG_ARRAY);
    if (AJ_OK != status) {
        goto Exit;
    }
    while (AJ_OK == status) {
        status = AJ_UnmarshalArgs(msg, "(ay)", &der.data, &der.size);
        if (AJ_OK != status) {
            // No more in array
            break;
        }
        AJ_SHA256_Update(&ctx->hash, der.data, der.size);

        node = (X509CertificateChain*) AJ_Malloc(sizeof (X509CertificateChain));
        if (NULL == node) {
            AJ_WarnPrintf(("AJ_ECDSA_Unmarshal(msg=%p): Resource error\n", msg));
            goto Exit;
        }
        // Set the der before its consumed
        node->certificate.der.size = der.size;
        node->certificate.der.data = der.data;
        status = AJ_X509DecodeCertificateDER(&node->certificate, &der);
        if (AJ_OK != status) {
            AJ_WarnPrintf(("AJ_ECDSA_Unmarshal(msg=%p): Certificate decode failed\n", msg));
            goto Exit;
        }
        // Push the certificate on to the front of the chain
        node->next = head;
        head = node;
    }
    if (AJ_ERR_NO_MORE != status) {
        AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=%p): Certificate chain error %s\n", msg, AJ_StatusText(status)));
        goto Exit;
    }
    status = AJ_UnmarshalCloseContainer(msg, &container2);
    if (AJ_OK != status) {
        goto Exit;
    }
    status = AJ_UnmarshalCloseContainer(msg, &container1);
    if (AJ_OK != status) {
        goto Exit;
    }

    // Verify the chain
    status = AJ_X509VerifyChain(head, NULL);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=%p): Certificate chain invalid\n", msg));
        goto Exit;
    }
    // Send the certificate chain to the application
    cred.direction = AJ_CRED_RESPONSE;
    cred.data = (uint8_t*) head;
    status = ctx->bus->authListenerCallback(AUTH_SUITE_ECDHE_ECDSA, AJ_CRED_CERT_CHAIN, &cred);
    if (AJ_OK == status) {
        trusted = 1;
    }

Exit:
    /* In case the list is empty and just node was allocated */
    if (!head) {
        if (node) {
            AJ_Free(node);
        }
    }
    while (head) {
        node = head;
        head = head->next;
        if (node) {
            AJ_Free(node);
        }
    }
    return trusted ? AJ_OK : AJ_ERR_SECURITY;
}

AJ_Status AJ_KeyAuthenticationMarshal(AJ_AuthenticationContext* ctx, AJ_Message* msg)
{
    AJ_Status status = AJ_ERR_SECURITY;
    switch (ctx->suite) {
    case AUTH_SUITE_ECDHE_NULL:
        status = NULLMarshal(ctx, msg);
        break;

    case AUTH_SUITE_ECDHE_PSK:
        status = PSKMarshal(ctx, msg);
        break;

    case AUTH_SUITE_ECDHE_ECDSA:
        status = ECDSAMarshal(ctx, msg);
        break;
    }
    return status;
}

AJ_Status AJ_KeyAuthenticationUnmarshal(AJ_AuthenticationContext* ctx, AJ_Message* msg)
{
    AJ_Status status = AJ_ERR_SECURITY;
    switch (ctx->suite) {
    case AUTH_SUITE_ECDHE_NULL:
        status = NULLUnmarshal(ctx, msg);
        break;

    case AUTH_SUITE_ECDHE_PSK:
        status = PSKUnmarshal(ctx, msg);
        break;

    case AUTH_SUITE_ECDHE_ECDSA:
        status = ECDSAUnmarshal(ctx, msg);
        break;
    }
    return status;
}

uint8_t AJ_IsSuiteEnabled(uint32_t suite, uint32_t version)
{
    switch (suite) {
    case AUTH_SUITE_ECDHE_NULL:
        return 1 == suites[0];

    case AUTH_SUITE_ECDHE_PSK:
        return 1 == suites[1];

    case AUTH_SUITE_ECDHE_ECDSA:
        if (version < 3) {
            return 0;
        }
        return 1 == suites[2];

    default:
        return 0;
    }

    return 0;
}

void AJ_EnableSuite(uint32_t suite)
{
    switch (suite) {
    case AUTH_SUITE_ECDHE_NULL:
        suites[0] = 1;
        break;

    case AUTH_SUITE_ECDHE_PSK:
        suites[1] = 1;
        break;

    case AUTH_SUITE_ECDHE_ECDSA:
        suites[2] = 1;
        break;
    }
}

