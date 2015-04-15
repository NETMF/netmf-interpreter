#ifndef _AJ_BUS_H
#define _AJ_BUS_H

/**
 * @file aj_bus.h
 * @defgroup aj_bus Bus Attachment
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
#include "aj_net.h"
#include "aj_status.h"
#include "aj_util.h"
#include "aj_auth_listener.h"

#ifdef __cplusplus
extern "C" {
#endif

/**
 * Forward declarations
 */
typedef struct _AJ_Message AJ_Message;
typedef struct _AJ_Arg AJ_Arg;

/**
 * Callback function prototype for requesting a password or pincode from an application.
 *
 * @param buffer  The buffer to receive the password.
 * @param bufLen  The size of the buffer
 *
 * @return  Returns the length of the password. If the length is zero this will be
 *          treated as a rejected password request.
 */
typedef uint32_t (*AJ_AuthPwdFunc)(uint8_t* buffer, uint32_t bufLen);

/**
 * Callback function prototype for authentication listener
 *
 * @param authmechansim The authentication mechanism used
 * @param command The listener command
 * @param creds The credentials
 *
 * @return  Returns true if authorized; false otherwise.
 */
typedef AJ_Status (*AJ_AuthListenerFunc)(uint32_t authmechanism, uint32_t command, AJ_Credential* creds);

/**
 * Type for a bus attachment
 */
typedef struct _AJ_BusAttachment {
    uint16_t aboutPort;          /**< The port to use in announcements */
    char uniqueName[16];         /**< The unique name returned by the hello message */
    AJ_NetSocket sock;           /**< Abstracts a network socket */
    uint32_t serial;             /**< Next outgoing message serial number */
    AJ_AuthPwdFunc pwdCallback;  /**< Callback for obtaining passwords */
    AJ_AuthListenerFunc authListenerCallback;  /**< Callback for obtaining passwords */
    uint32_t* suites;              /**< Supported cipher suites */
    size_t numsuites;             /**< Number of supported cipher suites */
} AJ_BusAttachment;

/**
 * Get the unique name for the bus
 *
 * @return  The unique name or NULL if the bus is not connected.
 */
const char* AJ_GetUniqueName(AJ_BusAttachment* bus);


#define AJ_NAME_REQ_ALLOW_REPLACEMENT 0x01  /**< Allow others to take ownership of this name */
#define AJ_NAME_REQ_REPLACE_EXISTING  0x02  /**< Attempt to take ownership of name if already taken */
#define AJ_NAME_REQ_DO_NOT_QUEUE      0x04  /**< Fail if name cannot be immediately obtained */

/**
 * Make a method call to request a well known name
 *
 * @param bus         The bus attachment
 * @param name        The name being requested
 * @param flags       An XOR of the name request flags
 *
 * @return
 *         - AJ_OK if the request was sent
 *         - An error status otherwise
 */
AJ_EXPORT
AJ_Status AJ_BusRequestName(AJ_BusAttachment* bus, const char* name, uint32_t flags);

#define AJ_TRANSPORT_NONE      0x0000    /**< no transports */
#define AJ_TRANSPORT_ALL       0xFFFF    /**< ALL Possible transports including EXPERIMENTAL ones */
#define AJ_TRANSPORT_LOCAL     0x0001    /**< Local (same device) transport */
#define AJ_TRANSPORT_BLUETOOTH 0x0002    /**< Bluetooth transport */
#define AJ_TRANSPORT_WLAN      0x0004    /**< Wireless local-area network transport */
#define AJ_TRANSPORT_WWAN      0x0008    /**< Wireless wide-area network transport */
#define AJ_TRANSPORT_LAN       0x0010    /**< Wired local-area network transport */

