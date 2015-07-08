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

#include "aj_nvram.h"
#include "aj_target_platform.h"
#include "aj_debug.h"
#include <sys/time.h>

void _AJ_PlatformInit(void)
{
    BoardPrintfInit(115200);
    return;
}
uint16_t AJ_ByteSwap16(uint16_t x)
{
    return ((((x) >> 8) & 0xff) | (((x) & 0xff) << 8));
}

uint32_t swap32(uint32_t x)
{
    return AJ_ByteSwap32(x);
}

uint32_t AJ_ByteSwap32(uint32_t x)
{
    return ((x >> 24) & 0x000000FF) | ((x >> 8) & 0x0000FF00) |
           ((x << 24) & 0xFF000000) | ((x << 8) & 0x00FF0000);
}

uint64_t AJ_ByteSwap64(uint64_t x)
{
    return ((x >> 56) & 0x00000000000000FF) | ((x >> 40) & 0x000000000000FF00) |
           ((x << 56) & 0xFF00000000000000) | ((x << 40) & 0x00FF000000000000) |
           ((x >> 24) & 0x0000000000FF0000) | ((x >>  8) & 0x00000000FF000000) |
           ((x << 24) & 0x0000FF0000000000) | ((x <<  8) & 0x000000FF00000000);
}

uint8_t AJ_SeedRNG(void)
{
    return 1;
}

void _exit(int i)
{
    while (1);
}

int _kill(int pid)
{
    return 1;
}

int _getpid()
{
    return 0;
}

/* Current time (ms) since boot for _gettimeofday */
extern uint32_t os_time;

int _gettimeofday(struct timeval* tv, struct timezone* tz)
{
    if (tv) {
        tv->tv_sec = (os_time / 1000);
        tv->tv_usec = (os_time % 1000) * 1000;
    }
    return 0;
}

