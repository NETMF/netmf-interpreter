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

/**
 * Per-module definition of the current module for debug logging.  Must be defined
 * prior to first inclusion of aj_debug.h
 */
#define AJ_MODULE DISCO

#include "aj_target.h"
#include "aj_status.h"
#include "aj_util.h"
#include "aj_net.h"
#include "aj_disco.h"
#include "aj_debug.h"
#include "aj_config.h"
#include "aj_connect.h"
#include "aj_guid.h"
#include "aj_creds.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgDISCO = 0;
#endif

typedef struct _NSHeader {
    uint8_t version;
    uint8_t qCount;
    uint8_t aCount;
    uint8_t ttl;
    uint8_t flags;
    uint8_t nameCount;
} NSHeader;

/*
 * Message V1 flag definitions
 */
#define U6_FLAG 0x01
#define R6_FLAG 0x02
#define U4_FLAG 0x04
#define R4_FLAG 0x08
#define C_FLAG  0x10
#define G_FLAG  0x20

#define MSG_TYPE(flags) ((flags) & 0xC0)

#define WHO_HAS_MSG   0x80
#define IS_AT_MSG     0x40

#define MSG_VERSION(flags)  ((flags) & 0x0F)

#define MSG_V0 0x00
#define MSG_V1 0x01
#define NSV_V1 0x10

static AJ_Status ComposeWhoHas(AJ_IOBuffer* txBuf, const char* prefix)
{
    size_t preLen = strlen(prefix);
    NSHeader* hdr = (NSHeader*)txBuf->writePtr;
    uint8_t* p = txBuf->writePtr + 6;
    size_t outLen = (6 + preLen + 2);

    AJ_InfoPrintf(("ComposeWhoHas(txbuf=0x%p, prefix=\"%s\")\n", txBuf, prefix));

    if (outLen > AJ_IO_BUF_SPACE(txBuf)) {
        AJ_ErrPrintf(("ComposeWhoHas(): AJ_ERR_RESOURCES\n"));
        return AJ_ERR_RESOURCES;
    }
    hdr->version = MSG_V1 | NSV_V1;
    hdr->qCount = 1;
    hdr->aCount = 0;
    hdr->ttl = 0;
    hdr->flags = WHO_HAS_MSG;
    hdr->nameCount = 1;
    *p++ = (uint8_t)(preLen + 1);
    memcpy(p, prefix, preLen);
    /*
     * Tack wild-card onto the end of the name to indicate it's prefix
     */
    p[preLen] = '*';
    txBuf->writePtr += outLen;
    return AJ_OK;
}

static AJ_Status ParseIsAt(AJ_IOBuffer* rxBuf, const char* prefix, AJ_Service* service)
{
    AJ_Status status = AJ_ERR_NO_MATCH;
    size_t preLen = strlen(prefix);
    NSHeader* hdr = (NSHeader*)rxBuf->readPtr;
    uint32_t len = AJ_IO_BUF_AVAIL(rxBuf);
    uint8_t* p = rxBuf->readPtr + 4;
    uint8_t* eod = (uint8_t*)hdr + len;

    AJ_InfoPrintf(("ParseIsAt(rxbuf=0x%p, prefix=\"%s\", service=0x%p)\n", rxBuf, prefix, service));

    service->addrTypes = 0;

    /*
     * Silently ignore versions we don't know how to parse
     */
    if (MSG_VERSION(hdr->version) != MSG_V1) {
        return status;
    }
    /*
     * Questions come in first - we currently ignore them
     */
    while (hdr->qCount--) {
        uint8_t flags = *p++;
        uint8_t nameCount = *p++;
        /*
         * Questions must be WHO_HAS messages
         */
        if (MSG_TYPE(flags) != WHO_HAS_MSG) {
            AJ_InfoPrintf(("ParseIsAt(): AJ_ERR_INVALID\n"));
            return AJ_ERR_INVALID;
        }
        while (nameCount--) {
            uint8_t sz = *p++;
            p += sz;
            if (p > eod) {
                AJ_InfoPrintf(("ParseIsAt(): AJ_ERR_END_OF_DATA\n"));
                status = AJ_ERR_END_OF_DATA;
                goto Exit;
            }
        }
    }
    /*
     * Now the answers - this is what we are looking for
     */
    while (hdr->aCount--) {
        uint8_t flags = *p++;
        uint8_t nameCount = *p++;
        /*
         * Answers must be IS_AT messages
         */
        if (MSG_TYPE(flags) != IS_AT_MSG) {
            AJ_InfoPrintf(("ParseIsAt(): AJ_ERR_INVALID\n"));
            return AJ_ERR_INVALID;
        }
        /*
         * Get transport mask
         */
        service->transportMask = (p[0] << 8) | p[1];
        p += 2;
        /*
         * Decode addresses
         */
        if (flags & R4_FLAG) {
            memcpy(&service->ipv4, p, sizeof(service->ipv4));
            p += sizeof(service->ipv4);
            service->ipv4port = (p[0] << 8) | p[1];
            p += 2;
            service->addrTypes |= AJ_ADDR_TCP4;
        }
        if (flags & U4_FLAG) {
            memcpy(&service->ipv4Udp, p, sizeof(service->ipv4Udp));
            p += sizeof(service->ipv4Udp);
            service->ipv4portUdp = (p[0] << 8) | p[1];
            p += 2;
            service->addrTypes |= AJ_ADDR_UDP4;
        }
        if (flags & R6_FLAG) {
            memcpy(&service->ipv6, p, sizeof(service->ipv6));
            p += sizeof(service->ipv6);
            service->ipv6port = (p[0] << 8) | p[1];
            p += 2;
            service->addrTypes |= AJ_ADDR_TCP6;
        }
        if (flags & U6_FLAG) {
            memcpy(&service->ipv6Udp, p, sizeof(service->ipv6Udp));
            p += sizeof(service->ipv6Udp);
            service->ipv6portUdp = (p[0] << 8) | p[1];
            p += 2;
            service->addrTypes |= AJ_ADDR_UDP6;
        }
        /*
         * Skip guid if it's present
         */
        if (flags & G_FLAG) {
            uint8_t sz = *p++;
            len -= 1 + sz;
            p += sz;
        }
        if (p >= eod) {
            AJ_InfoPrintf(("ParseIsAt(): AJ_ERR_END_OF_DATA\n"));
            return AJ_ERR_END_OF_DATA;
        }
        /*
         * Iterate over the names
         */
        while (nameCount--) {
            uint8_t sz = *p++;
            {
                char sav = p[sz];
                p[sz] = 0;
                AJ_InfoPrintf(("ParseIsAt(): Found \"%s\" IP 0x%x\n", p, service->addrTypes));
                p[sz] = sav;
            }
            if ((preLen <= sz) && (memcmp(p, prefix, preLen) == 0)) {
                status = AJ_OK;
                goto Exit;
            }
            p += sz;
            if (p > eod) {
                status = AJ_ERR_END_OF_DATA;
                AJ_InfoPrintf(("ParseIsAt(): AJ_ERR_END_OF_DATA\n"));
                goto Exit;
            }
        }
    }

Exit:
    return status;
}

