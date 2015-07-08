/**
 * @file HTC layer function declarations
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

#ifndef AJ_WSL_HTC_H_
#define AJ_WSL_HTC_H_

#include "aj_target.h"
#include "aj_wsl_target.h"
#include "aj_wsl_spi_constants.h"
#include "aj_buf.h"
#include "aj_debug.h"

#ifdef __cplusplus
extern "C" {
#endif

#pragma pack(push, 1)


void AJ_WSL_HTC_ModuleInit(void);
uint8_t AJ_WSL_IsDriverStarted(void);
/*
 *  Endpoints can be in any of these states
 */
typedef enum _AJ_WSL_HTC_STATE {
    AJ_WSL_HTC_UNINITIALIZED,
    AJ_WSL_HTC_UNINITIALIZED_RECV_READY,
    AJ_WSL_HTC_UNINITIALIZED_SENT_CRED_REQ,
    AJ_WSL_HTC_INITIALIZED,
    AJ_WSL_HTC_NO_CREDS,     /**< no credits available for this endpoint */
    AJ_WSL_HTC_CREDS         /**< okay to send packets to the target */
} AJ_WSL_HTC_STATE;

typedef struct _WSL_HTC_EP {
    uint8_t endpointId;
    uint16_t serviceId;
    uint16_t txCredits;
    AJ_WSL_HTC_STATE state;
} WSL_HTC_EP;

typedef struct _WSL_HTC_CONTEXT {
    uint16_t creditCount;
    uint16_t creditSize;
    uint8_t maxEndpoints;
    uint8_t HTCVersion;
    uint8_t maxMessagesPerBundle;
    WSL_HTC_EP endpoints[AJ_WSL_HTC_ENDPOINT_COUNT_MAX];
    uint8_t started;
} AJ_WSL_HTC_CONTEXT;




/*
 * HTC header flags
 */
#define AJ_WSL_HTC_NEED_CREDIT_UPDATE    (1 << 0)
#define AJ_WSL_HTC_RECV_TRAILER_PRESENT  (1 << 1)

void AJ_WSL_HTC_ProcessInterruptCause(void);
void AJ_WSL_HTC_ProcessIncoming(void);

#pragma pack(pop)

#ifdef __cplusplus
}
#endif

#endif /* AJ_WSL_HTC_H_ */
