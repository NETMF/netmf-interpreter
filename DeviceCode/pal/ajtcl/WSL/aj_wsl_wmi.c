/**
 * @file WMI layer implementation
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

#define AJ_MODULE WSL_WMI

#include "aj_target.h"

#include "aj_wsl_target.h"
#include "aj_buf.h"
#include "aj_wsl_spi_constants.h"
#include "aj_wsl_htc.h"
#include "aj_wsl_wmi.h"
#include "aj_wsl_net.h"
#include "aj_wsl_tasks.h"
#include "aj_malloc.h"
#include "aj_debug.h"
#include "aj_wsl_unmarshal.h"
#include <stdlib.h>
#include <stdio.h>
/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgWSL_WMI = 5;
#endif

extern AJ_WSL_HTC_CONTEXT AJ_WSL_HTC_Global;
static uint8_t deviceMAC[6];
/*
 * global that holds target hardware info.
 */
AJ_FW_Version AJ_WSL_TargetFirmware;

AJ_WifiCallbackFunc AJ_WSL_WifiConnectCallback;

extern wsl_socket_context AJ_WSL_SOCKET_CONTEXT[AJ_WSL_SOCKET_MAX];
/**
 * globals to track open socket slots
 */
uint32_t AJ_WSL_SOCKET_HANDLE_INVALID = UINT32_MAX;

struct AJ_TaskHandle* AJ_WSL_MBoxListenHandle;

#ifndef NDEBUG
const char* WSL_WorkItemText(uint32_t status);
#endif
/*
 * Maps command ID's and signatures
 * Use the enum "wsl_wmi_command_list" to index
 */
const CMDID_SIG_MAP cmd_map[] = {
    { 0x0001, "quyyyyyyyySqMqq", 0x3a },      //CONNECT
    { 0x0007, "quuuuuyyq",       0x1a },      //START_SCAN
    { 0x0008, "quqqqqqyyqqu",    0x1a },      //SET_SCAN_PARAMS
    { 0x0009, "quyyqu",          0x0e },      //SET_BSS_FILTER
    { 0x000a, "quyyyS",          0x29 },      //SET_PROBED_SSID
    { 0xf01b, "quqq",            0x0a },      //ALLOW_AGGR
    { 0x0012, "quy",             0x07 },      //SET_POWER_MODE
    { 0xf048, "quSPyy",          0x68 },      //SET_PASSPHRASE
    { 0xf04e, "quyy",            0x08 },      //STORECALL_CONFIGURE
    { 0xf08d, "",                   0 },      //SOCKET
    { 0xf00f, "quyyyyyyyySqMqq", 0x3a },      //SET_SOFT_AP
    { 0,      "uuyyyyyyyyyyuy",  0x52 },      //SEND
    { 0,      "uuyyyyyyyyyyuy",  0x1e },      //SENDTO
    { 0x0003, "qu",              0x06 },      //DISCONNECT
    { 0xf00b, "quy",             0x07 },      //SET_HIDDEN_AP
    { 0, "uuyyyyyyyyyyuyyyyyyyyqqu6uy", 0x4c }, //SENDTO6
    { 0,      "uuuuqq6uuu",      0x32 },      //BIND6
    { 0x0016, "quyyyyuuKyM",     0x39 },      //ADD_CIPHER_KEY
    { 0xf028, "quKy",            0x27 }       //SET_PMK
};

const uint16_t getCommandId(wsl_wmi_command_list command)
{
    return cmd_map[command].id;
}
const char* getCommandSignature(wsl_wmi_command_list command)
{
    return cmd_map[command].signature;
}
const uint16_t getPacketSize(wsl_wmi_command_list command)
{
    return cmd_map[command].size;
}
/*
 * Map of socket commands to signatures
 * The socket id is the index in the array
 */
