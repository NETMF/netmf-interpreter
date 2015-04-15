/**
 * @file Functions relating to wifi configuration and initialization
 */
/******************************************************************************
 * Copyright (c) 2014, AllSeen Alliance. All rights reserved.
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

#define AJ_MODULE WIFI


#include "aj_target.h"
#include "aj_wsl_target.h"
#include "aj_util.h"
#include "aj_status.h"
#include "aj_wifi_ctrl.h"
#include "aj_wsl_wmi.h"
#include "aj_debug.h"
#include "aj_target_rtos.h"
#include <stdio.h>

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgWIFI = 0;
#endif

extern AJ_WifiCallbackFunc AJ_WSL_WifiConnectCallback;

static char* AddrStr(uint32_t addr)
{
    static char txt[17];
    sprintf((char*)&txt, "%3lu.%3lu.%3lu.%3lu\0",
            (addr & 0xFF000000) >> 24,
            (addr & 0x00FF0000) >> 16,
            (addr & 0x0000FF00) >>  8,
            (addr & 0x000000FF)
            );

    return txt;
}

static AJ_Status AJ_Network_Up();

#define MAX_SSID_LENGTH                 32


static const uint32_t startIP = 0xC0A80101;
static const uint32_t endIP   = 0xC0A80102;

#define IP_LEASE    (60 * 60 * 1000)

static const uint8_t IP6RoutePrefix[16] = { 0x20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };

#define PREFIX_LEN          64
#define PREFIX_LIFETIME  12000

AJ_WiFiConnectState AJ_WSL_connectState = AJ_WIFI_IDLE;
static uint32_t athSecType = AJ_WIFI_SECURITY_NONE;

AJ_WiFiConnectState AJ_GetWifiConnectState(void)
{
    return AJ_WSL_connectState;
}


#define RSNA_AUTH_FAILURE  10
#define RSNA_AUTH_SUCCESS  16

static void WiFiCallback(int val)
{
    AJ_InfoPrintf(("\nWiFiCallback %d\n", val));
    if (val == 0) {
        if (AJ_WSL_connectState == AJ_WIFI_DISCONNECTING || AJ_WSL_connectState == AJ_WIFI_CONNECT_OK) {
            AJ_WSL_connectState = AJ_WIFI_IDLE;
            AJ_InfoPrintf(("\nWiFi Disconnected\n"));
        } else if (AJ_WSL_connectState != AJ_WIFI_CONNECT_FAILED) {
            AJ_WSL_connectState = AJ_WIFI_CONNECT_FAILED;
            AJ_InfoPrintf(("\nWiFi Connect Failed\n"));
        }
    } else if (val == 1) {
        /*
         * With WEP or no security a callback value == 1 means we are done. In the case of wEP this
         * means there is no way to tell if the association succeeded or failed.
         */
        if ((athSecType == AJ_WIFI_SECURITY_NONE) || (athSecType == AJ_WIFI_SECURITY_WEP)) {
            AJ_WSL_connectState = AJ_WIFI_CONNECT_OK;
            AJ_InfoPrintf(("\nConnected to AP\n"));
        }
    } else if (val == RSNA_AUTH_SUCCESS) {
        AJ_WSL_connectState = AJ_WIFI_CONNECT_OK;
        AJ_InfoPrintf(("\nConnected to AP\n"));
    } else if (val == RSNA_AUTH_FAILURE) {
        AJ_WSL_connectState = AJ_WIFI_AUTH_FAILED;
        AJ_InfoPrintf(("\nWiFi Authentication Failed\n"));
    }

    /*
     * Set up for IPV6
     */
    if (AJ_WSL_connectState == AJ_WIFI_CONNECT_OK) {
        AJ_WSL_NET_ip6config_router_prefix(IP6RoutePrefix, PREFIX_LEN, PREFIX_LIFETIME, PREFIX_LIFETIME);
    }
}

