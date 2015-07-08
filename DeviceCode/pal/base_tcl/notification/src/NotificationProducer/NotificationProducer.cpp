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
#include <alljoyn/notification/NotificationProducer.h>
#include <alljoyn/services_common/PropertyStore.h>
#include <aj_crypto.h>
#include <aj_config.h>

static const uint16_t AJNS_NotificationProducerVersion = 1;

static AJ_Status RegisterObjectList()
{
    uint8_t i = NOTIFICATION_PRODUCER_OBJECTS_INDEX;

    AJNS_Common_RegisterObjects();
    for (; i < NOTIFICATION_PRODUCER_OBJECTS_INDEX + NOTIFICATION_PRODUCER_OBJECTS_COUNT; i++) {
        AJNS_ObjectList[i].flags &= ~(AJ_OBJ_FLAG_HIDDEN | AJ_OBJ_FLAG_DISABLED);
        AJNS_ObjectList[i].flags |= AJ_OBJ_FLAG_ANNOUNCED;
    }

    return AJ_RegisterObjectList(AJNS_ObjectList, AJNS_OBJECT_LIST_INDEX);
}

AJ_Status AJNS_Producer_Start()
{
    AJ_Status status;

    status = RegisterObjectList();

    return status;
}

/*!
   \brief Get Property event for Emergency NotificationService object
   \details
   \dontinclude ProducerSample.c
   \skip switch (msg->msgId)
   \until }
 */
#define EMERGENCY_NOTIFICATION_GET_PROPERTY             AJ_ENCODE_MESSAGE_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_OBJECT_INDEX + AJNS_NOTIFICATION_MESSAGE_TYPE_EMERGENCY, 0, AJ_PROP_GET)

/*!
   \brief Get Property event for Warning NotificationService object
   \details
   \dontinclude ProducerSample.c
   \skip switch (msg->msgId)
   \until }
 */
#define WARNING_NOTIFICATION_GET_PROPERTY               AJ_ENCODE_MESSAGE_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_OBJECT_INDEX + AJNS_NOTIFICATION_MESSAGE_TYPE_WARNING, 0, AJ_PROP_GET)

/*!
   \brief Get Property event for Info NotificationService object
   \details
   \dontinclude ProducerSample.c
   \skip switch (msg->msgId)
   \until }
 */
#define INFO_NOTIFICATION_GET_PROPERTY                  AJ_ENCODE_MESSAGE_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_OBJECT_INDEX + AJNS_NOTIFICATION_MESSAGE_TYPE_INFO, 0, AJ_PROP_GET)

/*!
   \brief Set Property event for Emergency NotificationService object
   \details
   \dontinclude ProducerSample.c
   \skip switch (msg->msgId)
   \until }
 */
#define EMERGENCY_NOTIFICATION_SET_PROPERTY             AJ_ENCODE_MESSAGE_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_OBJECT_INDEX + AJNS_NOTIFICATION_MESSAGE_TYPE_EMERGENCY, 0, AJ_PROP_SET)

/*!
   \brief Set Property event for Warning NotificationService object
   \details
   \dontinclude ProducerSample.c
   \skip switch (msg->msgId)
   \until }
 */
#define WARNING_NOTIFICATION_SET_PROPERTY               AJ_ENCODE_MESSAGE_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_OBJECT_INDEX + AJNS_NOTIFICATION_MESSAGE_TYPE_WARNING, 0, AJ_PROP_SET)

/*!
   \brief Set Property event for Info NotificationService object
   \details
   \dontinclude ProducerSample.c
   \skip switch (msg->msgId)
   \until }
 */
#define INFO_NOTIFICATION_SET_PROPERTY                  AJ_ENCODE_MESSAGE_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_OBJECT_INDEX + AJNS_NOTIFICATION_MESSAGE_TYPE_INFO, 0, AJ_PROP_SET)

/*!
   \brief Get Version event for Emergency NotificationService object
   \details
   \dontinclude ProducerSample.c
   \skip switch (msg->msgId)
   \until }
 */
#define GET_EMERGENCY_NOTIFICATION_VERSION_PROPERTY     AJ_ENCODE_PROPERTY_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_OBJECT_INDEX + AJNS_NOTIFICATION_MESSAGE_TYPE_EMERGENCY, 1, 1)

/*!
   \brief Get Version event for Warning NotificationService object
   \details
   \dontinclude ProducerSample.c
   \skip switch (msg->msgId)
   \until }
 */
