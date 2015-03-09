////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace Microsoft.SPOT.Emulator.Events
{
    public interface IEventsDriver
    {
        bool Initialize  (               );
        bool Uninitialize(               );
        void Set         ( uint Events );
        // destructive read system event flags
        uint Get(uint EventsOfInterest);
        void Clear(uint Events);
        // non-destructive read system event flags
        uint MaskedRead(uint EventsOfInterest);
        uint WaitForEvents( uint powerLevel, uint WakeupSystemEvents, uint Timeout_Milliseconds );
        void SetBoolTimer( IntPtr TimerCompleteFlag, uint MillisecondsFromNow );
    }
}

