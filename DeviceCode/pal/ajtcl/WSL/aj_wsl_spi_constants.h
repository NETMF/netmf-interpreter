/**
 * @file Constants related to the QCA4004 wifi module
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

#ifndef AJ_WSL_SPI_CONSTANTS_H_
#define AJ_WSL_SPI_CONSTANTS_H_

#pragma pack(push, 1)

#define AJ_WSL_SPI_MBOX_SIZE 0xFF
#define AJ_WSL_SPI_MBOX_0 0x0
#define AJ_WSL_SPI_MBOX_0_END (AJ_WSL_SPI_MBOX_0 + AJ_WSL_SPI_MBOX_SIZE)
#define AJ_WSL_SPI_MBOX_0_EOM (AJ_WSL_SPI_MBOX_0_END + 1)

#define AJ_WSL_SPI_MBOX_ALIAS_SIZE 0x7FF
#define AJ_WSL_SPI_MBOX_0_ALIAS 0x800
#define AJ_WSL_SPI_MBOX_0_ALIAS_END (AJ_WSL_SPI_MBOX_0_ALIAS + AJ_WSL_SPI_MBOX_ALIAS_SIZE)
#define AJ_WSL_SPI_MBOX_0_EOM_ALIAS (AJ_WSL_SPI_MBOX_0_ALIAS_END + 1)


//Address for AHB Read Access
#define AJ_WSL_SPI_HOST_INT_STATUS 0x400
//CPU Sourced Interrupt Status
#define AJ_WSL_SPI_CPU_INT_STATUS 0x401
//Credit Counter Direct Access
#define AJ_WSL_SPI_CREDIT_COUNT 0x420
//Data transfer value
#define AJ_WSL_SPI_TARGET_VALUE 0x474
//Data transfer write from the host
#define AJ_WSL_SPI_TARGET_ADDR_WRITE 0x478
//Data transfer read from the target
#define AJ_WSL_SPI_TARGET_ADDR_READ 0x047C
//SPI Slave Interface
#define AJ_WSL_SPI_SPI_CONFIG 0x480
//SPI Status
#define AJ_WSL_SPI_SPI_STATUS 0x481

/*
 * SPI internal registers
 * description followed by register address
 */

//DMA size (
#define AJ_WSL_SPI_REG_DMA_SIZE 0x0100
//Write buffer space available
#define AJ_WSL_SPI_REG_WRBUF_SPC_AVA 0x0200

//Read buffer byte available
#define AJ_WSL_SPI_REG_RDBUF_BYTE_AVA 0x0300

//SPI configuration
#define AJ_WSL_SPI_REG_SPI_CONFIG 0x0400

//SPI status
#define AJ_WSL_SPI_REG_SPI_STATUS 0x0500
//Host control register access byte size
#define AJ_WSL_SPI_REG_HOST_CTRL_BYTE_SIZE 0x0600

//Host control register configure
#define AJ_WSL_SPI_REG_HOST_CTRL_CONFIG 0x0700
//Host control register read port
#define AJ_WSL_SPI_REG_HOST_CTRL_RD_PORT 0x0800
//Host control register write port
#define AJ_WSL_SPI_REG_HOST_CTRL_WR_PORT 0x0A00
//Interrupt cause
#define AJ_WSL_SPI_REG_INTR_CAUSE 0x0C00
#define AJ_WSL_SPI_REG_INTR_CAUSE_DATA_AVAILABLE (1 << 0)
#define AJ_WSL_SPI_REG_INTR_CAUSE_READ_DONE      (1 << 9)
#define AJ_WSL_SPI_REG_INTR_CAUSE_WRITE_DONE     (1 << 8)
#define AJ_WSL_SPI_REG_INTR_CAUSE_CPU_AWAKE      (1 << 6)
#define AJ_WSL_SPI_REG_INTR_CAUSE_COUNTER        (1 << 5)

//Interrupt enable
#define AJ_WSL_SPI_REG_INTR_ENABLE 0x0D00

//Write buffer Watermark
#define AJ_WSL_SPI_REG_WRBUF_WATERMARK 0x1300

//Read buffer lookahead 1
#define AJ_WSL_SPI_REG_RDBUF_LOOKAHEAD1 0x1400

//Read buffer lookahead 2
#define AJ_WSL_SPI_REG_RDBUF_LOOKAHEAD2 0x1500



// magic value from driver context data
#define AJ_WSL_AR4100_BUFFER_SIZE 1664

#define AJ_WSL_SPI_TARGET_CLOCK_SPEED_ADDR   0x00428878
#define AJ_WSL_SPI_TARGET_FLASH_PRESENT_ADDR 0x0042880C
#define AJ_WSL_SPI_TARGET_MBOX_BLOCKSZ_ADDR  0x0042886C

