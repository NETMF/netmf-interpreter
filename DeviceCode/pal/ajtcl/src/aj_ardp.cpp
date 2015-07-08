/**
 * @file ArdpProtocol is an implementation of the Reliable Datagram Protocol
 * (RDP) adapted to AllJoyn.
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

#ifdef AJ_ARDP

#ifdef __cplusplus
extern "C" {
#endif

#define AJ_MODULE ARDP

#include "aj_ardp.h"
#include "aj_msg.h"
#include "aj_net.h"
#include "aj_crypto.h"
#include "aj_debug.h"
#include "aj_util.h"

#ifndef NDEBUG
uint8_t dbgARDP = 0;
#endif

#define UDP_MTU 1472
#define ARDP_SYN_HEADER_SIZE 28

/* Maximum data payload in one ARDP segment */
#define ARDP_MAX_DLEN (UDP_SEGBMAX - (UDP_HEADER_SIZE + ARDP_HEADER_SIZE))

#define ARDP_FLAG_SYN  0x01    /**< Control flag. Request to open a connection.  Must be separate segment. */
#define ARDP_FLAG_ACK  0x02    /**< Control flag. Acknowledge a segment. May accompany message */
#define ARDP_FLAG_EACK 0x04    /**< Control flag. Non-cumulative (extended) acknowledgement */
#define ARDP_FLAG_RST  0x08    /**< Control flag. Reset this connection. Must be separate segment. */
#define ARDP_FLAG_NUL  0x10    /**< Control flag. Null (zero-length) segment.  Must have zero data length */
#define ARDP_FLAG_VER  0x40    /**< Control flag. Bits 6-7 of flags byte.  Current version is (1) */
#define ARDP_FLAG_SDM  0x0001  /**< Sequenced delivery mode option. Indicates in-order sequence delivery is in force. */

#define ARDP_VERSION_BITS 0xC0   /* Bits 6-7 of FLAGS byte in ARDP segment header*/

/* Reserved TTL value to indicate that data associated with the message has expired */
#define ARDP_TTL_EXPIRED    0xffffffff
/*  Maximum allowed TTL value */
#define ARDP_TTL_MAX        (ARDP_TTL_EXPIRED - 1)

/* Maximum number of accumulated unacknowledged RCV segments */
#define  ARDP_MAX_ACK_PENDING 2

/* Flag indicating that the buffer is occupied */
#define ARDP_BUFFER_IN_USE 0x01
/* Flag indicating that the buffer is delivered to the upper layer */
#define ARDP_BUFFER_DELIVERED 0x02

/* Minimum Roundtrip Time */
#define ARDP_MIN_RTO 100

/* Maximum Roundtrip Time */
#define ARDP_MAX_RTO 64000

/* Minimum Delayed ACK Timeout */
#define ARDP_MIN_DELAYED_ACK_TIMEOUT 10

#define MIN(a, b) ((a) < (b) ? (a) : (b))
#define MAX(a, b) ((a) > (b) ? (a) : (b))
#define ABS(a) ((a) >= 0 ? (a) : -(a))

/* Marshal/Unmarshal ARDP header offsets */
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

/* Additional Marshal/Unmarshal ARDP SYN header offsets */
#define SEGMAX_OFFSET   16
#define SEGBMAX_OFFSET  18
#define DACKT_OFFSET    20
#define OPTIONS_OFFSET  24
#define SYN_RSRV_OFFSET 26

/* Structure encapsulating timer to to handle timeouts */
struct ArdpTimer {
    AJ_Time tStart;
    uint32_t delta;
    uint32_t retry;
};

typedef struct AJ_ARDP_RCV_BUFFER {
    uint32_t seq;          /* Sequence number */
    uint8_t data[UDP_SEGBMAX - ARDP_HEADER_SIZE];   /* Data payload */
    struct AJ_ARDP_RCV_BUFFER* next; /* Pointer to the next buffer */
    uint32_t som;          /* Sequence number of first segment in fragmented message */
    uint16_t dataLen;      /* Data payload size */
    uint16_t fcnt;         /* Number of segments comprising fragmented message, doubles as "in use" indicator */
} ArdpRBuf;

/* Structure encapsulating the information about segments on SEND side */
typedef struct AJ_ARDP_SEND_BUF {
    uint32_t data[UDP_SEGBMAX >> 2];
    AJ_Time tStart;
    struct ArdpTimer timer;    /* Data retransmit timer */
    struct AJ_ARDP_SEND_BUF* next;
    uint16_t dataLen;
    uint8_t retransmits;
    uint8_t inFlight;
} ArdpSBuf;

/**
 * Structure encapsulating the send-related quantities. The stuff we manage on
 * the local side of the connection and which we may send to THEM.
 */
struct ArdpSnd {
    uint32_t NXT;         /* The sequence number of the next segment that is to be sent */
    uint32_t UNA;         /* The sequence number of the oldest unacknowledged segment */
    uint32_t ISS;         /* The initial send sequence number. The number that was sent in the SYN segment */
    uint32_t LCS;         /* Sequence number of last consumed segment (we get this form them) */
    uint32_t DACKT;       /* Delayed ACK timeout from the other side */
    uint32_t SEGMAX;      /* The maximum number of unacknowledged segments that can be sent */
    ArdpSBuf buf[UDP_SEGMAX]; /* Array holding in-flight sent  buffers. */
    uint32_t msgTTL;      /* TTL associated with the most recent outbound message */
    uint32_t msgLenTotal; /* Length of the most recent outbound message */
    uint32_t msgLenSent;  /* Cumulative length of all the segments that has been sent so far for the most recent outbound message */
    uint32_t msgSOM;      /* Sequence number of the first segment in the most recent outbound message */
    uint8_t pending;      /* Number of unacknowledged sent buffers */
    uint8_t newMsg;       /* Indicates that the next call to ARDP_Send() will carry new AllJoyn message */
};

/**
 * Structure encapsulating the receive-related quantities. The stuff managed on
 * the remote/foreign side, copies of which we may get from THEM.
 */
struct ArdpRcv {
    uint32_t CUR;             /* The sequence number of the last segment received correctly and in sequence */
    uint32_t LCS;             /* LCS - Last "in-order" consumed segment.*/
    ArdpRBuf buf[UDP_SEGMAX]; /* Array holding received buffers not consumed by the app */
    uint8_t pending;          /* Number of unacknowledged received buffers */
};

/**
 * Information encapsulating the various interesting tidbits we get from the
 * other side when we receive a datagram.  Some of the names are chosen so that they
 * are similar to the quantities found in RFC-908 when used.
 */
struct ArdpSeg {
    uint32_t SEQ;     /* The sequence number in the segment currently being processed. */
    uint32_t ACK;     /* The acknowledgement number in the segment currently being processed. */
    uint32_t LCS;     /* The last "in-sequence" consumed segment */
    uint32_t ACKNXT;  /* The first valid SND segment, TTL accounting */
    uint32_t SOM;     /* Start sequence number for fragmented message */
    uint32_t TTL;     /* Time-to-live */
    uint16_t FCNT;    /* Number of fragments comprising a message */
    uint16_t DLEN;    /* The length of the data that came in with the current segment. */
    uint8_t FLG;      /* The flags in the header of the segment currently being processed. */
    uint8_t HLEN;     /* The header length */
};

