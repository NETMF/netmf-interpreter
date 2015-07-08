#ifndef _AJ_INTROSPECT_H
#define _AJ_INTROSPECT_H

/**
 * @file aj_introspect.h
 * @defgroup aj_introspect Introspection Support
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
#include "aj_msg.h"

#ifdef __cplusplus
extern "C" {
#endif

/**
 * Support for introspection
 */

/**
 * An interface description is a NULL terminated array of strings. The first string is the interface
 * name. The subsequent strings are a compact representation of the members of the interface. In
 * this representation special characters encode information about the members, whitespace is
 * significant.
 *
 * If the first character of the interface name is a '$' character this indicates that the interface
 * is secure and only authenticated peers can make method calls and received signals defined in the
 * interface.  If the first character of the interface name is a '#' character this indicates that
 * security is not applicable to this interface even if the interface is implemented by an otherwise
 * secure object. The '$' and '#' characters are merely signifiers and are not part of the interface
 * name.
 *
 * The first character of a member string identifies the type of member:
 *
 * A '?' character indicates the member is a METHOD
 * A '!' character indicates the member is a SIGNAL
 * A '@' character indicates the member is a PROPERTY
 *
 * The type character is a signifier, it is not part of the member name. Characters following the
 * member type character up to the end of the string or to the first space character comprise the
 * member names. If the member is a METHOD or SIGNAL the remaining characters encode the argument
 * names, direction (IN or OUT) and the argument type as a standard AllJoyn signature string. For
 * SIGNALS for correctness the direction should be specified as OUT but it really doesn't matter as
 * the direction is ignored.
 *
 * Arguments are separated by a single space character. Argument names are optional and if present are
 * all characters between the space character and the directions character. All characters after the
 * direction character up to the next space or the end of the string are the argument type. The
 * argument direction is specified as follows:
 *
 * A '>' character indicates the argument is an OUT parameter.
 * A '<' character indicates the argument is an IN parameter.
 *
 * If the member is a PROPERTY the member name is terminated by an access rights character which is
 * immediately followed by the property type signature. The access rights for a property are
 * READ_ONLY, WRITE_ONLY and READ_WRITE. The access rights are specified as follows:
 *
 * A '>' character indicates the argument is READ_ONLY  (i.e. an OUT parameter)
 * A '<' character indicates the argument is WRITE_ONLY (i.e. an IN parameter)
 * A '=' character indicates the argument is READ/WRITE
 *
   @code

   static const char* const ExampleInterface[] = {
    "org.alljoyn.example",                  // The interface name
    "?StringPing inStr<s outStr>",          // A method called StringPing with an IN arg and OUT arg of type string
    "?Hello",                               // A method call with no arguments
    "?Add <i <i >i",                        // A method call that takes two integers and returns an integer. The args are not named
    "!ListChanged >a{ys}",                  // A signal that returns a dictionary
    "@TimeNow>(yyy)",                       // A READ_ONLY property that returns a struct with three 8 bit integers
    "@Counter=u",                           // A READ/WRITE property
    "@SecretKey<ay",                        // A WRITE_ONLY property that sets an array of bytes
    NULL                                    // End marker
   };

   @endcode

 *
 * This compact representation is expanded automatically into the very much more verbose XML form required to support introspection
 * requests.
 */

/**
 * Enmeration type for characterizing interface members
 */
typedef enum {
    AJ_INVALID_MEMBER   = 0,  /**< Invalid member */
    AJ_SIGNAL_MEMBER   = '!', /**< Member is a signal */
    AJ_METHOD_MEMBER   = '?', /**< Member is a method call */
    AJ_PROPERTY_MEMBER = '@'  /**< Member is a property */
} AJ_MemberType;

/**
 * Type for an interface description - NULL terminated array of strings.
 */
typedef const char* const* AJ_InterfaceDescription;