typedef enum {
    WMI_CONNECT_CMDID                   = 0x0001,
    WMI_RECONNECT_CMDID                 = 0x0002,
    WMI_DISCONNECT_CMDID                = 0x0003,
    WMI_SYNCHRONIZE_CMDID               = 0x0004,
    WMI_START_SCAN_CMDID                = 0x0007,
    WMI_SET_SCAN_PARAMS_CMDID           = 0x0008,
    WMI_SET_BSS_FILTER_CMDID            = 0x0009,
    WMI_SET_PROBED_SSID_CMDID           = 0x000a,
    WMI_SET_LISTEN_INT_CMDID            = 0x000b,
    WMI_ALLOW_AGGR_CMDID                = 0xf01b,
    WMI_SET_CHANNEL_CMDID               = 0xf042,
    WMI_GET_PMK_CMDID                   = 0xf047,
    WMI_SET_POWER_MODE_CMDID            = 0x0012,
    WMI_SET_PASSPHRASE_CMDID            = 0xf048,
    WMI_STORERECALL_CONFIGURE_CMDID     = 0xf05e,
    WMI_STORERECALL_RECALL_CMDID        = 0xf05f,
    WMI_STORERECALL_HOST_READY_CMDID    = 0xf060,
    /*Socket commands*/
    WMI_SOCKET_CMDID                    = 0xf08d,
    WMI_SET_SOFT_AP_CMDID                = 0xf00f
} wsl_wmi_command_id;

typedef enum  _wsl_SOCKET_CMDS {
    WSL_SOCK_OPEN       = 0x0,   /*Open a socket*/
    WSL_SOCK_CLOSE      = 0x1,  /*Close existing socket*/
    WSL_SOCK_CONNECT    = 0x2,  /*Connect to a peer*/
    WSL_SOCK_BIND       = 0x3,  /*Bind to interface*/
    WSL_SOCK_SELECT     = 0x6,  /*Wait for specified file descriptors*/
    WSL_SOCK_SETSOCKOPT = 0x7,  /*Set specified socket option*/
    WSL_SOCK_GETSOCKOPT = 0x8,  /*Get socket option*/
    WSL_SOCK_ERRNO,             /*Get error number for last error*/
    WSL_SOCK_IPCONFIG   = 0xA,  /*Set static IP information, or get current IP config*/
    WSL_SOCK_PING,
    WSL_SOCK_STACK_INIT = 0xC,  /*Command to initialize stack*/
    WSL_SOCK_STACK_MISC,        /*Used to exchanges miscellaneous info, e.g. reassembly etc*/
    WSL_SOCK_PING6,
    WSL_SOCK_IP6CONFIG  = 0xF,  /*Set static IP information, or get current IP config*/
    WSL_SOCK_IPCONFIG_DHCP_POOL,            /*Set DHCP Pool  */
    WSL_SOCK_IP6CONFIG_ROUTER_PREFIX,       /* Set ipv6 router prefix */
    WSL_SOCK_IP_SET_TCP_EXP_BACKOFF_RETRY,  /* set tcp exponential backoff retry */
    WSL_SOCK_IP_SET_IP6_STATUS,             /* set ip6 module status enable/disable */
    WSL_SOCK_IP_DHCP_RELEASE,       /* Release the DHCP IP Addres */
    WSL_SOCK_IP_SET_TCP_RX_BUF,     /* set tcp rx buffer space */
    WSL_SOCK_IP_HOST_NAME = 0x1d    /* Command to set the hostname on the QCA4002 */
} wsl_socket_cmds;



typedef enum  _WSL_WMI_EVENTID {
    WSL_WMI_READY_EVENTID           = 0x1001,
    WSL_WMI_CONNECT_EVENTID         = 0x1002,
    WSL_WMI_DISCONNECT_EVENTID      = 0x1003,
    WSL_BSS_INFO_EVENTID            = 0x1004,
    WSL_CMDERROR_EVENTID            = 0x1005,
    WSL_REGDOMAIN_EVENTID           = 0x1006,
    WSL_UNKNOWN1_EVENTID            = 0x1008,
    WSL_WMI_SCAN_COMPLETE_EVENTID   = 0x100A,
    WSL_WMI_APLIST_EVENTID          = 0x1017,
    WSL_WMI_PEER_NODE_EVENTID       = 0x101B,
    WSL_WMI_WLAN_VERSION_EVENTID    = 0x101E,
    WSL_UNKNOWN2_EVENTID            = 0x103b,
    WSL_WMI_SOCKET_RESPONSE_EVENTID = 0x9016,
} WSL_WMI_EVENTID;




enum AJ_WSL_HTC_ENDPOINT_COUNT {
    AJ_WSL_HTC_CONTROL_ENDPOINT = 0,
    AJ_WSL_HTC_DATA_ENDPOINT1   = 1,
    AJ_WSL_HTC_DATA_ENDPOINT2   = 2,
    AJ_WSL_HTC_DATA_ENDPOINT3   = 3,
    AJ_WSL_HTC_DATA_ENDPOINT4   = 4,
    AJ_WSL_HTC_ENDPOINT_COUNT_MAX,
};

/*
 * RX trailer type Ids
 */
#define AJ_WSL_HTC_RXTRAILER_CREDIT_REPORT       1

// message id values
#define AJ_WSL_HTC_MSG_READY_ID                 1
#define AJ_WSL_HTC_CONNECT_SERVICE_ID           2
#define AJ_WSL_HTC_SERVICE_CONNECT_RESPONSE_ID  3
#define AJ_WSL_HTC_SETUP_COMPLETE_ID            4
#define AJ_WSL_HTC_HOST_READY_ID                5

#pragma pack(pop)
#endif /* AJ_WSL_SPI_CONSTANTS_H_ */
