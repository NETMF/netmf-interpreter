/******************************************************************************
 * Copyright (c) 2013 - 2014, AllSeen Alliance. All rights reserved.
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

/**
 * Per-module definition of the current module for debug logging.  Must be defined
 * prior to first inclusion of aj_debug.h
 */
#define AJ_MODULE ABOUT

#include "alljoyn.h"
#include "aj_debug.h"
#include "aj_config.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgABOUT = 0;
#endif

#define ABOUT_VERSION      (1)
#define ABOUT_ICON_VERSION (1)

/*
 * Registered by the Property Store implementation
 */
static AJ_AboutPropGetter PropStoreGetter = NULL;

/*
 * About icon registration
 */
static struct {
    uint16_t size;
    const uint8_t* data;
    const char* mime;
    const char* URL;
} icon;

/*
 * Checked to see if announcements have been requested
 */
static uint8_t doAnnounce = TRUE;

void AJ_AboutRegisterPropStoreGetter(AJ_AboutPropGetter propGetter)
{
    PropStoreGetter = propGetter;
}

/*
 * Default about properties if there is no property store getter registered
 */
AJ_Status MarshalDefaultProps(AJ_Message* msg)
{
    AJ_Status status;
    AJ_Arg array;

    status = AJ_MarshalContainer(msg, &array, AJ_ARG_ARRAY);
    if (status == AJ_OK) {
        status = AJ_MarshalCloseContainer(msg, &array);
    }
    return status;
}

AJ_Status AJ_AboutInit(AJ_BusAttachment* bus, uint16_t boundPort)
{
    bus->aboutPort = boundPort;
    doAnnounce = TRUE;
    return AJ_AboutAnnounce(bus);
}

void AJ_AboutSetIcon(const uint8_t* data, uint16_t size, const char* mime, const char* url)
{
    icon.data = data;
    icon.size = data ? size : 0;
    icon.mime = mime;
    icon.URL = url;
}

/*
 * Handles a property GET request so marshals the property value to return
 */
static AJ_Status AboutGetProp(AJ_Message* replyMsg, uint32_t propId, void* context)
{
    if (propId == AJ_PROPERTY_ABOUT_VERSION) {
        return AJ_MarshalArgs(replyMsg, "q", (uint16_t)ABOUT_VERSION);
    } else {
        return AJ_ERR_UNEXPECTED;
    }
}

AJ_Status AJ_AboutHandleGetProp(AJ_Message* msg)
{
    return AJ_BusPropGet(msg, AboutGetProp, NULL);
}

AJ_Status AJ_AboutHandleGetAboutData(AJ_Message* msg, AJ_Message* reply)
{
    AJ_Status status = AJ_OK;
    const char* language;

    status = AJ_UnmarshalArgs(msg, "s", &language);
    if (status == AJ_OK) {
        AJ_MarshalReplyMsg(msg, reply);
        if (PropStoreGetter) {
            status = PropStoreGetter(reply, language);
        } else {
            status = MarshalDefaultProps(reply);
        }
        if (status != AJ_OK) {
            status = AJ_MarshalErrorMsg(msg, reply, AJ_ErrLanguageNotSuppored);
        }
    }
    return status;
}

