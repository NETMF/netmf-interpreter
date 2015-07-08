/**
 * @file aj_crypto_sha2.c
 *
 * Class for SHA-256
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

#include <aj_crypto.h>

#include "aj_crypto_sha2.h"
#include "aj_util.h"


/**
 * Initialize the hash context
 * @param context the hash context
 * @return AJ_OK if successful
 */
void AJ_SHA256_Init(AJ_SHA256_Context* context) {
    SHA256_Init(context);
}

/**
 * Update the digest using the specific bytes
 * @param context the hash context
 * @param buf the bytes to digest
 * @param bufSize the number of bytes to digest
 */
void AJ_SHA256_Update(AJ_SHA256_Context* context, const uint8_t* buf, size_t bufSize) {
    SHA256_Update(context, buf, bufSize);
}

/**
 * Retrieve the digest
 * @param context the hash context
 * @param digest the buffer to hold the digest.  Must be of size SHA256_DIGEST_LENGTH
 * @param keepAlive keep the digest process alive for continuing digest
 */

void AJ_SHA256_GetDigest(AJ_SHA256_Context* context, uint8_t* digest,
                         const uint8_t keepAlive) {
    AJ_SHA256_Context savedCtx;

    if (keepAlive != 0) {
        memcpy(&savedCtx, context, sizeof(AJ_SHA256_Context));
    }
    SHA256_Final(digest, context);
    if (keepAlive != 0) {
        memcpy(context, &savedCtx, sizeof(AJ_SHA256_Context));
    }
}

/**
 * Retrieve the final digest
 * @param context the hash context
 * @param digest the buffer to hold the digest.  Must be of size SHA256_DIGEST_LENGTH
 */
void AJ_SHA256_Final(AJ_SHA256_Context* context, uint8_t* digest) {
    AJ_SHA256_GetDigest(context, digest, 0);
}

AJ_Status AJ_HMAC_SHA256_Init(AJ_HMAC_SHA256_CTX* ctx, const uint8_t* key, size_t keyLen)
{
    int cnt;
    memset(ctx->ipad, 0, HMAC_SHA256_BLOCK_LENGTH);
    memset(ctx->opad, 0, HMAC_SHA256_BLOCK_LENGTH);
    /* if keyLen > 64, hash it and use it as key */
    if (keyLen > HMAC_SHA256_BLOCK_LENGTH) {
        uint8_t digest[SHA256_DIGEST_LENGTH];
        AJ_SHA256_Init(&ctx->hashCtx);
        AJ_SHA256_Update(&ctx->hashCtx, key, keyLen);
        AJ_SHA256_Final(&ctx->hashCtx, digest);
        keyLen = SHA256_DIGEST_LENGTH;
        memcpy(ctx->ipad, digest, SHA256_DIGEST_LENGTH);
        memcpy(ctx->opad, digest, SHA256_DIGEST_LENGTH);
    } else {
        memcpy(ctx->ipad, key, keyLen);
        memcpy(ctx->opad, key, keyLen);
    }
    /*
     * the HMAC_SHA256 process
     *
     * SHA256(K XOR opad, SHA256(K XOR ipad, msg))
     *
     * K is the key
     * ipad is filled with 0x36
     * opad is filled with 0x5c
     * msg is the message
     */

    /*
     * prepare inner hash SHA256(K XOR ipad, msg)
     * K XOR ipad
     */
    for (cnt = 0; cnt < HMAC_SHA256_BLOCK_LENGTH; cnt++) {
        ctx->ipad[cnt] ^= 0x36;
    }

    AJ_SHA256_Init(&ctx->hashCtx);
    AJ_SHA256_Update(&ctx->hashCtx, ctx->ipad, HMAC_SHA256_BLOCK_LENGTH);
    return AJ_OK;
}

AJ_Status AJ_HMAC_SHA256_Update(AJ_HMAC_SHA256_CTX* ctx, const uint8_t* data, size_t dataLen)
{
    AJ_SHA256_Update(&ctx->hashCtx, data, dataLen);
    return AJ_OK;
}

AJ_Status AJ_HMAC_SHA256_Final(AJ_HMAC_SHA256_CTX* ctx, uint8_t* digest)
{
    int cnt;
    /* complete inner hash SHA256(K XOR ipad, msg) */
    AJ_SHA256_Final(&ctx->hashCtx, digest);

    /*
     * perform outer hash SHA256(K XOR opad, SHA256(K XOR ipad, msg))
     */
    for (cnt = 0; cnt < HMAC_SHA256_BLOCK_LENGTH; cnt++) {
        ctx->opad[cnt] ^= 0x5c;
    }
    AJ_SHA256_Init(&ctx->hashCtx);
    AJ_SHA256_Update(&ctx->hashCtx, ctx->opad, HMAC_SHA256_BLOCK_LENGTH);
    AJ_SHA256_Update(&ctx->hashCtx, digest, SHA256_DIGEST_LENGTH);
    AJ_SHA256_Final(&ctx->hashCtx, digest);
    return AJ_OK;
}

AJ_Status AJ_Crypto_PRF_SHA256(const uint8_t** inputs, const uint8_t* lengths,
                               uint32_t count, uint8_t* out, uint32_t outLen)
{
    uint32_t cnt;
    AJ_HMAC_SHA256_CTX msgHash;
    uint8_t digest[SHA256_DIGEST_LENGTH];
    uint32_t len = 0;

    if (count < 2) {
        return AJ_ERR_INVALID;
    }
    while (outLen) {
        /*
         * Initialize SHA256 in HMAC mode with the secret
         */
        AJ_HMAC_SHA256_Init(&msgHash, inputs[0], lengths[0]);
        /*
         * If this is not the first iteration hash in the digest from the previous iteration.
         */
        if (len) {
            AJ_HMAC_SHA256_Update(&msgHash, digest, sizeof(digest));
        }
        for (cnt = 1; cnt < count; cnt++) {
            AJ_HMAC_SHA256_Update(&msgHash, inputs[cnt], lengths[cnt]);
        }
        AJ_HMAC_SHA256_Final(&msgHash, digest);
        if (outLen < sizeof(digest)) {
            len = outLen;
        } else {
            len = sizeof(digest);
        }
        memcpy(out, digest, len);
        outLen -= len;
        out += len;
    }

    return AJ_OK;
}
