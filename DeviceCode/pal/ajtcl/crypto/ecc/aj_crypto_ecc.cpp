/**
 * @file aj_crypto_ecc.cc
 *
 * Class for Elliptic Curve Cryptography
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
#include "aj_crypto_ecc.h"
#include "aj_crypto_sha2.h"


/* P256 is tested directly with known answer tests from example in
   ANSI X9.62 Annex L.4.2.  (See item in pt_mpy_testcases below.)
   Mathematica code, written in a non-curve-specific way, was also
   tested on the ANSI example, then used to generate both P192 and
   P256 test cases. */

/*
 * This file exports the functions ECDH_generate, ECDH_derive, and
 * optionally, ECDSA_sign and ECDSA_verify.  It depends on a function
 * get_random_bytes, which is expected to be of cryptographic quality.
 */

/*
 * References:
 *
 * [KnuthV2] is D.E. Knuth, The Art of Computer Programming, Volume 2:
 * Seminumerical Algorithms, 1969.
 *
 * [HMV] is D. Hankerson, A. Menezes, and S. Vanstone, Guide to
 * Elliptic Curve Cryptography, 2004.
 *
 * [Wallace] is C.S. Wallace, "A suggestion for a Fast Multiplier",
 * IEEE Transactions on Electronic Computers, EC-13 no. 1, pp 14-17,
 * 1964.
 *
 * [ANSIX9.62] is ANSI X9.62-2005, "Public Key Cryptography for the Financial
 * Services Industry The Elliptic Curve Digital Signature Algorithm
 * (ECDSA)".
 */

/*
 * The vast majority of cycles in programs like this are spent in
 * modular multiplication.  The usual approach is Montgomery
 * multiplication, which effectively does two multiplications in place
 * of one multiplication and one reduction. However, this program is
 * dedicated to the NIST standard curves P256 and P192.  Most of the
 * NIST curves have the property that they can be expressed as a_i *
 * 2^(32*i), where a_i is -1, 0, or +1.  For example P192 is 2^(6*32)
 * - 2^(2*32) - 2^(0*32).  This allows easy word-oriented reduction
 * (32 bit words): The word at position 6 can just be subtracted from
 * word 6 (i.e. word 6 zeroed), and added to words 2 and 0.  This is
 * faster than Montgomery multiplication.
 *
 * Two problems with the naive implementation suggested above are carry
 * propagation and getting the reduction precise.
 *
 * Every time you do an add or subtract you have to propagate carries.
 * The result might come out between the modulus and 2^192 or 2^256,
 * in which case you subtract the modulus.  Most carry propagation is avoided
 * by using 64 bit words during computation, even though the radix is only
 * 2^32.  A carry propagation is done once in the multiplication
 * and once again after the reduction step.  (This idea comes from the carry
 * save adder used in hardware designs.)
 *
 * Exact reduction is required for only a few operations: comparisons,
 * and halving.  The multiplier for point multiplication must also be
 * exactly reduced.  So we do away with the requirement for exact
 * reduction in most operations.  Thus, any reduced value, X, can may
 * represented by X + k * modulus, for any integer k, as long as the
 * result is representable in the data structure.  Typically k is
 * between -1 and 1.  (A bigval_t has one more 32 bit word than is
 * required to hold the modulus, and is interpreted as 2's complement
 * binary, little endian by word, native endian within words.)
 *
 * An exact reduction function is supplied, and must be called as necessary.
 */

/*
 * The external function get_random_bytes is expected to be avaiable.
 * It must return 0 on success, and -1 on error.  Feel free to raname
 * this function, if necessary.
 */
static int get_random_bytes(uint8_t* buf, int len);

/*
 * CONFIGURATION STUFF
 *
 * All these values are undefined.  It seems better to set the
 * preprocessor variables in the makefile, and thus avoid
 * generating many different versions of the code.
 * This may not be practical with ECC_P192 and ECC_P256, but at
 * least that is only in the ecc.h file.
 */

/* define ECDSA to include ECDSA functions */
#define ECDSA
/* define ECC_TEST to rename the the exported symbols to avoid name collisions
   with openSSL, and a few other things necessary for linking with the
   test program ecctest.c */
/* define ARM7_ASM to use assembly code specially for the ARM7 processor */
// #define ARM7_ASM
/* define SMALL_CODE to skip unrolling loops */
// #define SMALL_CODE
/* define SPECIAL_SQUARE to generate a special case for squaring.
   Special squaring should just about halve the number of multiplies,
   but on Windows machines and if loops are unrolled (SMALL_CODE not
   defined) actually causes slight slowing. */
#define SPECIAL_SQUARE
/* define MPY2BITS to consume the multiplier two bits at a time. */
#define MPY2BITS


#ifdef ECC_TEST
/* rename to avoid conflicts with OpenSSL in ecctest.c code. */
#define ECDSA_sign TEST_ECDSA_sign
#define ECDSA_verify TEST_ECDSA_verify
#define COND_STATIC

#else /* ECC_TEST not defined */

#define COND_STATIC static

#endif /* ECC_TEST not defined */


typedef struct {
    int64_t data[2 * BIGLEN];
} dblbigval_t;


/* These values describe why the verify failed.  This simplifies testing. */
typedef enum {V_SUCCESS = 0, V_R_ZERO, V_R_BIG, V_S_ZERO, V_S_BIG,
              V_INFINITY, V_UNEQUAL} verify_res_t;

typedef enum {MOD_MODULUS = 0, MOD_ORDER} modulus_val_t;


#define MSW (BIGLEN - 1)
static void big_adjustP(bigval_t* tgt, bigval_t const* a, int64_t k);
static void big_1wd_mpy(bigval_t* tgt, bigval_t const* a, int32_t k);
static void big_sub(bigval_t* tgt, bigval_t const* a, bigval_t const* b);
static void big_precise_reduce(bigval_t* tgt, bigval_t const* a,
                               bigval_t const* modulus);

#define big_is_negative(a) ((int32_t)(a)->data[MSW] < 0)

/*
 * Does approximate reduction.  Subtracts most significant word times
 * modulus from src.  The double cast is important to get sign
 * extension right.
 */
#define big_approx_reduceP(tgt, src)                \
    big_adjustP(tgt, src, -(int64_t)(int32_t)(src)->data[MSW])

/* if tgt is a modular value, it must be precisely reduced */
#define big_is_odd(tgt) ((tgt)->data[0] & 1)

// Squares, always modulo the modulus
#define big_sqrP(tgt, a) big_mpyP(tgt, a, a, MOD_MODULUS)


#define m1 0xffffffffU

#define MIN(a, b) ((a) < (b) ? (a) : (b))
#define MAX(a, b) ((a) > (b) ? (a) : (b))

#define OVERFLOWCHECK(sum, a, b) ((((a) > 0) && ((b) > 0) && ((sum) <= 0)) || \
                                  (((a) < 0) && ((b) < 0) && ((sum) >= 0)))

/* NOTE WELL! The Z component must always be precisely reduced. */
typedef struct {
    bigval_t X;
    bigval_t Y;
    bigval_t Z;
} jacobian_point_t;

static bigval_t const big_zero = { { 0, 0, 0, 0, 0, 0, 0 } };
static bigval_t const big_one = { { 1, 0, 0, 0, 0, 0, 0 } };
static affine_point_t const affine_infinity = { { { 0, 0, 0, 0, 0, 0, 0 } },
                                                { { 0, 0, 0, 0, 0, 0, 0 } },
                                                B_TRUE };
