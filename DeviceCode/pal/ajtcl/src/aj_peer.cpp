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
#define AJ_MODULE PEER

#include "aj_target.h"
#include "aj_peer.h"
#include "aj_bus.h"
#include "aj_msg.h"
#include "aj_util.h"
#include "aj_guid.h"
#include "aj_creds.h"
#include "aj_std.h"
#include "aj_crypto.h"
#include "aj_debug.h"
#include "aj_config.h"
#include "aj_authentication.h"
#include "aj_cert.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgPEER = 0;
#endif

/*
 * Version number of the key generation algorithm.
 */
#define MIN_KEYGEN_VERSION  0x00
#define MAX_KEYGEN_VERSION  0x00

/*
 * The base authentication version number
 */
#define MIN_AUTH_VERSION  0x0002
#define MAX_AUTH_VERSION  0x0003

#define REQUIRED_AUTH_VERSION  (((uint32_t)MAX_AUTH_VERSION << 16) | MIN_KEYGEN_VERSION)

#define AES_KEY_LEN   16

static AJ_Status SaveMasterSecret(const AJ_GUID* peerGuid, uint32_t expiration);
static AJ_Status ExchangeSuites(AJ_Message* msg);
static AJ_Status KeyExchange(AJ_Message* msg);
static AJ_Status KeyAuthentication(AJ_Message* msg);
static AJ_Status GenSessionKey(AJ_Message* msg);

typedef enum {
    AJ_AUTH_NONE,
    AJ_AUTH_EXCHANGED,
    AJ_AUTH_SUCCESS
} HandshakeState;

typedef struct _PeerContext {
    HandshakeState state;
    AJ_BusAuthPeerCallback callback; /* Callback function to report completion */
    void* cbContext;                 /* Context to pass to the callback function */
    const AJ_GUID* peerGuid;         /* GUID pointer for the currently authenticating peer */
    const char* peerName;            /* Name of the peer being authenticated */
    AJ_Time timer;                   /* Timer for detecting failed authentication attempts */
    char nonce[2 * AJ_NONCE_LEN + 1];   /* Nonce as ascii hex */
} PeerContext;

static PeerContext peerContext;
static AJ_AuthenticationContext authContext;

static uint32_t GetAcceptableVersion(uint32_t srcV)
{
    uint16_t authV = srcV >> 16;
    uint16_t keyV = srcV & 0xFFFF;

    if ((authV < MIN_AUTH_VERSION) || (authV > MAX_AUTH_VERSION)) {
        return 0;
    }
    if (keyV > MAX_KEYGEN_VERSION) {
        return 0;
    }

    if (authV < MAX_AUTH_VERSION) {
        return srcV;
    }
    if (keyV < MAX_KEYGEN_VERSION) {
        return srcV;
    }
    return REQUIRED_AUTH_VERSION;
}

static AJ_Status KeyGen(const char* peerName, uint8_t role, const char* nonce1, const char* nonce2, uint8_t* outBuf, uint32_t len)
{
    AJ_Status status;
    const uint8_t* data[4];
    uint8_t lens[4];
    const AJ_GUID* peerGuid = AJ_GUID_Find(peerName);

    AJ_InfoPrintf(("KeyGen(peerName=\"%s\", role=%d., nonce1=\"%s\", nonce2=\"%s\", outbuf=%p, len=%d.)\n",
                   peerName, role, nonce1, nonce2, outBuf, len));

    if (NULL == peerGuid) {
        AJ_ErrPrintf(("KeyGen(): AJ_ERR_UNEXPECTED\n"));
        return AJ_ERR_UNEXPECTED;
    }

    data[0] = authContext.mastersecret;
    lens[0] = (uint32_t)AJ_MASTER_SECRET_LEN;
    data[1] = (uint8_t*)"session key";
    lens[1] = 11;
    data[2] = (uint8_t*)nonce1;
    lens[2] = (uint32_t)strlen(nonce1);
    data[3] = (uint8_t*)nonce2;
    lens[3] = (uint32_t)strlen(nonce2);

    /*
     * We use the outBuf to store both the key and verifier string.
     * Check that there is enough space to do so.
     */
    if (len < (AES_KEY_LEN + AJ_VERIFIER_LEN)) {
        AJ_WarnPrintf(("KeyGen(): AJ_ERR_RESOURCES\n"));
        return AJ_ERR_RESOURCES;
    }

    status = AJ_Crypto_PRF_SHA256(data, lens, ArraySize(data), outBuf, AES_KEY_LEN + AJ_VERIFIER_LEN);
    /*
     * Store the session key and compose the verifier string.
     */
    if (status == AJ_OK) {
        status = AJ_SetSessionKey(peerName, outBuf, role, authContext.version);
    }
    if (status == AJ_OK) {
        memmove(outBuf, outBuf + AES_KEY_LEN, AJ_VERIFIER_LEN);
        status = AJ_RawToHex(outBuf, AJ_VERIFIER_LEN, (char*) outBuf, len, FALSE);
    }
    AJ_InfoPrintf(("KeyGen Verifier = %s.\n", outBuf));
    return status;
}

void AJ_ClearAuthContext()
{
    memset(&peerContext, 0, sizeof (PeerContext));
    memset(&authContext, 0, sizeof (AJ_AuthenticationContext));
}

static void HandshakeComplete(AJ_Status status)
{

    AJ_InfoPrintf(("HandshakeComplete(status=%d.)\n", status));

    if (peerContext.callback) {
        peerContext.callback(peerContext.cbContext, status);
    }

    AJ_ClearAuthContext();
}

