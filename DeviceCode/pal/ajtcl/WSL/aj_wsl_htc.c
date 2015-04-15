/**
 * @file HTC layer implementation
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

#define AJ_MODULE WSL_HTC

#include "aj_target.h"
#include "aj_wsl_spi.h"
#include "aj_wsl_htc.h"
#include "aj_wsl_wmi.h"
#include "aj_buf.h"
#include "aj_debug.h"
#include "aj_wsl_unmarshal.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgWSL_HTC = 0;
#endif

/**
 * global variable for WSL_HTC state
 */
AJ_WSL_HTC_CONTEXT AJ_WSL_HTC_Global;

void AJ_WSL_HTC_ModuleInit(void)
{
    //Initialize the credit-tracking information
    memset(&AJ_WSL_HTC_Global, 0, sizeof(AJ_WSL_HTC_Global));
    AJ_WSL_HTC_Global.endpoints[0].txCredits = 1;
    AJ_WSL_HTC_Global.endpoints[1].txCredits = 2;
    AJ_WSL_HTC_Global.endpoints[2].txCredits = 10;
    AJ_WSL_HTC_Global.started = FALSE;
    AJ_WSL_SPI_ModuleInit();
}
uint8_t AJ_WSL_IsDriverStarted()
{
    return AJ_WSL_HTC_Global.started;
}


void AJ_WSL_HTC_ProcessInterruptCause(void)
{
    uint16_t cause = 0;
    AJ_Status status = AJ_ERR_SPI_READ;

    status = AJ_WSL_SPI_RegisterRead(AJ_WSL_SPI_REG_INTR_CAUSE, (uint8_t*)&cause);
    AJ_ASSERT(status == AJ_OK);
    cause = LE16_TO_CPU(cause);
    if (cause & AJ_WSL_SPI_REG_INTR_CAUSE_DATA_AVAILABLE) {
        AJ_WSL_HTC_ProcessIncoming();
        cause = cause ^ AJ_WSL_SPI_REG_INTR_CAUSE_DATA_AVAILABLE; //clear the bit
    }
    if (cause & AJ_WSL_SPI_REG_INTR_CAUSE_READ_DONE) {
        uint16_t clearCause = CPU_TO_LE16(AJ_WSL_SPI_REG_INTR_CAUSE_READ_DONE);
        status = AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_INTR_CAUSE, clearCause);
        AJ_ASSERT(status == AJ_OK);
        cause = cause ^ AJ_WSL_SPI_REG_INTR_CAUSE_READ_DONE;
    }
    if (cause & AJ_WSL_SPI_REG_INTR_CAUSE_WRITE_DONE) {
        uint16_t clearCause = CPU_TO_LE16(AJ_WSL_SPI_REG_INTR_CAUSE_WRITE_DONE);
        status = AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_INTR_CAUSE, clearCause);
        AJ_ASSERT(status == AJ_OK);
        cause = cause ^ AJ_WSL_SPI_REG_INTR_CAUSE_WRITE_DONE;
    }
    if (cause & AJ_WSL_SPI_REG_INTR_CAUSE_CPU_AWAKE) {
        uint16_t clearCause = CPU_TO_LE16(AJ_WSL_SPI_REG_INTR_CAUSE_CPU_AWAKE);
        status = AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_INTR_CAUSE, clearCause);
        AJ_ASSERT(status == AJ_OK);
        cause = cause ^ AJ_WSL_SPI_REG_INTR_CAUSE_CPU_AWAKE;
    }
    if (cause & AJ_WSL_SPI_REG_INTR_CAUSE_COUNTER) {
        uint16_t clearCause = CPU_TO_LE16(AJ_WSL_SPI_REG_INTR_CAUSE_COUNTER);
        status = AJ_WSL_SPI_RegisterWrite(AJ_WSL_SPI_REG_INTR_CAUSE, clearCause);
        AJ_ASSERT(status == AJ_OK);
        cause = cause ^ AJ_WSL_SPI_REG_INTR_CAUSE_COUNTER;
    }
    if (cause & ~AJ_WSL_SPI_REG_INTR_CAUSE_DATA_AVAILABLE) {
        //AJ_InfoPrintf(("Some other interrupt cause as well %x\n", cause));
    }
}

/*
 *  read from the mailbox and do something useful with the HTC message.
 */
