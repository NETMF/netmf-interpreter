#ifndef _AJ_KEYEXCHANGE_H
#define _AJ_KEYEXCHANGE_H

/**
 * @file aj_keyexchange.h
 * @defgroup aj_keyexchange Implementation of Key Exchange mechanisms
 * @{
 */
/******************************************************************************
 * Copyright (c) 2014, AllSeen Alliance. All rights reserved.
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
#include "aj_peer.h"
#include "aj_crypto_sha2.h"

#ifdef __cplusplus
extern "C" {
#endif

/*
 * Key Exchange Types
 */
#define AJ_KE_ECDHE   0x1

typedef AJ_Status (*AJ_KEInit)(AJ_SHA256_Context* hash);
typedef AJ_Status (*AJ_KEMarshal)(AJ_Message* msg);
typedef AJ_Status (*AJ_KEUnmarshal)(AJ_Message* msg, uint8_t** secret, size_t* secretlen);

typedef struct _AJ_KeyExchange {
    AJ_KEInit Init;
    AJ_KEMarshal Marshal;
    AJ_KEUnmarshal Unmarshal;
} AJ_KeyExchange;

/**
 * ECDHE key exchange
 */
extern AJ_KeyExchange AJ_KeyExchangeECDHE;

#ifdef __cplusplus
}
#endif
/**
 * @}
 */
#endif
