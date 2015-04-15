/**
 * @file
 */
/******************************************************************************
 * Copyright (c) 2014, AllSeen Alliance. All rights reserved.
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
#define AJ_MODULE KEYAUTHENTICATION

#include "aj_target.h"
#include "aj_debug.h"
#include "aj_keyauthentication.h"
#include "aj_cert.h"
#include "aj_peer.h"
#include "aj_creds.h"
#include "aj_auth_listener.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgKEYAUTHENTICATION = 0;
#endif

static AJ_Status ComputeVerifier(const char* label, uint8_t* buffer, size_t bufferlen);

#ifdef AUTH_ECDSA
static AJ_Status ECDSA_Init(AJ_AuthListenerFunc authlistener, const uint8_t* mastersecret, size_t mastersecretlen, AJ_SHA256_Context* hash);
static AJ_Status ECDSA_Marshal(AJ_Message* msg, uint8_t role);
static AJ_Status ECDSA_Unmarshal(AJ_Message* msg, uint8_t role);
static AJ_Status ECDSA_Final(uint32_t* expiration);

AJ_KeyAuthentication AJ_KeyAuthenticationECDSA = {
    ECDSA_Init,
    ECDSA_Marshal,
    ECDSA_Unmarshal,
    ECDSA_Final
};

typedef struct _ECDSAContext {
    AJ_Certificate* certchain;
    size_t certchainlen;
    ecc_privatekey prv_key;
    ecc_publickey pub_key;
} ECDSAContext;
static ECDSAContext ecdsactx;
#endif

#ifdef AUTH_PSK
static AJ_Status PSK_Init(AJ_AuthListenerFunc authlistener, const uint8_t* mastersecret, size_t mastersecretlen, AJ_SHA256_Context* hash);
static AJ_Status PSK_Marshal(AJ_Message* msg, uint8_t role);
static AJ_Status PSK_Unmarshal(AJ_Message* msg, uint8_t role);
static AJ_Status PSK_Final(uint32_t* expiration);

AJ_KeyAuthentication AJ_KeyAuthenticationPSK = {
    PSK_Init,
    PSK_Marshal,
    PSK_Unmarshal,
    PSK_Final
};

#define AUTH_VERIFIER_LEN SHA256_DIGEST_LENGTH
typedef struct _PSKContext {
    uint8_t* hint;
    size_t hintlen;
    uint8_t* psk;
    size_t psklen;
    AJ_AuthPwdFunc pwdcallback;
} PSKContext;
static PSKContext pskctx;
#endif

#ifdef AUTH_NULL
static AJ_Status NULL_Init(AJ_AuthListenerFunc authlistener, const uint8_t* mastersecret, size_t mastersecretlen, AJ_SHA256_Context* hash);
static AJ_Status NULL_Marshal(AJ_Message* msg, uint8_t role);
static AJ_Status NULL_Unmarshal(AJ_Message* msg, uint8_t role);
static AJ_Status NULL_Final(uint32_t* expiration);

AJ_KeyAuthentication AJ_KeyAuthenticationNULL = {
    NULL_Init,
    NULL_Marshal,
    NULL_Unmarshal,
    NULL_Final
};
#endif

typedef struct _KeyAuthenticationContext {
    uint8_t* mastersecret;
    size_t mastersecretlen;
    uint32_t expiration;
    AJ_SHA256_Context* hash;
    AJ_AuthListenerFunc authlistener;
} KeyAuthenticationContext;

static KeyAuthenticationContext kactx;

static AJ_Status ComputeVerifier(const char* label, uint8_t* buffer, size_t bufferlen)
{
    AJ_Status status;
    const uint8_t* data[3];
    uint8_t lens[3];
    uint8_t digest[SHA256_DIGEST_LENGTH];

    AJ_SHA256_GetDigest(kactx.hash, digest, 1);

    data[0] = kactx.mastersecret;
    lens[0] = kactx.mastersecretlen;
    data[1] = (uint8_t*) label;
    lens[1] = (uint8_t) strlen(label);
    data[2] = digest;
    lens[2] = sizeof (digest);

    status = AJ_Crypto_PRF_SHA256(data, lens, ArraySize(data), buffer, bufferlen);

    return status;
}

#ifdef AUTH_ECDSA
static AJ_Status ECDSA_Init(AJ_AuthListenerFunc authlistener, const uint8_t* mastersecret, size_t mastersecretlen, AJ_SHA256_Context* hash)
{
    AJ_Status status = AJ_OK;
    AJ_Credential cred;
    AJ_PeerCred* prvcred;
    size_t chainlen;
    uint8_t keypair;

    AJ_InfoPrintf(("AJ_ECDSA_Init()\n"));

    /* mastersecret, hash, authlistener will not be NULL */
    kactx.mastersecret = (uint8_t*) mastersecret;
    kactx.mastersecretlen = mastersecretlen;
    kactx.hash = hash;
    kactx.authlistener = authlistener;

    /*
     * Load private key if available
     */
    status = AJ_GetLocalCredential(AJ_CRED_TYPE_DSA_PRIVATE, DSA_PRV_KEY_ID, &prvcred);
    if (AJ_OK == status) {
        if (!prvcred || (sizeof (ecc_privatekey) != prvcred->dataLen)) {
            AJ_WarnPrintf(("AJ_ECDSA_Init(): Invalid stored credential\n"));
            AJ_FreeCredential(prvcred);
            return AJ_ERR_SECURITY;
        }
        status = AJ_CredentialExpired(prvcred);
        if (AJ_ERR_KEY_EXPIRED == status) {
            /*
             * Key expired - delete it
             */
            AJ_DeleteLocalCredential(AJ_CRED_TYPE_DSA_PRIVATE, DSA_PRV_KEY_ID);
            AJ_InfoPrintf(("AJ_ECDSA_Init(): Private key expired\n"));
        } else {
            memcpy(&ecdsactx.prv_key, prvcred->data, sizeof (ecc_privatekey));
            kactx.expiration = prvcred->expiration;
        }
        AJ_FreeCredential(prvcred);

        status = AJ_GetLocalCredential(AJ_CRED_TYPE_DSA_PUBLIC, DSA_PUB_KEY_ID, &prvcred);
        if (!prvcred || (sizeof (ecc_publickey) != prvcred->dataLen)) {
            AJ_WarnPrintf(("AJ_ECDSA_Init(): Invalid stored credential\n"));
            AJ_FreeCredential(prvcred);
            return AJ_ERR_SECURITY;
        }
        status = AJ_CredentialExpired(prvcred);
        if (AJ_ERR_KEY_EXPIRED == status) {
            /*
             * Key expired - delete it
             */
            AJ_DeleteLocalCredential(AJ_CRED_TYPE_DSA_PUBLIC, DSA_PUB_KEY_ID);
            AJ_InfoPrintf(("AJ_ECDSA_Init(): Public key expired\n"));
        } else {
            memcpy(&ecdsactx.pub_key, prvcred->data, sizeof (ecc_publickey));
            kactx.expiration = prvcred->expiration;
        }
        AJ_FreeCredential(prvcred);
    }
    if (AJ_OK != status) {
        /*
         * Request key pair
         */
        status = authlistener(AUTH_SUITE_ECDHE_ECDSA, AJ_CRED_PRV_KEY, &cred);
        keypair = 0;
        if (AJ_OK == status) {
            if (sizeof (ecc_privatekey) != cred.len) {
                AJ_WarnPrintf(("AJ_ECDSA_Init(): Invalid credential\n"));
                return AJ_ERR_SECURITY;
            }
            memcpy(&ecdsactx.prv_key, cred.data, sizeof (ecdsactx.prv_key));
            kactx.expiration = cred.expiration;
            keypair++;
        }
        status = authlistener(AUTH_SUITE_ECDHE_ECDSA, AJ_CRED_PUB_KEY, &cred);
        if (AJ_OK == status) {
            if (sizeof (ecc_publickey) != cred.len) {
                AJ_WarnPrintf(("AJ_ECDSA_Init(): Invalid credential\n"));
                return AJ_ERR_SECURITY;
            }
            memcpy(&ecdsactx.pub_key, cred.data, sizeof (ecdsactx.pub_key));
            keypair++;
        }
        if (!keypair) {
            /*
             * Application didn't supply key pair - generate one
             */
            status = AJ_GenerateDSAKeyPair(&ecdsactx.pub_key, &ecdsactx.prv_key);
            if (AJ_OK != status) {
                AJ_WarnPrintf(("AJ_ECDSA_Init(): Generate DSA key pair error\n"));
                return AJ_ERR_SECURITY;
            }
            /*
             * Default key pair expiration is long
             */
            kactx.expiration = 0xFFFFFFFF;
            keypair = 2;
        }
        if (2 != keypair) {
            /*
             * Application only supplied one key
             */
            AJ_WarnPrintf(("AJ_ECDSA_Init(): Only one key provided\n"));
            return AJ_ERR_SECURITY;
        }
        /*
         * Store the DSA key pair
         */
        status = AJ_StoreLocalCredential(AJ_CRED_TYPE_DSA_PRIVATE, DSA_PRV_KEY_ID, (uint8_t*) &ecdsactx.prv_key, sizeof (ecc_privatekey), kactx.expiration);
        if (AJ_OK != status) {
            AJ_WarnPrintf(("AJ_ECDSA_Init(): Store local credential error\n"));
            return AJ_ERR_SECURITY;
        }
        status = AJ_StoreLocalCredential(AJ_CRED_TYPE_DSA_PUBLIC, DSA_PUB_KEY_ID, (uint8_t*) &ecdsactx.pub_key, sizeof (ecc_publickey), kactx.expiration);
        if (AJ_OK != status) {
            AJ_WarnPrintf(("AJ_ECDSA_Init(): Store local credential error\n"));
            return AJ_ERR_SECURITY;
        }
    }

    /*
     * Request certificate chain - there may not be one
     */
    status = authlistener(AUTH_SUITE_ECDHE_ECDSA, AJ_CRED_CERT_CHAIN, &cred);
    if (AJ_OK == status) {
        chainlen = cred.len;
        ecdsactx.certchainlen = chainlen;
        ecdsactx.certchain = NULL;
        if (chainlen) {
            ecdsactx.certchain = (AJ_Certificate*) AJ_Malloc(chainlen);
            if (!ecdsactx.certchain) {
                AJ_WarnPrintf(("AJ_ECDSA_Init(): AJ_ERR_RESOURCES\n"));
                return AJ_ERR_RESOURCES;
            }
            memcpy(ecdsactx.certchain, cred.data, cred.len);
        }
    }

    return status;
}

