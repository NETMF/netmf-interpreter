/*
 * @file
 */

/******************************************************************************
 * Copyright (c) 2013-2014, AllSeen Alliance. All rights reserved.
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

extern "C" {
#include "aj_debug.h"
#include "alljoyn.h"
#include "aj_cert.h"
#include "aj_peer.h"
#include "aj_creds.h"
#include "aj_auth_listener.h"
#include "aj_keyexchange.h"
#include "aj_keyauthentication.h"
#include "aj_config.h"
#include "aj_crypto.h"

}

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



static const char psk_hint[] = "bob";
static const char psk_char[] = "123456";

/*
 * The following public keys, private keys and certificates are generated from test 4,5,6 and 7.
 */
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
static size_t num = 2;
static size_t i;
static uint8_t* b8;
static char* pem;
static size_t pemlen;
static ecc_privatekey root_prvkey;
static ecc_publickey root_pubkey;
static uint8_t* manifest;
static size_t manifestlen;
static uint8_t digest[SHA256_DIGEST_LENGTH];
static ecc_privatekey peer_prvkey;
static ecc_publickey peer_pubkey;
static AJ_Certificate* cert;
static AJ_GUID guild;

/*
 * These are trusted peers namely svclite.c and bbservice.cc
 */
static const char* issuers[] = {
    "RCf5ihem02VFXvIa8EVJ1CJcJns3el0IH+H51s07rc0AAAAAn6KJifUPH1oRmPLoyBHGCg7/NT8kW67GD8kQjZh/U/AAAAAAAAAAAA==",
    "9RB2ExIO4VZqEwb+sWYVsozToGMgDZJzH0Yf4Q0sCC0AAAAAhuEeeMDIXKzoOg3aQqVdUKC0ekWIRizM5hcjzxAO8LUAAAAAAAAAAA==",
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

static void CreateManifest(uint8_t** manifest, size_t* len)
{
    *len = strlen(intfc);
    *manifest = (uint8_t*) AJ_Malloc(*len);
    AJ_ASSERT(*manifest);
    memcpy(*manifest, (uint8_t*) intfc, *len);
}

static void ManifestDigest(uint8_t* manifest, size_t* len, uint8_t* digest)
{
    AJ_SHA256_Context sha;
    AJ_SHA256_Init(&sha);
    AJ_SHA256_Update(&sha, (const uint8_t*) manifest, *len);
    AJ_SHA256_Final(&sha, digest);
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

/*  Test for generating owner public & private key pair  */
TEST_F(SecurityTest, Test4)
{

    AJ_Status status = AJ_OK;
    /*
     * Create an owner key pair
     */
    AJ_GenerateDSAKeyPair(&root_pubkey, &root_prvkey);

    b8 = (uint8_t*) AJ_Malloc(sizeof (ecc_publickey));
    AJ_ASSERT(b8);
    status = AJ_BigEndianEncodePublicKey(&root_pubkey, b8);
    ASSERT_EQ(AJ_OK, status) << "AJ_BigEndianEncodePublicKey returned status. " << AJ_StatusText(status);
    pemlen = 4 * ((sizeof (ecc_publickey) + 2) / 3) + 1;
    pem = (char*) AJ_Malloc(pemlen);
    status = AJ_RawToB64(b8, sizeof (ecc_publickey), pem, pemlen);
    ASSERT_EQ(AJ_OK, status) << "AJ_RawToB64 returned status. " << AJ_StatusText(status);
    AJ_Printf("Owner Public Key\n");
    AJ_Printf("-----BEGIN PUBLIC KEY-----\n%s\n-----END PUBLIC KEY-----\n", pem);

}


/*  Test for generating peer public & private key pair  */
TEST_F(SecurityTest, Test5)
{

    AJ_Status status = AJ_OK;

    AJ_RandBytes((uint8_t*) &guild, sizeof (AJ_GUID));

    for (i = 0; i < num; i++) {
        AJ_GenerateDSAKeyPair(&peer_pubkey, &peer_prvkey);

        b8 = (uint8_t*) AJ_Malloc(sizeof (ecc_publickey));
        AJ_ASSERT(b8);
        status = AJ_BigEndianEncodePublicKey(&peer_pubkey, b8);
        ASSERT_EQ(AJ_OK, status) << "AJ_BigEndianEncodePublicKey returned status. " << AJ_StatusText(status);
        pemlen = 4 * ((sizeof (ecc_publickey) + 2) / 3) + 1;
        pem = (char*) AJ_Malloc(pemlen);
        status = AJ_RawToB64(b8, sizeof (ecc_publickey), pem, pemlen);
        ASSERT_EQ(AJ_OK, status) << "AJ_RawToB64 returned status. " << AJ_StatusText(status);
        AJ_Printf("Peer Public Key\n");
        AJ_Printf("-----BEGIN PUBLIC KEY-----\n%s\n-----END PUBLIC KEY-----\n", pem);
        AJ_Free(b8);
        AJ_Free(pem);

        b8 = (uint8_t*) AJ_Malloc(sizeof (ecc_privatekey));
        AJ_ASSERT(b8);
        status = AJ_BigEndianEncodePrivateKey(&peer_prvkey, b8);
        ASSERT_EQ(AJ_OK, status) << "AJ_BigEndianEncodePrivateKey returned status. " << AJ_StatusText(status);
        pemlen = 4 * ((sizeof (ecc_privatekey) + 2) / 3) + 1;
        pem = (char*) AJ_Malloc(pemlen);
        status = AJ_RawToB64(b8, sizeof (ecc_privatekey), pem, pemlen);
        ASSERT_EQ(AJ_OK, status) << "AJ_RawToB64 returned status. " << AJ_StatusText(status);
        AJ_Printf("Peer Private Key\n");
        AJ_Printf("-----BEGIN PRIVATE KEY-----\n%s\n-----END PRIVATE KEY-----\n", pem);
        AJ_Free(b8);
        AJ_Free(pem);

    }

}


/*  Test for generating peer certificate  */

TEST_F(SecurityTest, Test6)
{

    AJ_Status status = AJ_OK;

    AJ_RandBytes((uint8_t*) &guild, sizeof (AJ_GUID));

    for (i = 0; i < num; i++) {
        cert = (AJ_Certificate*) AJ_Malloc(sizeof (AJ_Certificate));
        AJ_ASSERT(cert);
        status = AJ_CreateCertificate(cert, 0, &peer_pubkey, NULL, NULL, digest, 0);
        ASSERT_EQ(AJ_OK, status) << "AJ_CreateCertificate returned status. " << AJ_StatusText(status);
        status = AJ_SignCertificate(cert, &peer_prvkey);
        ASSERT_EQ(AJ_OK, status) << "AJ_SignCertificate returned status. " << AJ_StatusText(status);
        status = AJ_VerifyCertificate(cert);
        ASSERT_EQ(AJ_OK, status) << "AJ_VerifyCertificate returned status. " << AJ_StatusText(status);


        b8 = (uint8_t*) AJ_Malloc(sizeof (AJ_Certificate));
        AJ_ASSERT(b8);
        status = AJ_BigEndianEncodeCertificate(cert, b8, sizeof (AJ_Certificate));
        ASSERT_EQ(AJ_OK, status) << "AJ_BigEndianEncodeCertificate returned status. " << AJ_StatusText(status);
        pemlen = 4 * ((sizeof (AJ_Certificate) + 2) / 3) + 1;
        pem = (char*) AJ_Malloc(pemlen);
        status = AJ_RawToB64(b8, cert->size, pem, pemlen);
        ASSERT_EQ(AJ_OK, status) << "AJ_RawToB64 returned status. " << AJ_StatusText(status);
        AJ_Printf("Peer Certificate (Type 0)\n");
        AJ_Printf("-----BEGIN CERTIFICATE-----\n%s\n-----END CERTIFICATE-----\n", pem);
    }
}


/*  Test for generating owner type 1 certificate  */

TEST_F(SecurityTest, Test7)
{

    AJ_Status status = AJ_OK;

    AJ_RandBytes((uint8_t*) &guild, sizeof (AJ_GUID));

    for (i = 0; i < num; i++) {
        cert = (AJ_Certificate*) AJ_Malloc(sizeof (AJ_Certificate));
        AJ_ASSERT(cert);
        status = AJ_CreateCertificate(cert, 1, &root_pubkey, &peer_pubkey, NULL, digest, 0);
        ASSERT_EQ(AJ_OK, status) << "AJ_CreateCertificate returned status. " << AJ_StatusText(status);
        status = AJ_SignCertificate(cert, &root_prvkey);
        ASSERT_EQ(AJ_OK, status) << "AJ_SignCertificate returned status. " << AJ_StatusText(status);
        status = AJ_VerifyCertificate(cert);
        ASSERT_EQ(AJ_OK, status) << "AJ_VerifyCertificate returned status. " << AJ_StatusText(status);

        status = AJ_BigEndianEncodeCertificate(cert, b8, sizeof (AJ_Certificate));
        ASSERT_EQ(AJ_OK, status) << "AJ_BigEndianEncodeCertificate returned status. " << AJ_StatusText(status);
        status = AJ_RawToB64(b8, cert->size, pem, pemlen);
        ASSERT_EQ(AJ_OK, status) << "AJ_RawToB64 returned status. " << AJ_StatusText(status);
        AJ_Printf("Root Certificate (Type 1)\n");
        AJ_Printf("-----BEGIN CERTIFICATE-----\n%s\n-----END CERTIFICATE-----\n", pem);

    }
}

/*  Test for generating owner type 2 certificate  */

TEST_F(SecurityTest, Test8)
{

    AJ_Status status = AJ_OK;

    AJ_RandBytes((uint8_t*) &guild, sizeof (AJ_GUID));

    for (i = 0; i < num; i++) {
        cert = (AJ_Certificate*) AJ_Malloc(sizeof (AJ_Certificate));
        AJ_ASSERT(cert);
        status = AJ_CreateCertificate(cert, 2, &root_pubkey, &peer_pubkey, &guild, digest, 0);
        ASSERT_EQ(AJ_OK, status) << "AJ_CreateCertificate returned status. " << AJ_StatusText(status);
        status = AJ_SignCertificate(cert, &root_prvkey);
        ASSERT_EQ(AJ_OK, status) << "AJ_SignCertificate returned status. " << AJ_StatusText(status);
        status = AJ_VerifyCertificate(cert);
        ASSERT_EQ(AJ_OK, status) << "AJ_VerifyCertificate returned status. " << AJ_StatusText(status);


        status = AJ_BigEndianEncodeCertificate(cert, b8, sizeof (AJ_Certificate));
        ASSERT_EQ(AJ_OK, status) << "AJ_BigEndianEncodeCertificate returned status. " << AJ_StatusText(status);
        status = AJ_RawToB64(b8, cert->size, pem, pemlen);
        ASSERT_EQ(AJ_OK, status) << "AJ_RawToB64 returned status. " << AJ_StatusText(status);
        AJ_Printf("Root Certificate (Type 2)\n");
        AJ_Printf("-----BEGIN CERTIFICATE-----\n%s\n-----END CERTIFICATE-----\n", pem);
        AJ_Free(cert);
    }
}

