#define MDNS_QR 0x8000

typedef struct _MDNSHeader {
    uint16_t queryId;
    uint16_t qrType;
    uint16_t qdCount;
    uint16_t anCount;
    uint16_t m_nsCount;
    uint16_t arCount;
} MDNSHeader;

typedef struct _MDNSARData {
    char ipv4Addr[3 * 4 + 3 + 1];
} MDNSARData;

typedef struct _MDNSDomainName {
    char name[256];
} MDNSDomainName;

typedef struct _MDNSSrvRData {
    uint16_t priority;
    uint16_t weight;
    uint16_t port;
    MDNSDomainName target;
} MDNSSrvRData;

typedef struct _MDNSTextRData {
    char BusNodeName[256];
    char BusNodeTransport[256];
    char BusNodeProtocolVersion[8];
} MDNSTextRData;

typedef union _MDNSRData {
    MDNSARData aRData;
    MDNSSrvRData srvRData;
    MDNSTextRData textRData;
    MDNSDomainName ptrRData;
} MDNSRData;

typedef enum _RRType {
    A = 1,          //Host IPv4 Address
    NS = 2,         //Authoritative name server
    MD = 3,         //Mail destination
    MF = 4,         //Mail forwarder
    CNAME = 5,      //Canonical name for an alias
    SOA = 6,        //Marks the start zone of an authority
    MB = 7,         //Mailbox domain name
    MG = 8,         //Mail group member
    MR = 9,         //Mail rename domain
    RNULL = 10,     //Null RR
    WKS = 11,       //Well known service description
    PTR = 12,       //Domain name pointer
    HINFO = 13,     //Host information
    MINFO = 14,     //Mailbox info
    MX = 15,        //Mail Exchange
    TXT = 16,       //Text strings
    AAAA = 28,      //Host IPv6 Address
    SRV = 33,       //SRV record
    OPT = 41,       //OPT record
    NSEC = 47       //NSEC record
} RRType;

typedef enum _RRClass {
    INTERNET = 1,   //Internet
    CS = 2,         //CSNET class
    CH = 3,         //CHAOS class
    HS = 4          //Hesoid
} RRClass;

typedef struct _MDNSResourceRecord {
    MDNSDomainName rrDomainName;
    RRType rrType;
    RRClass rrClass;
    uint32_t rrTTL;
    MDNSRData rdata;
} MDNSResourceRecord;

