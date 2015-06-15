/**
 * @file Network functionality implementation
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

#define AJ_MODULE WSL_NET


#include "aj_target.h"
#include "aj_buf.h"
#include "aj_wsl_target.h"
#include "aj_wsl_spi_constants.h"
#include "aj_wsl_wmi.h"
#include "aj_wsl_htc.h"
#include "aj_wsl_net.h"
#include "aj_wsl_unmarshal.h"
#include "aj_wsl_marshal.h"
#include "aj_wifi_ctrl.h"
#include "aj_debug.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgWSL_NET = 0;
#endif

/*
 * Timeout for all network calls that don't have an explicit timeout passed in
 */
#define AJ_NET_TIMEOUT      2000
/*
 * Maximum timeout for DHCP
 */
#define AJ_IPCONFIG_TIMEOUT 60000
#define AJ_WSL_AF_INET 2
#define AJ_WSL_TCP 1
#define AJ_WSL_UDP 2

extern uint32_t AJ_WSL_MBOX_BLOCK_SIZE;

wsl_socket_context AJ_WSL_SOCKET_CONTEXT[5];

static wsl_scan_list list;

static AJ_WiFiScanResult wifiCallback = NULL;
static void* wifiContext;
static uint8_t maxAPs;

void AJ_WSL_RegisterWiFiCallback(void* context, AJ_WiFiScanResult callback)
{
    wifiContext = context;
    wifiCallback = callback;
}

void AJ_WSL_UnregisterWiFiCallback(void)
{
    wifiCallback = NULL;
}

/*
 * Callback function for BSSINFO packets
 */
void AJ_WSL_BSSINFO_Recv(AJ_BufNode* node)
{
    wsl_scan_item* item;
    item = (wsl_scan_item*)WMI_UnmarshalScan(node->bufferStart, node->length);
    memcpy(&list.items[list.size], item, sizeof(wsl_scan_item));

    if (wifiCallback) {
        wifiCallback(wifiContext, list.items[list.size].ssid, list.items[list.size].bssid, list.items[list.size].rssi, list.items[list.size].secType, list.items[list.size].cipherType);
    }
    AJ_WSL_Free(item);
    list.size++;
}

void WSL_ClearScanList()
{
    int i;
    for (i = 0; i < list.size; i++) {
        AJ_WSL_Free(list.items[i].ssid);
    }
    AJ_WSL_Free(list.items);
    list.size = 0;
}

wsl_scan_item* WSL_InitScanItem(void)
{
    wsl_scan_item* item;
    item = (wsl_scan_item*)AJ_WSL_Malloc(sizeof(wsl_scan_item));
    return item;
}

void AJ_WSL_InitScanList(uint8_t maxAP)
{
    maxAPs = maxAP;
    list.items = AJ_WSL_Malloc(sizeof(wsl_scan_item) * maxAPs);
    list.size = 0;
}

wsl_scan_item* AJ_WSL_GetScanList(void)
{
    return (wsl_scan_item*)&list;
}

int list_compare(const void* a, const void* b)
{
    wsl_scan_item* wsl_a = (wsl_scan_item*)a;
    wsl_scan_item* wsl_b = (wsl_scan_item*)b;
    return wsl_b->rssi - wsl_a->rssi;
}

void WSL_PrintScan(void)
{
    int i;
    for (i = 0; i < list.size; i++) {
        AJ_AlwaysPrintf(("%-17.17s ", list.items[i].ssid));
        AJ_AlwaysPrintf(("RSSI: %u ", list.items[i].rssi));
        AJ_AlwaysPrintf(("BSSID: %02x:%02x:%02x:%02x:%02x:%02x\n",
                         list.items[i].bssid[0],
                         list.items[i].bssid[1],
                         list.items[i].bssid[2],
                         list.items[i].bssid[3],
                         list.items[i].bssid[4],
                         list.items[i].bssid[5]));
    }
}

void WSL_PrintScanSorted(void)
{
    qsort(&list.items[0], list.size, sizeof(wsl_scan_item), list_compare);
    WSL_PrintScan();
}

void AJ_WSL_WMI_PadPayload(AJ_BufList* bssfilter)
{
    AJ_BufNode* node;
    uint16_t sizeTail;
    sizeTail = (AJ_BufListLengthOnWire(bssfilter) % AJ_WSL_MBOX_BLOCK_SIZE);
    if (sizeTail) {
        node = AJ_BufListCreateNode(AJ_WSL_MBOX_BLOCK_SIZE - sizeTail);
        AJ_BufListPushTail(bssfilter, node);
    }
}

void AJ_WSL_NET_BSS_FILTER(uint8_t flag) {
    AJ_InfoPrintf(("AJ_WSL_NET_BSS_FILTER(): SET_BSS_FILTER\n"));
    AJ_BufList* bssfilter;
    bssfilter = AJ_BufListCreate();
    WSL_MarshalPacket(bssfilter, WSL_SET_BSS_FILTER, 0, flag, 0, 0, 0);
    WMI_MarshalHeader(bssfilter, 1, 1);
    AJ_WSL_WMI_PadPayload(bssfilter);
    AJ_WSL_WMI_QueueWorkItem(0, WSL_SET_BSS_FILTER, AJ_WSL_HTC_DATA_ENDPOINT1, bssfilter);
}

void AJ_WSL_SetProbedSSID(const char* ssid, uint8_t flag)
{
    AJ_BufList* probed_ssid;
    probed_ssid = AJ_BufListCreate();
    WSL_MarshalPacket(probed_ssid, WSL_SET_PROBED_SSID, 0, 0, flag, strlen(ssid), ssid);
    WMI_MarshalHeader(probed_ssid, 1, 1);
    AJ_WSL_WMI_PadPayload(probed_ssid);
    AJ_WSL_WMI_QueueWorkItem(0, WSL_SET_PROBED_SSID, AJ_WSL_HTC_DATA_ENDPOINT1, probed_ssid);
}

void AJ_WSL_NET_add_cipher_key(uint8_t keyIndex, uint8_t* key, uint8_t keyLength)
{
    AJ_BufList* add_key;
    add_key = AJ_BufListCreate();
    static const uint8_t zero_mac[6] = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
    WSL_MarshalPacket(add_key, WSL_ADD_CIPHER_KEY, 0, 0, 0x02, 0x03, keyLength, 0, 0, key, 0x03, &zero_mac);
    WMI_MarshalHeader(add_key, 1, 1);
    AJ_WSL_WMI_PadPayload(add_key);
    AJ_WSL_WMI_QueueWorkItem(0, WSL_ADD_CIPHER_KEY, AJ_WSL_HTC_DATA_ENDPOINT1, add_key);

}

void AJ_WSL_NET_set_scan_params(void)
{
    AJ_BufList* packet;
    AJ_InfoPrintf(("AJ_WSL_NET_scan(): SET_SCAN_PARAMS\n"));
    packet = AJ_BufListCreate();
    WSL_MarshalPacket(packet, WSL_SET_SCAN_PARAMS, 0, 0x0008, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x00, 0x00, 0x0000, 0x2f03, 0x00000000);
    WMI_MarshalHeader(packet, 1, 1);
    AJ_WSL_WMI_PadPayload(packet);
    AJ_WSL_WMI_QueueWorkItem(0, WSL_SET_SCAN_PARAMS, AJ_WSL_HTC_DATA_ENDPOINT1, packet);
}

