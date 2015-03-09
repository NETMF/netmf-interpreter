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
#include <cmsis_os.h>
#include <cstdint>

#if !defined(__GNUC__)
#include <rt_fp.h>
#endif

extern UINT32 Load$$ER_LWIP_OS$$RW$$Base; 
extern UINT32 Image$$ER_LWIP_OS$$RW$$Base;
extern UINT32 Image$$ER_LWIP_OS$$RW$$Length; 
extern UINT32 Image$$ER_LWIP_OS$$ZI$$Base;
extern UINT32 Image$$ER_LWIP_OS$$ZI$$Length;

static ON_SOFT_REBOOT_HANDLER s_rebootHandlers[16] = {NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL};

#pragma arm section code = "SectionForBootstrapOperations"

static void __section(SectionForBootstrapOperations) Prepare_Copy( UINT32* src, UINT32* dst, UINT32 len )
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

static void __section(SectionForBootstrapOperations) Prepare_Zero( UINT32* dst, UINT32 len )
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

#pragma arm section code

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
    // Interrupts must be enabled to handle calls to OS
    // (Network stack uses the CMSIS-RTX OS, which uses
    // SVC calls, which will hard fault if the interrupts
    // are disabled at the Svc instruction )
    // SystemInit handles this for the startup from reset
    // However, this is also called from the CLR when doing
    // a soft reboot.
    __enable_irq();
    HAL_CONTINUATION::InitializeList();
    HAL_COMPLETION  ::InitializeList();

    HAL_Init_Custom_Heap();

    Time_Initialize();
    Events_Initialize();

    CPU_GPIO_Initialize();
    CPU_SPI_Initialize();

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
 
// Hack: refer to work item #2374
// For reasons unknown, the ARM linker is getting the
// fixup for the LoadRegion symbol incorrect. The data
// is located correctly but the fixed up pointer stored
// in the literal pool that this code loads for the address
// of the load base (src) is off by some factor. In initial
// testing it was always 144, unfortunately it turns out
// not to be consistent and is now at 0xED0...
const UINT32 ArmLinkerLoadRegionOffsetHack = 0x00000ED0;

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
    
    LwipRegionInit();
}

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

extern "C" void HARD_Breakpoint()
{
    __breakpoint( 0 );
}

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


