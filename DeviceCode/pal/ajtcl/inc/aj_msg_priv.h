#ifndef _AJ_MSG_PRIV_H
#define _AJ_MSG_PRIV_H

/**
 * @file aj_msg_priv.h
 * @defgroup aj_msg_priv Non-public Message Marshaling and Unmarshaling APIs
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

#include <alljoyn.h>

#ifdef __cplusplus
extern "C" {
#endif

/**
 * Returns the signature of the next arg to be unmarshalled.
 *
 * @param msg   A pointer to the message currently being unmarshaled
 *
 * @return  Pointer to the signature of the next arg to unmarshal.
 */
AJ_EXPORT
const char* AJ_NextArgSig(AJ_Message* msg);

/**
 * Check if the type ID is a container type (array, struct, dictionay entry).
 *
 * @param typeId  The type or element type if the array flag is set
 *
 * @return  Indication if the type is a container or not.
 *          - TRUE (1) iff the type is a container.
 *          - FLASE (0) iff the type is not a container.
 */
AJ_EXPORT
int AJ_IsContainerType(char typeId);

/**
 * Check if the type ID is a scalar type (boolean, byte, int16, int32, int64, uint16, uint32, uint64, double).
 *
 * @param typeId  The type or element type if the array flag is set
 *
 * @return  Indication if the type is a scalar or not.
 *          - TRUE (1) iff the type is a scalar.
 *          - FLASE (0) iff the type is not a scalar.
 */
AJ_EXPORT
int AJ_IsScalarType(char typeId);

/**
 * Check if the type ID is a string type (string, object path, signature).
 *
 * @param typeId  The type or element type if the array flag is set
 *
 * @return  Indication if the type is a string or not.
 *          - TRUE (1) iff the type is a string.
 *          - FLASE (0) iff the type is not a string.
 */
AJ_EXPORT
int AJ_IsStringType(char typeId);

/**
 * Check if the type ID is a basic type (string or scalar).
 *
 * @param typeId  The type or element type if the array flag is set
 *
 * @return  Indication if the type is a basic type or not.
 *          - TRUE (1) iff the type is a basic type.
 *          - FLASE (0) iff the type is not a basic type.
 */
AJ_EXPORT
int AJ_IsBasicType(char typeId);

/**
 * For scalar types, get the size of that type.  For non-scalar types, get the alignment.
 *
 * @param typeId  The type or element type if the array flag is set
 *
 * @return  Size (or alignment) of the type.
 */
AJ_EXPORT
size_t AJ_GetTypeSize(char typeId);

/**
 * Lookup the message identifier and set the msgId on the message.
 *
 * @param msg    The message already initialized with object, interface, and member fields
 * @param secure Returns boolen indicating if the object or interface was marked secure
 *
 * @return  - AJ_OK if the message was found
 *          - AJ_ERR_SIGNATURE if the message was found but the signature was missing or incorrect.
 *            The message identified is still set.
 *          - AJ_ERR_NO_MATCH if the message could not be identified
 */
AJ_EXPORT
AJ_Status AJ_LookupMessageId(AJ_Message* msg, uint8_t* secure);

/**
 * Lookup a property identifier and get the property signature
 *
 * @param msg     A property Get or Set method call or reply message
 * @param iface   The interface the property is defined on
 * @param prop    The property name
 * @param propId  Returns the property identifier
 * @param sig     Returns the property type signature
 * @param secure  Returns boolen indicating if the property is on an object or interface marked secure
 *
 * @return  - AJ_OK if the message was found
 *          - AJ_ERR_NO_MATCH if the property could not be identified
 */
AJ_EXPORT
AJ_Status AJ_IdentifyProperty(AJ_Message* msg, const char* iface, const char* prop, uint32_t* propId, const char** sig, uint8_t* secure);

#ifdef __cplusplus
}
#endif
/**
 * @}
 */
#endif

