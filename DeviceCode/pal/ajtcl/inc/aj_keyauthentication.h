#ifndef _AJ_KEYAUTHENTICATION_H
#define _AJ_KEYAUTHENTICATION_H

/**
 * @file aj_keyauthentication.h
 * @defgroup aj_keyauthentication Implementation of Key Authentication mechanisms
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

typedef AJ_Status (*AJ_KAInit)(AJ_AuthListenerFunc authlistener, const uint8_t* mastersecret, size_t mastersecretlen, AJ_SHA256_Context* hash);
typedef AJ_Status (*AJ_KAMarshal)(AJ_Message* msg, uint8_t role);
typedef AJ_Status (*AJ_KAUnmarshal)(AJ_Message* msg, uint8_t role);
typedef AJ_Status (*AJ_KAFinal)(uint32_t* expiration);

typedef struct _AJ_KeyAuthentication {
    AJ_KAInit Init;
    AJ_KAMarshal Marshal;
    AJ_KAUnmarshal Unmarshal;
    AJ_KAFinal Final;
} AJ_KeyAuthentication;

#define AUTH_ECDSA
#define AUTH_PSK
#define AUTH_NULL

#define AUTH_CLIENT 0
#define AUTH_SERVER 1

#define DSA_PRV_KEY_ID  1
#define DSA_PUB_KEY_ID  2

#ifdef AUTH_ECDSA
extern AJ_KeyAuthentication AJ_KeyAuthenticationECDSA;
#endif
#ifdef AUTH_PSK
extern AJ_KeyAuthentication AJ_KeyAuthenticationPSK;
void AJ_PSK_SetPwdCallback(AJ_AuthPwdFunc pwdcallback);
#endif
#ifdef AUTH_NULL
extern AJ_KeyAuthentication AJ_KeyAuthenticationNULL;
#endif

#ifdef __cplusplus
}
#endif
/**
 * @}
 */
#endif
