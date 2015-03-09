using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Drivers.Bluetooth
{
    /// <summary>
    /// An interface that represents a power management policy for a Bluetooth
    /// chip.
    /// </summary>
    public interface IPowerPolicy
    {
        /// <summary>
        /// Given the next BTCmd that will be executed, decides how to set the power state
        /// of the provided IPowerState.  A null command indicates that there are no commands
        /// that must be executed.
        /// </summary>
        /// <param name="ps">
        /// The IPowerState.
        /// </param>
        /// <param name="conn">
        /// The connection for which the next command will be executed or null if there are none.
        /// </param>
        /// <param name="cmd">
        /// The next BTCmd that will be executed or null if there is one.
        /// </param>
        void ApplyPolicy(IPowerState ips, BTConnection conn, BTConnection.BTCmd cmd);

    } // interface IPowerPolicy
}