/**
 * The states through which our main state machine transitions.
 */
typedef enum {
    CLOSED = 0,    /* No connection exists and no connection record available */
    SYN_SENT,      /* Entered after processing an active open request.  SYN is sent and ARDP waits here for ACK of open request. */
    OPEN,          /* Successful echange of state information happened,.  Data may be sent and received. */
    CLOSE_WAIT     /* Disconnecting. Waiting for the outbound data to be ACKed */
} ArdpState;

/**
 * The format of a SYN segment on the wire.
 */
struct AJ_ARDP_Syn_Hdr {
    uint8_t flags;      /* See Control flag definitions above */
    uint8_t hlen;       /* Length of the header in units of two octets (number of uint16_t) */
    uint16_t src;       /* Used to distinguish between multiple connections on the local side. */
    uint16_t dst;       /* Used to distinguish between multiple connections on the foreign side. */
    uint16_t dlen;      /* The length of the data in the current segment.  Does not include the header size. */
    uint32_t seq;       /* The sequence number of the current segment. */
    uint32_t ack;       /* The number of the segment that the sender of this segment last received correctly and in sequence. */
    uint16_t segmax;    /* The maximum number of outstanding segments the other side can send without acknowledgement. */
    uint16_t segbmax;   /* The maximum segment size we are willing to receive.  (the RBUF.MAX specified by the user calling open). */
    uint32_t dackt;     /* Receiver's delayed ACK timeout. Used in TTL estimate prior to sending a message. */
    uint16_t options;   /* Options for the connection.  Always Sequenced Delivery Mode (SDM). */
};

/**
 * A connection record describing each "connection."  This acts as a containter
 * to hold all of the interesting information about a reliable link between
 * hosts.
 */
struct ArdpConnection {
    AJ_NetSocket* netSock;  /* I/O Buffer management socket */
    ArdpState state;        /* The current sate of the connection */
    struct ArdpSnd snd;     /* Send-side related state information */
    struct ArdpRcv rcv;     /* Receive-side related state information */
    struct ArdpTimer connectTimer; /* Connect/Disconnect timer */
    struct ArdpTimer probeTimer;   /* Probe (link timeout) timer */
    struct ArdpTimer ackTimer;     /* Delayed ACK timer */
    struct ArdpTimer persistTimer; /* Persist (frozen window) timer */
    void* context;          /* Platform independent UDP socket equivalent */
    uint16_t local;         /* ARDP local port for this connection */
    uint16_t foreign;       /* ARDP foreign port for this connection */
    uint32_t rttMean;       /* Smoothed RTT value */
    uint32_t rttMeanVar;    /* RTT variance */
    uint32_t backoff;       /* Backoff factor accounting for retransmits on connection, resets to 1 when receive "good ack" */
    uint32_t rttMeanUnit;   /* Smoothed RTT value per UDP MTU */
    uint8_t rttInit;        /* Flag indicating that the first RTT was measured and SRTT calculation applies */
};

static struct ArdpConnection* conn = NULL;

/*
 * Important!!! All our numbers are within window size, the calculation below will hold.
 * If necessary, can add check that delta between the numbers does not exceed half-range.
 */
#define SEQ32_LT(a, b) ((int32_t)((uint32_t)(a) - (uint32_t)(b)) < 0)
#define SEQ32_LET(a, b) (((int32_t)((uint32_t)(a) - (uint32_t)(b)) < 0) || ((a) == (b)))

/**
 * Inside window calculation.
 * Returns TRUE if p is in range [beg, beg+sz)
 * This function properly accounts for possible wrap-around in [beg, beg+sz) region.
 */
#define IN_RANGE(tp, beg, sz, p) ((((tp) ((beg) + (sz)) > (beg)) && ((p) >= (beg)) && ((p) < (tp) ((beg) + (sz)))) || \
                                  (((tp) ((beg) + (sz)) < (beg)) && !(((p) < (beg)) && (p) >= (tp) ((beg) + (sz)))))


/* Housekeeping for data (inside ARDP rBuf) that have been received and potentially not consumed */
static struct {
    uint8_t* readBuf;     /* Pointer to current unconsumed data */
    uint16_t dataLen;     /* How many bytes are left to read */
    ArdpRBuf* rxContext;  /* Pointer to ARDP rBuf from where the data are being currently consumed */
} UDP_Recv_State;


static ReceiveFunction recvFunction;
static SendFunction sendFunction;

/**************
 * End of definitions
 */

void AJ_ARDP_InitFunctions(ReceiveFunction recv, SendFunction send)
{
    recvFunction = recv;
    sendFunction = send;
}

static AJ_Status InitConnection()
{
    uint32_t rand;
    uint32_t i;

    conn = (struct ArdpConnection*) AJ_Malloc(sizeof(struct ArdpConnection));
    if (conn == NULL) {
        return AJ_ERR_RESOURCES;
    }
    memset(conn, 0, sizeof(struct ArdpConnection));

    AJ_RandBytes((uint8_t*) &rand, sizeof(uint32_t));
    conn->local = (rand % 65534) + 1;  /* Allocate an "ephemeral" source port */

    /* Initialize the sender side of the connection */
    AJ_RandBytes((uint8_t*) &conn->snd.ISS, sizeof(conn->snd.ISS));
    conn->snd.NXT = conn->snd.ISS + 1; /* The sequence number of the next segment to be sent over this connection */
    conn->snd.UNA = conn->snd.ISS;     /* The oldest unacknowledged segment is the ISS */
    conn->snd.LCS = conn->snd.ISS;     /* The most recently consumed segment (we keep this in sync with the other side) */

    for (i = 0; i < UDP_SEGMAX; i++) {
        conn->snd.buf[i].next = &conn->snd.buf[(i + 1) % UDP_SEGMAX];
    }

    for (i = 0; i < UDP_SEGMAX; i++) {
        conn->rcv.buf[i].next = &conn->rcv.buf[(i + 1) % UDP_SEGMAX];
    }

    conn->rttInit = FALSE;
    conn->rttMean = UDP_INITIAL_DATA_TIMEOUT;
    conn->rttMeanUnit = UDP_INITIAL_DATA_TIMEOUT;
    conn->rttMeanVar = 0;

    conn->backoff = 0;
    return AJ_OK;
}

static void MarshalHeader(uint32_t* buf32, uint8_t flags, uint16_t dlen, uint32_t ttl, uint32_t som, uint16_t fcnt)
{
    uint8_t* txbuf = (uint8_t*) (buf32);

    *(txbuf + FLAGS_OFFSET) = flags;
    *(txbuf + HLEN_OFFSET) = (uint8_t)(ARDP_HEADER_SIZE >> 1);
    *((uint16_t*) (txbuf + SRC_OFFSET)) = htons(conn->local);
    *((uint16_t*) (txbuf + DST_OFFSET)) = htons(conn->foreign);
    *((uint16_t*) (txbuf + DLEN_OFFSET)) = htons(dlen);
    *((uint32_t*) (txbuf + SEQ_OFFSET)) = htonl(conn->snd.NXT);
    *((uint32_t*) (txbuf + ACK_OFFSET)) = htonl(conn->rcv.CUR);
    *((uint32_t*) (txbuf + TTL_OFFSET)) = htonl(ttl);
    *((uint32_t*) (txbuf + LCS_OFFSET)) = htonl(conn->rcv.LCS);
    *((uint32_t*) (txbuf + ACKNXT_OFFSET)) = htonl(conn->snd.UNA);
    *((uint32_t*) (txbuf + SOM_OFFSET)) = htonl(som);
    *((uint16_t*) (txbuf + FCNT_OFFSET)) = htons(fcnt);
    *((uint16_t*) (txbuf + RSRV_OFFSET)) = 0;
}

