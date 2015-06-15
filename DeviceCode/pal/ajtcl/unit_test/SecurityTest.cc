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

#include <gtest/gtest.h>

#define AJ_MODULE SECURITYTEST

extern "C" {

#include "alljoyn.h"
#include "aj_cert.h"
#include "aj_peer.h"
#include "aj_creds.h"
#include "aj_auth_listener.h"
#include "aj_authentication.h"
#include "aj_config.h"
#include "aj_crypto.h"
#include "aj_debug.h"

}

#ifndef NDEBUG
uint8_t dbgSECURITYTEST = 0;
#endif

#define CONNECT_TIMEOUT    (1000ul * 200)
#define UNMARSHAL_TIMEOUT  (1000 * 5)
#define METHOD_TIMEOUT     (1000 * 10)
#define PING_TIMEOUT       (1000 * 10)




/*Interface */
static const char* const Test1_Interface1[] = { "$org.alljoyn.alljoyn_test", "?my_ping inStr<s outStr>s", NULL };


static const AJ_InterfaceDescription Test1_Interfaces[] = { AJ_PropertiesIface, Test1_Interface1, NULL };

static const char testObj[] = "/org/alljoyn/alljoyn_test";


static const char intfc[] = "org.alljoyn.test";


static AJ_Object AppObjects[] = {
    { NULL, Test1_Interfaces },     /* Object path will be specified later */
    { NULL }
};

uint32_t TEST1_APP_MY_PING    = AJ_PRX_MESSAGE_ID(0, 1, 0);
/*
 * Default key expiration
 */
static const uint32_t keyexpiration = 0xFFFFFFFF;

static const char PWD[] = "123456";

static AJ_BusAttachment testBus;
static const char ServiceName[] = "org.alljoyn.svclite";

class SecurityTest : public testing::Test {
  public:

    SecurityTest() { authStatus = AJ_ERR_NULL; }

    static uint32_t PasswordCallback(uint8_t* buffer, uint32_t bufLen)
    {
        memcpy(buffer, PWD, sizeof(PWD));
        return sizeof(PWD) - 1;
    }

    static void AuthCallback(const void* context, AJ_Status status)
    {
        *((AJ_Status*)context) = status;
        ASSERT_EQ(AJ_OK, status) << "Auth callback returns fail" << AJ_StatusText(status);
    }

    AJ_Status authStatus;
};

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

static const char PingString[] = "Ping String";

void MakeMethodCall(int*count, uint32_t ID) {

    AJ_Message msg;
    AJ_Status status = AJ_OK;
    if (*count == 0) {
        *count = 1;
        status = AJ_MarshalMethodCall(&testBus, &msg, ID, ServiceName, 0, 0, 5000);
        ASSERT_EQ(AJ_OK, status) << "Cannot marshal method calls parameters" << AJ_StatusText(status);
        status = AJ_MarshalArgs(&msg, "s", PingString);
        ASSERT_EQ(AJ_OK, status) << "Cannot marshal method calls arguments" << AJ_StatusText(status);
        status = AJ_DeliverMsg(&msg);
        ASSERT_EQ(AJ_OK, status) << "Cannot deliver msg" << AJ_StatusText(status);
    }

}

/* Test for ECDHE_NULL  */

TEST_F(SecurityTest, Test1)
{

    // Register bus objects and proxy bus objects
    AJ_RegisterObjects(NULL, AppObjects);
    AJ_Status status = AJ_OK;
    int count = 0;
    AJ_Message msg;
    char*value;
    uint32_t suites[16];
    size_t numsuites = 0;

    AJ_Initialize();

    status = AJ_Connect(&testBus, NULL, CONNECT_TIMEOUT);
    ASSERT_EQ(AJ_OK, status) << "Unable to connect to the daemon. " << "The status returned is " << AJ_StatusText(status);
    if (AJ_OK == status) {
        AJ_Printf("Connected to the bus. The unique name is %s\n", AJ_GetUniqueName(&testBus));
    }

    suites[numsuites++] = AUTH_SUITE_ECDHE_NULL;
    AJ_BusEnableSecurity(&testBus, suites, numsuites);
    ASSERT_EQ(AJ_OK, status) << "Unable to enable security. " << "The status returned is " << AJ_StatusText(status);
    AJ_BusSetAuthListenerCallback(&testBus, AuthListenerCallback);

    status = AJ_BusAuthenticatePeer(&testBus, ServiceName, AuthCallback, &authStatus);


    while (TRUE) {
        status = AJ_SetProxyObjectPath(AppObjects, TEST1_APP_MY_PING, testObj);
        status = AJ_UnmarshalMsg(&testBus, &msg, UNMARSHAL_TIMEOUT);
        if (status == AJ_ERR_TIMEOUT) {
            if (authStatus == AJ_OK) {
                MakeMethodCall(&count, TEST1_APP_MY_PING);
            }
        } else if (msg.msgId == AJ_REPLY_ID(TEST1_APP_MY_PING)) {
            AJ_UnmarshalArgs(&msg, "s", &value);
            ASSERT_STREQ(PingString, value);
            AJ_CloseMsg(&msg);
            break;
        } else {
            status = AJ_BusHandleBusMessage(&msg);
        }


        AJ_CloseMsg(&msg);
    }
    AJ_ClearCredentials();
    ASSERT_EQ(AJ_OK, status) << "AJ_ClearCredentials returned status. " << AJ_StatusText(status);
    AJ_Disconnect(&testBus);
}



