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
#define AJ_MODULE SECURE_CLIENT

#include <stdio.h>
#include <stdlib.h>
#include <assert.h>

#include "alljoyn.h"
#include "aj_debug.h"
#include "aj_crypto.h"
#include "aj_crypto_ecc.h"
#include "aj_creds.h"
#include "aj_cert.h"
#include "aj_peer.h"
#include "aj_auth_listener.h"
#include "aj_util.h"

uint8_t dbgSECURE_CLIENT = 0;

/*
 * Default key expiration
 */
static const uint32_t keyexpiration = 0xFFFFFFFF;

static const char ServiceName[] = "org.alljoyn.bus.samples.secure";
static const char InterfaceName[] = "org.alljoyn.bus.samples.secure.SecureInterface";
static const char ServicePath[] = "/SecureService";
static const uint16_t ServicePort = 42;

/*
 * Buffer to hold the full service name. This buffer must be big enough to hold
 * a possible 255 characters plus a null terminator (256 bytes)
 */
static char fullServiceName[AJ_MAX_SERVICE_NAME_SIZE];

static const char* const secureInterface[] = {
    "$org.alljoyn.bus.samples.secure.SecureInterface",
    "?Ping inStr<s outStr>s",
    NULL
};

static const AJ_InterfaceDescription secureInterfaces[] = {
    secureInterface,
    NULL
};

/**
 * Objects implemented by the application
 */
static const AJ_Object ProxyObjects[] = {
    { ServicePath, secureInterfaces },
    { NULL }
};

#define PRX_PING   AJ_PRX_MESSAGE_ID(0, 0, 0)

/*
 * Let the application do some work
 */
static void AppDoWork()
{
}

/*
 * get a line of input from the file pointer (most likely stdin).
 * This will capture the the num-1 characters or till a newline character is
 * entered.
 *
 * @param[out] str a pointer to a character array that will hold the user input
 * @param[in]  num the size of the character array 'str'
 * @param[in]  fp the file pointer the sting will be read from. (most likely stdin)
 *
 * @return returns the length of the string received from the file.
 */
uint32_t get_line(char*str, int num, FILE*fp)
{
    uint32_t stringLength = 0;
    char*p = fgets(str, num, fp);

    // fgets will capture the '\n' character if the string entered is shorter than
    // num. Remove the '\n' from the end of the line and replace it with nul '\0'.
    if (p != NULL) {
        stringLength = (uint32_t)strlen(str) - 1;
        if (str[stringLength] == '\n') {
            str[stringLength] = '\0';
        }
    }

    return stringLength;
}

#define CONNECT_TIMEOUT    (1000 * 200)
#define UNMARSHAL_TIMEOUT  (1000 * 5)
#define METHOD_TIMEOUT     (100 * 10)

static char pingString[] = "Client AllJoyn Lite says Hello AllJoyn!";

AJ_Status SendPing(AJ_BusAttachment* bus, uint32_t sessionId)
{
    AJ_Status status;
    AJ_Message msg;

    AJ_Printf("Sending ping request '%s'.\n", pingString);

    status = AJ_MarshalMethodCall(bus,
                                  &msg,
                                  PRX_PING,
                                  fullServiceName,
                                  sessionId,
                                  AJ_FLAG_ENCRYPTED,
                                  METHOD_TIMEOUT);
    if (AJ_OK == status) {
        status = AJ_MarshalArgs(&msg, "s", pingString);
    } else {
        AJ_InfoPrintf(("In SendPing() AJ_MarshalMethodCall() status = %d.\n", status));
    }

    if (AJ_OK == status) {
        status = AJ_DeliverMsg(&msg);
    } else {
        AJ_InfoPrintf(("In SendPing() AJ_MarshalArgs() status = %d.\n", status));
    }

    if (AJ_OK != status) {
        AJ_InfoPrintf(("In SendPing() AJ_DeliverMsg() status = %d.\n", status));
    }

    return status;
}

//static const char psk_b64[] = "EBESExQVFhcYGRobHB0eHw==";
//static uint8_t psk[16];
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
    for (i = 0; i < 2; i++) {
        if (0 == strncmp(issuer, issuers[i], strlen(issuers[i]))) {
            return AJ_OK;
        }
    }
    return AJ_ERR_SECURITY;
}

static AJ_Status AuthListenerCallback(uint32_t authmechanism, uint32_t command, AJ_Credential* cred)
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

void AuthCallback(const void* context, AJ_Status status)
{
    *((AJ_Status*)context) = status;
}

