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
 * prior to first inclusion of aj_debug.h.
 * The corresponding flag dbgAJSVCAPP is defined in the containing sample app.
 */
#define AJ_MODULE AJSVCAPP
#include <aj_debug.h>

#include <alljoyn.h>
#include <errno.h>
#include "PropertyStoreOEMProvisioning.h"
#include <alljoyn/services_common/PropertyStore.h>
#include <alljoyn/services_common/ServicesCommon.h>
#ifdef ONBOARDING_SERVICE
    #include <alljoyn/onboarding/OnboardingManager.h>
#endif
#include <aj_guid.h>
#include <aj_nvram.h>
#include <aj_creds.h>
#include <aj_config.h>

#ifndef NDEBUG
extern AJ_EXPORT uint8_t dbgAJSVCAPP;
#endif

const PropertyStoreEntry propertyStoreProperties[AJSVC_PROPERTY_STORE_NUMBER_OF_KEYS] =
{
//  { "Key Name            ", W, A, M, I .. . . ., P },
    { "DeviceId",             0, 1, 0, 1, 0, 0, 0, 1 },
    { "AppId",                0, 1, 0, 1, 0, 0, 0, 1 },
#ifndef CONFIG_SERVICE
    { "DeviceName",           0, 1, 1, 1, 0, 0, 0, 1 },
// Add other runtime keys above this line
    { "DefaultLanguage",      0, 1, 0, 0, 0, 0, 0, 1 },
#else
    { "DeviceName",           1, 1, 1, 1, 0, 0, 0, 1 },
    { "DefaultLanguage",      1, 1, 0, 0, 0, 0, 0, 1 },
    { "Passcode",             1, 0, 0, 0, 0, 0, 0, 0 },
    { "RealmName",            1, 0, 0, 0, 0, 0, 0, 0 },
// Add other runtime keys above this line
#endif
    { "AppName",              0, 1, 0, 0, 0, 0, 0, 1 },
    { "Description",          0, 0, 1, 0, 0, 0, 0, 1 },
    { "Manufacturer",         0, 1, 1, 0, 0, 0, 0, 1 },
    { "ModelNumber",          0, 1, 0, 0, 0, 0, 0, 1 },
    { "DateOfManufacture",    0, 0, 0, 0, 0, 0, 0, 1 },
    { "SoftwareVersion",      0, 0, 0, 0, 0, 0, 0, 1 },
    { "AJSoftwareVersion",    0, 0, 0, 0, 0, 0, 0, 1 },
#ifdef CONFIG_SERVICE
    { "MaxLength",            0, 1, 0, 0, 0, 0, 0, 1 },
#endif
// Add other mandatory about keys above this line
    { "HardwareVersion",      0, 0, 0, 0, 0, 0, 0, 1 },
    { "SupportUrl",           0, 0, 1, 0, 0, 0, 0, 1 },
// Add other optional about keys above this line
};

static const char* defaultLanguagesKeyName = { "SupportedLanguages" };

static uint8_t numberOfLanguages = 0;

uint8_t AJSVC_PropertyStore_GetNumberOfLanguages()
{
    return numberOfLanguages;
}

uint8_t AJSVC_PropertyStore_GetMaxValueLength(int8_t fieldIndex)
{
    switch (fieldIndex) {
    case AJSVC_PROPERTY_STORE_DEVICE_NAME:
        return DEVICE_NAME_VALUE_LENGTH;

    case AJSVC_PROPERTY_STORE_DEFAULT_LANGUAGE:
        return LANG_VALUE_LENGTH;

#ifdef CONFIG_SERVICE
    case AJSVC_PROPERTY_STORE_PASSCODE:
        return PASSWORD_VALUE_LENGTH;
#endif

    default:
        return KEY_VALUE_LENGTH;
    }
}

