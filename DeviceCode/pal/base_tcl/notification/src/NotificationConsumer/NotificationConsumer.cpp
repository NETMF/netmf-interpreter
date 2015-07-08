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
 * prior to first inclusion of aj_debug.h.
 * The corresponding flag dbgAJNS is defined in NotificationCommon.h and implemented in NotificationCommon.c.
 */
#define AJ_MODULE AJNS
#include <aj_debug.h>

#include <alljoyn.h>
#include <alljoyn/notification/NotificationCommon.h>
#include <alljoyn/notification/NotificationConsumer.h>
#include <alljoyn/notification/NotificationProducer.h>
#include <alljoyn/services_common/ServicesCommon.h>
#include <aj_config.h>

/* Notification ProxyObject bus registration */
#define NOTIFICATION_PROXYOBJECT_INDEX                        NOTIFICATION_CONSUMER_OBJECTS_INDEX

#define INTERFACE_GET_PROPERTY_PROXY                          AJ_ENCODE_MESSAGE_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_PROXYOBJECT_INDEX, 0, AJ_PROP_GET)
#define INTERFACE_SET_PROPERTY_PROXY                          AJ_ENCODE_MESSAGE_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_PROXYOBJECT_INDEX, 0, AJ_PROP_SET)

#define NOTIFICATION_SIGNAL_RECEIVED                          AJ_ENCODE_MESSAGE_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_PROXYOBJECT_INDEX, 1, 0)
#define GET_NOTIFICATION_VERSION_PROPERTY_PROXY               AJ_ENCODE_PROPERTY_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_PROXYOBJECT_INDEX, 1, 1)

#define SUPERAGENT_SIGNAL_RECEIVED                            AJ_ENCODE_MESSAGE_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_PROXYOBJECT_INDEX, 2, 0)
#define GET_SUPERAGENT_VERSION_PROPERTY_PROXY                 AJ_ENCODE_PROPERTY_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_PROXYOBJECT_INDEX, 2, 1)

/* Producer ProxyObject bus registration */
#define NOTIFICATION_PRODUCER_PROXYOBJECT_INDEX               NOTIFICATION_PROXYOBJECT_INDEX + 1
#define NOTIFICATION_PRODUCER_GET_PROPERTY_PROXY              AJ_ENCODE_MESSAGE_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_PRODUCER_PROXYOBJECT_INDEX, 0, AJ_PROP_GET)
#define NOTIFICATION_PRODUCER_SET_PROPERTY_PROXY              AJ_ENCODE_MESSAGE_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_PRODUCER_PROXYOBJECT_INDEX, 0, AJ_PROP_SET)

#define NOTIFICATION_PRODUCER_DISMISS_PROXY                   AJ_ENCODE_MESSAGE_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_PRODUCER_PROXYOBJECT_INDEX, 1, 0)
#define GET_NOTIFICATION_PRODUCER_VERSION_PROPERTY_PROXY      AJ_ENCODE_PROPERTY_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_PRODUCER_PROXYOBJECT_INDEX, 1, 1)

/* Dismisser ProxyObject bus registration */
#define NOTIFICATION_DISMISSER_PROXYOBJECT_INDEX              NOTIFICATION_PRODUCER_PROXYOBJECT_INDEX + 1
#define NOTIFICATION_DISMISSER_GET_PROPERTY_PROXY             AJ_ENCODE_MESSAGE_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_DISMISSER_PROXYOBJECT_INDEX, 0, AJ_PROP_GET)
#define NOTIFICATION_DISMISSER_SET_PROPERTY_PROXY             AJ_ENCODE_MESSAGE_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_DISMISSER_PROXYOBJECT_INDEX, 0, AJ_PROP_SET)

#define NOTIFICATION_DISMISSER_DISMISS_RECEIVED               AJ_ENCODE_MESSAGE_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_DISMISSER_PROXYOBJECT_INDEX, 1, 0)
#define GET_NOTIFICATION_DISMISSER_VERSION_PROPERTY_PROXY     AJ_ENCODE_PROPERTY_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_PRODUCER_PROXYOBJECT_INDEX, 1, 1)

/**
 * Static constants.
 */
static const char SuperagentInterfaceName[]  = "org.alljoyn.Notification.Superagent";
static const char notificationMatch[] = "interface='org.alljoyn.Notification',sessionless='t'";
static const char superAgentMatch[] = "interface='org.alljoyn.Notification.Superagent',sessionless='t'";
static const char superAgentFilterMatch[] = "interface='org.alljoyn.Notification.Superagent',sessionless='t',sender='";
static const char dismisserMatch[] = "interface='org.alljoyn.Notification.Dismisser',sessionless='t'";

