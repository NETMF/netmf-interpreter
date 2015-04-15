/*
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

#ifndef NO_SECURITY
#define SECURE_INTERFACE
#endif

#include <aj_target.h>
#include <alljoyn.h>
#include <aj_cert.h>
#include <aj_peer.h>
#include <aj_creds.h>
#include <aj_auth_listener.h>
#include <aj_keyexchange.h>
#include <aj_keyauthentication.h>
#include "aj_config.h"

#ifdef SECURE_INTERFACE
static const char testInterfaceName[] = "$org.alljoyn.alljoyn_test";
static const char testValuesInterfaceName[] = "$org.alljoyn.alljoyn_test.values";
#else
static const char testInterfaceName[] = "org.alljoyn.alljoyn_test";
static const char testValuesInterfaceName[] = "org.alljoyn.alljoyn_test.values";
#endif

#if defined (NGNS) || defined (ANNOUNCE_BASED_DISCOVERY)
static const char* testInterfaceNames[] = {
    testInterfaceName,
    NULL
};
#else
static const char testServiceName[] = "org.alljoyn.svclite";
static const uint16_t testServicePort = 24;
#endif
static const uint32_t NumPings = 10;

/*
 * Default key expiration
 */
static const uint32_t keyexpiration = 0xFFFFFFFF;



/*
 * The app should authenticate the peer if one or more interfaces are secure
 * To define a secure interface, prepend '$' before the interface name, eg., "$org.alljoyn.alljoyn_test"
 */

static const char* const testInterface[] = {
    testInterfaceName,
    "?my_ping inStr<s outStr>s",
    "?delayed_ping inStr<s delay<u outStr>s",
    "?time_ping <u <q >u >q",
    "!my_signal >a{ys}",
    NULL
};


static const AJ_InterfaceDescription testInterfaces[] = {
    testInterface,
    NULL
};

/**
 * Objects implemented by the application
 */
static const AJ_Object ProxyObjects[] = {
    { "/org/alljoyn/alljoyn_test", testInterfaces },
    { NULL }
};


#define PRX_MY_PING         AJ_PRX_MESSAGE_ID(0, 0, 0)
#define PRX_DELAYED_PING    AJ_PRX_MESSAGE_ID(0, 0, 1)
#define PRX_TIME_PING       AJ_PRX_MESSAGE_ID(0, 0, 2)
#define PRX_MY_SIGNAL       AJ_PRX_MESSAGE_ID(0, 0, 3)

static AJ_Status SendSignal(AJ_BusAttachment* bus, uint32_t sessionId, const char* serviceName);

/*
 * Let the application do some work
 */

#define PINGS_PER_CALL 10

static AJ_Status AppDoWork(AJ_BusAttachment* bus, uint32_t sessionId, const char* serviceName)
{
    static uint32_t pings_sent = 0;
    uint32_t pings = 0;
    AJ_Status status = AJ_OK;

    while (pings_sent < NumPings && pings++ < PINGS_PER_CALL && status == AJ_OK) {
        status = SendSignal(bus, sessionId, serviceName);
        ++pings_sent;
    }

    return status;
}

static const char PWD[] = "123456";

#ifdef SECURE_INTERFACE
static uint32_t PasswordCallback(uint8_t* buffer, uint32_t bufLen)
{
    memcpy(buffer, PWD, sizeof(PWD));
    return sizeof(PWD) - 1;
}