static void SoftAPCallback(int val)
{
    if (val == 0) {
        if (AJ_WSL_connectState == AJ_WIFI_DISCONNECTING || AJ_WSL_connectState == AJ_WIFI_SOFT_AP_UP) {
            AJ_WSL_connectState = AJ_WIFI_IDLE;
            AJ_InfoPrintf(("Soft AP Down\n"));
        } else if (AJ_WSL_connectState == AJ_WIFI_STATION_OK) {
            AJ_WSL_connectState = AJ_WIFI_SOFT_AP_UP;
            AJ_InfoPrintf(("Soft AP Station Disconnected\n"));
        } else {
            AJ_WSL_connectState = AJ_WIFI_CONNECT_FAILED;
            AJ_InfoPrintf(("Soft AP Connect Failed\n"));
        }
    } else if (val == 1) {
        if (AJ_WSL_connectState == AJ_WIFI_SOFT_AP_INIT) {
            AJ_InfoPrintf(("Soft AP Initialized\n"));
            AJ_WSL_connectState = AJ_WIFI_SOFT_AP_UP;
        } else {
            AJ_InfoPrintf(("Soft AP Station Connected\n"));
            AJ_WSL_connectState = AJ_WIFI_STATION_OK;
        }
    }
}

AJ_Status AJ_PrintFWVersion()
{
    AJ_Status status = AJ_OK;
    AJ_FW_Version version;
    AJ_Network_Up();

    extern AJ_FW_Version AJ_WSL_TargetFirmware;
    version = AJ_WSL_TargetFirmware;
    if (status == AJ_OK) {
        AJ_InfoPrintf(("Host version :  %ld.%ld.%ld.%ld.%ld\n",
                       (version.host_ver & 0xF0000000) >> 28,
                       (version.host_ver & 0x0F000000) >> 24,
                       (version.host_ver & 0x00FC0000) >> 18,
                       (version.host_ver & 0x0003FF00) >> 8,
                       (version.host_ver & 0x000000FF)));

        AJ_InfoPrintf(("Target version   :  0x%lx\n", version.target_ver));
        AJ_InfoPrintf(("Firmware version :  %ld.%ld.%ld.%ld.%ld\n",
                       (version.wlan_ver & 0xF0000000) >> 28,
                       (version.wlan_ver & 0x0F000000) >> 24,
                       (version.wlan_ver & 0x00FC0000) >> 18,
                       (version.wlan_ver & 0x0003FF00) >> 8,
                       (version.wlan_ver & 0x000000FF)));
        AJ_InfoPrintf(("Interface version:  %ld\n", version.abi_ver));
    }
    return status;
}

static AJ_Status AJ_ConnectWiFiHelper(const char* ssid, AJ_WiFiSecurityType secType, AJ_WiFiCipherType cipherType, const char* passphrase)
{
    AJ_Status status;
    WSL_NET_AUTH_MODE secMode;
    WSL_NET_CRYPTO_TYPE cipher;

    /*
     * In WifiCallback we need to know if the connection was secure
     */
    athSecType = secType;
    /*
     * Clear the old connection state
     */
    AJ_WSL_connectState = AJ_WIFI_IDLE;
    status = AJ_DisconnectWiFi();
    if (status != AJ_OK) {
        return status;
    }
    /*
     * Set the new SSID
     */
    if (strlen(ssid) > MAX_SSID_LENGTH) {
        AJ_ErrPrintf(("SSID length exceeds Maximum value\n"));
        return AJ_ERR_INVALID;
    }
    /*
     * security mode
     */
    switch (secType) {
    case AJ_WIFI_SECURITY_WEP:
        secMode = WSL_NET_AUTH_NONE;
        break;

    case AJ_WIFI_SECURITY_WPA2:
        secMode = WSL_NET_AUTH_WPA2_PSK;
        break;

    case AJ_WIFI_SECURITY_WPA:
        secMode = WSL_NET_AUTH_WPA_PSK;
        break;

    case AJ_WIFI_SECURITY_NONE:
        secMode = WSL_NET_AUTH_NONE;
        break;

    default:
        secMode = WSL_NET_AUTH_WPA2_PSK;
        break;
    }
    /*
     * Setup the security parameters if needed
     */
    cipher = WSL_NET_CRYPTO_NONE;
    if (athSecType != AJ_WIFI_SECURITY_NONE) {
        uint32_t passLen = strlen(passphrase);
        /*
         * Cipher type - same for unicast and multicast
         */
        switch (cipherType) {
        case AJ_WIFI_CIPHER_WEP:
            cipher = WSL_NET_CRYPTO_WEP;
            break;

        case AJ_WIFI_CIPHER_TKIP:
            cipher = WSL_NET_CRYPTO_TKIP;
            break;

        case AJ_WIFI_CIPHER_CCMP:
            cipher = WSL_NET_CRYPTO_AES;
            break;

        default:
            cipher = WSL_NET_CRYPTO_NONE;
            break;
        }

        if (secType == AJ_WIFI_SECURITY_WEP) {
            AJ_WSL_NET_add_cipher_key(0, (uint8_t*)passphrase, 5);
        }
    }
    /*
     * Set power mode to Max-Perf
     */
    AJ_WSL_NET_SetPowerMode(2);

    /*
     * Set the callback for the connect state
     */
    AJ_WSL_WifiConnectCallback = &WiFiCallback;

    /*
     * Begin the actual connection process
     */
    AJ_WSL_connectState = AJ_WIFI_CONNECTING;

    status = AJ_WSL_NET_connect(ssid, passphrase, secMode, cipher, FALSE);
    return status;
}