void AJ_WSL_HTC_ProcessIncoming(void)
{
    uint16_t lenRead;
    uint8_t* bufRead;
    AJ_BufNode* pNodeHTCBody;
    uint8_t endpointID;
    uint8_t flags;
    uint16_t payloadLength;
    uint8_t controlBytes[2];

    AJ_Status status;
    status = AJ_WSL_ReadFromMBox(AJ_WSL_SPI_MBOX_0, &lenRead, &bufRead);
    AJ_ASSERT(status == AJ_OK);
    // now create a AJ_BufList from the data we read
    pNodeHTCBody = AJ_BufListCreateNodeExternalZero(bufRead, lenRead, FALSE);
    pNodeHTCBody->flags = 0; // reset the AJ_BUFNODE_EXTERNAL_BUFFER flag, because I own this now.
    WMI_Unmarshal(pNodeHTCBody->buffer, "yyqyy", &endpointID, &flags, &payloadLength, &controlBytes[0], &controlBytes[1]);
    AJ_BufNodePullBytes(pNodeHTCBody, 6);

    //AJ_WSL_WMI_PrintMessage(pNodeHTCBody);
    AJ_DumpBytes("HTC_CONTROL", pNodeHTCBody->buffer, pNodeHTCBody->length);
    /* examine the endpoint of the HTC message*/
    if ((flags & AJ_WSL_HTC_RECV_TRAILER_PRESENT) && ((payloadLength - controlBytes[0]) > 0)) {
        switch (endpointID) {
        case AJ_WSL_HTC_CONTROL_ENDPOINT: {
                uint16_t* messageID = (uint16_t*)pNodeHTCBody->buffer;
                AJ_InfoPrintf(("Read HTC control endpoint, messageID %x\n",  *messageID));

                switch (*messageID) {
                case AJ_WSL_HTC_MSG_READY_ID: {
                        /* process the message and change state */
                        uint16_t readyMessageID;
                        WMI_Unmarshal(pNodeHTCBody->buffer, "qqqyyy",
                                      &readyMessageID,
                                      &AJ_WSL_HTC_Global.creditCount,
                                      &AJ_WSL_HTC_Global.creditSize,
                                      &AJ_WSL_HTC_Global.maxEndpoints,
                                      &AJ_WSL_HTC_Global.HTCVersion,
                                      &AJ_WSL_HTC_Global.maxMessagesPerBundle);
                        /* SIDE EFFECT */
                        AJ_WarnPrintf(("MSG_READY, credit count %x, credit size:%d\n", AJ_WSL_HTC_Global.creditCount, AJ_WSL_HTC_Global.creditSize));
                        AJ_WSL_HTC_Global.endpoints[AJ_WSL_HTC_CONTROL_ENDPOINT].state = AJ_WSL_HTC_UNINITIALIZED_RECV_READY;
                        break;
                    }

                case AJ_WSL_HTC_SERVICE_CONNECT_RESPONSE_ID: {
                        AJ_WarnPrintf(("HTC MSG AJ_WSL_HTC_SERVICE_CONNECT_RESPONSE_ID\n"));
                        break;
                    }

                default:
                    AJ_WarnPrintf(("HTC MSG ID unknown\n"));
                    AJ_DumpBytes("WMI_SOCKET_RESPONSE", pNodeHTCBody->buffer, pNodeHTCBody->length);
                    break;
                }
                break;
            }

        case AJ_WSL_HTC_DATA_ENDPOINT1: {
//            AJ_DumpBytes("DATA ENDPOINT WMI ", pNodeHTCBody->buffer, pNodeHTCBody->length);
                AJ_WSL_WMI_ProcessWMIEvent(pNodeHTCBody);

                break;
            }

        case AJ_WSL_HTC_DATA_ENDPOINT2:
        case AJ_WSL_HTC_DATA_ENDPOINT3:
        case AJ_WSL_HTC_DATA_ENDPOINT4: {
                AJ_DumpBytes("DATA ENDPOINT WMI_SOCKET_RESPONSE", pNodeHTCBody->bufferStart, pNodeHTCBody->length);
                AJ_WSL_WMI_ProcessSocketDataResponse(pNodeHTCBody);

                //AJ_InfoPrintf(("Read HTC data endpoint %d\n", htcHdr.endpointID));
                // TODO send the data up to the next API level
                //AJ_WSL_WMI_PrintMessage(pNodeHTCBody);
                break;
            }

        default:
            AJ_ErrPrintf(("UNKNOWN Endpoint %d", endpointID));
            AJ_ASSERT(FALSE);
            break;
        }
    }
    /*
     * Trailers can come on any packet
     */
    if (flags & AJ_WSL_HTC_RECV_TRAILER_PRESENT) {

        uint16_t packetLength;
        uint8_t trailerLength =  controlBytes[0];
        packetLength = payloadLength - trailerLength;
        //AJ_InfoPrintf(("Read HTC RX trailer, length %d\n", htcHdr.controlBytes[0]));
        AJ_BufNodePullBytes(pNodeHTCBody, packetLength); // consume the packet, leave the trailer
        /*
         * handle multiple trailers per HTC message
         */
        while (trailerLength) {
            uint8_t trailerType;
            uint8_t length;
            //AJ_BufListNodePrintDump(pNodeHTCBody, NULL);
            WMI_Unmarshal(pNodeHTCBody->buffer, "yy", &trailerType, &length);
            AJ_BufNodePullBytes(pNodeHTCBody, 2); // consume the trailer

            trailerLength -= 2;
            switch (trailerType) {
            case AJ_WSL_HTC_RXTRAILER_CREDIT_REPORT: {
                    uint8_t credits;
                    uint8_t endpoint;
                    WMI_Unmarshal(pNodeHTCBody->buffer, "yy", &endpoint, &credits);
                    AJ_InfoPrintf(("Add %d credits to endpoint %d\n", credits, endpoint));
                    AJ_WSL_HTC_Global.endpoints[endpoint].txCredits += credits;
                    break;
                }

            default: {
                    AJ_InfoPrintf(("HTC trailer: type not handled %d\n", trailerType));

                }
            }
            trailerLength -= length;
            AJ_BufNodePullBytes(pNodeHTCBody, length); // consume the trailer
        }
    }

    //intentionally freeing the buffer here
    AJ_BufListFreeNodeAndBuffer(pNodeHTCBody, NULL);
}
