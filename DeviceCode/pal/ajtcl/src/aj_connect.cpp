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
#define AJ_MODULE CONNECT

#include "aj_target.h"
#include "aj_status.h"
#include "aj_bufio.h"
#include "aj_msg.h"
#include "aj_connect.h"
#include "aj_introspect.h"
#include "aj_net.h"
#include "aj_bus.h"
#include "aj_disco.h"
#include "aj_std.h"
#include "aj_debug.h"
#include "aj_config.h"
#include "aj_creds.h"
#include "aj_peer.h"

#ifdef AJ_ARDP
#include "aj_ardp.h"
#endif
#include "aj_crypto.h"

#if !(defined(ARDUINO) || defined(__linux) || defined(_WIN32) || defined(__MACH__))
#include "aj_wifi_ctrl.h"
#endif

#ifdef AJ_SERIAL_CONNECTION
#include "aj_serial.h"
#endif

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
AJ_EXPORT uint8_t dbgCONNECT = 0;
#endif
/*
 * Protocol version of the router you have connected to
 */
static uint8_t routingProtoVersion = 0;
/*
 * Minimum accepted protocol version of a router to be connected to
 * Version 10 (14.06) allows for NGNS and untrusted connection to router
 * May be set to earlier version with AJ_SetMinProtoVersion().
 */
static uint8_t minProtoVersion = 10;

/*
 * The amount of time to wait for routing node responses during discovery.
 * May be set to a different value with AJ_SetSelectionTimeout().
 */
static uint32_t selectionTimeout = AJ_SELECTION_TIMEOUT;

static const char daemonService[] = "org.alljoyn.BusNode";

uint8_t AJ_GetMinProtoVersion()
{
    return minProtoVersion;
}
void AJ_SetMinProtoVersion(uint8_t min)
{
    minProtoVersion = min;
}

void AJ_SetSelectionTimeout(uint32_t selection)
{
    selectionTimeout = selection;
}

void SetBusAuthPwdCallback(BusAuthPwdFunc callback)
{
    /*
     * This functionality is no longer provided but the function is still defined for backwards
     * compatibility.
     */
}

static AJ_Status SendHello(AJ_BusAttachment* bus)
{
    AJ_Status status;
    AJ_Message msg;

    AJ_InfoPrintf(("SendHello(bus=0x%p)\n", bus));

    status = AJ_MarshalMethodCall(bus, &msg, AJ_METHOD_HELLO, AJ_DBusDestination, 0, AJ_FLAG_ALLOW_REMOTE_MSG, 5000);
    if (status == AJ_OK) {
        status = AJ_DeliverMsg(&msg);
    }
    return status;
}

static void ResetRead(AJ_IOBuffer* rxBuf) {
    rxBuf->readPtr += AJ_IO_BUF_AVAIL(rxBuf);
    *rxBuf->writePtr = '\0';
}

static AJ_Status ReadLine(AJ_IOBuffer* rxBuf) {
    /*
     * All the authentication messages end in a CR/LF so read until we get a newline
     */
    AJ_Status status = AJ_OK;
    while ((AJ_IO_BUF_AVAIL(rxBuf) == 0) || (*(rxBuf->writePtr - 1) != '\n')) {
        status = rxBuf->recv(rxBuf, AJ_IO_BUF_SPACE(rxBuf), 3500);
        if (status != AJ_OK) {
            break;
        }
    }
    return status;
}

static AJ_Status WriteLine(AJ_IOBuffer* txBuf, char* line) {
    strcpy((char*) txBuf->writePtr, line);
    txBuf->writePtr += strlen(line);
    return txBuf->send(txBuf);
}
uint8_t AJ_GetRoutingProtoVersion(void)
{
    return routingProtoVersion;
}
/**
 * Since the routing node expects any of its clients to use SASL with Anonymous
 * or PINX in order to connect, this method will send the necessary SASL
 * Anonymous exchange in order to connect.  PINX is no longer supported on the
 * Thin Client.  All thin clients will connect as untrusted clients to the
 * routing node.
 */