const char* AJSVC_PropertyStore_GetFieldName(int8_t fieldIndex)
{
    if (fieldIndex <= AJSVC_PROPERTY_STORE_ERROR_FIELD_INDEX || fieldIndex >= AJSVC_PROPERTY_STORE_NUMBER_OF_KEYS) {
        return "N/A";
    }
    return propertyStoreProperties[fieldIndex].keyName;
}

int8_t AJSVC_PropertyStore_GetFieldIndex(const char* fieldName)
{
    int8_t fieldIndex;

    for (fieldIndex = 0; fieldIndex < AJSVC_PROPERTY_STORE_NUMBER_OF_KEYS; fieldIndex++) {
        if (!strcmp(propertyStoreProperties[fieldIndex].keyName, fieldName)) {
            return fieldIndex;
        }
    }
    return AJSVC_PROPERTY_STORE_ERROR_FIELD_INDEX;
}

static int8_t GetLanguageIndexForProperty(int8_t langIndex, int8_t fieldIndex)
{
    if (propertyStoreProperties[fieldIndex].mode2MultiLng) {
        return langIndex;
    }
    return AJSVC_PROPERTY_STORE_NO_LANGUAGE_INDEX;
}

const char* AJSVC_PropertyStore_GetValueForLang(int8_t fieldIndex, int8_t langIndex)
{
    if (fieldIndex <= AJSVC_PROPERTY_STORE_ERROR_FIELD_INDEX || fieldIndex >= AJSVC_PROPERTY_STORE_NUMBER_OF_KEYS) {
        return NULL;
    }
    langIndex = GetLanguageIndexForProperty(langIndex, fieldIndex);
    if (langIndex <= AJSVC_PROPERTY_STORE_ERROR_LANGUAGE_INDEX || langIndex >= AJSVC_PROPERTY_STORE_NUMBER_OF_LANGUAGES) {
        return NULL;
    }
    if (fieldIndex < AJSVC_PROPERTY_STORE_NUMBER_OF_RUNTIME_KEYS &&
        (propertyStoreProperties[fieldIndex].mode0Write || propertyStoreProperties[fieldIndex].mode3Init) &&
        propertyStoreRuntimeValues[fieldIndex].value != NULL &&
        (propertyStoreRuntimeValues[fieldIndex].value[langIndex]) != NULL &&
        (propertyStoreRuntimeValues[fieldIndex].value[langIndex])[0] != '\0') {
        //AJ_InfoPrintf(("Has key [%s] runtime Value [%s]\n", propertyStoreProperties[fieldIndex].keyName, propertyStoreRuntimeValues[fieldIndex].value[langIndex]));
        return propertyStoreRuntimeValues[fieldIndex].value[langIndex];
    } else if (propertyStoreDefaultValues[fieldIndex] != NULL &&
               (propertyStoreDefaultValues[fieldIndex])[langIndex] != NULL) {
        //AJ_InfoPrintf(("Has key [%s] default Value [%s]\n", propertyStoreProperties[fieldIndex].keyName, (propertyStoreDefaultValues[fieldIndex])[langIndex]));
        return (propertyStoreDefaultValues[fieldIndex])[langIndex];
    }

    return NULL;
}

const char* AJSVC_PropertyStore_GetValue(int8_t fieldIndex)
{
    return AJSVC_PropertyStore_GetValueForLang(fieldIndex, AJSVC_PROPERTY_STORE_NO_LANGUAGE_INDEX);
}

const char* AJSVC_PropertyStore_GetLanguageName(int8_t langIndex)
{
    if (langIndex <= AJSVC_PROPERTY_STORE_ERROR_LANGUAGE_INDEX ||
        langIndex >= AJSVC_PROPERTY_STORE_NUMBER_OF_LANGUAGES) {
        return "N/A";
    }
    return propertyStoreDefaultLanguages[langIndex];
}