#define GET_WARNING_NOTIFICATION_VERSION_PROPERTY       AJ_ENCODE_PROPERTY_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_OBJECT_INDEX + AJNS_NOTIFICATION_MESSAGE_TYPE_WARNING, 1, 1)

/*!
   \brief Get Version event for Info NotificationService object
   \details
   \dontinclude ProducerSample.c
   \skip switch (msg->msgId)
   \until }
 */
#define GET_INFO_NOTIFICATION_VERSION_PROPERTY          AJ_ENCODE_PROPERTY_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_OBJECT_INDEX + AJNS_NOTIFICATION_MESSAGE_TYPE_INFO, 1, 1)

/* Producer Object bus registration */
#define NOTIFICATION_PRODUCER_OBJECT_INDEX              NOTIFICATION_OBJECT_INDEX + AJNS_NUM_MESSAGE_TYPES
#define NOTIFICATION_PRODUCER_GET_PROPERTY              AJ_ENCODE_PROPERTY_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_PRODUCER_OBJECT_INDEX, 0, AJ_PROP_GET)
#define NOTIFICATION_PRODUCER_SET_PROPERTY              AJ_ENCODE_PROPERTY_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_PRODUCER_OBJECT_INDEX, 0, AJ_PROP_SET)

#define NOTIFICATION_PRODUCER_DISMISS                   AJ_ENCODE_MESSAGE_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_PRODUCER_OBJECT_INDEX, 1, 0)
#define GET_NOTIFICATION_PRODUCER_VERSION_PROPERTY      AJ_ENCODE_PROPERTY_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_PRODUCER_OBJECT_INDEX, 1, 1)

/**
 * Static non consts.
 */
static uint32_t notificationId = 0;

/**
 * MessageTracking
 */
typedef struct _AJNS_MessageTracking {
    uint32_t notificationId;     /**< notification id */
    uint32_t serialNum;          /**< serial number */
} AJNS_MessageTracking;

static AJNS_MessageTracking lastSentNotifications[AJNS_NUM_MESSAGE_TYPES] = { { 0, 0 }, { 0, 0 }, { 0, 0 } };

/**
 * Marshal Notification
 */