static AJ_Status SaveMasterSecret(const AJ_GUID* peerGuid, uint32_t expiration)
{
    AJ_Status status;

    AJ_InfoPrintf(("SaveMasterSecret(peerGuid=%p, expiration=%d)\n", peerGuid, expiration));

    if (peerGuid) {
        /*
         * If the authentication was succesful write the credentials for the authenticated peer to
         * NVRAM otherwise delete any stale credentials that might be stored.
         */
        if (AJ_AUTH_SUCCESS == peerContext.state) {
            status = AJ_StorePeerSecret(peerGuid, authContext.mastersecret, AJ_MASTER_SECRET_LEN, expiration);
        } else {
            AJ_WarnPrintf(("SaveMasterSecret(peerGuid=%p, expiration=%d): Invalid state\n", peerGuid, expiration));
            status = AJ_DeletePeerCredential(peerGuid);
        }
    } else {
        status = AJ_ERR_SECURITY;
    }

    return status;
}

static AJ_Status HandshakeTimeout() {
    uint8_t zero[sizeof (AJ_GUID)];
    memset(zero, 0, sizeof (zero));
    /*
     * If handshake started, check peer is still around
     * If peer disappeared, AJ_GUID_DeleteNameMapping writes zeros
     */
    if (peerContext.peerGuid) {
        if (!memcmp(peerContext.peerGuid, zero, sizeof (zero))) {
            AJ_WarnPrintf(("AJ_HandshakeTimeout(): Peer disappeared\n"));
            peerContext.peerGuid = NULL;
            HandshakeComplete(AJ_ERR_TIMEOUT);
            return AJ_ERR_TIMEOUT;
        }
    }
    if (AJ_GetElapsedTime(&peerContext.timer, TRUE) >= AJ_MAX_AUTH_TIME) {
        AJ_WarnPrintf(("AJ_HandshakeTimeout(): AJ_ERR_TIMEOUT\n"));
        HandshakeComplete(AJ_ERR_TIMEOUT);
        return AJ_ERR_TIMEOUT;
    }
    return AJ_OK;
}

static AJ_Status HandshakeValid(const AJ_GUID* peerGuid)
{
    /*
     * Handshake not yet started
     */
    if (!peerContext.peerGuid) {
        AJ_InfoPrintf(("AJ_HandshakeValid(peerGuid=%p): Invalid peer guid\n", peerGuid));
        return AJ_ERR_SECURITY;
    }
    /*
     * Handshake timed out
     */
    if (AJ_OK != HandshakeTimeout()) {
        AJ_InfoPrintf(("AJ_HandshakeValid(peerGuid=%p): Handshake timed out\n", peerGuid));
        return AJ_ERR_TIMEOUT;
    }
    /*
     * Handshake call from different peer
     */
    if (!peerGuid || (peerGuid != peerContext.peerGuid)) {
        AJ_WarnPrintf(("AJ_HandshakeValid(peerGuid=%p): Invalid peer guid\n", peerGuid));
        return AJ_ERR_RESOURCES;
    }

    return AJ_OK;
}

AJ_Status AJ_PeerAuthenticate(AJ_BusAttachment* bus, const char* peerName, AJ_PeerAuthenticateCallback callback, void* cbContext)
{
#ifndef NO_SECURITY
    AJ_Status status;
    AJ_Message msg;
    char guidStr[33];
    AJ_GUID localGuid;

    AJ_InfoPrintf(("PeerAuthenticate(bus=%p, peerName=\"%s\", callback=%p, cbContext=%p)\n",
                   bus, peerName, callback, cbContext));

    /*
     * If handshake in progress and not timed-out
     */
    if (peerContext.peerGuid) {
        status = HandshakeTimeout();
        if (AJ_ERR_TIMEOUT != status) {
            AJ_InfoPrintf(("PeerAuthenticate(): Handshake in progress\n"));
            return AJ_ERR_RESOURCES;
        }
    }

    /*
     * No handshake in progress or previous timed-out
     */
    AJ_ClearAuthContext();
    peerContext.callback = callback;
    peerContext.cbContext = cbContext;
    peerContext.peerName = peerName;
    AJ_InitTimer(&peerContext.timer);
    authContext.bus = bus;
    authContext.role = AUTH_CLIENT;
    AJ_SHA256_Init(&authContext.hash);
    if (bus->pwdCallback) {
        AJ_EnableSuite(AUTH_SUITE_ECDHE_PSK);
    }

    /*
     * Kick off authentication with an ExchangeGUIDS method call
     */
    AJ_MarshalMethodCall(bus, &msg, AJ_METHOD_EXCHANGE_GUIDS, peerName, 0, AJ_NO_FLAGS, AJ_CALL_TIMEOUT);
    AJ_GetLocalGUID(&localGuid);
    AJ_GUID_ToString(&localGuid, guidStr, sizeof(guidStr));
    authContext.version = REQUIRED_AUTH_VERSION;
    AJ_MarshalArgs(&msg, "su", guidStr, authContext.version);
    return AJ_DeliverMsg(&msg);
#else
    return AJ_OK;
#endif
}

