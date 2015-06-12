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
#define AJ_MODULE DEBUG

#include "aj_debug.h"

uint8_t dbgDEBUG = 0;

#ifndef NDEBUG

#include "aj_target.h"
#include "aj_util.h"
#include "aj_config.h"

#define Printable(c) (((c) >= ' ') && ((c) <= '~')) ? (c) : '.'

void _AJ_DumpBytes(const char* tag, const uint8_t* data, uint32_t len)
{
    uint32_t i;
    char ascii[AJ_DUMP_BYTE_SIZE + 1];

    if (tag) {
        AJ_AlwaysPrintf(("%s: (%u)\n", tag, len));
    }
    ascii[AJ_DUMP_BYTE_SIZE] = '\0';
    for (i = 0; i < len; i += AJ_DUMP_BYTE_SIZE) {
        uint32_t j;
        for (j = 0; j < AJ_DUMP_BYTE_SIZE; ++j, ++data) {
            if ((i + j) < len) {
                uint8_t n = *data;
                ascii[j] = Printable(n);
                if (n < 0x10) {
                    AJ_AlwaysPrintf(("0%x ", n));
                } else {
                    AJ_AlwaysPrintf(("%x ", n));
                }
            } else {
                ascii[j] = '\0';
                AJ_AlwaysPrintf(("   "));
            }
        }
        ascii[j] = '\0';
        AJ_AlwaysPrintf(("    %s\n", ascii));
    }
}

static const char* const msgType[] = { "INVALID", "CALL", "REPLY", "ERROR", "SIGNAL" };

void _AJ_DumpMsg(const char* tag, AJ_Message* msg, uint8_t body)
{
    if (msg->hdr && _AJ_DbgHeader(AJ_DEBUG_ERROR, NULL, 0)) {
#if AJ_DUMP_MSG_RAW
        uint8_t* p = (uint8_t*)msg->hdr + sizeof(AJ_MsgHeader);
        uint32_t hdrBytes = ((msg->hdr->headerLen + 7) & ~7);
#endif
        AJ_AlwaysPrintf(("%s message[%d] type %s sig=\"%s\"\n", tag, msg->hdr->serialNum, msgType[(msg->hdr->msgType <= 4) ? msg->hdr->msgType : 0], msg->signature));
        switch (msg->hdr->msgType) {
        case AJ_MSG_SIGNAL:
        case AJ_MSG_METHOD_CALL:
            AJ_AlwaysPrintf(("        %s::%s\n", msg->iface, msg->member));
            break;

        case AJ_MSG_ERROR:
            AJ_AlwaysPrintf(("        Error %s\n", msg->error));

        case AJ_MSG_METHOD_RET:
            AJ_AlwaysPrintf(("        Reply serial %d\n", msg->replySerial));
            break;
        }
        AJ_AlwaysPrintf(("        hdr len=%d\n", msg->hdr->headerLen));
#if AJ_DUMP_MSG_RAW
        AJ_DumpBytes(NULL, p,  hdrBytes);
        AJ_AlwaysPrintf(("body len=%d\n", msg->hdr->bodyLen));
        if (body) {
            AJ_DumpBytes(NULL, p + hdrBytes, msg->hdr->bodyLen);
        }
        AJ_AlwaysPrintf(("-----------------------\n"));
#endif
    }
}

int _AJ_DbgHeader(AJ_DebugLevel level, const char* file, int line)
{
    static AJ_Time initTime;
    uint32_t logTimeSecond;
    uint32_t logTimeMS;

    if (!(initTime.seconds | initTime.milliseconds)) {
        AJ_InitTimer(&initTime);
    }
    if (level <= AJ_DbgLevel) {
        AJ_Time debugTime;
        if (AJ_OK == AJ_GetDebugTime(&debugTime)) {
            logTimeSecond = debugTime.seconds;
            logTimeMS = debugTime.milliseconds;
        } else {
            uint32_t elapsedTime = AJ_GetElapsedTime(&initTime, TRUE);
            logTimeSecond = elapsedTime / 1000;
            logTimeMS = elapsedTime % 1000;
        }
        if (file) {
            const char* fn = file;
            while (*fn) {
                if ((*fn == '/') || (*fn == '\\')) {
                    file = fn + 1;
                }
                ++fn;
            }
            AJ_AlwaysPrintf(("%03d.%03d %s:%d ", logTimeSecond, logTimeMS, file, line));
        } else {
            AJ_AlwaysPrintf(("%03d.%03d ", logTimeSecond, logTimeMS));
        }
        return TRUE;
    } else {
        return FALSE;
    }
}


