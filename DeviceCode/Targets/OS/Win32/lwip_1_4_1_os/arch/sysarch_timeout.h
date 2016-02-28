#include "tinyhal.h"

#if NO_SYS
#error "ERROR: This LWIP Architecture port requires NO_SYS==0!"
#endif

#ifdef CONSTEXPR_SUPPORTED
#define CONSTEXPR constexpr
#else
#define CONSTEXPR inline
#endif

namespace lwIP
{
    namespace SysArch
    {
        const uint64_t SystemTimeUnitsPerMicrosecond = 10; // HAL system time is in 100ns intervals
        const uint64_t SystemTimeUnitsPerMillisecond = 1000 * SystemTimeUnitsPerMicrosecond;
        
        // converts milliseconds to NETMF HAL time (100ns intervals a.k.a FILETIME)
        CONSTEXPR uint64_t MillisecondsToHalTime( uint32_t ms )
        {
            return uint64_t( ms ) * SystemTimeUnitsPerMillisecond;
        }

        // converts from NETMF HAL time (100ns intervals a.k.a FILETIME) to milliseconds
        CONSTEXPR uint64_t HalTimeToMilliseconds( uint64_t halTime )
        {
            return halTime / SystemTimeUnitsPerMillisecond;
        }

        // lwip tracks and identifies timeout "instances"  by a combination of the
        // handler function and the argument to the handler. This class allows
        // storing a set of Timeouts in a collection to find it again when cancelation
        // is required.
        // 
        // Template Parameters:
        //  Timer_t - type of the underlying OS specific Timer object
        //  OsTimerFactory_t - type of factory for creating OS timers
        // 
        // Template Contracts:
        // Timer_t::Cancel(void) - Cancels a pending time. This must be safe to call multiple times,
        //                         only the first time will actually cancel the timer.
        // Timer_t::Timer_t()    - default constructable to a known inactive state
        // Timer_t::Start( uint32_t initialTimeout, bool periodic ) const
        //                       - Starts the timer with the specified initial timeout and period.
        // static Timer_t OsTimerFactory_t::CreateTimer( sys_timeout_handler handler, void* arg )
        //                       - Creates a new OS timer that, when triggered calls 'handler( arg );'
        //
        //
        template<typename Timer_t, typename OsTimerFactory_t>
        class Timeout
        {
        public:
            typedef Timeout<Timer_t, OsTimerFactory_t> _My_t;

            Timeout(sys_timeout_handler handler, void* arg )
                : OsTimer( OsTimerFactory_t::CreateTimer( handler, arg ) )
                , handler( handler )
                , arg( arg )
            {
            }

            // To prevent the timer from triggering and attempting to call a partially
            // constructed instance (if construction is preempted), the OsTimer is constructed
            // in an inactive state then started here when it is safe to trigger the callback.
            void Start( uint32_t initialTimeout, bool periodic = false ) const
            {
                OsTimer.Start( initialTimeout, periodic );
            }

            // Compare this Timeout with the specified handler and arg
            // returns 0 if equal, negative value if this instance is less
            // and 1 if this instance is greater.
            int Compare( sys_timeout_handler handler, void* arg ) const
            {
                if( handler < this->handler )
                    return -1;

                if( handler == this->handler && arg == this->arg )
                    return 0;

                if( handler > this->handler)
                    return 1;

                return arg > this->arg ? 1 : -1;
            }

            int Compare( Timeout const& other ) const
            {
                if( *this < other )
                    return -1;

                if( *this == other )
                    return 0;

                return 1;
            }

            bool operator==( Timeout const& other) const
            {
               return handler == other.handler && arg == other.arg;
            }

            bool operator<(Timeout const& other) const
            {
                if( handler < other.handler )
                    return true;

                if( handler > other.handler )
                    return false;

                return arg < other.arg;
            }

            bool operator>( const Timeout& other ) const
            {
                if( handler > other.handler )
                    return true;

                if( handler < other.handler )
                    return false;

                return arg > other.arg;
            }

            void Cancel() const
            {
                OsTimer.Cancel();
            }

        private:
            // The underlying os timer object is mutable
            // as it does not contribute to the identity
            // of the object. The handler+arg determines
            // the identity. This allows placing instances
            // of this class into standard collections. 
            mutable Timer_t OsTimer;
            sys_timeout_handler handler;
            void* arg;
        };
    }
}
