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

#define AJ_MODULE TARGET_RTOS

#include <stdio.h>
#include <stdlib.h>
#include <aj_debug.h>
#include "aj_target.h"
#include "aj_target_rtos.h"
#include "aj_util.h"
#include "aj_status.h"
#include "mbed.h"
#include "rtos.h"

extern "C" {
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

typedef struct {
    void* data;
} TopOfQueue;

/*
 * The opaque AJ types are now declared with FreeRTOS types
 */
struct AJ_Queue {
    Queue<void, 5>* q;
};

struct AJ_Mutex {
    int m;
};

struct AJ_TaskHandle {
    Thread* t;
};


struct AJ_Queue* AJ_QueueCreate(const char* name) {
    struct AJ_Queue* p = (struct AJ_Queue*)AJ_Malloc(sizeof(struct AJ_Queue));
    if (p) {
        Queue<void, 5>* queue = new Queue<void, 5>();
        p->q = queue;
    }
    return p;
}

void AJ_QueueDelete(struct AJ_Queue* q)
{
    delete q->q;
    AJ_Free(q);
}
/*
 * Place holder for the top of the queue so peek can be implemented
 */
static void* topOfQueue = NULL;

AJ_Status AJ_QueuePeek(struct AJ_Queue* q, void** data)
{
    if (q && q->q) {
        /*
         * The top of queue place holder is empty so we need to get a new one
         * and return the data pointer
         */
        if (topOfQueue == NULL) {
            osEvent event = q->q->get(0);
            if (event.status == osEventMessage) {
                topOfQueue = event.value.p;
                *data = topOfQueue;
            } else {
                return AJ_ERR_NULL;
            }
            /*
             * There is already something there so just return that
             */
        } else {
            *data = topOfQueue;
        }
        return AJ_OK;
    }
    return AJ_ERR_NULL;
}

AJ_Status AJ_QueueReset(struct AJ_Queue* q)
{
    return AJ_OK;
}

AJ_Status AJ_QueuePush(struct AJ_Queue* q, void** data, uint32_t timeout)
{
    if (q && q->q) {
        q->q->put(*data, timeout);
        return AJ_OK;
    }
    return AJ_ERR_NULL;
}

AJ_Status AJ_QueuePushFromISR(struct AJ_Queue* q, void** data)
{
    AJ_QueuePush(q, data, 0);
    return AJ_OK;
}

AJ_Status AJ_QueuePeekFromISR(struct AJ_Queue* q, void** data)
{
    AJ_QueuePeek(q, data);
    return AJ_ERR_NULL;
}

AJ_Status AJ_QueuePull(struct AJ_Queue* q, void** data, uint32_t timeout)
{
    if (q && q->q) {
        /*
         * No peek item place holder so pull normally
         */
        if (topOfQueue == NULL) {
            osEvent event = q->q->get(timeout);
            if (event.status == osEventMessage) {
                *data = event.value.p;
            } else {
                return AJ_ERR_NULL;
            }
            /*
             * There was a place holder so get it and reset it to NULL
             */
        } else {
            *data = topOfQueue;
            topOfQueue = NULL;
        }
        return AJ_OK;
    }
    return AJ_ERR_NULL;
}

uint32_t AJ_MsToTicks(uint32_t ms)
{
    return (ms);
}

/**
 * Nothing in the WSL driver uses mutex's currently so none
 * of the mutex functions are implemented.
 */
struct AJ_Mutex* AJ_MutexCreate(void) {
    return NULL;
}

/**
 * Locks a mutex
 * @param m         The mutex your locking
 * @param timeout   How long to wait if the mutex is locked (ms)
 */
AJ_Status AJ_MutexLock(struct AJ_Mutex* m, uint32_t timeout)
{
    return AJ_ERR_UNKNOWN;
}

AJ_Status AJ_MutexUnlock(struct AJ_Mutex* m)
{
    return AJ_ERR_NULL;
}

void AJ_MutexDelete(struct AJ_Mutex* m)
{
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
    /*
     * Create a new thread. Previous RTOS implementations (FreeRTOS) configured the stack depth in words, not bytes
     * for this reason we must multiply the stack depth by 4 because the higher level implementation assumed this
     */
    Thread* thread = new Thread((void (*)(const void*))task, parameters, osPriorityNormal, stackDepth * 4, NULL);
    if (thread) {
        struct AJ_TaskHandle* th = (struct AJ_TaskHandle*)AJ_Malloc(sizeof(struct AJ_TaskHandle));
        th->t = thread;
        if (handle) {
            *handle = th;
        }
        return AJ_OK;
    }
    AJ_ErrPrintf(("AJ_CreateTask(): Could not create task\n"));
    return AJ_ERR_NULL;
}

AJ_Status AJ_DestroyTask(struct AJ_TaskHandle* handle)
{
    handle->t->terminate();
    if (handle->t) {
        delete handle->t;
    }
    return AJ_OK;
}


AJ_Status AJ_SuspendTask(struct AJ_TaskHandle* handle)
{
    handle->t->signal_wait(0x1);
    return AJ_OK;
}

AJ_Status AJ_ResumeTask(struct AJ_TaskHandle* handle, uint8_t inISR)
{
    handle->t->signal_set(0x1);
    return AJ_OK;
}


void AJ_StartScheduler(void)
{
    /*
     * After we create our main task (AllJoyn task) we can't return
     * out of main so we just have to loop forever.
     */
    while (1);
}

void AJ_YieldCurrentTask(void)
{
    Thread::yield();
}

void AJ_EnterCriticalRegion(void)
{
    __disable_irq();
}

void AJ_LeaveCriticalRegion(void)
{
    __enable_irq();
}

void AJ_PlatformInit(void)
{
    _AJ_PlatformInit();
}

void AJ_Sleep(uint32_t time)
{
    Thread::wait(time);
}

extern uint32_t os_time;

uint32_t AJ_GetElapsedTime(AJ_Time* timer, uint8_t cumulative)
{
    uint32_t elapsed;
    uint32_t now_msec = os_time;
    uint32_t now_sec = os_time / 1000;         //Get the seconds
    now_msec = now_msec - (now_sec * 1000);
    elapsed = (1000 * (now_sec - timer->seconds)) + (now_msec - timer->milliseconds);
    if (!cumulative) {                  //Timer has not been initialized
        timer->seconds = now_sec;
        timer->milliseconds = now_msec;
    }
    return elapsed;
}

void AJ_InitTimer(AJ_Time* timer)
{
    uint32_t seconds = os_time / 1000;
    uint32_t msec = os_time - (seconds * 1000);
    timer->seconds = seconds;
    timer->milliseconds = msec;
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
    if ((int32_t)msec == -1) {
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

/*
 * AJ_Malloc, AJ_Free, and AJ_Realloc must be wrapped with
 * AJ_Enter/LeaveCriticalRegion as to not get interrupted.
 * Interruptions in the middle of malloc/free can result in
 * duplicate pointers being returned from different malloc calls.
 */
void* AJ_Malloc(size_t sz)
{
    AJ_EnterCriticalRegion();
    void* ptr = malloc(sz);
    AJ_LeaveCriticalRegion();
    return ptr;
}

void AJ_Free(void* mem)
{
    if (mem) {
        AJ_EnterCriticalRegion();
        free(mem);
        AJ_LeaveCriticalRegion();
    }
}


void* AJ_Realloc(void* ptr, size_t size)
{
    void* ptrNew;
    AJ_EnterCriticalRegion();
    ptrNew = realloc(ptr, size);
    AJ_LeaveCriticalRegion();
    return ptrNew;
}

void AJ_MemZeroSecure(void* s, size_t n)
{
    volatile unsigned char* p = s;
    while (n--) *p++ = '\0';
    return;
}

uint16_t AJ_EphemeralPort(void)
{
    uint16_t random = rand() & 0xFFFF;
    return 49152 + random % (65535 - 49152);
}

#ifndef NDEBUG

/*
 * This is not intended, nor required to be particularly efficient.  If you want
 * efficiency, turn off debugging.
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

}