static const char* SuperagentInterface[] = {
    SuperagentInterfaceName,
    AJNS_NotificationSignalName,
    AJNS_NotificationPropertyVersion,
    NULL
};

/**
 * A NULL terminated collection of all interfaces.
 */
static const AJ_InterfaceDescription SuperagentInterfaces[] = {
    AJ_PropertiesIface,
    SuperagentInterface,
    NULL
};

static const AJ_InterfaceDescription AllInterfaces[] = {
    AJ_PropertiesIface,
    AJNS_NotificationInterface,
    SuperagentInterface,
    NULL
};

static AJ_Object AllProxyObject          = { "!",   AllInterfaces };
static AJ_Object SuperAgentProxyObject   = { "!",   SuperagentInterfaces };
static AJ_Object NotificationProxyObject = { "!",   AJNS_NotificationInterfaces };

static uint8_t appSuperAgentMode = TRUE;

static char currentSuperAgentBusName[16] = { '\0' };
static uint32_t producerSessionId = 0;
static uint32_t lastSessionRequestSerialNum = 0;

static AJSVC_MethodCallCompleted onDismissCompleted = NULL;
static AJNS_Consumer_NotificationReference notificationInProcess;

static AJNS_Consumer_OnNotify appOnNotify;
static AJNS_Consumer_OnDismiss appOnDismiss;

static AJNS_DictionaryEntry textsRecd[NUMALLOWEDTEXTS], customAttributesRecd[NUMALLOWEDCUSTOMATTRIBUTES], richAudiosRecd[NUMALLOWEDRICHNOTS];

static AJ_Status RegisterObjectList()
{
    AJNS_Common_RegisterObjects();

    AllProxyObject.flags &= ~(AJ_OBJ_FLAG_HIDDEN | AJ_OBJ_FLAG_DISABLED);
    AllProxyObject.flags |= AJ_OBJ_FLAG_IS_PROXY;

    SuperAgentProxyObject.flags &= ~(AJ_OBJ_FLAG_HIDDEN | AJ_OBJ_FLAG_DISABLED);
    SuperAgentProxyObject.flags |= AJ_OBJ_FLAG_IS_PROXY;

    NotificationProxyObject.flags &= ~(AJ_OBJ_FLAG_HIDDEN | AJ_OBJ_FLAG_DISABLED);
    NotificationProxyObject.flags |= AJ_OBJ_FLAG_IS_PROXY;

    AJNS_ObjectList[NOTIFICATION_PRODUCER_PROXYOBJECT_INDEX].flags &= ~(AJ_OBJ_FLAG_HIDDEN | AJ_OBJ_FLAG_DISABLED);
    AJNS_ObjectList[NOTIFICATION_PRODUCER_PROXYOBJECT_INDEX].flags |= AJ_OBJ_FLAG_IS_PROXY;

    AJNS_ObjectList[NOTIFICATION_DISMISSER_PROXYOBJECT_INDEX].flags &= ~(AJ_OBJ_FLAG_HIDDEN | AJ_OBJ_FLAG_DISABLED);
    AJNS_ObjectList[NOTIFICATION_DISMISSER_PROXYOBJECT_INDEX].flags |= AJ_OBJ_FLAG_IS_PROXY;

    return AJ_RegisterObjectList(AJNS_ObjectList, AJNS_OBJECT_LIST_INDEX);
}

AJ_Status AJNS_Consumer_Start(uint8_t superAgentMode, AJNS_Consumer_OnNotify onNotify, AJNS_Consumer_OnDismiss onDismiss)
{
    AJ_Status status = AJ_OK;

    appSuperAgentMode = superAgentMode;
    appOnNotify = onNotify;
    appOnDismiss = onDismiss;

    AJNS_ObjectList[NOTIFICATION_PROXYOBJECT_INDEX] = appSuperAgentMode ? AllProxyObject : NotificationProxyObject;

    status = RegisterObjectList();

    return status;
}

