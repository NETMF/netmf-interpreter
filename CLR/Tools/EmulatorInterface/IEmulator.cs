////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using Microsoft.SPOT.Emulator.BlockStorage;

namespace Microsoft.SPOT.Emulator
{
    public interface IEmulator
    {
        void Initialize(IHal managedHal);

        void ExecuteCompletion( IntPtr completion );
        void ExecuteContinuation( IntPtr continuation );
        ulong GetCurrentTicks();
        void GpioIsrCallback( IntPtr isr, uint Pin, bool PinState, IntPtr param );
        void WatchdogCallback();
        void TouchCallback( IntPtr isr, uint Pin, uint value, IntPtr param );
        void LoadPE( String filename );
        void LoadDatabase( String filename );
        void WaitForDebuggerOnStartup();
        void SetCommandLineArguments( String cmdLineArgs );
        void Start();
        void Shutdown();
        void PostGesture(int gestureId, int x, int y, ushort data);

        void InsertRemovableBlockStorage(uint context, InternalBlockRegionInfo[] config, byte[] nameSpace, uint serialNumber, uint flags, uint bytesPerSector);
        void EjectRemovableBlockStorage(uint context);
    }
}
