/**
 * @file Unit tests for SPI AJ_WSL.
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

#ifdef __cplusplus
extern "C" {
#endif



#define AJ_MODULE WSL_UNIT_TEST

#include "aj_target.h"
#include "../../WSL/aj_buf.h"
#include "../../WSL/aj_wsl_htc.h"
#include "../../WSL/aj_wsl_spi.h"
#include "../../WSL/aj_wsl_wmi.h"
#include "../../WSL/aj_wsl_net.h"
#include "aj_debug.h"


/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgWSL_UNIT_TEST = 5;
#endif

static void ResetFakeWireBuffers()
{
    toTarget.fakeWireWrite = toTarget.fakeWireBuffer; // reset the write pointer to the start of our buffer
    toTarget.fakeWireRead = toTarget.fakeWireBuffer; // reset the read pointer to the start of our buffer
    fromTarget.fakeWireWrite = fromTarget.fakeWireBuffer; // reset the write pointer to the start of our buffer
    fromTarget.fakeWireRead = fromTarget.fakeWireBuffer; // reset the read pointer to the start of our buffer
}


static void run_wsl_htc_parsing(const struct test_case* test)
{

    /*
     * create a chain of buffers that simulate each protocol layer wrapping the layer above it.
     * The values chosen to populate the buffers are not legal protocol values, they just show
     * where the bytes are laid out in memory.
     */
    AJ_BufList* list2 = AJ_BufListCreate();

    AJ_BufNode* pNodeAppData = AJ_BufListCreateNodeZero(16, 1);
    AJ_BufNode_WMI* pNodeWMI;
    AJ_BufNode* pNodeWMITrail;
    AJ_BufNode* pNodeHTC;
    wsl_htc_msg_connect_service* msgConnectService;
    AJ_BufNode* pNodeHTCTrail;
    AJ_BufNode* pNodeHTCHeader;
    wsl_htc_hdr* msgHdr;

    AJ_AlwaysPrintf(("\n\n**************\nTEST:  %s\n\n", __FUNCTION__));


    strcpy((char*)pNodeAppData->buffer, "AppData string");

    pNodeWMI = AJ_BufListCreateNode(sizeof(wsl_spi_command));

    // stuff the buffer with a command.
    ((wsl_spi_command*)pNodeWMI->buffer)->cmd_addr = AJ_WSL_SPI_SPI_STATUS;
    ((wsl_spi_command*)pNodeWMI->buffer)->cmd_rx = 1;
    ((wsl_spi_command*)pNodeWMI->buffer)->cmd_reg = 1;
    *((uint16_t*)pNodeWMI->buffer) = CPU_TO_BE16(*(uint16_t*)pNodeWMI->buffer);   // Swap the bytes around

    // create a bogus trailer
    pNodeWMITrail = AJ_BufListCreateNode(8);
    memset(pNodeWMITrail->buffer, 0xA1, 8);


    // create an HTC command
    pNodeHTC = AJ_BufListCreateNode(sizeof(wsl_htc_msg_connect_service));
    msgConnectService = (wsl_htc_msg_connect_service*)pNodeHTC->buffer;
    msgConnectService->msg.messageID = AJ_WSL_HTC_CONNECT_SERVICE_ID;
    msgConnectService->serviceID = 0x5678; // random choice
    msgConnectService->flags = 0xCDEF; // random choice
    msgConnectService->serviceMetadataLength = 0; // random choice
    msgConnectService->metadata = 0; // random choice
    AJ_WSL_HTC_MSG_CONNECT_TO_WIRE(msgConnectService);
    // create another bogus trailer
    pNodeHTCTrail = AJ_BufListCreateNode(8);
    memset(pNodeHTCTrail->buffer, 0xB2, 8);



    // AppData was added then WMI was added, then HTC wrapped around that
    AJ_BufListPushHead(list2, pNodeAppData);
    AJ_BufListPushHead(list2, pNodeWMI);
    AJ_BufListPushTail(list2, pNodeWMITrail);

    AJ_BufListPushHead(list2, pNodeHTC);
    AJ_BufListPushTail(list2, pNodeHTCTrail);

    // create an HTC header structure with the correct size field (based on the size of the data to send on the wire)
    pNodeHTCHeader = AJ_BufListCreateNode(sizeof(wsl_htc_hdr));
    msgHdr = (wsl_htc_hdr*)pNodeHTCHeader->buffer;
    msgHdr->endpointID = AJ_WSL_HTC_CONTROL_ENDPOINT;
    msgHdr->flags = 0;
    msgHdr->controlBytes[0]  = 0xAB;
    msgHdr->controlBytes[1]  = 0xCD;
    msgHdr->payloadLength = AJ_BufListLengthOnWire(list2);
    AJ_WSL_HTC_HDR_TO_WIRE(msgHdr);

    AJ_AlwaysPrintf(("\n\nPayload size would be: %d\n\n", AJ_BufListLengthOnWire(list2)));
    AJ_BufListPushHead(list2, pNodeHTCHeader);


    AJ_AlwaysPrintf(("%s", "write this to the wire\n\n"));
    AJ_BufListIterateOnWire(AJ_BufListWriteToWire_Simulated, list2, &toTarget);
    AJ_AlwaysPrintf(("\n\nDone wire write, length on wire: %d\n\n", AJ_BufListLengthOnWire(list2)));

    AJ_BufListFree(list2, 1);

