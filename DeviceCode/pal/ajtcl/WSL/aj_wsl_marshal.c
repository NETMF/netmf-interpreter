/**
 * @file Marshaling implementation
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

#define AJ_MODULE WSL_MARSHAL

#include <stdarg.h>
#include "aj_buf.h"
#include "aj_wsl_marshal.h"
#include "aj_wsl_target.h"
#include "aj_wsl_wmi.h"
#include "aj_wsl_unmarshal.h"
#include "aj_msg.h"
#include "aj_debug.h"

#ifndef NDEBUG
uint8_t dbgWSL_MARSHAL = 5;
#endif

static uint8_t packetId = 0;
uint8_t WMI_MarshalArgsBuf(AJ_BufList* data, const char* sig, uint16_t size, va_list* argpp)
{
    va_list argp;
    va_copy(argp, *argpp);
    AJ_BufNode* node;
    node = AJ_BufListCreateNode(size);
    uint8_t* ptr = node->buffer;
    while (*sig) {
        switch (*sig++) {
        case (WMI_ARG_UINT64):
            {
                uint64_t u64;
                u64 = (uint64_t)va_arg(argp, uint64_t);
                memcpy(ptr, &u64, sizeof(uint64_t));
                ptr += 8;
            }
            break;

        case (WMI_ARG_UINT32):
            {
                uint32_t u32;
                u32 = (uint32_t)va_arg(argp, uint32_t);
                memcpy(ptr, &u32, sizeof(uint32_t));
                ptr += 4;
            }
            break;

        case (WMI_ARG_IPV4):
            {
                uint8_t* IPv4;
                IPv4 = (uint8_t*)va_arg(argp, uint32_t);
                memcpy(ptr, IPv4, sizeof(uint8_t) * 4);
                ptr += 4;
            }
            break;

        case (WMI_ARG_IPV6):
            {
                uint16_t* IPv6;
                IPv6 = (uint16_t*)va_arg(argp, uint32_t);
                memcpy(ptr, IPv6, sizeof(uint16_t) * 8);
                ptr += 16;
            }
            break;

        case (WMI_ARG_UINT16):
            {
                uint16_t u16;
                u16 = (uint16_t)va_arg(argp, uint32_t);
                memcpy(ptr, &u16, sizeof(uint16_t));
                ptr += 2;
            }
            break;

        case (WMI_ARG_BYTE):
            {
                uint8_t u8;
                u8 = (uint8_t)va_arg(argp, uint32_t);
                memcpy(ptr, &u8, sizeof(uint8_t));
                ptr += 1;
            }
            break;

        case (WMI_ARG_MAC):
            {
                uint8_t* mac;
                mac = (uint8_t*)va_arg(argp, uint32_t);
                memcpy(ptr, mac, sizeof(uint8_t) * 6);
                ptr += 6;
            }
            break;

        case (WMI_ARG_SSID):
            {
                char* str;
                str = (char*)va_arg(argp, char*);
                memcpy(ptr, str, sizeof(char*) * strlen(str));
                ptr += 32;
            }
            break;

        case (WMI_ARG_PASSPHRASE):
            {
                char* str;
                str = (char*)va_arg(argp, char*);
                memcpy(ptr, str, sizeof(char*) * strlen(str));
                ptr += 64;
            }
            break;

        case (WMI_ARG_KEY):
            {
                uint8_t* key;
                key = (uint8_t*)va_arg(argp, uint32_t);
                memcpy(ptr, key, sizeof(uint8_t) * 32);
                ptr += 32;
            }
            break;

        default:
            AJ_ErrPrintf(("WMI_MarshalArgsBuf(): Unknown signature: %c\n", *sig));
            return 0;
        }
    }
    va_end(argp);
    AJ_BufListPushTail(data, node);
    return 1;
}

void WMI_MarshalHeader(AJ_BufList* packet, uint8_t endpoint, uint8_t flags)
{
    AJ_BufNode* header;
    uint16_t size;
    uint8_t trailer;
    header = AJ_BufListCreateNode(6);
    size = AJ_BufListGetSize(packet);
    trailer = 0x00;
    memcpy(header->buffer, &endpoint, sizeof(uint8_t));
    memcpy(header->buffer + 1, &flags, sizeof(uint8_t));
    memcpy(header->buffer + 2, &size, sizeof(uint16_t));
    memcpy(header->buffer + 4, &trailer, sizeof(uint8_t));
    memcpy(header->buffer + 5, &packetId, sizeof(uint8_t));
    packetId++;
    AJ_BufListPushHead(packet, header);
}

void WSL_MarshalPacket(AJ_BufList* packet, wsl_wmi_command_list command, ...)
{
    va_list args;
    uint16_t size;
    uint16_t cmd;
    uint32_t zero = 0;
    AJ_BufNode* cmdid;
    const char* signature;
    va_start(args, command);
    cmd = getCommandId(command);
    // Socket commands need to get their signature from a different map
    if (command == WSL_SOCKET) {
        cmdid = AJ_BufListCreateNode(10);
        memcpy(cmdid->buffer, &cmd, sizeof(uint16_t));
        memcpy(cmdid->buffer + 2, &zero, sizeof(uint32_t));
        uint32_t sock_cmd = (uint32_t)va_arg(args, uint32_t);
        memcpy(cmdid->buffer + 6, &sock_cmd, sizeof(uint32_t));
        signature = (char*)getSockSignature((wsl_socket_cmds)sock_cmd);
        size = getSockSize((wsl_socket_cmds)sock_cmd);
        AJ_BufListPushTail(packet, cmdid);
        WMI_MarshalArgsBuf(packet, signature, size - 4, &args);
    } else if ((command == WSL_SEND) || (command == WSL_SENDTO) || (command == WSL_SENDTO6)) {
        signature = (char*)getCommandSignature(command);
        size = getPacketSize(command);
        WMI_MarshalArgsBuf(packet, signature, size, &args);
    } else if (command == WSL_BIND6) {
        signature  = (char*)getCommandSignature(command);
        size = getPacketSize(command);
        cmdid = AJ_BufListCreateNode(2);
        uint16_t cmd_bind = 0xf08d;
        memcpy(cmdid->buffer, &cmd_bind, 2);
        AJ_BufListPushTail(packet, cmdid);
        WMI_MarshalArgsBuf(packet, signature, size - 2, &args);
    } else {
        cmdid = AJ_BufListCreateNode(sizeof(uint16_t));
        memcpy(cmdid->buffer, &cmd, sizeof(uint16_t));
        AJ_BufListPushTail(packet, cmdid);
        signature = (char*)getCommandSignature(command);
        size = getPacketSize(command);
        WMI_MarshalArgsBuf(packet, signature + 1, size - 2, &args);
    }
}

void WMI_MarshalSend(AJ_BufList* packet, uint32_t sock, AJ_BufNode* data, uint16_t size)
{
    WSL_MarshalPacket(packet, WSL_SEND, 0xa0000000, 0x009c0000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, sock, size);
    // send and sendto are marshaled differently. There packet structure is not similar to other WMI commands
    AJ_BufListPushTail(packet, data);
    WMI_MarshalHeader(packet, 2, 1);
    packetId++;
}
void WMI_MarshalSendTo(AJ_BufList* packet, uint32_t sock, AJ_BufNode* data, uint16_t size, uint32_t addr, uint16_t port)
{
    AJ_BufNode* zero23;
    AJ_BufNode* whereto;

    whereto = AJ_BufListCreateNode(9);
    zero23 = AJ_BufListCreateNode(23);

    memset(zero23->buffer, 0, 23);

    WSL_MarshalPacket(packet, WSL_SENDTO, 0xa0000000, 0x009c0000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, sock, size);

    memcpy(whereto->buffer, &port, 2);
    memcpy(whereto->buffer + 2, (void*)2, 2);
    memcpy(whereto->buffer + 4, &addr, 4);
    memcpy(whereto->buffer + 8, (void*)8, 1);

    AJ_BufListPushTail(packet, whereto);
    AJ_BufListPushTail(packet, zero23);
    AJ_BufListPushTail(packet, data);
    WMI_MarshalHeader(packet, 2, 1);

    packetId++;

}
void WMI_MarshalSendTo6(AJ_BufList* packet, uint32_t sock, AJ_BufNode* data, uint16_t size, uint8_t* addr, uint16_t port)
{
    AJ_BufNode* zero6;
    zero6 = AJ_BufListCreateNode(6);
    memset(zero6->buffer, 0, 6);

    WSL_MarshalPacket(packet, WSL_SENDTO6, 0xa0000000, 0x00bc0000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, sock, 0x1b, 0, 0, 0, 0, 0, 0, 0, 0x03, port, 0, addr, 0, size + 1);

    AJ_BufListPushTail(packet, zero6);
    AJ_BufListPushTail(packet, data);



    //AJ_BufListPushTail(packet, zero6);
    WMI_MarshalHeader(packet, 2, 1);
    packetId++;
}