AJ_Status MarshalObjectDescriptions(AJ_Message* msg)
{
    AJ_Status status;
    AJ_Arg objList;
    AJ_ObjectIterator iter;
    const AJ_Object* obj;

    status = AJ_MarshalContainer(msg, &objList, AJ_ARG_ARRAY);
    if (status != AJ_OK) {
        goto ErrorExit;
    }
    /*
     * Announce object that a flagged for announcement and not hidden
     */
    for (obj = AJ_InitObjectIterator(&iter, AJ_OBJ_FLAG_ANNOUNCED | AJ_OBJ_FLAG_DESCRIBED, AJ_OBJ_FLAG_HIDDEN); obj != NULL; obj = AJ_NextObject(&iter)) {
        size_t i;
        AJ_Arg structure;
        AJ_Arg ifcList;
        const char* iface;

        status = AJ_MarshalContainer(msg, &structure, AJ_ARG_STRUCT);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        AJ_InfoPrintf(("Announcing object %s\n", obj->path));
        status = AJ_MarshalArgs(msg, "o", obj->path);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalContainer(msg, &ifcList, AJ_ARG_ARRAY);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        /*
         * Add the AllSeenIntrospectableInterface if this object is flagged as being described
         */
        if (obj->flags & AJ_OBJ_FLAG_DESCRIBED) {
            iface = AllSeenIntrospectableInterface;
            /*
             * Don't need the $ or # that indicate the interface is secure or not
             */
            if (*iface == '$' || *iface == '#') {
                ++iface;
            }
            AJ_InfoPrintf(("  %s\n", iface));
            status = AJ_MarshalArgs(msg, "s", iface);
            if (status != AJ_OK) {
                goto ErrorExit;
            }
        }
        for (i = 0; obj->interfaces[i]; ++i) {
            if (obj->interfaces[i] != AJ_PropertiesIface) {
                iface = obj->interfaces[i][0];
                if (iface) {
                    /*
                     * Don't need the $ or # that indicate the interface is secure or not
                     */
                    if (*iface == '$' || *iface == '#') {
                        ++iface;
                    }
                    AJ_InfoPrintf(("  %s\n", iface));
                    status = AJ_MarshalArgs(msg, "s", iface);
                    if (status != AJ_OK) {
                        goto ErrorExit;
                    }
                }
            }
        }
        status = AJ_MarshalCloseContainer(msg, &ifcList);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
        status = AJ_MarshalCloseContainer(msg, &structure);
        if (status != AJ_OK) {
            goto ErrorExit;
        }
    }
    return AJ_MarshalCloseContainer(msg, &objList);

ErrorExit:
    return status;
}

AJ_Status AJ_AboutAnnounce(AJ_BusAttachment* bus)
{
    AJ_Status status = AJ_OK;
    AJ_Message announcement;

    if (!doAnnounce) {
        return status;
    }
    doAnnounce = FALSE;
    if (!bus->aboutPort) {
        AJ_InfoPrintf(("AJ_AboutAnnounce - nothing to announce\n"));
        return status;
    }

    AJ_InfoPrintf(("AJ_AboutAnnounce - announcing port=%d\n", bus->aboutPort));

    status = AJ_MarshalSignal(bus, &announcement, AJ_SIGNAL_ABOUT_ANNOUNCE, NULL, 0, ALLJOYN_FLAG_SESSIONLESS, 0);
    if (status != AJ_OK) {
        goto ErrorExit;
    }
    status = AJ_MarshalArgs(&announcement, "q", (uint16_t)ABOUT_VERSION);
    if (status != AJ_OK) {
        goto ErrorExit;
    }
    status = AJ_MarshalArgs(&announcement, "q", bus->aboutPort);
    if (status != AJ_OK) {
        goto ErrorExit;
    }
    status = MarshalObjectDescriptions(&announcement);
    if (status != AJ_OK) {
        goto ErrorExit;
    }
    if (PropStoreGetter) {
        status = PropStoreGetter(&announcement, "");
    } else {
        status = MarshalDefaultProps(&announcement);
    }
    if (status != AJ_OK) {
        goto ErrorExit;
    }
    return AJ_DeliverMsg(&announcement);

ErrorExit:
    return status;
}

AJ_Status AJ_AboutHandleGetObjectDescription(AJ_Message* msg, AJ_Message* reply)
{
    AJ_Status status = AJ_MarshalReplyMsg(msg, reply);
    if (status == AJ_OK) {
        return MarshalObjectDescriptions(reply);
    } else {
        return status;
    }
}

