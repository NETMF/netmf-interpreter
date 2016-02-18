#ifndef CMSIS_OS_CPP_H__
#define CMSIS_OS_CPP_H__

#include <cmsis_os.h>

#define CMSIS_VERSION(m,mm) ( ( ( m )<< 16 ) | ( mm ) )

#if osCMSIS < CMSIS_VERSION(1, 2)
#error "Unsupported CMSIS version: This header requires a CMSIS-RTOS API implementation with at least version 1.2"
#endif

#if !defined(osCMSIS_RTX) || osCMSIS_RTX != CMSIS_VERSION(4, 78)
// unfortunately the implementation of the osXxxxDef and other similar macros
// are implementation defined and, at least for CMSIS-RTX cannot be used to
// declare a C++ data member. Thus, this header is specific to a particular 
// version of CMSIS-RTX and requires re-evaluation and verification on any
// other versions. The structures can and have changed even on minor version
// changes, so the check here is for an exact match on the supported version.
#error "Unsupported CMSIS-RTOS implementation: This header requires the CMSIS-RTX v4.78 implementation of the CMSIS-RTOS API"
#endif

// The following two fields are culled from the CMSIS-RTX header implementation
// of the osMutexDef macro.
// this variant of the standard CMSIS-RTOS macro allows use as a class or struct data member
#define osMutexDefMember( name )\
    uint32_t os_mutex_cb_##name[4]; /* = { 0 }; */\
    osMutexDef_t os_mutex_def_##name/* = { (os_mutex_cb_##name) }*/

// This macro is used in the C++ class constructor initializer list to create
// default initialized members
#define osMutexInitMember( name )\
        os_mutex_cb_##name()\
        ,os_mutex_def_##name()

// This variant of the standard CMSIS-RTOS macro constructs a member
// since a standard constructor isn't available.
#define osMutexConstructMember( name )\
    memset( &os_mutex_cb_##name, 0, sizeof( os_mutex_cb_##name ) );\
    memset( &os_mutex_def_##name, 0, sizeof( os_mutex_def_##name ) );\
    os_mutex_def_##name.mutex = &os_mutex_cb_##name

// This macor is used to define a timer instance as a member of a class or struct
#define osTimerDefMember( name )\
    uint32_t os_timer_cb_##name[6]; /* = { 0 }*/\
    osTimerDef_t os_timer_def_##name /* = { (function), (os_timer_cb_##name) }*/

// This macro is used in the C++ class constructor initializer list to create
// initialized members
#define osTimerInitMember( name )\
        os_timer_cb_##name()\
        ,os_timer_def_##name()

// ARMCC 5.04 and earlier don't support C++11 initializer list syntax for member initialization
// Therefore, this is used in the constructor to set the internal members as needed.
#define osTimerConstructMember( name, function )\
        memset( &os_timer_cb_##name, 0, sizeof( os_timer_cb_##name ) );\
        os_timer_def_##name.ptimer = function;\
        os_timer_def_##name.timer = &os_timer_cb_##name

namespace MsOpenTech
{
    namespace CMSIS_RTOS
    {
        class Timer
        {
        public:
            Timer( os_ptimer handler, void* arg )
                : CallbackArg( arg )
                , TimerId( NULL )
                , osTimerInitMember( TimerDef )
            {
                osTimerConstructMember( TimerDef, handler );
            }

            void Cancel(void)
            {
                osTimerStop( TimerId );
            }

            void Start( uint32_t initialTimeout, bool periodic )
            {
                TimerId = osTimerCreate( osTimer(TimerDef), periodic ? osTimerPeriodic : osTimerOnce, CallbackArg );
                              osTimerStart(TimerId, initialTimeout);
            }

            // C++11 move semantics not supported in the ARM compiler 5.04 and earlier
            // so an explicit Close() is used whenever the actual timer is no longer in
            // use by the application.
            void Close()
            {
                if( TimerId != NULL )
                {
                    osTimerDelete( TimerId );
                    CallbackArg = NULL;
                    TimerId = NULL;
                    (osTimer( TimerDef ))->ptimer = NULL;
                    (osTimer( TimerDef ))->timer = NULL;
                }
            }
        
        // property accessors
        public:
            const void* get_Arg() const { return CallbackArg; }
            os_ptimer get_Handler() const { return (osTimer( TimerDef ))->ptimer; }

        private:
            void* CallbackArg;
            osTimerId TimerId;
            osTimerDefMember( TimerDef );
        };
    }
}

#endif
