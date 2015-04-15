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
#define AJ_MODULE TEST_CERTIFICATE

#include <stdio.h>
#include <stdlib.h>
#include <time.h>
#include "aj_debug.h"
#include "alljoyn.h"
#include "aj_cert.h"
#include "aj_crypto.h"

uint8_t dbgTEST_CERTIFICATE = 0;

static const char intfc[] = "org.alljoyn.test";

static void CreateManifest(uint8_t** manifest, size_t* len)
{
    *len = strlen(intfc);
    *manifest = (uint8_t*) AJ_Malloc(*len);
    AJ_ASSERT(*manifest);
    memcpy(*manifest, (uint8_t*) intfc, *len);
}

static void ManifestDigest(uint8_t* manifest, size_t* len, uint8_t* digest)
{
    AJ_SHA256_Context sha;
    AJ_SHA256_Init(&sha);
    AJ_SHA256_Update(&sha, (const uint8_t*) manifest, *len);
    AJ_SHA256_Final(&sha, digest);
}


int AJ_Main(int ac, char** av)
{
    AJ_Status status = AJ_OK;
    size_t num = 2;
    size_t i;
    uint8_t* b8;
    char* pem;
    size_t pemlen;
    ecc_privatekey root_prvkey;
    ecc_publickey root_pubkey;
    uint8_t* manifest;
    size_t manifestlen;
    uint8_t digest[SHA256_DIGEST_LENGTH];
    ecc_privatekey peer_prvkey;
    ecc_publickey peer_pubkey;
    AJ_Certificate* cert;
    AJ_GUID guild;

    /*
     * Create an owner key pair
     */
    AJ_GenerateDSAKeyPair(&root_pubkey, &root_prvkey);

    b8 = (uint8_t*) AJ_Malloc(sizeof (ecc_publickey));
    AJ_ASSERT(b8);
    status = AJ_BigEndianEncodePublicKey(&root_pubkey, b8);
    AJ_ASSERT(AJ_OK == status);
    pemlen = 4 * ((sizeof (ecc_publickey) + 2) / 3) + 1;
    pem = (char*) AJ_Malloc(pemlen);
    status = AJ_RawToB64(b8, sizeof (ecc_publickey), pem, pemlen);
    AJ_ASSERT(AJ_OK == status);
    AJ_Printf("Owner Public Key\n");
    AJ_Printf("-----BEGIN PUBLIC KEY-----\n%s\n-----END PUBLIC KEY-----\n", pem);
    AJ_Free(b8);
    AJ_Free(pem);

    CreateManifest(&manifest, &manifestlen);
    ManifestDigest(manifest, &manifestlen, digest);

    AJ_RandBytes((uint8_t*) &guild, sizeof (AJ_GUID));

    for (i = 0; i < num; i++) {
        AJ_GenerateDSAKeyPair(&peer_pubkey, &peer_prvkey);

        b8 = (uint8_t*) AJ_Malloc(sizeof (ecc_publickey));
        AJ_ASSERT(b8);
        status = AJ_BigEndianEncodePublicKey(&peer_pubkey, b8);
        AJ_ASSERT(AJ_OK == status);
        pemlen = 4 * ((sizeof (ecc_publickey) + 2) / 3) + 1;
        pem = (char*) AJ_Malloc(pemlen);
        status = AJ_RawToB64(b8, sizeof (ecc_publickey), pem, pemlen);
        AJ_ASSERT(AJ_OK == status);
        AJ_Printf("Peer Public Key\n");
        AJ_Printf("-----BEGIN PUBLIC KEY-----\n%s\n-----END PUBLIC KEY-----\n", pem);
        AJ_Free(b8);
        AJ_Free(pem);

        b8 = (uint8_t*) AJ_Malloc(sizeof (ecc_privatekey));
        AJ_ASSERT(b8);
        status = AJ_BigEndianEncodePrivateKey(&peer_prvkey, b8);
        AJ_ASSERT(AJ_OK == status);
        pemlen = 4 * ((sizeof (ecc_privatekey) + 2) / 3) + 1;
        pem = (char*) AJ_Malloc(pemlen);
        status = AJ_RawToB64(b8, sizeof (ecc_privatekey), pem, pemlen);
        AJ_ASSERT(AJ_OK == status);
        AJ_Printf("Peer Private Key\n");
        AJ_Printf("-----BEGIN PRIVATE KEY-----\n%s\n-----END PRIVATE KEY-----\n", pem);
        AJ_Free(b8);
        AJ_Free(pem);

        cert = (AJ_Certificate*) AJ_Malloc(sizeof (AJ_Certificate));
        AJ_ASSERT(cert);
        status = AJ_CreateCertificate(cert, 0, &peer_pubkey, NULL, NULL, digest, 0);
        AJ_ASSERT(AJ_OK == status);
        status = AJ_SignCertificate(cert, &peer_prvkey);
        AJ_ASSERT(AJ_OK == status);
        status = AJ_VerifyCertificate(cert);
        AJ_ASSERT(AJ_OK == status);

        b8 = (uint8_t*) AJ_Malloc(sizeof (AJ_Certificate));
        AJ_ASSERT(b8);
        status = AJ_BigEndianEncodeCertificate(cert, b8, sizeof (AJ_Certificate));
        AJ_ASSERT(AJ_OK == status);
        pemlen = 4 * ((sizeof (AJ_Certificate) + 2) / 3) + 1;
        pem = (char*) AJ_Malloc(pemlen);
        status = AJ_RawToB64(b8, cert->size, pem, pemlen);
        AJ_ASSERT(AJ_OK == status);
        AJ_Printf("Peer Certificate (Type 0)\n");
        AJ_Printf("-----BEGIN CERTIFICATE-----\n%s\n-----END CERTIFICATE-----\n", pem);

        status = AJ_CreateCertificate(cert, 1, &root_pubkey, &peer_pubkey, NULL, digest, 0);
        AJ_ASSERT(AJ_OK == status);
        status = AJ_SignCertificate(cert, &root_prvkey);
        AJ_ASSERT(AJ_OK == status);
        status = AJ_VerifyCertificate(cert);
        AJ_ASSERT(AJ_OK == status);

        status = AJ_BigEndianEncodeCertificate(cert, b8, sizeof (AJ_Certificate));
        AJ_ASSERT(AJ_OK == status);
        status = AJ_RawToB64(b8, cert->size, pem, pemlen);
        AJ_ASSERT(AJ_OK == status);
        AJ_Printf("Root Certificate (Type 1)\n");
        AJ_Printf("-----BEGIN CERTIFICATE-----\n%s\n-----END CERTIFICATE-----\n", pem);

        status = AJ_CreateCertificate(cert, 2, &root_pubkey, &peer_pubkey, &guild, digest, 0);
        AJ_ASSERT(AJ_OK == status);
        status = AJ_SignCertificate(cert, &root_prvkey);
        AJ_ASSERT(AJ_OK == status);
        status = AJ_VerifyCertificate(cert);
        AJ_ASSERT(AJ_OK == status);

        status = AJ_BigEndianEncodeCertificate(cert, b8, sizeof (AJ_Certificate));
        AJ_ASSERT(AJ_OK == status);
        status = AJ_RawToB64(b8, cert->size, pem, pemlen);
        AJ_ASSERT(AJ_OK == status);
        AJ_Printf("Root Certificate (Type 2)\n");
        AJ_Printf("-----BEGIN CERTIFICATE-----\n%s\n-----END CERTIFICATE-----\n", pem);
        AJ_Free(cert);
        AJ_Free(b8);
        AJ_Free(pem);
    }

    AJ_Free(manifest);

    return 0;
}

#ifdef AJ_MAIN
int main(int ac, char** av)
{
    return AJ_Main(ac, av);
}
#endif
