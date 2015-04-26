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
 * prior to first inclusion of aj_debug.h.
 * The corresponding flag dbgAJSVC is defined in ServicesCommon.h and implemented
 * in ServicesCommon.c.
 */
#define AJ_MODULE AJSVC
#include <aj_debug.h>

#include <alljoyn.h>

#include "ServicesCommon.h"
#include "PropertyStore.h"
#include "ServicesHandlers.h"
#ifdef CONFIG_SERVICE
#include <alljoyn/config/ConfigService.h>
#endif
#ifdef ONBOARDING_SERVICE
#include <alljoyn/onboarding/OnboardingService.h>
#include <alljoyn/onboarding/OnboardingManager.h>
#endif
#ifdef NOTIFICATION_SERVICE_CONSUMER
#include <alljoyn/notification/NotificationConsumer.h>
#endif
#ifdef NOTIFICATION_SERVICE_PRODUCER
#include <alljoyn/notification/NotificationProducer.h>
#endif
#ifdef CONTROLPANEL_SERVICE
#include <alljoyn/controlpanel/ControlPanelService.h>
#endif
#ifdef TIME_SERVICE_SERVER
#include <alljoyn/time/TimeServiceServer.h>
#endif
#ifdef TIME_SERVICE_CLIENT
#include <alljoyn/time/TimeServiceClient.h>
#endif

#include <aj_config.h>
#include <aj_link_timeout.h>

AJ_Status AJSVC_RoutingNodeConnect(AJ_BusAttachment* busAttachment, const char* routingNodeName, uint32_t connectTimeout, uint32_t connectPause, uint32_t busLinkTimeout, uint8_t* isConnected)
{
    AJ_Status status = AJ_OK;
    const char* busUniqueName;

    while (TRUE) {
#ifdef ONBOARDING_SERVICE
        status = AJOBS_EstablishWiFi();
        if (status != AJ_OK) {
            AJ_ErrPrintf(("Failed to establish WiFi connectivity with status=%s\n", AJ_StatusText(status)));
            AJ_Sleep(connectPause);
            if (isConnected != NULL) {
                *isConnected = FALSE;
            }
            return status;
        }
#endif
        AJ_InfoPrintf(("Attempting to connect to bus '%s'\n", routingNodeName));
        status = AJ_FindBusAndConnect(busAttachment, routingNodeName, connectTimeout);
        if (status != AJ_OK) {
            AJ_ErrPrintf(("Failed attempt to connect to bus, sleeping for %d seconds\n", connectPause / 1000));
            AJ_Sleep(connectPause);
#ifdef ONBOARDING_SERVICE
            if (status == AJ_ERR_DHCP || status == AJ_ERR_TIMEOUT) {
                status = AJOBS_SwitchToRetry();
                if (status != AJ_OK) {
                    AJ_ErrPrintf(("Failed to switch to Retry mode status=%s\n", AJ_StatusText(status)));
                }
            }
#endif
            continue;
        }
        busUniqueName = AJ_GetUniqueName(busAttachment);
        if (busUniqueName == NULL) {
            AJ_ErrPrintf(("Failed to GetUniqueName() from newly connected bus, retrying\n"));
            continue;
        }
        AJ_InfoPrintf(("Connected to Routing Node with BusUniqueName=%s\n", busUniqueName));
        break;
    }

    /* Configure timeout for the link to the Routing Node bus */
    AJ_SetBusLinkTimeout(busAttachment, busLinkTimeout);

    if (isConnected != NULL) {
        *isConnected = TRUE;
    }
    return status;
}

