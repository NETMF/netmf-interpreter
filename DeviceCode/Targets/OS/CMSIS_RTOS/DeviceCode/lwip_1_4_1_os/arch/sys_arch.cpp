/*
 * Copyright (c) 2001-2003 Swedish Institute of Computer Science.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice,
 *    this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
 * SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
 * OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING
 * IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 * OF SUCH DAMAGE.
 *
 * This file is part of the lwIP TCP/IP stack.
 *
 * Author: Adam Dunkels <adam@sics.se>
 *
 */

// Port to CMSIS-RTOS and the .NET Micro Framework
// Copyright (c) Microsoft Open Technologies
#include <tinyhal.h>
#include <cmsis_os_cpp.h>
#include <ctime>
#include <mutex>
#include <new>

#include <lwip/opt.h>
#include <lwip/arch.h>
#include <lwip/stats.h>
#include <lwip/debug.h>
#include <lwip/sys.h>
#include <lwip/tcpip.h>
#include "sysarch_timeout.h"

using namespace lwIP::SysArch;
using namespace std;

#ifndef UINT32_MAX
#define UINT32_MAX  (0xffffffff)
#endif

#ifndef SYS_SEM_COUNT
#define SYS_SEM_COUNT   4
#endif

#ifndef SYS_THREAD_COUNT
#define SYS_THREAD_COUNT 6
#endif

#ifndef SYS_MBOX_COUNT
#define SYS_MBOX_COUNT  16
#endif
#ifndef SYS_MBOX_SIZE
#define SYS_MBOX_SIZE   32
#endif

struct os_semaphore_cb_t
{
    uint32_t cb[ 2 ];
};

struct os_semaphore_t
{
    os_semaphore_cb_t cb;
    osSemaphoreDef_t  def;
};

struct os_messageQ_cb_t
{
    uint32_t cb[ 4 + SYS_MBOX_SIZE ];
};

struct os_messageQ_t
{
    os_messageQ_cb_t cb;
    osMessageQDef_t  def;
};

osPoolDef( sys_pool_sem, SYS_SEM_COUNT, os_semaphore_t );
osPoolId  sys_pool_sem;

osPoolDef( sys_pool_thrd, SYS_THREAD_COUNT, osThreadDef_t );
osPoolId  sys_pool_thrd;

osPoolDef( sys_pool_mbox, SYS_MBOX_COUNT, os_messageQ_t );
osPoolId  sys_pool_mbox;


namespace
{
//#if SYS_LIGHTWEIGHT_PROT
    recursive_mutex SysArchLock;
    typedef unique_lock<recursive_mutex> ScopedLock;
//#endif
    
    // timeout support for CMSIS-RTOS
    typedef Timeout<MsOpenTech::CMSIS_RTOS::Timer> OsTimeout;
    HAL_DblLinkedList<OsTimeout> MasterTimeoutQueue;
	
