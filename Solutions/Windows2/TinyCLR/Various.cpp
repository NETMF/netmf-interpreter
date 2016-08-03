////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

//--//

using namespace System;
using namespace System::Diagnostics;
using namespace Microsoft::SPOT::Emulator;

BOOL HAL_Windows_IsShutdownPending()
{
    return EmulatorNative::GetEmulatorNative()->IsShuttingDown();
}

void HAL_Windows_AcquireGlobalLock()
{
    EmulatorNative::GetEmulatorNative()->DisableInterrupts();
}

void HAL_Windows_ReleaseGlobalLock()
{
    EmulatorNative::GetEmulatorNative()->EnableInterrupts();
}

BOOL HAL_Windows_HasGlobalLock()
{
    return !EmulatorNative::GetEmulatorNative()->AreInterruptsEnabled();
}

UINT64 HAL_Windows_GetPerformanceTicks()
{
    return EmulatorNative::GetEmulatorNative()->GetCurrentTicks();
}

void HAL_Windows_Debug_Print( LPSTR szText )
{
    System::Diagnostics::Debug::Print( gcnew System::String(szText) );
}


HAL_Configuration_Windows g_HAL_Configuration_Windows;

UINT32 LOAD_IMAGE_CalcCRC;

OEM_MODEL_SKU OEM_Model_SKU = { 1, 2, 3 };

HAL_SYSTEM_CONFIG HalSystemConfig =
{
    { TRUE }, // HAL_DRIVER_CONFIG_HEADER Header;

    //--//

    {                                               // UINT32      DebuggerPorts[MAX_DEBUGGERS];
        DEBUGGER_PORT,
    },

    {
        MESSAGING_PORT,
    },

    DEBUG_TEXT_PORT,
    115200,
    0,  // STDIO = COM2 or COM1

    { 0, 0 },   // { SRAM1_MEMORY_Base, SRAM1_MEMORY_Size },
    { 0, 0 },   // { FLASH_MEMORY_Base, FLASH_MEMORY_Size },
};

const ConfigurationSector g_ConfigurationSector =
{
    // ConfigurationLength
    offsetof(ConfigurationSector, FirstConfigBlock),

    //CONFIG_SECTOR_VERSION
    {
        ConfigurationSector::c_CurrentVersionMajor,
        ConfigurationSector::c_CurrentVersionMinor,
        ConfigurationSector::c_CurrentVersionTinyBooter,
        0, // extra
    },

    // backwards compatibility buffer (88 bytes to put booterflagarray at offset 96)
    {
        0x0,
    },

    // BooterFlagArray - determines if we enter the tinybooter or not
    {
        0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,
        0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,
        0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,
        0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,
        0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,
    },

    // UINT32 SectorSignatureCheck[9*8]; // 287 sectors max * 8 changes before erase
    {
        0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,
        0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,
        0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,
        0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,
        0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,
        0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,
        0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,
        0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,0xFFFFFFFF,
    },

    //TINYBOOTER_KEY_CONFIG DeploymentKey =
    { 
        {// ODM key configuration for programming firmware (non deployment sectors)
            { // ODM public key for firware sectors
                // exponent length
                0xFF,0xFF,0xFF, 0xFF,

                // module
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   

                // exponent
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   
            }
        },
        {// OEM key configuration for programming Deployment sector
            { // OEM public key for Deployment sector
                // exponent length
                0xFF,0xFF,0xFF, 0xFF,

                // module
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   

                // exponent
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,
                0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   0xFF,   
            }
        }
    },

    //--//--//--//

    // OEM_MODEL_SKU OEM_Model_SKU;
    {
        1,     // UINT8   OEM;
        2,     // UINT8   Model;
        3,     // UINT16  SKU;
    },

    //--//--//--//

    // OEM_SERIAL_NUMBERS OemSerialNumbers
    {
        { 0, },
        { 0, }    // UINT8 system_serial_number[16];
    },

    // CLR Config Data
    {
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 

        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 

        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 
    },

    //--//--//--//

    // HAL_CONFIG_BLOCK FirstConfigBlock;
    {
        HAL_CONFIG_BLOCK::c_Version_V2, // UINT32 Signature;
        0x8833794c,                     // UINT32 HeaderCRC;
        0x00000000,                     // UINT32 DataCRC;
        0x00000000,                     // UINT32 Size;
                                        // char   DriverName[64];
    },
};