AJ_Status AJ_WSL_NET_scan(void)
{
    AJ_Status status;
    AJ_BufList* start_scan;
    wsl_work_item* item;

    AJ_WSL_NET_BSS_FILTER(1);

    AJ_WSL_NET_set_scan_params();

    AJ_InfoPrintf((("AJ_WSL_NET_scan(): START_SCAN\n")));
    start_scan = AJ_BufListCreate();
    WSL_MarshalPacket(start_scan, WSL_START_SCAN, 0, 0, 0, 0, 0, 0, 0, 0);
    WMI_MarshalHeader(start_scan, 1, 1);
    AJ_WSL_WMI_PadPayload(start_scan);

    AJ_WSL_WMI_QueueWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_NETWORK, WSL_NET_SCAN), AJ_WSL_HTC_DATA_ENDPOINT1, start_scan);
    status = AJ_WSL_WMI_WaitForWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_NETWORK, WSL_NET_SCAN), &item, AJ_NET_TIMEOUT);
    if (item && (status == AJ_OK)) {
        if (item->itemType == WSL_NET_SCAN) {
            AJ_InfoPrintf(("AJ_WSL_NET_scan(): WORK ITEM RECEIVED\n"));
            uint16_t WMIEvent;
            uint32_t toss;
            uint32_t error;
            WMI_Unmarshal(item->node->buffer, "quu", &WMIEvent, &toss, &error);
            if (error != 0) {
                AJ_ErrPrintf(("AJ_WSL_NET_scan(): Scan error, scan returned: %u", error));
                status = AJ_ERR_INVALID;
            }
        } else {
            AJ_WarnPrintf(("AJ_WSL_NET_scan(): BAD WORK ITEM RECEIVED\n"));
        }
        AJ_WSL_WMI_FreeWorkItem(item);
    }
    return status;
}

void AJ_WSL_NET_scan_stop(void)
{
    AJ_InfoPrintf(("AJ_WSL_NET_scan_stop(): SET_BSS_FILTER\n"));
    AJ_WSL_NET_BSS_FILTER(0);
}

AJ_Status AJ_WSL_NET_SetPassphrase(const char* SSID, const char* passphrase, uint32_t passLen)
{
    AJ_Status status = AJ_OK;
    AJ_BufList* passphraseList;
    passphraseList = AJ_BufListCreate();
    uint8_t* hexPassphrase = NULL;

    if (passLen == 64) {
        hexPassphrase = (uint8_t*)AJ_WSL_Malloc(32);
        status = AJ_HexToRaw(passphrase, 64, hexPassphrase, 32);
        if (status == AJ_OK) {
            WSL_MarshalPacket(passphraseList, WMI_SET_PMK, 0, hexPassphrase, 32);
        }
    } else {
        WSL_MarshalPacket(passphraseList, WSL_SET_PASSPHRASE, 0, SSID, passphrase, strlen(SSID), passLen);
    }
    if (status == AJ_OK) {
        WMI_MarshalHeader(passphraseList, 1, 1);
        AJ_InfoPrintf(("AJ_WSL_NET_SetPassphrase(): SET_PASSPHRASE\n"));
        AJ_WSL_WMI_PadPayload(passphraseList);
        //AJ_BufListPrintDumpContinuous(passphraseList);
        status = AJ_WSL_WMI_QueueWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_NETWORK, WSL_SET_PASSPHRASE), AJ_WSL_HTC_DATA_ENDPOINT1, passphraseList);
    }
    if (hexPassphrase != NULL) {
        AJ_MemZeroSecure(hexPassphrase, 32);
        AJ_WSL_Free(hexPassphrase);
    }
    return status;
}

AJ_Status AJ_WSL_NET_SetHiddenAP(uint8_t hidden)
{
    AJ_Status status;
    AJ_BufList* hiddenList;
    hiddenList = AJ_BufListCreate();

    WSL_MarshalPacket(hiddenList, WSL_SET_HIDDEN_AP, 0, hidden);
    WMI_MarshalHeader(hiddenList, 1, 1);
    AJ_InfoPrintf(("AJ_WSL_NET_SetHiddenAP(): AP_HIDDEN_SSID\n"));
    AJ_WSL_WMI_PadPayload(hiddenList);
    //AJ_BufListPrintDumpContinuous(passphraseList);
    status = AJ_WSL_WMI_QueueWorkItem(0, WSL_SET_HIDDEN_AP, AJ_WSL_HTC_DATA_ENDPOINT1, hiddenList);
    return status;
}


AJ_Status AJ_WSL_NET_SetPowerMode(uint8_t mode)
{
    AJ_Status status;
    AJ_BufList* power_mode;
    power_mode = AJ_BufListCreate();
    WSL_MarshalPacket(power_mode, WSL_SET_POWER_MODE, 0, mode);
    WMI_MarshalHeader(power_mode, 1, 1);

    AJ_WSL_WMI_PadPayload(power_mode);
    //AJ_BufListPrintDumpContinuous(power_mode);
    status = AJ_WSL_WMI_QueueWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_NETWORK, WSL_SET_POWER_MODE), AJ_WSL_HTC_DATA_ENDPOINT1, power_mode);
    return status;
}

void AJ_WSL_NET_StackInit(void)
{
    AJ_InfoPrintf(("AJ_WSL_NET_StackInit(): STACK_INIT\n"));
    AJ_BufList* stack_init;
    stack_init = AJ_BufListCreate();

    WSL_MarshalPacket(stack_init, WSL_SOCKET, WSL_SOCK_STACK_INIT, 0x04, 0x01, 0x05, 0x08, 0x1f);
    WMI_MarshalHeader(stack_init, 1, 1);
    AJ_WSL_WMI_PadPayload(stack_init);
    //AJ_BufListPrintDumpContinuous(stack_init);
    AJ_WSL_WMI_QueueWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_STACK_INIT), AJ_WSL_HTC_DATA_ENDPOINT1, stack_init);
}

/*
 * Get an AP's mac address from a SSID scan list
 */
static uint8_t getMacFromSSID(const char* ssid, uint8_t* mac, wsl_scan_list* list)
{
    int i;
    for (i = 0; i < list->size; i++) {
        if (0 == strcmp(list->items[i].ssid, ssid)) {
            memcpy(mac, list->items[i].bssid, 6);
            return 1;
        }
    }
    return 0;
}

#define AJ_WSL_CONNECT_WAIT 500
#define AJ_WSL_CONNECT_TIMEOUT 20000