AJ_Status AJSVC_ConnectedHandler(AJ_BusAttachment* busAttachment)
{
    AJ_Status status = AJ_OK;

#ifdef CONFIG_SERVICE
    status = AJCFG_ConnectedHandler(busAttachment);
    if (status != AJ_OK) {
        goto ErrorExit;
    }
#endif
#ifdef ONBOARDING_SERVICE
    status = AJOBS_ConnectedHandler(busAttachment);
    if (status != AJ_OK) {
        goto ErrorExit;
    }
#endif
#ifdef NOTIFICATION_SERVICE_PRODUCER
    status = AJNS_Producer_ConnectedHandler(busAttachment);
    if (status != AJ_OK) {
        goto ErrorExit;
    }
#endif
#ifdef CONTROLPANEL_SERVICE
    status = AJCPS_ConnectedHandler(busAttachment);
    if (status != AJ_OK) {
        goto ErrorExit;
    }
#endif
#ifdef NOTIFICATION_SERVICE_CONSUMER
    status = AJNS_Consumer_ConnectedHandler(busAttachment);
    if (status != AJ_OK) {
        goto ErrorExit;
    }
#endif
#ifdef TIME_SERVICE_SERVER
    status = AJTS_Server_ConnectedHandler(busAttachment);
    if (status != AJ_OK) {
        goto ErrorExit;
    }
#endif
#ifdef TIME_SERVICE_CLIENT
    status = AJTS_Client_ConnectedHandler(busAttachment);
    if (status != AJ_OK) {
        goto ErrorExit;
    }
#endif

    return status;

ErrorExit:

    AJ_ErrPrintf(("Service ConnectedHandler returned an error %s\n", (AJ_StatusText(status))));
    return status;
}

uint8_t AJSVC_CheckSessionAccepted(uint16_t port, uint32_t sessionId, char* joiner)
{
    uint8_t session_accepted = FALSE;

#ifdef NOTIFICATION_SERVICE_PRODUCER
    session_accepted |= AJNS_Producer_CheckSessionAccepted(port, sessionId, joiner);
#endif

#ifdef CONTROLPANEL_SERVICE
    session_accepted |= AJCPS_CheckSessionAccepted(port, sessionId, joiner);
#endif

#ifdef TIME_SERVICE_SERVER
    session_accepted |= AJTS_CheckSessionAccepted(port, sessionId, joiner);
#endif

    return session_accepted;
}

static AJSVC_ServiceStatus SessionJoinedHandler(AJ_BusAttachment* busAttachment, uint32_t sessionId, uint32_t replySerialNum)
{
    AJSVC_ServiceStatus serviceStatus = AJSVC_SERVICE_STATUS_NOT_HANDLED;

#ifdef NOTIFICATION_SERVICE_CONSUMER
    if (serviceStatus == AJSVC_SERVICE_STATUS_NOT_HANDLED) {
        serviceStatus = AJNS_Consumer_SessionJoinedHandler(busAttachment, sessionId, replySerialNum);
    }
#endif
#ifdef TIME_SERVICE_CLIENT
    if (serviceStatus == AJSVC_SERVICE_STATUS_NOT_HANDLED) {
        serviceStatus = AJTS_Client_SessionJoinedHandler(busAttachment, sessionId, replySerialNum);
    }
#endif

    return serviceStatus;
}

static AJSVC_ServiceStatus SessionRejectedHandler(AJ_BusAttachment* busAttachment, uint32_t sessionId, uint32_t replySerialNum, uint32_t replyCode)
{
    AJSVC_ServiceStatus serviceStatus = AJSVC_SERVICE_STATUS_NOT_HANDLED;

#ifdef NOTIFICATION_SERVICE_CONSUMER
    if (serviceStatus == AJSVC_SERVICE_STATUS_NOT_HANDLED) {
        serviceStatus = AJNS_Consumer_SessionRejectedHandler(busAttachment, replySerialNum, replyCode);
    }
#endif
#ifdef TIME_SERVICE_CLIENT
    if (serviceStatus == AJSVC_SERVICE_STATUS_NOT_HANDLED) {
        serviceStatus = AJTS_Client_SessionRejectedHandler(busAttachment, replySerialNum, replyCode);
    }
#endif

    return serviceStatus;
}

