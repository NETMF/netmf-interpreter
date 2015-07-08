#ifndef _AJ_STD_H
#define _AJ_STD_H
/**
 * @file aj_std.h
 * @defgroup aj_std AllJoyn Standard Object Definitions
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
#include "aj_introspect.h"

/**
 * Identifiers for standard methods and signals. These are the values returned by
 * AJ_IdentifyMessage() for correctly formed method and signal messages.
 *
 * The first value is the index into the object array.
 * The second value is the index into the interfaces array for the object.
 * The third value is the index into the members array of the interface.
 */


/*
 * Members of the /org/freedesktop/DBus interface org.freedesktop.DBus
 */
#define AJ_METHOD_HELLO                AJ_BUS_MESSAGE_ID(0, 0, 0)    /**< method for hello */
#define AJ_SIGNAL_NAME_OWNER_CHANGED   AJ_BUS_MESSAGE_ID(0, 0, 1)    /**< signal for name owner changed */
#define AJ_SIGNAL_NAME_ACQUIRED        AJ_BUS_MESSAGE_ID(0, 0, 2)    /**< signal for name acquired */
#define AJ_SIGNAL_NAME_LOST            AJ_BUS_MESSAGE_ID(0, 0, 3)    /**< signal for name lost */
#define AJ_SIGNAL_PROPS_CHANGED        AJ_BUS_MESSAGE_ID(0, 0, 4)    /**< signal for props changed */
#define AJ_METHOD_REQUEST_NAME         AJ_BUS_MESSAGE_ID(0, 0, 5)    /**< method for request name */
#define AJ_METHOD_ADD_MATCH            AJ_BUS_MESSAGE_ID(0, 0, 6)    /**< method for add match */
#define AJ_METHOD_REMOVE_MATCH         AJ_BUS_MESSAGE_ID(0, 0, 7)    /**< method for remove match */
#define AJ_METHOD_RELEASE_NAME         AJ_BUS_MESSAGE_ID(0, 0, 8)    /**< method for release name */
#define AJ_METHOD_NAME_HAS_OWNER       AJ_BUS_MESSAGE_ID(0, 0, 9)    /**< method for name has owner */

/*
 * Members of /org/alljoyn/Bus interface org.alljoyn.Bus
 */
#define AJ_SIGNAL_SESSION_LOST                  AJ_BUS_MESSAGE_ID(1, 0, 0)    /**< signal for session lost */
#define AJ_SIGNAL_FOUND_ADV_NAME                AJ_BUS_MESSAGE_ID(1, 0, 1)    /**< signal for found advertising name */
#define AJ_SIGNAL_LOST_ADV_NAME                 AJ_BUS_MESSAGE_ID(1, 0, 2)    /**< signal for lost advertising name */
#define AJ_SIGNAL_MP_SESSION_CHANGED            AJ_BUS_MESSAGE_ID(1, 0, 3)    /**< signal for mp session changed */
#define AJ_METHOD_ADVERTISE_NAME                AJ_BUS_MESSAGE_ID(1, 0, 4)    /**< method for advertise name */
#define AJ_METHOD_CANCEL_ADVERTISE              AJ_BUS_MESSAGE_ID(1, 0, 5)    /**< method for cancel advertise */
#define AJ_METHOD_FIND_NAME                     AJ_BUS_MESSAGE_ID(1, 0, 6)    /**< method for find name */
#define AJ_METHOD_CANCEL_FIND_NAME              AJ_BUS_MESSAGE_ID(1, 0, 7)    /**< method for cancel find name */
#define AJ_METHOD_BIND_SESSION_PORT             AJ_BUS_MESSAGE_ID(1, 0, 8)    /**< method for bind session port */
#define AJ_METHOD_UNBIND_SESSION                AJ_BUS_MESSAGE_ID(1, 0, 9)    /**< method for unbind session */
#define AJ_METHOD_JOIN_SESSION                  AJ_BUS_MESSAGE_ID(1, 0, 10)   /**< method for join session */
#define AJ_METHOD_LEAVE_SESSION                 AJ_BUS_MESSAGE_ID(1, 0, 11)   /**< method for leave session */
#define AJ_METHOD_CANCEL_SESSIONLESS            AJ_BUS_MESSAGE_ID(1, 0, 12)   /**< method for cancel sessionless */
#define AJ_METHOD_FIND_NAME_BY_TRANSPORT        AJ_BUS_MESSAGE_ID(1, 0, 13)   /**< method for find name by specific transports */
#define AJ_METHOD_CANCEL_FIND_NAME_BY_TRANSPORT AJ_BUS_MESSAGE_ID(1, 0, 14)   /**< method for cancel find name by specific transports */
#define AJ_METHOD_SET_LINK_TIMEOUT              AJ_BUS_MESSAGE_ID(1, 0, 15)   /**< method for setting the link timeout for a session */
#define AJ_METHOD_REMOVE_SESSION_MEMBER         AJ_BUS_MESSAGE_ID(1, 0, 16)   /**< method for removing a member in a session */
#define AJ_SIGNAL_SESSION_LOST_WITH_REASON      AJ_BUS_MESSAGE_ID(1, 0, 17)   /**< signal for session lost with a reason */
#define AJ_METHOD_BUS_PING                      AJ_BUS_MESSAGE_ID(1, 0, 18)   /**< method for ping */
#define AJ_METHOD_BUS_SET_IDLE_TIMEOUTS         AJ_BUS_MESSAGE_ID(1, 0, 19)   /**< method for set idle timeouts */
#define AJ_METHOD_BUS_SIMPLE_HELLO              AJ_BUS_MESSAGE_ID(1, 0, 20)   /**< Simple Hello, similar to BusHello */

