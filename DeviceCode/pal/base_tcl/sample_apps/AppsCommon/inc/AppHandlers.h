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

#ifndef _APP_HANDLERS_H_
#define _APP_HANDLERS_H_

#include <alljoyn.h>
#include <alljoyn/services_common/ServicesCommon.h>

/**
 * Initialize the Services. called at the start of the application
 * @param aboutServicePort
 * @param deviceManufactureName
 * @param deviceProductName
 * @return aj_status
 */
AJ_Status AJServices_Init(uint16_t aboutServicePort, const char* deviceManufactureName, const char* deviceProductName);

/**
 * Run when the bus is connected to the Routing Node
 * application is idle
 * @param busAttachment
 * @param maxNumberOfAttempts - maximum number of attempts to initialize the services
 * @param sleepTimeBetweenAttempts - time in ms to sleep between attempts to initialize the services
 * @return aj_status for last request to Routing Node
 */
AJ_Status AJApp_ConnectedHandler(AJ_BusAttachment* busAttachment, uint8_t maxNumberOfAttempts, uint32_t sleepTimeBetweenAttempts);

/**
 * Process an incoming message
 * @param busAttachment
 * @param msg
 * @param status
 * @return servicestatus - shows if the message was processed or not
 */
AJSVC_ServiceStatus AJApp_MessageProcessor(AJ_BusAttachment* busAttachment, AJ_Message* msg, AJ_Status* status);

/**
 * Run when there is a timeout reading off the bus
 * application is idle
 * @param busAttachment
 */
void AJApp_DoWork(AJ_BusAttachment* busAttachment);

/**
 * Run when the bus is disconnecting from the Routing Node
 * Connection to Routing Node is either restarting or was stopped
 * @param busAttachment
 * @param restart
 * @return aj_status for last request to Routing Node
 */
AJ_Status AJApp_DisconnectHandler(AJ_BusAttachment* busAttachment, uint8_t restart);

#endif /* _APP_HANDLERS_H_ */