static AJ_Status ComposeMDnsReq(AJ_IOBuffer* txBuf, const char* prefix, AJ_GUID* guid, uint16_t sidVal)
{
    uint16_t dataLength;
    int pktLen;

    static uint8_t hdr[] = {
        0x00, 0x00, // transId
        0x00, 0x00, // flags
        0x00, 0x02, // qCount
        0x00, 0x00, // aCount
        0x00, 0x00, // nsCount
        0x00, 0x02  // arCount
    };

    static uint8_t queries[] = {                               // Question 1:
        0x08, 0x5f, 0x61, 0x6c, 0x6c, 0x6a, 0x6f, 0x79, 0x6e,  // _alljoyn
        0x04, 0x5f, 0x74, 0x63, 0x70,                          // _tcp
        0x05, 0x6c, 0x6f, 0x63, 0x61, 0x6c,                    // local
        0x00,
        0x00, 0x0c,                                            // Type
        0x80, 0x01,                                            // Class
                                                               // Question 2:
        0x08, 0x5f, 0x61, 0x6c, 0x6c, 0x6a, 0x6f, 0x79, 0x6e,  // _alljoyn
        0x04, 0x5f, 0x75, 0x64, 0x70,                          // _udp
        0xc0, 0x1a,                                            // local (compressed)
        0x00, 0x0c,                                            // Type
        0x80, 0x01                                             // Class
    };

    /*
     * Additional record: search.<guid>.local
     */
    static uint8_t search[] = {
        0x06, 0x73, 0x65, 0x61, 0x72, 0x63, 0x68               // search
    };

    static uint8_t local[] = {
        0xc0, 0x1a,                                            // local (compressed)
        0x00, 0x10,                                            // Type (TXT)
        0x00, 0x01,                                            // Class
        0x00, 0x00, 0x00, 0x78                                 // TTL
    };

    static uint8_t txtvers[] = {
        0x09, 0x74, 0x78, 0x74, 0x76, 0x65, 0x72, 0x73, 0x3d, 0x30 // txtvers=0
    };

    static uint8_t nameone[] = {
        0x6e, 0x5f, 0x31, 0x3d                                 // n_1=
    };

    static uint8_t sendmatchonly[] = {                          // m=1
        0x03, 0x6d, 0x3d, 0x31
    };

    /*
     * Additional record: sender-info.<guid>.local
     */
    static uint8_t senderinfo[] = {
        0x0b, 0x73, 0x65, 0x6e, 0x64, 0x65, 0x72, 0x2d,        // sender-info
        0x69, 0x6e, 0x66, 0x6f,
        0xc0, 0x40,                                            // <guid>.local (compressed)
        0x00, 0x10,                                            // Type (TXT)
        0x00, 0x01,                                            // Class
        0x00, 0x00, 0x00, 0x78,                                // TTL
        0x00, 0x42,                                            // Data Length=66
        0x09, 0x74, 0x78, 0x74, 0x76, 0x65, 0x72, 0x73,        // txtvers=0
        0x3d, 0x30,
        0x07, 0x61, 0x6a, 0x70, 0x76, 0x3d, 0x31, 0x30,        // ajpv=10
        0x04, 0x70, 0x76, 0x3d, 0x32,                          // pv=2
        // These next three must be re-written by the net transmit layer
        0x09, 0x73, 0x69, 0x64, 0x3d, 0x35, 0x35, 0x35,        // sid=55555
        0x35, 0x35,
        0x0a, 0x69, 0x70, 0x76, 0x34, 0x3d, 0x35, 0x35,        // ipv4=55555
        0x35, 0x35, 0x35,
        0x15, 0x75, 0x70, 0x63, 0x76, 0x34, 0x3d, 0x35,        // upcv4=555.555.555.555
        0x35, 0x35, 0x2e, 0x35, 0x35, 0x35, 0x2e, 0x35,
        0x35, 0x35, 0x2e, 0x35, 0x35, 0x35
    };

    uint8_t* pkt = (uint8_t*)txBuf->writePtr;

    hdr[0] = (sidVal >> 8) & 0xFF;
    hdr[1] = sidVal & 0xFF;
    memcpy(pkt, hdr, sizeof(hdr));
    pkt += sizeof(hdr);

    memcpy(pkt, queries, sizeof(queries));
    pkt += sizeof(queries);

    /*
     * Append search TXT record with actual GUID and prefix
     */
    memcpy(pkt, search, sizeof(search));
    pkt += sizeof(search);

    *pkt++ = 32;
    AJ_GUID_ToString(guid, (char*) pkt, 33);
    pkt += 32;

    memcpy(pkt, local, sizeof(local));
    pkt += sizeof(local);

    dataLength = sizeof(txtvers) + 1 + sizeof(nameone) + strlen(prefix) + 1 + sizeof(sendmatchonly);
    *pkt++ = (uint8_t) (dataLength >> 8);
    *pkt++ = (uint8_t) (dataLength & 0xFF);

    memcpy(pkt, txtvers, sizeof(txtvers));
    pkt += sizeof(txtvers);

    if ((sizeof(nameone) + strlen(prefix) + 1) > 255) {
        AJ_ErrPrintf(("ComposeMDnsReq(): prefix too long: %d\n", strlen(prefix)));
        return AJ_ERR_INVALID;
    }
    *pkt++ = sizeof(nameone) + strlen(prefix) + 1;
    memcpy(pkt, nameone, sizeof(nameone));
    pkt += sizeof(nameone);
    memcpy(pkt, prefix, strlen(prefix));
    pkt += strlen(prefix);
    *pkt++ = '*';
    memcpy(pkt, sendmatchonly, sizeof(sendmatchonly));
    pkt += sizeof(sendmatchonly);

    /*
     * Append sender-info TXT record static fields
     */
    memcpy(pkt, senderinfo, sizeof(senderinfo));
    pkt += sizeof(senderinfo);

    pktLen = pkt - txBuf->writePtr;
    txBuf->writePtr += pktLen;
    return AJ_OK;
}

static size_t ParseMDNSHeader(uint8_t const* buffer, uint32_t bufsize, MDNSHeader* hdr)
{
    size_t size = 0;
    if (bufsize < 12) {
        AJ_ErrPrintf(("ParseMDNSHeader(): Insufficient bufsize %d\n", bufsize));
        return 0;
    }

    //
    // The first two octets are ID
    //
    hdr->queryId = (buffer[0] << 8) | buffer[1];
    size += 2;

    //
    // The next two octects are the flags
    //
    hdr->qrType = (buffer[2] << 8) | buffer[3];
    size += 2;

    //
    // The next two octets are QDCOUNT
    //
    hdr->qdCount = (buffer[4] << 8) | buffer[5];
    size += 2;

    //
    // The next two octets are ANCOUNT
    //
    hdr->anCount = (buffer[6] << 8) | buffer[7];
    size += 2;

    //
    // The next two octets are NSCOUNT
    //
    hdr->m_nsCount = (buffer[8] << 8) | buffer[9];
    size += 2;

    //
    // The next two octets are ARCOUNT
    //
    hdr->arCount = (buffer[10] << 8) | buffer[11];
    size += 2;

    bufsize -= 12;

    return size;

}

