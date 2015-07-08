#ifndef _AJ_HELPER_H
#define _AJ_HELPER_H

/**
 * @file aj_helper.h
 * @defgroup aj_helper Helper Functions
 * @{
 * @file
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

#include "aj_status.h"
#include "aj_bus.h"
#include "aj_about.h"

#ifdef __cplusplus
extern "C" {
#endif

#define AJ_JOINSESSION_REPLY_SUCCESS              1   /**< JoinSession reply: Success */
#define AJ_JOINSESSION_REPLY_NO_SESSION           2   /**< JoinSession reply: Session with given name does not exist */
#define AJ_JOINSESSION_REPLY_UNREACHABLE          3   /**< JoinSession reply: Failed to find suitable transport */
#define AJ_JOINSESSION_REPLY_CONNECT_FAILED       4   /**< JoinSession reply: Connect to advertised address */
#define AJ_JOINSESSION_REPLY_REJECTED             5   /**< JoinSession reply: The session creator rejected the join req */
#define AJ_JOINSESSION_REPLY_BAD_SESSION_OPTS     6   /**< JoinSession reply: Failed due to session option incompatibilities */
#define AJ_JOINSESSION_REPLY_ALREADY_JOINED       7   /**< JoinSession reply: Caller has already joined this session */
#define AJ_JOINSESSION_REPLY_FAILED              10   /**< JoinSession reply: Failed for unknown reason */

/**
 * Callback function prototype for a callback function to handle a method or signal
 *
 * @param msg   The message received
 * @param reply The method reply (NULL if msg is a signal or method reply)
 *
 * @return  Return AJ_Status
 *          - AJ_OK if the message was correctly decoded (and a reply sent in the case of a method call)
 *          - An error status if something went wrong decoding the message
 */
typedef AJ_Status (*MessageHandler)(AJ_Message* msg, AJ_Message* reply);

/**
 * Callback function prototype for a callback function to handle new sessions
 *
 * @param msg  The message received
 *
 * @return  Return uint8_t
 *          - TRUE if the service should allow the session
 *          - FALSE if the service should *NOT* allow the session
 */
typedef uint8_t (*AcceptSessionHandler)(AJ_Message* msg);

/**
 * Callback function prototype to indicate when the Daemon connection status changes
 *
 * @param connected     TRUE if connected, FALSE if disconnected
 */
typedef void (*ConnectionHandler)(uint8_t connected);

/**
 *  Type to describe a mapping of message id to message handler.
 */
typedef struct {
    uint32_t msgid;
    MessageHandler handler;
} MessageHandlerEntry;

/**
 *  Type to describe a mapping of property get/set message id
 *  to get/set handler with context pointer
 */
typedef struct {
    uint32_t msgid;
    AJ_BusPropGetCallback callback;
    void* context;
} PropHandlerEntry;

/**
 *  Type to describe the AllJoyn service configuration
 */
typedef struct {
    const char* daemonName;         /**< Name of a specific daemon service to connect to, NULL for the default name. */
    uint32_t connect_timeout;       /**< How long to spend attempting to connect to the bus */
    uint8_t connected;              /**< Whether the bus attachment is already connected to the daemon bus */
    uint16_t session_port;          /**< The port to bind */
    const char* service_name;       /**< The name being requested */
    uint32_t flags;                 /**< An OR of the name request flags */
    const AJ_SessionOpts* opts;     /**< The session option setting. */

    AJ_AuthPwdFunc password_callback;   /**< The auth password callback */
    uint32_t link_timeout;              /**< The daemon connection's link timeout */

    AcceptSessionHandler acceptor;          /**< The AcceptSession callback */
    ConnectionHandler connection_handler;   /**< A callback for when the daemon connection status changes */

    const MessageHandlerEntry* message_handlers;    /**< An array of message handlers */
    const PropHandlerEntry* prop_handlers;          /**< An array of property get/set handlers */

} AllJoynConfiguration;

/**
 * Helper function that connects to a bus initializes an AllJoyn service.
 *
 * @param bus       The bus attachment
 * @param config    The AllJoyn configuration object
 *
 * @return AJ_OK if service was successfully run to completion.
 */
AJ_Status AJ_RunAllJoynService(AJ_BusAttachment* bus, AllJoynConfiguration* config);

/**
 * Callback function prototype for a timer function callback
 *
 * @param context   The context pointer passed in to AJ_SetTimer
 */
typedef void (*TimeoutHandler)(void* context);

/**
 *  Start a timer
 *
 * @param relative_time The time (relative to now) when the timer should first go off
 * @param handler       The callback to execute after <relative_time> milliseconds
 * @param context       The context pointer that will be passed into the handler
 * @param repeat        If nonzero, repeat this timer every <repeat> msec
 *
 * @return The id of the new timer, which can be used to cancel it later
 *          0 if timer was not set.
 */
uint32_t AJ_SetTimer(uint32_t relative_time, TimeoutHandler handler, void* context, uint32_t repeat);

/**
 *  Cancel the timer specified
 *
 * @param id    The id of the timer to cancel (returned by AJ_SetTimer)
 */
void AJ_CancelTimer(uint32_t id);

