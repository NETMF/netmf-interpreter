////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#if !defined(__GNUC__)
#include <rt_fp.h>
#endif

extern "C" void SystemClock_Config(void);

//--//

//extern UINT32 _end;

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
                
                    #ifdef FEATURE_WATCHDOG
                        ::Watchdog_ResetCounter();
                    #endif

                    if(deviceInfo->Attribute.SupportsXIP)
                    {

                        // will be either directly read from  NOR
                    #ifdef FEATURE_CPUCACHE            
                        dataAddress = (volatile UINT32*)CPU_GetUncachableAddress(&pAddr[i]);
                    #else
                        dataAddress = (volatile UINT32*)&pAddr[i];
                    #endif                        

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

            #ifdef FEATURE_WATCHDOG
                ::Watchdog_ResetCounter();
            #endif

            BYTE *data  = (BYTE*)  private_malloc(pBlockRegionInfo->BytesPerBlock);

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

                #ifdef FEATURE_WATCHDOG
                    ::Watchdog_ResetCounter();
                #endif

                // write back to sector, as we only change one bit from 0 to 1, no need to erase sector
                bRet = (TRUE == pBlockDevice->Write( configSectAddress, pBlockRegionInfo->BytesPerBlock, data, FALSE ));

                private_free(data);
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
    
#ifdef FEATURE_PALEVENT
    Events_Initialize();
#endif    

#ifdef FEATURE_GPIO
    CPU_GPIO_Initialize();
#endif

#ifdef FEATURE_SPI    
    CPU_SPI_Initialize();
#endif 

#if !defined(PLATFORM_ARM_OS_PORT)
    // this is the place where interrupts are enabled after boot for the first time after boot
    ENABLE_INTERRUPTS();
#endif

    // have to initialize the blockstorage first, as the USB device needs to update the configure block

    BlockStorageList::Initialize();

    BlockStorage_AddDevices();

    BlockStorageList::InitializeDevices();

#ifdef FEATURE_FS
    FS_Initialize();

    FileSystemVolumeList::Initialize();

    FS_AddVolumes();

    FileSystemVolumeList::InitializeVolumes();
#endif

#ifdef FEATURE_LCD
    LCD_Initialize();
#endif    
    
    CPU_InitializeCommunication();

#ifdef FEATURE_I2C
    I2C_Initialize();
#endif    

#ifdef FEATURE_BUTTONS
    Buttons_Initialize();
#endif    

#ifdef FEATURE_BACKLIGHT    
    // Initialize the backlight to a default off state
    BackLight_Initialize();
#endif

#ifdef FEATURE_PIEZO
    Piezo_Initialize();
#endif

#ifdef FEATURE_BATTERY
    Battery_Initialize();
#endif
#ifdef FEATURE_CHARGER    
    Charger_Initialize();
#endif

#ifdef FEATURE_PALEVENT
    PalEvent_Initialize();
#endif    
    
#ifdef FEATURE_GESTURE 
    Gesture_Initialize();
#endif

#ifdef FEATURE_INK    
    Ink_Initialize();
#endif    
   
#ifdef FEATURE_TIMESERVICE    
    TimeService_Initialize();
#endif

#if defined(ENABLE_NATIVE_PROFILER)
    Native_Profiler_Init();
#endif

#ifdef FEATURE_HARDWARE_CRC
    // enable clock for the CRC calculation module
    __HAL_RCC_CRC_CLK_ENABLE();
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

#ifdef FEATURE_HARDWARE_CRC
    // disable clock for the CRC calculation module
    __HAL_RCC_CRC_CLK_DISABLE();
#endif

#ifdef FEATURE_LCD
    LCD_Uninitialize();
#endif

#ifdef FEATURE_I2C
    I2C_Uninitialize();
#endif

#ifdef FEATURE_BUTTONS
    Buttons_Uninitialize();
#endif

#ifdef FEATURE_BACKLIGHT
    // Initialize the backlight to a default off state
    BackLight_Uninitialize();
#endif    

#ifdef FEATURE_PIEZO
    Piezo_Uninitialize();
#endif

#ifdef FEATURE_BATTERY
    Battery_Uninitialize();
#endif

#ifdef FEATURE_CHARGER    
    Charger_Uninitialize();
#endif    

    TimeService_UnInitialize();
#ifdef FEATURE_INK    
    Ink_Uninitialize();
#endif    

#ifdef FEATURE_GESTURE    
    Gesture_Uninitialize();
#endif
    
#ifdef FEATURE_PALEVENT
    PalEvent_Uninitialize();
#endif 

    SOCKETS_CloseConnections();

#if !defined(HAL_REDUCESIZE)
    CPU_UninitializeCommunication();
#endif

#ifdef FEATURE_FS
    FileSystemVolumeList::UninitializeVolumes();
#endif

    BlockStorageList::UnInitializeDevices();

    USART_CloseAllPorts();

#ifdef FEATURE_SPI
    CPU_SPI_Uninitialize();
#endif

    HAL_UnReserveAllGpios();

#ifdef FEATURE_GPIO
    CPU_GPIO_Uninitialize();
#endif

    DISABLE_INTERRUPTS();

#ifdef FEATURE_PALEVENT
    Events_Uninitialize();
#endif
    
    Time_Uninitialize();

    HAL_CONTINUATION::Uninitialize();
    HAL_COMPLETION  ::Uninitialize();
}

