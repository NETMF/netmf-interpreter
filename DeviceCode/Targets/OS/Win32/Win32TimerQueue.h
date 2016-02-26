////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#include <Windows.h>
#include <functional>
#include <utility>
#include <memory>
#include "EnumFlags.h"

namespace Microsoft
{
    namespace Win32
    {
        // std swap implementation for HANDLES
        inline void swap(HANDLE& a, HANDLE& b)
        {
            HANDLE c = a;
            a = b;
            b= c;
        }
        
        enum class TimerOption  
        {
            ExecuteDefault = WT_EXECUTEDEFAULT,
            OneShot = WT_EXECUTEONLYONCE,
            ExecuteOnTimerThread = WT_EXECUTEINTIMERTHREAD
        };
        ENUM_FLAGS( TimerOption );

        // class to implement timers based on Win32 TimerQueueTimers
        // Win32 TimerQueue timers with WT_EXECUTEDEFAULT (the default)
        // will trigger callbacks on a threadpool thread when the timer
        // expires.
        class Timer
        {
        public:
            Timer( HANDLE hQueue, UINT32 initialPeriod, UINT32 period, TimerOption flags, std::function<void()> func )
                : hQueue( hQueue )
                , hTimer( NULL )
                , Callback( std::make_unique< std::function< void() > >( func ) )
                , Period( period )
                , Flags( flags | ( period == 0 ? TimerOption::OneShot : TimerOption::ExecuteDefault ) )
            {
                Start(initialPeriod, period );
            }

            Timer()
                : Timer( nullptr, 0, 0, TimerOption::ExecuteDefault, [](){})
            {
            }

            Timer( UINT32 initialPeriod, UINT32 period, TimerOption flags, std::function<void()> func )
                : Timer( nullptr, initialPeriod, period, flags, func )
            {
            }

            Timer( UINT32 initialPeriod, std::function<void()> func)
                : Timer( nullptr, initialPeriod, 0, TimerOption::ExecuteDefault | TimerOption::OneShot, func )
            {
            }
            
            Timer( TimerOption flags, std::function<void()> func )
                : Timer( nullptr, 0, 0, flags, func )
            {
            }

            Timer( std::function<void()> func)
                : Timer( nullptr, 0, 0, TimerOption::ExecuteDefault, func )
            {
            }

            ~Timer( )
            {
                Cancel();
            }

            void Cancel()
            {
                if( hTimer != nullptr )
                {
                    if( !DeleteTimerQueueTimer( hQueue, hTimer, nullptr ) )
                    {
                        auto err = ::GetLastError();
                        if( ERROR_IO_PENDING != err )
                            throw std::exception( "Internal error: DeleteTimerQueueTimer() FAILED!", err);
                    }
                }

                hTimer = nullptr;
            }
    
            void Start( UINT32 initialPeriod, bool periodic )
            {
                Start( initialPeriod, periodic ? initialPeriod : 0 );
            }

            void Start( UINT32 initialPeriod, UINT32 period = 0)
            {
                Cancel();
                if( !(bool)Callback )
                    throw std::exception("Internal error: timer callback is empty!");
                
                Period = initialPeriod;
                if( initialPeriod > 0 )
                {
                    // period is 0 for a one-shot timer otherwise it is the period for a periodic timer
                    // which is normally, but not necessarily the same as initialPeriod.
                    if( period == 0 )
                        Flags |= TimerOption::OneShot;

                    // NOTE: using Callback.get() as the callback arg to avoid complications
                    // of tieing the OS callback to this exact instance, which would prevent
                    // move assign/copy etc... Using a unique pointer ensures that only one
                    // instance actually owns the pointer, but it can be moved around without
                    // impacting the actual OS callback. If "this" was used the OS would call
                    // the callback passing in a "this" pointer to a potentially long since
                    // destroyed instance (due to move assign/copy)
                    CreateTimerQueueTimer( &hTimer
                                         , hQueue
                                         , ( WAITORTIMERCALLBACK )OsTimerCallbackStub
                                         , Callback.get() 
                                         , initialPeriod
                                         , period
                                         , (ULONG)Flags
                                         );
                }
            }

            void Restart()
            {
                Start( Period );
            }
            
            Timer(const Timer& ) = delete;
            Timer& operator=(const Timer& ) = delete;

            // move copyable
            Timer( Timer&& other)
                : hQueue( nullptr )
                , hTimer( NULL )
                , Period( 0 )
            {
                swap( hQueue, other.hQueue );
                swap( hTimer, other.hTimer );
                swap( std::move( Callback ), std::move( other.Callback ) );
                Period = other.Period;
                Flags = other.Flags;
            }
            
            // move assignable
            Timer& operator=(Timer&& other)
            {
                swap( hQueue, other.hQueue );
                swap( hTimer, other.hTimer );
                swap( Callback, other.Callback );
                std::swap( Period, other.Period );
                std::swap( Flags, other.Flags);
                return *this;
            }

        private:
            HANDLE hQueue;
            std::unique_ptr< std::function<void()> > Callback;
            HANDLE hTimer;
            UINT32 Period;
            TimerOption Flags;

            static void CALLBACK OsTimerCallbackStub( _In_ void* lpParameter, _In_  BOOLEAN TimerOrWaitFired )
            {
                UNREFERENCED_PARAMETER( TimerOrWaitFired );
                auto pCallback = reinterpret_cast< std::function<void()>* >( lpParameter );
                if( (bool)( *pCallback ) )
                    (*pCallback)();
            }
        };
    }
}