const SOCKET_SIG_MAP sock_map[] = {
    { "uuuu", 20 },                         //OPEN
    { "uu", 12 },                           //CLOSE
    { "uuqq4q", 26 },                       //CONNECT
    { "uuqq4q", 26 },                       //BIND
    { "", 0 },
    { "", 0 },
    { "", 0 },
    { "uuuuu", 41 },                        //SETSOCKOPT
    { "", 0 },
    { "", 0 },
    { "uu4446666uuuu", 110 },                //IP_CONFIG
    { "", 0 },
    { "uyyyy", 12 },                        //STACK_INIT
    { "", 0 },
    { "", 0 },
    { "uu4446666uuuu", 104 },                //IP6_CONFIG
    { "u44u", 20 },                          //IPCONFIG_DHCP_POOL
    { "u6uuu", 36 },                         //IP6_CONFIG_ROUTER_PREFIX
    { "", 0 },
    { "", 0 },
    { "", 0 },
    { "", 0 },
    { "", 0 },
    { "", 0 },
    { "", 0 },
    { "", 0 },
    { "", 0 },
    { "", 0 },
    { "", 0 },
    { "uS", 46 }                            //SET_HOSTNAME
};
const char* getSockSignature(wsl_socket_cmds command)
{
    return sock_map[command].signature;
}
uint16_t getSockSize(wsl_socket_cmds command)
{
    return sock_map[command].size;
}
uint8_t* getDeviceMac(void)
{
    return (uint8_t*)&deviceMAC;
}

static const AJ_HeapConfig wsl_heapConfig[] = {
    { 8,     30, 0 },
    { 16,    100, 0 },
    { 20,    80, 0 },
    { 24,    10, 0 },
    { 32,    20, 0 },
    { 48,    10, 0 },
    { 64,    10, 0 },
    { 84,     6, 0 },
    { 100,    2, 0 },
};
#define WSL_HEAP_WORD_COUNT (7360 / 4)
static uint32_t wsl_heap[WSL_HEAP_WORD_COUNT];



void* AJ_WSL_Malloc(size_t size)
{
    void* mem = NULL;
    // allocate from the WSL pool first.
    AJ_EnterCriticalRegion();
    if (size <= 100) {
        mem = AJ_PoolAlloc(size);
    }

    // if the pool was full or the size too big, fall back to malloc
    if (!mem) {
        mem = AJ_Malloc(size);
    }
    if (!mem) {
        AJ_ErrPrintf(("AJ_WSL_Malloc(): Malloc failed\n"));
        AJ_Reboot();
    }
    AJ_LeaveCriticalRegion();
    return mem;
}

void AJ_WSL_Free(void* ptr)
{
    AJ_EnterCriticalRegion();
    // if the address is within the WSL heap, free the pool entry, else fallback to free.
    if ((ptr > (void*)&wsl_heap) && (ptr < (void*)&wsl_heap[WSL_HEAP_WORD_COUNT])) {
        AJ_PoolFree(ptr);
    } else {
        AJ_Free(ptr);
    }
    AJ_LeaveCriticalRegion();
}


void AJ_WSL_ModuleInit(void)
{
    //prepare the WSL heap
    size_t heapSz;

    heapSz = AJ_PoolRequired(wsl_heapConfig, ArraySize(wsl_heapConfig));
    if (heapSz > sizeof(wsl_heap)) {
        AJ_ErrPrintf(("Heap space is too small %d required %d\n", sizeof(wsl_heap), heapSz));
        return;
    }
    AJ_InfoPrintf(("Allocated heap %d bytes\n", (int)heapSz));
    AJ_PoolInit(wsl_heap, heapSz, wsl_heapConfig, ArraySize(wsl_heapConfig));
    AJ_PoolDump();

    AJ_WSL_SOCKNUM i;
    for (i = 0; i < AJ_WSL_SOCKET_MAX; i++) {
        memset(&AJ_WSL_SOCKET_CONTEXT[i], 0, sizeof(AJ_WSL_SOCKET_CONTEXT[0]));
        AJ_WSL_SOCKET_CONTEXT[i].targetHandle = AJ_WSL_SOCKET_HANDLE_INVALID;
        AJ_WSL_SOCKET_CONTEXT[i].stashedRxList = AJ_BufListCreate();
        AJ_WSL_SOCKET_CONTEXT[i].workRxQueue = AJ_QueueCreate("RxQueue");
        AJ_WSL_SOCKET_CONTEXT[i].workTxQueue = AJ_QueueCreate("TxQueue");
    }

    AJ_WSL_WMI_ModuleInit();

}

