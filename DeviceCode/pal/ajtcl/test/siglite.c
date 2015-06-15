/*
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

#define AJ_MODULE SIGLITE

#ifndef NO_SECURITY
#define SECURE_INTERFACE
#endif

#include <aj_target.h>
#include <alljoyn.h>
#include <aj_cert.h>
#include <aj_peer.h>
#include <aj_creds.h>
#include <aj_auth_listener.h>
#include <aj_authentication.h>
#include "aj_config.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
uint8_t dbgSIGLITE = 0;

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

// Copied from alljoyn/alljoyn_core/test/bbclient.cc
static const char pem_prv[] = {
    "-----BEGIN EC PRIVATE KEY-----"
    "MHcCAQEEIAqN6AtyOAPxY5k7eFNXAwzkbsGMl4uqvPrYkIj0LNZBoAoGCCqGSM49"
    "AwEHoUQDQgAEvnRd4fX9opwgXX4Em2UiCMsBbfaqhB1U5PJCDZacz9HumDEzYdrS"
    "MymSxR34lL0GJVgEECvBTvpaHP2bpTIl6g=="
    "-----END EC PRIVATE KEY-----"
};

/*
 * Order of certificates is important.
 */
static const char pem_x509[] = {
    "-----BEGIN CERTIFICATE-----"
    "MIIBtDCCAVmgAwIBAgIJAMlyFqk69v+OMAoGCCqGSM49BAMCMFYxKTAnBgNVBAsM"
    "IDdhNDhhYTI2YmM0MzQyZjZhNjYyMDBmNzdhODlkZDAyMSkwJwYDVQQDDCA3YTQ4"
    "YWEyNmJjNDM0MmY2YTY2MjAwZjc3YTg5ZGQwMjAeFw0xNTAyMjYyMTUxMjVaFw0x"
    "NjAyMjYyMTUxMjVaMFYxKTAnBgNVBAsMIDZkODVjMjkyMjYxM2IzNmUyZWVlZjUy"
    "NzgwNDJjYzU2MSkwJwYDVQQDDCA2ZDg1YzI5MjI2MTNiMzZlMmVlZWY1Mjc4MDQy"
    "Y2M1NjBZMBMGByqGSM49AgEGCCqGSM49AwEHA0IABL50XeH1/aKcIF1+BJtlIgjL"
    "AW32qoQdVOTyQg2WnM/R7pgxM2Ha0jMpksUd+JS9BiVYBBArwU76Whz9m6UyJeqj"
    "EDAOMAwGA1UdEwQFMAMBAf8wCgYIKoZIzj0EAwIDSQAwRgIhAKfmglMgl67L5ALF"
    "Z63haubkItTMACY1k4ROC2q7cnVmAiEArvAmcVInOq/U5C1y2XrvJQnAdwSl/Ogr"
    "IizUeK0oI5c="
    "-----END CERTIFICATE-----"
    ""
    "-----BEGIN CERTIFICATE-----"
    "MIIBszCCAVmgAwIBAgIJAILNujb37gH2MAoGCCqGSM49BAMCMFYxKTAnBgNVBAsM"
    "IDdhNDhhYTI2YmM0MzQyZjZhNjYyMDBmNzdhODlkZDAyMSkwJwYDVQQDDCA3YTQ4"
    "YWEyNmJjNDM0MmY2YTY2MjAwZjc3YTg5ZGQwMjAeFw0xNTAyMjYyMTUxMjNaFw0x"
    "NjAyMjYyMTUxMjNaMFYxKTAnBgNVBAsMIDdhNDhhYTI2YmM0MzQyZjZhNjYyMDBm"
    "NzdhODlkZDAyMSkwJwYDVQQDDCA3YTQ4YWEyNmJjNDM0MmY2YTY2MjAwZjc3YTg5"
    "ZGQwMjBZMBMGByqGSM49AgEGCCqGSM49AwEHA0IABGEkAUATvOE4uYmt/10vkTcU"
    "SA0C+YqHQ+fjzRASOHWIXBvpPiKgHcINtNFQsyX92L2tMT2Kn53zu+3S6UAwy6yj"
    "EDAOMAwGA1UdEwQFMAMBAf8wCgYIKoZIzj0EAwIDSAAwRQIgKit5yeq1uxTvdFmW"
    "LDeoxerqC1VqBrmyEvbp4oJfamsCIQDvMTmulW/Br/gY7GOP9H/4/BIEoR7UeAYS"
    "4xLyu+7OEA=="
    "-----END CERTIFICATE-----"
};

