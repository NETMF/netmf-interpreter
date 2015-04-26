/* Source: "ToasterNotifications.c" */

//#include "ToasterNotifications.h"
//#include "PropertyStoreOEMProvisioning.h"
//#include <alljoyn/services_common/PropertyStore.h>
//#include <alljoyn/services_common/ServicesCommon.h>
//#include <alljoyn/services_common/ServicesHandlers.h>
#include "NotificationCommon.h"
#include "NotificationProducer.h"

/**
 * Notification content
 */
const static char* lang1  = "en";
const static char* hello1 = "Toast is done!";


#define NUM_TEXTS   1
static AJNS_DictionaryEntry textToSend[NUM_TEXTS];

/* Interface: "com.microsoft.sample.toaster" */

/**
 * Initialize the Notifications that will be sent during this sample app
 */


static AJNS_NotificationContent notificationContent;
void InitToasterNotificationContent()
{
    notificationContent.numTexts = NUM_TEXTS;
    textToSend[0].key   = lang1;
    textToSend[0].value = hello1;
    notificationContent.texts = textToSend;

    AJNS_Producer_Start();

}

void SendNotification(AJ_BusAttachment* busAttachment)
{
    uint16_t messageType = AJNS_NOTIFICATION_MESSAGE_TYPE_INFO;
    uint32_t ttl = AJNS_NOTIFICATION_TTL_MIN; /* Note needs to be in the range AJNS_NOTIFICATION_TTL_MIN..AJNS_NOTIFICATION_TTL_MAX */ 

    uint32_t serialNum = 4;

    AJNS_Producer_SendNotification(busAttachment, &notificationContent, messageType, ttl, &serialNum);
}

