/**
 * @file aj_cert.c
 *
 * Utilites for SPKI-style Certificates
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
#define AJ_MODULE CERTIFICATE

#include "aj_debug.h"
#include "aj_cert.h"
#include "aj_util.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgCERTIFICATE = 0;
#endif

void HostU32ToBigEndianU8(uint32_t* u32, size_t len, uint8_t* u8)
{
    uint32_t x;
    size_t i;

    for (i = 0; i < len; i += sizeof (uint32_t)) {
        x = u32[i / sizeof (uint32_t)];
#if HOST_IS_LITTLE_ENDIAN
        x = AJ_ByteSwap32(x);
#endif
        memcpy(&u8[i], &x, sizeof (x));
    }
}

static void BigEndianU8ToHostU32(uint8_t* u8, uint32_t* u32, size_t len)
{
    uint32_t x;
    size_t i;

    for (i = 0; i < len; i += sizeof (uint32_t)) {
        memcpy(&x, &u8[i], sizeof (x));
#if HOST_IS_LITTLE_ENDIAN
        x = AJ_ByteSwap32(x);
#endif
        u32[i / sizeof (uint32_t)] = x;
    }
}

static void HostU64ToBigEndianU8(uint64_t* u64, size_t len, uint8_t* u8)
{
    uint64_t x;
    size_t i;

    for (i = 0; i < len; i += sizeof (uint64_t)) {
        x = u64[i / sizeof (uint64_t)];
#if HOST_IS_LITTLE_ENDIAN
        x = AJ_ByteSwap64(x);
#endif
        memcpy(&u8[i], &x, sizeof (x));
    }
}

static void BigEndianU8ToHostU64(uint8_t* u8, uint64_t* u64, size_t len)
{
    uint64_t x;
    size_t i;

    for (i = 0; i < len; i += sizeof (uint64_t)) {
        memcpy(&x, &u8[i], sizeof (x));
#if HOST_IS_LITTLE_ENDIAN
        x = AJ_ByteSwap64(x);
#endif
        u64[i / sizeof (uint64_t)] = x;
    }
}

AJ_Status AJ_BigEndianEncodePublicKey(ecc_publickey* publickey, uint8_t* b8)
{
    HostU32ToBigEndianU8((uint32_t*) publickey, sizeof (ecc_publickey), b8);
    return AJ_OK;
}

AJ_Status AJ_BigEndianDecodePublicKey(ecc_publickey* publickey, uint8_t* b8)
{
    BigEndianU8ToHostU32(b8, (uint32_t*) publickey, sizeof (ecc_publickey));
    return AJ_OK;
}

AJ_Status AJ_BigEndianEncodePrivateKey(ecc_privatekey* privatekey, uint8_t* b8)
{
    HostU32ToBigEndianU8((uint32_t*) privatekey, sizeof (ecc_privatekey), b8);
    return AJ_OK;
}

AJ_Status AJ_BigEndianDecodePrivateKey(ecc_privatekey* privatekey, uint8_t* b8)
{
    BigEndianU8ToHostU32(b8, (uint32_t*) privatekey, sizeof (ecc_privatekey));
    return AJ_OK;
}

static void CertificateSize(AJ_Certificate* certificate)
{
    certificate->size = sizeof (uint32_t) + sizeof (ecc_publickey) + SHA256_DIGEST_LENGTH;
    switch (certificate->version) {
    case 1:
    case 2:
        certificate->size += sizeof (ecc_publickey) + 2 * sizeof (uint64_t) + sizeof (uint8_t);
        break;
    }
    switch (certificate->version) {
    case 2:
        certificate->size += AJ_GUID_LENGTH;
        break;
    }
    certificate->size += sizeof (ecc_signature);
}

AJ_Status AJ_BigEndianEncodeCertificate(AJ_Certificate* certificate, uint8_t* b8, size_t b8len)
{
    if (b8len < sizeof (uint32_t)) {
        /* Require at least a version */
        return AJ_ERR_RESOURCES;
    }
    HostU32ToBigEndianU8(&certificate->version, sizeof (uint32_t), b8);
    b8 += sizeof (uint32_t);
    CertificateSize(certificate);
    if (b8len < certificate->size) {
        return AJ_ERR_RESOURCES;
    }
    AJ_BigEndianEncodePublicKey(&certificate->issuer, b8);
    b8 += sizeof (ecc_publickey);
    switch (certificate->version) {
    case 1:
    case 2:
        AJ_BigEndianEncodePublicKey(&certificate->subject, b8);
        b8 += sizeof (ecc_publickey);
        HostU64ToBigEndianU8(&certificate->validity.validfrom, sizeof (uint64_t), b8);
        b8 += sizeof (uint64_t);
        HostU64ToBigEndianU8(&certificate->validity.validto, sizeof (uint64_t), b8);
        b8 += sizeof (uint64_t);
        *b8 = certificate->delegate;
        b8 += sizeof (uint8_t);
        break;
    }
    switch (certificate->version) {
    case 2:
        memcpy(b8, certificate->guild, AJ_GUID_LENGTH);
        b8 += AJ_GUID_LENGTH;
        break;
    }
    memcpy(b8, certificate->digest, SHA256_DIGEST_LENGTH);
    b8 += SHA256_DIGEST_LENGTH;
    HostU32ToBigEndianU8((uint32_t*) &certificate->signature, sizeof (ecc_signature), b8);
    b8 += sizeof (ecc_signature);

    return AJ_OK;
}

