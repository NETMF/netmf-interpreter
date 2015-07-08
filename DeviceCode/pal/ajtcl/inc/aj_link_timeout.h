#ifndef _AJ_LINK_TIMEOUT_H
#define _AJ_LINK_TIMEOUT_H

/**
 * @file aj_link_timeout.h
 * @defgroup aj_link_timeout Daemon Connection Keepalive Support
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
#include <aj_util.h>
#include <alljoyn.h>

#ifdef __cplusplus
extern "C" {
#endif

/**
 * Enable link timeout for the connection between the application and the daemon bus. If there are
 * no link activities during that period, at most 3 probe packets are sent to the daemon bus with
 * an interval of 5 seconds. If none of the probe packets are acknowledged by the daemon bus due
 * to any resons (eg., WIFI is off), AJ_BusLinkStateProc will return AJ_ERR_LINK_TIMEOUT
 * so that the application has to re-connect to the daemon bus.
 *
 * @param bus        The bus attachment to which the app is connected to
 * @param timeout    The time unit is second. The minimum value is 40.
 *
 * @return  Return AJ_Status
 *          - AJ_OK if the bus link timeout is set successfully
 *          - AJ_ERR_FAILURE if timeout is 0
 */
AJ_EXPORT
AJ_Status AJ_SetBusLinkTimeout(AJ_BusAttachment* bus, uint32_t timeout);

/**
 * Call to notify that the bus link is currently active. This is implicitly implied upon receiving packets from the bus.
 */
AJ_EXPORT
void AJ_NotifyLinkActive();

/**
 * Call to do the work of bus link maintainance.
 *
 * @return  Return AJ_Status
 *          - AJ_ERR_LINK_TIMEOUT if the bus link is considered as dead. The application has to re-connect to the daemon bus.
 *          - AJ_OK otherwise
 */
AJ_EXPORT
AJ_Status AJ_BusLinkStateProc(AJ_BusAttachment* bus);

/**
 * Set the idle timeouts from the Routing node to the TCL.
 *
 * @param bus        The bus attachment to which the app is connected to
 * @param idleTo     Requested Idle Timeout for the link. i.e. time after which the Routing node
 *                   must send a DBus ping to Leaf node in case of inactivity.
 *                   Use 0 to leave unchanged.
 * @param probeTo    Requested Probe timeout. The time from the Routing node sending the DBus
 *                   ping to the expected response.
 *                   Use 0 to leave unchanged.
 * @return  Return AJ_Status
 *         - AJ_OK if the request was sent
 *         - An error status otherwise
 */
AJ_EXPORT
AJ_Status AJ_SetIdleTimeouts(AJ_BusAttachment* bus, uint32_t idleTo, uint32_t probeTo);

#ifdef __cplusplus
}
#endif
/**
 * @}
 */
#endif
