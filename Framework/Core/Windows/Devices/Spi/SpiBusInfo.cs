using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Windows.Devices.Spi
{
// warning CS0649: Field 'Windows.Devices.Spi.SpiBusInfo.xxx' is never assigned to, and will always have its default value 0
//                 - These are all initialized in native code constructor.
#pragma warning disable 0649

    /// <summary>
    /// Represents the information about a SPI bus.
    /// </summary>
    public sealed class SpiBusInfo
    {
        private int MinClockFrequency_;
        private int MaxClockFrequency_;
        private int ChipSelectLineCount_;

        [MethodImplAttribute( MethodImplOptions.InternalCall )]
        extern internal SpiBusInfo( int busNum );

        /// <summary>
        /// Gets the number of chip select lines available on the bus.
        /// </summary>
        /// <value>Number of chip select lines.</value>
        public int ChipSelectLineCount
        {
            get { return ChipSelectLineCount_; }
        }

        /// <summary>
        /// Minimum clock cycle frequency of the bus.
        /// </summary>
        /// <value>The clock cycle in Hz.</value>
        public int MinClockFrequency
        {
            get { return MinClockFrequency_; }
        }

        /// <summary>
        /// Maximum clock cycle frequency of the bus.
        /// </summary>
        /// <value>The clock cycle in Hz.</value>
        public int MaxClockFrequency
        {
            get { return MaxClockFrequency_; }
        }

        /// <summary>
        /// Gets the bit lengths that can be used on the bus for transmitting data.
        /// </summary>
        /// <value>The supported data lengths.</value>
        public int[] SupportedDataBitLengths
        {
            get
            {
                // Currently only support BYTE and USHORT.
                return new int[ ] { 8, 16 };
            }
        }
    }
}
