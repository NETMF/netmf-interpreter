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
#define AJ_MODULE BUS

#include "aj_target.h"
#include "aj_debug.h"
#include "aj_msg.h"
#include "aj_bufio.h"
#include "aj_bus.h"
#include "aj_util.h"
#include "aj_creds.h"
#include "aj_std.h"
#include "aj_introspect.h"
#include "aj_peer.h"
#include "aj_config.h"
#include "aj_about.h"
#include "aj_authentication.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgBUS = 0;
#endif

const char* AJ_GetUniqueName(AJ_BusAttachment* bus)
{
    return (*bus->uniqueName) ? bus->uniqueName : NULL;
}

AJ_Status AJ_BusRequestName(AJ_BusAttachment* bus, const char* name, uint32_t flags)
{
    AJ_Status status;
    AJ_Message msg;

    AJ_InfoPrintf(("AJ_BusRequestName(bus=0x%p, name=\"%s\", flags=0x%x)\n", bus, name, flags));


    status = AJ_MarshalMethodCall(bus, &msg, AJ_METHOD_REQUEST_NAME, AJ_BusDestination, 0, 0, AJ_METHOD_TIMEOUT);
    if (status == AJ_OK) {
        status = AJ_MarshalArgs(&msg, "su", name, flags);
    }
    if (status == AJ_OK) {
        status = AJ_DeliverMsg(&msg);
    }
    return status;
}

AJ_Status AJ_BusReleaseName(AJ_BusAttachment* bus, const char* name)
{
    AJ_Status status;
    AJ_Message msg;

    AJ_InfoPrintf(("AJ_BusReleaseName(bus=0x%p, name=\"%s\")\n", bus, name));

    status = AJ_MarshalMethodCall(bus, &msg, AJ_METHOD_RELEASE_NAME, AJ_BusDestination, 0, 0, AJ_METHOD_TIMEOUT);
    if (status == AJ_OK) {
        status = AJ_MarshalArgs(&msg, "s", name);
    }
    if (status == AJ_OK) {
        status = AJ_DeliverMsg(&msg);
    }
    return status;
}

AJ_Status AJ_BusAdvertiseName(AJ_BusAttachment* bus, const char* name, uint16_t transportMask, uint8_t op, uint8_t flags)
{
    AJ_Status status;
    AJ_Message msg;
    uint32_t msgId = (op == AJ_BUS_START_ADVERTISING) ? AJ_METHOD_ADVERTISE_NAME : AJ_METHOD_CANCEL_ADVERTISE;

    AJ_InfoPrintf(("AJ_BusAdvertiseName(bus=0x%p, name=\"%s\", transportMask=0x%x, op=%d.)\n", bus, name, transportMask, op));

    status = AJ_MarshalMethodCall(bus, &msg, msgId, AJ_BusDestination, 0, flags, AJ_METHOD_TIMEOUT);
    if (status == AJ_OK) {
        status = AJ_MarshalArgs(&msg, "sq", name, transportMask);
    }
    if (status == AJ_OK) {
        status = AJ_DeliverMsg(&msg);
    }
    return status;
}

AJ_Status AJ_BusFindAdvertisedName(AJ_BusAttachment* bus, const char* namePrefix, uint8_t op)
{
    AJ_Status status;
    AJ_Message msg;
    uint32_t msgId = (op == AJ_BUS_START_FINDING) ? AJ_METHOD_FIND_NAME : AJ_METHOD_CANCEL_FIND_NAME;

    AJ_InfoPrintf(("AJ_BusFindAdvertiseName(bus=0x%p, namePrefix=\"%s\", op=%d.)\n", bus, namePrefix, op));

    status = AJ_MarshalMethodCall(bus, &msg, msgId, AJ_BusDestination, 0, 0, AJ_METHOD_TIMEOUT);
    if (status == AJ_OK) {
        status = AJ_MarshalArgs(&msg, "s", namePrefix);
    }
    if (status == AJ_OK) {
        status = AJ_DeliverMsg(&msg);
    }
    return status;
}