static AJ_Status SendHeader(uint8_t flags)
{
    uint32_t buf32[ARDP_HEADER_SIZE >> 2];
    size_t sent;

    AJ_InfoPrintf(("SendHeader(flags=0x%02x, seq=%u, ack=%u)\n", flags, conn->snd.NXT, conn->rcv.CUR));

    /* Marshal the header structure into a byte buffer */
    MarshalHeader(buf32, flags, 0, ARDP_TTL_INFINITE, 0, 0);

    AJ_InfoPrintf(("SendHeader: cancel ackTimer\n"));
    conn->ackTimer.retry = 0;
    conn->rcv.pending = 0;

    return (*sendFunction)(conn->context, (uint8_t*) &buf32, ARDP_HEADER_SIZE, &sent);
}

static AJ_Status SendSyn(uint16_t dataLen)
{
    size_t sent;
    uint8_t* txbuf = (uint8_t*) &(conn->snd.buf[0].data[0]);

    /* Marshal SYN header */
    *(txbuf + FLAGS_OFFSET) = ARDP_FLAG_SYN | ARDP_FLAG_VER;
    *(txbuf + HLEN_OFFSET)  = ARDP_SYN_HEADER_SIZE >> 1;
    *((uint16_t*) (txbuf + SRC_OFFSET)) = htons(conn->local);
    *((uint16_t*) (txbuf + DST_OFFSET)) = 0; /* optional, can be removed to reduce code size */
    *((uint16_t*) (txbuf + DLEN_OFFSET)) = htons(dataLen);
    *((uint32_t*) (txbuf + SEQ_OFFSET)) = htonl(conn->snd.ISS);
    *((uint32_t*) (txbuf + ACK_OFFSET)) = 0; /* optional , can be removed to reduce code size*/
    *((uint16_t*) (txbuf + SEGMAX_OFFSET)) = htons(UDP_SEGMAX);
    *((uint16_t*) (txbuf + SEGBMAX_OFFSET)) = htons(UDP_SEGBMAX);
    *((uint32_t*) (txbuf + DACKT_OFFSET)) = htonl(UDP_DELAYED_ACK_TIMEOUT);
    *((uint16_t*) (txbuf + OPTIONS_OFFSET)) = htons(ARDP_FLAG_SIMPLE_MODE | ARDP_FLAG_SDM);
    *((uint16_t*) (txbuf + SYN_RSRV_OFFSET)) = 0;

    return (*sendFunction)(conn->context, (uint8_t*) &conn->snd.buf[0].data[0], ARDP_SYN_HEADER_SIZE + dataLen, &sent);
}

static void InitTimer(struct ArdpTimer* timer, uint32_t delta, uint8_t retry)
{
    AJ_InitTimer(&timer->tStart);
    timer->delta = delta;
    timer->retry = retry;
}

static AJ_Status ConnectTimerHandler()
{
    AJ_Status status;

    AJ_InfoPrintf(("ConnectTimerHandler: retries left %d\n", conn->connectTimer.retry));

    if (conn->connectTimer.retry > 1) {
        size_t sent;
        uint16_t len = conn->snd.buf[0].dataLen + ARDP_SYN_HEADER_SIZE;
        AJ_InfoPrintf(("ConnectTimerHandler: send %d bytes\n", len));
        status = (*sendFunction)(conn->context, (uint8_t*) conn->snd.buf[0].data, len, &sent);
        if (status == AJ_ERR_WOULD_BLOCK) {
            status = AJ_OK;
        }
        conn->connectTimer.retry--;
    } else {
        status = AJ_ERR_TIMEOUT;
    }

    if (status != AJ_OK) {
        conn->state = CLOSED;
        conn->connectTimer.retry = 0;
        AJ_ErrPrintf(("ConnectTimerHandler(): %s\n", AJ_StatusText(status)));
        printf("ConnectTimerHandler(): %s\n", AJ_StatusText(status));
        AJ_Free(conn);
        conn = NULL;
        return AJ_ERR_CONNECT;
    } else {
        return AJ_OK;
    }
}

static uint32_t GetDataTimeout()
{
    uint32_t timeout = UDP_TOTAL_DATA_RETRY_TIMEOUT;

    if (conn->rttInit) {
        timeout = MAX(timeout, (UDP_SEGMAX * UDP_SEGBMAX * (conn->rttMean >> 1)) / UDP_MTU);
    }
    return timeout;
}

static uint32_t GetRTO()
{
    /* RTO = (rttMean + (4 * rttMeanVar)) << backoff */
    uint32_t ms = (MAX((uint32_t)ARDP_MIN_RTO, conn->rttMean + (4 * conn->rttMeanVar))) << conn->backoff;
    AJ_InfoPrintf(("GetRTO(): rto=%u RTO = %u)\n", ms, MIN(ms, (uint32_t)ARDP_MAX_RTO)));

    return MIN(MAX(ms, conn->snd.DACKT), (uint32_t)ARDP_MAX_RTO);
}

static AJ_Status DataTimerHandler(ArdpSBuf* sBuf)
{
    AJ_Status status;
    struct ArdpTimer* timer = &sBuf->timer;
    uint32_t msElapsed = AJ_GetElapsedTime(&sBuf->tStart, FALSE);
    uint32_t timeout = GetDataTimeout();

#ifndef NDEBUG
    uint32_t seq = ntohl(*(uint32_t*)((uint8_t*)sBuf->data + SEQ_OFFSET));
#endif

    sBuf->retransmits++;

    do {
        if ((msElapsed >= timeout) && (timer->retry > UDP_MIN_DATA_RETRIES)) {
            AJ_ErrPrintf(("DataTimerHandler(): hit timeout for %u\n", seq));
            status = AJ_ERR_TIMEOUT;
        } else {
            size_t sent;
            uint8_t* txbuf = (uint8_t*) sBuf->data;
            uint16_t len = sBuf->dataLen + ARDP_HEADER_SIZE;

            /* Currently, we do not check TTL for in-flight SND packets */

            *((uint32_t*) (txbuf + ACK_OFFSET)) = htonl(conn->rcv.CUR);
            *((uint32_t*) (txbuf + LCS_OFFSET)) = htonl(conn->rcv.LCS);
            *((uint32_t*) (txbuf + ACKNXT_OFFSET)) = htonl(conn->snd.UNA);

            AJ_InfoPrintf(("DataTimerHandler: send %d bytes (seq %u, ack %u)\n", len, seq, conn->rcv.CUR));
            status =  (*sendFunction)(conn->context, (uint8_t*) sBuf->data, len, &sent);
            AJ_InitTimer(&sBuf->timer.tStart);

            if (status == AJ_OK) {
                conn->backoff = MAX(conn->backoff, timer->retry);
                if (conn->rttInit) {
                    timer->delta = GetRTO();
                } else {
                    timer->delta = UDP_INITIAL_DATA_TIMEOUT;
                }
                timer->retry++;
                AJ_InfoPrintf(("DataTimerHandler: cancel ackTimer\n"));
                conn->ackTimer.retry = 0;
                conn->rcv.pending = 0;
            } else {
                AJ_ErrPrintf(("DataTimerHandler():Write to Socket went bad"));
            }
        }
        sBuf = sBuf->next;
    } while (status == AJ_OK && (sBuf->timer.retry != 0)); /* Here "retry" check equates checking for "in flight" */

    return status;
}