AJ_DebugLevel AJ_DbgLevel = AJ_DEBUG_INFO;
uint8_t dbgALL = 0;

#endif

#define AJ_CASE(_status) case _status: return # _status

const char* AJ_StatusText(AJ_Status status)
{
#ifdef NDEBUG
    /* Expectation is that thin client status codes will NOT go beyond 255 */
    static char code[4];

#ifdef _WIN32
    _snprintf(code, sizeof(code), "%03u", status);
#else
    snprintf(code, sizeof(code), "%03u", status);
#endif

    return code;
#else
    switch (status) {
        AJ_CASE(AJ_OK);
        AJ_CASE(AJ_ERR_NULL);
        AJ_CASE(AJ_ERR_UNEXPECTED);
        AJ_CASE(AJ_ERR_INVALID);
        AJ_CASE(AJ_ERR_IO_BUFFER);
        AJ_CASE(AJ_ERR_READ);
        AJ_CASE(AJ_ERR_WRITE);
        AJ_CASE(AJ_ERR_TIMEOUT);
        AJ_CASE(AJ_ERR_MARSHAL);
        AJ_CASE(AJ_ERR_UNMARSHAL);
        AJ_CASE(AJ_ERR_END_OF_DATA);
        AJ_CASE(AJ_ERR_RESOURCES);
        AJ_CASE(AJ_ERR_NO_MORE);
        AJ_CASE(AJ_ERR_SECURITY);
        AJ_CASE(AJ_ERR_CONNECT);
        AJ_CASE(AJ_ERR_UNKNOWN);
        AJ_CASE(AJ_ERR_NO_MATCH);
        AJ_CASE(AJ_ERR_SIGNATURE);
        AJ_CASE(AJ_ERR_DISALLOWED);
        AJ_CASE(AJ_ERR_FAILURE);
        AJ_CASE(AJ_ERR_RESTART);
        AJ_CASE(AJ_ERR_LINK_TIMEOUT);
        AJ_CASE(AJ_ERR_DRIVER);
        AJ_CASE(AJ_ERR_OBJECT_PATH);
        AJ_CASE(AJ_ERR_BUSY);
        AJ_CASE(AJ_ERR_DHCP);
        AJ_CASE(AJ_ERR_ACCESS);
        AJ_CASE(AJ_ERR_SESSION_LOST);
        AJ_CASE(AJ_ERR_LINK_DEAD);
        AJ_CASE(AJ_ERR_HDR_CORRUPT);
        AJ_CASE(AJ_ERR_RESTART_APP);
        AJ_CASE(AJ_ERR_INTERRUPTED);
        AJ_CASE(AJ_ERR_REJECTED);
        AJ_CASE(AJ_ERR_RANGE);
        AJ_CASE(AJ_ERR_ACCESS_ROUTING_NODE);
        AJ_CASE(AJ_ERR_KEY_EXPIRED);
        AJ_CASE(AJ_ERR_SPI_NO_SPACE);
        AJ_CASE(AJ_ERR_SPI_READ);
        AJ_CASE(AJ_ERR_SPI_WRITE);
        AJ_CASE(AJ_ERR_OLD_VERSION);
        AJ_CASE(AJ_ERR_NVRAM_READ);
        AJ_CASE(AJ_ERR_NVRAM_WRITE);

        AJ_CASE(AJ_ERR_WOULD_BLOCK);
        AJ_CASE(AJ_ERR_ARDP_DISCONNECTED);
        AJ_CASE(AJ_ERR_ARDP_DISCONNECTING);
        AJ_CASE(AJ_ERR_ARDP_REMOTE_CONNECTION_RESET);
        AJ_CASE(AJ_ERR_ARDP_PROBE_TIMEOUT);
        AJ_CASE(AJ_ERR_ARDP_BACKPRESSURE);
        AJ_CASE(AJ_ERR_ARDP_SEND_EXPIRED);
        AJ_CASE(AJ_ERR_ARDP_RECV_EXPIRED);
        AJ_CASE(AJ_ERR_ARDP_VERSION_NOT_SUPPORTED);

    default:
        return "<unknown>";
    }
#endif
}
