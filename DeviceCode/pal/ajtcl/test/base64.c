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
#define AJ_MODULE TEST_BASE64

#include <stdio.h>
#include <stdlib.h>
#include <time.h>
#include "aj_debug.h"
#include "alljoyn.h"
#include "aj_util.h"

uint8_t dbgTEST_BASE64 = 0;

static int test(char* input, char* output)
{
    AJ_Status status;
    int inputlen;
    int outputlen;
    char encode[1024];
    char decode[1024];

    inputlen = strlen(input);
    outputlen = strlen(output);

    status = AJ_RawToB64((uint8_t*) input, inputlen, encode, sizeof (encode));
    if (AJ_OK != status) {
        AJ_AlwaysPrintf(("FAILED STATUS\n"));
        return 1;
    }
    if (0 != strncmp(output, encode, outputlen)) {
        AJ_AlwaysPrintf(("FAILED ENCODE\n"));
        return 1;
    }

    status = AJ_B64ToRaw(output, outputlen, (uint8_t*) decode, sizeof (decode));
    if (AJ_OK != status) {
        AJ_AlwaysPrintf(("FAILED STATUS\n"));
        return 1;
    }
    if (0 != strncmp(input, decode, inputlen)) {
        AJ_AlwaysPrintf(("FAILED DECODE\n"));
        return 1;
    }

    return 0;
}

int AJ_Main(void)
{
    /*
     * put your test cases here.
     */

    if (test("This is a test.", "VGhpcyBpcyBhIHRlc3Qu")) {
        AJ_AlwaysPrintf(("FAILED\n"));
    } else {
        AJ_AlwaysPrintf(("PASSED\n"));
    }

    return 0;
}

#ifdef AJ_MAIN
int main(void)
{
    return AJ_Main();
}
#endif