static AJ_Status CheckDataTimers()
{
    uint32_t index;

    /* Check data retransmit timer */
    index = conn->snd.UNA % UDP_SEGMAX;
    if (conn->snd.buf[index].timer.retry != 0 &&
        AJ_GetElapsedTime(&conn->snd.buf[index].timer.tStart, TRUE) >= conn->snd.buf[index].timer.delta) {
        AJ_InfoPrintf(("CheckDataTimers: Fire data timer\n"));
        return DataTimerHandler(&conn->snd.buf[index]);
    }
    return AJ_OK;
}

static AJ_Status CheckTimers()
{
    AJ_Status status = AJ_OK;
    uint32_t delta;

    /*
     * Check connection timer. This timer is alive only when the connection is being established.
     * No other timers should be active on the connection.
     */
    if (conn->connectTimer.retry != 0) {
        if (AJ_GetElapsedTime(&conn->connectTimer.tStart, TRUE) >= conn->connectTimer.delta) {
            AJ_InfoPrintf(("CheckTimers: Fire connection timer\n"));
            return ConnectTimerHandler();
        }
        return AJ_OK;
    }

    /* Check probe timer, it's always turned on */
    delta = AJ_GetElapsedTime(&conn->probeTimer.tStart, TRUE);
    if (delta >= conn->probeTimer.delta) {
        if (conn->probeTimer.retry == 0) {
            AJ_ErrPrintf(("CheckTimers: link timeout\n"));
            return AJ_ERR_ARDP_PROBE_TIMEOUT;
        }
        AJ_InfoPrintf(("CheckTimers: Fire probe timer\n"));
        status = SendHeader(ARDP_FLAG_ACK | ARDP_FLAG_VER | ARDP_FLAG_NUL);
        AJ_InitTimer(&conn->probeTimer.tStart);
        conn->probeTimer.retry--;
    }

    status = CheckDataTimers();

    /* Check delayed ACK timer */
    delta = AJ_GetElapsedTime(&conn->ackTimer.tStart, TRUE);
    if ((conn->ackTimer.retry != 0) && ((delta >= conn->ackTimer.delta) || (conn->rcv.pending >= ARDP_MAX_ACK_PENDING))) {
        AJ_InfoPrintf(("CheckTimers: Fire ACK timer (elapsed %u vs %u)\n", delta, conn->ackTimer.delta));
        status = SendHeader(ARDP_FLAG_ACK | ARDP_FLAG_VER);
    }

    return status;
}

static AJ_Status UnmarshalSynSegment(uint8_t* buf, struct ArdpSeg* seg)
{
    uint16_t segmax;
    uint16_t segbmax;
    conn->foreign = ntohs(*((uint16_t*)(buf + SRC_OFFSET))); /* The source ARDP port */
    conn->snd.DACKT = ntohl(*((uint32_t*)(buf + DACKT_OFFSET))); /* Delayed ACK timeout from the other side.  */

    segmax = ntohs(*((uint16_t*)(buf + SEGMAX_OFFSET)));     /* Max number of unacknowledged packets other side can buffer */
    segbmax = ntohs(*((uint16_t*)(buf + SEGBMAX_OFFSET)));   /* Max size segment the other side can handle */

    if ((segmax < UDP_SEGMAX) || (segbmax < UDP_SEGBMAX)) {
        AJ_WarnPrintf(("UnmarshalSynSegment: unacceptable segmax=%d, segbmax=%d\n", segmax, segbmax));
        return AJ_ERR_RANGE;
    }

    conn->snd.SEGMAX = segmax;
    AJ_InfoPrintf(("UnmarshalSynSegment: segmax=%d, segbmax=%d\n", segmax, segbmax));
    conn->rcv.CUR = seg->SEQ;
    conn->rcv.LCS = seg->SEQ;
    return AJ_OK;
}

static AJ_Status RecvValidateSegment(uint8_t* rxbuf, uint16_t len, struct ArdpSeg* seg)
{
    uint16_t hdrSz;
    AJ_InfoPrintf(("Receive: rxbuf=%p, len=%u)\n", rxbuf, len));

    seg->FLG = rxbuf[FLAGS_OFFSET];        /* The flags of the current segment */
    seg->HLEN = rxbuf[HLEN_OFFSET];      /* The header len */

    if (seg->FLG & ARDP_FLAG_RST) {
        /* This is a disconnect from the remote, no checks are needed */
        AJ_WarnPrintf(("Receive: Remote disconnect RST\n"));
        AJ_Free(conn);
        conn = NULL;
        return AJ_ERR_ARDP_REMOTE_CONNECTION_RESET;
    }

    seg->DLEN = ntohs(*((uint16_t*)(rxbuf + DLEN_OFFSET))); /* The data payload size */

    hdrSz = (seg->FLG & ARDP_FLAG_SYN) ? ARDP_SYN_HEADER_SIZE : ARDP_HEADER_SIZE;

    /* Perform length validation checks */
    if (((seg->HLEN * 2) < hdrSz) || (len < hdrSz) || (seg->DLEN + (seg->HLEN * 2)) != len) {
        AJ_ErrPrintf(("Receive: length check failed len = %u, seg->hlen = %u, seg->dlen = %u\n",
                      len, (seg->HLEN * 2), seg->DLEN));
        return AJ_ERR_INVALID;
    }

    seg->SEQ = ntohl(*((uint32_t*)(rxbuf + SEQ_OFFSET))); /* The send sequence of the current segment */
    seg->ACK = ntohl(*((uint32_t*)(rxbuf + ACK_OFFSET))); /* The cumulative acknowledgement number to our sends */

    if (seg->FLG & ARDP_FLAG_SYN) {
        return AJ_OK;
    }

    seg->LCS = ntohl(*((uint32_t*)(rxbuf + LCS_OFFSET))); /* The last consumed segment on receiver side (them) */
    seg->ACKNXT = ntohl(*((uint32_t*)(rxbuf + ACKNXT_OFFSET))); /* The first valid segment sender wants to be acknowledged */

    AJ_InfoPrintf(("Receive() seq = %u, ack = %u, lcs = %u, acknxt = %u\n",
                   seg->SEQ, seg->ACK, seg->LCS, seg->ACKNXT));

    seg->TTL = ntohl(*((uint32_t*)(rxbuf + TTL_OFFSET)));       /* TTL associated with this segment */
    seg->SOM = ntohl(*((uint32_t*)(rxbuf + SOM_OFFSET)));       /* Sequence number of the first fragment in message */
    seg->FCNT = ntohs(*((uint16_t*)(rxbuf + FCNT_OFFSET)));     /* Number of segments comprising fragmented message */

    /* Perform sequence validation checks */
    if (SEQ32_LT(conn->snd.NXT, seg->ACK)) {
        AJ_ErrPrintf(("Receive: ack %u ahead of SND>NXT %u\n", seg->ACK, conn->snd.NXT));
        return AJ_ERR_INVALID;
    }

    if (SEQ32_LT(seg->ACK, seg->LCS)) {
        AJ_ErrPrintf(("Receive: lcs %u and ack %u out of order\n", seg->LCS, seg->ACK));
        return AJ_ERR_INVALID;
    }

    /*
     * SEQ and ACKNXT must fall within receive window. In case of segment with no payload,
     * allow one extra.
     */
    if (((seg->SEQ - seg->ACKNXT) > UDP_SEGMAX) || (SEQ32_LT(seg->SEQ, seg->ACKNXT)) ||
        ((seg->DLEN != 0) && ((seg->SEQ - seg->ACKNXT) == UDP_SEGMAX))) {
        AJ_ErrPrintf(("Receive: incorrect sequence numbers seg->seq = %u, seg->acknxt = %u\n",
                      seg->SEQ, seg->ACKNXT));
        return AJ_ERR_INVALID;
    }

    /* Additional checks for invalid payload values */
    if (seg->DLEN != 0) {
        if ((seg->FCNT == 0) || ((seg->SEQ - seg->SOM) >= seg->FCNT)) {
            AJ_ErrPrintf(("Receive: incorrect data segment seq = %u, som = %u,  fcnt = %u\n",
                          seg->SEQ, seg->SOM, seg->FCNT));
            return AJ_ERR_INVALID;
        }
    }

    return AJ_OK;
}