AJ_Status AJNS_Consumer_SetSignalRules(AJ_BusAttachment* busAttachment, uint8_t superAgentMode, const char* senderBusName)
{
    AJ_Status status = AJ_OK;
    char senderMatch[76];
    size_t availableLen;

    AJ_InfoPrintf(("In SetSignalRules()\n"));
    AJ_InfoPrintf(("Adding Dismisser interface match.\n"));
    status = AJ_BusSetSignalRuleFlags(busAttachment, dismisserMatch, AJ_BUS_SIGNAL_ALLOW, AJ_FLAG_NO_REPLY_EXPECTED);
    if (status != AJ_OK) {
        AJ_ErrPrintf(("Could not set Dismisser Interface AddMatch\n"));
        return status;
    }

    if (senderBusName == NULL) {
        AJ_InfoPrintf(("Adding Notification interface match.\n"));

        status = AJ_BusSetSignalRuleFlags(busAttachment, notificationMatch, AJ_BUS_SIGNAL_ALLOW, AJ_FLAG_NO_REPLY_EXPECTED);
        if (status != AJ_OK) {
            AJ_ErrPrintf(("Could not set Notification Interface AddMatch\n"));
            return status;
        }

        if (currentSuperAgentBusName[0]) {
            AJ_InfoPrintf(("Removing Superagent interface matched for specific sender bus name %s.\n", currentSuperAgentBusName));

            availableLen = sizeof(senderMatch);
            availableLen -= strlen(strncpy(senderMatch, superAgentFilterMatch, availableLen));
            availableLen -= strlen(strncat(senderMatch, currentSuperAgentBusName, availableLen));
            availableLen -= strlen(strncat(senderMatch, "'", availableLen));

            status = AJ_BusSetSignalRuleFlags(busAttachment, senderMatch, AJ_BUS_SIGNAL_DENY, AJ_FLAG_NO_REPLY_EXPECTED);
            if (status != AJ_OK) {
                AJ_ErrPrintf(("Could not remove SuperAgent specific match\n"));
                return status;
            }

            status = AJ_BusFindAdvertisedName(busAttachment, currentSuperAgentBusName, AJ_BUS_STOP_FINDING);
            if (status != AJ_OK) {
                AJ_ErrPrintf(("Could not unregister to find advertised name of lost SuperAgent\n"));
                return status;
            }

            currentSuperAgentBusName[0] = '\0'; // Clear current SuperAgent BusUniqueName
        }

        if (superAgentMode) {
            AJ_InfoPrintf(("Adding Superagent interface match.\n"));
            status = AJ_BusSetSignalRuleFlags(busAttachment, superAgentMatch, AJ_BUS_SIGNAL_ALLOW, AJ_FLAG_NO_REPLY_EXPECTED);
            if (status != AJ_OK) {
                AJ_ErrPrintf(("Could not set Notification Interface AddMatch\n"));
                return status;
            }
        }
    } else {
        AJ_InfoPrintf(("Running SetSignalRules with sender bus name.\n"));

        AJ_InfoPrintf(("Removing Notification interface match.\n"));
        status = AJ_BusSetSignalRuleFlags(busAttachment, notificationMatch, AJ_BUS_SIGNAL_DENY, AJ_FLAG_NO_REPLY_EXPECTED);
        if (status != AJ_OK) {
            AJ_ErrPrintf(("Could not remove Notification Interface match\n"));
            return status;
        }

        availableLen = sizeof(senderMatch);
        availableLen -= strlen(strncpy(senderMatch, superAgentFilterMatch, availableLen));
        availableLen -= strlen(strncat(senderMatch, senderBusName, availableLen));
        availableLen -= strlen(strncat(senderMatch, "'", availableLen));

        AJ_InfoPrintf(("Adding Superagent interface matched for specific sender bus name %s.\n", senderBusName));

        status = AJ_BusSetSignalRuleFlags(busAttachment, senderMatch, AJ_BUS_SIGNAL_ALLOW, AJ_FLAG_NO_REPLY_EXPECTED);
        if (status != AJ_OK) {
            AJ_ErrPrintf(("Could not add SuperAgent specific match\n"));
            return status;
        }

        status = AJ_BusFindAdvertisedName(busAttachment, senderBusName, AJ_BUS_START_FINDING);
        if (status != AJ_OK) {
            AJ_ErrPrintf(("Could not register to find advertised name of SuperAgent\n"));
            return status;
        }

        strncpy(currentSuperAgentBusName, senderBusName, 16); // Save current SuperAgent BusUniqueName

        AJ_InfoPrintf(("Removing Superagent interface match.\n"));
        status = AJ_BusSetSignalRuleFlags(busAttachment, superAgentMatch, AJ_BUS_SIGNAL_DENY, AJ_FLAG_NO_REPLY_EXPECTED);
        if (status != AJ_OK) {
            AJ_ErrPrintf(("Could not remove SuperAgent Interface match\n"));
            return status;
        }
    }

    return status;
}

