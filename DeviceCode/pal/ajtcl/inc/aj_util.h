#ifndef _AJ_UTIL_H
#define _AJ_UTIL_H

/**
 * @file aj_util.h
 * @defgroup aj_util Utility Functions
 * @{
 */
/******************************************************************************
 * Copyright (c) 2012-2014, AllSeen Alliance. All rights reserved.
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

#ifdef __cplusplus
extern "C" {
#endif

/**
 * Structure for holding a time
 */
typedef struct _AJ_Time {
    uint16_t milliseconds;      /**< The number of milliseconds in the time */
    uint32_t seconds;           /**< The number of seconds in the time */
} AJ_Time;

/**
 * Get the current alljoyn version found in aj_version.h.
 * Format is:
 * "<Major>.<Minor>.<Release> Tag <tag>"
 *
 * @return  A string containing the alljoyn version
 */
AJ_EXPORT
const char* AJ_GetVersion();

/**
 * Get the time elapsed in milliseconds since this function was called with the same timer.
 * Call AJ_InitTimer() to initialize the timer before calling this function.
 *
 * @param timer      Tracks relative time.
 * @param cumulative If TRUE the elapsed time returned is cumulative, otherwise it is relative to
 *                   the time the function was last called.
 *
 * @return  The elapsed time in milliseconds.
 */
AJ_EXPORT
uint32_t AJ_GetElapsedTime(AJ_Time* timer, uint8_t cumulative);

/**
 * Gets the time for the debug statement.
 *
 * @param timer the time for the debug statement.
 *
 * @return AJ_OK if the time has been set. An error indicates that the time has not been set and the default debug time should be used.
 */
AJ_EXPORT
AJ_Status _AJ_GetDebugTime(AJ_Time* timer);

/**
 * Initialize a timer
 *
 * @param timer  The timer to initialize
 */
AJ_EXPORT
void AJ_InitTimer(AJ_Time* timer);

AJ_EXPORT
int32_t AJ_GetTimeDifference(AJ_Time* timerA, AJ_Time* timerB);

AJ_EXPORT
void AJ_TimeAddOffset(AJ_Time* timerA, uint32_t msec);

AJ_EXPORT
int8_t AJ_CompareTime(AJ_Time timerA, AJ_Time timerB);

/**
 * Indicates a future time so a periodic event won't happen
 */
#define AJ_TIMER_FOREVER  -1


/**
 * Suspend to low-power mode on embedded devices
 *
 * @param msec milliseconds to wait before waking up
 *
 * @return        Return AJ_Status
 */
AJ_Status AJ_SuspendWifi(uint32_t msec);


/**
 * Pause the current thread for a number of milliseconds
 */
AJ_EXPORT
void AJ_Sleep(uint32_t time);

/**
 * Allocate memory. This function should only be used for allocation of short term buffers that
 * might otherwise be allocated on the stack.
 */
AJ_EXPORT
void* AJ_Malloc(size_t size);

/**
 * Reallocate a previously allocated memory block to a new size
 */
AJ_EXPORT
void* AJ_Realloc(void* ptr, size_t size);

/**
 * Free memory previously allocated by AJ_Malloc()
 */
AJ_EXPORT
void AJ_Free(void* mem);


/**
 * Macro for getting the size of an array variable
 */
#define ArraySize(a)  (sizeof(a) / sizeof(a[0]))

/**
 * Find position of first instance of any character in a given string in a string.
 *
 * @param str   The string to search
 * @param chars The characters to search for
 *
 * @return  The position of the first instance of the character in the string or -1 if the character
 *          does not appear in the string.
 *
 */
int32_t AJ_StringFindFirstOf(const char* str, char* chars);

/**
 * Convert a raw byte string to NUL terminated ascii hex string. It is permitted for raw and hex to
 * point to the same memory location.
 *
 * @param raw     The bytes to convert
 * @param rawLen  The number of bytes to convert
 * @param hex     The buffer to receive the converted hex data
 * @param hexLen  The length of the hex buffer
 * @param lower   TRUE for lower-case, FALSE for upper-case
 *
 * @return
 *          - AJ_OK if the string was converted
 *          - AJ_ERR_RESOURCES if the hexLen is too small to fit the converted string.
 */