static const char psk_hint[] = "bob";
static const char psk_char[] = "123456";
static const char owner_pub_b64[] = "RCf5ihem02VFXvIa8EVJ1CJcJns3el0IH+H51s07rc0AAAAAn6KJifUPH1oRmPLoyBHGCg7/NT8kW67GD8kQjZh/U/AAAAAAAAAAAA==";
static const char ecc_pub_b64[] = "JmZC779f7YjYPa3rU0xdifnW0qyiCmmUXcN1XExC334AAAAA1j95MCfIAFa6Fpa5vJ+2tUMfYVmhny04itEwJPnfDqAAAAAAAAAAAA==";
static const char ecc_prv_b64[] = "koWEteat13YRYrv/olCqEmMg7YufcTsjSQNbIL1ue+wAAAAA";
static const char owner_cert1_b64[] = "\
AAAAAUQn+YoXptNlRV7yGvBFSdQiXCZ7N3pdCB/h+dbNO63NAAAAAJ+iiYn1Dx9a\
EZjy6MgRxgoO/zU/JFuuxg/JEI2Yf1PwAAAAAAAAAAAmZkLvv1/tiNg9retTTF2J\
+dbSrKIKaZRdw3VcTELffgAAAADWP3kwJ8gAVroWlrm8n7a1Qx9hWaGfLTiK0TAk\
+d8OoAAAAAAAAAAAAAAAAAAAAAAAAAAA/////wBOnWRZjvJdd9adaDleMIDQJOJC\
OuSepUTdfamDakEy/rQbXYuqvmUj1ZiGGpPYBfh7aNkFE4rng9TixhKXJ15XAAAA\
AN6X04g62BUVvnCbFuBiw2r783HQeBdGUdUrsnVoHUKkAAAAAA==";
static const char owner_cert2_b64[] = "\
AAAAAkQn+YoXptNlRV7yGvBFSdQiXCZ7N3pdCB/h+dbNO63NAAAAAJ+iiYn1Dx9a\
EZjy6MgRxgoO/zU/JFuuxg/JEI2Yf1PwAAAAAAAAAAAmZkLvv1/tiNg9retTTF2J\
+dbSrKIKaZRdw3VcTELffgAAAADWP3kwJ8gAVroWlrm8n7a1Qx9hWaGfLTiK0TAk\
+d8OoAAAAAAAAAAAAAAAAAAAAAAAAAAA/////wD5/PM2YlgaDcbxM2GD2BntTp1k\
WY7yXXfWnWg5XjCA0CTiQjrknqVE3X2pg2pBMv4X9K7ntr5Z4AQzJnz9DaHh0clG\
WYk3iayjtM2IUTldlgAAAAALQdeFHaHyScnOSPXzaHV/tLCTPKogvpv4gWOfQAsy\
2AAAAAA=";
static ecc_publickey ecc_pub;
static ecc_privatekey ecc_prv;
static AJ_Certificate root_cert;

static const char* issuers[] = {
    "RCf5ihem02VFXvIa8EVJ1CJcJns3el0IH+H51s07rc0AAAAAn6KJifUPH1oRmPLoyBHGCg7/NT8kW67GD8kQjZh/U/AAAAAAAAAAAA==",
    "9RB2ExIO4VZqEwb+sWYVsozToGMgDZJzH0Yf4Q0sCC0AAAAAhuEeeMDIXKzoOg3aQqVdUKC0ekWIRizM5hcjzxAO8LUAAAAAAAAAAA=="
};

static AJ_Status IsTrustedIssuer(const char* issuer)
{
    size_t i;
    for (i = 0; i < ArraySize(issuers); i++) {
        if (0 == strncmp(issuer, issuers[i], strlen(issuers[i]))) {
            return AJ_OK;
        }
    }
    return AJ_ERR_SECURITY;
}

static AJ_Status AuthListenerCallback(uint32_t authmechanism, uint32_t command, AJ_Credential* cred)
{
    AJ_Status status = AJ_ERR_INVALID;

    uint8_t* b8;
    size_t b8len;
    char* b64;
    size_t b64len;
    AJ_AlwaysPrintf(("AuthListenerCallback authmechanism %d command %d\n", authmechanism, command));

    switch (authmechanism) {
    case AUTH_SUITE_ECDHE_NULL:
        cred->expiration = keyexpiration;
        status = AJ_OK;
        break;

    case AUTH_SUITE_ECDHE_PSK:
        switch (command) {
        case AJ_CRED_PUB_KEY:
            break; // Don't use username - use anon
            cred->mask = AJ_CRED_PUB_KEY;
            cred->data = (uint8_t*) psk_hint;
            cred->len = strlen(psk_hint);
            status = AJ_OK;
            break;

        case AJ_CRED_PRV_KEY:
            if (AJ_CRED_PUB_KEY == cred->mask) {
                AJ_AlwaysPrintf(("Request Credentials for PSK ID: %s\n", cred->data));
            }
            cred->mask = AJ_CRED_PRV_KEY;
            cred->data = (uint8_t*) psk_char;
            cred->len = strlen(psk_char);
            cred->expiration = keyexpiration;
            status = AJ_OK;
            break;
        }
        break;

    case AUTH_SUITE_ECDHE_ECDSA:
        switch (command) {
        case AJ_CRED_PUB_KEY:
            b8len = 3 * strlen(ecc_pub_b64) / 4;
            b8 = (uint8_t*) AJ_Malloc(b8len);
            AJ_ASSERT(b8);
            status = AJ_B64ToRaw(ecc_pub_b64, strlen(ecc_pub_b64), b8, b8len);
            AJ_ASSERT(AJ_OK == status);
            status = AJ_BigEndianDecodePublicKey(&ecc_pub, b8);
            AJ_ASSERT(AJ_OK == status);
            cred->mask = AJ_CRED_PUB_KEY;
            cred->data = (uint8_t*) &ecc_pub;
            cred->len = sizeof (ecc_pub);
            cred->expiration = keyexpiration;
            AJ_Free(b8);
            break;

        case AJ_CRED_PRV_KEY:
            b8len = 3 * strlen(ecc_prv_b64) / 4;
            b8 = (uint8_t*) AJ_Malloc(b8len);
            AJ_ASSERT(b8);
            status = AJ_B64ToRaw(ecc_prv_b64, strlen(ecc_prv_b64), b8, b8len);
            AJ_ASSERT(AJ_OK == status);
            status = AJ_BigEndianDecodePrivateKey(&ecc_prv, b8);
            AJ_ASSERT(AJ_OK == status);
            cred->mask = AJ_CRED_PRV_KEY;
            cred->data = (uint8_t*) &ecc_prv;
            cred->len = sizeof (ecc_prv);
            cred->expiration = keyexpiration;
            AJ_Free(b8);
            break;

        case AJ_CRED_CERT_CHAIN:
            b8len = sizeof (AJ_Certificate);
            b8 = (uint8_t*) AJ_Malloc(b8len);
            AJ_ASSERT(b8);
            status = AJ_B64ToRaw(owner_cert1_b64, strlen(owner_cert1_b64), b8, b8len);
            AJ_ASSERT(AJ_OK == status);
            status = AJ_BigEndianDecodeCertificate(&root_cert, b8, b8len);
            AJ_ASSERT(AJ_OK == status);
            cred->mask = AJ_CRED_CERT_CHAIN;
            cred->data = (uint8_t*) &root_cert;
            cred->len = sizeof (root_cert);
            AJ_Free(b8);
            break;

        case AJ_CRED_CERT_TRUST:
            b64len = 4 * ((cred->len + 2) / 3) + 1;
            b64 = (char*) AJ_Malloc(b64len);
            AJ_ASSERT(b64);
            status = AJ_RawToB64(cred->data, cred->len, b64, b64len);
            AJ_ASSERT(AJ_OK == status);
            status = IsTrustedIssuer(b64);
            AJ_AlwaysPrintf(("TRUST: %s %d\n", b64, status));
            AJ_Free(b64);

            break;

        case AJ_CRED_CERT_ROOT:
            b64len = 4 * ((cred->len + 2) / 3) + 1;
            b64 = (char*) AJ_Malloc(b64len);
            AJ_ASSERT(b64);
            status = AJ_RawToB64(cred->data, cred->len, b64, b64len);
            AJ_ASSERT(AJ_OK == status);
            AJ_AlwaysPrintf(("ROOT: %s\n", b64));
            status = AJ_OK;
            AJ_Free(b64);
            break;
        }
        break;

    default:
        break;
    }
    return status;
}
#endif