AJ_Status AJ_BusFindAdvertisedNameByTransport(AJ_BusAttachment* bus, const char* namePrefix, uint16_t transport, uint8_t op)
{
    AJ_Status status;
    AJ_Message msg;
    uint32_t msgId = (op == AJ_BUS_START_FINDING) ? AJ_METHOD_FIND_NAME_BY_TRANSPORT : AJ_METHOD_CANCEL_FIND_NAME_BY_TRANSPORT;

    AJ_InfoPrintf(("AJ_BusFindAdvertiseNameByTransport(bus=0x%p, namePrefix=\"%s\", transport=%d., op=%d.)\n", bus, namePrefix, transport, op));

    status = AJ_MarshalMethodCall(bus, &msg, msgId, AJ_BusDestination, 0, 0, AJ_METHOD_TIMEOUT);
    if (status == AJ_OK) {
        status = AJ_MarshalArgs(&msg, "sq", namePrefix, transport);
    }
    if (status == AJ_OK) {
        status = AJ_DeliverMsg(&msg);
    }
    return status;
}

static AJ_Status MarshalSessionOpts(AJ_Message* msg, const AJ_SessionOpts* opts)
{
    AJ_Arg dictionary;

    AJ_MarshalContainer(msg, &dictionary, AJ_ARG_ARRAY);

    AJ_MarshalArgs(msg, "{sv}", "traf",  "y", opts->traffic);
    AJ_MarshalArgs(msg, "{sv}", "multi", "b", opts->isMultipoint);
    AJ_MarshalArgs(msg, "{sv}", "prox",  "y", opts->proximity);
    AJ_MarshalArgs(msg, "{sv}", "trans", "q", opts->transports);

    AJ_MarshalCloseContainer(msg, &dictionary);

    return AJ_OK;
}

/*
 * Default session options
 */
static const AJ_SessionOpts defaultSessionOpts = {
    AJ_SESSION_TRAFFIC_MESSAGES,
    AJ_SESSION_PROXIMITY_ANY,
    AJ_TRANSPORT_ANY,
    FALSE
};

AJ_Status AJ_BusBindSessionPort(AJ_BusAttachment* bus, uint16_t port, const AJ_SessionOpts* opts, uint8_t flags)
{
    AJ_Status status;
    AJ_Message msg;

    AJ_InfoPrintf(("AJ_BusBindSessionPort(bus=0x%p, port=%d., opts=0x%p)\n", bus, port, opts));

    if (!opts) {
        opts = &defaultSessionOpts;
    }
    status = AJ_MarshalMethodCall(bus, &msg, AJ_METHOD_BIND_SESSION_PORT, AJ_BusDestination, 0, flags, AJ_METHOD_TIMEOUT);
    if (status == AJ_OK) {
        AJ_MarshalArgs(&msg, "q", port);
        status = MarshalSessionOpts(&msg, opts);
    }
    if (status == AJ_OK) {
        status = AJ_DeliverMsg(&msg);
    }
    return status;
}

AJ_Status AJ_BusUnbindSession(AJ_BusAttachment* bus, uint16_t port)
{
    AJ_Status status;
    AJ_Message msg;

    AJ_InfoPrintf(("AJ_BusUnbindSession(bus=0x%p, port=%d.)\n", bus, port));

    status = AJ_MarshalMethodCall(bus, &msg, AJ_METHOD_UNBIND_SESSION, AJ_BusDestination, 0, 0, AJ_METHOD_TIMEOUT);
    if (status == AJ_OK) {
        AJ_MarshalArgs(&msg, "q", port);
    }
    if (status == AJ_OK) {
        status = AJ_DeliverMsg(&msg);
    }
    return status;
}

AJ_Status AJ_BusCancelSessionless(AJ_BusAttachment* bus, uint32_t serialNum)
{
    AJ_Status status;
    AJ_Message msg;

    AJ_InfoPrintf(("AJ_BusCancelSessionless(bus=0x%p, serialNum=%d.)\n", bus, serialNum));

    status = AJ_MarshalMethodCall(bus, &msg, AJ_METHOD_CANCEL_SESSIONLESS, AJ_BusDestination, 0, 0, AJ_METHOD_TIMEOUT);
    if (status == AJ_OK) {
        AJ_MarshalArgs(&msg, "u", serialNum);
    }
    if (status == AJ_OK) {
        status = AJ_DeliverMsg(&msg);
    }
    return status;
}