static jacobian_point_t const jacobian_infinity = { { { 1, 0, 0, 0, 0, 0, 0 } },
                                                    { { 1, 0, 0, 0, 0, 0, 0 } },
                                                    { { 0, 0, 0, 0, 0, 0, 0 } } };
static bigval_t const modulusP256 = { { m1, m1, m1, 0, 0, 0, 1, m1, 0 } };
static bigval_t const b_P256 =
{ { 0x27d2604b, 0x3bce3c3e, 0xcc53b0f6, 0x651d06b0,
    0x769886bc, 0xb3ebbd55, 0xaa3a93e7, 0x5ac635d8, 0x00000000 } };
static bigval_t const orderP256 =
{ { 0xfc632551, 0xf3b9cac2, 0xa7179e84, 0xbce6faad,
    0xffffffff, 0xffffffff, 0x00000000, 0xffffffff,
    0x00000000 } };
#ifdef ECDSA
static dblbigval_t const orderDBL256 =
{ { 0xfc632551LL - 0x100000000LL,
    0xf3b9cac2LL - 0x100000000LL + 1LL,
    0xa7179e84LL - 0x100000000LL + 1LL,
    0xbce6faadLL - 0x100000000LL + 1LL,
    0xffffffffLL - 0x100000000LL + 1LL,
    0xffffffffLL - 0x100000000LL + 1LL,
    0x00000000LL + 0x1LL,
    0xffffffffLL - 0x100000000LL,
    0x00000000LL + 1LL } };
#endif /* ECDSA */


static affine_point_t const baseP256 = {
    { { 0xd898c296, 0xf4a13945, 0x2deb33a0, 0x77037d81,
        0x63a440f2, 0xf8bce6e5, 0xe12c4247, 0x6b17d1f2 } },
    { { 0x37bf51f5, 0xcbb64068, 0x6b315ece, 0x2bce3357,
        0x7c0f9e16, 0x8ee7eb4a, 0xfe1a7f9b, 0x4fe342e2 } },
    B_FALSE
};
#define modulusP modulusP256
#define orderP orderP256
#define orderDBL orderDBL256
#define base_point baseP256
#define curve_b b_P256



#ifdef ARM7_ASM
#define MULACC(a, b)                            \
    __asm                    \
    {                        \
        UMULL tmpr0, tmpr1, a, b;           \
        ADDS sum0, sum0, tmpr0;           \
        ADCS sum1, sum1, tmpr1;           \
        ADC cum_carry, cum_carry, 0x0;       \
    }
// cumcarry: 32-bit word that accumulates carries
// sum0: lower half 32-bit word of sum
// sum1: higher half 32-bit word of sum
// a   : 32-bit operand to be multiplied
// b   : 32-bit operand to be multiplied
// tmpr0, tmpr1: two temporary words
// sum = sum + A*B where cout may contain carry info from previous operations

#define MULACC_DOUBLE(a, b)                     \
    __asm                    \
    {                        \
        UMULL tmpr0, tmpr1, a, b;           \
        ADDS sum0, sum0, tmpr0;           \
        ADCS sum1, sum1, tmpr1;           \
        ADC cum_carry, cum_carry, 0x0;       \
        ADDS sum0, sum0, tmpr0;           \
        ADCS sum1, sum1, tmpr1;           \
        ADC cum_carry, cum_carry, 0x0;       \
    }

#define ACCUM(ap, bp) MULACC(*(ap), *(bp))
#define ACCUMDBL(ap, bp) MULACC_DOUBLE(*(ap), *(bp))

#else /* ARM7_ASM, below is platform independent */

/* (sum, carry) += a * b */
static void mpy_accum(int* cumcarry, uint64_t* sum, uint32_t a, uint32_t b)
{
    uint64_t product = (uint64_t)a * (uint64_t)b;
    uint64_t lsum = *sum;

    lsum += product;
    if (lsum < product) {
        *cumcarry += 1;
    }
    *sum = lsum;
}

#ifdef SPECIAL_SQUARE

/* (sum, carry += 2 * a * b.  Attempts to reduce writes to memory and
   branches caused slowdown on windows machines. */
static void mpy_accum_dbl(int* cumcarry, uint64_t* sum, uint32_t a, uint32_t b)
{
    uint64_t product = (uint64_t)a * (uint64_t)b;
    uint64_t lsum = *sum;
    lsum += product;
    if (lsum < product) {
        *cumcarry += 1;
    }
    lsum += product;
    if (lsum < product) {
        *cumcarry += 1;
    }
    *sum = lsum;
}

#endif /* SPECIAL_SQUARE */

/* ap and bp are pointers to the words to be multiplied and accumulated */
#define ACCUM(ap, bp) mpy_accum(&cum_carry, &u_accum, *(ap),  *(bp))
#define ACCUMDBL(ap, bp) mpy_accum_dbl(&cum_carry, &u_accum, *(ap),  *(bp))

#endif /* !ARM7_ASM, ie platform independent */


/*
 * The big_mpyP algorithm first multiplies the two arguments, with the
 * outer loop indexing over output words, and the inner "loop"
 * (unrolled unless SMALL_CODE is defined), collecting all the terms
 * that contribute to that output word.
 *
 * The impementation is inspired by the Wallace Tree often used in
 * hardware [Wallace], where (0, 1) terms of the same weight are
 * collected together into a sequence values each of which can be on
 * the order of the number of bits in a word, and then the sequence is
 * turned into a binary number with a carry save adder.  This is
 * generized from base 2 to base 2^32.
 *
 * The first part of the algorithm sums together products of equal
 * weight.  The outer loop does carry propagation and makes each value
 * at most 32 bits.
 *
 * Then corrections are applied for negative arguments.  (The first
 * part essentially does unsigned multiplication.)
 *
 * The reduction proceeds in 2 steps.  The first treats the 32 bit
 * values (in 64 bit words) from above as though they were
 * polynomials, and reduces by the paper and pencil method.  Carries
 * are propagated and the result collapsed to a sequence of 32 bit
 * words (in the target).  The second step subtracts MSW * modulus
 * from the result.  This usually (but not always) results in the MSW
 * being zero.  (And that makes subsequent mutliplications faster.)
 *
 * The modselect parameter chooses whether reduction is mod the modulus
 * or the order of the curve.  If ECDSA is not defined, this parameter
 * is ignored, and the curve modulus is used.
 */

/*
 * Computes a * b, approximately reduced mod modulusP or orderP,
 * depending on the modselect flag.
 */