#define AJ_TRANSPORT_TCP       0x0004    /**< Transport using TCP (same thing as WLAN that implies TCP) */
#define AJ_TRANSPORT_UDP       0x0100    /**< Transport using the AllJoyn Reliable Datagram Protocol (flavor of reliable UDP) */
#define AJ_TRANSPORT_IP        (AJ_TRANSPORT_TCP | AJ_TRANSPORT_UDP) /**< Let the system decide which to use */

#define AJ_TRANSPORT_ANY       (AJ_TRANSPORT_ALL)   /**< ANY non-EXPERIMENTAL transport */

/**
 * Make a method call to release a previously requested well known name.
 *
 * @param bus         The bus attachment
 * @param name        The name being released
 *
 * @return
 *         - AJ_OK if the request was sent
 *         - An error status otherwise
 */
AJ_EXPORT
AJ_Status AJ_BusReleaseName(AJ_BusAttachment* bus, const char* name);

#define AJ_BUS_START_ADVERTISING 0      /**< start advertising */
#define AJ_BUS_STOP_ADVERTISING  1      /**< stop advertising */

/**
 * Make a method call to start or stop advertising a name
 *
 * @param bus           The bus attachment
 * @param name          The name to be advertised
 * @param transportMask Restricts the transports the advertisement will be stopped/started on.
 * @param op            Either AJ_BUS_START_ADVERTISING or AJ_BUS_STOP_ADVERTISING
 * @param flags         Flags to pass into AJ_MarshalMsg
 *
 * @return  Return AJ_Status
 *         - AJ_OK if the request was sent
 *         - An error status otherwise
 */
AJ_EXPORT
AJ_Status AJ_BusAdvertiseName(AJ_BusAttachment* bus, const char* name, uint16_t transportMask, uint8_t op, uint8_t flags);

#define AJ_BUS_START_FINDING 0       /**< Start finding advertised name */
#define AJ_BUS_STOP_FINDING  1       /**< Stop finding advertised name */

#define AJ_FIND_NAME_STARTED    0x1   /**< Started to find the name as requested */
#define AJ_FIND_NAME_ALREADY    0x2   /**< Was already finding the requested name */
#define AJ_FIND_NAME_FAILURE    0x3   /**< Attempt to find the name failed */

/**
 * Register interest in a well-known name prefix for the purpose of discovery.
 *
 * @param  bus          The bus attachment
 * @param  namePrefix   Well-known name prefix that application is interested in receiving
 *                      FoundAdvertisedName notifications about.
 * @param op            Either AJ_BUS_START_FINDING or AJ_BUS_STOP_FINDING
 * @param flags         Flags being passed in
 *
 * @return  Return AJ_Status
 *         - AJ_OK if the request was sent
 *         - An error status otherwise
 */
AJ_EXPORT
AJ_Status AJ_BusFindAdvertisedName(AJ_BusAttachment* bus, const char* namePrefix, uint8_t op);

/**
 * Register interest in a well-known name prefix for the purpose of discovery.
 *
 * @param  bus          The bus attachment
 * @param  namePrefix   Well-known name prefix that application is interested in receiving
 * @param  transport    Transports by which for well-known name discovery
 *                      FoundAdvertisedName notifications about.
 * @param op            Either AJ_BUS_START_FINDING or AJ_BUS_STOP_FINDING
 *
 * @return  Return AJ_Status
 *         - AJ_OK if the request was sent
 *         - An error status otherwise
 */
AJ_EXPORT
AJ_Status AJ_BusFindAdvertisedNameByTransport(AJ_BusAttachment* bus, const char* namePrefix, uint16_t transport, uint8_t op);

#define AJ_SESSION_PROXIMITY_ANY          0xFF   /**< No proximity restrictions */
#define AJ_SESSION_PROXIMITY_PHYSICAL     0x01   /**< Limit to services that are physically proximal */
#define AJ_SESSION_PROXIMITY_NETWORK      0x02   /**< Allow services that are on the same subnet */

