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
#define AJ_MODULE CRYPTO

#include "aj_target.h"
#include "aj_util.h"
#include "aj_crypto.h"
#include "aj_debug.h"
#include "aj_config.h"
/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgCRYPTO = 0;
#endif
/*
 * AES-128 processes data 16 bytes at a time
 */
#define AJ_BLOCKSZ 16

#if AJ_CCM_TRACE
#define Trace(tag, data, len) AJ_DumpBytes(tag, data, len)
#else
#define Trace(tag, data, len)
#endif

/*
 * Struct for a single AES data block
 */
typedef struct _AES_Block {
    uint8_t data[AJ_BLOCKSZ];
} AES_Block;

#define ZERO(b)  memset((b).data, 0, AJ_BLOCKSZ);

/*
 * Struct holding CCM state information
 */
typedef struct _CCM_Context {
    AES_Block T;      /* authentication tag */
    AES_Block ivec0;  /* ivec for CBC MAC */
    AES_Block ivec;   /* ivec for CTR mode encrypt/decrypt */
    union {
        AES_Block A;   /* Working data for CBC MAC */
        AES_Block B_0; /* Initial block for CBC MAC */
    };
} CCM_Context;

/**
 * Compute the CBC MAC over some data
 */
static void CBC_MAC(const uint8_t* key, const uint8_t* in, uint32_t len, CCM_Context* context)
{
    while (len >= AJ_BLOCKSZ) {
        AJ_AES_CBC_128_ENCRYPT(key, in, context->T.data, AJ_BLOCKSZ, context->ivec0.data);
        Trace("After AES", context->T.data, AJ_BLOCKSZ);
        in += AJ_BLOCKSZ;
        len -= AJ_BLOCKSZ;
    }
    if (len) {
        ZERO(context->A);
        memcpy(context->A.data, in, len);
        AJ_AES_CBC_128_ENCRYPT(key, context->A.data, context->T.data, AJ_BLOCKSZ, context->ivec0.data);
        Trace("After AES", context->T.data, AJ_BLOCKSZ);
    }
}

/**
 * Compute the AES-CCM authentication tag.
 */
static void Compute_CCM_AuthTag(const uint8_t* key,
                                CCM_Context* context,
                                const uint8_t* msg,
                                uint32_t mLen,
                                uint32_t hdrLen)
{
    /*
     * Initialize CBC-MAC with B_0 initialization vector is 0.
     */
    Trace("CBC IV in", context->B_0.data, AJ_BLOCKSZ);
    AJ_AES_CBC_128_ENCRYPT(key, context->B_0.data, context->T.data, AJ_BLOCKSZ, context->ivec0.data);
    Trace("CBC IV out", context->T.data, AJ_BLOCKSZ);
    /*
     * Compute CBC-MAC for the add data.
     */
    if (hdrLen) {
        uint32_t firstFew;
        /*
         * This encodes the header data length and the first few bytes of the header data
         */
        ZERO(context->A);
        context->A.data[0] = (uint8_t)(hdrLen >> 8);
        context->A.data[1] = (uint8_t)(hdrLen >> 0);
        firstFew = min(hdrLen, 14);
        memcpy(&context->A.data[2], msg, firstFew);
        /*
         * Adjust for the hdr data bytes that were encoded in the length block
         */
        msg += firstFew;
        hdrLen -= firstFew;
        /*
         * Continue the MAC by encrypting the length block
         */
        Trace("Before AES", context->A.data, AJ_BLOCKSZ);
        AJ_AES_CBC_128_ENCRYPT(key, context->A.data, context->T.data, AJ_BLOCKSZ, context->ivec0.data);
        Trace("After AES", context->T.data, AJ_BLOCKSZ);
        /*
         * Continue computing the CBC-MAC
         */
        CBC_MAC(key, msg, hdrLen, context);
        msg += hdrLen;
    }
    /*
     * Continue computing CBC-MAC over the message data.
     */
    if (mLen) {
        CBC_MAC(key, msg, mLen, context);
    }
    Trace("CBC-MAC", context->T.data, context->M);
}