AJ_EXPORT AJ_Status AJ_WSL_NET_connect(const char* SSID, const char* passphrase, WSL_NET_AUTH_MODE auth, WSL_NET_CRYPTO_TYPE crypto, uint8_t softAP)
{
    AJ_Status status = AJ_OK;
    wsl_scan_list* list;
    list = (wsl_scan_list*)AJ_WSL_GetScanList();

    AJ_WSL_NET_SetPowerMode(2);

    uint8_t bss_mac[6];
    // Open auth does not require you to explicitly set the BSSID so this secondary scan is not needed

    if (!softAP) {
        if (auth != WSL_NET_AUTH_NONE) {
            AJ_Time timer;
            uint8_t found = 0;
            AJ_InitTimer(&timer);
            while (!found) {
                AJ_WSL_InitScanList(AJ_WSL_SCAN_LIST_SIZE);
                AJ_WSL_SetProbedSSID(SSID, 1);
                status = AJ_WSL_NET_scan();
                // Some kind of scan error
                if (status != AJ_OK) {
                    WSL_ClearScanList();
                    continue;
                }
                AJ_WSL_NET_scan_stop();
                if (AJ_GetElapsedTime(&timer, TRUE) > AJ_WSL_CONNECT_TIMEOUT) {
                    AJ_ErrPrintf(("AJ_WSL_NET_connect() Could not find the access point %s\n", SSID));
                    WSL_ClearScanList();
                    return AJ_ERR_FAILURE;
                }
                // Find the SSID you want to connect to in the second scan list
                if (getMacFromSSID(SSID, bss_mac, list)) {
                    WSL_ClearScanList();
                    found = 1;
                    break;
                } else {
                    WSL_ClearScanList();
                    AJ_Sleep(AJ_WSL_CONNECT_WAIT);
                    continue;
                }
            }
            if (crypto != WSL_NET_CRYPTO_WEP) {
                status = AJ_WSL_NET_SetPassphrase(SSID, passphrase, strlen(passphrase));
                if (status != AJ_OK) {
                    return status;
                }
            }
        }
    }
    {
        AJ_BufList* connect;
        AJ_BufList* connectOut;
        static const uint8_t zero_mac[6] = { 0, 0, 0, 0, 0, 0 };
        uint8_t connect_mac[6];
        wsl_work_item* item = NULL;
        connect = AJ_BufListCreate();
        /* Three different ways connect can be called.
         * 1. SoftAP: The devices mac is fetched and used
         * 2. Using Auth: The SSID's mac is found and used
         * 3. Open auth: A zero'ed mac is used
         */
        if (softAP) {
            WSL_MarshalPacket(connect, WSL_SET_SOFT_AP, 0, 0x04, 0x01, auth, crypto, 0x00, crypto, 0x00, strlen(SSID), SSID, 0x0, getDeviceMac(), 0x0044, 0x0000);
            WMI_MarshalHeader(connect, 1, 1);
        } else if ((auth != WSL_NET_AUTH_NONE) && (crypto != WSL_NET_CRYPTO_WEP)) {
            WSL_MarshalPacket(connect, WSL_CONNECT, 0, 0x01, 0x01, auth, crypto, 0x00, crypto, 0x00, strlen(SSID), SSID, 0x0, &bss_mac, 0x0044, 0x0000);
            WMI_MarshalHeader(connect, 1, 1);
        } else { // if the auth mode is open, use zero_mac, and set flags to zero
            WSL_MarshalPacket(connect, WSL_CONNECT, 0, 0x01, 0x01, auth, crypto, 0x00, crypto, 0x00, strlen(SSID), SSID, 0x0, &zero_mac, 0x0000, 0x0000);
            WMI_MarshalHeader(connect, 1, 1);
        }
        AJ_InfoPrintf(("AJ_WSL_NET_connect(): CONNECT\n"));
        AJ_WSL_WMI_PadPayload(connect);
        //AJ_BufListPrintDumpContinuous(connect);
        connectOut = AJ_BufListCreateCopy(connect);

        AJ_WSL_WMI_QueueWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_NETWORK, WSL_NET_CONNECT), AJ_WSL_HTC_DATA_ENDPOINT1, connectOut);

        if (softAP) {
            AJ_AlwaysPrintf(("Waiting for a connection to the softAP %s\n", SSID));
            memcpy(&connect_mac, (uint8_t*)getDeviceMac(), sizeof(connect_mac));
            while (memcmp((uint8_t*)&connect_mac, (uint8_t*)getDeviceMac(), 6) == 0) {
                status = AJ_WSL_WMI_WaitForWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_NETWORK, WSL_NET_CONNECT), &item, AJ_TIMER_FOREVER);
                if (item && (status == AJ_OK)) {
                    if (item->itemType == WSL_NET_CONNECT) {
                        AJ_InfoPrintf(("AJ_WSL_NET_connect(): WORK ITEM RECEIVED\n"));
                        uint16_t WMIEvent;
                        uint32_t toss;
                        uint16_t channel;
                        WMI_Unmarshal(item->node->buffer, "quqM", &WMIEvent, &toss, &channel, &connect_mac);
                    } else {
                        AJ_WarnPrintf(("AJ_WSL_NET_connect(): BAD WORK ITEM RECEIVED\n"));
                    }
                    AJ_WSL_WMI_FreeWorkItem(item);
                }
            }
        } else {
            status = AJ_WSL_WMI_WaitForWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_NETWORK, WSL_NET_CONNECT), &item, AJ_TIMER_FOREVER);
            if (item && (status == AJ_OK)) {
                AJ_WSL_WMI_FreeWorkItem(item);
            }
        }
        AJ_BufListFree(connect, 1);
        return status;
    }
}
void AJ_WSL_PrintIP(uint32_t ip)
{
    uint8_t* ptr;
    ptr = (uint8_t*)&ip;
    AJ_AlwaysPrintf(("%i.%i.%i.%i\n", *(ptr + 3), *(ptr + 2), *(ptr + 1), *(ptr)));
}
AJ_Status AJ_WSL_ip6config(uint32_t mode, uint8_t* globalAddr, uint8_t* localAddr, uint8_t* gateway, uint8_t* exAddr, uint32_t linkPrefix, uint32_t globalPrefix, uint32_t gwPrefix, uint32_t glbPrefixExt)
{
    AJ_Status status;
    if (mode != IPCONFIG_QUERY) {
        AJ_ErrPrintf(("AJ_WSL_ip6config(): Can only query IPV6, cannot use mode %lu\n", mode));
        return AJ_ERR_UNEXPECTED;
    }
    AJ_BufList* ip6config;
    ip6config = AJ_BufListCreate();                                                 //First zeros are for v4
    WSL_MarshalPacket(ip6config, WSL_SOCKET, WSL_SOCK_IP6CONFIG, 0x60, 0, 0, 0, 0, globalAddr, localAddr, gateway, exAddr, linkPrefix, globalPrefix, gwPrefix, glbPrefixExt);
    WMI_MarshalHeader(ip6config, 1, 1);
    AJ_WSL_WMI_PadPayload(ip6config);

    //AJ_BufListPrintDumpContinuous(ip6config);
    AJ_WSL_WMI_QueueWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_IP6CONFIG), AJ_WSL_HTC_DATA_ENDPOINT1, ip6config);

    {
        wsl_work_item* item = NULL;

        status = AJ_WSL_WMI_WaitForWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_IP6CONFIG), &item, AJ_NET_TIMEOUT);
        if (item && (status == AJ_OK)) {
            if (item->itemType == AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_IP6CONFIG)) {
                AJ_InfoPrintf(("AJ_WSL_ip6config(): WORK ITEM RECEIVED\n"));
                uint16_t WMIEvent;
                uint32_t reserved;
                uint32_t _command;
                uint32_t _handle;
                uint32_t _error;
                uint32_t _mode;
                uint32_t _ipv4 = 0;
                uint32_t _ipv4mask = 0;
                uint32_t _ipv4gateway = 0;

                WMI_Unmarshal(item->node->buffer, "quuuuuuuu6666uuuu", &WMIEvent, &reserved, &_command, &_handle, &_error, &_mode, &_ipv4, &_ipv4mask, &_ipv4gateway, localAddr, globalAddr, gateway, exAddr, &linkPrefix, &globalPrefix, &gwPrefix, &glbPrefixExt);
                //AJ_DumpBytes("IPV6", item->node->buffer, 0x60);
            } else {
                AJ_WarnPrintf(("AJ_WSL_ip6config(): BAD WORK ITEM RECEIVED\n"));
            }
            AJ_WSL_WMI_FreeWorkItem(item);
        }
    }
    return status;
}