AJ_Status AJ_BusJoinSession(AJ_BusAttachment* bus, const char* sessionHost, uint16_t port, const AJ_SessionOpts* opts)
{
    AJ_Status status;
    AJ_Message msg;

    AJ_InfoPrintf(("AJ_BusJoinSession(bus=0x%p, sessionHost=\"%s\", port=%d., opts=0x%p)\n", bus, sessionHost, port, opts));

    if (!opts) {
        opts = &defaultSessionOpts;
    }
    status = AJ_MarshalMethodCall(bus, &msg, AJ_METHOD_JOIN_SESSION, AJ_BusDestination, 0, 0, AJ_METHOD_TIMEOUT);
    if (status == AJ_OK) {
        status = AJ_MarshalArgs(&msg, "sq", sessionHost, port);

        if (status == AJ_OK) {
            status = MarshalSessionOpts(&msg, opts);
        }
    }
    if (status == AJ_OK) {
        status = AJ_DeliverMsg(&msg);
    }
    return status;
}

AJ_Status AJ_BusLeaveSession(AJ_BusAttachment* bus, uint32_t sessionId)
{
    AJ_Status status;
    AJ_Message msg;

    AJ_InfoPrintf(("AJ_BusLeaveSession(bus=0x%p, sessionId=%d.)\n", bus, sessionId));

    status = AJ_MarshalMethodCall(bus, &msg, AJ_METHOD_LEAVE_SESSION, AJ_BusDestination, 0, 0, AJ_METHOD_TIMEOUT);
    if (status == AJ_OK) {
        status = AJ_MarshalArgs(&msg, "u", sessionId);
    }
    if (status == AJ_OK) {
        status = AJ_DeliverMsg(&msg);
    }
    return status;
}

AJ_Status AJ_BusSetLinkTimeout(AJ_BusAttachment* bus, uint32_t sessionId, uint32_t linkTimeout)
{
    AJ_Status status;
    AJ_Message msg;

    AJ_InfoPrintf(("AJ_BusSetLinkTimeout(bus=0x%p, sessionId=%d., linkTimeout=%d.)\n", bus, sessionId, linkTimeout));

    status = AJ_MarshalMethodCall(bus, &msg, AJ_METHOD_SET_LINK_TIMEOUT, AJ_BusDestination, 0, 0, AJ_METHOD_TIMEOUT);
    if (status == AJ_OK) {
        (void)AJ_MarshalArgs(&msg, "u", sessionId);
        (void)AJ_MarshalArgs(&msg, "u", linkTimeout);
        status = AJ_DeliverMsg(&msg);
    }
    return status;
}

AJ_Status AJ_BusSetSignalRule(AJ_BusAttachment* bus, const char* ruleString, uint8_t rule)
{
    return AJ_BusSetSignalRuleFlags(bus, ruleString, rule, 0);
}

AJ_Status AJ_BusSetSignalRuleSerial(AJ_BusAttachment* bus, const char* ruleString, uint8_t rule, uint8_t flags, uint32_t* serialNum)
{
    AJ_Status status;
    AJ_Message msg;
    uint32_t msgId = (rule == AJ_BUS_SIGNAL_ALLOW) ? AJ_METHOD_ADD_MATCH : AJ_METHOD_REMOVE_MATCH;

    AJ_InfoPrintf(("AJ_BusSetSignalRuleSerial(bus=0x%p, ruleString=\"%s\", rule=%d.)\n", bus, ruleString, rule));

    status = AJ_MarshalMethodCall(bus, &msg, msgId, AJ_DBusDestination, 0, flags, AJ_METHOD_TIMEOUT);
    if (status == AJ_OK) {
        uint32_t sz = 0;
        uint8_t nul = 0;
        if (serialNum) {
            *serialNum = msg.hdr->serialNum;
        }
        sz = (uint32_t)strlen(ruleString);
        status = AJ_DeliverMsgPartial(&msg, sz + 5);
        AJ_MarshalRaw(&msg, &sz, 4);
        AJ_MarshalRaw(&msg, ruleString, strlen(ruleString));
        AJ_MarshalRaw(&msg, &nul, 1);
    }
    if (status == AJ_OK) {
        status = AJ_DeliverMsg(&msg);
    }
    return status;
}

