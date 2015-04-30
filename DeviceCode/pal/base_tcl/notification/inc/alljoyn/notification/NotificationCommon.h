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

#ifndef _NOTIFICATIONCOMMON_H_
#define _NOTIFICATIONCOMMON_H_

#include <alljoyn.h>

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
extern uint8_t dbgAJNS;
#endif

/** @defgroup NotificationCommon Notification Common
 * details Functions and variables that assist in writing Notification Producers
 *  @{
 */

/**
 * Definitions of notification attribute keys.
 */
#define AJNS_RICH_CONTENT_ICON_URL_ATTRIBUTE_KEY             0    /**< Content icon url */
#define AJNS_RICH_CONTENT_AUDIO_URL_ATTRIBUTE_KEY            1    /**< Content audio url */
#define AJNS_RICH_CONTENT_ICON_OBJECT_PATH_ATTRIBUTE_KEY     2    /**< Content icon object path */
#define AJNS_RICH_CONTENT_AUDIO_OBJECT_PATH_ATTRIBUTE_KEY    3    /**< Content audio object path */
#define AJNS_CONTROLPANELSERVICE_OBJECT_PATH_ATTRIBUTE_KEY   4    /**< Control panel service object path */
#define AJNS_ORIGINAL_SENDER_NAME_ATTRIBUTE_KEY              5    /**< Original sender name */

/**
 * Number of message types.
 */
#define AJNS_NUM_MESSAGE_TYPES 3                                  /**< Number of message types */

/**
 * Generic structure for key value pairs.
 */
typedef struct _AJNS_DictionaryEntry {
    const char* key;                           /**< key of dictionary */
    const char* value;                         /**< value of dictionary */
} AJNS_DictionaryEntry;

/*!
   \brief struct that holds the notification content
 */
typedef struct _AJNS_NotificationContent {
    int8_t numCustomAttributes;                 /**< numCustomAttributes number of custom Attributs */
    AJNS_DictionaryEntry* customAttributes;     /**< customAttributes Custom attributs */
    int8_t numTexts;                            /**< numTexts number of versions of the notification text */
    AJNS_DictionaryEntry* texts;                /**< texts The text of the notification, one entry per language */
    int8_t numAudioUrls;                        /**< numAudioUrls The number of audio URLs sent */
    AJNS_DictionaryEntry* richAudioUrls;        /**< richAudioUrls The audio URLs, one per language */
    const char* richIconUrl;                    /**< richIconUrl A URL for an icon to be displayed along with the notification */
    const char* richIconObjectPath;             /**< richIconObjectPath The AllJoyn object path of an accompanying icons object */
    const char* richAudioObjectPath;            /**< richAudioObjectPath The AllJoyn object path of an accompanying audio object */
    const char* controlPanelServiceObjectPath;  /**< controlPanelServiceObjectPath The AllJoyn object path of an accompanying Control Panel Service object */
    const char* originalSenderName;             /**< originalSenderName The AllJoyn bus unique name of the originating producer application */
} AJNS_NotificationContent;

/*!
   \brief struct that holds the notification (header fields + content)
 */
typedef struct _AJNS_Notification {
    uint16_t version;                           /**< version of notification */
    uint16_t messageType;                       /**< messageType One of \ref AJNS_NOTIFICATION_MESSAGE_TYPE_INFO, \ref AJNS_NOTIFICATION_MESSAGE_TYPE_WARNING, or \ref AJNS_NOTIFICATION_MESSAGE_TYPE_EMERGENCY */
    int32_t notificationId;                     /**< notification message id */
    const char* originalSenderName;             /**< originalSenderName The AllJoyn bus unique name of the originating producer application */
    const char* deviceId;                       /**< device id of originating producer application */
    const char* deviceName;                     /**< device name of originating producer application */
    const char* appId;                          /**< application id of originating producer application */
    const char* appName;                        /**< application name of originating producer application */
    AJNS_NotificationContent* content;          /**< content of notification */
} AJNS_Notification;

/**
 * Notification interface signal
 */
extern const char AJNS_NotificationSignalName[];
/**
 * Notification interface property version
 */