/* Test for ECDHE_PSK  */


TEST_F(SecurityTest, Test2)
{

    // Register bus objects and proxy bus objects
    AJ_RegisterObjects(NULL, AppObjects);
    AJ_Status status = AJ_OK;
    int count = 0;
    AJ_Message msg;
    char*value;
    uint32_t suites[16];
    size_t numsuites = 0;

    AJ_Initialize();


    status = AJ_Connect(&testBus, NULL, CONNECT_TIMEOUT);
    ASSERT_EQ(AJ_OK, status) << "Unable to connect to the daemon. " << "The status returned is " << AJ_StatusText(status);
    if (AJ_OK == status) {
        AJ_Printf("Connected to the bus. The unique name is %s\n", AJ_GetUniqueName(&testBus));
    }

    suites[numsuites++] = AUTH_SUITE_ECDHE_PSK;
    AJ_BusEnableSecurity(&testBus, suites, numsuites);
    ASSERT_EQ(AJ_OK, status) << "Unable to enable security. " << "The status returned is " << AJ_StatusText(status);
    AJ_BusSetAuthListenerCallback(&testBus, AuthListenerCallback);
    status = AJ_BusAuthenticatePeer(&testBus, ServiceName, AuthCallback, &authStatus);

    while (TRUE) {
        status = AJ_SetProxyObjectPath(AppObjects, TEST1_APP_MY_PING, testObj);
        status = AJ_UnmarshalMsg(&testBus, &msg, UNMARSHAL_TIMEOUT);
        if (status == AJ_ERR_TIMEOUT) {
            if (authStatus == AJ_OK) {
                MakeMethodCall(&count, TEST1_APP_MY_PING);
            }
        } else if (msg.msgId == AJ_REPLY_ID(TEST1_APP_MY_PING)) {
            AJ_UnmarshalArgs(&msg, "s", &value);
            ASSERT_STREQ(PingString, value);
            AJ_CloseMsg(&msg);
            break;
        } else {
            status = AJ_BusHandleBusMessage(&msg);
        }


        AJ_CloseMsg(&msg);
    }
    AJ_ClearCredentials();
    ASSERT_EQ(AJ_OK, status) << "AJ_ClearCredentials returned status. " << AJ_StatusText(status);
    AJ_Disconnect(&testBus);

}


/* Test for ECDHE_ECDSA  */

TEST_F(SecurityTest, Test3)
{
    // Register bus objects and proxy bus objects
    AJ_RegisterObjects(NULL, AppObjects);
    AJ_Status status = AJ_OK;
    int count = 0;
    AJ_Message msg;
    char*value;
    uint32_t suites[16];
    size_t numsuites = 0;

    AJ_Initialize();

    status = AJ_Connect(&testBus, NULL, CONNECT_TIMEOUT);
    ASSERT_EQ(AJ_OK, status) << "Unable to connect to the daemon" << "The status returned is " << AJ_StatusText(status);
    if (AJ_OK == status) {
        AJ_Printf("Connected to the bus. The unique name is %s\n", AJ_GetUniqueName(&testBus));
    }

    suites[numsuites++] = AUTH_SUITE_ECDHE_ECDSA;
    AJ_BusEnableSecurity(&testBus, suites, numsuites);
    ASSERT_EQ(AJ_OK, status) << "Unable to enable security" << "The status returned is " << AJ_StatusText(status);
    AJ_BusSetAuthListenerCallback(&testBus, AuthListenerCallback);
    status = AJ_BusAuthenticatePeer(&testBus, ServiceName, AuthCallback, &authStatus);

    while (TRUE) {
        status = AJ_SetProxyObjectPath(AppObjects, TEST1_APP_MY_PING, testObj);
        status = AJ_UnmarshalMsg(&testBus, &msg, UNMARSHAL_TIMEOUT);
        if (status == AJ_ERR_TIMEOUT) {
            if (authStatus == AJ_OK) {
                MakeMethodCall(&count, TEST1_APP_MY_PING);
            }
        } else if (msg.msgId == AJ_REPLY_ID(TEST1_APP_MY_PING)) {
            AJ_UnmarshalArgs(&msg, "s", &value);
            ASSERT_STREQ(PingString, value);
            AJ_CloseMsg(&msg);
            break;
        } else {
            status = AJ_BusHandleBusMessage(&msg);
        }


        AJ_CloseMsg(&msg);
    }

    AJ_ClearCredentials();
    ASSERT_EQ(AJ_OK, status) << "AJ_ClearCredentials returned status. " << AJ_StatusText(status);
    AJ_Disconnect(&testBus);

}
