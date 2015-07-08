/**
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

/**
 * Per-module definition of the current module for debug logging.  Must be defined
 * prior to first inclusion of aj_debug.h
 */
#define AJ_MODULE GUID

#include "aj_target.h"
#include "aj_guid.h"
#include "aj_util.h"
#include "aj_crypto.h"
#include "aj_debug.h"
#include "aj_config.h"
#include "aj_std.h"
#include "aj_connect.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgGUID = 0;
#endif

typedef struct _NameToGUID {
    uint8_t keyRole;
    char uniqueName[AJ_MAX_NAME_SIZE + 1];
    const char* serviceName;
    AJ_GUID guid;
    uint8_t sessionKey[16];
    uint8_t groupKey[16];
    uint32_t replySerial;
    uint32_t authVersion;
} NameToGUID;

static uint8_t localGroupKey[16];

static NameToGUID nameMap[AJ_NAME_MAP_GUID_SIZE];

static AJ_Status SetNameOwnerChangedRule(AJ_BusAttachment* bus, const char* oldOwner, uint8_t rule, uint32_t* serialNum);
static AJ_Status NameHasOwner(AJ_Message* msg, const char* name, uint32_t* serialNum);

AJ_Status AJ_GUID_ToString(const AJ_GUID* guid, char* buffer, uint32_t bufLen)
{
    return AJ_RawToHex(guid->val, 16, buffer, bufLen, TRUE);
}

AJ_Status AJ_GUID_FromString(AJ_GUID* guid, const char* str)
{
    return AJ_HexToRaw(str, 32, guid->val, 16);
}

static NameToGUID* LookupName(const char* name)
{
    uint32_t i;
    AJ_InfoPrintf(("LookupName(name=\"%s\")\n", name));

    for (i = 0; i < AJ_NAME_MAP_GUID_SIZE; ++i) {
        if (strcmp(nameMap[i].uniqueName, name) == 0) {
            return &nameMap[i];
        }
        if (nameMap[i].serviceName && (strcmp(nameMap[i].serviceName, name)) == 0) {
            return &nameMap[i];
        }
    }
    AJ_InfoPrintf(("LookupName(): NULL\n"));
    return NULL;
}

static NameToGUID* LookupReplySerial(uint32_t replySerial)
{
    uint32_t i;

    for (i = 0; i < AJ_NAME_MAP_GUID_SIZE; ++i) {
        if (nameMap[i].replySerial == replySerial) {
            return &nameMap[i];
        }
    }
    return NULL;
}

AJ_Status AJ_GUID_AddNameMapping(AJ_BusAttachment* bus, const AJ_GUID* guid, const char* uniqueName, const char* serviceName)
{
    AJ_Status status;
    size_t len = strlen(uniqueName);
    NameToGUID* mapping;
    int isNew;
    uint32_t serialNum;

    AJ_InfoPrintf(("AJ_GUID_AddNameMapping(guid=0x%p, uniqueName=\"%s\", serviceName=\"%s\")\n", guid, uniqueName, serviceName));

    mapping = LookupName(uniqueName);
    isNew = !mapping;
    if (isNew) {
        mapping = LookupName("");
    }
    if (mapping && (len <= AJ_MAX_NAME_SIZE)) {
        if (isNew && (AJ_GetRoutingProtoVersion() >= 11)) {
            status = SetNameOwnerChangedRule(bus, uniqueName, AJ_BUS_SIGNAL_ALLOW, &serialNum);
            if (status != AJ_OK) {
                AJ_ErrPrintf(("AJ_GUID_AddNameMapping(guid=0x%p, uniqueName=\"%s\", serviceName=\"%s\"): Add match rule error\n",
                              guid, uniqueName, serviceName));
                return status;
            }
            mapping->replySerial = serialNum;
        }
        memcpy(&mapping->guid, guid, sizeof(AJ_GUID));
        memcpy(&mapping->uniqueName, uniqueName, len + 1);
        mapping->serviceName = serviceName;
        return AJ_OK;
    } else {
        AJ_ErrPrintf(("AJ_GUID_AddNameMapping(): AJ_ERR_RESOURCES\n"));
        return AJ_ERR_RESOURCES;
    }
}

void AJ_GUID_DeleteNameMapping(AJ_BusAttachment* bus, const char* uniqueName)
{
    AJ_Status status;
    NameToGUID* mapping;

    AJ_InfoPrintf(("AJ_GUID_DeleteNameMapping(uniqueName=\"%s\")\n", uniqueName));
    mapping = LookupName(uniqueName);
    if (mapping) {
        if (AJ_GetRoutingProtoVersion() >= 11) {
            status = SetNameOwnerChangedRule(bus, uniqueName, AJ_BUS_SIGNAL_DENY, NULL);
            if (status != AJ_OK) {
                AJ_WarnPrintf(("AJ_GUID_DeleteNameMapping(uniqueName=\"%s\"): Remove match rule error\n", uniqueName));
            }
        }
        memset(mapping, 0, sizeof(NameToGUID));
    }
}

