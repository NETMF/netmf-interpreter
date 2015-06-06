////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
#include <tinyhal.h>
#include "stm32f4xx.h"

#if defined(PLATFORM_ARM_OS_PORT)
#include <cmsis_os.h>
#include <stdint.h>
#endif

#undef  TRACE_ALWAYS
#define TRACE_ALWAYS               0x00000001

#undef  DEBUG_TRACE
#define DEBUG_TRACE (TRACE_ALWAYS)
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

#if defined(PLATFORM_ARM_OS_PORT) && defined(TCPIP_LWIP_OS)
extern UINT32 Load$$ER_LWIP_OS$$RW$$Base; 
extern UINT32 Image$$ER_LWIP_OS$$RW$$Base;
extern UINT32 Image$$ER_LWIP_OS$$RW$$Length; 
extern UINT32 Image$$ER_LWIP_OS$$ZI$$Base;
extern UINT32 Image$$ER_LWIP_OS$$ZI$$Length;
#endif

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

#if !defined(PLATFORM_ARM_OS_PORT) || defined(__GNUC__)
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
#else
extern "C" void PrepareImageRegions()
{
    // This space intentionally left blank... 8^)
    //
    // The OS boot of CLR on CMSIS-RTX doesn't
    // use this as it relies on the C/C++ runtime
    // to handle initialization. However, to keep
    // from adding more libraries or #if checks
    // in code this is defined to allow normal
    // linking with the same HAL libs used in a 
    // boot loader.
}
#endif

#pragma arm section code

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

    HAL_Init_Custom_Heap();

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

    //FS_Initialize();

    //FileSystemVolumeList::Initialize();

    //FS_AddVolumes();

    //FileSystemVolumeList::InitializeVolumes();

    //LCD_Initialize();
    
#if !defined(HAL_REDUCESIZE)
    CPU_InitializeCommunication();
#endif

    I2C_Initialize();

    Buttons_Initialize();

    // Initialize the backlight to a default off state
    //BackLight_Initialize();

    //Piezo_Initialize();

    //Battery_Initialize();

    //Charger_Initialize();

    PalEvent_Initialize();
    //Gesture_Initialize();
    //Ink_Initialize();
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

#if defined(PLATFORM_ARM_OS_PORT) && defined(TCPIP_LWIP_OS)
// Hack: refer to work item #2374
// For reasons unknown, the ARM linker is getting the
// fixup for the LoadRegion symbol incorrect. The data
// is located correctly but the fixed up pointer stored
// in the literal pool that this code loads for the address
// of the load base (src) is off by some factor. In initial
// testing it was always 0x90, unfortunately it turns out
// not to be consistent and bumped up to 0xED0, and is now
// back at 0x90... Sigh... Hope to hear back from ARM support
// on this soon. ARM has confirmed the bug as related to the
// use of a non-compressed region in the scatter file. Since
// the code here doesn't understand the linker compression
// the region is uncompressed, however the linker bug is 
// that an uncompressed region that follows after a compressed
// one will get the invalid Load$$xxx$$Base value. If the
// scatter file is updated to ensure all regions occuring
// before this one are also uncompressed it works ok.
const UINT32 ArmLinkerLoadRegionOffsetHack = 0x00000000;
void LwipRegionInit()
{
    // Copy RAM RW regions into proper location.
    {
        UINT32* src = &Load$$ER_LWIP_OS$$RW$$Base; 
        UINT32* dst = &Image$$ER_LWIP_OS$$RW$$Base;
        UINT32  len = (UINT32) &Image$$ER_LWIP_OS$$RW$$Length; 

        // Hack: refer to work item #2374
        src = reinterpret_cast<UINT32*>(reinterpret_cast<UINT32>(src) - ArmLinkerLoadRegionOffsetHack );

        Prepare_Copy( src, dst, len );
    }

    // Initialize RAM ZI regions.
    {
        UINT32* dst = &Image$$ER_LWIP_OS$$ZI$$Base;
        UINT32  len = (UINT32) &Image$$ER_LWIP_OS$$ZI$$Length;

        Prepare_Zero( dst, len );
    }
}
#endif

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

    //LCD_Uninitialize();

    I2C_Uninitialize();

    Buttons_Uninitialize();

    // Initialize the backlight to a default off state
    //BackLight_Uninitialize();

    //Piezo_Uninitialize();

    //Battery_Uninitialize();
    //Charger_Uninitialize();

    TimeService_UnInitialize();
    //Ink_Uninitialize();
    //Gesture_Uninitialize();
    PalEvent_Uninitialize();

    SOCKETS_CloseConnections();

#if !defined(HAL_REDUCESIZE)
    CPU_UninitializeCommunication();
#endif

    //FileSystemVolumeList::UninitializeVolumes();

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
    
#if defined(PLATFORM_ARM_OS_PORT) && defined(TCPIP_LWIP_OS)
    LwipRegionInit();
#endif
}

extern "C"
{
#if defined( __GNUC__ )
    extern "C++" int main(void);
    extern void __libc_init_array();
    void __main()
    {
        // Copy writeable data and zero init BSS
        PrepareImageRegions();

        // Call static constructors
        __libc_init_array();

        // Call the application's entry point.
        main();
    }
#endif

#if !defined(PLATFORM_ARM_OS_PORT)
void BootEntry()
{
#if !defined(BUILD_RTM)
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

    debug_printf("\f");

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

    // the runtime is by default using a watchdog 
   
    Watchdog_GetSetTimeout ( WATCHDOG_TIMEOUT , TRUE );
    Watchdog_GetSetBehavior( WATCHDOG_BEHAVIOR, TRUE );
    Watchdog_GetSetEnabled ( WATCHDOG_ENABLE, TRUE );

 
    // HAL initialization completed.  Interrupts are enabled.  Jump to the Application routine
    ApplicationEntryPoint();

    debug_printf("main exited!!???.  Halting CPU\r\n");

#if defined(BUILD_RTM)
    CPU_Reset();
#else
    CPU_Halt();
#endif
}
#endif

} // extern "C"


#if defined(PLATFORM_ARM_OS_PORT)
extern "C" void STM32F4_BootstrapCode();

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
    STM32F4_BootstrapCode();
    CPU_Initialize();
    __enable_irq();
}

#endif //PLATFORM_ARM_OS_PORT

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

void lcd_printf( const char* format, ... )
{
    va_list arg_ptr;

    va_start( arg_ptr, format );

    hal_vfprintf( STREAM_LCD, format, arg_ptr );
}

#endif  // !defined(BUILD_RTM)

#if !defined(BUILD_RTM)

UINT32 Checksum_RAMConstants()
{
    UINT32* RAMConstants = (UINT32*)&IMAGE_RAM_RO_BASE; 
    UINT32  Length       = (UINT32 )&IMAGE_RAM_RO_LENGTH; 

    UINT32 crc;

    // start with Vector area CRC
    crc = SUPPORT_ComputeCRC(NULL, 0x00000020, 0);

    // add the big block of RAM constants to CRC
    crc = SUPPORT_ComputeCRC(RAMConstants, Length, crc);

    return crc;
}

void Verify_RAMConstants( void* arg )
{
    BOOL BreakpointOnError = (BOOL)arg;

    //debug_printf("RAMC\r\n");

    UINT32 crc = Checksum_RAMConstants();

    if (crc != g_Boot_RAMConstants_CRC)
    {
        hal_printf("RAMC CRC:%08x!=%08x\r\n", crc, g_Boot_RAMConstants_CRC);

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