/*
 * AJ_DESCRIPTION_ID(BusObject base ID, Interface index, Member index, Arg index)
 * Interface, Member, and Arg indexes starts at 1 and represent the readible index in a list.
 * [ a, b, ... ] a would be index 1, b 2, etc.
 */
#define AJ_DESCRIPTION_ID(o, i, m, a)   ((((uint32_t)(o)) << 24) | (((uint32_t)(i)) << 16) | (((uint32_t)(m)) << 8) | (a))

/* Helper macros for reuse of the message and property Ids from the AJ_ENCODE_*_ID macros: */

/*
 * AJ_DESC_ID_FROM_MSG_ID(msgId, argIdx)
 * msgId Id of the method or the signal member
 * argIdx starts at 1 as in (a) in AJ_DESCRIPTION_ID. argIdx 0 indicate the interface member itself (i.e. method or signal) instead of the member's arguments.
 */
#define AJ_DESC_ID_FROM_MSG_ID(msgId, argIdx) (((uint32_t)(((uint32_t)(msgId) & 0xFFFFFF) + 0x101) << 8) | argIdx)

/*
 * AJ_DESC_ID_FROM_PROP_ID(propId)
 * propId Id of the property member
 */
#define AJ_DESC_ID_FROM_PROP_ID(propId) ((uint32_t)(((uint32_t)(propId) & 0xFFFFFF) + 0x101) << 8)


/*
 * AJ_DESC_ID_FROM_OBJ_INDEX(objIdx)
 * objIdx index of the object in its registered object list
 */
#define AJ_DESC_ID_FROM_OBJ_INDEX(objIdx) (((uint32_t)(objIdx) & 0xFF) << 24)

/*
 * AJ_DESC_ID_FROM_INTERFACE_INDEX(objIdx, ifaceIdx)
 * objIdx index of the object in its registered object list
 * ifaceIdx index of the interface in the object description for this object index.
 */
#define AJ_DESC_ID_FROM_INTERFACE_INDEX(objIdx, ifaceIdx) (AJ_DESC_ID_FROM_OBJ_INDEX(objIdx) | (((uint32_t)(ifaceIdx) & 0xFF) + 0x1)) << 16))

/* end of helper macros */

/**
 * Function pointer type for an abstracted Description lookup function
 *
 * @param descId    The encoded id that represents the request description value
 * @param lang      The language that is being asked for
 */
typedef const char* (*AJ_DescriptionLookupFunc)(uint32_t descId, const char* lang);

#define AJ_OBJ_FLAG_SECURE    0x01  /**< If set this bit indicates that an object is secure */
#define AJ_OBJ_FLAG_HIDDEN    0x02  /**< If set this bit indicates this is object is not announced */
#define AJ_OBJ_FLAG_DISABLED  0x04  /**< If set this bit indicates that method calls cannot be made to the object at this time */
#define AJ_OBJ_FLAG_ANNOUNCED 0x08  /**< If set this bit indicates this object is announced by ABOUT */
#define AJ_OBJ_FLAG_IS_PROXY  0x10  /**< If set this bit indicates this object is a proxy object */
#define AJ_OBJ_FLAG_DESCRIBED 0x20  /**< If set this bit indicates this object has descriptions and is announced by ABOUT with 'org.allseen.Introspectable' interface added to the announcement */

#define AJ_OBJ_FLAGS_ALL_INCLUDE_MASK 0xFF  /**< The include filter mask for the object iterator indicating ALL objects */

/**
 * Type for an AllJoyn object description
 */
typedef struct _AJ_Object {
    const char* path;                               /**< object path */
    const AJ_InterfaceDescription* interfaces;      /**< interface descriptor */
    uint8_t flags;                                  /**< flags for the object */
    void* context;                                  /**< an application provided context pointer for this object */
} AJ_Object;

/**
 * The root object
 */
extern const AJ_Object AJ_ROOT_OBJECT;

