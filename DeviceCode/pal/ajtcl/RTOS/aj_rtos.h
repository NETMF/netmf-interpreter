/**
 * @file RTOS abstraction layer contains declarations for all the needed
 * RTOS primitives that should be implemented for a specific RTOS.
 */
/******************************************************************************
 * Copyright (c) 2014, AllSeen Alliance. All rights reserved.
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
#ifndef AJ_RTOS_H_
#define AJ_RTOS_H_

#include "aj_target.h"
#include "aj_status.h"
#include "aj_target_rtos.h"

#ifdef __cplusplus
extern "C" {
#endif

/*
 * These are opaque types that hold RTOS specific types;
 */
struct AJ_Queue;
struct AJ_TaskHandle;
struct AJ_Mutex;

uint32_t AJ_MsToTicks(uint32_t ms);

/**
 * create a queue
 *
 * @param name          Name of the queue to create
 *
 * @return              pointer to an AJ_Queue object
 */
struct AJ_Queue* AJ_QueueCreate(const char* name);

/**
 * delete a queue
 *
 * @param q             pointer to the AJ_Queue to delete, deletes the opaque type as well
 *
 */
void AJ_QueueDelete(struct AJ_Queue* q);
/**
 * Peek at an item in a queue
 *
 * @param q             pointer to the AJ_Queue structure
 * @param data          location for the item in the queue to go
 *
 * @return              AJ_OK if an item was in the queue
 *                      AJ_ERR_NULL if the queue was empty
 */
AJ_Status AJ_QueuePeek(struct AJ_Queue* q, void* data);
/**
 * Reset or flush a queue
 *
 * @param q             The AJ_Queue structure you want to empty
 *
 * @return              This will always return AJ_OK
 */
AJ_Status AJ_QueueReset(struct AJ_Queue* q);
/**
 * push an item on to a queue, wait at most until timeout
 *
 * @param q             pointer to the AJ_Queue to push
 * @param data          data to push onto the queue
 * @param timeout       amount of time in milliseconds to wait to push an item
 *
 * @return              AJ_OK item was pushed
 *                      AJ_ERR_RESOURCES item couldn't be pushed within timeout
 */
AJ_Status AJ_QueuePush(struct AJ_Queue* q, void* data, uint32_t timeout);
/**
 * Push an item to the front of a queue from an ISR
 * note: This function pushes to the front of the queue (used for immediate signaling)
 *
 * @param q             pointer to the queue
 * @param data          pointer to the data you adding to the queue
 */
AJ_Status AJ_QueuePushFromISR(struct AJ_Queue* q, void* data);
/**
 * peek at an item in the queue from an ISR
 *
 * @param q             pointer to the queue
 * @param data          location for the item in the queue to go
 *
 * @return              AJ_OK if the peek was successful
 *                      AJ_ERR_NULL if there was no item to peek at
 */
AJ_Status AJ_QueuePeekFromISR(struct AJ_Queue* q, void* data);
/**
 * pull an item from a queue, wait at most until timeout
 *
 * @param q             pointer to the AJ_Queue to pull
 * @param data          data to pull from the queue
 * @param timeout       amount of time in milliseconds to wait to pull an item
 *
 * @return              AJ_OK item was pulled
 *                      AJ_ERR_RESOURCES item couldn't be pulled before timeout
 */
AJ_Status AJ_QueuePull(struct AJ_Queue* q, void* data, uint32_t timeout);

/*
 * Create a mutex
 */
struct AJ_Mutex* AJ_MutexCreate(void);
/*
 * Try and take a lock on a mutex
 *
 * @param m             Pointer to the mutex created by AJ_MutexCreate()
 * @param timeout       How long to block if the mutex is already locked
 */
AJ_Status AJ_MutexLock(struct AJ_Mutex* m, uint32_t timeout);

/*
 * Unlock a mutex
 *
 * @param m             Mutex pointer
 */
AJ_Status AJ_MutexUnlock(struct AJ_Mutex* m);
/*
 * Delete a muted
 *
 * @param m             Mutex to delete
 */
void AJ_MutexDelete(struct AJ_Mutex* m);

/*
 * Create a task to be started
 *
 * @param task          Function pointer to where you want the task to start
 * @param name          Name for the task
 * @param stackDepth    How big you want the stack to be
 * @param parameters    Any parameters to pass into the task when its started
 * @param priority      Priority of the task
 * @param handle        A context for referencing the task
 */
AJ_Status AJ_CreateTask(void (*task)(void*),
                        const signed char* const name,
                        unsigned short stackDepth,
                        void* parameters,
                        uint8_t priority,
                        struct AJ_TaskHandle** handle);
/*
 * Remove a task from running
 *
 * @param handle        The handle that was set by AJ_CreateTask()
 */
AJ_Status AJ_DestroyTask(struct AJ_TaskHandle* handle);

/*
 * suspend a task from running
 *
 * @param handle        The handle that was set by AJ_CreateTask()
 */
AJ_Status AJ_SuspendTask(struct AJ_TaskHandle* handle);

/*
 * resume a suspended task, possibly from an ISR
 *
 * @param handle        The handle that was set by AJ_CreateTask()
 * @param inISR         is this being called from an interrupt service routine?
 */
AJ_Status AJ_ResumeTask(struct AJ_TaskHandle* handle, uint8_t inISR);


/*
 * Start the RTOS specific scheduler
 */
void AJ_StartScheduler(void);

/**
 * force the current task to yield its timeslice
 */
void AJ_YieldCurrentTask(void);

/**
 * Disable interrupts for a time
 */
void AJ_EnterCriticalRegion(void);

/**
 * exit the critical region and enable interrupts if needed
 */
void AJ_LeaveCriticalRegion(void);

/**
 * Do any platform specific initialization
 *  - Clock init, serial debug, SPI etc.
 */
void AJ_PlatformInit(void);

/**
 * Pre-AllJoyn entry function. This function does all the network initialization
 * that AllJoyn needs to run. Before calling AJ_Main() a network needs to be setup
 * which entails connecting to an access point (or softAP) and getting an IP address
 */
void AllJoyn_Start(unsigned long arg);

#ifdef __cplusplus
}
#endif

#endif /* RTOS_H_ */
