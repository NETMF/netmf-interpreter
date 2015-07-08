#ifndef _AJ_AUTH_LISTENER_H
#define _AJ_AUTH_LISTENER_H

/**
 * @file aj_auth_listener.h
 * @defgroup aj_auth_listener Authentication Listener
 * @{
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
#include "aj_net.h"
#include "aj_status.h"
#include "aj_util.h"

#ifdef __cplusplus
extern "C" {
#endif

/*
 * Command for auth listener callback
 */
#define AJ_CRED_PRV_KEY    0x0001 /**< private key */
#define AJ_CRED_PUB_KEY    0x0002 /**< public key */
#define AJ_CRED_CERT_CHAIN 0x0003 /**< certificate chain */

#define AJ_CRED_REQUEST    0
#define AJ_CRED_RESPONSE   1

/*
 * Type for a Credential entry for the auth listener callback
 */
typedef struct _AJ_Credential {
    uint8_t direction;   /**< request or response */
    uint32_t expiration; /**< auth listener to set key expiration value */
    uint8_t* data;       /**< data to or from the auth listener */
    size_t len;          /**< length of data */
} AJ_Credential;

#ifdef __cplusplus
}
#endif
/**
 * @}
 */
#endif