#define DHCP_TIMEOUT 5000
AJ_Status AJ_ConnectWiFi(const char* ssid, AJ_WiFiSecurityType secType, AJ_WiFiCipherType cipherType, const char* passphrase)
{
    AJ_WiFiConnectState connectState;
    uint32_t ip;
    uint32_t mask;
    uint32_t gateway;

    AJ_Status status = AJ_Network_Up();
    if (status != AJ_OK) {
        AJ_ErrPrintf(("AJ_ConnectWiFi(): AJ_Network_Up error"));
        return status;
    }

    AJ_SetIPAddress(0, 0, 0);

    status = AJ_ConnectWiFiHelper(ssid, secType, cipherType, passphrase);
    if (status != AJ_OK) {
        AJ_ErrPrintf(("ConfigureWifi error\n"));
        return status;
    }

    /*
     * Poll until we are connected or the connection fails
     */
    connectState = AJ_GetWifiConnectState();
    while (connectState == AJ_WIFI_CONNECTING) {
        AJ_InfoPrintf(("ConnectWiFi Connecting...\n"));
        AJ_Sleep(250);
        connectState = AJ_GetWifiConnectState();
    }

    if (connectState == AJ_WIFI_CONNECT_OK) {
        status = AJ_AcquireIPAddress(&ip, &mask, &gateway, DHCP_TIMEOUT);
        AJ_InfoPrintf(("AJ_ConnectWiFi(): AJ_AcquireIPAddress status=%s\n", AJ_StatusText(status)));
        if (status == AJ_OK) {
            AJ_AlwaysPrintf(("Got IP %s\n", AddrStr(ip)));
        }
    } else if (connectState == AJ_WIFI_CONNECT_FAILED) {
        AJ_ErrPrintf(("ConnectWiFi failed to connect\n"));
        status = AJ_ERR_CONNECT;
    } else if (connectState == AJ_WIFI_AUTH_FAILED) {
        AJ_ErrPrintf(("ConnectWiFi failed to authenticate\n"));
        status = AJ_ERR_SECURITY;
    } else {
        AJ_ErrPrintf(("ConnectWiFi failed\n"));
        status = AJ_ERR_UNKNOWN;
    }

    return status;
}

AJ_Status AJ_DisconnectWiFi(void)
{
    AJ_Status status = AJ_OK;
    AJ_WiFiConnectState oldState = AJ_WSL_connectState;

    if (oldState != AJ_WIFI_DISCONNECTING) {
        /*
         * Commit the changes
         */
        if (oldState != AJ_WIFI_IDLE) {
            AJ_WSL_connectState = AJ_WIFI_DISCONNECTING;
        }
        AJ_WSL_NET_disconnect();
    }
    return status;
}