AJ_Status AJ_PeerHandleExchangeGUIDs(AJ_Message* msg, AJ_Message* reply)
{
    AJ_Status status;
    char guidStr[33];
    char* str;
    AJ_GUID remoteGuid;
    AJ_GUID localGuid;
    AJ_PeerCred* cred;

    AJ_InfoPrintf(("AJ_PeerHandleExchangeGuids(msg=%p, reply=%p)\n", msg, reply));

    /*
     * If handshake in progress and not timed-out
     */
    if (peerContext.peerGuid) {
        status = HandshakeTimeout();
        if (AJ_ERR_TIMEOUT != status) {
            AJ_InfoPrintf(("AJ_PeerHandleExchangeGuids(msg=%p, reply=%p): Handshake in progress\n", msg, reply));
            return AJ_MarshalErrorMsg(msg, reply, AJ_ErrResources);
        }
    }

    /*
     * No handshake in progress or previous timed-out
     */
    AJ_ClearAuthContext();
    AJ_InitTimer(&peerContext.timer);
    authContext.bus = msg->bus;
    authContext.role = AUTH_SERVER;
    AJ_SHA256_Init(&authContext.hash);
    if (msg->bus->pwdCallback) {
        AJ_EnableSuite(AUTH_SUITE_ECDHE_PSK);
    }

    status = AJ_UnmarshalArgs(msg, "su", &str, &authContext.version);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("AJ_PeerHandleExchangeGuids(msg=%p, reply=%p): Unmarshal error\n", msg, reply));
        HandshakeComplete(AJ_ERR_SECURITY);
        return AJ_MarshalErrorMsg(msg, reply, AJ_ErrSecurityViolation);
    }
    status = AJ_GUID_FromString(&remoteGuid, str);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("AJ_PeerHandleExchangeGuids(msg=%p, reply=%p): Invalid GUID\n", msg, reply));
        HandshakeComplete(AJ_ERR_SECURITY);
        return AJ_MarshalErrorMsg(msg, reply, AJ_ErrSecurityViolation);
    }
    status = AJ_GUID_AddNameMapping(msg->bus, &remoteGuid, msg->sender, NULL);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("AJ_PeerHandleExchangeGuids(msg=%p, reply=%p): Add name mapping error\n", msg, reply));
        HandshakeComplete(AJ_ERR_RESOURCES);
        return AJ_MarshalErrorMsg(msg, reply, AJ_ErrResources);
    }
    peerContext.peerGuid = AJ_GUID_Find(msg->sender);

    /*
     * If we have a mastersecret stored - use it
     */
    status = AJ_GetPeerCredential(peerContext.peerGuid, &cred);
    if (AJ_OK == status) {
        status = AJ_CredentialExpired(cred);
        if (AJ_ERR_KEY_EXPIRED != status) {
            /* secret not expired or time unknown */
            peerContext.state = AJ_AUTH_SUCCESS;
            /* assert that MASTER_SECRET_LEN == cred->dataLen */
            memcpy(authContext.mastersecret, cred->data, cred->dataLen);
        } else {
            AJ_DeletePeerCredential(peerContext.peerGuid);
        }
        AJ_FreeCredential(cred);
    }

    /*
     * We are not currently negotiating versions so we tell the peer what version we require.
     */
    authContext.version = GetAcceptableVersion(authContext.version);
    if (0 == authContext.version) {
        authContext.version = REQUIRED_AUTH_VERSION;
    }
    AJ_InfoPrintf(("AJ_PeerHandleExchangeGuids(msg=%p, reply=%p): Version %x\n", msg, reply, authContext.version));

    AJ_MarshalReplyMsg(msg, reply);
    AJ_GetLocalGUID(&localGuid);
    AJ_GUID_ToString(&localGuid, guidStr, sizeof(guidStr));
    return AJ_MarshalArgs(reply, "su", guidStr, authContext.version);
}

AJ_Status AJ_PeerHandleExchangeGUIDsReply(AJ_Message* msg)
{
    AJ_Status status;
    const char* guidStr;
    AJ_GUID remoteGuid;
    AJ_PeerCred* cred;

    AJ_InfoPrintf(("AJ_PeerHandleExchangeGUIDsReply(msg=%p)\n", msg));

    if (msg->hdr->msgType == AJ_MSG_ERROR) {
        AJ_WarnPrintf(("AJ_PeerHandleExchangeGUIDsReply(msg=%p): error=%s.\n", msg, msg->error));
        if (0 == strncmp(msg->error, AJ_ErrResources, sizeof(AJ_ErrResources))) {
            status = AJ_ERR_RESOURCES;
        } else {
            status = AJ_ERR_SECURITY;
            HandshakeComplete(status);
        }
        return status;
    }

    /*
     * If handshake in progress and not timed-out
     */
    if (peerContext.peerGuid) {
        status = HandshakeTimeout();
        if (AJ_ERR_TIMEOUT != status) {
            AJ_WarnPrintf(("AJ_PeerHandleExchangeGUIDsReply(msg=%p): Handshake in progress\n", msg));
            return AJ_ERR_RESOURCES;
        }
    }

    status = AJ_UnmarshalArgs(msg, "su", &guidStr, &authContext.version);
    if (status != AJ_OK) {
        AJ_WarnPrintf(("AJ_PeerHandleExchangeGUIDsReply(msg=%p): Unmarshal error\n", msg));
        goto Exit;
    }
    authContext.version = GetAcceptableVersion(authContext.version);
    if (0 == authContext.version) {
        AJ_WarnPrintf(("AJ_PeerHandleExchangeGUIDsReply(msg=%p): Invalid version\n", msg));
        goto Exit;
    }
    status = AJ_GUID_FromString(&remoteGuid, guidStr);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_PeerHandleExchangeGUIDsReply(msg=%p): Invalid GUID\n", msg));
        goto Exit;
    }
    /*
     * Two name mappings to add, the well known name, and the unique name from the message.
     */
    status = AJ_GUID_AddNameMapping(msg->bus, &remoteGuid, msg->sender, peerContext.peerName);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_PeerHandleExchangeGUIDsReply(msg=%p): Add name mapping error\n", msg));
        goto Exit;
    }
    /*
     * Remember which peer is being authenticated
     */
    peerContext.peerGuid = AJ_GUID_Find(msg->sender);

    /*
     * If we have a mastersecret stored - use it
     */
    status = AJ_GetPeerCredential(peerContext.peerGuid, &cred);
    if (AJ_OK == status) {
        status = AJ_CredentialExpired(cred);
        if (AJ_ERR_KEY_EXPIRED != status) {
            /* secret not expired or time unknown */
            peerContext.state = AJ_AUTH_SUCCESS;
            /* assert that MASTER_SECRET_LEN == cred->dataLen */
            memcpy(authContext.mastersecret, cred->data, cred->dataLen);
            AJ_FreeCredential(cred);
            status = GenSessionKey(msg);
            return status;
        } else {
            AJ_DeletePeerCredential(peerContext.peerGuid);
        }
        AJ_FreeCredential(cred);
    }

    /*
     * Start the ALLJOYN conversation
     */
    status = ExchangeSuites(msg);
    if (AJ_OK != status) {
        goto Exit;
    }
    return status;

