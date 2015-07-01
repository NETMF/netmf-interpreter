////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#if !defined(__GNUC__)
#include <rt_fp.h>
#endif

//--//

// we need this to force inclusion from library at link time
#pragma import(EntryPoint)


#undef  TRACE_ALWAYS
#define TRACE_ALWAYS               0x00000001

#undef  DEBUG_TRACE
#define DEBUG_TRACE (TRACE_ALWAYS)

//--//

#if !defined(BUILD_RTM) && !defined(PLATFORM_ARM_OS_PORT)

UINT32 Stack_MaxUsed()
{
    // this is the value we check for stack overruns
    const UINT32 StackCheckVal = 0xBAADF00D;

    size_t  size = (size_t)&StackTop - (size_t)&StackBottom;
    UINT32* ptr  = (UINT32*)&StackBottom;

    DEBUG_TRACE1(TRACE_ALWAYS, "Stack Max  = %d\r\n", size);

    while(*ptr == StackCheckVal)
    {
        size -= 4;
        ptr++;
    }

    DEBUG_TRACE1(TRACE_ALWAYS, "Stack Used = %d\r\n", size);

    return size;
}

#endif  // !defined(BUILD_RTM)

//--//
// this is the first C function called after bootstrapping ourselves into ram

// these define the region to zero initialize
extern UINT32 Image$$ER_RAM_RW$$ZI$$Base;
extern UINT32 Image$$ER_RAM_RW$$ZI$$Length;

// here is the execution address/length of code to move from FLASH to RAM
#define IMAGE_RAM_RO_BASE   Image$$ER_RAM_RO$$Base
#define IMAGE_RAM_RO_LENGTH Image$$ER_RAM_RO$$Length

extern UINT32 IMAGE_RAM_RO_BASE;
extern UINT32 IMAGE_RAM_RO_LENGTH;

// here is the execution address/length of data to move from FLASH to RAM
extern UINT32 Image$$ER_RAM_RW$$Base;
extern UINT32 Image$$ER_RAM_RW$$Length;

// here is the load address of the RAM code/data
#define LOAD_RAM_RO_BASE Load$$ER_RAM_RO$$Base

extern UINT32 LOAD_RAM_RO_BASE;
extern UINT32 Load$$ER_RAM_RW$$Base;

//--//

#if defined(TARGETLOCATION_RAM)

extern UINT32 Load$$ER_RAM$$Base;
extern UINT32 Image$$ER_RAM$$Length;

#elif defined(TARGETLOCATION_FLASH)

extern UINT32 Load$$ER_FLASH$$Base;
extern UINT32 Image$$ER_FLASH$$Length;

#else
    !ERROR
#endif

UINT32 LOAD_IMAGE_Start;
UINT32 LOAD_IMAGE_Length;
UINT32 LOAD_IMAGE_CalcCRC;

//
//  The ARM linker is not keeping FirstEntry.obj (and EntryPoint) for RTM builds of NativeSample (possibly others)
//  The --keep FirstEntry.obj linker option also does not work, however, this unused method call to EntryPoint does the trick.
//
void KEEP_THE_LINKER_HAPPY_SINCE_KEEP_IS_NOT_WORKING()
{
    EntryPoint();
}

//--//

#pragma arm section code = "SectionForBootstrapOperations"

static void __section("SectionForBootstrapOperations") Prepare_Copy( UINT32* src, UINT32* dst, UINT32 len )
{
    if(dst != src)
    {
        INT32 extraLen = len & 0x00000003;
        len            = len & 0xFFFFFFFC;
        
        while(len != 0)
        {
            *dst++ = *src++;

            len -= 4;
        }

        // thumb2 code can be multiples of 2...

        UINT8 *dst8 = (UINT8*) dst, *src8 = (UINT8*) src;

        while (extraLen > 0)
        {
            *dst8++ = *src8++;

            extraLen--;
        }
    }
}

static void __section("SectionForBootstrapOperations") Prepare_Zero( UINT32* dst, UINT32 len )
{
    INT32 extraLen = len & 0x00000003;
    len            = len & 0xFFFFFFFC;

    while(len != 0)
    {
        *dst++ = 0;

        len -= 4;
    }

    // thumb2 code can be multiples of 2...

    UINT8 *dst8 = (UINT8*) dst;

    while (extraLen > 0)
    {
        *dst8++ = 0;

        extraLen--;
    }
}

