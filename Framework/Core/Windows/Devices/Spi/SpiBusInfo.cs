using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Windows.Devices.Spi
{
    /// <summary>
    /// Represents the information about a SPI bus.
    /// </summary>
    public sealed class SpiBusInfo
    {
        /// <summary>
        /// Initializes a new instance of SpiBusInfo.
        /// </summary>
        internal SpiBusInfo()
        {
        }

        /// <summary>
        /// Gets the number of chip select lines available on the bus.
        /// </summary>
        /// <value>Number of chip select lines.</value>
        extern public int ChipSelectLineCount
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        /// <summary>
        /// Minimum clock cycle frequency of the bus.
        /// </summary>
        /// <value>The clock cycle in Hz.</value>
        public int MinClockFrequency
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Maximum clock cycle frequency of the bus.
        /// </summary>
        /// <value>The clock cycle in Hz.</value>
        public int MaxClockFrequency
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the bit lengths that can be used on the bus for transmitting data.
        /// </summary>
        /// <value>The supported data lengths.</value>
        public int[] SupportedDataBitLengths
        {
            get
            {
                // We only support BYTE and USHORT.
                return new int[] { 8, 16 };
            }
        }
    }
}