Exit:
    HandshakeComplete(AJ_ERR_SECURITY);
    return AJ_ERR_SECURITY;
}

static AJ_Status ExchangeSuites(AJ_Message* msg)
{
    AJ_Status status;
    AJ_Message call;
    uint32_t suites[AJ_AUTH_SUITES_NUM];
    size_t num = 0;

    AJ_InfoPrintf(("ExchangeSuites(msg=%p)\n", msg));

    authContext.role = AUTH_CLIENT;
    AJ_SHA256_Init(&authContext.hash);

    /*
     * Send suites in this priority order
     */
    if (AJ_IsSuiteEnabled(AUTH_SUITE_ECDHE_ECDSA, authContext.version >> 16)) {
        suites[num++] = AUTH_SUITE_ECDHE_ECDSA;
    }
    if (AJ_IsSuiteEnabled(AUTH_SUITE_ECDHE_PSK, authContext.version >> 16)) {
        suites[num++] = AUTH_SUITE_ECDHE_PSK;
    }
    if (AJ_IsSuiteEnabled(AUTH_SUITE_ECDHE_NULL, authContext.version >> 16)) {
        suites[num++] = AUTH_SUITE_ECDHE_NULL;
    }
    if (!num) {
        AJ_WarnPrintf(("ExchangeSuites(msg=%p): No suites available\n", msg));
        HandshakeComplete(AJ_ERR_SECURITY);
        return AJ_ERR_SECURITY;
    }
    AJ_MarshalMethodCall(msg->bus, &call, AJ_METHOD_EXCHANGE_SUITES, msg->sender, 0, AJ_NO_FLAGS, AJ_AUTH_CALL_TIMEOUT);
    status = AJ_MarshalArgs(&call, "au", suites, num * sizeof (uint32_t));
    if (AJ_OK != status) {
        AJ_WarnPrintf(("ExchangeSuites(msg=%p): Marshal error\n", msg));
        HandshakeComplete(AJ_ERR_SECURITY);
        return AJ_ERR_SECURITY;
    }

    return AJ_DeliverMsg(&call);
}

AJ_Status AJ_PeerHandleExchangeSuites(AJ_Message* msg, AJ_Message* reply)
{
    AJ_Status status;
    AJ_Arg array;
    uint32_t* suites;
    size_t numsuites;
    uint32_t i;
    const AJ_GUID* peerGuid = AJ_GUID_Find(msg->sender);

    AJ_InfoPrintf(("AJ_PeerHandleExchangeSuites(msg=%p, reply=%p)\n", msg, reply));

    status = HandshakeValid(peerGuid);
    if (AJ_OK != status) {
        return AJ_MarshalErrorMsg(msg, reply, AJ_ErrResources);
    }

    authContext.role = AUTH_SERVER;
    AJ_SHA256_Init(&authContext.hash);

    /*
     * Receive suites
     */
    status = AJ_UnmarshalArgs(msg, "au", &suites, &numsuites);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("AJ_PeerHandleExchangeSuites(msg=%p, reply=%p): Unmarshal error\n", msg, reply));
        goto Exit;
    }
    numsuites /= sizeof (uint32_t);

    /*
     * Calculate common suites
     */
    AJ_MarshalReplyMsg(msg, reply);
    status = AJ_MarshalContainer(reply, &array, AJ_ARG_ARRAY);
    /* Iterate through the available suites.
     * If it's enabled, marshal the suite to send to the other peer.
     */
    for (i = 0; i < numsuites; i++) {
        if (AJ_IsSuiteEnabled(suites[i], authContext.version >> 16)) {
            status = AJ_MarshalArgs(reply, "u", suites[i]);
        }
    }
    status = AJ_MarshalCloseContainer(reply, &array);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_PeerHandleExchangeSuites(msg=%p, reply=%p): Marshal error\n", msg, reply));
        goto Exit;
    }

    AJ_InfoPrintf(("Exchange Suites Complete\n"));
    return status;

Exit:

    HandshakeComplete(AJ_ERR_SECURITY);
    return AJ_MarshalErrorMsg(msg, reply, AJ_ErrSecurityViolation);
}

