#ifndef _AJ_MSG_H
#define _AJ_MSG_H

/**
 * @file aj_msg.h
 * @defgroup aj_msg Message Marshaling and Unmarshaling
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

#include "aj_target.h"
#include "aj_status.h"
#include "aj_bus.h"
#include "aj_util.h"

#ifdef __cplusplus
extern "C" {
#endif

/*
 * Message argument types
 */
#define AJ_ARG_INVALID           '\0'   /**< AllJoyn invalid type */
#define AJ_ARG_ARRAY             'a'    /**< AllJoyn array container type */
#define AJ_ARG_BOOLEAN           'b'    /**< AllJoyn boolean basic type */
#define AJ_ARG_DOUBLE            'd'    /**< AllJoyn IEEE 754 double basic type */
#define AJ_ARG_SIGNATURE         'g'    /**< AllJoyn signature basic type */
#define AJ_ARG_HANDLE            'h'    /**< AllJoyn socket handle basic type */
#define AJ_ARG_INT32             'i'    /**< AllJoyn 32-bit signed integer basic type */
#define AJ_ARG_INT16             'n'    /**< AllJoyn 16-bit signed integer basic type */
#define AJ_ARG_OBJ_PATH          'o'    /**< AllJoyn Name of an AllJoyn object instance basic type */
#define AJ_ARG_UINT16            'q'    /**< AllJoyn 16-bit unsigned integer basic type */
#define AJ_ARG_STRING            's'    /**< AllJoyn UTF-8 NULL terminated string basic type */
#define AJ_ARG_UINT64            't'    /**< AllJoyn 64-bit unsigned integer basic type */
#define AJ_ARG_UINT32            'u'    /**< AllJoyn 32-bit unsigned integer basic type */
#define AJ_ARG_VARIANT           'v'    /**< AllJoyn variant container type */
#define AJ_ARG_INT64             'x'    /**< AllJoyn 64-bit signed integer basic type */
#define AJ_ARG_BYTE              'y'    /**< AllJoyn 8-bit unsigned integer basic type */
#define AJ_ARG_STRUCT            '('    /**< AllJoyn struct container type */
#define AJ_ARG_DICT_ENTRY        '{'    /**< AllJoyn dictionary or map container type - an array of key-value pairs */

/*
 * Message argument flags
 */
#define AJ_ARRAY_FLAG            0x01   /**< Indicates an argument is an array */

/*
 * Endianess flag. This is the first byte of a message
 */
#define AJ_LITTLE_ENDIAN 'l'           /**< Indicates the bus is little-endian */
#define AJ_BIG_ENDIAN    'B'           /**< Indicates the bus is big-endian */

/*
 * Set the native endianness
 */
#if HOST_IS_BIG_ENDIAN
#define AJ_NATIVE_ENDIAN AJ_BIG_ENDIAN
#else
#define AJ_NATIVE_ENDIAN AJ_LITTLE_ENDIAN
#endif
/*
 * Message flags are or'd together
 */
#define AJ_NO_FLAGS                0x00    /**< No message flags */
#define AJ_FLAG_NO_REPLY_EXPECTED  0x01    /**< Not expecting a reply */
#define AJ_FLAG_AUTO_START         0x02    /**< Auto start the service */
#define AJ_FLAG_ALLOW_REMOTE_MSG   0x04    /**< Allow messeages from remote hosts */
#define AJ_FLAG_SESSIONLESS        0x10    /**< Sessionless message */
#define AJ_FLAG_GLOBAL_BROADCAST   0x20    /**< Global (bus-to-bus) broadcast */
#define AJ_FLAG_COMPRESSED         0x40    /**< Header is compressed */
#define AJ_FLAG_ENCRYPTED          0x80    /**< Body is encrypted */

#define ALLJOYN_FLAG_SESSIONLESS   0x10    /**< Deprecated: Use AJ_FLAG_SESSIONLESS instead */

/*
 * Wire protocol version number
 */
#define AJ_MAJOR_PROTOCOL_VERSION  1       /**< AllJoyn protocol version */

/*
 * Message types
 */
#define AJ_MSG_INVALID      0              /**< Invalid message type */
#define AJ_MSG_METHOD_CALL  1              /**< Method call message type */
#define AJ_MSG_METHOD_RET   2              /**< Method return message type */
#define AJ_MSG_ERROR        3              /**< Error message type */
#define AJ_MSG_SIGNAL       4              /**< Signal message type */