static CCM_Context* InitCCMContext(const uint8_t* nonce, uint32_t nLen, uint32_t hdrLen, uint32_t msgLen, uint8_t M)
{
    int i;
    int l;
    uint8_t L  = 15 - max(nLen, 11);
    uint8_t flags = ((hdrLen) ? 0x40 : 0) | (((M - 2) / 2) << 3) | (L - 1);
    CCM_Context* context;

    AJ_ASSERT(nLen <= 15);

    context = (CCM_Context*)AJ_Malloc(sizeof(CCM_Context));
    if (context) {
        memset(context, 0, sizeof(CCM_Context));
        /*
         * Set ivec and other initial args.
         */
        context->ivec.data[0] = L - 1;
        memcpy(&context->ivec.data[1], nonce, nLen);
        /*
         * Compute the B_0 block. This encodes the flags, the nonce, and the message length.
         */
        context->B_0.data[0] = flags;
        memcpy(&context->B_0.data[1], nonce, nLen);
        for (i = 15, l = msgLen - hdrLen; l != 0; i--) {
            context->B_0.data[i] = (uint8_t)l;
            l >>= 8;
        }
    }
    return context;
}

/*
 * Implements AES-CCM (Counter with CBC-MAC) encryption as described in RFC 3610
 */
AJ_Status AJ_Encrypt_CCM(const uint8_t* key,
                         uint8_t* msg,
                         uint32_t msgLen,
                         uint32_t hdrLen,
                         uint8_t tagLen,
                         const uint8_t* nonce,
                         uint32_t nLen)
{
    AJ_Status status = AJ_OK;
    CCM_Context* context;

    if (!(context = InitCCMContext(nonce, nLen, hdrLen, msgLen, tagLen))) {
        AJ_ErrPrintf(("AJ_Encrypt_CCM(): AJ_ERR_RESOURCES\n"));
        return AJ_ERR_RESOURCES;
    }
    /*
     * Do any platform specific operations to enable AES
     */
    AJ_AES_Enable(key);
    /*
     * Compute the authentication tag
     */
    Compute_CCM_AuthTag(key, context, msg, msgLen - hdrLen, hdrLen);
    /*
     * Encrypt the authentication tag
     */
    AJ_AES_CTR_128(key, context->T.data, msg + msgLen, tagLen, context->ivec.data);
    Trace("CTR Start", context->ivec.data, AJ_BLOCKSZ);
    /*
     * Encrypt the message
     */
    if (msgLen != hdrLen) {
        AJ_AES_CTR_128(key, msg + hdrLen, msg + hdrLen, msgLen - hdrLen, context->ivec.data);
    }
    /*
     * Balance the enable call above
     */
    AJ_AES_Disable();
    /*
     * Done with the context
     */
    AJ_Free(context);
    return status;
}

/*
 * Implements AES-CCM (Counter with CBC-MAC) decryption as described in RFC 3610
 */
AJ_Status AJ_Decrypt_CCM(const uint8_t* key,
                         uint8_t* msg,
                         uint32_t msgLen,
                         uint32_t hdrLen,
                         uint8_t tagLen,
                         const uint8_t* nonce,
                         uint32_t nLen)
{
    AJ_Status status = AJ_OK;
    CCM_Context* context;

    if (!(context = InitCCMContext(nonce, nLen, hdrLen, msgLen, tagLen))) {
        AJ_ErrPrintf(("AJ_Decrypt_CCM(): AJ_ERR_RESOURCES\n"));
        return AJ_ERR_RESOURCES;
    }
    /*
     * Do any platform specific operations to enable AES
     */
    AJ_AES_Enable(key);
    /*
     * Decrypt the authentication field
     */
    AJ_AES_CTR_128(key, msg + msgLen, msg + msgLen, tagLen, context->ivec.data);
    /*
     * Decrypt message.
     */
    if (msgLen != hdrLen) {
        AJ_AES_CTR_128(key, msg + hdrLen, msg + hdrLen, msgLen - hdrLen, context->ivec.data);
    }
    /*
     * Compute and verify the authentication tag T.
     */
    Compute_CCM_AuthTag(key, context, msg, msgLen - hdrLen, hdrLen);
    /*
     * Balance the enable call above
     */
    AJ_AES_Disable();
    if (memcmp(context->T.data, msg + msgLen, tagLen) != 0) {
        /*
         * Authentication failed Clear the decrypted data
         */
        memset(msg, 0, msgLen + tagLen);
        AJ_ErrPrintf(("AJ_Decrypt_CCM(): AJ_ERR_SECURITY\n"));
        status = AJ_ERR_SECURITY;
    }
    /*
     * Done with the context
     */
    AJ_Free(context);
    return status;
}