static AJ_Status AnonymousAuthAdvance(AJ_IOBuffer* rxBuf, AJ_IOBuffer* txBuf) {
    AJ_Status status = AJ_OK;
    AJ_GUID localGuid;
    char buf[40];

    /* initiate the SASL exchange with AUTH ANONYMOUS */
    status = WriteLine(txBuf, "AUTH ANONYMOUS\n");
    ResetRead(rxBuf);

    if (status == AJ_OK) {
        /* expect server to send back OK GUID */
        status = ReadLine(rxBuf);
        if (status == AJ_OK) {
            if (memcmp(rxBuf->readPtr, "OK", 2) != 0) {
                return AJ_ERR_ACCESS_ROUTING_NODE;
            }
        }
    }

    if (status == AJ_OK) {
        status = WriteLine(txBuf, "INFORM_PROTO_VERSION 10\n");
        ResetRead(rxBuf);
    }

    if (status == AJ_OK) {
        /* expect server to send back INFORM_PROTO_VERSION version# */
        status = ReadLine(rxBuf);
        if (status == AJ_OK) {
            if (memcmp(rxBuf->readPtr, "INFORM_PROTO_VERSION", strlen("INFORM_PROTO_VERSION")) != 0) {
                return AJ_ERR_ACCESS_ROUTING_NODE;
            }
            routingProtoVersion = atoi((const char*)(rxBuf->readPtr + strlen("INFORM_PROTO_VERSION") + 1));
            if (routingProtoVersion < AJ_GetMinProtoVersion()) {
                AJ_InfoPrintf(("ERR_OLD_VERSION: Found version %u but minimum %u required", routingProtoVersion, AJ_GetMinProtoVersion()));
                return AJ_ERR_OLD_VERSION;
            }
        }
    }

    if (status == AJ_OK) {
        /* send BEGIN LocalGUID to server */
        AJ_GetLocalGUID(&localGuid);
        strcpy(buf, "BEGIN ");
        status = AJ_GUID_ToString(&localGuid, buf + strlen(buf), 33);
        strcat(buf, "\n");
        status = WriteLine(txBuf, buf);
        ResetRead(rxBuf);
    }
    return status;
}

static AJ_Status SetSignalRules(AJ_BusAttachment* bus)
{
    AJ_Status status = AJ_OK;
    /*
     * AJ_GUID needs the NameOwnerChanged signal to clear out entries in
     * its map.  Prior to router version 10 this means we must set a
     * signal rule to receive every NameOwnerChanged signal.  With
     * version 10 the router supports the arg[0,1,...] key in match
     * rules, allowing us to set a signal rule for just the
     * NameOwnerChanged signals of entries in the map.  See aj_guid.c
     * for usage of the arg key.
     */
    if (AJ_GetRoutingProtoVersion() < 11) {
        status = AJ_BusSetSignalRule(bus, "type='signal',member='NameOwnerChanged',interface='org.freedesktop.DBus'", AJ_BUS_SIGNAL_ALLOW);
        if (status == AJ_OK) {
            uint8_t found_reply = FALSE;
            AJ_Message msg;
            AJ_Time timer;
            AJ_InitTimer(&timer);

            while (found_reply == FALSE && AJ_GetElapsedTime(&timer, TRUE) < 3000) {
                status = AJ_UnmarshalMsg(bus, &msg, 3000);
                if (status == AJ_OK) {
                    switch (msg.msgId) {
                    case AJ_REPLY_ID(AJ_METHOD_ADD_MATCH):
                        found_reply = TRUE;
                        break;

                    default:
                        // ignore everything else
                        AJ_BusHandleBusMessage(&msg);
                        break;
                    }

                    AJ_CloseMsg(&msg);
                }
            }
        }
    }

    return status;
}

AJ_Status AJ_Authenticate(AJ_BusAttachment* bus)
{
    AJ_Status status = AJ_OK;
    AJ_Message helloResponse;

    if (bus->isAuthenticated) {
        // ARDP does not do SASL and it sends BusHello as part of the SYN message.
        // Therefore, Hello has already been sent by the time AJ_Net_Connect() returns,
        // *before* AJ_Authenticate is called.
        return AJ_OK;
    }

    /*
     * Send initial NUL byte
     */
    bus->sock.tx.writePtr[0] = 0;
    bus->sock.tx.writePtr += 1;
    status = bus->sock.tx.send(&bus->sock.tx);
    if (status != AJ_OK) {
        AJ_InfoPrintf(("AJ_Authenticate(): status=%s\n", AJ_StatusText(status)));
        goto ExitConnect;
    }

    /* Use SASL Anonymous to connect to routing node */
    status = AnonymousAuthAdvance(&bus->sock.rx, &bus->sock.tx);
    if (status == AJ_OK) {
        status = SendHello(bus);
    }

    if (status == AJ_OK) {
        status = AJ_UnmarshalMsg(bus, &helloResponse, 5000);
    }

    if (status == AJ_OK) {
        if (helloResponse.hdr->msgType == AJ_MSG_ERROR) {
            AJ_ErrPrintf(("AJ_Authenticate(): AJ_ERR_TIMEOUT\n"));
            status = AJ_ERR_TIMEOUT;
        } else {
            AJ_Arg arg;
            status = AJ_UnmarshalArg(&helloResponse, &arg);
            if (status == AJ_OK) {
                if (arg.len >= (sizeof(bus->uniqueName) - 1)) {
                    AJ_ErrPrintf(("AJ_Authenticate(): AJ_ERR_RESOURCES\n"));
                    status = AJ_ERR_RESOURCES;
                } else {
                    memcpy(bus->uniqueName, arg.val.v_string, arg.len);
                    bus->uniqueName[arg.len] = '\0';
                }
            }
        }
        AJ_CloseMsg(&helloResponse);
    }

ExitConnect:

    if (status != AJ_OK) {
        AJ_InfoPrintf(("AJ_Authenticate(): status=%s\n", AJ_StatusText(status)));
    } else {
        bus->isAuthenticated = TRUE;
    }
    return status;
}

