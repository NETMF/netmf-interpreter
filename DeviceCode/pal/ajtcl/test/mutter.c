/**
 * @file  Marhal/Unmarshal Tester
 */
/******************************************************************************
 * Copyright (c) 2012-2014, AllSeen Alliance. All rights reserved.
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

#include "alljoyn.h"
#include "aj_util.h"
#include "aj_debug.h"
#include "aj_bufio.h"

static uint8_t wireBuffer[8 * 1024];
static size_t wireBytes = 0;

static uint8_t txBuffer[1024];
static uint8_t rxBuffer[1024];

static AJ_Status TxFunc(AJ_IOBuffer* buf)
{
    size_t tx = AJ_IO_BUF_AVAIL(buf);;


    if ((wireBytes + tx) > sizeof(wireBuffer)) {
        return AJ_ERR_WRITE;
    } else {
        memcpy(wireBuffer + wireBytes, buf->bufStart, tx);
        AJ_IO_BUF_RESET(buf);
        wireBytes += tx;
        return AJ_OK;
    }
}

AJ_Status RxFunc(AJ_IOBuffer* buf, uint32_t len, uint32_t timeout)
{
    size_t rx = AJ_IO_BUF_SPACE(buf);

    rx = min(len, rx);
    rx = min(wireBytes, rx);
    if (!rx) {
        return AJ_ERR_READ;
    } else {
        memcpy(buf->writePtr, wireBuffer, rx);
        /*
         * Shuffle the remaining data to the front of the buffer
         */
        memmove(wireBuffer, wireBuffer + rx, wireBytes - rx);
        wireBytes -= rx;
        buf->writePtr += rx;
        return AJ_OK;
    }
}

static const char* const testSignature[] = {
    "ays",
    "a{us}",
    "u(usu(ii)qsq)yyy",
    "a(usay)",
    "aas",
    "ivi",
    "v",
    "v",
    "(vvvvvv)",
    "uqay",
    "a(uuuu)",
    "a(sss)",
    "ya{ss}",
    "yyyyya{ys}",
    "(iay)",
    "ia{iv}i",
    "ay",
    "a{s(us)}"
};

typedef struct {
    uint32_t a;
    uint32_t b;
    uint32_t c;
    uint32_t d;
} TestStruct;

typedef struct {
    int32_t m;
    int32_t n;
} TestInnerStruct;

typedef struct {
    uint32_t u;
    char* str1;
    uint32_t v;
    TestInnerStruct inner;
    uint16_t q;
    char* str2;
    uint16_t r;
} TestNestedStruct;

#ifndef NDEBUG
static AJ_Status MsgInit(AJ_Message* msg, uint32_t msgId, uint8_t msgType)
{
    msg->objPath = "/test/mutter";
    msg->iface = "test.mutter";
    msg->member = "mumble";
    msg->msgId = msgId;
    msg->signature = testSignature[msgId];
    return AJ_OK;
}

extern AJ_MutterHook MutterHook;
#endif

static const char* const Fruits[] = {
    "apple", "banana", "cherry", "durian", "elderberry", "fig", "grape"
};

static const char* const Colors[] = {
    "azure", "blue", "cyan", "dun", "ecru"
};

