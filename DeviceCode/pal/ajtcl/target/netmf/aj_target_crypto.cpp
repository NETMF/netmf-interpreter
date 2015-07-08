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
#define AJ_MODULE TARGET_CRYPTO

#include "aj_target.h"
#include "aj_crypto.h"
#include "aj_debug.h"
#include <openssl/aes.h>
#include <openssl/bn.h>

extern uint32_t Random_Generate();
extern void Random_Start();
extern void Random_Stop();


/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgTARGET_CRYPTO = 0;
#endif

/*
 * Context for AES-128 CTR DRBG
 */
static CTR_DRBG_CTX drbgctx;

static AES_KEY keyState;

// we are using the ajtcl\crypto instead of using the SSL in our branch. less codes.
// but we can uncomment the below for using the SSL AES
#if 0
void AJ_AES_Enable(const uint8_t* key)
{
    AES_set_encrypt_key(key, 16 * 8, &keyState);
}

void AJ_AES_Disable(void)
{
}

void AJ_AES_CTR_128(const uint8_t* key, const uint8_t* in, uint8_t* out, uint32_t len, uint8_t* ctr)
{
    /*
       Counter mode the hard way because the SSL CTR-mode API is just wierd.
     */
    while (len) {
        size_t n = min(len, 16);
        uint8_t enc[16];
        uint8_t* p = enc;
        uint16_t counter = (ctr[14] << 8) | ctr[15];
        len -= n;
        AES_encrypt(ctr, enc, &keyState);
        while (n--) {
            *out++ = *p++ ^ *in++;
        }
        ++counter;
        ctr[15] = counter;
        ctr[14] = counter >> 8;
    }
}

void AJ_AES_CBC_128_ENCRYPT(const uint8_t* key, const uint8_t* in, uint8_t* out, uint32_t len, uint8_t* iv)
{
    AES_cbc_encrypt(in, out, len, &keyState, iv, AES_ENCRYPT);
}

void AJ_AES_ECB_128_ENCRYPT(const uint8_t* key, const uint8_t* in, uint8_t* out)
{
    AES_encrypt(in, out, &keyState);
}
#endif
uint32_t AJ_PlatformEntropy(uint8_t* data, uint32_t size)
{


#if BUILD_SSL_CRYPTO
    BIGNUM* bn = BN_new();
    BN_rand(bn, size * 8, -1, 0);
    BN_bn2bin(bn, data);
    BN_free(bn);
    
#else

#if 0
    static uint8_t seed = 0;
    for (int i = 0; i < size; i++)
    {
        *data++ = seed++ ^ 0xaa;
    }

#else
    Random_Start();

    while (size) {
        *data = Random_Generate() & 0xFF;
        size -= 1;
        data += 1;
    }

    Random_Stop();
#endif
    
#endif
    return size;
}

void AJ_RandBytes(uint8_t* rand, uint32_t size)
{

    AJ_Status status = AJ_ERR_SECURITY;
    uint8_t seed[SEEDLEN];

    if (rand && size) {
        status = AES_CTR_DRBG_Generate(&drbgctx, rand, size);
        if (AJ_OK != status) {
            // Reseed required
            AJ_PlatformEntropy(seed, sizeof (seed));
            AES_CTR_DRBG_Reseed(&drbgctx, seed, sizeof (seed));
            status = AES_CTR_DRBG_Generate(&drbgctx, rand, size);
        }
    } else {
        // This is the first call to initialize
        size = AJ_PlatformEntropy(seed, sizeof (seed));
        drbgctx.df = (SEEDLEN == size) ? 0 : 1;
        AES_CTR_DRBG_Instantiate(&drbgctx, seed, sizeof (seed), drbgctx.df);
    }
}


