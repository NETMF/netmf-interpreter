/**
 * @file
 */
/******************************************************************************
 * Copyright (c) 2013-2014, AllSeen Alliance. All rights reserved.
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
#define AJ_MODULE LINK_TIMEOUT

#include <aj_link_timeout.h>
#include "aj_debug.h"
#include "aj_config.h"
/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgLINK_TIMEOUT = 0;
#endif

typedef struct _AJ_BusLinkWatcher {
    uint8_t numOfPingTimedOut;           /**< Number of probe request packets already sent but unacked */
    uint8_t linkTimerInited;             /**< If timer linkTimer is inited */
    uint8_t pingTimerInited;             /**< If timer pingTimer is inited */
    AJ_Time linkTimer;                   /**< Timer for tracking activities over the link to the daemon bus */
    AJ_Time pingTimer;                   /**< Timer for tracking probe request packets */
} AJ_BusLinkWatcher;

static uint32_t busLinkTimeout;          /**< Timeout value for the link to the daemon bus */
static AJ_BusLinkWatcher busLinkWatcher; /**< Data structure that maintains information for tracking the link to the daemon bus */

/**
 * Forward declaration
 */
AJ_Status AJ_SendLinkProbeReq(AJ_BusAttachment* bus);

AJ_Status AJ_SetBusLinkTimeout(AJ_BusAttachment* bus, uint32_t timeout)
{
    if (!timeout) {
        return AJ_ERR_FAILURE;
    }
    timeout = (timeout > AJ_MIN_BUS_LINK_TIMEOUT) ? timeout : AJ_MIN_BUS_LINK_TIMEOUT;
    busLinkTimeout = timeout * 1000;
    return AJ_OK;
}
AJ_Status AJ_SetIdleTimeouts(AJ_BusAttachment* bus, uint32_t idleTo, uint32_t probeTo)
{
    AJ_Status status;
    AJ_Message msg;

    AJ_InfoPrintf(("AJ_SetIdleTimeouts(bus=0x%p, idleTo=%d, probeTo=%d)\n", bus, idleTo, probeTo));

    status = AJ_MarshalMethodCall(bus, &msg, AJ_METHOD_BUS_SET_IDLE_TIMEOUTS, AJ_BusDestination, 0, 0, AJ_METHOD_TIMEOUT);
    if (status == AJ_OK) {
        status = AJ_MarshalArgs(&msg, "uu", idleTo, probeTo);
    }
    if (status == AJ_OK) {
        status = AJ_DeliverMsg(&msg);
    }
    return status;
}

void AJ_NotifyLinkActive()
{
    memset(&busLinkWatcher, 0, sizeof(AJ_BusLinkWatcher));
}

AJ_Status AJ_BusLinkStateProc(AJ_BusAttachment* bus)
{
    AJ_Status status = AJ_OK;
    if (busLinkTimeout) {
        if (!busLinkWatcher.linkTimerInited) {
            busLinkWatcher.linkTimerInited = TRUE;
            AJ_InitTimer(&(busLinkWatcher.linkTimer));
        } else {
            uint32_t eclipse = AJ_GetElapsedTime(&(busLinkWatcher.linkTimer), TRUE);
            if (eclipse >= busLinkTimeout) {
                if (!busLinkWatcher.pingTimerInited) {
                    busLinkWatcher.pingTimerInited = TRUE;
                    AJ_InitTimer(&(busLinkWatcher.pingTimer));
                    if (AJ_OK != AJ_SendLinkProbeReq(bus)) {
                        AJ_ErrPrintf(("AJ_BusLinkStateProc(): AJ_SendLinkProbeReq() failure"));
                    }
                } else {
                    eclipse = AJ_GetElapsedTime(&(busLinkWatcher.pingTimer), TRUE);
                    if (eclipse >=  AJ_BUS_LINK_PING_TIMEOUT) {
                        if (++busLinkWatcher.numOfPingTimedOut < AJ_MAX_LINK_PING_PACKETS) {
                            AJ_InitTimer(&(busLinkWatcher.pingTimer));
                            if (AJ_OK != AJ_SendLinkProbeReq(bus)) {
                                AJ_ErrPrintf(("AJ_BusLinkStateProc(): AJ_SendLinkProbeReq() failure"));
                            }
                        } else {
                            AJ_ErrPrintf(("AJ_BusLinkStateProc(): AJ_ERR_LINK_TIMEOUT"));
                            status = AJ_ERR_LINK_TIMEOUT;
                            // stop sending probe messages until next link timeout event
                            memset(&busLinkWatcher, 0, sizeof(AJ_BusLinkWatcher));
                        }
                    }
                }
            }
        }
    }
    return status;
}

AJ_Status AJ_SendLinkProbeReq(AJ_BusAttachment* bus)
{
    AJ_Status status;
    AJ_Message msg;

    status = AJ_MarshalSignal(bus, &msg, AJ_SIGNAL_PROBE_REQ, AJ_BusDestination, 0, 0, 0);
    if (status == AJ_OK) {
        status = AJ_DeliverMsg(&msg);
    }
    return status;
}

