/**
 * @file RTOS specific implementation
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

/******************************************************************************
 * Any time in this file there is a comment including FreeRTOS or calling a
 * FreeRTOS API, note that the API associated with it may be subject to the
 * FreeRTOS GPL with exception license copied here:
 * http://www.freertos.org/license.txt :

 * The FreeRTOS.org source code is licensed by the modified GNU General Public
 * License (GPL) text provided below.  The FreeRTOS download also includes
 * demo application source code, some of which is provided by third parties
 * AND IS LICENSED SEPARATELY FROM FREERTOS.ORG.
 * For the avoidance of any doubt refer to the comment included at the top
 * of each source and header file for license and copyright information.
 ******************************************************************************/

#define AJ_MODULE TARGET_RTOS

#include <stdio.h>
#include <stdlib.h>
#include <aj_debug.h>
#include "aj_target.h"
#include "aj_target_rtos.h"
#include "aj_util.h"
#include "aj_status.h"


/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgTARGET_RTOS = 0;
#endif

/*
 * Temporary queue and item size
 *  - Still need to determine how big the queue should be
 *  - What type of structure we are sending -> to find the size
 */
#define QUEUE_SIZE 5
#define ITEM_SIZE sizeof(void*)

uint32_t AJ_MsToTicks(uint32_t ms);

/*
 * The opaque AJ types are now declared with FreeRTOS types
 */
struct AJ_Queue {
    xQueueHandle q;
};

struct AJ_Mutex {
    xSemaphoreHandle m;
};

struct AJ_TaskHandle {
    xTaskHandle t;
};


struct AJ_Queue* AJ_QueueCreate(const char* name) {
    struct AJ_Queue* p = (struct AJ_Queue*)AJ_Malloc(sizeof(struct AJ_Queue));
    if (p) {
        p->q = xQueueCreate(QUEUE_SIZE, ITEM_SIZE);
        vQueueAddToRegistry(p->q, (signed char*)name);
    }
    return p;
}
void AJ_QueueDelete(struct AJ_Queue* q)
{
    vQueueUnregisterQueue(q->q);
    vQueueDelete(q->q);
}
AJ_Status AJ_QueuePeek(struct AJ_Queue* q, void* data)
{
    uint8_t ret;
    if (q && q->q) {
        ret = xQueuePeek(q->q, data, 0);
        if (ret == pdTRUE) {
            return AJ_OK;
        }
    }
    return AJ_ERR_NULL;
}
AJ_Status AJ_QueueReset(struct AJ_Queue* q)
{
    if (q && q->q) {
        xQueueReset(q->q);
    }
    return AJ_OK;
}
AJ_Status AJ_QueuePush(struct AJ_Queue* q, void* data, uint32_t timeout)
{
    uint8_t ret;
    if (q && q->q && data) {
        ret = xQueueSend(q->q, data, AJ_MsToTicks(timeout));
        if (ret == errQUEUE_FULL) {
            return AJ_ERR_RESOURCES;
        }
        return AJ_OK;
    }
    return AJ_ERR_NULL;
}
AJ_Status AJ_QueuePushFromISR(struct AJ_Queue* q, void* data)
{
    uint8_t hasWoken;
    uint8_t ret;
    if (q && q->q && data) {
        ret = xQueueSendToFrontFromISR(q->q, data, (long int*)&hasWoken);
    }
    return AJ_OK;
}
AJ_Status AJ_QueuePeekFromISR(struct AJ_Queue* q, void* data)
{
    uint8_t ret;
    if (q && q->q) {
        ret = xQueuePeekFromISR(q->q, data);
        if (ret) {
            return AJ_OK;
        }
    }
    return AJ_ERR_NULL;
}
AJ_Status AJ_QueuePull(struct AJ_Queue* q, void* data, uint32_t timeout)
{
    uint8_t ret;
    if (q && q->q) {
        ret = xQueueReceive(q->q, data, AJ_MsToTicks(timeout));
        if (ret) {
            return AJ_OK;
        }
        return AJ_ERR_RESOURCES;
    }
    return AJ_ERR_NULL;
}
uint32_t AJ_MsToTicks(uint32_t ms)
{
    return (ms);
}
/**
 * Create a mutex. AllJoyn stores the mutex as null pointer
 * in the AJ_Mutex structure so if changing to another RTOS
 * this pointer can easily point to a new type of structure
 */
struct AJ_Mutex* AJ_MutexCreate(void) {
    struct AJ_Mutex* mutex = (struct AJ_Mutex*)AJ_Malloc(sizeof(struct AJ_Mutex));
    if (mutex) {
        mutex->m = xSemaphoreCreateBinary();
        xSemaphoreGive(mutex->m);
    }
    return mutex;
}
/**
 * Locks a mutex
 * @param m         The mutex your locking
 * @param timeout   How long to wait if the mutex is locked (ms)
 */
AJ_Status AJ_MutexLock(struct AJ_Mutex* m, uint32_t timeout)
{
    uint8_t ret;
    if (m->m) {
        ret = xSemaphoreTake(m->m, AJ_MsToTicks(timeout));
        if (ret) {
            return AJ_OK;
        }
        return AJ_ERR_TIMEOUT;
    }
    return AJ_ERR_UNKNOWN;
}
AJ_Status AJ_MutexUnlock(struct AJ_Mutex* m)
{
    uint8_t ret;
    if (m->m) {
        ret = xSemaphoreGive(m->m);
        if (ret) {
            return AJ_OK;
        }
        /* The semaphore was not obtained correctly */
        return AJ_ERR_DISALLOWED;
    }
    return AJ_ERR_NULL;
}
void AJ_MutexDelete(struct AJ_Mutex* m)
{
    vSemaphoreDelete(m->m);
}
/**
 * @param task       the function you want to execute as this task
 * @param name       name of the task
 * @param stackDepth size of the stack for this task
 * @param parameters any parameters you want to pass in
 */