    osPoolDef( sys_pool_timeout, MEMP_NUM_SYS_TIMEOUT, OsTimeout );
    osPoolId  sys_pool_timeout;
}
extern "C"
{
    void sys_signal_sock_event()
    {
        Events_Set(SYSTEM_EVENT_FLAG_SOCKET);
    }
	
    void sys_init_timing( )
    {
    }

    u32_t sys_jiffies( )
    {
        return ( u32_t )HAL_Time_CurrentTime();
    }

    u32_t sys_now( )
    {
        u32_t now = HalTimeToMilliseconds( HAL_Time_CurrentTime() );
        return now > UINT32_MAX ? UINT32_MAX : now;
    }

#if SYS_LIGHTWEIGHT_PROT
    u32_t sys_arch_protect( )
    {
        SysArchLock.lock();
        return 0;
    }

    void sys_arch_unprotect( u32_t pval )
    {
        LWIP_UNUSED_ARG( pval );
        SysArchLock.unlock();
    }
#else
    u32_t sys_arch_protect( )
    {
        return 0;
    }

    void sys_arch_unprotect( u32_t pval )
    {
        LWIP_UNUSED_ARG( pval );
    }
#endif /* SYS_LIGHTWEIGHT_PROT */

    void sys_init( )
    {
        sys_pool_sem = osPoolCreate( osPool( sys_pool_sem ) );
        sys_pool_mbox = osPoolCreate( osPool( sys_pool_mbox ) );
#ifndef CMSIS_RTOS_COMPATIBLE
        sys_pool_thrd = osPoolCreate( osPool( sys_pool_thrd ) );
#endif

        sys_pool_timeout = osPoolCreate( osPool( sys_pool_timeout ) );
        MasterTimeoutQueue.Initialize();
    }
	
    err_t sys_sem_new( sys_sem_t *sem, u8_t count )
    {
        os_semaphore_t *ptr;
        osSemaphoreId   id;

        ptr = (os_semaphore_t *) osPoolCAlloc( sys_pool_sem );
        if( ptr != NULL )
        {
            ptr->def.semaphore = ptr;
            id = osSemaphoreCreate( &ptr->def, count );
            if(id != NULL)
            {
                *sem = id;
                return ERR_OK;
            }
        }
        /* failed to allocate memory... */
        SYS_STATS_INC( sem.err );
        sem = SYS_SEM_NULL;
        return ERR_MEM;
    }

    void sys_sem_free( sys_sem_t *sem )
    {
        osSemaphoreDelete( *sem );
        osPoolFree( sys_pool_sem, *sem );
    }

    u32_t sys_arch_sem_wait( sys_sem_t *sem, u32_t timeout )
    {
        int64_t starttime, endtime;
        LWIP_ASSERT( "sem != NULL", sem != NULL );
        //LWIP_ASSERT( "sem != INVALID_HANDLE_VALUE", sem != INVALID_HANDLE_VALUE );
        starttime = HAL_Time_CurrentTime( );
        if( timeout == 0 )
        {
            /* wait infinite */
            osSemaphoreWait( *sem, osWaitForever );
            endtime = HAL_Time_CurrentTime( );
            /* return the time we waited for the sem */
            int64_t waittime = endtime - starttime;
            return  waittime > UINT32_MAX ? UINT32_MAX : waittime;
        }
        int count = osSemaphoreWait( *sem, timeout );
        if( count == 0 )
            return SYS_ARCH_TIMEOUT;

        endtime = HAL_Time_CurrentTime( );
        /* return the time we waited for the sem */
        int64_t waittime = endtime - starttime;
        return  waittime > UINT32_MAX ? UINT32_MAX : waittime;
    }

    void sys_sem_signal( sys_sem_t *sem )
    {
        osSemaphoreRelease( *sem );
    }

#ifdef CMSIS_RTOS_COMPATIBLE

#define LWIP_THREADS 3  /* Number of lwIP user threads */

    static lwip_thread_fn tcpip_thread_fn;
    static lwip_thread_fn slipif_thread_fn;
    static lwip_thread_fn ppp_thread_fn;
    static lwip_thread_fn lwip_threads_fn[ LWIP_THREADS ];
    static uint32_t       lwip_threads_n = 0;
    static uint32_t       lwip_threads_i = 0;

    static void tcpip_thread( void const *arg )
    {
        tcpip_thread_fn( const_cast< void * >( arg ) );
    }

    static void slipif_thread( void const *arg )
    {
        slipif_thread_fn( const_cast< void * >( arg ) );
    }

    static void ppp_thread( void const *arg )
    {
        ppp_thread_fn( const_cast< void * >( arg ) );
    }

    static void lwip_threads( void const *arg )
    {
        lwip_threads_fn[ lwip_threads_i++ ]( const_cast< void * >( arg ) );
    }

    osThreadDef( tcpip_thread, osPriorityNormal, 1, TCPIP_THREAD_STACKSIZE );
    osThreadDef( slipif_thread, osPriorityNormal, 1, SLIPIF_THREAD_STACKSIZE );
    osThreadDef( ppp_thread, osPriorityNormal, 1, TCPIP_THREAD_STACKSIZE );
    osThreadDef( lwip_threads, osPriorityNormal, LWIP_THREADS, DEFAULT_THREAD_STACKSIZE );

    sys_thread_t sys_thread_new( const char *name, lwip_thread_fn function, void *arg, int stacksize, int prio )
    {
        osThreadId id;
        LWIP_UNUSED_ARG( stacksize );
        LWIP_UNUSED_ARG( prio );

        if( strcmp( name, TCPIP_THREAD_NAME ) == 0 )
        {
            tcpip_thread_fn = function;
            id = osThreadCreate( osThread( tcpip_thread ), arg );
            return id;
        }
        if( strcmp( name, SLIPIF_THREAD_NAME ) == 0 )
        {
            slipif_thread_fn = function;
            id = osThreadCreate( osThread( slipif_thread ), arg );
            return id;
        }
        if( strcmp( name, PPP_THREAD_NAME ) == 0 )
        {
            ppp_thread_fn = function;
            id = osThreadCreate( osThread( ppp_thread ), arg );
            return id;
        }
        if( lwip_threads_n < LWIP_THREADS )
        {
            lwip_threads_fn[ lwip_threads_n++ ] = function;
            id = osThreadCreate( osThread( lwip_threads ), arg );
            return id;
        }

        return NULL;
    }
#else


    sys_thread_t sys_thread_new( const char *name, lwip_thread_fn function, void *arg, int stacksize, int prio )
    {
        osThreadDef_t *def;
        osThreadId     id;
        LWIP_UNUSED_ARG( name );
        LWIP_UNUSED_ARG( prio );

        def = (osThreadDef_t *) osPoolAlloc( sys_pool_thrd );
        if( def == NULL )
        {
            return NULL;
        }
        def->pthread = ( os_pthread )function;
        def->tpriority = osPriorityNormal;
        def->stacksize = stacksize;
        def->instances = 1;
        id = osThreadCreate( def, arg );

        return id;
    }

#endif /* CMSIS_RTOS_COMPATIBLE */

    err_t sys_mbox_new( sys_mbox_t *mbox, int size )
    {
        os_messageQ_t *ptr;
        osMessageQId   id;

        if( size > SYS_MBOX_SIZE )
        {
            return ERR_MEM;
        }

        ptr = (os_messageQ_t *) osPoolCAlloc( sys_pool_mbox );
        if( ptr == NULL )
        {
            return ERR_MEM;
        }
        ptr->def.queue_sz = SYS_MBOX_SIZE;
        ptr->def.pool = ptr;
        id = osMessageCreate( &ptr->def, NULL );
        if( id == NULL )
        {
            return ERR_MEM;
        }
        *mbox = id;

        return ERR_OK;
    }

    void sys_mbox_free( sys_mbox_t *mbox )
    {
        osPoolFree( sys_pool_mbox, *mbox );
    }

    void sys_mbox_post( sys_mbox_t *q, void *msg )
    {
        osMessagePut( *q, ( uint32_t )msg, osWaitForever );
    }

    err_t sys_mbox_trypost( sys_mbox_t *q, void *msg )
    {
        osStatus status;

        status = osMessagePut( *q, ( uint32_t )msg, 0 );
        if( status != osOK )
        {
            return ERR_MEM;
        }
        return ERR_OK;
    }

    u32_t sys_arch_mbox_fetch( sys_mbox_t *q, void **msg, u32_t timeout )
    {
        osEvent  event;
        uint32_t tick;

        tick = osKernelSysTick( );
        if( timeout == 0 )
        {
            event = osMessageGet( *q, osWaitForever );
        }
        else
        {
            event = osMessageGet( *q, timeout );
        }
        if( event.status != osEventMessage )
        {
            return SYS_ARCH_TIMEOUT;
        }
        if( msg != NULL )
        {
            *msg = event.value.p;
        }
        tick = 1000 * ( uint64_t )( ( osKernelSysTick( ) - tick ) ) / osKernelSysTickFrequency;

        return tick;
    }

    u32_t sys_arch_mbox_tryfetch( sys_mbox_t *q, void **msg )
    {
        osEvent event;

        event = osMessageGet( *q, 0 );
        if( event.status != osEventMessage )
        {
            return SYS_MBOX_EMPTY;
        }
        if( msg != NULL )
        {
            *msg = event.value.p;
        }

        return 0;
    }

    //------------------------------------------------------------------------
    // Hoisted out of original timers.c

    /**
     * Create a one-shot timer (aka timeout). Timeouts are processed in the
     * following cases:
     * - while waiting for a message using sys_timeouts_mbox_fetch()
     * - by calling sys_check_timeouts() (NO_SYS==1 only)
     *
     * @param msecs time in milliseconds after that the timer should expire
     * @param handler callback function to call when msecs have elapsed
     * @param arg argument to pass to the callback function
     */
#if LWIP_DEBUG_TIMERNAMES
    void sys_timeout_debug( u32_t msecs, sys_timeout_handler handler, void *arg, const char* handler_name )
#else /* LWIP_DEBUG_TIMERNAMES */
    void sys_timeout( u32_t msecs, sys_timeout_handler handler, void *arg )
#endif /* LWIP_DEBUG_TIMERNAMES */
    {
        ScopedLock scopedLock( SysArchLock );
		
        void *ptr = osPoolCAlloc( sys_pool_timeout );
        OsTimeout* timeout = new(ptr) OsTimeout( handler, arg );
        MasterTimeoutQueue.LinkAtBack(timeout);
        //LWIP_ASSERT("FAILED to allocate timeout object", resultPair.second );
        timeout->Start( msecs );
    }

    // create a periodic timer that triggers on a fixed interval (period)
    void sys_periodic_timeout( u32_t msecs, sys_timeout_handler handler, void *arg )
    {
        ScopedLock scopedLock( SysArchLock );
		
        void *ptr = osPoolCAlloc( sys_pool_timeout );
        OsTimeout* timeout = new(ptr) OsTimeout( handler, arg );
        MasterTimeoutQueue.LinkAtBack(timeout);
        //LWIP_ASSERT("FAILED to allocate periodic timeout object", resultPair.second );
        timeout->Start( msecs, true );
    }

    /**
     * Go through timeout list (for this task only) and remove the first matching
     * entry, even though the timeout has not triggered yet.
     *
     * @note This function only works as expected if there is only one timeout
     * calling 'handler' in the list of timeouts.
     *
     * @param handler callback function that would be called by the timeout
     * @param arg callback argument that would be passed to handler
     */
    void sys_untimeout( sys_timeout_handler handler, void *arg )
    {
        ScopedLock scopedLock( SysArchLock );
		
        OsTimeout* node = MasterTimeoutQueue.FirstValidNode();
        while( node != NULL && node->Compare(handler,arg))
        {
            node = node->Next();
        }
		
        LWIP_ASSERT("sys_untimeout: Failed to find requested timeout!", node != NULL );
        
        node->Unlink();
        node->Cancel();
		
		osPoolFree(sys_pool_timeout,node);
    }

    /**
     * Wait (forever) for a message to arrive in an mbox.
     * While waiting, timeouts are processed.
     *
     * @param mbox the mbox to fetch the message from
     * @param msg the place to store the message
     */
    void sys_timeouts_mbox_fetch( sys_mbox_t *mbox, void **msg )
    {
        sys_arch_mbox_fetch( mbox, msg, 0 );
    }

    void sys_timeouts_uninit(void)
    {
        ScopedLock scopedLock(SysArchLock);

        OsTimeout* node;
        while ((node = MasterTimeoutQueue.FirstValidNode()) != NULL)
        {
            node->Unlink();
            node->Cancel();

            osPoolFree(sys_pool_timeout, node);
        }
    }
}
