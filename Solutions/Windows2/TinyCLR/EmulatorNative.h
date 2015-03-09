////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _EMULATORINTERFACE_H_
#define _EMULATORINTERFACE_H_

#ifndef _WIN32_WINNT
#define _WIN32_WINNT 0x0501
#endif
#include <windows.h>
#include <vcclr.h>
#include <string>
#include <tinyhal.h>

////////////////////////////////////////////////////////////////////////////////////////////////////

struct Settings;

extern BlockDeviceInfo*    g_Emulator_BS_DevicesInfo;
extern BlockStorageDevice* g_Emulator_BS_Devices;
extern IBlockStorageDevice Emulator_BS_DeviceTable;

////////////////////////////////////////////////////////////////////////////////////////////////////

using namespace System;
using namespace Microsoft::SPOT::Emulator;
using namespace Microsoft::SPOT::Emulator::Battery;
using namespace Microsoft::SPOT::Emulator::Com;
using namespace Microsoft::SPOT::Emulator::Gpio;
using namespace Microsoft::SPOT::Emulator::Serial;
using namespace Microsoft::SPOT::Emulator::Spi;
using namespace Microsoft::SPOT::Emulator::Time;
using namespace Microsoft::SPOT::Emulator::Lcd;
using namespace Microsoft::SPOT::Emulator::Events;
using namespace Microsoft::SPOT::Emulator::Usb;
using namespace Microsoft::SPOT::Emulator::Memory;
using namespace Microsoft::SPOT::Emulator::Sockets;
using namespace Microsoft::SPOT::Emulator::Sockets::Security;
using namespace Microsoft::SPOT::Emulator::I2c;
using namespace Microsoft::SPOT::Emulator::FS;
using namespace Microsoft::SPOT::Emulator::TouchPanel;
using namespace Microsoft::SPOT::Emulator::BlockStorage;
using namespace Microsoft::SPOT::Emulator::Watchdog;
using namespace Microsoft::SPOT::Emulator::PKCS11;
using namespace Microsoft::SPOT::Emulator::Update;


namespace Microsoft
{
    namespace SPOT
    {
        namespace Emulator
        {
            public ref class EmulatorNative : public IEmulator
            {
            private:
                static EmulatorNative^ s_emulatorNative;

                double m_RDTSC_To_Ticks;
                IHal^ m_managedHal;
                IBatteryDriver^ m_batteryDriver;
                IComDriver^ m_comDriver;
                ISerialDriver^ m_serialDriver;
                ISpiDriver^ m_spiDriver;
                ILcdDriver^ m_lcdDriver;
                IMemoryDriver^ m_memoryDriver;
                ITimeDriver^ m_timeDriver;
                IGpioDriver^ m_gpioDriver;
                IEventsDriver^ m_eventsDriver;
                IUsbDriver^ m_usbDriver;
                ISocketsDriver^ m_socketsDriver;
                ISslDriver^ m_sslDriver;
                II2cDriver^ m_i2cDriver;
                IFSDriver^ m_fsDriver;
                IBlockStorageDriver^ m_blockStorageDriver;
                ITouchPanelDriver^ m_touchPanelDriver;
                IWatchdogDriver^ m_watchdogDriver;

                ISessionDriver^ m_sessionDriver;
                IEncryptionDriver^ m_encryptionDriver;
                IDigestDriver^ m_digestDriver;
                ISignatureDriver^ m_signatureDriver;
                IKeyManagementDriver^ m_keyManagementDriver;
                ICryptokiObjectDriver^ m_crypokiObjectMgrDriver;
                IRandomDriver^ m_randomDriver;

                IUpdateDriver^ m_updateProvider;
                IUpdateStorageDriver^ m_updateStorage;
                IUpdateBackupDriver^ m_updateBackup;
                IUpdateValidationDriver^ m_updateValidation;

                Settings* settings;
                bool m_fShuttingDown;
                bool m_isClrStarted;

            public:
                EmulatorNative();
                ~EmulatorNative();

                virtual void Initialize(IHal^ managedHal);

                virtual void ExecuteCompletion( IntPtr completion );
                virtual void ExecuteContinuation( IntPtr continuation );
                virtual UINT64 GetCurrentTicks();
                virtual void GpioIsrCallback( IntPtr isr, GPIO_PIN Pin, bool PinState, IntPtr param );
                virtual void WatchdogCallback();
                virtual void TouchCallback( IntPtr isr, GPIO_PIN Pin, unsigned int value, IntPtr param );
                virtual void LoadPE( String^ filename );
                virtual void LoadDatabase( String^ filename );
                virtual void WaitForDebuggerOnStartup();
                virtual void SetCommandLineArguments( String^ cmdLineArgs );
                virtual void Start();
                virtual void Shutdown();
                virtual void PostGesture( int gestureId, int x, int y, unsigned short data );

                virtual void InsertRemovableBlockStorage( unsigned int context, array<InternalBlockRegionInfo>^ config, 
                    array<unsigned char>^ nameSpace, unsigned int serialNumber, unsigned int flags, unsigned int bytesPerSector );
                virtual void EjectRemovableBlockStorage( unsigned int context );

            internal:
                static EmulatorNative^ GetEmulatorNative();
                static IHal^ GetManagedHal();
                static IBatteryDriver^ GetIBatteryDriver();
                static IComDriver^ GetIComDriver();
                static ISerialDriver^ GetISerialDriver();
                static ISpiDriver^ GetISpiDriver();
                static ILcdDriver^ GetILcdDriver();
                static IMemoryDriver^ GetIMemoryDriver();
                static ITimeDriver^ GetITimeDriver();
                static IGpioDriver^ GetIGpioDriver();
                static IEventsDriver^ GetIEventsDriver();
                static IUsbDriver^ GetIUsbDriver();
                static ISocketsDriver^ GetISocketsDriver();
                static ISslDriver^ GetISslDriver();
                static II2cDriver^ GetII2cDriver();
                static IFSDriver^ GetIFSDriver();
                static IBlockStorageDriver^ GetIBlockStorageDriver();
                static ITouchPanelDriver^ GetITouchPanelDriver();
                static IWatchdogDriver^ GetIWatchdogDriver();

                static ISessionDriver^ GetISessionDriver();
                static IEncryptionDriver^ GetIEncryptionDriver();
                static IDigestDriver^ GetIDigestDriver();
                static ISignatureDriver^ GetISignatureDriver();
                static IKeyManagementDriver^ GetIKeyManagementDriver();
                static ICryptokiObjectDriver^ GetICryptokiObjectDriver();
                static IRandomDriver^ GetIRandomDriver();

                static IUpdateDriver^ GetIUpdateProvider();
                static IUpdateStorageDriver^ GetIUpdateStorage();
                static IUpdateBackupDriver^ GetIUpdateBackup();
                static IUpdateValidationDriver^ GetIUpdateValidation();

                void DisableInterrupts();
                void EnableInterrupts();
                BOOL AreInterruptsEnabled();
                BOOL IsShuttingDown();
            };
        }
    }
}

#endif // _EMULATORINTERFACE_H_