static AJ_Status ECDSA_MarshalCertificate(AJ_Message* msg, AJ_Certificate* certificate, uint8_t role)
{
    AJ_Status status;
    AJ_Arg struct1;
    uint8_t* b8;

    AJ_InfoPrintf(("AJ_ECDSA_MarshalCertificate(msg=0x%p)\n", msg));

    if (certificate->version > MAX_NUM_CERTIFICATES) {
        AJ_WarnPrintf(("AJ_ECDSA_MarshalCertificate(msg=0x%p): Invalid certificate version\n", msg));
        return AJ_ERR_SECURITY;
    }
    b8 = (uint8_t*) AJ_Malloc(certificate->size);
    if (!b8) {
        AJ_WarnPrintf(("AJ_ECDSA_MarshalCertificate(msg=0x%p): AJ_ERR_RESOURCS\n", msg));
        return AJ_ERR_RESOURCES;
    }
    status = AJ_BigEndianEncodeCertificate(certificate, b8, certificate->size);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_ECDSA_MarshalCertificate(msg=0x%p): Encode certificate error\n", msg));
        status = AJ_ERR_SECURITY;
        goto Exit;
    }

    status = AJ_MarshalContainer(msg, &struct1, AJ_ARG_STRUCT);
    status = AJ_MarshalArgs(msg, "ay", b8, certificate->size);
    status = AJ_MarshalCloseContainer(msg, &struct1);

    if (AUTH_CLIENT == role) {
        AJ_SHA256_Update(kactx.hash, b8, certificate->size);
    }

