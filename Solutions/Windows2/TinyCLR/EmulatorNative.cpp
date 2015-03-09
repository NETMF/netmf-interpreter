////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"

#include <Shellapi.h>

#include <vcclr.h>
#include <vector>

using namespace System;

using namespace System::Reflection;
using namespace Microsoft::SPOT::Emulator::BlockStorage;

extern void EmulatorHook__Watchdog_Callback();
extern void ClrReboot();

////////////////////////////////////////////////////////////////////////////////////////////////////


struct Settings
{
    CLR_SETTINGS m_emuSettings;


private:
    Settings();
    ~Settings();

public:
    static HRESULT CreateInstance( Settings*& settings );
    static void DeleteInstance( Settings*& settings );

    void SetCommandLineArguments( LPCWSTR szCmdLineArgs );

    HRESULT System_Start();
    void System_Stop();
};

Settings::Settings()
{
    m_emuSettings.WaitForDebugger = false;

    // Add ways to modify these?
    m_emuSettings.PerformGarbageCollection = false;
    m_emuSettings.PerformHeapCompaction = false;
    m_emuSettings.MaxContextSwitches = 1;
    m_emuSettings.EnterDebuggerLoopAfterExit = false;
    m_emuSettings.EmulatorArgs[0] = NULL;
}

Settings::~Settings()
{
    System_Stop();
}

//--//

HRESULT Settings::CreateInstance( Settings*& settings )
{
    settings = new Settings(); if(settings == NULL) return CLR_E_OUT_OF_MEMORY;

    return settings->System_Start();
}

void Settings::DeleteInstance( Settings*& settings )
{
    if(settings)
    {
        settings->System_Stop();

        delete settings; settings = NULL;
    }
}

//--//

#ifdef NETMF_RUN_NATIVE_UNIT_TESTS
extern BOOL TestEntry();
#endif

HRESULT Settings::System_Start()
{
    TINYCLR_HEADER();

    HAL_Init_Custom_Heap();

    Time_Initialize();

    if(!CPU_GPIO_Initialize()) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

    // 
    // the runtime is by default using a watchdog 
    // 
    Watchdog_GetSetTimeout ( WATCHDOG_TIMEOUT , TRUE );
    Watchdog_GetSetBehavior( WATCHDOG_BEHAVIOR, TRUE );
    Watchdog_GetSetEnabled ( WATCHDOG_ENABLE  , TRUE );
    
    TimeService_Initialize();

    BlockStorageList::Initialize();

    BlockStorage_AddDevices();

    BlockStorageList::InitializeDevices();

    FileSystemVolumeList::Initialize();

    FS_AddVolumes();

    FileSystemVolumeList::InitializeVolumes();

    /*
    PalEvent_Initialize();
    Gesture_Initialize();
    Ink_Initialize();
    */

#ifdef NETMF_RUN_NATIVE_UNIT_TESTS
    TestEntry();
#endif

    TINYCLR_NOCLEANUP();
}

void Settings::System_Stop()
{
}

//--//

void Settings::SetCommandLineArguments( LPCWSTR szCmdLineArgs )
{
    wcscpy_s(m_emuSettings.EmulatorArgs, ARRAYSIZE(m_emuSettings.EmulatorArgs), szCmdLineArgs);
}

//--//

#pragma managed(push, off)

inline __declspec(naked) __int64 RDTSC()
{
    _asm
    {
        RDTSC
        ret
    }
}

__int64 GetRDTSC()
{
    return RDTSC();
}

#pragma managed(pop)

namespace Microsoft
{
    namespace SPOT
    {
        namespace Emulator
        {
            EmulatorNative::EmulatorNative()
            {
                s_emulatorNative = this;
            }

            EmulatorNative::~EmulatorNative()
            {
                Settings* st = settings;
                Settings::DeleteInstance( st );
                st = NULL;

                ::CoUninitialize();
            }