static AJ_Status AJ_EnableSoftAPHelper(const char* ssid, uint8_t hidden, const char* passphrase)
{
    AJ_Status status = AJ_OK;
    WSL_NET_AUTH_MODE secType = passphrase ?   WSL_NET_AUTH_WPA2_PSK : WSL_NET_AUTH_NONE;
    WSL_NET_CRYPTO_TYPE cipher = WSL_NET_CRYPTO_NONE;

    /*
     * Clear the current connection
     */
    AJ_WSL_connectState = AJ_WIFI_IDLE;
    if (strlen(ssid) > MAX_SSID_LENGTH) {
        AJ_ErrPrintf(("SSID length exceeds Maximum value\n"));
        return AJ_ERR_INVALID;
    }
    /*
     * Set flag to indicate if AP SSID is hidden
     */
    if (hidden) {
        status = AJ_WSL_NET_SetHiddenAP(hidden);
        if (status != AJ_OK) {
            return status;
        }
    }
    /*
     * Set security parameters if AP is not open
     */
    if (secType != WSL_NET_AUTH_NONE) {
        /*
         * Set cipher type to CCMP
         */
        cipher = WSL_NET_CRYPTO_AES;
        /*
         * Set the passphrase
         */
        status = AJ_WSL_NET_SetPassphrase(ssid, passphrase, strlen(passphrase));
        if (status != AJ_OK) {
            return status;
        }
    }
    /*
     * Set the callback for the connect state
     */
    AJ_WSL_WifiConnectCallback = &SoftAPCallback;

    /*
     * Set the IP range for DHCP
     */
    if (AJ_WSL_NET_ipconfig_dhcp_pool(&startIP, &endIP, IP_LEASE) != AJ_OK) {
        return AJ_ERR_DRIVER;
    }

    /*
     * Set up for IPV6
     */
    if (AJ_WSL_NET_ip6config_router_prefix(IP6RoutePrefix, PREFIX_LEN, PREFIX_LIFETIME, PREFIX_LIFETIME) != AJ_OK) {
        return AJ_ERR_DRIVER;
    }

    /*
     * Begin the creation of the SoftAP on the target
     */
    AJ_WSL_connectState = AJ_WIFI_SOFT_AP_INIT;

    AJ_WSL_NET_connect(ssid, passphrase, secType, cipher, TRUE);

    return status;
}


#define SOFTAP_SLEEP_TIMEOUT 100
// block until somebody connects to us or the timeout expires
AJ_Status AJ_EnableSoftAP(const char* ssid, uint8_t hidden, const char* passphrase, const uint32_t timeout)
{
    AJ_Status status = AJ_Network_Up();
    uint32_t time2 = 0;

    if (status != AJ_OK) {
        AJ_ErrPrintf(("AJ_EnableSoftAP(): AJ_Network_Up error"));
        return status;
    }

    status = AJ_EnableSoftAPHelper(ssid, hidden, passphrase);
    if (status != AJ_OK) {
        AJ_ErrPrintf(("AJ_EnableSoftAP error\n"));
        return status;
    }

    /*
     * Wait until a remote station connects
     */
    AJ_InfoPrintf(("Waiting for remote station to connect\n"));

    do {
        AJ_Sleep(SOFTAP_SLEEP_TIMEOUT);
        time2 += SOFTAP_SLEEP_TIMEOUT;
    } while (AJ_GetWifiConnectState() != AJ_WIFI_STATION_OK && (timeout == 0 || time2 < timeout));

    return (AJ_GetWifiConnectState() == AJ_WIFI_STATION_OK) ? AJ_OK : AJ_ERR_TIMEOUT;
}

AJ_Status AJ_WiFiScan(void* context, AJ_WiFiScanResult callback, uint8_t maxAPs)
{
    AJ_Status status = AJ_OK;

    AJ_WSL_RegisterWiFiCallback(context, callback);
    status = AJ_Network_Up();
    if (status != AJ_OK) {
        AJ_ErrPrintf(("AJ_WiFiScan(): AJ_Network_Up error"));
        return status;
    }
    AJ_WSL_InitScanList(maxAPs);
    AJ_WSL_NET_scan();
    AJ_WSL_NET_scan_stop();
    WSL_ClearScanList();
    AJ_WSL_UnregisterWiFiCallback();

    return status;
}

static uint8_t get_tx_status()
{
    return 0;
}

AJ_Status AJ_SuspendWifi(uint32_t msec)
{
    static uint8_t suspendEnabled = FALSE;
    return AJ_OK;
}


static uint8_t wifi_initialized = FALSE;
static uint32_t wifi_state = 0;