AJ_Status AJ_BusAddSignalRule(AJ_BusAttachment* bus, const char* signalName, const char* interfaceName, uint8_t rule)
{
    AJ_Status status;
    AJ_Message msg;
    const char* str[5];
    uint32_t msgId = (rule == AJ_BUS_SIGNAL_ALLOW) ? AJ_METHOD_ADD_MATCH : AJ_METHOD_REMOVE_MATCH;

    AJ_InfoPrintf(("AJ_BusAddSignalRule(bus=0x%p, signalName=\"%s\", interfaceName=\"%s\", rule=%d.)\n", bus, signalName, interfaceName, rule));

    str[0] = "type='signal',member='";
    str[1] = signalName;
    str[2] = "'interface='";
    str[3] = interfaceName;
    str[4] = "'";

    status = AJ_MarshalMethodCall(bus, &msg, msgId, AJ_DBusDestination, 0, 0, AJ_METHOD_TIMEOUT);
    if (status == AJ_OK) {
        size_t i;
        uint32_t sz = 0;
        uint8_t nul = 0;
        for (i = 0; i < ArraySize(str); ++i) {
            sz += (uint32_t)strlen(str[i]);
        }
        status = AJ_DeliverMsgPartial(&msg, sz + 5);
        AJ_MarshalRaw(&msg, &sz, 4);
        for (i = 0; i < ArraySize(str); ++i) {
            AJ_MarshalRaw(&msg, str[i], strlen(str[i]));
        }
        AJ_MarshalRaw(&msg, &nul, 1);
    }
    if (status == AJ_OK) {
        status = AJ_DeliverMsg(&msg);
    }
    return status;
}

AJ_Status AJ_BusSetSignalRuleFlags(AJ_BusAttachment* bus, const char* ruleString, uint8_t rule, uint8_t flags)
{
    return AJ_BusSetSignalRuleSerial(bus, ruleString, rule, flags, NULL);
}

AJ_Status AJ_BusReplyAcceptSession(AJ_Message* msg, uint32_t accept)
{
    AJ_Message reply;

    AJ_InfoPrintf(("AJ_BusReplyAcceptSession(msg=0x%p, accept=%d.)\n", msg, accept));

    AJ_MarshalReplyMsg(msg, &reply);
    AJ_MarshalArgs(&reply, "b", accept);
    return AJ_DeliverMsg(&reply);
}

static AJ_Status HandleGetMachineId(AJ_Message* msg, AJ_Message* reply)
{
    char guidStr[33];
    AJ_GUID localGuid;

    AJ_InfoPrintf(("HandleGetMachineId(msg=0x%p, reply=0x%p)\n", msg, reply));

    AJ_MarshalReplyMsg(msg, reply);
    AJ_GetLocalGUID(&localGuid);
    AJ_GUID_ToString(&localGuid, guidStr, sizeof(guidStr));
    return AJ_MarshalArgs(reply, "s", guidStr);
}

AJ_Status AJ_BusRemoveSessionMember(AJ_BusAttachment* bus, uint32_t sessionId, const char* member)
{
    AJ_Status status;
    AJ_Message msg;

    AJ_InfoPrintf(("AJ_BusRemoveSessionMember(bus=0x%p, sessionId=%d, member=%s.)\n", bus, sessionId, member));
    status = AJ_MarshalMethodCall(bus, &msg, AJ_METHOD_REMOVE_SESSION_MEMBER, AJ_BusDestination, 0, 0, AJ_METHOD_TIMEOUT);
    if (status == AJ_OK) {
        AJ_MarshalArgs(&msg, "us", sessionId, member);
    }
    if (status == AJ_OK) {
        status = AJ_DeliverMsg(&msg);
    }
    return status;

}

AJ_Status AJ_BusPing(AJ_BusAttachment* bus, const char* name, uint32_t timeout)
{
    AJ_Status status;
    AJ_Message msg;

    AJ_InfoPrintf(("AJ_BusPing(bus=0x%p, name=%s, timeout=%d)\n", bus, name, timeout));
    status = AJ_MarshalMethodCall(bus, &msg, AJ_METHOD_BUS_PING, AJ_BusDestination, 0, 0, AJ_METHOD_TIMEOUT);
    if (status == AJ_OK) {
        AJ_MarshalArgs(&msg, "su", name, timeout);
    }
    if (status == AJ_OK) {
        status = AJ_DeliverMsg(&msg);
    }
    return status;

}