AJ_Status AJ_PeerHandleExchangeSuitesReply(AJ_Message* msg)
{
    AJ_Status status;
    uint32_t* suites;
    size_t numsuites;
    size_t i;
    const AJ_GUID* peerGuid = AJ_GUID_Find(msg->sender);

    AJ_InfoPrintf(("AJ_PeerHandleExchangeSuitesReply(msg=%p)\n", msg));

    if (msg->hdr->msgType == AJ_MSG_ERROR) {
        AJ_WarnPrintf(("AJ_PeerHandleExchangeSuitesReply(msg=%p): error=%s.\n", msg, msg->error));
        goto Exit;
    }

    status = HandshakeValid(peerGuid);
    if (AJ_OK != status) {
        return status;
    }

    /*
     * Receive suites
     */
    status = AJ_UnmarshalArgs(msg, "au", &suites, &numsuites);
    if (AJ_OK != status) {
        goto Exit;
    }
    numsuites /= sizeof (uint32_t);

    /*
     * Double check we can support (ie. that server didn't send something bogus)
     */
    authContext.suite = 0;
    for (i = 0; i < numsuites; i++) {
        if (AJ_IsSuiteEnabled(suites[i], authContext.version >> 16)) {
            // Pick the highest priority suite, which happens to be the highest integer
            authContext.suite = (suites[i] > authContext.suite) ? suites[i] : authContext.suite;
        }
    }
    if (!authContext.suite) {
        AJ_InfoPrintf(("AJ_PeerHandleExchangeSuitesReply(msg=%p): No common suites\n", msg));
        goto Exit;
    }

    /*
     * Exchange suites complete.
     */
    AJ_InfoPrintf(("Exchange Suites Complete\n"));
    status = KeyExchange(msg);
    if (AJ_OK != status) {
        goto Exit;
    }
    return status;

Exit:

    HandshakeComplete(AJ_ERR_SECURITY);
    return AJ_ERR_SECURITY;
}

static AJ_Status KeyExchange(AJ_Message* msg)
{
    AJ_Status status;
    uint8_t suiteb8[sizeof (uint32_t)];
    AJ_Message call;

    AJ_InfoPrintf(("KeyExchange(msg=%p)\n", msg));

    AJ_InfoPrintf(("Authenticating using suite %x\n", authContext.suite));

    /*
     * Send suite and key material
     */
    AJ_MarshalMethodCall(msg->bus, &call, AJ_METHOD_KEY_EXCHANGE, msg->sender, 0, AJ_NO_FLAGS, AJ_AUTH_CALL_TIMEOUT);
    status = AJ_MarshalArgs(&call, "u", authContext.suite);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("KeyExchange(msg=%p): Marshal error\n", msg));
        goto Exit;
    }

    HostU32ToBigEndianU8(&authContext.suite, sizeof (authContext.suite), suiteb8);
    AJ_SHA256_Update(&authContext.hash, suiteb8, sizeof (suiteb8));
    status = AJ_KeyExchangeMarshal(&authContext, &call);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("KeyExchange(msg=%p): Key exchange marshal error\n", msg));
        goto Exit;
    }

    return AJ_DeliverMsg(&call);

Exit:
    HandshakeComplete(AJ_ERR_SECURITY);
    return AJ_ERR_SECURITY;
}

AJ_Status AJ_PeerHandleKeyExchange(AJ_Message* msg, AJ_Message* reply)
{
    AJ_Status status;
    uint8_t suiteb8[sizeof (uint32_t)];
    const AJ_GUID* peerGuid = AJ_GUID_Find(msg->sender);

    AJ_InfoPrintf(("AJ_PeerHandleKeyExchange(msg=%p, reply=%p)\n", msg, reply));

    status = HandshakeValid(peerGuid);
    if (AJ_OK != status) {
        return AJ_MarshalErrorMsg(msg, reply, AJ_ErrResources);
    }

    /*
     * Receive suite
     */
    AJ_UnmarshalArgs(msg, "u", &authContext.suite);
    if (!AJ_IsSuiteEnabled(authContext.suite, authContext.version >> 16)) {
        goto Exit;
    }
    HostU32ToBigEndianU8(&authContext.suite, sizeof (authContext.suite), suiteb8);
    AJ_SHA256_Update(&authContext.hash, suiteb8, sizeof (suiteb8));

    /*
     * Receive key material
     */
    status = AJ_KeyExchangeUnmarshal(&authContext, msg);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("AJ_PeerHandleKeyExchange(msg=%p, reply=%p): Key exchange unmarshal error\n", msg, reply));
        goto Exit;
    }

    /*
     * Send key material
     */
    AJ_MarshalReplyMsg(msg, reply);
    status = AJ_MarshalArgs(reply, "u", authContext.suite);
    if (AJ_OK != status) {
        goto Exit;
    }
    AJ_SHA256_Update(&authContext.hash, (uint8_t*) suiteb8, sizeof (suiteb8));
    status = AJ_KeyExchangeMarshal(&authContext, reply);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_PeerHandleKeyExchange(msg=%p, reply=%p): Key exchange marshal error\n", msg, reply));
        goto Exit;
    }
    peerContext.state = AJ_AUTH_EXCHANGED;
    AJ_InfoPrintf(("Key Exchange Complete\n"));
    return status;

Exit:
    HandshakeComplete(AJ_ERR_SECURITY);
    return AJ_MarshalErrorMsg(msg, reply, AJ_ErrSecurityViolation);
}