static AJSVC_ServiceStatus SessionLostHandler(AJ_BusAttachment* busAttachment, uint32_t sessionId, uint32_t reason)
{
    AJSVC_ServiceStatus serviceStatus = AJSVC_SERVICE_STATUS_NOT_HANDLED;

#ifdef NOTIFICATION_SERVICE_CONSUMER
    if (serviceStatus == AJSVC_SERVICE_STATUS_NOT_HANDLED) {
        serviceStatus = AJNS_Consumer_SessionLostHandler(busAttachment, sessionId, reason);
    }
#endif
#ifdef TIME_SERVICE_CLIENT
    if (serviceStatus == AJSVC_SERVICE_STATUS_NOT_HANDLED) {
        serviceStatus = AJTS_Client_SessionLostHandler(busAttachment, sessionId, reason);
    }
#endif

    return serviceStatus;
}

AJSVC_ServiceStatus AJSVC_MessageProcessorAndDispatcher(AJ_BusAttachment* busAttachment, AJ_Message* msg, AJ_Status* status)
{
    AJSVC_ServiceStatus serviceStatus = AJSVC_SERVICE_STATUS_NOT_HANDLED;

    if (msg->msgId == AJ_REPLY_ID(AJ_METHOD_JOIN_SESSION)) {     // Process all incoming replies to join a session and pass session state change to all services
        uint32_t replyCode = 0;
        uint32_t sessionId = 0;
        uint8_t sessionJoined = FALSE;
        uint32_t joinSessionReplySerialNum = msg->replySerial;
        if (msg->hdr->msgType == AJ_MSG_ERROR) {
            AJ_ErrPrintf(("JoinSessionReply: AJ_METHOD_JOIN_SESSION: AJ_ERR_FAILURE\n"));
            *status = AJ_ERR_FAILURE;
        } else {
            *status = AJ_UnmarshalArgs(msg, "uu", &replyCode, &sessionId);
            if (*status != AJ_OK) {
                AJ_ErrPrintf(("JoinSessionReply: failed to unmarshal\n"));
            } else {
                if (replyCode == AJ_JOINSESSION_REPLY_SUCCESS) {
                    AJ_InfoPrintf(("JoinSessionReply: AJ_JOINSESSION_REPLY_SUCCESS with sessionId=%u and replySerial=%u\n", sessionId, joinSessionReplySerialNum));
                    sessionJoined = TRUE;
                } else {
                    AJ_ErrPrintf(("JoinSessionReply: AJ_ERR_FAILURE\n"));
                    *status = AJ_ERR_FAILURE;
                }
            }
        }
        if (sessionJoined) {
            serviceStatus = SessionJoinedHandler(busAttachment, sessionId, joinSessionReplySerialNum);
        } else {
            serviceStatus = SessionRejectedHandler(busAttachment, sessionId, joinSessionReplySerialNum, replyCode);
        }
        if (serviceStatus == AJSVC_SERVICE_STATUS_NOT_HANDLED) {
            AJ_ResetArgs(msg);
        }
    } else if (msg->msgId == AJ_SIGNAL_SESSION_LOST || msg->msgId == AJ_SIGNAL_SESSION_LOST_WITH_REASON) {     // Process all incoming LeaveSession replies and lost session signals and pass session state change to all services
        uint32_t sessionId = 0;
        uint32_t reason = 0;
        if (msg->msgId == AJ_SIGNAL_SESSION_LOST_WITH_REASON) {
            *status = AJ_UnmarshalArgs(msg, "uu", &sessionId, &reason);
        } else {
            *status = AJ_UnmarshalArgs(msg, "u", &sessionId);
        }
        if (*status != AJ_OK) {
            AJ_ErrPrintf(("JoinSessionReply: failed to marshal\n"));
        } else {
            AJ_InfoPrintf(("Session lost: sessionId = %u, reason = %u\n", sessionId, reason));
            serviceStatus = SessionLostHandler(busAttachment, sessionId, reason);
            if (serviceStatus == AJSVC_SERVICE_STATUS_NOT_HANDLED) {
                AJ_ResetArgs(msg);
            }
        }
    } else {
#ifdef CONFIG_SERVICE
        if (serviceStatus == AJSVC_SERVICE_STATUS_NOT_HANDLED) {
            serviceStatus = AJCFG_MessageProcessor(busAttachment, msg, status);
        }
#endif
#ifdef ONBOARDING_SERVICE
        if (serviceStatus == AJSVC_SERVICE_STATUS_NOT_HANDLED) {
            serviceStatus = AJOBS_MessageProcessor(busAttachment, msg, status);
        }
#endif
#ifdef NOTIFICATION_SERVICE_PRODUCER
        if (serviceStatus == AJSVC_SERVICE_STATUS_NOT_HANDLED) {
            serviceStatus = AJNS_Producer_MessageProcessor(busAttachment, msg, status);
        }
#endif
#ifdef NOTIFICATION_SERVICE_CONSUMER
        if (serviceStatus == AJSVC_SERVICE_STATUS_NOT_HANDLED) {
            serviceStatus = AJNS_Consumer_MessageProcessor(busAttachment, msg, status);
        }
#endif
#ifdef CONTROLPANEL_SERVICE
        if (serviceStatus == AJSVC_SERVICE_STATUS_NOT_HANDLED) {
            serviceStatus = AJCPS_MessageProcessor(busAttachment, msg, status);
        }
#endif
#ifdef TIME_SERVICE_SERVER
        if (serviceStatus == AJSVC_SERVICE_STATUS_NOT_HANDLED) {
            serviceStatus = AJTS_Server_MessageProcessor(busAttachment, msg, status);
        }
#endif
#ifdef TIME_SERVICE_CLIENT
        if (serviceStatus == AJSVC_SERVICE_STATUS_NOT_HANDLED) {
            serviceStatus = AJTS_Client_MessageProcessor(busAttachment, msg, status);
        }
#endif

    }
    return serviceStatus;
}