AJ_Status AJ_WSL_ipconfig(uint32_t mode, uint32_t* ip, uint32_t* mask, uint32_t* gateway)
{
    AJ_Status status = AJ_ERR_DHCP;
    uint16_t ipv6[8] = { 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000 };
    wsl_work_item* item = NULL;
    switch (mode) {
    case (IPCONFIG_QUERY):
        {
            AJ_InfoPrintf(("AJ_WSL_ipconfig(): IPCONFIG_QUERY\n"));
            AJ_BufList* ipconfig;
            ipconfig = AJ_BufListCreate();
            WSL_MarshalPacket(ipconfig, WSL_SOCKET, WSL_SOCK_IPCONFIG, 0x60, 0, ip, mask, gateway, &ipv6, &ipv6, &ipv6, &ipv6, 0, 0, 0, 0);
            WMI_MarshalHeader(ipconfig, 1, 1);
            AJ_WSL_WMI_PadPayload(ipconfig);

            AJ_WSL_WMI_QueueWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_IPCONFIG), AJ_WSL_HTC_DATA_ENDPOINT1, ipconfig);
            {
                status = AJ_WSL_WMI_WaitForWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_IPCONFIG), &item, AJ_IPCONFIG_TIMEOUT);
                if (item && (status == AJ_OK)) {
                    if (item->itemType == AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_IPCONFIG)) {
                        AJ_InfoPrintf(("AJ_WSL_ipconfig(): WORK ITEM RECEIVED\n"));
                        uint16_t WMIEvent;
                        uint32_t reserved;
                        uint32_t _command;
                        uint32_t _handle;
                        uint32_t _error;
                        uint32_t _mode;

                        WMI_Unmarshal(item->node->buffer, "quuuuuuuu", &WMIEvent, &reserved, &_command, &_handle, &_error, &_mode, ip, mask, gateway);
                        if (ip != 0) {
                            status = AJ_OK;
                        }
                    } else {
                        AJ_WarnPrintf(("AJ_WSL_ipconfig(): BAD WORK ITEM RECEIVED\n"));
                    }
                    AJ_WSL_WMI_FreeWorkItem(item);
                }
            }
        }
        break;

    case (IPCONFIG_STATIC):
        AJ_InfoPrintf(("AJ_WSL_ipconfig(): IPCONFIG_STATIC\n"));
        AJ_BufList* ipconfig_dhcp_static;

        ipconfig_dhcp_static = AJ_BufListCreate();
        WSL_MarshalPacket(ipconfig_dhcp_static, WSL_SOCKET, WSL_SOCK_IPCONFIG, 0x60, 1, ip, mask, gateway, &ipv6, &ipv6, &ipv6, &ipv6, 0, 0, 0, 0);
        WMI_MarshalHeader(ipconfig_dhcp_static, 1, 1);
        AJ_WSL_WMI_PadPayload(ipconfig_dhcp_static);
        //AJ_BufListPrintDumpContinuous(ipconfig_dhcp_static);
        AJ_WSL_WMI_QueueWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_IPCONFIG), AJ_WSL_HTC_DATA_ENDPOINT1, ipconfig_dhcp_static);
        status = AJ_WSL_WMI_WaitForWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_IPCONFIG), &item, AJ_IPCONFIG_TIMEOUT);
        if (item && (status == AJ_OK)) {
            AJ_WSL_WMI_FreeWorkItem(item);
        }
        AJ_Sleep(1000);
        break;

    case (IPCONFIG_DHCP):
        AJ_InfoPrintf(("AJ_WSL_ipconfig(): IPCONFIG_DHCP\n"));
        AJ_BufList* ipconfig_dhcp;

        ipconfig_dhcp = AJ_BufListCreate();
        WSL_MarshalPacket(ipconfig_dhcp, WSL_SOCKET, WSL_SOCK_IPCONFIG, 0x60, 2, ip, mask, gateway, &ipv6, &ipv6, &ipv6, &ipv6, 0, 0, 0, 0);
        WMI_MarshalHeader(ipconfig_dhcp, 1, 1);
        AJ_WSL_WMI_PadPayload(ipconfig_dhcp);
        AJ_WSL_WMI_QueueWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_IPCONFIG), AJ_WSL_HTC_DATA_ENDPOINT1, ipconfig_dhcp);
        status = AJ_WSL_WMI_WaitForWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_IPCONFIG), &item, AJ_IPCONFIG_TIMEOUT);
        if (item && (status == AJ_OK)) {
            AJ_WSL_WMI_FreeWorkItem(item);
        }
        AJ_Sleep(100);
        break;
    }
    return status;
}

#define DHCP_WAIT       100

/*
 *  create the WMI request to open a socket on the target device
 */
int8_t AJ_WSL_NET_socket_open(uint16_t domain, uint16_t type, uint16_t protocol)
{
    AJ_Status status;
    int8_t handle = INVALID_SOCKET;

    AJ_BufList* open;
    open = AJ_BufListCreate();
    WSL_MarshalPacket(open, WSL_SOCKET, WSL_SOCK_OPEN, 0x0c, domain, type, protocol);
    WMI_MarshalHeader(open, 1, 1);
    AJ_WSL_WMI_PadPayload(open);
    //AJ_BufListPrintDumpContinuous(open);
    AJ_WSL_WMI_QueueWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_OPEN), AJ_WSL_HTC_DATA_ENDPOINT1, open);
    // wait until the command completes
    {
        wsl_work_item* item = NULL;
        status = AJ_WSL_WMI_WaitForWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_OPEN), &item, AJ_NET_TIMEOUT);
        if (status != AJ_OK) {
            //AJ_WSL_WMI_FreeWorkItem(item);
            return -1;
        }
        if (item && (status == AJ_OK)) {
            if (item->itemType == AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_OPEN)) {
                AJ_InfoPrintf(("AJ_WSL_NET_socket_open(): WORK ITEM RECEIVED\n"));
                uint16_t WMIEvent;
                uint32_t reserved;
                uint32_t _command;
                uint32_t _handle;
                uint32_t _error;
                uint32_t _mode;

                WMI_Unmarshal(item->node->buffer, "quuuuu", &WMIEvent, &reserved, &_command, &_handle, &_error, &_mode);
                AJ_InfoPrintf((" Socket Open: handle %08lx error %08lx\n", _handle, _error));
                handle = AJ_WSL_FindOpenSocketContext();
                if (handle != INVALID_SOCKET) {
                    AJ_WSL_SOCKET_CONTEXT[handle].targetHandle = _handle;
                    AJ_WSL_SOCKET_CONTEXT[handle].valid = TRUE;
                    AJ_WSL_SOCKET_CONTEXT[handle].domain = domain;
                    AJ_WSL_SOCKET_CONTEXT[handle].type = type;
                    AJ_WSL_SOCKET_CONTEXT[handle].protocol = protocol;

                }
            } else {
                AJ_WarnPrintf(("AJ_WSL_NET_socket_open(): BAD WORK ITEM RECEIVED\n"));
            }
            AJ_WSL_WMI_FreeWorkItem(item);
        }
    }
    AJ_Sleep(100);
    return handle;
}

/*
 *  create the WMI request to close a socket on the target device
 */