static AJ_Status AJNS_Producer_MarshalNotificationMsg(AJ_BusAttachment* busAttachment, AJ_Message* msg, AJNS_Notification* notification, uint32_t ttl)
{
    AJ_Status status = AJ_OK;
    AJ_Arg attrbtArray;
    AJ_Arg customAttributeArray;
    AJ_Arg notTextArray;
    AJ_Arg richAudioArray;
    AJ_Arg dictArg;
    AJ_Arg customAttributeDictArg;
    AJ_Arg structArg;
    AJ_Arg audioStructArg;
    AJ_Arg richAudioAttrArray;
    int8_t indx;

    if (notification == NULL) {
        AJ_InfoPrintf(("Nothing to send\n"));
        return status;
    }

    status = AJ_MarshalSignal(busAttachment, msg, AJ_ENCODE_MESSAGE_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_OBJECT_INDEX + notification->messageType, 1, 0), NULL, 0, ALLJOYN_FLAG_SESSIONLESS, ttl);
    if (status != AJ_OK) {
        AJ_ErrPrintf(("Could not Marshal Signal\n"));
        return status;
    }

    ///////////////////       Proto     /////////////////////
    status = AJ_MarshalArgs(msg, "q", notification->version);
    if (status != AJ_OK) {
        goto ErrorExit;
    }

    ///////////////////    MessgeId    /////////////////////
    status = AJ_MarshalArgs(msg, "i", notification->notificationId);
    if (status != AJ_OK) {
        goto ErrorExit;
    }

    ///////////////////    MessageType   ////////////////////////////
    status = AJ_MarshalArgs(msg, "q", notification->messageType);
    if (status != AJ_OK) {
        goto ErrorExit;
    }

    ///////////////////    DeviceId   ////////////////////////////
    status = AJ_MarshalArgs(msg, "s", notification->deviceId);
    if (status != AJ_OK) {
        goto ErrorExit;
    }

    ///////////////////    DeviceName   ////////////////////////////
    status = AJ_MarshalArgs(msg, "s", notification->deviceName);
    if (status != AJ_OK) {
        goto ErrorExit;
    }

    ///////////////////    AppId   ////////////////////////////
    status = AJSVC_MarshalAppId(msg, notification->appId);
    if (status != AJ_OK) {
        goto ErrorExit;
    }

    ///////////////////    AppName   ////////////////////////////
    status = AJ_MarshalArgs(msg, "s", notification->appName);
    if (status != AJ_OK) {
        goto ErrorExit;
    }

    ///////////////////    Attributes   ////////////////////////////
    status = AJ_MarshalContainer(msg, &attrbtArray, AJ_ARG_ARRAY);
    if (status != AJ_OK) {
        goto ErrorExit;
    }

    if (notification->content->richIconUrl != 0) {
        status = AJ_MarshalContainer(msg, &dictArg, AJ_ARG_DICT_ENTRY);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalArgs(msg, "i", AJNS_RICH_CONTENT_ICON_URL_ATTRIBUTE_KEY);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalVariant(msg, "s");
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalArgs(msg, "s", notification->content->richIconUrl);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalCloseContainer(msg, &dictArg);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
    }
    if (notification->content->numAudioUrls > 0) {
        status = AJ_MarshalContainer(msg, &richAudioArray, AJ_ARG_DICT_ENTRY);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalArgs(msg, "i", AJNS_RICH_CONTENT_AUDIO_URL_ATTRIBUTE_KEY);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalVariant(msg, "a(ss)");
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalContainer(msg, &richAudioAttrArray, AJ_ARG_ARRAY);
        if (status != AJ_OK) {
            goto ErrorExit;
        }

        for (indx = 0; indx < notification->content->numAudioUrls; indx++) {
            if ((strlen(notification->content->richAudioUrls[indx].key) == 0) || (strlen(notification->content->richAudioUrls[indx].value) == 0)) {
                AJ_ErrPrintf(("Rich Audio Language/Url can not be empty\n"));
                AJ_MarshalCloseContainer(msg, &richAudioArray);
                status = AJ_ERR_DISALLOWED;
                goto ErrorExit;
            }
            status = AJ_MarshalContainer(msg, &audioStructArg, AJ_ARG_STRUCT);
            if (status != AJ_OK) {
                goto ErrorExit;
            }
            status = AJ_MarshalArgs(msg, "ss", notification->content->richAudioUrls[indx].key, notification->content->richAudioUrls[indx].value);
            if (status != AJ_OK) {
                goto ErrorExit;
            }
            status = AJ_MarshalCloseContainer(msg, &audioStructArg);
            if (status != AJ_OK) {
                goto ErrorExit;
            }
        }

        status = AJ_MarshalCloseContainer(msg, &richAudioAttrArray);
        if (status != AJ_OK) {
            goto ErrorExit;
        }

        status = AJ_MarshalCloseContainer(msg, &richAudioArray);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
    }

    if (notification->content->richIconObjectPath != 0) {
        status = AJ_MarshalContainer(msg, &dictArg, AJ_ARG_DICT_ENTRY);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalArgs(msg, "i", AJNS_RICH_CONTENT_ICON_OBJECT_PATH_ATTRIBUTE_KEY);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalVariant(msg, "s");
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalArgs(msg, "s", notification->content->richIconObjectPath);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalCloseContainer(msg, &dictArg);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
    }

    if (notification->content->richAudioObjectPath != 0) {
        status = AJ_MarshalContainer(msg, &dictArg, AJ_ARG_DICT_ENTRY);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalArgs(msg, "i", AJNS_RICH_CONTENT_AUDIO_OBJECT_PATH_ATTRIBUTE_KEY);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalVariant(msg, "s");
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalArgs(msg, "s", notification->content->richAudioObjectPath);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalCloseContainer(msg, &dictArg);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
    }

    if (notification->content->controlPanelServiceObjectPath != 0) {
        status = AJ_MarshalContainer(msg, &dictArg, AJ_ARG_DICT_ENTRY);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalArgs(msg, "i", AJNS_CONTROLPANELSERVICE_OBJECT_PATH_ATTRIBUTE_KEY);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalVariant(msg, "s");
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalArgs(msg, "s", notification->content->controlPanelServiceObjectPath);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalCloseContainer(msg, &dictArg);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
    }

    if (notification->version > 1) {
        status = AJ_MarshalContainer(msg, &dictArg, AJ_ARG_DICT_ENTRY);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalArgs(msg, "i", AJNS_ORIGINAL_SENDER_NAME_ATTRIBUTE_KEY);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalVariant(msg, "s");
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalArgs(msg, "s", notification->originalSenderName);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalCloseContainer(msg, &dictArg);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
    }

    status = AJ_MarshalCloseContainer(msg, &attrbtArray);
    if (status != AJ_OK) {
        goto ErrorExit;
    }

    ///////////////////    Custom Attributes   ///////////////////
    status = AJ_MarshalContainer(msg, &customAttributeArray, AJ_ARG_ARRAY);
    if (status != AJ_OK) {
        goto ErrorExit;
    }

    for (indx = 0; indx < notification->content->numCustomAttributes; indx++) {
        status = AJ_MarshalContainer(msg, &customAttributeDictArg, AJ_ARG_DICT_ENTRY);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalArgs(msg, "ss", notification->content->customAttributes[indx].key, notification->content->customAttributes[indx].value);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalCloseContainer(msg, &customAttributeDictArg);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
    }

    status = AJ_MarshalCloseContainer(msg, &customAttributeArray);
    if (status != AJ_OK) {
        goto ErrorExit;
    }

    ///////////////////   Notifications   ////////////////////////////
    status = AJ_MarshalContainer(msg, &notTextArray, AJ_ARG_ARRAY);
    if (status != AJ_OK) {
        goto ErrorExit;
    }

    for (indx = 0; indx < notification->content->numTexts; indx++) {
        if ((strlen(notification->content->texts[indx].key) == 0) || (strlen(notification->content->texts[indx].value) == 0)) {
            AJ_ErrPrintf(("Language/Text can not be empty\n"));
            AJ_MarshalCloseContainer(msg, &notTextArray);
            status = AJ_ERR_DISALLOWED;
            goto ErrorExit;
        }
        status = AJ_MarshalContainer(msg, &structArg, AJ_ARG_STRUCT);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalArgs(msg, "ss", notification->content->texts[indx].key, notification->content->texts[indx].value);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalCloseContainer(msg, &structArg);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
    }

    status = AJ_MarshalCloseContainer(msg, &notTextArray);
    if (status != AJ_OK) {
        goto ErrorExit;
    }

    return AJ_OK;