/*
 * Header field types
 */
#define AJ_HDR_INVALID               0x00  /**< Invalid header field type */
#define AJ_HDR_OBJ_PATH              0x01  /**< Object path header field type */
#define AJ_HDR_INTERFACE             0x02  /**< Message interface header field type */
#define AJ_HDR_MEMBER                0x03  /**< Member (message/signal) name header field type */
#define AJ_HDR_ERROR_NAME            0x04  /**< Error name header field type */
#define AJ_HDR_REPLY_SERIAL          0x05  /**< Reply serial number header field type */
#define AJ_HDR_DESTINATION           0x06  /**< Message destination header field type */
#define AJ_HDR_SENDER                0x07  /**< Sender well-known name header field type */
#define AJ_HDR_SIGNATURE             0x08  /**< Message signature header field type */
#define AJ_HDR_HANDLES               0x09  /**< Number of file/socket handles that accompany the message */
#define AJ_HDR_TIMESTAMP             0x10  /**< Time stamp header field type (AllJoyn specific headers start at 0x10. Time stamp header field type) */
#define AJ_HDR_TIME_TO_LIVE          0x11  /**< Messages time-to-live header field type */
#define AJ_HDR_COMPRESSION_TOKEN     0x12  /**< Messages compression token header field type */
#define AJ_HDR_SESSION_ID            0x13  /**< Session id header field type */

/**
 * Maximum string length of a service (including null terminator)
 */
#define AJ_MAX_SERVICE_NAME_SIZE 256

/**
 * Type for a message argument
 */
struct _AJ_Arg {

    uint8_t typeId;    /**< the argument type */
    uint8_t flags;     /**< non-zero if the value is a variant - values > 1 indicate variant-of-variant etc. */
    uint16_t len;      /**< length of a string or array in bytes */

    /*
     * Union of the various argument values.
     */
    union {
        uint8_t*     v_byte;        /**< byte type field value in the message */
        int16_t*     v_int16;       /**< int16 type field value in the message */
        uint16_t*    v_uint16;      /**< uint16 type field value in the message */
        uint32_t*    v_bool;        /**< boolean type field value in the message */
        uint32_t*    v_uint32;      /**< uint32 type field value in the message */
        int32_t*     v_int32;       /**< int32 type field value in the message */
        int64_t*     v_int64;       /**< int64 type field value in the message */
        uint64_t*    v_uint64;      /**< uint64 type field value in the message */
        double*      v_double;      /**< double type field value in the message */
        const char*  v_string;      /**< string(char *) type field value in the message */
        const char*  v_objPath;     /**< objPath(char *) type field value in the message */
        const char*  v_signature;   /**< signature(char *) type field value in the message */
        const void*  v_data;        /**< data(void *) type field value in the message */
    } val;                          /**< union of the field value in the message */

    const char* sigPtr;             /**< pointer to the signature */
    struct _AJ_Arg* container;      /**< container argument */
};

/**
 * AllJoyn Message Header
 */
typedef struct _AJ_MsgHeader {
    char endianess;        /**< The endianness of this message */
    uint8_t msgType;       /**< Indicates if the message is method call, signal, etc. */
    uint8_t flags;         /**< Flag bits */
    uint8_t majorVersion;  /**< Major version of this message */
    uint32_t bodyLen;      /**< Length of the body data */
    uint32_t serialNum;    /**< serial of this message */
    uint32_t headerLen;    /**< Length of the header data */
} AJ_MsgHeader;

/**
 * AllJoyn Message
 */
struct _AJ_Message {
    uint32_t msgId;            /**< Identifies the message to the application */
    AJ_MsgHeader* hdr;         /**< The message header */
    union {
        const char* objPath;   /**< The nul terminated object path string or NULL */
        uint32_t replySerial;  /**< The reply serial number */
    };
    union {
        const char* member;    /**< The nul terminated member name string or NULL */
        const char* error;     /**< The nul terminated error name string or NULL */
    };
    const char* iface;         /**< The nul terminated interface string or NULL */
    const char* sender;        /**< The nul terminated sender string or NULL */
    const char* destination;   /**< The nul terminated destination string or NULL */
    const char* signature;     /**< The nul terminated signature string or NULL */
    uint32_t sessionId;        /**< Session id */
    uint32_t timestamp;        /**< Timestamp */
    uint32_t ttl;              /**< Time to live */
    /*
     * Private message state - the application should not touch this data
     */
    uint8_t sigOffset;         /**< Offset to current position in the signature */
    uint8_t varOffset;         /**< For variant marshalling/unmarshalling - Offset to start of variant signature */
    uint16_t bodyBytes;        /**< Running count of the number body bytes written */
    AJ_BusAttachment* bus;     /**< Bus attachment for this message */
    struct _AJ_Arg* outer;     /**< Container arg current being marshaled */
    uint32_t timeout;          /**< Remaining time to wait for all bytes of this message */
    uint32_t authVersion;      /**< Authentication version used */
};