AJ_Status AJ_WSL_NET_socket_close(AJ_WSL_SOCKNUM sock)
{
    AJ_Status status;
    AJ_BufList* close;

    if (AJ_WSL_SOCKET_CONTEXT[sock].valid == TRUE) {
        close = AJ_BufListCreate();
        WSL_MarshalPacket(close, WSL_SOCKET, WSL_SOCK_CLOSE, 0x0, AJ_WSL_SOCKET_CONTEXT[sock].targetHandle);
        WMI_MarshalHeader(close, 1, 1);
        AJ_WSL_WMI_PadPayload(close);
        //AJ_BufListPrintDumpContinuous(close);
        AJ_WSL_WMI_QueueWorkItem(sock, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_CLOSE), AJ_WSL_HTC_DATA_ENDPOINT1, close);
        // wait until the command completes
        do {
            wsl_work_item* item = NULL;
            status = AJ_WSL_WMI_WaitForWorkItem(sock, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_CLOSE), &item, AJ_NET_TIMEOUT);
            if (item && (status == AJ_OK)) {
                if (item->itemType == AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_CLOSE)) {
                    AJ_InfoPrintf(("AJ_WSL_NET_socket_close(): WORK ITEM RECEIVED\n"));
                    uint16_t WMIEvent;
                    uint32_t reserved;
                    uint32_t _command;
                    uint32_t _handle;
                    uint32_t _error;
                    uint32_t _mode;

                    WMI_Unmarshal(item->node->buffer, "quuuuu", &WMIEvent, &reserved, &_command, &_handle, &_error, &_mode);
                    AJ_InfoPrintf((" Socket close: handle %08lx error %08lx\n", _handle, _error));
                    if (_handle == AJ_WSL_SOCKET_CONTEXT[sock].targetHandle) {
                        AJ_WSL_SOCKET_CONTEXT[sock].targetHandle = UINT32_MAX;
                        AJ_WSL_SOCKET_CONTEXT[sock].valid = FALSE;
                    }
                    AJ_WSL_WMI_FreeWorkItem(item);
                    break; // waited until close command completed
                } else {
                    AJ_InfoPrintf(("AJ_WSL_NET_socket_close(): BAD WORK ITEM RECEIVED\n"));
                }
                AJ_WSL_WMI_FreeWorkItem(item);
            }
        } while (1);

    }
    return status;
}


/*
 *  create the WMI request to bind a socket on the target device to an address and port
 */
AJ_Status AJ_WSL_NET_socket_bind(AJ_WSL_SOCKNUM sock, uint32_t addr, uint16_t port)
{
    AJ_Status status;
    AJ_BufList* bind;
    wsl_work_item* item;
    bind = AJ_BufListCreate();
    WSL_MarshalPacket(bind, WSL_SOCKET, WSL_SOCK_BIND, 0x0, AJ_WSL_SOCKET_CONTEXT[sock].targetHandle, port, AJ_WSL_UDP, &addr, 0x8);
    WMI_MarshalHeader(bind, 1, 1);
    AJ_WSL_WMI_PadPayload(bind);
    //AJ_BufListPrintDumpContinuous(bind);
    AJ_WSL_WMI_QueueWorkItem(sock, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_BIND), AJ_WSL_HTC_DATA_ENDPOINT1, bind);
    status = AJ_WSL_WMI_WaitForWorkItem(sock, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_BIND), &item, AJ_NET_TIMEOUT);
    if (item && (status == AJ_OK)) {
        AJ_WSL_WMI_FreeWorkItem(item);
    }
    return status;
}
AJ_Status AJ_WSL_NET_socket_bind6(AJ_WSL_SOCKNUM sock, uint8_t* addr, uint16_t port)
{
    AJ_Status status;
    AJ_BufList* bind;
    wsl_work_item* item;
    bind = AJ_BufListCreate();
    WSL_MarshalPacket(bind, WSL_BIND6, 0x0, 0x03, 0x24, AJ_WSL_SOCKET_CONTEXT[sock].targetHandle, 0x00, port, addr, 0x00, 0x00020383, 0x0002001c);
    WMI_MarshalHeader(bind, 1, 1);
    AJ_WSL_WMI_PadPayload(bind);
    //AJ_BufListPrintDumpContinuous(bind);
    AJ_WSL_WMI_QueueWorkItem(sock, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_BIND), AJ_WSL_HTC_DATA_ENDPOINT1, bind);
    status = AJ_WSL_WMI_WaitForWorkItem(sock, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_NET_BIND), &item, AJ_NET_TIMEOUT);
    if (item && (status == AJ_OK)) {
        AJ_WSL_WMI_FreeWorkItem(item);
    }
    return status;
}
/*
 *  create the WMI request to connect to a socket
 */
AJ_Status AJ_WSL_NET_socket_connect(AJ_WSL_SOCKNUM sock, uint32_t addr, uint16_t port, uint16_t family)
{
    AJ_Status status;
    AJ_BufList* connectV4;
    wsl_work_item* item = NULL;
    connectV4 = AJ_BufListCreate();
    WSL_MarshalPacket(connectV4, WSL_SOCKET, WSL_SOCK_CONNECT, 0x0, AJ_WSL_SOCKET_CONTEXT[sock].targetHandle, port, family, &addr, 0x8);
    WMI_MarshalHeader(connectV4, 1, 1);
    AJ_WSL_WMI_PadPayload(connectV4);

    //AJ_BufListPrintDumpContinuous(connectV4);
    AJ_WSL_WMI_QueueWorkItem(sock, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_CONNECT), AJ_WSL_HTC_DATA_ENDPOINT1, connectV4);

    do {
        status = AJ_WSL_WMI_WaitForWorkItem(sock, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_CONNECT), &item, AJ_NET_TIMEOUT);

        if (item && (status == AJ_OK)) {
            if (item->itemType == AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_CONNECT)) {
                AJ_InfoPrintf(("AJ_WSL_NET_socket_connect(): WORK ITEM RECEIVED\n"));
                AJ_WSL_WMI_FreeWorkItem(item);
                break;
            } else {
                AJ_WarnPrintf(("AJ_WSL_NET_socket_connect(): BAD WORK ITEM RECEIVED\n"));
            }
            AJ_WSL_WMI_FreeWorkItem(item);
        }
    } while (1);

    return status;
}
/*
 * Poll over a socket until there is data ready or till a timeout
 *
 * Returns -2 on timeout
 *         -1 on interrupted
 *          1 on success
 *          0 on close
 *
 */
