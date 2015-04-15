/**
 * @file
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
#define AJ_MODULE TARGET_UTIL
#undef  NDEBUG
#define AJ_DEBUG_RESTRICT AJ_DEBUG_INFO
#define AJ_PRINTF   1
   
#include <TinyCLR_Runtime.h>

#include <stdlib.h>
#include <aj_debug.h>
#include "aj_target.h"
#include "aj_util.h"

uint8_t dbgTARGET_UTIL = 0;

void AJ_Sleep(uint32_t time)
{
    uint32_t timeSeconds = time / 1000;
    uint32_t timeMicroSeconds = (time % 1000)*1000;
    while (timeSeconds > 0)
    {
        timeSeconds--;
        HAL_Time_Sleep_MicroSeconds_InterruptEnabled(1000000);
    }
    HAL_Time_Sleep_MicroSeconds_InterruptEnabled(timeMicroSeconds);
}

uint32_t AJ_GetElapsedTime(AJ_Time* timer, uint8_t cumulative)
{
    uint32_t elapsed;
    INT64 now = Time_GetTickCount();
    INT64 time;

    time = (INT64) timer->seconds * 1000 + timer->milliseconds;

    elapsed = now - time;

    if (!cumulative) {
        timer->seconds = now / 1000;
        timer->milliseconds = now % 1000;
    }

    return elapsed;
}

void AJ_InitTimer(AJ_Time* timer)
{
    INT64 now = Time_GetTickCount();
    timer->seconds = now / 1000;
    timer->milliseconds = now % 1000;
}

int32_t AJ_GetTimeDifference(AJ_Time* timerA, AJ_Time* timerB)
{
    INT64 timeA = (INT64)timerA->seconds * 1000 + timerA->milliseconds;
    INT64 timeB = (INT64)timerB->seconds * 1000 + timerB->milliseconds;
    int32_t diff = timeB - timeA;
    return diff;
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

// Use .NetMF LWIP socket memory allocator, there is no malloc...
extern "C" {
    void *mem_malloc(size_t size);
    void *mem_calloc(size_t count, size_t size);
    void  mem_free(void *mem);
}

void* AJ_Malloc(size_t sz)
{
    return mem_malloc(sz);
}

void* AJ_Realloc(void* ptr, size_t size)
{
    void* newptr = mem_malloc(size);

    if (newptr)
    {
        // don't know the old size, just copy all...
        memcpy(newptr, ptr, size);
        mem_free(ptr);
    }

    return newptr;
}

void AJ_Free(void* mem)
{
    if (mem) {
        mem_free(mem);
    }
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
char*AJ_GetLine(char*str, size_t num, void*fp)
{
#ifdef BUILD_UTIL
    char*p = fgets(str, num, fp);

    if (p != NULL) {
        size_t last = strlen(str) - 1;
        if (str[last] == '\n') {
            str[last] = '\0';
        }
    }
    return p;
#else
    return 0;
#endif
}

#ifdef BUILD_UTIL
static uint8_t ioThreadRunning = FALSE;
static char cmdline[1024];
static uint8_t consumed = TRUE;
static pthread_t threadId;

void* RunFunc(void* threadArg)
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
#endif

uint8_t AJ_StartReadFromStdIn()
{
#ifdef BUILD_UTIL
    int ret = 0;
    if (!ioThreadRunning) {
        ret = pthread_create(&threadId, NULL, RunFunc, NULL);
        if (ret != 0) {
            AJ_ErrPrintf(("Error: fail to spin a thread for reading from stdin\n"));
        }
        ioThreadRunning = TRUE;
        return TRUE;
    }
#endif
    return FALSE;
}

char* AJ_GetCmdLine(char* buf, size_t num)
{
#ifdef BUILD_UTIL
    if (!consumed) {
        strncpy(buf, cmdline, num);
        buf[num - 1] = '\0';
        consumed = TRUE;
        return buf;
    }
#endif
    return NULL;
}

uint8_t AJ_StopReadFromStdIn()
{
#ifdef BUILD_UTIL
    void* exit_status;
    if (ioThreadRunning) {
        ioThreadRunning = FALSE;
        pthread_join(threadId, &exit_status);
        return TRUE;
    }
#endif
    return FALSE;
}

#ifndef NDEBUG

/*
 * This is not intended, nor required to be particularly efficient.  If you want
 * efficiency, turn of debugging.
 */
int _AJ_DbgEnabled(const char* module)
{
    return TRUE;
}

#endif

uint16_t AJ_ByteSwap16(uint16_t u)
{
    return (u >> 8) | (u << 8);
}

uint32_t AJ_ByteSwap32(uint32_t u)
{
    return (u >> 24) | ((u & 0xff0000UL) >> 8) | ((u & 0xff00UL) << 8) | ((u) << 24);
}

uint64_t AJ_ByteSwap64(uint64_t u)
{
    uint32_t t;
    uint64_t h;
    t = (uint32_t)((u & 0xFFFFFFFF00000000ull) >> 32);
    h = AJ_ByteSwap32(t);
    t = (uint32_t)(u & 0xFFFFFFFFull);
    h |= (((uint64_t)AJ_ByteSwap32(t)) << 32);
    return h;
}