/**
 * Unmarshals a message returning a message structure. Note that if a message is received but is not
 * recognized this function will return an uninitialized msg with msgId == 0. The application must
 * be prepared to handle this case.
 *
 * @param bus     The bus attachment
 * @param msg     Pointer to a structure to receive the unmarshalled message
 * @param timeout How long to wait for a message
 *
 * @return
 *          - AJ_OK if a message header was succesfully unmarshaled. Note that the message may have
 *            been consumed or rejected internally in which case the msgId will be zero.
 *          - AJ_ERR_UNMARSHAL if the message was badly formed
 *          - AJ_ERR_RESOURCES if the message header is too big to unmarshal into the attached buffer
 *          - AJ_ERR_TIMEOUT if there was no message to unmarshal within the timeout period
 *          - AJ_ERR_READ if there was a read failure
 */
AJ_EXPORT
AJ_Status AJ_UnmarshalMsg(AJ_BusAttachment* bus, AJ_Message* msg, uint32_t timeout);

/**
 * Unmarshals the next argument from a message or next element in a container (array, struct,
 * dictionary entry, or variant).
 *
 * @param msg     A pointer to a message that was unmarshaled by an earlier call to AJ_UnmarshalMsg
 * @param arg     Pointer to unmarshal the argument
 *
 * @return
 *          - AJ_OK if the argument was succesfully unmarshaled.
 *          - AJ_ERR_UNMARSHAL if the arg was badly formed
 *          - AJ_ERR_READ if there was a read failure
 *          - AJ_ERR_NO_MORE when there is no more to unmarshal (typically for array and container elements)
 */
AJ_EXPORT
AJ_Status AJ_UnmarshalArg(AJ_Message* msg, AJ_Arg* arg);

/**
 * Unmarshals and discard the next argument from a message or next element in a container (array,
 * struct, dictionary entry). Variants must be skipped atomically, that is AJ_UnmarshalVariant()
 * should not be called.
 *
 * @param msg     A pointer to a message that was unmarshaled by an earlier call to AJ_UnmarshalMsg
 *
 * @return
 *          - AJ_OK if the argument was succesfully skipped.
 *          - AJ_ERR_UNMARSHAL if the arg was badly formed
 *          - AJ_ERR_READ if there was a read failure
 */
AJ_EXPORT
AJ_Status AJ_SkipArg(AJ_Message* msg);

/**
 * Attempts to reset the state of message so the arguments can be unmarshaled again. All of the
 * arguments must have been unmarshaled, a restriction to ensure that endianness conversions
 * are correct. This function allows a message handler to inspect the message arguments and
 * decide whether to handle it or not. For example a service handler might need to check if it
 * is supposed to accept a session joiner by inspecting the session port.
 *
 * @param msg     A pointer to a message that was unmarshaled by an earlier call to AJ_UnmarshalMsg
 *
 * @return
 *          - AJ_OK if the message arguments were succesfully reset
 *          - AJ_ERR_UNMARSHAL if the arguments could not be reset
 */
AJ_EXPORT
AJ_Status AJ_ResetArgs(AJ_Message* msg);

/**
 * Unmamarshals one or arguments of basic types such as integers, strings.
 *
 * @param msg       A pointer to a message that was unmarshaled by an earlier call to AJ_UnmarshalMsg
 * @param signature The signature of the argument list to unmarshal.
 * @param ...       Pointers to values of the correct sizeo and type per the signature.
 *
 * @return
 *          - AJ_OK if the arguments were succesfully unmarshaled.
 *          - AJ_ERR_UNMARSHAL if the arg was badly formed
 *          - AJ_ERR_READ if there was a read failure
 *          - AJ_ERR_UNEXPECTED if any of the argument types in the signature is not a basic type
 */