int16_t AJ_WSL_NET_socket_select(AJ_WSL_SOCKNUM sock, uint32_t timeout)
{
    AJ_Time timer;
    wsl_work_item* peek;
    AJ_Status status;
    int16_t ret = 0;
    // Check if the socket is valid
    if ((sock >= AJ_WSL_SOCKET_MAX) || (sock < 0)) {
        // tried to get data from an invalid socket, return an error
        return ret;
    }
    if (AJ_WSL_SOCKET_CONTEXT[sock].valid == FALSE) {
        // tried to get data from an invalid socket, return the data read from the stash
        AJ_BufListFree(AJ_WSL_SOCKET_CONTEXT[sock].stashedRxList, 1);
        AJ_WSL_SOCKET_CONTEXT[sock].stashedRxList = AJ_BufListCreate();
        //flush the queue
        return ret;
    }
    AJ_InitTimer(&timer);
    // There are 5 conditions that we need to check for
    // 1. If we got an interrupted work item
    // 2. If there is data in the RX stash
    // 3. If there is a RX work item in the queue
    // 4. If there is a socket close work item in the queue
    // 5. If the timeout has expired
    while (1) {
        status = AJ_QueuePeek(AJ_WSL_SOCKET_CONTEXT[sock].workRxQueue, &peek);
        //AJ_AlwaysPrintf(("Item type = %u\n", peek->itemType));
        if ((status == AJ_OK) && (peek->itemType == WSL_NET_INTERUPT)) {
            // Pull the interrupted item off because we dont need it anymore
            status = AJ_QueuePull(AJ_WSL_SOCKET_CONTEXT[sock].workRxQueue, &peek, 0);
            if (peek && (status == AJ_OK)) {
                AJ_WSL_WMI_FreeWorkItem(peek);
            }
            ret = -1;
            break;
        } else if (AJ_BufListLengthOnWire(AJ_WSL_SOCKET_CONTEXT[sock].stashedRxList) > 0) {
            ret = 1;
            break;
        } else if ((status == AJ_OK) && (peek->itemType == WSL_NET_DATA_RX)) {
            ret = 1;
            break;
        } else if ((status == AJ_OK) && (peek->itemType == WSL_NET_CLOSE ||
                                         peek->itemType == WSL_NET_DISCONNECT ||
                                         peek->itemType == AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_CLOSE))) {
            wsl_work_item* clear;
            // Pull the close work item off the queue
            AJ_QueuePull(AJ_WSL_SOCKET_CONTEXT[sock].workRxQueue, &peek, 0);
            // Socket was closed so tear down the connections
            AJ_WSL_SOCKET_CONTEXT[sock].valid = FALSE;
            // Removed any stashed data
            AJ_BufListFree(AJ_WSL_SOCKET_CONTEXT[sock].stashedRxList, 1);
            // Reallocate a new stash
            AJ_WSL_SOCKET_CONTEXT[sock].stashedRxList = AJ_BufListCreate();
            // Reset the queue, any work items are now invalid since the socket was closed
            while (AJ_QueuePull(AJ_WSL_SOCKET_CONTEXT[sock].workRxQueue, &clear, 0) == AJ_OK) {
                AJ_WSL_WMI_FreeWorkItem(clear);
            }
            while (AJ_QueuePull(AJ_WSL_SOCKET_CONTEXT[sock].workTxQueue, &clear, 0) == AJ_OK) {
                AJ_WSL_WMI_FreeWorkItem(clear);
            }
            AJ_QueueReset(AJ_WSL_SOCKET_CONTEXT[sock].workRxQueue);
            AJ_QueueReset(AJ_WSL_SOCKET_CONTEXT[sock].workTxQueue);
            AJ_WSL_WMI_FreeWorkItem(peek);
            ret = 0;
            break;
        } else if (AJ_GetElapsedTime(&timer, TRUE) >= timeout) {
            ret = -2;
            break;
        }
    }
    return ret;
}
void AJ_WSL_NET_signal_interrupted(AJ_WSL_SOCKNUM sock)
{
    wsl_work_item* item = (wsl_work_item*)AJ_WSL_Malloc(sizeof(wsl_work_item));

    item->itemType = WSL_NET_INTERUPT;
    item->sequenceNumber = 0;
    item->list = NULL;
    item->node = NULL;
    item->size = 0;
    item->endpoint = 0;

    if (AJ_WSL_SOCKET_CONTEXT[sock].valid == FALSE) {
        return;
    }
    // Push the interrupted work item to the front of the RX queue
    AJ_QueuePushFromISR(AJ_WSL_SOCKET_CONTEXT[sock].workRxQueue, &item);
}

/************************************************************************/
/*  there isn't a command to recv....you have to grab data as it comes....*/
/************************************************************************/
/*
 *  create the WMI request to bind a socket on the target device to an address and port
 */
int16_t AJ_WSL_NET_socket_recv(AJ_WSL_SOCKNUM sock, uint8_t* buffer, uint32_t sizeBuffer, uint32_t timeout)
{
//    AJ_InfoPrintf(("AJ_WSL_NET_socket_recv()\n"));
    int16_t ret = -1;
    uint32_t rx = sizeBuffer;
    uint32_t stash = 0;
    // read from stash first.
    uint16_t stashLength;

    if ((sock >= AJ_WSL_SOCKET_MAX) || (sock < 0)) {
        // tried to get data from an invalid socket, return an error
        return ret;
    }
    if (AJ_WSL_SOCKET_CONTEXT[sock].valid == FALSE) {
        wsl_work_item* clear;
        // tried to get data from an invalid socket, return the data read from the stash
        AJ_BufListFree(AJ_WSL_SOCKET_CONTEXT[sock].stashedRxList, 1);
        AJ_WSL_SOCKET_CONTEXT[sock].stashedRxList = AJ_BufListCreate();
        //flush the queue
        while (AJ_QueuePull(AJ_WSL_SOCKET_CONTEXT[sock].workRxQueue, &clear, 0) == AJ_OK) {
            AJ_WSL_WMI_FreeWorkItem(clear);
        }
        AJ_QueueReset(AJ_WSL_SOCKET_CONTEXT[sock].workRxQueue);
        return ret;
    }

    if (AJ_WSL_SOCKET_CONTEXT[sock].stashedRxList) {
        stashLength = AJ_BufListLengthOnWire(AJ_WSL_SOCKET_CONTEXT[sock].stashedRxList);
        if (stashLength != 0) {
            stash = min(rx, stashLength);
            AJ_BufListCopyBytes(AJ_WSL_SOCKET_CONTEXT[sock].stashedRxList, stash, buffer);
            AJ_BufListPullBytes(AJ_WSL_SOCKET_CONTEXT[sock].stashedRxList, stash); // shift left-overs toward the start.
            ret = stash;
            sizeBuffer -= stash;
        }
    }


    // wait until there is data
    if (sizeBuffer) {
        wsl_work_item* item = NULL;
        AJ_Status status;

        // the stash was depleted and you want more data
        if (AJ_WSL_SOCKET_CONTEXT[sock].valid == FALSE) {
            // tried to get data from an invalid socket, return the data read from the stash
            return ret;
        }


        status = AJ_QueuePull(AJ_WSL_SOCKET_CONTEXT[sock].workRxQueue, &item, timeout);
        if (item && (status == AJ_OK)) {
            if (item->itemType == WSL_NET_INTERUPT) {
                // At this point we dont care about the interrupted signal but we are expecting a RX packet
                // so we need to pull the next item off the queue
                AJ_WSL_WMI_FreeWorkItem(item);
                status = AJ_QueuePull(AJ_WSL_SOCKET_CONTEXT[sock].workRxQueue, &item, timeout);
            }
            if ((status == AJ_OK) && (item->itemType == WSL_NET_DISCONNECT)) {
                AJ_InfoPrintf(("Disconnect received\n"));
                // Clean up the network queues
                int i;
                for (i = 0; i < AJ_WSL_SOCKET_MAX; i++) {
                    wsl_work_item* clear;
                    AJ_WSL_SOCKET_CONTEXT[i].valid = FALSE;
                    // Removed any stashed data
                    AJ_BufListFree(AJ_WSL_SOCKET_CONTEXT[i].stashedRxList, 1);
                    // Reallocate a new stash
                    AJ_WSL_SOCKET_CONTEXT[i].stashedRxList = AJ_BufListCreate();
                    // Reset the queue, any work items are now invalid since the socket was closed
                    while (AJ_QueuePull(AJ_WSL_SOCKET_CONTEXT[i].workRxQueue, &clear, 0) == AJ_OK) {
                        AJ_WSL_WMI_FreeWorkItem(clear);
                    }
                    while (AJ_QueuePull(AJ_WSL_SOCKET_CONTEXT[i].workTxQueue, &clear, 0) == AJ_OK) {
                        AJ_WSL_WMI_FreeWorkItem(clear);
                    }
                    AJ_QueueReset(AJ_WSL_SOCKET_CONTEXT[i].workRxQueue);
                    AJ_QueueReset(AJ_WSL_SOCKET_CONTEXT[i].workTxQueue);
                }
                ret = -1;
            } else if ((status == AJ_OK) && (item->itemType == WSL_NET_CLOSE || item->itemType == AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_CLOSE))) {
                wsl_work_item* clear;
                // Pull the close work item off the queue
                AJ_QueuePull(AJ_WSL_SOCKET_CONTEXT[sock].workRxQueue, &item, 0);
                // Socket was closed so tear down the connections
                AJ_WSL_SOCKET_CONTEXT[sock].valid = FALSE;
                // Removed any stashed data
                AJ_BufListFree(AJ_WSL_SOCKET_CONTEXT[sock].stashedRxList, 1);
                // Reallocate a new stash
                AJ_WSL_SOCKET_CONTEXT[sock].stashedRxList = AJ_BufListCreate();
                // Reset the queue, any work items are now invalid since the socket was closed
                while (AJ_QueuePull(AJ_WSL_SOCKET_CONTEXT[sock].workRxQueue, &clear, 0) == AJ_OK) {
                    AJ_WSL_WMI_FreeWorkItem(clear);
                }
                while (AJ_QueuePull(AJ_WSL_SOCKET_CONTEXT[sock].workTxQueue, &clear, 0) == AJ_OK) {
                    AJ_WSL_WMI_FreeWorkItem(clear);
                }
                AJ_QueueReset(AJ_WSL_SOCKET_CONTEXT[sock].workRxQueue);
                AJ_QueueReset(AJ_WSL_SOCKET_CONTEXT[sock].workTxQueue);
                ret = -1;
            } else if ((status == AJ_OK) && (item->itemType == WSL_NET_DATA_RX)) {

                //AJ_InfoPrintf(("=====DATA RX ITEM RECEIVED=====\n"));
                //AJ_DumpBytes("DATA RX ITEM", item->node->buffer, item->size);
                rx = min(sizeBuffer, item->size);
                memcpy(buffer + stash, item->node->buffer, rx);
                AJ_BufNodePullBytes(item->node, rx);
                // check node: if not empty assign to stash.
                if (item->node->length) {
                    AJ_BufListPushTail(AJ_WSL_SOCKET_CONTEXT[sock].stashedRxList, item->node);
                    item->node = NULL;
                }
                ret = rx + stash;
            } else {
                AJ_InfoPrintf(("AJ_WSL_NET_socket_recv(): BAD WORK ITEM RECEIVED\n"));
            }
            AJ_WSL_WMI_FreeWorkItem(item);
        } else {
            ret = 0;
            AJ_InfoPrintf(("socket_recv timed out\n"));
        }
    }

    return ret;
}

