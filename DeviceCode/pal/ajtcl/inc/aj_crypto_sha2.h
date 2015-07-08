#ifndef _AJ_CRYPTO_SHA2_H
#define _AJ_CRYPTO_SHA2_H

/**
 * @file aj_crypto_sha2.h
 * @defgroup aj_crypto SHA-256 Cryptographic Support
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

#include "sha2.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef SHA256_CTX AJ_SHA256_Context;

#define HMAC_SHA256_DIGEST_LENGTH SHA256_DIGEST_LENGTH
#define HMAC_SHA256_BLOCK_LENGTH  64

typedef struct _AJ_HMAC_SHA256_CTX {
    uint8_t ipad[HMAC_SHA256_BLOCK_LENGTH];
    uint8_t opad[HMAC_SHA256_BLOCK_LENGTH];
    AJ_SHA256_Context hashCtx;
} AJ_HMAC_SHA256_CTX;


/*** SHA-256/384/512 Function Prototypes ******************************/
#ifndef NOPROTO

/**
 * Initialize the hash context
 * @param context the hash context
 */
void AJ_SHA256_Init(AJ_SHA256_Context* context);

/**
 * Update the digest using the specific bytes
 * @param context the hash context
 * @param buf the bytes to digest
 * @param bufSize the number of bytes to digest
 */
void AJ_SHA256_Update(AJ_SHA256_Context* context, const uint8_t* buf, size_t bufSize);

/**
 * Retrieve the digest
 * @param context the hash context
 * @param digest the buffer to hold the digest.  Must be of size SHA256_DIGEST_LENGTH
 * @param keepAlive keep the digest process alive for continuing digest
 */
void AJ_SHA256_GetDigest(AJ_SHA256_Context* context, uint8_t* digest, const uint8_t keepAlive);

/**
 * Retrieve the final digest
 * @param context the hash context
 * @param digest the buffer to hold the digest.  Must be of size SHA256_DIGEST_LENGTH
 */
void AJ_SHA256_Final(AJ_SHA256_Context* context, uint8_t* digest);

/**
 * Initialize the HMAC context
 * @param ctx the HMAC context
 * @param key the key
 * @param keyLen the length of the key
 * @return
 *  - AJ_OK if successful
 *  - AJ_ERR_INVALID if the length is negative
 */
AJ_Status AJ_HMAC_SHA256_Init(AJ_HMAC_SHA256_CTX* ctx, const uint8_t* key, size_t keyLen);

/**
 * Update the hash with data
 * @param ctx the HMAC context
 * @param data the data
 * @param dataLen the length of the data
 * @return
 *  - AJ_OK if successful
 *  - AJ_ERR_INVALID if the length is negative
 */
AJ_Status AJ_HMAC_SHA256_Update(AJ_HMAC_SHA256_CTX* ctx, const uint8_t* data, size_t dataLen);

/**
 * Retrieve the final digest for the HMAC
 * @param ctx the HMAC context
 * @param digest the buffer to hold the digest.  Must be of size SHA256_DIGEST_LENGTH
 */
AJ_Status AJ_HMAC_SHA256_Final(AJ_HMAC_SHA256_CTX* ctx, uint8_t* digest);

/**
 * Random function
 * @param inputs    array holding secret, label, seed
 * @param lengths   array holding the lengths of the inputs
 * @param count     the size of the input array
 * @param out       the buffer holding the random value
 * @param outLen    the buffer size
 * @return  AJ_OK if succeeds; otherwise error
 */
AJ_Status AJ_Crypto_PRF_SHA256(const uint8_t** inputs, const uint8_t* lengths,
                               uint32_t count, uint8_t* out, uint32_t outLen);

#else /* NOPROTO */

/**
 * Initialize the hash context
 * @param context the hash context
 * @return AJ_OK if successful
 */
void AJ_SHA256_Init();

/**
 * Update the digest using the specific bytes
 * @param context the hash context
 * @param buf the bytes to digest
 * @param bufSize the number of bytes to digest
 * @return AJ_OK if successful
 */
void AJ_SHA256_Update();

/**
 * Update the digest using the specific bytes
 * @param context the hash context
 * @param digest the buffer to hold the digest.  Must be of size SHA256_DIGEST_LENGTH
 * @return AJ_OK if successful
 */
void AJ_SHA256_GetDigest();

#endif /* NOPROTO */

#ifdef __cplusplus
}
#endif
/**
 * @}
 */
#endif