AJ_EXPORT
AJ_Status AJ_UnmarshalArgs(AJ_Message* msg, const char* signature, ...);

/**
 * Unmarshals data from a message as raw bytes.
 *
 * The main use of this function is for unmarshalling message payloads that exceed the size of the
 * network transmit buffer. Note that the data pointer returned is only valid until the next call to
 * AJ_UnmarshalRaw() so must be consumed or buffered by the application.
 *
 * @param msg    A pointer to the message currently being marshaled
 * @param data   Returns a pointer to the unmarshalled data
 * @param len    The number of bytes to unmarshal
 * @param actual Returns the actual number of bytes unmarshaled
 *
 * @return
 *          - AJ_OK if the data was succesfully unmarshaled.
 *          - AJ_ERR_READ if there was a read failure
 *          - AJ_ERR_UNMARSHAL if there is no more data to unmarshal
 *          - AJ_ERR_SIGNATURE of an invalid type was found in the message
 */
AJ_EXPORT
AJ_Status AJ_UnmarshalRaw(AJ_Message* msg, const void** data, size_t len, size_t* actual);

/**
 * Begin unmarshalling a container argument.
 *
 * @param msg     A pointer to a message that was unmarshaled by an earlier call to AJ_UnmarshalMsg
 * @param arg     Returns the unmarshaled container argument
 * @param typeId  The expected type of the container (for checking purposes)
 *
 * @return   Return AJ_Status
 *          - AJ_OK if the container was succesfully unmarshaled.
 *          - AJ_ERR_UNMARSHAL if the arg was badly formed
 *          - AJ_ERR_READ if there was a read failure
 *          - AJ_ERR_NO_MORE when there is no more to unmarshal (typically for array and container elements)
 */
AJ_EXPORT
AJ_Status AJ_UnmarshalContainer(AJ_Message* msg, AJ_Arg* arg, uint8_t typeId);

/**
 * Finish unmarshalling a container argument
 *
 * @param msg   A pointer to a message that was unmarshaled by an earlier call to AJ_UnmarshalMsg
 * @param arg   The container argument currently being unmarshaled
 *
 * @return   Return AJ_Status
 */
AJ_EXPORT
AJ_Status AJ_UnmarshalCloseContainer(AJ_Message* msg, AJ_Arg* arg);

/**
 * Prepare to unmarshal a variant. The next argument unmarshalled is expected to be variant.
 *
 * @param msg   A pointer to the message currently being marshaled
 * @param sig   Returns the signature for the variant
 *
 * @return   Return AJ_Status
 */
AJ_EXPORT
AJ_Status AJ_UnmarshalVariant(AJ_Message* msg, const char** sig);

/**
 * Closes an ummarshalled message when it is no longer needed. This releases resources and makes the
 * bus available for unmarshalling another message. After a message has been closed unmarshaled
 * values are no longer valid so this function should not be called until the message and its
 * arguments are no longer needed.
 *
 * @param msg     The message to close.
 *
 * @return   Return AJ_Status
 */
AJ_EXPORT
AJ_Status AJ_CloseMsg(AJ_Message* msg);

/**
 * Type for a session identifier
 */
typedef uint32_t AJ_SessionId;

/**
 * Marshal a METHOD_CALL message.
 *
 * @param bus          The bus attachment
 * @param msg          Pointer to a message structure
 * @param msgId        The message identifier for this message
 * @param destination  Bus address of the destination for this message
 * @param sessionId    The session this message is for.
 * @param flags        A logical OR of the applicable message flags
 * @param timeout      Time in milliseconds to allow for a reply to the message before reporting
 *                     a timeout error message is reported to the application.
 *
 * @return
 *          - AJ_OK if a message header was succesfully marshaled
 *          - AJ_ERR_RESOURCES if the message is too big to marshal into the message buffer
 *          - AJ_ERR_WRITE if there was a write failure
 */
AJ_EXPORT
AJ_Status AJ_MarshalMethodCall(AJ_BusAttachment* bus, AJ_Message* msg, uint32_t msgId, const char* destination, AJ_SessionId sessionId, uint8_t flags, uint32_t timeout);