AJ_Status AJ_BusHandleBusMessage(AJ_Message* msg)
{
    AJ_Status status = AJ_OK;
    AJ_BusAttachment* bus = msg->bus;
    char* languageTag;
    AJ_Message reply;

    AJ_InfoPrintf(("AJ_BusHandleBusMessage(msg=0x%p)\n", msg));
    memset(&reply, 0, sizeof(AJ_Message));
    /*
     * Check we actually have a message to handle
     */
    if (!msg->hdr) {
        return AJ_OK;
    }

    switch (msg->msgId) {
    case AJ_METHOD_PING:
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): AJ_METHOD_PING\n"));
        status = AJ_MarshalReplyMsg(msg, &reply);
        break;

    case AJ_METHOD_GET_MACHINE_ID:
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): AJ_METHOD_GET_MACHINE_ID\n"));
        status = HandleGetMachineId(msg, &reply);
        break;

    case AJ_METHOD_INTROSPECT:
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): AJ_METHOD_INTROSPECT\n"));
        status = AJ_HandleIntrospectRequest(msg, &reply, NULL);
        break;

    case AJ_METHOD_GET_DESCRIPTION_LANG:
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): AJ_METHOD_GET_DESCRIPTION_LANG\n"));
        status = AJ_HandleGetDescriptionLanguages(msg, &reply);
        break;

    case AJ_METHOD_INTROSPECT_WITH_DESC:
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): AJ_METHOD_INTROSPECT_WITH_DESC\n"));
        AJ_UnmarshalArgs(msg, "s", &languageTag);
        status = AJ_HandleIntrospectRequest(msg, &reply, languageTag);
        break;

    case AJ_METHOD_EXCHANGE_GUIDS:
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): AJ_METHOD_EXCHANGE_GUIDS\n"));
        status = AJ_PeerHandleExchangeGUIDs(msg, &reply);
        break;

    case AJ_METHOD_GEN_SESSION_KEY:
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): AJ_METHOD_GEN_SESSION_KEY\n"));
        status = AJ_PeerHandleGenSessionKey(msg, &reply);
        break;

    case AJ_METHOD_EXCHANGE_GROUP_KEYS:
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): AJ_METHOD_EXCHANGE_GROUP_KEYS\n"));
        status = AJ_PeerHandleExchangeGroupKeys(msg, &reply);
        break;

    case AJ_METHOD_EXCHANGE_SUITES:
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): AJ_METHOD_EXCHANGE_SUITES\n"));
        status = AJ_PeerHandleExchangeSuites(msg, &reply);
        break;

    case AJ_METHOD_KEY_EXCHANGE:
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): AJ_METHOD_KEY_EXCHANGE\n"));
        status = AJ_PeerHandleKeyExchange(msg, &reply);
        break;

    case AJ_METHOD_KEY_AUTHENTICATION:
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): AJ_METHOD_KEY_AUTHENTICATION\n"));
        status = AJ_PeerHandleKeyAuthentication(msg, &reply);
        break;

    case AJ_REPLY_ID(AJ_METHOD_EXCHANGE_GUIDS):
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): AJ_REPLY_ID(AJ_METHOD_EXCHANGE_GUIDS)\n"));
        status = AJ_PeerHandleExchangeGUIDsReply(msg);
        break;

    case AJ_REPLY_ID(AJ_METHOD_EXCHANGE_SUITES):
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): AJ_REPLY_ID(AJ_METHOD_EXCHANGE_SUITES)\n"));
        status = AJ_PeerHandleExchangeSuitesReply(msg);
        break;

    case AJ_REPLY_ID(AJ_METHOD_KEY_EXCHANGE):
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): AJ_REPLY_ID(AJ_METHOD_KEY_EXCHANGE)\n"));
        status = AJ_PeerHandleKeyExchangeReply(msg);
        break;

    case AJ_REPLY_ID(AJ_METHOD_KEY_AUTHENTICATION):
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): AJ_REPLY_ID(AJ_METHOD_KEY_AUTHENTICATION)\n"));
        status = AJ_PeerHandleKeyAuthenticationReply(msg);
        break;

    case AJ_REPLY_ID(AJ_METHOD_GEN_SESSION_KEY):
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): AJ_REPLY_ID(AJ_METHOD_GEN_SESSION_KEY)\n"));
        status = AJ_PeerHandleGenSessionKeyReply(msg);
        break;

    case AJ_REPLY_ID(AJ_METHOD_EXCHANGE_GROUP_KEYS):
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): AJ_REPLY_ID(AJ_METHOD_EXCHANGE_GROUP_KEYS)\n"));
        status = AJ_PeerHandleExchangeGroupKeysReply(msg);
        break;

    case AJ_REPLY_ID(AJ_METHOD_CANCEL_SESSIONLESS):
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): AJ_REPLY_ID(AJ_METHOD_CANCEL_SESSIONLESS)\n"));
        // handle return code here
        status = AJ_OK;
        break;

    case AJ_SIGNAL_SESSION_JOINED:
    case AJ_SIGNAL_NAME_ACQUIRED:
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): AJ_SIGNAL_{SESSION_JOINED|NAME_ACQUIRED}\n"));
        // nothing to do here
        status = AJ_OK;
        break;

    case AJ_REPLY_ID(AJ_METHOD_CANCEL_ADVERTISE):
    case AJ_REPLY_ID(AJ_METHOD_ADVERTISE_NAME):
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): AJ_REPLY_ID(AJ_METHOD_{CANCEL_ADVERTISE|ADVERTISE_NAME})\n"));
        if (msg->hdr->msgType == AJ_MSG_ERROR) {
            status = AJ_ERR_FAILURE;
        }
        break;

    case AJ_METHOD_ABOUT_GET_PROP:
        return AJ_AboutHandleGetProp(msg);

    case AJ_METHOD_ABOUT_GET_ABOUT_DATA:
        status = AJ_AboutHandleGetAboutData(msg, &reply);
        break;

    case AJ_METHOD_ABOUT_GET_OBJECT_DESCRIPTION:
        status = AJ_AboutHandleGetObjectDescription(msg, &reply);
        break;

    case AJ_METHOD_ABOUT_ICON_GET_PROP:
        return AJ_AboutIconHandleGetProp(msg);

    case AJ_METHOD_ABOUT_ICON_GET_URL:
        status = AJ_AboutIconHandleGetURL(msg, &reply);
        break;

    case AJ_METHOD_ABOUT_ICON_GET_CONTENT:
        status = AJ_AboutIconHandleGetContent(msg, &reply);
        break;

