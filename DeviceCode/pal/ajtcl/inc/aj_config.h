#ifndef _AJ_CONFIG_H
#define _AJ_CONFIG_H

/**
 * @file aj_config.h
 * @defgroup aj_config Configuration
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
#include "aj_debug.h"

#ifdef __cplusplus
extern "C" {
#endif

/* Debug options */
#define _AJ_AUTH_DEBUG              0           //Enable for authentication debugging
#ifndef AJ_DEBUG_RESTRICT
#define AJ_DEBUG_RESTRICT AJ_DEBUG_WARN         //Debug output level                    (aj_debug.h)
#endif
#define AJ_DUMP_MSG_RAW             0           //set to see raw msg bytes
#define AJ_DUMP_BYTE_SIZE           16          //aj_debug.c

/* Network options */
#define AJ_CONNECT_LOCALHOST        0           //Enable to bypass discovery and connect locally
#define AJ_MAX_TIMERS               4           //maximum number of timers              (aj_helper.c)
#define AJ_ROUTING_NODE_BLACKLIST_SIZE 16       // maximum number of blacklisted routing nodes
#define AJ_ROUTING_NODE_RESPONSELIST_SIZE 3     // maximum number of routing node responses to track

/* Auth options */
#define AJ_NONCE_LEN                28          //Length of the nonce.
#define AJ_VERIFIER_LEN             12          //Length of the verifier string
#define AJ_PINX_MASTER_SECRET_LEN   24          //Length of the master secret PINX
#define AJ_MASTER_SECRET_LEN        48          //Length of the master secret - RFC 5246
#define AJ_ADHOC_LEN                16          //AD-HOC maximal passcode length        (aj_auth.h)
#define AJ_NAME_MAP_GUID_SIZE       4           //aj_guid.c
#define AJ_MAX_CREDS                40          //Max number of credentials that can store credentials (aj_creds.h)
#define AJ_LOCAL_GUID_NV_ID         1
#define AJ_CREDS_NV_ID_BEGIN (AJ_LOCAL_GUID_NV_ID + 1)
#define AJ_CREDS_NV_ID_END   (AJ_CREDS_NV_ID_BEGIN + AJ_MAX_CREDS)

/* Timeouts */
#define AJ_UNMARSHAL_TIMEOUT     (100 * 1000)      //unmarshal timeout                                (aj_helper.c + aj_msg.c)
#define AJ_CONNECT_TIMEOUT       (60 * 1000)       //connection timeout                               (aj_helper.c)
#define AJ_SELECTION_TIMEOUT     (5 * 1000)        //selection  timeout                               (aj_helper.c)
#define AJ_CONNECT_PAUSE         (10 * 1000)       //how long to pause between failed connects        (aj_helper.c)
#define AJ_DEFAULT_REPLY_TIMEOUT (1000 * 20)       //reply timeout                                    (aj_introspect.c)
#define AJ_MIN_BUS_LINK_TIMEOUT  (40)              //min link timeout for the bus                     (aj_link_timeout.c)
#define AJ_BUS_LINK_PING_TIMEOUT (5 * 1000)        //time period in which probe requests sb acked     (aj_link_timeout.c)
#define AJ_MAX_LINK_PING_PACKETS (3)               //max number of outstanding probe requests         (aj_link_timeout.c)
#define AJ_METHOD_TIMEOUT        (1000 * 3)        //timeout for method calls                         (aj_bus.c)
#define AJ_MAX_AUTH_TIME         (5 * 60 * 1000ul) //max time for incomplete authentication           (aj_peer.c)
#define AJ_AUTH_CALL_TIMEOUT     (2 * 60 * 1000ul) //long timeout for method calls w/ user input      (aj_peer.c)
#define AJ_CALL_TIMEOUT          (1000ul * 5)      //default timout for method calls                  (aj_peer.c)
#define AJ_UDP_CONNECT_TIMEOUT   5000              //UDP connection timeout                           (aj_connect.c)

/* Message identification related */

#if !defined(AJ_NUM_REPLY_CONTEXTS)
#define AJ_NUM_REPLY_CONTEXTS    (3)               //number of concurrent method calls     (aj_introspect.c)
#endif

#if !(defined(AJ_MAX_OBJECT_LISTS))
#define AJ_MAX_OBJECT_LISTS      (9)               //maximum number of object lists        (aj_introspect.c)
#endif

/* Crypto */
#define AJ_CCM_TRACE                0           //Enables fine-grained tracing for debugging new implementations.

#define _SO_REUSEPORT               0       //Linux target

/* About client Announcement buffer */
#define AJ_MAX_NUM_OF_OBJ_DESC      (32)           //number of object descriptions in an Announcement payload (aj_about.c)
#define AJ_MAX_NUM_OF_INTERFACES    (16)           //number of interfaces per object description in an Annoucement payload (aj_about.c)

/* Below sets the actual #define's based on values above */
#if _AJ_AUTH_DEBUG
#define AUTH_DEBUG
#endif
#if _SO_REUSEPORT
#define SO_REUSEPORT
#endif

#if HOST_IS_LITTLE_ENDIAN
#define HOST_ENDIANESS AJ_LITTLE_ENDIAN
#elif HOST_IS_BIG_ENDIAN
#define HOST_ENDIANESS AJ_BIG_ENDIAN
#endif

#ifdef __cplusplus
}
#endif
/**
 * @}
 */
#endif //AJ_CONFIG_H