static const uint8_t Data8[] = { 0xA0, 0xB0, 0xC0, 0xD0, 0xE0, 0xF0, 0xA1, 0xB1, 0xC2, 0xD3 };
static const uint16_t Data16[] = { 0xFF01, 0xFF02, 0xFF03, 0xFF04, 0xFF05, 0xFF06 };
static const char* const string_marshalled_after_scalar_array = "string after array";
int AJ_Main()
{
    AJ_Status status;
    AJ_BusAttachment bus;
    AJ_Message txMsg;
    AJ_Message rxMsg;
    AJ_Arg arg;
    AJ_Arg array1;
    AJ_Arg array2;
    AJ_Arg struct1;
    AJ_Arg struct2;
    size_t sz;
    uint32_t test;
    uint32_t i;
    uint32_t j;
    uint32_t k;
    uint32_t key;
    uint32_t len;
    uint32_t u;
    uint32_t v;
    int32_t n;
    int32_t m;
    uint16_t q;
    uint16_t r;
    uint8_t y;
    char* str;
    char* sig;
    void* raw;

    const size_t lengthOfShortGUID = 16;

    bus.sock.tx.direction = AJ_IO_BUF_TX;
    bus.sock.tx.bufSize = sizeof(txBuffer);
    bus.sock.tx.bufStart = txBuffer;
    bus.sock.tx.readPtr = bus.sock.tx.bufStart;
    bus.sock.tx.writePtr = bus.sock.tx.bufStart;
    bus.sock.tx.send = TxFunc;

    bus.sock.rx.direction = AJ_IO_BUF_RX;
    bus.sock.rx.bufSize = sizeof(rxBuffer);
    bus.sock.rx.bufStart = rxBuffer;
    bus.sock.rx.readPtr = bus.sock.rx.bufStart;
    bus.sock.rx.writePtr = bus.sock.rx.bufStart;
    bus.sock.rx.recv = RxFunc;

    /*
     * mutter doesn't connect to an actual daemon.
     * Hence, to ensure that we don't fail the header validation checks,
     * manually set the unique name of the Bus.
     */
    strncpy(bus.uniqueName, "DummyNaaaame.N1", lengthOfShortGUID);

    /*
     * Set the hook
     */
#ifndef NDEBUG
    MutterHook = MsgInit;
#else
    AJ_AlwaysPrintf(("mutter only works in DEBUG builds\n"));
    return -1;
#endif

    for (test = 0; test < ArraySize(testSignature); ++test) {

        status = AJ_MarshalSignal(&bus, &txMsg, test, "mutter.service", 0, 0, 0);
        if (status != AJ_OK) {
            break;
        }

        switch (test) {

        case 0:
            status = AJ_MarshalArgs(&txMsg, "ays", Data8, sizeof(Data8), string_marshalled_after_scalar_array);
            if (status != AJ_OK) {
                break;
            }
            break;

        case 1:
            status = AJ_MarshalContainer(&txMsg, &array1, AJ_ARG_ARRAY);
            if (status != AJ_OK) {
                break;
            }
            for (key = 0; key < ArraySize(Fruits); ++key) {
#ifdef EXPANDED_FORM
                AJ_Arg dict;
                status = AJ_MarshalContainer(&txMsg, &dict, AJ_ARG_DICT_ENTRY);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_MarshalArgs(&txMsg, "us", key, Fruits[key]);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_MarshalCloseContainer(&txMsg, &dict);
                if (status != AJ_OK) {
                    break;
                }
#else
                status = AJ_MarshalArgs(&txMsg, "{us}", key, Fruits[key]);
                if (status != AJ_OK) {
                    break;
                }
#endif
            }
            if (status == AJ_OK) {
                status = AJ_MarshalCloseContainer(&txMsg, &array1);
                if (status != AJ_OK) {
                    break;
                }
            }
            break;

        case 2:
#ifdef EXPANDED_FORM
            status = AJ_MarshalArgs(&txMsg, "u", 11111);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalContainer(&txMsg, &struct1, AJ_ARG_STRUCT);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArgs(&txMsg, "usu", 22222, "hello", 33333);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalContainer(&txMsg, &struct2, AJ_ARG_STRUCT);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArgs(&txMsg, "ii", -100, -200);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalCloseContainer(&txMsg, &struct2);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArgs(&txMsg, "qsq", 4444, "goodbye", 5555);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalCloseContainer(&txMsg, &struct1);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArgs(&txMsg, "yyy", 1, 2, 3);
            if (status != AJ_OK) {
                break;
            }
#else
            status = AJ_MarshalArgs(&txMsg, "u(usu(ii)qsq)yyy", 11111, 22222, "hello", 33333, -100, -200, 4444, "goodbye", 5555, 1, 2, 3);
            if (status != AJ_OK) {
                break;
            }
#endif
            break;

        case 3:
            status = AJ_MarshalContainer(&txMsg, &array1, AJ_ARG_ARRAY);
            if (status != AJ_OK) {
                break;
            }
            for (u = 0; u < ArraySize(Fruits); ++u) {
#ifdef EXPANDED_FORM
                status = AJ_MarshalContainer(&txMsg, &struct1, AJ_ARG_STRUCT);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_MarshalArgs(&txMsg, "us", u, Fruits[u]);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_MarshalArg(&txMsg, AJ_InitArg(&arg, AJ_ARG_BYTE, AJ_ARRAY_FLAG, Data8, u));
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_MarshalCloseContainer(&txMsg, &struct1);
                if (status != AJ_OK) {
                    break;
                }
#else
                status = AJ_MarshalArgs(&txMsg, "(usay)", u, Fruits[u], Data8, u);
                if (status != AJ_OK) {
                    break;
                }
#endif
            }
            if (status == AJ_OK) {
                status = AJ_MarshalCloseContainer(&txMsg, &array1);
                if (status != AJ_OK) {
                    break;
                }
            }
            break;

        case 4:
            status = AJ_MarshalContainer(&txMsg, &array1, AJ_ARG_ARRAY);
            if (status != AJ_OK) {
                break;
            }
            for (j = 0; j < 3; ++j) {
                status = AJ_MarshalContainer(&txMsg, &array2, AJ_ARG_ARRAY);
                if (status != AJ_OK) {
                    break;
                }
                for (k = j; k < ArraySize(Fruits); ++k) {
                    status = AJ_MarshalArgs(&txMsg, "s", Fruits[k]);
                    if (status != AJ_OK) {
                        break;
                    }
                }
                status = AJ_MarshalCloseContainer(&txMsg, &array2);
                if (status != AJ_OK) {
                    break;
                }
            }
            if (status == AJ_OK) {
                status = AJ_MarshalCloseContainer(&txMsg, &array1);
                if (status != AJ_OK) {
                    break;
                }
            }
            break;

        case 5:
            status = AJ_MarshalArgs(&txMsg, "i", 987654321);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalVariant(&txMsg, "a(ii)");
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalContainer(&txMsg, &array1, AJ_ARG_ARRAY);
            if (status != AJ_OK) {
                break;
            }
            for (j = 0; j < 16; ++j) {
#ifdef EXPANDED_FORM
                status = AJ_MarshalContainer(&txMsg, &struct1, AJ_ARG_STRUCT);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_MarshalArgs(&txMsg, "ii", j + 1, (j + 1) * 100);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_MarshalCloseContainer(&txMsg, &struct1);
                if (status != AJ_OK) {
                    break;
                }

#else
                status = AJ_MarshalArgs(&txMsg, "(ii)", j + 1, (j + 1) * 100);
                if (status != AJ_OK) {
                    break;
                }
#endif
            }
            if (status == AJ_OK) {
                status = AJ_MarshalCloseContainer(&txMsg, &array1);
                if (status != AJ_OK) {
                    break;
                }
            }
            status = AJ_MarshalArgs(&txMsg, "i", 123456789);
            if (status != AJ_OK) {
                break;
            }
            break;

        case 6:
#ifdef EXPANDED_FORM
            status = AJ_MarshalVariant(&txMsg, "(ivi)");
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalContainer(&txMsg, &struct1, AJ_ARG_STRUCT);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArgs(&txMsg, "i", 1212121);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalVariant(&txMsg, "s");
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArgs(&txMsg, "s", "inner variant");
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArgs(&txMsg, "i", 3434343);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalCloseContainer(&txMsg, &struct1);
            if (status != AJ_OK) {
                break;
            }
#else
            status = AJ_MarshalArgs(&txMsg, "v", "(ivi)", 121212121, "s", "inner variant", 3434343);
            if (status != AJ_OK) {
                break;
            }
#endif
            break;

        case 7:
            status = AJ_MarshalVariant(&txMsg, "v");
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalVariant(&txMsg, "v");
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalVariant(&txMsg, "v");
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalVariant(&txMsg, "v");
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalVariant(&txMsg, "s");
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArgs(&txMsg, "s", "deep variant");
            if (status != AJ_OK) {
                break;
            }
            break;

        case 8:
#ifdef EXPANDED_FORM
            status = AJ_MarshalContainer(&txMsg, &struct1, AJ_ARG_STRUCT);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalVariant(&txMsg, "i");
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArgs(&txMsg, "i", 1212121);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalVariant(&txMsg, "s");
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArgs(&txMsg, "s", "variant");
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalVariant(&txMsg, "ay");
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArg(&txMsg, AJ_InitArg(&arg, AJ_ARG_BYTE, AJ_ARRAY_FLAG, Data8, sizeof(Data8)));
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalVariant(&txMsg, "ay");
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArg(&txMsg, AJ_InitArg(&arg, AJ_ARG_BYTE, AJ_ARRAY_FLAG, Data8, sizeof(Data8)));
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalVariant(&txMsg, "aq");
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArg(&txMsg, AJ_InitArg(&arg, AJ_ARG_UINT16, AJ_ARRAY_FLAG, Data16, sizeof(Data16)));
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalVariant(&txMsg, "s");
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArgs(&txMsg, "s", "variant2");
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalCloseContainer(&txMsg, &struct1);
            if (status != AJ_OK) {
                break;
            }
#else
            status = AJ_MarshalArgs(&txMsg, "(vvvvvv)",
                                    "i", 121212121,
                                    "s", "variant",
                                    "ay",  Data8, sizeof(Data8),
                                    "ay",  Data8, sizeof(Data8),
                                    "aq",  Data16, sizeof(Data16),
                                    "s",  "variant2");
            if (status != AJ_OK) {
                break;
            }
#endif
            break;

        case 9:
            status = AJ_MarshalArgs(&txMsg, "uq", 0xF00F00F0, 0x0707);
            if (status != AJ_OK) {
                break;
            }
            len = 5000;
            status = AJ_DeliverMsgPartial(&txMsg, len + 4);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalRaw(&txMsg, &len, 4);
            if (status != AJ_OK) {
                break;
            }
            for (j = 0; j < len; ++j) {
                uint8_t n = (uint8_t)j;
                status = AJ_MarshalRaw(&txMsg, &n, 1);
                if (status != AJ_OK) {
                    break;
                }
            }
            break;

        case 10:
            len = 500;
            u = len * sizeof(TestStruct);
            status = AJ_DeliverMsgPartial(&txMsg, u + sizeof(u) + 4);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalRaw(&txMsg, &u, sizeof(u));
            if (status != AJ_OK) {
                break;
            }
            /*
             * Structs are always 8 byte aligned
             */
            u = 0;
            status = AJ_MarshalRaw(&txMsg, &u, 4);
            if (status != AJ_OK) {
                break;
            }
            for (j = 0; j < len; ++j) {
                TestStruct ts;
                ts.a = j;
                ts.b = j + 1;
                ts.c = j + 2;
                ts.d = j + 3;
                status = AJ_MarshalRaw(&txMsg, &ts, sizeof(ts));
                if (status != AJ_OK) {
                    break;
                }
            }
            break;

        case 11:
            status = AJ_MarshalContainer(&txMsg, &array1, AJ_ARG_ARRAY);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalCloseContainer(&txMsg, &array1);
            if (status != AJ_OK) {
                break;
            }
            break;

        case 12:
            status = AJ_MarshalArgs(&txMsg, "y", 127);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalContainer(&txMsg, &array1, AJ_ARG_ARRAY);
            if (status != AJ_OK) {
                break;
            }
            for (key = 0; key < ArraySize(Colors); ++key) {
                AJ_Arg dict;
                status = AJ_MarshalContainer(&txMsg, &dict, AJ_ARG_DICT_ENTRY);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_MarshalArgs(&txMsg, "ss", Colors[key], Fruits[key]);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_MarshalCloseContainer(&txMsg, &dict);
                if (status != AJ_OK) {
                    break;
                }
            }
            if (status == AJ_OK) {
                status = AJ_MarshalCloseContainer(&txMsg, &array1);
                if (status != AJ_OK) {
                    break;
                }
            }
            break;

        case 13:
            status = AJ_MarshalArgs(&txMsg, "y", 0x11);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArgs(&txMsg, "y", 0x22);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArgs(&txMsg, "y", 0x33);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArgs(&txMsg, "y", 0x44);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArgs(&txMsg, "y", 0x55);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalContainer(&txMsg, &array1, AJ_ARG_ARRAY);
            if (status != AJ_OK) {
                break;
            }
            for (key = 0; key < ArraySize(Colors); ++key) {
                AJ_Arg dict;
                status = AJ_MarshalContainer(&txMsg, &dict, AJ_ARG_DICT_ENTRY);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_MarshalArgs(&txMsg, "ys", (uint8_t)key, Colors[key]);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_MarshalCloseContainer(&txMsg, &dict);
                if (status != AJ_OK) {
                    break;
                }
            }
            if (status == AJ_OK) {
                status = AJ_MarshalCloseContainer(&txMsg, &array1);
                if (status != AJ_OK) {
                    break;
                }
            }
            break;

        case 14:
            status = AJ_MarshalContainer(&txMsg, &struct1, AJ_ARG_STRUCT);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArgs(&txMsg, "i", 3434343);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArg(&txMsg, AJ_InitArg(&arg, AJ_ARG_BYTE, AJ_ARRAY_FLAG, Data8, sizeof(Data8)));
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalCloseContainer(&txMsg, &struct1);
            if (status != AJ_OK) {
                break;
            }
            break;

        case 15:
            status = AJ_MarshalArgs(&txMsg, "i", 0x1111);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalContainer(&txMsg, &array1, AJ_ARG_ARRAY);
            if (status != AJ_OK) {
                break;
            }

            for (j = 0; j < 8; ++j) {
#ifdef EXPANDED_FORM
                AJ_Arg dict;
                status = AJ_MarshalContainer(&txMsg, &dict, AJ_ARG_DICT_ENTRY);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_MarshalArgs(&txMsg, "i", j);
                if (status != AJ_OK) {
                    break;
                }
                if (j == 4) {
                    status = AJ_MarshalVariant(&txMsg, "s");
                    if (status != AJ_OK) {
                        break;
                    }
                    status = AJ_MarshalArgs(&txMsg, "s", "This is a variant string");
                    if (status != AJ_OK) {
                        break;
                    }
                } else {
                    status = AJ_MarshalVariant(&txMsg, "i");
                    if (status != AJ_OK) {
                        break;
                    }
                    status = AJ_MarshalArgs(&txMsg, "i", j + 200);
                    if (status != AJ_OK) {
                        break;
                    }
                }
                status = AJ_MarshalCloseContainer(&txMsg, &dict);
                if (status != AJ_OK) {
                    break;
                }
#else
                if (j == 4) {
                    status = AJ_MarshalArgs(&txMsg, "{iv}", j, "s", "This is a variant string");
                    if (status != AJ_OK) {
                        break;
                    }
                } else {
                    status = AJ_MarshalArgs(&txMsg, "{iv}", j, "i", j + 200);
                    if (status != AJ_OK) {
                        break;
                    }
                }
#endif
            }

            status = AJ_MarshalCloseContainer(&txMsg, &array1);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_MarshalArgs(&txMsg, "i", 0x2222);
            if (status != AJ_OK) {
                break;
            }
            break;

        case 16:
            status = AJ_MarshalContainer(&txMsg, &array1, AJ_ARG_ARRAY);
            if (status != AJ_OK) {
                break;
            }
            for (i = 0; i < 100; ++i) {
                status = AJ_MarshalArgs(&txMsg, "y", i);
            }
            status = AJ_MarshalCloseContainer(&txMsg, &array1);
            if (status != AJ_OK) {
                break;
            }
            break;

        case 17:
            status = AJ_MarshalContainer(&txMsg, &array1, AJ_ARG_ARRAY);
            if (status != AJ_OK) {
                break;
            }
            for (key = 0; key < ArraySize(Colors); ++key) {
#ifdef EXPANDED_FORM
                AJ_Arg dict;
                status = AJ_MarshalContainer(&txMsg, &dict, AJ_ARG_DICT_ENTRY);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_MarshalArgs(&txMsg, "s", Colors[key]);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_MarshalContainer(&txMsg, &struct1, AJ_ARG_STRUCT);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_MarshalArgs(&txMsg, "us", key, Fruits[key]);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_MarshalCloseContainer(&txMsg, &struct1);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_MarshalCloseContainer(&txMsg, &dict);
                if (status != AJ_OK) {
                    break;
                }
#else
                status = AJ_MarshalArgs(&txMsg, "{s(us)}", Colors[key], key, Fruits[key]);
                if (status != AJ_OK) {
                    break;
                }
#endif
            }
            if (status == AJ_OK) {
                status = AJ_MarshalCloseContainer(&txMsg, &array1);
                if (status != AJ_OK) {
                    break;
                }
            }
            break;
        }
        if (status != AJ_OK) {
            AJ_AlwaysPrintf(("Failed %d\n", test));
            break;
        }

        AJ_AlwaysPrintf(("deliver\n"));
        AJ_DeliverMsg(&txMsg);

        status = AJ_UnmarshalMsg(&bus, &rxMsg, 0);
        if (status != AJ_OK) {
            break;
        }

        switch (test) {
        case 0:
            status = AJ_UnmarshalArgs(&rxMsg, "ays", (const void**)&raw, &sz, &str);
            if (status != AJ_OK) {
                break;
            }
            if (strncmp(str, string_marshalled_after_scalar_array, strlen(string_marshalled_after_scalar_array))) {
                break;
            }
            break;

        case 1:
            status = AJ_UnmarshalContainer(&rxMsg, &array1, AJ_ARG_ARRAY);
            if (status != AJ_OK) {
                break;
            }
            for (j = 0; j >= 0; ++j) {
                if (j & 1) {
                    AJ_AlwaysPrintf(("Skipping dict entry %d\n", j));
                    status = AJ_SkipArg(&rxMsg);
                    if (status != AJ_OK) {
                        break;
                    }
                } else {
                    char* fruit;
#ifdef EXPANDED_FORM
                    AJ_Arg dict;
                    status = AJ_UnmarshalContainer(&rxMsg, &dict, AJ_ARG_DICT_ENTRY);
                    if (status != AJ_OK) {
                        break;
                    }
                    status = AJ_UnmarshalArgs(&rxMsg, "us", &key, &fruit);
                    if (status != AJ_OK) {
                        break;
                    }
                    AJ_AlwaysPrintf(("Unmarshal[%d] = %s\n", key, fruit));
                    status = AJ_UnmarshalCloseContainer(&rxMsg, &dict);
                    if (status != AJ_OK) {
                        break;
                    }
#else
                    status = AJ_UnmarshalArgs(&rxMsg, "{us}", &key, &fruit);
                    if (status != AJ_OK) {
                        break;
                    }
                    AJ_AlwaysPrintf(("Unmarshal[%d] = %s\n", key, fruit));
#endif
                }
            }
            /*
             * We expect AJ_ERR_NO_MORE
             */
            if (status == AJ_ERR_NO_MORE) {
                status = AJ_UnmarshalCloseContainer(&rxMsg, &array1);
                if (status != AJ_OK) {
                    break;
                }
            }
            break;

        case 2:
#ifdef EXPANDED_FORM
            status = AJ_UnmarshalArgs(&rxMsg, "u", &u);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal %u\n", u));
            status = AJ_UnmarshalContainer(&rxMsg, &struct1, AJ_ARG_STRUCT);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalArgs(&rxMsg, "usu", &u, &str, &v);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal %u %s %u\n", u, str, v));
            status = AJ_UnmarshalContainer(&rxMsg, &struct2, AJ_ARG_STRUCT);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalArgs(&rxMsg, "ii", &n, &m);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal %d %d\n", n, m));
            status = AJ_UnmarshalCloseContainer(&rxMsg, &struct2);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalArgs(&rxMsg, "qsq", &q, &str, &r);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal %u %s %u\n", q, str, r));
            status = AJ_UnmarshalCloseContainer(&rxMsg, &struct1);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalArgs(&rxMsg, "y", &y);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal %d\n", y));
            status = AJ_UnmarshalArgs(&rxMsg, "y", &y);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal %d\n", y));
            status = AJ_UnmarshalArgs(&rxMsg, "y", &y);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal %d\n", y));
#else
            {
                TestNestedStruct ns;
                uint8_t y1;
                uint8_t y2;

                status = AJ_UnmarshalArgs(&rxMsg, "u(usu(ii)qsq)yyy", &u, &ns.u, &ns.str1, &ns.v, &ns.inner.m, &ns.inner.n, &ns.q, &ns.str2, &ns.r, &y, &y1, &y2);
                if (status != AJ_OK) {
                    break;
                }
                AJ_AlwaysPrintf(("Unmarshal %u (%u %s %u (%d %d) %u %s %u) %d %d %d\n", u, ns.u, ns.str1, ns.v, ns.inner.m, ns.inner.n, ns.q, ns.str2, ns.r, y, y1, y2));
            }
#endif
            break;

        case 3:
            status = AJ_UnmarshalContainer(&rxMsg, &array1, AJ_ARG_ARRAY);
            if (status != AJ_OK) {
                break;
            }
            while (status == AJ_OK) {
#ifdef EXPANDED_FORM
                status = AJ_UnmarshalContainer(&rxMsg, &struct1, AJ_ARG_STRUCT);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_UnmarshalArgs(&rxMsg, "us", &u, &str);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_UnmarshalArg(&rxMsg, &arg);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_UnmarshalCloseContainer(&rxMsg, &struct1);
                if (status != AJ_OK) {
                    break;
                }
#else
                size_t len;
                uint8_t* data;
                status = AJ_UnmarshalArgs(&rxMsg, "(usay)", &u, &str, &data, &len);
                if (status != AJ_OK) {
                    break;
                }
#endif
                AJ_AlwaysPrintf(("Unmarshal %d %s\n", u, str));
            }
            /*
             * We expect AJ_ERR_NO_MORE
             */
            if (status == AJ_ERR_NO_MORE) {
                status = AJ_UnmarshalCloseContainer(&rxMsg, &array1);
                if (status != AJ_OK) {
                    break;
                }
            }
            break;

        case 4:
            status = AJ_UnmarshalContainer(&rxMsg, &array1, AJ_ARG_ARRAY);
            if (status != AJ_OK) {
                break;
            }
            while (status == AJ_OK) {
                status = AJ_UnmarshalContainer(&rxMsg, &array2, AJ_ARG_ARRAY);
                if (status != AJ_OK) {
                    break;
                }
                while (status == AJ_OK) {
                    status = AJ_UnmarshalArg(&rxMsg, &arg);
                    if (status != AJ_OK) {
                        break;
                    }
                    AJ_AlwaysPrintf(("Unmarshal %s\n", arg.val.v_string));
                }
                /*
                 * We expect AJ_ERR_NO_MORE
                 */
                if (status == AJ_ERR_NO_MORE) {
                    status = AJ_UnmarshalCloseContainer(&rxMsg, &array2);
                    if (status != AJ_OK) {
                        break;
                    }
                }
            }
            /*
             * We expect AJ_ERR_NO_MORE
             */
            if (status == AJ_ERR_NO_MORE) {
                status = AJ_UnmarshalCloseContainer(&rxMsg, &array1);
                if (status != AJ_OK) {
                    break;
                }
            }
            break;

        case 5:
            status = AJ_UnmarshalArgs(&rxMsg, "i", &j);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal %d\n", j));
            status = AJ_UnmarshalVariant(&rxMsg, (const char**)&sig);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal variant %s\n", sig));
            status = AJ_UnmarshalContainer(&rxMsg, &array1, AJ_ARG_ARRAY);
            if (status != AJ_OK) {
                break;
            }
            while (status == AJ_OK) {
                status = AJ_UnmarshalContainer(&rxMsg, &struct1, AJ_ARG_STRUCT);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_UnmarshalArgs(&rxMsg, "ii", &j, &k);
                if (status != AJ_OK) {
                    break;
                }
                AJ_AlwaysPrintf(("Unmarshal[%d] %d\n", j, k));
                status = AJ_UnmarshalCloseContainer(&rxMsg, &struct1);
                if (status != AJ_OK) {
                    break;
                }
            }
            /*
             * We expect AJ_ERR_NO_MORE
             */
            if (status != AJ_ERR_NO_MORE) {
                break;
            }
            status = AJ_UnmarshalCloseContainer(&rxMsg, &array1);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalArgs(&rxMsg, "i", &j);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal %d\n", j));
            break;

        case 6:
#ifdef EXPANDED_FORM
            status = AJ_UnmarshalVariant(&rxMsg, (const char**)&sig);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal variant %s\n", sig));
            status = AJ_UnmarshalContainer(&rxMsg, &struct1, AJ_ARG_STRUCT);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalArgs(&rxMsg, "i", &j);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal %d\n", j));
            status = AJ_UnmarshalVariant(&rxMsg, (const char**)&sig);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal variant %s\n", sig));
            status = AJ_UnmarshalArgs(&rxMsg, "s", &str);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal %s\n", str));
            status = AJ_UnmarshalArgs(&rxMsg, "i", &j);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal %d\n", j));
            status = AJ_UnmarshalCloseContainer(&rxMsg, &struct1);
            if (status != AJ_OK) {
                break;
            }
#else
            status = AJ_UnmarshalArgs(&rxMsg, "v", "(ivi)", &j, "s", &str, &j);
            if (status != AJ_OK) {
                break;
            }
#endif
            break;

        case 7:
            status = AJ_UnmarshalVariant(&rxMsg, (const char**)&sig);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal variant %s\n", sig));
            status = AJ_UnmarshalVariant(&rxMsg, (const char**)&sig);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal variant %s\n", sig));
            status = AJ_UnmarshalVariant(&rxMsg, (const char**)&sig);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal variant %s\n", sig));
            status = AJ_UnmarshalVariant(&rxMsg, (const char**)&sig);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal variant %s\n", sig));
            status = AJ_UnmarshalVariant(&rxMsg, (const char**)&sig);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal variant %s\n", sig));
            status = AJ_UnmarshalArgs(&rxMsg, "s", &str);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal %s\n", str));
            break;

        case 8:
            status = AJ_UnmarshalContainer(&rxMsg, &struct1, AJ_ARG_STRUCT);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalVariant(&rxMsg, (const char**)&sig);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal variant %s\n", sig));
            status = AJ_UnmarshalArgs(&rxMsg, "i", &j);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal %d\n", j));
            status = AJ_UnmarshalVariant(&rxMsg, (const char**)&sig);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal variant %s\n", sig));
            status = AJ_UnmarshalArgs(&rxMsg, "s", &str);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalVariant(&rxMsg, (const char**)&sig);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal variant %s\n", sig));
            status = AJ_UnmarshalArg(&rxMsg, &arg);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Skipping variant\n"));
            status = AJ_SkipArg(&rxMsg);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalVariant(&rxMsg, (const char**)&sig);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal variant %s\n", sig));
            status = AJ_UnmarshalArg(&rxMsg, &arg);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Skipping variant\n"));
            status = AJ_SkipArg(&rxMsg);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalCloseContainer(&rxMsg, &struct1);
            if (status != AJ_OK) {
                break;
            }
            break;

        case 9:
            status = AJ_UnmarshalArgs(&rxMsg, "uq", &j, &q);
            if (status != AJ_OK) {
                break;
            }
            AJ_AlwaysPrintf(("Unmarshal %x\n", j));
            AJ_AlwaysPrintf(("Unmarshal %x\n", q));
            status = AJ_UnmarshalRaw(&rxMsg, (const void**)&raw, sizeof(len), &sz);
            if (status != AJ_OK) {
                break;
            }
            len = *((uint32_t*)raw);
            AJ_AlwaysPrintf(("UnmarshalRaw %d\n", len));
            for (j = 0; j < len; ++j) {
                uint8_t v;
                status = AJ_UnmarshalRaw(&rxMsg, (const void**)&raw, 1, &sz);
                if (status != AJ_OK) {
                    break;
                }
                v = *((uint8_t*)raw);
                if (v != (uint8_t)j) {
                    status = AJ_ERR_FAILURE;
                    break;
                }
            }
            break;

        case 10:
            status = AJ_UnmarshalRaw(&rxMsg, (const void**)&raw, 4, &sz);
            if (status != AJ_OK) {
                break;
            }
            len = *((uint32_t*)raw) / sizeof(TestStruct);
            /*
             * Structs are always 8 byte aligned
             */
            status = AJ_UnmarshalRaw(&rxMsg, (const void**)&raw, 4, &sz);
            if (status != AJ_OK) {
                break;
            }
            for (j = 0; j < len; ++j) {
                TestStruct* ts;
                status = AJ_UnmarshalRaw(&rxMsg, (const void**)&ts, sizeof(TestStruct), &sz);
                if (status != AJ_OK) {
                    break;
                }
                if ((ts->a != j) || (ts->b != (j + 1)) || (ts->c != (j + 2)) || (ts->d != (j + 3))) {
                    status = AJ_ERR_FAILURE;
                    break;
                }
            }
            break;

        case 11:
            status = AJ_UnmarshalContainer(&rxMsg, &array1, AJ_ARG_ARRAY);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalArg(&rxMsg, &arg);
            /*
             * We expect AJ_ERR_NO_MORE
             */
            if (status == AJ_ERR_NO_MORE) {
                status = AJ_UnmarshalCloseContainer(&rxMsg, &array1);
                if (status != AJ_OK) {
                    break;
                }
            }
            break;

        case 12:
            status = AJ_UnmarshalArgs(&rxMsg, "y", &y);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalContainer(&rxMsg, &array1, AJ_ARG_ARRAY);
            if (status != AJ_OK) {
                break;
            }
            while (TRUE) {
                AJ_Arg dict;
                char* fruit;
                char* color;
                status = AJ_UnmarshalContainer(&rxMsg, &dict, AJ_ARG_DICT_ENTRY);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_UnmarshalArgs(&rxMsg, "ss", &color, &fruit);
                if (status != AJ_OK) {
                    break;
                }
                AJ_AlwaysPrintf(("Unmarshal[%s] = %s\n", color, fruit));
                status = AJ_UnmarshalCloseContainer(&rxMsg, &dict);
                if (status != AJ_OK) {
                    break;
                }
            }
            /*
             * We expect AJ_ERR_NO_MORE
             */
            if (status == AJ_ERR_NO_MORE) {
                status = AJ_UnmarshalCloseContainer(&rxMsg, &array1);
                if (status != AJ_OK) {
                    break;
                }
            }
            break;

        case 13:
            status = AJ_UnmarshalArgs(&rxMsg, "y", &y);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalArgs(&rxMsg, "y", &y);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalArgs(&rxMsg, "y", &y);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalArgs(&rxMsg, "y", &y);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalArgs(&rxMsg, "y", &y);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalContainer(&rxMsg, &array1, AJ_ARG_ARRAY);
            if (status != AJ_OK) {
                break;
            }
            while (TRUE) {
                char* color;
#ifdef EXPANDED_FORM
                AJ_Arg dict;
                status = AJ_UnmarshalContainer(&rxMsg, &dict, AJ_ARG_DICT_ENTRY);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_UnmarshalArgs(&rxMsg, "ys", &y, &color);
                if (status != AJ_OK) {
                    break;
                }
                AJ_AlwaysPrintf(("Unmarshal[%d] = %s\n", y, color));
                status = AJ_UnmarshalCloseContainer(&rxMsg, &dict);
                if (status != AJ_OK) {
                    break;
                }
#else
                status = AJ_UnmarshalArgs(&rxMsg, "{ys}", &y, &color);
                if (status != AJ_OK) {
                    break;
                }
                AJ_AlwaysPrintf(("Unmarshal[%d] = %s\n", y, color));
#endif
            }
            /*
             * We expect AJ_ERR_NO_MORE
             */
            if (status == AJ_ERR_NO_MORE) {
                status = AJ_UnmarshalCloseContainer(&rxMsg, &array1);
                if (status != AJ_OK) {
                    break;
                }
            }
            break;

        case 14:
            status = AJ_UnmarshalContainer(&rxMsg, &struct1, AJ_ARG_STRUCT);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalArgs(&rxMsg, "i", &n);
            if (status != AJ_OK) {
                break;
            }
            AJ_ASSERT(n == 3434343);

            status = AJ_UnmarshalArg(&rxMsg, &arg);
            if (status != AJ_OK) {
                break;
            }
            for (j = 0; j < arg.len; ++j) {
                uint8_t val = arg.val.v_byte[j];
                AJ_AlwaysPrintf(("Unmarhsalled array1[%u] = %u\n", j, val));
                AJ_ASSERT(val == Data8[j]);
            }

            status = AJ_UnmarshalCloseContainer(&rxMsg, &struct1);
            if (status != AJ_OK) {
                break;
            }
            break;

        case 15:
            status = AJ_UnmarshalArgs(&rxMsg, "i", &j);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalContainer(&rxMsg, &array1, AJ_ARG_ARRAY);
            if (status != AJ_OK) {
                break;
            }
            for (j = 0;; ++j) {
                AJ_Arg dict;
                int key;
                status = AJ_UnmarshalContainer(&rxMsg, &dict, AJ_ARG_DICT_ENTRY);
                if (status != AJ_OK) {
                    break;
                }
                status = AJ_UnmarshalArgs(&rxMsg, "i", &key);
                if (status != AJ_OK) {
                    break;
                }
                if (key == 4) {
                    status = AJ_UnmarshalVariant(&rxMsg, (const char**)&sig);
                    if (status != AJ_OK) {
                        break;
                    }
                    AJ_AlwaysPrintf(("Unmarshal dict entry key=%d variant %s\n", key, sig));
                    status = AJ_UnmarshalArgs(&rxMsg, sig, &str);
                    if (status != AJ_OK) {
                        break;
                    }
                } else {
                    AJ_AlwaysPrintf(("Skipping dict entry key=%d\n", key));
                    status = AJ_SkipArg(&rxMsg);
                    if (status != AJ_OK) {
                        break;
                    }
                }
                status = AJ_UnmarshalCloseContainer(&rxMsg, &dict);
                if (status != AJ_OK) {
                    break;
                }
            }
            status = AJ_UnmarshalCloseContainer(&rxMsg, &array1);
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalArgs(&rxMsg, "i", &j);
            if (status != AJ_OK) {
                break;
            }
            AJ_ASSERT(j == 0x2222);
            break;

        case 16:
            status = AJ_UnmarshalContainer(&rxMsg, &array1, AJ_ARG_ARRAY);
            if (status != AJ_OK) {
                break;
            }
            while (status == AJ_OK) {
                status = AJ_UnmarshalArgs(&rxMsg, "y", &i);
            }
            /*
             * We expect AJ_ERR_NO_MORE
             */
            if (status == AJ_ERR_NO_MORE) {
                status = AJ_OK;
            }
            if (status != AJ_OK) {
                break;
            }
            status = AJ_UnmarshalCloseContainer(&rxMsg, &array1);
            if (status != AJ_OK) {
                break;
            }
            break;

        case 17:
            status = AJ_UnmarshalContainer(&rxMsg, &array1, AJ_ARG_ARRAY);
            if (status != AJ_OK) {
                break;
            }
            for (j = 0; j >= 0; ++j) {
                if (j & 1) {
                    AJ_AlwaysPrintf(("Skipping dict entry %d\n", j));
                    status = AJ_SkipArg(&rxMsg);
                    if (status != AJ_OK) {
                        break;
                    }
                } else {
                    char* color;
                    char* fruit;
#ifdef EXPANDED_FORM
                    AJ_Arg dict;
                    status = AJ_UnmarshalContainer(&rxMsg, &dict, AJ_ARG_DICT_ENTRY);
                    if (status != AJ_OK) {
                        break;
                    }
                    status = AJ_UnmarshalArgs(&rxMsg, "s", &color);
                    if (status != AJ_OK) {
                        break;
                    }
                    status = AJ_UnmarshalContainer(&rxMsg, &struct1, AJ_ARG_STRUCT);
                    if (status != AJ_OK) {
                        break;
                    }
                    status = AJ_UnmarshalArgs(&rxMsg, "us", &key, &fruit);
                    if (status != AJ_OK) {
                        break;
                    }
                    AJ_AlwaysPrintf(("Unmarshal %s => (%d, %s)\n", color, key, fruit));
                    status = AJ_UnmarshalCloseContainer(&rxMsg, &struct1);
                    if (status != AJ_OK) {
                        break;
                    }
                    status = AJ_UnmarshalCloseContainer(&rxMsg, &dict);
                    if (status != AJ_OK) {
                        break;
                    }
#else
                    status = AJ_UnmarshalArgs(&rxMsg, "{s(us)}", &color, &key, &fruit);
                    if (status != AJ_OK) {
                        break;
                    }
                    AJ_AlwaysPrintf(("Unmarshal %s => (%d, %s)\n", color, key, fruit));
#endif
                }
            }
            /*
             * We expect AJ_ERR_NO_MORE
             */
            if (status == AJ_ERR_NO_MORE) {
                status = AJ_UnmarshalCloseContainer(&rxMsg, &array1);
                if (status != AJ_OK) {
                    break;
                }
            }
            break;
        }

        if (status != AJ_OK) {
            AJ_AlwaysPrintf(("Failed %d\n", test));
            break;
        }
        AJ_CloseMsg(&rxMsg);
        AJ_AlwaysPrintf(("Passed %d \"%s\"\n", test, testSignature[test]));
    }
    if (status != AJ_OK) {
        AJ_AlwaysPrintf(("Marshal/Unmarshal unit test[%d] failed %d\n", test, status));
    }

    return status;
}

#ifdef AJ_MAIN
int main()
{
    return AJ_Main();
}
#endif