static void big_mpyP(bigval_t* tgt, bigval_t const* a, bigval_t const* b,
                     modulus_val_t modselect)
{
    int64_t w[2 * BIGLEN];
    int64_t s_accum; /* signed */
    int i, minj, maxj, a_words, b_words, cum_carry;
#ifdef SMALL_CODE
    int j;
#else
    uint32_t const* ap;
    uint32_t const* bp;
#endif

#ifdef ARM7_ASM
    uint32_t tmpr0, tmpr1, sum0, sum1;
#else
    uint64_t u_accum;
#endif

#ifdef ECDSA
#define MODSELECT modselect
#else
#define MODSELECT MOD_MODULUS
#endif


    a_words = BIGLEN;
    while (a_words > 0 && a->data[a_words - 1] == 0) {
        --a_words;
    }

    /*
     * i is target index.  The j (in comments only) indexes
     * through the multiplier.
     */
#ifdef ARM7_ASM
    sum0 = 0;
    sum1 = 0;
    cum_carry = 0;
#else
    u_accum = 0;
    cum_carry = 0;
#endif

#ifndef SPECIAL_SQUARE
#define NO_SPECIAL_SQUARE 1
#else
#define NO_SPECIAL_SQUARE 0
#endif


    if (NO_SPECIAL_SQUARE || a != b) {

        /* normal multiply */

        /* compute length of b */
        b_words = BIGLEN;
        while (b_words > 0 && b->data[b_words - 1] == 0) {
            --b_words;
        }

        /* iterate over words of output */
        for (i = 0; i < a_words + b_words - 1; ++i) {
            /* Run j over all possible values such that
               0 <= j < b_words && 0 <= i-j < a_words.
               Hence
               j >= 0 and j > i - a_words and
               j < b_words and j <= i

               (j exists only in the mind of the reader.)
             */
            maxj = MIN(b_words - 1, i);
            minj = MAX(0, i - a_words + 1);

            /* ACCUM accumlates into <cum_carry, u_accum>. */
#ifdef SMALL_CODE
            for (j = minj; j <= maxj; ++j) {
                ACCUM(a->data + i - j, b->data + j);
            }
#else /* SMALL_CODE not defined */
      /*
       * The inner loop (over j, running from minj to maxj) is
       * unrolled.  Sequentially increasing case values in the code
       * are intended to coax the compiler into emitting a jump
       * table. Here j runs from maxj to minj, but addition is
       * commutative, so it doesn't matter.
       */

            ap = &a->data[i - minj];
            bp = &b->data[minj];

            /* the order is opposite the loop, but addition is commutative */
            switch (8 - (maxj - minj)) {
            case 0: ACCUM(ap - 8, bp + 8); /* j = 8 */

            case 1: ACCUM(ap - 7, bp + 7);

            case 2: ACCUM(ap - 6, bp + 6);

            case 3: ACCUM(ap - 5, bp + 5);

            case 4: ACCUM(ap - 4, bp + 4);

            case 5: ACCUM(ap - 3, bp + 3);

            case 6: ACCUM(ap - 2, bp + 2);

            case 7: ACCUM(ap - 1, bp + 1);

            case 8: ACCUM(ap - 0, bp + 0); /* j = 0 */
            }
#endif /* SMALL_CODE not defined */


            /* The total value is
               w + u_accum << (32 *i) + cum_carry << (32 * i + 64).
               The steps from here to the end of the i-loop (not counting
               squaring branch) and the increment of i by the loop
               maintain the invariant that the value is constant.
               (Assume w had been initialized to zero, even though we
               really didn't.) */

#ifdef ARM7_ASM
            w[i] = sum0;
            sum0 = sum1;
            sum1 = cum_carry;
            cum_carry = 0;
#else
            w[i] = u_accum & 0xffffffffULL;
            u_accum = (u_accum >> 32) + ((uint64_t)cum_carry << 32);
            cum_carry = 0;
#endif
        }
    } else {
        /* squaring */

#ifdef SPECIAL_SQUARE

        /* a[i] * a[j] + a[j] * a[i] == 2 * (a[i] * a[j]), so
           we can cut the number of multiplies nearly in half. */
        for (i = 0; i < 2 * a_words - 1; ++i) {

            /* Run j over all possible values such that
               0 <= j < a_words && 0 <= i-j < a_words && j < i-j
               Hence
               j >= 0 and j > i - a_words and
               j < a_words and 2*j < i
             */
            maxj = MIN(a_words - 1, i);
            /* Only go half way.  Must use (i-1)>> 1, not (i-1)/ 2 */
            maxj = MIN(maxj, (i - 1) >> 1);
            minj = MAX(0, i - a_words + 1);
#ifdef SMALL_CODE
            for (j = minj; j <= maxj; ++j) {
                ACCUMDBL(a->data + i - j, a->data + j);
            }
            /* j live */
            if ((i & 1) == 0) {
                ACCUM(a->data + j, a->data + j);
            }
#else /* SMALL_CODE not defined */
            ap = &a->data[i - minj];
            bp = &a->data[minj];
            switch (8 - (maxj - minj)) {
            case 0: ACCUMDBL(ap - 8, bp + 8); /* j = 8 */

            case 1: ACCUMDBL(ap - 7, bp + 7);

            case 2: ACCUMDBL(ap - 6, bp + 6);

            case 3: ACCUMDBL(ap - 5, bp + 5);

            case 4: ACCUMDBL(ap - 4, bp + 4);

            case 5: ACCUMDBL(ap - 3, bp + 3);

            case 6: ACCUMDBL(ap - 2, bp + 2);

            case 7: ACCUMDBL(ap - 1, bp + 1);

            case 8: ACCUMDBL(ap - 0, bp + 0); /* j = 0 */
            }

            /* Even numbered columns (zero based) have a middle element. */
            if ((i & 1) == 0) {
                ACCUM(a->data + maxj + 1, a->data + maxj + 1);
            }
#endif /* SMALL_CODE not defined */

            /* The total value is
               w + u_accum << (32 *i) + cum_carry << (32 * i + 64).
               The steps from here to the end of i-loop and
               the increment of i by the loop maintain the invariant
               that the total value is unchanged.
               (Assume w had been initialized to zero, even though we
               really didn't.) */
#ifdef ARM7_ASM
            w[i] = sum0;
            sum0 = sum1;
            sum1 = cum_carry;
            cum_carry = 0;
#else /* ARM7_ASM not defined */
            w[i] = u_accum & 0xffffffffULL;
            u_accum = (u_accum >> 32) + ((uint64_t)cum_carry << 32);
            cum_carry = 0;
#endif /* ARM7_ASM not defined */
        }
#endif /* SPECIAL_SQUARE */
    } /* false branch of NO_SPECIAL_SQUARE || (a != b)  */

    /* The total value as indicated above is maintained invariant
       down to the approximate reduction code below. */

    /* propagate any residual to next to end of array */
    for (; i < 2 * BIGLEN - 1; ++i) {
#ifdef ARM7_ASM
        w[i] = sum0;
        sum0 = sum1;
        sum1 = 0;
#else
        w[i] = u_accum & 0xffffffffULL;
        u_accum >>= 32;
#endif
    }
    /* i is still live */
    /* from here on, think of w as containing signed values */

    /* Last value of the array, still using i.  We store the entire 64
       bits.  There are two reasons for this.  The pedantic one is that
       this clearly maintains our invariant that the value has not
       changed.  The other one is that this makes w[BIGNUM-1] negative
       if the result was negative, and reduction depends on this. */

#ifdef ARM7_ASM
    w[i] = ((uint64_t)sum1 << 32) | sum0;
    /* sum1 = sum0 = 0;  maintain invariant */
#else
    w[i] = u_accum;
    /* u_accum = 0; maintain invariant */
#endif

    /*
     * Apply correction if a or b are negative.  It would be nice to
     * put this inside the i-loop to reduce memory bandwidth.  Later...
     *
     * signvedval(a) = unsignedval(a) - 2^(32*BIGLEN)*isneg(a).
     *
     * so signval(a) * signedval(b) = unsignedval(a) * unsignedval[b] -
     *   isneg(a) * unsignedval(b) * 2^(32*BIGLEN) -
     *   isneg(b) * unsingedval(a) * 2^ (32*BIGLEN) +
     *   isneg(a) * isneg(b) * 2 ^(2 * 32 * BIGLEN)
     *
     * If one arg is zero and the other is negative, obviously no
     * correction is needed, but we do not make a special case, since
     * the "correction" only adds in zero.
     */

    if (big_is_negative(a)) {
        for (i = 0; i < BIGLEN; ++i) {
            w[i + BIGLEN] -= b->data[i];
        }
    }
    if (big_is_negative(b)) {
        for (i = 0; i < BIGLEN; ++i) {
            w[i + BIGLEN] -= a->data[i];
        }
        if (big_is_negative(a)) {
            /* both negative */
            w[2 * BIGLEN - 1] += 1ULL << 32;
        }
    }

    /*
     * The code from here to the end of the function maintains w mod
     * modulusP constant, even though it changes the value of w.
     */

    /* reduce (approximate) */
    if (MODSELECT == MOD_MODULUS) {
        for (i = 2 * BIGLEN - 1; i >= MSW; --i) {
            int64_t v;
            v = w[i];
            if (v != 0) {
                w[i] = 0;
                w[i - 1] += v;
                w[i - 2] -= v;
                w[i - 5] -= v;
                w[i - 8] += v;
            }
        }
    } else {
        /* modulo order.  Not performance critical */
#ifdef ECDSA

        int64_t carry;

        /* convert to 32 bit values, except for most signifiant word */
        carry = 0;
        for (i = 0; i < 2 * BIGLEN - 1; ++i) {
            w[i] += carry;
            carry =  w[i] >> 32;
            w[i] -= carry << 32;
        }
        /* i is live */
        w[i] += carry;

        /* each iteration knocks off word i */
        for (i = 2 * BIGLEN - 1; i >= MSW; --i) { /* most to least significant */
            int64_t v;
            int64_t tmp;
            int64_t tmp2;
            int j;
            int k;

            for (k = 0; w[i] != 0 && k < 3; ++k) {
                v = w[i];
                carry = 0;
                for (j = i - MSW; j < 2 * BIGLEN; ++j) {
                    if (j <= i) {
                        tmp2 = -(v * orderDBL.data[j - i + MSW]);
                        tmp = w[j] + tmp2 + carry;
                    } else {
                        tmp = w[j] + carry;
                    }
                    if (j < 2 * BIGLEN - 1) {
                        carry = tmp >> 32;
                        tmp -= carry << 32;
                    } else {
                        carry = 0;
                    }
                    w[j] = tmp;
                }
            }
        }
#endif /* ECDSA */
    }

    /* propagate carries and copy out to tgt in 32 bit chunks. */
    s_accum = 0;
    for (i = 0; i < BIGLEN; ++i) {
        s_accum += w[i];
        tgt->data[i] = (uint32_t)s_accum;
        s_accum >>= 32; /* signed, so sign bit propagates */
    }

    /* final approximate reduction */

    if (MODSELECT == MOD_MODULUS) {
        big_approx_reduceP(tgt, tgt);
    } else {
#ifdef ECDSA
        if (tgt->data[MSW]) {
            /* Keep it simple! At one time all this was done in place,
               and was totally unobvious. */
            bigval_t tmp;
            /* The most significant word is signed, even though the
               whole array has declared uint32_t.  */
            big_1wd_mpy(&tmp, &orderP, (int32_t)tgt->data[MSW]);
            big_sub(tgt, tgt, &tmp);
        }
#endif /* ECDSA */
    }
}