const AJ_GUID* AJ_GUID_Find(const char* name)
{
    NameToGUID* mapping = LookupName(name);
    AJ_InfoPrintf(("AJ_GUID_Find(name=\"%s\")\n", name));

    return mapping ? &mapping->guid : NULL;
}


void AJ_GUID_ClearNameMap(void)
{
    AJ_InfoPrintf(("AJ_GUID_ClearNameMap()\n"));
    memset(nameMap, 0, sizeof(nameMap));
}

AJ_Status AJ_SetGroupKey(const char* uniqueName, const uint8_t* key)
{
    NameToGUID* mapping;

    AJ_InfoPrintf(("AJ_SetGroupKey(uniqueName=\"%s\", key=0x%p)\n", uniqueName, key));

    mapping = LookupName(uniqueName);
    if (mapping) {
        memcpy(mapping->groupKey, key, 16);
        return AJ_OK;
    } else {
        AJ_WarnPrintf(("AJ_SetGroupKey(): AJ_ERR_NO_MATCH\n"));
        return AJ_ERR_NO_MATCH;
    }
}

AJ_Status AJ_SetSessionKey(const char* uniqueName, const uint8_t* key, uint8_t role, uint32_t authVersion)
{
    NameToGUID* mapping;

    AJ_InfoPrintf(("AJ_SetGroupKey(uniqueName=\"%s\", key=0x%p)\n", uniqueName, key));

    mapping = LookupName(uniqueName);
    if (mapping) {
        mapping->keyRole = role;
        mapping->authVersion = authVersion;
        memcpy(mapping->sessionKey, key, 16);
        return AJ_OK;
    } else {
        AJ_WarnPrintf(("AJ_SetSessionKey(): AJ_ERR_NO_MATCH\n"));
        return AJ_ERR_NO_MATCH;
    }
}

AJ_Status AJ_GetSessionKey(const char* name, uint8_t* key, uint8_t* role, uint32_t* authVersion)
{
    NameToGUID* mapping;

    AJ_InfoPrintf(("AJ_GetSessionKey(name=\"%s\", key=0x%p, role=0x%p)\n", name, key, role));

    mapping = LookupName(name);
    if (mapping) {
        *role = mapping->keyRole;
        *authVersion = mapping->authVersion;
        memcpy(key, mapping->sessionKey, 16);
        return AJ_OK;
    } else {
        AJ_WarnPrintf(("AJ_GetSessionKey(): AJ_ERR_NO_MATCH\n"));
        return AJ_ERR_NO_MATCH;
    }
}

AJ_Status AJ_GetGroupKey(const char* name, uint8_t* key)
{
    AJ_InfoPrintf(("AJ_GetGroupKey(name=\"%s\", key=0x%p)\n", name, key));
    if (name) {
        NameToGUID* mapping = LookupName(name);
        if (!mapping) {
            AJ_WarnPrintf(("AJ_GetGroupKey(): AJ_ERR_NO_MATCH\n"));
            return AJ_ERR_NO_MATCH;
        }
        memcpy(key, mapping->groupKey, 16);
    } else {
        /*
         * Check if the group key needs to be initialized
         */
        memset(key, 0, 16);
        if (memcmp(localGroupKey, key, 16) == 0) {
            AJ_RandBytes(localGroupKey, 16);
        }
        memcpy(key, localGroupKey, 16);
    }
    return AJ_OK;
}

static AJ_Status SetNameOwnerChangedRule(AJ_BusAttachment* bus, const char* oldOwner, uint8_t rule, uint32_t* serialNum)
{
    AJ_Status status;
    size_t ruleLen;
    char* ruleStr;
    const char* rulePrefix = "type='signal',member='NameOwnerChanged',interface='org.freedesktop.DBus',arg1='";
    const char* ruleSuffix = "',arg2=''";

    ruleLen = strlen(rulePrefix) + strlen(oldOwner) + strlen(ruleSuffix);
    ruleStr = (char*) AJ_Malloc(ruleLen + 1 /* \0 */);
    if (ruleStr == NULL) {
        return AJ_ERR_RESOURCES;
    }
    strcpy(ruleStr, rulePrefix);
    strcat(ruleStr, oldOwner);
    strcat(ruleStr, ruleSuffix);
    status = AJ_BusSetSignalRuleSerial(bus, ruleStr, rule, 0, serialNum);
    AJ_Free(ruleStr);
    return status;
}