int8_t AJSVC_PropertyStore_GetLanguageIndex(const char* const language)
{
    int8_t langIndex;
    const char* search = language;

    if (search != NULL) {
        if (search[0] == '\0') { // Check for empty language, if yes then search for current default language index
            search = AJSVC_PropertyStore_GetValue(AJSVC_PROPERTY_STORE_DEFAULT_LANGUAGE);
            if (search == NULL) {
                return AJSVC_PROPERTY_STORE_ERROR_LANGUAGE_INDEX;
            }
        }
        for (langIndex = 0; langIndex < AJSVC_PROPERTY_STORE_NUMBER_OF_LANGUAGES; langIndex++) {
            if (!strcmp(search, propertyStoreDefaultLanguages[langIndex])) {
                return langIndex;
            }
        }
    }
    return AJSVC_PROPERTY_STORE_ERROR_LANGUAGE_INDEX;
}

uint8_t AJSVC_PropertyStore_SetValueForLang(int8_t fieldIndex, int8_t langIndex, const char* value)
{
    size_t var_size;
    if (fieldIndex <= AJSVC_PROPERTY_STORE_ERROR_FIELD_INDEX ||
        fieldIndex >= AJSVC_PROPERTY_STORE_NUMBER_OF_RUNTIME_KEYS) {
        return FALSE;
    }
    langIndex = GetLanguageIndexForProperty(langIndex, fieldIndex);
    if (langIndex <= AJSVC_PROPERTY_STORE_ERROR_LANGUAGE_INDEX || langIndex >= AJSVC_PROPERTY_STORE_NUMBER_OF_LANGUAGES) {
        return FALSE;
    }
    //AJ_InfoPrintf(("Set key [%s] defaultValue [%s]\n", propertyStoreProperties[fieldIndex].keyName, value));
    var_size = propertyStoreRuntimeValues[fieldIndex].size;
    strncpy(propertyStoreRuntimeValues[fieldIndex].value[langIndex], value, var_size - 1);
    (propertyStoreRuntimeValues[fieldIndex].value[langIndex])[var_size - 1] = '\0';

    return TRUE;
}

uint8_t AJSVC_PropertyStore_SetValue(int8_t fieldIndex, const char* value)
{
    return AJSVC_PropertyStore_SetValueForLang(fieldIndex, AJSVC_PROPERTY_STORE_NO_LANGUAGE_INDEX, value);
}

int8_t AJSVC_PropertyStore_GetCurrentDefaultLanguageIndex()
{
    const char* currentDefaultLanguage = AJSVC_PropertyStore_GetValue(AJSVC_PROPERTY_STORE_DEFAULT_LANGUAGE);
    int8_t currentDefaultLanguageIndex = AJSVC_PropertyStore_GetLanguageIndex(currentDefaultLanguage);
    if (currentDefaultLanguageIndex == AJSVC_PROPERTY_STORE_ERROR_LANGUAGE_INDEX) {
        currentDefaultLanguageIndex = AJSVC_PROPERTY_STORE_NO_LANGUAGE_INDEX;
        AJ_WarnPrintf(("Failed to find default language %s defaulting to %s", (currentDefaultLanguage != NULL ? currentDefaultLanguage : "NULL"), propertyStoreDefaultLanguages[AJSVC_PROPERTY_STORE_NO_LANGUAGE_INDEX]));
    }
    return currentDefaultLanguageIndex;
}

#ifdef CONFIG_SERVICE
static void ClearPropertiesInRAM()
{
    int8_t fieldIndex;
    int8_t langIndex;
    char* buf;

    for (fieldIndex = 0; fieldIndex < AJSVC_PROPERTY_STORE_NUMBER_OF_RUNTIME_KEYS; fieldIndex++) {
        if (propertyStoreRuntimeValues[fieldIndex].value) {
            for (langIndex = 0; langIndex < AJSVC_PROPERTY_STORE_NUMBER_OF_LANGUAGES; langIndex++) {
                if (propertyStoreProperties[fieldIndex].mode2MultiLng || langIndex == AJSVC_PROPERTY_STORE_NO_LANGUAGE_INDEX) {
                    buf = propertyStoreRuntimeValues[fieldIndex].value[langIndex];
                    if (buf) {
                        memset(buf, 0, propertyStoreRuntimeValues[fieldIndex].size);
                    }
                }
            }
        }
    }
}
#endif