static size_t ParseMDNSARData(uint8_t const* buffer, uint32_t bufsize, MDNSARData* a)
{
    memset(a->ipv4Addr, 0, 16);
    if (bufsize < 6) {
        return 0;
    }
    if (buffer[0] != 0 || buffer[1] != 4) {
        return 0;

    }
    memset(a->ipv4Addr, 0, 4);
    memcpy(a->ipv4Addr, (buffer + 2), 4);
    return 6;
}

static size_t ParseMDNSDefaultRData(uint8_t const* buffer, uint32_t bufsize)
{
    // Default data is skipped as it's deemed to be a
    // resource record type that is not currently used
    // or reserved for future use.
    uint16_t rdlen = 0;

    if (bufsize < 2) {
        return 0;
    }
    rdlen = (buffer[0] << 8 | buffer[1]);
    bufsize -= 2;
    if (bufsize < rdlen) {
        return 0;
    }
    return rdlen + 2;
}

static size_t ParseMDNSDomainName(uint8_t const* buffer, uint32_t bufsize, MDNSDomainName* domainName, const uint8_t* payload, uint32_t paylen)
{
    // The expression for domain names can be a series of labels,
    // a pointer to a byte offset of previously encountered series
    // of labels, or a series of labels ending with a pointer to
    // a previously encountered series of labels. Labels have a
    // size limit of 63 octets and domain names have a size limit
    // of 255 octets. We add an extra byte for the trailing null.
    // A pointer is denoted by an octet where the first two bits
    // are ones. Since the size of labels are at most 63 octets,
    // the size of labels begin with two zero bits and can be
    // easily distinguished from pointers.
    uint32_t offset = 0;
    size_t size = 0;
    uint8_t const* pos = buffer;
    int32_t len = bufsize;
    memset(domainName->name, 0, 256);
    while (len) {
        if (((*pos & 0xc0) >> 6) == 3 && len > 1) {
            uint32_t pointer = ((pos[0] << 8 | pos[1]) & 0x3FFF);
            if (pointer >= paylen) {
                AJ_ErrPrintf(("ParseMDNSDomainName(): Insufficient bufsize\n"));
                return 0;
            }
            if (payload[pointer] & 0xc0) {
                AJ_ErrPrintf(("ParseMDNSDomainName(): Invalid compression\n"));
                return 0;
            }
            if (pos >= buffer) {
                size += 2;
            }
            pos = payload + pointer;
            len = (paylen - pointer);
        } else {
            uint8_t label_len = *pos;
            pos++;
            len--;
            if (pos >= buffer) {
                size++;
            }
            if (label_len > len) {
                AJ_ErrPrintf(("ParseMDNSDomainName(): Insufficient bufsize\n"));
                return 0;
            }
            if (domainName->name[0]) {
                memcpy((domainName->name + offset), ".", 1);
                offset++;
            }
            if ((label_len > 0) && (offset + label_len) < 256) {
                memcpy((domainName->name + offset), (pos), label_len);
                len -= label_len;
                pos += label_len;
                if (pos >= buffer) {
                    size += label_len;
                }
                offset += label_len;
            } else {
                break;
            }
        }
    }
    return size;
}

static size_t ParseMDNSTextRData(uint8_t const* buffer, uint32_t bufsize, MDNSTextRData* textRData, uint8_t const* payload, uint32_t paylen, const char* prefix)
{
    //
    // If there's not enough data in the buffer to even get the string size out
    // then bail.
    //
    uint16_t rdlen = 0;
    uint8_t const* p = NULL;
    uint8_t* pos = NULL;
    uint8_t sz = 0;
    size_t size = 0;
    memset(textRData->BusNodeName, 0, 256);
    memset(textRData->BusNodeTransport, 0, 256);
    memset(textRData->BusNodeProtocolVersion, 0, 8);

    if (bufsize < 2) {
        AJ_ErrPrintf(("ParseMDNSTextRData(): Insufficient bufsize %d", bufsize));
        return 0;
    }
    rdlen = buffer[0] << 8 | buffer[1];

    bufsize -= 2;
    size = 2 + rdlen;

    //
    // If there's not enough data in the buffer then bail.
    //
    if (bufsize < rdlen) {
        AJ_ErrPrintf(("ParseMDNSTextRData(): Insufficient bufsize %d", bufsize));
        return 0;
    }
    p = &buffer[2];
    while (rdlen > 0 && bufsize > 0) {
        sz = *p++;
        bufsize--;
        if (!sz || !bufsize ||  bufsize < sz) {
            AJ_ErrPrintf(("ParseMDNSTextRData(): Insufficient bufsize %d", bufsize));
            return 0;
        }
        // For now we are only interested in three specific
        // key-value pairs: the bus node transport, bus node
        // name and the bus node protocol version.
        pos = (uint8_t*) memchr((char const*)p, '=', sz);
        if (pos) {
            uint8_t indx = pos - p;
            uint8_t valsz = (sz - indx - 1);
            if (!memcmp(p, "ajpv", 4)) {
                if (valsz < 8) {
                    memcpy(textRData->BusNodeProtocolVersion, pos + 1, valsz);
                }
            }
            if (!memcmp(p, "t_", 2) && !textRData->BusNodeName[0]) {
                memcpy(textRData->BusNodeTransport, pos + 1, valsz);
            }
            if (!memcmp(p, "n_", 2) &&
                (prefix != NULL) && (valsz >= strlen(prefix)) && !memcmp(pos + 1, prefix, strlen(prefix))) {
                memcpy(textRData->BusNodeName, pos + 1, valsz);
            }
        }
        p += sz;
        rdlen -= sz + 1;
        bufsize -= sz;
    }

    if (rdlen != 0) {
        AJ_ErrPrintf(("ParseMDNSTextRData(): Mismatched RDLength"));
        return 0;
    }
    return size;
}

