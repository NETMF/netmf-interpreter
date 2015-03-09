////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_CPU_WATCHDOG_DECL_H_
#define _DRIVERS_CPU_WATCHDOG_DECL_H_ 1

//--//

typedef void (*WATCHDOG_INTERRUPT_CALLBACK)(void*);

/// <summary>
/// Enables the watchdog timer with the given timeout.  If possible the implementation should use an interrupt based watchdog
/// that invokes the given callback when fired.  The "context" parameter should be passed to the callback method when the 
/// watchdog has fired.
/// </summary>
/// <param name="TimeoutMilliseconds">watchdog timeout period in milliseconds</param>
/// <param name="callback">watchdog callback method which should be called when a watchdog event occurs</param>
/// <param name="context">watchdog callback parameter, which should be passed to the callback on the watchdog event</param>
/// <returns></returns>
HRESULT Watchdog_Enable      ( UINT32 TimeoutMilliseconds, WATCHDOG_INTERRUPT_CALLBACK callback, void* context );

/// <summary>
/// Disables the watchdog mechanism.  
/// </summary>
void    Watchdog_Disable     ( );

/// <summary>
/// Resets the watchdog timer. This method is called periodically by the system to ensure that the watchdog event does not occur, unless the 
/// system is in a stalled state.
/// </summary>
void    Watchdog_ResetCounter( );

/// <summary>
/// Performs a hard reset of the processor.  This is called when a watchdog event occurs if the watchdog behavior was 
/// is to hard reboot by the user.
/// </summary>
void    Watchdog_ResetCpu    ( );

//--//

//
// This enum must be sync'ed with uptime.cs 
//
/// <summary>
/// The WatchdogBehavior enumeration lists the different ways in which the system can handle watchdog events. All 
/// behavior types will attempt to log a watchdog event that can be retrieved with LastOccurrence property on the 
/// Watchdog class.
///     None                - Continues execution (may leave the system in a stalled state)
///     SoftReboot          - Performs a software reboot of the CLR (if the device does not support soft reboot, a hard reboot will occur)
///     HardReboot          - Performs a hardware reboot of the device
///     EnterBooter         - Enters the bootloader and waits for commands.
///     DebugBreak_Managed  - Attempts to enter a break state in the Visual Studio debugger
///     DebugBreak_Native   - Intended for native debugging (porting kit users).  Stops execution at the native level.
/// </summary>
enum Watchdog_Behavior
{
    Watchdog_Behavior__None                 = 0x00000000,
    Watchdog_Behavior__SoftReboot           = 0x00000001,
    Watchdog_Behavior__HardReboot           = 0x00000002,
    Watchdog_Behavior__EnterBooter          = 0x00000003,
    Watchdog_Behavior__DebugBreak_Managed   = 0x00000004,
    Watchdog_Behavior__DebugBreak_Native    = 0x00000005,
};

//--//

/// <summary>
/// Gets or sets the watchdog enable state.
/// </summary>
/// <param name="enabled">Sets the watchdog enabled state when fSet is true; otherwise this parameter is ignored</param>
/// <param name="fSet">Determines if this call is getting or setting the enabled state</param>
/// <returns>Returns the current enabled state of the watchdog</returns>
BOOL              Watchdog_GetSetEnabled ( BOOL enabled, BOOL fSet );

/// <summary>
/// Gets or sets the watchdog timeout value in milliseconds.
/// </summary>
/// <param name="timeout">Sets the timeout value (milliseconds) when fSet is true; otherwise this parameter is ignored</param>
/// <param name="fSet">Determines if this call is getting or setting the watchdog timeout</param>
/// <returns>Returns the current watchdog timeout</returns>
UINT32            Watchdog_GetSetTimeout ( INT32 timeout, BOOL fSet );