Exit:
    AJ_Free(b8);

    return status;
}

AJ_Status ECDSA_Marshal(AJ_Message* msg, uint8_t role)
{
    AJ_Status status;
    AJ_Arg array1;
    AJ_Certificate* certificate;
    AJ_Certificate* chain = ecdsactx.certchain;
    int chainlen = ecdsactx.certchainlen;
    uint8_t verifier[SHA256_DIGEST_LENGTH];

    AJ_InfoPrintf(("AJ_ECDSA_Marshal(msg=0x%p)\n", msg));

    if (AUTH_CLIENT == role) {
        status = ComputeVerifier("client finished", verifier, sizeof (verifier));
    } else {
        status = ComputeVerifier("server finished", verifier, sizeof (verifier));
    }
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_ECDSA_Marshal(msg=0x%p): Compute verifier error\n", msg));
        return AJ_ERR_SECURITY;
    }

    /*
     * Create leaf certificate and sign
     */
    certificate = (AJ_Certificate*) AJ_Malloc(sizeof (AJ_Certificate));
    if (!certificate) {
        AJ_WarnPrintf(("AJ_ECDSA_Marshal(msg=0x%p): AJ_ERR_RESOURCES\n", msg));
        return AJ_ERR_RESOURCES;
    }
    status = AJ_CreateCertificate(certificate, 0, &ecdsactx.pub_key, NULL, NULL, verifier, 0);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_ECDSA_Marshal(msg=0x%p): Create certificate error\n", msg));
        goto Exit;
    }
    status = AJ_SignCertificate(certificate, &ecdsactx.prv_key);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_ECDSA_Marshal(msg=0x%p): Sign certificate error\n", msg));
        goto Exit;
    }

    status = AJ_MarshalVariant(msg, "a(ay)");
    status = AJ_MarshalContainer(msg, &array1, AJ_ARG_ARRAY);

    /*
     * Marshal leaf certificate
     */
    status = ECDSA_MarshalCertificate(msg, certificate, role);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_ECDSA_Marshal(msg=0x%p): Marshal certificate error\n", msg));
        goto Exit;
    }

    /*
     * Marshal chain certificates
     */
    while (chainlen > 0) {
        if (chainlen < sizeof (AJ_Certificate)) {
            AJ_WarnPrintf(("AJ_ECDSA_Marshal(msg=0x%p): Invalid certificate chain\n", msg));
            status = AJ_ERR_SECURITY;
            goto Exit;
        }
        status = ECDSA_MarshalCertificate(msg, chain, role);
        if (AJ_OK != status) {
            AJ_WarnPrintf(("AJ_ECDSA_Marshal(msg=0x%p): Marshal certificate error\n", msg));
            goto Exit;
        }
        chain++;
        chainlen -= sizeof (AJ_Certificate);
    }
    status = AJ_MarshalCloseContainer(msg, &array1);