ErrorExit:

    AJ_ErrPrintf(("MarshalNotification failed: '%s'\n", AJ_StatusText(status)));
    return status;
}

/**
 * Send notify signal
 */
AJ_Status AJNS_Producer_SendNotifySignal(AJ_BusAttachment* busAttachment, AJNS_Notification* notification, uint32_t ttl, uint32_t* messageSerialNumber)
{
    AJ_Status status;
    AJ_Message msg;
    uint32_t serialNum;

    AJ_InfoPrintf(("In SendNotifySignal\n"));
    status = AJNS_Producer_MarshalNotificationMsg(busAttachment, &msg, notification, ttl);
    if (status != AJ_OK) {
        AJ_InfoPrintf(("Could not Marshal Message\n"));
        return status;
    }
    serialNum = msg.hdr->serialNum;
    status = AJ_DeliverMsg(&msg);
    if (status != AJ_OK) {
        AJ_ErrPrintf(("Could not Deliver Message\n"));
        return status;
    }
    AJ_InfoPrintf(("***************** Notification id %d delivered successfully with serial number %u *****************\n", notification->notificationId, serialNum));
    if (messageSerialNumber != NULL) {
        *messageSerialNumber = serialNum;
    }

    AJ_CloseMsg(&msg);

    return status;
}

/**
 * Send Notification - see notes in h file
 */
