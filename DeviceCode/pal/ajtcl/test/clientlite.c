/*
 * clientlite.c
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

#define AJ_MODULE CLIENTLITE

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

uint8_t dbgCLIENTLITE = 1;

/*
 * Default key expiration
 */
static const uint32_t keyexpiration = 0xFFFFFFFF;

/*
 * The app should authenticate the peer if one or more interfaces are secure
 * To define a secure interface, prepend '$' before the interface name, eg., "$org.alljoyn.alljoyn_test"
 */
#ifdef SECURE_INTERFACE
static const char testInterfaceName[] = "$org.alljoyn.alljoyn_test";
static const char testValuesInterfaceName[] = "$org.alljoyn.alljoyn_test.values";
#else
static const char testInterfaceName[] = "org.alljoyn.alljoyn_test";
static const char testValuesInterfaceName[] = "org.alljoyn.alljoyn_test.values";
#endif

#if defined(ANNOUNCE_BASED_DISCOVERY) || defined(NGNS)
static const char* testInterfaceNames[] = {
    testInterfaceName,
    testValuesInterfaceName,
    NULL
};
#else
static const char testServiceName[] = "org.alljoyn.svclite";
#endif

/*
 * Buffer to hold the peer's full service name or unique name.
 */
#if defined(ANNOUNCE_BASED_DISCOVERY) || defined(NGNS)
static char g_peerServiceName[AJ_MAX_NAME_SIZE + 1];
#else
static char g_peerServiceName[AJ_MAX_SERVICE_NAME_SIZE];
#endif

static const uint16_t testServicePort = 24;

static const char* const testInterface[] = {
    testInterfaceName,
    "?my_ping inStr<s outStr>s",
    NULL
};

static const char* const testValuesInterface[] = {
    testValuesInterfaceName,
    "@int_val=i",
    NULL
};

static const AJ_InterfaceDescription testInterfaces[] = {
    AJ_PropertiesIface,
    testInterface,
    testValuesInterface,
    NULL
};

static const char testObj[] = "/org/alljoyn/alljoyn_test";

/**
 * Objects implemented by the application
 */

#ifdef SECURE_OBJECT
static AJ_Object ProxyObjects[] = {
    { NULL, testInterfaces, AJ_OBJ_FLAG_SECURE },   /* Object path will be specified later */
    { NULL }
};
#else
static AJ_Object ProxyObjects[] = {
    { NULL, testInterfaces },    /* Object path will be specified later */
    { NULL }
};
#endif

#define PRX_GET_PROP  AJ_PRX_MESSAGE_ID(0, 0, AJ_PROP_GET)
#define PRX_SET_PROP  AJ_PRX_MESSAGE_ID(0, 0, AJ_PROP_SET)
#define PRX_MY_PING   AJ_PRX_MESSAGE_ID(0, 1, 0)
#define PRX_GET_INT   AJ_PRX_PROPERTY_ID(0, 2, 0)
#define PRX_SET_INT   AJ_PRX_PROPERTY_ID(0, 2, 0)

#define CONNECT_TIMEOUT    (1000 * 200)
#define UNMARSHAL_TIMEOUT  (1000 * 5)
#define METHOD_TIMEOUT     (1000 * 10)
#define PING_TIMEOUT       (1000 * 10)

/**
 * Peer discovery
 */
