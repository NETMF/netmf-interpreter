////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#if !defined(__GNUC__)
#include <rt_fp.h>
#endif

//--//

#undef  TRACE_ALWAYS
#define TRACE_ALWAYS               0x00000001

#undef  DEBUG_TRACE
#define DEBUG_TRACE (TRACE_ALWAYS)


//--//

#if !defined(BUILD_RTM)
static UINT32 g_Boot_RAMConstants_CRC = 0;
#endif

static ON_SOFT_REBOOT_HANDLER s_rebootHandlers[16] = {NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL};

void HAL_AddSoftRebootHandler(ON_SOFT_REBOOT_HANDLER handler)
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

    ASSERT(FALSE);
}


void HAL_EnterBooterMode()
{
    const UINT32        c_Empty     = 0xFFFFFFFF;
    const UINT32        c_Key       = ConfigurationSector::c_BootEntryKey;

    volatile UINT32*    pAddr       = (volatile UINT32 *)&g_ConfigurationSector.BooterFlagArray[0];
    bool                bDone       = false;


    bool bRet = false;
    volatile UINT32* dataAddress;
    BlockStorageDevice *pBlockDevice = BlockStorageList::s_primaryDevice;

    if ( pBlockDevice != NULL)
    {
        const BlockDeviceInfo* deviceInfo = pBlockDevice->GetDeviceInfo();

        ByteAddress configSectAddress;
        UINT32        iRegion, iRange;

        pBlockDevice->FindForBlockUsage(BlockUsage::CONFIG, configSectAddress, iRegion, iRange );

        // if non-XIP, load data
        if (!deviceInfo->Attribute.SupportsXIP)
        {
            pBlockDevice->Read(configSectAddress, sizeof(ConfigurationSector), (BYTE*)&g_ConfigurationSector);
        }


        for(int i=0; !bDone && i<ConfigurationSector::c_MaxBootEntryFlags; i++ )
        {
            switch(pAddr[i])
            {
                case c_Empty:
                    ::Watchdog_ResetCounter();

                    if(deviceInfo->Attribute.SupportsXIP)
                    {

                        // will be either directly read from  NOR
                        dataAddress = (volatile UINT32*)CPU_GetUncachableAddress(&pAddr[i]);

                        // write directly
                        bRet = (TRUE == pBlockDevice->Write( (UINT32)dataAddress, sizeof(UINT32), (PBYTE)&c_Key, FALSE ));
                    }
                    else
                    {
                        // updated the g_ConfigurationSector with the latest value
                        // as the g_ConfigurationSector must be at the RAM area, so it should be ok to write to it.

                        dataAddress =(volatile UINT32 *) &(g_ConfigurationSector.BooterFlagArray[i]);
                        (*dataAddress) = c_Key;

                        // write back to sector, as we only change one bit from 0 to 1, no need to erase sector
                        bRet = (TRUE == pBlockDevice->Write( configSectAddress, sizeof(ConfigurationSector), (BYTE*)&g_ConfigurationSector, FALSE ));

                    }

                    bDone = true;
                    break;

                case ConfigurationSector::c_BootEntryKey: // looks like we already have a key set
                    bDone = true;
                    break;
            }
        }

        if(!bDone) // we must be full, so rewrite sector
        {
            // reading whole block, not just the configurationsector
            const BlockRegionInfo * pBlockRegionInfo = &deviceInfo->Regions[iRegion];

            ::Watchdog_ResetCounter();

            BYTE *data  = (BYTE*)  malloc(pBlockRegionInfo->BytesPerBlock);

            if(data != NULL)
            {
                ConfigurationSector *pCfg;

                if(deviceInfo->Attribute.SupportsXIP)
                {
                    memcpy(data, (void*)configSectAddress, pBlockRegionInfo->BytesPerBlock);
                }
                else
                {
                    pBlockDevice->Read(configSectAddress, pBlockRegionInfo->BytesPerBlock, data);
                }

                pCfg = (ConfigurationSector *)data;
                memset( (void*)&pCfg->BooterFlagArray[0], 0xFF, sizeof(pCfg->BooterFlagArray) );

                // updated the g_ConfigurationSector with the latest value
                // as the g_ConfigurationSector must be at the RAM area, so it should be ok to write to it.
                pCfg->BooterFlagArray[0]  = c_Key;

                pBlockDevice->EraseBlock(configSectAddress);

                ::Watchdog_ResetCounter();

                // write back to sector, as we only change one bit from 0 to 1, no need to erase sector
                bRet = (TRUE == pBlockDevice->Write( configSectAddress, pBlockRegionInfo->BytesPerBlock, data, FALSE ));

                free(data);
            }
        }

        CPU_Reset();
    }
}