/*
 * Adds k * modulusP to a and stores into target.  -2^62 <= k <= 2^62 .
 * (This is conservative.)
 */
static void big_adjustP(bigval_t* tgt, bigval_t const* a, int64_t k)
{


#define RDCSTEP(i, adj)                         \
    w += a->data[i];             \
    w += (adj);                  \
    tgt->data[i] = (uint32_t)(int32_t)w;     \
    w >>= 32;

    /* add k * modulus */

    if (k != 0) {
        int64_t w = 0;
        RDCSTEP(0, -k);
        RDCSTEP(1, 0);
        RDCSTEP(2, 0);
        RDCSTEP(3, k);
        RDCSTEP(4, 0);
        RDCSTEP(5, 0);
        RDCSTEP(6, k);
        RDCSTEP(7, -k);
        RDCSTEP(8, k);
    } else if (tgt != a) {
        *tgt = *a;
    }
}

/*
 * Computes k * a and stores into target.  Conditions: product must
 * be representable in bigval_t.
 */
static void big_1wd_mpy(bigval_t* tgt, bigval_t const* a, int32_t k)
{
    int64_t w = 0;
    int64_t tmp;
    int64_t prod;
    int j;

    for (j = 0; j <= MSW; ++j) {
        prod = (int64_t)k * (int64_t)a->data[j];
        tmp = w + prod;
        w = tmp;
        tgt->data[j] = (uint32_t)w;
        w -= tgt->data[j];
        w >>= 32;
    }
}


/*
 * Adds a to b as signed (2's complement) numbers.  Ok to use for
 * modular values if you don't let the sum overflow.
 */
COND_STATIC void big_add(bigval_t* tgt, bigval_t const* a, bigval_t const* b)
{
    uint64_t v;
    int i;

    v = 0;
    for (i = 0; i < BIGLEN; ++i) {
        v += a->data[i];
        v += b->data[i];
        tgt->data[i] = (uint32_t)v;
        v >>= 32;
    }
}

/*
 * modulo modulusP addition with approximate reduction.
 */
static void big_addP(bigval_t* tgt, bigval_t const* a, bigval_t const* b)
{
    big_add(tgt, a, b);
    big_approx_reduceP(tgt, tgt);
}


/* 2's complement subtraction */
static void big_sub(bigval_t* tgt, bigval_t const* a, bigval_t const* b)
{
    uint64_t v;
    int i;
    /* negation is equivalent to 1's complement and increment */

    v = 1; /* increment */
    for (i = 0; i < BIGLEN; ++i) {
        v += a->data[i];
        v += ~b->data[i]; /* 1's complement */
        tgt->data[i] = (uint32_t)v;
        v >>= 32;
    }
}


/*
 * modulo modulusP subtraction with approximate reduction.
 */
static void big_subP(bigval_t* tgt, bigval_t const* a, bigval_t const* b)
{
    big_sub(tgt, a, b);
    big_approx_reduceP(tgt, tgt);
}


/* returns 1 if a > b, -1 if a < b, and 0 if a == b.
   a and b are 2's complement.  When applied to modular values,
   args must be precisely reduced. */
static int big_cmp(bigval_t const* a, bigval_t const* b)
{
    int i;

    /* most significant word is treated as 2's complement */
    if ((int32_t)a->data[MSW] > (int32_t)b->data[MSW]) {
        return (1);
    } else if ((int32_t)a->data[MSW] < (int32_t)b->data[MSW]) {
        return (-1);
    }
    /* remainder treated as unsigned */
    for (i = MSW - 1; i >= 0; --i) {
        if (a->data[i] > b->data[i]) {
            return (1);
        } else if (a->data[i] < b->data[i]) {
            return (-1);
        }
    }
    return (0);
}


/*
 * Computes tgt = a mod modulus.  Only works with modluii slightly
 * less than 2**(32*(BIGLEN-1)).  Both modulusP and orderP qualify.
 */