static const char psk_hint[] = "<anonymous>";
/*
 * The tests were changed at some point to make the psk longer.
 * If doing backcompatibility testing with previous versions (14.08 or before),
 * define LITE_TEST_BACKCOMPAT to use the old version of the password.
 */
#ifndef LITE_TEST_BACKCOMPAT
static const char psk_char[] = "faaa0af3dd3f1e0379da046a3ab6ca44";
#else
static const char psk_char[] = "123456";
#endif
static X509CertificateChain* chain = NULL;
static ecc_privatekey prv;
static AJ_Status AuthListenerCallback(uint32_t authmechanism, uint32_t command, AJ_Credential*cred)
{
    AJ_Status status = AJ_ERR_INVALID;
    X509CertificateChain* node;

    AJ_AlwaysPrintf(("AuthListenerCallback authmechanism %d command %d\n", authmechanism, command));

    switch (authmechanism) {
    case AUTH_SUITE_ECDHE_NULL:
        cred->expiration = keyexpiration;
        status = AJ_OK;
        break;

    case AUTH_SUITE_ECDHE_PSK:
        switch (command) {
        case AJ_CRED_PUB_KEY:
            cred->data = (uint8_t*) psk_hint;
            cred->len = strlen(psk_hint);
            cred->expiration = keyexpiration;
            status = AJ_OK;
            break;

        case AJ_CRED_PRV_KEY:
            cred->data = (uint8_t*) psk_char;
            cred->len = strlen(psk_char);
            cred->expiration = keyexpiration;
            status = AJ_OK;
            break;
        }
        break;

    case AUTH_SUITE_ECDHE_ECDSA:
        switch (command) {
        case AJ_CRED_PRV_KEY:
            cred->len = sizeof (ecc_privatekey);
            status = AJ_DecodePrivateKeyPEM(&prv, pem_prv);
            if (AJ_OK != status) {
                return status;
            }
            cred->data = (uint8_t*) &prv;
            cred->expiration = keyexpiration;
            break;

        case AJ_CRED_CERT_CHAIN:
            switch (cred->direction) {
            case AJ_CRED_REQUEST:
                // Free previous certificate chain
                while (chain) {
                    node = chain;
                    chain = chain->next;
                    AJ_Free(node->certificate.der.data);
                    AJ_Free(node);
                }
                chain = AJ_X509DecodeCertificateChainPEM(pem_x509);
                if (NULL == chain) {
                    return AJ_ERR_INVALID;
                }
                cred->data = (uint8_t*) chain;
                cred->expiration = keyexpiration;
                status = AJ_OK;
                break;

            case AJ_CRED_RESPONSE:
                node = (X509CertificateChain*) cred->data;
                while (node) {
                    AJ_DumpBytes("CERTIFICATE", node->certificate.der.data, node->certificate.der.size);
                    node = node->next;
                }
                status = AJ_OK;
                break;
            }
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
    X509CertificateChain* node;
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
                AJ_BusAddSignalRule(&bus, "my_signal", testInterfaceName, AJ_BUS_SIGNAL_ALLOW);

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

    // Clean up certificate chain
    while (chain) {
        node = chain;
        chain = chain->next;
        AJ_Free(node->certificate.der.data);
        AJ_Free(node);
    }

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