////////////////////////////////////////////////////////////////////////////////////////////////////

static ON_SOFT_REBOOT_HANDLER s_rebootHandlers[5] = {NULL, NULL, NULL, NULL, NULL};

void __cdecl HAL_AddSoftRebootHandler(ON_SOFT_REBOOT_HANDLER handler)
{
    for(int i=0; i<ARRAYSIZE(s_rebootHandlers); i++)
    {
        if(s_rebootHandlers[i] == NULL)
        {
            s_rebootHandlers[i] = handler;
            return;
        }
        else if(s_rebootHandlers[i] == handler)
        {
            return;
        }
    }
}

bool g_fDoNotUninitializeDebuggerPort = false;

void __cdecl HAL_Initialize(void)
{
    // In the case of the Extensible Emulator, the work typically done here is
    // carried out in EmulatorNative::Settings::System_Start(), but this ep is
    // here to satisfy ClrStartup()
}

void __cdecl HAL_Uninitialize(void)
{
    int i;

    CPU_GPIO_Uninitialize();

    for(i=0; i<ARRAYSIZE(s_rebootHandlers); i++)
    {
        if(s_rebootHandlers[i] != NULL)
        {
            s_rebootHandlers[i]();
            return;
        }
    }       
}

void HAL_EnterBooterMode()
{
}

////////////////////////////////////////////////////////////////////////////////////////////////////

#if !defined(BUILD_RTM)
void __cdecl HARD_Breakpoint()
{
    if(::IsDebuggerPresent())
    {
        ::DebugBreak();
    }
}
#endif

////////////////////////////////////////////////////////////////////////////////////////////////////

HAL_Mutex::HAL_Mutex()
{
    ::InitializeCriticalSection( &m_data );
}

HAL_Mutex::~HAL_Mutex()
{
    ::DeleteCriticalSection( &m_data );
}

void HAL_Mutex::Lock()
{
    ::EnterCriticalSection( &m_data );
}

