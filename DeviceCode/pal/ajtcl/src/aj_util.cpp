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

#include "aj_target.h"
#include "aj_util.h"
#include "aj_version.h"

#define AJ_TO_STRING(x) # x
#define AJ_VERSION_STRING(a, b, c, d, e) AJ_TO_STRING(a) "." AJ_TO_STRING(b) "." AJ_TO_STRING(c) AJ_TO_STRING(d) " Tag " AJ_TO_STRING(e) "\0"
const char* AJ_GetVersion()
{
    static const char VERSION[] = AJ_VERSION_STRING(AJ_RELEASE_YEAR_STR, AJ_RELEASE_MONTH_STR, AJ_FEATURE_VERSION_STR, AJ_BUGFIX_VERSION_STR, AJ_RELEASE_TAG);
    return &VERSION[0];
}

static uint8_t A2H(char hex, AJ_Status* status)
{
    if (hex >= '0' && hex <= '9') {
        return hex - '0';
    }
    hex |= 0x20;
    if (hex >= 'a' && hex <= 'f') {
        return 10 + hex - 'a';
    } else if (hex >= 'A' && hex <= 'F') {
        return 10 + hex - 'A';
    } else {
        *status = AJ_ERR_INVALID;
        return 0;
    }
}

int32_t AJ_StringFindFirstOf(const char* str, char* chars)
{
    if (str) {
        const char* p = str;
        do {
            const char* c = chars;
            while (*c) {
                if (*p == *c++) {
                    return (int32_t)(p - str);
                }
            }
        } while (*(++p));
    }
    return -1;
}

AJ_Status AJ_RawToHex(const uint8_t* raw, size_t rawLen, char* hex, size_t hexLen, uint8_t lower)
{
    static const char nibble_upper[] = "0123456789ABCDEF";
    static const char nibble_lower[] = "0123456789abcdef";
    const char* nibble = lower ? nibble_lower : nibble_upper;
    char* h = hex + 2 * rawLen;
    const uint8_t* a = raw + rawLen;

    if ((2 * rawLen + 1) > hexLen) {
        return AJ_ERR_RESOURCES;
    }
    h[0] = '\0';
    /*
     * Running backwards encode each byte in inStr as a pair of ascii hex digits.
     * Going backwards allows the raw and hex buffers to be the same buffer.
     */
    while (rawLen--) {
        uint8_t n = *(--a);
        h -= 2;
        h[0] = nibble[n >> 4];
        h[1] = nibble[n & 0xF];
    }
    return AJ_OK;
}

AJ_Status AJ_HexToRaw(const char* hex, size_t hexLen, uint8_t* raw, size_t rawLen)
{
    AJ_Status status = AJ_OK;
    char* p = (char*)raw;
    size_t sz = hexLen ? hexLen : strlen(hex);
    size_t i;

    /*
     * Length of encoded hex must be an even number
     */
    if (sz & 1) {
        return AJ_ERR_UNEXPECTED;
    }
    if (rawLen < (sz / 2)) {
        return AJ_ERR_RESOURCES;
    }
    for (i = 0; (i < sz) && (status == AJ_OK); i += 2, hex += 2) {
        *p++ = (A2H(hex[0], &status) << 4) | A2H(hex[1], &status);
    }
    return status;
}

static const char encode_token[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
static const uint8_t decode_token[] = {
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3e, 0x00, 0x00, 0x00, 0x3f,
    0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e,
    0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28,
    0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f, 0x30, 0x31, 0x32, 0x33, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
};

AJ_Status AJ_RawToB64(const uint8_t* raw, size_t rawlen, char* pem, size_t pemlen)
{
    char* p = pem;
    uint32_t b;
    int n = rawlen;

    if (pemlen < 4 * ((rawlen + 2) / 3) + 1) {
        return AJ_ERR_RESOURCES;
    }

    while (n > 0) {
        b = 0;
        b |= (n > 0 ? (*raw++) << 2 * 8 : 0);
        b |= (n > 1 ? (*raw++) << 1 * 8 : 0);
        b |= (n > 2 ? (*raw++) << 0 * 8 : 0);
        *p++ = encode_token[(b >> 3 * 6) & 0x3F];
        *p++ = encode_token[(b >> 2 * 6) & 0x3F];
        *p++ = (n > 1 ? encode_token[(b >> 1 * 6) & 0x3F] : '=');
        *p++ = (n > 2 ? encode_token[(b >> 0 * 6) & 0x3F] : '=');
        n -= 3;
    }
    *p = '\0';

    return AJ_OK;
}

AJ_Status AJ_B64ToRaw(const char* pem, size_t pemlen, uint8_t* raw, size_t rawlen)
{
    uint8_t* r = raw;
    uint32_t b;
    int n = pemlen;

    if (rawlen < 3 * (pemlen / 4)) {
        return AJ_ERR_RESOURCES;
    }
    if (0 != (pemlen % 4)) {
        return AJ_ERR_RESOURCES;
    }

    /*
     * Don't need to explicitly look for '=' padding,
     * the decode_token for '=' is 0x00.
     */
    while (n > 0) {
        b = 0;
        b |= decode_token[(uint8_t) *pem++] << 3 * 6;
        b |= decode_token[(uint8_t) *pem++] << 2 * 6;
        b |= decode_token[(uint8_t) *pem++] << 1 * 6;
        b |= decode_token[(uint8_t) *pem++] << 0 * 6;
        *r++ = (b >> 2 * 8) & 0xFF;
        *r++ = (b >> 1 * 8) & 0xFF;
        *r++ = (b >> 0 * 8) & 0xFF;
        n -= 4;
    }

    return AJ_OK;
}