            void EmulatorNative::Initialize( IHal^ managedHal )
            {
                LARGE_INTEGER pfStart, pfEnd, pfFreq;
                INT64 rdtscStart, rdtscEnd;
                double RDTSC_To_PerfCount_Ratio;

                m_isClrStarted = false;
                m_managedHal = managedHal;
                m_batteryDriver = managedHal->Battery;
                m_comDriver = managedHal->Com;
                m_serialDriver = managedHal->Serial;
                m_spiDriver = managedHal->Spi;
                m_lcdDriver = managedHal->Lcd;
                m_memoryDriver = managedHal->Memory;
                m_timeDriver = managedHal->Time;
                m_gpioDriver = managedHal->Gpio;
                m_eventsDriver = managedHal->Events;
                m_usbDriver = managedHal->Usb;
                m_socketsDriver = managedHal->Sockets;
                m_i2cDriver = managedHal->I2c;
                m_fsDriver = managedHal->FileSystem;
                m_touchPanelDriver = managedHal->TouchPanel;
                m_watchdogDriver = managedHal->Watchdog;
                m_blockStorageDriver = managedHal->BlockStorage;
                m_sslDriver = managedHal->Ssl;

                m_sessionDriver = managedHal->Session;
                m_encryptionDriver = managedHal->Encryption;
                m_signatureDriver = managedHal->Signature;
                m_digestDriver = managedHal->Digest;
                m_keyManagementDriver = managedHal->KeyManagement;
                m_crypokiObjectMgrDriver = managedHal->CryptokiObjectMgr;
                m_randomDriver = managedHal->Random;

                m_updateProvider = managedHal->UpdateProvider;
                m_updateBackup = managedHal->UpdateBackup;
                m_updateStorage = managedHal->UpdateStorage;
                m_updateValidation = managedHal->UpdateValidation;

                m_socketsDriver->Initialize();

                ClrSetLcdDimensions(managedHal->Lcd->GetWidth(),managedHal->Lcd->GetHeight(),managedHal->Lcd->GetBitsPerPixel());

                GetRDTSC();
                ::QueryPerformanceCounter( &pfStart ); // probably this calls will be in the cache at execution time, so load them

                ::QueryPerformanceCounter( &pfStart );
                rdtscStart = GetRDTSC(); // RDTSC should be fast enough not to pollute the timing, calling after a separate sleep would lead to more error
                ::Sleep( 100 );
                rdtscEnd = GetRDTSC();
                ::QueryPerformanceCounter( &pfEnd );
                ::QueryPerformanceFrequency( &pfFreq  );

                RDTSC_To_PerfCount_Ratio = (double)(pfEnd.QuadPart - pfStart.QuadPart) / (double)(rdtscEnd - rdtscStart);

                m_RDTSC_To_Ticks = RDTSC_To_PerfCount_Ratio * (double)TIME_CONVERSION__TO_SECONDS / pfFreq.QuadPart;


                //CLR_RT_Assembly::InitString();

                Settings* st;
                HRESULT hr = Settings::CreateInstance( st );

                if(FAILED(hr))
                {
                    throw gcnew EmulatorException("Error creating EmulatorNative Setting class", hr);
                }

                settings = st;
            }

            void EmulatorNative::Start()
            {
                ASSERT(settings);

                if(m_isClrStarted)
                {
                    ClrReboot();
                }
                else
                {
                    m_isClrStarted = true;

                    ClrStartup(settings->m_emuSettings);
                }
            }

            void EmulatorNative::Shutdown()
            {
                //synchronization?
                m_fShuttingDown = true;
                m_socketsDriver->Uninitialize();
            }

            void EmulatorNative::LoadPE( String^ filename )
            {
                ASSERT(settings);

                pin_ptr<const wchar_t> filenameLptStr = PtrToStringChars( filename );

                if(FAILED(ClrLoadPE(filenameLptStr)))
                {
                    throw gcnew Exception( "Error loading PE file: " + filename );
                }
            }

            void EmulatorNative::LoadDatabase( String^ filename )
            {
                ASSERT(settings);

                pin_ptr<const wchar_t> filenameLptStr = PtrToStringChars( filename );

                if(FAILED(ClrLoadDAT(filenameLptStr)))
                {
                    throw gcnew Exception( "Error loading database file: " + filename );
                }
            }
            void EmulatorNative::WaitForDebuggerOnStartup()
            {
                ASSERT(settings);

                settings->m_emuSettings.WaitForDebugger = true;
            }

            void EmulatorNative::SetCommandLineArguments( String^ cmdLineArgs )
            {
                using namespace System::Runtime::InteropServices;

                const wchar_t* chars = (const wchar_t*)(Marshal::StringToHGlobalUni(cmdLineArgs)).ToPointer();
                settings->SetCommandLineArguments( chars );
                Marshal::FreeHGlobal(IntPtr((void*)chars));
            }

            EmulatorNative^ EmulatorNative::GetEmulatorNative()
            {
                return s_emulatorNative;
            }

            IHal^ EmulatorNative::GetManagedHal()
            {
                return s_emulatorNative->m_managedHal;
            }