void __section("SectionForBootstrapOperations") PrepareImageRegions()
{
    //
    // Copy RAM RO regions into proper location.
    //
    {
        UINT32* src = (UINT32*)&LOAD_RAM_RO_BASE; 
        UINT32* dst = (UINT32*)&IMAGE_RAM_RO_BASE;
        UINT32  len = (UINT32 )&IMAGE_RAM_RO_LENGTH; 

        Prepare_Copy( src, dst, len );
    }

    //
    // Copy RAM RW regions into proper location.
    //
    {
        UINT32* src = (UINT32*)&Load$$ER_RAM_RW$$Base; 
        UINT32* dst = (UINT32*)&Image$$ER_RAM_RW$$Base;
        UINT32  len =  (UINT32)&Image$$ER_RAM_RW$$Length; 

        Prepare_Copy( src, dst, len );
    }

    //
    // Initialize RAM ZI regions.
    //
    {
        UINT32* dst = (UINT32*)&Image$$ER_RAM_RW$$ZI$$Base;
        UINT32  len = (UINT32 )&Image$$ER_RAM_RW$$ZI$$Length;

        Prepare_Zero( dst, len );
    }
}

#pragma arm section code

//--//

static void InitCRuntime()
{
#if (defined(HAL_REDUCESIZE) || defined(PLATFORM_EMULATED_FLOATINGPOINT))

    // Don't initialize floating-point on small builds.

#else

#if  !defined(__GNUC__)
    _fp_init();
#endif

   setlocale( LC_ALL, "" );
#endif
}

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

                ::Watchdog_ResetCounter();

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
    HAL_CONTINUATION::InitializeList();
    HAL_COMPLETION  ::InitializeList();

    HAL_Init_Custom_Heap();

    Time_Initialize();
    Events_Initialize();

    CPU_GPIO_Initialize();
    CPU_SPI_Initialize();

    // this is the place where interrupts are enabled after boot for the first time after boot
    ENABLE_INTERRUPTS();

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

