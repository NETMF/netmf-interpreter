////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using Microsoft.SPOT.Emulator.Com;
using Microsoft.SPOT.Emulator.Gpio;
using Microsoft.SPOT.Emulator.Spi;
using Microsoft.SPOT.Emulator.Serial;
using Microsoft.SPOT.Emulator.Lcd;
using Microsoft.SPOT.Emulator.Time;
using Microsoft.SPOT.Emulator.Events;
using Microsoft.SPOT.Emulator.Usb;
using Microsoft.SPOT.Emulator.Memory;
using Microsoft.SPOT.Emulator.Sockets;
using Microsoft.SPOT.Emulator.FS;
using Microsoft.SPOT.Emulator.I2c;
using Microsoft.SPOT.Emulator.Battery;
using Microsoft.SPOT.Emulator.TouchPanel;
using Microsoft.SPOT.Emulator.BlockStorage;
using Microsoft.SPOT.Emulator.Watchdog;
using Microsoft.SPOT.Emulator.Sockets.Security;
using Microsoft.SPOT.Emulator.PKCS11;
using Microsoft.SPOT.Emulator.Update;


namespace Microsoft.SPOT.Emulator
{
    public interface IHal
    {
        IBatteryDriver Battery { get; }
        IComDriver Com { get; }
        ISerialDriver Serial { get; }
        ISpiDriver Spi { get; }
        ILcdDriver Lcd { get; }
        ITimeDriver Time { get; }
        IGpioDriver Gpio { get; }
        IEventsDriver Events { get; }
        IUsbDriver Usb { get; }
        IMemoryDriver Memory { get;}
        ISocketsDriver Sockets { get;}
        II2cDriver I2c { get; }
        ITouchPanelDriver TouchPanel { get; }
        IWatchdogDriver Watchdog { get; }
        IFSDriver FileSystem { get; }
        IBlockStorageDriver BlockStorage { get; }
        ISslDriver Ssl { get; }

        ISessionDriver Session { get; }
        IKeyManagementDriver KeyManagement { get; }
        IEncryptionDriver Encryption { get; }
        IDigestDriver Digest { get; }
        ISignatureDriver Signature { get; }
        ICryptokiObjectDriver CryptokiObjectMgr { get; }
        IRandomDriver Random { get; }

        IUpdateDriver UpdateProvider { get; }
        IUpdateStorageDriver UpdateStorage { get; }
        IUpdateBackupDriver UpdateBackup { get; }
        IUpdateValidationDriver UpdateValidation { get; }

        //random other stuff
        void EnableInterrupts();
        void DisableInterrupts();
        bool AreInterruptsEnabled { get; }
    }
}
