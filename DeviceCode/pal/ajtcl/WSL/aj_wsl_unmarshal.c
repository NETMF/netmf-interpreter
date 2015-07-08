/**
 * @file Unmarshaling implementation
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
#include "aj_util.h"
#include "aj_buf.h"
#include <stdarg.h>
#include "aj_debug.h"
#include "aj_wsl_unmarshal.h"
#include "aj_wsl_wmi.h"
#include "aj_wifi_ctrl.h"

#define str(x) # x
#define xstr(x) str(x)

#define PRE  0x10
#define POST 0x00

#define AJ_WSL_SSID_IE  0x00
#define AJ_WSL_RSN_IE   0x30

#define AJ_RSSI_BYTE    15  //Byte location where the signal strength is
#define AJ_MAC_BYTE     16  //Byte location where the mac address is
#define AJ_80211_START  36  //Byte location where the 802.11 IE section begins

wsl_scan_item* WMI_UnmarshalScan(void* data, uint16_t length)
{
    wsl_scan_item* scan;
    uint8_t* ptr;
    uint16_t i = AJ_80211_START; //start of IE section
    uint8_t hasSecurity = 0;
    ptr = (uint8_t*)data;
    scan = (wsl_scan_item*)WSL_InitScanItem();
    /*
     * Get signal strength
     */
    scan->rssi = *(ptr + AJ_RSSI_BYTE);
    /*
     * Get MAC address
     */
    memcpy(scan->bssid, (ptr + AJ_MAC_BYTE), 6);
    /*
     * Go through each IE until the packet it over and get what we need
     * (SSID and RSN).
     */
    while (i < (length - AJ_80211_START)) {
        uint8_t EID;
        uint8_t EIDlen;
        EID = *(ptr + i);
        i++;
        EIDlen = *(ptr + i);
        i++;
        switch (EID) {
        case (0x00):
            scan->ssid = (char*)AJ_WSL_Malloc((sizeof(char) * EIDlen) + 1);
            memcpy(scan->ssid, (ptr + i), EIDlen);
            scan->ssid[EIDlen] = '\0';
            i += EIDlen;
            break;

        case (0x30):
            /*
             * 5 bytes into the RSN IE is the security information
             */
            if (*(ptr + i + 5) == 2) {
                scan->secType = AJ_WIFI_SECURITY_WPA;
                scan->cipherType = AJ_WIFI_CIPHER_TKIP;
            } else if (*(ptr + i + 5) == 4) {
                scan->secType = AJ_WIFI_SECURITY_WPA2;
                scan->cipherType = AJ_WIFI_CIPHER_CCMP;
            } else {
                scan->secType = AJ_WIFI_SECURITY_WEP;
                scan->cipherType = AJ_WIFI_CIPHER_WEP;
            }
            hasSecurity = 1;
            i += EIDlen;
            break;

        default:
            i += EIDlen;
            break;
        }
    }
    if (!hasSecurity) {
        scan->secType = AJ_WIFI_SECURITY_NONE;
        scan->cipherType = AJ_WIFI_CIPHER_NONE;
    }
    return scan;
}

int32_t WMI_Unmarshal(void* data, const char* sig, ...)
{
    va_list args;
    uint8_t* ptr;
    va_start(args, sig);
    ptr = (uint8_t*)data;
    while (*sig) {
        switch (*sig++) {
        case (WMI_ARG_UINT64):
            {
                uint64_t* u64;
                u64 = (uint64_t*)va_arg(args, uint64_t);
                memcpy(u64, ptr, sizeof(uint64_t));
                ptr += 8;
            }
            break;

        case (WMI_ARG_UINT32):
            {
                uint32_t* u32;
                u32 = (uint32_t*)va_arg(args, uint32_t);
                memcpy(u32, ptr, sizeof(uint32_t));
                ptr += 4;
            }
            break;

        case (WMI_ARG_UINT16):
            {
                uint16_t* u16;
                u16 = (uint16_t*)va_arg(args, uint32_t);
                memcpy(u16, ptr, sizeof(uint16_t));
                ptr += 2;
            }
            break;

        case (WMI_ARG_MAC):
            {
                uint8_t* mac;
                mac = (uint8_t*)va_arg(args, uint32_t);
                memcpy(mac, ptr, sizeof(uint8_t) * 6);
                ptr += 6;
            }
            break;

        case (WMI_ARG_IPV4):
            {
                uint8_t* IPv4;
                IPv4 = (uint8_t*)va_arg(args, uint32_t);
                memcpy(IPv4, ptr, sizeof(uint8_t) * 4);
                ptr += 4;
            }
            break;

        case (WMI_ARG_IPV6):
            {
                uint8_t* IPv6;
                IPv6 = (uint8_t*)va_arg(args, uint32_t);
                memcpy(IPv6, ptr, sizeof(uint8_t) * 16);
                ptr += 16;
            }
            break;

        case (WMI_ARG_BYTE):
            {
                uint8_t* u8;
                u8 = (uint8_t*)va_arg(args, uint32_t);
                memcpy(u8, ptr, sizeof(uint8_t));
                ptr += 1;
            }
            break;

        case (WMI_ARG_STRING):
            {
                char** str;
                uint8_t size;
                memcpy(&size, ptr, sizeof(uint8_t));
                ptr++;
                str = (char**)va_arg(args, char*);
                *str = (char*)AJ_WSL_Malloc(sizeof(char) * size + 1);
                memcpy(*str, ptr, sizeof(char) * size + 1);
                (*str)[size] = '\0';
                ptr += size;
            }
            break;
        }
    }
    va_end(args);
    return ptr - (uint8_t*)data;
}