AJ_Status AJ_GUID_HandleAddMatchReply(AJ_Message* msg)
{
    AJ_Status status;
    NameToGUID* mapping;
    uint32_t serialNum = 0;

    AJ_InfoPrintf(("AJ_GUID_HandleAddMatchReply(msg=0x%p)\n", msg));

    mapping = LookupReplySerial(msg->replySerial);
    if (!mapping) {
        return AJ_OK;
    }
    mapping->replySerial = 0;

    if (msg->hdr->msgType == AJ_MSG_ERROR) {
        AJ_ErrPrintf(("AJ_GUID_HandleAddMatchReply(msg=0x%p): error=%s.\n", msg, msg->error));
        AJ_GUID_DeleteNameMapping(msg->bus, mapping->uniqueName);
        return AJ_ERR_FAILURE;
    }

    /*
     * Add match complete.
     */
    AJ_InfoPrintf(("Add match Complete\n"));
    status = NameHasOwner(msg, mapping->uniqueName, &serialNum);
    if (status == AJ_OK) {
        mapping->replySerial = serialNum;
    } else {
        AJ_GUID_DeleteNameMapping(msg->bus, mapping->uniqueName);
    }
    return status;
}

static AJ_Status NameHasOwner(AJ_Message* msg, const char* name, uint32_t* serialNum)
{
    AJ_Status status;
    AJ_Message call;

    AJ_InfoPrintf(("NameHasOwner(msg=0x%p)\n", msg));

    /*
     * Ask if name has an owner
     */
    status = AJ_MarshalMethodCall(msg->bus, &call, AJ_METHOD_NAME_HAS_OWNER, AJ_DBusDestination, 0, 0, AJ_METHOD_TIMEOUT);
    if (status == AJ_OK) {
        *serialNum = call.hdr->serialNum;
        status = AJ_MarshalArgs(&call, "s", name);
    }
    if (status != AJ_OK) {
        AJ_ErrPrintf(("NameHasOwner(msg=0x%p): Marshal error\n", msg));
        return status;
    }
    return AJ_DeliverMsg(&call);
}

AJ_Status AJ_GUID_HandleNameHasOwnerReply(AJ_Message* msg)
{
    AJ_Status status;
    NameToGUID* mapping;
    uint32_t hasOwner;

    AJ_InfoPrintf(("AJ_GUID_HandleNameHasOwnerReply(msg=0x%p)\n", msg));
    mapping = LookupReplySerial(msg->replySerial);
    if (!mapping) {
        return AJ_OK;
    }
    mapping->replySerial = 0;

    if (msg->hdr->msgType == AJ_MSG_ERROR) {
        AJ_ErrPrintf(("AJ_GUID_HandleNameHasOwnerReply(msg=0x%p): error=%s.\n", msg, msg->error));
        status = AJ_ERR_FAILURE;
        AJ_GUID_DeleteNameMapping(msg->bus, mapping->uniqueName);
        return status;
    }

    status = AJ_UnmarshalArgs(msg, "b", &hasOwner);
    if (status != AJ_OK) {
        AJ_ErrPrintf(("AJ_GUID_HandleNameHasOwnerReply(msg=0x%p): Unmarshal error\n", msg));
        AJ_GUID_DeleteNameMapping(msg->bus, mapping->uniqueName);
        return status;
    }

    /*
     * Name has owner complete.
     */
    AJ_InfoPrintf(("Name %s has owner %d\n", mapping->uniqueName, hasOwner));
    if (!hasOwner) {
        AJ_GUID_DeleteNameMapping(msg->bus, mapping->uniqueName);
    }

    return status;
}

AJ_Status AJ_GUID_HandleNameOwnerChanged(AJ_Message* msg)
{
    AJ_Status status;
    char* name;
    char* oldOwner;
    char* newOwner;

    status = AJ_UnmarshalArgs(msg, "sss", &name, &oldOwner, &newOwner);
    AJ_InfoPrintf(("AJ_GUID_HandleNameOwnerChanged(name=%s,oldOwner=%s,newOwner=%s)\n", name, oldOwner, newOwner));
    if ((status == AJ_OK) && newOwner && oldOwner && newOwner[0] == '\0') {
        AJ_GUID_DeleteNameMapping(msg->bus, oldOwner);
    }
    return status;
}

AJ_Status AJ_GUID_HandleRemoveMatchReply(AJ_Message* msg)
{
    if (msg->hdr->msgType == AJ_MSG_ERROR) {
        AJ_WarnPrintf(("AJ_GUID_HandleRemoveMatchReply(msg=0x%p): error=%s.\n", msg, msg->error));
        return AJ_ERR_FAILURE;
    }
    return AJ_OK;
}