static void InitMandatoryPropertiesInRAM()
{
    char* machineIdValue = propertyStoreRuntimeValues[AJSVC_PROPERTY_STORE_APP_ID].value[AJSVC_PROPERTY_STORE_NO_LANGUAGE_INDEX];
    const char* currentAppIdValue = AJSVC_PropertyStore_GetValue(AJSVC_PROPERTY_STORE_APP_ID);
    const char* currentDeviceIdValue = AJSVC_PropertyStore_GetValue(AJSVC_PROPERTY_STORE_DEVICE_ID);
    const char* currentDeviceNameValue;
    int8_t langIndex;
    size_t serialIdLen = 0;
    size_t machineIdLen = 0;
    AJ_GUID theAJ_GUID;
    AJ_Status status;
    char deviceName[DEVICE_NAME_VALUE_LENGTH + 1] = { 0 };

    if (currentAppIdValue == NULL || currentAppIdValue[0] == '\0') {
        status = AJ_GetLocalGUID(&theAJ_GUID);
        if (status == AJ_OK) {
            AJ_GUID_ToString(&theAJ_GUID, machineIdValue, propertyStoreRuntimeValues[AJSVC_PROPERTY_STORE_APP_ID].size);
        }
    }
    if (currentDeviceIdValue == NULL || currentDeviceIdValue[0] == '\0') {
        AJSVC_PropertyStore_SetValue(AJSVC_PROPERTY_STORE_DEVICE_ID, machineIdValue);
    }
    for (langIndex = 0; langIndex < AJSVC_PROPERTY_STORE_NUMBER_OF_LANGUAGES; langIndex++) {
        currentDeviceNameValue = AJSVC_PropertyStore_GetValueForLang(AJSVC_PROPERTY_STORE_DEVICE_NAME, langIndex);
        if (currentDeviceNameValue == NULL || currentDeviceNameValue[0] == '\0') {
            if (deviceName[0] == '\0') {
#ifdef ONBOARDING_SERVICE
                serialIdLen = AJOBS_DEVICE_SERIAL_ID_LEN;
#else
                serialIdLen = 7;
#endif
                machineIdLen = strlen(machineIdValue);
#ifdef _WIN32
                _snprintf(deviceName, DEVICE_NAME_VALUE_LENGTH + 1, "%s %s %s", deviceManufactureName, deviceProductName, &machineIdValue[machineIdLen - min(serialIdLen, machineIdLen)]);
#else
                //snprintf(deviceName, DEVICE_NAME_VALUE_LENGTH + 1, "%s %s %s", deviceManufactureName, deviceProductName, &machineIdValue[machineIdLen - min(serialIdLen, machineIdLen)]);
#endif
            }
            AJSVC_PropertyStore_SetValueForLang(AJSVC_PROPERTY_STORE_DEVICE_NAME, langIndex, deviceName);
        }
    }
}

/*
 * This function is registered with About and handles property store read requests
 */
