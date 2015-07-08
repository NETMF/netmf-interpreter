/******************************************************************************
 *
 *
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

#ifdef AJ_ARDP

#include <gtest/gtest.h>

#include "aj_debug.h"
#include "alljoyn.h"
#include "aj_ardp.h"

#define ARDP_FLAG_SYN  0x01    /**< Control flag. Request to open a connection.  Must be separate segment. */
#define ARDP_FLAG_ACK  0x02    /**< Control flag. Acknowledge a segment. May accompany message */
#define ARDP_FLAG_EACK 0x04    /**< Control flag. Non-cumulative (extended) acknowledgement */
#define ARDP_FLAG_RST  0x08    /**< Control flag. Reset this connection. Must be separate segment. */
#define ARDP_FLAG_NUL  0x10    /**< Control flag. Null (zero-length) segment.  Must have zero data length */
#define ARDP_FLAG_VER  0x40    /**< Control flag. Bits 6-7 of flags byte.  Current version is (1) */
#define ARDP_FLAG_SDM  0x0001  /**< Sequenced delivery mode option. Indicates in-order sequence delivery is in force. */

#define ARDP_SYN_HEADER_SIZE 28

#define FLAGS_OFFSET   0
#define HLEN_OFFSET    1
#define SRC_OFFSET     2
#define DST_OFFSET     4
#define DLEN_OFFSET    6
#define SEQ_OFFSET     8
#define ACK_OFFSET    12
#define TTL_OFFSET    16
#define LCS_OFFSET    20
#define ACKNXT_OFFSET 24
#define SOM_OFFSET    28
#define FCNT_OFFSET   32
#define RSRV_OFFSET   34

#define SEGMAX_OFFSET   16
#define SEGBMAX_OFFSET  18
#define DACKT_OFFSET    20
#define OPTIONS_OFFSET  24
#define SYN_RSRV_OFFSET 26

static const char TestHelloData[] = "HELLO";

static enum {
    Connecting = 0,
    Connected = 1,
    Disconnecting = 2
} State;

static uint16_t local_port;

static uint8_t ConnectedResponse[] = {
    ARDP_FLAG_SYN | ARDP_FLAG_ACK | ARDP_FLAG_VER,   // flags
    0x0E,       // HLEN
    0x00, 0x00, // local port
    0xFF, 0xFF, // remote port
    0x00, 0x06,    // DLEN
    0x00, 0x00, 0x00, 0x00, // SEQ
    0x00, 0x00, 0x00, 0x00, // ACK
    0x00, 0x5D, // segmax == 93
    0x11, 0x58, // segbmax == 4400
    0x00, 0x00, 0x00, 0x64, // 100 == ack timeout
    0x00, 0x01,  // options
    0x00, 0x00,  // padding?
    'H', 'E', 'L', 'L', 'O', '\0'
};