bool g_fDoNotUninitializeDebuggerPort = false;

void HAL_Initialize()
{ 
#if defined(PLATFORM_ARM_OS_PORT)
    // Interrupts must be enabled to handle calls to OS
    // (Network stack uses the CMSIS-RTX OS, which uses
    // SVC calls, which will hard fault if the interrupts
    // are disabled at the Svc instruction )
    // SystemInit handles this for the startup from reset
    // However, this is also called from the CLR when doing
    // a soft reboot.
    __enable_irq();
#endif

    HAL_CONTINUATION::InitializeList();
    HAL_COMPLETION  ::InitializeList();

    Time_Initialize();
    Events_Initialize();

    CPU_GPIO_Initialize();
    CPU_SPI_Initialize();

#if !defined(PLATFORM_ARM_OS_PORT)
    // this is the place where interrupts are enabled after boot for the first time after boot
    ENABLE_INTERRUPTS();
#endif

    // have to initialize the blockstorage first, as the USB device needs to update the configure block

    BlockStorageList::Initialize();

    BlockStorage_AddDevices();

    BlockStorageList::InitializeDevices();

    FS_Initialize();

    FileSystemVolumeList::Initialize();

    FS_AddVolumes();

    FileSystemVolumeList::InitializeVolumes();

    LCD_Initialize();
    
    CPU_InitializeCommunication();

    I2C_Initialize();

    Buttons_Initialize();

    // Initialize the backlight to a default off state
    BackLight_Initialize();

    Piezo_Initialize();

    Battery_Initialize();

    Charger_Initialize();

    PalEvent_Initialize();
    Gesture_Initialize();
    Ink_Initialize();
    TimeService_Initialize();

#if defined(ENABLE_NATIVE_PROFILER)
    Native_Profiler_Init();
#endif
}

void HAL_UnReserveAllGpios()
{
    for(INT32 i = CPU_GPIO_GetPinCount()-1; i >=0; i--)
    {
        CPU_GPIO_ReservePin((GPIO_PIN)i, false);
    }
}

void HAL_Uninitialize()
{
    int i;
    
#if defined(ENABLE_NATIVE_PROFILER)
    Native_Profiler_Stop();
#endif
 
    for(i=0; i<ARRAYSIZE(s_rebootHandlers); i++)
    {
        if(s_rebootHandlers[i] != NULL)
        {
            s_rebootHandlers[i]();
        }
        else
        {
            break;
        }
    }    

    LCD_Uninitialize();

    I2C_Uninitialize();

    Buttons_Uninitialize();

    // Initialize the backlight to a default off state
    BackLight_Uninitialize();

    Piezo_Uninitialize();

    Battery_Uninitialize();
    Charger_Uninitialize();

    TimeService_UnInitialize();
    Ink_Uninitialize();
    Gesture_Uninitialize();
    PalEvent_Uninitialize();

    SOCKETS_CloseConnections();

#if !defined(HAL_REDUCESIZE)
    CPU_UninitializeCommunication();
#endif

    FileSystemVolumeList::UninitializeVolumes();

    BlockStorageList::UnInitializeDevices();

    USART_CloseAllPorts();

    CPU_SPI_Uninitialize();

    HAL_UnReserveAllGpios();

    CPU_GPIO_Uninitialize();

    DISABLE_INTERRUPTS();

    Events_Uninitialize();
    Time_Uninitialize();

    HAL_CONTINUATION::Uninitialize();
    HAL_COMPLETION  ::Uninitialize();
}

