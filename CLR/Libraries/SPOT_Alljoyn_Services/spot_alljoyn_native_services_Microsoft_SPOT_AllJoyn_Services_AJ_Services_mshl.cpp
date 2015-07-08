////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\SPOT_Alljoyn\spot_alljoyn.h"
#include "spot_alljoyn_native_services.h"

//--//

#define AJSVC_PROPERTY_STORE_NUMBER_OF_KEYS 13

typedef struct _PropertyStoreRuntimeEntry {
    char** value;
    uint8_t size;
} PropertyStoreConfigEntry;

// externs required to compile base_tcl. However, they are not used,
// so OK to make them NULL

const char*** propertyStoreDefaultValues = NULL;
const char* const* propertyStoreDefaultLanguages = NULL;
const char* deviceProductName = NULL;
const char* deviceManufactureName = NULL;
PropertyStoreConfigEntry* propertyStoreRuntimeValues = NULL;

AJ_Status AJNS_Producer_SendNotifySignal(AJ_BusAttachment* busAttachment, AJNS_Notification* notification, uint32_t ttl, uint32_t* messageSerialNumber);
AJ_Status InitNotificationContent();

const static char* langEn  = "en";

#define AJ_InfoPrintf(_msg)
#define NUM_TEXTS   1

static AJNS_DictionaryEntry     textToSend[NUM_TEXTS];
static AJNS_NotificationContent notificationContent;


AJ_Status InitNotificationContent( const char* text )
{
    notificationContent .numTexts   = NUM_TEXTS;
    textToSend[0]       .key        = langEn;
    textToSend[0]       .value      = text;
    notificationContent.texts       = textToSend;

    return AJNS_Producer_Start();    
}

HRESULT Library_spot_alljoyn_native_services_Microsoft_SPOT_AllJoyn_Services_AJ_Services::Initialize_NotificationService___MicrosoftSPOTAlljoynMicrosoftSPOTAllJoynAJStatus__STRING( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    AJ_Status status = AJ_OK; 

    LPCSTR text = stack.Arg1().RecoverString(); 

    status = InitNotificationContent( text );

    stack.SetResult_I4( status );    
    
    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_alljoyn_native_services_Microsoft_SPOT_AllJoyn_Services_AJ_Services::SendNotifySignal___MicrosoftSPOTAlljoynMicrosoftSPOTAllJoynAJStatus__U4__MicrosoftSPOTAllJoynServicesAJServicesAJNSNotification__U4__STRING__BYREF_U4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER(); 

    //--//
    
    typedef Library_spot_alljoyn_native_services_Microsoft_SPOT_AllJoyn_Services_AJ_Services__AJNS_Notification Managed_AJNS_Notification;

    CLR_UINT32        ttl;
    CLR_RT_HeapBlock  hbSerialNumber;    
    CLR_UINT32        serialNumber;    
    CLR_RT_HeapBlock* managedObj = NULL;
    AJNS_Notification ntf        = {0};
    AJ_BusAttachment* bus        =  (AJ_BusAttachment *)stack.Arg1().NumericByRef().u4;
    AJ_Status         status = AJ_OK;
    
    managedObj = stack.Arg2().Dereference();  FAULT_ON_NULL(managedObj);
    
    ntf.version             = managedObj[Managed_AJNS_Notification::FIELD__version]           .NumericByRef().u2;
    ntf.messageType         = managedObj[Managed_AJNS_Notification::FIELD__messageType]       .NumericByRef().u2;
    ntf.notificationId      = managedObj[Managed_AJNS_Notification::FIELD__notificationId]    .NumericByRef().s4;
    ntf.originalSenderName  = managedObj[Managed_AJNS_Notification::FIELD__originalSenderName].DereferenceString()->StringText();
    ntf.deviceId            = managedObj[Managed_AJNS_Notification::FIELD__deviceId]          .DereferenceString()->StringText();
    ntf.deviceName          = managedObj[Managed_AJNS_Notification::FIELD__deviceName]        .DereferenceString()->StringText();
    ntf.appId               = managedObj[Managed_AJNS_Notification::FIELD__appId]             .DereferenceString()->StringText();
    ntf.appName             = managedObj[Managed_AJNS_Notification::FIELD__appName]           .DereferenceString()->StringText();
    ntf.content             = &notificationContent;
    
    ttl = stack.Arg3().NumericByRef().u4;
    
    TINYCLR_CHECK_HRESULT(hbSerialNumber.LoadFromReference( stack.Arg4() ));    
    serialNumber = hbSerialNumber.NumericByRef().u4;

    //--//
    
    textToSend[0].key   = langEn;
    textToSend[0].value = stack.Arg4().RecoverString();

    status = AJNS_Producer_SendNotifySignal(bus, &ntf, ttl, &serialNumber);

    hbSerialNumber.SetInteger( serialNumber ); 

    TINYCLR_CHECK_HRESULT( hbSerialNumber.StoreToReference( stack.Arg4(), 0 ));

    stack.SetResult_I4( status );    

    TINYCLR_NOCLEANUP();
}