#define AJ_DHCP_TIMEOUT  5000

// TODO: deprecate this function; replace it with AJ_FindBusAndConnect
AJ_Status AJ_Connect(AJ_BusAttachment* bus, const char* serviceName, uint32_t timeout)
{
    AJ_Status status;
    AJ_Service service;
    bus->isAuthenticated = FALSE;

#ifdef AJ_SERIAL_CONNECTION
    AJ_Time start, now;
    AJ_InitTimer(&start);
#endif

    AJ_InfoPrintf(("AJ_Connect(bus=0x%p, serviceName=\"%s\", timeout=%d, selectionTimeout=%d.)\n", bus, serviceName, timeout, selectionTimeout));

    /*
     * Clear the bus struct
     */
    memset(bus, 0, sizeof(AJ_BusAttachment));
    /*
     * Clear stale name->GUID mappings
     */
    AJ_GUID_ClearNameMap();

#if !(defined(ARDUINO) || defined(__linux) || defined(_WIN32) || defined(__MACH__))
    /*
     * Get an IP address.  We don't want to break this older version
     * of AJ_Connect, so acquire an IP if we don't already have one.
     *
     * This does not work on non-embedded platforms!
     */
    {
        uint32_t ip, mask, gw;
        status = AJ_AcquireIPAddress(&ip, &mask, &gw, AJ_DHCP_TIMEOUT);

        if (status != AJ_OK) {
            AJ_ErrPrintf(("AJ_Net_Up(): AJ_AcquireIPAddress Failed\n"));
        }
    }
#endif

    /*
     * Discover a daemon or service to connect to
     */
    if (!serviceName) {
        serviceName = daemonService;
    }
#if AJ_CONNECT_LOCALHOST
    service.ipv4port = 9955;
#if HOST_IS_LITTLE_ENDIAN
    service.ipv4 = 0x0100007F; // 127.0.0.1
#endif
#if HOST_IS_BIG_ENDIAN
    service.ipv4 = 0x7f000001; // 127.0.0.1
#endif
    service.addrTypes = AJ_ADDR_TCP4;
#elif defined(ARDUINO)
    service.ipv4port = 9955;
    service.ipv4 = 0x6501A8C0; // 192.168.1.101
    service.addrTypes = AJ_ADDR_TCP4;
    status = AJ_Discover(serviceName, &service, timeout, selectionTimeout);
    if (status != AJ_OK) {
        AJ_InfoPrintf(("AJ_Connect(): AJ_Discover status=%s\n", AJ_StatusText(status)));
        goto ExitConnect;
    }
#elif defined(AJ_SERIAL_CONNECTION)
    // don't bother with discovery, we are connected to a daemon.
    // however, take this opportunity to bring up the serial connection
    // in a way that depends on the target
    status = AJ_Serial_Up();
    if (status != AJ_OK) {
        AJ_InfoPrintf(("AJ_Connect(): AJ_Serial_Up status=%s\n", AJ_StatusText(status)));
    }
#else
    status = AJ_Discover(serviceName, &service, timeout, selectionTimeout);
    if (status != AJ_OK) {
        AJ_InfoPrintf(("AJ_Connect(): AJ_Discover status=%s\n", AJ_StatusText(status)));
        goto ExitConnect;
    }
#endif
    status = AJ_Net_Connect(bus, &service);
    if (status != AJ_OK) {
        AJ_InfoPrintf(("AJ_Connect(): AJ_Net_Connect status=%s\n", AJ_StatusText(status)));
        goto ExitConnect;
    }

#ifdef AJ_SERIAL_CONNECTION
    // run the state machine for long enough to (hopefully) do the SLAP handshake
    do {
        AJ_StateMachine();
        AJ_InitTimer(&now);
    } while (AJ_SerialLinkParams.linkState != AJ_LINK_ACTIVE && AJ_GetTimeDifference(&now, &start) < timeout);

    if (AJ_SerialLinkParams.linkState != AJ_LINK_ACTIVE) {
        AJ_InfoPrintf(("Failed to establish active SLAP connection in %u msec\n", timeout));
        AJ_SerialShutdown();
        return AJ_ERR_TIMEOUT;
    }
#endif

    status = AJ_Authenticate(bus);
    if (status != AJ_OK) {
        AJ_InfoPrintf(("AJ_Connect(): AJ_Authenticate status=%s\n", AJ_StatusText(status)));
        goto ExitConnect;
    }

    status = SetSignalRules(bus);
    if (status != AJ_OK) {
        AJ_InfoPrintf(("AJ_Connect(): SetSignalRules status=%s\n", AJ_StatusText(status)));
        goto ExitConnect;
    }


ExitConnect:

    if (status != AJ_OK) {
        AJ_InfoPrintf(("AJ_Connect(): status=%s\n", AJ_StatusText(status)));
        AJ_Disconnect(bus);
    }
    AJ_InitRoutingNodeResponselist();
    return status;
}