/*
    {
        // simulate a number of reads from the SPI buffer
        fakeWireRead = fakeWireBuffer;
        uint16_t readBufferSize = sizeof(fakeWireBuffer);

        AJ_AlwaysPrintf(("%s", "Wire Bufer\n"));
        while (readBufferSize > 0) {
            uint8_t byteRead  = AJ_BufListReadByteFromWire_Simulated();
            AJ_AlwaysPrintf(("%02X ", byteRead));
            readBufferSize--;
        }
    }
 */

    toTarget.fakeWireRead = toTarget.fakeWireBuffer;
//    fakeWireRead = fakeWireBuffer; // reset the read pointer to the start of our buffer
    {
        wsl_htc_hdr htcHdr1;
        AJ_BufNode* pNodeHTCBody;
        wsl_htc_msg* htcMsg1;

        AJ_AlwaysPrintf(("%s", "Read HTC header from wire buffer\n"));
        AJ_BufListReadBytesFromWire_Simulated(sizeof(htcHdr1), (uint8_t*)&htcHdr1, &toTarget);

        // convert the fields to the correct endianness
        AJ_WSL_HTC_HDR_FROM_WIRE(&htcHdr1);
        AJ_AlwaysPrintf(("\n HTC Hdr payload length 0x%04x\n\n", htcHdr1.payloadLength));

        AJ_AlwaysPrintf(("%s", "Read HTC from wire buffer\n"));
        pNodeHTCBody = AJ_BufListCreateNode(htcHdr1.payloadLength /*+ sizeof(wsl_htc_msg)*/);
        htcMsg1 = (wsl_htc_msg*)pNodeHTCBody->buffer;

        AJ_BufListReadBytesFromWire_Simulated(pNodeHTCBody->length, pNodeHTCBody->buffer, &toTarget);

        switch (htcHdr1.endpointID) {
        case AJ_WSL_HTC_CONTROL_ENDPOINT:
            AJ_AlwaysPrintf(("%s", "Read HTC control endpoint\n"));

//                break;
        case AJ_WSL_HTC_DATA_ENDPOINT1:
        case AJ_WSL_HTC_DATA_ENDPOINT2:
        case AJ_WSL_HTC_DATA_ENDPOINT3:
        case AJ_WSL_HTC_DATA_ENDPOINT4:
            AJ_AlwaysPrintf(("\n%s %d\n", "Read HTC data endpoint", htcHdr1.endpointID));
            // TODO send the data up to the next API level
            AJ_WSL_WMI_PrintMessage(pNodeHTCBody);
            break;

        default:
            AJ_AlwaysPrintf(("%s %d", "UNKNOWN Endpoint", htcHdr1.endpointID));
            break;

        }



//        AJ_BufListReadBytesFromWire_Simulated(pNodeHTCBody->length, pNodeHTCBody->buffer);
        AJ_WSL_HTC_MSG_FROM_WIRE(htcMsg1);
        switch (htcMsg1->messageID) {
        case AJ_WSL_HTC_MSG_READY_ID: {
                wsl_htc_msg_ready* htcMsgReady1 = (wsl_htc_msg_ready*)pNodeHTCBody->buffer;
                AJ_AlwaysPrintf(("%s", "Read HTC msg AJ_WSL_HTC_MSG_READY_ID \n"));
                AJ_WSL_HTC_MSG_READY_FROM_WIRE(htcMsgReady1);
                AJ_AlwaysPrintf(("\n HTC connect service message 0x%04x, CreditCount 0x%04X CreditSize 0x%04X\n\n", htcMsgReady1->msg.messageID, htcMsgReady1->creditCount, htcMsgReady1->creditSize));
                break;
            }

        case AJ_WSL_HTC_CONNECT_SERVICE_ID: {
                wsl_htc_msg_connect_service* htcMsgCS1 = (wsl_htc_msg_connect_service*) pNodeHTCBody->buffer;
                AJ_AlwaysPrintf(("%s", "Read HTC msg AJ_WSL_HTC_CONNECT_SERVICE_ID \n"));
                AJ_WSL_HTC_MSG_CONNECT_FROM_WIRE(htcMsgCS1);
                AJ_AlwaysPrintf(("\n HTC connect service message 0x%04x, serviceID 0x%04X \n\n", htcMsgCS1->msg.messageID, htcMsgCS1->serviceID));
                break;
            }

        case AJ_WSL_HTC_SERVICE_CONNECT_RESPONSE_ID: {
                wsl_htc_msg_service_connect_response* htcServiceConnectResponse1 = (wsl_htc_msg_service_connect_response*)pNodeHTCBody->buffer;
                AJ_AlwaysPrintf(("%s", "Read HTC msg AJ_WSL_HTC_SERVICE_CONNECT_RESPONSE_ID \n"));
                AJ_WSL_HTC_MSG_SERVICE_CONNECT_RESPONSE_FROM_WIRE(htcServiceConnectResponse1);
                AJ_AlwaysPrintf(("\n HTC service connect response 0x%04x, serviceID 0x%04X metadatalength 0x%04X\n\n", htcServiceConnectResponse1->msg.messageID, htcServiceConnectResponse1->serviceID, htcServiceConnectResponse1->serviceMetadataLength));
                break;
            }

        case AJ_WSL_HTC_SETUP_COMPLETE_ID: {
                AJ_AlwaysPrintf(("%s", "Read HTC msg AJ_WSL_HTC_SETUP_COMPLETE_ID \n"));
                break;
            }

        case AJ_WSL_HTC_HOST_READY_ID: {
                AJ_AlwaysPrintf(("%s", "Read HTC msg AJ_WSL_HTC_HOST_READY_ID \n"));
                break;
            }

        default: {
                AJ_ASSERT(htcMsg1->messageID <= AJ_WSL_HTC_HOST_READY_ID);
                break;
            }
        }
    }
}