#ifdef ANNOUNCE_BASED_DISCOVERY
    case AJ_SIGNAL_ABOUT_ANNOUNCE:
        status = AJ_AboutHandleAnnounce(msg, NULL, NULL, NULL, NULL);
        break;
#endif

    default:
        AJ_InfoPrintf(("AJ_BusHandleBusMessage(): default\n"));
        if (msg->hdr->msgType == AJ_MSG_METHOD_CALL) {
            status = AJ_MarshalErrorMsg(msg, &reply, AJ_ErrRejected);
        }
        break;
    }
    if ((status == AJ_OK) && (msg->hdr->msgType == AJ_MSG_METHOD_CALL)) {
        status = AJ_DeliverMsg(&reply);
    }
    /*
     * Check if there is anything to announce
     */
    if (status == AJ_OK) {
        AJ_AboutAnnounce(bus);
    }
    return status;
}

void AJ_BusSetPasswordCallback(AJ_BusAttachment* bus, AJ_AuthPwdFunc pwdCallback)
{
#ifndef NO_SECURITY
    AJ_InfoPrintf(("AJ_BusSetPasswordCallback(bus=0x%p, pwdCallback=0x%p)\n", bus, pwdCallback));
    bus->pwdCallback = pwdCallback;
#endif
}

/**
 * Set a callback for auth listener
 * until a password callback function has been set.
 *
 * @param bus          The bus attachment struct
 * @param authListenerCallback  The auth listener callback function.
 */
void AJ_BusSetAuthListenerCallback(AJ_BusAttachment* bus, AJ_AuthListenerFunc authListenerCallback) {
    AJ_InfoPrintf(("AJ_BusSetAuthListenerCallback(bus=0x%p, authListenerCallback=0x%p)\n", bus, authListenerCallback));
    bus->authListenerCallback = authListenerCallback;
}

AJ_Status AJ_BusAuthenticatePeer(AJ_BusAttachment* bus, const char* peerName, AJ_BusAuthPeerCallback callback, void* cbContext)
{
    AJ_InfoPrintf(("AJ_BusAuthenticatePeer(bus=0x%p, peerName=\"%s\", callback=0x%p, cbContext=0x%p)\n", bus, peerName, callback, cbContext));
    return AJ_PeerAuthenticate(bus, peerName, callback, cbContext);
}