void HAL_Mutex::Unlock()
{
    ::LeaveCriticalSection( &m_data );
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void CPU_ChangePowerLevel(POWER_LEVEL level)
{
    switch(level)
    {
        case POWER_LEVEL__MID_POWER:
            break;

        case POWER_LEVEL__LOW_POWER:
            break;

        case POWER_LEVEL__HIGH_POWER:
        default:
            break;
    }
}

void CPU_Hibernate()
{
    INT64 start = ::HAL_Time_CurrentTime();

    while(true)
    {
        //wait on SYSTEM_EVENT_FLAG_DEBUGGER_ACTIVITY as well??
        UINT32 mask = ::Events_WaitForEvents( SLEEP_LEVEL__SLEEP, SYSTEM_EVENT_FLAG_COM_IN | SYSTEM_EVENT_HW_INTERRUPT | SYSTEM_EVENT_FLAG_BUTTON | SYSTEM_EVENT_FLAG_CHARGER_CHANGE, 1000 );

        if(mask)
        {
            break;
        }
    }
}

void CPU_Shutdown()
{
    ClrExit();
}

void
CPU_Reset()
{
    ::ExitProcess( 0 );
}

BOOL CPU_IsSoftRebootSupported ()
{
    return TRUE;
}

char TinyClr_Dat_Start[512*1024];
char TinyClr_Dat_End  [1       ];

////////////////////////////////////////////////////////////////////////////////////////////////////

BOOL Piezo_Tone( UINT32 Frequency_Hertz, UINT32 Duration_Milliseconds )
{
    return TRUE;
}

BOOL Piezo_IsEnabled()
{
    return FALSE;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void SecurityKey_Copy( UINT8 KeyCopy[], INT32 BytesToCopy )
{
    memset( KeyCopy, 0, BytesToCopy );
}

void SecurityKey_LowLevelCopy( UINT8 KeyCopy[], INT32 BytesToCopy )
{
    memset( KeyCopy, 0, BytesToCopy );
}

void SecurityKey_Print()
{
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void HAL_Windows_FastSleep( INT64 ticks )
{
    LARGE_INTEGER frequency;
    LARGE_INTEGER countStart;
    LARGE_INTEGER countEnd;

    if(ticks > 0)
    {
        ::QueryPerformanceFrequency( &frequency  );

        ::QueryPerformanceCounter  ( &countStart );

        double ratio = (double)TIME_CONVERSION__TO_SECONDS / (double)frequency.QuadPart;

        countEnd.QuadPart = (INT64)(((countStart.QuadPart * ratio) + ticks) / ratio);

        while(countStart.QuadPart < countEnd.QuadPart)
        {
            ::QueryPerformanceCounter( &countStart );
        }
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////

#pragma managed(push, off)

void debug_printf( char const* format, ... )
{
    va_list arg_ptr;

    va_start( arg_ptr, format );

    int chars = hal_vprintf( format, arg_ptr );

    va_end( arg_ptr );
}

void lcd_printf( char const * format, ... )
{
    va_list arg_ptr;

    va_start( arg_ptr, format );

    int chars = hal_vprintf( format, arg_ptr );

    va_end( arg_ptr );
}

int hal_printf( const char* format, ... )
{
    va_list arg_ptr;

    va_start(arg_ptr, format);

    int chars = hal_vprintf( format, arg_ptr );

    va_end( arg_ptr );

    return chars;
}

int hal_vprintf( const char* format, va_list arg )
{
    return vprintf( format, arg );
}


int hal_fprintf( COM_HANDLE stream, const char* format, ... )
{
    va_list arg_ptr;
    int     chars;

    va_start( arg_ptr, format );

    chars = hal_vfprintf( stream, format, arg_ptr );

    va_end( arg_ptr );

    return chars;
}

int hal_vfprintf( COM_HANDLE stream, const char* format, va_list arg )
{
    char buffer[512];
    int chars = 0;

    chars = hal_vsnprintf( buffer, sizeof(buffer), format, arg );

    switch(ExtractTransport(stream))
    {
    default:
        DebuggerPort_Write( stream, buffer, chars, 0 ); // skip null terminator
        break;

    case LCD_TRANSPORT:
        break;

    case FLASH_WRITE_TRANSPORT:
        _ASSERTE(FALSE);
    }

    return chars;
}

int hal_snprintf( char* buffer, size_t len, const char* format, ... )
{
    va_list arg_ptr;
    int     chars;

    va_start( arg_ptr, format );

    chars = hal_vsnprintf( buffer, len, format, arg_ptr );

    va_end( arg_ptr );

    return chars;
}

int hal_vsnprintf( char* buffer, size_t len, const char* format, va_list arg )
{
    return _vsnprintf_s( buffer, len, len-1/* force space for trailing zero*/, format, arg );
}

#pragma managed(pop)

///////////////////////////////////////////////////////////////

size_t CPU_GetCachableAddress( size_t address )
{
    return address;
}

//--//

size_t CPU_GetUncachableAddress( size_t address )
{
    return address;
}

///////////////////////////////////////////////////////////////

BOOL Charger_Status( UINT32& Status )
{
    return TRUE;
}

///////////////////////////////////////////////////////////////

SmartPtr_IRQ::SmartPtr_IRQ(void* context)
{
    m_context = context;
    Disable();
}

SmartPtr_IRQ::~SmartPtr_IRQ()
{
    Restore();
}

void SmartPtr_IRQ::Release()
{
    HAL_Windows_ReleaseGlobalLock();
}

void SmartPtr_IRQ::Disable()
{
    HAL_Windows_AcquireGlobalLock();
}

void SmartPtr_IRQ::Restore()
{
    HAL_Windows_ReleaseGlobalLock();
}

BOOL SmartPtr_IRQ::ForceDisabled(void* context)
{
    BOOL ret = GetState();

    HAL_Windows_AcquireGlobalLock();

    return ret;
}

BOOL SmartPtr_IRQ::ForceEnabled(void* context)
{
    BOOL ret = GetState();

    HAL_Windows_ReleaseGlobalLock();

    return ret;
}


BOOL SmartPtr_IRQ::GetState(void* context)
{
    return HAL_Windows_HasGlobalLock();
}