static void AJ_WSL_HTCProcessControlMessage_Fake(AJ_BufNode* pNodeHTCBody)
{
    wsl_wmi_cmd_hdr* wmiCmdHdr2;

    wmiCmdHdr2 = (wsl_wmi_cmd_hdr*)pNodeHTCBody->buffer;
    AJ_WSL_WMI_CMD_HDR_FROM_WIRE(wmiCmdHdr2);
    AJ_WSL_WMI_CMD_HDR_Print(wmiCmdHdr2);
    AJ_BufNodePullBytes(pNodeHTCBody, sizeof(wsl_wmi_cmd_hdr));
    if (wmiCmdHdr2->commandID == WMI_SOCKET_CMDID) {
        wsl_wmi_socket_cmd* pSocketCmd = (wsl_wmi_socket_cmd*)pNodeHTCBody->buffer;
        AJ_WSL_WMI_SOCKET_CMD_FROM_WIRE(pSocketCmd);
        AJ_WSL_WMI_SOCKET_CMD_HDR_Print(pSocketCmd);


        if (pSocketCmd->cmd_type == WSL_SOCK_PING) {
            switch (pSocketCmd->cmd_type) {
            case WSL_SOCK_OPEN: {
                    wsl_wmi_sock_open* open = (wsl_wmi_sock_open*)pNodeHTCBody->buffer;
                    AJ_WSL_WMI_SOCK_OPEN_FROM_WIRE(open);
                    AJ_AlwaysPrintf(("OPEN call was received over the wire: domain 0x%x, type 0x%x protocol 0x%x\n\n", open->domain, open->protocol, open->type));
                    break;
                }

            case WSL_SOCK_PING: {
                    wsl_wmi_sock_ping* ping = (wsl_wmi_sock_ping*)pNodeHTCBody->buffer;
                    AJ_WSL_WMI_SOCK_PING_FROM_WIRE(ping);
                    AJ_AlwaysPrintf(("PING call was received over the wire: addr 0x%x, size 0x%x\n\n", ping->ip_addr, ping->size));
                    break;
                }

            default:
                AJ_ASSERT("unknown socket command\n\n");
            }
        }

    }
}