/*
 * Members of /org/alljoyn/Bus/Peer interface org.alljoyn.Bus.Peer.Session
 */
#define AJ_METHOD_ACCEPT_SESSION       AJ_BUS_MESSAGE_ID(2, 0, 0)    /**< method for accept session */
#define AJ_SIGNAL_SESSION_JOINED       AJ_BUS_MESSAGE_ID(2, 0, 1)    /**< signal for session joined */

/*
 * Members of /org/alljoyn/Bus/Peer interface org.alljoyn.Bus.Peer.Authentication
 */
#define AJ_METHOD_EXCHANGE_GUIDS       AJ_BUS_MESSAGE_ID(2, 1, 0)    /**< method for exchange guids */
#define AJ_METHOD_GEN_SESSION_KEY      AJ_BUS_MESSAGE_ID(2, 1, 1)    /**< method for generate session key */
#define AJ_METHOD_EXCHANGE_GROUP_KEYS  AJ_BUS_MESSAGE_ID(2, 1, 2)    /**< method for exchange group keys */
#define AJ_METHOD_AUTH_CHALLENGE       AJ_BUS_MESSAGE_ID(2, 1, 3)    /**< method for auth challenge */
#define AJ_METHOD_EXCHANGE_SUITES      AJ_BUS_MESSAGE_ID(2, 1, 4)    /**< method for exchange suites*/
#define AJ_METHOD_KEY_EXCHANGE         AJ_BUS_MESSAGE_ID(2, 1, 5)    /**< method for key exchange*/
#define AJ_METHOD_KEY_AUTHENTICATION   AJ_BUS_MESSAGE_ID(2, 1, 6)    /**< method for authenticating key exchange*/

/*
 * Members of interface org.freedesktop.DBus.Introspectable
 *
 * Note - If you use this message id explicitly to construct a method call it will always introspect
 * the root object.
 */
#define AJ_METHOD_INTROSPECT           AJ_BUS_MESSAGE_ID(3, 0, 0)    /**< method for introspect */

/*
 * Members of the interface org.freedesktop.DBus.Peer
 */
#define AJ_METHOD_PING                 AJ_BUS_MESSAGE_ID(3, 1, 0)    /**< method for ping */
#define AJ_METHOD_GET_MACHINE_ID       AJ_BUS_MESSAGE_ID(3, 1, 1)    /**< method for get machine id */

/*
 * Members of the interface org.allseen.Introspectable
 */
#define AJ_METHOD_GET_DESCRIPTION_LANG AJ_BUS_MESSAGE_ID(3, 2, 0)    /**< method for get description langauges */
#define AJ_METHOD_INTROSPECT_WITH_DESC AJ_BUS_MESSAGE_ID(3, 2, 1)    /**< method for introspect with descriptions */

/*
 * Members of /org/alljoyn/Daemon interface org.alljoyn.Daemon
 */
#define AJ_SIGNAL_PROBE_REQ            AJ_BUS_MESSAGE_ID(4, 0, 0)    /**< signal for link probe request */
#define AJ_SIGNAL_PROBE_ACK            AJ_BUS_MESSAGE_ID(4, 0, 1)    /**< signal for link probe acknowledgement */

