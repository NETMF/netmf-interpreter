#ifndef _AJ_VERSION_H
#define _AJ_VERSION_H
/**
 * @file aj_version.h
 * @defgroup aj_version Current AllJoyn Thin Client Version
 * @{
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

#define AJ_RELEASE_YEAR        15       /**< release year */
#define AJ_MAJOR_VERSION       AJ_RELEASE_YEAR /**< deprecated */
#define AJ_RELEASE_MONTH       4        /**< release month */
#define AJ_MINOR_VERSION       AJ_RELEASE_MONTH /**< deprecated */
#define AJ_FEATURE_VERSION     0        /**< feature version */
#define AJ_RELEASE_VERSION     AJ_FEATURE_VERSION /**< deprecated */
#define AJ_BUGFIX_VERSION      0        /**< bugfix version (0=first, 0x61==a, 0x62==b, etc.) */
#define AJ_RELEASE_YEAR_STR    15       /**< release year string (two digits) */
#define AJ_RELEASE_MONTH_STR   04       /**< release month string (two digits) */
#define AJ_FEATURE_VERSION_STR 00       /**< feature version string (00, 01, 02, ...) */
#define AJ_BUGFIX_VERSION_STR           /**< bugfix version string (blank, a, b, ...) */
#define AJ_RELEASE_TAG         "v15.04"

#define AJ_VERSION (((AJ_RELEASE_YEAR) << 24) | ((AJ_RELEASE_MONTH) << 16) | ((AJ_FEATURE_VERSION) << 8) | (AJ_BUGFIX_VERSION))  /**< macro to generate the version from major, minor, release, bugfix */

/**
 * @}
 */
#endif