static void AJ_WSL_HTCProcessControlMessageResponse_Fake(AJ_BufNode* pNodeHTCBody)
{
    wsl_wmi_cmd_hdr* wmiCmdHdr2;
    wmiCmdHdr2 = (wsl_wmi_cmd_hdr*)pNodeHTCBody->buffer;
    AJ_WSL_WMI_CMD_HDR_FROM_WIRE(wmiCmdHdr2);
    AJ_WSL_WMI_CMD_HDR_Print(wmiCmdHdr2);
    AJ_BufNodePullBytes(pNodeHTCBody, sizeof(wsl_wmi_cmd_hdr));
    if (wmiCmdHdr2->commandID == WMI_SOCKET_CMDID) {
        wsl_wmi_socket_response_event* pSocketResp = (wsl_wmi_socket_response_event*)pNodeHTCBody->buffer;
        AJ_WSL_WMI_SOCK_RESPONSE_FROM_WIRE(pSocketResp);

        switch (pSocketResp->responseType) {
        case WSL_SOCK_OPEN: {
                //AJ_AlwaysPrintf(("PING response was received over the wire: addr 0x%x, size 0x%x\n\n", ping->ip_addr, ping->size));
                AJ_AlwaysPrintf(("OPEN response was received over the wire, Handle is %x\n\n", pSocketResp->socketHandle));
                break;
            }

        case WSL_SOCK_PING: {
                //wsl_wmi_sock_ping* ping = (wsl_wmi_sock_ping*)pNodeHTCBody->buffer;
                //AJ_WSL_WMI_SOCK_PING_FROM_WIRE(ping);
                //AJ_AlwaysPrintf(("PING response was received over the wire: addr 0x%x, size 0x%x\n\n", ping->ip_addr, ping->size));
                AJ_AlwaysPrintf(("PING response was received over the wire, Handle is %x\n\n", pSocketResp->socketHandle));
                break;
            }

        default:
            AJ_ASSERT("unknown socket command\n\n");
        }
    }
}





static void run_wsl_simulate_ping(const struct test_case* test)
{
    AJ_BufNode* pNodeHTCBody;
    wsl_htc_hdr htcHdr1;

    AJ_AlwaysPrintf(("\n\n**************\nTEST:  %s\n\n", __FUNCTION__));


    toTarget.fakeWireWrite = toTarget.fakeWireBuffer; // reset the write pointer to the start of our buffer
    toTarget.fakeWireRead = toTarget.fakeWireBuffer; // reset the read pointer to the start of our buffer

    AJ_WSL_NET_ping(0xC0A80101, 100);




    // now, try to receive a command from the target (we should really try to receive a response and event id)

    AJ_AlwaysPrintf(("%s", "Read HTC header from wire buffer\n"));
    AJ_BufListReadBytesFromWire_Simulated(sizeof(htcHdr1), (uint8_t*)&htcHdr1, &toTarget);

    // convert the fields to the correct endianness
    AJ_WSL_HTC_HDR_FROM_WIRE(&htcHdr1);
    AJ_AlwaysPrintf(("\n HTC Hdr payload length 0x%04x\n\n", htcHdr1.payloadLength));

    AJ_AlwaysPrintf(("%s", "Read HTC from wire buffer\n"));
    pNodeHTCBody = AJ_BufListCreateNode(htcHdr1.payloadLength);

    AJ_BufListReadBytesFromWire_Simulated(pNodeHTCBody->length, pNodeHTCBody->buffer, &toTarget);


    /* examine the endpoint of the HTC message*/
    switch (htcHdr1.endpointID) {
    case AJ_WSL_HTC_CONTROL_ENDPOINT: {
            AJ_AlwaysPrintf(("%s", "Read HTC control endpoint\n"));

            /* For this test, we know this is a control endpoint */
            AJ_WSL_HTCProcessControlMessage_Fake(pNodeHTCBody);

            break;
        }

    case AJ_WSL_HTC_DATA_ENDPOINT1:
    case AJ_WSL_HTC_DATA_ENDPOINT2:
    case AJ_WSL_HTC_DATA_ENDPOINT3:
    case AJ_WSL_HTC_DATA_ENDPOINT4:
        AJ_AlwaysPrintf(("\n%s %d\n", "Read HTC data endpoint", htcHdr1.endpointID));
        // TODO send the data up to the next API level
        AJ_WSL_WMI_PrintMessage(pNodeHTCBody);
        break;

    default:
        AJ_AlwaysPrintf(("%s %d", "UNKNOWN Endpoint", htcHdr1.endpointID));
        break;
    }

    //        wsl_htc_msg* htcMsg1 = pNodeHTCBody->buffer;
    //        AJ_WSL_HTC_MSG_FROM_WIRE(htcMsg1);


}