extern "C"
{
// defined as weak to allow it to be overriden by equivalent function at Solution level  
__attribute__((weak)) int main(void)
{
    HAL_Time_Initialize();

    HAL_Initialize();



#if !defined(BUILD_RTM) 
    //DEBUG_TRACE4( STREAM_LCD, ".NetMF v%d.%d.%d.%d\r\n", VERSION_MAJOR, VERSION_MINOR, VERSION_BUILD, VERSION_REVISION);
    //DEBUG_TRACE3(TRACE_ALWAYS, "%s, Build Date:\r\n\t%s %s\r\n", HalName, __DATE__, __TIME__);
    
    #if defined(__GNUC__)
        //DEBUG_TRACE1(TRACE_ALWAYS, "GNU Compiler version %d\r\n", __GNUC__);
    #else
        //DEBUG_TRACE1(TRACE_ALWAYS, "ARM Compiler version %d\r\n", __ARMCC_VERSION);
    #endif

    debug_printf("%-15s\r\n", HalName);
    debug_printf("%-15s\r\n", "Build Date:");
    debug_printf("  %-13s\r\n", __DATE__);
    debug_printf("  %-13s\r\n", __TIME__);

#endif  // !defined(BUILD_RTM)

    /***********************************************************************************/

    {
#if defined(FIQ_SAMPLING_PROFILER)
        FIQ_Profiler_Init();
#endif
    }

    Watchdog_GetSetTimeout ( WATCHDOG_TIMEOUT , TRUE );
    Watchdog_GetSetBehavior( WATCHDOG_BEHAVIOR, TRUE );
    Watchdog_GetSetEnabled ( WATCHDOG_ENABLE, TRUE );
        
    // HAL initialization completed.  Interrupts are enabled.  Jump to the Application routine
    ApplicationEntryPoint();

    lcd_printf("\fmain exited!!???.  Halting CPU\r\n");
    //debug_printf("main exited!!???.  Halting CPU\r\n");        

#if defined(BUILD_RTM)
    CPU_Reset();
#else
    CPU_Halt();
#endif
}

} // extern "C"

extern "C" void BootstrapCode();

// performs base level system initialization
// This typically consists of setting up clocks
// and PLLs along with any external memory needed
// to boot. 
// NOTE:
// It is important to keep in mind that this is 
// called *BEFORE* any C/C++ runtime initialization
// That is, zero init of uninitialied writeable data
// and copying of initialized values for initialized 
// writeable data have not yet occured. Thus, any code
// called from SystemInit must not use or rely on 
// initializtion having occured. This also precludes
// the use of any OS provided primitives and support
// as the kernel isn't initialized yet either.
extern "C" void SystemInit()
{
    BootstrapCode();
    CPU_Initialize();
    __enable_irq();
}

//--//

#if !defined(BUILD_RTM)

void debug_printf( const char* format, ... )
{
    char    buffer[256];
    va_list arg_ptr;

    va_start( arg_ptr, format );

   int len = hal_vsnprintf( buffer, sizeof(buffer)-1, format, arg_ptr );

    // flush existing characters
    DebuggerPort_Flush( HalSystemConfig.DebugTextPort );

    // write string
    DebuggerPort_Write( HalSystemConfig.DebugTextPort, buffer, len, 0 );

    // flush new characters
    DebuggerPort_Flush( HalSystemConfig.DebugTextPort );

    va_end( arg_ptr );
}

void lcd_printf( const char* format, ... )
{
    va_list arg_ptr;

    va_start( arg_ptr, format );

    hal_vfprintf( STREAM_LCD, format, arg_ptr );
}

#endif  // !defined(BUILD_RTM)

//--//

volatile INT32 SystemStates[SYSTEM_STATE_TOTAL_STATES];


#if defined(PLATFORM_ARM)

 void SystemState_SetNoLock( SYSTEM_STATE State )
{
    //ASSERT(State < SYSTEM_STATE_TOTAL_STATES);

    ASSERT_IRQ_MUST_BE_OFF();

    SystemStates[State]++;

    //ASSERT(SystemStates[State] > 0);
}


void SystemState_ClearNoLock( SYSTEM_STATE State )
{
    //ASSERT(State < SYSTEM_STATE_TOTAL_STATES);

    ASSERT_IRQ_MUST_BE_OFF();

    SystemStates[State]--;

    //ASSERT(SystemStates[State] >= 0);
}


 BOOL SystemState_QueryNoLock( SYSTEM_STATE State )
{
    //ASSERT(State < SYSTEM_STATE_TOTAL_STATES);

    ASSERT_IRQ_MUST_BE_OFF();

    return (SystemStates[State] > 0) ? TRUE : FALSE;
}

#endif



void SystemState_Set( SYSTEM_STATE State )
{
    GLOBAL_LOCK(irq);

    SystemState_SetNoLock( State );
}


void SystemState_Clear( SYSTEM_STATE State )
{
    GLOBAL_LOCK(irq);

    SystemState_ClearNoLock( State );
}


BOOL SystemState_Query( SYSTEM_STATE State )
{
    GLOBAL_LOCK(irq);

    return SystemState_QueryNoLock( State );
}