#define AJ_SESSION_TRAFFIC_MESSAGES       0x01   /**< Session carries message traffic */
#define AJ_SESSION_TRAFFIC_RAW_UNRELIABLE 0x02   /**< Not supported by this implementation */
#define AJ_SESSION_TRAFFIC_RAW_RELIABLE   0x04   /**< Not supported by this implementation */

#define AJ_SESSION_PORT_ANY                       0x00   /**< Use a daemon assigned ephemeral session port */

/**
 * Type for describing session options
 */
typedef struct _AJ_SessionOpts {
    uint8_t traffic;            /**< traffic type */
    uint8_t proximity;          /**< proximity */
    uint16_t transports;        /**< allowed transports */
    uint32_t isMultipoint;      /**< multi-point session capable */
} AJ_SessionOpts;

/**
 * Make a method call to bind a session port.
 *
 * @param bus          The bus attachment
 * @param port         The port to bind, if AJ_SESSION_PORT_ANY is passed in, the daemon
 *                     will assign an ephemeral session port
 * @param opts         Options for establishing a session, if NULL defaults are used.
 * @param flags        Flags to pass into AJ_MarshalMsg
 *
 * @return  Return AJ_Status
 *         - AJ_OK if the request was sent
 *         - An error status otherwise
 */
AJ_EXPORT
AJ_Status AJ_BusBindSessionPort(AJ_BusAttachment* bus, uint16_t port, const AJ_SessionOpts* opts, uint8_t flags);

/**
 * Make a method call to unbind a session port.
 *
 * @param bus          The bus attachment
 * @param port         The port the session is associated with
 * @param flags        The flags associated with binding a port
 *
 * @return  Return AJ_Status
 *         - AJ_OK if the request was sent
 *         - An error status otherwise
 */
AJ_EXPORT
AJ_Status AJ_BusUnbindSession(AJ_BusAttachment* bus, uint16_t port);

/**
 * Make a method call to cancel a sessionless signal
 *
 * @param bus          The bus attachment
 * @param serialNum    The serial number of the signal to cancel
 *
 * @return
 *         - AJ_OK if the request was sent
 *         - An error status otherwise
 */
AJ_EXPORT
AJ_Status AJ_BusCancelSessionless(AJ_BusAttachment* bus, uint32_t serialNum);

/**
 * Possible response codes for AJ_BusCancelSessionless
 */
#define AJ_CANCELSESSIONLESS_REPLY_SUCCESS     1       /**< Cancel Sessionless: reply success */
#define AJ_CANCELSESSIONLESS_REPLY_NO_SUCH_MSG 2       /**< Cancel Sessionless: no such msg */
#define AJ_CANCELSESSIONLESS_REPLY_NOT_ALLOWED 3       /**< Cancel Sessionless: not allowed */
#define AJ_CANCELSESSIONLESS_REPLY_FAILED      4       /**< Cancel Sessionless: reply failed */

/**
 * Send a reply to an accept session method call
 *
 * @param msg         The AcceptSession method call
 * @param accept      TRUE to accept the session, FALSE to reject it.
 *
 * @return  Return AJ_Status
 *          - AJ_OK if the message was succesfully delivered
 *          - AJ_ERR_MARSHAL if the message arguments were incompletely marshaled
 */
AJ_EXPORT
AJ_Status AJ_BusReplyAcceptSession(AJ_Message* msg, uint32_t accept);

/**
 * Make a method call join a session.
 *
 * @param bus          The bus attachment
 * @param sessionHost  Bus name of attachment that is hosting the session to be joined.
 * @param port         The session port to join
 * @param opts         Options for establishing a session, if NULL defaults are used.
 *
 * @return  Return AJ_Status
 *         - AJ_OK if the request was sent
 *         - An error status otherwise
 */
AJ_EXPORT
AJ_Status AJ_BusJoinSession(AJ_BusAttachment* bus, const char* sessionHost, uint16_t port, const AJ_SessionOpts* opts);

