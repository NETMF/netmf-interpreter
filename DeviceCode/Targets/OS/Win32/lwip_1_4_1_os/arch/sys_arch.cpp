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
 *         Simon Goldschmidt
 *
 */

// Extensively modified for Win32
// Copyright (c) Microsoft Open Technologies


#include <cstdlib>
#include <cstdio>
#include <cstdint>
#include <tinyhal.h>
#include <windows.h>
#include <ctime>
#include <queue>
#include <functional>
#include <mutex>
#include <set>

#include <lwip/opt.h>
#include <lwip/arch.h>
#include <lwip/stats.h>
#include <lwip/debug.h>
#include <lwip/sys.h>
#include <lwip/tcpip.h>

#include "sysarch_timeout.h"
#include "Win32TimerQueue.h"

using namespace lwIP::SysArch;
using namespace Microsoft::Win32;
using namespace std;

namespace
{
#if SYS_LIGHTWEIGHT_PROT
    recursive_mutex SysArchLock;
    typedef unique_lock<recursive_mutex> ScopedLock;
#endif
    
    // factory to create win32 timers for common timeout template
    struct Win32TimerQueueTimerFactory
    {
        static Timer CreateTimer( sys_timeout_handler handler, void* arg )
        {
            return Timer( TimerOption::ExecuteOnTimerThread, [handler,arg]() { handler( arg ); } );
        }
    };

    // timeout support for win32
    typedef Timeout<Timer, Win32TimerQueueTimerFactory> OsTimeout;

    std::set<OsTimeout> MasterTimeoutQueue;
}

