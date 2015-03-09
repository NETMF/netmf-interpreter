
namespace Microsoft.SPOT.Drivers.Bluetooth
{
    /// <summary>
    /// An enumeration of the possible power states for a Bluetooth chip.
    /// These values are to be used as suggestions to IPowerState when it
    /// sets the power mode of the Bluetooth chip before a given operation.
    /// </summary>
    public enum PowerState
    {
        Off,
        On,
        Sniff,
        Hold,
        Park,
    };

    /// <summary>
    /// An interface that represents the power state of a Bluetooth chip.
    /// </summary>
    public interface IPowerState
    {
        /// <summary>
        /// Takes a PowerState as a suggestion and tries to put the Bluetooth chip
        /// in that state (or in as close a state as possible to that state).
        /// </summary>
        /// <param name="ps">
        /// The suggested PowerState.  Before returning, this variable is assigned
        /// the PowerState that the Bluetooth chip was actually set to.
        /// </param>
        /// <returns>
        /// True if the Bluetooth chip was put in the requested PowerState or the closest
        /// state possible, false if changing the PowerState of the chip was not possible
        /// </returns>
        bool SetPowerState(ref PowerState ps);

    } // interface IPowerState
}