static void AddRoutingNodeToBlacklist(AJ_Service* service);

AJ_Status AJ_FindBusAndConnect(AJ_BusAttachment* bus, const char* serviceName, uint32_t timeout)
{
    AJ_Status status;
    AJ_Service service;
    AJ_Time connectionTimer;
    int32_t connectionTime;
    uint8_t finished = FALSE;
    bus->isAuthenticated = FALSE;

#ifdef AJ_SERIAL_CONNECTION
    AJ_Time start, now;
    AJ_InitTimer(&start);
#endif

    AJ_InfoPrintf(("AJ_FindBusAndConnect(bus=0x%p, serviceName=\"%s\", timeout=%d, selection timeout=%d.)\n", bus, serviceName, timeout, selectionTimeout));

    /*
     * Clear the bus struct
     */
    memset(bus, 0, sizeof(AJ_BusAttachment));
    /*
     * Clear stale name->GUID mappings
     */
    AJ_GUID_ClearNameMap();

    /*
     * Discover a daemon or service to connect to
     */
    if (!serviceName) {
        serviceName = daemonService;
    }

    while (finished == FALSE) {
        finished = TRUE;
        connectionTime = (int32_t) timeout;

#if AJ_CONNECT_LOCALHOST
        service.ipv4port = 9955;
#if HOST_IS_LITTLE_ENDIAN
        service.ipv4 = 0x0100007F; // 127.0.0.1
#endif
#if HOST_IS_BIG_ENDIAN
        service.ipv4 = 0x7f000001; // 127.0.0.1
#endif
        service.addrTypes = AJ_ADDR_TCP4;
#elif defined(ARDUINO)
        service.ipv4port = 9955;
        service.ipv4 = 0x6501A8C0; // 192.168.1.101
        service.addrTypes = AJ_ADDR_TCP4;
        AJ_InitTimer(&connectionTimer);
        AJ_InfoPrintf(("AJ_FindBusAndConnect(): Connection timer started\n"));
        status = AJ_Discover(serviceName, &service, timeout, selectionTimeout);
        if (status != AJ_OK) {
            AJ_InfoPrintf(("AJ_FindBusAndConnect(): AJ_Discover status=%s\n", AJ_StatusText(status)));
            goto ExitConnect;
        }
#elif defined(AJ_SERIAL_CONNECTION)
        // don't bother with discovery, we are connected to a daemon.
        // however, take this opportunity to bring up the serial connection
        status = AJ_Serial_Up();
        if (status != AJ_OK) {
            AJ_InfoPrintf(("AJ_FindBusAndConnect(): AJ_Serial_Up status=%s\n", AJ_StatusText(status)));
        }
#else
        AJ_InitTimer(&connectionTimer);
        AJ_InfoPrintf(("AJ_FindBusAndConnect(): Connection timer started\n"));
        status = AJ_Discover(serviceName, &service, timeout, selectionTimeout);
        if (status != AJ_OK) {
            AJ_InfoPrintf(("AJ_FindBusAndConnect(): AJ_Discover status=%s\n", AJ_StatusText(status)));
            goto ExitConnect;
        }
#endif

        // this calls into platform code that will decide whether to use UDP or TCP, based on what is available
        status = AJ_Net_Connect(bus, &service);
        if (status != AJ_OK) {
            AJ_InfoPrintf(("AJ_FindBusAndConnect(): AJ_Net_Connect status=%s\n", AJ_StatusText(status)));
            goto ExitConnect;
        }

#ifdef AJ_SERIAL_CONNECTION
        // run the state machine for long enough to (hopefully) do the SLAP handshake
        do {
            AJ_StateMachine();
            AJ_InitTimer(&now);
        } while (AJ_SerialLinkParams.linkState != AJ_LINK_ACTIVE && AJ_GetTimeDifference(&now, &start) < timeout);

        if (AJ_SerialLinkParams.linkState != AJ_LINK_ACTIVE) {
            AJ_InfoPrintf(("AJ_FindBusAndConnect(): Failed to establish active SLAP connection in %u msec\n", timeout));
            AJ_SerialShutdown();
            return AJ_ERR_TIMEOUT;
        }
#endif

        status = AJ_Authenticate(bus);
        if (status != AJ_OK) {
            AJ_InfoPrintf(("AJ_FindBusAndConnect(): AJ_Authenticate status=%s\n", AJ_StatusText(status)));

#if !AJ_CONNECT_LOCALHOST && !defined(ARDUINO) && !defined(AJ_SERIAL_CONNECTION)
            AJ_InfoPrintf(("AJ_FindBusAndConnect(): Blacklisting routing node"));
            AddRoutingNodeToBlacklist(&service);
            // try again
            finished = FALSE;
            connectionTime -= AJ_GetElapsedTime(&connectionTimer, FALSE);
            // select a new node from the response list
            while (connectionTime > 0) {
                status = AJ_SelectRoutingNodeFromResponseList(&service);
                if (status == AJ_ERR_END_OF_DATA) {
                    status = AJ_ERR_TIMEOUT;
                    AJ_InfoPrintf(("Exhausted all the retries from the response list\n"));
                    finished = FALSE;
                    break;
                }
                AJ_InfoPrintf(("Retrying with a new selection from the routing node response list\n"));
                status = AJ_Net_Connect(bus, &service);
                if (status != AJ_OK) {
                    AJ_InfoPrintf(("AJ_FindBusAndConnect(): AJ_Net_Connect status=%s\n", AJ_StatusText(status)));
                    goto ExitConnect;
                }
                status = AJ_Authenticate(bus);
                if (status == AJ_OK) {
                    finished = TRUE;
                    break;
                } else {
                    connectionTime -= AJ_GetElapsedTime(&connectionTimer, FALSE);
                }
            }
#endif
        }

        if (status != AJ_OK) {
            AJ_InfoPrintf(("AJ_FindBusAndConnect(): AJ_Authenticate status=%s\n", AJ_StatusText(status)));
            goto ExitConnect;
        }

        status = SetSignalRules(bus);
        if (status != AJ_OK) {
            AJ_InfoPrintf(("AJ_FindBusAndConnect(): SetSignalRules status=%s\n", AJ_StatusText(status)));
            goto ExitConnect;
        }

        AJ_InitRoutingNodeResponselist();
    }

ExitConnect:

    AJ_InitRoutingNodeResponselist();
    if (status != AJ_OK) {
        AJ_InfoPrintf(("AJ_FindBusAndConnect(): status=%s\n", AJ_StatusText(status)));
        AJ_Disconnect(bus);
    }

    return status;
}