Exit:
    AJ_Free(certificate);

    return status;
}

static AJ_Status ECDSA_UnmarshalCertificate(AJ_Message* msg, AJ_Certificate* certificate, uint8_t role)
{
    AJ_Status status;
    AJ_Arg struct1;
    uint8_t* b8;
    size_t b8len;

    AJ_InfoPrintf(("AJ_ECDSA_UnmarshalCertificate(msg=0x%p)\n", msg));

    status = AJ_UnmarshalContainer(msg, &struct1, AJ_ARG_STRUCT);
    if (AJ_OK != status) {
        if (AJ_ERR_NO_MORE != status) {
            AJ_WarnPrintf(("AJ_ECDSA_UnmarshalCertificate(msg=0x%p): Unmarshal container error\n", msg));
        }
        return status;
    }
    status = AJ_UnmarshalArgs(msg, "ay", &b8, &b8len);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_ECDSA_UnmarshalCertificate(msg=0x%p): Unmarshal error\n", msg));
        return AJ_ERR_SECURITY;
    }
    status = AJ_BigEndianDecodeCertificate(certificate, b8, b8len);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_ECDSA_UnmarshalCertificate(msg=0x%p): Decode certificate error\n", msg));
        return AJ_ERR_SECURITY;
    }
    if (certificate->version > MAX_NUM_CERTIFICATES) {
        AJ_WarnPrintf(("AJ_ECDSA_UnmarshalCertificate(msg=0x%p): Invalid certificate version\n", msg));
        return AJ_ERR_SECURITY;
    }
    status = AJ_UnmarshalCloseContainer(msg, &struct1);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_ECDSA_UnmarshalCertificate(msg=0x%p): Unmarshal container error\n", msg));
        return AJ_ERR_SECURITY;
    }

    if (AUTH_SERVER == role) {
        AJ_SHA256_Update(kactx.hash, b8, certificate->size);
    }

    return status;
}