extern "C"
{
    // This is called from sockets.c and api_msg.c to notify the
    // CLR to wakeup managed threads that are pending on network
    // socket status. This, is due to the fact that the CLR is
    // one big native thread that cannot block, as it schedules
    // (and interprets) all managed thread code. However, lwIP
    // is designed where the tcpip/netif APIs will post/dispatch
    // a message to the tcpip_thread and block waiting for a 
    // response, which, in the case of the CLR, would block all
    // managed code execution and native continuations. Ideally
    // lwIP would have an async activity with a callback that 
    // could either signal a semaphore or post a message to 
    // another thread etc... as a continuation. Barring that
    // we use a surgical strike and insert calls to this function
    // in a few key places. This keeps the code churn on lWIP
    // minimal while working around the problem.
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
        auto now = HalTimeToMilliseconds( HAL_Time_CurrentTime() );
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
        sys_init_timing( );
    }

    struct threadlist
    {
        DWORD id;
        struct threadlist *next;
    };

    struct threadlist *lwip_win32_threads = NULL;

    void do_sleep( int ms )
    {
        Sleep( ms );
    }

    err_t sys_sem_new( sys_sem_t *sem, u8_t count )
    {
        HANDLE new_sem = NULL;

        LWIP_ASSERT( "sem != NULL", sem != NULL );

        new_sem = CreateSemaphore( 0, count, 100000, 0 );
        LWIP_ASSERT( "Error creating semaphore", new_sem != NULL );
        if( new_sem != NULL )
        {
            SYS_STATS_INC_USED( sem );
    #if LWIP_STATS && SYS_STATS
            LWIP_ASSERT( "sys_sem_new() counter overflow", lwip_stats.sys.sem.used != 0 );
    #endif /* LWIP_STATS && SYS_STATS*/
            sem->sem = new_sem;
            return ERR_OK;
        }

        /* failed to allocate memory... */
        SYS_STATS_INC( sem.err );
        sem->sem = SYS_SEM_NULL;
        return ERR_MEM;
    }

    void sys_sem_free( sys_sem_t *sem )
    {
        /* parameter check */
        LWIP_ASSERT( "sem != NULL", sem != NULL );
        LWIP_ASSERT( "sem->sem != SYS_SEM_NULL", sem->sem != SYS_SEM_NULL );
        LWIP_ASSERT( "sem->sem != INVALID_HANDLE_VALUE", sem->sem != INVALID_HANDLE_VALUE );
        CloseHandle( sem->sem );

        SYS_STATS_DEC( sem.used );
    #if LWIP_STATS && SYS_STATS
        LWIP_ASSERT( "sys_sem_free() closed more than created", lwip_stats.sys.sem.used != ( u16_t )-1 );
    #endif /* LWIP_STATS && SYS_STATS */
        sem->sem = NULL;
    }

    u32_t sys_arch_sem_wait( sys_sem_t *sem, u32_t timeout )
    {
        uint32_t ret;
        int64_t starttime, endtime;
        LWIP_ASSERT( "sem != NULL", sem != NULL );
        LWIP_ASSERT( "sem != INVALID_HANDLE_VALUE", sem != INVALID_HANDLE_VALUE );
        starttime = HAL_Time_CurrentTime( );
        if( timeout == 0 )
        {
            /* wait infinite */
            ret = WaitForSingleObject( sem->sem, INFINITE );
            LWIP_ASSERT( "Error waiting for semaphore", ret == WAIT_OBJECT_0 );
            endtime = HAL_Time_CurrentTime( );
            /* return the time we waited for the sem */
            auto waittime = endtime - starttime;
            return  waittime > UINT32_MAX ? UINT32_MAX : waittime;
        }
        ret = WaitForSingleObject( sem->sem, timeout );
        LWIP_ASSERT( "Error waiting for semaphore", ( ret == WAIT_OBJECT_0 ) || ( ret == WAIT_TIMEOUT ) );
        if( ret != WAIT_OBJECT_0 )
            return SYS_ARCH_TIMEOUT;

        endtime = HAL_Time_CurrentTime( );
        /* return the time we waited for the sem */
        auto waittime = endtime - starttime;
        return  waittime > UINT32_MAX ? UINT32_MAX : waittime;
    }

    void sys_sem_signal( sys_sem_t *sem )
    {
        DWORD ret;
        LWIP_ASSERT( "sem != NULL", sem != NULL );
        LWIP_ASSERT( "sem->sem != NULL", sem->sem != NULL );
        LWIP_ASSERT( "sem->sem != INVALID_HANDLE_VALUE", sem->sem != INVALID_HANDLE_VALUE );
        LONG oldCount;
        ret = ReleaseSemaphore( sem->sem, 1, &oldCount );
        LWIP_ASSERT( "Error releasing semaphore", ret != 0 );
        Events_Set(SYSTEM_EVENT_FLAG_NETWORK);
    }

    sys_thread_t sys_thread_new( const char *name, lwip_thread_fn function, void *arg, int stacksize, int prio )
    {
        struct threadlist *new_thread;
        HANDLE h;

        LWIP_UNUSED_ARG( name );
        LWIP_UNUSED_ARG( stacksize );
        LWIP_UNUSED_ARG( prio );

        new_thread = ( struct threadlist* )malloc( sizeof( struct threadlist ) );
        LWIP_ASSERT( "new_thread != NULL", new_thread != NULL );
        if( new_thread != NULL )
        {
            ScopedLock scopeLock( SysArchLock );
            new_thread->next = lwip_win32_threads;
            lwip_win32_threads = new_thread;

            h = CreateThread( 0, 0, ( LPTHREAD_START_ROUTINE )function, arg, 0, &( new_thread->id ) );
            LWIP_ASSERT( "h != 0", h != 0 );
            LWIP_ASSERT( "h != -1", h != INVALID_HANDLE_VALUE );

            return new_thread->id;
        }
        return 0;
    }

    err_t sys_mbox_new( sys_mbox_t *mbox, int size )
    {
        LWIP_ASSERT( "mbox != NULL", mbox != NULL );
        LWIP_UNUSED_ARG( size );

        mbox->sem = CreateSemaphore( 0, 0, MAX_QUEUE_ENTRIES, 0 );
        LWIP_ASSERT( "Error creating semaphore", mbox->sem != NULL );
        if( mbox->sem == NULL )
        {
            SYS_STATS_INC( mbox.err );
            return ERR_MEM;
        }
        memset( &mbox->q_mem, 0, sizeof( u32_t )*MAX_QUEUE_ENTRIES );
        mbox->head = 0;
        mbox->tail = 0;
        SYS_STATS_INC_USED( mbox );
    #if LWIP_STATS && SYS_STATS
        LWIP_ASSERT( "sys_mbox_new() counter overflow", lwip_stats.sys.mbox.used != 0 );
    #endif /* LWIP_STATS && SYS_STATS */
        return ERR_OK;
    }

    void sys_mbox_free( sys_mbox_t *mbox )
    {
        /* parameter check */
        LWIP_ASSERT( "mbox != NULL", mbox != NULL );
        LWIP_ASSERT( "mbox->sem != NULL", mbox->sem != NULL );
        LWIP_ASSERT( "mbox->sem != INVALID_HANDLE_VALUE", mbox->sem != INVALID_HANDLE_VALUE );

        CloseHandle( mbox->sem );

        SYS_STATS_DEC( mbox.used );
    #if LWIP_STATS && SYS_STATS
        LWIP_ASSERT( "sys_mbox_free() ", lwip_stats.sys.mbox.used != ( u16_t )-1 );
    #endif /* LWIP_STATS && SYS_STATS */
        mbox->sem = NULL;
    }

    void sys_mbox_post( sys_mbox_t *q, void *msg )
    {
        DWORD ret;
        SYS_ARCH_DECL_PROTECT( lev );

        /* parameter check */
        LWIP_ASSERT( "q != SYS_MBOX_NULL", q != SYS_MBOX_NULL );
        LWIP_ASSERT( "q->sem != NULL", q->sem != NULL );
        LWIP_ASSERT( "q->sem != INVALID_HANDLE_VALUE", q->sem != INVALID_HANDLE_VALUE );

        SYS_ARCH_PROTECT( lev );
        q->q_mem[ q->head ] = msg;
        ( q->head )++;
        if( q->head >= MAX_QUEUE_ENTRIES )
        {
            q->head = 0;
        }
        LWIP_ASSERT( "mbox is full!", q->head != q->tail );
        ret = ReleaseSemaphore( q->sem, 1, 0 );
        LWIP_ASSERT( "Error releasing sem", ret != 0 );

        SYS_ARCH_UNPROTECT( lev );
    }

    err_t sys_mbox_trypost( sys_mbox_t *q, void *msg )
    {
        u32_t new_head;
        DWORD ret;
        SYS_ARCH_DECL_PROTECT( lev );

        /* parameter check */
        LWIP_ASSERT( "q != SYS_MBOX_NULL", q != SYS_MBOX_NULL );
        LWIP_ASSERT( "q->sem != NULL", q->sem != NULL );
        LWIP_ASSERT( "q->sem != INVALID_HANDLE_VALUE", q->sem != INVALID_HANDLE_VALUE );

        SYS_ARCH_PROTECT( lev );

        new_head = q->head + 1;
        if( new_head >= MAX_QUEUE_ENTRIES )
        {
            new_head = 0;
        }
        if( new_head == q->tail )
        {
            SYS_ARCH_UNPROTECT( lev );
            return ERR_MEM;
        }

        q->q_mem[ q->head ] = msg;
        q->head = new_head;
        LWIP_ASSERT( "mbox is full!", q->head != q->tail );
        ret = ReleaseSemaphore( q->sem, 1, 0 );
        LWIP_ASSERT( "Error releasing sem", ret != 0 );

        SYS_ARCH_UNPROTECT( lev );
        return ERR_OK;
    }

    u32_t sys_arch_mbox_fetch( sys_mbox_t *q, void **msg, u32_t timeout )
    {
        uint64_t starttime;
        /* parameter check */
        LWIP_ASSERT( "q != SYS_MBOX_NULL", q != SYS_MBOX_NULL );
        LWIP_ASSERT( "q->sem != NULL", q->sem != NULL );
        LWIP_ASSERT( "q->sem != INVALID_HANDLE_VALUE", q->sem != INVALID_HANDLE_VALUE );

        if( msg != NULL )
            *msg = NULL;

        if( timeout == 0 )
            timeout = INFINITE;

        starttime = HAL_Time_CurrentTime( );
        while(true)
        {
            auto ret = WaitForSingleObjectEx( q->sem, timeout, TRUE );
            if( ret == WAIT_OBJECT_0 )
            {
                ScopedLock scopedLock( SysArchLock );
                if( msg != NULL )
                    *msg = q->q_mem[ q->tail ];

                ( q->tail )++;
                if( q->tail >= MAX_QUEUE_ENTRIES )
                    q->tail = 0;

                Events_Set(SYSTEM_EVENT_FLAG_NETWORK);
                return HalTimeToMilliseconds( HAL_Time_CurrentTime() - starttime );
            }

            if( ret ==  WAIT_TIMEOUT )
                return SYS_ARCH_TIMEOUT;

            if( ret == WAIT_FAILED )
            {
                auto err = GetLastError();
                LWIP_ASSERT("WAIT_FAILED!", false);
                return HalTimeToMilliseconds( HAL_Time_CurrentTime() - starttime );
            }

            if( ret == WAIT_IO_COMPLETION )
                continue;

            LWIP_ASSERT( "ERROR waiting for mail box", false );
            return SYS_ARCH_TIMEOUT;
        }
    }

    u32_t sys_arch_mbox_tryfetch( sys_mbox_t *q, void **msg )
    {
        DWORD ret;

        /* parameter check */
        LWIP_ASSERT( "q != SYS_MBOX_NULL", q != SYS_MBOX_NULL );
        LWIP_ASSERT( "q->sem != NULL", q->sem != NULL );
        LWIP_ASSERT( "q->sem != INVALID_HANDLE_VALUE", q->sem != INVALID_HANDLE_VALUE );

        // test for signalled state with timeout==0
        if( ( ret = WaitForSingleObject( q->sem, 0 ) ) == WAIT_OBJECT_0 )
        {
            ScopedLock scopedLock( SysArchLock );
            if( msg != NULL )
            {
                *msg = q->q_mem[ q->tail ];
            }

            ( q->tail )++;
            if( q->tail >= MAX_QUEUE_ENTRIES )
            {
                q->tail = 0;
            }
            return 0;
        }
        else
        {
            LWIP_ASSERT( "Error waiting for sem", ret == WAIT_TIMEOUT );
            if( msg != NULL )
            {
                *msg = NULL;
            }

            return SYS_ARCH_TIMEOUT;
        }
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
        auto resultPair = MasterTimeoutQueue.emplace( handler, arg );
        LWIP_ASSERT("FAILED to allocate timeout object", resultPair.second || (resultPair.first != MasterTimeoutQueue.end() && resultPair.first->Compare(handler,arg)) );
        resultPair.first->Start( msecs );
    }

    // create a periodic timer that triggers on a fixed interval (period)
    void sys_periodic_timeout( u32_t msecs, sys_timeout_handler handler, void *arg )
    {
        ScopedLock scopedLock( SysArchLock );
        auto resultPair = MasterTimeoutQueue.emplace( handler, arg );
        LWIP_ASSERT("FAILED to allocate periodic timeout object", resultPair.second || (resultPair.first != MasterTimeoutQueue.end() && !resultPair.first->Compare(handler,arg)) );
        resultPair.first->Start( msecs, true );
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
        auto it = MasterTimeoutQueue.find( OsTimeout( handler, arg ) );    
        //LWIP_ASSERT("sys_untimeout: Failed to find requested timeout!", it != MasterTimeoutQueue.end() );
        if( it == MasterTimeoutQueue.end() )
            return;

        MasterTimeoutQueue.erase( it );
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

    void sys_timeouts_uninit( void )
    {
        ScopedLock scopedLock( SysArchLock );
        for( auto&& to : MasterTimeoutQueue )
        {
            to.Cancel( );
        }

        MasterTimeoutQueue.clear( );
    }
}