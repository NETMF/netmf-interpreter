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
#define AJ_MODULE SVCLITE

#ifndef NO_SECURITY
#define SECURE_INTERFACE
#endif

#include <aj_target.h>
#include <aj_link_timeout.h>
#include <aj_debug.h>
#include <alljoyn.h>
#include <aj_cert.h>
#include <aj_creds.h>
#include <aj_peer.h>
#include <aj_auth_listener.h>
#include <aj_authentication.h>

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
uint8_t dbgSVCLITE = 1;

/*
 * Modify these variables to change the service's behavior
 */
static const char ServiceName[] = "org.alljoyn.svclite";
static const uint16_t ServicePort = 24;
static const uint8_t CancelAdvertiseName = FALSE;
static const uint8_t ReflectSignal = FALSE;

/*
 * An application property to SET or GET
 */
static int32_t propVal = 123456;

/*
 * Default key expiration
 */
static const uint32_t keyexpiration = 0xFFFFFFFF;

/*
 * To define a secure interface, prepend '$' before the interface name, eg., "$org.alljoyn.alljoyn_test"
 */
static const char* const testInterface[] = {
#ifdef SECURE_INTERFACE
    "$org.alljoyn.alljoyn_test",
#else
    "org.alljoyn.alljoyn_test",
#endif
    "?my_ping inStr<s outStr>s",
    "?delayed_ping inStr<s delay<u outStr>s",
    "?time_ping <u <q >u >q",
    "!my_signal >a{ys}",
    NULL
};

static const char* const testValuesInterface[] = {
#ifdef SECURE_INTERFACE
    "$org.alljoyn.alljoyn_test.values",
#else
    "org.alljoyn.alljoyn_test.values",
#endif
    "@int_val=i",
    "@str_val=s",
    "@ro_val>s",
    NULL
};

static const AJ_InterfaceDescription testInterfaces[] = {
    AJ_PropertiesIface,
    testInterface,
    testValuesInterface,
    NULL
};

static AJ_Object AppObjects[] = {
    { "/org/alljoyn/alljoyn_test", testInterfaces, AJ_OBJ_FLAG_ANNOUNCED },
    { NULL }
};

/*
 * Message identifiers for the method calls this application implements
 */
#define APP_GET_PROP        AJ_APP_MESSAGE_ID(0, 0, AJ_PROP_GET)
#define APP_SET_PROP        AJ_APP_MESSAGE_ID(0, 0, AJ_PROP_SET)
#define APP_MY_PING         AJ_APP_MESSAGE_ID(0, 1, 0)
#define APP_DELAYED_PING    AJ_APP_MESSAGE_ID(0, 1, 1)
#define APP_TIME_PING       AJ_APP_MESSAGE_ID(0, 1, 2)
#define APP_MY_SIGNAL       AJ_APP_MESSAGE_ID(0, 1, 3)


/*
 * Property identifiers for the properies this application implements
 */
#define APP_INT_VAL_PROP AJ_APP_PROPERTY_ID(0, 2, 0)

/*
 * Let the application do some work
 */
static void AppDoWork()
{
    /*
     * This function is called if there are no messages to unmarshal
     */
    AJ_InfoPrintf(("do work\n"));
}


static const char PWD[] = "123456";

#if defined(SECURE_INTERFACE) || defined(SECURE_OBJECT)

