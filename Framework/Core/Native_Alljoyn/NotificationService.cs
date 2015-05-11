// NotificationService.cs
//
//  Implements alljoyn notification service
//

using System;
using System.Text;
using Microsoft.SPOT;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.AllJoyn.Services
{
    public partial class AJ_Services
    {        
        public class AJNS_Notification
        {
            public UInt16 version;                           // version of notification 
            public UInt16 messageType;                       // messageType One of \ref AJNS_NOTIFICATION_MESSAGE_TYPE_INFO, \ref AJNS_NOTIFICATION_MESSAGE_TYPE_WARNING, or \ref AJNS_NOTIFICATION_MESSAGE_TYPE_EMERGENCY 
            public UInt32 notificationId;                    // notification message id 
            public string originalSenderName;                // originalSenderName The AllJoyn bus unique name of the originating producer application 
            public string deviceId;                          // device id of originating producer application 
            public string deviceName;                        // device name of originating producer application 
            public string appId;                             // application id of originating producer application 
            public string appName;                           // application name of originating producer application 
        };
        
        public class AJNS_MessageTracking
        {
            public UInt32 notificationId;     // notification id 
            public UInt32 serialNum;          // serial number 
        };        
        
        UInt16 AJNS_NotificationVersion = 2;
        const UInt32 AJNS_NUM_MESSAGE_TYPES = 3;
        const UInt16 AJNS_NOTIFICATION_TTL_MIN = 30;
        const UInt16 AJNS_NOTIFICATION_TTL_MAX = 43200;
        
        UInt32 NotificationId = 123456;
        
        public AJNS_MessageTracking [] LastSentNotifications = new AJNS_MessageTracking[3]
        {
            new AJNS_MessageTracking(),
            new AJNS_MessageTracking(),
            new AJNS_MessageTracking()
        };
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status Initialize_NotificationService();
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status SendNotifySignal(UInt32 bus, AJNS_Notification not, UInt32 ttl, string message, ref UInt32 serialNumber);
        
        private bool IsNullOrEmpty(string s)
        {
            if(s == null)
            {
                return true;
            }
            
            if(s == String.Empty)
            {
                return true;
            }
            
            return false;
        }
        
        public AJ_Status SendNotification(UInt32 bus, string message, UInt16 messageType, UInt32 ttl, ref UInt32 messageSerialNumber)
        {
            AJ_Status status;
            AJNS_Notification notification = new AJNS_Notification();

            notification.version = AJNS_NotificationVersion;
            if (messageType >= AJNS_NUM_MESSAGE_TYPES) {
                return AJ_Status.AJ_ERR_DISALLOWED;
            }
            notification.messageType = messageType;

            if ((ttl < AJNS_NOTIFICATION_TTL_MIN) || (ttl > AJNS_NOTIFICATION_TTL_MAX)) {      //ttl is mandatory and must be in range
                return AJ_Status.AJ_ERR_DISALLOWED;
            }

            notification.deviceId   = propertyStore.GetValue((int)PropertyStoreFieldIndices.AJSVC_PROPERTY_STORE_DEVICE_ID);
            notification.deviceName = propertyStore.GetValueForLang((int)PropertyStoreFieldIndices.AJSVC_PROPERTY_STORE_DEVICE_NAME, propertyStore.GetLanguageIndex(""));
            notification.appId      = propertyStore.GetValue((int)PropertyStoreFieldIndices.AJSVC_PROPERTY_STORE_APP_ID);
            notification.appName    = propertyStore.GetValue((int)PropertyStoreFieldIndices.AJSVC_PROPERTY_STORE_APP_NAME);

            if ((IsNullOrEmpty(notification.deviceId)) || (IsNullOrEmpty(notification.deviceName)) ||
                (IsNullOrEmpty(notification.appId)) || (IsNullOrEmpty(notification.appName))) {
                return AJ_Status.AJ_ERR_DISALLOWED;
            }
            
            if (notification.version > 1) {
                notification.originalSenderName = AjInst.GetUniqueName(bus);

                if (IsNullOrEmpty(notification.originalSenderName)) {
                    return AJ_Status.AJ_ERR_DISALLOWED;
                }

            } else {
                notification.originalSenderName = String.Empty;
            }

            if (NotificationId == 0) {
                //NotificationId = GetGUID();
            }

            notification.notificationId = NotificationId;

            UInt32 serialNumber = 0;
            status = SendNotifySignal(bus, notification, ttl, message, ref serialNumber);

            if (status == AJ_Status.AJ_OK) {
                LastSentNotifications[messageType].notificationId = NotificationId++;
                LastSentNotifications[messageType].serialNum = serialNumber;
                messageSerialNumber = serialNumber;
            }

            return status;
        }
        
        AJ_Status DeleteLastNotification(UInt32 busAttachment, UInt16 messageType)
        {
            AJ_Status status;
            UInt32 lastSentSerialNumber;

            if (messageType >= AJNS_NUM_MESSAGE_TYPES) {
                return AJ_Status.AJ_ERR_DISALLOWED;
            }

            lastSentSerialNumber = LastSentNotifications[messageType].serialNum;
            if (lastSentSerialNumber == 0) {
                return AJ_Status.AJ_OK;
            }

            status = AjInst.BusCancelSessionless(busAttachment, lastSentSerialNumber);

            if (status != AJ_Status.AJ_OK) {
                return status;
            }

            LastSentNotifications[messageType].notificationId = 0;
            LastSentNotifications[messageType].serialNum = 0;

            return status;
        }
        
        public AJ_Status CancelNotification(UInt32 busAttachment, UInt32 serialNum)
        {
            AJ_Status status;
            UInt32 messageType = 0;

            if (serialNum == 0) {
                return AJ_Status.AJ_OK;
            }
            
            for (; messageType < AJNS_NUM_MESSAGE_TYPES; messageType++) {
                if (LastSentNotifications[messageType].serialNum == serialNum) {
                    break;
                }
            }
            
            if (messageType >= AJNS_NUM_MESSAGE_TYPES) {
                return AJ_Status.AJ_OK;
            }

            status = AjInst.BusCancelSessionless(busAttachment, serialNum);

            if (status != AJ_Status.AJ_OK) {
                return status;
            }

            LastSentNotifications[messageType].notificationId = 0;
            LastSentNotifications[messageType].serialNum = 0;

            return status;
        }
    }    
}    