static AJ_Status AJ_ARDP_UDP_Send(void* context, uint8_t* txbuf, size_t len, size_t* sent)
{
    switch (State) {
    case Connecting:
        EXPECT_EQ(*(txbuf + 0), ARDP_FLAG_SYN | ARDP_FLAG_VER);
        EXPECT_EQ(*(txbuf + HLEN_OFFSET), ARDP_SYN_HEADER_SIZE >> 1);
        local_port = ntohs(*((uint16_t*) (txbuf + SRC_OFFSET)));
        EXPECT_EQ(*((uint16_t*) (txbuf + DST_OFFSET)), 0);
        EXPECT_EQ(*((uint16_t*) (txbuf + DLEN_OFFSET)), htons(sizeof(TestHelloData)));

        // we will need to acknowledge the response
        memcpy(ConnectedResponse + ACK_OFFSET, txbuf + SEQ_OFFSET, sizeof(uint32_t));

        EXPECT_EQ(*((uint32_t*) (txbuf + ACK_OFFSET)), 0);
        EXPECT_EQ(*((uint16_t*) (txbuf + SEGMAX_OFFSET)), htons(UDP_SEGMAX));
        EXPECT_EQ(*((uint16_t*) (txbuf + SEGBMAX_OFFSET)), htons(UDP_SEGBMAX));
        EXPECT_EQ(*((uint32_t*) (txbuf + DACKT_OFFSET)), htonl(UDP_DELAYED_ACK_TIMEOUT));
        EXPECT_EQ(*((uint16_t*) (txbuf + OPTIONS_OFFSET)), htons(ARDP_FLAG_SIMPLE_MODE | ARDP_FLAG_SDM));
        EXPECT_EQ(*((uint16_t*) (txbuf + SYN_RSRV_OFFSET)), 0);

        EXPECT_EQ(len, sizeof(TestHelloData) + ARDP_SYN_HEADER_SIZE);

        EXPECT_TRUE(0 == memcmp(txbuf + ARDP_SYN_HEADER_SIZE, TestHelloData, sizeof(TestHelloData)));

        State = Connected;

        break;

    case Connected:
        // now check the ack going back out
        EXPECT_EQ(*(txbuf + 0), ARDP_FLAG_ACK | ARDP_FLAG_VER);
        EXPECT_EQ(*(txbuf + HLEN_OFFSET), 18);
        EXPECT_EQ(*((uint16_t*) (txbuf + SRC_OFFSET)), htons(local_port));
        EXPECT_EQ(*((uint16_t*) (txbuf + DST_OFFSET)), 0);
        break;

    case Disconnecting:
        EXPECT_EQ(*(txbuf + FLAGS_OFFSET), (ARDP_FLAG_RST | ARDP_FLAG_ACK | ARDP_FLAG_VER));
        EXPECT_EQ(*(txbuf + HLEN_OFFSET), (uint8_t)(ARDP_HEADER_SIZE >> 1));
        EXPECT_EQ(*((uint16_t*) (txbuf + DLEN_OFFSET)), 0);
        EXPECT_EQ(*((uint32_t*) (txbuf + TTL_OFFSET)), ARDP_TTL_INFINITE);
        EXPECT_EQ(*((uint32_t*) (txbuf + SOM_OFFSET)), 0);
        EXPECT_EQ(*((uint16_t*) (txbuf + FCNT_OFFSET)), 0);
        EXPECT_EQ(*((uint16_t*) (txbuf + RSRV_OFFSET)), 0);


        break;
    }

    *sent = len;
    return AJ_OK;
}

static AJ_Status AJ_ARDP_UDP_Recv(void* context, uint8_t** data, uint32_t* recved, uint32_t timeout)
{
    *data = ConnectedResponse;
    *recved = sizeof(ConnectedResponse);
    return AJ_OK;
}



class ARDPTest : public testing::Test {
  public:

    virtual void SetUp() {
        AJ_ARDP_InitFunctions(&AJ_ARDP_UDP_Recv, &AJ_ARDP_UDP_Send);
    }
    virtual void TearDown() {

    }
};

void SendBackConnected()
{
    uint8_t rxData[1024];
    AJ_Status status;
    AJ_IOBuffer buf;

    *((uint16_t*) (ConnectedResponse + DST_OFFSET)) = htons(local_port);

    AJ_IOBufInit(&buf, rxData, sizeof(rxData), AJ_IO_BUF_RX, NULL);
    status = AJ_ARDP_Recv(&buf, sizeof(rxData), 0);
    EXPECT_EQ(AJ_OK, status) << "  Actual Status: " << AJ_StatusText(status);

    EXPECT_TRUE(0 == memcmp(TestHelloData, buf.readPtr, sizeof(TestHelloData)));
}

TEST_F(ARDPTest, TestSynAck)
{
    State = Connecting;
    AJ_Status status = AJ_ARDP_Connect((uint8_t*) TestHelloData, sizeof(TestHelloData), NULL, NULL);
    EXPECT_EQ(AJ_OK, status) << "  Actual Status: " << AJ_StatusText(status);

    // simulate an accepted connect request
    SendBackConnected();

    // now maybe do some send and receive

    State = Disconnecting;
    AJ_ARDP_Disconnect(TRUE);
}


#endif
