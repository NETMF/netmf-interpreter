/**
 * @file Startup sequence for Alljoyn under the WSL build
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

#define AJ_MODULE ALLJOYN
#include "aj_target.h"
#include "aj_wifi_ctrl.h"
#include "aj_debug.h"
#include "aj_wsl_tasks.h"
#include "aj_wsl_wmi.h"
#include "aj_target_platform.h"
/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
AJ_EXPORT uint8_t dbgALLJOYN = 0;
#endif

#define NET_UP_TIMEOUT  5000

extern void AJ_Main();

#ifdef WIFI_SSID
/*
 * This function is provided for testing convenience. Generates a 5 byte WEP hex key from an ascii
 * passphrase. We don't support the 13 byte version which uses an MD5 hash to generate the hex key
 * from the passphrase.
 */
static AJ_Status WEPKey(const char* pwd, uint8_t* hex, uint32_t hexLen)
{
    static const uint32_t WEP_MAGIC1 = 0x343FD;
    static const uint32_t WEP_MAGIC2 = 0x269EC3;
    uint32_t i;
    uint32_t seed;
    uint8_t key[5];

    for (i = 0; *pwd; ++i) {
        seed ^= *pwd++ << (i & 3) << 8;
    }
    for (i = 0; i < (hexLen / 2); ++i) {
        seed = WEP_MAGIC1 * seed + WEP_MAGIC2;
        key[i] = (seed >> 16);
    }
    return AJ_RawToHex(key, sizeof(key), hex, hexLen, FALSE);
}

static AJ_Status ConfigureWifi()
{
    AJ_Status status = AJ_ERR_CONNECT;
    static const char ssid[] = WIFI_SSID;
# ifdef WIFI_PASSPHRASE
    static const char passphrase[] = WIFI_PASSPHRASE;
    const AJ_WiFiSecurityType secType = AJ_WIFI_SECURITY_WPA2;
# else
    const char* passphrase = NULL;
    const AJ_WiFiSecurityType secType = AJ_WIFI_SECURITY_NONE;
# endif

# ifdef WIFI_DEVICE_NAME
    static const char deviceName[] = WIFI_DEVICE_NAME;
    AJ_WSL_NET_set_hostname(deviceName);
#endif

    AJ_AlwaysPrintf(("Trying to connect to AP %s\n", ssid));
    if (secType == AJ_WIFI_SECURITY_WEP) {
        char wepKey[11];
        WEPKey(passphrase, wepKey, sizeof(wepKey));
        status = AJ_ConnectWiFi(ssid, AJ_WIFI_SECURITY_WEP, AJ_WIFI_CIPHER_WEP, wepKey);
    } else {
        status = AJ_ConnectWiFi(ssid, secType, AJ_WIFI_CIPHER_CCMP, passphrase);
    }

    if (status != AJ_OK) {
        AJ_AlwaysPrintf(("ConfigureWifi error\n"));
    }

    if (AJ_GetWifiConnectState() == AJ_WIFI_AUTH_FAILED) {
        AJ_AlwaysPrintf(("ConfigureWifi authentication failed\n"));
        status = AJ_ERR_SECURITY;
    }
    return status;
}

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
#endif

#ifdef SOFTAP_SSID
static AJ_Status ConfigureSoftAP()
{
    AJ_Status status = AJ_ERR_CONNECT;
    static const char ssid[] = SOFTAP_SSID;
#  ifdef SOFTAP_PASSPHRASE
    static const char passphrase[] = SOFTAP_PASSPHRASE;
#  else
    const char* passphrase = NULL;
#  endif
    AJ_AlwaysPrintf(("Configuring soft AP %s\n", ssid));
    status = AJ_EnableSoftAP(ssid, FALSE, passphrase, UINT32_MAX);
    if (status == AJ_ERR_TIMEOUT) {
        AJ_AlwaysPrintf(("AJ_EnableSoftAP timeout\n"));
    } else if (status != AJ_OK) {
        AJ_AlwaysPrintf(("AJ_EnableSoftAP error\n"));
    }
    return status;
}
#endif

#ifdef WIFI_SCAN
static void ScanResult(void* context, const char* ssid, const uint8_t mac[6], uint8_t rssi, AJ_WiFiSecurityType secType, AJ_WiFiCipherType cipherType)
{
    static const char* const sec[] = { "OPEN", "WEP", "WPA", "WPA2" };
    static const char* const typ[] = { "", ":TKIP", ":CCMP", ":WEP" };

    AJ_AlwaysPrintf(("SSID %s [%02x:%02X:%02x:%02x:%02x:%02x] RSSI=%d security=%s%s\n", ssid, mac[0], mac[1], mac[2], mac[3], mac[4], mac[5], rssi, sec[secType], typ[cipherType]));
}
#endif

void AllJoyn_Start(unsigned long arg)
{
    AJ_Status status = AJ_OK;
    AJ_AlwaysPrintf(("\n******************************************************"));
    AJ_AlwaysPrintf(("\n                AllJoyn Thin-Client"));
    AJ_AlwaysPrintf(("\n******************************************************\n"));

    AJ_PrintFWVersion();

    AJ_InfoPrintf(("AllJoyn Version %s\n", AJ_GetVersion()));
#ifdef AJ_CONFIGURE_WIFI_UPON_START
#ifdef WIFI_SCAN
    status = AJ_WiFiScan(NULL, ScanResult, 32);
    if (status != AJ_OK) {
        AJ_AlwaysPrintf(("WiFi scan failed\n"));
    }
#endif

#ifdef WIFI_SSID
    while (1) {
        uint32_t ip, mask, gw;
        status = ConfigureWifi();

        if (status != AJ_OK) {
            AJ_InfoPrintf(("AllJoyn_Start(): ConfigureWifi status=%s", AJ_StatusText(status)));
            continue;
        } else {
            status = AJ_AcquireIPAddress(&ip, &mask, &gw, NET_UP_TIMEOUT);
            AJ_AlwaysPrintf(("Got IP %s\n", AddrStr(ip)));
            if (status != AJ_OK) {
                AJ_InfoPrintf(("AllJoyn_Start(): AJ_AcquireIPAddress status=%s", AJ_StatusText(status)));
            }

            break;
        }
    }
#elif defined SOFTAP_SSID
    status = ConfigureSoftAP();
#endif
#endif
    if (status == AJ_OK) {
        AJ_Main();
    }

    AJ_AlwaysPrintf(("Quitting\n"));
    while (TRUE) {
    }
}

