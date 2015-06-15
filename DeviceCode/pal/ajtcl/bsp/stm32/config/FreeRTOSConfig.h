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

#ifndef FREERTOSCONFIG_H_
#define FREERTOSCONFIG_H_

#include <stdint.h>

/*
 * Configuration options
 */
#define configUSE_PREEMPTION                    1
#define configUSE_IDLE_HOOK                     0
#define configUSE_MALLOC_FAILED_HOOK            1
#define configUSE_TICK_HOOK                     0
#define configCPU_CLOCK_HZ                      (168000000UL)
#define configTICK_RATE_HZ                      ((portTickType) 1000)
#define configMAX_PRIORITIES                    ((unsigned portBASE_TYPE) 12)
#define configMINIMAL_STACK_SIZE                ((unsigned short) 120)
#define configTOTAL_HEAP_SIZE                   ((size_t) 112471)
#define configMAX_TASK_NAME_LEN                 (10)
#define configUSE_TRACE_FACILITY                1
#define configUSE_STATS_FORMATTING_FUNCTIONS    1
#define configUSE_16_BIT_TICKS                  0
#define configIDLE_SHOULD_YIELD                 1
#define configUSE_MUTEXES                       1
#define configUSE_RECURSIVE_MUTEXES             1
#define configUSE_COUNTING_SEMAPHORES           1
#define configCHECK_FOR_STACK_OVERFLOW          2
#define configQUEUE_REGISTRY_SIZE               8
//#define configGENERATE_RUN_TIME_STATS           1
#define configUSE_CO_ROUTINES                   0
#define configMAX_CO_ROUTINE_PRIORITIES         (2)
#define configUSE_TIMERS                        1
#define configTIMER_TASK_PRIORITY               (configMAX_PRIORITIES - 1)
#define configTIMER_QUEUE_LENGTH                5
#define configTIMER_TASK_STACK_DEPTH            (configMINIMAL_STACK_SIZE * 2)
#define configCOMMAND_INT_MAX_OUTPUT_SIZE       400
/*
 * Define what functions should be included
 */
#define INCLUDE_vTaskPrioritySet                1
#define INCLUDE_uxTaskPriorityGet               1
#define INCLUDE_vTaskDelete                     1
#define INCLUDE_vTaskSuspend                    1
#define INCLUDE_vTaskDelayUntil                 1
#define INCLUDE_vTaskDelay                      1

/*
 * Priority configurations:
 * Cortex-Mx may have its own priority definition and levels
 */
#if defined(__NVIC_PRIO_BITS)
#define PRIORITY_BITS                                   __NVIC_PRIO_BITS
#else
#define PRIORITY_BITS                                   4
#endif

#define LOWEST_PRIORITY                                 15
#define configLIBRARY_MAX_SYSCALL_INTERRUPT_PRIORITY    5
#define configKERNEL_INTERRUPT_PRIORITY                 (LOWEST_PRIORITY << (8 - PRIORITY_BITS))
#define configMAX_SYSCALL_INTERRUPT_PRIORITY            (configLIBRARY_MAX_SYSCALL_INTERRUPT_PRIORITY << (8 - PRIORITY_BITS))

#define configASSERT(x)                                 if ((x) == 0) { taskDISABLE_INTERRUPTS(); while (1); }
#define INCLUDE_MODULE_TEST                             0

#define vPortSVCHandler SVC_Handler
#define xPortPendSVHandler PendSV_Handler
#define xPortSysTickHandler SysTick_Handler



#endif /* FREERTOSCONFIG_H_ */
