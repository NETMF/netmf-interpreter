/**
 * @file Unmarshaling declarations
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

#include "aj_target.h"
#include "aj_wsl_target.h"
#include "aj_wsl_wmi.h"
#include "aj_wsl_net.h"

#ifdef __cplusplus
extern "C" {
#endif

/*
 * Basic Types
 */
#define WMI_ARG_INVALID           '\0'   /**< AllJoyn invalid type */
#define WMI_ARG_ARRAY             'a'    /**< AllJoyn array container type */
#define WMI_ARG_BOOLEAN           'b'    /**< AllJoyn boolean basic type */
#define WMI_ARG_INT32             'i'    /**< AllJoyn 32-bit signed integer basic type */
#define WMI_ARG_INT16             'n'    /**< AllJoyn 16-bit signed integer basic type */
#define WMI_ARG_UINT16            'q'    /**< AllJoyn 16-bit unsigned integer basic type */
#define WMI_ARG_STRING            's'    /**< AllJoyn UTF-8 NULL terminated string basic type */
#define WMI_ARG_UINT32            'u'    /**< AllJoyn 32-bit unsigned integer basic type */
#define WMI_ARG_UINT64            't'
#define WMI_ARG_VARIANT           'v'    /**< AllJoyn variant container type */
#define WMI_ARG_BYTE              'y'    /**< AllJoyn 8-bit unsigned integer basic type */
#define WMI_ARG_STRUCT            '('    /**< AllJoyn struct container type */
#define WMI_ARG_DICT_ENTRY        '{'    /**< AllJoyn dictionary or map container type - an array of key-value pairs */
/*
 * Specific WMI types
 */
#define WMI_ARG_MAC               'M'    /**< MAC address */
#define WMI_ARG_IPV4              '4'    /**< IPV4 Address */
#define WMI_ARG_IPV6              '6'    /**< IPV6 Address */
#define WMI_ARG_SSID              'S'    /**< SSID 32 byte */
#define WMI_ARG_BSSID             'B'    /**< BSSID */
#define WMI_ARG_ENCRYPTION        'E'    /**< Encryption type */
#define WMI_ARG_SIZE              'Z'    /**< Size specifier */
#define WMI_ARG_RSSI              'R'    /**< RSSI */
#define WMI_ARG_HEADER            'H'    /**< WMI Header */
#define WMI_ARG_PASSPHRASE        'P'    /**< 64 Byte pass phrase */
#define WMI_ARG_TAG               'T'    /**< Tag identifier */
#define WMI_ARG_KEY               'K'    /**< Cipher key/ Pairwise master key (32 byte) */

//#define WMI_TYPE_SCAN             "HP09RMP0eS" /**< WiFi scan. [Header][RSSI][MAC][SSID] */

typedef enum {
    BSS_SCAN = 0x1004
}EVENT;

typedef enum {
    TAG_SSID = 0,
    TAG_CHANNEL = 3,
    TAG_RSN = 48,
    TAG_VENDOR = 221
}TAG_NUMBERS;

typedef struct {
    uint8_t length;
    void* data;
}RSN_INFO;

typedef struct {
    uint8_t size;
    void* data;
}TAG_INFO;

typedef struct {
    uint16_t channel;
    uint8_t frame;
    uint8_t rssi;
    uint8_t bssid[6];
    uint32_t seq_num;
}BSS_INFO;

typedef struct _WMI_HDR_INFO {
    uint8_t flag;       /* Flag in the header (second byte) */
    uint16_t size;      /* Size of the packet (bytes 3 and 4) */
    uint8_t offset;     /* Offset (byte 5) */
    uint8_t id;         /* Packet ID (byte 6) */
    uint16_t event;     /* Event type (bytes 7 and 8) */
}WMI_HDR_INFO;

typedef struct {
    WMI_HDR_INFO* hdr_info;
    union {
        BSS_INFO bss;
    }event;
    uint8_t timestamp[8];
    uint16_t interval;
    uint16_t capabilities;
    char* SSID;
    uint8_t channel;
    RSN_INFO rsn_info;
    uint8_t* vendor;
}SCAN_PACKET;

/**
 * Generic unmarshal function. This will take a block of data and fill in
 * the parameters byte by byte according to the signature passed in.
 *
 * @param data      Pointer to a block of data
 * @param sig       Signature string that corresponds to the subsequent parameters
 * @param ...[out]  Variable length list of parameters that correspond to the signature.
 *
 * @return          The total size of data that was unmarshalled.
 */
int32_t WMI_Unmarshal(void* data, const char* sig, ...);

/**
 * Unmarshal a BSSINFO (scan) packet.
 *
 * @param data      Pointer to a block of data containing the scan info
 * @param lenght    Size of the data block
 *
 * @return          Pointer to a scan item containing parsed data.
 */
wsl_scan_item* WMI_UnmarshalScan(void* data, uint16_t length);

#ifdef __cplusplus
}
#endif