AJ_Status AJ_WSL_DriverStart(void)
{
    AJ_CreateTask(AJ_WSL_MBoxListenAndProcessTask, (const signed char*)"AJWSLMBoxListen", 1000, NULL, 2, &AJ_WSL_MBoxListenHandle);
    while (!AJ_WSL_IsDriverStarted());
    return AJ_OK;
}

AJ_Status AJ_WSL_DriverStop(void)
{
    AJ_DestroyTask(AJ_WSL_MBoxListenHandle);
    return AJ_OK;
}


void AJ_WSL_WMI_ModuleInit()
{
    AJ_WSL_HTC_ModuleInit();
}


void AJ_WSL_WMI_PrintMessage(AJ_BufNode* pNodeWMIPacket)
{
    AJ_AlwaysPrintf(("WMI_PrintMessage node %p, length %d, buffer %p\n", pNodeWMIPacket, pNodeWMIPacket->length, pNodeWMIPacket->buffer));
}

/**
 *  return index of matching socket or AJ_WSL_SOCKET_MAX on not found
 *  skip the global socket context
 */
AJ_WSL_SOCKNUM AJ_WSL_FindOpenSocketContext(void)
{
    AJ_WSL_SOCKNUM i;
    for (i = 1; i < ArraySize(AJ_WSL_SOCKET_CONTEXT); i++) {
        if (AJ_WSL_SOCKET_CONTEXT[i].valid == FALSE) {
            return i;
        }
    }
    return INVALID_SOCKET;
}

/**
 *  return index of matching socket or AJ_WSL_SOCKET_MAX on not found
 */
AJ_WSL_SOCKNUM AJ_WSL_FindSocketContext(uint32_t handle)
{
    AJ_WSL_SOCKNUM i;
    for (i = 0; i < ArraySize(AJ_WSL_SOCKET_CONTEXT); i++) {
        if (AJ_WSL_SOCKET_CONTEXT[i].targetHandle == handle) {
            return i;
        }
    }
    return INVALID_SOCKET;
}