int AJ_Main(int ac, char** av)
{
    int done = FALSE;
    AJ_Status status = AJ_OK;
    AJ_BusAttachment bus;
    uint8_t connected = FALSE;
    uint32_t sessionId = 0;
    AJ_Status authStatus = AJ_ERR_NULL;

    uint32_t suites[16];
    size_t numsuites = 0;
    uint8_t clearkeys = FALSE;

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
        } else {
            AJ_Printf("SecureClientECDHE [-e|-ek] <encryption suite>\n"
                      "-e <encryption suite>\n"
                      "   Specify an encryption suite to use: ECDHE_ECDSA, ECDHE_PSK, or ECDHE_NULL\n"
                      "   -e can be specified multiple times to support multiple encryption suites\n"
                      "-ek <encryption suite>\n"
                      "    Same as -e, except that any existing authentication keys are cleared. This \n"
                      "    will ensure a new key exchange/password validation occurs\n");
            return AJ_ERR_NULL;
        }
        if (!ac) {
            AJ_Printf("-e(k) requires an auth mechanism.\n");
            return 1;
        }
        while (ac) {
            if (0 == strncmp(*av, "ECDHE_ECDSA", 11)) {
                suites[numsuites++] = AUTH_SUITE_ECDHE_ECDSA;
            } else if (0 == strncmp(*av, "ECDHE_PSK", 9)) {
                suites[numsuites++] = AUTH_SUITE_ECDHE_PSK;
            } else if (0 == strncmp(*av, "ECDHE_NULL", 10)) {
                suites[numsuites++] = AUTH_SUITE_ECDHE_NULL;
            }
            ac--;
            av++;
        }
    }

    /*
     * One time initialization before calling any other AllJoyn APIs
     */
    AJ_Initialize();

    AJ_PrintXML(ProxyObjects);
    AJ_RegisterObjects(NULL, ProxyObjects);

    while (!done) {
        AJ_Message msg;

        if (!connected) {
            status = AJ_StartClientByName(&bus, NULL, CONNECT_TIMEOUT, FALSE, ServiceName, ServicePort, &sessionId, NULL, fullServiceName);
            if (status == AJ_OK) {
                AJ_InfoPrintf(("StartClient returned %d, sessionId=%u\n", status, sessionId));
                AJ_Printf("StartClient returned %d, sessionId=%u\n", status, sessionId);
                connected = TRUE;
                AJ_BusEnableSecurity(&bus, suites, numsuites);
                AJ_BusSetAuthListenerCallback(&bus, AuthListenerCallback);
                if (clearkeys) {
                    status = AJ_ClearCredentials();
                    AJ_ASSERT(AJ_OK == status);
                }

                status = AJ_BusAuthenticatePeer(&bus, fullServiceName, AuthCallback, &authStatus);
                if (status != AJ_OK) {
                    AJ_Printf("AJ_BusAuthenticatePeer returned %d\n", status);
                    break;
                }
            } else {
                AJ_InfoPrintf(("StartClient returned %d\n", status));
                AJ_Printf("StartClient returned %d\n", status);
                break;
            }
        }

        if (authStatus != AJ_ERR_NULL) {
            if (authStatus != AJ_OK) {
                AJ_Disconnect(&bus);
                break;
            }
            authStatus = AJ_ERR_NULL;
            status = SendPing(&bus, sessionId);
            if (status != AJ_OK) {
                AJ_Printf("SendPing returned %d\n", status);
                continue;
            }
        }

        status = AJ_UnmarshalMsg(&bus, &msg, UNMARSHAL_TIMEOUT);

        if (AJ_ERR_TIMEOUT == status) {
            AppDoWork();
            continue;
        }

        if (AJ_OK == status) {
            switch (msg.msgId) {
            case AJ_REPLY_ID(PRX_PING):
                {
                    AJ_Arg arg;

                    if (AJ_OK == AJ_UnmarshalArg(&msg, &arg)) {
                        AJ_Printf("%s.Ping (path=%s) returned \"%s\".\n", InterfaceName,
                                  ServicePath, arg.val.v_string);

                        if (strcmp(arg.val.v_string, pingString) == 0) {
                            AJ_InfoPrintf(("Ping was successful.\n"));
                        } else {
                            AJ_InfoPrintf(("Ping returned different string.\n"));
                        }
                    } else {
                        AJ_ErrPrintf(("Bad ping response.\n"));
                    }

                    done = TRUE;
                }
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

        if (status == AJ_ERR_READ) {
            AJ_Printf("AllJoyn disconnect.\n");
            AJ_Disconnect(&bus);
            exit(0);
        }
    }

    AJ_Printf("SecureClient EXIT %d.\n", status);

    return status;
}

#ifdef AJ_MAIN
int main(int argc, char** argv)
{
    return AJ_Main(argc, argv);
}

#endif