uint8_t AJNS_Consumer_IsSuperAgentLost(AJ_Message* msg)
{
    if (msg->msgId == AJ_SIGNAL_LOST_ADV_NAME) {
        AJ_Arg arg;
        AJ_UnmarshalArg(msg, &arg); // <arg name="name" type="s" direction="out"/>
        AJ_SkipArg(msg);            // <arg name="transport" type="q" direction="out"/>
        AJ_SkipArg(msg);            // <arg name="prefix" type="s" direction="out"/>
        AJ_ResetArgs(msg);          // Reset to allow others to re-unmarshal message
        AJ_InfoPrintf(("LostAdvertisedName(%s)\n", arg.val.v_string));
        return (strcmp(arg.val.v_string, currentSuperAgentBusName) == 0);
    }
    return FALSE;
}

AJ_Status AJNS_Consumer_NotifySignalHandler(AJ_Message* msg)
{
    AJ_Status status;
    AJNS_Notification notification;
    AJNS_NotificationContent content;

    char appId[UUID_LENGTH * 2 + 1];
    AJ_Arg attrbtArray;
    AJ_Arg customAttributeArray;
    AJ_Arg notTextArray;
    AJ_Arg richAudioArray;

    memset(&notification, 0, sizeof(AJNS_Notification));
    memset(&content, 0, sizeof(AJNS_NotificationContent));
    notification.content = &content;

    AJ_InfoPrintf(("Received notification signal from sender %s\n", msg->sender));

    status = AJ_UnmarshalArgs(msg, "q", &notification.version);
    if (status != AJ_OK) {
        goto Exit;
    }

    status = AJ_UnmarshalArgs(msg, "i", &notification.notificationId);
    if (status != AJ_OK) {
        goto Exit;
    }

    status = AJ_UnmarshalArgs(msg, "q", &notification.messageType);
    if (status != AJ_OK) {
        goto Exit;
    }

    status = AJ_UnmarshalArgs(msg, "s", &notification.deviceId);
    if (status != AJ_OK) {
        goto Exit;
    }

    status = AJ_UnmarshalArgs(msg, "s", &notification.deviceName);
    if (status != AJ_OK) {
        goto Exit;
    }

    status = AJSVC_UnmarshalAppId(msg, appId, sizeof(appId));
    if (status != AJ_OK) {
        goto Exit;
    }
    notification.appId = appId;

    status = AJ_UnmarshalArgs(msg, "s", &notification.appName);
    if (status != AJ_OK) {
        goto Exit;
    }

    status = AJ_UnmarshalContainer(msg, &attrbtArray, AJ_ARG_ARRAY);
    if (status != AJ_OK) {
        goto Exit;
    }

    while (1) {
        AJ_Arg dictArg;
        int32_t attrbtKey;
        const char* variantSig;

        status = AJ_UnmarshalContainer(msg, &dictArg, AJ_ARG_DICT_ENTRY);
        if (status == AJ_ERR_NO_MORE) {
            status = AJ_UnmarshalCloseContainer(msg, &attrbtArray);
            if (status != AJ_OK) {
                goto Exit;
            } else {
                break;
            }
        } else if (status != AJ_OK) {
            goto Exit;
        }

        status = AJ_UnmarshalArgs(msg, "i", &attrbtKey);
        if (status != AJ_OK) {
            goto Exit;
        }

        switch (attrbtKey) {
        case AJNS_RICH_CONTENT_ICON_URL_ATTRIBUTE_KEY:
            {
                status = AJ_UnmarshalVariant(msg, &variantSig);
                if (status != AJ_OK) {
                    goto Exit;
                }
                status = AJ_UnmarshalArgs(msg, "s", &(notification.content->richIconUrl));
                if (status != AJ_OK) {
                    goto Exit;
                }
            }
            break;

        case AJNS_RICH_CONTENT_ICON_OBJECT_PATH_ATTRIBUTE_KEY:
            {
                status = AJ_UnmarshalVariant(msg, &variantSig);
                if (status != AJ_OK) {
                    goto Exit;
                }
                status = AJ_UnmarshalArgs(msg, "s", &(notification.content->richIconObjectPath));
                if (status != AJ_OK) {
                    goto Exit;
                }
            }
            break;

        case AJNS_RICH_CONTENT_AUDIO_OBJECT_PATH_ATTRIBUTE_KEY:
            {
                status = AJ_UnmarshalVariant(msg, &variantSig);
                if (status != AJ_OK) {
                    goto Exit;
                }
                status = AJ_UnmarshalArgs(msg, "s", &(notification.content->richAudioObjectPath));
                if (status != AJ_OK) {
                    goto Exit;
                }
            }
            break;

        case AJNS_CONTROLPANELSERVICE_OBJECT_PATH_ATTRIBUTE_KEY:
            {
                status = AJ_UnmarshalVariant(msg, &variantSig);
                if (status != AJ_OK) {
                    goto Exit;
                }
                status = AJ_UnmarshalArgs(msg, "s", &(notification.content->controlPanelServiceObjectPath));
                if (status != AJ_OK) {
                    goto Exit;
                }
            }
            break;

        case AJNS_ORIGINAL_SENDER_NAME_ATTRIBUTE_KEY:
            {
                status = AJ_UnmarshalVariant(msg, &variantSig);
                if (status != AJ_OK) {
                    goto Exit;
                }
                status = AJ_UnmarshalArgs(msg, "s", &notification.originalSenderName);
                if (status != AJ_OK) {
                    goto Exit;
                }
            }
            break;

        case AJNS_RICH_CONTENT_AUDIO_URL_ATTRIBUTE_KEY:
            {
                status = AJ_UnmarshalVariant(msg, &variantSig);
                if (status != AJ_OK) {
                    goto Exit;
                }
                status = AJ_UnmarshalContainer(msg, &richAudioArray, AJ_ARG_ARRAY);
                if (status != AJ_OK) {
                    goto Exit;
                }

                while (1) {
                    AJ_Arg structArg;
                    char* urlLanguage;
                    char* urlText;

                    status = AJ_UnmarshalContainer(msg, &structArg, AJ_ARG_STRUCT);
                    if (status == AJ_ERR_NO_MORE) {
                        status = AJ_UnmarshalCloseContainer(msg, &richAudioArray);
                        if (status != AJ_OK) {
                            goto Exit;
                        } else {
                            break;
                        }
                    } else if (status != AJ_OK) {
                        goto Exit;
                    }

                    status = AJ_UnmarshalArgs(msg, "ss", &urlLanguage, &urlText);
                    if (status != AJ_OK) {
                        goto Exit;
                    }
                    if (notification.content->numAudioUrls < NUMALLOWEDRICHNOTS) {             // if it doesn't fit we just skip
                        richAudiosRecd[notification.content->numAudioUrls].key   = urlLanguage;
                        richAudiosRecd[notification.content->numAudioUrls].value = urlText;
                        notification.content->numAudioUrls++;
                    }

                    status = AJ_UnmarshalCloseContainer(msg, &structArg);
                    if (status != AJ_OK) {
                        goto Exit;
                    }
                }
                notification.content->richAudioUrls = richAudiosRecd;
            }
            break;

        default:
            AJ_InfoPrintf(("Unknown argument - skipping\n"));
            status = AJ_SkipArg(msg);
            if (status != AJ_OK) {
                AJ_ErrPrintf(("Error could not skip argument\n"));
                return status;
            }
        }
        status = AJ_UnmarshalCloseContainer(msg, &dictArg);
        if (status != AJ_OK) {
            goto Exit;
        }
    }

    status = AJ_UnmarshalContainer(msg, &customAttributeArray, AJ_ARG_ARRAY);
    if (status != AJ_OK) {
        goto Exit;
    }

    while (1) {
        AJ_Arg customAttributeDictArg;
        char* customKey;
        char* customVal;

        status = AJ_UnmarshalContainer(msg, &customAttributeDictArg, AJ_ARG_DICT_ENTRY);
        if (status == AJ_ERR_NO_MORE) {
            status = AJ_UnmarshalCloseContainer(msg, &customAttributeArray);
            if (status != AJ_OK) {
                goto Exit;
            } else {
                break;
            }
        } else if (status != AJ_OK) {
            goto Exit;
        }

        status = AJ_UnmarshalArgs(msg, "ss", &customKey, &customVal);
        if (status != AJ_OK) {
            goto Exit;
        }

        if (notification.content->numCustomAttributes < NUMALLOWEDCUSTOMATTRIBUTES) {     // if it doesn't fit we just skip
            customAttributesRecd[notification.content->numCustomAttributes].key   = customKey;
            customAttributesRecd[notification.content->numCustomAttributes].value = customVal;
            notification.content->numCustomAttributes++;
        }

        status = AJ_UnmarshalCloseContainer(msg, &customAttributeDictArg);
        if (status != AJ_OK) {
            goto Exit;
        }
    }
    notification.content->customAttributes = customAttributesRecd;

    status = AJ_UnmarshalContainer(msg, &notTextArray, AJ_ARG_ARRAY);
    if (status != AJ_OK) {
        goto Exit;
    }

    while (1) {
        AJ_Arg structArg;
        char* notificationLanguage;
        char* notificationText;

        status = AJ_UnmarshalContainer(msg, &structArg, AJ_ARG_STRUCT);
        if (status == AJ_ERR_NO_MORE) {
            status = AJ_UnmarshalCloseContainer(msg, &notTextArray);
            if (status != AJ_OK) {
                goto Exit;
            } else {
                break;
            }
        } else if (status != AJ_OK) {
            goto Exit;
        }

        status = AJ_UnmarshalArgs(msg, "ss", &notificationLanguage, &notificationText);
        if (status != AJ_OK) {
            goto Exit;
        }

        if (notification.content->numTexts < NUMALLOWEDTEXTS) {     // if it doesn't fit we just skip
            textsRecd[notification.content->numTexts].key   = notificationLanguage;
            textsRecd[notification.content->numTexts].value = notificationText;
            notification.content->numTexts++;
        }

        status = AJ_UnmarshalCloseContainer(msg, &structArg);
        if (status != AJ_OK) {
            goto Exit;
        }
    }
    notification.content->texts = textsRecd;

Exit:

    if (status != AJ_OK) {
        AJ_ErrPrintf(("Handle Notification failed: '%s'\n", AJ_StatusText(status)));
    } else {
        if (appOnNotify) {
            status = (*appOnNotify)(&notification);
        }
    }

    AJ_CloseMsg(msg);

    return status;
}