void AJ_WSL_WMI_ProcessWMIEvent(AJ_BufNode* pNodeHTCBody)
{
    uint16_t eventID;
    uint16_t info1;
    uint16_t reserved;
    int32_t dataUnmarshaled;
    dataUnmarshaled = WMI_Unmarshal(pNodeHTCBody->buffer, "qqq", &eventID, &info1, &reserved);

    switch (eventID) {
    case WSL_WMI_READY_EVENTID: {
            uint8_t capability;
            dataUnmarshaled += WMI_Unmarshal(pNodeHTCBody->buffer + dataUnmarshaled, "uuMy", &AJ_WSL_TargetFirmware.target_ver, &AJ_WSL_TargetFirmware.abi_ver, &deviceMAC, &capability);
            AJ_InfoPrintf(("WMI_READY, version A %08lx, version B %08lx capability %x\n",  AJ_WSL_TargetFirmware.target_ver, AJ_WSL_TargetFirmware.abi_ver, capability));
            AJ_InfoPrintf(("Device MAC: %02x:%02x:%02x:%02x:%02x:%02x\n", deviceMAC[0], deviceMAC[1], deviceMAC[2], deviceMAC[3], deviceMAC[4], deviceMAC[5]));

            AJ_WSL_HTC_Global.started = TRUE;
            break;
        }

    case WSL_BSS_INFO_EVENTID: {
            extern void AJ_WSL_BSSINFO_Recv(AJ_BufNode* node);
            AJ_WSL_BSSINFO_Recv(pNodeHTCBody);
            break;
        }

    case WSL_CMDERROR_EVENTID: {
            uint16_t commandId;
            uint8_t error;
            dataUnmarshaled += WMI_Unmarshal(pNodeHTCBody->buffer + dataUnmarshaled, "qy", &commandId, &error);

            AJ_InfoPrintf(("WMI_CMDERROR, last command %04x, error code %02x \n",  commandId, error));
            break;
        }

    case WSL_WMI_SCAN_COMPLETE_EVENTID: {
            //  now signal the waiting code that the scan has completed
            // we can do this by posting an item to the global socket context recv queue
            // the client will pull off a scan complete event and then continue.

            wsl_work_item* scanCompleteResponse;
            wsl_work_item** pItem;

            scanCompleteResponse = (wsl_work_item*)AJ_WSL_Malloc(sizeof(wsl_work_item));
            memset(scanCompleteResponse, 0, sizeof(wsl_work_item));
            scanCompleteResponse->itemType = WSL_NET_SCAN;
            scanCompleteResponse->node = AJ_BufNodeCreateAndTakeOwnership(pNodeHTCBody);

            pItem = &scanCompleteResponse;
            AJ_QueuePush(AJ_WSL_SOCKET_CONTEXT[0].workRxQueue, pItem, AJ_TIMER_FOREVER);

            break;
        }

    case WSL_WMI_DISCONNECT_EVENTID: {
            uint16_t protocolReason;
            uint8_t bssid[6];
            uint16_t disconnectReason;
            WMI_Unmarshal(pNodeHTCBody->buffer + dataUnmarshaled, "qMq", &protocolReason, &bssid, &disconnectReason);

            AJ_InfoPrintf(("WMI_DISCONNECT  event: protocolReason %x, disconnectReason %x\n", protocolReason, disconnectReason));
            if (AJ_WSL_WifiConnectCallback) {
                (AJ_WSL_WifiConnectCallback)(0);
            }

            //  now signal the waiting code that the scan has completed
            // we can do this by posting an item to the global socket context recv queue
            // the client will pull off a scan complete event and then continue.
            wsl_work_item* disconnectResponse;
            wsl_work_item** pItem;
            disconnectResponse = (wsl_work_item*)AJ_WSL_Malloc(sizeof(wsl_work_item));
            memset(disconnectResponse, 0, sizeof(wsl_work_item));
            disconnectResponse->itemType = WSL_NET_DISCONNECT;
            disconnectResponse->node = AJ_BufNodeCreateAndTakeOwnership(pNodeHTCBody);

            pItem = &disconnectResponse;
            AJ_QueuePush(AJ_WSL_SOCKET_CONTEXT[0].workRxQueue, pItem, AJ_TIMER_FOREVER);


            break;
        }

    case WSL_WMI_CONNECT_EVENTID: {
            //  now signal the waiting code that the scan has completed
            // we can do this by posting an item to the global socket context recv queue
            // the client will pull off a scan complete event and then continue.
            wsl_work_item* connectResponse;
            wsl_work_item** pItem;

            connectResponse = (wsl_work_item*)AJ_WSL_Malloc(sizeof(wsl_work_item));
            memset(connectResponse, 0, sizeof(wsl_work_item));
            connectResponse->itemType = WSL_NET_CONNECT;
            connectResponse->node = AJ_BufNodeCreateAndTakeOwnership(pNodeHTCBody);
            pItem = &connectResponse;
            AJ_QueuePush(AJ_WSL_SOCKET_CONTEXT[0].workRxQueue, pItem, AJ_TIMER_FOREVER);

            if (AJ_WSL_WifiConnectCallback) {
                (AJ_WSL_WifiConnectCallback)(1);
            }

            break;
        }

    case WSL_WMI_SOCKET_RESPONSE_EVENTID: {
            uint32_t responseType;
            uint32_t socketHandle;
            uint32_t error;
            WMI_Unmarshal(pNodeHTCBody->buffer + dataUnmarshaled, "uuu", &responseType, &socketHandle, &error);

            int8_t socketIndex;
            //AJ_BufListNodePrintDump(pNodeHTCBody, NULL);
            /* look for the matching handle in the global context then mark it invalid */
            switch (responseType) {
            // some of the commands operate on the global socket context
            case WSL_SOCK_OPEN:
            case WSL_SOCK_IPCONFIG:
            case WSL_SOCK_IP6CONFIG:
            case WSL_SOCK_STACK_INIT:
            case WSL_SOCK_IP_HOST_NAME:
                socketIndex = 0;
                break;

            default: {
                    socketIndex = AJ_WSL_FindSocketContext(socketHandle);
                    if (socketIndex == INVALID_SOCKET) {
                        AJ_DumpBytes("INVALID SOCKET DUMP", pNodeHTCBody->buffer, pNodeHTCBody->length);
                        AJ_WarnPrintf(("SOCKET_PING response for invalid socket!\n"));
                        break;
                    }
                }
            }

            if (socketIndex != INVALID_SOCKET) {
                AJ_Status status = AJ_OK;

                //push a work item into the Read queue
                if (status == AJ_OK) {
                    wsl_work_item** ppWork;
                    wsl_work_item* sockResp;
                    sockResp = (wsl_work_item*)AJ_WSL_Malloc(sizeof(wsl_work_item));
                    memset(sockResp, 0, sizeof(wsl_work_item));
                    sockResp->itemType = AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, responseType);
                    //sockResp->size = payloadSize;
                    sockResp->node = AJ_BufNodeCreateAndTakeOwnership(pNodeHTCBody);
                    ppWork = &sockResp;
                    AJ_QueuePush(AJ_WSL_SOCKET_CONTEXT[socketIndex].workRxQueue, ppWork, AJ_TIMER_FOREVER);


                }

                if ((responseType == AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, WSL_SOCK_CLOSE)) && (socketIndex != INVALID_SOCKET)) {
                    AJ_WSL_SOCKET_CONTEXT[socketIndex].targetHandle = UINT32_MAX;
                    AJ_WSL_SOCKET_CONTEXT[socketIndex].valid = FALSE;
                }


            }
            break;
        }

    case WSL_WMI_PEER_NODE_EVENTID: {
            uint8_t reason;
            WMI_Unmarshal(pNodeHTCBody->buffer + dataUnmarshaled, "q", &reason);
            if ((reason == 0) /*&& (AJ_WSL_connectState == AJ_WIFI_CONNECTING)*/) {
                /*
                 * The 4-way handshake is complete, indicate the state has changed
                 */
                if (AJ_WSL_WifiConnectCallback) {
                    (AJ_WSL_WifiConnectCallback)(16 /*RSNA_AUTH_SUCCESS*/);
                }
            }
            break;
        }

    // There are several WMI events that aren't parsed.
    case WSL_REGDOMAIN_EVENTID:
    case WSL_UNKNOWN2_EVENTID:
    case WSL_UNKNOWN1_EVENTID:
        break;

    default: {
            AJ_InfoPrintf(("Unknown WMI Event %x\n", eventID));
            return;
        }
    }
    AJ_InfoPrintf(("Processed WMI Event: %s\n", WSL_WorkItemText(eventID)));
}