/**
 * Make a method call join a session.
 *
 * @param bus          The bus attachment
 * @param sessionId    The Id of the session joined
 *
 * @return  Return AJ_Status
 *         - AJ_OK if the request was sent
 *         - An error status otherwise
 */
AJ_EXPORT
AJ_Status AJ_BusLeaveSession(AJ_BusAttachment* bus, uint32_t sessionId);

#define AJ_BUS_SIGNAL_ALLOW  0     /**< Allow signals */
#define AJ_BUS_SIGNAL_DENY   1     /**< Deny signals */

/**
 * Add a SIGNAL match rule. A rule must be added for every non-session signal that the application
 * is interested in receiving.
 *
 * @param bus           The bus attachment
 * @param ruleString    Match rule to be added/removed
 * @param rule          Either AJ_BUS_SIGNAL_ALLOW or AJ_BUS_SIGNAL_DENY
 *
 * @return  Return AJ_Status
 *         - AJ_OK if the request was sent
 *         - An error status otherwise
 */
AJ_EXPORT
AJ_Status AJ_BusSetSignalRule(AJ_BusAttachment* bus, const char* ruleString, uint8_t rule);

/**
 * Add a SIGNAL match rule. A rule must be added for every non-session signal that the application
 * is interested in receiving.
 *
 * @param bus             The bus attachment
 * @param ruleString      Match rule to be added/removed
 * @param rule            Either AJ_BUS_SIGNAL_ALLOW or AJ_BUS_SIGNAL_DENY
 * @param flags         Flags associated with the new rule
 * @param[out] serialNum  The serial number of the method call
 *
 * @return  Return AJ_Status
 *         - AJ_OK if the request was sent
 *         - An error status otherwise
 */
AJ_Status AJ_BusSetSignalRuleSerial(AJ_BusAttachment* bus, const char* ruleString, uint8_t rule, uint8_t flags, uint32_t* serialNum);

/**
 * Add a SIGNAL match rule. A rule must be added for every non-session signal that the application
 * is interested in receiving.
 *
 * @param bus           The bus attachment
 * @param signalName    The name of the signal.
 * @param interfaceName The name of the interface.
 * @param rule          Either AJ_BUS_SIGNAL_ALLOW or AJ_BUS_SIGNAL_DENY
 *
 * @return  Return AJ_Status
 *         - AJ_OK if the request was sent
 *         - An error status otherwise
 */
AJ_EXPORT
AJ_Status AJ_BusAddSignalRule(AJ_BusAttachment* bus, const char* signalName, const char* interfaceName, uint8_t rule);

/**
 * Add a SIGNAL match rule. A rule must be added for every non-session signal that the application
 * is interested in receiving.
 *
 * @param bus           The bus attachment
 * @param ruleString    Match rule to be added/removed
 * @param rule          Either AJ_BUS_SIGNAL_ALLOW or AJ_BUS_SIGNAL_DENY
 * @param flags         Flags associated with the new rule
 *
 * @return  Return AJ_Status
 *         - AJ_OK if the request was sent
 *         - An error status otherwise
 */
AJ_EXPORT
AJ_Status AJ_BusSetSignalRuleFlags(AJ_BusAttachment* bus, const char* ruleString, uint8_t rule, uint8_t flags);

#define AJ_SETLINKTIMEOUT_SUCCESS          1   /**< SetLinkTimeout reply: Success */
#define AJ_SETLINKTIMEOUT_NO_DEST_SUPPORT  2   /**< SetLinkTimeout reply: Destination endpoint does not support link monitoring */
#define AJ_SETLINKTIMEOUT_NO_SESSION       3   /**< SetLinkTimeout reply: Session with given id does not exist */
#define AJ_SETLINKTIMEOUT_FAILED           4   /**< SetLinkTimeout reply: Failed */