static AJ_Status AboutIconGetProp(AJ_Message* replyMsg, uint32_t propId, void* context)
{
    AJ_Status status = AJ_ERR_UNEXPECTED;

    if (propId == AJ_PROPERTY_ABOUT_ICON_VERSION_PROP) {
        status = AJ_MarshalArgs(replyMsg, "q", (uint16_t)ABOUT_ICON_VERSION);
    } else if (propId == AJ_PROPERTY_ABOUT_ICON_MIMETYPE_PROP) {
        status = AJ_MarshalArgs(replyMsg, "s", icon.mime ? icon.mime : "");
    } else if (propId == AJ_PROPERTY_ABOUT_ICON_SIZE_PROP) {
        status = AJ_MarshalArgs(replyMsg, "u", (uint32_t)icon.size);
    }
    return status;
}

AJ_Status AJ_AboutIconHandleGetProp(AJ_Message* msg)
{
    return AJ_BusPropGet(msg, AboutIconGetProp, NULL);
}

AJ_Status AJ_AboutIconHandleGetURL(AJ_Message* msg, AJ_Message* reply)
{
    AJ_Status status;

    status = AJ_MarshalReplyMsg(msg, reply);
    if (status != AJ_OK) {
        goto ErrorExit;
    }
    return AJ_MarshalArgs(reply, "s", icon.URL ? icon.URL : "");

ErrorExit:
    return status;
}

AJ_Status AJ_AboutIconHandleGetContent(AJ_Message* msg, AJ_Message* reply)
{
    AJ_Status status;
    uint32_t u = (uint32_t)icon.size;

    status = AJ_MarshalReplyMsg(msg, reply);
    if (status != AJ_OK) {
        goto ErrorExit;
    }
    status = AJ_DeliverMsgPartial(reply, u + 4);
    if (status != AJ_OK) {
        goto ErrorExit;
    }
    status = AJ_MarshalRaw(reply, &u, 4);
    if (status != AJ_OK) {
        goto ErrorExit;
    }
    return AJ_MarshalRaw(reply, icon.data, u);

ErrorExit:
    return status;
}

void AJ_AboutSetShouldAnnounce()
{
    doAnnounce = TRUE;
}

void AJ_AboutSetAnnounceObjects(AJ_Object* objList)
{
    if (objList) {
        while (objList->path) {
            objList->flags |= AJ_OBJ_FLAG_ANNOUNCED;
            ++objList;
        }
    }
}

#ifdef ANNOUNCE_BASED_DISCOVERY
static const AJ_AboutPeerDescription* peerList = NULL;
static uint16_t peerListLength = 0;

void AJ_AboutRegisterAnnounceHandlers(AJ_AboutPeerDescription* peerDescs, uint16_t peerDescCount)
{
    peerList = peerDescs;
    peerListLength = peerDescCount;
}

/**
 * It is assumed that the AJ_Message *msg supplied to this function is in memory while using objDescs. objDescs is merely pointing to entries in the messgae buffer in msg.
 */
AJ_Status AJ_AboutUnmarshalObjectDescriptions(AJ_Message* msg, AJ_AboutObjectDescription* objDescs, uint16_t* objDescsCount)
{
    AJ_Status status = AJ_OK;
    AJ_Arg objList;


    status = AJ_UnmarshalContainer(msg, &objList, AJ_ARG_ARRAY);
    if (status != AJ_OK) {
        goto ErrorExit;
    }
    /*
     * Announce object that a flagged for announcement and not hidden
     */
    while (status == AJ_OK) {
        AJ_Arg structure;
        AJ_Arg ifcList;
        uint16_t count = 0;

        status = AJ_UnmarshalContainer(msg, &structure, AJ_ARG_STRUCT);
        if (status != AJ_OK) {
            break;
        }

        status = AJ_UnmarshalArgs(msg, "o", &objDescs[*objDescsCount].path);
        if (status != AJ_OK) {
            goto ErrorExit;
        }

        status = AJ_UnmarshalContainer(msg, &ifcList, AJ_ARG_ARRAY);
        if (status != AJ_OK) {
            goto ErrorExit;
        }

        while (status == AJ_OK) {
            status = AJ_UnmarshalArgs(msg, "s", &objDescs[*objDescsCount].interfaces[count]);

            count++;
            if (count >= AJ_MAX_NUM_OF_INTERFACES) {
                AJ_ErrPrintf(("Maximum Predefined number of interfaces (%d) exceeded\n", AJ_MAX_NUM_OF_INTERFACES));
                status = AJ_ERR_RESOURCES;
                goto ErrorExit;
            }
        }

        if ((status != AJ_ERR_NO_MORE) && (status != AJ_OK)) {
            goto ErrorExit;
        }

        objDescs[*objDescsCount].interfacesCount = count - 1;

        status = AJ_UnmarshalCloseContainer(msg, &ifcList);
        if (status != AJ_OK) {
            goto ErrorExit;
        }

        status = AJ_UnmarshalCloseContainer(msg, &structure);

        (*objDescsCount)++;

        if (*objDescsCount >= AJ_MAX_NUM_OF_OBJ_DESC) {
            AJ_ErrPrintf(("Maximum Predefined number of object descriptions (%d) exceeded\n", AJ_MAX_NUM_OF_OBJ_DESC));
            status = AJ_ERR_RESOURCES;
            goto ErrorExit;
        }
    }

    if (status == AJ_ERR_NO_MORE) {
        return AJ_UnmarshalCloseContainer(msg, &objList);
    }

ErrorExit:
    return status;
}