AJ_Status AJ_CreateTask(void (*task)(void*),
                        const signed char* const name,
                        unsigned short stackDepth,
                        void* parameters,
                        uint8_t priority,
                        struct AJ_TaskHandle** handle)
{
    int status;
    if (handle) {
        struct AJ_TaskHandle* th = (struct AJ_TaskHandle*)AJ_Malloc(sizeof(struct AJ_TaskHandle));
        if (th) {
            status = xTaskCreate(task, name, stackDepth, parameters, priority, &th->t);
            *handle = th;
        } else {
            return AJ_ERR_RESOURCES;
        }
    } else {
        status = xTaskCreate(task, name, stackDepth, parameters, priority, NULL);
    }
    if (status == pdPASS) {
        return AJ_OK;
    } else {
        return AJ_ERR_UNKNOWN;
    }
}

AJ_Status AJ_DestroyTask(struct AJ_TaskHandle* handle)
{
    vTaskDelete(handle->t);
    return AJ_OK;
}


AJ_Status AJ_SuspendTask(struct AJ_TaskHandle* handle)
{
    vTaskSuspend(handle);
    return AJ_OK;
}

AJ_Status AJ_ResumeTask(struct AJ_TaskHandle* handle, uint8_t inISR)
{
    if (inISR) {
        xTaskResumeFromISR(handle->t);
    } else {
        vTaskResume(handle->t);
    }
    return AJ_OK;
}


void AJ_StartScheduler(void)
{
    vTaskStartScheduler();
}

void AJ_YieldCurrentTask(void)
{
    taskYIELD();
}

void AJ_EnterCriticalRegion(void)
{
    taskENTER_CRITICAL();
}

void AJ_LeaveCriticalRegion(void)
{
    taskEXIT_CRITICAL();
}
void AJ_PlatformInit(void)
{
    _AJ_PlatformInit();
}
/*
   AJ_Status AJ_SuspendWifi(uint32_t msec)
   {
    return AJ_OK;
   }
 */

void AJ_Sleep(uint32_t time)
{
    /* This function does not work until AJ_StartScheduler is called */
    const portTickType delay = (time / portTICK_RATE_MS);
    vTaskDelay(delay);

}

uint32_t AJ_GetElapsedTime(AJ_Time* timer, uint8_t cumulative)
{
    uint32_t elapsed;
    uint32_t now_msec = xTaskGetTickCount() / portTICK_RATE_MS;
    uint32_t now_sec = now_msec / 1000;         //Get the seconds
    now_msec = now_msec - (now_sec * 1000);     //Get the additional msec's

    elapsed = (1000 * (now_sec - timer->seconds)) + (now_msec - timer->milliseconds);
    if (!cumulative) {                  //Timer has not been initialized
        timer->seconds = now_sec;
        timer->milliseconds = now_msec;
    }
    return elapsed;
}
void AJ_InitTimer(AJ_Time* timer)
{
    uint32_t now_msec = xTaskGetTickCount() / portTICK_RATE_MS;
    uint32_t now_sec = now_msec / 1000;         //Get the seconds
    now_msec = now_msec - (now_sec * 1000);     //Get the additional msec's
    timer->seconds = now_sec;
    timer->milliseconds = now_msec;
}

int32_t AJ_GetTimeDifference(AJ_Time* timerA, AJ_Time* timerB)
{
    int32_t diff;

    diff = (1000 * (timerA->seconds - timerB->seconds)) + (timerA->milliseconds - timerB->milliseconds);
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

uint64_t AJ_DecodeTime(char* der, char* fmt)
{
    return 0;
}

void* AJ_Malloc(size_t sz)
{
    return pvPortMalloc(sz);
}

void AJ_Free(void* mem)
{
    if (mem) {
        vPortFree(mem);
    }
}

void AJ_MemZeroSecure(void* s, size_t n)
{
    volatile unsigned char* p = s;
    while (n--) *p++ = '\0';
    return;
}

void* AJ_Realloc(void* ptr, size_t size)
{
    void* ptrNew;
#ifdef AJ_HEAP4
    if (!ptr) {
        return AJ_Malloc(size);
    }
    ptrNew = AJ_Malloc(size);
    if (ptrNew) {
        memcpy(ptrNew, ptr, size);
        AJ_Free(ptr);
    }
#else
    vTaskSuspendAll();
    ptrNew = realloc(ptr, size);
    xTaskResumeAll();
#endif
    return ptrNew;
}


void AJ_RandBytes(uint8_t* random, uint32_t len)
{
    AJ_SeedRNG();
    while (len) {
        *random = AJ_SeedRNG() & 0xFF;
        len -= 1;
        random += 1;
    }
}
uint16_t AJ_EphemeralPort(void)
{
    AJ_SeedRNG();
    uint16_t random = AJ_SeedRNG() & 0xFFFF;
    return 49152 + random % (65535 - 49152);
}

/*
 *  This function is called when a malloc failure is detected
 */
void vApplicationMallocFailedHook(void)
{
    AJ_ASSERT(FALSE);
    while (1) {
    }
    ;
}

/*
 *  This function is called when a stack overflow is detected
 */
void vApplicationStackOverflowHook(xTaskHandle pxTask, signed char*pcTaskName)
{
    AJ_ASSERT(FALSE);
    while (1) {
    }
    ;
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