static size_t ParseMDNSSrvRData(uint8_t const* buffer, uint32_t bufsize, MDNSSrvRData* srvRData, uint8_t* payload, uint32_t paylen)
{
    uint16_t length = 0;
    size_t size = 0;
    size_t ret = 0;
    uint8_t const* p = NULL;
    if (bufsize < 2) {
        AJ_ErrPrintf(("ParseMDNSSrvRData(): Insufficient bufsize %d", bufsize));
        return 0;
    }

    length = buffer[0] << 8 | buffer[1];
    bufsize -= 2;

    if (bufsize < length || length < 6) {
        AJ_ErrPrintf(("ParseMDNSSrvRData(): Insufficient bufsize %d or invalid length %d", bufsize, length));
        return 0;
    }
    srvRData->priority = buffer[2] << 8 | buffer[3];
    bufsize -= 2;

    srvRData->weight = buffer[4] << 8 | buffer[5];
    bufsize -= 2;

    srvRData->port = buffer[6] << 8 | buffer[7];
    bufsize -= 2;

    size = 8;
    p = &buffer[size];
    ret = ParseMDNSDomainName(p, bufsize, &(srvRData->target), payload, paylen);
    if (ret) {
        return (size + ret);
    } else {
        return 0;
    }
}

static size_t ParseMDNSResourceRecord(uint8_t const* buffer, uint32_t bufsize, MDNSResourceRecord* record, uint8_t* payload, uint32_t paylen, const char* prefix)
{
    size_t processed = 0;
    uint16_t ptr_len = 0;
    uint8_t const* p = NULL;
    size_t size = ParseMDNSDomainName(buffer, bufsize, &(record->rrDomainName), payload, paylen);

    if (size == 0 || bufsize < 8) {
        AJ_ErrPrintf(("ParseMDNSResourceRecord(): Error occured while deserializing domain name\n"));
        return 0;
    }

    if (size > bufsize || ((size + 8) > bufsize)) {
        AJ_ErrPrintf(("ParseMDNSResourceRecord(): Insufficient buffer size\n"));
        return 0;
    }
    record->rrType = (RRType)((buffer[size] << 8) | buffer[size + 1]);

    //Next two octets are CLASS
    record->rrClass = (RRClass)((buffer[size + 2] << 8) | buffer[size + 3]);

    //Next four octets are TTL
    record->rrTTL = (buffer[size + 4] << 24) | (buffer[size + 5] << 16) | (buffer[size + 6] << 8) | buffer[size + 7];
    bufsize -= (size + 8);
    size += 8;
    p = &buffer[size];

    switch (record->rrType) {
    case A:
        AJ_InfoPrintf(("ParseMDNSResourceRecord(): Found an A record\n"));
        processed = ParseMDNSARData(p, bufsize, &(record->rdata.aRData));
        break;

    case PTR:
        AJ_InfoPrintf(("ParseMDNSResourceRecord(): Found a PTR record\n"));
        ptr_len = (*p << 8 | *(p + 1));
        p += 2;
        bufsize -= 2;
        if (ptr_len > bufsize) {
            return 0;
        }
        processed = ParseMDNSDomainName(p, bufsize, &(record->rdata.ptrRData), payload, paylen);
        if (processed) {
            processed += 2;
        }
        break;

    case SRV:
        AJ_InfoPrintf(("ParseMDNSResourceRecord(): Found a SRV record\n"));
        processed = ParseMDNSSrvRData(p, bufsize, &(record->rdata.srvRData), payload, paylen);
        break;

    case TXT:
        AJ_InfoPrintf(("ParseMDNSResourceRecord(): Found a TXT record\n"));
        processed = ParseMDNSTextRData(p, bufsize, &(record->rdata.textRData), payload, paylen, prefix);
        break;

    case NS:
    case MD:
    case MF:
    case CNAME:
    case MB:
    case MG:
    case MR:
    case RNULL:
    case HINFO:
    case AAAA:
    default:
        AJ_InfoPrintf(("ParseMDNSResourceRecord(): Found a non-relevant or unknown record type that will be ignored\n"));
        processed = ParseMDNSDefaultRData(p, bufsize);
        break;
    }
    if (!processed) {
        AJ_ErrPrintf(("ParseMDNSResourceRecord(): Error occured while deserializing resource data"));
        return 0;
    }
    size += processed;
    return size;
}