AJ_Status AJNS_Consumer_DismissSignalHandler(AJ_Message* msg)
{
    AJ_Status status;
    uint32_t notificationId = 0;
    char appId[UUID_LENGTH * 2 + 1];

    status = AJ_UnmarshalArgs(msg, "i", &notificationId);
    if (status != AJ_OK) {
        goto Exit;
    }

    status = AJSVC_UnmarshalAppId(msg, appId, sizeof(appId));
    if (status != AJ_OK) {
        goto Exit;
    }

    if (appOnDismiss) {
        (*appOnDismiss)(notificationId, appId);
    }

Exit:

    AJ_CloseMsg(msg);

    return status;
}

AJ_Status AJNS_Consumer_CreateSessionWithProducer(AJ_BusAttachment* busAttachment, const char* senderName, uint32_t* requestSerialNumber)
{
    AJ_Status status = AJ_OK;
    AJ_SessionOpts sessionOpts = {
        AJ_SESSION_TRAFFIC_MESSAGES,
        AJ_SESSION_PROXIMITY_ANY,
        AJ_TRANSPORT_ANY,
        FALSE
    };

    AJ_InfoPrintf(("Inside CreateSessionWithProducer()\n"));
    *requestSerialNumber = busAttachment->serial;
    AJ_InfoPrintf(("CreateSessionWithProducer(): Joining session with %s on port %u with serial number %u\n", senderName, AJNS_NotificationProducerPort, *requestSerialNumber));
    status = AJ_BusJoinSession(busAttachment, senderName, AJNS_NotificationProducerPort, &sessionOpts);
    AJ_InfoPrintf(("CreateSessionWithProducer(): AJ_BusJoinSession() returned with status %s\n", AJ_StatusText(status)));
    return status;
}