extern "C"
{
// defined as weak to allow it to be overriden by equivalent function at Solution level  
__attribute__((weak)) int main(void)
{
    /* CMSIS HAL library initialization:
       - Configure the Flash prefetch, instruction and Data caches
       - Configure the Systick to generate an interrupt each 1 msec
       - Set NVIC Group Priority to 4
       - Global MSP (MCU Support Package) initialization
    */
    if(HAL_Init() == HAL_OK)
    {
        // debug enable options
        #if !defined(BUILD_RTM)     
            HAL_DBGMCU_EnableDBGSleepMode();
            HAL_DBGMCU_EnableDBGStopMode();
            HAL_DBGMCU_EnableDBGStandbyMode();
        #endif
                
        /* Configure the system clock */
        SystemClock_Config();

        HAL_Time_Initialize();

        HAL_Initialize();
  
        // check if system is waking-up from STANDBY power mode 
        if (__HAL_PWR_GET_FLAG(PWR_FLAG_SB) != RESET)
        {
            // TODO
            // we may want to handle this to pass it somehow to the NETMF API
        }
        
        #if !defined(BUILD_RTM) 
            //DEBUG_TRACE4( STREAM_LCD, ".NetMF v%d.%d.%d.%d\r\n", VERSION_MAJOR, VERSION_MINOR, VERSION_BUILD, VERSION_REVISION);
            //DEBUG_TRACE3(TRACE_ALWAYS, "%s, Build Date:\r\n\t%s %s\r\n", HalName, __DATE__, __TIME__);
            
            #if defined(__GNUC__)
                //DEBUG_TRACE1(TRACE_ALWAYS, "GNU Compiler version %d\r\n", __GNUC__);
            #else
                //DEBUG_TRACE1(TRACE_ALWAYS, "ARM Compiler version %d\r\n", __ARMCC_VERSION);
            #endif

            UINT8* BaseAddress;
            UINT32 SizeInBytes;

            HeapLocation( BaseAddress,    SizeInBytes );
            memset      ( BaseAddress, 0, SizeInBytes );

            #ifdef FEATURE_LCD
                lcd_printf("\f");
            
                lcd_printf("%-15s\r\n", HalName);
                lcd_printf("%-15s\r\n", "Build Date:");
                lcd_printf("  %-13s\r\n", __DATE__);
                lcd_printf("  %-13s\r\n", __TIME__);
            #endif

        #endif  // !defined(BUILD_RTM)

        /***********************************************************************************/

        {
            #if defined(FIQ_SAMPLING_PROFILER)
                    FIQ_Profiler_Init();
            #endif
        }

        #ifdef FEATURE_WATCHDOG
            Watchdog_GetSetTimeout ( WATCHDOG_TIMEOUT , TRUE );
            Watchdog_GetSetBehavior( WATCHDOG_BEHAVIOR, TRUE );
            Watchdog_GetSetEnabled ( WATCHDOG_ENABLE, TRUE );
        #endif
            
        // CMSIS & NETMF HALs initialization completed.  Interrupts are enabled.  Jump to the Application routine
        ApplicationEntryPoint();

        #ifdef FEATURE_LCD
            lcd_printf("\fmain exited!!???.  Halting CPU\r\n");
        #endif    
        //debug_printf("main exited!!???.  Halting CPU\r\n");        
    }    

    #if defined(BUILD_RTM)
        CPU_Reset();
    #else
        CPU_Halt();
    #endif
}

} // extern "C"

//--//

#if !defined(BUILD_RTM)

void debug_printf( const char* format, ... )
{
    char buffer[256] = {0};
    va_list arg_ptr;

    va_start( arg_ptr, format );

   int len = hal_vsnprintf( buffer, sizeof(buffer)-1, format, arg_ptr );

   { // take CLR lock to send whole message
       GLOBAL_LOCK(clrLock);
       // send characters directly to the trace port
       for( char* p = buffer; *p != '\0' || p-buffer >= 256; ++p )
            ITM_SendChar( *p );
   }

    va_end( arg_ptr );
}

#ifdef FEATURE_LCD
void lcd_printf( const char* format, ... );

void lcd_printf( const char* format, ... )
{
    va_list arg_ptr;

    va_start( arg_ptr, format );

    hal_vfprintf( STREAM_LCD, format, arg_ptr );
}
#endif  // !defined(FEATURE_LCD)

#endif  // !defined(BUILD_RTM)