/*
 * When a message unmarshalled the message is validated by matching it against a list of object
 * tables that fully describe the message. If the message matches the unmarshal code sets the msgId
 * field in the AJ_Message struct. Rather than using a series of string comparisons, application code
 * can simply use this msgId to identify the message. There are three predefined object tables and
 * applications and services are free to add additional tables. The maximum number of table is 127
 * because the most signifant bit in the msgId is reserved to distinguish between method calls and
 * their corresponding replies.
 *
 * Of the three predefined tables the first is reserved for bus management messages. The second is
 * for objects implemented by the application. The third is for proxy (remote) objects the
 * application interacts with.
 *
 * The same message identifiers are also used by the marshalling code to populate the message header
 * with the appropriate strings for the object path, interface name, member, and signature. This
 * relieves the application developer from having to explicitly set these values in the message.
 */
#define AJ_BUS_ID_FLAG   0x00  /**< Identifies that a message belongs to the set of builtin bus object messages */
#define AJ_APP_ID_FLAG   0x01  /**< Identifies that a message belongs to the set of objects implemented by the application */
#define AJ_PRX_ID_FLAG   0x02  /**< Identifies that a message belongs to the set of objects implemented by remote peers */

/*
 * This flag AJ_REP_ID_FLAG is set in the msgId filed to indentify that a message is a reply to a
 * method call. Because the object description describes the out (call) and in (reply) arguments the
 * same entry in the object table is used for both method calls and replies but since they are
 * handled differently this flags is set by the unmarshaller to indicate whether the specific
 * message is the call or reply.
 */
#define AJ_REP_ID_FLAG   0x80  /**< Indicates a message is a reply message */

/*
 * Macros to encode a message or property id from object table index, object path, interface, and member indices.
 */
#define AJ_ENCODE_MESSAGE_ID(o, p, i, m)  (((uint32_t)(o) << 24) | (((uint32_t)(p)) << 16) | (((uint32_t)(i)) << 8) | (m)) /**< Encode a message id */
#define AJ_ENCODE_PROPERTY_ID(o, p, i, m) (((uint32_t)(o) << 24) | (((uint32_t)(p)) << 16) | (((uint32_t)(i)) << 8) | (m)) /**< Encode a property id */

/*
 * Macros for encoding the standard bus and applications messages
 */
#define AJ_BUS_MESSAGE_ID(p, i, m) AJ_ENCODE_MESSAGE_ID(AJ_BUS_ID_FLAG, p, i, m)   /**< Encode a message id from bus object */
#define AJ_APP_MESSAGE_ID(p, i, m) AJ_ENCODE_MESSAGE_ID(AJ_APP_ID_FLAG, p, i, m)   /**< Encode a message id from application object */
#define AJ_PRX_MESSAGE_ID(p, i, m) AJ_ENCODE_MESSAGE_ID(AJ_PRX_ID_FLAG, p, i, m)   /**< Encode a message id from proxy object */
/*
 * Macros for encoding the standard bus and application properties
 */
#define AJ_BUS_PROPERTY_ID(p, i, m) AJ_ENCODE_PROPERTY_ID(AJ_BUS_ID_FLAG, p, i, m) /**< Encode a property id from bus object */
#define AJ_APP_PROPERTY_ID(p, i, m) AJ_ENCODE_PROPERTY_ID(AJ_APP_ID_FLAG, p, i, m) /**< Encode a property id from application object */
#define AJ_PRX_PROPERTY_ID(p, i, m) AJ_ENCODE_PROPERTY_ID(AJ_PRX_ID_FLAG, p, i, m) /**< Encode a property id from proxy object */

/**
 * Macro to generate the reply message identifier from method call message. This is the message
 * identifier in the reply context.
 */
#define AJ_REPLY_ID(id)  ((id) | (uint32_t)(AJ_REP_ID_FLAG << 24))

