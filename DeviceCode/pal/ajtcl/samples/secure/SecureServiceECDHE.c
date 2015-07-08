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
#define AJ_MODULE SECURE_SERVICE

#include <stdio.h>
#include <stdlib.h>
#include <time.h>
#include <aj_debug.h>
#include "alljoyn.h"
#include "aj_crypto_ecc.h"
#include "aj_cert.h"
#include "aj_peer.h"
#include "aj_creds.h"
#include "aj_auth_listener.h"
#include "aj_authentication.h"
#include "aj_util.h"

uint8_t dbgSECURE_SERVICE = 1;

/*
 * Default key expiration
 */
static const uint32_t keyexpiration = 0xFFFFFFFF;

#define CONNECT_ATTEMPTS   10
static const char ServiceName[] = "org.alljoyn.bus.samples.secure";
static const char InterfaceName[] = "org.alljoyn.bus.samples.secure.SecureInterface";
static const char ServicePath[] = "/SecureService";
static const uint16_t ServicePort = 42;


/**
 * The interface name followed by the method signatures.
 *
 * See also .\inc\aj_introspect.h
 */
static const char* const secureInterface[] = {
    "$org.alljoyn.bus.samples.secure.SecureInterface",
    "?Ping inStr<s outStr>s",  /* Method at index 0. */
    NULL
};

/**
 * A NULL terminated collection of all interfaces.
 */
static const AJ_InterfaceDescription secureInterfaces[] = {
    secureInterface,
    NULL
};

/**
 * Objects implemented by the application. The first member in the AJ_Object structure is the path.
 * The second is the collection of all interfaces at that path.
 */
static const AJ_Object AppObjects[] = {
    { ServicePath, secureInterfaces },
    { NULL }
};

/*
 * The value of the arguments are the indices of the
 * object path in AppObjects (above), interface in sampleInterfaces (above), and
 * member indices in the interface.
 * The 'ping' index is 0 because the first entry in sampleInterface is the interface name.
 * This makes the first index (index 0 of the methods) the second string in
 * secureInterfaces[].
 *
 * See also .\inc\aj_introspect.h
 */
#define BASIC_SERVICE_PING AJ_APP_MESSAGE_ID(0, 0, 0)

static AJ_Status AppHandlePing(AJ_Message* msg)
{
    AJ_Status status;
    AJ_Message reply;
    AJ_Arg arg;

    status = AJ_UnmarshalArg(msg, &arg);

    if (AJ_OK == status) {

        if (arg.typeId == AJ_ARG_STRING) {
            AJ_Printf("Received ping request '%s'.\n", arg.val.v_string);
        } else {
            AJ_Printf("Unexpected arg type '%d' in ping request.\n", arg.typeId);
        }

        status = AJ_MarshalReplyMsg(msg, &reply);

        if (AJ_OK == status) {
            /*
             * Just return the arg we received
             */
            status = AJ_MarshalArg(&reply, &arg);

            if (AJ_OK == status) {
                status = AJ_DeliverMsg(&reply);
            }
        }
    }

    return status;
}

// Copied from alljoyn/alljoyn_core/test/bbservice.cc
static const char pem_prv[] = {
    "-----BEGIN EC PRIVATE KEY-----"
    "MDECAQEEIICSqj3zTadctmGnwyC/SXLioO39pB1MlCbNEX04hjeioAoGCCqGSM49"
    "AwEH"
    "-----END EC PRIVATE KEY-----"
};

static const char pem_x509[] = {
    "-----BEGIN CERTIFICATE-----"
    "MIIBWjCCAQGgAwIBAgIHMTAxMDEwMTAKBggqhkjOPQQDAjArMSkwJwYDVQQDDCAw"
    "ZTE5YWZhNzlhMjliMjMwNDcyMGJkNGY2ZDVlMWIxOTAeFw0xNTAyMjYyMTU1MjVa"
    "Fw0xNjAyMjYyMTU1MjVaMCsxKTAnBgNVBAMMIDZhYWM5MjQwNDNjYjc5NmQ2ZGIy"
    "NmRlYmRkMGM5OWJkMFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEP/HbYga30Afm"
    "0fB6g7KaB5Vr5CDyEkgmlif/PTsgwM2KKCMiAfcfto0+L1N0kvyAUgff6sLtTHU3"
    "IdHzyBmKP6MQMA4wDAYDVR0TBAUwAwEB/zAKBggqhkjOPQQDAgNHADBEAiAZmNVA"
    "m/H5EtJl/O9x0P4zt/UdrqiPg+gA+wm0yRY6KgIgetWANAE2otcrsj3ARZTY/aTI"
    "0GOQizWlQm8mpKaQ3uE="
    "-----END CERTIFICATE-----"
};