/*
 *    error = measuredRTT - meanRTT
 *    new meanRTT = 7/8 * meanRTT + 1/8 * error
 *    if (measuredRTT >= (meanRTT - meanVAR))
 *        new meanVar = 3/4 * meanVar + 1/4 * |error|
 *    else
 *        new meanVar = 31/32 * meanVar + 1/32 * |error|
 *
 *    Since ARDP segments can have varying length, maintain additional
 *    mean RTT calculation of UDP MTU unit that will be used in message TTL estimate.
 */
static void AdjustRTT(ArdpSBuf* sBuf)
{
    uint16_t units = (sBuf->dataLen + ARDP_HEADER_SIZE + UDP_MTU - 1) / UDP_MTU;
    uint32_t rtt = AJ_GetElapsedTime(&sBuf->timer.tStart, TRUE);
    uint32_t rttUnit = rtt / units;
    int32_t err;

    if (!conn->rttInit) {
        conn->rttMean = rtt;
        conn->rttMeanVar = rtt >> 1;
        conn->rttInit = TRUE;
    }

    err = rtt - conn->rttMean;

    AJ_InfoPrintf(("AdjustRtt: mean = %u, var =%u, rtt = %u, error = %d\n",
                   conn->rttMean, conn->rttMeanVar, rtt, err));
    conn->rttMean = (7 * conn->rttMean + rtt) >> 3;

    if ((rtt + conn->rttMeanVar) >= conn->rttMean) {
        conn->rttMeanVar = (conn->rttMeanVar * 3 + ABS(err)) >> 2;
    } else {
        conn->rttMeanVar = (conn->rttMeanVar * 31 + ABS(err)) >> 5;
    }

    rttUnit = (7 * conn->rttMeanUnit + rttUnit) >> 3;

    conn->backoff = 0;

    AJ_InfoPrintf(("AdjustRtt: New mean = %u, var =%u\n", conn->rttMean, conn->rttMeanVar));
}

static void UpdateSndSegments(uint32_t ack)
{
    uint16_t index = ack % UDP_SEGMAX;
    ArdpSBuf* sBuf = &conn->snd.buf[index];
    uint32_t i;

    /* Nothing to clean up */
    if (conn->snd.pending == 0) {
        return;
    }

    /*
     * Count only "good" roundrips to ajust RTT values.
     */
    if ((sBuf->retransmits == 0) && (sBuf->timer.retry != 0)) {
        AdjustRTT(sBuf);
    }

    sBuf = &conn->snd.buf[0];

    /* Cycle through all the buffers */
    for (i = 0; i < UDP_SEGMAX; i++) {
        uint32_t seq = ntohl(*(uint32_t*)((uint8_t*)(sBuf->data) + SEQ_OFFSET));

        /* If the remote acknowledged the segment, stop retransmit attempts. */
        AJ_InfoPrintf(("UpdateSndSegments(): cancel retransmit for %u\n", seq));

        if (SEQ32_LET(seq, ack) && (sBuf->inFlight != 0)) {
            sBuf->timer.retry = 0;
            sBuf->dataLen = 0;
            sBuf->inFlight = 0;
            sBuf->retransmits = 0;
            conn->snd.pending--;
        }

        if (conn->snd.pending == 0) {
            break;
        }
        sBuf = sBuf->next;
    }
}