AJ_Status AJNS_Producer_SendNotification(AJ_BusAttachment* busAttachment, AJNS_NotificationContent* content, uint16_t messageType, uint32_t ttl, uint32_t* messageSerialNumber)
{
    AJ_Status status;
    AJNS_Notification notification;
    uint32_t serialNumber;

    AJ_InfoPrintf(("In SendNotification\n"));

    notification.version = AJNS_NotificationVersion;
    if (messageType >= AJNS_NUM_MESSAGE_TYPES) {
        AJ_ErrPrintf(("Could not Send Notification - MessageType is not valid\n"));
        return AJ_ERR_DISALLOWED;
    }
    notification.messageType = messageType;

    if ((ttl < AJNS_NOTIFICATION_TTL_MIN) || (ttl > AJNS_NOTIFICATION_TTL_MAX)) {      //ttl is mandatory and must be in range
        AJ_ErrPrintf(("TTL '%u' is not a valid TTL value\n", ttl));
        return AJ_ERR_DISALLOWED;
    }

    notification.deviceId = AJSVC_PropertyStore_GetValue(AJSVC_PROPERTY_STORE_DEVICE_ID);
    notification.deviceName = AJSVC_PropertyStore_GetValueForLang(AJSVC_PROPERTY_STORE_DEVICE_NAME, AJSVC_PropertyStore_GetLanguageIndex(""));
    notification.appId = AJSVC_PropertyStore_GetValue(AJSVC_PROPERTY_STORE_APP_ID);
    notification.appName = AJSVC_PropertyStore_GetValue(AJSVC_PROPERTY_STORE_APP_NAME);

    if ((notification.deviceId == 0) || (notification.deviceName == 0) ||
        (notification.appId == 0) || (notification.appName == 0)) {
        AJ_ErrPrintf(("DeviceId/DeviceName/AppId/AppName can not be NULL\n"));
        return AJ_ERR_DISALLOWED;
    }

    if ((strlen(notification.deviceId) == 0) || (strlen(notification.deviceName) == 0) ||
        (strlen(notification.appId) == 0) || (strlen(notification.appName) == 0)) {
        AJ_ErrPrintf(("DeviceId/DeviceName/AppId/AppName can not be empty\n"));
        return AJ_ERR_DISALLOWED;
    }

    if (notification.version > 1) {
        notification.originalSenderName = AJ_GetUniqueName(busAttachment);

        if (notification.originalSenderName == 0) {
            AJ_ErrPrintf(("OriginalSender can not be NULL\n"));
            return AJ_ERR_DISALLOWED;
        }

        if (strlen(notification.originalSenderName) == 0) {
            AJ_ErrPrintf(("OriginalSender can not be empty\n"));
            return AJ_ERR_DISALLOWED;
        }
    } else {
        notification.originalSenderName = NULL;
    }

    if (!notificationId) {
        AJ_InfoPrintf(("Generating random number for notification id\n"));
        AJ_RandBytes((uint8_t*)&notificationId, 4);
    }

    notification.notificationId = notificationId;
    notification.content = content;

    status = AJNS_Producer_SendNotifySignal(busAttachment, &notification, ttl, &serialNumber);

    if (status == AJ_OK) {
        lastSentNotifications[messageType].notificationId = notificationId++;
        lastSentNotifications[messageType].serialNum = serialNumber;
        if (messageSerialNumber != NULL) {
            *messageSerialNumber = serialNumber;
        }
    }

    return status;
}

AJ_Status AJNS_Producer_DeleteLastNotification(AJ_BusAttachment* busAttachment, uint16_t messageType)
{
    AJ_Status status;
    uint32_t lastSentSerialNumber;

    AJ_InfoPrintf(("In DeleteLastNotification\n"));
    if (messageType >= AJNS_NUM_MESSAGE_TYPES) {
        AJ_ErrPrintf(("Could not delete Notification - MessageType is not valid\n"));
        return AJ_ERR_DISALLOWED;
    }

    lastSentSerialNumber = lastSentNotifications[messageType].serialNum;
    if (lastSentSerialNumber == 0) {
        AJ_ErrPrintf(("Could not Delete Message - no message to delete\n"));
        return AJ_OK;
    }

    status = AJ_BusCancelSessionless(busAttachment, lastSentSerialNumber);

    if (status != AJ_OK) {
        AJ_ErrPrintf(("Could not Delete Message\n"));
        return status;
    }

    AJ_InfoPrintf(("***************** Message with Notification id %d and serialNum %u deleted successfully *****************\n", lastSentNotifications[messageType].notificationId, lastSentNotifications[messageType].serialNum));

    lastSentNotifications[messageType].notificationId = 0;
    lastSentNotifications[messageType].serialNum = 0;

    return status;
}