AJ_Status ECDSA_Unmarshal(AJ_Message* msg, uint8_t role)
{
    AJ_Status status;
    AJ_Arg array1;
    AJ_Credential credential;
    AJ_Certificate* certificate;
    uint8_t* b8;
    uint8_t verifier[SHA256_DIGEST_LENGTH];
    char* variant;
    ecc_publickey issuer;
    AJ_Validity validity;
    uint8_t trusted = 0;
    uint8_t isfirst = 1;
    uint8_t delegate = 0;

    AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=0x%p)\n", msg));

    if (AUTH_CLIENT == role) {
        status = ComputeVerifier("server finished", verifier, sizeof (verifier));
    } else {
        status = ComputeVerifier("client finished", verifier, sizeof (verifier));
    }
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_ECDSA_Unmarshal(msg=0x%p): Compute verifier error\n", msg));
        return AJ_ERR_SECURITY;
    }

    status = AJ_UnmarshalVariant(msg, (const char**) &variant);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=0x%p): Unmarshal variant error\n", msg));
        return AJ_ERR_SECURITY;
    }
    if (0 != strncmp(variant, "a(ay)", 5)) {
        AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=0x%p): Invalid variant\n", msg));
        return AJ_ERR_SECURITY;
    }

    status = AJ_UnmarshalContainer(msg, &array1, AJ_ARG_ARRAY);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=0x%p): Unmarshal container error\n", msg));
        return AJ_ERR_SECURITY;
    }

    /*
     * Unmarshal leaf certificate
     */
    certificate = (AJ_Certificate*) AJ_Malloc(sizeof (AJ_Certificate));
    if (!certificate) {
        AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=0x%p): AJ_ERR_RESOURCES\n", msg));
        return AJ_ERR_RESOURCES;
    }
    status = ECDSA_UnmarshalCertificate(msg, certificate, role);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=0x%p): Unmarshal certificate error\n", msg));
        goto Exit;
    }
    /*
     * Leaf certificate must be type 0
     */
    if (0 != certificate->version) {
        AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=0x%p): Invalid certificate version %X\n", msg, certificate->version));
        status = AJ_ERR_SECURITY;
        goto Exit;
    }

    /*
     * Verify the leaf certificate
     */
    if (0 != memcmp(&certificate->digest, verifier, sizeof (verifier))) {
        AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=0x%p): Invalid verifier\n", msg));
        status = AJ_ERR_SECURITY;
        goto Exit;
    }
    status = AJ_VerifyCertificate(certificate);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=0x%p): Invalid certificate\n", msg));
        goto Exit;
    }
    memcpy(&issuer, &certificate->issuer, sizeof (ecc_publickey));
    validity.validfrom = 0x00000000;
    validity.validto   = 0xFFFFFFFF;

    /*
     * Unmarshal certificate chain - verify as we go
     */
    while (AJ_OK == status) {
        /*
         * Ask the application if we trust the issuer
         */
        if (!trusted) {
            b8 = (uint8_t*) AJ_Malloc(sizeof (issuer));
            if (!b8) {
                AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=0x%p): AJ_ERR_RESOURCES\n", msg));
                status = AJ_ERR_RESOURCES;
                goto Exit;
            }
            AJ_BigEndianEncodePublicKey(&issuer, b8);
            credential.data = b8;
            credential.len  = sizeof (ecc_publickey);
            status = kactx.authlistener(AUTH_SUITE_ECDHE_ECDSA, AJ_CRED_CERT_TRUST, &credential);
            if (AJ_OK == status) {
                trusted = 1;
            }
            AJ_Free(b8);
        }

        status = ECDSA_UnmarshalCertificate(msg, certificate, role);
        if (AJ_OK != status) {
            break;
        }

        /*
         * Check the chaining conditions
         */
        if (0 == certificate->version) {
            AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=0x%p): Invalid certificate version %X\n", msg, certificate->version));
            status = AJ_ERR_SECURITY;
            goto Exit;
        }
        if (!isfirst && !delegate) {
            AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=0x%p): Certificate delegation invalid\n", msg));
            status = AJ_ERR_SECURITY;
            goto Exit;
        }
        if (0 != memcmp(&issuer, &certificate->subject, sizeof (ecc_publickey))) {
            AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=0x%p): Certificate chaining invalid\n", msg));
            status = AJ_ERR_SECURITY;
            goto Exit;
        }

        status = AJ_VerifyCertificate(certificate);
        if (AJ_OK != status) {
            AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=0x%p): Invalid certificate\n", msg));
            goto Exit;
        }
        isfirst = 0;
        memcpy(&issuer, &certificate->issuer, sizeof (ecc_publickey));
        /*
         * If parent certificate has a shorter validity, use that instead
         */
        if (certificate->validity.validfrom > validity.validfrom) {
            validity.validfrom = certificate->validity.validfrom;
        }
        if (certificate->validity.validto < validity.validto) {
            validity.validto = certificate->validity.validto;
        }
    }
    if (AJ_ERR_NO_MORE == status) {
        status = AJ_UnmarshalCloseContainer(msg, &array1);
        if (AJ_OK != status) {
            AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=0x%p): Unmarshal container error\n", msg));
            goto Exit;
        }
    }
    if (AJ_OK != status) {
        AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=0x%p): Unmarshal certificate error\n", msg));
        goto Exit;
    }
    if (trusted) {
        /*
         * Notify the application what the root certificate was
         */
        b8 = (uint8_t*) AJ_Malloc(certificate->size);
        if (!b8) {
            AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=0x%p): AJ_ERR_RESOURCES\n", msg));
            status = AJ_ERR_RESOURCES;
            goto Exit;
        }
        status = AJ_BigEndianEncodeCertificate(certificate, b8, certificate->size);
        if (AJ_OK == status) {
            credential.data = b8;
            credential.len  = certificate->size;
            kactx.authlistener(AUTH_SUITE_ECDHE_ECDSA, AJ_CRED_CERT_ROOT, &credential);
        }
        AJ_Free(b8);
    } else {
        AJ_InfoPrintf(("AJ_ECDSA_Unmarshal(msg=0x%p): Certificate issuer not trusted\n", msg));
        status = AJ_ERR_SECURITY;
    }

Exit:
    AJ_Free(certificate);

    return status;
}

AJ_Status ECDSA_Final(uint32_t* expiration)
{
    AJ_InfoPrintf(("AJ_ECDSA_Final()\n"));

    *expiration = kactx.expiration;
    if (ecdsactx.certchain) {
        AJ_Free(ecdsactx.certchain);
    }
    ecdsactx.certchain = NULL;

    return AJ_OK;
}
#endif

#ifdef AUTH_PSK
void AJ_PSK_SetPwdCallback(AJ_AuthPwdFunc pwdcallback)
{
    AJ_InfoPrintf(("AJ_PSK_SetPwdCallback()\n"));
    pskctx.pwdcallback = pwdcallback;
}

