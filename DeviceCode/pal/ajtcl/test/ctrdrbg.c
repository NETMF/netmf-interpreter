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
#define AJ_MODULE CTRDRBG

#include "aj_target.h"

#include "alljoyn.h"
#include "aj_crypto.h"
#include "aj_debug.h"

uint8_t dbgCTRDRBG = 0;

typedef struct {
    uint8_t df;            /* Use DF or not */
    const char* entropy;   /* Entropy input */
    const char* nonce;     /* Nonce */
    const char* personal;  /* Personalization string */
    const char* reseed;    /* Reseed input */
    const char* rand;      /* Output */
} TEST_CASE;

/*
 * Known answer tests taken from
 * http://csrc.nist.gov/groups/STM/cavp/
 * http://csrc.nist.gov/groups/STM/cavp/documents/drbg/drbgtestvectors.zip
 * AES-128 use df, no predication resisitance, no reseed
 */
static TEST_CASE const testVector[] = {
    // DF - no reseed
    {
        1,
        "890eb067acf7382eff80b0c73bc872c6",
        "aad471ef3ef1d203",
        "",
        "",
        "a5514ed7095f64f3d0d3a5760394ab42062f373a25072a6ea6bcfd8489e94af6cf18659fea22ed1ca0a9e33f718b115ee536b12809c31b72b08ddd8be1910fa3",
    },
    {
        1,
        "c47be8e8219a5a87c94064a512089f2b",
        "f2a23e636aee75c6",
        "",
        "",
        "5a1650bb6d6a16f6040591d56abcd5dd3db8772a9c75c44d9fc64d51b733d4a6759bd5a64ec4231a24e662fdd47c82db63b200daf8d098560eb5ba7bf3f9abf7",
    },
    // DF - reseed
    {
        1,
        "0f65da13dca407999d4773c2b4a11d85",
        "5209e5b4ed82a234",
        "",
        "1dea0a12c52bf64339dd291c80d8ca89",
        "2859cc468a76b08661ffd23b28547ffd0997ad526a0f51261b99ed3a37bd407bf418dbe6c6c3e26ed0ddefcb7474d899bd99f3655427519fc5b4057bcaf306d4",
    },
    {
        1,
        "1ff8f4a85dbf2f6bb2648967419bb270",
        "b0cdf7bc47ca5f8b",
        "",
        "f90699441c1ece41cf1f6a32e4948656",
        "d9ae8b33f1a10cbf516d97b9ad7baf0d596a081a0ff0f4717674239b9e339354d813b2bb71c10f7d2e34994e0030e4fbfba6438d077c361745993b9d6f669b24",
    },
    // no DF - reseed
    {
        0,
        "ed1e7f21ef66ea5d8e2a85b9337245445b71d6393a4eecb0e63c193d0f72f9a9",
        "",
        "",
        "303fb519f0a4e17d6df0b6426aa0ecb2a36079bd48be47ad2a8dbfe48da3efad",
        "f80111d08e874672f32f42997133a5210f7a9375e22cea70587f9cfafebe0f6a6aa2eb68e7dd9164536d53fa020fcab20f54caddfab7d6d91e5ffec1dfd8deaa",
    },
    {
        0,
        "eab5a9f23ceac9e4195e185c8cea549d6d97d03276225a7452763c396a7f70bf",
        "",
        "",
        "4258765c65a03af92fc5816f966f1a6644a6134633aad2d5d19bd192e4c1196a",
        "2915c9fabfbf7c62d68d83b4e65a239885e809ceac97eb8ef4b64df59881c277d3a15e0e15b01d167c49038fad2f54785ea714366d17bb2f8239fd217d7e1cba",
    },
};

int AJ_Main(void)
{
    AJ_Status status = AJ_OK;
    CTR_DRBG_CTX ctx;
    size_t i;
    size_t size;
    uint8_t* d;
    uint8_t* data;
    uint8_t* rand;
    char* hex;

    for (i = 0; i < ArraySize(testVector); i++) {
        size_t elen = strlen(testVector[i].entropy) / 2;
        size_t nlen = strlen(testVector[i].nonce) / 2;
        size_t plen = strlen(testVector[i].personal) / 2;
        size_t rlen = strlen(testVector[i].reseed) / 2;

        size = elen + nlen + plen;
        data = AJ_Malloc(size);
        AJ_ASSERT(data);
        d = data;
        AJ_HexToRaw(testVector[i].entropy, 2 * elen, d, size);
        d += elen;
        AJ_HexToRaw(testVector[i].nonce, 2 * nlen, d, size);
        d += nlen;
        AJ_HexToRaw(testVector[i].personal, 2 * plen, d, size);
        d += plen;

        AJ_DumpBytes("SEED", data, size);
        AES_CTR_DRBG_Instantiate(&ctx, data, size, testVector[i].df);
        AJ_Free(data);

        if (rlen) {
            size = rlen;
            data = AJ_Malloc(size);
            AJ_ASSERT(data);
            AJ_HexToRaw(testVector[i].reseed, 2 * rlen, data, size);
            AES_CTR_DRBG_Reseed(&ctx, data, size);
            AJ_Free(data);
        }

        size = strlen(testVector[i].rand) / 2;
        data = AJ_Malloc(size);
        rand = AJ_Malloc(size);
        AJ_ASSERT(data);
        AJ_ASSERT(rand);
        AJ_HexToRaw(testVector[i].rand, 2 * size, rand, size);

        status = AES_CTR_DRBG_Generate(&ctx, data, size);
        if (AJ_OK != status) {
            AJ_AlwaysPrintf(("Generate failed for test #%zu\n", i));
            goto Exit;
        }
        status = AES_CTR_DRBG_Generate(&ctx, data, size);
        if (AJ_OK != status) {
            AJ_AlwaysPrintf(("Generate failed for test #%zu\n", i));
            goto Exit;
        }
        if (0 != memcmp(data, rand, size)) {
            AJ_AlwaysPrintf(("Expected failed for test #%zu\n", i));
            status = AJ_ERR_SECURITY;
            goto Exit;
        }

        AJ_Free(data);
        AJ_Free(rand);
    }

    // Initialize the core context
    AJ_RandBytes(NULL, 0);

    // Get some random data
    size = 64;
    rand = AJ_Malloc(size);
    AJ_ASSERT(rand);
    hex = AJ_Malloc(2 * size + 1);
    AJ_ASSERT(hex);
    for (i = 0; i < 64; i++) {
        AJ_RandBytes(rand, size);
        AJ_RawToHex(rand, size, hex, 2 * size + 1, 0);
        AJ_AlwaysPrintf(("%s\n", hex));
    }
    AJ_Free(rand);
    AJ_Free(hex);

Exit:
    AJ_AlwaysPrintf(("CTR DRBG test: %s\n", AJ_StatusText(status)));

    return 0;
}

#ifdef AJ_MAIN
int main()
{
    return AJ_Main();
}
#endif
