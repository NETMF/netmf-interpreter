using System;

namespace Microsoft.SPOT.Drivers.Bluetooth
{
    /// <summary>
    /// Contains the Name, Address, and Class of a Bluetooth device.
    /// </summary>
    public class BTDeviceInfo
    {
        private const string c_DefaultClass = "2";

        //--//

        public readonly string Name;
        public readonly string Address;
        public readonly string Class;

        //--//

        /// <summary>
        /// Creates a new BTDeviceInfo object of the default class.
        /// </summary>
        /// <param name="name">Name of the device</param>
        /// <param name="addr">Address of the device</param>
        public BTDeviceInfo( string name, string addr )
        {
            Name    = name;
            Address = addr;
            Class   = c_DefaultClass;
        }

        /// <summary>
        /// Creates a new BTDevice info object.
        /// </summary>
        /// <param name="name">Name of the device</param>
        /// <param name="addr">Address of the device</param>
        /// <param name="devClass">Class of the device</param>
        public BTDeviceInfo( string name, string addr, string devClass )
        {
            Name    = name;
            Address = addr;
            Class   = devClass;
        }

        /// <summary>
        /// Creates and returns a string representing this device.
        /// </summary>
        /// <returns>The string</returns>
        public override string ToString()
        {
            return Name + "," + Address + "," + Class;
        }
    } // class BTDeviceInfo
}