#ifdef ANNOUNCE_BASED_DISCOVERY
static void handleMandatoryProps(const char* peerName,
                                 const char* appId,
                                 const char* appName,
                                 const char* deviceId,
                                 const char* deviceName,
                                 const char* manufacturer,
                                 const char* modelNumber,
                                 const char* defaultLanguage)
{
    AJ_AlwaysPrintf(("Mandatory Properties for %s\n", peerName));
    AJ_AlwaysPrintf(("Mandatory property: AppId=\"%s\"\n", (appId == NULL || appId[0] == '\0') ? "N/A" : appId));
    AJ_AlwaysPrintf(("Mandatory property: AppName=\"%s\"\n", (appName == NULL || appName[0] == '\0') ? "N/A" : appName));
    AJ_AlwaysPrintf(("Mandatory property: DeviceId=\"%s\"\n", (deviceId == NULL || deviceId[0] == '\0') ? "N/A" : deviceId));
    AJ_AlwaysPrintf(("Mandatory property: DeviceName=\"%s\"\n", (deviceName == NULL || deviceName[0] == '\0') ? "N/A" : deviceName));
    AJ_AlwaysPrintf(("Mandatory property: Manufacturer=\"%s\"\n", (manufacturer == NULL || manufacturer[0] == '\0') ? "N/A" : manufacturer));
    AJ_AlwaysPrintf(("Mandatory property: ModelNumber=\"%s\"\n", (modelNumber == NULL || modelNumber[0] == '\0') ? "N/A" : modelNumber));
    AJ_AlwaysPrintf(("Mandatory property: DefaultLanguage=\"%s\"\n", (defaultLanguage == NULL || defaultLanguage[0] == '\0') ? "N/A" : defaultLanguage));
}

static void handleOptionalProperty(const char* peerName, const char* key, const char* sig, const AJ_Arg* value) {
    if (strcmp(sig, "s") == 0) {
        AJ_AlwaysPrintf(("Optional Prop: %s=\"%s\"\n", key, value->val.v_string));
    } else {
        AJ_AlwaysPrintf(("Optional Prop: %s=[Not A String]\n", key));
    }
}

static uint8_t FoundNewTestPeer(uint16_t version, uint16_t port, const char* peerName, const char* objPath)
{
    AJ_AlwaysPrintf(("FoundNewTestPeer: version:%u port:%u name:%s path=%s\n", version, port, peerName, objPath));
    if ((strcmp(objPath, testObj) == 0) && (port == testServicePort)) {
        if (g_peerServiceName[0] == '\0') {
            strncpy(g_peerServiceName, peerName, AJ_MAX_NAME_SIZE);
            g_peerServiceName[AJ_MAX_NAME_SIZE] = '\0';
        }
    }

    return FALSE;
}

static uint8_t AcceptNewTestPeer(const char* peerName)
{
    AJ_AlwaysPrintf(("AcceptNewTestPeer: name:%s\n", peerName));
    if ((strcmp(g_peerServiceName, peerName) == 0)) {
        return TRUE;
    }

    return FALSE;
}

static const char* testIFaces[] = {
    "org.alljoyn.alljoyn_test",
    "org.alljoyn.alljoyn_test.values"
};

static AJ_AboutPeerDescription pingServicePeer = {
    testIFaces, (uint16_t)(sizeof(testIFaces) / sizeof(*testIFaces)), FoundNewTestPeer, AcceptNewTestPeer, NULL, handleMandatoryProps, handleOptionalProperty
};
#endif

/*
 * Let the application do some work
 */
static AJ_Status SendPing(AJ_BusAttachment* bus, uint32_t sessionId, const char* serviceName, unsigned int num);
static int32_t g_iterCount = 0;
static void AppDoWork(AJ_BusAttachment* bus, uint32_t sessionId, const char* serviceName)
{
    AJ_AlwaysPrintf(("AppDoWork\n"));
    /*
     * This function is called if there are no messages to unmarshal
     * Alternate between alljoyn_test ping and Bus ping
     */
    g_iterCount = g_iterCount + 1;
    if (g_iterCount & 1) {
        SendPing(bus, sessionId, serviceName, g_iterCount);
    } else {
        AJ_BusPing(bus, serviceName, PING_TIMEOUT);
    }
}

/*
 * The tests were changed at some point to make the PWD longer.
 * If doing backcompatibility testing with previous versions (14.08 or before),
 * define LITE_TEST_BACKCOMPAT to use the old version of the password.
 */
#ifndef LITE_TEST_BACKCOMPAT
static const char PWD[] = "faaa0af3dd3f1e0379da046a3ab6ca44";
#else
static const char PWD[] = "123456";
#endif

#if defined(SECURE_INTERFACE) || defined(SECURE_OBJECT)
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
 * Child first, then parent.
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

static const char PingString[] = "Ping String";

