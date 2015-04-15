#ifndef _AJ_ABOUT_H
#define _AJ_ABOUT_H

/**
 * @file aj_about.h
 * @defgroup aj_about Bus Attachment
 * @{
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

#include "aj_target.h"
#include "aj_status.h"
#include "aj_util.h"
#include "aj_config.h"

#ifdef __cplusplus
extern "C" {
#endif

/**
 * Prototype for a function provided by the property store for getting ANNOUNCE and ABOUT properties
 *
 * @param reply     The message to marshal the property values into. The getter can also figure out
 *                  from the msgId in the reply message if the reply is for ANNOUNCE or ABOUT.
 *
 * @param language  The language to use to return the string properties. If this is NULL the default
 *                  language will be used.
 *
 * @return   Return AJ_OK if the properties were succesfully marshaled into the reply.
 *
 */
typedef AJ_Status (*AJ_AboutPropGetter)(AJ_Message* reply, const char* language);

/**
 * Called by the property store to register the about property getter. Functionality will be limited
 * if there is not property store.
 *
 * @param propGetter  The property getter function being registered.
 */
void AJ_AboutRegisterPropStoreGetter(AJ_AboutPropGetter propGetter);

/**
 * Initialize About and send the initial announcement.
 *
 * @param bus        The bus attachment
 * @param boundPort  Session port the application has bound
 */
AJ_Status AJ_AboutInit(AJ_BusAttachment* bus, uint16_t boundPort);

/**
 * Emit an announcement if one has been scheduled.
 *
 * @param bus   The bus attachment context.
 */
AJ_Status AJ_AboutAnnounce(AJ_BusAttachment* bus);

/**
 * Set a device icon to be returned by About
 *
 * @param icon  Pointer to the icon data blob. This pointer must remain live until the next time this
 *              function is called. Can be NULL if there is not icon data.
 * @param size  The size of the icon data blob.
 * @param mime  The mime type for the icon
 * @param url   Optional URL for an icon
 */
void AJ_AboutSetIcon(const uint8_t* icon, uint16_t size, const char* mimeType, const char* url);

/**
 * Handle a GET_PROP method call
 *
 * @param msg    The GET_PROP message
 *
 * @return  Return AJ_Status
 *          - AJ_OK if successfully handled
 *          - AJ_ERR_WRITE if there was a write failure
 */
AJ_Status AJ_AboutHandleGetProp(AJ_Message* msg);

/**
 * Handle a GET_ABOUT_DATA method call
 *
 * @param msg    The GET_ABOUT_DATA message
 * @param reply  The GET_ABOUT_DATA reply message
 *
 * @return  Return AJ_Status
 *          - AJ_OK if successfully handled
 *          - AJ_ERR_WRITE if there was a write failure
 */
AJ_Status AJ_AboutHandleGetAboutData(AJ_Message* msg, AJ_Message* reply);

/**
 * Handle a GET_ABOUT_OBJECT_DESCRIPTION method call
 *
 * @param msg    The GET_ABOUT_OBJECT_DESCRIPTION message
 * @param reply  The GET_ABOUT_OBJECT_DESCRIPTION reply message
 *
 * @return  Return AJ_Status
 *          - AJ_OK if successfully handled
 *          - AJ_ERR_WRITE if there was a write failure
 */
AJ_Status AJ_AboutHandleGetObjectDescription(AJ_Message* msg, AJ_Message* reply);

/**
 * Handle a GET_PROP method call
 *
 * @param msg    The GET_PROP message
 *
 * @return  Return AJ_Status
 *          - AJ_OK if successfully handled
 *          - AJ_ERR_WRITE if there was a write failure
 */
AJ_Status AJ_AboutIconHandleGetProp(AJ_Message* msg);

/**
 * Handle a GET_ABOUT_ICON_GET_URL method call
 *
 * @param msg    The GET_ABOUT_ICON_GET_URL message
 * @param reply  The GET_ABOUT_ICON_GET_URL reply message
 *
 * @return  Return AJ_Status
 *          - AJ_OK if successfully handled
 *          - AJ_ERR_WRITE if there was a write failure
 */
AJ_Status AJ_AboutIconHandleGetURL(AJ_Message* msg, AJ_Message* reply);

/**
 * Handle a GET_ABOUT_ICON_GET_CONTENT method call
 *
 * @param msg    The GET_ABOUT_ICON_GET_CONTENT message
 * @param reply  The GET_ABOUT_ICON_GET_CONTENT reply message
 *
 * @return  Return AJ_Status
 *          - AJ_OK if successfully handled
 *          - AJ_ERR_WRITE if there was a write failure
 */
AJ_Status AJ_AboutIconHandleGetContent(AJ_Message* msg, AJ_Message* reply);

