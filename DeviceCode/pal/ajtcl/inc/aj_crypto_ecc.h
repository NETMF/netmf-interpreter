#ifndef _AJ_CRYPTO_ECC_H
#define _AJ_CRYPTO_ECC_H

/**
 * @file aj_crypto_ecc.h
 * @defgroup aj_crypto Cryptographic Support
 * @{
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

#include "aj_target.h"
#include "aj_status.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef enum {B_FALSE, B_TRUE} boolean_t;


#define BIGLEN 9
/*
 * For P256 bigval_t types hold 288-bit 2's complement numbers (9
 * 32-bit words).  For P192 they hold 224-bit 2's complement numbers
 * (7 32-bit words).
 *
 * The representation is little endian by word and native endian
 * within each word.
 */

typedef struct {
    uint32_t data[BIGLEN];
} bigval_t;


typedef struct {
    bigval_t x;
    bigval_t y;
    uint32_t infinity;
} affine_point_t;

typedef struct {
    bigval_t r;
    bigval_t s;
} ECDSA_sig_t;

typedef bigval_t ecc_privatekey;
typedef affine_point_t ecc_publickey;
typedef affine_point_t ecc_secret;
typedef ECDSA_sig_t ecc_signature;

/**
 * ECC type sizes
 */
#define KEY_ECC_SZ (8 * sizeof (uint32_t))
#define KEY_ECC_PRV_SZ KEY_ECC_SZ
#define KEY_ECC_PUB_SZ (2 * KEY_ECC_SZ)
#define KEY_ECC_SEC_SZ (2 * KEY_ECC_SZ)
#define KEY_ECC_SIG_SZ (2 * KEY_ECC_SZ)

/**
 * Generates the Ephemeral Diffie-Hellman key pair.
 *
 * @param publicKey The output public key
 * @param privateKey The output private key
 *
 * @return  - AJ_OK if the key pair is successfully generated.
 *          - AJ_ERR_SECURITY otherwise
 */
AJ_Status AJ_GenerateDHKeyPair(ecc_publickey* publicKey, ecc_privatekey* privateKey);

/**
 * Generates the Diffie-Hellman share secret.
 *
 * @param peerPublicKey The peer's public key
 * @param privateKey The private key
 * @param secret The output share secret
 *
 * @return  - AJ_OK if the share secret is successfully generated.
 *          - AJ_ERR_SECURITY otherwise
 */
AJ_Status AJ_GenerateShareSecret(ecc_publickey* peerPublicKey, ecc_privatekey* privateKey, ecc_secret* secret);

/**
 * Generates the DSA key pair.
 *
 * @param publicKey The output public key
 * @param privateKey The output private key
 * @return  - AJ_OK if the key pair is successfully generated
 *          - AJ_ERR_SECURITY otherwise
 */
AJ_Status AJ_GenerateDSAKeyPair(ecc_publickey* publicKey, ecc_privatekey* privateKey);

/**
 * Sign a digest using the DSA key
 * @param digest The digest to sign
 * @param signingPrivateKey The signing private key
 * @param sig The output signature
 * @return  - AJ_OK if the signing process succeeds
 *          - AJ_ERR_SECURITY otherwise
 */
AJ_Status AJ_DSASignDigest(const uint8_t* digest, const ecc_privatekey* signingPrivateKey, ecc_signature* sig);

/**
 * Sign a buffer using the DSA key
 * @param buf The buffer to sign
 * @param len The buffer len
 * @param signingPrivateKey The signing private key
 * @param sig The output signature
 * @return  - AJ_OK if the signing process succeeds
 *          - AJ_ERR_SECURITY otherwise
 */
AJ_Status AJ_DSASign(const uint8_t* buf, uint16_t len, const ecc_privatekey* signingPrivateKey, ecc_signature* sig);

/**
 * Verify DSA signature of a digest
 * @param digest The digest to sign
 * @param sig The signature
 * @param pubKey The signing public key
 * @return  - AJ_OK if the signature verification succeeds
 *          - AJ_ERR_SECURITY otherwise
 */
AJ_Status AJ_DSAVerifyDigest(const uint8_t* digest, const ecc_signature* sig, const ecc_publickey* pubKey);

/**
 * Verify DSA signature of a buffer
 * @param buf The buffer to sign
 * @param len The buffer len
 * @param sig The signature
 * @param pubKey The signing public key
 * @return  - AJ_OK if the signature verification succeeds
 *          - AJ_ERR_SECURITY otherwise
 */
AJ_Status AJ_DSAVerify(const uint8_t* buf, uint16_t len, const ecc_signature* sig, const ecc_publickey* pubKey);

/**
 * Encode Bigval to big-endian byte array
 * @param src    The input bigval
 * @param tgt    The output buffer
 * @param tgtlen The output buffer length
 */
void AJ_BigvalEncode(const bigval_t* src, uint8_t* tgt, size_t tgtlen);

/**
 * Decode Bigval from big-endian byte array
 * @param src    The input buffer
 * @param tgt    The output bigval
 * @param srclen The input buffer length
 */
void AJ_BigvalDecode(const uint8_t* src, bigval_t* tgt, size_t srclen);

#ifdef __cplusplus
}
#endif
/**
 * @}
 */
#endif
