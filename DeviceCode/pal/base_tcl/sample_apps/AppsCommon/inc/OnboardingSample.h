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

#ifndef _ONBOARDINGSAMPLE_H_
#define _ONBOARDINGSAMPLE_H_

/** @defgroup Onboarding Sample
 *
 *  @{
 */

#include <alljoyn.h>

/**
 * on init
 */
AJ_Status Onboarding_Init(const char* deviceManufactureName, const char* deviceProductName);

/**
 * on idle connected
 * @param bus
 */
void Onboarding_DoWork(AJ_BusAttachment* busAttachment);

/**
 * on finish
 */
AJ_Status Onboarding_Finish();

/** @} */
 #endif // _ONBOARDINGSAMPLE_H_
