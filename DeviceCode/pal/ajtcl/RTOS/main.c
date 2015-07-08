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

#include "aj_target_platform.h"
#include "aj_target.h"
#include "aj_debug.h"
#include "aj_target_rtos.h"

static void main_task(void* parameters)
{
    AJ_PlatformInit();
    AJ_AlwaysPrintf((" ==============================================\n"));
    AJ_AlwaysPrintf(("||       Alljoyn Thin Client + FreeRTOS       ||\n"));
    AJ_AlwaysPrintf((" ==============================================\n"));
    AllJoyn_Start(0);
    while (1);
}

int main(void)
{
    AJ_CreateTask(main_task, (const signed char*)"AlljoynTask", AJ_WSL_STACK_SIZE, NULL, 2, NULL);
    AJ_StartScheduler();
    return 0;
}