AJ_Status AJ_BigEndianDecodeCertificate(AJ_Certificate* certificate, uint8_t* b8, size_t b8len)
{
    if (b8len < sizeof (uint32_t)) {
        /* Require at least a version */
        return AJ_ERR_RESOURCES;
    }
    BigEndianU8ToHostU32(b8, &certificate->version, sizeof (uint32_t));
    b8 += sizeof (uint32_t);
    CertificateSize(certificate);
    if (b8len < certificate->size) {
        return AJ_ERR_RESOURCES;
    }
    AJ_BigEndianDecodePublicKey(&certificate->issuer, b8);
    b8 += sizeof (ecc_publickey);
    switch (certificate->version) {
    case 1:
    case 2:
        AJ_BigEndianDecodePublicKey(&certificate->subject, b8);
        b8 += sizeof (ecc_publickey);
        BigEndianU8ToHostU64(b8, &certificate->validity.validfrom, sizeof (uint64_t));
        b8 += sizeof (uint64_t);
        BigEndianU8ToHostU64(b8, &certificate->validity.validto, sizeof (uint64_t));
        b8 += sizeof (uint64_t);
        certificate->delegate = *b8;
        b8 += sizeof (uint8_t);
        break;
    }
    switch (certificate->version) {
    case 2:
        memcpy(certificate->guild, b8, AJ_GUID_LENGTH);
        b8 += AJ_GUID_LENGTH;
        break;
    }
    memcpy(certificate->digest, b8, SHA256_DIGEST_LENGTH);
    b8 += SHA256_DIGEST_LENGTH;
    BigEndianU8ToHostU32(b8, (uint32_t*) &certificate->signature, sizeof (ecc_signature));
    b8 += sizeof (ecc_signature);

    return AJ_OK;
}

AJ_Status AJ_CreateCertificate(AJ_Certificate* certificate, const uint32_t version, const ecc_publickey* issuer, const ecc_publickey* subject, const AJ_GUID* guild, const uint8_t* digest, const uint8_t delegate)
{
    certificate->version = version;
    memcpy((uint8_t*) &certificate->issuer, (uint8_t*) issuer, sizeof (ecc_publickey));
    switch (version) {
    case 1:
    case 2:
        memcpy((uint8_t*) &certificate->subject, (uint8_t*) subject, sizeof (ecc_publickey));
        certificate->validity.validfrom = 0;
        certificate->validity.validto   = 0xFFFFFFFF;
        certificate->delegate = delegate;
        break;
    }
    switch (version) {
    case 2:
        memcpy((uint8_t*) &certificate->guild, (uint8_t*) guild, AJ_GUID_LENGTH);
        break;
    }
    memcpy((uint8_t*) &certificate->digest, (uint8_t*) digest, SHA256_DIGEST_LENGTH);

    CertificateSize(certificate);

    return AJ_OK;
}

static AJ_Status CertificateDigest(AJ_Certificate* certificate, uint8_t* digest)
{
    AJ_SHA256_Context ctx;
    uint8_t buf[sizeof (AJ_Certificate)];

    AJ_BigEndianEncodeCertificate(certificate, buf, sizeof (buf));
    AJ_SHA256_Init(&ctx);
    AJ_SHA256_Update(&ctx, buf, certificate->size - sizeof (ecc_signature));
    AJ_SHA256_Final(&ctx, digest);

    return AJ_OK;
}

AJ_Status AJ_SignCertificate(AJ_Certificate* certificate, const ecc_privatekey* issuer_private)
{
    AJ_Status status = AJ_ERR_SECURITY;
    uint8_t digest[SHA256_DIGEST_LENGTH];

    switch (certificate->version) {
    case 0:
        status = AJ_DSASignDigest(certificate->digest, issuer_private, &certificate->signature);
        break;

    case 1:
    case 2:
        status = CertificateDigest(certificate, digest);
        status = AJ_DSASignDigest(digest, issuer_private, &certificate->signature);
        break;
    }

    return status;
}

AJ_Status AJ_VerifyCertificate(AJ_Certificate* certificate)
{
    AJ_Status status = AJ_ERR_SECURITY;
    uint8_t digest[SHA256_DIGEST_LENGTH];

    switch (certificate->version) {
    case 0:
        status = AJ_DSAVerifyDigest(certificate->digest, &certificate->signature, &certificate->issuer);
        break;

    case 1:
    case 2:
        status = CertificateDigest(certificate, digest);
        status = AJ_DSAVerifyDigest(digest, &certificate->signature, &certificate->issuer);
        break;
    }

    return status;
}