static AJ_Status ParseMDNSResp(AJ_IOBuffer* rxBuf, const char* prefix, AJ_Service* service)
{
    uint8_t* buffer = (uint8_t*)rxBuf->readPtr;
    uint32_t bufsize = AJ_IO_BUF_AVAIL(rxBuf);
    uint32_t paylen = bufsize;
    MDNSHeader header;
    uint8_t* p = NULL;
    int i = 0;
    size_t ret = 0;
    size_t size = 0;
    uint8_t alljoyn_ptr_record_tcp = 0;
    uint8_t alljoyn_ptr_record_udp = 0;
    uint8_t bus_transport = 0;
    uint8_t bus_protocol = 0;
    uint8_t bus_a_record = 0;
    uint16_t service_port_tcp = 0;
    uint16_t service_port_udp = 0;
    uint16_t service_priority = 0;
    uint32_t protocol_version;
    uint8_t bus_addr[3 * 4 + 3 + 1] = { 0 };
    uint8_t service_target[256] = { 0 };
    MDNSResourceRecord r;

    memset(&header, 0, sizeof(MDNSHeader));
    size = ParseMDNSHeader(buffer, bufsize, &header);
    if (size == 0) {
        AJ_ErrPrintf(("Error occured while deserializing header\n"));
        return AJ_ERR_NO_MATCH;
    } else {
        AJ_InfoPrintf(("Successfully parsed header with %d answers and %d additional\n", header.anCount, header.arCount));
    }

    if ((header.qrType & MDNS_QR) == 0) {
        return AJ_ERR_NO_MATCH;
    }
    if (!header.anCount || !header.arCount) {
        return AJ_ERR_NO_MATCH;
    }
    if (size >= bufsize) {
        return AJ_ERR_NO_MATCH;
    }
    bufsize -= size;
    p = &buffer[size];

    for (i = 0; i < header.qdCount; i++) {
        memset(&r, 0, sizeof(MDNSResourceRecord));
        ret = ParseMDNSResourceRecord(p, bufsize, &r, buffer, paylen, NULL);
        if (ret == 0 || ret > bufsize) {
            AJ_ErrPrintf(("Error while deserializing question record.\n"));
            return AJ_ERR_NO_MATCH;
        }
        size += ret;
        bufsize -= ret;
        p += ret;
        AJ_InfoPrintf(("Skipping unexpected question in response, will be silently ignored.\n"));
    }
    for (i = 0; i < header.anCount; i++) {
        memset(&r, 0, sizeof(MDNSResourceRecord));
        ret = ParseMDNSResourceRecord(p, bufsize, &r, buffer, paylen, NULL);
        if (ret == 0 || ret > bufsize) {
            AJ_ErrPrintf(("Error while deserializing answer record.\n"));
            return AJ_ERR_NO_MATCH;
        }
        size += ret;
        bufsize -= ret;
        p += ret;
        AJ_InfoPrintf(("Processed answer %d\n", (i + 1)));

        if (r.rrType == PTR && !memcmp(r.rrDomainName.name, "_alljoyn._tcp.local", 19)) {
            AJ_InfoPrintf(("Found _alljoyn_.tcp.local PTR record.\n"));
            alljoyn_ptr_record_tcp = 1;
        }

        if (r.rrType == PTR && !memcmp(r.rrDomainName.name, "_alljoyn._udp.local", 19)) {
            AJ_InfoPrintf(("Found _alljoyn_._udp.local PTR record.\n"));
            alljoyn_ptr_record_udp = 1;
        }

        // We ignore the sender's "guid." (32 chars + 1 char for the dot) in the <guid>._alljoyn._tcp.local domain name.
        if (r.rrType == SRV && !memcmp(r.rrDomainName.name + 33, "_alljoyn._tcp.local", 19)) {
            AJ_InfoPrintf(("Found a SRV answer with domain name  %s.\n", r.rdata.srvRData.target.name));
            memset(service_target, 0, 256);
            memcpy(service_target, r.rdata.srvRData.target.name, 256);
            service_port_tcp = r.rdata.srvRData.port;
            service_priority = r.rdata.srvRData.priority;
        }

        // We ignore the sender's "guid." (32 chars + 1 char for the dot) in the <guid>._alljoyn._udp.local domain name.
        if (r.rrType == SRV && !memcmp(r.rrDomainName.name + 33, "_alljoyn._udp.local", 19)) {
            AJ_InfoPrintf(("Found a SRV answer with domain name  %s.\n", r.rdata.srvRData.target.name));
            memset(service_target, 0, 256);
            memcpy(service_target, r.rdata.srvRData.target.name, 256);
            service_port_udp = r.rdata.srvRData.port;
            service_priority = r.rdata.srvRData.priority;
        }
    }

    // PTR record must be parsed and service port should be non-zero
    // to continue with the parsing. Zero is an invalid service port.
    if ((!alljoyn_ptr_record_tcp && !alljoyn_ptr_record_udp) || (!service_port_tcp && !service_port_udp)) {
        return AJ_ERR_NO_MATCH;
    }

    for (i = 0; i < header.m_nsCount; i++) {
        memset(&r, 0, sizeof(MDNSResourceRecord));
        ret = ParseMDNSResourceRecord(p, bufsize, &r, buffer, paylen, NULL);
        if (ret == 0 || ret > bufsize) {
            AJ_ErrPrintf(("Error while deserializing authority record.\n"));
            return AJ_ERR_NO_MATCH;
        }
        size += ret;
        bufsize -= ret;
        p += ret;
        AJ_InfoPrintf(("Skipping non-relevant authority record, will be silently ignored.\n"));
    }
    for (i = 0; i < header.arCount; i++) {
        memset(&r, 0, sizeof(MDNSResourceRecord));
        ret = ParseMDNSResourceRecord(p, bufsize, &r, buffer, paylen, prefix);
        if (ret == 0 || ret > bufsize) {
            AJ_ErrPrintf(("Error while deserializing additional record.\n"));
            return AJ_ERR_NO_MATCH;
        }
        size += ret;
        bufsize -= ret;
        p += ret;
        AJ_InfoPrintf(("Processing additional record %d\n", (i + 1)));
        if (r.rrType == TXT) {
            // Ensure the advertise TXT record refers to the same guid in the SRV record.
            if (!memcmp(r.rrDomainName.name, "advertise.", 10) && !memcmp(r.rrDomainName.name + 10, service_target, 38)) {
                AJ_InfoPrintf(("Found advertise.* TXT record with full label %s.\n", r.rrDomainName.name));
                // Ensure the sender-info TXT record included a transport and had the requested name prefix
                if (r.rdata.textRData.BusNodeTransport[0] && r.rdata.textRData.BusNodeName[0]) {
                    bus_transport = 1;
                }
            }
            // Ensure the sender-info TXT record refers to the same guid in the SRV record.
            if (!memcmp(r.rrDomainName.name, "sender-info.", 12) && !memcmp(r.rrDomainName.name + 12, service_target, 38)) {
                AJ_InfoPrintf(("Found sender-info.* TXT record with full name: %s.\n", r.rrDomainName.name));
                // If the sender-info TXT record included the protocol version
                protocol_version = 0;
                if (r.rdata.textRData.BusNodeProtocolVersion[0]) {
                    // Ensure that it greater than or equal to the minimum allowed
                    protocol_version = atoi(r.rdata.textRData.BusNodeProtocolVersion);
                    if (protocol_version >= AJ_GetMinProtoVersion()) {
                        bus_protocol = 1;
                    }
                } else {
                    // Only protocol version 10 does not send the protocol version
                    // Ensure this is greater than or equal to the minimum allowed
                    if (10 >= AJ_GetMinProtoVersion()) {
                        protocol_version = 10;
                        bus_protocol = 1;
                    }
                }
            }
        }
        // Ensure the A record refers to the same guid in the SRV record.
        if (r.rrType == A && !memcmp(r.rrDomainName.name, service_target, 38)) {
            AJ_InfoPrintf(("Found an A additional record.\n"));
            memset(bus_addr, 0, (3 * 4 + 3 + 1));
            memcpy(bus_addr, r.rdata.aRData.ipv4Addr, (3 * 4 + 3 + 1));
            bus_a_record = 1;
        }
    }

    // To report a match, we must have successfully parsed an _alljoyn._tcp.local OR _alljoyn._udp.local
    // PRT record, SRV record, advertise TXT record and A record for the same
    // guid. Note that other records might have been ignored to ensure forward
    // compatibility with other record types that may be in use in the future.
    if ((alljoyn_ptr_record_tcp || alljoyn_ptr_record_udp) && (service_port_tcp || service_port_udp)
        && bus_transport && bus_protocol && bus_a_record) {

        if (alljoyn_ptr_record_tcp && service_port_tcp) {
            service->ipv4port = service_port_tcp;
            memcpy(&service->ipv4, bus_addr, sizeof(service->ipv4));
            service->addrTypes |= AJ_ADDR_TCP4;
            service->pv = protocol_version;
            service->priority = service_priority;
        }

        if (alljoyn_ptr_record_udp && service_port_udp) {
            service->ipv4portUdp = service_port_udp;
            memcpy(&service->ipv4Udp, bus_addr, sizeof(service->ipv4Udp));
            service->addrTypes |= AJ_ADDR_UDP4;
            service->pv = protocol_version;
            service->priority = service_priority;
        }

        return AJ_OK;
    } else {
        return AJ_ERR_NO_MATCH;
    }
}