int16_t AJ_WSL_NET_socket_send(uint32_t socket, uint8_t* data, uint16_t size, uint32_t timeout)
{
    AJ_Status status;
    AJ_InfoPrintf(("AJ_WSL_NET_socket_send()\n"));
    AJ_BufList* send;
    AJ_BufNode* tx_data_node;

    if (AJ_WSL_SOCKET_CONTEXT[socket].valid == FALSE) {
        AJ_InfoPrintf(("send on invalid socket %lu\n", socket));
        return -1;
    }

    send = AJ_BufListCreate();
    tx_data_node = AJ_BufListCreateNodeExternalZero(data, size, FALSE);

    WMI_MarshalSend(send, AJ_WSL_SOCKET_CONTEXT[socket].targetHandle, tx_data_node, size);
    //send now contains the header info for the two part packet your sending
    //now write send to the MBOX then the data your sending

    AJ_WSL_WMI_PadPayload(send);
    AJ_WSL_WMI_QueueWorkItem(socket, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_NET_DATA_TX), AJ_WSL_HTC_DATA_ENDPOINT2, send);
    /*
     *  Because these are blocking sends, we need to wait until the data has been passed to the target.
     */
    do {
        wsl_work_item* item = NULL;
        status = AJ_WSL_WMI_WaitForWorkItem(socket, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_NET_DATA_TX), &item, timeout);

        if (item && (status == AJ_OK)) {
            if (item->itemType == AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_NET_DATA_TX)) {
                AJ_InfoPrintf(("AJ_WSL_NET_socket_send(): WORK ITEM RECEIVED\n"));
                AJ_WSL_WMI_FreeWorkItem(item);
                break;
            } else {
                AJ_WarnPrintf(("AJ_WSL_NET_socket_send(): BAD WORK ITEM RECEIVED\n"));
            }
            AJ_WSL_WMI_FreeWorkItem(item);
        }
    } while (1);

    return size;
}
int16_t AJ_WSL_NET_socket_sendto6(uint32_t socket, uint8_t* data, uint16_t size, uint8_t* addr, uint16_t port, uint32_t timeout)
{
    AJ_Status status;
    AJ_InfoPrintf(("AJ_WSL_NET_socket_sendto()\n"));
    AJ_BufList* send;
    AJ_BufNode* tx_data_node;

    if (AJ_WSL_SOCKET_CONTEXT[socket].valid == FALSE) {
        AJ_InfoPrintf(("sendto on invalid socket %ld\n", socket));
        return -1;
    }

    send = AJ_BufListCreate();
    tx_data_node = AJ_BufListCreateNodeExternalZero(data, size, FALSE);

    WMI_MarshalSendTo6(send, AJ_WSL_SOCKET_CONTEXT[socket].targetHandle, tx_data_node, size, addr, port);
    //send now contains the header info for the two part packet your sending
    //now write send to the MBOX then the data your sending
    AJ_WSL_WMI_PadPayload(send);
    //AJ_BufListPrintDumpContinuous(send);

    AJ_WSL_WMI_QueueWorkItem(socket, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_NET_DATA_TX), AJ_WSL_HTC_DATA_ENDPOINT2, send);
    wsl_work_item* item = NULL;

    /*
     *  Because these are blocking sends, we need to wait until the data has been passed to the target.
     */
    do {
        status = AJ_WSL_WMI_WaitForWorkItem(socket, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_NET_DATA_TX), &item, timeout);

        if (item && (status == AJ_OK)) {
            if (item->itemType == AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_NET_DATA_TX)) {
                AJ_InfoPrintf(("AJ_WSL_NET_socket_send(): WORK ITEM RECEIVED\n"));
                AJ_WSL_WMI_FreeWorkItem(item);
                break;
            } else {
                AJ_InfoPrintf(("AJ_WSL_NET_socket_sendto(): BAD WORK ITEM RECEIVED\n"));
            }
            AJ_WSL_WMI_FreeWorkItem(item);
        }
    } while (1);

    return size;
}
int16_t AJ_WSL_NET_socket_sendto(uint32_t socket, uint8_t* data, uint16_t size, uint32_t addr, uint16_t port, uint32_t timeout)
{
    AJ_Status status;
    AJ_InfoPrintf(("AJ_WSL_NET_socket_sendto()\n"));
    AJ_BufList* send;
    AJ_BufNode* tx_data_node;

    if (AJ_WSL_SOCKET_CONTEXT[socket].valid == FALSE) {
        AJ_InfoPrintf(("sendto on invalid socket %ld\n", socket));
        return -1;
    }

    send = AJ_BufListCreate();
    tx_data_node = AJ_BufListCreateNodeExternalZero(data, size, FALSE);

    WMI_MarshalSendTo(send, AJ_WSL_SOCKET_CONTEXT[socket].targetHandle, tx_data_node, size, AJ_ByteSwap32(addr), port);
    //send now contains the header info for the two part packet your sending
    //now write send to the MBOX then the data your sending
    AJ_WSL_WMI_PadPayload(send);
    AJ_WSL_WMI_QueueWorkItem(socket, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_NET_DATA_TX), AJ_WSL_HTC_DATA_ENDPOINT2, send);
    wsl_work_item* item = NULL;

    /*
     *  Because these are blocking sends, we need to wait until the data has been passed to the target.
     */
    do {
        status = AJ_WSL_WMI_WaitForWorkItem(socket, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_NET_DATA_TX), &item, timeout);

        if (item && (status == AJ_OK)) {
            if (item->itemType == AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_NET_DATA_TX)) {
                AJ_InfoPrintf(("AJ_WSL_NET_socket_send(): WORK ITEM RECEIVED\n"));
                AJ_WSL_WMI_FreeWorkItem(item);
                break;
            } else {
                AJ_InfoPrintf(("AJ_WSL_NET_socket_sendto(): BAD WORK ITEM RECEIVED\n"));
            }
            AJ_WSL_WMI_FreeWorkItem(item);
        }
    } while (1);
    return size;
}