static void big_precise_reduce(bigval_t* tgt, bigval_t const* a, bigval_t const* modulus)
{
    /*
     * src is a trick to avoid an extra copy of a to arg a to a
     * temporary.  Every statement uses src as the src and tgt as the
     * destination, and it executes src = tgt, so all subsequent
     * operations affect the modified data, not the original.  There is
     * a case to handle the situation of no modifications having been
     * made.
     */
    bigval_t const* src = a;

    /* If tgt < 0, a positive value gets added in, so eventually tgt
       will be >= 0.  If tgt > 0 and the MSW is non-zero, a non-zero
       value smaller than tgt gets subtracted, so eventually target
       becomes < 1 * 2**(32*MSW), but not negative, i.e. tgt->data[MSW]
       == 0, and thus loop termination is guaranteed. */


    while ((int32_t)src->data[MSW] != 0) {
        if (modulus != &modulusP) {
            /* General case.  Keep it simple! */
            bigval_t tmp;

            /* The most significant word is signed, even though the
               whole array has been declared uint32_t.  */
            big_1wd_mpy(&tmp, modulus, (int32_t)src->data[MSW]);
            big_sub(tgt, src, &tmp);
        } else {
            /* just an optimization.  The other branch would work, but slower. */
            big_adjustP(tgt, src, -(int64_t)(int32_t)src->data[MSW]);
        }
        src = tgt;
    }

    while (big_cmp(src, modulus) >= 0) {
        big_sub(tgt, src, modulus);
        src = tgt;
    }
    while ((int32_t)src->data[MSW] < 0) {
        big_add(tgt, src, modulus);
        src = tgt;
    }

    /* copy src to tgt if not already done */

    if (src != tgt) {
        *tgt = *src;
    }
}

/* computes floor(a / 2), 2's complement. */
static void big_halve(bigval_t* tgt, bigval_t const* a)
{
    uint32_t shiftval;
    uint32_t new_shiftval;
    int i;

    /* most significant word is 2's complement.  Do it separately. */
    shiftval = a->data[MSW] & 1;
    tgt->data[MSW] = (uint32_t)((int32_t)a->data[MSW] >> 1);

    for (i = MSW - 1; i >= 0; --i) {
        new_shiftval = a->data[i] & 1;
        tgt->data[i] = (a->data[i] >> 1) | (shiftval << 31);
        shiftval = new_shiftval;
    }
}

/*
 * computes tgt, such that 2 * tgt === a, (mod modulusP).  NOTE WELL:
 * arg a must be precisely reduced.  This function could do that, but
 * in some cases, arg a is known to already be reduced and we don't
 * want to waste cycles.  The code could be written more cleverly to
 * avoid passing over the data twice in the case of an odd value.
 */
static void big_halveP(bigval_t* tgt, bigval_t const* a)
{
    if (a->data[0] & 1) {
        /* odd */
        big_adjustP(tgt, a, 1);
        big_halve(tgt, tgt);
    } else {
        /* even */
        big_halve(tgt, a);
    }
}

/* returns B_TRUE if a is zero */
boolean_t big_is_zero(bigval_t const* a)
{
    int i;

    for (i = 0; i < BIGLEN; ++i) {
        if (a->data[i] != 0) {
            return (B_FALSE);
        }
    }
    return (B_TRUE);
}

/* returns B_TRUE if a is one */
static boolean_t big_is_one(bigval_t const* a)
{
    int i;

    if (a->data[0] != 1) {
        return (B_FALSE);
    }
    for (i = 1; i < BIGLEN; ++i) {
        if (a->data[i] != 0) {
            return (B_FALSE);
        }
    }
    return (B_TRUE);
}


/*
 * This uses the extended binary GCD (Greatest Common Divisor)
 * algorithm.  The binary GCD algorithm is presented in [KnuthV2] as
 * Algorithm X.  The extension to do division is presented in Homework
 * Problem 15 and its solution in the back of the book.
 *
 * The implementation here follows the presentation in [HMV] Algorithm
 * 2.22.
 *
 * If the denominator is zero, it will loop forever.  Be careful!
 * Modulus must be odd.  num and den must be positive.
 */
static void big_divide(bigval_t* tgt, bigval_t const* num, bigval_t const* den,
                       bigval_t const* modulus)
{
    bigval_t u, v, x1, x2;

    u = *den;
    v = *modulus;
    x1 = *num;
    x2 = big_zero;

    while (!big_is_one(&u) && !big_is_one(&v)) {
        while (!big_is_odd(&u)) {
            big_halve(&u, &u);
            if (big_is_odd(&x1)) {
                big_add(&x1, &x1, modulus);
            }
            big_halve(&x1, &x1);
        }
        while (!big_is_odd(&v)) {
            big_halve(&v, &v);
            if (big_is_odd(&x2)) {
                big_add(&x2, &x2, modulus);
            }
            big_halve(&x2, &x2);
        }
        if (big_cmp(&u, &v) >= 0) {
            big_sub(&u, &u, &v);
            big_sub(&x1, &x1, &x2);
        } else {
            big_sub(&v, &v, &u);
            big_sub(&x2, &x2, &x1);
        }
    }

    if (big_is_one(&u)) {
        big_precise_reduce(tgt, &x1, modulus);
    } else {
        big_precise_reduce(tgt, &x2, modulus);
    }
}


static void big_triple(bigval_t* tgt, bigval_t const* a)
{
    int i;
    uint64_t accum = 0;

    /* technically, the lower significance words should be treated as
       unsigned and the most significant word treated as signed
       (arithmetic right shift instead of logical right shift), but
       accum can never get negative during processing the lower
       significance words, and the most significant word is the last
       word processed, so what is left in the accum after the final
       shift does not matter.
     */

    for (i = 0; i < BIGLEN; ++i) {
        accum += a->data[i];
        accum += a->data[i];
        accum += a->data[i];
        tgt->data[i] = (uint32_t)accum;
        accum >>= 32;
    }
}



/*
 * The point add and point double algorithms use mixed Jacobian
 * and affine coordinates.  The affine point (x,y) corresponds
 * to the Jacobian point (X, Y, Z), for any non-zero Z, with X = Z^2 * x
 * and Y = Z^3 * y.  The infinite point is represented in Jacobian
 * coordinates as (1, 1, 0).
 */

#define jacobian_point_is_infinity(P) (big_is_zero(&(P)->Z))

static void toJacobian(jacobian_point_t* tgt, affine_point_t const* a) {
    tgt->X = a->x;
    tgt->Y = a->y;
    tgt->Z = big_one;
}

/* a->Z must be precisely reduced */
static void toAffine(affine_point_t* tgt, jacobian_point_t const* a)
{
    bigval_t zinv, zinvpwr;

    if (big_is_zero(&a->Z)) {
        *tgt = affine_infinity;
        return;
    }
    big_divide(&zinv, &big_one, &a->Z, &modulusP);
    big_sqrP(&zinvpwr, &zinv);  /* Zinv^2 */
    big_mpyP(&tgt->x, &a->X, &zinvpwr, MOD_MODULUS);
    big_mpyP(&zinvpwr, &zinvpwr, &zinv, MOD_MODULUS); /* Zinv^3 */
    big_mpyP(&tgt->y, &a->Y, &zinvpwr, MOD_MODULUS);
    big_precise_reduce(&tgt->x, &tgt->x, &modulusP);
    big_precise_reduce(&tgt->y, &tgt->y, &modulusP);
    tgt->infinity = B_FALSE;
}

