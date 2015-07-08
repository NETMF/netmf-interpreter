#ifndef _AJ_WIFI_CTRL_H
#define _AJ_WIFI_CTRL_H

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
#include "aj_target.h"
#include "aj_status.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef enum {
    AJ_WIFI_IDLE,
    AJ_WIFI_CONNECTING,         /**< Connecting to an AP */
    AJ_WIFI_CONNECT_OK,         /**< Connected to an AP */
    AJ_WIFI_SOFT_AP_INIT,       /**< Initialized Soft AP */
    AJ_WIFI_SOFT_AP_UP,         /**< Soft AP is up and ready for stations to connect */
    AJ_WIFI_STATION_OK,         /**< Station has connected to Soft AP */
    AJ_WIFI_CONNECT_FAILED,     /**< Connneciton to an AP failed */
    AJ_WIFI_AUTH_FAILED,        /**< Authentication failure while connecting to AP */
    AJ_WIFI_DISCONNECTING       /**< Currently disconnecting */

} AJ_WiFiConnectState;


/**
 * Enumeration of WiFi security types. These values are returned in an AllJoyn message so the
 * integer mappings must not be changed.
 */
typedef enum {
    AJ_WIFI_SECURITY_NONE = 0x00,  /**< No security, network is open */
    AJ_WIFI_SECURITY_WEP  = 0x01,  /**< WiFi WEP security */
    AJ_WIFI_SECURITY_WPA  = 0x02,  /**< WiFi WPA Security */
    AJ_WIFI_SECURITY_WPA2 = 0x03   /**< WiFi WPA2 Security */
} AJ_WiFiSecurityType;

/**
 * Enumeration of WiFi cipher types.
 */
typedef enum {
    AJ_WIFI_CIPHER_NONE = 0x00, /**< No cipher specified */
    AJ_WIFI_CIPHER_TKIP = 0x01, /**< Legacy TKIP cipher */
    AJ_WIFI_CIPHER_CCMP = 0x02, /**< CCMP cipher */
    AJ_WIFI_CIPHER_WEP  = 0x03  /**< WEP cipher */
} AJ_WiFiCipherType;

/**
 * Get the current connection state
 */
AJ_WiFiConnectState AJ_GetWifiConnectState(void);

/**
 * Connect to a WiFi access point
 *
 * @param ssid       NUL terminated string with the name of the ssid to connect to
 * @param secType    The type of WiFi security to use.
 * @param cipherType The cipher type to use.
 * @param passphrase The WiFi security passphrase if security is not AJ_WIFI_SECURITY_NONE.
 *
 */
AJ_Status AJ_ConnectWiFi(const char* ssid, AJ_WiFiSecurityType secType, AJ_WiFiCipherType cipherType, const char* passphrase);

/**
 * Print the boards firmware version
 */
AJ_Status AJ_PrintFWVersion();

/**
 * Disconnect from the current WiFi access point
 */
AJ_Status AJ_DisconnectWiFi(void);

/**
 * Enable soft-AP mode
 *
 * @param ssid        The SSID for the AP
 * @param hidden      If TRUE the SSID is not broadcast
 * @param passphrase  The passphrase if secType != AJ_WIFI_SECURITY_NONE
 * @param timeout     Return AJ_ERR_TIMEOUT if nobody connects within <timeout> msec.  0 means wait forever.
 */
AJ_Status AJ_EnableSoftAP(const char* ssid, uint8_t hidden, const char* passphrase, uint32_t timeout);

/*
 * Put the wifi radio to sleep for a while
 *
 * @param msec  Number of milliseconds to sleep radio
 */
AJ_Status AJ_SuspendWifi(uint32_t msec);

/**
 * Function prototype for receiving scan results
 *
 * @param context    The caller's context passed in to AJ_WiFiScan()
 * @param ssid       NUL terminated string containing the SSID of a WiFi AP
 * @param bssid      The MAC address of the WiFi AP
 * @param rssi       The receive signal strength indication
 * @param secType    The security type for the WiFi AP
 * @param cipherType The cipher type used in conjunction with the security type
 *
 */
typedef void (*AJ_WiFiScanResult)(void* context, const char* ssid, const uint8_t bssid[6], uint8_t rssi, AJ_WiFiSecurityType secType, AJ_WiFiCipherType cipherType);

/**
 * Scan for WiFi access points. Will callback for each AP found with the acccess points sorted in
 * descending RSSI. This function returns when the scan is complete.
 *
 * @param context   A caller-provided context to pass into the callback function
 * @param callback  A function to be called for each WiFi AP found during the scan.
 * @param maxAPs    The maximum number of APs to return via the callback function.
 *
 * @return AJ_OK if the scan completed succesfully.
 */
AJ_Status AJ_WiFiScan(void* context, AJ_WiFiScanResult callback, uint8_t maxAPs);

/**
 * Reset the WiFi driver
 */
AJ_Status AJ_ResetWiFi(void);

/**
 * Get an IP address from DHCP and return it
 *
 * @return        Return AJ_Status
 */
AJ_Status AJ_AcquireIPAddress(uint32_t* ip, uint32_t* mask, uint32_t* gateway, int32_t timeout);

/**
 * Return the current IP
 *
 * @return        Return AJ_Status
 */
AJ_Status AJ_GetIPAddress(uint32_t* ip, uint32_t* mask, uint32_t* gateway);

/**
 * Set the system's IP address
 *
 * @return        Return AJ_Status
 */
AJ_Status AJ_SetIPAddress(uint32_t ip, uint32_t mask, uint32_t gateway);

#ifdef __cplusplus
}
#endif

#endif