#ifdef AJ_ARDP

AJ_Status AJ_ARDP_UDP_Connect(AJ_BusAttachment* bus, void* context, const AJ_Service* service, AJ_NetSocket* netSock)
{
    AJ_Message hello;
    AJ_GUID localGuid;
    char guid_buf[33];
    AJ_Status status;
    AJ_Message helloResponse;

    AJ_GetLocalGUID(&localGuid);
    AJ_GUID_ToString(&localGuid, guid_buf, sizeof(guid_buf));

    AJ_MarshalMethodCall(bus, &hello, AJ_METHOD_BUS_SIMPLE_HELLO, AJ_BusDestination, 0, AJ_FLAG_ALLOW_REMOTE_MSG, AJ_UDP_CONNECT_TIMEOUT);
    AJ_MarshalArgs(&hello, "su", guid_buf, 10);
    hello.hdr->bodyLen = hello.bodyBytes;

    status = AJ_ARDP_Connect(bus->sock.tx.readPtr, AJ_IO_BUF_AVAIL(&bus->sock.tx), context, netSock);
    if (status != AJ_OK) {
        return status;
    }

    status = AJ_UnmarshalMsg(bus, &helloResponse, AJ_UDP_CONNECT_TIMEOUT);
    if (status == AJ_OK && helloResponse.msgId == AJ_REPLY_ID(AJ_METHOD_BUS_SIMPLE_HELLO)) {
        if (helloResponse.hdr->msgType == AJ_MSG_ERROR) {
            status = AJ_ERR_CONNECT;
        } else {
            AJ_Arg uniqueName, protoVersion;
            AJ_UnmarshalArg(&helloResponse, &uniqueName);
            AJ_SkipArg(&helloResponse);
            AJ_UnmarshalArg(&helloResponse, &protoVersion);

            /**
             * The two most-significant bits are reserved for the nameType,
             * which we don't currently care about in the thin client
             */
            routingProtoVersion = (uint8_t) ((*protoVersion.val.v_uint32) & 0x3FFFFFFF);

            if (uniqueName.len >= (sizeof(bus->uniqueName) - 1)) {
                AJ_ErrPrintf(("AJ_ARDP_Connect(): AJ_ERR_RESOURCES\n"));
                status = AJ_ERR_RESOURCES;
            } else {
                memcpy(bus->uniqueName, uniqueName.val.v_string, uniqueName.len);
                bus->uniqueName[uniqueName.len] = '\0';
            }

            AJ_InfoPrintf(("Received name: %s and version %u\n", bus->uniqueName, routingProtoVersion));
            if (routingProtoVersion < AJ_GetMinProtoVersion()) {
                AJ_InfoPrintf(("AJ_ARDP_Connect(): Blacklisting routing node, found %u but require >= %u\n",
                               routingProtoVersion, AJ_GetMinProtoVersion()));
                AddRoutingNodeToBlacklist(service);
                status = AJ_ERR_CONNECT;
            }
        }
    } else {
        status = AJ_ERR_CONNECT;
    }

    AJ_CloseMsg(&helloResponse);

    // reset the transmit queue!
    AJ_IO_BUF_RESET(&bus->sock.tx);

    if (status == AJ_OK) {
        bus->isAuthenticated = TRUE;
    }

    return status;
}