#define UUID_LENGTH 16
#define APP_ID_SIGNATURE "ay"

AJ_Status AJ_AboutUnmarshalAppIdFromVariant(AJ_Message* msg, char* buf, size_t bufLen)
{
    AJ_Status status;
    uint8_t* appId;
    size_t appIdLen;

    if (bufLen < (UUID_LENGTH * 2 + 1)) {
        AJ_ErrPrintf(("UnmarshalAppId: Insufficient buffer size! Should be at least %u but got %u\n", UUID_LENGTH * 2 + 1, (uint32_t)bufLen));
        return AJ_ERR_RESOURCES;
    }
    status = AJ_UnmarshalArgs(msg, "v", APP_ID_SIGNATURE, &appId, &appIdLen);
    if (status != AJ_OK) {
        return status;
    }
    status = AJ_RawToHex((const uint8_t*)appId, appIdLen, buf, ((appIdLen > UUID_LENGTH) ? UUID_LENGTH : appIdLen) * 2 + 1, FALSE);

    return status;
}

AJ_Status AJ_AboutUnmarshalProps(AJ_Message* msg, AJ_AboutHandleMandatoryProps onMandatoryProperties, AJ_AboutHandleOptionalProperty onOptionalProperty)
{
    AJ_Status status = AJ_OK;
    AJ_Arg array;
    AJ_Arg dict;
    char* key;
    const char* peerName = msg->sender;
    char appId[UUID_LENGTH * 2 + 1];
    const char* appName = NULL;
    const char* deviceId = NULL;
    const char* deviceName = NULL;
    const char* manufacturer = NULL;
    const char* modelNumber = NULL;
    const char* defaultLanguage = NULL;

    appId[0] = '\0';

    status = AJ_UnmarshalContainer(msg, &array, AJ_ARG_ARRAY);

    while (status == AJ_OK) {

        status = AJ_UnmarshalContainer(msg, &dict, AJ_ARG_DICT_ENTRY);

        if (status != AJ_OK) {
            break;
        }

        status = AJ_UnmarshalArgs(msg, "s", &key);
        if (status != AJ_OK) {
            break;
        }

        if (!strcmp(key, "AppId")) {
            status = AJ_AboutUnmarshalAppIdFromVariant(msg, appId, sizeof(appId));
        } else if (!strcmp(key, "AppName")) {
            status = AJ_UnmarshalArgs(msg, "v", "s", &appName);
        } else if (!strcmp(key, "DeviceId")) {
            status = AJ_UnmarshalArgs(msg, "v", "s", &deviceId);
        } else if (!strcmp(key, "DeviceName")) {
            status = AJ_UnmarshalArgs(msg, "v", "s", &deviceName);
        } else if (!strcmp(key, "Manufacturer")) {
            status = AJ_UnmarshalArgs(msg, "v", "s", &manufacturer);
        } else if (!strcmp(key, "ModelNumber")) {
            status = AJ_UnmarshalArgs(msg, "v", "s", &modelNumber);
        } else if (!strcmp(key, "DefaultLanguage")) {
            status = AJ_UnmarshalArgs(msg, "v", "s", &defaultLanguage);
        } else {
            if (onOptionalProperty == NULL) {
                status = AJ_SkipArg(msg);
            } else {
                const char* vsig;
                AJ_Arg value;

                status = AJ_UnmarshalVariant(msg, &vsig);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_UnmarshalArg(msg, &value);
                if (status != AJ_OK) {
                    break;
                }
                onOptionalProperty(peerName, key, vsig, &value);
            }
        }

        if (status != AJ_OK) {
            break;
        }

        status = AJ_UnmarshalCloseContainer(msg, &dict);
    }

    if (status == AJ_ERR_NO_MORE) {
        AJ_InfoPrintf(("About Data:\nAppId:'%s'\nAppName:'%s'\nDeviceId:'%s'\nDeviceName:'%s'\nManufacturer:'%s'\nModelNumber'%s'\nDefaultLanguage:'%s'\n",
                       (appId[0] == '\0' ? "N/A" : appId),
                       ((appName == NULL || appName[0] == '\0') ? "N/A" : appName),
                       ((deviceId == NULL || deviceId[0] == '\0') ? "N/A" : deviceId),
                       ((deviceName == NULL || deviceName[0] == '\0') ? "N/A" : deviceName),
                       ((manufacturer == NULL || manufacturer[0] == '\0') ? "N/A" : manufacturer),
                       ((modelNumber == NULL || modelNumber[0] == '\0') ? "N/A" : modelNumber),
                       ((defaultLanguage == NULL || defaultLanguage[0] == '\0') ? "N/A" : defaultLanguage)));
        status = AJ_UnmarshalCloseContainer(msg, &array);
        if ((status == AJ_OK) && (onMandatoryProperties != NULL)) {
            onMandatoryProperties(peerName, appId, appName, deviceId, deviceName, manufacturer, modelNumber, defaultLanguage);
        }
    }

    return status;
}