/*
 * This test builds a ping response in a simulated wire buffer, and then parses the response
 */
static void run_wsl_simulate_ping_recv(const struct test_case* test)
{
    AJ_AlwaysPrintf(("\n\n**************\nTEST:  %s\n\n", __FUNCTION__));
    ResetFakeWireBuffers();
    AJ_WSL_NET_ping_FAKERESPONSE();
    AJ_WSL_HTC_ProcessIncoming();
}



/*
 * This test builds a ping response in a simulated wire buffer, and then parses the response
 */
static void run_wsl_simulate_socket_open_recv(const struct test_case* test)
{
    AJ_AlwaysPrintf(("\n\n**************\nTEST:  %s\n\n", __FUNCTION__));
    ResetFakeWireBuffers();
    AJ_WSL_NET_socket_open_FAKERESPONSE();
    AJ_WSL_HTC_ProcessIncoming();



}


/*
 * This test builds a socket close response in a simulated wire buffer, and then parses the response
 */
static void run_wsl_simulate_socket_close_recv(const struct test_case* test)
{
    AJ_AlwaysPrintf(("\n\n**************\nTEST:  %s\n\n", __FUNCTION__));

    ResetFakeWireBuffers();
    AJ_WSL_NET_socket_close_FAKERESPONSE();
    AJ_WSL_HTC_ProcessIncoming();
}


/*
 * This test builds a ping response in a simulated wire buffer, and then parses the response
 */