static AJ_Status AboutPropGetter(AJ_Message* msg, const char* language)
{
    AJ_Status status = AJ_ERR_INVALID;
    int8_t langIndex;
    AJSVC_PropertyStoreCategoryFilter filter;

    memset(&filter, 0, sizeof(AJSVC_PropertyStoreCategoryFilter));

    if (msg->msgId == AJ_SIGNAL_ABOUT_ANNOUNCE) {
        filter.bit2Announce = TRUE;
        langIndex = AJSVC_PropertyStore_GetLanguageIndex(language);
        status = AJ_OK;
    } else if (msg->msgId == AJ_REPLY_ID(AJ_METHOD_ABOUT_GET_ABOUT_DATA)) {
        filter.bit0About = TRUE;
        langIndex = AJSVC_PropertyStore_GetLanguageIndex(language);
        status = (langIndex == AJSVC_PROPERTY_STORE_ERROR_LANGUAGE_INDEX) ? AJ_ERR_UNKNOWN : AJ_OK;
    }
    if (status == AJ_OK) {
        status = AJSVC_PropertyStore_ReadAll(msg, filter, langIndex);
    }
    return status;
}

AJ_Status PropertyStore_Init()
{
    AJ_Status status = AJ_OK;
    const char* const* supportedLanguage = propertyStoreDefaultLanguages;
    numberOfLanguages = 0;
    while (*(supportedLanguage++) != NULL) {
        numberOfLanguages++;
    }
#ifdef CONFIG_SERVICE
    status = AJSVC_PropertyStore_LoadAll();
#endif
    InitMandatoryPropertiesInRAM();
    /*
     * About needs to get values from the property store
     */
    AJ_AboutRegisterPropStoreGetter(AboutPropGetter);
    return status;
}

#ifdef CONFIG_SERVICE
static AJ_Status PropertyStore_ReadConfig(uint16_t index, void* ptr, uint16_t size)
{
    AJ_Status status = AJ_OK;
    uint16_t sizeRead = 0;

    AJ_NV_DATASET* nvramHandle = AJ_NVRAM_Open(index, "r", 0);
    if (nvramHandle != NULL) {
        sizeRead = AJ_NVRAM_Read(ptr, size, nvramHandle);
        status = AJ_NVRAM_Close(nvramHandle);
        if (sizeRead != size) {
            status = AJ_ERR_NVRAM_READ;
        }
    }

    return status;
}

static AJ_Status PropertyStore_WriteConfig(uint16_t index, void* ptr, uint16_t size, char* mode)
{
    AJ_Status status = AJ_OK;
    uint16_t sizeWritten = 0;

    AJ_NV_DATASET* nvramHandle = AJ_NVRAM_Open(index, mode, size);
    if (nvramHandle != NULL) {
        sizeWritten = AJ_NVRAM_Write(ptr, size, nvramHandle);
        status = AJ_NVRAM_Close(nvramHandle);
        if (sizeWritten != size) {
            status = AJ_ERR_NVRAM_WRITE;
        }
    }

    return status;
}

AJ_Status AJSVC_PropertyStore_LoadAll()
{
    AJ_Status status = AJ_OK;
    int8_t langIndex;
    int8_t fieldIndex;
    void* buf = NULL;
    uint16_t size = 0;
    uint16_t entry;

    for (langIndex = 0; langIndex < AJSVC_PROPERTY_STORE_NUMBER_OF_LANGUAGES; langIndex++) {
        for (fieldIndex = 0; fieldIndex < AJSVC_PROPERTY_STORE_NUMBER_OF_RUNTIME_KEYS; fieldIndex++) {
            if (propertyStoreRuntimeValues[fieldIndex].value == NULL ||
                !propertyStoreProperties[fieldIndex].mode0Write ||
                (langIndex != AJSVC_PROPERTY_STORE_NO_LANGUAGE_INDEX && !propertyStoreProperties[fieldIndex].mode2MultiLng)) {
                continue;
            }
            buf = propertyStoreRuntimeValues[fieldIndex].value[langIndex];
            if (buf) {
                size = propertyStoreRuntimeValues[fieldIndex].size;
                entry = (int)fieldIndex + (int)langIndex * (int)AJSVC_PROPERTY_STORE_NUMBER_OF_RUNTIME_KEYS;
                status = PropertyStore_ReadConfig(AJ_PROPERTIES_NV_ID_BEGIN + entry, buf, size);
                //AJ_InfoPrintf(("nvram read fieldIndex=%d [%s] langIndex=%d [%s] entry=%d val=%s size=%u status=%s\n", (int)fieldIndex, propertyStoreProperties[fieldIndex].keyName, (int)langIndex, propertyStoreDefaultLanguages[langIndex], (int)entry, propertyStoreRuntimeValues[fieldIndex].value[langIndex], (int)size, AJ_StatusText(status)));
            }
        }
    }

    return status;
}