            IBatteryDriver^ EmulatorNative::GetIBatteryDriver()
            {
                return s_emulatorNative->m_batteryDriver;
            }

            IMemoryDriver^ EmulatorNative::GetIMemoryDriver()
            {
                return s_emulatorNative->m_memoryDriver;
            }

            ISerialDriver^ EmulatorNative::GetISerialDriver()
            {
                return s_emulatorNative->m_serialDriver;
            }

            IComDriver^ EmulatorNative::GetIComDriver()
            {
                return s_emulatorNative->m_comDriver;
            }

            ISpiDriver^ EmulatorNative::GetISpiDriver()
            {
                return s_emulatorNative->m_spiDriver;
            }

            ITimeDriver^ EmulatorNative::GetITimeDriver()
            {
                return s_emulatorNative->m_timeDriver;
            }

            ILcdDriver^ EmulatorNative::GetILcdDriver()
            {
                return s_emulatorNative->m_lcdDriver;
            }

            IGpioDriver^ EmulatorNative::GetIGpioDriver()
            {
                return s_emulatorNative->m_gpioDriver;
            }

            IEventsDriver^ EmulatorNative::GetIEventsDriver()
            {
                return s_emulatorNative->m_eventsDriver;
            }

            IUsbDriver^ EmulatorNative::GetIUsbDriver()
            {
                return s_emulatorNative->m_usbDriver;
            }

            ISocketsDriver^ EmulatorNative::GetISocketsDriver()
            {
                return s_emulatorNative->m_socketsDriver;
            }

            ISslDriver^ EmulatorNative::GetISslDriver()
            {
                return s_emulatorNative->m_sslDriver;
            }

            II2cDriver^ EmulatorNative::GetII2cDriver()
            {
                return s_emulatorNative->m_i2cDriver;
            }

            IFSDriver^ EmulatorNative::GetIFSDriver()
            {
                return s_emulatorNative->m_fsDriver;
            }

            IBlockStorageDriver^ EmulatorNative::GetIBlockStorageDriver()
            {
                return s_emulatorNative->m_blockStorageDriver;
            }

            ITouchPanelDriver^ EmulatorNative::GetITouchPanelDriver()
            {
                return s_emulatorNative->m_touchPanelDriver;
            }
            
            IWatchdogDriver^ EmulatorNative::GetIWatchdogDriver()
            {
                return s_emulatorNative->m_watchdogDriver;
            }

            ISessionDriver^ EmulatorNative::GetISessionDriver()
            {
                return s_emulatorNative->m_sessionDriver;
            }

            IEncryptionDriver^ EmulatorNative::GetIEncryptionDriver()
            {
                return s_emulatorNative->m_encryptionDriver;
            }

            IDigestDriver^ EmulatorNative::GetIDigestDriver()
            {
                return s_emulatorNative->m_digestDriver;
            }
            ISignatureDriver^ EmulatorNative::GetISignatureDriver()
            {
                return s_emulatorNative->m_signatureDriver;
            }
            IKeyManagementDriver^ EmulatorNative::GetIKeyManagementDriver()
            {
                return s_emulatorNative->m_keyManagementDriver;
            }
            ICryptokiObjectDriver^ EmulatorNative::GetICryptokiObjectDriver()
            {
                return s_emulatorNative->m_crypokiObjectMgrDriver;
            }
            IRandomDriver^ EmulatorNative::GetIRandomDriver()
            {
                return s_emulatorNative->m_randomDriver;
            }
            
            IUpdateDriver^ EmulatorNative::GetIUpdateProvider()
            {
                return s_emulatorNative->m_updateProvider;
            }
            IUpdateStorageDriver^ EmulatorNative::GetIUpdateStorage()
            {
                return s_emulatorNative->m_updateStorage;
            }
            IUpdateBackupDriver^ EmulatorNative::GetIUpdateBackup()
            {
                return s_emulatorNative->m_updateBackup;
            }
            IUpdateValidationDriver^ EmulatorNative::GetIUpdateValidation()
             {
                return s_emulatorNative->m_updateValidation;
            }


            void EmulatorNative::ExecuteCompletion( IntPtr completion )
            {
                HAL_COMPLETION* c = (HAL_COMPLETION*)(completion.ToPointer());

                c->Execute();
            }

            void EmulatorNative::ExecuteContinuation( IntPtr continuation )
            {
                HAL_CONTINUATION* c = (HAL_CONTINUATION*)(continuation.ToPointer());

                c->Execute();
            }

            UINT64 EmulatorNative::GetCurrentTicks()
            {
                return (UINT64)(GetRDTSC() * m_RDTSC_To_Ticks);
            }