AJ_Status AJ_RawToHex(const uint8_t* raw, size_t rawLen, char* hex, size_t hexLen, uint8_t lower);

/**
 * Convert a NUL terminated ascii hex string to raw bytes. It is permitted for raw and hex buffer to
 * point to the same memory location.
 *
 * @param hex     The buffer containing the ASCII hex string
 * @param hexLen  Length of the hex data to decode or zero to decode entire string.
 * @param raw     The bytes to convert
 * @param rawLen  The number of bytes to convert
 *
 * @return
 *          - AJ_OK if the string was converted.
 *          - AJ_ERR_RESOURCES if the rawLen is too small to fit the converted string.
 *          - AJ_ERR_UNEXPECTED if the string is not a hexidecimal string.
 */
AJ_Status AJ_HexToRaw(const char* hex, size_t hexLen, uint8_t* raw, size_t rawLen);

/**
 * get a line of input from the the file pointer (most likely stdin).
 * This will capture the the num-1 characters or till a newline character is
 * entered.
 *
 * @param[out] str a pointer to a character array that will hold the user input
 * @param[in]  num the size of the character array 'str'
 * @param[in]  fp  the file pointer the sting will be read from. (most likely stdin)
 *
 * @return returns the same string as 'str' if there has been a read error a null
 *                 pointer will be returned and 'str' will remain unchanged.
 */
char* AJ_GetLine(char* str, size_t num, void* fp);

/**
 * Prepare to read input from stdin
 */
uint8_t AJ_StartReadFromStdIn();

/**
 * Stop reading input from stdin
 */
uint8_t AJ_StopReadFromStdIn();

/**
 * get a line of input from stdin.
 * This will capture the the num-1 characters or till a newline character is
 * entered.
 *
 * @param[out] buf a character array that will hold the user input
 * @param[in]  num the size of the character array 'str'
 *
 * @return returns the same string as 'buf' if there has been a read error a null
 *                 pointer will be returned and 'buf' will remain unchanged.
 */
char* AJ_GetCmdLine(char* buf, size_t num);

/**
 * Encodes raw bytes to base 64
 *
 * @param raw    The raw bytes to encode
 * @param rawlen The length of the raw bytes to encode
 * @param pem    The base64 encoded string
 * @param pemlen The size of the input buffer
 *
 * @return AJ_OK on success, AJ_ERR_RESOURCES if input buffer too short
 */
AJ_Status AJ_RawToB64(const uint8_t* raw, size_t rawlen, char* pem, size_t pemlen);

/**
 * Decodes base64 to raw bytes
 *
 * @param pem    The base64 encoded string
 * @param pemlen The length of the encoded string
 * @param raw    The decoded raw bytes
 * @param rawlen The size of the input buffer
 *
 * @return AJ_OK on success, AJ_ERR_RESOURCES if input buffer too short
 */
AJ_Status AJ_B64ToRaw(const char* pem, size_t pemlen, uint8_t* raw, size_t rawlen);

uint16_t AJ_ByteSwap16(uint16_t x);
uint32_t AJ_ByteSwap32(uint32_t x);
uint64_t AJ_ByteSwap64(uint64_t x);

#ifdef __cplusplus
}
#endif
/**
 * Convert integer to decimal string representation
 *
 * @param val    The integer to convert
 * @param buf    The input buffer where converted string is stored
 * @param buflen The size of the input buffer
 *
 * @return AJ_OK on success, AJ_ERR_RESOURCES if input buffer too short
 */
AJ_Status AJ_IntToString(int32_t val, char* buf, size_t buflen);

/**
 * Convert AF_INET address to dotted decimal string representation
 *
 * @param val    The address to convert
 * @param buf    The input buffer where converted string is stored
 * @param buflen The size of the input buffer
 *
 * @return AJ_OK on success, AJ_ERR_RESOURCES if input buffer too short
 */
AJ_Status AJ_InetToString(uint32_t addr, char* buf, size_t buflen);

/**
 * @}
 */
#endif