AJ_Status AJSVC_PropertyStore_SaveAll()
{
    AJ_Status status = AJ_OK;
    int8_t langIndex;
    int8_t fieldIndex;
    void* buf = NULL;
    uint16_t size = 0;
    uint16_t entry;

    for (langIndex = 0; langIndex < AJSVC_PROPERTY_STORE_NUMBER_OF_LANGUAGES; langIndex++) {
        for (fieldIndex = 0; fieldIndex < AJSVC_PROPERTY_STORE_NUMBER_OF_RUNTIME_KEYS; fieldIndex++) {
            if (propertyStoreRuntimeValues[fieldIndex].value == NULL ||
                !propertyStoreProperties[fieldIndex].mode0Write ||
                (langIndex != AJSVC_PROPERTY_STORE_NO_LANGUAGE_INDEX && !propertyStoreProperties[fieldIndex].mode2MultiLng)) {
                continue;
            }
            buf = propertyStoreRuntimeValues[fieldIndex].value[langIndex];
            if (buf) {
                size = propertyStoreRuntimeValues[fieldIndex].size;
                entry = (int)fieldIndex + (int)langIndex * (int)AJSVC_PROPERTY_STORE_NUMBER_OF_RUNTIME_KEYS;
                status = PropertyStore_WriteConfig(AJ_PROPERTIES_NV_ID_BEGIN + entry, buf, size, "w");
                //AJ_InfoPrintf(("nvram write fieldIndex=%d [%s] langIndex=%d [%s] entry=%d val=%s size=%u status=%s\n", (int)fieldIndex, propertyStoreProperties[fieldIndex].keyName, (int)langIndex, propertyStoreDefaultLanguages[langIndex], (int)entry, propertyStoreRuntimeValues[fieldIndex].value[langIndex], (int)size, AJ_StatusText(status)));
            }
        }
    }
    AJ_AboutSetShouldAnnounce(); // Set flag for sending an updated Announcement

    return status;
}

static uint8_t UpdateFieldInRAM(int8_t fieldIndex, int8_t langIndex, const char* fieldValue)
{
    uint8_t ret = FALSE;

    if (propertyStoreProperties[fieldIndex].mode0Write && propertyStoreProperties[fieldIndex].mode7Public) {
        ret = AJSVC_PropertyStore_SetValueForLang(fieldIndex, langIndex, fieldValue);
    } else {
        AJ_ErrPrintf(("UpdateFieldInRAM ERROR - field %s has read only attribute or is private\n", propertyStoreProperties[fieldIndex].keyName));
    }

    return ret;
}

static uint8_t DeleteFieldFromRAM(AJSVC_PropertyStoreFieldIndices fieldIndex, int8_t langIndex)
{
    return UpdateFieldInRAM(fieldIndex, langIndex, "");
}
#endif