/*
 * From [HMV] Algorithm 3.21.
 */

/* tgt = 2 * P.  P->Z must be precisely reduced and
   tgt->Z will be precisely reduced */
static void pointDouble(jacobian_point_t* tgt, jacobian_point_t const* P)
{
    bigval_t x3loc, y3loc, z3loc, t1, t2, t3;

#define x1 (&P->X)
#define y1 (&P->Y)
#define z1 (&P->Z)
#define x3 (&x3loc)
#define y3 (&y3loc)
#define z3 (&z3loc)

    /* This requires P->Z be precisely reduced */
    if (jacobian_point_is_infinity(P)) {
        *tgt = jacobian_infinity;
        return;
    }

    big_sqrP(&t1, z1);
    big_subP(&t2, x1, &t1);
    big_addP(&t1, x1, &t1);
    big_mpyP(&t2, &t2, &t1, MOD_MODULUS);
    big_triple(&t2, &t2);
    big_addP(y3, y1, y1);
    big_mpyP(z3, y3, z1, MOD_MODULUS);
    big_sqrP(y3, y3);
    big_mpyP(&t3, y3, x1, MOD_MODULUS);
    big_sqrP(y3, y3);
    big_halveP(y3, y3);
    big_sqrP(x3, &t2);
    big_addP(&t1, &t3, &t3);
    /* x1 not used after this point.  Safe to store to tgt, even if aliased */
    big_subP(&tgt->X /* x3 */, x3, &t1);
#undef  x3
#define x3 (&tgt->X)
    big_subP(&t1, &t3, x3);
    big_mpyP(&t1, &t1, &t2, MOD_MODULUS);
    big_subP(&tgt->Y, &t1, y3);

    /* Z components of returned Jacobian points must
       be precisely reduced */
    big_precise_reduce(&tgt->Z, z3, &modulusP);
#undef x1
#undef y1
#undef z1
#undef x3
#undef y3
#undef z3
}


/* From [HMV] Algorithm 3.22 */

/* tgt = P + Q.  P->Z must be precisely reduced.
   tgt->Z will be precisely reduced.  tgt and P can be aliased.
 */
static void pointAdd(jacobian_point_t* tgt, jacobian_point_t const* P,
                     affine_point_t const* Q)
{
    bigval_t t1, t2, t3, t4, x3loc;

    if (Q->infinity) {
        if (tgt != P) {
            *tgt = *P;
        }
        return;
    }

    /* This requires that P->Z be precisely reduced */
    if (jacobian_point_is_infinity(P)) {
        toJacobian(tgt, Q);
        return;
    }


#define x1 (&P->X)
#define y1 (&P->Y)
#define z1 (&P->Z)
#define x2 (&Q->x)
#define y2 (&Q->y)
#define x3 (&x3loc)
#define y3 (&y3loc)
#define z3 (&tgt->Z)

    big_sqrP(&t1, z1);
    big_mpyP(&t2, &t1, z1, MOD_MODULUS);
    big_mpyP(&t1, &t1, x2, MOD_MODULUS);
    big_mpyP(&t2, &t2, y2, MOD_MODULUS);
    big_subP(&t1, &t1, x1);
    big_subP(&t2, &t2, y1);
    /* big_is_zero requires precisely reduced arg */
    big_precise_reduce(&t1, &t1, &modulusP);
    if (big_is_zero(&t1)) {
        big_precise_reduce(&t2, &t2, &modulusP);
        if (big_is_zero(&t2)) {
            toJacobian(tgt, Q);
            pointDouble(tgt, tgt);
        } else {
            *tgt = jacobian_infinity;
        }
        return;
    }
    /* store into target.  okay, even if tgt is aliased with P,
       as z1 is not subsequently used */
    big_mpyP(z3, z1, &t1, MOD_MODULUS);
    /* z coordinates of returned jacobians must be precisely reduced. */
    big_precise_reduce(z3, z3, &modulusP);
    big_sqrP(&t3, &t1);
    big_mpyP(&t4, &t3, &t1, MOD_MODULUS);
    big_mpyP(&t3, &t3, x1, MOD_MODULUS);
    big_addP(&t1, &t3, &t3);
    big_sqrP(x3, &t2);
    big_subP(x3, x3, &t1);
    big_subP(&tgt->X /* x3 */, x3, &t4);
    /* switch x3 to tgt */
#undef x3
#define x3 (&tgt->X)
    big_subP(&t3, &t3, x3);
    big_mpyP(&t3, &t3, &t2, MOD_MODULUS);
    big_mpyP(&t4, &t4, y1, MOD_MODULUS);
    /* switch y3 to tgt */
#undef y3
#define y3 (&tgt->Y)
    big_subP(y3, &t3, &t4);
#undef  x1
#undef  y1
#undef  z1
#undef  x2
#undef  y2
#undef  x3
#undef  y3
#undef  z3

}

/* pointMpyP uses a left-to-right binary double-and-add method, which
 * is an exact analogy to the left-to-right binary meethod for
 * exponentiation described in [KnuthV2] Section 4.6.3.
 */

/* returns bit i of bignum n.  LSB of n is bit 0. */
#define big_get_bit(n, i) (((n)->data[(i) / 32] >> ((i) % 32)) & 1)
/* returns bits i+1 and i of bignum n.  LSB of n is bit 0; i <= 30 */
#define big_get_2bits(n, i) (((n)->data[(i) / 32] >> ((i) % 32)) & 3)

/* k must be non-negative.  Negative values (incorrectly)
   return the infinite point */

static void pointMpyP(affine_point_t* tgt, bigval_t const* k, affine_point_t const* P)
{
    int i;
    jacobian_point_t Q;
#ifdef MPY2BITS
    affine_point_t const* mpyset[4];
    affine_point_t twoP, threeP;
#endif /* MPY2BITS */

    if (big_is_negative(k)) {
        /* This should never happen.*/
        *tgt = affine_infinity;
        return;
    }

    Q = jacobian_infinity;

    /* faster */
    if (big_is_zero(k) || big_is_negative(k)) {
        *tgt = affine_infinity;
        return;
    }

#ifndef MPY2BITS
    /* Classical high-to-low method */
    /* discard high order zeros */
    for (i = BIGLEN * 32 - 1; i >= 0; --i) {
        if (big_get_bit(k, i)) {
            break;
        }
    }
    /* Can't fall through since k is non-zero.  We get here only via the break */
    /* discard highest order 1 bit */
    --i;

    toJacobian(&Q, P);
    for (; i >= 0; --i) {
        pointDouble(&Q, &Q);
        if (big_get_bit(k, i)) {
            pointAdd(&Q, &Q, P);
        }
    }
#else /* MPY2BITS defined */
      /* multiply 2 bits at a time */
      /* precompute 1P, 2P, and 3P */
    mpyset[0] = (affine_point_t*)0;
    mpyset[1] = P;
    toJacobian(&Q, P);  /* Q = P */
    pointDouble(&Q, &Q); /* now Q = 2P */
    toAffine(&twoP, &Q);
    mpyset[2] = &twoP;
    pointAdd(&Q, &Q, P); /* now Q = 3P */
    toAffine(&threeP, &Q);
    mpyset[3] = &threeP;

    /* discard high order zeros (in pairs) */
    for (i = BIGLEN * 32 - 2; i >= 0; i -= 2) {
        if (big_get_2bits(k, i)) {
            break;
        }
    }

    Q = jacobian_infinity;

    for (; i >= 0; i -= 2) {
        int mbits = big_get_2bits(k, i);
        pointDouble(&Q, &Q);
        pointDouble(&Q, &Q);
        if (mpyset[mbits] != (affine_point_t*)0) {
            pointAdd(&Q, &Q, mpyset[mbits]);
        }
    }

#endif /* MPY2BITS */

    toAffine(tgt, &Q);
}

