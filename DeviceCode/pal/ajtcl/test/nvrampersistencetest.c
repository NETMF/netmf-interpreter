/**
 * @file
 */
/******************************************************************************
 * Copyright (c) 2012-2013, AllSeen Alliance. All rights reserved.
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

#include <alljoyn.h>
#include <aj_nvram.h>

extern void AJ_NVRAM_Layout_Print();

AJ_Status TestPersistence()
{
    AJ_NV_DATASET* handle = NULL;
    int id = 50;
    char writeBuf[] = "Hello, World";
    char readBuf[sizeof(writeBuf)];

    handle = AJ_NVRAM_Open(id, "r", sizeof(int));
    if (handle != NULL) {
        int val;
        size_t sz = AJ_NVRAM_Read(&val, sizeof(int), handle);
        AJ_ASSERT(sz == sizeof(int));
        AJ_ASSERT(val == 1);
        AJ_NVRAM_Close(handle);

        handle = AJ_NVRAM_Open(id + 1, "r", sizeof(readBuf));
        AJ_ASSERT(handle != NULL);
        sz = AJ_NVRAM_Read(readBuf, sizeof(readBuf), handle);
        AJ_ASSERT(sz == sizeof(readBuf));
        AJ_NVRAM_Close(handle);

        if (0 == strcmp(readBuf, writeBuf)) {
            return AJ_OK;
        } else {
            return AJ_ERR_FAILURE;
        }
    } else {
        int val = 1;
        size_t sz;

        AJ_NVRAM_Clear();

        // write a flag so we know we've done the first half of the test
        handle = AJ_NVRAM_Open(id, "w", sizeof(int));
        AJ_ASSERT(handle != NULL);
        sz = AJ_NVRAM_Write(&val, sizeof(int), handle);
        AJ_ASSERT(sz == sizeof(int));
        AJ_NVRAM_Close(handle);

        handle = AJ_NVRAM_Open(id + 1, "w", sizeof(writeBuf));
        sz = AJ_NVRAM_Write(writeBuf, sizeof(writeBuf), handle);
        AJ_ASSERT(sz == sizeof(writeBuf));
        AJ_NVRAM_Close(handle);

        // RESET!
        AJ_Reboot();
    }

    return AJ_OK;
}


int AJ_Main()
{
    AJ_Status status = AJ_OK;
    AJ_Initialize();

    status = TestPersistence();
    AJ_ASSERT(status == AJ_OK);
    return 0;
}

#ifdef AJ_MAIN
int main()
{
    return AJ_Main();
}
#endif