/*
 * Members of interface org.alljoyn.About
 */
#define AJ_METHOD_ABOUT_GET_PROP                AJ_BUS_MESSAGE_ID(5, 0, AJ_PROP_GET)
#define AJ_METHOD_ABOUT_SET_PROP                AJ_BUS_MESSAGE_ID(5, 0, AJ_PROP_SET)

#define AJ_PROPERTY_ABOUT_VERSION               AJ_BUS_PROPERTY_ID(5, 1, 0)

#define AJ_METHOD_ABOUT_GET_ABOUT_DATA          AJ_BUS_MESSAGE_ID(5, 1, 1)
#define AJ_METHOD_ABOUT_GET_OBJECT_DESCRIPTION  AJ_BUS_MESSAGE_ID(5, 1, 2)
#define AJ_SIGNAL_ABOUT_ANNOUNCE                AJ_BUS_MESSAGE_ID(5, 1, 3)

/*
 * Members of interface org.alljoyn.AboutIcon
 */
#define AJ_METHOD_ABOUT_ICON_GET_PROP          AJ_BUS_MESSAGE_ID(6, 0, AJ_PROP_GET)
#define AJ_METHOD_ABOUT_ICON_SET_PROP          AJ_BUS_MESSAGE_ID(6, 0, AJ_PROP_SET)

#define AJ_PROPERTY_ABOUT_ICON_VERSION_PROP    AJ_BUS_PROPERTY_ID(6, 1, 0)
#define AJ_PROPERTY_ABOUT_ICON_MIMETYPE_PROP   AJ_BUS_PROPERTY_ID(6, 1, 1)
#define AJ_PROPERTY_ABOUT_ICON_SIZE_PROP       AJ_BUS_PROPERTY_ID(6, 1, 2)

#define AJ_METHOD_ABOUT_ICON_GET_URL           AJ_BUS_MESSAGE_ID(6, 1, 3)
#define AJ_METHOD_ABOUT_ICON_GET_CONTENT       AJ_BUS_MESSAGE_ID(6, 1, 4)

/**
 * Message identifier that indicates a message was invalid.
 */
#define AJ_INVALID_MSG_ID              (0xFFFFFFFF)

/**
 * Message identifier that indicates a property was invalid.
 */
#define AJ_INVALID_PROP_ID             (0xFFFFFFFF)

/**
 * DBus well-known bus name
 */
extern const char AJ_DBusDestination[21];

/**
 * AllJoyn well-known bus name
 */
extern const char AJ_BusDestination[16];

/*
 * Error message strings
 */
extern const char AJ_ErrSecurityViolation[34];    /**< Error security violation string */
extern const char AJ_ErrTimeout[24];              /**< Error timeout string */
extern const char AJ_ErrRejected[25];             /**< Error rejected string */
extern const char AJ_ErrResources[26];            /**< Error resource string */
extern const char AJ_ErrServiceUnknown[42];       /**< Error service unknown string */

extern const char AJ_ErrUpdateNotAllowed[35];     /**< Error update not allowed string */
extern const char AJ_ErrInvalidValue[31];         /**< Error invalid value string */
extern const char AJ_ErrFeatureNotAvailable[38];  /**< Error feature not available string */
extern const char AJ_ErrMaxSizeExceeded[34];      /**< Error max size exceeded string */
extern const char AJ_ErrLanguageNotSuppored[39];  /**< Error language not supported string */

/**
 * The properties interface. This interface must be included in the property lists of all local and
 * proxy objects that have properties.
 */
extern const char* const AJ_PropertiesIface[5];

/**
 * The DBUS Peer interface name
 */
extern const char DBusPeerInterface[27];

/*
 * Constants for the various property method indices in the properties interface
 */
#define AJ_PROP_GET     0        /**< index for property method get */
#define AJ_PROP_SET     1        /**< index for property method set */
#define AJ_PROP_GET_ALL 2        /**< index for property method get_all */

/**
 * The introspection interface.
 */
extern const char* const AJ_IntrospectionIface[3];

/**
 * The AllSeen introspection interface name
 */
extern const char AllSeenIntrospectableInterface[28];

/**
 * The AllSeen introspection interface with the additional descriptions
 */
extern const char* const AJ_AllSeenIntrospectionIface[4];
/**
 * The standard objects that implement AllJoyn core functionality
 */
extern const AJ_Object AJ_StandardObjects[8];

/**
 * @}
 */
#endif