void AJ_WSL_WMI_ProcessSocketDataResponse(AJ_BufNode* pNodeHTCBody)
{
    int8_t socketIndex;
    uint32_t u32;
    uint16_t lead, payloadSize, _port;
    uint32_t _handle, srcAddr;
    uint16_t ipv6addr[8];
    uint16_t bufferOffset = 0;
    wsl_work_item** ppWork;
    wsl_work_item* sockResp;
//    AJ_DumpBytes("WMI_SOCKET_RESPONSE B", pNodeHTCBody->buffer, pNodeHTCBody->length);
    // Get the initial bytes of data in the packet
    WMI_Unmarshal(pNodeHTCBody->buffer, "quuq", &lead, &u32, &_handle, &_port);
    //AJ_BufNodePullBytes(pNodeHTCBody, 12);
    bufferOffset += 12;
    // look for the matching handle in the global context then mark it invalid
    socketIndex = AJ_WSL_FindSocketContext(_handle);
    if (socketIndex == INVALID_SOCKET) {
        AJ_WarnPrintf(("data returned for invalid socket. Handle = %lu\n", _handle));
        return;
    }
    if (AJ_WSL_SOCKET_CONTEXT[socketIndex].domain == WSL_AF_INET6) {
        bufferOffset += 6;
        // Get the IPv6 address and payload size
        WMI_Unmarshal(pNodeHTCBody->buffer + bufferOffset, "6uq", &ipv6addr, &u32, &payloadSize);
        // Advance the buffer to the start of the payload
        bufferOffset += 28;
    } else {
        bufferOffset += 2;
        // Get the IPv4 address
        WMI_Unmarshal(pNodeHTCBody->buffer + bufferOffset, "4q", &srcAddr, &payloadSize);
        // Advance the buffer to the start of the payload
        bufferOffset += 12;
    }

    sockResp = (wsl_work_item*)AJ_WSL_Malloc(sizeof(wsl_work_item));
    memset(sockResp, 0, sizeof(wsl_work_item));
    sockResp->itemType = WSL_NET_DATA_RX;
    sockResp->size = payloadSize;
    sockResp->node = AJ_BufNodeCreateAndTakeOwnership(pNodeHTCBody);
    AJ_BufNodePullBytes(sockResp->node, bufferOffset);  /// the length of the socket header info header
    sockResp->node->length = payloadSize;

    ppWork = &sockResp;
    AJ_QueuePush(AJ_WSL_SOCKET_CONTEXT[socketIndex].workRxQueue, ppWork, AJ_TIMER_FOREVER);
}