static AJ_Status PSK_Init(AJ_AuthListenerFunc authlistener, const uint8_t* mastersecret, size_t mastersecretlen, AJ_SHA256_Context* hash)
{
    AJ_InfoPrintf(("AJ_PSK_Init()\n"));

    /* mastersecret, hash will not be NULL */
    kactx.mastersecret = (uint8_t*) mastersecret;
    kactx.mastersecretlen = mastersecretlen;
    kactx.hash = hash;

    /* authlistener might be NULL */
    kactx.authlistener = authlistener;

    pskctx.hint = NULL;
    pskctx.hintlen = 0;
    pskctx.psk  = NULL;
    pskctx.psklen  = 0;

    return AJ_OK;
}

static AJ_Status PSK_Marshal(AJ_Message* msg, uint8_t role)
{
    AJ_Status status;
    AJ_Credential cred;
    uint8_t verifier[AUTH_VERIFIER_LEN];
    const char* anon = "<anonymous>";

    AJ_InfoPrintf(("AJ_PSK_Marshal(msg=0x%p)\n", msg));

    if (pskctx.hint) {
        /*
         * Client has set it.
         */
        cred.mask = AJ_CRED_PUB_KEY;
        cred.data = pskctx.hint;
        cred.len  = pskctx.hintlen;
    } else {
        /*
         * Client to set it.
         */
        status = AJ_ERR_INVALID;
        if (kactx.authlistener) {
            status = kactx.authlistener(AUTH_SUITE_ECDHE_PSK, AJ_CRED_PUB_KEY, &cred);
        }
        if (AJ_OK == status) {
            pskctx.hintlen = cred.len;
            pskctx.hint = (uint8_t*) AJ_Malloc(pskctx.hintlen);
            if (!pskctx.hint) {
                AJ_WarnPrintf(("AJ_PSK_Marshal(msg=0x%p): AJ_ERR_RESOURCES\n", msg));
                return AJ_ERR_RESOURCES;
            }
            memcpy(pskctx.hint, cred.data, pskctx.hintlen);
        } else {
            /*
             * No hint - use anonymous
             */
            pskctx.hintlen = strlen(anon);
            pskctx.hint = (uint8_t*) AJ_Malloc(pskctx.hintlen);
            if (!pskctx.hint) {
                AJ_WarnPrintf(("AJ_PSK_Marshal(msg=0x%p): AJ_ERR_RESOURCES\n", msg));
                return AJ_ERR_RESOURCES;
            }
            memcpy(pskctx.hint, anon, pskctx.hintlen);
        }
    }
    if (pskctx.psk) {
        /*
         * Already saved PSK
         */
    } else if (kactx.authlistener) {
        status = kactx.authlistener(AUTH_SUITE_ECDHE_PSK, AJ_CRED_PRV_KEY, &cred);
        if (AJ_OK != status) {
            AJ_WarnPrintf(("AJ_PSK_Marshal(msg=0x%p): No PSK supplied\n", msg));
            return AJ_ERR_SECURITY;
        }
        pskctx.psklen = cred.len;
        pskctx.psk = (uint8_t*) AJ_Malloc(pskctx.psklen);
        if (!pskctx.psk) {
            AJ_WarnPrintf(("AJ_PSK_Marshal(msg=0x%p): AJ_ERR_RESOURCES\n", msg));
            return AJ_ERR_RESOURCES;
        }
        memcpy(pskctx.psk, cred.data, pskctx.psklen);
        kactx.expiration = cred.expiration;
    } else if (pskctx.pwdcallback) {
        /*
         * Assume application does not copy in more than this size buffer
         * Expiration not set by application
         */
        pskctx.psk = (uint8_t*) AJ_Malloc(16);
        if (!pskctx.psk) {
            AJ_WarnPrintf(("AJ_PSK_Marshal(msg=0x%p): AJ_ERR_RESOURCES\n", msg));
            return AJ_ERR_RESOURCES;
        }
        pskctx.psklen = pskctx.pwdcallback(pskctx.psk, 16);
        kactx.expiration = 0xFFFFFFFF;
    } else {
        AJ_WarnPrintf(("AJ_PSK_Marshal(msg=0x%p): No PSK supplied\n", msg));
        return AJ_ERR_SECURITY;
    }

    if (AUTH_CLIENT == role) {
        AJ_SHA256_Update(kactx.hash, pskctx.hint, pskctx.hintlen);
        AJ_SHA256_Update(kactx.hash, pskctx.psk, pskctx.psklen);
        status = ComputeVerifier("client finished", verifier, sizeof (verifier));
        AJ_SHA256_Update(kactx.hash, verifier, sizeof (verifier));
    } else {
        status = ComputeVerifier("server finished", verifier, sizeof (verifier));
    }
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_PSK_Marshal(msg=0x%p): Compute verifier error\n", msg));
        return AJ_ERR_SECURITY;
    }
    status = AJ_MarshalVariant(msg, "(ayay)");
    status = AJ_MarshalArgs(msg, "(ayay)", pskctx.hint, pskctx.hintlen, verifier, sizeof (verifier));

    return status;
}

