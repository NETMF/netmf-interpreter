/**
 * @file
 */
/******************************************************************************
 * Copyright (c) 2012-2014, AllSeen Alliance. All rights reserved.
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

static const char daemonService[] = "org.alljoyn.BusNode";

uint8_t AJ_GetMinProtoVersion()
{
    return minProtoVersion;
}
void AJ_SetMinProtoVersion(uint8_t min)
{
    minProtoVersion = min;
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

AJ_Status AJ_Authenticate(AJ_BusAttachment* bus)
{
    AJ_Status status = AJ_OK;

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
        AJ_Message helloResponse;
        status = AJ_UnmarshalMsg(bus, &helloResponse, 5000);
        if (status == AJ_OK) {
            /*
             * The only error we might get is a timeout
             */
            if (helloResponse.hdr->msgType == AJ_MSG_ERROR) {
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
        }
    }

ExitConnect:

    if (status != AJ_OK) {
        AJ_InfoPrintf(("AJ_Authenticate(): status=%s\n", AJ_StatusText(status)));
    }
    return status;
}

#define AJ_DHCP_TIMEOUT  5000

// TODO: deprecate this function; replace it with AJ_FindBusAndConnect
AJ_Status AJ_Connect(AJ_BusAttachment* bus, const char* serviceName, uint32_t timeout)
{
    AJ_Status status;
    AJ_Service service;

#ifdef AJ_SERIAL_CONNECTION
    AJ_Time start, now;
    AJ_InitTimer(&start);
#endif

    AJ_InfoPrintf(("AJ_Connect(bus=0x%p, serviceName=\"%s\", timeout=%d.)\n", bus, serviceName, timeout));

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
    service.addrTypes = AJ_ADDR_IPV4;
#elif defined(ARDUINO)
    service.ipv4port = 9955;
    service.ipv4 = 0x6501A8C0; // 192.168.1.101
    service.addrTypes = AJ_ADDR_IPV4;
    status = AJ_Discover(serviceName, &service, timeout);
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
    status = AJ_Discover(serviceName, &service, timeout);
    if (status != AJ_OK) {
        AJ_InfoPrintf(("AJ_Connect(): AJ_Discover status=%s\n", AJ_StatusText(status)));
        goto ExitConnect;
    }
#endif
    status = AJ_Net_Connect(&bus->sock, service.ipv4port, service.addrTypes & AJ_ADDR_IPV4, &service.ipv4);
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

ExitConnect:

    if (status != AJ_OK) {
        AJ_InfoPrintf(("AJ_Connect(): status=%s\n", AJ_StatusText(status)));
        AJ_Disconnect(bus);
    }
    return status;
}

static void AddRoutingNodeToBlacklist(AJ_Service* service);

AJ_Status AJ_FindBusAndConnect(AJ_BusAttachment* bus, const char* serviceName, uint32_t timeout)
{
    AJ_Status status;
    AJ_Service service;
    uint8_t finished = FALSE;

#ifdef AJ_SERIAL_CONNECTION
    AJ_Time start, now;
    AJ_InitTimer(&start);
#endif

    AJ_InfoPrintf(("AJ_Connect(bus=0x%p, serviceName=\"%s\", timeout=%d.)\n", bus, serviceName, timeout));

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

#if AJ_CONNECT_LOCALHOST
        service.ipv4port = 9955;
#if HOST_IS_LITTLE_ENDIAN
        service.ipv4 = 0x0100007F; // 127.0.0.1
#endif
#if HOST_IS_BIG_ENDIAN
        service.ipv4 = 0x7f000001; // 127.0.0.1
#endif
        service.addrTypes = AJ_ADDR_IPV4;
#elif defined(ARDUINO)
        service.ipv4port = 9955;
        service.ipv4 = 0x6501A8C0; // 192.168.1.101
        service.addrTypes = AJ_ADDR_IPV4;
        status = AJ_Discover(serviceName, &service, timeout);
        if (status != AJ_OK) {
            AJ_InfoPrintf(("AJ_Connect(): AJ_Discover status=%s\n", AJ_StatusText(status)));
            goto ExitConnect;
        }
#elif defined(AJ_SERIAL_CONNECTION)
        // don't bother with discovery, we are connected to a daemon.
        // however, take this opportunity to bring up the serial connection
        status = AJ_Serial_Up();
        if (status != AJ_OK) {
            AJ_InfoPrintf(("AJ_Connect(): AJ_Serial_Up status=%s\n", AJ_StatusText(status)));
        }
#else
        status = AJ_Discover(serviceName, &service, timeout);
        if (status != AJ_OK) {
            AJ_InfoPrintf(("AJ_Connect(): AJ_Discover status=%s\n", AJ_StatusText(status)));
            goto ExitConnect;
        }
#endif
        status = AJ_Net_Connect(&bus->sock, service.ipv4port, service.addrTypes & AJ_ADDR_IPV4, &service.ipv4);
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

#if !AJ_CONNECT_LOCALHOST && !defined(ARDUINO) && !defined(AJ_SERIAL_CONNECTION)
            AJ_InfoPrintf(("AJ_Connect(): Blacklisting routing node"));
            AddRoutingNodeToBlacklist(&service);
            // try again
            finished = FALSE;
#endif
            // else we will end the loop
        }
    }


ExitConnect:

    if (status != AJ_OK) {
        AJ_InfoPrintf(("AJ_Connect(): status=%s\n", AJ_StatusText(status)));
        AJ_Disconnect(bus);
    }

    return status;
}

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
     * Free cipher suite memory and clear auth context
     */
    if (bus->suites) {
        AJ_Free(bus->suites);
        bus->suites = NULL;
        bus->numsuites = 0;
    }
    AJ_ClearAuthContext();

    /*
     * Set the routing nodes proto version to zero (not connected)
     */
    routingProtoVersion = 0;
}

static uint32_t RoutingNodeIPBlacklist[AJ_ROUTING_NODE_BLACKLIST_SIZE];
static uint16_t RoutingNodePortBlacklist[AJ_ROUTING_NODE_BLACKLIST_SIZE];
static uint8_t RoutingNodeBlacklist_idx = 0;

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

static void AddRoutingNodeToBlacklist(AJ_Service* service)
{
    RoutingNodeIPBlacklist[RoutingNodeBlacklist_idx] = service->ipv4;
    RoutingNodePortBlacklist[RoutingNodeBlacklist_idx] = service->ipv4port;
    RoutingNodeBlacklist_idx = (RoutingNodeBlacklist_idx + 1) % AJ_ROUTING_NODE_BLACKLIST_SIZE;
}

void AJ_InitRoutingNodeBlacklist()
{
    memset(RoutingNodeIPBlacklist, 0, sizeof(RoutingNodeIPBlacklist));
    memset(RoutingNodePortBlacklist, 0, sizeof(RoutingNodePortBlacklist));
    RoutingNodeBlacklist_idx = 0;
}