AJ_Status SendPing(AJ_BusAttachment* bus, uint32_t sessionId, const char* serviceName, unsigned int num)
{
    AJ_Status status;
    AJ_Message msg;

    /*
     * Since the object path on the proxy object entry was not set in the proxy object table above
     * it must be set before marshalling the method call.
     */
    status = AJ_SetProxyObjectPath(ProxyObjects, PRX_MY_PING, testObj);
    if (status == AJ_OK) {
        status = AJ_MarshalMethodCall(bus, &msg, PRX_MY_PING, serviceName, sessionId, 0, METHOD_TIMEOUT);
    }
    if (status == AJ_OK) {
        status = AJ_MarshalArgs(&msg, "s", PingString);
    }
    if (status == AJ_OK) {
        status = AJ_DeliverMsg(&msg);
    }
    return status;
}

AJ_Status SendGetProp(AJ_BusAttachment* bus, uint32_t sessionId, const char* serviceName)
{
    AJ_Status status;
    AJ_Message msg;

    status = AJ_MarshalMethodCall(bus, &msg, PRX_GET_PROP, serviceName, sessionId, 0, METHOD_TIMEOUT);
    if (status == AJ_OK) {
        AJ_MarshalPropertyArgs(&msg, PRX_GET_INT);
        status = AJ_DeliverMsg(&msg);
    }
    return status;
}

AJ_Status SendSetProp(AJ_BusAttachment* bus, uint32_t sessionId, const char* serviceName, int val)
{
    AJ_Status status;
    AJ_Message msg;

    status = AJ_MarshalMethodCall(bus, &msg, PRX_SET_PROP, serviceName, sessionId, 0, METHOD_TIMEOUT);
    if (status == AJ_OK) {
        status = AJ_MarshalPropertyArgs(&msg, PRX_SET_INT);

        if (status == AJ_OK) {
            status = AJ_MarshalArgs(&msg, "i", val);
        } else {
            AJ_AlwaysPrintf((">>>>>>>>In SendSetProp() AJ_MarshalPropertyArgs() returned status = 0x%04x\n", status));
        }

        if (status == AJ_OK) {
            status = AJ_DeliverMsg(&msg);
        } else {
            AJ_AlwaysPrintf((">>>>>>>>In SendSetProp() AJ_MarshalArgs() returned status = 0x%04x\n", status));
        }
    }

    return status;
}


#if defined(SECURE_INTERFACE) || defined(SECURE_OBJECT)
void AuthCallback(const void* context, AJ_Status status)
{
    *((AJ_Status*)context) = status;
}
#endif

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

#ifdef SECURE_INTERFACE
    uint32_t suites[16];
    size_t numsuites = 0;
    uint8_t clearkeys = FALSE;
    uint8_t enablepwd = FALSE;
    X509CertificateChain* node;
#endif

#ifdef MAIN_ALLOWS_ARGS
#if defined(SECURE_INTERFACE) || defined(SECURE_OBJECT)
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
#if defined (ANNOUNCE_BASED_DISCOVERY)
            status = AJ_StartClientByPeerDescription(&bus, NULL, CONNECT_TIMEOUT, FALSE, &pingServicePeer, testServicePort, &sessionId, g_peerServiceName, NULL);
#elif defined (NGNS)
            status = AJ_StartClientByInterface(&bus, NULL, CONNECT_TIMEOUT, FALSE, testInterfaceNames, &sessionId, g_peerServiceName, NULL);
#else
            status = AJ_StartClientByName(&bus, NULL, CONNECT_TIMEOUT, FALSE, testServiceName, testServicePort, &sessionId, NULL, g_peerServiceName);
