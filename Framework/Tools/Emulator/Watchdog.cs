////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Emulator.Watchdog;

using Hardware = Microsoft.SPOT.Hardware;


namespace Microsoft.SPOT.Emulator.Watchdog
{
    internal class WatchdogDriver : HalDriver<IWatchdogDriver>, IWatchdogDriver
    {
        Timer _timer;
        int   _timeout_ms;

        private void Callback( object state )
        {
            this.Hal.Time.IsExecutionPaused = true;

            this.Emulator.EmulatorNative.WatchdogCallback();

            this.Hal.Time.IsExecutionPaused = false;
        }

        #region IWatchdogDriver Members

        bool IWatchdogDriver.Enable( int timeout_ms )
        {
            if(_timer != null) _timer.Dispose();
            
            _timeout_ms = timeout_ms;

            _timer = new Timer( new TimerCallback( this.Callback ), null, timeout_ms, Timeout.Infinite );

            return true;
        }

        void IWatchdogDriver.Disable()
        {
            if (_timer != null)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        bool IWatchdogDriver.ResetCounter()
        {
            if (_timer != null)
            {
                return _timer.Change(_timeout_ms, Timeout.Infinite);
            }

            return false;
        }

        void IWatchdogDriver.ResetCpu()
        {
            //not implemented now
        }

        #endregion
    }
}
    