/**
 * Function called by the application and other services when there are changes that warrant sending
 * of a new announcement. The announce condition is cleared after all AJ_AboutAnnounce() is called.
 */
void AJ_AboutSetShouldAnnounce();

/**
 * Sets the announce flag on a list of objects
 *
 * @param objList  The object list to set.
 */
void AJ_AboutSetAnnounceObjects(AJ_Object* objList);

#ifdef ANNOUNCE_BASED_DISCOVERY
/**
 * Type for received object description
 *
 * This structure is experimental, and as such has not been fully tested.
 * Please help make it more solid by contributing fixes if you find issues.
 */
typedef struct _AJ_AboutObjectDescription {
    const char* path;                                 /**< the object path in an Announcement's ObjectDescription */
    const char* interfaces[AJ_MAX_NUM_OF_INTERFACES]; /**< array of interface names in an Announcement's ObjectDescription */
    uint8_t interfacesCount;                          /**< number of interface names in an Announcement's ObjectDescription */
} AJ_AboutObjectDescription;

/**
 * Unmarshal an Announcement message ObjectDescription section.
 *
 * @param announcement        The received Announcement message.
 * @param objDescs[out]       An array of AJ_AboutObjectDescriptions that points at the Announcement message.
 * @param objDescsCount[out]  The number of AJ_AboutObjectDescriptions in the objDescs array.
 *
 * @return  Return AJ_Status
 *
 * This function is experimental, and as such has not been fully tested.
 * Please help make it more solid by contributing fixes if you find issues.
 */
AJ_Status AJ_AboutUnmarshalObjectDescriptions(AJ_Message* announcement, AJ_AboutObjectDescription* objDescs, uint16_t* objDescsCount);

/**
 * About found peer function prototype for indicating an Annoucement from a peer with matching interfaces was received.
 *
 * @param  version      The org.alljoyn.About interface version
 * @param  port         The application session port that is used by About
 * @param  peerName     The peer bus unique name
 * @param  objPath      The object path of the queried interface or "/" otherwise
 *
 * @return continueProcessing TRUE to continue processing the Announcement properties.
 *
 * Important: functions which implement this prototype MUST copy the input strings
 *            (i.e. peerName and objPath) if they need to use it later!
 *
 * This function is experimental, and as such has not been fully tested.
 * Please help make it more solid by contributing fixes if you find issues.
 */
typedef uint8_t (*AJ_AboutPeerDescriptionMatched)(uint16_t version, uint16_t port, const char* peerName, const char* objPath);

/**
 * About peer's object description prototype for retrieving an Annoucement object description.
 *
 * @param  peerName                The peer bus unique name
 * @param  aboutObjectDescription  The object description
 *
 * Important: functions which implement this prototype MUST copy the input strings
 *            (i.e. peerName and objPath) if they need to use it later!
 *
 * This function is experimental, and as such has not been fully tested.
 * Please help make it more solid by contributing fixes if you find issues.
 */
typedef void (*AJ_AboutHandleObjectDescription)(const char* peerName, const AJ_AboutObjectDescription* aboutObjectDescription);

/**
 * About peer's mandatory properties function prototype for retrieving an Annoucement mandatory properties.
 *
 * @param  peerName          The peer bus unique name
 * @param  appId             The string value of the peer's AppId
 * @param  appName           The string value of the peer's AppName
 * @param  deviceId          The string value of the peer's DeviceId
 * @param  deviceName        The string value of the peer's DeviceName
 * @param  manufacturer      The string value of the peer's Manufacturer
 * @param  modelNumber       The string value of the peer's ModelNumber
 * @param  defaultLanguage   The string value of the peer's DefaultLanguage
 *
 * Important: functions which implement this prototype MUST copy the input strings
 *            (e.g. appId, deviceName etc.) if they need to use it later!
 *
 * This function is experimental, and as such has not been fully tested.
 * Please help make it more solid by contributing fixes if you find issues.
 */
typedef void (*AJ_AboutHandleMandatoryProps)(const char* peerName,
                                             const char* appId,
                                             const char* appName,
                                             const char* deviceId,
                                             const char* deviceName,
                                             const char* manufacturer,
                                             const char* modelNumber,
                                             const char* defaultLanguage);

/**
 * About peer's optional property function prototype for retrieving an Annoucement optional property.
 *
 * @param  peerName  The peer bus unique name
 * @param  key       The optional property key name
 * @param  sig       The optional property value signature
 * @param  value     The optional property value variant AJ_Arg of a basic type!
 *
 * Important: functions which implement this prototype MUST copy the input key
 *            and unmarshal as variant the value if they need to use it later!
 *
 * This function is experimental, and as such has not been fully tested.
 * Please help make it more solid by contributing fixes if you find issues.
 */