AJ_Status AJNS_Consumer_SendDismissRequest(AJ_BusAttachment* busAttachment, uint16_t version, int32_t notificationId, const char* appId, const char* senderName, uint32_t sessionId)
{
    AJ_Status status = AJ_OK;

    if ((status == AJ_OK) && (sessionId != 0)) {
        AJ_Message dismissMsg;
        status = AJ_MarshalMethodCall(busAttachment, &dismissMsg, NOTIFICATION_PRODUCER_DISMISS_PROXY, senderName, sessionId, AJ_NO_FLAGS, AJ_CALL_TIMEOUT);
        if (status != AJ_OK) {
            AJ_ErrPrintf(("Could not marshal method call\n"));
            return status;
        }
        status = AJ_MarshalArgs(&dismissMsg, "i", notificationId);
        if (status != AJ_OK) {
            AJ_ErrPrintf(("Could not marshal arguments\n"));
            return status;
        }
        status = AJ_DeliverMsg(&dismissMsg);
        if (status != AJ_OK) {
            AJ_ErrPrintf(("Could not deliver message\n"));
            return status;
        }
        AJ_CloseMsg(&dismissMsg);
    }

    return status;
}

AJ_Status AJNS_Consumer_ConnectedHandler(AJ_BusAttachment* busAttachment)
{
    return AJNS_Consumer_SetSignalRules(busAttachment, appSuperAgentMode, 0);
}