/**
 * Register an object list with a specific index. This overrides any existing object list
 * registered at the specified index.
 *
 * @param objList   A NULL terminated array of object info structs.
 * @param index     The index for the object list to register.
 *
 * @return  - AJ_OK if the object list was succesfully registered
 *          - AJ_ERR_RANGE if the index is outside the allowed range
 *          - AJ_ERR_DISALLOWED if the index is a predefined object index
 */
AJ_EXPORT
AJ_Status AJ_RegisterObjectList(const AJ_Object* objList, uint8_t index);

/**
 * Register an object list with a specific index providing also a lookup function for the descriptions of objects in the list.
 * This overrides any existing object list and lookup function registered at the specified index.
 *
 * @param objList    A NULL terminated array of object info structs.
 * @param index      The index for the object list to register.
 * @param descLookup A lookup function for the descriptions of the describable items namely the objects, interfaces, members and arguments.
 *
 * @return  - AJ_OK if the object list was succesfully registered
 *          - AJ_ERR_RANGE if the index is outside the allowed range
 *          - AJ_ERR_DISALLOWED if the index is a predefined object index
 */
AJ_EXPORT
AJ_Status AJ_RegisterObjectListWithDescriptions(const AJ_Object* objList, uint8_t index, AJ_DescriptionLookupFunc descLookup);

/**
 * Register a local array of languages that will allow for a description to be supplied.
 *
 * @param languages The NULL-terminated list of languages that are supported for descriptions.
 */
AJ_EXPORT
void AJ_RegisterDescriptionLanguages(const char* const* languages);

/**
 * Register the local objects and the remote objects for this application.  Local objects have
 * methods that remote applications can call, have properties that a remote application can GET or
 * SET or define signals that the local application can emit.  Proxy objects describe the remote
 * objects that have methods that this object can call and signals
 * that remote objects emit that this application can receive.
 *
 * @param localObjects  A NULL terminated array of object info structs.
 * @param proxyObjects  A NULL terminated array of object info structs.
 */
AJ_EXPORT
void AJ_RegisterObjects(const AJ_Object* localObjects, const AJ_Object* proxyObjects);

/**
 * Object iterator type - treat as opaque
 */
typedef struct {
    uint8_t fin;
    uint8_t fex;
    uint8_t l;
    uint16_t n;
} AJ_ObjectIterator;

/**
 * Initialize announce object iterator.
 *
 * @param iter      Struct for maintaining object iterator state
 * @param inFlags   Object flags included in the iteration (logical AND with the object flags)
 * @param exFlags   Object flags excluded from the iteration (logical AND with the object flags)
 *
 *
 * @return  Returns the first iterated object. Call AJ_NextAnnounceObject to return successive
 *          objects.
 */
AJ_EXPORT
const AJ_Object* AJ_InitObjectIterator(AJ_ObjectIterator* iter, uint8_t inFlags, uint8_t exFlags);

/**
 * Returns the next iterated object
 */
AJ_EXPORT
const AJ_Object* AJ_NextObject(AJ_ObjectIterator* iter);

/**
 * This function checks that a message ifrom a remote peer is valid and correct and returns the
 * message id for that message.
 *
 * For method calls this function checks that the object is one of the registered objects, checks
 * that the interface and method are implemented by the object and checks that the signature is
 * correct.
 *
 * For signals this function checks that the interface is a known interface, the signal name is
 * defined for that interface, and the signature is correct.
 *
 * For method replies and error message this function matches the serial number of the response to
 * the serial number in the list of reply contexts. If the reply matches the signature is checked.
 *
 * If everything is correct the the message identifier is set in the message struct
 *
 * @param msg           The message to identify
 *
 * @return              Return AJ_Status
 */
AJ_Status AJ_IdentifyMessage(AJ_Message* msg);