#endif // AJ_ARDP

void AJ_Disconnect(AJ_BusAttachment* bus)
{
    /*
     * We won't be getting any more method replies.
     */
    AJ_ReleaseReplyContexts();
    /*
     * Disconnect the network closing sockets etc.
     */
    AJ_Net_Disconnect(&bus->sock);

#ifdef AJ_SERIAL_CONNECTION
    AJ_SerialShutdown();
#endif

    /*
     * Clear auth context
     */
    AJ_ClearAuthContext();

    /*
     * Set the routing nodes proto version to zero (not connected)
     */
    routingProtoVersion = 0;
}

static uint32_t RoutingNodeIPBlacklist[AJ_ROUTING_NODE_BLACKLIST_SIZE];
static uint16_t RoutingNodePortBlacklist[AJ_ROUTING_NODE_BLACKLIST_SIZE];
static uint8_t RoutingNodeBlacklist_idx = 0;

static AJ_Service RoutingNodeResponselist[AJ_ROUTING_NODE_RESPONSELIST_SIZE];
static uint16_t RoutingNodeAttemptsResponselist[AJ_ROUTING_NODE_RESPONSELIST_SIZE];
static uint8_t RoutingNodeResponselist_idx = 0;

uint8_t AJ_IsRoutingNodeBlacklisted(AJ_Service* service)
{
    uint8_t i = 0;
    for (; i < AJ_ROUTING_NODE_BLACKLIST_SIZE; ++i) {
        if (RoutingNodeIPBlacklist[i]) {
            if (RoutingNodeIPBlacklist[i] == service->ipv4 && RoutingNodePortBlacklist[i] == service->ipv4port) {
                return TRUE;
            }
        } else {
            // break early if list isn't full
            break;
        }

    }

    return FALSE;
}

void AJ_AddRoutingNodeToResponseList(AJ_Service* service)
{
    /*
     * An unsorted fixed length list is kept such that the entries
     * in the list have the highest protocol version and the lowest
     * service priority (inverse of static rank/score) of the routing
     * node responses that have been received so far.  When the list
     * is full and there are new responses of the same rank as entries
     * in the list, the previously received responses are preferred
     * over the newer responses.  This may have a side-effect of not
     * allowing some responses to be considered. An alternative is
     * to randomly select which responses will get added to the list
     * in such a situation.
     */
    int i = 0;
    uint8_t replace = 0;
    int RoutingNodeSlot = 0;
    if (RoutingNodeResponselist_idx == AJ_ROUTING_NODE_RESPONSELIST_SIZE) {
        replace = 0;
        RoutingNodeSlot = 0;
    } else if (RoutingNodeResponselist_idx < AJ_ROUTING_NODE_RESPONSELIST_SIZE) {
        replace = 1;
        RoutingNodeSlot = RoutingNodeResponselist_idx;
    }
    for (i = 0; i  < AJ_ROUTING_NODE_RESPONSELIST_SIZE; ++i) {
        if (RoutingNodeResponselist[i].ipv4 || RoutingNodeResponselist[i].ipv4Udp) {
            if ((RoutingNodeResponselist[i].ipv4 && RoutingNodeResponselist[i].ipv4 == service->ipv4 && RoutingNodeResponselist[i].ipv4port == service->ipv4port)
                || (RoutingNodeResponselist[i].ipv4Udp && RoutingNodeResponselist[i].ipv4Udp == service->ipv4Udp && RoutingNodeResponselist[i].ipv4portUdp == service->ipv4portUdp)) {
                // track only the highest protocol version per service
                if (RoutingNodeResponselist[i].pv < service->pv) {
                    RoutingNodeResponselist[i].pv = service->pv;
                    RoutingNodeResponselist[i].priority = service->priority;
                    AJ_InfoPrintf(("Updated routing node entry to 0x%x (pv = %d, port = %d, priority = %d) to response list with %d response(s) in list\n", service->ipv4, service->pv, service->ipv4port, service->priority, RoutingNodeResponselist_idx));
                } else if (RoutingNodeResponselist[i].pv == service->pv) {
                    // update the priority if necessary
                    if (RoutingNodeResponselist[i].priority != service->priority) {
                        RoutingNodeResponselist[i].priority = service->priority;
                        AJ_InfoPrintf(("Updated the priority value for routing node entry to 0x%x (pv = %d, port = %d, priority = %d) to response list with %d response(s) in list\n", service->ipv4, service->pv, service->ipv4port, service->priority, RoutingNodeResponselist_idx));
                    }
                }
                // entry already present in the list
                return;
            } else {
                // if the list is full, find a tentative candidate for eviction, if possible
                if (RoutingNodeResponselist_idx == AJ_ROUTING_NODE_RESPONSELIST_SIZE) {
                    if (RoutingNodeResponselist[i].pv > RoutingNodeResponselist[RoutingNodeSlot].pv) {
                        continue;
                    } else if (RoutingNodeResponselist[i].pv < RoutingNodeResponselist[RoutingNodeSlot].pv) {
                        RoutingNodeSlot = i;
                        replace = 1;
                    } else if (RoutingNodeResponselist[i].priority < RoutingNodeResponselist[RoutingNodeSlot].priority) {
                        continue;
                    } else if (RoutingNodeResponselist[i].priority > RoutingNodeResponselist[RoutingNodeSlot].priority) {
                        RoutingNodeSlot = i;
                        replace = 1;
                    }
                }
            }
        } else {
            // break early if list isn't full
            break;
        }
    }
    if (replace) {
        if (RoutingNodeResponselist_idx == AJ_ROUTING_NODE_RESPONSELIST_SIZE) {
            // Is current candidate for eviction of a lower ranking ?
            if (service->pv < RoutingNodeResponselist[RoutingNodeSlot].pv) {
                return;
            }
            if (service->pv == RoutingNodeResponselist[RoutingNodeSlot].pv && service->priority >= RoutingNodeResponselist[RoutingNodeSlot].priority) {
                return;
            }
            AJ_InfoPrintf(("Evicting slot number %d\n", RoutingNodeSlot));
        }
        RoutingNodeResponselist[RoutingNodeSlot].ipv4 = service->ipv4;
        RoutingNodeResponselist[RoutingNodeSlot].ipv4port = service->ipv4port;
        RoutingNodeResponselist[RoutingNodeSlot].ipv4Udp = service->ipv4Udp;
        RoutingNodeResponselist[RoutingNodeSlot].ipv4portUdp = service->ipv4portUdp;
        RoutingNodeResponselist[RoutingNodeSlot].addrTypes = service->addrTypes;
        RoutingNodeResponselist[RoutingNodeSlot].pv = service->pv;
        RoutingNodeResponselist[RoutingNodeSlot].priority = service->priority;
        if (RoutingNodeResponselist_idx < AJ_ROUTING_NODE_RESPONSELIST_SIZE) {
            RoutingNodeResponselist_idx++;
        }
        AJ_InfoPrintf(("Added routing node 0x%x (pv = %d, port = %d, priority = %d) to response list with %d response(s) in list\n", service->ipv4, service->pv, service->ipv4port, service->priority, RoutingNodeResponselist_idx));
    }
}