static void AddRcvBuffer(struct ArdpSeg* seg, uint8_t* rxBuf, uint16_t dataOffset)
{
    uint32_t index = seg->SEQ % UDP_SEGMAX;

    AJ_InfoPrintf(("AddRcvBuffer: seq=%u\n", seg->SEQ));
    AJ_ASSERT(conn->rcv.buf[index].fcnt == 0);

    conn->rcv.buf[index].seq = seg->SEQ;
    conn->rcv.buf[index].som = seg->SOM;
    conn->rcv.buf[index].fcnt = seg->FCNT;
    conn->rcv.buf[index].dataLen = seg->DLEN;
    memcpy(conn->rcv.buf[index].data, rxBuf + dataOffset, seg->DLEN);

    if (UDP_Recv_State.rxContext == NULL) {
        UDP_Recv_State.readBuf = conn->rcv.buf[index].data;
        UDP_Recv_State.dataLen = seg->DLEN;
        UDP_Recv_State.rxContext = (void*) &conn->rcv.buf[index];
    }
}
static AJ_Status ArdpMachine(struct ArdpSeg* seg, uint8_t* rxBuf, uint16_t len)
{
    AJ_Status status = AJ_OK;

    AJ_InfoPrintf(("ArdpMachine(seg=%p, buf=%p, len=%d)\n", seg, rxBuf, len));

    switch (conn->state) {
    case SYN_SENT:
        {
            AJ_InfoPrintf(("ArdpMachine(): conn->state = SYN_SENT\n"));

            if (seg->FLG & ARDP_FLAG_SYN) {
                AJ_InfoPrintf(("ArdpMachine(): SYN_SENT: SYN received\n"));

                status = UnmarshalSynSegment(rxBuf, seg);

                if (status == AJ_OK) {
                    if ((seg->FLG  & ARDP_VERSION_BITS) != ARDP_FLAG_VER) {
                        AJ_WarnPrintf(("ArdpMachine(): SYN_SENT: Unsupported protocol version 0x%x\n",
                                       seg->FLG & ARDP_VERSION_BITS));
                        status = AJ_ERR_ARDP_VERSION_NOT_SUPPORTED;
                    } else if (!(seg->FLG & ARDP_FLAG_ACK) || (seg->ACK != conn->snd.ISS)) {
                        AJ_WarnPrintf(("ArdpMachine(): SYN_SENT: does not ACK ISS\n"));
                        status = AJ_ERR_INVALID;
                    } else {
                        AJ_InfoPrintf(("ArdpMachine(): SYN_SENT: SYN | ACK received. state -> OPEN\n"));

                        conn->snd.UNA = seg->ACK + 1;
                        conn->state = OPEN;
                        conn->snd.buf[0].timer.retry = 0;
                        conn->snd.buf[0].dataLen = 0;
                        conn->snd.buf[0].inFlight = 0;

                        /* Initialize and kick off link timeout timer */
                        InitTimer(&conn->probeTimer, UDP_LINK_TIMEOUT / UDP_KEEPALIVE_RETRIES, UDP_KEEPALIVE_RETRIES);

                        /*
                         * <SEQ=snd.NXT><ACK=RCV.CUR><ACK>
                         */
                        status = SendHeader(ARDP_FLAG_ACK | ARDP_FLAG_VER);

                    }
                }
            }

            if (status == AJ_OK) {
                AddRcvBuffer(seg, rxBuf, ARDP_SYN_HEADER_SIZE);
            }

            /* Stop connect retry timer */
            conn->connectTimer.retry = 0;

            break;
        }

    case CLOSE_WAIT:
    case OPEN:
        {
            AJ_InfoPrintf(("ArdpMachine(): conn->state = OPEN\n"));

            if (seg->FLG & ARDP_FLAG_SYN) {
                /* Ignore */
                return AJ_OK;
            }

            if (seg->FLG & ARDP_FLAG_ACK) {
                AJ_InfoPrintf(("ArdpMachine(): OPEN: Got ACK %u LCS %u\n", seg->ACK, seg->LCS));

                if (IN_RANGE(uint32_t, conn->snd.UNA, ((conn->snd.NXT - conn->snd.UNA) + 1), seg->ACK) == TRUE) {
                    conn->snd.UNA = seg->ACK + 1;
                    UpdateSndSegments(seg->ACK);
                }

                conn->snd.LCS = seg->LCS;

                if (conn->state == CLOSE_WAIT) {
                    if (conn->snd.pending != 0) {
                        return AJ_ERR_ARDP_DISCONNECTING;
                    } else {
                        return AJ_ERR_ARDP_DISCONNECTED;
                    }
                }
            }

            if (SEQ32_LT(conn->rcv.CUR + 1, seg->ACKNXT)) {
                AJ_InfoPrintf(("ArdpMachine(): OPEN: FlushExpiredRcvMessages: seq = %u, expected %u got %u\n",
                               seg->SEQ, conn->rcv.CUR + 1, seg->ACKNXT));
                status = AJ_ERR_ARDP_RECV_EXPIRED;
            }

            /* If we got NUL segment, send ACK without delay */
            if (seg->FLG & ARDP_FLAG_NUL) {
                SendHeader(ARDP_FLAG_ACK | ARDP_FLAG_VER);
            } else if (seg->DLEN) {
                if (conn->ackTimer.retry == 0) {
                    InitTimer(&conn->ackTimer, UDP_DELAYED_ACK_TIMEOUT, 1);
                }
                conn->rcv.pending++;

                /* Update with new data */
                if (SEQ32_LET((conn->rcv.CUR + 1), seg->SEQ)) {
                    AddRcvBuffer(seg, rxBuf, ARDP_HEADER_SIZE);
                    conn->rcv.CUR = seg->SEQ;
                    AJ_InfoPrintf(("ArdpMachine(): OPEN: received data with seq %u, som %u, fcnt %u\n",
                                   seg->SEQ, seg->SOM, seg->FCNT));
                } else {
                    AJ_InfoPrintf(("ArdpMachine(): OPEN: duplicate data with seq %u (cur=%u)\n", seg->SEQ, conn->rcv.CUR));
                }
            }

            InitTimer(&conn->probeTimer, UDP_LINK_TIMEOUT / UDP_KEEPALIVE_RETRIES, UDP_KEEPALIVE_RETRIES);

            break;
        }

    default:
        status = AJ_ERR_DISALLOWED;
        AJ_ASSERT(0 && "ArdpMachine(): unexpected conn->state %d");
        break;
    }

    AJ_InfoPrintf(("ArdpMachine(): %s(0x%x)\n", AJ_StatusText(status), status));
    return status;
}

static AJ_Status ARDP_Recv(uint8_t* rxBuf, uint16_t len)
{
    AJ_Status status = AJ_OK;
    struct ArdpSeg seg;

    memset(&seg, 0, sizeof(struct ArdpSeg));

    status = RecvValidateSegment(rxBuf, len, &seg);
    if (status == AJ_OK) {
        status = ArdpMachine(&seg, rxBuf, len);
    }

    if (status != AJ_OK) {
        AJ_ErrPrintf(("ARDP_Recv(): returned %s (0x%x)\n", AJ_StatusText(status), status));
    }
    return status;
}

static void RecvReady(void* rxContext)
{
    ArdpRBuf* rBuf = (ArdpRBuf*) rxContext;

    AJ_InfoPrintf(("RecvReady: buf=%p, seq=%u\n", rBuf, rBuf->seq));

    if (rBuf->fcnt != 0) {
        AJ_ASSERT((conn->rcv.LCS + 1) == rBuf->seq);
    } else {
        /* This is the very first call into this function. Connection is being established */
        AJ_ASSERT(conn->rcv.LCS == rBuf->seq);
    }

    rBuf->fcnt = 0;
    rBuf->dataLen = 0;
    conn->rcv.LCS = rBuf->seq;

    if (conn->ackTimer.retry == 0) {
        InitTimer(&conn->ackTimer, 0, 1);
    }
}

AJ_Status AJ_ARDP_StartMsgSend(uint32_t ttl)
{
    if (conn == NULL) {
        return AJ_ERR_DISALLOWED;
    }

    AJ_InfoPrintf(("ARDP_StartMsgSend: ttl=%d\n", ttl));

    AJ_ASSERT(conn->snd.newMsg == FALSE);
    conn->snd.newMsg = TRUE;
    conn->snd.msgTTL = ttl;
    return AJ_OK;
}

/*
 *       Send is a synchronous send. The data being sent are buffered at the protocol
 *       level.
 *       Returns error code:
 *         AJ_OK - all is good
 *         AJ_ERR_ARDP_TTL_EXPIRED - Discard the message that is currently being marshalled.
 *         AJ_ERR_ARDP_BACKPRESSURE - Send window does not allow immediate transmission.
 *         AJ_ERR_DISALLOWED - Connection does not exist (efffectively connection record is NULL)
 *
 */
