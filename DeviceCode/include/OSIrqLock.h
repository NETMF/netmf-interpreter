///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework
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
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#ifndef _OSIRQLOCK_H_
#define _OSIRQLOCK_H_
#include <tinyhal.h>

namespace MsOpenTech
{
    namespace NETMF
    {
        // Cross platform special case handling for CLR IRQ lock
        // The locking mechanism is simulating a GLOBAL
        // CPU State, which is what it was originally 
        // designed to do, so recursive locks with counts
        // don't work correctly here. This is an all or
        // nothing gate. If the lock is owned already
        // (e.g. interrupts are disabled) then no further
        // action is needed and the lock *MUST NOT* be
        // taken recursively.
        //
        // Template parameters:
        //    lock_t - type of wrapped lock
        //           * Contract requirements for lock_t:
        //           = void lock() takes lock, potentially blocking to wait,
        //             and records current thread as owner thread of the lock
        //             once acquired.
        //           = void unlock() release a previously owned lock, setting
        //             the lock's owner to an implementation defined "no-owner"
        //             value. if unlock() is called from a thread other than
        //             the owner this must not unlock the lock (e.g. it becomes
        //             a no-op) although it is acceptable to ASSERT( owned() )
        //             to aid in catching this case during development. 
        //           = bool owned() returns true if the current thread is the
        //             lock owner.
        //           - static method bool InIsr() returns true if the caller is
        //             currently executing in the context of an ISR (and therefore
        //             should take or release the lock)
        template<typename lock_t>
        class OsIrqLock
        {
        public:
            void lock( )
            {
                if( lock_t::InIsr() )
                    return;

                // if the lock is already owned (e.g. interrupts are disabled already)
                // then don't re-enter the lock as that will increase the lock's count
                // and ultimately require another call to unlock() that won't happen.
                if( !Lock.owned() )
                   Lock.lock(); 
            }

            void unlock( )
            {
                if( lock_t::InIsr() )
                    return;

				// If this is called from the non-owner thread.
                // Don't make things worse by trying to unlock it.
                // This can happen if a previous call to lock was
                // made when the Irqs were locked already by the
                // calling thread.
                if( Lock.owned() )
                    Lock.unlock();
            }

            bool owned()  { return Lock.owned(); }

        private:
            lock_t Lock;
        };
    }
}
#endif