AJ_Status AJ_WSL_WMI_QueueWorkItem(uint32_t socket, uint8_t command, uint8_t endpoint, AJ_BufList* list)
{
    AJ_InfoPrintf(("AJ_WSL_WMI_QueueWorkItem(): %s\n", WSL_WorkItemText(command)));
    wsl_work_item** ppWork;
    wsl_work_item* sockWork;
    sockWork = (wsl_work_item*)AJ_WSL_Malloc(sizeof(wsl_work_item));
    memset(sockWork, 0, sizeof(wsl_work_item));
    sockWork->itemType = command;
    sockWork->list = list;
    sockWork->endpoint = endpoint;
    ppWork = &sockWork;
    AJ_ResumeTask(AJ_WSL_MBoxListenHandle, 0); // wake up the driver task so it can create space in the workTxQueue
    AJ_QueuePush(AJ_WSL_SOCKET_CONTEXT[socket].workTxQueue, ppWork, AJ_TIMER_FOREVER);
    AJ_ResumeTask(AJ_WSL_MBoxListenHandle, 0); // wake up the driver task after pushing, allowing the task to process the workitem
    return AJ_OK;
}

/*
 * This function just returns the work item. If there is data inside that you want
 * you have to unmarshal it after you receive the work item.
 */
AJ_Status AJ_WSL_WMI_WaitForWorkItem(uint32_t socket, uint8_t command, wsl_work_item** item, uint32_t timeout)
{
    AJ_Status status;
    AJ_Time timer;
    AJ_InitTimer(&timer);
//    AJ_AlwaysPrintf(("WaitForWorkItem: %x\n", command));
    //wsl_work_item* item;
    status = AJ_QueuePull(AJ_WSL_SOCKET_CONTEXT[socket].workRxQueue, item, timeout);
    timeout -= AJ_GetElapsedTime(&timer, TRUE);
    if ((status == AJ_OK) && item && *item) {
        if ((status == AJ_OK) && ((*item)->itemType == WSL_NET_INTERUPT)) {
            // We don't care about the interrupted signal for any calls using this function
            AJ_WSL_WMI_FreeWorkItem((*item));
            status = AJ_QueuePull(AJ_WSL_SOCKET_CONTEXT[socket].workRxQueue, item, timeout);
        }
        if ((status == AJ_OK) && ((*item)->itemType == command)) {
            AJ_InfoPrintf(("AJ_WSL_WMI_WaitForWorkItem(): Received work item %s\n", WSL_WorkItemText(command)));
            return AJ_OK;
        } else if ((status == AJ_OK) && ((*item)->itemType == WSL_NET_DISCONNECT)) {
            AJ_InfoPrintf(("Got disconnect while waiting for %s\n", WSL_WorkItemText(command)));
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
            AJ_WSL_WMI_FreeWorkItem((*item));
            return AJ_ERR_LINK_DEAD;
        } else if ((status == AJ_OK) && ((*item)->itemType == WSL_NET_DATA_RX)) {
            // If we got data we want to save it and not throw it away, its still not what we
            // wanted so we can free the work item as it wont be needed at a higher level
            AJ_InfoPrintf(("Got data while waiting for %s\n", WSL_WorkItemText(command)));
            if ((*item)->node->length) {
                AJ_BufNode* new_node = AJ_BufNodeCreateAndTakeOwnership((*item)->node);
                AJ_BufListPushTail(AJ_WSL_SOCKET_CONTEXT[socket].stashedRxList, new_node);
                AJ_WSL_WMI_FreeWorkItem((*item));
                return AJ_ERR_NULL;
            }
        } else {
            AJ_WarnPrintf(("AJ_WSL_WMI_WaitForWorkItem(): Received incorrect work item %s, wanted %s\n", WSL_WorkItemText((*item)->itemType), WSL_WorkItemText(command)));
            // Wrong work item, but return NULL because we can free the item internally
            AJ_WSL_WMI_FreeWorkItem((*item));
            return AJ_ERR_NULL;
        }
    }
    return AJ_ERR_NULL;
}

