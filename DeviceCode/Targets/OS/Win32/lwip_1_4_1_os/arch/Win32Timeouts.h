#pragma once
#include <cstdint>
#include <functional>
#include <utility>
#include <TinySupport.h>
#include <lwip/timers.h>

namespace Microsoft
{
    namespace Win32
    {
        const std::uint64_t SystemTimeUnitsPerMicrosecond = 10; // HAL system time is in 100ns intervals
        const std::uint64_t SystemTimeUnitsPerMillisecond = 1000 * SystemTimeUnitsPerMicrosecond;
        
        // converts milliseconds to NETMF HAL time (100ns intervals a.k.a FILETIME)
        std::uint64_t MillisecondsToHalTime( std::uint32_t ms )
        {
            return uint64_t( ms ) * SystemTimeUnitsPerMillisecond;
        }

        // converts from NETMF HAL time (100ns intervals a.k.a FILETIME) to milliseconds
        uint64_t HalTimeToMilliseconds( std::uint64_t halTime )
        {
            return halTime / SystemTimeUnitsPerMillisecond;
        }

        // general swap utility to support swapping handle values which are typedef'd as void*
        template<typename T>
        inline void swap( T& a, T& b )
        {
            T c = std::move( a );
            a = std::move( b );
            b = std::move( c );
        }

        class Timeout
        {
        public:
            Timeout( uint32_t timeout, sys_timeout_handler handler, void* arg, bool periodic = false )
                : hTimer( ::CreateWaitableTimer( nullptr, FALSE, nullptr) )
                , Period( periodic ? timeout : 0 )
                , Deadline( ComputeDeadline( timeout, HAL_Time_CurrentTime() ) )
                , Callback( handler )
                , Arg( arg )
            {
                LWIP_ASSERT("NULL handler function provided", handler != nullptr );
                LWIP_ASSERT("FAILED to create OS Waitable timer", hTimer != nullptr && hTimer != INVALID_HANDLE_VALUE);
                LWIP_ASSERT("Inivalid period for periodic timout", Period > 0 || !periodic );

                auto stat = ::SetWaitableTimer( hTimer, &Deadline, Period, nullptr, nullptr, FALSE );
                LWIP_ASSERT("FAILED to set waitable timer timeout", stat );
            }

            ~Timeout( )
            {
                if( hTimer != nullptr )
                    ::CloseHandle( hTimer );
            }

            // NOT Copyable as that would create aliasing
            Timeout( const Timeout& ) = delete;
            Timeout& operator=( const Timeout& ) = delete;

            // move constructable
            Timeout( Timeout&& other )
                : Timeout( )
            {
                swap( hTimer, other.hTimer );
                swap( Callback, other.Callback );
                swap( Arg, other.Arg );
                swap( Deadline, other.Deadline );
                swap( Period, other.Period );
            }

            // move assignable
            Timeout& operator=( Timeout&& other )
            {
                swap( hTimer, other.hTimer );
                swap( Callback, other.Callback );
                swap( Arg, other.Arg );
                swap( Deadline, other.Deadline );
                swap( Period, other.Period );
                return *this;
            }

            operator HANDLE( ) const
            {
                return hTimer;
            }

            void operator()()
            {
                // if periodic, update the deadline for the next time
                // so that the deadline can be used in sorting by the
                // earliest deadline when inserted back into a queue.
                if( IsPeriodic() )
                    Deadline = ComputeDeadline( Period, Deadline.QuadPart );

                Callback( Arg );
            }

            bool IsExpired( ) const
            {
                return ::WaitForSingleObject( hTimer, 0 ) == WAIT_OBJECT_0;
            }

            bool IsCanceled() const
            {
                return Deadline.QuadPart == 0;
            }

            void Cancel( )
            {
                if( IsCanceled() )
                    return;

                if( hTimer != nullptr )
                {
                    if( !CancelWaitableTimer( hTimer ) )
                    {
                        auto err = ::GetLastError( );
                        throw std::exception( "Internal error: CancelWaitableTimer() FAILED!", err );
                    }
                }

                Deadline.QuadPart = 0;
            }

            uint64_t get_Deadline( ) const
            {
                return Deadline.QuadPart;
            }

            uint32_t get_Period() const
            {
                return Period;
            }

            bool Match( sys_timeout_handler handler, void* arg ) const
            {
                return Arg == arg && handler == handler;
            }

            bool IsPeriodic() const
            {
                return Period > 0;
            }

        private:
            Timeout()
                : hTimer( nullptr )
                , Period( 0 )
                , Deadline( { 0 } )
                , Callback( nullptr )
                , Arg( nullptr )
            {
            }

            HANDLE hTimer;
            int64_t Period;
            LARGE_INTEGER Deadline;
            sys_timeout_handler Callback;
            void* Arg;

            static LARGE_INTEGER ComputeDeadline( uint32_t timeout, uint64_t baseTime )
            {
                LARGE_INTEGER retVal;
                auto deltaT = ComputePeriod( timeout );
                retVal.QuadPart = deltaT.QuadPart + baseTime;
                return retVal;
            }

            static LARGE_INTEGER ComputePeriod( uint32_t timeout )
            {
                LARGE_INTEGER retVal;
                retVal.QuadPart = uint64_t( timeout ) * SystemTimeUnitsPerMillisecond;
                return retVal;
            }
        };
    }
}