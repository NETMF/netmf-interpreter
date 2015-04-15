/**
 * @file WMI layer function declarations
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
#ifndef AJ_WSL_WMI_H_
#define AJ_WSL_WMI_H_

#include "aj_target.h"
#include "aj_wsl_target.h"
#include "aj_wsl_net.h"
#include "aj_buf.h"
#include "aj_target_platform.h"
#include "aj_target_rtos.h"
#include "aj_wsl_spi_constants.h"

#ifdef __cplusplus
extern "C" {
#endif

void AJ_WSL_WMI_ModuleInit(void);
// prototype for functions that are invoked for wifi connection status
typedef void (*AJ_WifiCallbackFunc)(int val);




/*
 * Defines what byte maps to data in the header for commands/events
 */
#define WMI_HDR_START 0
#define WMI_HDR_FLAGS 1
#define WMI_HDR_PKT_SIZE 2
#define WMI_HDR_TRAILER_SIZE 4
#define WMI_HDR_PKT_ID 5
#define WMI_HDR_CMD_ID 6

#define IPCONFIG_QUERY  0
#define IPCONFIG_STATIC 1
#define IPCONFIG_DHCP   2

#define NUM_COMMANDS 30
#define NUM_SOCK_CMDS 29

typedef struct _CMDID_SIG_MAP {
    const uint16_t id;
    const char* signature;
    const uint16_t size;
} CMDID_SIG_MAP;

typedef struct _SOCKET_SIG_MAP {
    const char* signature;
    const uint16_t size;
}SOCKET_SIG_MAP;



/*
 * Signature definitions for various WMI Commands
 * Will always start with "q" (command ID)
 */
#define WMI_SIG_SET_SCAN_PARAMS     "quqqqqqyyqqu"
#define WMI_SIG_START_SCAN          "quuuuuyyq"
#define WMI_SIG_ALLOW_AGGR          "quqq"
#define WMI_SIG_CONNECT             "quyyyyyyyySqMqq"
#define WMI_SIG_SET_PASSPHRASE      "quSPyy"
#define WMI_SIG_SET_PROBED_SSID     "quyyyS"
#define WMI_SIG_ALLOW_AGGR          "quqq"
#define WMI_SIG_SET_BSS_FILTER      "quyyqu"
//note: on socket signatures dont add the command ID or socket command
//      they are marshaled in automatically
#define WMI_SIG_SOCKET_IPCONFIG     "uu4446666uuuu"
#define WMI_SIG_SOCKET_STACK_INIT   "uyyyy"
#define WMI_SIG_SOCKET_OPEN         "uuuu"
#define WMI_SIG_SOCKET_CLOSE        "uu"
#define WMI_SIG_SOCKET_CONNECT      "uuqq4q"
#define WMI_SIG_SOCKET_BIND         "uuqq4q"
#define WMI_SIG_SET_POWER_MODE      "quy"
#define WMI_STORERECALL_CONFIG      "quyy"
#define WMI_SIG_IP_HOST_NAME        "uS"
#define WMI_SIG_SET_SOFT_AP         "quyyyyyyyySqMqq"

//#define WMI_SIG_CONNECT_EVENT       "qMqquyyyay"
#define WMI_SIG_SETSOCKOPTS         "uuuuu" //ay"     //Array of options is marshaled in separately
typedef  struct ip6_addr {
    uint8_t addr[16];     /* 128 bit IPv6 address */
} IP6_ADDR_T;

#ifdef WIN32
typedef struct sockaddr_in SOCKADDR_T;
typedef struct sockaddr_in SOCKADDR_6_T;
#else
typedef  struct sockaddr {
    uint16_t sin_port;          //Port number
    uint16_t sin_family;        //ATH_AF_INET
    uint32_t sin_addr;          //IPv4 Address
}  SOCKADDR_T;

typedef  struct sockaddr_6 {
    uint16_t sin6_family;           // ATH_AF_INET6
    uint16_t sin6_port;             // transport layer port #
    uint32_t sin6_flowinfo;         // IPv6 flow information
    IP6_ADDR_T sin6_addr;           // IPv6 address
    uint32_t sin6_scope_id;         // set of interfaces for a scope
} SOCKADDR_6_T;
#endif

/*
 * This list is used for indexing into the CMDID_SIG_MAP
 */
typedef enum {
    WSL_CONNECT = 0,
    WSL_START_SCAN,
    WSL_SET_SCAN_PARAMS,
    WSL_SET_BSS_FILTER,
    WSL_SET_PROBED_SSID,
    WSL_ALLOW_AGGR,
    WSL_SET_POWER_MODE,
    WSL_SET_PASSPHRASE,
    WSL_STORECALL_CONFIGURE,
    WSL_SOCKET,
    WSL_SET_SOFT_AP,
    WSL_SEND,
    WSL_SENDTO,
    WSL_DISCONNECT,
    WSL_SET_HIDDEN_AP,
    WSL_SENDTO6,
    WSL_BIND6,
    WSL_ADD_CIPHER_KEY,
    WMI_SET_PMK
}wsl_wmi_command_list;