/**
 * Set a link timeout on a session. This will ensure that a session lost signal is reported by the
 * daemon within the specified timeout period if the session peer unexpectedly leaves the bus, for
 * example because the peer moved out of range. The application may want to handle to reply to
 * this method call to determine if the request succeeded.
 *
 * @param bus          The bus attachment
 * @param sessionId    The session id for the session to set the timeout on.
 * @param linkTimeout  The link timeout value to set specified in milliseconds
 *
 * @return  Return AJ_Status
 *         - AJ_OK if the link timwout request was sent
 *         - An error status otherwise
 */
AJ_EXPORT
AJ_Status AJ_BusSetLinkTimeout(AJ_BusAttachment* bus, uint32_t sessionId, uint32_t linkTimeout);
/*
 * Use to remove a member from the session
 *
 * @param bus           The bus attachment your on
 * @param sessionId     The session ID for the session your removing the member from
 * @param member        The unique ID of the member you wish to remove from the session
 *
 * @return  Return AJ_Status
 *          - AJ_OK if the member was removed successfully
 *          - An error if not removed
 */
AJ_EXPORT
AJ_Status AJ_BusRemoveSessionMember(AJ_BusAttachment* bus, uint32_t sessionId, const char* member);
/*
 * Is the bus name reachable?
 *
 * @param bus           The bus attachment
 * @param name          The unique or well-known name of the object to ping
 * @param timeout       Timeout (in milliseconds) to wait for a reply
 *
 * @return  Return AJ_Status
 *          - AJ_OK if ping was sent
 *          - An error status otherwise
 */
AJ_EXPORT
AJ_Status AJ_BusPing(AJ_BusAttachment* bus, const char* name, uint32_t timeout);

#define AJ_PING_SUCCESS                    1   /**< Ping reply: Success */
#define AJ_PING_FAILED                     2   /**< Ping reply: Failed */
#define AJ_PING_TIMEOUT                    3   /**< Ping reply: Timed out */

/**
 * Invoke a built-in handler for standard bus messages. Signals passed to this function that are
 * not bus messages are silently ignored. Method calls passed to this function that are not
 * recognized bus messages are rejected with an error response.
 *
 * Method calls that currently have built-in handlers are:
 *
 *  - AJ_BUS_METHOD_PING
 *  - AJ_BUS_METHOD_GET_MACHINE_ID
 *  - AJ_BUS_METHOD_INTROSPECT
 *  - AJ_BUS_METHOD_EXCHANGE_GUIDS
 *  - AJ_BUS_METHOD_GEN_SESSION_KEY
 *  - AJ_BUS_METHOD_EXCHANGE_GROUP_KEYS
 *  - AJ_BUS_METHOD_AUTH_CHALLENGE
 *
 * @param msg     The message to handle
 *
 * @return  Return AJ_Status
 *         - AJ_OK if the message was handled or ingored
 */
AJ_EXPORT
AJ_Status AJ_BusHandleBusMessage(AJ_Message* msg);

/**
 * Set a callback for returning passwords for peer authentication. Authentication is not enabled
 * until a password callback function has been set.
 *
 * @param bus          The bus attachment struct
 * @param pwdCallback  The password callback function.
 */
AJ_EXPORT
void AJ_BusSetPasswordCallback(AJ_BusAttachment* bus, AJ_AuthPwdFunc pwdCallback);

/**
 * Set a callback for auth listener
 * until a password callback function has been set.
 *
 * @param bus          The bus attachment struct
 * @param authListenerCallback  The auth listener callback function.
 */
void AJ_BusSetAuthListenerCallback(AJ_BusAttachment* bus, AJ_AuthListenerFunc authListenerCallback);

/**
 * Callback function prototype for the function called when an authentication completes or fails.
 *
 * @param context   The context provided when AJ_PeerAuthenticate() was called.
 * @param status    A status code indicating if the authentication was succesful
 *                  - AJ_OK indicates the authentication succeeded
 *                  - AJ_ERR_SECURITY indicates the authentication failed
 *                  - AJ_ERR_TIMEOUT indciates the authentication timed-out
 */