AJ_Status AJ_PeerHandleKeyExchangeReply(AJ_Message* msg)
{
    AJ_Status status;
    uint32_t suite;
    uint8_t suiteb8[sizeof (uint32_t)];
    const AJ_GUID* peerGuid = AJ_GUID_Find(msg->sender);

    AJ_InfoPrintf(("AJ_PeerHandleKeyExchangeReply(msg=%p)\n", msg));

    if (msg->hdr->msgType == AJ_MSG_ERROR) {
        AJ_WarnPrintf(("AJ_PeerHandleKeyExchangeReply(msg=%p): error=%s.\n", msg, msg->error));
        if (0 == strncmp(msg->error, AJ_ErrResources, sizeof(AJ_ErrResources))) {
            status = AJ_ERR_RESOURCES;
        } else {
            status = AJ_ERR_SECURITY;
            HandshakeComplete(status);
        }
        return status;
    }

    status = HandshakeValid(peerGuid);
    if (AJ_OK != status) {
        return status;
    }

    /*
     * Receive key material
     */
    status = AJ_UnmarshalArgs(msg, "u", &suite);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_PeerHandleKeyExchangeReply(msg=%p): Unmarshal error\n", msg));
        goto Exit;
    }
    if (suite != authContext.suite) {
        AJ_WarnPrintf(("AJ_PeerHandleKeyExchangeReply(msg=%p): Suite mismatch\n", msg));
        goto Exit;
    }
    HostU32ToBigEndianU8(&suite, sizeof (suite), suiteb8);
    AJ_SHA256_Update(&authContext.hash, suiteb8, sizeof (suiteb8));
    status = AJ_KeyExchangeUnmarshal(&authContext, msg);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_PeerHandleKeyExchangeReply(msg=%p): Key exchange unmarshal error\n", msg));
        goto Exit;
    }

    /*
     * Key exchange complete - start the authentication
     */
    peerContext.state = AJ_AUTH_EXCHANGED;
    AJ_InfoPrintf(("Key Exchange Complete\n"));
    status = KeyAuthentication(msg);
    if (AJ_OK != status) {
        goto Exit;
    }
    return status;

Exit:
    HandshakeComplete(AJ_ERR_SECURITY);
    return AJ_ERR_SECURITY;
}

static AJ_Status KeyAuthentication(AJ_Message* msg)
{
    AJ_Status status;
    AJ_Message call;
    const AJ_GUID* peerGuid = AJ_GUID_Find(msg->sender);

    AJ_InfoPrintf(("AJ_KeyAuthentication(msg=%p)\n", msg));

    status = HandshakeValid(peerGuid);
    if (AJ_OK != status) {
        return status;
    }

    /*
     * Send authentication material
     */
    AJ_MarshalMethodCall(msg->bus, &call, AJ_METHOD_KEY_AUTHENTICATION, msg->sender, 0, AJ_NO_FLAGS, AJ_AUTH_CALL_TIMEOUT);
    status = AJ_KeyAuthenticationMarshal(&authContext, &call);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_KeyAuthentication(msg=%p): Key authentication marshal error\n", msg));
        goto Exit;
    }

    return AJ_DeliverMsg(&call);

Exit:
    HandshakeComplete(AJ_ERR_SECURITY);
    return AJ_ERR_SECURITY;
}

AJ_Status AJ_PeerHandleKeyAuthentication(AJ_Message* msg, AJ_Message* reply)
{
    AJ_Status status;
    const AJ_GUID* peerGuid = AJ_GUID_Find(msg->sender);

    AJ_InfoPrintf(("AJ_PeerHandleKeyAuthentication(msg=%p, reply=%p)\n", msg, reply));

    status = HandshakeValid(peerGuid);
    if (AJ_OK != status) {
        return AJ_MarshalErrorMsg(msg, reply, AJ_ErrResources);
    }

    if (AJ_AUTH_EXCHANGED != peerContext.state) {
        AJ_InfoPrintf(("AJ_PeerHandleKeyAuthentication(msg=%p, reply=%p): Invalid state\n", msg, reply));
        goto Exit;
    }

    /*
     * Receive authentication material
     */
    status = AJ_KeyAuthenticationUnmarshal(&authContext, msg);
    if (AJ_OK != status) {
        AJ_InfoPrintf(("AJ_PeerHandleKeyAuthentication(msg=%p, reply=%p): Key authentication unmarshal error\n", msg, reply));
        goto Exit;
    }

    /*
     * Send authentication material
     */
    AJ_MarshalReplyMsg(msg, reply);
    status = AJ_KeyAuthenticationMarshal(&authContext, reply);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_PeerHandleKeyAuthentication(msg=%p, reply=%p): Key authentication marshal error\n", msg, reply));
        goto Exit;
    }

    AJ_InfoPrintf(("Key Authentication Complete\n"));
    peerContext.state = AJ_AUTH_SUCCESS;

    if (authContext.expiration) {
        status = SaveMasterSecret(peerGuid, authContext.expiration);
        if (AJ_OK != status) {
            AJ_WarnPrintf(("AJ_PeerHandleKeyAuthentication(msg=%p, reply=%p): Save master secret error\n", msg, reply));
        }
    }

    return status;

Exit:
    HandshakeComplete(AJ_ERR_SECURITY);
    return AJ_MarshalErrorMsg(msg, reply, AJ_ErrSecurityViolation);
}

