////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <stdafx.h>
#include <iostream>

////////////////////////////////////////////////////////////////////////////////////////////////////

int HAL_Windows_IsShutdownPending() { return 0; }

void HAL_Windows_FastSleep(__int64) {}

// --- stubs required by Core.lib ----------------------

BOOL Watchdog_GetSetEnabled( BOOL enabled, BOOL fSet )
{
    return TRUE;
}

void Watchdog_ResetCounter() {}

void CPU_Reset() {}

unsigned int Events_MaskedRead( unsigned int ) { return 0; }

#if defined(PLATFORM_WINDOWS_EMULATOR)
void CLR_RT_EmulatorHooks::Notify_ExecutionStateChanged( void ) {}
#endif

unsigned int Events_WaitForEvents( unsigned int powerLevel, unsigned int,unsigned int ) { return 0; }

void Events_SetBoolTimer( int *,unsigned int ) {}

BOOL DebuggerPort_Flush( int ) { return FALSE; }

BOOL DebuggerPort_IsSslSupported( COM_HANDLE ComPortNum ) { return FALSE; }

BOOL DebuggerPort_UpgradeToSsl( COM_HANDLE ComPortNum, UINT32 flags ) { return FALSE; }

BOOL DebuggerPort_IsUsingSsl( COM_HANDLE ComPortNum )
{
    return FALSE;
}


// ----------------------------------------------------------

int __cdecl hal_vprintf( const char* format, va_list arg )
{
    return vprintf_s ( format, arg );
}

int __cdecl hal_printf( const char* format, ... )
{
    va_list arg_ptr;
    int     chars;
    
    va_start ( arg_ptr, format );

    chars = hal_vprintf( format, arg_ptr );

    va_end ( arg_ptr );

    return chars;
}

int __cdecl hal_vsnprintf( char* buffer, size_t len, const char* format, va_list arg )
{
    return _vsnprintf_s( buffer, len, len-1/* force space for trailing zero*/, format, arg );
}

int __cdecl hal_snprintf(char * buffer, unsigned int len, char const * format, ...)
{
    va_list arg_ptr;
    int     chars;

    va_start( arg_ptr, format );

    chars = hal_vsnprintf( buffer, len, format, arg_ptr );

    va_end( arg_ptr );

    return chars;
}

void __cdecl HAL_Windows_Debug_Print( char * txt )
{
    std::cerr << txt << std::endl;
}

UINT32 LCD_ConvertColor( UINT32 color )
{
    return color;
}


// ----------------------------------------------------------

#if !defined(BUILD_RTM)
void HARD_Breakpoint()
{
    if(::IsDebuggerPresent())
    {
        ::DebugBreak();
    }
}
#endif