/// <summary>
/// Gets or sets the watchdog behavior for handling watchdog events (see Watchdog_Behavior enum above for details)
/// </summary>
/// <param name="behavior">Sets the watchdog behavior when fSet is true; otherwise this parameter is ignored</param>
/// <param name="fSet">Determines if this call is getting or setting the watchdog behavior</param>
/// <returns>Returns the current wathdog behavior</returns>
Watchdog_Behavior Watchdog_GetSetBehavior( Watchdog_Behavior behavior, BOOL fSet );

/// <summary>
/// Gets the last watchdog event log (if it exists).  For every watchdog behavior, the driver will attempt to log the event.  This
/// method enables the caller to retrieve the latest watchdog event.
/// </summary>
/// <param name="time">The timestamp of the watchdog in system time (100ns ticks)</param>
/// <param name="timeout">The watchdog timeout value when the watchdog event occurred</param>
/// <param name="assembly">The assembly index of the managed code that caused the watchdog</param>
/// <param name="method">The method index of the managed code that caused the watchdog</param>
/// <param name="fSet">Determins if the method is intended to set or get the watchdog information</param>
/// <returns>
/// Returns FALSE if fSet is FALSE and there are no logged watchdog events.
/// Returns FALSE if fSet is TRUE and the watchdog event could not be saved.
/// Returns TRUE otherwise.
/// </returns>
BOOL              Watchdog_LastOccurence ( INT64& time, INT64& timeout, UINT32& assembly, UINT32& method, BOOL fSet );

//--//

///
///  Watchdog defaults - solutions can override these settings in their platform_selector.h files.
///
///  WATCHDOG_ENABLE - Determine if the watchdog should be enabled by default
///
#ifdef  PLATFORM_DEPENDENT_WATCHDOG_ENABLE
#define WATCHDOG_ENABLE    PLATFORM_DEPENDENT_WATCHDOG_ENABLE
#else
#define WATCHDOG_ENABLE    TRUE
#endif

///
///  WATCHDOG_TIMEOUT - Sets the default watchdog timeout value (defaults to 60 seconds)
///
#ifdef  PLATFORM_DEPENDENT_WATCHDOG_TIMEOUT
#define WATCHDOG_TIMEOUT   PLATFORM_DEPENDENT_WATCHDOG_TIMEOUT
#else
#define WATCHDOG_TIMEOUT   60 * 1000 // 60 secs
#endif

///
///  WATCHDOG_BEHAVIOR - Sets the default watchdog behavior (defaults to hard reboot)
///
#ifdef  PLATFORM_DEPENDENT_WATCHDOG_BEHAVIOR
#define WATCHDOG_BEHAVIOR  PLATFORM_DEPENDENT_WATCHDOG_BEHAVIOR
#else
#define WATCHDOG_BEHAVIOR Watchdog_Behavior__HardReboot
#endif

///
///  WATCHDOG_RETRY_COUNT - Sets the maximum number of watchdog resets (within the WATCHDOG_RETRY_TIMEOUT)
///  before the driver tries to resolve the problem by first increasing the watchdog timeout, 
///  then by resetting the device.
///
#ifdef  PLATFORM_DEPENDENT_WATCHDOG_RETRY_COUNT
#define WATCHDOG_RETRY_COUNT    PLATFORM_DEPENDENT_WATCHDOG_RETRY_COUNT
#else
#define WATCHDOG_RETRY_COUNT    5
#endif

///
///  WATCHDOG_RETRY_TIMEOUT - Sets the maximum number of seconds between watchdogs before the WATCHOG_RETRY_COUNT is incremented.
///
#ifdef  PLATFORM_DEPENDENT_WATCHDOG_RETRY_TIMEOUT
#define WATCHDOG_RETRY_TIMEOUT_SECONDS  PLATFORM_DEPENDENT_WATCHDOG_RETRY_TIMEOUT
#else
#define WATCHDOG_RETRY_TIMEOUT_SECONDS  5
#endif

//--//

#endif // _DRIVERS_CPU_WATCHDOG_DECL_H_