typedef struct {
    void* context;
    union {
        AJ_BusPropGetCallback Get;
        AJ_BusPropSetCallback Set;
    };
} PropCallback;

static AJ_Status PropAccess(AJ_Message* msg, PropCallback* cb, uint8_t op)
{
    AJ_Status status;
    AJ_Message reply;
    uint32_t propId;
    const char* sig;

    AJ_InfoPrintf(("PropAccess(msg=0x%p, cb=0x%p, op=%s)\n", msg, cb, (op == AJ_PROP_GET) ? "get" : "set"));

    /*
     * Find out which property is being accessed and whether the access is a GET or SET
     */
    status = AJ_UnmarshalPropertyArgs(msg, &propId, &sig);
    if (status == AJ_OK) {
        AJ_MarshalReplyMsg(msg, &reply);
        /*
         * Callback to let the application marshal or unmarshal the value
         */
        if (op == AJ_PROP_GET) {
            AJ_MarshalVariant(&reply, sig);
            status = cb->Get(&reply, propId, cb->context);
        } else {
            const char* variant;
            AJ_UnmarshalVariant(msg, &variant);
            /*
             * Check that the value has the expected signature
             */
            if (strcmp(sig, variant) == 0) {
                status = cb->Set(msg, propId, cb->context);
            } else {
                AJ_InfoPrintf(("PropAccess(): AJ_ERR_SIGNATURE\n"));
                status = AJ_ERR_SIGNATURE;
            }
        }
    }
    if (status != AJ_OK) {
        AJ_MarshalStatusMsg(msg, &reply, status);
    }
    return AJ_DeliverMsg(&reply);
}

static AJ_Status PropAccessAll(AJ_Message* msg, PropCallback* cb)
{
    AJ_Status status;
    AJ_Message reply;
    const char* iface;

    AJ_InfoPrintf(("PropAccessAll(msg=0x%p, cb=0x%p)\n", msg, cb));

    status = AJ_UnmarshalArgs(msg, "s", &iface);
    if (status == AJ_OK) {
        status = AJ_MarshalReplyMsg(msg, &reply);
    }
    if (status == AJ_OK) {
        status = AJ_MarshalAllPropertiesArgs(&reply, iface, cb->Get, cb->context);
    }
    if (status != AJ_OK) {
        AJ_MarshalStatusMsg(msg, &reply, status);
    }
    return AJ_DeliverMsg(&reply);
}

AJ_Status AJ_BusPropGet(AJ_Message* msg, AJ_BusPropGetCallback callback, void* context)
{
    PropCallback cb;

    AJ_InfoPrintf(("AJ_BusPropGet(msg=0x%p, callback=0x%p, context=0x%p)\n", msg, callback, context));

    cb.context = context;
    cb.Get = callback;
    return PropAccess(msg, &cb, AJ_PROP_GET);
}

AJ_Status AJ_BusPropSet(AJ_Message* msg, AJ_BusPropSetCallback callback, void* context)
{
    PropCallback cb;

    AJ_InfoPrintf(("AJ_BusPropSet(msg=0x%p, callback=0x%p, context=0x%p)\n", msg, callback, context));

    cb.context = context;
    cb.Set = callback;
    return PropAccess(msg, &cb, AJ_PROP_SET);
}

AJ_Status AJ_BusPropGetAll(AJ_Message* msg, AJ_BusPropGetCallback callback, void* context)
{
    PropCallback cb;

    AJ_InfoPrintf(("AJ_BusPropGetAll(msg=0x%p, callback=0x%p, context=0x%p)\n", msg, callback, context));

    cb.context = context;
    cb.Get = callback;
    return PropAccessAll(msg, &cb);
}

AJ_Status AJ_BusEnableSecurity(AJ_BusAttachment* bus, const uint32_t* suites, size_t numsuites)
{
    size_t i;

    AJ_InfoPrintf(("AJ_BusEnableSecurity(bus=0x%p, suites=0x%p)\n", bus, suites));

    for (i = 0; i < numsuites; i++) {
        AJ_EnableSuite(suites[i]);
    }

    return AJ_OK;
}