extern const char AJNS_NotificationPropertyVersion[];
/**
 * Notification interface version property value
 */
extern const uint16_t AJNS_NotificationVersion;

/**
 * Notification interface name followed by the method signatures.
 *
 * See also ".\inc\aj_introspect.h"
 */
extern const char* AJNS_NotificationInterface[];

/**
 * A NULL terminated collection of all interfaces.
 */
extern const AJ_InterfaceDescription AJNS_NotificationInterfaces[];

/*!
   \brief Minimal time in seconds for the notification signal to live
 */
extern const uint16_t AJNS_NOTIFICATION_TTL_MIN;

/*!
   \brief Maximal time in seconds for the notification signal to live
 */
extern const uint16_t AJNS_NOTIFICATION_TTL_MAX;

/**
 * Notification Common objects range
 */
/**
 * Starting index
 */
#define NOTIFICATION_COMMON_OBJECTS_INDEX           0
/**
 * Count
 */
#define NOTIFICATION_COMMON_OBJECTS_COUNT           1

/**
 * Register Notfication Common objects
 */
void AJNS_Common_RegisterObjects();

/**
 * Notification Dismisser object for the Dismiss signal emitter
 */
/**
 * Notification Dismisser interface name followed by the method signatures.
 *
 * See also ".\inc\aj_introspect.h"
 */
/**
 * Notification Dismisser interface version property value
 */
extern const uint16_t AJNS_NotificationDismisserVersion;
/**
 * Notification Dismisser interface signal emitter
 */
AJ_Status AJNS_SendDismissSignal(AJ_BusAttachment* busAttachment, int32_t msgId, const char* appId);

/**
 * Notification dismisser object index
 */
#define NOTIFICATION_DISMISSER_OBJECT_INDEX         NOTIFICATION_COMMON_OBJECTS_INDEX
/**
 * Notification dismisser get property
 */
#define NOTIFICATION_DISMISSER_GET_PROPERTY         AJ_ENCODE_PROPERTY_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_DISMISSER_OBJECT_INDEX, 0, AJ_PROP_GET)
/**
 * Notification dismisser set property
 */
#define NOTIFICATION_DISMISSER_SET_PROPERTY         AJ_ENCODE_PROPERTY_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_DISMISSER_OBJECT_INDEX, 0, AJ_PROP_SET)
/**
 * Notification dismisser emitter
 */
#define NOTIFICATION_DISMISSER_DISMISS_EMITTER      AJ_ENCODE_MESSAGE_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_DISMISSER_OBJECT_INDEX, 1, 0)
/**
 * Notification dismisser version property
 */
#define GET_NOTIFICATION_DISMISSER_VERSION_PROPERTY AJ_ENCODE_PROPERTY_ID(AJNS_OBJECT_LIST_INDEX, NOTIFICATION_DISMISSER_OBJECT_INDEX, 1, 1)

/**
 * Notification Producer interface name followed by the method signatures.
 *
 * See also ".\inc\aj_introspect.h"
 */
/**
 * The Notification service Producer port
 */
extern const uint16_t AJNS_NotificationProducerPort;

/**
 * Notification Consumer objects range
 */
/**
 * Starting index
 */
#define NOTIFICATION_CONSUMER_OBJECTS_INDEX         NOTIFICATION_COMMON_OBJECTS_INDEX + NOTIFICATION_COMMON_OBJECTS_COUNT
/**
 * Count
 */
#define NOTIFICATION_CONSUMER_OBJECTS_COUNT         AJNS_NUM_MESSAGE_TYPES

/**
 * Notification Producer objects range
 */
/**
 * Starting index
 */
#define NOTIFICATION_PRODUCER_OBJECTS_INDEX         NOTIFICATION_CONSUMER_OBJECTS_INDEX + NOTIFICATION_CONSUMER_OBJECTS_COUNT
/**
 * Count
 */
#define NOTIFICATION_PRODUCER_OBJECTS_COUNT         AJNS_NUM_MESSAGE_TYPES + 1

/**
 * Notification Service object list
 */
extern AJ_Object AJNS_ObjectList[];
/** @} */ // End of group 'NotificationCommon'
#endif /* _NOTIFICATIONCOMMON_H_ */