            void EmulatorNative::GpioIsrCallback( IntPtr isr, GPIO_PIN pin, bool pinState, IntPtr param )
            {
                GPIO_INTERRUPT_SERVICE_ROUTINE callback = (GPIO_INTERRUPT_SERVICE_ROUTINE)isr.ToPointer();

                _ASSERTE(callback);

                callback( pin, pinState, param.ToPointer() );
            }

            void EmulatorNative::WatchdogCallback()
            {
                EmulatorHook__Watchdog_Callback();
            }

            void EmulatorNative::TouchCallback(IntPtr isr, GPIO_PIN pin, unsigned int value, IntPtr param)
            {
                GPIO_INTERRUPT_SERVICE_ROUTINE callback = (GPIO_INTERRUPT_SERVICE_ROUTINE)isr.ToPointer();

                _ASSERTE(callback);

                callback( pin, value, param.ToPointer() );
            }

            //--//

            void EmulatorNative::InsertRemovableBlockStorage( unsigned int context, array<InternalBlockRegionInfo>^ config, 
                array<unsigned char>^ nameSpace, unsigned int serialNumber, unsigned int deviceFlags, unsigned int bytesPerSector )
            {
                _ASSERTE(g_Emulator_BS_DevicesInfo[context].NumRegions == 0);
                _ASSERTE(g_Emulator_BS_DevicesInfo[context].Regions == NULL);

                int numRegions = config->Length;
                BlockRegionInfo* regionInfos = new BlockRegionInfo[numRegions];
                
                g_Emulator_BS_DevicesInfo[context].NumRegions     = numRegions;
                g_Emulator_BS_DevicesInfo[context].Regions        = regionInfos;
                g_Emulator_BS_DevicesInfo[context].BytesPerSector = bytesPerSector;

                BlockRegionInfo* regionInfo;
                array<InternalBlockRange>^ ranges;
                int numRanges;

                for(int j = 0; j < numRegions; j++)
                {
                    regionInfo = &(regionInfos[j]);

                    numRanges = config[j].NumBlockRanges;
                    
                    regionInfo->Start              = config[j].Start;
                    regionInfo->NumBlocks          = config[j].NumBlocks;
                    regionInfo->NumBlockRanges     = numRanges;
                    regionInfo->BytesPerBlock      = config[j].BytesPerBlock;
                    regionInfo->BlockRanges        = new BlockRange[numRanges];

                    ranges = config[j].BlockRanges;

                    for(int k = 0; k < numRanges; k++)
                    {
                        BlockRange *pRange = (BlockRange*)&regionInfo->BlockRanges[k];

                        pRange->RangeType  = ranges[k].RangeType;
                        pRange->StartBlock = ranges[k].StartBlock;
                        pRange->EndBlock   = ranges[k].EndBlock;
                    }
                }

                BlockStorageList::AddDevice( &(g_Emulator_BS_Devices[context]), &Emulator_BS_DeviceTable, (void*)context, TRUE );

                pin_ptr<const unsigned char> nameSpaceLptStr = &(nameSpace[0]);

                FS_MountVolume( (LPCSTR)nameSpaceLptStr, serialNumber, deviceFlags, &g_Emulator_BS_Devices[context] );
            }

            void EmulatorNative::EjectRemovableBlockStorage( unsigned int context )
            {
                FS_UnmountVolume( &g_Emulator_BS_Devices[context] );

                // release the memory
                delete [] g_Emulator_BS_DevicesInfo[context].Regions;
                g_Emulator_BS_DevicesInfo[context].Regions = NULL;
                g_Emulator_BS_DevicesInfo[context].NumRegions = 0;

                BlockStorageList::RemoveDevice( &(g_Emulator_BS_Devices[context]), TRUE );
            }

            void EmulatorNative::PostGesture( int gestureId, int x, int y, unsigned short data )
            {
                PostManagedEvent(EVENT_GESTURE, gestureId, data, ((UINT32)x << 16) | y);
            }
            
            //--//

            void EmulatorNative::DisableInterrupts()
            {
                m_managedHal->DisableInterrupts();
            }

            void EmulatorNative::EnableInterrupts()
            {
                m_managedHal->EnableInterrupts();
            }

            BOOL EmulatorNative::AreInterruptsEnabled()
            {
                return m_managedHal->AreInterruptsEnabled;
            }

            BOOL EmulatorNative::IsShuttingDown()
            {
                return BOOL_TO_INT( m_fShuttingDown );
            }
        }
    }
}