void AJ_WSL_WMI_FreeWorkItem(wsl_work_item* item)
{
    if (item) {
        AJ_BufListFree(item->list, TRUE);
        AJ_BufListFreeNodeAndBuffer(item->node, NULL);
        AJ_WSL_Free(item);
    }
}

#ifndef NDEBUG
#define AJ_CASE_NETWORK(_status) case AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_NETWORK, _status): return # _status
#define AJ_CASE_SOCKET(_status) case AJ_WSL_WORKITEM(AJ_WSL_WORKITEM_SOCKET, _status): return # _status
#define AJ_CASE_GENERIC(_status) case _status: return # _status

const char* WSL_WorkItemText(uint32_t status)
{
    static char buf[4];
    switch (status) {
        // Network cases
        AJ_CASE_NETWORK(WSL_CONNECT);
        AJ_CASE_NETWORK(WSL_START_SCAN);
        AJ_CASE_NETWORK(WSL_SET_SCAN_PARAMS);
        AJ_CASE_NETWORK(WSL_SET_BSS_FILTER);
        AJ_CASE_NETWORK(WSL_SET_PROBED_SSID);
        AJ_CASE_NETWORK(WSL_ALLOW_AGGR);
        AJ_CASE_NETWORK(WSL_SET_POWER_MODE);
        AJ_CASE_NETWORK(WSL_SET_PASSPHRASE);
        AJ_CASE_NETWORK(WSL_STORECALL_CONFIGURE);
        AJ_CASE_NETWORK(WSL_SOCKET);
        AJ_CASE_NETWORK(WSL_SET_SOFT_AP);
        AJ_CASE_NETWORK(WSL_SEND);
        AJ_CASE_NETWORK(WSL_SENDTO);
        AJ_CASE_NETWORK(WSL_DISCONNECT);
        AJ_CASE_NETWORK(WSL_SET_HIDDEN_AP);
        AJ_CASE_NETWORK(WSL_SENDTO6);
        AJ_CASE_NETWORK(WSL_BIND6);
        AJ_CASE_NETWORK(WSL_ADD_CIPHER_KEY);
        AJ_CASE_NETWORK(WMI_SET_PMK);
        // Socket cases
        AJ_CASE_SOCKET(WSL_SOCK_OPEN);
        AJ_CASE_SOCKET(WSL_SOCK_CLOSE);
        AJ_CASE_SOCKET(WSL_SOCK_CONNECT);
        AJ_CASE_SOCKET(WSL_SOCK_BIND);
        AJ_CASE_SOCKET(WSL_SOCK_SELECT);
        AJ_CASE_SOCKET(WSL_SOCK_SETSOCKOPT);
        AJ_CASE_SOCKET(WSL_SOCK_GETSOCKOPT);
        AJ_CASE_SOCKET(WSL_SOCK_IPCONFIG);
        AJ_CASE_SOCKET(WSL_SOCK_IP6CONFIG);
        AJ_CASE_SOCKET(WSL_SOCK_PING);
        AJ_CASE_SOCKET(WSL_SOCK_STACK_INIT);
        AJ_CASE_SOCKET(WSL_SOCK_STACK_MISC);
        AJ_CASE_SOCKET(WSL_NET_DATA_TX);
        AJ_CASE_SOCKET(WSL_SOCK_IP_HOST_NAME);
        AJ_CASE_SOCKET(WSL_SOCK_IP6CONFIG_ROUTER_PREFIX);
        // Events
        AJ_CASE_GENERIC(WSL_WMI_READY_EVENTID);
        AJ_CASE_GENERIC(WSL_WMI_CONNECT_EVENTID);
        AJ_CASE_GENERIC(WSL_WMI_DISCONNECT_EVENTID);
        AJ_CASE_GENERIC(WSL_BSS_INFO_EVENTID);
        AJ_CASE_GENERIC(WSL_CMDERROR_EVENTID);
        AJ_CASE_GENERIC(WSL_WMI_SCAN_COMPLETE_EVENTID);
        AJ_CASE_GENERIC(WSL_WMI_APLIST_EVENTID);
        AJ_CASE_GENERIC(WSL_WMI_PEER_NODE_EVENTID);
        AJ_CASE_GENERIC(WSL_WMI_WLAN_VERSION_EVENTID);
        AJ_CASE_GENERIC(WSL_WMI_SOCKET_RESPONSE_EVENTID);

    default:
        snprintf(buf, sizeof(buf), "%lu", (uint32_t)status);
        return buf;
    }
}