static AJ_Status AJNS_Producer_CancelNotificationById(AJ_BusAttachment* busAttachment, int32_t notificationId)
{
    AJ_Status status;
    uint16_t messageType = 0;

    AJ_InfoPrintf(("In CancelNotificationById\n"));

    if (notificationId == 0) {
        AJ_ErrPrintf(("Could not cancel Message - no message to cancel\n"));
        return AJ_OK;
    }
    for (; messageType < AJNS_NUM_MESSAGE_TYPES; messageType++) {
        if (lastSentNotifications[messageType].notificationId == notificationId) {
            break;
        }
    }
    if (messageType >= AJNS_NUM_MESSAGE_TYPES) {
        AJ_ErrPrintf(("Could not find matching Message serial number - no message to cancel\n"));
        return AJ_OK;
    }

    status = AJ_BusCancelSessionless(busAttachment, lastSentNotifications[messageType].serialNum);

    if (status != AJ_OK) {
        AJ_ErrPrintf(("Failed to send cancelation\n"));
        return status;
    }

    AJ_InfoPrintf(("***************** Message with Notification id %d and serialNum %u deleted successfully *****************\n", lastSentNotifications[messageType].notificationId, lastSentNotifications[messageType].serialNum));

    lastSentNotifications[messageType].notificationId = 0;
    lastSentNotifications[messageType].serialNum = 0;

    return status;
}

AJ_Status AJNS_Producer_CancelNotification(AJ_BusAttachment* busAttachment, uint32_t serialNum)
{
    AJ_Status status;
    uint16_t messageType = 0;

    AJ_InfoPrintf(("In CancelNotificationBySerialNum\n"));

    if (serialNum == 0) {
        AJ_ErrPrintf(("Could not cancel Message - no message to cancel\n"));
        return AJ_OK;
    }
    for (; messageType < AJNS_NUM_MESSAGE_TYPES; messageType++) {
        if (lastSentNotifications[messageType].serialNum == serialNum) {
            break;
        }
    }
    if (messageType >= AJNS_NUM_MESSAGE_TYPES) {
        AJ_ErrPrintf(("Could not find matching Message serial number - no message to cancel\n"));
        return AJ_OK;
    }

    status = AJ_BusCancelSessionless(busAttachment, serialNum);

    if (status != AJ_OK) {
        AJ_ErrPrintf(("Failed to send cancelation\n"));
        return status;
    }

    AJ_InfoPrintf(("***************** Message with Notification id %d and serialNum %u deleted successfully *****************\n", lastSentNotifications[messageType].notificationId, lastSentNotifications[messageType].serialNum));

    lastSentNotifications[messageType].notificationId = 0;
    lastSentNotifications[messageType].serialNum = 0;

    return status;
}

AJ_Status AJNS_Producer_DismissRequestHandler(AJ_BusAttachment* busAttachment, AJ_Message* msg)
{
    AJ_Status status;
    int32_t notificationId;
    const char* appId;
    AJ_Message reply;

    AJ_InfoPrintf(("In DismissMsg\n"));
    status = AJ_UnmarshalArgs(msg, "i", &notificationId);
    if (status != AJ_OK) {
        AJ_ErrPrintf(("Could not unmarshal message\n"));
        return status;
    }

    status = AJ_MarshalReplyMsg(msg, &reply);
    if (status != AJ_OK) {
        return status;
    }

    status = AJ_DeliverMsg(&reply);
    if (status != AJ_OK) {
        return status;
    }

    appId = AJSVC_PropertyStore_GetValue(AJSVC_PROPERTY_STORE_APP_ID);
    status = AJNS_SendDismissSignal(busAttachment, notificationId, appId);
    if (status != AJ_OK) {
        return status;
    }

    status = AJNS_Producer_CancelNotificationById(busAttachment, notificationId);
    if (status != AJ_OK) {
        return status;
    }

    AJ_InfoPrintf(("***************** Message with Notification id %d dismissed successfully *****************\n", notificationId));

    return status;
}

AJ_Status AJNS_Producer_PropGetHandler(AJ_Message* replyMsg, uint32_t propId, void* context)
{
    AJ_Status status = AJ_ERR_UNEXPECTED;

    switch (propId) {
    case GET_EMERGENCY_NOTIFICATION_VERSION_PROPERTY:
    case GET_WARNING_NOTIFICATION_VERSION_PROPERTY:
    case GET_INFO_NOTIFICATION_VERSION_PROPERTY:
        status = AJ_MarshalArgs(replyMsg, "q", AJNS_NotificationVersion);
        break;

    case GET_NOTIFICATION_PRODUCER_VERSION_PROPERTY:
        status = AJ_MarshalArgs(replyMsg, "q", AJNS_NotificationProducerVersion);
        break;

    case GET_NOTIFICATION_DISMISSER_VERSION_PROPERTY:
        status = AJ_MarshalArgs(replyMsg, "q", AJNS_NotificationDismisserVersion);
        break;
    }
    return status;
}

