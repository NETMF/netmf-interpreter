////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_HW_Hardware::CreateInstance()
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    TINYCLR_CLEAR(g_CLR_HW_Hardware);

    g_CLR_HW_Hardware.m_fInitialized = false;

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT CLR_HW_Hardware::Hardware_Initialize()
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    if(m_fInitialized == false)
    {
        Time_Initialize();

        m_interruptData.m_HalQueue.Initialize( (CLR_HW_Hardware::HalInterruptRecord*)&g_scratchInterruptDispatchingStorage, InterruptRecords() );

        m_interruptData.m_applicationQueue.DblLinkedList_Initialize ();

        m_interruptData.m_queuedInterrupts = 0;

        m_DebuggerEventsMask  = 0;
        m_MessagingEventsMask = 0;

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        for(int i = 0; i < HalSystemConfig.c_MaxDebuggers; i++)
        {
            m_DebuggerEventsMask |= ExtractEventFromTransport( HalSystemConfig.DebuggerPorts[ i ] );
        }
#endif

        for(int i = 0; i < HalSystemConfig.c_MaxMessaging; i++)
        {
            m_MessagingEventsMask |= ExtractEventFromTransport( HalSystemConfig.MessagingPorts[ i ] );
        }

        m_wakeupEvents = c_Default_WakeupEvents | m_DebuggerEventsMask;
        m_powerLevel   = PowerLevel__Active;
            
        m_fInitialized = true;
    }

    TINYCLR_NOCLEANUP_NOLABEL();
}

//--//

HRESULT CLR_HW_Hardware::DeleteInstance()
{
    NATIVE_PROFILE_CLR_HARDWARE();
    TINYCLR_HEADER();

    g_CLR_HW_Hardware.Hardware_Cleanup();

    TINYCLR_NOCLEANUP_NOLABEL();
}

void CLR_HW_Hardware::Hardware_Cleanup()
{
    NATIVE_PROFILE_CLR_HARDWARE();
    if(m_fInitialized == true)
    {
        m_fInitialized = false;
    }
}

//--//

void CLR_HW_Hardware::PrepareForGC()
{
    NATIVE_PROFILE_CLR_HARDWARE();
}

void CLR_HW_Hardware::ProcessActivity()
{
    NATIVE_PROFILE_CLR_HARDWARE();

    for(int i=0; i<10; i++)
    {
        if(!HAL_CONTINUATION::Dequeue_And_Execute()) break;
    }

    TINYCLR_FOREACH_MESSAGING(msg)
    {
        if(!msg.IsDebuggerInitialized())
        {
            msg.InitializeDebugger();
        }
        msg.PurgeCache();
    }
    TINYCLR_FOREACH_MESSAGING_END();
    
    TINYCLR_FOREACH_DEBUGGER(dbg)
    {
        dbg.PurgeCache();
    }
    TINYCLR_FOREACH_DEBUGGER_END();

    UINT32 events    = ::Events_Get( m_wakeupEvents );    
    UINT32 eventsCLR = 0;

    if(events & m_MessagingEventsMask)
    {
        TINYCLR_FOREACH_MESSAGING(msg)
        {
            msg.ProcessCommands();
        }
        TINYCLR_FOREACH_MESSAGING_END();
    }

    if(events & m_DebuggerEventsMask)
    {
        TINYCLR_FOREACH_DEBUGGER(dbg)
        {
            dbg.ProcessCommands();
        }
        TINYCLR_FOREACH_DEBUGGER_END();

#if defined(PLATFORM_ARM)
        if(CLR_EE_DBG_IS(RebootPending))
        {
#if !defined(BUILD_RTM)
            CLR_Debug::Printf( "Rebooting...\r\n" );
#endif

            if(!CLR_EE_REBOOT_IS(ClrOnly))
            {
                CLR_RT_ExecutionEngine::Reboot( true );
            }
        }
#endif
    }

    if( events & (SYSTEM_EVENT_FLAG_COM_IN | SYSTEM_EVENT_FLAG_COM_OUT) )
    {
        eventsCLR |= CLR_RT_ExecutionEngine::c_Event_SerialPort;
    }

    if(events & SYSTEM_EVENT_I2C_XACTION)
    {                    
        eventsCLR |= CLR_RT_ExecutionEngine::c_Event_I2C;
    }

    if((events & SYSTEM_EVENT_HW_INTERRUPT)
#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        || (!CLR_EE_DBG_IS(Stopped) && !g_CLR_HW_Hardware.m_interruptData.m_applicationQueue.IsEmpty())
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        )
    {
        ProcessInterrupts();
    }

    if(events & SYSTEM_EVENT_FLAG_SOCKET)
    {
        eventsCLR |= CLR_RT_ExecutionEngine::c_Event_Socket;
    }

    if(events & SYSTEM_EVENT_FLAG_IO)
    {
        eventsCLR |= CLR_RT_ExecutionEngine::c_Event_IO;
    }

    if(events & SYSTEM_EVENT_FLAG_CHARGER_CHANGE)
    {
        static UINT32 lastStatus;
        UINT32        status;

        if(::Charger_Status( status ))
        {
            status &= CHARGER_STATUS_ON_AC_POWER;

            if(lastStatus != status)
            {
                lastStatus = status;

                eventsCLR |= CLR_RT_ExecutionEngine::c_Event_Battery;
            }
        }
    }

    if(eventsCLR)
    {
        g_CLR_RT_ExecutionEngine.SignalEvents( eventsCLR );
    }
}

//--//

void CLR_HW_Hardware::Screen_Flush( CLR_GFX_Bitmap& bitmap, CLR_UINT16 x, CLR_UINT16 y, CLR_UINT16 width, CLR_UINT16 height )
{
    NATIVE_PROFILE_CLR_HARDWARE();
    CLR_INT32 widthMax  = LCD_SCREEN_WIDTH;
    CLR_INT32 heightMax = LCD_SCREEN_HEIGHT;

    if((CLR_UINT32)(x + width)  > bitmap.m_bm.m_width ) width  = bitmap.m_bm.m_width  - x;
    if((CLR_UINT32)(y + height) > bitmap.m_bm.m_height) height = bitmap.m_bm.m_height - y;    

    if(bitmap.m_bm.m_width                 != widthMax                              ) return;
    if(bitmap.m_bm.m_height                != heightMax                             ) return;
    if(bitmap.m_bm.m_bitsPerPixel          != CLR_GFX_BitmapDescription::c_NativeBpp) return;
    if(bitmap.m_palBitmap.transparentColor != PAL_GFX_Bitmap::c_InvalidColor        ) return;

    LCD_BitBltEx( x, y, width, height, bitmap.m_palBitmap.data );
}

//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

