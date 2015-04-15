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

//static const char psk_b64[] = "EBESExQVFhcYGRobHB0eHw==";
//static uint8_t psk[16];
static const char psk_hint[] = "bob";
static const char psk_char[] = "123456";
static const char ecc_pub_b64[] = "C/KGAyLE5jyVqHEipZBhPb7Ahj/MBdNLtpDvT9OJ0LYAAAAAn8QabXetJcPD7OWmEB6uXGXh+ftJOLlCTJhAHjTJsDkAAAAAAAAAAA==";
static const char ecc_prv_b64[] = "wxieVOfgCMgys3m+V82eV/B/p0WlIMu8fizZiqMQnYsAAAAA";
static const char owner_cert1_b64[] = "\
AAAAAUQn+YoXptNlRV7yGvBFSdQiXCZ7N3pdCB/h+dbNO63NAAAAAJ+iiYn1Dx9a\
EZjy6MgRxgoO/zU/JFuuxg/JEI2Yf1PwAAAAAAAAAAAL8oYDIsTmPJWocSKlkGE9\
vsCGP8wF00u2kO9P04nQtgAAAACfxBptd60lw8Ps5aYQHq5cZeH5+0k4uUJMmEAe\
NMmwOQAAAAAAAAAAAAAAAAAAAAAAAAAA/////wBOnWRZjvJdd9adaDleMIDQJOJC\
OuSepUTdfamDakEy/s6dN/ePP+iDV96kBT0XkQfNKiyfGbPf+ux6a2mx48/rAAAA\
AGfrER3HqAGYic+k8B/iIWUyJy414G+4+tTklxFAatmmAAAAAA==";
static const char owner_cert2_b64[] = "\
AAAAAkQn+YoXptNlRV7yGvBFSdQiXCZ7N3pdCB/h+dbNO63NAAAAAJ+iiYn1Dx9a\
EZjy6MgRxgoO/zU/JFuuxg/JEI2Yf1PwAAAAAAAAAAAL8oYDIsTmPJWocSKlkGE9\
vsCGP8wF00u2kO9P04nQtgAAAACfxBptd60lw8Ps5aYQHq5cZeH5+0k4uUJMmEAe\
NMmwOQAAAAAAAAAAAAAAAAAAAAAAAAAA/////wD5/PM2YlgaDcbxM2GD2BntTp1k\
WY7yXXfWnWg5XjCA0CTiQjrknqVE3X2pg2pBMv7ZCwVue216z7QXomTSt4nPyFum\
tj2XcycgTidW60XeVAAAAADCAWDa119gVqq2GOiteOKBaM7huRPUOl+ytTMQQpCj\
WAAAAAA=";
static ecc_publickey ecc_pub;
static ecc_privatekey ecc_prv;
static AJ_Certificate root_cert;

static const char* issuers[] = {
    "RCf5ihem02VFXvIa8EVJ1CJcJns3el0IH+H51s07rc0AAAAAn6KJifUPH1oRmPLoyBHGCg7/NT8kW67GD8kQjZh/U/AAAAAAAAAAAA==",
    "nUsoaWelVW1XhJrVNQuzEYlH0LndSrkAfd/GrEmM11gAAAAAChtt28EprD14ejHuj181s3m6y5nDxeRI9KaKmKRgI8kAAAAAAAAAAA=="
};

static AJ_Status IsTrustedIssuer(const char* issuer)
{
    size_t i;
    for (i = 0; i < 2; i++) {
        if (0 == strncmp(issuer, issuers[i], strlen(issuers[i]))) {
            return AJ_OK;
        }
    }
    return AJ_ERR_SECURITY;
}

static AJ_Status AuthListenerCallback(uint32_t authmechanism, uint32_t command, AJ_Credential*cred)
{
    AJ_Status status = AJ_ERR_INVALID;
    uint8_t b8[sizeof (AJ_Certificate)];
    char b64[400];
    AJ_Printf("AuthListenerCallback authmechanism %d command %d\n", authmechanism, command);

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
                AJ_Printf("Request Credentials for PSK ID: %s\n", cred->data);
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
            status = AJ_B64ToRaw(ecc_pub_b64, strlen(ecc_pub_b64), b8, sizeof (b8));
            AJ_ASSERT(AJ_OK == status);
            status = AJ_BigEndianDecodePublicKey(&ecc_pub, b8);
            AJ_ASSERT(AJ_OK == status);
            cred->mask = AJ_CRED_PUB_KEY;
            cred->data = (uint8_t*) &ecc_pub;
            cred->len = sizeof (ecc_pub);
            cred->expiration = keyexpiration;
            break;

        case AJ_CRED_PRV_KEY:
            status = AJ_B64ToRaw(ecc_prv_b64, strlen(ecc_prv_b64), b8, sizeof (b8));
            AJ_ASSERT(AJ_OK == status);
            status = AJ_BigEndianDecodePrivateKey(&ecc_prv, b8);
            AJ_ASSERT(AJ_OK == status);
            cred->mask = AJ_CRED_PRV_KEY;
            cred->data = (uint8_t*) &ecc_prv;
            cred->len = sizeof (ecc_prv);
            cred->expiration = keyexpiration;
            break;

        case AJ_CRED_CERT_CHAIN:
            status = AJ_B64ToRaw(owner_cert1_b64, strlen(owner_cert1_b64), b8, sizeof (b8));
            AJ_ASSERT(AJ_OK == status);
            status = AJ_BigEndianDecodeCertificate(&root_cert, b8, sizeof (b8));
            AJ_ASSERT(AJ_OK == status);
            cred->mask = AJ_CRED_CERT_CHAIN;
            cred->data = (uint8_t*) &root_cert;
            cred->len = sizeof (root_cert);
            break;

        case AJ_CRED_CERT_TRUST:
            status = AJ_RawToB64(cred->data, cred->len, b64, sizeof (b64));
            AJ_ASSERT(AJ_OK == status);
            status = IsTrustedIssuer(b64);
            AJ_Printf("TRUST: %s %d\n", b64, status);
            break;

        case AJ_CRED_CERT_ROOT:
            status = AJ_RawToB64(cred->data, cred->len, b64, sizeof (b64));
            AJ_ASSERT(AJ_OK == status);
            AJ_Printf("ROOT: %s\n", b64);
            status = AJ_OK;
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

    return status;
}

#ifdef AJ_MAIN
int main(int argc, char** argv)
{
    return AJ_Main(argc, argv);
}
#endif
