#ifndef _AJ_CERT_H
#define _AJ_CERT_H

/**
 * @file
 *
 * Header file for certificate utilities
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

#include "aj_guid.h"
#include "aj_crypto_ecc.h"
#include "aj_crypto_sha2.h"

#ifdef __cplusplus
extern "C" {
#endif

#define MAX_NUM_CERTIFICATES 2

/*
 * Certificate types are native.
 * Conversion to network byte-order (big-endian) is done via encoding and decoding.
 */
typedef struct _AJ_Validity {
    uint64_t validfrom;
    uint64_t validto;
} AJ_Validity;

#define AJ_GUID_LENGTH (sizeof (AJ_GUID))
typedef struct _AJ_Certificate {
    uint32_t version;
    ecc_publickey issuer;
    ecc_publickey subject;
    AJ_Validity validity;
    uint8_t delegate;
    uint8_t guild[AJ_GUID_LENGTH];
    uint8_t digest[SHA256_DIGEST_LENGTH];
    ecc_signature signature;
    uint32_t size;
} AJ_Certificate;

/**
 * Convert unsigned 32-bit int array to network order (big endian) bytes.
 *
 * @param u32  Unsigned 32-bit array
 * @param len  Length of 32-bit array
 * @param u8   Unsigned 8-bit array
 *
 */
void HostU32ToBigEndianU8(uint32_t* u32, size_t len, uint8_t* u8);

/**
 * Encode native public key to network order bytes.
 *
 * @param publickey  The ECC public key
 * @param[out] b8    Big endian byte array
 *
 * @return AJ_OK
 *
 */
AJ_Status AJ_BigEndianEncodePublicKey(ecc_publickey* publickey, uint8_t* b8);

/**
 * Decode network order bytes to native public key.
 *
 * @param[out] publickey  The ECC public key
 * @param b8    Big endian byte array
 *
 * @return AJ_OK
 *
 */
AJ_Status AJ_BigEndianDecodePublicKey(ecc_publickey* publickey, uint8_t* b8);

/**
 * Encode native private key to network order bytes.
 *
 * @param privatekey The ECC private key
 * @param[out] b8    Big endian byte array
 *
 * @return AJ_OK
 *
 */
AJ_Status AJ_BigEndianEncodePrivateKey(ecc_privatekey* privatekey, uint8_t* b8);

/**
 * Decode network order bytes to native private key.
 *
 * @param[out] privatekey The ECC private key
 * @param b8    Big endian byte array
 *
 * @return AJ_OK
 *
 */
AJ_Status AJ_BigEndianDecodePrivateKey(ecc_privatekey* privatekey, uint8_t* b8);

/**
 * Encode native certificate to network order bytes.
 *
 * @param certificate The ECDSA certificate
 * @param[out] b8    Big endian byte array
 * @param b8len       Byte array length
 *
 * @return
 *         - AJ_OK on success
 *         - AJ_ERR_RESOURCES if buffer not large enough
 *
 */
AJ_Status AJ_BigEndianEncodeCertificate(AJ_Certificate* certificate, uint8_t* b8, size_t b8len);

/**
 * Decode network order bytes to native certificate.
 *
 * @param[out] certificate The ECDSA certificate
 * @param b8    Big endian byte array
 * @param b8len       Byte array length
 *
 * @return
 *         - AJ_OK on success
 *         - AJ_ERR_RESOURCES if buffer not large enough
 *
 */
AJ_Status AJ_BigEndianDecodeCertificate(AJ_Certificate* certificate, uint8_t* b8, size_t b8len);

/**
 * Create certificate
 *
 * @param certificate The ECDSA certificate
 * @param version     Certificate version
 * @param issuer      Certificate issuer
 * @param subject     Certificate subject
 * @param guild       Certificate guild (optional)
 * @param digest      Certificate digest
 * @param delegate    Certificate delegate
 *
 * @return AJ_OK
 *
 */
AJ_Status AJ_CreateCertificate(AJ_Certificate* certificate, const uint32_t version,
                               const ecc_publickey* issuer, const ecc_publickey* subject,
                               const AJ_GUID* guild, const uint8_t* digest, const uint8_t delegate);

/**
 * Sign certificate
 *
 * @param certificate    The ECDSA certificate
 * @param issuer_private Private key of issuer
 *
 * @return
 *         - AJ_OK on success
 *         - AJ_ERR_SECURITY on sign error
 *
 */
AJ_Status AJ_SignCertificate(AJ_Certificate* certificate, const ecc_privatekey* issuer_private);

/**
 * Verify certificate
 *
 * @param certificate    The ECDSA certificate
 *
 * @return
 *         - AJ_OK on success
 *         - AJ_ERR_SECURITY on verify error
 *
 */
AJ_Status AJ_VerifyCertificate(AJ_Certificate* certificate);

#ifdef __cplusplus
}
#endif

#endif