static AJ_Status PSK_Unmarshal(AJ_Message* msg, uint8_t role)
{
    AJ_Status status;
    AJ_Credential cred;
    char* variant;
    uint8_t verifier[AUTH_VERIFIER_LEN];
    uint8_t* remotehint;
    uint8_t* remotesig;
    size_t remotehintlen;
    size_t remotesiglen;

    AJ_InfoPrintf(("AJ_PSK_Unmarshal(msg=0x%p)\n", msg));

    status = AJ_UnmarshalVariant(msg, (const char**) &variant);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("AJ_PSK_Unmarshal(msg=0x%p): Unmarshal variant error\n", msg));
        return AJ_ERR_SECURITY;
    }
    if (0 != strncmp(variant, "(ayay)", 6)) {
        AJ_InfoPrintf(("AJ_PSK_Unmarshal(msg=0x%p): Invalid variant\n", msg));
        return AJ_ERR_SECURITY;
    }
    status = AJ_UnmarshalArgs(msg, "(ayay)", &remotehint, &remotehintlen, &remotesig, &remotesiglen);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("AJ_PSK_Unmarshal(msg=0x%p): Unmarshal error\n", msg));
        return AJ_ERR_SECURITY;
    }
    if (AUTH_VERIFIER_LEN != remotesiglen) {
        AJ_InfoPrintf(("AJ_PSK_Unmarshal(msg=0x%p): Invalid signature size\n", msg));
        return AJ_ERR_SECURITY;
    }

    if (pskctx.hint) {
        if (pskctx.hintlen != remotehintlen) {
            AJ_InfoPrintf(("AJ_PSK_Unmarshal(msg=0x%p): Invalid hint size\n", msg));
            return AJ_ERR_SECURITY;
        }
        if (0 != memcmp(pskctx.hint, remotehint, pskctx.hintlen)) {
            AJ_InfoPrintf(("AJ_PSK_Unmarshal(msg=0x%p): Invalid hint\n", msg));
            return AJ_ERR_SECURITY;
        }
    } else {
        pskctx.hintlen = remotehintlen;
        pskctx.hint = (uint8_t*) AJ_Malloc(pskctx.hintlen);
        if (!pskctx.hint) {
            AJ_WarnPrintf(("AJ_PSK_Unmarshal(msg=0x%p): AJ_ERR_RESOURCES\n", msg));
            return AJ_ERR_RESOURCES;
        }
        memcpy(pskctx.hint, remotehint, pskctx.hintlen);
    }
    if (pskctx.psk) {
        /*
         * Already saved PSK
         */
    } else if (kactx.authlistener) {
        cred.mask = AJ_CRED_PUB_KEY;
        cred.data = pskctx.hint;
        cred.len  = pskctx.hintlen;
        status = kactx.authlistener(AUTH_SUITE_ECDHE_PSK, AJ_CRED_PRV_KEY, &cred);
        if (AJ_OK != status) {
            AJ_WarnPrintf(("AJ_PSK_Unmarshal(msg=0x%p): No PSK supplied\n", msg));
            return AJ_ERR_SECURITY;
        }
        pskctx.psklen = cred.len;
        pskctx.psk = (uint8_t*) AJ_Malloc(pskctx.psklen);
        if (!pskctx.psk) {
            AJ_WarnPrintf(("AJ_PSK_Unmarshal(msg=0x%p): AJ_ERR_RESOURCES\n", msg));
            return AJ_ERR_RESOURCES;
        }
        memcpy(pskctx.psk, cred.data, pskctx.psklen);
        kactx.expiration = cred.expiration;
    } else if (pskctx.pwdcallback) {
        /*
         * Assume application does not copy in more than this size buffer
         * Expiration not set by application
         */
        pskctx.psk = (uint8_t*) AJ_Malloc(16);
        if (!pskctx.psk) {
            AJ_WarnPrintf(("AJ_PSK_Unmarshal(msg=0x%p): AJ_ERR_RESOURCES\n", msg));
            return AJ_ERR_RESOURCES;
        }
        pskctx.psklen = pskctx.pwdcallback(pskctx.psk, 16);
        kactx.expiration = 0xFFFFFFFF;
    } else {
        AJ_WarnPrintf(("AJ_PSK_Unmarshal(msg=0x%p): No PSK supplied\n", msg));
        return AJ_ERR_SECURITY;
    }

    if (AUTH_CLIENT == role) {
        status = ComputeVerifier("server finished", verifier, sizeof (verifier));
    } else {
        AJ_SHA256_Update(kactx.hash, pskctx.hint, pskctx.hintlen);
        AJ_SHA256_Update(kactx.hash, pskctx.psk, pskctx.psklen);
        status = ComputeVerifier("client finished", verifier, sizeof (verifier));
        AJ_SHA256_Update(kactx.hash, verifier, sizeof (verifier));
    }
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_PSK_Unmarshal(msg=0x%p): Compute verifier error\n", msg));
        return AJ_ERR_SECURITY;
    }
    if (0 != memcmp(verifier, remotesig, AUTH_VERIFIER_LEN)) {
        AJ_InfoPrintf(("AJ_PSK_Unmarshal(msg=0x%p): Invalid verifier\n", msg));
        return AJ_ERR_SECURITY;
    }

    return status;
}

