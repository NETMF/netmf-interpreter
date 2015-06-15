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
#define AJ_MODULE TARGET_UTIL

#include <windows.h>
#include <process.h>
#include <sys/timeb.h>
#include <stdio.h>
#include <stdlib.h>
#include <assert.h>
#include <string.h>
#include "aj_debug.h"
#include "aj_target.h"
#include "aj_util.h"

uint8_t dbgTARGET_UTIL = 0;

void AJ_Sleep(uint32_t time)
{
    Sleep(time);
}

uint32_t AJ_GetElapsedTime(AJ_Time* timer, uint8_t cumulative)
{
    uint32_t elapsed;
    struct _timeb now;

    _ftime(&now);

    elapsed = (uint32_t)((1000 * (now.time - timer->seconds)) + (now.millitm - timer->milliseconds));
    if (!cumulative) {
        timer->seconds = (uint32_t)now.time;
        timer->milliseconds = (uint32_t)now.millitm;
    }
    return elapsed;
}
void AJ_InitTimer(AJ_Time* timer)
{
    struct _timeb now;
    _ftime(&now);
    timer->seconds = (uint32_t)now.time;
    timer->milliseconds = (uint32_t)now.millitm;
}
/*
 * get a line of input from the the file pointer (most likely stdin).
 * This will capture the the num-1 characters or till a newline character is
 * entered.
 *
 * @param[out] str a pointer to a character array that will hold the user input
 * @param[in]  num the size of the character array 'str'
 * @param[in]  fp  the file pointer the sting will be read from. (most likely stdin)
 *
 * @return returns the same string as 'str' if there has been a read error a null
 *                 pointer will be returned and 'str' will remain unchanged.
 */
char* AJ_GetLine(char* str, size_t num, void* fp)
{
    char*p = fgets(str, (int)num, (FILE*)fp);

    if (p != NULL) {
        size_t last = strlen(str) - 1;
        if (str[last] == '\n') {
            str[last] = '\0';
        }
    }
    return p;
}

static boolean ioThreadRunning = FALSE;
static char cmdline[1024];
static const uint32_t stacksize = 80 * 1024;
static uint8_t consumed = TRUE;
static HANDLE handle;
static unsigned int threadId;

unsigned __stdcall RunFunc(void* threadArg)
{
    while (ioThreadRunning) {
        if (consumed) {
            AJ_GetLine(cmdline, sizeof(cmdline), stdin);
            consumed = FALSE;
        }
        AJ_Sleep(1000);
    }
    return 0;
}

uint8_t AJ_StartReadFromStdIn()
{
    if (!ioThreadRunning) {
        handle = (HANDLE)_beginthreadex(NULL, stacksize, &RunFunc, NULL, 0, &threadId);
        if (handle <= 0) {
            AJ_ErrPrintf(("Fail to spin a thread for reading from stdin\n"));
            return FALSE;
        }
        ioThreadRunning = TRUE;
        return TRUE;
    }
    return FALSE;
}

char* AJ_GetCmdLine(char* buf, size_t num)
{
    if (!consumed) {
        strncpy(buf, cmdline, num);
        buf[num - 1] = '\0';
        consumed = TRUE;
        return buf;
    }
    return NULL;
}

uint8_t AJ_StopReadFromStdIn()
{
    if (ioThreadRunning) {
        ioThreadRunning = FALSE;
        return TRUE;
    }
    return FALSE;
}

uint64_t AJ_DecodeTime(char* der, char* fmt)
{
    //TODO
    return 0;
}

void* AJ_Malloc(size_t sz)
{
    return malloc(sz);
}
void* AJ_Realloc(void* ptr, size_t size)
{
    return realloc(ptr, size);
}

void AJ_Free(void* p)
{
    free(p);
}

void AJ_MemZeroSecure(void* s, size_t n)
{
    SecureZeroMemory(s, n);
}

#ifndef NDEBUG

/*
 * This is not intended, nor required to be particularly efficient.  If you want
 * efficiency, turn of debugging.
 */
int _AJ_DbgEnabled(const char* module)
{
    char buffer[128];
    char* env;

    strcpy(buffer, "ER_DEBUG_ALL");
    env = getenv(buffer);
    if (env && strcmp(env, "1") == 0) {
        return TRUE;
    }

    strcpy(buffer, "ER_DEBUG_");
    strcat(buffer, module);
    env = getenv(buffer);
    if (env && strcmp(env, "1") == 0) {
        return TRUE;
    }

    return FALSE;
}

#endif

uint16_t AJ_ByteSwap16(uint16_t x)
{
    return _byteswap_ushort(x);
}

uint32_t AJ_ByteSwap32(uint32_t x)
{
    return _byteswap_ulong(x);
}

uint64_t AJ_ByteSwap64(uint64_t x)
{
    return _byteswap_uint64(x);
}

AJ_Status AJ_IntToString(int32_t val, char* buf, size_t buflen)
{
    AJ_Status status = AJ_OK;
    int c = _snprintf(buf, buflen, "%d", val);
    if (c <= 0 || c > buflen) {
        status = AJ_ERR_RESOURCES;
    }
    return status;
}

AJ_Status AJ_InetToString(uint32_t addr, char* buf, size_t buflen)
{
    AJ_Status status = AJ_OK;
    int c = _snprintf(buf, buflen, "%u.%u.%u.%u", (addr & 0xFF000000) >> 24, (addr & 0x00FF0000) >> 16, (addr & 0x0000FF00) >> 8, (addr & 0x000000FF));
    if (c <= 0 || c > buflen) {
        status = AJ_ERR_RESOURCES;
    }
    return status;
}

void AJ_TimeAddOffset(AJ_Time* timerA, uint32_t msec)
{
    uint32_t msecNew;
    if (msec == -1) {
        timerA->seconds = -1;
        timerA->milliseconds = -1;
    } else {
        msecNew = (timerA->milliseconds + msec);
        timerA->seconds = timerA->seconds + (msecNew / 1000);
        timerA->milliseconds = msecNew % 1000;
    }
}


int8_t AJ_CompareTime(AJ_Time timerA, AJ_Time timerB)
{
    if (timerA.seconds == timerB.seconds) {
        if (timerA.milliseconds == timerB.milliseconds) {
            return 0;
        } else if (timerA.milliseconds > timerB.milliseconds) {
            return 1;
        } else {
            return -1;
        }
    } else if (timerA.seconds > timerB.seconds) {
        return 1;
    } else {
        return -1;
    }
}
