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

#include "Arduino.h"    // for digitalRead, digitalWrite, etc

#include "aj_target.h"
#include "aj_crypto.h"

/*
 * Context for AES-128 CTR DRBG
 */
static CTR_DRBG_CTX drbgctx;

int analogPin = 3;
/*
 * The host has various ADC's. We are going to accumulate entropy by repeatedly
 * reading the ADC and accumulating the least significant bit or each reading.
 */
uint32_t AJ_PlatformEntropy(uint8_t* data, uint32_t size)
{
    int i;
    uint32_t val;

    /*
     * Start accumulating entropy one bit at a time
     */
    for (i = 0; i < (8 * size); ++i) {
        val = analogRead(analogPin);
        data[i / 8] ^= ((val & 1) << (i & 7));
    }
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
