using System;
using System.Threading;
using System.Collections;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Drivers.Bluetooth
{
    //--//

    /// <summary>
    /// A struct that holds information about a particular Bluetooth device.
    /// </summary>
    public struct BTDeviceInfo
    {
        public string Address;
        public string Name;
    };

    //--//

    /// <summary>
    /// An abstract managed driver for a generic Bluetooth device.
    ///
    /// The contained abstract methods constitute this namespace's low-level Bluetooth API.
    /// Methods are meant to be implemented synchronously as asynchronous processing will
    /// be handled by the BTManager and BTConnection classes.
    /// </summary>
    public abstract class BTDriver : IDisposable, IPowerState
    {
        // An enumeration containing the possible states for the local device
        public enum BTDeviceState
        {
            OFF,                    // Device is off
            UNINITIALIZED,          // Device is off and has been disposed or has not yet been created
            STANDBY,                // Device is on and ready to receive commands
            PENDING,                // Device is busy processing a command
            ONLINE,                 // Device is connected to another device and can read/write data to that device
        };

        //--//

        public BTDeviceInfo DeviceInfo;            // The device information for the local device

        public bool Authentication;        // Whether or not the local device is using authentication
        public bool Encryption;            // Whether or not the local device is using encryption
        public bool Parked;                // Whether or not the local device is parked

        //--//
        ////////////////////////////////////////////////////////////////////////////////////
        // Abstract methods

        public abstract bool SwitchPower(bool on);
        public abstract bool CheckDevicePresent();
        public abstract bool ResetDevice();
        public abstract bool SetFriendlyDeviceName(string name);
        public abstract bool SetPassKey(string key);
        public abstract bool SetParkMode(bool park);
        public abstract bool InquiryScan(int timeout);
        public abstract bool PageAddress(string addr, int timeout);
        public abstract bool PageScan(int timeout);
        public abstract bool PageScanForAddress(string addr, int timeout);
        public abstract bool InquiryAndPageScans(int timeout);
        public abstract bool Disconnect();
        public abstract int Send(byte[] data);
        public abstract int Receive(ref byte[] buffer, int timeout);
        public abstract void UpdateDeviceStatus();
        public abstract string GetLastConnectedAddress();
        public abstract BTDeviceInfo[] Inquiry();

        //--//

        /// <summary>
        /// Takes a PowerState as a suggestion and tries to put the Bluetooth chip
        /// in that state (or in as close a state as possible to that state).
        /// </summary>
        /// <param name="ps">
        /// The suggested PowerState
        /// </param>
        /// <returns>
        /// The PowerState that the Bluetooth chip was actually set to
        /// </returns>
        public virtual bool SetPowerState(ref PowerState ps)
        {
            return true;
        }

        /// <summary>
        /// Dispose method of the IDisposable interface
        /// </summary>
        public virtual void Dispose()
        {
            // Functionality to be overridden by child classes
        }

        //--//

        // The current state of the local device
        protected BTDeviceState m_state = BTDeviceState.UNINITIALIZED;

        // Whether or not the local device is present
        protected bool m_devicePresent;

        //--//

    } // class BTDriver
} // namespace Bluetooth