typedef void (*AJ_AboutHandleOptionalProperty)(const char* peerName, const char* key, const char* sig, const AJ_Arg* value);

/**
 * About peer isRelevant function prototype for indicating the peer is relevant for engagement.
 *
 * @param  peerName     The peer bus name
 *
 * @return relevant TRUE to indicate the Announcement is from a relevant peer.
 *
 * Important: functions which implement this prototype MUST copy the input strings
 *            (i.e. peerName and objPath) if they need to use it later!
 *
 * This function is experimental, and as such has not been fully tested.
 * Please help make it more solid by contributing fixes if you find issues.
 */
typedef uint8_t (*AJ_AboutPeerIsRelevant)(const char* peerName);

/**
 * Type for a peer Announcement filter criterion
 *
 * This structure is experimental, and as such has not been fully tested.
 * Please help make it more solid by contributing fixes if you find issues.
 */
typedef struct _AJ_AboutPeerDescription {
    const char** implementsInterfaces;                        /**< interface names to resolve in peer Announcement */
    uint16_t numberInterfaces;                                /**< number of interface names */
    AJ_AboutPeerDescriptionMatched handleMatch;               /**< handleMatched called when a peer with a matching description is found. The callee returns whether to continue to process this description */
    AJ_AboutPeerIsRelevant handleIsRelevant;                  /**< handleIsRelevant called when the Announcement message was unmarshalled entirely. The callee returns whether the peer is relevant for enagement */
    AJ_AboutHandleObjectDescription handleObjectDescription;  /**< handleObjectDescription called when an object description is found. */
    AJ_AboutHandleMandatoryProps handleMandatoryProps;        /**< handleMandatoryProps called when mandatory props are found. */
    AJ_AboutHandleOptionalProperty handleOptionalProperty;    /**< handleOptionalProperty called when an optional property is found. This may be called prior to handleMandatoryProps */
} AJ_AboutPeerDescription;

/**
 * Register Announce signal handlers matching each provided peer description.
 *
 * @param peerDescs       An array of peer description structs.
 * @param peerDescsCount  The number of AJ_AboutPeerDescriptions in the peerDescs array.
 *
 * This function is experimental, and as such has not been fully tested.
 * Please help make it more solid by contributing fixes if you find issues.
 */
void AJ_AboutRegisterAnnounceHandlers(AJ_AboutPeerDescription* peerDescs, uint16_t peerDescsCount);

/**
 * Unmarshal an Announcement message AboutData section.
 *
 * @param announcement        The received Announcement message.
 * @param onMandatoryProps    A callback function to return the unmarshalled mandatory properties.
 * @param onOptionalProperty  A callback function to return each unmarshalled optional property.
 *
 * @return  Return AJ_Status
 *
 * This function is experimental, and as such has not been fully tested.
 * Please help make it more solid by contributing fixes if you find issues.
 */
AJ_Status AJ_AboutUnmarshalProps(AJ_Message* announcement, AJ_AboutHandleMandatoryProps onMandatoryProps, AJ_AboutHandleOptionalProperty onOptionalProperty);

/**
 * Unmarshal the AppId variant from an Announcement message AboutData section.
 *
 * @param announcement   The received Announcement message.
 * @param appIdBuf[out]  A buffer to store the AppId string value.
 * @param appIdBufLen    The length of the appIdBuf.
 *
 * @return  Return AJ_Status
 *
 * This function is experimental, and as such has not been fully tested.
 * Please help make it more solid by contributing fixes if you find issues.
 */
AJ_Status AJ_AboutUnmarshalAppIdFromVariant(AJ_Message* announcement, char* appIdBuf, size_t appIdBufLen);

/**
 * Handle an ANNOUNCE signal
 *
 * @param  announcement  The received Announcement message.
 * @param  version[out]  The org.alljoyn.About interface version in the Announcenment
 * @param  port[out]     The application session port in the Annoucnement
 * @param  peerName[out] The peer bus unique name of the Announcement sender (supply array of size AJ_MAX_NAME_SIZE+1)
 * @param  handled[out]  One of the registered Announce handlers indicated this peer is relevant for engagement
 *
 * @return  Return AJ_Status
 *
 * This function is experimental, and as such has not been fully tested.
 * Please help make it more solid by contributing fixes if you find issues.
 */
AJ_Status AJ_AboutHandleAnnounce(AJ_Message* announcement, uint16_t* version, uint16_t* port, char* peerName, uint8_t* relevant);
#endif

#ifdef __cplusplus
}
#endif

/** End of insert */
/**
 * @}
 */
#endif