AJSVC_ServiceStatus AJNS_Consumer_SessionJoinedHandler(AJ_BusAttachment* busAttachment, uint32_t sessionId, uint32_t replySerialNum)
{
    AJSVC_ServiceStatus serviceStatus = AJSVC_SERVICE_STATUS_NOT_HANDLED;
    AJ_Status status;

    if (lastSessionRequestSerialNum != 0 && lastSessionRequestSerialNum == replySerialNum) { // Check if this is a reply to our request
        AJ_InfoPrintf(("HandleSessionJoined(): Got reply serial number %u that matches last request serial number %u\n", replySerialNum, lastSessionRequestSerialNum));
        if (producerSessionId == 0) {
            producerSessionId = sessionId;
            if (onDismissCompleted) {
                status = AJNS_Consumer_SendDismissRequest(busAttachment, notificationInProcess.version, notificationInProcess.notificationId, notificationInProcess.appId, notificationInProcess.originalSenderName, producerSessionId);
                AJ_InfoPrintf(("HandleSessionJoined(): SendDismissRequest returned status %s\n", AJ_StatusText(status)));
            }
            serviceStatus = AJSVC_SERVICE_STATUS_HANDLED;
        }
    }

    return serviceStatus;
}

AJSVC_ServiceStatus AJNS_Consumer_SessionRejectedHandler(AJ_BusAttachment* busAttachment, uint32_t replySerialNum, uint32_t replyCode)
{
    AJSVC_ServiceStatus serviceStatus = AJSVC_SERVICE_STATUS_NOT_HANDLED;
    AJ_Status status;

    if (lastSessionRequestSerialNum != 0 && lastSessionRequestSerialNum == replySerialNum) { // Check if this is a reply to our request
        AJ_InfoPrintf(("HandleSessionRejected(): Got reply serial number %u that matches last request serial number %u\n", replySerialNum, lastSessionRequestSerialNum));
        if (producerSessionId == 0) {
            if (onDismissCompleted) {
                status = AJNS_SendDismissSignal(busAttachment, notificationInProcess.notificationId, notificationInProcess.appId);
                AJ_InfoPrintf(("HandleSessionJoinFailed(): SendDismissSignal returned status %s\n", AJ_StatusText(status)));
                (*onDismissCompleted)(status, NULL);
                onDismissCompleted = NULL;
            }
            memset(&notificationInProcess, 0, sizeof(AJNS_Consumer_NotificationReference));
            lastSessionRequestSerialNum = 0;
            serviceStatus = AJSVC_SERVICE_STATUS_HANDLED;
        }
    }

    return serviceStatus;
}

AJSVC_ServiceStatus AJNS_Consumer_SessionLostHandler(AJ_BusAttachment* busAttachment, uint32_t sessionId, uint32_t reason)
{
    AJSVC_ServiceStatus serviceStatus = AJSVC_SERVICE_STATUS_NOT_HANDLED;
    AJ_Status status = AJ_OK;

    if ((lastSessionRequestSerialNum != 0) && (producerSessionId == sessionId)) {
        if (onDismissCompleted) {
            (*onDismissCompleted)(status, NULL);
            onDismissCompleted = NULL;
        }
        memset(&notificationInProcess, 0, sizeof(AJNS_Consumer_NotificationReference));
        lastSessionRequestSerialNum = 0;
        producerSessionId = 0;
        serviceStatus = AJSVC_SERVICE_STATUS_HANDLED;
    }

    return serviceStatus;
}