AJ_Status AJ_Crypto_PRF(const uint8_t** inputs,
                        const uint8_t* lengths,
                        uint32_t count,
                        uint8_t* out,
                        uint32_t outLen)
{
    AJ_Status status = AJ_OK;
    uint8_t nonce[4];
    uint32_t inLen = 0;
    uint8_t* inBuf;
    uint8_t* key;
    uint8_t* p;
    uint32_t i;

    for (i = 0; i < count; ++i) {
        inLen += lengths[i];
    }
    if (inLen <= 32) {
        AJ_ErrPrintf(("AJ_Crypto_PRF(): AJ_ERR_INVALID\n"));
        return AJ_ERR_INVALID;
    }
    /*
     * Need 16 bytes at the end for the CCM-MAC
     */
    inBuf = (uint8_t*)AJ_Malloc(inLen + 16);
    if (!inBuf) {
        AJ_ErrPrintf(("AJ_Crypto_PRF(): AJ_ERR_RESOURCES\n"));
        return AJ_ERR_RESOURCES;
    }
    /*
     * Concatenate the inputs
     */
    for (i = 0, p = inBuf; i < count; ++i) {
        memcpy(p, inputs[i], lengths[i]);
        p += lengths[i];
    }
    /*
     * Clear the nonce (it's declared as an array of bytes because of endianess)
     */
    *((uint32_t*)nonce) = 0;
    /*
     * The key first 16 bytes of the input is used as the AES key.
     */
    key = inBuf;
    inLen -= 16;
    inBuf += 16;
    while (outLen) {
        uint32_t len =  min(16, outLen);
        status = AJ_Encrypt_CCM(key, inBuf, inLen, inLen, 16, nonce, sizeof(nonce));
        if (status != AJ_OK) {
            break;
        }
        /*
         * Append CCM-MAC to the output buffer
         */
        memcpy(out, inBuf + inLen, len);
        outLen -= len;
        out += len;
        ++nonce[0];
    }
    inBuf -= 16;
    AJ_Free(inBuf);
    return status;
}

AJ_Status AJ_RandHex(char* rand, uint32_t bufLen, uint32_t len)
{
    AJ_RandBytes((uint8_t*)rand, len);
    return AJ_RawToHex((const uint8_t*) rand, len, rand, bufLen, FALSE);
}

/*
 * CTR DRBG is implemented using algorithms described in the
 * NIST SP 800-90A standard, which can be found at
 * http://csrc.nist.gov/publications/nistpubs/800-90A/SP800-90A.pdf
 */

static void AES_CTR_DRBG_Increment(uint8_t* data, size_t size)
{
    while (size--) {
        data[size]++;
        if (data[size]) {
            break;
        }
    }
}

static void AES_CTR_DRBG_Update(CTR_DRBG_CTX* ctx, uint8_t data[SEEDLEN])
{
    size_t i = 0;
    uint8_t tmp[SEEDLEN];
    uint8_t* t = tmp;

    AJ_AES_Enable(ctx->k);
    for (i = 0; i < SEEDLEN; i += OUTLEN) {
        AES_CTR_DRBG_Increment(ctx->v, OUTLEN);
        AJ_AES_ECB_128_ENCRYPT(ctx->k, ctx->v, t);
        t += OUTLEN;
    }

    for (i = 0; i < SEEDLEN; i++) {
        tmp[i] ^= data[i];
    }

    memcpy(ctx->k, tmp, KEYLEN);
    memcpy(ctx->v, tmp + KEYLEN, OUTLEN);
}