#define CONNECT_TIMEOUT    (1000 * 200)
#define UNMARSHAL_TIMEOUT  (1000 * 5)
#define METHOD_TIMEOUT     (100 * 10)


static AJ_Status SendSignal(AJ_BusAttachment* bus, uint32_t sessionId, const char* serviceName)
{
    AJ_Status status;
    AJ_Message msg;


    status = AJ_MarshalSignal(bus, &msg, PRX_MY_SIGNAL, serviceName, sessionId, 0, 0);
    if (status == AJ_OK) {
        AJ_Arg arg;
        status = AJ_MarshalContainer(&msg, &arg, AJ_ARG_ARRAY);
        status = AJ_MarshalCloseContainer(&msg, &arg);
    }
    if (status == AJ_OK) {
        status = AJ_DeliverMsg(&msg);
    }
    return status;
}


void AuthCallback(const void* context, AJ_Status status)
{
    *((AJ_Status*)context) = status;
}

#ifdef MAIN_ALLOWS_ARGS
int AJ_Main(int ac, char** av)
#else
int AJ_Main()
#endif
{
    AJ_Status status = AJ_OK;
    AJ_BusAttachment bus;
    uint8_t connected = FALSE;
    uint32_t sessionId = 0;
    AJ_Status authStatus = AJ_ERR_NULL;
    /*
     * Buffer to hold the peer's full service name or unique name.
     */
#if defined (NGNS) || defined (ANNOUNCE_BASED_DISCOVERY)
    char peerServiceName[AJ_MAX_NAME_SIZE + 1];
#else
    char peerServiceName[AJ_MAX_SERVICE_NAME_SIZE];
#endif

#ifdef SECURE_INTERFACE
    uint32_t suites[16];
    size_t numsuites = 0;
    uint8_t clearkeys = FALSE;
    uint8_t enablepwd = FALSE;
#endif

#ifdef MAIN_ALLOWS_ARGS
#ifdef SECURE_INTERFACE

    ac--;
    av++;
    /*
     * Enable authentication mechanism by command line
     */
    if (ac) {
        if (0 == strncmp(*av, "-ek", 3)) {
            clearkeys = TRUE;
            ac--;
            av++;
        } else if (0 == strncmp(*av, "-e", 2)) {
            ac--;
            av++;
        }
        if (!ac) {
            AJ_AlwaysPrintf(("-e(k) requires an auth mechanism.\n"));
            return 1;
        }
        while (ac) {
            if (0 == strncmp(*av, "ECDHE_ECDSA", 11)) {
                suites[numsuites++] = AUTH_SUITE_ECDHE_ECDSA;
            } else if (0 == strncmp(*av, "ECDHE_PSK", 9)) {
                suites[numsuites++] = AUTH_SUITE_ECDHE_PSK;
            } else if (0 == strncmp(*av, "ECDHE_NULL", 10)) {
                suites[numsuites++] = AUTH_SUITE_ECDHE_NULL;
            } else if (0 == strncmp(*av, "PIN", 3)) {
                enablepwd = TRUE;
            }
            ac--;
            av++;
        }
    }
#endif
#endif

    /*
     * One time initialization before calling any other AllJoyn APIs
     */
    AJ_Initialize();

    AJ_PrintXML(ProxyObjects);
    AJ_RegisterObjects(NULL, ProxyObjects);

    while (TRUE) {
        AJ_Message msg;

        if (!connected) {
#if defined (NGNS) || defined (ANNOUNCE_BASED_DISCOVERY)
            status = AJ_StartClientByInterface(&bus, NULL, CONNECT_TIMEOUT, FALSE, testInterfaceNames, &sessionId, peerServiceName, NULL);
#else
            status = AJ_StartClientByName(&bus, NULL, CONNECT_TIMEOUT, FALSE, testServiceName, testServicePort, &sessionId, NULL, peerServiceName);
#endif
            if (status == AJ_OK) {
                AJ_AlwaysPrintf(("StartClient returned %d, sessionId=%u, serviceName=%s\n", status, sessionId, peerServiceName));
                AJ_AlwaysPrintf(("Connected to Daemon:%s\n", AJ_GetUniqueName(&bus)));
                connected = TRUE;
#ifdef SECURE_INTERFACE
                if (enablepwd) {
                    AJ_BusSetPasswordCallback(&bus, PasswordCallback);
                }
                AJ_BusEnableSecurity(&bus, suites, numsuites);
                AJ_BusSetAuthListenerCallback(&bus, AuthListenerCallback);
                if (clearkeys) {
                    status = AJ_ClearCredentials();
                    AJ_ASSERT(AJ_OK == status);
                }
                status = AJ_BusAuthenticatePeer(&bus, peerServiceName, AuthCallback, &authStatus);
                if (status != AJ_OK) {
                    AJ_AlwaysPrintf(("AJ_BusAuthenticatePeer returned %d\n", status));
                }
#else
                authStatus = AJ_OK;
#endif


            } else {
                AJ_AlwaysPrintf(("StartClient returned %d\n", status));
                break;
            }
        }

        if (authStatus != AJ_ERR_NULL) {
            if (authStatus != AJ_OK) {
                AJ_Disconnect(&bus);
                break;
            }
            authStatus = AJ_ERR_NULL;
        }

        status = AJ_UnmarshalMsg(&bus, &msg, UNMARSHAL_TIMEOUT);
        if (status == AJ_ERR_TIMEOUT) {
            status = AppDoWork(&bus, sessionId, peerServiceName);
            continue;
        }

        if (status == AJ_OK) {
            switch (msg.msgId) {
            case PRX_MY_SIGNAL:
                AJ_AlwaysPrintf(("Received my_signal\n"));
                status = AJ_OK;
                break;

            case AJ_SIGNAL_SESSION_LOST_WITH_REASON:
                /*
                 * Force a disconnect
                 */
                {
                    uint32_t id, reason;
                    AJ_UnmarshalArgs(&msg, "uu", &id, &reason);
                    AJ_AlwaysPrintf(("Session lost. ID = %u, reason = %u", id, reason));
                }
                status = AJ_ERR_SESSION_LOST;
                break;

            default:
                /*
                 * Pass to the built-in handlers
                 */
                status = AJ_BusHandleBusMessage(&msg);
                break;
            }
        }
        /*
         * Messages must be closed to free resources
         */
        AJ_CloseMsg(&msg);

        if ((status == AJ_ERR_SESSION_LOST) || (status == AJ_ERR_READ) || (status == AJ_ERR_LINK_DEAD)) {
            AJ_AlwaysPrintf(("AllJoyn disconnect\n"));
            AJ_AlwaysPrintf(("Disconnected from Daemon:%s\n", AJ_GetUniqueName(&bus)));
            AJ_Disconnect(&bus);
            connected = FALSE;
        }
    }
    AJ_AlwaysPrintf(("clientlite EXIT %d\n", status));

    return status;
}

#ifdef AJ_MAIN
#ifdef MAIN_ALLOWS_ARGS
int main(int ac, char** av)
{
    return AJ_Main(ac, av);
}
#else
int main()
{
    return AJ_Main();
}
#endif
#endif
