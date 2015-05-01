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

#ifndef _SERVICES_COMMON_H_
#define _SERVICES_COMMON_H_

#include <aj_config.h>
#include <alljoyn.h>

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
extern uint8_t dbgAJSVC;
#endif

/**
 * Function prototype for return callback when a method call is completed.
 */
typedef void (*AJSVC_MethodCallCompleted)(AJ_Status status, void* context);

/**
 * Service Status is an enum that signals whether a call was handled
 * or not handled within an AJSVC_MessageProcessor function
 */
typedef enum _AJSVC_ServiceStatus {
    AJSVC_SERVICE_STATUS_HANDLED,       //!< SERVICE_STATUS_HANDLED
    AJSVC_SERVICE_STATUS_NOT_HANDLED,   //!< SERVICE_STATUS_NOT_HANDLED
} AJSVC_ServiceStatus;

/**
 * Function used to process request messages.
 * @param busAttachment
 * @param msg
 * @param msgStatus
 * @return serviceStatus
 */
typedef AJSVC_ServiceStatus (*AJSVC_MessageProcessor)(AJ_BusAttachment* busAttachment, AJ_Message* msg, AJ_Status* msgStatus);

/**
 * UpdateNotAllowed Error Message for services
 */
#define AJSVC_ERROR_UPDATE_NOT_ALLOWED     AJ_ErrUpdateNotAllowed

/**
 * InvalidValue Error Message for services
 */
#define AJSVC_ERROR_INVALID_VALUE          AJ_ErrInvalidValue

/**
 * FeatureNotAvailable Error Message for services
 */
#define AJSVC_ERROR_FEATURE_NOT_AVAILABLE  AJ_ErrFeatureNotAvailable

/**
 * MazSizeExceeded Error Message for services
 */
#define AJSVC_ERROR_MAX_SIZE_EXCEEDED      AJ_ErrMaxSizeExceeded

/**
 * LanguageNotSupported Error Message for services
 */
#define AJSVC_ERROR_LANGUAGE_NOT_SUPPORTED AJ_ErrLanguageNotSuppored

/**
 * returns the language index for the given language name possibly creating an error reply message if erred
 * @param msg
 * @param reply
 * @param language
 * @param langIndex
 * @return success
 */
uint8_t AJSVC_IsLanguageSupported(AJ_Message* msg, AJ_Message* reply, const char* language, int8_t* langIndex);

/**
 * Signature of the AppId field
 */
#define APP_ID_SIGNATURE "ay"

/**
 * Length of UUID that is used for the AppId field
 */
#define UUID_LENGTH 16

/**
 * Marshals the appId Hex string as a variant into the provided message.
 * @param msg       the message to marshal the appId into
 * @param appId     the application id to marshal
 * @return status
 */
AJ_Status AJSVC_MarshalAppIdAsVariant(AJ_Message* msg, const char* appId);

/**
 * Marshals the appId Hex string into the provided message.
 * @param msg       the message to marshal the appId into
 * @param appId     the application id to marshal
 * @return status
 */
AJ_Status AJSVC_MarshalAppId(AJ_Message* msg, const char* appId);

/**
 * Unmarshals the appId from a variant in the provided message.
 * @param msg       the message to unmarshal the appId from
 * @param buf       the buffer where the application id is unmarshalled into
 * @param bufLen    the size of the provided buffer. Should be UUID_LENGTH * 2 + 1.
 * @return status
 */
AJ_Status AJSVC_UnmarshalAppIdFromVariant(AJ_Message* msg, char* buf, size_t bufLen);

/**
 * Unmarshals the appId from the provided message.
 * @param msg       the message to unmarshal the appId from
 * @param buf       the buffer where the application id is unmarshalled into
 * @param bufLen    the size of the provided buffer. Should be UUID_LENGTH * 2 + 1.
 * @return status
 */
AJ_Status AJSVC_UnmarshalAppId(AJ_Message* msg, char* buf, size_t bufLen);

// The following is the static registration of all services' bus objects

/*
 * For each service:
 * 1) Define pre objects - the amount of objects registered before the service
 * 2) If service is included:
 *    i)   include service header file(s)
 *    If service is NOT included:
 *    i)   define the default number of appobjects and number of objects
 *    ii)  define the default announce objects
 */
/*
 * ObjectsList definitions for ALL the services
 */
/*
 * ObjectsList index for Config Service objects
 */
#define AJCFG_OBJECT_LIST_INDEX            3

/*
 * ObjectsList index for Onboarding Service objects
 */
#define AJOBS_OBJECT_LIST_INDEX            4

/*
 * ObjectsList index for Notification Service objects
 */
#define AJNS_OBJECT_LIST_INDEX             5

/*
 * ObjectsList index for ControlPanel Service objects
 */
#define AJCPS_OBJECT_LIST_INDEX            6

/*
 * ObjectsList index for Time Service objects
 */
#define AJTS_OBJECT_LIST_INDEX             7

/*
 * ObjectsList index for Application objects
 */
#define AJAPP_OBJECTS_LIST_INDEX           8

#if AJAPP_OBJECTS_LIST_INDEX >= AJ_MAX_OBJECT_LISTS
#error AJ_MAX_OBJECT_LISTS in aj_config.h too small
#endif

/**
 * The NVRAM starting id for the PropertyStore
 */
#define AJ_PROPERTIES_NV_ID_BEGIN          (AJ_NVRAM_ID_CREDS_MAX + 1)
/**
 * The NVRAM maximum id for the PropertyStore
 */
#define AJ_PROPERTIES_NV_ID_MAX            (AJ_NVRAM_ID_CREDS_MAX + 1000)

/**
 * The NVRAM starting id for the Onboarding Service
 */
#define AJ_OBS_NV_ID_BEGIN                 (AJ_PROPERTIES_NV_ID_MAX + 1)

#endif /* _SERVICES_COMMON_H_ */