static uint32_t PasswordCallback(uint8_t* buffer, uint32_t bufLen)
{
    memcpy(buffer, PWD, sizeof(PWD));
    return sizeof(PWD) - 1;
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

static AJ_Status AppHandlePing(AJ_Message* msg)
{
    AJ_Message reply;
    AJ_Arg arg;

    AJ_UnmarshalArg(msg, &arg);
    AJ_MarshalReplyMsg(msg, &reply);
    /*
     * Just return the arg we received
     */
    AJ_MarshalArg(&reply, &arg);
    AJ_InfoPrintf(("Handle Ping\n"));
    return AJ_DeliverMsg(&reply);
}

/*
 * Handles a property GET request so marshals the property value to return
 */
static AJ_Status PropGetHandler(AJ_Message* replyMsg, uint32_t propId, void* context)
{
    if (propId == APP_INT_VAL_PROP) {
        return AJ_MarshalArgs(replyMsg, "i", propVal);
    } else {
        return AJ_ERR_UNEXPECTED;
    }
}

/*
 * Handles a property SET request so unmarshals the property value to apply.
 */
static AJ_Status PropSetHandler(AJ_Message* replyMsg, uint32_t propId, void* context)
{
    if (propId == APP_INT_VAL_PROP) {
        return AJ_UnmarshalArgs(replyMsg, "i", &propVal);
    } else {
        return AJ_ERR_UNEXPECTED;
    }
}

#define UUID_LENGTH 16
#define APP_ID_SIGNATURE "ay"

static AJ_Status MarshalAppId(AJ_Message* msg, const char* appId)
{
    AJ_Status status;
    uint8_t binAppId[UUID_LENGTH];
    uint32_t sz = strlen(appId);

    if (sz > UUID_LENGTH * 2) { // Crop application id that is too long
        sz = UUID_LENGTH * 2;
    }
    status = AJ_HexToRaw(appId, sz, binAppId, UUID_LENGTH);
    if (status != AJ_OK) {
        return status;
    }
    status = AJ_MarshalArgs(msg, "{sv}", "AppId", APP_ID_SIGNATURE, binAppId, sz / 2);

    return status;
}

static AJ_Status AboutPropGetter(AJ_Message* reply, const char* language)
{
    AJ_Status status = AJ_OK;
    AJ_Arg array;
    AJ_GUID theAJ_GUID;
    char machineIdValue[UUID_LENGTH * 2 + 1];
    machineIdValue[UUID_LENGTH * 2] = '\0';

    if ((language != NULL) && (0 != strcmp(language, "en")) && (0 != strcmp(language, ""))) {
        /* the language supplied was not supported */
        status = AJ_ERR_NO_MATCH;
    }

    if (status == AJ_OK) {
        status = AJ_MarshalContainer(reply, &array, AJ_ARG_ARRAY);
        if (status == AJ_OK) {
            status = AJ_GetLocalGUID(&theAJ_GUID);
            if (status == AJ_OK) {
                AJ_GUID_ToString(&theAJ_GUID, machineIdValue, UUID_LENGTH * 2 + 1);
            }
            if (status == AJ_OK) {
                status = MarshalAppId(reply, &machineIdValue[0]);
            }
            if (status == AJ_OK) {
                status = AJ_MarshalArgs(reply, "{sv}", "AppName", "s", "svclite");
            }
            if (status == AJ_OK) {
                status = AJ_MarshalArgs(reply, "{sv}", "DeviceId", "s", machineIdValue);
            }
            if (status == AJ_OK) {
                status = AJ_MarshalArgs(reply, "{sv}", "DeviceName", "s", "Tester");
            }
            if (status == AJ_OK) {
                status = AJ_MarshalArgs(reply, "{sv}", "Manufacturer", "s", "QCE");
            }
            if (status == AJ_OK) {
                status = AJ_MarshalArgs(reply, "{sv}", "ModelNumber", "s", "1.0");
            }
            //SupportedLanguages
            if (status == AJ_OK) {
                AJ_Arg dict;
                AJ_Arg languageListArray;
                status = AJ_MarshalContainer(reply, &dict, AJ_ARG_DICT_ENTRY);
                if (status == AJ_OK) {
                    status = AJ_MarshalArgs(reply, "s", "SupportedLanguages");
                }
                if (status == AJ_OK) {
                    status = AJ_MarshalVariant(reply, "as");
                }
                if (status == AJ_OK) {
                    status = AJ_MarshalContainer(reply, &languageListArray, AJ_ARG_ARRAY);
                }
                if (status == AJ_OK) {
                    status = AJ_MarshalArgs(reply, "s", "en");
                }
                if (status == AJ_OK) {
                    status = AJ_MarshalCloseContainer(reply, &languageListArray);
                }
                if (status == AJ_OK) {
                    status = AJ_MarshalCloseContainer(reply, &dict);
                }

            }
            if (status == AJ_OK) {
                status = AJ_MarshalArgs(reply, "{sv}", "Description", "s", "svclite test app");
            }
            if (status == AJ_OK) {
                status = AJ_MarshalArgs(reply, "{sv}", "DefaultLanguage", "s", "en");
            }
            if (status == AJ_OK) {
                status = AJ_MarshalArgs(reply, "{sv}", "SoftwareVersion", "s", AJ_GetVersion());
            }
            if (status == AJ_OK) {
                status = AJ_MarshalArgs(reply, "{sv}", "AJSoftwareVersion", "s", AJ_GetVersion());
            }
        }
        if (status == AJ_OK) {
            status = AJ_MarshalCloseContainer(reply, &array);
        }
    }
    return status;
}

uint32_t MyBusAuthPwdCB(uint8_t* buf, uint32_t bufLen)
{
    const char* myPwd = "1234";
    strncpy((char*)buf, myPwd, bufLen);
    return (uint32_t)strlen(myPwd);
}

static const uint32_t suites[3] = { AUTH_SUITE_ECDHE_ECDSA, AUTH_SUITE_ECDHE_PSK, AUTH_SUITE_ECDHE_NULL };
static const size_t numsuites = 3;

#define CONNECT_TIMEOUT    (1000 * 1000)
#define UNMARSHAL_TIMEOUT  (1000 * 5)

int AJ_Main()
{
    AJ_Status status = AJ_OK;
    AJ_BusAttachment bus;
    uint8_t connected = FALSE;
    uint32_t sessionId = 0;
    X509CertificateChain* node;

    /*
     * One time initialization before calling any other AllJoyn APIs
     */
    AJ_Initialize();

    AJ_PrintXML(AppObjects);
    AJ_RegisterObjects(AppObjects, NULL);
    AJ_AboutRegisterPropStoreGetter(AboutPropGetter);

    SetBusAuthPwdCallback(MyBusAuthPwdCB);
    while (TRUE) {
        AJ_Message msg;

        if (!connected) {
            status = AJ_StartService(&bus, NULL, CONNECT_TIMEOUT, FALSE, ServicePort, ServiceName, AJ_NAME_REQ_DO_NOT_QUEUE, NULL);
            if (status != AJ_OK) {
                continue;
            }
            AJ_InfoPrintf(("StartService returned AJ_OK\n"));
            AJ_InfoPrintf(("Connected to Daemon:%s\n", AJ_GetUniqueName(&bus)));

            AJ_SetIdleTimeouts(&bus, 10, 4);

            connected = TRUE;
#ifdef SECURE_OBJECT
            status = AJ_SetObjectFlags("/org/alljoyn/alljoyn_test", AJ_OBJ_FLAG_SECURE, 0);
            if (status != AJ_OK) {
                AJ_ErrPrintf(("Error calling AJ_SetObjectFlags.. [%s] \n", AJ_StatusText(status)));
                return -1;
            }
#endif

#if defined(SECURE_INTERFACE) || defined(SECURE_OBJECT)

            /* Register a callback for providing bus authentication password */
            AJ_BusSetPasswordCallback(&bus, PasswordCallback);
            AJ_BusEnableSecurity(&bus, suites, numsuites);
            AJ_BusSetAuthListenerCallback(&bus, AuthListenerCallback);
#endif

            /* Configure timeout for the link to the daemon bus */
            AJ_SetBusLinkTimeout(&bus, 60); // 60 seconds
        }

        status = AJ_UnmarshalMsg(&bus, &msg, UNMARSHAL_TIMEOUT);
        if (AJ_ERR_TIMEOUT == status && AJ_ERR_LINK_TIMEOUT == AJ_BusLinkStateProc(&bus)) {
            status = AJ_ERR_READ;
        }
        if (status != AJ_OK) {
            if (status == AJ_ERR_TIMEOUT) {
                AppDoWork();
                continue;
            }
        }

        if (status == AJ_OK) {
            switch (msg.msgId) {

            case AJ_REPLY_ID(AJ_METHOD_ADD_MATCH):
                if (msg.hdr->msgType == AJ_MSG_ERROR) {
                    AJ_InfoPrintf(("Failed to add match\n"));
                    status = AJ_ERR_FAILURE;
                } else {
                    status = AJ_OK;
                }
                break;

            case AJ_METHOD_ACCEPT_SESSION:
                {
                    uint16_t port;
                    char* joiner;
                    AJ_UnmarshalArgs(&msg, "qus", &port, &sessionId, &joiner);
                    if (port == ServicePort) {
                        status = AJ_BusReplyAcceptSession(&msg, TRUE);
                        AJ_InfoPrintf(("Accepted session session_id=%u joiner=%s\n", sessionId, joiner));
                    } else {
                        status = AJ_BusReplyAcceptSession(&msg, FALSE);
                        AJ_InfoPrintf(("Accepted rejected session_id=%u joiner=%s\n", sessionId, joiner));
                    }
                }
                break;

            case APP_MY_PING:
                status = AppHandlePing(&msg);
                break;

            case APP_GET_PROP:
                status = AJ_BusPropGet(&msg, PropGetHandler, NULL);
                break;

            case APP_SET_PROP:
                status = AJ_BusPropSet(&msg, PropSetHandler, NULL);
                if (status == AJ_OK) {
                    AJ_InfoPrintf(("Property successfully set to %d.\n", propVal));
                } else {
                    AJ_InfoPrintf(("Property set attempt unsuccessful. Status = 0x%04x.\n", status));
                }
                break;

            case AJ_SIGNAL_SESSION_LOST_WITH_REASON:
                {
                    uint32_t id, reason;
                    AJ_UnmarshalArgs(&msg, "uu", &id, &reason);
                    AJ_InfoPrintf(("Session lost. ID = %u, reason = %u", id, reason));
                    if (CancelAdvertiseName) {
                        status = AJ_BusAdvertiseName(&bus, ServiceName, AJ_TRANSPORT_ANY, AJ_BUS_START_ADVERTISING, 0);
                    }
                    status = AJ_ERR_SESSION_LOST;
                }
                break;

            case AJ_SIGNAL_SESSION_JOINED:
                if (CancelAdvertiseName) {
                    status = AJ_BusAdvertiseName(&bus, ServiceName, AJ_TRANSPORT_ANY, AJ_BUS_STOP_ADVERTISING, 0);
                }
                break;

            case AJ_REPLY_ID(AJ_METHOD_CANCEL_ADVERTISE):
            case AJ_REPLY_ID(AJ_METHOD_ADVERTISE_NAME):
                if (msg.hdr->msgType == AJ_MSG_ERROR) {
                    status = AJ_ERR_FAILURE;
                }
                break;

            case AJ_REPLY_ID(AJ_METHOD_BUS_SET_IDLE_TIMEOUTS):
                {
                    uint32_t disposition, idleTo, probeTo;
                    if (msg.hdr->msgType == AJ_MSG_ERROR) {
                        status = AJ_ERR_FAILURE;
                    }
                    AJ_UnmarshalArgs(&msg, "uuu", &disposition, &idleTo, &probeTo);
                    AJ_InfoPrintf(("SetIdleTimeouts response disposition=%u idleTimeout=%u probeTimeout=%u\n", disposition, idleTo, probeTo));
                }
                break;

            case APP_MY_SIGNAL:
                AJ_InfoPrintf(("Received my_signal\n"));
                status = AJ_OK;

                if (ReflectSignal) {
                    AJ_Message out;
                    AJ_Arg arg;
                    AJ_MarshalSignal(&bus, &out, APP_MY_SIGNAL, msg.sender, msg.sessionId, 0, 0);
                    AJ_MarshalContainer(&out, &arg, AJ_ARG_ARRAY);
                    AJ_MarshalCloseContainer(&out, &arg);
                    AJ_DeliverMsg(&out);
                    AJ_CloseMsg(&out);
                }
                break;

            default:
                /*
                 * Pass to the built-in bus message handlers
                 */
                status = AJ_BusHandleBusMessage(&msg);
                break;
            }

            // Any received packets indicates the link is active, so call to reinforce the bus link state
            AJ_NotifyLinkActive();
        }
        /*
         * Unarshaled messages must be closed to free resources
         */
        AJ_CloseMsg(&msg);

        if ((status == AJ_ERR_READ) || (status == AJ_ERR_LINK_DEAD)) {
            AJ_InfoPrintf(("AllJoyn disconnect\n"));
            AJ_InfoPrintf(("Disconnected from Daemon:%s\n", AJ_GetUniqueName(&bus)));
            AJ_Disconnect(&bus);
            connected = FALSE;
            /*
             * Sleep a little while before trying to reconnect
             */
            AJ_Sleep(10 * 1000);
        }
    }
    AJ_WarnPrintf(("svclite EXIT %d\n", status));

#if defined(SECURE_INTERFACE) || defined(SECURE_OBJECT)
    // Clean up certificate chain
    while (chain) {
        node = chain;
        chain = chain->next;
        AJ_Free(node->certificate.der.data);
        AJ_Free(node);
    }
#endif

    return status;
}


#ifdef AJ_MAIN
int main()
{
    return AJ_Main();
}
#endif