AJ_Status AJNS_Consumer_DismissNotification(AJ_BusAttachment* busAttachment, uint16_t version, int32_t notificationId, const char* appId, const char* senderName, AJSVC_MethodCallCompleted completedCallback)
{
    AJ_Status status = AJ_OK;

    AJ_InfoPrintf(("Inside DismissNotification()\n"));

    if ((version < 2) || (senderName == NULL) || (senderName[0] == '\0')) { // Producer does not support dismissal but other consumers might so send Dismiss signal
        status = AJNS_SendDismissSignal(busAttachment, notificationId, appId);
        return status;
    }

    if (!lastSessionRequestSerialNum) {
        onDismissCompleted = completedCallback;
        status = AJNS_Consumer_CreateSessionWithProducer(busAttachment, senderName, &lastSessionRequestSerialNum);
        if (status == AJ_OK) {
            notificationInProcess.version = version;
            notificationInProcess.notificationId = notificationId;
            strncpy(notificationInProcess.appId, appId, sizeof(notificationInProcess.appId));
            strncpy(notificationInProcess.originalSenderName, senderName, sizeof(notificationInProcess.originalSenderName));
        } else {
            if (onDismissCompleted) {
                status = AJNS_SendDismissSignal(busAttachment, notificationId, appId);
                AJ_InfoPrintf(("DismissNotification(): SendDismissSignal returned status %s\n", AJ_StatusText(status)));
                (*onDismissCompleted)(status, NULL);
                onDismissCompleted = NULL;
            }
            memset(&notificationInProcess, 0, sizeof(AJNS_Consumer_NotificationReference));
        }
    }

    return status;
}

AJSVC_ServiceStatus AJNS_Consumer_MessageProcessor(AJ_BusAttachment* busAttachment, AJ_Message* msg, AJ_Status* msgStatus)
{
    switch (msg->msgId) {
    case NOTIFICATION_SIGNAL_RECEIVED:
        AJ_InfoPrintf(("Received Producer signal.\n"));
        *msgStatus = AJNS_Consumer_NotifySignalHandler(msg);
        break;

    case SUPERAGENT_SIGNAL_RECEIVED:
        AJ_InfoPrintf(("Received Superagent signal.\n"));
        *msgStatus = AJNS_Consumer_SetSignalRules(busAttachment, appSuperAgentMode, msg->sender);
        if (AJ_OK == *msgStatus && AJNS_ObjectList != NULL) {
            AJNS_ObjectList[NOTIFICATION_PROXYOBJECT_INDEX] = SuperAgentProxyObject;
        }
        *msgStatus = AJNS_Consumer_NotifySignalHandler(msg);
        break;

    case NOTIFICATION_DISMISSER_DISMISS_RECEIVED:
        AJ_InfoPrintf(("Received Dismisser Dismiss signal.\n"));
        *msgStatus = AJNS_Consumer_DismissSignalHandler(msg);
        break;

    case AJ_SIGNAL_LOST_ADV_NAME:
        if (appSuperAgentMode && AJNS_Consumer_IsSuperAgentLost(msg)) {
            *msgStatus = AJNS_Consumer_SetSignalRules(busAttachment, appSuperAgentMode, NULL);
            if (AJ_OK == *msgStatus && AJNS_ObjectList != NULL) {
                AJNS_ObjectList[NOTIFICATION_PROXYOBJECT_INDEX] = AllProxyObject;
            }
        } else {
            return AJSVC_SERVICE_STATUS_NOT_HANDLED;
        }
        break;

    case AJ_REPLY_ID(NOTIFICATION_PRODUCER_DISMISS_PROXY):
        if (producerSessionId != 0) {
            AJ_InfoPrintf(("NotificationProducer method replied. Leaving session %u\n", producerSessionId));
            *msgStatus = AJ_BusLeaveSession(busAttachment, producerSessionId);
            if (AJ_OK != *msgStatus) {
                AJ_InfoPrintf(("Failed to leave session %u\n", producerSessionId));
            }
            producerSessionId = 0;
            return AJNS_Consumer_SessionLostHandler(busAttachment, producerSessionId, 0); // Don't wait for SessionLost handle it immediately
        }
        break;

    default:
        return AJSVC_SERVICE_STATUS_NOT_HANDLED;
    }
    return AJSVC_SERVICE_STATUS_HANDLED;
}

AJ_Status AJNS_Consumer_DisconnectHandler(AJ_BusAttachment* busAttachment)
{
    return AJ_OK;
}