/*
 * Print relevant information about the drivers current state. This should
 * only be called upon a crash as it will corrupt the current state information
 * after it has been called.
 */
void AJ_WSL_PrintDriverTraceback(void)
{
    int i;
    dbgWSL_WMI = 5;
    AJ_AlwaysPrintf(("Driver state: "));
    if (AJ_WSL_HTC_Global.started) {
        AJ_AlwaysPrintf(("Started\n"));
    } else {
        AJ_AlwaysPrintf(("Not Started\n"));
    }
    for (i = 0; i < 4; i++) {
        AJ_AlwaysPrintf(("Endpoint %d credits: %d\n", i, AJ_WSL_HTC_Global.endpoints[i].txCredits));
    }
    for (i = 0; i < AJ_WSL_SOCKET_MAX; i++) {
        uint32_t stashSz = 0;
        AJ_BufNode* stashNode = AJ_WSL_SOCKET_CONTEXT[i].stashedRxList->head;
        int j = 0;
        wsl_work_item* item;
        AJ_AlwaysPrintf(("------ SOCKET CONTEXT %d -----\n", i));
        AJ_AlwaysPrintf(("\tSocket Handle: 0x%x\n", AJ_WSL_SOCKET_CONTEXT[i].targetHandle));
        if (AJ_WSL_SOCKET_CONTEXT[i].domain == WSL_AF_INET) {
            AJ_AlwaysPrintf(("Domain: AF_INET\n"));
        } else {
            AJ_AlwaysPrintf(("Domain: AF_INET6\n"));
        }
        if (AJ_WSL_SOCKET_CONTEXT[i].type == WSL_SOCK_STREAM) {
            AJ_AlwaysPrintf(("Type: SOCK_STREAM\n"));
        } else {
            AJ_AlwaysPrintf(("Type: SOCK_DGRAM\n"));
        }
        if (AJ_WSL_SOCKET_CONTEXT[i].valid) {
            while (stashNode != NULL) {
                stashSz += stashNode->length;
                stashNode = stashNode->next;
            }
            AJ_AlwaysPrintf(("\tStash size = %u\n", stashSz));
            AJ_AlwaysPrintf(("\tworkRxQueue:\n"));
            while (AJ_QueuePull(AJ_WSL_SOCKET_CONTEXT[i].workRxQueue, &item, 0) == AJ_OK) {
                AJ_AlwaysPrintf(("\t\tQueue Index %i:", j));
                AJ_AlwaysPrintf(("\tItem Type: %s\n", WSL_WorkItemText(item->itemType)));
                j++;
            }
            if (j == 0) {
                AJ_AlwaysPrintf(("\tworkRxQueue empty\n"));
            }
            j = 0;
            AJ_AlwaysPrintf(("\tworkTxQueue:\n"));
            while (AJ_QueuePull(AJ_WSL_SOCKET_CONTEXT[i].workTxQueue, &item, 0) == AJ_OK) {
                AJ_AlwaysPrintf(("\t\tQueue Index %i:", j));
                AJ_AlwaysPrintf(("\tItem Type: %s\n", WSL_WorkItemText(item->itemType)));
                j++;
            }
            if (j == 0) {
                AJ_AlwaysPrintf(("\tworkTxQueue empty\n"));
            }
        } else {
            AJ_AlwaysPrintf(("Socket context %d: Not valid or not open\n", i));
        }
        AJ_AlwaysPrintf(("\n"));
    }
}

void HardFault_Handler(void)
{
    AJ_AlwaysPrintf(("HARD FAULT OCCURED, PRINTING DRIVER TRACEBACK\n"));
    AJ_WSL_PrintDriverTraceback();
    while (1);
}

#endif
