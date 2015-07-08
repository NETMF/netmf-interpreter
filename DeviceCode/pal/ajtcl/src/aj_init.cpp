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
#define AJ_MODULE INIT

#include "aj_target.h"
#include "aj_init.h"
#include "aj_nvram.h"
#include "aj_creds.h"
#include "aj_guid.h"
#include "aj_crypto.h"
#include "aj_debug.h"
#include "aj_connect.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgINIT = 0;
#endif

static uint8_t initialized = FALSE;

void AJ_Initialize(void)
{
    AJ_GUID localGuid;
    if (!initialized) {
        initialized = TRUE;
        AJ_NVRAM_Init();
        /*
         * This will seed the random number generator
         */
        AJ_RandBytes(NULL, 0);
        /*
         * This will initialize credentials if needed
         */
        AJ_GetLocalGUID(&localGuid);

        /*
         * Clear the Routing Node black list
         */
        AJ_InitRoutingNodeBlacklist();

        AJ_InitRoutingNodeResponselist();
    }
}