#endif
            if (status == AJ_OK) {
                AJ_AlwaysPrintf(("StartClient returned %d, sessionId=%u, serviceName=%s\n", status, sessionId, g_peerServiceName));
                AJ_AlwaysPrintf(("Connected to Daemon:%s\n", AJ_GetUniqueName(&bus)));
                connected = TRUE;
#if defined(SECURE_INTERFACE) || defined(SECURE_OBJECT)
                if (enablepwd) {
                    AJ_BusSetPasswordCallback(&bus, PasswordCallback);
                }
                AJ_BusEnableSecurity(&bus, suites, numsuites);
                AJ_BusSetAuthListenerCallback(&bus, AuthListenerCallback);
                if (clearkeys) {
                    status = AJ_ClearCredentials();
                    AJ_ASSERT(AJ_OK == status);
                }
                status = AJ_BusAuthenticatePeer(&bus, g_peerServiceName, AuthCallback, &authStatus);
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


        AJ_AlwaysPrintf(("Auth status %d and AllJoyn status %d\n", authStatus, status));

        if (status == AJ_ERR_RESOURCES) {
            AJ_InfoPrintf(("Peer is busy, disconnecting and retrying auth...\n"));
            AJ_Disconnect(&bus);
            connected = FALSE;
            continue;
        }

        if (authStatus != AJ_ERR_NULL) {
            if (authStatus != AJ_OK) {
                AJ_Disconnect(&bus);
                break;
            }
            authStatus = AJ_ERR_NULL;
            AJ_BusSetLinkTimeout(&bus, sessionId, 10 * 1000);
        }

        status = AJ_UnmarshalMsg(&bus, &msg, UNMARSHAL_TIMEOUT);
        if (status != AJ_OK) {
            if (status == AJ_ERR_TIMEOUT) {
                AppDoWork(&bus, sessionId, g_peerServiceName);
                continue;
            }
        } else {
            switch (msg.msgId) {

            case AJ_REPLY_ID(AJ_METHOD_SET_LINK_TIMEOUT):
                {
                    uint32_t disposition;
                    uint32_t timeout;
                    status = AJ_UnmarshalArgs(&msg, "uu", &disposition, &timeout);
                    if (disposition == AJ_SETLINKTIMEOUT_SUCCESS) {
                        AJ_AlwaysPrintf(("Link timeout set to %d\n", timeout));
                    } else {
                        AJ_AlwaysPrintf(("SetLinkTimeout failed %d\n", disposition));
                    }
                    SendPing(&bus, sessionId, g_peerServiceName, 1);
                }
                break;

            case AJ_REPLY_ID(AJ_METHOD_BUS_PING):
                {
                    uint32_t disposition;
                    status = AJ_UnmarshalArgs(&msg, "u", &disposition);
                    if (disposition == AJ_PING_SUCCESS) {
                        AJ_AlwaysPrintf(("Bus Ping reply received\n"));
                    } else {
                        AJ_AlwaysPrintf(("Bus Ping failed, disconnecting: %d\n", disposition));
                        status = AJ_ERR_LINK_DEAD;
                    }
                }
                break;

            case AJ_REPLY_ID(PRX_MY_PING):
                {
                    AJ_Arg arg;
                    AJ_UnmarshalArg(&msg, &arg);
                    AJ_AlwaysPrintf(("Got ping reply\n"));
                    AJ_InfoPrintf(("INFO Got ping reply\n"));
                    status = SendGetProp(&bus, sessionId, g_peerServiceName);
                }
                break;

            case AJ_REPLY_ID(PRX_GET_PROP):
                {
                    const char* sig;
                    status = AJ_UnmarshalVariant(&msg, &sig);
                    if (status == AJ_OK) {
                        status = AJ_UnmarshalArgs(&msg, sig, &g_iterCount);
                        AJ_AlwaysPrintf(("Get prop reply %d\n", g_iterCount));

                        if (status == AJ_OK) {
                            g_iterCount = g_iterCount + 1;
                            status = SendSetProp(&bus, sessionId, g_peerServiceName, g_iterCount);
                        }
                    }
                }
                break;

            case AJ_REPLY_ID(PRX_SET_PROP):
                AJ_AlwaysPrintf(("Set prop reply\n"));
                break;

            case AJ_SIGNAL_SESSION_LOST_WITH_REASON:
                /*
                 * Force a disconnect
                 */
                {
                    uint32_t id, reason;
                    AJ_UnmarshalArgs(&msg, "uu", &id, &reason);
                    AJ_AlwaysPrintf(("Session lost. ID = %u, reason = %u\n", id, reason));
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
            break;
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