COND_STATIC boolean_t in_curveP(affine_point_t const* P)
{
    bigval_t sum, product;

    if (P->infinity) {
        return (B_TRUE);
    }

    big_sqrP(&product, &P->x);
    big_mpyP(&sum, &product, &P->x, MOD_MODULUS); /* x^3 */
    big_triple(&product, &P->x); /* 3 x */
    big_subP(&sum, &sum, &product); /* x^3 -3x */
    big_addP(&sum, &sum, &curve_b); /* x^3 -3x + b */
    big_sqrP(&product, &P->y); /* y^2 */
    big_subP(&sum, &sum, &product); /* -y^2 + x^3 -3x + b */
    big_precise_reduce(&sum, &sum, &modulusP);

    return(big_is_zero(&sum));

}

/* returns a bigval between 0 or 1 (depending on allow_zero)
   and order-1, inclusive.  Returns 0 on success, -1 otherwise */
COND_STATIC int big_get_random_n(bigval_t* tgt, boolean_t allow_zero)
{
    int rv;

    tgt->data[BIGLEN - 1] = 0;
    do {
        rv = get_random_bytes((uint8_t*) tgt, sizeof (uint32_t) * (BIGLEN - 1));
        if (rv < 0) {
            return (-1);
        }
    } while ((!allow_zero && big_is_zero(tgt)) ||
             (big_cmp(tgt, &orderP) >= 0));

    return (0);
}

/*
 * computes a secret value, k, and a point, P1, to send to the other
 * party.  Returns 0 on success, -1 on failure (of the RNG).
 */
int ECDH_generate(affine_point_t* P1, bigval_t* k)
{
    int rv;

    rv = big_get_random_n(k, B_FALSE);
    if (rv < 0) {
        return (-1);
    }
    pointMpyP(P1, k, &base_point);

    return (0);
}

/* takes the point sent by the other party, and verifies that it is a
   valid point.  If 1 <= k < orderP and the point is valid, it stores
   the resuling point *tgt and returns B_TRUE.  If the point is invalid it
   returns B_FALSE.  The behavior with k out of range is unspecified,
   but safe. */

COND_STATIC boolean_t ECDH_derive_pt(affine_point_t* tgt, bigval_t const* k, affine_point_t const* Q)
{
    if (Q->infinity) {
        return (B_FALSE);
    }
    if (big_is_negative(&Q->x)) {
        return (B_FALSE);
    }
    if (big_cmp(&Q->x, &modulusP) >= 0) {
        return (B_FALSE);
    }
    if (big_is_negative(&Q->y)) {
        return (B_FALSE);
    }
    if (big_cmp(&Q->y, &modulusP) >= 0) {
        return (B_FALSE);
    }
    if (!in_curveP(Q)) {
        return (B_FALSE);
    }

    /* [HMV] Section 4.3 states that the above steps, combined with the
     * fact the h=1 for the curves used here, implies that order*Q =
     * Infinity, which is required by ANSI X9.63.
     */

    pointMpyP(tgt, k, Q);
    /* Q2 can't be infinity if 1 <= k < orderP, which is supposed to be
       the case, but the test is so cheap, we just do it. */
    if (tgt->infinity) {
        return (B_FALSE);
    }
    return (B_TRUE);
}

void AJ_BigvalEncode(const bigval_t* src, uint8_t* tgt, size_t tgtlen)
{
    size_t i;
    uint8_t v;
    uint8_t highbytes = big_is_negative(src) ? 0xff : 0;

    /* LSbyte to MS_byte */
    for (i = 0; i < 4 * BIGLEN; ++i) {
        if (i < tgtlen) {
            v = src->data[i / 4] >> (8 * (i % 4));
            ((uint8_t*)tgt)[tgtlen - 1 - i] = v;
        }
    }
    /* i is live */
    for (; i < tgtlen; ++i) {
        ((uint8_t*)tgt)[tgtlen - 1 - i] = highbytes;
    }
}

void AJ_BigvalDecode(const uint8_t* src, bigval_t* tgt, size_t srclen)
{
    size_t i;
    uint8_t v;

    /* zero the bigval_t */
    memset(tgt, 0, sizeof (bigval_t));
    /* scan from LSbyte to MSbyte */
    for (i = 0; i < srclen && i < 4 * BIGLEN; ++i) {
        v = ((uint8_t*)src)[srclen - 1 - i];
        tgt->data[i / 4] |= (uint32_t)v << (8 * (i % 4));
    }
}

#ifdef ECDSA
/*
 * This function sets the r and s fields of sig.  The implementation
 * follows HMV Algorithm 4.29.
 */
static int ECDSA_sign(bigval_t const* msgdgst,
                      bigval_t const* privkey,
                      ECDSA_sig_t* sig)
{
    int rv;
    affine_point_t P1;
    bigval_t k;
    bigval_t t;

startpoint:

    rv = ECDH_generate(&P1, &k);
    if (rv) {
        return (rv);
    }

    big_precise_reduce(&sig->r, &P1.x, &orderP);
    if (big_is_zero(&sig->r)) {
        goto startpoint;
    }

    big_mpyP(&t, privkey, &sig->r, MOD_ORDER);
    big_add(&t, &t, msgdgst);
    big_precise_reduce(&t, &t, &orderP); /* may not be necessary */
    big_divide(&sig->s, &t, &k, &orderP);
    if (big_is_zero(&sig->s)) {
        goto startpoint;
    }

    return (0);
}

/*
 * Returns B_TRUE if the signature is valid.
 * The implementation follow HMV Algorithm 4.30.
 */
static verify_res_t ECDSA_verify_inner(bigval_t const* msgdgst,
                                       affine_point_t const* pubkey,
                                       ECDSA_sig_t const* sig)
{

    /* We could reuse variables and save stack space.  If stack space
       is tight, u1 and u2 could be the same variable by interleaving
       the big multiplies and the point multiplies. P2 and X could be
       the same variable.  X.x could be reduced in place, eliminating
       v. And if you really wanted to get tricky, I think one could use
       unions between the affine and jacobian versions of points. But
       check that out before doing it. */

    bigval_t v;
    bigval_t w;
    bigval_t u1;
    bigval_t u2;
    affine_point_t P1;
    affine_point_t P2;
    affine_point_t X;
    jacobian_point_t P2Jacobian;
    jacobian_point_t XJacobian;

    if (big_cmp(&sig->r, &big_one) < 0) {
        return (V_R_ZERO);
    }
    if (big_cmp(&sig->r, &orderP) >= 0) {
        return(V_R_BIG);
    }
    if (big_cmp(&sig->s, &big_one) < 0) {
        return (V_S_ZERO);
    }
    if (big_cmp(&sig->s, &orderP) >= 0) {
        return(V_S_BIG);
    }

    big_divide(&w, &big_one, &sig->s, &orderP);
    big_mpyP(&u1, msgdgst, &w, MOD_ORDER);
    big_precise_reduce(&u1, &u1, &orderP);
    big_mpyP(&u2, &sig->r, &w, MOD_ORDER);
    big_precise_reduce(&u2, &u2, &orderP);
    pointMpyP(&P1, &u1, &base_point);
    pointMpyP(&P2, &u2, pubkey);
    toJacobian(&P2Jacobian, &P2);
    pointAdd(&XJacobian, &P2Jacobian, &P1);
    toAffine(&X, &XJacobian);
    if (X.infinity) {
        return (V_INFINITY);
    }
    big_precise_reduce(&v, &X.x, &orderP);
    if (big_cmp(&v, &sig->r) != 0) {
        return (V_UNEQUAL);
    }
    return (V_SUCCESS);
}