AJ_Status PSK_Final(uint32_t* expiration)
{
    AJ_InfoPrintf(("AJ_PSK_Final()\n"));

    *expiration = kactx.expiration;
    if (pskctx.hint) {
        AJ_Free(pskctx.hint);
        pskctx.hint = NULL;
    }
    if (pskctx.psk) {
        AJ_Free(pskctx.psk);
        pskctx.psk  = NULL;
    }

    return AJ_OK;
}
#endif

#ifdef AUTH_NULL
static AJ_Status NULL_Init(AJ_AuthListenerFunc authlistener, const uint8_t* mastersecret, size_t mastersecretlen, AJ_SHA256_Context* hash)
{
    AJ_Status status;
    AJ_Credential cred;

    AJ_InfoPrintf(("AJ_NULL_Init()\n"));

    /* mastersecret, hash, authlistener will not be NULL */
    kactx.mastersecret = (uint8_t*) mastersecret;
    kactx.mastersecretlen = mastersecretlen;
    kactx.hash = hash;
    status = authlistener(AUTH_SUITE_ECDHE_NULL, 0, &cred);
    kactx.expiration = cred.expiration;

    return status;
}

static AJ_Status NULL_Marshal(AJ_Message* msg, uint8_t role)
{
    AJ_Status status;
    uint8_t verifier[AUTH_VERIFIER_LEN];

    AJ_InfoPrintf(("AJ_NULL_Marshal(msg=0x%p)\n", msg));

    if (AUTH_CLIENT == role) {
        status = ComputeVerifier("client finished", verifier, sizeof (verifier));
    } else {
        status = ComputeVerifier("server finished", verifier, sizeof (verifier));
    }
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_NULL_Marshal(msg=0x%p): Compute verifier error\n", msg));
        return AJ_ERR_SECURITY;
    }
    status = AJ_MarshalVariant(msg, "ay");
    status = AJ_MarshalArgs(msg, "ay", verifier, sizeof (verifier));
    AJ_SHA256_Update(kactx.hash, verifier, sizeof (verifier));

    return status;
}

static AJ_Status NULL_Unmarshal(AJ_Message* msg, uint8_t role)
{
    AJ_Status status;
    char* variant;
    uint8_t verifier[AUTH_VERIFIER_LEN];
    uint8_t* remotesig;
    size_t remotesiglen;

    AJ_InfoPrintf(("AJ_NULL_Unmarshal(msg=0x%p)\n", msg));

    if (AUTH_CLIENT == role) {
        status = ComputeVerifier("server finished", verifier, sizeof (verifier));
    } else {
        status = ComputeVerifier("client finished", verifier, sizeof (verifier));
    }
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_NULL_Unmarshal(msg=0x%p): Compute verifier error\n", msg));
        return AJ_ERR_SECURITY;
    }

    status = AJ_UnmarshalVariant(msg, (const char**) &variant);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("AJ_NULL_Unmarshal(msg=0x%p): Unmarshal variant error\n", msg));
        return AJ_ERR_SECURITY;
    }
    if (0 != strncmp(variant, "ay", 4)) {
        AJ_InfoPrintf(("AJ_NULL_Unmarshal(msg=0x%p): Invalid variant\n", msg));
        return AJ_ERR_SECURITY;
    }
    status = AJ_UnmarshalArgs(msg, "ay", &remotesig, &remotesiglen);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("AJ_NULL_Unmarshal(msg=0x%p): Unmarshal error\n", msg));
        return AJ_ERR_SECURITY;
    }
    if (AUTH_VERIFIER_LEN != remotesiglen) {
        AJ_InfoPrintf(("AJ_NULL_Unmarshal(msg=0x%p): Invalid signature size\n", msg));
        return AJ_ERR_SECURITY;
    }
    if (0 != memcmp(verifier, remotesig, AUTH_VERIFIER_LEN)) {
        AJ_InfoPrintf(("AJ_NULL_Unmarshal(msg=0x%p): Invalid verifier\n", msg));
        return AJ_ERR_SECURITY;
    }
    AJ_SHA256_Update(kactx.hash, verifier, sizeof (verifier));

    return status;
}

AJ_Status NULL_Final(uint32_t* expiration)
{
    AJ_InfoPrintf(("AJ_NULL_Final()\n"));

    *expiration = kactx.expiration;
    return AJ_OK;
}
#endif