static AJ_Status ARDP_Send(uint8_t* txBuf, uint16_t len)
{
    uint16_t pending;
    ArdpSBuf* sBuf;
    uint32_t ttl;
    uint16_t fcnt;
    uint16_t offset;
    AJ_Status status;

    AJ_InfoPrintf(("ARDP_Send: buf=%p, len=%d ((nxt %u, lcs %u))\n", txBuf, len, conn->snd.NXT, conn->snd.LCS));

    if ((conn == NULL) || ((conn->state != OPEN))) {
        return AJ_ERR_DISALLOWED;
    }

    status = CheckDataTimers();
    if (status != AJ_OK) {
        return status;
    }

    pending = (conn->snd.NXT - conn->snd.LCS) - 1;
    sBuf = &(conn->snd.buf[conn->snd.NXT % UDP_SEGMAX]);

    AJ_ASSERT(conn->snd.pending <= UDP_SEGMAX);
    if (conn->snd.pending == UDP_SEGMAX) {
        AJ_InfoPrintf(("ARDP_Send: backpressure, all (%u) SND buffers are in flight\n", conn->snd.pending));
        return AJ_ERR_ARDP_BACKPRESSURE;
    }
    AJ_ASSERT(sBuf->inFlight == 0);

    ttl = conn->snd.msgTTL;
    offset = sBuf->dataLen;

    /* If this is the start of a new message, extract the total message length */
    if (conn->snd.newMsg == TRUE) {
        AJ_MsgHeader* hdr = (AJ_MsgHeader*) txBuf;
        conn->snd.msgLenTotal = sizeof(AJ_MsgHeader) + ((hdr->headerLen + 7) & 0xFFFFFFF8) + hdr->bodyLen;
        conn->snd.msgLenSent = 0;
        AJ_InfoPrintf(("ARDP_Send: new message len = %u\n", conn->snd.msgLenTotal));
        conn->snd.newMsg = FALSE;
        conn->snd.msgSOM = conn->snd.NXT;
    }

    /*
     * Check whether there is enough local buffer space to fit the data.
     * Also, check if the remote side can currently accept these data.
     */
    if (((len + offset) > (ARDP_MAX_DLEN * (UDP_SEGMAX - conn->snd.pending))) || ((len + offset) > (ARDP_MAX_DLEN * (conn->snd.SEGMAX - pending)))) {
        AJ_InfoPrintf(("ARDP_Send: backpressure, cannot send %u (%u + %u): local send pending %u, remote consume pending %u\n", len + offset, len, offset, conn->snd.pending, pending));
        return AJ_ERR_ARDP_BACKPRESSURE;
    }

    AJ_ASSERT(conn->snd.msgLenTotal > 0);

    if (conn->rttInit && (ttl != ARDP_TTL_INFINITE)) {
        uint32_t expireThreshold = (conn->rttMeanUnit * (conn->snd.msgLenTotal + UDP_MTU - 1) / UDP_MTU) >> 1;
        if ((ttl < (ARDP_TTL_MAX - conn->snd.DACKT)) && ((ttl + conn->snd.DACKT) <= expireThreshold)) {
            return AJ_ERR_ARDP_SEND_EXPIRED;
        }

        /* If we passed the above "expire" test only due to factoring in DACKT, do not adjust ttl */
        if (ttl > expireThreshold) {
            ttl = ttl - expireThreshold;
        }
    }

    fcnt = (conn->snd.msgLenTotal + ARDP_MAX_DLEN - 1) / ARDP_MAX_DLEN;

    AJ_ASSERT(fcnt > 0);

    do {
        uint16_t dataLen;
        size_t sent;
        uint32_t timeout;
        AJ_Status status;

        /* Check whether the current buffer is not in a process of being populated with fragmented data */
        dataLen = (len <= (ARDP_MAX_DLEN - offset)) ? offset + len : ARDP_MAX_DLEN;

        memcpy(((uint8_t*) sBuf->data) + offset + ARDP_HEADER_SIZE, txBuf, dataLen - offset);
        sBuf->dataLen = dataLen;
        AJ_ASSERT(sBuf->inFlight == 0);

        AJ_InfoPrintf(("ARDP_Send(): len %u, dataLen %u, offset %u\n", len, dataLen, offset));
        /*
         * Check if the segment is not filled to full capacity and is not the last segment. If so, we need
         * to wait for more data to pack in. Do not send, do not update counters.
         * Note: Soft check (instead of checking len == 0): a precaution for avoiding infinte loop in case we miscalculated.
         */
        if ((dataLen < ARDP_MAX_DLEN) && dataLen != (conn->snd.msgLenTotal - conn->snd.msgLenSent)) {
            AJ_InfoPrintf(("ARDP_Send(): queued %d bytes\n", dataLen));
            break;
        }

        sBuf->inFlight = 1;
        conn->snd.msgLenSent += dataLen;

        MarshalHeader(sBuf->data, ARDP_FLAG_ACK | ARDP_FLAG_VER, dataLen, ttl, conn->snd.msgSOM, fcnt);

        AJ_InfoPrintf(("ARDP_Send(): send %d bytes (seq %u, ack %u)\n", ARDP_HEADER_SIZE + dataLen, conn->snd.NXT, conn->rcv.CUR));
        status = (*sendFunction)(conn->context, (uint8_t*) sBuf->data, ARDP_HEADER_SIZE + dataLen, &sent);

        if (status != AJ_OK) {
            AJ_ErrPrintf(("ARDP_Send(): %s\n", AJ_StatusText(status)));
            return status;
        }

        AJ_InfoPrintf(("ArdpSend(): cancel ackTimer\n"));
        conn->ackTimer.retry = 0;
        conn->rcv.pending = 0;

        len -= (dataLen - offset);

        conn->snd.NXT++;
        conn->snd.pending++;

        if (conn->rttInit) {
            timeout = GetRTO();
        } else {
            timeout = UDP_INITIAL_DATA_TIMEOUT;
        }

        AJ_InitTimer(&sBuf->tStart);
        InitTimer(&sBuf->timer, timeout, 1);
        sBuf = sBuf->next;
        txBuf += (dataLen - offset);
        offset = 0;

    } while (len != 0);

    return AJ_OK;
}

AJ_Status AJ_ARDP_Connect(uint8_t* data, uint16_t dataLen, void* context, AJ_NetSocket* netSock)
{
    AJ_Status status;

    memset(&UDP_Recv_State, 0, sizeof(UDP_Recv_State));

    status = InitConnection();

    if (status != AJ_OK) {
        return status;
    }
    AJ_ASSERT(dataLen < (UDP_SEGBMAX - ARDP_SYN_HEADER_SIZE));
    memcpy(((uint8_t*) conn->snd.buf[0].data) + ARDP_SYN_HEADER_SIZE, data, dataLen);
    conn->snd.buf[0].dataLen = dataLen;
    conn->snd.buf[0].inFlight = 1;
    conn->context = context;
    conn->netSock = netSock;

    status = SendSyn(dataLen);

    if (status != AJ_OK) {
        AJ_Free(conn);
    } else {
        InitTimer(&conn->connectTimer, UDP_CONNECT_TIMEOUT, UDP_CONNECT_RETRIES);
        conn->state = SYN_SENT;
    }

    return status;
}

void AJ_ARDP_Disconnect(uint8_t forced)
{
    AJ_WarnPrintf(("ARDP Disconnect Request (local)\n"));
    if (conn == NULL) {
        return;
    }

    if ((forced == FALSE) && (conn->snd.pending != 0)) {
        AJ_InfoPrintf(("ARDP_Disconnect: wait for tx queue to drain\n"));
        conn->state = CLOSE_WAIT;
        /* Block here  to give data retransmits a chance to go through */
        AJ_ARDP_Recv(&conn->netSock->rx, 0, UDP_DISCONNECT_TIMEOUT);
    }

    AJ_WarnPrintf(("ARDP_Disconnect: Send RST\n"));
    SendHeader(ARDP_FLAG_RST | ARDP_FLAG_ACK | ARDP_FLAG_VER);

    AJ_Free(conn);
    conn = NULL;
}