boolean_t ECDSA_verify(bigval_t const* msgdgst,
                       affine_point_t const* pubkey,
                       ECDSA_sig_t const* sig)
{
    if (ECDSA_verify_inner(msgdgst, pubkey, sig) == V_SUCCESS) {
        return B_TRUE;
    }
    return B_FALSE;
}



/*
 * Converts a hash value to a bigval_t.  The rules for this in
 * ANSIX9.62 are strange.  Let b be the number of octets necessary to
 * represent the modulus.  If the size of the hash is less than or
 * equal to b, the hash is interpreted directly as a number.
 * Otherwise the left most b octets of the hash are converted to a
 * number. The hash must be big-endian by byte. There is no alignment
 * requirement on hashp.
 */
void ECC_hash_to_bigval(bigval_t* tgt, void const* hashp, unsigned int hashlen)
{
    unsigned int i;

    /* The "4"s in the rest of this function are the number of bytes in
       a uint32_t (what bigval_t's are made of).  The "8" is the number
       of bits in a byte. */

    /* reduce hashlen to modulus size, if necessary */

    if (hashlen > 4 * (BIGLEN - 1)) {
        hashlen = 4 * (BIGLEN - 1);
    }

    *tgt = big_zero;
    /* move one byte at a time starting with least significant byte */
    for (i = 0; i < hashlen; ++i) {
        tgt->data[i / 4] |=
            ((uint8_t*)hashp)[hashlen - 1 - i] << (8 * (i % 4));
    }
}
#endif /* ECDSA */

#ifdef ECC_TEST
char* ECC_feature_list(void)
{
    return ("ECC_P256"
#ifdef ECDSA
            " ECDSA"
#endif
#ifdef SPECIAL_SQUARE
            " SPECIAL_SQUARE"
#endif
#ifdef SMALL_CODE
            " SMALL_CODE"
#endif
#ifdef MPY2BITS
            " MPY2BITS"
#endif
#ifdef ARM7_ASM
            " ARM7_ASM"
#endif
            );
}
#endif /* ECC_TEST */

static int get_random_bytes(uint8_t* buf, int len) {
    AJ_RandBytes(buf, len);
    return 0;
}



/**
 * Generates the Ephemeral Diffie-Hellman key pair.
 *
 * @param publicKey The output public key
 * @param privateKey The output private key
 *
 * @return  - AJ_OK if the key pair is successfully generated.
 *          - AJ_ERR_SECURITY otherwise
 */
AJ_Status AJ_GenerateDHKeyPair(ecc_publickey* publicKey, ecc_privatekey* privateKey)
{
    if (ECDH_generate(publicKey, privateKey) == 0) {
        return AJ_OK;
    }
    return AJ_ERR_SECURITY;
}


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
AJ_Status AJ_GenerateShareSecret(ecc_publickey* peerPublicKey, ecc_privatekey* privateKey, ecc_secret* secret)
{
    boolean_t derive_rv;

    derive_rv = ECDH_derive_pt(secret, privateKey, peerPublicKey);
    if (!derive_rv) {
        return AJ_ERR_SECURITY;  /* bad */
    }
    if (derive_rv) {
        if (!in_curveP(secret)) {
            return AJ_ERR_SECURITY;  /* bad */
        }
    }
    return AJ_OK;
}

/**
 * Generates the DSA key pair.
 *
 * @param publicKey The output public key
 * @param privateKey The output private key
 * @return  - AJ_OK if the key pair is successfully generated
 *          - AJ_ERR_SECURITY otherwise
 */
AJ_Status AJ_GenerateDSAKeyPair(ecc_publickey* publicKey, ecc_privatekey* privateKey)
{
    if (ECDH_generate(publicKey, privateKey) == 0) {
        return AJ_OK;
    }
    return AJ_ERR_SECURITY;
}

/**
 * Sign a digest using the DSA key
 * @param digest The digest to sign
 * @param signingPrivateKey The signing private key
 * @param sig The output signature
 * @return  - AJ_OK if the signing process succeeds
 *          - AJ_ERR_SECURITY otherwise
 */
AJ_Status AJ_DSASignDigest(const uint8_t* digest, const ecc_privatekey* signingPrivateKey, ecc_signature* sig)
{
    bigval_t source;

    ECC_hash_to_bigval(&source, digest, SHA256_DIGEST_LENGTH);
    if (ECDSA_sign(&source, signingPrivateKey, sig) == 0) {
        return AJ_OK;
    }
    return AJ_ERR_SECURITY;
}

/**
 * Sign a buffer using the DSA key
 * @param buf The buffer to sign
 * @param len The buffer len
 * @param signingPrivateKey The signing private key
 * @param sig The output signature
 * @return  - AJ_OK if the signing process succeeds
 *          - AJ_ERR_SECURITY otherwise
 */
AJ_Status AJ_DSASign(const uint8_t* buf, uint16_t len, const ecc_privatekey* signingPrivateKey, ecc_signature* sig)
{
    AJ_SHA256_Context ctx;
    uint8_t digest[SHA256_DIGEST_LENGTH];

    AJ_SHA256_Init(&ctx);
    AJ_SHA256_Update(&ctx, (const uint8_t*) buf, (size_t) len);
    AJ_SHA256_Final(&ctx, digest);

    return AJ_DSASignDigest(digest, signingPrivateKey, sig);
}

/**
 * Verify DSA signature of a digest
 * @param digest The digest to sign
 * @param sig The signature
 * @param pubKey The signing public key
 * @return  - AJ_OK if the signature verification succeeds
 *          - AJ_ERR_SECURITY otherwise
 */
AJ_Status AJ_DSAVerifyDigest(const uint8_t* digest, const ecc_signature* sig, const ecc_publickey* pubKey)
{
    bigval_t source;

    ECC_hash_to_bigval(&source, digest, SHA256_DIGEST_LENGTH);
    if (ECDSA_verify(&source, pubKey, sig) == B_TRUE) {
        return AJ_OK;
    }

    return AJ_ERR_SECURITY;
}
/**
 * Verify DSA signature of a buffer
 * @param buf The buffer to sign
 * @param len The buffer len
 * @param sig The signature
 * @param pubKey The signing public key
 * @return  - AJ_OK if the signature verification succeeds
 *          - AJ_ERR_SECURITY otherwise
 */
AJ_Status AJ_DSAVerify(const uint8_t* buf, uint16_t len, const ecc_signature* sig, const ecc_publickey* pubKey)
{
    AJ_SHA256_Context ctx;
    uint8_t digest[SHA256_DIGEST_LENGTH];

    AJ_SHA256_Init(&ctx);
    AJ_SHA256_Update(&ctx, (const uint8_t*) buf, (size_t) len);
    AJ_SHA256_Final(&ctx, digest);

    return AJ_DSAVerifyDigest(digest, sig, pubKey);
}
