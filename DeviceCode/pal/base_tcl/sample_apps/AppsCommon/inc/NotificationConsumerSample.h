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

#ifndef _NOTIFICATIONCONSUMERSAMPLE_H_
#define _NOTIFICATIONCONSUMERSAMPLE_H_

#include <alljoyn.h>

#define CONSUMER_ACTION_NOTHING         0
#define CONSUMER_ACTION_DISMISS         1

/*
 * Functions that the application needs to implement
 */

/**
 * Initialize the Notification Service Consumer
 * @return status
 */
AJ_Status NotificationConsumer_Init();

/**
 * If idle, do work
 * @param busAttachment
 */
void NotificationConsumer_DoWork(AJ_BusAttachment* busAttachment);

/**
 * Finish Consumer - called when bus disconnects
 * @return status
 */
AJ_Status NotificationConsumer_Finish();

#endif /* _NOTIFICATIONCONSUMERSAMPLE_H_ */