static AJ_Status AJ_Network_Up()
{
    AJ_Status status = AJ_OK;

    AJ_WSL_connectState = AJ_WIFI_IDLE;

    if (wifi_initialized == FALSE) {
        AJ_WSL_ModuleInit();
        wifi_initialized = TRUE;
    }


    if (wifi_state == 0) {
        wifi_state = 1;
        /*
         * Initialize the device
         */
        status = AJ_WSL_DriverStart();
        if (status != AJ_OK) {
            AJ_ErrPrintf(("AJ_WSL_DriverStart failed %d\n", status));
            return AJ_ERR_DRIVER;
        } else {
            AJ_WSL_NET_StackInit();
        }
    }

    return status;
}


static AJ_Status AJ_Network_Down()
{
    AJ_Status err;
    AJ_WSL_connectState = AJ_WIFI_IDLE;

    if (wifi_state == 1) {
        wifi_state = 0;
        err = AJ_WSL_DriverStop();
        if (err != AJ_OK) {
            AJ_ErrPrintf(("AJ_WSL_DriverStop failed %d\n", err));
            return AJ_ERR_DRIVER;
        }
    }

    return AJ_OK;
}

AJ_Status AJ_ResetWiFi(void)
{
    AJ_Status status;
    AJ_InfoPrintf(("Reset WiFi driver\n"));
    AJ_WSL_connectState = AJ_WIFI_IDLE;
    status = AJ_Network_Down();
    if (status != AJ_OK) {
        AJ_ErrPrintf(("AJ_ResetWiFi(): AJ_Network_Down failed %s\n", AJ_StatusText(status)));
        return status;
    }
    wifi_initialized = FALSE; // restarting _everything_
    return AJ_Network_Up();
}

AJ_Status AJ_GetIPAddress(uint32_t* ip, uint32_t* mask, uint32_t* gateway)
{
    // set to zero first
    *ip = *mask = *gateway = 0;

    return AJ_WSL_ipconfig(IPCONFIG_QUERY, ip, mask, gateway);
}

#define DHCP_WAIT       100

AJ_Status AJ_AcquireIPAddress(uint32_t* ip, uint32_t* mask, uint32_t* gateway, int32_t timeout)
{
    AJ_Status status;
    AJ_WiFiConnectState current_wifi_state = AJ_GetWifiConnectState();

    switch (current_wifi_state) {
    case AJ_WIFI_CONNECT_OK:
        break;

    // no need to do anything in Soft-AP mode
    case AJ_WIFI_SOFT_AP_INIT:
    case AJ_WIFI_SOFT_AP_UP:
    case AJ_WIFI_STATION_OK:
        return AJ_OK;

    // shouldn't call this function unless already connected!
    case AJ_WIFI_IDLE:
    case AJ_WIFI_CONNECTING:
    case AJ_WIFI_CONNECT_FAILED:
    case AJ_WIFI_AUTH_FAILED:
    case AJ_WIFI_DISCONNECTING:
        return AJ_ERR_DHCP;
    }

    status = AJ_GetIPAddress(ip, mask, gateway);
    if (status != AJ_OK) {
        return status;
    }

    while (0 == *ip) {
        if (timeout < 0) {
            AJ_ErrPrintf(("AJ_AcquireIPAddress(): DHCP Timeout\n"));
            return AJ_ERR_TIMEOUT;
        }

        AJ_InfoPrintf(("Sending DHCP request\n"));
        /*
         * This call kicks off DHCP but we need to poll until the values are populated
         */
        status = AJ_WSL_ipconfig(IPCONFIG_DHCP, ip, mask, gateway);
        if (status != AJ_OK) {
            return AJ_ERR_DHCP;
        }

        AJ_Sleep(DHCP_WAIT);
        status = AJ_GetIPAddress(ip, mask, gateway);
        if (status != AJ_OK) {
            return status;
        }
        timeout -= DHCP_WAIT;
    }

    if (status == AJ_OK) {
        AJ_InfoPrintf(("*********** DHCP succeeded %s\n", AddrStr(*ip)));
    }

    return status;
}

AJ_Status AJ_SetIPAddress(uint32_t ip, uint32_t mask, uint32_t gateway)
{
    return AJ_WSL_ipconfig(IPCONFIG_STATIC, &ip, &mask, &gateway);
}