AJ_Status AJ_SelectRoutingNodeFromResponseList(AJ_Service* service)
{
    /*
     * The selection involves choosing the router with the
     * highest protocol version and the lowest service priority
     * (inverse of static rank/score).
     */
    uint8_t i = 1;
    uint8_t selectedIndex = 0;
    uint32_t runningSum = 0;
    uint8_t skip = 0;
    uint32_t priority_idx = 0;
    uint32_t priority_srv = 0;
    uint32_t random = 0;
    if (RoutingNodeResponselist[0].ipv4 || RoutingNodeResponselist[0].ipv4Udp) {
        service->ipv4 = RoutingNodeResponselist[0].ipv4;
        service->ipv4port = RoutingNodeResponselist[0].ipv4port;
        service->ipv4Udp = RoutingNodeResponselist[0].ipv4Udp;
        service->ipv4portUdp = RoutingNodeResponselist[0].ipv4portUdp;
        service->pv = RoutingNodeResponselist[0].pv;
        service->addrTypes = RoutingNodeResponselist[0].addrTypes;
        service->priority = RoutingNodeResponselist[0].priority;
        runningSum = service->priority;
        skip = RoutingNodeAttemptsResponselist[0];
        if (skip) {
            AJ_InfoPrintf(("Index 0 was previously selected\n"));
        }
        for (; i  < AJ_ROUTING_NODE_RESPONSELIST_SIZE; ++i) {
            if (RoutingNodeResponselist[i].ipv4 || RoutingNodeResponselist[i].ipv4Udp) {
                if (RoutingNodeAttemptsResponselist[i]) {
                    AJ_InfoPrintf(("Index %d was previously selected\n", i));
                    continue;
                }
                if (skip) {
                    service->ipv4 = RoutingNodeResponselist[i].ipv4;
                    service->ipv4port = RoutingNodeResponselist[i].ipv4port;
                    service->ipv4Udp = RoutingNodeResponselist[i].ipv4Udp;
                    service->ipv4portUdp = RoutingNodeResponselist[i].ipv4portUdp;
                    service->pv = RoutingNodeResponselist[i].pv;
                    service->addrTypes = RoutingNodeResponselist[i].addrTypes;
                    service->priority = RoutingNodeResponselist[i].priority;
                    selectedIndex = i;
                    runningSum = service->priority;
                    skip = 0;
                    continue;
                }
                if (RoutingNodeResponselist[i].pv < service->pv) {
                    continue;
                }
                if (RoutingNodeResponselist[i].pv > service->pv || (RoutingNodeResponselist[i].pv == service->pv && RoutingNodeResponselist[i].priority < service->priority)) {
                    service->ipv4 = RoutingNodeResponselist[i].ipv4;
                    service->ipv4port = RoutingNodeResponselist[i].ipv4port;
                    service->ipv4Udp = RoutingNodeResponselist[i].ipv4Udp;
                    service->ipv4portUdp = RoutingNodeResponselist[i].ipv4portUdp;
                    service->pv = RoutingNodeResponselist[i].pv;
                    service->addrTypes = RoutingNodeResponselist[i].addrTypes;
                    service->priority = RoutingNodeResponselist[i].priority;
                    runningSum = service->priority;
                    selectedIndex = i;
                    AJ_InfoPrintf(("Tentatively selecting routing node %x (pv = %d, port = %d, priority = %d).\n", service->ipv4, service->pv, service->ipv4port, service->priority));
                } else if (RoutingNodeResponselist[i].priority == service->priority) {
                    /*
                     * Randomly select one of out of all the routing nodes with the same
                     * protocol version and priority with each node given an equal chance
                     * of being selected. To select from a pair of nodes, the first node's
                     * priority is used as its associated sum, while the sum of the two
                     * priorities under consideration is used as the second node's
                     * associated sum. A uniform random number up to the sum of the two
                     * priorities (inclusive) is chosen and the first node whose associated
                     * sum is greater than or equal to the random number is selected.
                     */
                    random = 0;
                    AJ_RandBytes((uint8_t*)&random, sizeof(random));
                    priority_idx = RoutingNodeResponselist[i].priority + runningSum;
                    priority_srv = runningSum;
                    runningSum = priority_idx;
                    random %= (runningSum + 1);
                    AJ_InfoPrintf(("P_idx is %u and P_srv is %u and random is %u\n", priority_idx, priority_srv, random));
                    if (random > priority_srv) {
                        AJ_InfoPrintf(("Picking index %d on this round\n", i));
                        service->ipv4 = RoutingNodeResponselist[i].ipv4;
                        service->ipv4port = RoutingNodeResponselist[i].ipv4port;
                        service->ipv4Udp = RoutingNodeResponselist[i].ipv4Udp;
                        service->ipv4portUdp = RoutingNodeResponselist[i].ipv4portUdp;
                        service->pv = RoutingNodeResponselist[i].pv;
                        service->addrTypes = RoutingNodeResponselist[i].addrTypes;
                        service->priority = RoutingNodeResponselist[i].priority;
                        selectedIndex = i;
                        AJ_InfoPrintf(("Tentatively selecting routing node 0x%x (pv = %d, port = %d, priority = %d).\n", service->ipv4, service->pv, service->ipv4port, service->priority));
                    }
                }
            } else {
                // break early if list isn't full
                break;
            }
        }
    } else {
        AJ_InitRoutingNodeResponselist();
        return AJ_ERR_TIMEOUT;
    }

    if (skip) {
        AJ_InfoPrintf(("All entries in the response list have been previously selected\n"));
        return AJ_ERR_END_OF_DATA;
    }
    RoutingNodeAttemptsResponselist[selectedIndex] = 1;
    AJ_InfoPrintf(("Selected routing node 0x%x (pv = %d, port = %d, priority = %d) out of %d responses in the list.\n", service->ipv4, service->pv, service->ipv4port, service->priority, RoutingNodeResponselist_idx));
    return AJ_OK;
}

