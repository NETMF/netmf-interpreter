using System;
using System.Threading;
using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Drivers.Bluetooth
{
    /// <summary>
    /// Implements a default power policy that is designed specifically for
    /// use with the Promi-ESD-02 Bluetooth chip.
    /// </summary>
    class DefaultPowerPolicy : IPowerPolicy
    {
        private const int c_powerOffTimeout = 100;  // Time until power-off after no commands left to execute

        //--//

        private Timer m_powerOffTimer;              // Timer fires when it is time to shut off the chip

        //--//

        /// <summary>
        /// Creates a new DefaultPowerPolicy with a null power-off Timer.
        /// </summary>
        public DefaultPowerPolicy()
        {
        }

        /// <summary>
        /// Sets the power state of the provided IPowerState given that the
        /// specified BTCmd will be executed next.  A null BTCmd indicates that
        /// there are no more pending BTCmds to be executed from any connection.
        /// </summary>
        /// <param name="ips">
        /// The IPowerState
        /// </param>
        /// <param name="conn">
        /// The BTConnection for which the BTCmd is being executed.
        /// </param>
        /// <param name="cmd">
        /// The next BTCmd to be executed, or null if there are no pending BTCmds.
        /// </param>
        public void ApplyPolicy(IPowerState ips, BTConnection conn, BTConnection.BTCmd cmd)
        {
            lock (this)
            {
                if (null != cmd)
                {   // Dispose of the Timer if it's active, then PARK the chip
                    if (null != m_powerOffTimer)
                    {
                        m_powerOffTimer.Dispose();
                        m_powerOffTimer = null;
                    }

                    PowerState state = PowerState.Park;

                    while (!ips.SetPowerState(ref state))
                    {
                        Thread.Sleep(1);
                    }
                }
                else
                {   // Then set a Timer that will turn the chip OFF if no new commands in the specified timeout
                    if (null == conn || !conn.Connected)
                    {
                        if (null == m_powerOffTimer)
                        {
                            m_powerOffTimer = new Timer(PowerOffTimerFired, ips, c_powerOffTimeout, Timeout.Infinite);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The event handler for the power off Timer firing event.
        /// </summary>
        /// <param name="sender">
        /// The sending object (i.e. the Timer).
        /// </param>
        /// <param name="eargs">
        /// The event arguments.
        /// </param>
        public void PowerOffTimerFired(object state)
        {
            IPowerState ips = (IPowerState)state;

            lock (this)
            {
                if (null != m_powerOffTimer)
                {
                    m_powerOffTimer.Dispose();
                    m_powerOffTimer = null;

                    PowerState ps = PowerState.Off;

                    while (!ips.SetPowerState(ref ps))
                    {
                        Thread.Sleep(1);
                    }
                }
            }
        }

    } // end class DefaultPowerPolicy
}