#define AJ_BURST_INTERVAL    100
#define AJ_BURST_COUNT       3
#define AJ_INITIAL_INTERVAL  1000

static uint32_t searchId = 0;

AJ_Status AJ_Discover(const char* prefix, AJ_Service* service, uint32_t timeout, uint32_t selectionTimeout)
{
    AJ_Status status;
    uint32_t burstCount;
    uint32_t interval = AJ_INITIAL_INTERVAL;
    uint32_t queries = 0;
    int32_t discover = (int32_t) timeout;
    int32_t selection = (int32_t) selectionTimeout;
    int32_t listen;
    AJ_Time discoverTimer;
    AJ_Time listenTimer;
    AJ_Time selectionTimer;
    AJ_MCastSocket sock;
    AJ_GUID guid;

    if (selectionTimeout > timeout) {
        selectionTimeout = timeout;
        selection = (int32_t) selectionTimeout;
    }
    AJ_InfoPrintf(("AJ_Discover(prefix=\"%s\", service=0x%p, timeout=%d, selection timeout=%d.)\n", prefix, service, timeout, selectionTimeout));

    /*
     * Enable multicast I/O for the discovery packets.
     */
    status = AJ_Net_MCastUp(&sock);
    if (status != AJ_OK) {
        AJ_InfoPrintf(("AJ_Discover(): status=%s\n", AJ_StatusText(status)));
        return status;
    }

    /*
     * Perform discovery until node discovered or overall discover timeout reached
     */
    burstCount = 0;
    AJ_InitTimer(&selectionTimer);
    AJ_InfoPrintf(("Selection timer started\n"));
    AJ_InitTimer(&discoverTimer);
    while (discover > 0) {
        burstCount++;
        /*
         * Only send WHO-HAS if configured to consider pre-14.06 routers.
         */
        if (AJ_GetMinProtoVersion() < 10) {
            AJ_IO_BUF_RESET(&sock.tx);
            AJ_InfoPrintf(("AJ_Discover(): WHO-HAS \"%s\"\n", prefix));
            status = ComposeWhoHas(&sock.tx, prefix);
            if (status == AJ_OK) {
                sock.tx.flags |= AJ_IO_BUF_AJ;
                status = sock.tx.send(&sock.tx);
                AJ_InfoPrintf(("AJ_Discover(): WHO-HAS send status=%s\n", AJ_StatusText(status)));
                /*
                 * If the send failed the socket has probably gone away.
                 */
                if (status != AJ_OK) {
                    goto _Exit;
                }
            } else {
                /*
                 * If compose failed just continue on
                 */
                status = AJ_OK;
            }
        }

        status = AJ_GetLocalGUID(&guid);
        if (status != AJ_OK) {
            AJ_ErrPrintf(("AJ_Discover(): No GUID!\n"));
            goto _Exit;
        }

        AJ_IO_BUF_RESET(&sock.tx);
        AJ_InfoPrintf(("AJ_Discover(): mDNS \"%s\"\n", prefix));
        status = ComposeMDnsReq(&sock.tx, prefix, &guid, searchId);
        if (status == AJ_OK) {
            sock.tx.flags |= AJ_IO_BUF_MDNS;
            status = sock.tx.send(&sock.tx);
            AJ_InfoPrintf(("AJ_Discover(): mDNS send status=%s\n", AJ_StatusText(status)));
            if (status != AJ_OK) {
                goto _Exit;
            }
        } else {
            status = AJ_OK;
        }

        /*
         * Calculate listen interval
         */
        if (burstCount >= AJ_BURST_COUNT) {
            burstCount = 0;
            searchId++;
            queries++;
            if (queries == 10) {
                interval = 10000;
            } else if (queries == 11) {
                interval = 20000;
            } else if (queries >= 12) {
                interval = 40000;
            }
            listen = interval + AJ_BURST_INTERVAL;
        } else {
            listen = AJ_BURST_INTERVAL;
        }

        /*
         * If selection period has not passed do not listen longer than the selection timeout
         */
        if ((selection > 0) && (listen > selection)) {
            listen = selection;
        }

        /*
         * Do not listen longer than the overall discover timeout
         */
        if (listen > discover) {
            listen = discover;
        }

        /*
         * recv for the listen period
         */
        AJ_InitTimer(&listenTimer);
        while (listen > 0) {
            AJ_IO_BUF_RESET(&sock.rx);
            status = sock.rx.recv(&sock.rx, AJ_IO_BUF_SPACE(&sock.rx), listen);
            if (status != AJ_OK) {
                /*
                 * Anything other than AJ_ERR_TIMEOUT means bail
                 */
                if (status != AJ_ERR_TIMEOUT) {
                    goto _Exit;
                }
            } else {
                if (sock.rx.flags & AJ_IO_BUF_MDNS) {
                    memset(service, 0, sizeof(AJ_Service));
                    status = ParseMDNSResp(&sock.rx, prefix, service);
                    if (status == AJ_OK) {
                        AJ_InfoPrintf(("AJ_Discover(): mDNS discovered \"%s\"\n", prefix));

                        // skip blacklisted addresses!
                        if (!AJ_IsRoutingNodeBlacklisted(service)) {
                            AJ_AddRoutingNodeToResponseList(service);
                        } else {
                            AJ_InfoPrintf(("AJ_Discover(): Skipping blacklisted Routing Node\n"));
                        }
                    }
                }
                if (sock.rx.flags & AJ_IO_BUF_AJ) {
                    memset(service, 0, sizeof(AJ_Service));
                    status = ParseIsAt(&sock.rx, prefix, service);
                    if (status == AJ_OK) {
                        AJ_InfoPrintf(("AJ_Discover(): IS-AT discovered \"%s\"\n", prefix));

                        // skip blacklisted addresses!
                        if (!AJ_IsRoutingNodeBlacklisted(service)) {
                            AJ_AddRoutingNodeToResponseList(service);
                        } else {
                            AJ_InfoPrintf(("AJ_Discover(): Skipping blacklisted Routing Node\n"));
                        }
                    }
                }
            }
            listen -= AJ_GetElapsedTime(&listenTimer, FALSE);
        }
        selection -= AJ_GetElapsedTime(&selectionTimer, FALSE);
        if (selection < 0 && AJ_GetRoutingNodeResponseListSize() > 0) {
            break;
        }
        discover -= AJ_GetElapsedTime(&discoverTimer, FALSE);
    }

_Exit:
    memset(service, 0, sizeof(AJ_Service));
    status = AJ_SelectRoutingNodeFromResponseList(service);
    /*
     * All done with multicast for now
     */
    AJ_Net_MCastDown(&sock);
    if (status != AJ_OK) {
        AJ_InfoPrintf(("AJ_Discover(): Stop discovery of \"%s\"\n", prefix));
    }
    return status;
}
