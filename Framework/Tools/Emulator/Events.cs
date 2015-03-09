////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading;
using Microsoft.SPOT.Emulator.Time;

namespace Microsoft.SPOT.Emulator.Events
{
    #region SystemEvents
    internal enum SystemEvents : uint
    {
        NONE                 = 0x00000000,
        COM_IN               = 0x00000001,
        COM_OUT              = 0x00000002,
        FLAG_USB_IN          = 0x00000004,
        FLAG_USB_OUT         = 0x00000008,
        SYSTEM_TIMER         = 0x00000010,
        TIMER1               = 0x00000020,
        TIMER2               = 0x00000040,
        BUTTON               = 0x00000080,
        TONE_COMPLETE        = 0x00001000,
        TONE_BUFFER_EMPTY    = 0x00002000,
        SOCKET               = 0x00004000,
        SPI                  = 0x00008000,
        CHARGER_CHANGE       = 0x00010000,
        OEM_RESERVED_1       = 0x00020000,
        OEM_RESERVED_2       = 0x00040000,
        IO                   = 0x00080000,
        HW_INTERRUPT         = 0x08000000,
        I2C_XACTION          = 0x10000000,
        DEBUGGER_ACTIVITY    = 0x20000000,
        MESSAGING_ACTIVITY   = 0x40000000,
        ALL                  = 0xffffffff,
    }
    #endregion

    internal class EventsDriver : HalDriver<IEventsDriver>, IEventsDriver
    {
        private class BooleanTimer : TimingServices.Completion
        {
            private IntPtr _ptrTimeQuantumExpired;

            public BooleanTimer(Emulator emulator)
                : base(emulator)
            {
            }

            public IntPtr TimeQuantumExpired
            {
                [MethodImplAttribute(MethodImplOptions.Synchronized)]
                set { _ptrTimeQuantumExpired = value; }
            }

            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            protected internal override void OnCompletion()
            {
                if (_ptrTimeQuantumExpired != IntPtr.Zero)
                {
                    Marshal.WriteInt32(_ptrTimeQuantumExpired, 1);
                }
            }            
        }

        private const int c_WaitHandleShutdown = 0;
        private const int c_WaitHandleEvents = 1;
        private const int c_WaitHandles = 2;

        uint _events;
        AutoResetEvent _are;
        BooleanTimer _booleanTimer;
        WaitHandle[] _waitHandles;

        public override void SetupComponent()
        {
            _are = new AutoResetEvent(false);
            _booleanTimer = new BooleanTimer(this.Emulator);
            _waitHandles = new WaitHandle[c_WaitHandles];

            _waitHandles[c_WaitHandleShutdown] = this.Emulator.ShutdownHandle;
            _waitHandles[c_WaitHandleEvents] = _are;    
        }

        #region IEventsDriver Members

        bool IEventsDriver.Initialize()
        {
            return true;
        }

        bool IEventsDriver.Uninitialize()
        {
            return true;
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        void IEventsDriver.Set(uint Events)
        {
            _events |= Events;
            _are.Set();
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        uint IEventsDriver.Get(uint EventsOfInterest)
        {
            uint res = _events & EventsOfInterest;
            _events &= ~EventsOfInterest;

            return res;
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        void IEventsDriver.Clear(uint Events)
        {
            _events &= ~Events;
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        uint IEventsDriver.MaskedRead(uint EventsOfInterest)
        {
            uint res = _events & EventsOfInterest;

            return res;
        }

        uint IEventsDriver.WaitForEvents(uint powerLevel, uint WakeupSystemEvents, uint Timeout_Milliseconds)
        {
            TimingServices timingServices = this.Emulator.TimingServices;            
            int timeout = (int)Timeout_Milliseconds;
            ulong end = timingServices.CurrentTicks + (ulong)(timeout * TimeSpan.TicksPerMillisecond);
            uint res;

            Debug.Assert(timeout >= 0 || WakeupSystemEvents != 0);

            while ((res = ((IEventsDriver)this).MaskedRead(WakeupSystemEvents)) == 0)
            {
                if (!timingServices.DequeueAndExecuteContinuation())
                {
                    if (timeout != Timeout.Infinite)
                    {
                        ulong now = timingServices.CurrentTicks;

                        if (now >= end)
                        {
                            break;
                        }
                        else
                        {
                            timeout = (int)((end - now) / TimeSpan.TicksPerMillisecond);
                            Debug.Assert(timeout >= 0);
                        }
                    }

                    if (WaitHandle.WaitAny(_waitHandles, timeout, false) == c_WaitHandleShutdown)
                    {
                        break;
                    }
                }
            }

            return res;
        }

        void IEventsDriver.SetBoolTimer(IntPtr TimerCompleteFlag, uint MillisecondsFromNow)
        {
            _booleanTimer.TimeQuantumExpired = TimerCompleteFlag;
            _booleanTimer.AbortCompletion();

            if (TimerCompleteFlag != IntPtr.Zero)
            {
                uint uSecFromNow = MillisecondsFromNow * 1000;

                _booleanTimer.EnqueueCompletion(uSecFromNow);
            }
        }

        #endregion
    }
}