AJ_Status AJNS_Producer_PropSetHandler(AJ_Message* replyMsg, uint32_t propId, void* context)
{
    return AJ_ERR_DISALLOWED;
}

AJ_Status AJNS_Producer_ConnectedHandler(AJ_BusAttachment* busAttachment)
{
    AJ_Status status;
    AJ_SessionOpts sessionOpts = {
        AJ_SESSION_TRAFFIC_MESSAGES,
        AJ_SESSION_PROXIMITY_ANY,
        AJ_TRANSPORT_ANY,
        FALSE
    };
    uint8_t serviceStarted;
    AJ_Message msg;

    status = AJ_BusBindSessionPort(busAttachment, AJNS_NotificationProducerPort, &sessionOpts, 0);
    if (status != AJ_OK) {
        AJ_ErrPrintf(("Failed to send bind session port message\n"));
    }

    serviceStarted = FALSE;
    while (!serviceStarted && (status == AJ_OK)) {

        status = AJ_UnmarshalMsg(busAttachment, &msg, AJ_UNMARSHAL_TIMEOUT);
        if (status == AJ_ERR_NO_MATCH) {
            status = AJ_OK;
            continue;
        } else if (status != AJ_OK) {
            break;
        }

        switch (msg.msgId) {
        case AJ_REPLY_ID(AJ_METHOD_BIND_SESSION_PORT):
            if (msg.hdr->msgType == AJ_MSG_ERROR) {
                status = AJ_ERR_FAILURE;
            } else {
                serviceStarted = TRUE;
            }
            break;

        default:
            /*
             * Pass to the built-in bus message handlers
             */
            status = AJ_BusHandleBusMessage(&msg);
            break;
        }
        AJ_CloseMsg(&msg);
    }

    if (status != AJ_OK) {
        AJ_ErrPrintf(("AllJoyn disconnect bus status=%d\n", status));
        status = AJ_ERR_READ;
    }
    return status;
}

uint8_t AJNS_Producer_CheckSessionAccepted(uint16_t port, uint32_t sessionId, const char* joiner)
{
    if (port != AJNS_NotificationProducerPort) {
        return FALSE;
    }
    AJ_InfoPrintf(("Producer: Accepted session on port %u from %s\n", port, joiner));
    return TRUE;
}

AJSVC_ServiceStatus AJNS_Producer_MessageProcessor(AJ_BusAttachment* busAttachment, AJ_Message* msg, AJ_Status* msgStatus)
{
    AJSVC_ServiceStatus serviceStatus = AJSVC_SERVICE_STATUS_NOT_HANDLED;

    switch (msg->msgId) {
    case EMERGENCY_NOTIFICATION_GET_PROPERTY:
    case WARNING_NOTIFICATION_GET_PROPERTY:
    case INFO_NOTIFICATION_GET_PROPERTY:
    case NOTIFICATION_PRODUCER_GET_PROPERTY:
        *msgStatus = AJ_BusPropGet(msg, AJNS_Producer_PropGetHandler, NULL);
        serviceStatus = AJSVC_SERVICE_STATUS_HANDLED;
        break;

    case EMERGENCY_NOTIFICATION_SET_PROPERTY:
    case WARNING_NOTIFICATION_SET_PROPERTY:
    case INFO_NOTIFICATION_SET_PROPERTY:
    case NOTIFICATION_PRODUCER_SET_PROPERTY:
        *msgStatus = AJ_BusPropSet(msg, AJNS_Producer_PropSetHandler, NULL);
        serviceStatus = AJSVC_SERVICE_STATUS_HANDLED;
        break;

    case NOTIFICATION_PRODUCER_DISMISS:
        *msgStatus = AJNS_Producer_DismissRequestHandler(busAttachment, msg);
        serviceStatus = AJSVC_SERVICE_STATUS_HANDLED;
        break;

    default:
        break;
    }

    return serviceStatus;
}

AJ_Status AJNS_Producer_DisconnectHandler(AJ_BusAttachment* busAttachment)
{
    AJ_Status status = AJ_OK;
//    status = AJ_BusUnbindSession(busAttachment, AJNS_NotificationProducerPort);
//    if (status != AJ_OK) {
//        AJ_ErrPrintf(("Failed to send unbind session port=%d\n", AJNS_NotificationProducerPort));
//    }
    return status;
}