AJ_Status AJ_PeerHandleKeyAuthenticationReply(AJ_Message* msg)
{
    AJ_Status status;
    const AJ_GUID* peerGuid = AJ_GUID_Find(msg->sender);

    AJ_InfoPrintf(("AJ_PeerHandleKeyAuthenticationReply(msg=%p)\n", msg));

    if (msg->hdr->msgType == AJ_MSG_ERROR) {
        AJ_WarnPrintf(("AJ_PeerHandleKeyAuthenticationReply(msg=%p): error=%s.\n", msg, msg->error));
        if (0 == strncmp(msg->error, AJ_ErrResources, sizeof(AJ_ErrResources))) {
            status = AJ_ERR_RESOURCES;
        } else {
            status = AJ_ERR_SECURITY;
            HandshakeComplete(status);
        }
        return status;
    }

    status = HandshakeValid(peerGuid);
    if (AJ_OK != status) {
        return status;
    }

    if (AJ_AUTH_EXCHANGED != peerContext.state) {
        AJ_WarnPrintf(("AJ_PeerHandleKeyAuthenticationReply(msg=%p): Invalid state\n", msg));
        goto Exit;
    }

    /*
     * Receive authentication material
     */
    status = AJ_KeyAuthenticationUnmarshal(&authContext, msg);
    if (AJ_OK != status) {
        AJ_WarnPrintf(("AJ_PeerHandleKeyAuthenticationReply(msg=%p): Key authentication unmarshal error\n", msg));
        goto Exit;
    }

    /*
     * Key authentication complete - start the session
     */
    AJ_InfoPrintf(("Key Authentication Complete\n"));
    peerContext.state = AJ_AUTH_SUCCESS;

    if (authContext.expiration) {
        status = SaveMasterSecret(peerGuid, authContext.expiration);
        if (AJ_OK != status) {
            AJ_WarnPrintf(("AJ_PeerHandleKeyAuthenticationReply(msg=%p): Save master secret error\n", msg));
        }
    }

    status = GenSessionKey(msg);
    if (AJ_OK != status) {
        goto Exit;
    }

    return status;

Exit:
    HandshakeComplete(AJ_ERR_SECURITY);
    return AJ_ERR_SECURITY;
}

static AJ_Status GenSessionKey(AJ_Message* msg)
{
    AJ_Message call;
    char guidStr[33];
    AJ_GUID localGuid;

    AJ_InfoPrintf(("GenSessionKey(msg=%p)\n", msg));

    AJ_MarshalMethodCall(msg->bus, &call, AJ_METHOD_GEN_SESSION_KEY, msg->sender, 0, AJ_NO_FLAGS, AJ_CALL_TIMEOUT);
    /*
     * Marshal local peer GUID, remote peer GUID, and local peer's GUID
     */
    AJ_GetLocalGUID(&localGuid);
    AJ_GUID_ToString(&localGuid, guidStr, sizeof(guidStr));
    AJ_MarshalArgs(&call, "s", guidStr);
    AJ_GUID_ToString(peerContext.peerGuid, guidStr, sizeof(guidStr));
    AJ_RandHex(peerContext.nonce, sizeof(peerContext.nonce), AJ_NONCE_LEN);
    AJ_MarshalArgs(&call, "ss", guidStr, peerContext.nonce);

    return AJ_DeliverMsg(&call);
}

AJ_Status AJ_PeerHandleGenSessionKey(AJ_Message* msg, AJ_Message* reply)
{
    AJ_Status status;
    char* remGuid;
    char* locGuid;
    char* nonce;
    AJ_GUID guid;
    AJ_GUID localGuid;
    const AJ_GUID* peerGuid = AJ_GUID_Find(msg->sender);

    /*
     * For 12 bytes of verifier, we need at least 12 * 2 characters
     * to store its representation in hex (24 octets + 1 octet for \0).
     * However, the KeyGen function demands a bigger buffer
     * (to store 16 bytes key in addition to the 12 bytes verifier).
     * Hence we allocate, the maximum of (12 * 2 + 1) and (16 + 12).
     */
    char verifier[AES_KEY_LEN + AJ_VERIFIER_LEN];

    AJ_InfoPrintf(("AJ_PeerHandleGenSessionKey(msg=%p, reply=%p)\n", msg, reply));

    status = HandshakeValid(peerGuid);
    if (AJ_OK != status) {
        return AJ_MarshalErrorMsg(msg, reply, AJ_ErrResources);
    }
    if (AJ_AUTH_SUCCESS != peerContext.state) {
        /*
         * We don't have a saved master secret and we haven't generated one yet
         */
        AJ_InfoPrintf(("AJ_PeerHandleGenSessionKey(msg=%p, reply=%p): Key not available\n", msg, reply));
        return AJ_MarshalErrorMsg(msg, reply, AJ_ErrRejected);
    }

    /*
     * Remote peer GUID, Local peer GUID and Remote peer's nonce
     */
    AJ_UnmarshalArgs(msg, "sss", &remGuid, &locGuid, &nonce);
    /*
     * We expect arg[1] to be the local GUID
     */
    status = AJ_GUID_FromString(&guid, locGuid);
    if (AJ_OK == status) {
        status = AJ_GetLocalGUID(&localGuid);
    }
    if ((status != AJ_OK) || (memcmp(&guid, &localGuid, sizeof(AJ_GUID)) != 0)) {
        HandshakeComplete(AJ_ERR_SECURITY);
        return AJ_MarshalErrorMsg(msg, reply, AJ_ErrSecurityViolation);
    }
    AJ_RandHex(peerContext.nonce, sizeof(peerContext.nonce), AJ_NONCE_LEN);
    status = KeyGen(msg->sender, AJ_ROLE_KEY_RESPONDER, nonce, peerContext.nonce, (uint8_t*)verifier, sizeof(verifier));
    if (status == AJ_OK) {
        AJ_MarshalReplyMsg(msg, reply);
        status = AJ_MarshalArgs(reply, "ss", peerContext.nonce, verifier);
    } else {
        HandshakeComplete(AJ_ERR_SECURITY);
        status = AJ_MarshalErrorMsg(msg, reply, AJ_ErrSecurityViolation);
    }
    return status;
}