AJ_Status AJSVC_DisconnectHandler(AJ_BusAttachment* busAttachment)
{
    AJ_Status status = AJ_OK;

#ifdef CONFIG_SERVICE
    AJCFG_DisconnectHandler(busAttachment);
#endif
#ifdef ONBOARDING_SERVICE
    AJOBS_DisconnectHandler(busAttachment);
#endif
#ifdef NOTIFICATION_SERVICE_CONSUMER
    AJNS_Consumer_DisconnectHandler(busAttachment);
#endif
#ifdef NOTIFICATION_SERVICE_PRODUCER
    AJNS_Producer_DisconnectHandler(busAttachment);
#endif
#ifdef CONTROLPANEL_SERVICE
    AJCPS_DisconnectHandler(busAttachment);
#endif
#ifdef TIME_SERVICE_SERVER
    AJTS_Server_DisconnectHandler(busAttachment);
#endif
#ifdef TIME_SERVICE_CLIENT
    AJTS_Client_DisconnectHandler(busAttachment);
#endif

    return status;
}

AJ_Status AJSVC_RoutingNodeDisconnect(AJ_BusAttachment* busAttachment, uint8_t disconnectWiFi, uint32_t preDisconnectPause, uint32_t postDisconnectPause, uint8_t* isConnected)
{
    AJ_Status status = AJ_OK;

    AJ_ErrPrintf(("AllJoyn disconnect\n"));
    AJ_Sleep(preDisconnectPause); // Sleep a little to let any pending requests to Routing Node to be sent
    AJ_Disconnect(busAttachment);
#ifdef ONBOARDING_SERVICE
    if (disconnectWiFi) {
        status = AJOBS_DisconnectWiFi();
    }
#endif
    AJ_Sleep(postDisconnectPause); // Sleep a little while before trying to reconnect

    if (isConnected != NULL) {
        *isConnected = FALSE;
    }
    return status;
}