AJ_Status AJ_ARDP_Send(AJ_IOBuffer* buf)
{
    size_t tx = AJ_IO_BUF_AVAIL(buf);
    AJ_Status status;

    AJ_InfoPrintf(("AJ_ARDP_Send(buf=0x%p)\n", buf));

    if (conn == NULL) {
        return AJ_ERR_DISALLOWED;
    }

    AJ_ASSERT(buf->direction == AJ_IO_BUF_TX);

    if (tx > 0) {
        status = ARDP_Send(buf->readPtr, tx);

        /* We essentially block until the backpressure is releived */
        if (status == AJ_ERR_ARDP_BACKPRESSURE) {
            do {
                AJ_InfoPrintf(("AJ_ARDP_Send: dealing with backpressure\n"));
                /*
                 * If we can't make room in the send window within a certain amount of time,
                 * assume that the connection has failed.
                 */
                status = AJ_ARDP_Recv(&conn->netSock->rx, 0, UDP_BACKPRESSURE_TIMEOUT);

                if (status != AJ_OK && status != AJ_ERR_TIMEOUT) {
                    /* Something has gone wrong */
                    AJ_ErrPrintf(("AJ_ARDP_Send: (*recvFunction) returns %s\n", AJ_StatusText(status)));
                    return AJ_ERR_WRITE;
                }

                status = ARDP_Send(buf->readPtr, tx);
                /* Loop while backpressure continues */
            } while (status == AJ_ERR_ARDP_BACKPRESSURE);
        } else if (status != AJ_OK) {
            /* Something other than backpressure */
            return AJ_ERR_WRITE;
        }

        if ((status == AJ_OK) || (status == AJ_ERR_ARDP_RECV_EXPIRED)) {
            buf->readPtr += tx;
        }
    }

    if (AJ_IO_BUF_AVAIL(buf) == 0) {
        AJ_IO_BUF_RESET(buf);
    }

    AJ_InfoPrintf(("AJ_ARDP_Send(): status=AJ_OK\n"));
    return status;
}

static void UpdateReadBuffer(AJ_IOBuffer* rxBuf, uint32_t len) {

    AJ_InfoPrintf(("UpdateRead: rxBuf %p, len %u\n", rxBuf, len));

    while ((UDP_Recv_State.rxContext != NULL) && len != 0) {
        ArdpRBuf* rBuf = UDP_Recv_State.rxContext;
        size_t rx = AJ_IO_BUF_SPACE(rxBuf);
        uint32_t consumed;

        /* How much buffer space is available */
        rx = min(rx, len);

        /* How much we can consume from the current rBuf */
        consumed = min(rx, UDP_Recv_State.dataLen);

        memcpy(rxBuf->writePtr, UDP_Recv_State.readBuf, consumed);

        /* Advance the write pointer */
        rxBuf->writePtr += consumed;
        len -= consumed;

        if (consumed == UDP_Recv_State.dataLen) {
            /*
             * We are done with the current rBuf. Release and potentially
             * move on to next rBuf.
             */
            AJ_InfoPrintf(("UpdateRead: ready\n"));
            RecvReady(rBuf);

            /* Advance to the next rBuf */
            if (rBuf->next->dataLen) {
                AJ_InfoPrintf(("UpdateRead: Start reading from next RCV\n"));
                UDP_Recv_State.readBuf = rBuf->next->data;
                UDP_Recv_State.dataLen = rBuf->next->dataLen;
                UDP_Recv_State.rxContext = rBuf->next;
            } else {
                AJ_InfoPrintf(("UpdateRead: Nothing in next RCV\n"));
                memset(&UDP_Recv_State, 0, sizeof(UDP_Recv_State));
            }
        } else {
            /* No more space to write data. Update the internal read state and return */
            UDP_Recv_State.readBuf += rx;
            UDP_Recv_State.dataLen -= rx;
            return;
        }
    }
}

AJ_Status AJ_ARDP_Recv(AJ_IOBuffer* rxBuf, uint32_t len, uint32_t timeout)
{
    AJ_Status status = AJ_ERR_TIMEOUT;
    AJ_Status localStatus;
    uint32_t timeout2 = min(timeout, UDP_MINIMUM_TIMEOUT);
    AJ_Time now, end;

    AJ_InfoPrintf(("AJ_ARDP_Recv(rxBuf=0x%p, len=%u, timeout=%u)\n", rxBuf, len, timeout));

    if (conn == NULL) {
        return AJ_ERR_READ;
    }

    AJ_InitTimer(&end);
    AJ_TimeAddOffset(&end, timeout);

    if ((len != 0) && (UDP_Recv_State.rxContext != NULL)) {
        timeout2 = 0;
    }

    do {
        uint32_t received = 0;
        uint8_t* buf = NULL;

        status = (*recvFunction)(rxBuf->context, &buf, &received, timeout2);

        switch (status) {
        case AJ_ERR_TIMEOUT:
            if ((len != 0) && (UDP_Recv_State.rxContext != NULL)) {
                status = AJ_OK;
                goto UPDATE_READ;
            }
            break;

        case AJ_OK:
            status = ARDP_Recv(buf, received);

            if ((status == AJ_OK) || (status == AJ_ERR_ARDP_RECV_EXPIRED)) {
                goto UPDATE_READ;
            } else if (status == AJ_ERR_ARDP_DISCONNECTING) {
                /* We are waiting for either TX queue to drain or timeout */
                break;
            } else if (status != AJ_ERR_ARDP_REMOTE_CONNECTION_RESET) {
                AJ_WarnPrintf(("AJ_ARDP_Recv: received bad data, disconnecting\n"));
                AJ_ARDP_Disconnect(TRUE);
            }
            status = AJ_ERR_READ;

        /* Fall through */

        case AJ_ERR_INTERRUPTED:
        case AJ_ERR_READ:
            return status;

        default:
            AJ_WarnPrintf(("AJ_ARDP_Recv: Invalid\n"));
            AJ_ASSERT(!"this shouldn't happen!");
            break;
        }

        AJ_InitTimer(&now);
    } while (AJ_CompareTime(now, end) < 0);

UPDATE_READ:

    if (status == AJ_ERR_ARDP_RECV_EXPIRED) {
        /*
         * Currently we do not do anything with special expired messages.
         * Just deliver the accumulated data and inform the upper layer
         * (via status) that the the previous message may be incomplete.
         */
        AJ_WarnPrintf(("AJ_ARDP_Recv: Expired message\n"));
    }

    if ((len != 0) && (UDP_Recv_State.rxContext != NULL)) {
        UpdateReadBuffer(rxBuf, len);
    }

    localStatus = CheckTimers();

    if (localStatus != AJ_OK) {
        status = localStatus;
    }

    AJ_InfoPrintf(("AJ_ARDP_Recv exit with %s\n", AJ_StatusText(status)));

    return status;
}

#ifdef __cplusplus
}
#endif

#endif // AJ_ARDP