static void run_wsl_simulate_state_machine(const struct test_case* test)
{
    AJ_Status status = AJ_OK;
    uint16_t i;

    AJ_AlwaysPrintf(("\n\n**************\nTEST:  %s\n\n", __FUNCTION__));

    /*
     * reset the state of the simulated wire buffers
     */
    memset(toTarget.fakeWireBuffer, 0, sizeof(toTarget.fakeWireBuffer));
    toTarget.fakeWireCurr = toTarget.fakeWireBuffer;
    memset(fromTarget.fakeWireBuffer, 0, sizeof(fromTarget.fakeWireBuffer));
    fromTarget.fakeWireCurr = fromTarget.fakeWireBuffer;
    ResetFakeWireBuffers();

    /*
     * reset the state of the HTC endpoints
     */
    memset(&AJ_WSL_HTC_Global, 0, sizeof(AJ_WSL_HTC_Global));

    /*
     * TODO: prime the state machine by creating a HTC Ready packet and adding it to the simulated wirebufer
     */
    {
        wsl_htc_hdr readyHdr;
        wsl_htc_msg_ready readyMsg;

        readyMsg.msg.messageID = AJ_WSL_HTC_MSG_READY_ID;
        readyMsg.creditCount = 20;
        readyMsg.creditSize = 512;
        readyMsg.HTCVersion = 0;
        readyMsg.maxEndpoints = 5;
        readyMsg.maxMessagesPerBundle = 1;
        AJ_WSL_HTC_MSG_READY_TO_WIRE(&readyMsg);
        readyHdr.endpointID = 0;
        readyHdr.flags = 0;
        readyHdr.controlBytes[0] = 0;
        readyHdr.controlBytes[1] = 0;
        readyHdr.payloadLength = sizeof(wsl_htc_msg_ready);
        AJ_WSL_HTC_HDR_TO_WIRE(&readyHdr);
        memcpy(fromTarget.fakeWireBuffer, &readyHdr, sizeof(readyHdr));
        memcpy(fromTarget.fakeWireBuffer + sizeof(readyHdr), &readyMsg, sizeof(readyMsg));
    }

    for (i = 0; i < 5; ++i) {


        switch (i) {
        case 0:
        case 1:
        case 2: {
                AJ_AlwaysPrintf(("Case %d send\n", i));
                AJ_WSL_HTC_StateMachine(&AJ_WSL_HTC_Global.endpoints[0]);
                if (status != AJ_OK) {
                    break;
                }
                break;
            }

        default:
            break;
        }

        // now send the thing.

        // now process the other side of the thing
        switch (i) {
        case 0:
        case 1:
        case 2: {
                AJ_AlwaysPrintf(("Case %d response\n", i));
                AJ_WSL_HTC_StateMachine(&AJ_WSL_HTC_Global.endpoints[0]);
                if (status != AJ_OK) {
                    break;
                }
                break;
            }

        default:
            break;
        }
    }

    AJ_DumpBytes(__FUNCTION__, fromTarget.fakeWireBuffer, sizeof(fromTarget.fakeWireBuffer));

}


/*
 * This test builds a opens and closes a socket in a simulated wire buffer
 */
static void run_wsl_open_and_close(const struct test_case* test)
{
    AJ_Status status = AJ_OK;
    AJ_WSL_SOCKNUM sockNum;

    AJ_AlwaysPrintf(("\n\n**************\nTEST:  %s\n\n", __FUNCTION__));


    // 2 = AF_INET
    // 1 = SOCK_STREAM
    sockNum = AJ_WSL_NET_socket_open(2, 1, 0);
    AJ_WSL_NET_socket_close(sockNum);

}

/*
 * This test sends small amount of data using a simulated wire buffer
 */
static void run_wsl_send_small(const struct test_case* test)
{
    AJ_Status status = AJ_OK;
    AJ_WSL_SOCKNUM sockNum;
    char* hello = "Hello, World.\0";

    AJ_AlwaysPrintf(("\n\n**************\nTEST:  %s\n\n", __FUNCTION__));

    // 2 = AF_INET
    // 1 = SOCK_STREAM
    sockNum = AJ_WSL_NET_socket_open(2, 1, 0);

    // fake a connect
    AJ_WSL_SOCKET_CONTEXT[sockNum].domain = 2; // AF_INET
    AJ_WSL_SOCKET_CONTEXT[sockNum].type = 1; //sock_stream
    AJ_WSL_SOCKET_CONTEXT[sockNum].name.sin_addr.s_addr = 0xAABBCCDD;
    AJ_WSL_SOCKET_CONTEXT[sockNum].name.sin_port = 0x9988;


    AJ_WSL_NET_send(sockNum, hello, 13, UINT32_MAX);
    AJ_WSL_NET_socket_close(sockNum);

}

/**
 * \brief Run SPI driver unit test.
 */
int AJ_Main(void)
{
    AJ_WSL_ModuleInit();

    toTarget.fakeWireCurr = toTarget.fakeWireBuffer;
    fromTarget.fakeWireCurr = fromTarget.fakeWireBuffer;
    ResetFakeWireBuffers();

    run_wsl_htc_parsing(NULL);
    run_wsl_simulate_ping(NULL);


    run_wsl_simulate_state_machine(NULL);

    run_wsl_simulate_socket_open_recv(NULL);
    run_wsl_simulate_ping_recv(NULL);
    run_wsl_simulate_socket_close_recv(NULL);
    run_wsl_simulate_ping_recv(NULL); // this one would return failure because the socket has been closed

    run_wsl_open_and_close(NULL);
    run_wsl_send_small(NULL);
}


#ifdef __cplusplus
}
#endif

