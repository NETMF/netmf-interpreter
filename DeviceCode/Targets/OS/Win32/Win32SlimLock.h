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

namespace Microsoft
{
    namespace Win32
    {
        // Behavioral traits style template for lock objects
        // supporting shared and exclusive access. This is
        // specialized for lock objects that don't meet
        // the defaults.
        //
        // Essentially this maps a well known API onto whatever calls 
        // are needed for a particular lock type to achieve the intended
        // behavior. This prevents the need to further specialize other
        // templates for each type of lock object like the scoped locking
        // templates SharedLock or ExclusiveLock.
        template<typename Lock_t>
        struct LockBehaviors
        {
            // exclusive (R/W) lock
            static void LockExclusive( Lock_t& lock)   { lock.LockExclusive(); }
            static void UnlockExclusive( Lock_t& lock) { lock.UnlockExclusive(); }
    
            // Share lock (multiple shared lock allowed, blocks and blocked by exclusive)
            static void LockShared( Lock_t& lock)   { lock.LockShared(); }
            static void UnlockShared( Lock_t& lock) { lock.UnlockShared(); }
        };

        // specialization for a raw standard SRWLOCK
        template<>
        struct LockBehaviors<SRWLOCK>
        {
            static void LockExclusive( SRWLOCK& lock)   { ::AcquireSRWLockExclusive( &lock ); }
            static void UnlockExclusive( SRWLOCK& lock) { ::ReleaseSRWLockExclusive( &lock ); }

            static void LockShared( SRWLOCK& lock)   { ::AcquireSRWLockShared( &lock ); }
            static void UnlockShared( SRWLOCK& lock) { ::ReleaseSRWLockShared( &lock ); }
        };

        // specialization for a raw CS
        template<>
        struct LockBehaviors<CRITICAL_SECTION>
        {
            static void LockExclusive( CRITICAL_SECTION& lock)   { EnterCriticalSection( &lock) ; }
            static void UnlockExclusive( CRITICAL_SECTION& lock) { LeaveCriticalSection( &lock ); }

            // CS cannot be locked for share mode, these are deliberately undefined
            // any attempts use the CS in a manner that resolves to a shared lock
            // will generate a compile time error
            //static void LockShared( Lock_t& lock);
            //static void UnlockShared( Lock_t& lock);
        };

        // Wrapper for an SRWLOCK with support for nesting
        // SRWLOCK isn't re-entrant, if a thread requesting exclusive
        // access currently has the lock while another thread also
        // waiting for exclusive access a deadlock will occur. To
        // resolve this case this class uses a lock ref count to
        // manage the recursion on a given thread and tracks the id
        // of the owning thread. 
        class SlimReadWriteLock
        {
        public:
            SlimReadWriteLock() 
                : m_ExLockCount( 0 )
                , m_ExOwner( NoOwnerId )
            {
                InitializeSRWLock( &m_SrwLock );
            }
    
            void LockExclusive()
            { 
                if( OnExOwnerThread() )
                    ++m_ExLockCount;
                else
                {
                    LockBehaviors<SRWLOCK>::LockExclusive( m_SrwLock );
                    SetOwnerThread();
                }
            }

            void UnlockExclusive()
            { 
                _ASSERT( OnExOwnerThread() );
                if( --m_ExLockCount == 0 )
                {
                    m_ExOwner = NoOwnerId;
                    LockBehaviors<SRWLOCK>::UnlockExclusive( m_SrwLock );
                }
            }
    
            // shared access locking methods
            void LockShared()
            { 
                _ASSERT( !OnExOwnerThread() );
                LockBehaviors<SRWLOCK>::LockShared( m_SrwLock );
            }

            void UnlockShared()
            { 
                _ASSERT( !OnExOwnerThread() );
                LockBehaviors<SRWLOCK>::UnlockShared( m_SrwLock );
            }
    
            DWORD WaitShared( ConditionVariable& cv, DWORD timeout = INFINITE )
            {
                _ASSERT( !OnExOwnerThread() );
                return LockBehaviors<SRWLOCK>::WaitShared( m_SrwLock, cv, timeout );
            }

            DWORD WaitExclusive( ConditionVariable& cv, DWORD timeout = INFINITE )
            {
                _ASSERT( OnExOwnerThread() );
                return LockBehaviors<SRWLOCK>::WaitExclusive( m_SrwLock, cv, timeout );
            }
    
        private:
            // test if current thread is the Exclusive owner of the lock
            bool OnExOwnerThread()
            {
                return ( m_ExOwner == ::GetCurrentThreadId() ) && ( m_ExLockCount > 0 );
            }

            void SetOwnerThread()
            {
                _ASSERT( m_ExLockCount == 0 && NoOwnerId == m_ExOwner );
                m_ExOwner = ::GetCurrentThreadId();
                m_ExLockCount = 1;
            }

            int m_ExLockCount;
            DWORD m_ExOwner;
            SRWLOCK m_SrwLock;

            static const DWORD NoOwnerId = 0xFFFFFFFF;    
        };

        //////////////////////////////////////////////////////////
        // Description:
        //   Scoped shared lock
        //
        // Parameters:
        //   lock_t - type of underlying lock object
        //
        // Constraints:
        //   void LockBehaviors<Lock_t>::LockShared(void) required method
        //   void LockBehaviors<Lock_t>::UnlockShared(void) required method
        template<typename Lock_t>
        struct SharedLock
        {
            SharedLock( Lock_t& lockObj)
                : m_LockObj( lockObj )
            {
                LockBehaviors<Lock_t>::LockShared( m_LockObj );
            }

            ~SharedLock()
            {
                LockBehaviors<Lock_t>::UnlockShared( m_LockObj );
            }

            // Atomic, unlock, wait, lock
            DWORD Wait( ConditionVariable& cond, DWORD timeout = INFINITE )
            {
                return LockBehaviors<Lock_t>::WaitShared( m_LockObj, cond, timeout );
            }

        private:
            Lock_t& m_LockObj;

            // Private to avoid accidental use
            SharedLock( const SharedLock& ) throw();
            SharedLock& operator=( const SharedLock& ) throw();
            void* operator new(size_t);
        };

        // creates a scoped exclusive lock
        template<typename Lock_t>
        struct ExclusiveLock
        {
            ExclusiveLock( Lock_t& lockObj)
                : m_LockObj( lockObj )
            {
                LockBehaviors<Lock_t>::LockExclusive( m_LockObj );
            }

            ~ExclusiveLock()
            {
                LockBehaviors<Lock_t>::UnlockExclusive( m_LockObj );
            }

            // Atomic, unlock, wait, lock
            DWORD Wait( ConditionVariable& cond, DWORD timeout = INFINITE )
            {
                return LockBehaviors<Lock_t>::WaitExclusive( m_LockObj, cond, timeout );
            }

        private:
            Lock_t& m_LockObj;

            // Private to avoid accidental use
            ExclusiveLock( const ExclusiveLock& ) throw();
            ExclusiveLock& operator=( const ExclusiveLock& ) throw();
            void* operator new(size_t);
        };
    }
}