static void AES_CTR_DRBG_BCC(uint8_t* k, uint8_t* data, size_t size, uint8_t* out)
{
    size_t i;
    size_t j;

    AJ_ASSERT(0 == (size % OUTLEN));
    memset(out, 0, OUTLEN);

    AJ_AES_Enable(k);
    for (i = 0; i < size; i += OUTLEN) {
        for (j = 0; j < OUTLEN; j++) {
            out[j] ^= data[j];
        }
        AJ_AES_ECB_128_ENCRYPT(k, out, out);
        data += OUTLEN;
    }
}

static void AES_CTR_DRBG_DF(uint8_t* seed, size_t size, uint8_t data[SEEDLEN])
{
    // Variable names reflect NIST SP 800-90A
    uint32_t i = 0;
    uint32_t L = size;
    uint32_t N = SEEDLEN;
    uint32_t n = OUTLEN + sizeof (L) + sizeof (N) + size + sizeof (0x80);
    uint8_t* S;
    uint8_t* s;
    uint8_t k[KEYLEN];
    uint8_t K[KEYLEN];
    uint8_t X[KEYLEN];

    n += (OUTLEN - (n % OUTLEN));
    AJ_ASSERT(0 == (n % OUTLEN));
    S = (uint8_t*)AJ_Malloc(n);
    if (NULL == S) {
        // Errors are not propagated up
        return;
    }

    memset(S, 0, n);
    s = S + OUTLEN;
    *s++ = (L >> 24) & 0xFF;
    *s++ = (L >> 16) & 0xFF;
    *s++ = (L >>  8) & 0xFF;
    *s++ = (L >>  0) & 0xFF;
    *s++ = (N >> 24) & 0xFF;
    *s++ = (N >> 16) & 0xFF;
    *s++ = (N >>  8) & 0xFF;
    *s++ = (N >>  0) & 0xFF;
    memcpy(s, seed, size);
    s += size;
    *s++ = 0x80;

    for (i = 0; i < KEYLEN; i++) {
        k[i] = i;
    }

    AES_CTR_DRBG_BCC(k, S, n, K);
    AES_CTR_DRBG_Increment(S, 4);
    AES_CTR_DRBG_BCC(k, S, n, X);

    AJ_AES_Enable(K);
    AJ_AES_ECB_128_ENCRYPT(K, X, X);
    memcpy(data, X, OUTLEN);
    data += OUTLEN;
    AJ_AES_ECB_128_ENCRYPT(K, X, X);
    memcpy(data, X, OUTLEN);

    AJ_Free(S);
}

void AES_CTR_DRBG_Reseed(CTR_DRBG_CTX* ctx, uint8_t* seed, size_t size)
{
    uint8_t data[SEEDLEN];
    if (ctx->df) {
        AES_CTR_DRBG_DF(seed, size, data);
        AES_CTR_DRBG_Update(ctx, data);
    } else {
        AJ_ASSERT(SEEDLEN == size);
        AES_CTR_DRBG_Update(ctx, seed);
    }
    ctx->c = 1;
}

void AES_CTR_DRBG_Instantiate(CTR_DRBG_CTX* ctx, uint8_t* seed, size_t size, uint8_t df)
{
    memset(ctx->k, 0, KEYLEN);
    memset(ctx->v, 0, OUTLEN);
    ctx->df = df;
    AES_CTR_DRBG_Reseed(ctx, seed, size);
}

AJ_Status AES_CTR_DRBG_Generate(CTR_DRBG_CTX* ctx, uint8_t* rand, size_t size)
{
    uint8_t data[SEEDLEN];
    size_t copy;

    // Reseed interval 2^32 (counter wraps to zero)
    if (0 == ctx->c) {
        return AJ_ERR_SECURITY;
    }
    AJ_AES_Enable(ctx->k);
    while (size) {
        AES_CTR_DRBG_Increment(ctx->v, OUTLEN);
        AJ_AES_ECB_128_ENCRYPT(ctx->k, ctx->v, data);
        copy = (size < OUTLEN) ? size : OUTLEN;
        memcpy(rand, data, copy);
        rand += copy;
        size -= copy;
    }
    memset(data, 0, SEEDLEN);
    AES_CTR_DRBG_Update(ctx, data);
    ctx->c++;

    return AJ_OK;
}