/*
 *  This macro converts socket command numbers into WSL workitem numbers
 */
#define AJ_WSL_WORKITEM(g, c) (((uint8_t)(g) << 6) | (c)) /**< Encode a socket command into a workitem */
#define AJ_WSL_WORKITEM_NETWORK 0
#define AJ_WSL_WORKITEM_SOCKET  1
#define AJ_WSL_WORKITEM_DEVICE  2

AJ_EXPORT void AJ_WSL_WMI_PrintMessage(AJ_BufNode* pNodeWMIPacket);

typedef struct _wsl_work_item {
    uint16_t itemType;                 /**< what type of work item is this*/
    uint16_t sequenceNumber;           /**< use this match a call with the response data */
    AJ_BufList* list;                    /**< either the data going into or coming out of the function, direction depends on the call type */
    AJ_BufNode* node;                    /**< either the data going into or coming out of the function, direction depends on the call type */
    uint32_t size;
    uint8_t endpoint;                  /**< Which endpoint does the workitem use */
} wsl_work_item;


/**
 * structure that maps socket handles on the target to socket handles on the host
 */
typedef struct _wsl_socket_context {
    uint32_t targetHandle;     /**< holds the target-side handle value */
    uint8_t valid;             /**< is the socket valid? */
    struct AJ_Queue* workTxQueue;    /**< work items to be sent to the target are pushed here */
    struct AJ_Queue* workRxQueue;    /**< work items received from the target are pulled from here */
    AJ_BufList* stashedRxList;     /**< leftover data from a network packet waiting to be read */
    uint32_t domain;     /**<  AF_INET  or AF_INET6 */
    uint32_t type;       /**<  SOCK_STREAM or SOCK_DGRAM*/
    uint32_t protocol;   /**<  */
    union {
        SOCKADDR_T name;
        SOCKADDR_6_T name6;
    };
} wsl_socket_context;

/**
 * Find a socket number
 *
 * @param handle        The socket handle
 *
 * @return              The socket number with associated handle
 */
AJ_WSL_SOCKNUM AJ_WSL_FindSocketContext(uint32_t handle);

/**
 * Find a socket that you can open
 *
 * @return              The socket number avaliable
 */
AJ_WSL_SOCKNUM AJ_WSL_FindOpenSocketContext(void);

/**
 * Process a WMI event
 *
 * @param pNodeHTCBody  The buf node that was received over WMI
 */
AJ_EXPORT void AJ_WSL_WMI_ProcessWMIEvent(AJ_BufNode* pNodeHTCBody);

/**
 * Process a data WMI event
 *
 * @param pNodeHTCBody  The buf node that was received over WMI
 */
AJ_EXPORT void AJ_WSL_WMI_ProcessSocketDataResponse(AJ_BufNode* pNodeHTCBody);

/**
 * Queue a work item to be sent to the target
 *
 * @param socket        Socket to send over
 * @param command       Enum WMI command your sending
 * @param endpoint      Endpoint on the target to send to
 * @param list          Buf List that your sending (must be previously marshalled)
 *
 * @return              AJ_OK on success
 */
AJ_Status AJ_WSL_WMI_QueueWorkItem(uint32_t socket, uint8_t command, uint8_t endpoint, AJ_BufList* list);

/**
 * Wait for a work item that was previously send on the queue
 *
 * @param socket        Socket that the work item was sent to
 * @param command       Command the was sent
 * @param item          Address of the work item pointer
 * @param timeout       Milliseconds to wait for this work item
 */
AJ_Status AJ_WSL_WMI_WaitForWorkItem(uint32_t socket, uint8_t command, wsl_work_item** item, uint32_t timeout);

/**
 * Free a work item pointer. This frees everything inside the work item structure
 *
 * @param item          Item to be freed
 */
void AJ_WSL_WMI_FreeWorkItem(wsl_work_item* item);

/**
 * Get the WMI command ID from the enum command list
 *
 * @param command       Enum command your getting the ID for
 *
 * @return              The command ID
 */
const uint16_t getCommandId(wsl_wmi_command_list command);

/**
 * Get the command signature
 *
 * @param command       Command you want the signature to
 *
 * @return              The signature
 */
const char* getCommandSignature(wsl_wmi_command_list command);

/**
 * Get the packet size for a given command
 *
 * @param command       The command you need the size for
 *
 * @return              The size of the packet
 */
const uint16_t getPacketSize(wsl_wmi_command_list command);

/**
 * Get the socket signature for a socket command
 *
 * @param command       The SOCKET command you need the signature for
 *
 * @return              The signature
 */
const char* getSockSignature(wsl_socket_cmds command);

/**
 * Get the packet size for a socket command
 *
 * @param command       The command you need the size for
 *
 * @return              The size of the socket packet
 */
uint16_t getSockSize(wsl_socket_cmds command);

/**
 * Get the devices MAC address
 *
 * @return              Pointer to the MAC address data
 */
uint8_t* getDeviceMac(void);


#ifdef __cplusplus
}
#endif

#endif /* AJ_WSL_WMI_H_ */
