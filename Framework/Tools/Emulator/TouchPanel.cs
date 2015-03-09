////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Emulator.TouchPanel
{
    internal class TouchPanelDriver : HalDriver<ITouchPanelDriver>, ITouchPanelDriver
    {
        struct TPoint
        {
            public int _tipState;
            public int _x;
            public int _y;
            public int _source;
        };

        int lastx=0, lasty = 0;

        List<TPoint> _queue = new List<TPoint>();
        

        #region ITouchPanelDriver Members

        bool ITouchPanelDriver.TouchPanel_Disable()
        {
            return true;
        }

        bool ITouchPanelDriver.TouchPanel_Enable()
        {
            _queue.Clear();
            return true;
        }

        void ITouchPanelDriver.TouchPanel_GetPoint(
            ref int tipState,
            ref int source,
            ref int unCalX,
            ref int unCalY)
        {
            if (_queue.Count > 0)
            {
                TPoint tp;
                int index = 0;

                if ((tipState & 0x3) == 0)
                {
                    index = _queue.Count-1;
                }

                tp = (TPoint)_queue[index];

                if ((tipState & 0x3) != 0)
                {
                    _queue.RemoveAt(index);
                }

                tipState = tp._tipState;
                unCalX = tp._x;
                unCalY = tp._y;
                source = tp._source;
            }
            else
            {
                tipState = 0;
                unCalX = 0;
                unCalY = 0;
                source = 0;
            }
        }

        void ITouchPanelDriver.TouchPanelSetPoint(
            int tipState,
            int source,
            int unCalX,
            int unCalY)
        {
            if(lastx == unCalX && lasty == unCalY) return;
            
            TPoint tp = new TPoint();
            tp._tipState = tipState;
            tp._x = unCalX;
            tp._y = unCalY;
            tp._source = source;

            if(_queue.Count > 10)
            {
                _queue.RemoveAt(0);
            }

            _queue.Add(tp);
        }

        #endregion
    }

    public class TouchGpioPort : Gpio.GpioPort
    {
        public const Cpu.Pin DefaultTouchPin = (Cpu.Pin)100;
        public TouchGpioPort(Cpu.Pin pin)
            : base(pin, Microsoft.SPOT.Emulator.Gpio.GpioPortMode.InputPort, Microsoft.SPOT.Emulator.Gpio.GpioPortMode.InputPort)
        {
        }

        const int TouchSampleDownFlag = 0x02;
        const int TouchSamplePreviousDownFlag = 0x08;

        public void WriteTouchData(int tipState, int source, int x, int y)
        {
            /// Save touch data always.
            this.Emulator.TouchPanelDriver.TouchPanelSetPoint(tipState, source, x, y);

            /// Call interrupt only when it is StylusDown or StylusUp.
            if (!(((tipState & TouchSamplePreviousDownFlag) != 0) &&
                ((tipState & TouchSampleDownFlag) != 0)) &&
                (source == 0))
            {
                TouchCallback(_isr, _pin, ((tipState & TouchSampleDownFlag) != 0) ? 0 : 1, _isrParam);

                if ((tipState & TouchSamplePreviousDownFlag) != 0)
                {
                    this.Emulator.TouchPanelDriver.TouchPanel_Enable();
                }
            }
        }

        public void PostGesture(int gestureId, int x, int y, ushort data)
        {
            // simulate touch/stylus down
            this.Emulator.TouchPanelDriver.TouchPanelSetPoint(3, 0, x, y);

            // ISR for touch/stylus down
            TouchCallback(_isr, _pin, 0, _isrParam);

            // post multitouch gesture to emulator
            this.Emulator.EmulatorNative.PostGesture(gestureId, x, y, data);

            // simulate touch/stylus up
            this.Emulator.TouchPanelDriver.TouchPanelSetPoint(9, 0, x, y);

            // ISR for touch/stylus up
            TouchCallback(_isr, _pin, 1, _isrParam);
        }
        

        internal void TouchCallback(IntPtr isr, Cpu.Pin pin, int val, IntPtr param)
        {
            if (isr != IntPtr.Zero)
            {
                this.Emulator.ExecuteWithInterruptsDisabled(
                    delegate
                    {
                        this.Emulator.EmulatorNative.TouchCallback(isr, (uint)pin, (uint)val, param);
                    }
                );
            }
        }
    }
}