static const char psk_hint[] = "<anonymous>";
static const char psk_char[] = "faaa0af3dd3f1e0379da046a3ab6ca44";
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

static const uint32_t suites[3] = { AUTH_SUITE_ECDHE_ECDSA, AUTH_SUITE_ECDHE_PSK, AUTH_SUITE_ECDHE_NULL };
static const size_t numsuites = 3;

/* All times are expressed in milliseconds. */
#define CONNECT_TIMEOUT     (1000 * 60)
#define UNMARSHAL_TIMEOUT   (1000 * 5)
#define SLEEP_TIME          (1000 * 2)

int AJ_Main(int argc, char** argv)
{
    AJ_Status status = AJ_OK;
    AJ_BusAttachment bus;
    uint8_t connected = FALSE;
    uint32_t sessionId = 0;
    X509CertificateChain* node;

    /* One time initialization before calling any other AllJoyn APIs. */
    AJ_Initialize();

    /* This is for debug purposes and is optional. */
    AJ_PrintXML(AppObjects);
    AJ_RegisterObjects(AppObjects, NULL);

    while (TRUE) {
        AJ_Message msg;

        if (!connected) {
            status = AJ_StartService(&bus,
                                     NULL,
                                     CONNECT_TIMEOUT,
                                     FALSE,
                                     ServicePort,
                                     ServiceName,
                                     AJ_NAME_REQ_DO_NOT_QUEUE,
                                     NULL);

            if (status != AJ_OK) {
                continue;
            }

            AJ_InfoPrintf(("StartService returned %d, session_id=%u\n", status, sessionId));
            connected = TRUE;

            AJ_BusEnableSecurity(&bus, suites, numsuites);
            AJ_BusSetAuthListenerCallback(&bus, AuthListenerCallback);
        }

        status = AJ_UnmarshalMsg(&bus, &msg, UNMARSHAL_TIMEOUT);

        if (AJ_ERR_TIMEOUT == status) {
            continue;
        }

        if (AJ_OK == status) {
            switch (msg.msgId) {
            case AJ_METHOD_ACCEPT_SESSION:
                {
                    uint16_t port;
                    char* joiner;
                    AJ_UnmarshalArgs(&msg, "qus", &port, &sessionId, &joiner);
                    status = AJ_BusReplyAcceptSession(&msg, TRUE);
                    AJ_InfoPrintf(("Accepted session session_id=%u joiner=%s\n", sessionId, joiner));
                }
                break;

            case BASIC_SERVICE_PING:
                status = AppHandlePing(&msg);
                break;

            case AJ_SIGNAL_SESSION_LOST_WITH_REASON:
                /* Force a disconnect. */
                {
                    uint32_t id, reason;
                    AJ_UnmarshalArgs(&msg, "uu", &id, &reason);
                    AJ_AlwaysPrintf(("Session lost. ID = %u, reason = %u", id, reason));
                }
                status = AJ_ERR_SESSION_LOST;
                break;

            default:
                /* Pass to the built-in handlers. */
                status = AJ_BusHandleBusMessage(&msg);
                break;
            }
        }

        /* Messages MUST be discarded to free resources. */
        AJ_CloseMsg(&msg);

        if (status == AJ_ERR_READ) {
            AJ_Printf("AllJoyn disconnect.\n");
            AJ_Disconnect(&bus);
            connected = FALSE;

            /* Sleep a little while before trying to reconnect. */
            AJ_Sleep(SLEEP_TIME);
        }
    }

    AJ_Printf("Secure service exiting with status 0x%04x.\n", status);

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
int main(int argc, char** argv)
{
    return AJ_Main(argc, argv);
}
#endif