/**
 * Helper function that connects to a bus initializes an AllJoyn service.
 *
 * @param bus          The bus attachment
 * @param daemonName   Name of a specific daemon service to connect to, NULL for the default name.
 * @param timeout      How long to spend attempting to connect to the bus
 * @param connected    Whether the bus attachment is already connected to the daemon bus
 * @param port         The port to bind
 * @param name         The name being requested
 * @param flags        An OR of the name request flags
 * @param opts         The session option setting.
 *
 * @return AJ_OK if service was successfully started.
 */
AJ_EXPORT
AJ_Status AJ_StartService(AJ_BusAttachment* bus,
                          const char* daemonName,
                          uint32_t timeout,
                          uint8_t connected,
                          uint16_t port,
                          const char* name,
                          uint32_t flags,
                          const AJ_SessionOpts* opts);

/**
 * @deprecated
 * Initializes an AllJoyn client and connect to a service. Note that this function is deprecated
 * and AJ_StartClientByName() should be used instead.
 *
 * @param bus            The bus attachment
 * @param daemonName     Name of a specific daemon service to connect to, NULL for the default name.
 * @param timeout        How long to spend attempting to find a remote service to connect to.
 * @param connected      Whether the bus attachment is already connected to the daemon bus.
 * @param name           The name of the service to connect to.
 * @param port           The service port to connect to.
 * @param[out] sessionId The session id returned if the connection was successful
 * @param opts           The session option setting.
 *
 * @return AJ_OK if connection was successfully established
 */
AJ_EXPORT
AJ_Status AJ_StartClient(AJ_BusAttachment* bus,
                         const char* daemonName,
                         uint32_t timeout,
                         uint8_t connected,
                         const char* name,
                         uint16_t port,
                         uint32_t* sessionId,
                         const AJ_SessionOpts* opts);

/**
 * Initializes an AllJoyn client and connect to a service
 *
 * @param bus            The bus attachment
 * @param daemonName     Name of a specific daemon service to connect to, NULL for the default name.
 * @param timeout        How long to spend attempting to find a remote service to connect to.
 * @param connected      Whether the bus attachment is already connected to the daemon bus.
 * @param name           The name of the service to connect to.
 * @param port           The service port to connect to.
 * @param[out] sessionId The session id returned if the connection was successful
 * @param opts           The session option setting.
 * @param[out] fullName  This buffer passed in will be filled with the full service name if the connection
 *                       was successful. The buffer should be of size AJ_MAX_SERVICE_NAME_SIZE or buffer overflow may occur.
 *
 * @return AJ_OK if connection was successfully established
 */
AJ_EXPORT
AJ_Status AJ_StartClientByName(AJ_BusAttachment* bus,
                               const char* daemonName,
                               uint32_t timeout,
                               uint8_t connected,
                               const char* name,
                               uint16_t port,
                               uint32_t* sessionId,
                               const AJ_SessionOpts* opts,
                               char* fullName);

/**
 * Initialize an AllJoyn client, discover service by interface name, and connect.
 *
 * @param bus             The bus attachment
 * @param daemonName      Name of a specific daemon service to connect to, NULL for the default name.
 * @param timeout         How long to spend attempting to find a remote service to connect to.
 * @param connected       Whether the bus attachment is already connected to the daemon bus.
 * @param interfaces      Find a service that implements these interface(s) (NULL-terminated list of names)
 * @param[out] sessionId  The session id if the connection was successful
 * @param[out] uniqueName The unique name of the service if the connection was successful (supply array of size AJ_MAX_NAME_SIZE+1)
 * @param opts            The session option setting.
 *
 * @return AJ_OK if connection was successfully established
 */
AJ_EXPORT
AJ_Status AJ_StartClientByInterface(AJ_BusAttachment* bus,
                                    const char* daemonName,
                                    uint32_t timeout,
                                    uint8_t connected,
                                    const char** interfaces,
                                    uint32_t* sessionId,
                                    char* uniqueName,
                                    const AJ_SessionOpts* opts);

#ifdef ANNOUNCE_BASED_DISCOVERY
/**
 * Initialize an AllJoyn client, discover service by peer description, and connect.
 *
 * @param bus               The bus attachment
 * @param daemonName        Name of a specific daemon service to connect to, NULL for the default name.
 * @param timeout           How long to spend attempting to find a remote service to connect to.
 * @param connected         Whether the bus attachment is already connected to the daemon bus.
 * @param peerDesc          Find a peer that matched the description i.e. that implements thes interface(s) and register match callbacks
 * @param port              The service port to connect to. If value is 0 use the About port in the Announcement.
 * @param[out] sessionId    The session id if the connection was successful
 * @param[out] uniqueName   The unique name of the service if the connection was successful (supply array of size AJ_MAX_NAME_SIZE+1)
 * @param opts              The session option setting.
 *
 * @return AJ_OK if connection was successfully established
 *
 * This function is experimental, and as such has not been fully tested.
 * Please help make it more solid by contributing fixes if you find issues.
 */
AJ_EXPORT
AJ_Status AJ_StartClientByPeerDescription(AJ_BusAttachment* bus,
                                          const char* daemonName,
                                          uint32_t timeout,
                                          uint8_t connected,
                                          const AJ_AboutPeerDescription* peerDesc,
                                          uint16_t port,
                                          uint32_t* sessionId,
                                          char* uniqueName,
                                          const AJ_SessionOpts* opts);
#endif

#ifdef __cplusplus
}
#endif
/**
 * @}
 */
#endif /* _AJ_HELPER_H */