void BootEntry()
{

#if (defined(GCCOP) && defined(COMPILE_THUMB))

// the IRQ_Handler routine generated from the compiler is incorrect, the return address LR has been decrement twice
// it decrements LR at the first instruction of IRQ_handler and then before return, it decrements LR again.
// temporary fix is at the ARM_Vector ( IRQ), make it jump to 2nd instruction of IRQ_handler to skip teh first subs LR, LR #4;
//
    volatile int *ptr;
    ptr =(int*) 0x28;
    *ptr = *ptr +4;
#endif


#if !defined(BUILD_RTM) && !defined(PLATFORM_ARM_OS_PORT)
    {
        int  marker;
        int* ptr = &marker - 1; // This will point to the current top of the stack.
        int* end = &StackBottom;

        while(ptr >= end)
        {
            *ptr-- = 0xBAADF00D;
        }
    }
#endif

    // these are needed for patch access

#if defined(TARGETLOCATION_RAM)

    LOAD_IMAGE_Start  = (UINT32)&Load$$ER_RAM$$Base;
    LOAD_IMAGE_Length = (UINT32)&Image$$ER_RAM$$Length;

#elif defined(TARGETLOCATION_FLASH)

    LOAD_IMAGE_Start  = (UINT32)&Load$$ER_FLASH$$Base;
    LOAD_IMAGE_Length = (UINT32)&Image$$ER_FLASH$$Length;

#else
    !ERROR
#endif

    InitCRuntime();

    LOAD_IMAGE_Length += (UINT32)&IMAGE_RAM_RO_LENGTH + (UINT32)&Image$$ER_RAM_RW$$Length;

#if !defined(BUILD_RTM)
    g_Boot_RAMConstants_CRC = Checksum_RAMConstants();
#endif


    CPU_Initialize();

    HAL_Time_Initialize();

    HAL_Initialize();

#if !defined(BUILD_RTM) 
    DEBUG_TRACE4( STREAM_LCD, ".NetMF v%d.%d.%d.%d\r\n", VERSION_MAJOR, VERSION_MINOR, VERSION_BUILD, VERSION_REVISION);
    DEBUG_TRACE3(TRACE_ALWAYS, "%s, Build Date:\r\n\t%s %s\r\n", HalName, __DATE__, __TIME__);
#if defined(__GNUC__)
    DEBUG_TRACE1(TRACE_ALWAYS, "GNU Compiler version %d\r\n", __GNUC__);
#else
    DEBUG_TRACE1(TRACE_ALWAYS, "ARM Compiler version %d\r\n", __ARMCC_VERSION);
#endif

    UINT8* BaseAddress;
    UINT32 SizeInBytes;

    HeapLocation( BaseAddress,    SizeInBytes );
    memset      ( BaseAddress, 0, SizeInBytes );

    lcd_printf("\f");

    lcd_printf("%-15s\r\n", HalName);
    lcd_printf("%-15s\r\n", "Build Date:");
    lcd_printf("  %-13s\r\n", __DATE__);
    lcd_printf("  %-13s\r\n", __TIME__);

#endif  // !defined(BUILD_RTM)

    /***********************************************************************************/

    {
#if defined(FIQ_SAMPLING_PROFILER)
        FIQ_Profiler_Init();
#endif
    }

    // 
    // the runtime is by default using a watchdog 
    // 
   
    Watchdog_GetSetTimeout ( WATCHDOG_TIMEOUT , TRUE );
    Watchdog_GetSetBehavior( WATCHDOG_BEHAVIOR, TRUE );
    Watchdog_GetSetEnabled ( WATCHDOG_ENABLE, TRUE );

 
    // HAL initialization completed.  Interrupts are enabled.  Jump to the Application routine
    ApplicationEntryPoint();

    lcd_printf("\fmain exited!!???.  Halting CPU\r\n");
    debug_printf("main exited!!???.  Halting CPU\r\n");

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

//--//

#if !defined(BUILD_RTM)

UINT32 Checksum_RAMConstants()
{
    UINT32* RAMConstants = (UINT32*)&IMAGE_RAM_RO_BASE; 
    UINT32  Length       = (UINT32 )&IMAGE_RAM_RO_LENGTH; 

    UINT32 CRC;

    // start with Vector area CRC
    CRC = SUPPORT_ComputeCRC( NULL, 0x00000020, 0 );

    // add the big block of RAM constants to CRC
    CRC = SUPPORT_ComputeCRC( RAMConstants, Length, CRC );

    return CRC;
}

void Verify_RAMConstants( void* arg )
{
    BOOL BreakpointOnError = (BOOL)arg;

    //debug_printf("RAMC\r\n");

    UINT32 CRC = Checksum_RAMConstants();

    if(CRC != g_Boot_RAMConstants_CRC)
    {
        hal_printf( "RAMC CRC:%08x!=%08x\r\n", CRC, g_Boot_RAMConstants_CRC );

        UINT32* ROMConstants  = (UINT32*)&LOAD_RAM_RO_BASE;
        UINT32* RAMConstants  = (UINT32*)&IMAGE_RAM_RO_BASE;
        UINT32  Length        = (UINT32 )&IMAGE_RAM_RO_LENGTH;
        BOOL    FoundMismatch = FALSE;

        for(int i = 0; i < Length; i += 4)
        {
            if(*RAMConstants != *ROMConstants)
            {
                hal_printf( "RAMC %08x:%08x!=%08x\r\n", (UINT32) RAMConstants, *RAMConstants, *ROMConstants );

                if(!FoundMismatch) lcd_printf( "\fRAMC:%08x\r\n", (UINT32)RAMConstants );  // first one only to LCD
                FoundMismatch = TRUE;
            }

            RAMConstants++;
            ROMConstants++;
        }

        if(!FoundMismatch)
        {
            // the vector area must have been trashed
            lcd_printf("\fRAMC:%08x\r\n", (UINT32) NULL);
            RAMConstants = (UINT32*)NULL;

            for(int i = 0; i < 32; i += 4)
            {
                hal_printf( "RAMC %02x:%08x\r\n", i, *RAMConstants   );
                lcd_printf( "%02x:%08x\r\n"     , i, *RAMConstants++ );
            }
        }

        DebuggerPort_Flush( HalSystemConfig.DebugTextPort );

        if(BreakpointOnError)
        {
            HARD_BREAKPOINT();
        }
    }
}

#endif  // !defined(BUILD_RTM)