typedef void (*AJ_BusAuthPeerCallback)(const void* context, AJ_Status status);

/**
 * Initiate a secure connection to a remote peer authenticating if necessary.
 *
 * @param bus            The bus attachment
 * @param peerBusName    The bus name of the remove peer to secure.
 * @param callback       A function to be called when the authentication completes
 * @param cbContext      A caller provided context to pass to the callback function
 *
 * @return  Return AJ_Status
 *         - AJ_OK if the request was sent
 *         - An error status otherwise
 */
AJ_Status AJ_BusAuthenticatePeer(AJ_BusAttachment* bus, const char* peerBusName, AJ_BusAuthPeerCallback callback, void* cbContext);

/**
 * Callback function prototype for a callback function to GET a property. All this function has to
 * do is marshal the property value.
 *
 * @param replyMsg  The GET_PROPERTY or GET_ALL_PROPERTIES reply message
 * @param propId    The property identifier
 * @param context   The caller provided context that was passed into AJ_BusPropGet()
 *
 * @return  Return AJ_Status
 *          - AJ_OK if the property was read and marshaled
 *          - An error status if the property could not be returned for any reason.
 */
typedef AJ_Status (*AJ_BusPropGetCallback)(AJ_Message* replyMsg, uint32_t propId, void* context);

/**
 * Helper function that provides all the boilerplate for responding to a GET_PROPERTY. All the
 * application has to do is marshal the property value.
 *
 * @param msg       An unmarshalled GET_PROPERTY message
 * @param callback  The function called to request the application to marshal the property value.
 * @param context   A caller provided context that is passed into the callback function
 *
 * @return  Return AJ_Status
 */
AJ_EXPORT
AJ_Status AJ_BusPropGet(AJ_Message* msg, AJ_BusPropGetCallback callback, void* context);

/**
 * Helper function that provides all the boilerplate for responding to a GET_ALL_PROPERTIES. All the
 * application has to do is marshal each of the property values.
 *
 * @param msg       An unmarshalled GET_ALL_PROPERTIES message
 * @param callback  The function called to request the application to marshal the property value.
 * @param context   A caller provided context that is passed into the callback function
 *
 * @return  Return AJ_Status
 */
AJ_EXPORT
AJ_Status AJ_BusPropGetAll(AJ_Message* msg, AJ_BusPropGetCallback callback, void* context);

/**
 * Callback function prototype for a callback function to SET an application property. All this
 * function has to do is unmarshal the property value.
 *
 * @param replyMsg  The SET_PROPERTY reply message
 * @param propId    The property identifier
 * @param context   The caller provided context that was passed into AJ_BusPropSet()
 *
 * @return  Return AJ_Status
 *          - AJ_OK if the property was unmarshaled
 *          - An error status if the property could not be set for any reason.
 */
typedef AJ_Status (*AJ_BusPropSetCallback)(AJ_Message* replyMsg, uint32_t propId, void* context);

/**
 * Helper function that provides all the boilerplate for responding to a SET_PROPERTY. All the
 * application has to do is unmarshal the property value.
 *
 * @param msg       An unmarshalled SET_PROPERTY message
 * @param callback  The function called to request the application to marshal the property value.
 * @param context   A caller provided context that is passed into the callback function
 *
 * @return  Return AJ_Status
 */
AJ_EXPORT
AJ_Status AJ_BusPropSet(AJ_Message* msg, AJ_BusPropSetCallback callback, void* context);

/**
 * Function to specify which authentication suites are available.
 *
 * @param bus       The bus attachment struct
 * @param suites    The authentication suites to enable
 * @param numsuites The number of authentication suites
 *
 * @return  Return AJ_Status
 */
AJ_Status AJ_BusEnableSecurity(AJ_BusAttachment* bus, const uint32_t* suites, size_t numsuites);

#ifdef __cplusplus
}
#endif
/**
 * @}
 */
#endif