/**
 * Marshal a SIGNAL message.
 *
 * @param bus          The bus attachment
 * @param msg          Pointer to a message structure
 * @param msgId        The message identifier for this message
 * @param destination  Bus address of the destination for this message
 * @param sessionId    The session this message is for.
 * @param flags        A logical OR of the applicable message flags
 * @param ttl          Time to live for this signal in milliseconds. This parameter should be set to 0
 *                     for a signal with no ttl.
 *
 * @return
 *          - AJ_OK if a message header was succesfully marshaled
 *          - AJ_ERR_RESOURCES if the message is too big to marshal into the message buffer
 *          - AJ_ERR_WRITE if there was a write failure
 */
AJ_EXPORT
AJ_Status AJ_MarshalSignal(AJ_BusAttachment* bus, AJ_Message* msg, uint32_t msgId, const char* destination, AJ_SessionId sessionId, uint8_t flags, uint32_t ttl);

/**
 * Initialize and marshal a message that is a reply to a method call.
 *
 * @param methodCall  The method call message that was received
 * @param reply       The reply to be initialized
 *
 * @return   Return AJ_Status
 */
AJ_EXPORT
AJ_Status AJ_MarshalReplyMsg(const AJ_Message* methodCall, AJ_Message* reply);

/**
 * Initialize and marshal a message that is a error response to a method call.
 *
 * @param methodCall  The method call message that was received
 * @param reply       The reply to be initialized
 * @param error       The error name to use in the response.
 *
 * @return   Return AJ_Status
 */
AJ_EXPORT
AJ_Status AJ_MarshalErrorMsg(const AJ_Message* methodCall, AJ_Message* reply, const char* error);

/**
 * Initialize and marshal a message that is a error response to a method call.
 *
 * @param methodCall  The method call message that was received
 * @param reply       The reply to be initialized
 * @param error       The error name to use in the response.
 * @param info        A text string that provides additional information about the error. Can be
 *                    NULL in which case this is equivalent to calling AJ_MarshalErrorMsg().
 *
 * @return   Return AJ_Status
 */
AJ_EXPORT
AJ_Status AJ_MarshalErrorMsgWithInfo(const AJ_Message* methodCall, AJ_Message* reply, const char* error, const char* info);

/**
 * Initialize and marshal a message that is a error response to a method call. This is a wrapper
 * function around AJ_MarshalErrorMsg that chooses an appropriate error message depending on the
 * AJ_Status value passed in.
 *
 * @param methodCall  The method call message that was received
 * @param reply       The reply to be initialized
 * @param status      The status code for the error
 *
 * @return   Return AJ_Status
 */
AJ_EXPORT
AJ_Status AJ_MarshalStatusMsg(const AJ_Message* methodCall, AJ_Message* reply, AJ_Status status);

/**
 * Delivers a marshalled message to the network.
 *
 * @param msg     The message to deliver.
 *
 * @return
 *          - AJ_OK if the message was succesfully delivered
 *          - AJ_ERR_MARSHAL if the message arguments were incompletely marshaled
 */
AJ_EXPORT
AJ_Status AJ_DeliverMsg(AJ_Message* msg);

/**
 * This function does partial delivery of a marshalled message. This allow an application to send
 * messages that are larger (much larger) than the transmit buffer. The remaining data must be
 * custom marshaled by the application using AJ_MarshalRaw(). After all the remaining data has been
 * marshaled the applicatiom must call AJ_DeliverMsg() to complete the delivery of the message to
 * the network.
 *
 *
 * @param msg            The message to deliver.
 * @param bytesRemaining The bytes yet to be marshaled. This cannot be zero.
 *
 * @return
 *          - AJ_OK if the message partial delivery was successful
 *          - AJ_ERR_SIGNATURE if there are no arguments left to marshal
 *
 */
AJ_EXPORT
AJ_Status AJ_DeliverMsgPartial(AJ_Message* msg, uint32_t bytesRemaining);

/**
 * Marshals one or arguments of basic types such as integers, strings, etc. Container types
 * (structs and arrays) must use AJ_MarshalContainer()
 *
 * @param msg       A pointer to a message currently being marshaled.
 * @param signature The signature of the argument list to marshal.
 * @param ...       Values of the correct size and type per the signature
 *
 * @return
 *          - AJ_OK if the arguments were succesfully marshaled.
 *          - AJ_ERR_RESOURCES if the arguments are too big to marshal into the message buffer
 *          - AJ_ERR_WRITE if there was a write failure
 *          - AJ_ERR_UNEXPECTED if any of the argument types in the signature is not a basic type
 */