static void handleMandatoryProps(const char* peerName,
                                 const char* appId,
                                 const char* appName,
                                 const char* deviceId,
                                 const char* deviceName,
                                 const char* manufacturer,
                                 const char* modelNumber,
                                 const char* defaultLanguage)
{
    uint16_t i;

    if ((peerList != NULL) && (peerListLength > 0)) {
        for (i = 0; i < peerListLength; i++) {
            if (peerList[i].handleMandatoryProps != NULL) {
                peerList[i].handleMandatoryProps(peerName, appId, appName, deviceId, deviceName, manufacturer, modelNumber, defaultLanguage);
            }
        }
    }
}

static void handleOptionalProp(const char* peerName, const char* key, const char* sig, const AJ_Arg* value) {
    uint16_t i;

    if ((peerList != NULL) && (peerListLength > 0)) {
        for (i = 0; i < peerListLength; i++) {
            if (peerList[i].handleOptionalProperty != NULL) {
                peerList[i].handleOptionalProperty(peerName, key, sig, value);
            }
        }
    }
}

AJ_Status AJ_AboutHandleAnnounce(AJ_Message* announcement, uint16_t* outAboutVersion, uint16_t* outAboutPort, char* peerName, uint8_t* outRelevant)
{
    AJ_Status status = AJ_OK;
    const char* objPath = "/";
    uint16_t peerListCount = 0;
    AJ_AboutObjectDescription objDescs[AJ_MAX_NUM_OF_OBJ_DESC];
    uint16_t objDescsCount = 0;
    uint16_t aboutVersion = 0;
    uint16_t aboutPort = 0;
    uint8_t relevant = FALSE;

    /**
     * Extract basic information from Announcement message
     */
    status = AJ_UnmarshalArgs(announcement, "qq", &aboutVersion, &aboutPort);
    if (status != AJ_OK) {
        return status;
    }
    if (outAboutVersion != NULL) {
        *outAboutVersion = aboutVersion;
    }
    if (outAboutPort != NULL) {
        *outAboutPort = aboutPort;
    }
    if (outRelevant != NULL) {
        *outRelevant = relevant;
    }

    if (peerName != NULL) {
        strncpy(peerName, announcement->sender, AJ_MAX_NAME_SIZE);
        peerName[AJ_MAX_NAME_SIZE] = '\0';
    }

    /**
     * If there are no registered peer descriptions to match then skip all processing of Announce message and return basic information
     */
    if ((peerList == NULL) || (peerListLength == 0)) {
        return status;
    }

    /**
     * Unmarshal the object description section i.e. the objects with their paths and published interface names
     */
    status = AJ_AboutUnmarshalObjectDescriptions(announcement, objDescs, &objDescsCount);
    if (status != AJ_OK) {
        return status;
    }

    /**
     * Match object descriptions against registered peer descriptions looking for implemented interfaces
     */
    for (peerListCount = 0; peerListCount < peerListLength; peerListCount++) {
        const AJ_AboutPeerDescription* peerDesc = &peerList[peerListCount];
        const char** ifaceNames = peerDesc->implementsInterfaces;

        if (ifaceNames != NULL) {
            uint8_t count;
            uint8_t found = 0;

            /**
             * Loop through peer interfaces
             */
            for (count = 0; count < peerDesc->numberInterfaces; count++) {
                uint16_t i;

                /**
                 * Loop through the object descriptions
                 */
                for (i = 0; i != objDescsCount; i++) {
                    uint16_t j;
                    AJ_AboutObjectDescription* currObjDesc = &objDescs[i];

                    if (peerDesc->handleObjectDescription != NULL) {
                        peerDesc->handleObjectDescription(announcement->sender, currObjDesc);
                    }

                    /**
                     * Loop through the interfaces
                     */
                    for (j = 0; j != currObjDesc->interfacesCount; j++) {
                        if (strcmp(currObjDesc->interfaces[j], ifaceNames[count]) == 0) {
                            objPath = currObjDesc->path;
                            found++;
                        }
                    }
                    /**
                     * Check if the current object contains ALL the interfaces and fire handle on the match
                     */
                    if ((found == peerDesc->numberInterfaces) && (peerDesc->handleMatch != NULL)) {
                        if (peerDesc->handleMatch(aboutVersion, aboutPort, announcement->sender, objPath) == FALSE) {
                            goto NextPeerDescription;
                        }
                        found = 0;
                    }
                }
            }
            /**
             * Check if ALL the interfaces where found on peer and fire handle on the match
             */
            if ((found == peerDesc->numberInterfaces) && (peerDesc->handleMatch != NULL)) {
                /**
                 * If peer description includes multiple interfaces on different objects or no interfaces return the root object path
                 */
                if (peerDesc->numberInterfaces != 1) {
                    objPath = "/";
                }
                peerDesc->handleMatch(aboutVersion, aboutPort, announcement->sender, objPath);
            }
        } else {
            /**
             * If no interface names criteria registered simply call the registered peer found handler with root object path.
             * The handler will be able to perform introspection using the given peerName if required.
             */
            if (peerDesc->handleMatch != NULL) {
                peerDesc->handleMatch(aboutVersion, aboutPort, announcement->sender, objPath);
            }
        }
    NextPeerDescription:
        continue;
    }

    /**
     * Unmarshal the metadata section i.e. the property list
     */
    status = AJ_AboutUnmarshalProps(announcement, handleMandatoryProps, handleOptionalProp);

    for (peerListCount = 0; peerListCount < peerListLength; peerListCount++) {
        const AJ_AboutPeerDescription* peerDesc = &peerList[peerListCount];

        relevant |= peerDesc->handleIsRelevant(announcement->sender);
    }
    if (outRelevant != NULL) {
        *outRelevant = relevant;
    }

    return status;
}
#endif