#define AJ_SOCK_OPTS_OFFSET 19
AJ_Status AJ_WSL_NET_set_sock_options(uint32_t socket, uint32_t level, uint32_t optname, uint32_t optlen, uint8_t* optval)
{
    AJ_Status status;
    AJ_InfoPrintf(("AJ_WSL_NET_set_sock_options()\n"));
    wsl_work_item* item;
    AJ_BufList* opts;
    AJ_BufNode* trailer;
    uint32_t total_length;
    opts = AJ_BufListCreate();
    total_length = AJ_SOCK_OPTS_OFFSET + optlen;

    WSL_MarshalPacket(opts, WSL_SOCKET, WSL_SOCK_SETSOCKOPT, total_length, AJ_WSL_SOCKET_CONTEXT[socket].targetHandle, level, optname, optlen);
    if (optname == WSL_JOIN_GROUP) {
        trailer = AJ_BufListCreateNode((optlen / 2) + 2);
        memcpy(opts->tail->buffer + opts->tail->length - 17, optval, (optlen / 2) + 1);
        memcpy(trailer->buffer, optval + 17, 15);
    } else {
        trailer = AJ_BufListCreateNode(optlen + 3);
        memset(trailer->buffer, 0, optlen + 3);
    }
    AJ_BufListPushTail(opts, trailer);


    WMI_MarshalHeader(opts, 1, 1);
    //AJ_BufListPrintDumpContinuous(opts);
    AJ_WSL_WMI_QueueWorkItem(socket, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_SETSOCKOPT), AJ_WSL_HTC_DATA_ENDPOINT1, opts);
    // wait until the command completes
    status = AJ_WSL_WMI_WaitForWorkItem(socket, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_SETSOCKOPT), &item, AJ_NET_TIMEOUT);
    if (item && (status == AJ_OK)) {
        AJ_WSL_WMI_FreeWorkItem(item);
    }
    return status;
}

AJ_Status AJ_WSL_NET_set_hostname(const char* hostname)
{
    AJ_Status status;
    AJ_InfoPrintf(("===== SET HOSTNAME ====\n"));
    wsl_work_item* item;
    AJ_BufList* host;
    host = AJ_BufListCreate();
    WSL_MarshalPacket(host, WSL_SOCKET, WSL_SOCK_IP_HOST_NAME, 0x21, hostname);
    WMI_MarshalHeader(host, 1, 1);
    AJ_WSL_WMI_PadPayload(host);
    //AJ_BufListPrintDumpContinuous(host);
    AJ_WSL_WMI_QueueWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_IP_HOST_NAME), AJ_WSL_HTC_DATA_ENDPOINT1, host);
    status = AJ_WSL_WMI_WaitForWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_IP_HOST_NAME), &item, AJ_NET_TIMEOUT);
    if (item && (status == AJ_OK)) {
        AJ_WSL_WMI_FreeWorkItem(item);
    }
    return status;
}


AJ_Status AJ_WSL_NET_ip6config_router_prefix(const uint8_t* ipv6addr, uint32_t prefix_length, uint32_t lifetimePrefix, uint32_t lifetimeValid)
{
    AJ_InfoPrintf(("===== ip6config router prefix ====\n"));
    AJ_BufList* prefix;
    prefix = AJ_BufListCreate();
    WSL_MarshalPacket(prefix, WSL_SOCKET, WSL_SOCK_IP6CONFIG_ROUTER_PREFIX, 0x21, ipv6addr, prefix_length, lifetimePrefix, lifetimeValid);
    WMI_MarshalHeader(prefix, 1, 1);
    AJ_WSL_WMI_PadPayload(prefix);
    //AJ_BufListPrintDumpContinuous(prefix);
    AJ_WSL_WMI_QueueWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_IP6CONFIG_ROUTER_PREFIX), AJ_WSL_HTC_DATA_ENDPOINT1, prefix);
    return AJ_OK;
}

AJ_Status AJ_WSL_NET_ipconfig_dhcp_pool(const uint32_t* startIP, const uint32_t* endIP, uint32_t leaseTime)
{
    AJ_InfoPrintf(("===== ipconfig dhcp pool ====\n"));
    AJ_BufList* pool;
    pool = AJ_BufListCreate();
    WSL_MarshalPacket(pool, WSL_SOCKET, WSL_SOCK_IPCONFIG_DHCP_POOL, 0x0c, startIP, endIP, leaseTime);
    WMI_MarshalHeader(pool, 1, 1);
    AJ_WSL_WMI_PadPayload(pool);
    //AJ_BufListPrintDumpContinuous(pool);
    AJ_WSL_WMI_QueueWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_NETWORK, WSL_SOCK_IPCONFIG_DHCP_POOL), AJ_WSL_HTC_DATA_ENDPOINT1, pool);
    return AJ_OK;
}


AJ_Status AJ_WSL_NET_disconnect(void)
{
    AJ_Status status;
    AJ_BufList* disconnect;
    wsl_work_item* item;
    AJ_InfoPrintf((("AJ_WSL_NET_disconnect(): DISCONNECT\n")));
    disconnect = AJ_BufListCreate();
    WSL_MarshalPacket(disconnect, WSL_DISCONNECT, 0, 0, 0, 0, 0, 0, 0, 0);
    WMI_MarshalHeader(disconnect, 1, 1);
    AJ_WSL_WMI_PadPayload(disconnect);
    //AJ_BufListPrintDumpContinuous(disconnect);
    AJ_WSL_WMI_QueueWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_NETWORK, WSL_NET_DISCONNECT), AJ_WSL_HTC_DATA_ENDPOINT1, disconnect);

    status = AJ_WSL_WMI_WaitForWorkItem(0, AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_NETWORK, WSL_NET_DISCONNECT), &item, AJ_NET_TIMEOUT);
    if (item && (status == AJ_OK)) {
        AJ_WSL_WMI_FreeWorkItem(item);
    }
    return status;
}