/**
 * This function unmarshals the first two arguments of a property SET or GET message, identifies
 * which property the method is accessing and returns the id for the property.
 *
 * @param msg     The property GET or SET message to identify
 * @param propId  Returns the id for the identified property
 * @param sig     Pointer to string to return the signature
 *
 * @return   Return AJ_Status
 *         - ER_OK if the property was identified
 *         - AJ_ERR_NO_MATCH if there is no matching property
 *         - AJ_ERR_DISALLOWED if the property exists but has access rights do not permit the requested GET or SET operation.
 */
AJ_EXPORT
AJ_Status AJ_UnmarshalPropertyArgs(AJ_Message* msg, uint32_t* propId, const char** sig);

/**
 * This function marshals the first two arguments of a property SET or GET message.
 *
 * @param msg     The property GET or SET message to be initialized
 * @param propId  The the id for the specified property
 *
 * @return        Return AJ_Status
 */
AJ_EXPORT
AJ_Status AJ_MarshalPropertyArgs(AJ_Message* msg, uint32_t propId);

/**
 * This function marshals ALL the properties arguments (names and values) of a given interface of a property GET_ALL message.
 *
 * If everything is correct the reply message contains the marshalled response with the ALL the properties
 * defined for the given interface of the object associated with the request. Otherwise, an error reply
 * message is marshalled with the last error status.
 *
 * @param replyMsg      The message to marshal the reply into. Assumes the message header is already set by a previous call to AJ_MarshalReplyMsg().
 * @param iface         The interface name (obtained from the request) whose properties are to be marshalled.
 * @param callback      The function called to request the application to marshal each property value.
 * @param context       A caller provided context that is passed into the callback function.
 *
 * @return              Return AJ_Status
 */
AJ_EXPORT
AJ_Status AJ_MarshalAllPropertiesArgs(AJ_Message* replyMsg, const char* iface, AJ_BusPropGetCallback callback, void* context);

/**
 * Handle an introspection request
 *
 * @param msg        The introspection request method call
 * @param reply      The reply to the introspection request
 * @param languageTag The language that descriptions should be returned in if supported, else default lang used
 *
 * @return           Return AJ_Status
 */
AJ_Status AJ_HandleIntrospectRequest(const AJ_Message* msg, AJ_Message* reply, const char* languageTag);

/**
 * Handle a request to get the supported description languages
 *
 * @param msg       The get description languages method call
 * @param reply     The reply to the get description languages request
 *
 */
AJ_Status AJ_HandleGetDescriptionLanguages(const AJ_Message* msg, AJ_Message* reply);

/**
 * Internal function for initializing a message from information obtained via the message id.
 *
 * @param msg       The message to initialize
 * @param msgId     The message id
 * @param msgType   The type of the message
 *
 * @return          Return AJ_Status
 */
AJ_Status AJ_InitMessageFromMsgId(AJ_Message* msg, uint32_t msgId, uint8_t msgType, uint8_t* secure);

/**
 * Set or update the object path on a proxy object entry. This function makes is used for making
 * method calls to remote objects when the object path is not known until runtime. Note the proxy
 * object table cannot be declared as const in this case.
 *
 * @param proxyObjects  Pointer to the proxy object table (for validation purposes)
 * @param msgId         The message identifier for the methods
 * @param objPath       The object path to set. This value must remain valid while the method is
 *                      being marshaled.
 *
 * @return  - AJ_OK if the object path was sucessfully set.
 *          - AJ_ERR_OBJECT_PATH if the object path is NULL or invalid.
 *          - AJ_ERR_NO_MATCH if the message id does not identify a proxy object method call.
 */
AJ_EXPORT
AJ_Status AJ_SetProxyObjectPath(AJ_Object* proxyObjects, uint32_t msgId, const char* objPath);

/**
 * Internal function to allocate a reply context for a method call message. Reply contexts are used
 * to associate method replies with method calls. Depending on avaiable system resources the number
 * of reply contexts may be very limited, in some cases only one reply context.
 *
 * @param msg      A method call message that needs a reply context
 * @param timeout  The time to wait for a reply  (0 to use the internal default)
 *
 * @return   Return AJ_Status
 *         - AJ_OK if the reply context was allocated
 *         - AJ_ERR_RESOURCES if the reply context could not be allocated
 */