uint8_t AJ_GetRoutingNodeResponseListSize()
{
    return RoutingNodeResponselist_idx;
}

static void AddRoutingNodeToBlacklist(AJ_Service* service)
{
    RoutingNodeIPBlacklist[RoutingNodeBlacklist_idx] = service->ipv4;
    RoutingNodePortBlacklist[RoutingNodeBlacklist_idx] = service->ipv4port;
    RoutingNodeBlacklist_idx = (RoutingNodeBlacklist_idx + 1) % AJ_ROUTING_NODE_BLACKLIST_SIZE;

    RoutingNodeIPBlacklist[RoutingNodeBlacklist_idx] = service->ipv4Udp;
    RoutingNodePortBlacklist[RoutingNodeBlacklist_idx] = service->ipv4portUdp;
    RoutingNodeBlacklist_idx = (RoutingNodeBlacklist_idx + 1) % AJ_ROUTING_NODE_BLACKLIST_SIZE;
}

void AJ_InitRoutingNodeResponselist()
{
    memset(RoutingNodeResponselist, 0, sizeof(RoutingNodeResponselist));
    memset(RoutingNodeAttemptsResponselist, 0, sizeof(RoutingNodeAttemptsResponselist));
    RoutingNodeResponselist_idx = 0;
}

void AJ_InitRoutingNodeBlacklist()
{
    memset(RoutingNodeIPBlacklist, 0, sizeof(RoutingNodeIPBlacklist));
    memset(RoutingNodePortBlacklist, 0, sizeof(RoutingNodePortBlacklist));
    RoutingNodeBlacklist_idx = 0;
}