AJ_EXPORT
AJ_Status AJ_MarshalArgs(AJ_Message* msg, const char* signature, ...);

/**
 * Initializes a non-container argument of any of the following types:
 *
 * - Scalar values (boolean, bytes, signed and unsigned integer of various sizes, and doubles)
 * - Various string types
 * - Arrays of scalar values
 *
 * @param arg     The argument to initialize
 * @param typeId  The type or element type if the array flag is set
 * @param flags   Indicates if the argument is an array. Valid values are AJ_ARRAY_FLAG and 0
 * @param val     The value to set, a string pointer or an address
 * @param len     The length of the value if flags is AJ_ARRAY_FLAG or 0 otherwise
 *
 * @return  Returns the address of the initialized arg.
 */
AJ_EXPORT
AJ_Arg* AJ_InitArg(AJ_Arg* arg, uint8_t typeId, uint8_t flags, const void* val, size_t len);

/**
 * Marshals a single argument.
 *
 * @param msg     A pointer to a message that has marshaled by an earlier call to AJ_MarshalMsg
 * @param arg     The argument to marshal
 *
 * @return
 *          - AJ_OK if the argument was succesfully marshaled.
 *          - AJ_ERR_RESOURCES if the arg is too big to marshal into the message buffer
 *          - AJ_ERR_WRITE if there was a write failure
 */
AJ_EXPORT
AJ_Status AJ_MarshalArg(AJ_Message* msg, AJ_Arg* arg);

/**
 * Marshals data for a message as raw bytes. The application is responsible for correctly composing
 * the data according to the wire protocol specification including any padding that may be required
 * as required by the alignment rules.
 *
 * The main use of this function is for marshalling message payloads that exceed the size of the
 * network transmit buffer. Before calling this function the application will typically call
 * AJ_DeliverMsgPartial() to establish the total length of the message in the message header.
 *
 * The simple uses cases are marshaling of long strings and arrays of bytes or integers.
 * In these two cases the application must marshal a 32 bit length then then marshal the data.
 *
 * Note that strings must be NUL terminated but NUL is not included in the length.
 *
 * @param msg   A pointer to the message currently being marshaled
 * @param data  The data to marshal
 * @param len   The length of the data
 *
 * @return
 *          - AJ_OK if the data was succesfully marshaled.
 *          - AJ_ERR_WRITE if there was a write failure
 */
AJ_EXPORT
AJ_Status AJ_MarshalRaw(AJ_Message* msg, const void* data, size_t len);

/**
 * Begin marshalling a container argument.
 *
 * @param msg    A pointer to the message currently being marshaled
 * @param arg    The container argument to marshal
 * @param typeId The type of container begin marshaled.
 *
 * @return   Return AJ_Status
 *          - AJ_OK if the argument was succesfully marshaled.
 *          - AJ_ERR_RESOURCES if the arg is too big to marshal into the message buffer
 *          - AJ_ERR_WRITE if there was a write failure
 */
AJ_EXPORT
AJ_Status AJ_MarshalContainer(AJ_Message* msg, AJ_Arg* arg, uint8_t typeId);

/**
 * Finish marshalling a container argument
 *
 * @param msg   A pointer to the message currently being marshaled
 * @param arg   The container argument being marshalled
 *
 * @return   Return AJ_Status
 *          - AJ_OK if the signature is correct
 *          - AJ_ERR_SIGNATURE if the signature is not correctly closed
 */
AJ_EXPORT
AJ_Status AJ_MarshalCloseContainer(AJ_Message* msg, AJ_Arg* arg);

/**
 * Prepare to marshal a variant. The next argument marshalled will be marshalled as a variant.
 *
 * @param msg   A pointer to the message currently being marshaled
 * @param sig   The signature for the variant
 *
 * @return   Return AJ_Status
 *          - AJ_OK if the argument was succesfully marshaled.
 *          - AJ_ERR_RESOURCES if the arg is too big to marshal into the message buffer
 *          - AJ_ERR_WRITE if there was a write failure
 */
AJ_EXPORT
AJ_Status AJ_MarshalVariant(AJ_Message* msg, const char* sig);

#ifdef __cplusplus
}
#endif
/**
 * @}
 */
#endif