AJ_Status AJ_PeerHandleGenSessionKeyReply(AJ_Message* msg)
{
    AJ_Status status;
    /*
     * For 12 bytes of verifier, we need at least 12 * 2 characters
     * to store its representation in hex (24 octets + 1 octet for \0).
     * However, the KeyGen function demands a bigger buffer
     * (to store 16 bytes key in addition to the 12 bytes verifier).
     * Hence we allocate, the maximum of (12 * 2 + 1) and (16 + 12).
     */
    char verifier[AJ_VERIFIER_LEN + AES_KEY_LEN];
    char* nonce;
    char* remVerifier;
    const AJ_GUID* peerGuid = AJ_GUID_Find(msg->sender);

    AJ_InfoPrintf(("AJ_PeerHandleGetSessionKeyReply(msg=%p)\n", msg));

    if (msg->hdr->msgType == AJ_MSG_ERROR) {
        AJ_WarnPrintf(("AJ_PeerHandleGetSessionKeyReply(msg=%p): error=%s.\n", msg, msg->error));
        if (0 == strncmp(msg->error, AJ_ErrResources, sizeof(AJ_ErrResources))) {
            status = AJ_ERR_RESOURCES;
        } else if (0 == strncmp(msg->error, AJ_ErrRejected, sizeof(AJ_ErrRejected))) {
            status = ExchangeSuites(msg);
        } else {
            status = AJ_ERR_SECURITY;
            HandshakeComplete(status);
        }
        return status;
    }

    status = HandshakeValid(peerGuid);
    if (AJ_OK != status) {
        return status;
    }

    AJ_UnmarshalArgs(msg, "ss", &nonce, &remVerifier);
    status = KeyGen(msg->sender, AJ_ROLE_KEY_INITIATOR, peerContext.nonce, nonce, (uint8_t*)verifier, sizeof(verifier));
    if (status == AJ_OK) {
        /*
         * Check verifier strings match as expected
         */
        if (strcmp(remVerifier, verifier) != 0) {
            AJ_WarnPrintf(("AJ_PeerHandleGetSessionKeyReply(): AJ_ERR_SECURITY\n"));
            status = AJ_ERR_SECURITY;
        }
    }
    if (status == AJ_OK) {
        AJ_Arg key;
        AJ_Message call;
        uint8_t groupKey[AES_KEY_LEN];
        /*
         * Group keys are exchanged via an encrypted message
         */
        AJ_MarshalMethodCall(msg->bus, &call, AJ_METHOD_EXCHANGE_GROUP_KEYS, msg->sender, 0, AJ_FLAG_ENCRYPTED, AJ_CALL_TIMEOUT);
        AJ_GetGroupKey(NULL, groupKey);
        AJ_MarshalArg(&call, AJ_InitArg(&key, AJ_ARG_BYTE, AJ_ARRAY_FLAG, groupKey, sizeof(groupKey)));
        status = AJ_DeliverMsg(&call);
    }
    if (status != AJ_OK) {
        HandshakeComplete(status);
    }
    return status;
}

AJ_Status AJ_PeerHandleExchangeGroupKeys(AJ_Message* msg, AJ_Message* reply)
{
    AJ_Status status;
    AJ_Arg key;
    const AJ_GUID* peerGuid = AJ_GUID_Find(msg->sender);

    AJ_InfoPrintf(("AJ_PeerHandleExchangeGroupKeys(msg=%p, reply=%p)\n", msg, reply));

    status = HandshakeValid(peerGuid);
    if (AJ_OK != status) {
        return status;
    }

    AJ_UnmarshalArg(msg, &key);
    /*
     * We expect the key to be 16 bytes
     */
    if (key.len != AES_KEY_LEN) {
        AJ_WarnPrintf(("AJ_PeerHandleExchangeGroupKeys(): AJ_ERR_INVALID\n"));
        status = AJ_ERR_INVALID;
    } else {
        status = AJ_SetGroupKey(msg->sender, key.val.v_byte);
    }
    if (status == AJ_OK) {
        uint8_t groupKey[AES_KEY_LEN];
        AJ_MarshalReplyMsg(msg, reply);
        AJ_GetGroupKey(NULL, groupKey);
        status = AJ_MarshalArg(reply, AJ_InitArg(&key, AJ_ARG_BYTE, AJ_ARRAY_FLAG, groupKey, sizeof(groupKey)));
    } else {
        status = AJ_MarshalErrorMsg(msg, reply, AJ_ErrSecurityViolation);
    }
    HandshakeComplete(status);
    return status;
}

AJ_Status AJ_PeerHandleExchangeGroupKeysReply(AJ_Message* msg)
{
    AJ_Status status;
    AJ_Arg arg;
    const AJ_GUID* peerGuid = AJ_GUID_Find(msg->sender);

    AJ_InfoPrintf(("AJ_PeerHandleExchangeGroupKeysReply(msg=%p)\n", msg));

    status = HandshakeValid(peerGuid);
    if (AJ_OK != status) {
        return status;
    }

    AJ_UnmarshalArg(msg, &arg);
    /*
     * We expect the key to be 16 bytes
     */
    if (arg.len != AES_KEY_LEN) {
        AJ_WarnPrintf(("AJ_PeerHandleExchangeGroupKeysReply(msg=%p): AJ_ERR_INVALID\n", msg));
        status = AJ_ERR_INVALID;
    } else {
        status = AJ_SetGroupKey(msg->sender, arg.val.v_byte);
    }
    HandshakeComplete(status);
    return status;
}