AJ_Status AJ_AllocReplyContext(AJ_Message* msg, uint32_t timeout);

/**
 * Internal function to release all reply contexts. Called when disconnecting from the bus.
 */
void AJ_ReleaseReplyContexts(void);

/**
 * Internal function to check for timed out method calls. Returns TRUE and sets some information in
 * the message struct to identify the timed-out call if there was one. This function is called by
 * AJ_UnmarshalMessage() when there are no messages to unmarshal.
 *
 * @param msg  A message structure to initialize if there was a timed-out method call.
 *
 * @return  Returns TRUE if there was a timed-out method call, FALSE otherwise.
 */
uint8_t AJ_TimedOutMethodCall(AJ_Message* msg);

/**
 * Internal function called to release a reply context in the case that a message could not be marshaled.
 *
 * @param msg  The message that a reply context might have been allocated for.
 */
void AJ_ReleaseReplyContext(AJ_Message* msg);

/**
 * Recursively set and/or clear the object flags on an application object and all the children of
 * the object. This function can be called to disable, hide, or secure and entire object tree. Note
 * that to use this funcion the application object list must not be declared as const.
 *
 * To disable an application object and all of its children recursively
 * @code
 * AJ_SetObjectFlags("/foo/bar", AJ_OBJ_FLAG_DISABLED, 0);
 * @endcode
 *
 * To enable an application object and all of its children recursively but leave them hidden
 * @code
 * AJ_SetObjectFlags("/foo/bar", AJ_OBJ_FLAG_HIDDEN, AJ_OBJ_FLAG_DISABLED);
 * @endcode
 *
 * @param objPath    The object path for the parent object to set the flags on
 * @param setFlags   The flags to set OR'd together
 * @param clearFlags The flags to clear OR'd together
 *
 * @return   Return AJ_Status
 *         - ER_OK if the flags were set
 *         - AJ_ERR_NO_MATCH if there are no matching objects
 */
AJ_EXPORT
AJ_Status AJ_SetObjectFlags(const char* objPath, uint8_t setFlags, uint8_t clearFlags);

/**
 * Returns the member type for a given message or property Id. Returns 0 if the
 * identifier is not a message or property identifier.
 *
 * @param identifier  The identifier to characterize.
 * @param member      If not NULL returns the member string
 * @param isSecure    If not NULL returns TRUE if the member is secure either because the interface
 *                    is secure or the object instance is secure.
 *
 * @return  One of the AJ_MemberType enumeration values.
 */
AJ_EXPORT
AJ_MemberType AJ_GetMemberType(uint32_t identifier, const char** member, uint8_t* isSecure);

/**
 * Debugging aid that prints out the XML for an object table
 *
 * @param objs        A NULL terminated array of object info structs.
 */
#ifdef NDEBUG
#define AJ_PrintXML(objs)
#else
AJ_EXPORT
void AJ_PrintXML(const AJ_Object* objs);
#endif

/**
 * Debugging aid that prints out the XML for an object table
 *
 * @param objs        A NULL terminated array of object info structs.
 * @param languageTag The language that descriptions should be returned in if supported, else default lang used
 */
#ifdef NDEBUG
#define AJ_PrintXMLWithDescriptions(...)
#else
AJ_EXPORT
void AJ_PrintXMLWithDescriptions(const AJ_Object* objs, const char* languageTag);
#endif

/**
 * Hook for unit testing marshal/unmarshal
 */
#ifndef NDEBUG
typedef AJ_Status (*AJ_MutterHook)(AJ_Message* msg, uint32_t msgId, uint8_t msgType);
#endif

#ifdef __cplusplus
}
#endif
/**
 * @}
 */
#endif

