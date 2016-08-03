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
        typedef void (*TimerCallback) (void const *);
        
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
        // handler function and the argument to the handler.
        template< typename OsTimerT >
        class TimeoutCompare
        {
        public:
            // Compare this Timeout with the specified handler and arg
            // returns 0 if equal, negative value if this instance is less
            // and 1 if this instance is greater.
            static int Compare( const OsTimerT& lhs, sys_timeout_handler handler, void* arg )
            {
                return Compare( lhs, reinterpret_cast<TimerCallback>(handler), arg );
            }

            static int Compare( const OsTimerT& lhs, const OsTimerT& rhs )
            {
                return Compare( lhs, rhs.get_Handler(), rhs.get_Arg() );
            }
            
            static int Compare( const OsTimerT& lhs, TimerCallback handler, void* arg )
            {
                if( handler < lhs.get_Handler() )
                    return -1;

                if( handler == lhs.get_Handler() && arg == lhs.get_Arg() )
                    return 0;

                if( handler > lhs.get_Handler() )
                    return 1;

                return arg > lhs.get_Arg() ? 1 : -1;
            }
        };


        // This class allows storing a set of Timeouts in a collection to find
        // it again when cancelation is required.
        // 
        // Template Parameters:
        //  Timer_t - type of the underlying OS specific Timer object
        // 
        template<typename Timer_t>
        class Timeout : public HAL_DblLinkedNode< Timeout<Timer_t> >
        {
        public:
            typedef Timeout<Timer_t> _My_t;

            Timeout( sys_timeout_handler handler, void* arg )
                : OsTimer( reinterpret_cast<TimerCallback>(handler), arg )
            {
                this->Initialize();
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
            // Compare this Timeout with the specified handler and arg
            // returns 0 if equal, negative value if this instance is less
            // and 1 if this instance is greater.
            int Compare( sys_timeout_handler handler, void* arg ) const
            {
                return TimeoutCompare<Timer_t>::Compare( OsTimer, handler, arg );
            }

            int Compare( _My_t const& other ) const
            {
                return TimeoutCompare<Timer_t>::Compare( OsTimer, other.OsTimer );
            }

            bool operator==( _My_t const& other) const
            {
                return 0 == TimeoutCompare<Timer_t>::Compare( OsTimer, other.OsTimer );
            }

            bool operator<(_My_t const& other) const
            {
                return -1 == TimeoutCompare<Timer_t>::Compare( OsTimer, other.OsTimer );
            }

            bool operator>( _My_t const& other ) const
            {
                return 1 == TimeoutCompare<Timer_t>::Compare( OsTimer, other.OsTimer );
            }

            void Cancel() const
            {
                OsTimer.Cancel();
                OsTimer.Close();
            }

        private:
            // The underlying os timer object is mutable
            // as it does not contribute to the identity
            // of the object. The Functor determines
            // the identity. This allows placing instances
            // of this class into standard collections. 
            mutable Timer_t OsTimer;
        };
    }
}