AJ_Status AJSVC_PropertyStore_ReadAll(AJ_Message* msg, AJSVC_PropertyStoreCategoryFilter filter, int8_t langIndex)
{
    AJ_Status status = AJ_OK;
    AJ_Arg array;
    AJ_Arg array2;
    AJ_Arg dict;
    const char* value;
    uint8_t index;
    const char* ajVersion;
    int8_t fieldIndex;

    //AJ_InfoPrintf(("PropertyStore_ReadAll()\n"));

    status = AJ_MarshalContainer(msg, &array, AJ_ARG_ARRAY);
    if (status != AJ_OK) {
        return status;
    }

    for (fieldIndex = 0; fieldIndex < AJSVC_PROPERTY_STORE_NUMBER_OF_KEYS; fieldIndex++) {
#ifdef CONFIG_SERVICE
        if (propertyStoreProperties[fieldIndex].mode7Public && (filter.bit0About || (filter.bit1Config && propertyStoreProperties[fieldIndex].mode0Write) || (filter.bit2Announce && propertyStoreProperties[fieldIndex].mode1Announce))) {
#else
        if (propertyStoreProperties[fieldIndex].mode7Public && (filter.bit0About || (filter.bit2Announce && propertyStoreProperties[fieldIndex].mode1Announce))) {
#endif
            value = AJSVC_PropertyStore_GetValueForLang(fieldIndex, langIndex);

            if (value == NULL && (int8_t)fieldIndex >= (int8_t)AJSVC_PROPERTY_STORE_NUMBER_OF_MANDATORY_KEYS) {     // Non existing values are skipped!
                AJ_WarnPrintf(("PropertyStore_ReadAll - Failed to get value for field=(name=%s, index=%d) and language=(name=%s, index=%d), skipping.\n", AJSVC_PropertyStore_GetFieldName(fieldIndex), (int)fieldIndex, AJSVC_PropertyStore_GetLanguageName(langIndex), (int)langIndex));
            } else {
                if (fieldIndex == AJSVC_PROPERTY_STORE_APP_ID) {
                    if (value == NULL) {
                        AJ_ErrPrintf(("PropertyStore_ReadAll - Failed to get value for mandatory field=(name=%s, index=%d) and language=(name=%s, index=%d), aborting.\n", AJSVC_PropertyStore_GetFieldName(fieldIndex), (int)fieldIndex, AJSVC_PropertyStore_GetLanguageName(langIndex), (int)langIndex));
                        return AJ_ERR_NULL;
                    }
                    status = AJ_MarshalContainer(msg, &dict, AJ_ARG_DICT_ENTRY);
                    if (status != AJ_OK) {
                        return status;
                    }
                    status = AJ_MarshalArgs(msg, "s", propertyStoreProperties[fieldIndex].keyName);
                    if (status != AJ_OK) {
                        return status;
                    }
                    status = AJSVC_MarshalAppIdAsVariant(msg, value);
                    if (status != AJ_OK) {
                        return status;
                    }
                    status = AJ_MarshalCloseContainer(msg, &dict);
                    if (status != AJ_OK) {
                        return status;
                    }
#ifdef CONFIG_SERVICE
                } else if (fieldIndex == AJSVC_PROPERTY_STORE_MAX_LENGTH) {
                    status = AJ_MarshalArgs(msg, "{sv}", propertyStoreProperties[fieldIndex].keyName, "q", DEVICE_NAME_VALUE_LENGTH);
                    if (status != AJ_OK) {
                        return status;
                    }
                    //AJ_InfoPrintf(("Has key [%s] runtime Value [%d]\n", propertyStoreProperties[AJSVC_PROPERTY_STORE_MAX_LENGTH].keyName, DEVICE_NAME_VALUE_LENGTH));
#endif
                } else if (fieldIndex == AJSVC_PROPERTY_STORE_AJ_SOFTWARE_VERSION) {
                    ajVersion = AJ_GetVersion();
                    if (ajVersion == NULL) {
                        AJ_ErrPrintf(("PropertyStore_ReadAll - Failed to get value for mandatory field=(name=%s, index=%d) and language=(name=%s, index=%d), aborting.\n", AJSVC_PropertyStore_GetFieldName(fieldIndex), (int)fieldIndex, AJSVC_PropertyStore_GetLanguageName(langIndex), (int)langIndex));
                        return AJ_ERR_NULL;
                    }
                    status = AJ_MarshalArgs(msg, "{sv}", propertyStoreProperties[fieldIndex].keyName, "s", ajVersion);
                    if (status != AJ_OK) {
                        return status;
                    }
                    //AJ_InfoPrintf(("Has key [%s] runtime Value [%s]\n", propertyStoreProperties[AJSVC_PROPERTY_STORE_AJ_SOFTWARE_VERSION].keyName, ajVersion));
                } else {
                    if (value == NULL) {
                        AJ_ErrPrintf(("PropertyStore_ReadAll - Failed to get value for mandatory field=(name=%s, index=%d) and language=(name=%s, index=%d), aborting.\n", AJSVC_PropertyStore_GetFieldName(fieldIndex), (int)fieldIndex, AJSVC_PropertyStore_GetLanguageName(langIndex), (int)langIndex));
                        return AJ_ERR_NULL;
                    }
                    status = AJ_MarshalArgs(msg, "{sv}", propertyStoreProperties[fieldIndex].keyName, "s", value);
                    if (status != AJ_OK) {
                        return status;
                    }
                }
            }
        }
    }

    if (filter.bit0About) {
        // Add supported languages
        status = AJ_MarshalContainer(msg, &dict, AJ_ARG_DICT_ENTRY);
        if (status != AJ_OK) {
            return status;
        }
        status = AJ_MarshalArgs(msg, "s", defaultLanguagesKeyName);
        if (status != AJ_OK) {
            return status;
        }
        status = AJ_MarshalVariant(msg, "as");
        if (status != AJ_OK) {
            return status;
        }
        status = AJ_MarshalContainer(msg, &array2, AJ_ARG_ARRAY);
        if (status != AJ_OK) {
            return status;
        }

        for (index = 0; index < AJSVC_PROPERTY_STORE_NUMBER_OF_LANGUAGES; index++) {
            status = AJ_MarshalArgs(msg, "s", propertyStoreDefaultLanguages[index]);
            if (status != AJ_OK) {
                return status;
            }
        }

        status = AJ_MarshalCloseContainer(msg, &array2);
        if (status != AJ_OK) {
            return status;
        }
        status = AJ_MarshalCloseContainer(msg, &dict);
        if (status != AJ_OK) {
            return status;
        }
    }
    status = AJ_MarshalCloseContainer(msg, &array);
    if (status != AJ_OK) {
        return status;
    }

    return status;
}

#ifdef CONFIG_SERVICE
AJ_Status AJSVC_PropertyStore_Update(const char* key, int8_t langIndex, const char* value)
{
    int8_t fieldIndex = AJSVC_PropertyStore_GetFieldIndex(key);
    if (fieldIndex <= AJSVC_PROPERTY_STORE_ERROR_FIELD_INDEX || fieldIndex >= AJSVC_PROPERTY_STORE_NUMBER_OF_RUNTIME_KEYS) {
        return AJ_ERR_INVALID;
    }
    if (!UpdateFieldInRAM(fieldIndex, langIndex, value)) {
        return AJ_ERR_FAILURE;
    }
    return AJ_OK;
}

AJ_Status AJSVC_PropertyStore_Reset(const char* key, int8_t langIndex)
{
    int8_t fieldIndex = AJSVC_PropertyStore_GetFieldIndex(key);
    if (fieldIndex <= AJSVC_PROPERTY_STORE_ERROR_FIELD_INDEX || fieldIndex >= AJSVC_PROPERTY_STORE_NUMBER_OF_RUNTIME_KEYS) {
        return AJ_ERR_INVALID;
    }
    if (!DeleteFieldFromRAM(fieldIndex, langIndex)) {
        return AJ_ERR_FAILURE;
    }
    InitMandatoryPropertiesInRAM();
    return AJ_OK;
}

AJ_Status AJSVC_PropertyStore_ResetAll()
{
    ClearPropertiesInRAM();
    InitMandatoryPropertiesInRAM();
    return AJSVC_PropertyStore_SaveAll();
}
#endif
