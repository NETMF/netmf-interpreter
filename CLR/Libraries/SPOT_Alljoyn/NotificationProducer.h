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

#ifndef NOTIFICATIONPRODUCER_H_
#define NOTIFICATIONPRODUCER_H_

#include "NotificationCommon.h"
#include "ServicesCommon.h"

/** @defgroup NotificationProducer Notification Producer
 * details Functions and variables that assist in writing Notification Producers
 *  @{
 */
/*!
   \brief Start index for Notification emitters
 */
#define NOTIFICATION_OBJECT_INDEX                NOTIFICATION_PRODUCER_OBJECTS_INDEX

/*!
   \brief Constant for messageType
 */
#define AJNS_NOTIFICATION_MESSAGE_TYPE_EMERGENCY 0

/*!
   \brief Constant for messageType
 */
#define AJNS_NOTIFICATION_MESSAGE_TYPE_WARNING   1

/*!
   \brief Constant for messageType
 */
#define AJNS_NOTIFICATION_MESSAGE_TYPE_INFO      2

/**
 * Notification Service Producer API
 */

/**
 * Start Notification service framework as Producer.
 * @return aj_status
 */
AJ_Status AJNS_Producer_Start();

/*!
   \brief Send a notification with the given content
   \param busAttachment The bus through which the Routing Node is reached
   \param content
   \param messageType
   \param ttl
   \param messageSerialNumber Returned serial number of the outgoing notification message. Use later when calling \ref AJNS_Producer_CancelNotification.
   \return AJ_Status
 */
AJ_Status AJNS_Producer_SendNotification(AJ_BusAttachment* busAttachment, AJNS_NotificationContent* notificationContent, uint16_t messageType, uint32_t ttl, uint32_t* messageSerialNumber);

/*!
   \brief Instruct the AllJoyn bus to remove last message of the specified message type from the bus
   \details
   Effectively, this overrides the ttl parameter in the function
   \param busAttachment The bus through which the Routing Node is reached
   \param messageType One of \ref AJNS_NOTIFICATION_MESSAGE_TYPE_INFO, \ref AJNS_NOTIFICATION_MESSAGE_TYPE_WARNING, or \ref AJNS_NOTIFICATION_MESSAGE_TYPE_EMERGENCY
   \return AJ_Status
 */
AJ_Status AJNS_Producer_DeleteLastNotification(AJ_BusAttachment* busAttachment, uint16_t messageType);

/*!
   \brief Instruct the AllJoyn bus to remove message of the specified notification id the bus
   \details
   Effectively, this overrides the ttl parameter in the function
   \param busAttachment The bus through which the Routing Node is reached
   \param messageSerialNumber The serial number of the notification to cancel on the daemon
   \return AJ_Status
 */
AJ_Status AJNS_Producer_CancelNotification(AJ_BusAttachment* busAttachment, uint32_t messageSerialNumber);

/*!
   \brief Implementation of Dismiss functionality canceling the message on daemon and sending a Dismiss SSL to recall received message
   \details
   Effectively, this overrides the ttl parameter in the function
   \param busAttachment The bus through which the Routing Node is reached
   \param msg The received Dismiss request message to process
   \return AJ_Status
 */
AJ_Status AJNS_Producer_DismissRequestHandler(AJ_BusAttachment* busAttachment, AJ_Message* msg);

/*!
   \brief Implementation of GetProperty functionality for the notification objects
   \details
   Use this function to respond to Get Property events as a callback function passed to AJ_BusPropGet
   \skip switch (msg->msgId)
   \until }
   \param replyMsg The reply message that will be sent
   \param propId The identifier of the property field
   \param context For internal use
   \return AJ_Status
 */
AJ_Status AJNS_Producer_PropGetHandler(AJ_Message* replyMsg, uint32_t propId, void* context);

/*!
   \brief Implementation of SetProperty functionality for the notification objects
   \details
   Use this function to respond to Set Property events as a callback function passed to AJ_BusPropSet
   \skip switch (msg->msgId)
   \until }
   \param replyMsg The reply message that will be sent
   \param propId The identifier of the property field
   \param context For internal use
   \return AJ_Status
 */
AJ_Status AJNS_Producer_PropSetHandler(AJ_Message* replyMsg, uint32_t propId, void* context);

/**
 * Function called after busAttachment connects to Routing Node
 * @param busAttachment
 * @return status
 */
AJ_Status AJNS_Producer_ConnectedHandler(AJ_BusAttachment* busAttachment);

/**
 * Session request accept/reject function for service targetted session
 * @param port
 * @param sessionId
 * @param joiner
 */
uint8_t AJNS_Producer_CheckSessionAccepted(uint16_t port, uint32_t sessionId, const char* joiner);

/**
 * MessageProcessor function for the producer
 * @param busAttachment
 * @param msg
 * @param msgStatus
 * @return serviceStatus - was message handled
 */
AJSVC_ServiceStatus AJNS_Producer_MessageProcessor(AJ_BusAttachment* busAttachment, AJ_Message* msg, AJ_Status* msgStatus);

/**
 * Function called after busAttachment disconnects from Routing Node
 * @param busAttachment
 * @return status
 */
AJ_Status AJNS_Producer_DisconnectHandler(AJ_BusAttachment* busAttachment);

/** @} */ // End of group 'NotificationProducer'

#endif /* NOTIFICATIONPRODUCER_H_ */
